using RealGame.Application.GameObjects;
using RealGame.GameEngine;
using RealGame.GameEngine.Entities.EntityObjects;
using RealGame.GameEngine.Entities.GameEntities;
using SFML.Graphics;
using SFML.System;
using SFML.Window;


namespace RealGame.Application
{
    internal class GameTask
    {

        public GameWindow GameWindow = new GameWindow(VideoMode.DesktopMode, "SPECIAL GAME", Styles.Fullscreen);
        private GameScene ActiveScene => GameWindow.ActiveScene;
        private float MinimumAltitute = 680f;
        public void Run() => GameWindow.Run();
        public event EventHandler<Drone>? OnPlayerDeath;

        public enum CharacterState
        {
            Idle,
            Walking,
            Running,
            Jumping,
            Falling,
            Attacking,
            Dead
        }


        /// <summary>
        /// Class that constructs game using GameEngine library amd starts it.
        /// </summary>
        public GameTask()
        {

            const float movementSpeed = .05f;
            GameWindow.ConfigureDrawingOptions(o =>
            {
                //o.ShowBoundaries = true;
                //o.ShowCenterPoint = true;
                //o.ShowOriginPoint = true;
                //o.ShowCollisionBox = true;
                //o.ShowIds = true;
                //o.ShowTags = true;
                //o.ShowCurrentState = true;
                ////o.ShowAnimation = true;
                ////o.ShowCollidedIds = true;
                //o.ShowFPS = true;
                //o.ShowViewMoved = true;
                ////o.ShowViewSize = true;
                ////o.ShowViewCenter = true;
                //o.ShowDeltaTime = true;
                //o.UseAverageDeltaTime = 100;
                //o.UseAverageFPS = 100;
                ////o.ShowBoundaryBox = true;
                //o.ShowMousePosition = true;
                //o.WindowClearColor = Color.Black;
                //o.TextColor = Color.White;
                //o.ShowPositions = true;
            });
            //game.EnableViewScrollZoom();
            //game.EnableViewDragMove();



            #region Drone 1 Textures
            var txDrone1Idle = new GameTexture(Properties.Resources.drone1_idle);
            var txDrone1Move = new GameTexture(Properties.Resources.drone1_moving);
            //var txDrone1Attack = new GameTexture(Properties.Resources.drone1_attack);
            var txDrone1Dead = new GameTexture(Properties.Resources.drone1_dead);
            #endregion

            #region Drone 2 Textures

            var txDrone2Idle = new GameTexture(Properties.Resources.drone2_idle);
            var txDrone2Move = new GameTexture(Properties.Resources.drone2_moving);
            //var txDrone2Attack = new GameTexture(Properties.Resources.drone2_attack);
            var txDrone2Dead = new GameTexture(Properties.Resources.drone2_dead);

            #endregion


            #region Base 1 Textures
            var txBase1Idle = new GameTexture(Properties.Resources.base1_idle);
            var txBase1Damage = new GameTexture(Properties.Resources.base1_damage);
            #endregion

            #region Base 2 Textures
            var txBase2Idle = new GameTexture(Properties.Resources.base2_idle);
            var txBase2Damage = new GameTexture(Properties.Resources.base2_damage);
            #endregion


            #region Base 1 Animations
            var animBase1Idle = new Animation("base1_idle");
            animBase1Idle.SetLoop(txBase1Idle, new(72, 72), TimeSpan.FromMilliseconds(50));
            animBase1Idle.RectIndexRange = (0, 3);

            var animBase1Damage = new Animation("base1_damage");
            animBase1Damage.SetOnce(txBase1Damage, new(72, 72), TimeSpan.FromMilliseconds(50), (a) => { a.Reset(TimeSpan.FromMilliseconds(50)); });
            animBase1Damage.RectIndexRange = (0, 5);
            #endregion

            #region Base 2 Animations
            var animBase2Idle = new Animation("base2_idle");
            animBase2Idle.SetLoop(txBase2Idle, new(72, 72), TimeSpan.FromMilliseconds(50));
            animBase2Idle.RectIndexRange = (0, 3);

            var animBase2Damage = new Animation("base2_damage");
            animBase2Damage.SetOnce(txBase2Damage, new(72, 72), TimeSpan.FromMilliseconds(50), (a) => { a.Reset(TimeSpan.FromMilliseconds(50)); });
            animBase2Damage.RectIndexRange = (0, 5);
            #endregion

            #region Drone 1 Animations
            var animDrone1Idle = new Animation("drone1_idle");
            animDrone1Idle.SetLoop(txDrone1Idle, new(48, 48), TimeSpan.FromMilliseconds(150));
            animDrone1Idle.RectIndexRange = (0, 7);

            var animDrone1Attack = new Animation("drone1_attack");
            animDrone1Attack.SetOnce(txDrone1Idle, new(48, 48), TimeSpan.FromMilliseconds(150), anim => anim.Reset(new TimeSpan(0, 0, 1)));
            animDrone1Attack.RectIndexRange = (0, 7);

            var animDrone1Move = new Animation("drone1_move");
            animDrone1Move.SetLoop(txDrone1Move, new(48, 48), TimeSpan.FromMilliseconds(150));
            animDrone1Move.RectIndexRange = (0, 7);

            var animDrone1Dead = new Animation("drone1_dead");
            animDrone1Dead.SetOnce(txDrone1Dead, new(48, 48), TimeSpan.FromMilliseconds(150));
            animDrone1Dead.RectIndexRange = (0, 5);
            #endregion

            #region Drone 2 Animations

            var animDrone2Idle = new Animation("drone2_idle");
            animDrone2Idle.SetLoop(txDrone2Idle, new(48, 48), TimeSpan.FromMilliseconds(150));
            animDrone2Idle.RectIndexRange = (0, 7);

            var animDrone2Attack = new Animation("drone2_attack");
            animDrone2Attack.SetOnce(txDrone2Idle, new(48, 48), TimeSpan.FromMilliseconds(150), anim => anim.Reset(new TimeSpan(0, 0, 1)));
            animDrone2Attack.RectIndexRange = (0, 7);

            var animDrone2Move = new Animation("drone2_move");
            animDrone2Move.SetLoop(txDrone2Move, new(48, 48), TimeSpan.FromMilliseconds(150));
            animDrone2Move.RectIndexRange = (0, 7);

            var animDrone2Dead = new Animation("drone2_dead");
            animDrone2Dead.SetOnce(txDrone2Dead, new(48, 48), TimeSpan.FromMilliseconds(150));
            animDrone2Dead.RectIndexRange = (0, 5);
            #endregion


            #region Drone 1 - Entity & States
            var drone1Keys = new Dictionary<Keyboard.Key, DroneState>()
            {
                {Keyboard.Key.W, DroneState.MovingUp},
                {Keyboard.Key.A, DroneState.MovingLeft},
                {Keyboard.Key.S, DroneState.MovingDown},
                {Keyboard.Key.D, DroneState.MovingRight},
                {Keyboard.Key.Space, DroneState.Attacking}
            };

            var drone1 = new Drone("drone1", drone1Keys);
            drone1[DroneState.Idle] = animDrone1Idle;
            drone1[DroneState.MovingUp] = animDrone1Move;
            drone1[DroneState.MovingDown] = animDrone1Move;
            drone1[DroneState.MovingLeft] = animDrone1Move;
            drone1[DroneState.MovingRight] = animDrone1Move;
            drone1[DroneState.Attacking] = animDrone1Attack;
            drone1[DroneState.Dead] = animDrone1Dead;
            #endregion

            #region Drone 2 - Entity & States
            var drone2Keys = new Dictionary<Keyboard.Key, DroneState>()
            {
                {Keyboard.Key.Numpad8, DroneState.MovingUp},
                {Keyboard.Key.Numpad4, DroneState.MovingLeft},
                {Keyboard.Key.Numpad5, DroneState.MovingDown},
                {Keyboard.Key.Numpad6, DroneState.MovingRight},
                {Keyboard.Key.Num0, DroneState.Attacking}
            };

            var drone2 = new Drone("drone2", drone2Keys);

            drone2[DroneState.Idle] = animDrone2Idle;
            drone2[DroneState.MovingUp] = animDrone2Move;
            drone2[DroneState.MovingDown] = animDrone2Move;
            drone2[DroneState.MovingLeft] = animDrone2Move;
            drone2[DroneState.MovingRight] = animDrone2Move;
            drone2[DroneState.Attacking] = animDrone2Attack;
            drone2[DroneState.Dead] = animDrone2Dead;
            #endregion


            #region Base 1 - Entity & States
            var base1 = new BasePlatform("base1", drone1);
            base1[BaseState.Idle] = animBase1Idle;
            base1[BaseState.GotHit] = animBase1Damage;
            base1.ConfigurePhysics(p =>
            {
                p.IsStatic = true;
                p.CollisionBoxTrimHeight = 35;
            });
            #endregion

            #region Base 2 - Entity & States
            var base2 = new BasePlatform("base2", drone2);
            base2[BaseState.Idle] = animBase2Idle;
            base2[BaseState.GotHit] = animBase2Damage;
            base2.ConfigurePhysics(p =>
            {
                p.IsStatic = true;
                p.CollisionBoxTrimHeight = 35;
            });
            #endregion



            var wp = GameWindow.GetViewport(GameWindow.GetView());
            var drone1Position = new Vector2f(wp.Size.X / 4, 200);
            var drone2Position = new Vector2f(wp.Size.X / 4 * 3, 200);
            drone2.Position = drone2Position;
            drone1.Position = drone1Position;

            base2.Position = new Vector2f(wp.Size.X / 4 * 3 - 72, wp.Size.Y - 200);
            base1.Position = new Vector2f(wp.Size.X / 4 - 72, wp.Size.Y - 200);


            drone1.Scale = new(2, 2);
            drone2.Scale = new(2, 2);
            base1.Scale = new(2, 2);
            base2.Scale = new(2, 2);



            for (var i = 0; i < 10; i++)
            {
                if (i > 3 && i < 7)
                    continue;
                var spike = new SpikeRoll(Guid.NewGuid().ToString());
                spike.Position = new(10, i * 64);
                spike.AddFriendlyWith(drone1);
                ActiveScene.Add(spike);
            }


            for (var i = 0; i < 10; i++)
            {
                if (i > 3 && i < 7)
                    continue;
                var spike = new SpikeRoll(Guid.NewGuid().ToString());
                spike.Position = new(wp.Size.X - 10, i * 64);
                spike.AddFriendlyWith(drone2);
                ActiveScene.Add(spike);
            }

            drone1.OnDeath += (a, b) => OnPlayerDeath?.Invoke(this, (Drone)a);
            drone2.OnDeath += (a, b) => OnPlayerDeath?.Invoke(this, (Drone)a);


            ActiveScene.Add(drone1);
            ActiveScene.Add(drone2);
            ActiveScene.Add(base1);
            ActiveScene.Add(base2);

            var bgTexture = new GameTexture(Properties.Resources.space_bg);
            var bg = new RectangleShape();
            bg.Texture = bgTexture;


            var topBorder = new GameRectangleShape();
            var bottomBorder = new GameRectangleShape();
            var leftBorder = new GameRectangleShape();
            var rightBorder = new GameRectangleShape();

            topBorder.ConfigurePhysics(p => p.IsStatic = true);
            bottomBorder.ConfigurePhysics(p => p.IsStatic = true);
            leftBorder.ConfigurePhysics(p => p.IsStatic = true);
            rightBorder.ConfigurePhysics(p => p.IsStatic = true);
            bottomBorder.Physics.OnCollision += (_, e) =>
            {
                if (e is Bomb bomb)
                {
                    bomb.IsDestroyed = true;
                }
            };


            ActiveScene.Add(topBorder);
            ActiveScene.Add(bottomBorder);
            ActiveScene.Add(leftBorder);
            ActiveScene.Add(rightBorder);

            var bombDrop1 = DateTime.Now;
            var bombDrop2 = DateTime.Now;

            GameWindow.AddKeyAction(Keyboard.Key.Space, GameEngine.KeyEvent.Pressed, () =>
            {
                var bomb = new Bomb();
                bomb.Position = drone1.Position + new Vector2f(0, 32);
                ActiveScene.Add(bomb);
                bombDrop1 = DateTime.Now;

            }, TimeSpan.FromSeconds(1));

            GameWindow.AddKeyAction(Keyboard.Key.Numpad0, GameEngine.KeyEvent.Pressed, () =>
            {
                var bomb = new Bomb();
                bomb.Position = drone2.Position + new Vector2f(0, 32);
                ActiveScene.Add(bomb);
                bombDrop2 = DateTime.Now;
            }, TimeSpan.FromSeconds(1));


            GameWindow.OnUpdate += (_, _) =>
            {
                var drones = ActiveScene.GetAll<Drone>();
                foreach (var dr in drones)
                {
                    if (dr.Position.Y > MinimumAltitute)
                    {
                        var difference = dr.Position.Y - MinimumAltitute;
                        var farce = new Vector2f(0, (float)(.05 * Math.Pow(Math.E, difference / 200)));
                        //var force = (float)Math.Pow(2.852582, 0.1* difference); 
                        // var normal = new Vector2f(0, -movementSpeed);
                        //new Vector2f(0, force);
                        // dr.Physics.Properties.Velocity *= force;
                        dr.Physics.Properties.Velocity -= farce;
                    }
                }

                var wp = GameWindow.GetView();

                float left = wp.Center.X - wp.Size.X / 2;
                float top = wp.Center.Y - wp.Size.Y / 2;
                float thickness = 50;

                // Top border
                topBorder.Size = new Vector2f(wp.Size.X, thickness);
                topBorder.Position = new Vector2f(left, top - thickness);
                topBorder.FillColor = Color.White;

                // Bottom border
                bottomBorder.Size = new Vector2f(wp.Size.X, thickness);
                bottomBorder.Position = new Vector2f(left, top + wp.Size.Y);
                bottomBorder.FillColor = Color.White;

                // Left border
                leftBorder.Size = new Vector2f(thickness, wp.Size.Y);
                leftBorder.Position = new Vector2f(left - thickness, top);
                leftBorder.FillColor = Color.White;

                // Right border
                rightBorder.Size = new Vector2f(thickness, wp.Size.Y);
                rightBorder.Position = new Vector2f(left + wp.Size.X, top);
                rightBorder.FillColor = Color.White;

                // Update low altitude background
                bg.Position = new Vector2f(left, top);
                bg.Size = wp.Size;

                // Enqueue background
                ActiveScene.Enqueue(bg);
            };


        }
    }
}
