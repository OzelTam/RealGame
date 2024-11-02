using RealGame.GameEngine;
using RealGame.GameEngine.Entities.GameEntities;
using RealGame.GameEngine.Entities.Interfaces;
using SFML.System;

namespace RealGame.Application.GameObjects
{
    public enum BaseState
    {
        Idle,
        GotHit,
    }
    public class BasePlatform : StatefulGameSprite<BaseState>
    {

        private float _oscilationFrequencyX = .001f;
        private float _oscilationAmplitudeX = .08f;


        private float _oscilationFrequencyY = .01f;
        private float _oscilationAmplitudeY = .15f;

        private string _ownerId;
        private float _elapsedTime = 0;
        private void Oscilate()
        {
            _elapsedTime += GameWindow.DeltaTime.Milliseconds;

            float offsetX = _oscilationAmplitudeX * MathF.Sin(_oscilationFrequencyX * _elapsedTime);
            float offsetY = _oscilationAmplitudeY * MathF.Cos(_oscilationFrequencyY * _elapsedTime);

            Position += new Vector2f(offsetX, offsetY);

            float cycleDuration = 2 * MathF.PI / Math.Min(_oscilationFrequencyX, _oscilationFrequencyY);
            if (_elapsedTime > cycleDuration)
            {
                _elapsedTime -= cycleDuration;
            }
        }


        public BasePlatform(string id, Drone owner, string tag = "base") : base(id, tag)
        {
            _ownerId = owner.Id;
            var random = new Random();
            _elapsedTime = (float)(random.NextDouble() * (1000 - 0) + 0);
            OnDrawing += BasePlatform_OnDrawing;
            ConfigurePhysics();
            Physics.OnCollision += Physics_OnCollision;
        }

        private void BasePlatform_OnDrawing(object? sender, IGameDrawable e)
        {
            Oscilate();
            if (CurrentState?.Type == BaseState.GotHit && Animation!.IsDone)
            {
                SetState(BaseState.Idle);
            }
        }

        private void Physics_OnCollision(object? sender, IGameDrawable e)
        {
            if (e is Bomb bomb)
            {
                var owner = GameWindow.ActiveScene.Get<Drone>(_ownerId);
                if (owner == null || bomb.IsDestroyed)
                {
                    IsDestroyed = true;
                    SetState(BaseState.GotHit);


                    return;
                }
                owner.Health.Damage(bomb.Damage);
                SetState(BaseState.GotHit);

            }
        }
    }
}
