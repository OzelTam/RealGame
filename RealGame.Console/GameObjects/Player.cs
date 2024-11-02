using RealGame.GameEngine;
using RealGame.GameEngine.Entities.GameEntities;
using RealGame.GameEngine.Entities.Interfaces;
using RealGame.GameEngine.Helpers;
using SFML.System;
using SFML.Window;

namespace RealGame.Application.GameObjects
{
    public enum PlayerState
    {
        WalkingRight,
        WalkingLeft,
        WalkingUp,
        WalkingDown,
        Jumping,
        Falling,
        Idle,
        Attacking,
    }
    internal class Player : StatefulGameSprite<PlayerState>
    {
        public Direction Facing { get; set; } = Direction.Down;

        public HealthBar Health { get; set; }

        public Player(string id, Dictionary<Keyboard.Key, PlayerState> stateMap, string tag = "stateSprite") : base(id, tag)
        {
            foreach (var state in stateMap)
            {
                MapStateCondition(state.Value, () => Keyboard.IsKeyPressed(state.Key));
            }
            MapStateCondition(PlayerState.Idle, () => !stateMap.Any(s => Keyboard.IsKeyPressed(s.Key)));
            OnDrawing += Player_OnDrawing;
            Health = new HealthBar(100)
            {
                TrackingEntity = this,
                TrackingOffset = new Vector2f(0, -50)

            };
            ConfigurePhysics();


        }

        public void MapStateEvent(PlayerState state, Action action)
        {
            OnDrawing += (_, _) =>
            {
                if (CurrentState?.Type == state)
                {
                    action();
                }
            };

        }

        private void Player_OnDrawing(object? sender, IGameDrawable e)
        {

            GameWindow.ActiveScene.RawDrawingspPrior.Enqueue(Health);
            switch (CurrentState?.Type)
            {
                case PlayerState.WalkingRight:
                    Facing = Direction.Right;
                    break;
                case PlayerState.WalkingLeft:
                    Facing = Direction.Left;
                    break;
                default:
                    break;
            }

            if (Facing == Direction.Left && HasAnimation)
            {
                Animation!.Flipped(true, false);
            }
            else if (Facing == Direction.Right && HasAnimation)
            {
                Animation!.Flipped(false, false);
            }
            else
            {
                Animation!.Flipped(false, false);
            }


        }




    }
}
