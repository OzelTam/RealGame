using RealGame.GameEngine.ConfigurationObjects;
using RealGame.GameEngine.Entities.EntityObjects;
using RealGame.GameEngine.Entities.Interfaces;
using SFML.Graphics;

namespace RealGame.GameEngine.Entities.GameEntities
{
    public class GameText : Text, IGameDrawable
    {
        public string Id { get; init; }
        public string Tag { get; init; } = "";
        public int DrawingIndex { get; set; }
        public bool IsDestroyed { get; set; }
        public bool Visible { get; set; } = true;

        public GameText() : base()
        {
            Id = Guid.NewGuid().ToString();
            Physics = new();
        }
        public GameText(string str, Font font, string id, string tag = "text") : base(str, font) { Id = id; Tag = tag; Physics = new(); }

        public GameText(string str, Font font, uint characterSize, string id, string tag = "text") : base(str, font, characterSize) { Id = id; Tag = tag; Physics = new(); }

        public GameText(GameText copyText, string id, string tag) : base(copyText) { Id = id; Tag = tag; Physics = new(); }

        public Physics Physics { get; set; }

        public event EventHandler<IGameDrawable>? OnDrawing;
        public event EventHandler<IGameDrawable>? OnDrawed;

        public void ConfigurePhysics(Action<PhysicalProperties> physicalProertyConfigurator)
        {
            Physics = new Physics();
            var physicalProerty = new PhysicalProperties();
            physicalProertyConfigurator(physicalProerty);
            Physics.Properties = physicalProerty;
        }

        public void RaiseOnDrawingEvent()
        {
            OnDrawing?.Invoke(this, this);
        }

        public void RaiseOnDrawedEvent()
        {
            OnDrawed?.Invoke(this, this);
        }
    }

}
