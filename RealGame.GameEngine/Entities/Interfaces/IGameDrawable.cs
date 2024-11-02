using RealGame.GameEngine.ConfigurationObjects;
using RealGame.GameEngine.Entities.EntityObjects;
using SFML.Graphics;
using SFML.System;

namespace RealGame.GameEngine.Entities.Interfaces
{
    public interface IGameDrawable : Drawable, IIdentifiable
    {
        public int DrawingIndex { get; set; }
        public bool IsDestroyed { get; set; }
        public bool Visible { get; set; }

        public event EventHandler<IGameDrawable>? OnDrawing;

        public event EventHandler<IGameDrawable>? OnDrawed;

        public Vector2f Position { get; set; }
        public void RaiseOnDrawingEvent();
        public void RaiseOnDrawedEvent();

        public FloatRect GetGlobalBounds();
        public FloatRect GetLocalBounds();

        public void ConfigurePhysics(Action<PhysicalProperties> physicalProertyConfigurator);
        public Physics Physics { get; set; }

    }
}
