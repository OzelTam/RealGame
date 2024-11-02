public static class ConsoleHelper
{
    public enum LogLevel
    {
        Info,
        Success,
        Warn,
        Err,
        Fatal,
        None
    }
    public static event Action<string> OnCommand;
    private static readonly List<string> Logs = new List<string>();
    private static readonly List<string> CommandHistory = new List<string>();
    private static int CommandHistoryIndex = 0;
    private static readonly object LockObj = new object();
    private static string Separator = new string('_', Console.BufferWidth);
    private static string currentCommand = string.Empty;
    private static int logDisplayOffset = 0;


    public static void Print(string message, LogLevel level = LogLevel.Info)
    {
        lock (LockObj)
        {
            Logs.Add(FormatLogMessage(message, level));
            if (logDisplayOffset > 0) logDisplayOffset++;
            DrawConsole();
        }
    }

    private static string FormatLogMessage(string message, LogLevel level)
    {
        string levelText = level switch
        {
            LogLevel.Info => "[Info]",
            LogLevel.Warn => "[Warn]",
            LogLevel.Err => "[Err]",
            LogLevel.Fatal => "[Fatal]",
            LogLevel.Success => "[Success]",
            _ => ""
        };

        return $"{levelText} {message}";
    }

    public static void RunCommandListener()
    {
        while (true)
        {
            lock (LockObj)
            {
                DrawConsole();
            }

            var key = Console.ReadKey(true);
            if (key.Key == ConsoleKey.Enter)
            {
                CommandHistory.Add(currentCommand);
                CommandHistoryIndex = CommandHistory.Count;
                OnCommand?.Invoke(currentCommand);
                currentCommand = string.Empty;
            }
            else if (key.Key == ConsoleKey.Backspace && currentCommand.Length > 0)
            {
                currentCommand = currentCommand[..^1];
            }
            else if (key.Key == ConsoleKey.UpArrow)
            {
                ScrollUp();
            }
            else if (key.Key == ConsoleKey.DownArrow)
            {
                ScrollDown();
            }
            else if (key.Key == ConsoleKey.LeftArrow)
            {
                if (CommandHistoryIndex > 0)
                {
                    CommandHistoryIndex--;
                    currentCommand = CommandHistory[CommandHistoryIndex];
                }
            }
            else if (key.Key == ConsoleKey.RightArrow)
            {
                if (CommandHistoryIndex < CommandHistory.Count - 1)
                {
                    CommandHistoryIndex++;
                    currentCommand = CommandHistory[CommandHistoryIndex];
                }
            }
            else if (key.Key != ConsoleKey.Backspace)
            {
                currentCommand += key.KeyChar;
            }

        }
    }

    private static void ScrollUp()
    {
        if (logDisplayOffset < Logs.Count - (Console.WindowHeight - 3))
        {
            logDisplayOffset++;
            DrawConsole();
        }
    }

    private static void ScrollDown()
    {
        if (logDisplayOffset > 0)
        {
            logDisplayOffset--;
            DrawConsole();
        }
    }


    private static void DrawConsole()
    {
        Console.Clear();
        int maxVisibleLogs = Console.WindowHeight - 3;
        int startLog = Math.Max(0, Logs.Count - maxVisibleLogs - logDisplayOffset);
        int endLog = Math.Min(Logs.Count, startLog + maxVisibleLogs);

        for (int i = startLog; i < endLog; i++)
        {
            SetLogLevelColor(Logs[i]);
            Console.WriteLine(Logs[i]);
            Console.ResetColor();
        }
        Console.SetCursorPosition(0, Console.WindowHeight - 2);
        Console.WriteLine(Separator);
        Console.Write("Command: " + currentCommand);
        Console.ResetColor();
    }

    private static void SetLogLevelColor(string log)
    {
        if (log.StartsWith("[Info]")) Console.ForegroundColor = ConsoleColor.Blue;
        else if (log.StartsWith("[Warn]")) Console.ForegroundColor = ConsoleColor.Yellow;
        else if (log.StartsWith("[Err]")) Console.ForegroundColor = ConsoleColor.Red;
        else if (log.StartsWith("[Fatal]")) Console.ForegroundColor = ConsoleColor.DarkRed;
        else if (log.StartsWith("[Success]")) Console.ForegroundColor = ConsoleColor.Green;
        else Console.ResetColor();
    }
}