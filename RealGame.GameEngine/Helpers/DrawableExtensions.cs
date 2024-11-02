using RealGame.GameEngine.Entities.EntityObjects;
using RealGame.GameEngine.Entities.Interfaces;
using SFML.Graphics;
using SFML.System;

namespace RealGame.GameEngine.Helpers
{
    public static partial class Extensions
    {

        public static RectangleShape ToRectangleShape(this FloatRect rect, Color fillColor, Color outlineColor, float outlineThickness)
        {
            var shape = new RectangleShape(new SFML.System.Vector2f(rect.Width, rect.Height))
            {
                Position = new SFML.System.Vector2f(rect.Left, rect.Top),
                FillColor = fillColor,
                OutlineColor = outlineColor,
                OutlineThickness = outlineThickness
            };
            return shape;
        }
        public static RectangleShape ToRectangleShape(this FloatRect rect)
        {
            var shape = new RectangleShape(new SFML.System.Vector2f(rect.Width, rect.Height))
            {
                Position = new SFML.System.Vector2f(rect.Left, rect.Top),
            };
            return shape;
        }

        public static FloatRect Trim(this FloatRect rect, float trimWidth, float trimHeight)
        {
            var newRect = new FloatRect(rect.Left + trimWidth / 2, rect.Top + trimHeight / 2, rect.Width - trimWidth, rect.Height - trimHeight);
            return newRect;
        }

        public static FloatRect Scale(this FloatRect rect, Vector2f scale)
        {
            var newRect = new FloatRect(rect.Left, rect.Top, rect.Width * scale.X, rect.Height * scale.Y);
            newRect.Left = rect.Left - (newRect.Width - rect.Width) / 2;
            newRect.Top = rect.Top - (newRect.Height - rect.Height) / 2;
            return newRect;
        }

        public static FloatRect GetGlobalBounds(this IGameDrawable drawable, Vector2f scale) => drawable.GetGlobalBounds().Scale(scale);

        public static FloatRect GetGlobalBounds(this IGameDrawable drawable, float trimWidth, float trimHeight) => drawable.GetGlobalBounds().Trim(trimWidth, trimHeight);

        public static FloatRect GetLocalBounds(this IGameDrawable drawable, float scale)
        {
            var currentBounds = drawable.GetLocalBounds();
            var bounds = drawable.GetLocalBounds();
            bounds.Height *= scale;
            bounds.Width *= scale;
            bounds.Left = currentBounds.Left - (bounds.Width - currentBounds.Width) / 2;
            bounds.Top = currentBounds.Top - (bounds.Height - currentBounds.Height) / 2;
            return bounds;
        }

        public static FloatRect GetLocalBounds(this IGameDrawable drawable, float trimWidth, float trimHeight)
        {
            var bounds = drawable.GetLocalBounds();
            bounds.Width -= trimWidth;
            bounds.Height -= trimHeight;
            bounds.Left += trimWidth / 2;
            bounds.Top += trimHeight / 2;
            return bounds;
        }

        public static RectangleShape GetBoundaryBox(this IGameDrawable drawable, Color outlineColor, float outlineThickness)
        {
            var bounds = drawable.GetGlobalBounds();
            var box = new RectangleShape(new SFML.System.Vector2f(bounds.Width, bounds.Height))
            {
                Position = new SFML.System.Vector2f(bounds.Left, bounds.Top),
                FillColor = new Color(0, 0, 0, 0),
                OutlineColor = outlineColor,
                OutlineThickness = outlineThickness,
            };
            return box;
        }

        public static Vector2f GetOriginGlobalPosition(this Transformable transformable)
        {
            return transformable.Origin + transformable.Position;
        }

        public static Vector2f GetCenter(this IGameDrawable drawable)
        {
            var bounds = drawable.GetGlobalBounds();
            return new Vector2f(bounds.Left + bounds.Width / 2, bounds.Top + bounds.Height / 2);
        }

        public static CircleShape GetCenterPoint(this IGameDrawable drawable, Color color, float radius)
        {
            var center = drawable.GetCenter();
            var point = new CircleShape(radius)
            {
                FillColor = color,
                Origin = new(radius / 2, radius / 2),
                Position = center
            };
            return point;
        }

        public static bool IsPhysicsConfigured(this IGameDrawable drawable)
        {
            return drawable.Physics != null && drawable.Physics.Properties != null;
        }

        public static bool IsPhysicsConfigured(this Physics physics)
        {
            return physics != null && physics.Properties != null;
        }

        public static bool IsCollidable(this IGameDrawable drawable)
        {
            return drawable.IsPhysicsConfigured() && drawable.Physics.Properties!.CollisionEnabled;
        }

        public static bool ApplyGravityIfApplicable(this IGameDrawable drawable, Vector2f gravityVector)
        {
            if (!drawable.IsPhysicsConfigured() ||
                drawable.Physics.Properties!.IsStatic ||
                drawable.Physics.Properties!.Mass == 0 ||
                gravityVector == new Vector2f(0, 0) ||
                drawable.Physics.Properties!.IsCollidingWithStatic)
                return false;

            drawable.ApplyForce(gravityVector);
            return true;
        }
        public static string GetPairId(this IGameDrawable drawable, IGameDrawable other)
        {
            var maxId = Math.Max(drawable.Id.GetHashCode(), other.Id.GetHashCode());
            var minId = Math.Min(drawable.Id.GetHashCode(), other.Id.GetHashCode());
            return $"{maxId}.{minId}";

        }

        public static string GetPosition(this IGameDrawable drawable)
        {
            return $"{drawable.Position.X},{drawable.Position.Y}";
        }
        public static IGameDrawable Other(this (IGameDrawable, IGameDrawable) pair, IGameDrawable drawable)
        {
            return pair.Item1.Id == drawable.Id ? pair.Item2 : pair.Item1;
        }
        public static IEnumerable<IGameDrawable> Collisions(this IGameDrawable drawable) => GameWindow.GetCollisions(drawable);
        public static bool HasCollision(this IGameDrawable drawable) => GameWindow.GetCollisions(drawable).Any();
    }
}
