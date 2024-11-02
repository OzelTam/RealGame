using RealGame.GameEngine.Entities.GameEntities;
using RealGame.GameEngine.Entities.Interfaces;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace RealGame.GameEngine.Helpers
{
    public static partial class Extensions
    {
        public static bool AnyKeyPressed(this GameWindow game, params Keyboard.Key[] keys)
        {
            return keys.Any(key => game.IsKeyPressed(key));
        }

        public static bool AnyKeyPressed(this GameWindow game)
        {
            return game.DownKeys.Any();
        }

        public static bool KeysPressed(this GameWindow game, params Keyboard.Key[] keys)
        {
            return keys.All(key => game.IsKeyPressed(key));
        }

        public static GameScene ActiveScene(this GameWindow game)
        {
            return GameWindow.ActiveScene;
        }

        public static TimeSpan DeltaTime(this GameWindow game)
        {
            return GameWindow.DeltaTime;
        }

        public static float DeltaFPS(this GameWindow game)
        {
            return GameWindow.DeltaFPS;
        }


        public static void ViewCenterEntity(this GameWindow game, IGameDrawable drawable)
        {
            var view = game.GetView();
            var center = drawable.GetGlobalBounds().Center();
            view.Center = new SFML.System.Vector2f(center.X, center.Y);
            game.SetView(view);
        }

        public static void Zoom(this GameWindow game, float factor)
        {
            var view = game.GetView();
            view.Zoom(factor);
            game.SetView(view);
        }

        private static bool _viewDragMoveEnabled = false;
        private static bool _viewDragMoveInstantiated = false;
        private static float _viewDragMoveSpeed = 1;
        static Mouse.Button _dragMouseButton = Mouse.Button.Left;
        public static void EnableViewDragMove(this GameWindow game, float dragSpeed = 1f, Mouse.Button dragButton = Mouse.Button.Left)
        {
            _dragMouseButton = dragButton;
            _viewDragMoveSpeed = dragSpeed;
            if (!_viewDragMoveInstantiated)
            {
                InstantiateViewDragMove(game);
                _viewDragMoveInstantiated = true;
            }
            _viewDragMoveEnabled = true;
        }
        public static void DisableViewDragMove(this GameWindow game) => _viewDragMoveEnabled = false;
        public static bool IsViewDragMoveEnabled(this GameWindow game) => _viewDragMoveEnabled;


        private static bool _viewScrollZoomEnabled = false;
        private static bool _viewScrollZoomInstantiated = false;
        private static float _viewScrollZoomSpeed = 0.1f;
        public static void EnableViewScrollZoom(this GameWindow game, float zoomSpeed = 0.1f)
        {
            _viewScrollZoomSpeed = zoomSpeed;
            if (!_viewScrollZoomInstantiated)
            {
                InstantiateViewScrollZoom(game);
                _viewScrollZoomInstantiated = true;
            }
            _viewScrollZoomEnabled = true;
        }

        public static void DisableViewScrollZoom(this GameWindow game) => _viewScrollZoomEnabled = false;
        public static bool IsViewScrollZoomEnabled(this GameWindow game) => _viewScrollZoomEnabled;

        private static void InstantiateViewScrollZoom(GameWindow game)
        {
            game.MouseWheelScrolled += (s, e) =>
            {
                if (!_viewScrollZoomEnabled)
                    return;

                var view = game.GetView();
                view.Zoom(1 - e.Delta * _viewScrollZoomSpeed);
                game.SetView(view);

            };
        }



        static Vector2f? vdRefferencePosition = null;
        static bool vdUpdateMovement = false;

        private static void InstantiateViewDragMove(GameWindow game)
        {


            game.MouseButtonPressed += (s, e) =>
            {
                if (!_viewDragMoveEnabled)
                    return;
                if (e.Button == _dragMouseButton)
                {
                    vdRefferencePosition = game.MapPixelToCoords(new Vector2i(e.X, e.Y));
                    vdUpdateMovement = true;
                }
            };

            game.MouseMoved += (s, e) =>
            {
                if (!_viewDragMoveEnabled)
                    return;
                if (vdUpdateMovement)
                {
                    var mousePos = game.MapPixelToCoords(new Vector2i(e.X, e.Y));
                    if (vdRefferencePosition == null)
                        return;

                    var diff = mousePos - vdRefferencePosition.Value;
                    // Scale the movement by the speed factor
                    var movementWithSpeed = new Vector2f(diff.X * _viewDragMoveSpeed, diff.Y * _viewDragMoveSpeed);

                    var currentView = game.GetView();
                    currentView.Move(-movementWithSpeed);
                    game.SetView(currentView);

                    vdRefferencePosition = game.MapPixelToCoords(new Vector2i(e.X, e.Y));
                }
            };

            game.MouseButtonReleased += (s, e) =>
            {
                if (!_viewDragMoveEnabled)
                    return;
                if (e.Button == _dragMouseButton)
                {
                    vdUpdateMovement = false;
                    vdRefferencePosition = null;
                }
            };
        }


        static IGameDrawable[] _boundaries = new IGameDrawable[4];
        static bool _boundariesInitialized = false;
        static bool _boundariesActive = false;
        private static void InitializeSolidViewBoundaries(this GameWindow game)
        {
            if (!_boundariesActive)
                return;

            if (_boundaries[0] == null)
            {
                var topBoundaryBox = new GameRectangleShape(new Vector2f(game.Size.X, 2));
                topBoundaryBox.Physics.Properties = new() { IsStatic = true };
                topBoundaryBox.Position = new Vector2f(0, 0);
                topBoundaryBox.FillColor = Color.Magenta;
                _boundaries[0] = topBoundaryBox;
            }

            if (_boundaries[1] == null)
            {
                var bottomBoundaryBox = new GameRectangleShape(new Vector2f(game.Size.X, 2));
                bottomBoundaryBox.Physics.Properties = new() { IsStatic = true };
                bottomBoundaryBox.Position = new Vector2f(0, game.Size.Y - 2);
                bottomBoundaryBox.FillColor = Color.Magenta;
                _boundaries[1] = bottomBoundaryBox;
            }

            if (_boundaries[2] == null)
            {
                var leftBoundaryBox = new GameRectangleShape(new Vector2f(2, game.Size.Y));
                leftBoundaryBox.Physics.Properties = new() { IsStatic = true };
                leftBoundaryBox.Position = new Vector2f(0, 0);
                leftBoundaryBox.FillColor = Color.Magenta;
                _boundaries[2] = leftBoundaryBox;
            }

            if (_boundaries[3] == null)
            {
                var rightBoundaryBox = new GameRectangleShape(new Vector2f(2, game.Size.Y));
                rightBoundaryBox.Physics.Properties = new() { IsStatic = true };
                rightBoundaryBox.Position = new Vector2f(game.Size.X - 2, 0);
                rightBoundaryBox.FillColor = Color.Magenta;
                _boundaries[3] = rightBoundaryBox;
            }


            GameWindow.ActiveScene.Add(_boundaries[0]);
            GameWindow.ActiveScene.Add(_boundaries[1]);
            GameWindow.ActiveScene.Add(_boundaries[2]);
            GameWindow.ActiveScene.Add(_boundaries[3]);


            game.OnUpdate += (s, e) =>
            {

                foreach (var boundary in _boundaries)
                {
                    if (!_boundariesActive)
                    {
                        boundary!.Physics.Properties!.CollisionEnabled = _boundariesActive;
                        game.Draw(boundary);
                    }
                }

            };
        }
        public static void EnableSolidViewBoundaries(this GameWindow game)
        {
            _boundariesActive = true;
            if (!_boundariesInitialized)
            {
                InitializeSolidViewBoundaries(game);
                _boundariesInitialized = true;
            }
        }
        public static void DisableSolidViewBoundaries(this GameWindow game)
        {
            _boundariesActive = false;
            if (!_boundariesInitialized)
            {
                InitializeSolidViewBoundaries(game);
                _boundariesInitialized = true;
            }
        }
    }
}
