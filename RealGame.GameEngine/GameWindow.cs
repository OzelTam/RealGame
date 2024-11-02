using RealGame.GameEngine.ConfigurationObjects;
using RealGame.GameEngine.Entities.Interfaces;
using RealGame.GameEngine.Helpers;
using SFML.Graphics;
using SFML.System;
using SFML.Window;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace RealGame.GameEngine
{
    public enum KeyEvent
    {
        Pressed,
        Released
    }

    public enum CollisionType
    {
        None,
        [Experimental("CollisionOBB")]
        OrientedBoundingBox,
        AxisAlignedBoundingBox,
        Circle
    }
    internal class KeyEventFlag
    {
        public Keyboard.Key Key { get; internal set; }
        public KeyEvent KeyEvent { get; internal set; }
        public bool FireOnce { get; internal set; }
        public TimeSpan? Cooldown { get; internal set; } = null;
        public bool HasCooldown => Cooldown != null;
        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            return obj is KeyEventFlag flag &&
                   Key == flag.Key &&
                   KeyEvent == flag.KeyEvent &&
                   FireOnce == flag.FireOnce
                   ;
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(Key, KeyEvent, FireOnce);
        }
    }

    public class GameWindow : RenderWindow
    {
        public static GameScene ActiveScene { get; set; } = new();
        public static float DeltaFPS => 1000 / (float)DeltaTime.TotalMilliseconds;
        public static TimeSpan DeltaTime { get; set; } = TimeSpan.FromMilliseconds(16.66f);
        public static bool Freeze { get; set; } = false;
        public static Font? DefaultFont { get; set; } = null;
        public static Dictionary<string, (IGameDrawable, IGameDrawable)> CurrentCollisions { get; set; } = new(); // Key: PairId, Value: (Drawable1, Drawable2)
        public Vector2f GravityVector { get; set; } = new(0, 0);
        public List<Keyboard.Key> DownKeys => KeyStates.Where(k => k.Value).Select(k => k.Key).ToList();
        public CollisionType CollisionType { get; set; } = CollisionType.AxisAlignedBoundingBox;


        public event EventHandler? OnUpdate;
        private DrawingOptions DrawingOptions = new();
        private Dictionary<Keyboard.Key, bool> KeyStates = new();
        private Dictionary<KeyEventFlag, List<Action>> KeyEventActions = new();
        private Dictionary<KeyEventFlag, DateTime> KeyEventActionsLastFired = new();
        float[]? fps;
        int fpsIndex = 0;
        float[]? deltaMs;
        int deltaIndex = 0;
        bool averageFlag = true;
        int first5FramesCounter = 0;

        #region Constructors
        public GameWindow(VideoMode mode, string title) : base(mode, title) { }
        public GameWindow(VideoMode mode, string title, Styles style) : base(mode, title, style) { }
        public GameWindow(VideoMode mode, string title, Styles style, ContextSettings settings) : base(mode, title, style, settings) { }
        public GameWindow(nint handle) : base(handle) { }
        public GameWindow(nint handle, ContextSettings settings) : base(handle, settings) { }

        #endregion

        #region Gravity Setters
        public void SetGravity(Vector2f gravity) => GravityVector = gravity;

        public void SetGravity(float x, float y) => GravityVector = new Vector2f(x, y);

        public void SetGravity(float value) => GravityVector = new Vector2f(0, value);
        #endregion

        #region Key Events
        /// <summary>
        /// If <paramref name="fireOnce"/> is true, the action will be fired only once when the key state change.
        /// If <paramref name="fireOnce"/> is false, the action will be fired every frame depending on the Key State.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="keyEvent"></param>
        /// <param name="action"></param>
        /// <param name="fireOnce"></param>
        public void AddKeyAction(Keyboard.Key key, KeyEvent keyEvent, Action action, bool fireOnce = false)
        {
            var flag = new KeyEventFlag { FireOnce = fireOnce, Key = key, KeyEvent = keyEvent };

            if (KeyEventActions.ContainsKey(flag))
            {
                KeyEventActions[flag].Add(action);
                return;
            }

            KeyEventActions[flag] = new List<Action> { action };
        }

        public void AddKeyAction(Keyboard.Key key, KeyEvent keyEvent, Action action, TimeSpan cooldown, bool fireOnce = false)
        {
            var flag = new KeyEventFlag { FireOnce = fireOnce, Key = key, KeyEvent = keyEvent, Cooldown = cooldown };

            if (KeyEventActions.ContainsKey(flag))
            {
                KeyEventActions[flag].Add(action);
                return;
            }

            KeyEventActions[flag] = new List<Action> { action };
        }

        public bool IsKeyPressed(Keyboard.Key key)
        {
            return KeyStates.ContainsKey(key) && KeyStates[key];
        }


        private bool IsCooldownDone(KeyEventFlag keyEventFlag)
        {
            if (!KeyEventActionsLastFired.ContainsKey(keyEventFlag))
                return true;
            var lastFired = KeyEventActionsLastFired[keyEventFlag];
            var cooldown = keyEventFlag.Cooldown;
            if (cooldown == null)
                return true;
            return DateTime.Now - lastFired > cooldown;

        }
        private void FireIfCooldownDone(KeyEventFlag keyEventFlag)
        {
            if (IsCooldownDone(keyEventFlag))
            {
                KeyEventActionsLastFired[keyEventFlag] = DateTime.Now;
                KeyEventActions[keyEventFlag].ForEach(a => a());
            }
        }
        private void FireKeyEventActions()
        {
            var keys = KeyEventActions.Keys.Where(k => !k.FireOnce);
            var cooldownEvents = keys.Where(k => k.HasCooldown);
            cooldownEvents.Where(k => k.KeyEvent == KeyEvent.Pressed && IsKeyPressed(k.Key)).ToList().ForEach(k => FireIfCooldownDone(k));
            cooldownEvents.Where(k => k.KeyEvent == KeyEvent.Released && !IsKeyPressed(k.Key)).ToList().ForEach(k => FireIfCooldownDone(k));
            keys = keys.Where(k => !k.HasCooldown);
            keys.Where(k => k.KeyEvent == KeyEvent.Pressed && IsKeyPressed(k.Key)).ToList().ForEach(k => KeyEventActions[k].ForEach(a => a()));
            keys.Where(k => k.KeyEvent == KeyEvent.Released && !IsKeyPressed(k.Key)).ToList().ForEach(k => KeyEventActions[k].ForEach(a => a()));
        }
        #endregion

        #region Initializers
        public void ConfigureDrawingOptions(Action<DrawingOptions> options) => options(DrawingOptions);

        public void SetActiveScene(GameScene scene)
        {
            ActiveScene = scene;
        }

        private void Initialize()
        {


            // Set Default Framerate Limit
            SetFramerateLimit(200);

            // Adjust View on Resize
            Resized += (sender, e) =>
            {
                var currentView = GetView();
                currentView.Size = new Vector2f(e.Width, e.Height);
                SetView(currentView);
            };

            // Window Close Event
            Closed += (sender, e) => base.Close();

            // Key States
            KeyPressed += (sender, e) =>
            {
                if (!KeyStates.ContainsKey(e.Code) || !KeyStates[e.Code])
                {
                    var key = new KeyEventFlag { Key = e.Code, KeyEvent = KeyEvent.Pressed, FireOnce = true };
                    if (KeyEventActions.ContainsKey(key))
                    {
                        key = KeyEventActions.Keys.First(k => k.Equals(key));
                        if (key.HasCooldown)
                            FireIfCooldownDone(key);
                        else
                            KeyEventActions[key].ForEach(a => a());
                    }
                }
                KeyStates[e.Code] = true;
            };
            KeyReleased += (sender, e) =>
            {
                if (KeyStates.ContainsKey(e.Code) && KeyStates[e.Code])
                {
                    var key = new KeyEventFlag { Key = e.Code, KeyEvent = KeyEvent.Released, FireOnce = true };
                    if (KeyEventActions.ContainsKey(key))
                    {
                        key = KeyEventActions.Keys.First(k => k.Equals(key));
                        if (key.HasCooldown)
                            FireIfCooldownDone(key);
                        else
                            KeyEventActions[key].ForEach(a => a());
                    }
                }
                KeyStates[e.Code] = false;
            };
            SetDefaultFont();
        }
        public void SetDefaultFont(string? fileName = null)
        {
            if (string.IsNullOrEmpty(fileName) || !File.Exists(fileName))
            {
                var bytes = Properties.Resources.arial;
                DefaultFont = new Font(bytes);
                return;
            }
            DefaultFont = new Font(fileName);
        }

        public void SetDefaultFont(Font font)
        {
            DefaultFont = font;
        }


        #endregion

        public void Run()
        {
            Initialize();
            var stopwatch = Stopwatch.StartNew();
            while (IsOpen)
            {
                Clear(DrawingOptions.WindowClearColor);
                DispatchEvents();
                stopwatch.Restart();
                FireKeyEventActions();
                DrawScene();
                OnUpdate?.Invoke(this, new());
#if DEBUG

                Logger.StdOut(Logger.CreateDebugText().ToString());
#endif
                base.Display();
                DeltaTime = stopwatch.Elapsed;

            }

        }



        private void DrawScene()
        {
            var drawings = ActiveScene.GameDrawings.GetAll().ToArray();
            var collisions = new Dictionary<string, (IGameDrawable, IGameDrawable)>();
            var deleteList = new List<IGameDrawable>();

            while (ActiveScene.RawDrawingspLater.Count > 0) // DRAW QUEUED RAW DRAWINGS
            {
                var drawing = ActiveScene.RawDrawingspLater.Dequeue();
                Draw(drawing);
            }

            ActiveScene.ResetAllIsCollidedProperties();
            for (int i = 0; i < drawings.Length; i++)
            {
                var drawing = drawings[i];
                if (!drawing.Visible) continue;
                if (drawing.IsDestroyed)
                {
                    deleteList.Add(drawing);
                    continue;
                }
                drawing.RaiseOnDrawingEvent(); // START DRAWING EVENT
                if (drawing is IAnimated animated) animated.UpdateAnimation();
                drawing.ApplyGravityIfApplicable(GravityVector);
                for (var j = i + 1; j < drawings.Length; j++) // SIMULATE PHYSICS & COLLISION AGANST OTHER DRAWINGS
                {
                    var other = drawings[j];
                    var isCollided = drawing.CheckAndResolveCollision(other);
                    if (isCollided)
                    {
                        collisions.Add(drawing.GetPairId(other), (drawing, other));
                    }
                }
                drawing.SimulatePhysics();
                Draw(drawing);
                drawing.RaiseOnDrawedEvent(); // END DRAWING EVENT

                DrawDrawableUtils(drawing); // DRAW UTILS FOR DRAWABLE (BOUNDARY BOX, COLLISION BOX, ORIGIN POINT, CENTER POINT, TEXTS, etc...)
            }


            deleteList.ForEach(item => ActiveScene.GameDrawings.Remove(item)); // DELETE DESTROYED DRAWINGS

            while (ActiveScene.RawDrawingspPrior.Count > 0) // DRAW QUEUED RAW DRAWINGS
            {
                var drawing = ActiveScene.RawDrawingspPrior.Dequeue();
                Draw(drawing);
            }

            DrawViewUtils();
            SetNewCollisions(collisions); // SET NEW COLLISIONS
        }

        public static IEnumerable<IGameDrawable> GetCollisions(IGameDrawable drawable)
        {
            foreach (var collision in CurrentCollisions)
            {
                var (d1, d2) = collision.Value;
                if (d1 == drawable || d2 == drawable)
                    yield return collision.Value.Other(drawable);
            }
        }
        private void SetNewCollisions(Dictionary<string, (IGameDrawable, IGameDrawable)> newCollisions)
        {
            var currentCollisionKeys = newCollisions.Keys;
            var previousCollisionKeys = CurrentCollisions.Keys;
            var newCollisionKeys = currentCollisionKeys.Except(previousCollisionKeys);
            var removedCollisionKeys = previousCollisionKeys.Except(currentCollisionKeys);
            foreach (var newcol in newCollisionKeys)
            {
                var (d1, d2) = newCollisions[newcol];
                d1.Physics.RaiseOnCollision(d2);
                d2.Physics.RaiseOnCollision(d1);
            }

            foreach (var sep in removedCollisionKeys)
            {
                var (d1, d2) = CurrentCollisions[sep];
                d1.Physics.RaiseOnSeparation(d2);
                d2.Physics.RaiseOnSeparation(d1);
            }

            CurrentCollisions = newCollisions;
        }

        private void DrawDrawableUtils(IGameDrawable drawable)
        {
            var opt = DrawingOptions;
            var text = "";

            if (opt.ShowBoundaryBox)
            {
                var boundaryBox = drawable.GetGlobalBounds().ToRectangleShape();
                boundaryBox.OutlineColor = opt.BoundaryBoxColor;
                boundaryBox.OutlineThickness = opt.BoundaryBoxThickness;
                boundaryBox.FillColor = new(255, 255, 255, 10);
                Draw(boundaryBox);
            }

            if (opt.ShowCollisionBox && drawable.IsPhysicsConfigured())
            {
                var collisionBox = drawable.GetOBBox()?.AsRectangleShape();

                if (collisionBox != null)
                {
                    collisionBox.FillColor = new(0, 0, 0, 0);
                    collisionBox.OutlineColor = opt.CollisionBoxColor;
                    collisionBox.OutlineThickness = opt.CollisionBoxThickness;
                    Draw(collisionBox);
                }
            }

            if (DefaultFont != null)
            {

                if (opt.ShowIds)
                    text += $"Id: {drawable.Id}\n";

                if (opt.ShowTags)
                    text += $"Tag: {drawable.Tag}\n";

                if (opt.ShowAnimation && drawable is IAnimated animated)
                    text += $"Animation: {animated.Animation?.Id}\n";

                if (opt.ShowCurrentState && drawable is IStateful stateful)
                    text += stateful.CurrentStateName + "\n";

                if (opt.ShowPositions)
                    text += $"Position: {drawable.GetGlobalBounds().Position.ToString()}\n";
            }

            if (drawable.IsCollidable() && opt.ShowCollidedIds)
            {
                var collidedIds = GetCollisions(drawable).Select(p => p.Id);
                text += $"Colliding: {string.Join(",", collidedIds)}\n";
            }


            if (drawable is Transformable transformable)
            {
                if (opt.ShowOriginPoint)
                {
                    var originGlobal = transformable.GetOriginGlobalPosition();
                    var originLocal = transformable.Origin;

                    text += $"Origin G/R:X:{originGlobal.X.ToString("f0")} Y:{originGlobal.Y.ToString("f0")} / X:{originLocal.X.ToString("f0")} Y:{originLocal.Y.ToString("f0")}  \n";
                    var originPoint = drawable.GetCenterPoint(opt.OriginPointColor, opt.OriginPointRadius);

                    Draw(originPoint);
                }

                if (opt.ShowCenterPoint)
                {
                    text += $"Center: {drawable.GetCenter().ToString()}\n";
                    var centerPoint = drawable.GetCenterPoint(opt.CenterPointColor, opt.CenterPointThickness);
                    Draw(centerPoint);
                }

            }


            if (!string.IsNullOrEmpty(text))
            {
                var descriptionText = new Text(text, DefaultFont, opt.TextSize)
                {
                    Position = drawable.GetGlobalBounds().Position,
                    FillColor = opt.TextColor
                };
                Draw(descriptionText);
            }
        }

        private void DrawViewUtils()
        {
            if (first5FramesCounter < 5)
            {
                first5FramesCounter++;
                return;
            }

            var opt = DrawingOptions;
            var text = "";
            if (opt.ShowFPS)
            {
                if (opt.UseAverageFPS > 0)
                {
                    if (averageFlag)
                        fps = Enumerable.Repeat(DeltaFPS, opt.UseAverageFPS).ToArray();
                    fps[fpsIndex] = DeltaFPS;
                    fpsIndex = (fpsIndex + 1) % opt.UseAverageFPS;
                    text += $"FPS: {fps.Average().ToString("F2")}\n";
                }
                else
                    text += $"FPS: {DeltaFPS.ToString("F2")}\n";
            }

            if (opt.ShowDeltaTime)
            {
                if (opt.UseAverageDeltaTime > 0)
                {

                    if (averageFlag)
                        deltaMs = Enumerable.Repeat((float)DeltaTime.TotalMilliseconds, opt.UseAverageDeltaTime).ToArray();
                    deltaMs[deltaIndex] = (float)DeltaTime.TotalMilliseconds;
                    deltaIndex = (deltaIndex + 1) % opt.UseAverageDeltaTime;
                    text += $"DeltaTime: {deltaMs.Average().ToString("F2")} ms\n";
                }
                else
                    text += $"DeltaTime: {DeltaTime.TotalMilliseconds.ToString("F2")} ms\n";
            }


            if (opt.ShowViewSize)
            {
                text += $"View Size: X:{GetView().Size.X} Y:{GetView().Size.Y}\n";
            }

            if (opt.ShowViewCenter)
            {
                text += $"View Center: X:{GetView().Center.X} Y:{GetView().Center.Y}\n";
            }


            var viewTopLeft = GetView().Center - GetView().Size / 2;
            if (opt.ShowViewMoved)
            {

                text += $"View Moved: X:{viewTopLeft.X} Y:{viewTopLeft.Y}\n";
            }

            if (opt.ShowMousePosition)
            {
                var mousePosition = Mouse.GetPosition(this);
                text += $"Mouse Position: X:{mousePosition.X} Y:{mousePosition.Y}\n";
            }

            if (!string.IsNullOrEmpty(text))
            {
                var descriptionText = new Text(text, DefaultFont, opt.TextSize)
                {
                    Position = viewTopLeft,
                    FillColor = opt.TextColor
                };
                Draw(descriptionText);
            }



            averageFlag = false;
        }
    }
}
