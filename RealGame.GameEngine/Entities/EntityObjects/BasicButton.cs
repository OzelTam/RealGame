using SFML.Graphics;
using SFML.System;
using SFML.Window;


namespace RealGame.GameEngine.Entities.EntityObjects
{
    public class BasicButton : Drawable
    {
        public string TextContent { get; set; } = "";
        public Color TextColor { get; set; } = Color.White;
        public Color ButtonColor { get; set; } = Color.Blue;
        public Color HoverColor { get; set; } = Color.Cyan;
        public Color PressedColor { get; set; } = Color.Green;

        public event EventHandler<MouseButtonEvent>? OnClick;
        public event EventHandler? OnHover;
        public event EventHandler<MouseButtonEvent>? OnMouseDown;
        public event EventHandler? OnMouseLeave;

        public bool IsHovered { get; private set; } = false;
        public bool IsPressed { get; private set; } = false;
        public Vector2f Position { get; set; } = new();

        public float Padding { get; set; }

        public Vector2f Size => Rect?.Size ?? new();
        public RectangleShape Rect { get; set; }
        public Text Text { get; set; }
        public uint FontSize { get; set; } = 10;

        private RenderWindow? _window;
        public BasicButton(string textContent, float padding = 5, RenderWindow? relativeTo = null)
        {
            _window = relativeTo;
            TextContent = textContent;
            Padding = padding;
            var defalutFont = GameWindow.DefaultFont;
            Text = defalutFont == null ? new Text() : new Text(TextContent, defalutFont);
            var size = Text.GetLocalBounds();
            Rect = new RectangleShape(new Vector2f(size.Width + Padding * 2, size.Height + Padding * 2));
        }


        public void Draw(RenderTarget target, RenderStates states)
        {
            var mousePosition = _window == null ? Mouse.GetPosition() : Mouse.GetPosition(_window);
            Text.FillColor = TextColor;
            Text.Position = Position + new Vector2f(Padding, Padding);
            Text.CharacterSize = FontSize;
            var size = Text.GetGlobalBounds().Size;

            Rect.Position = Position + new Vector2f(Padding, Padding) / 2;
            Rect.Size = new Vector2f(size.X + Padding, size.Y + Padding);
            var bounds = Rect.GetGlobalBounds();
            if (_window != null)
            {
                var viewTopLeft = _window.GetView().Center - _window.GetView().Size / 2;
                var movedBounds = new FloatRect(bounds.Left - viewTopLeft.X, bounds.Top - viewTopLeft.Y, bounds.Width, bounds.Height);
                var viewSize = _window.GetView().Size;
                var windowSize = _window.Size;
                var zoom = viewSize.X / windowSize.X;
                movedBounds.Left /= zoom;
                movedBounds.Top /= zoom;
                movedBounds.Width /= zoom;
                movedBounds.Height /= zoom;

                bounds = movedBounds;
            }
            if (bounds.Contains(mousePosition.X, mousePosition.Y))
            {
                var leftPressed = Mouse.IsButtonPressed(Mouse.Button.Left);
                var rightPressed = Mouse.IsButtonPressed(Mouse.Button.Left);
                var middlePressed = Mouse.IsButtonPressed(Mouse.Button.Left);
                if (leftPressed || rightPressed || middlePressed)
                {
                    var pressedButton = leftPressed ? Mouse.Button.Left : rightPressed ? Mouse.Button.Right : Mouse.Button.Middle;
                    if (!IsPressed)
                        OnMouseDown?.Invoke(this, new MouseButtonEvent { Button = pressedButton, X = mousePosition.X, Y = mousePosition.Y });
                    IsPressed = true;
                    IsHovered = false;
                }
                else
                {
                    OnHover?.Invoke(this, new EventArgs());
                    if (IsPressed)
                        OnClick?.Invoke(this, new MouseButtonEvent { Button = Mouse.Button.Left, X = mousePosition.X, Y = mousePosition.Y });
                    IsHovered = true;
                    IsPressed = false;
                }
            }
            else
            {
                if (IsHovered)
                    OnMouseLeave?.Invoke(this, new EventArgs());
                IsHovered = false;
                IsPressed = false;
            }

            Rect.FillColor = IsPressed ? PressedColor : IsHovered ? HoverColor : ButtonColor;

            Rect.Draw(target, states);
            Text.Draw(target, states);

        }
    }
}
