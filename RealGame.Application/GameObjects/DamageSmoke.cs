using RealGame.GameEngine;
using RealGame.GameEngine.Entities.EntityObjects;
using RealGame.GameEngine.Entities.GameEntities;
using SFML.System;

namespace RealGame.Application.GameObjects
{
    public class DamageSmoke : GameSprite
    {
        public DamageSmoke(Vector2f position, string tag = "smoke") : base(Guid.NewGuid().ToString(), tag)
        {
            var texture = GameWindow.ActiveScene.Textures.Get("smoke");
            if (texture == null)
            {
                texture = new GameTexture(Properties.Resources.smoke);
                GameWindow.ActiveScene.Textures.Add(texture);
            }
            var animation = new Animation();
            animation.SetOnce(texture, new(64, 64), TimeSpan.FromMilliseconds(50), (_) => Disappear());
            animation.RectIndexRange = new(11, 21);
            GameWindow.ActiveScene.Animations.Add(animation);
            this.Animation = animation;
            this.Scale = new Vector2f(2, 2);
            this.Position = position - new Vector2f(64, 64);

        }


        public void Disappear()
        {
            if (Animation != null)
                GameWindow.ActiveScene.Animations.Remove(Animation);
            this.IsDestroyed = true;
        }
    }
}
