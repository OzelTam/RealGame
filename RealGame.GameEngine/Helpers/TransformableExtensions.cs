using SFML.Graphics;
using SFML.System;

namespace RealGame.GameEngine.Helpers
{
    public enum Direction
    {
        Up,
        Down,
        Left,
        Right,
        UpLeft,
        UpRight,
        DownLeft,
        DownRight,
    }

    public partial class Extensions
    {
        public static void MoveTowards(this Transformable transformable, Vector2f position, float unit = 0.1f)
        {
            var currentPos = transformable.Position;
            var direction = position - currentPos;
            var distance = Math.Sqrt(direction.X * direction.X + direction.Y * direction.Y);
            if (distance != 0)
            {
                var normalizedDirection = new Vector2f(direction.X / (float)distance, direction.Y / (float)distance);
                var movement = normalizedDirection * unit * GameWindow.DeltaTime.Milliseconds;
                if (distance < unit)
                {
                    transformable.Position = position;
                }
                else
                {
                    transformable.Position += movement;
                }
            }
        }
        public static void MoveTowards(this Transformable transformable, Transformable target, float unit = 0.1f)
        {
            transformable.MoveTowards(target.Position, unit);
        }
        public static Vector2f GetOffset(this Direction dir, float unit = 1)
        {
            switch (dir)
            {
                case Direction.Up:
                    return new Vector2f(0, -unit);
                case Direction.Down:
                    return new Vector2f(0, unit);
                case Direction.Left:
                    return new Vector2f(-unit, 0);
                case Direction.Right:
                    return new Vector2f(unit, 0);
                case Direction.UpLeft:
                    return new Vector2f(-unit, -unit);
                case Direction.UpRight:
                    return new Vector2f(unit, -unit);
                case Direction.DownLeft:
                    return new Vector2f(-unit, unit);
                case Direction.DownRight:
                    return new Vector2f(unit, unit);
                default:
                    return new Vector2f(0, 0);
            }

        }
        public static Vector2f GetOffset(this IEnumerable<Direction> directions, float unit = 1) => directions.Select(d => d.GetOffset(unit)).Aggregate((a, b) => a + b);
        public static void Move(this Transformable transformable, Direction direction, float unit = 0.1f)
        {
            var offset = direction.GetOffset(unit);
            var targetPosition = transformable.Position + new Vector2f(offset.X, offset.Y);
            transformable.MoveTowards(targetPosition, unit);
        }
        public static void Move(this Transformable transformable, float unit = 0.1f, params Direction[] directions)
        {
            var offset = directions.Select(d => d.GetOffset(unit)).Aggregate((a, b) => a + b);
            var targetPosition = transformable.Position + new Vector2f(offset.X, offset.Y);
            transformable.MoveTowards(targetPosition, unit);

        }

        public static void Move(this Transformable transformable, Vector2f offset)
        {
            transformable.Position += offset;
        }


    }


}
