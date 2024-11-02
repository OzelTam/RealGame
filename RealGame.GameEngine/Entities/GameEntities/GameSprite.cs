using RealGame.GameEngine.ConfigurationObjects;
using RealGame.GameEngine.Entities.EntityObjects;
using RealGame.GameEngine.Entities.Interfaces;
using SFML.Graphics;

namespace RealGame.GameEngine.Entities.GameEntities
{

    public class GameSprite : Sprite, IGameDrawable, IAnimated
    {
        public string Id { get; init; }
        public string Tag { get; init; }
        public int DrawingIndex { get; set; }
        public bool IsDestroyed { get; set; }
        public bool Visible { get; set; } = true;

        #region Animation Region

        public string? AnimationId { get; set; }

        public virtual Animation? Animation
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

        public virtual bool HasAnimation => string.IsNullOrEmpty(AnimationId) ? false : GameWindow.ActiveScene.Animations.Contains(AnimationId);

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

        public GameSprite(GameSprite copySprite, string id, string tag = "sprite") : base(copySprite)
        {
            Id = id;
            Tag = tag;
            Physics = new();
        }

        public GameSprite(string id, string tag = "sprite")
        {
            Id = id;
            Tag = tag;
            Physics = new();
        }

        public GameSprite(Animation animation, string id, string tag = "sprite") : base()
        {
            Id = id;
            Tag = tag;
            Animation = animation;
            Physics = new();
        }

        public Physics Physics { get; set; }
        public void ConfigurePhysics(Action<PhysicalProperties>? physicalProertyConfigurator = null)
        {
            if (Physics.Properties == null)
                Physics.Properties = new();
            if (physicalProertyConfigurator != null)
                physicalProertyConfigurator(Physics.Properties);
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
