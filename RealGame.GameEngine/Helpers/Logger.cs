using System.Text;

namespace RealGame.GameEngine.Helpers
{

    public enum LogLevel
    {
        Default = 0,
        Info = 1,
        Warn = 2,
        Error = 3,
        Fatal = 4,
    }

    public class Log
    {
        public LogLevel Level { get; set; }
        public string Message { get; set; }
        public string? LogId { get; set; }
        public DateTime Time { get; set; }

        public Log(string message, LogLevel level = LogLevel.Default, string? logId = null)
        {
            Level = level;
            Message = message;
            LogId = logId;
            Time = DateTime.Now;
        }
    }
    public static class Logger
    {
        private static List<Log> Logs { get; set; } = new();
        private static bool WorkOnlyOnDebug = false;

        public new static string ToString()
        {
            if (WorkOnlyOnDebug && !System.Diagnostics.Debugger.IsAttached)
                return string.Empty;

            var sb = new StringBuilder();
            Logs.ForEach(s => sb.AppendLine($"[{s.Level.ToString()}] {s.Time.ToLongTimeString()} - {s.Message}"));
            return sb.ToString();

        }
        public static void Log(string message, LogLevel level = LogLevel.Default, string? logId = null)
        {
            if (WorkOnlyOnDebug && !System.Diagnostics.Debugger.IsAttached)
                return;
            if (string.IsNullOrEmpty(logId))
            {
                Logs.Add(new(message, level, null));
            }
            else
            {
                if (Logs.Any(s => s.LogId == logId))
                {
                    Logs.Where(s => s.LogId == logId).ToList().ForEach(e => e.Level = level);
                    Logs.Where(s => s.LogId == logId).ToList().ForEach(e => e.Message = message);
                    Logs.Where(s => s.LogId == logId).ToList().ForEach(e => e.Time = DateTime.Now);
                }
                else
                {
                    Logs.Add(new(message, level, logId));
                }
            }

        }


        public static StringBuilder CreateDebugText()
        {
            var str = new StringBuilder();
            str.AppendLine("Object Count: " + GameWindow.ActiveScene.GameDrawings.Count);
            str.AppendLine("Tags: " + string.Join(", ", GameWindow.ActiveScene.GameDrawings.Tags));
            str.AppendLine("Drawable Ids: " + string.Join(", ", GameWindow.ActiveScene.GameDrawings.Ids));
            str.AppendLine("Animation Ids: " + string.Join(", ", GameWindow.ActiveScene.Animations.Ids));
            str.AppendLine("Texture Ids: " + string.Join(", ", GameWindow.ActiveScene.Textures.Ids));
            str.AppendLine("");
            str.AppendLine("");
            ToString().Split("\n").ToList().ForEach(s => str.AppendLine(s));
            return str;
        }

        public static void ClearLogs()
        {
            Logs.Clear();
        }


        static string lastMessage = "";
        public static void StdOut(string message)
        {
            if (lastMessage != message)
            {
                //var newLines = message.Split("\n");
                //var oldLines = lastMessage.Split("\n");
                //var maxLen = Math.Max(newLines.Length, oldLines.Length);
                //var minLen = Math.Min(newLines.Length, oldLines.Length);
                //var i = 0;
                //while (i < (minLen -1))
                //{
                //    if (newLines[i] != oldLines[i])
                //    {
                //        Console.SetCursorPosition(0, i);
                //        Console.Write(newLines[i]);
                //    }
                //}
                //for (int j = i; j < maxLen -1; j++)
                //{
                //    var updateLine = j >= Console.BufferHeight ? Console.BufferHeight -1 : j;

                //    Console.SetCursorPosition(0, updateLine);
                //    if (j < newLines.Length)
                //        Console.WriteLine(newLines[updateLine]);


                //}

            }
        }

    }
}
