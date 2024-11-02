using RealGame.GameEngine;
using RealGame.GameEngine.Entities.GameEntities;
using RealGame.GameEngine.Entities.Interfaces;
using RealGame.GameEngine.Helpers;
using SFML.System;
using SFML.Window;

namespace RealGame.Application.GameObjects
{

    public enum DroneState
    {
        Idle,
        MovingRight,
        MovingLeft,
        MovingUp,
        MovingDown,
        Attacking,
        Dead
    }
    public class Drone : StatefulGameSprite<DroneState>
    {
        public float Speed { get; set; } = 0.1f;
        public float Damage { get; set; } = 10;
        public HealthBar Health { get; set; }
        public EventHandler? OnDeath;
        //public float MinimumAltitude = 40; // Will reprsent the minimum altitude the drone can fly at

        private bool _xFlip = false;

        private float _oscilationFrequencyX = .0f;
        private float _oscilationAmplitudeX = .5f;
        private float _oscilationFrequencyY = .01f;
        private float _oscilationAmplitudeY = .1f;
        private float _elapsedTime = 0;

        private void Oscilate()
        {
            _elapsedTime += GameWindow.DeltaTime.Milliseconds;

            float offsetX = _oscilationAmplitudeX * MathF.Sin(_oscilationFrequencyX * _elapsedTime);
            float offsetY = _oscilationAmplitudeY * MathF.Cos(_oscilationFrequencyY * _elapsedTime);

            Position += new Vector2f(offsetX, offsetY);

            float cycleDuration = 2 * MathF.PI / Math.Max(_oscilationFrequencyX, _oscilationFrequencyY);
            if (_elapsedTime > cycleDuration)
            {
                _elapsedTime -= cycleDuration;
            }
        }

        public Drone(string id, Dictionary<Keyboard.Key, DroneState> stateMap, string tag = "drone") : base(id, tag)
        {
            MapStateCondition(DroneState.Idle, () => !stateMap.Any(s => Keyboard.IsKeyPressed(s.Key)));

            foreach (var state in stateMap)
            {
                MapStateCondition(state.Value, () =>
                {
                    var res = Keyboard.IsKeyPressed(state.Key);
                    switch (CurrentState?.Type)
                    {
                        case DroneState.Idle:
                            break;
                        case DroneState.MovingRight:
                            this.ApplyForce(Speed, 0);
                            break;
                        case DroneState.MovingLeft:
                            this.ApplyForce(-Speed, 0);
                            break;
                        case DroneState.MovingUp:
                            this.ApplyForce(0, -Speed);
                            break;
                        case DroneState.MovingDown:
                            this.ApplyForce(0, Speed);
                            break;
                        case DroneState.Attacking:
                            break;
                        case DroneState.Dead:
                            break;
                        default:
                            break;
                    }
                    return res;
                });
            }

            Health = new HealthBar(100, 5)
            {
                OutlineColor = SFML.Graphics.Color.Yellow,
                OutlineThickness = 1,
                TrackingEntity = this,
                TrackingOffset = new Vector2f(0, -30),
            };

            Health.OnDamage += Health_OnDamage;

            OnDrawing += Drone_OnDrawing;
            OnDrawed += Drone_OnDrawed;
            ConfigurePhysics(p =>
            {
                p.Damping = 0.008f;
                p.Mass = 30;
                p.Restitution = .03f;
                p.CollisionBoxScale = new Vector2f(0.65f, 0.5f);
            });
            Physics.OnCollision += Physics_OnCollision;
        }

        private DateTime _lastDamagedAnimation = DateTime.Now;
        private void Health_OnDamage(object? sender, EventArgs e)
        {
            if (DateTime.Now - _lastDamagedAnimation < TimeSpan.FromMilliseconds(250))
                return;
            if (Health.CurrentHealth <= 0)
            {
                SetState(DroneState.Dead);
                IsDestroyed = true;
                OnDeath?.Invoke(this, EventArgs.Empty);
            }
            var newSmoke = new DamageSmoke(Position);
            GameWindow.ActiveScene.Add(newSmoke);
            _lastDamagedAnimation = DateTime.Now;
        }


        private void Physics_OnCollision(object? sender, IGameDrawable e)
        {
            if (e is Drone enemyDrone)
            {
                var mySpeed = Physics.Properties?.Velocity.Length() ?? 0;
                var enemySpeed = enemyDrone.Physics.Properties?.Velocity.Length() ?? 0;

                if (mySpeed > enemySpeed)
                {
                    enemyDrone.Health.Damage(mySpeed);
                }
            }
        }

        private void Drone_OnDrawed(object? sender, IGameDrawable e)
        {
            Oscilate();
        }

        private void Drone_OnDrawing(object? sender, IGameDrawable e)
        {

            if (CurrentState?.Type == DroneState.Dead && HasAnimation && Animation!.IsDone)
            {
                IsDestroyed = true;
                return;
            }

            GameWindow.ActiveScene.RawDrawingspPrior.Enqueue(Health);
            _xFlip = CurrentState?.Type == DroneState.MovingLeft;


            if (HasAnimation)
                Animation!.Flipped(_xFlip, false);

        }
    }
}
