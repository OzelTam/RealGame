using RealGame.GameEngine.ConfigurationObjects;
using RealGame.GameEngine.Entities.Interfaces;

namespace RealGame.GameEngine.Entities.EntityObjects
{
    public class Physics
    {
        public PhysicalProperties? Properties { get; set; } = null;

        public event EventHandler<IGameDrawable>? OnCollision;
        public event EventHandler<IGameDrawable>? OnSeparation;

        internal void RaiseOnCollision(IGameDrawable collidedObject)
        {
            OnCollision?.Invoke(this, collidedObject);
        }

        internal void RaiseOnSeparation(IGameDrawable collidedObject)
        {
            OnSeparation?.Invoke(this, collidedObject);
        }

        public Physics Clone()
        {
            var clone = new Physics();
            if (Properties != null)
                clone.Properties = Properties?.Clone();
            return clone;
        }

    }
}
