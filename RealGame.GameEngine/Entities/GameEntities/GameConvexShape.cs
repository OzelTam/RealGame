using RealGame.GameEngine.ConfigurationObjects;
using RealGame.GameEngine.Entities.EntityObjects;
using RealGame.GameEngine.Entities.Interfaces;
using SFML.Graphics;

namespace RealGame.GameEngine.Entities.GameEntities
{
    public class GameConvexShape : ConvexShape, IGameShape
    {
        public string Id { get; init; }
        public string Tag { get; init; }
        public int DrawingIndex { get; set; }
        public bool IsDestroyed { get; set; }
        public bool Visible { get; set; }

        public GameConvexShape(string? id, string tag = "convex")
        {
            Id = string.IsNullOrEmpty(id) ? Guid.NewGuid().ToString() : id;
            Tag = tag;
            DrawingIndex = 0;
            IsDestroyed = false;
            Visible = true;
            Physics = new();
        }

        public GameConvexShape(GameConvexShape copyShape, string? id = null, string tag = "convex") : base(copyShape)
        {
            Id = string.IsNullOrEmpty(id) ? Guid.NewGuid().ToString() : id;
            Tag = tag;
            DrawingIndex = copyShape.DrawingIndex;
            IsDestroyed = copyShape.IsDestroyed;
            Visible = copyShape.Visible;
            Physics = new();
        }


        #region Animation Region

        public string? AnimationId { get; set; }

        public Animation? Animation
        {
            get { return string.IsNullOrEmpty(AnimationId) ? null : GameWindow.ActiveScene.Animations.Get(AnimationId); }
            set
            {
                if (value != null)
                {
                    if (GameWindow.ActiveScene.Animations.Contains(value.Id))
                    {
                        AnimationId = value.Id;
                        Texture = value.GameTexture;
                        return;
                    }

                    GameWindow.ActiveScene.Animations.Add(value);
                    Texture = value.GameTexture;
                    AnimationId = value.Id;
                }
                else
                {
                    AnimationId = null;
                }
            }
        }

        public bool HasAnimation => string.IsNullOrEmpty(AnimationId) ? false : GameWindow.ActiveScene.Animations.Contains(AnimationId);

        public event EventHandler<IGameDrawable>? OnDrawing;
        public event EventHandler<IGameDrawable>? OnDrawed;

        public virtual void UpdateAnimation()
        {
            if (HasAnimation)
            {
                Animation?.UpdateFrame();
                TextureRect = Animation?.GetCurrentRect() ?? new();
            }
        }

        #endregion
        public Physics Physics { get; set; }
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
