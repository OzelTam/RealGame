using RealGame.GameEngine.ConfigurationObjects;
using RealGame.GameEngine.Entities.EntityObjects;
using RealGame.GameEngine.Entities.GameEntities;
using RealGame.GameEngine.Entities.Interfaces;
using SFML.Graphics;
using SFML.System;

namespace RealGame.Application.GameObjects
{
    public class HealthBar : Drawable
    {
        public float CurrentHealth { get; private set; } = 100;
        public float MaxHealth { get; private set; } = 100;

        public event EventHandler? OnDamage;
        public event EventHandler? OnHeal;

        private RectangleShape barrBg = new RectangleShape();
        private RectangleShape bar = new RectangleShape();
        private Color _outlineColor = Color.Black;
        public Color OutlineColor
        {
            get { return _outlineColor; }
            set
            {
                _outlineColor = value;
                barrBg.OutlineColor = value;
            }
        }

        private float _outlineThickness = 2;
        public float OutlineThickness
        {
            get { return _outlineThickness; }
            set
            {
                _outlineThickness = value;
                barrBg.OutlineThickness = value;
            }
        }
        public event EventHandler<IGameDrawable>? OnDrawing;
        public event EventHandler<IGameDrawable>? OnDrawed;

        public Vector2f TrackingOffset { get; set; }

        private float _barThickness;

        public HealthBar(float maxHealth = 100, float barThickness = 10)
        {
            Id = Guid.NewGuid().ToString();
            barrBg.FillColor = Color.Red;
            bar.FillColor = Color.Green;
            MaxHealth = maxHealth;
            CurrentHealth = maxHealth;
            bar.Size = new Vector2f(maxHealth, barThickness);
            barrBg.Size = new Vector2f(maxHealth, barThickness);
            var origin = new Vector2f(maxHealth / 2, barThickness);
            bar.Origin = origin;
            barrBg.Origin = origin;
            _barThickness = barThickness;
        }

        public HealthBar(Color fillColor, float maxHealth = 100, float barThickness = 10)
        {
            Id = Guid.NewGuid().ToString();
            barrBg.FillColor = Color.Red;
            bar.FillColor = fillColor;
            MaxHealth = maxHealth;
            CurrentHealth = maxHealth;
            _barThickness = barThickness;
            bar.Size = new Vector2f(maxHealth, barThickness);
            barrBg.Size = new Vector2f(maxHealth, barThickness);
            var origin = new Vector2f(maxHealth / 2, barThickness);
            bar.Origin = origin;
            barrBg.Origin = origin;
            Physics = new();
        }
        public HealthBar(Color fillColor, Color bgColor, float maxHealth = 100, float barThickness = 10)
        {
            Id = Guid.NewGuid().ToString();
            barrBg.FillColor = bgColor;
            bar.FillColor = fillColor;
            MaxHealth = maxHealth;
            CurrentHealth = maxHealth;
            _barThickness = barThickness;

            bar.Size = new Vector2f(maxHealth, barThickness);
            barrBg.Size = new Vector2f(maxHealth, barThickness);
            var origin = new Vector2f(maxHealth / 2, barThickness);
            bar.Origin = origin;
            barrBg.Origin = origin;
            Physics = new();
        }



        public Vector2f Position
        {
            get => bar.Position; set
            {
                bar.Position = value;
                barrBg.Position = value;
            }
        }
        public Vector2f Scale
        {
            get => bar.Scale; set
            {
                bar.Scale = value;
                barrBg.Scale = value;
            }
        }


        public string Id { get; init; }
        public string Tag { get; init; } = "HealthBar";
        public int DrawingIndex { get; set; }
        public bool IsDestroyed { get; set; }
        public bool Visible { get; set; } = true;

        public void SetHealth(float health)
        {
            CurrentHealth = health;
            bar.Size = new Vector2f(health, _barThickness);
        }

        public void SetMaxHealth(float maxHealth)
        {
            MaxHealth = maxHealth;
            barrBg.Size = new Vector2f(maxHealth, _barThickness);
        }

        public void Damage(float damage)
        {
            CurrentHealth -= damage;
            if (CurrentHealth < 0)
                CurrentHealth = 0;
            bar.Size = new Vector2f(CurrentHealth, _barThickness);
            OnDamage?.Invoke(this, EventArgs.Empty);
        }

        public void Heal(float heal)
        {
            CurrentHealth += heal;
            if (CurrentHealth > MaxHealth)
                CurrentHealth = MaxHealth;
            bar.Size = new Vector2f(CurrentHealth, _barThickness);
            OnHeal?.Invoke(this, EventArgs.Empty);
        }

        public GameSprite? TrackingEntity { get; set; }
        public Physics Physics { get; set; }

        public void Draw(RenderTarget target, RenderStates states)
        {
            if (TrackingEntity != null)
            {
                Position = TrackingEntity.Position + TrackingOffset;
            }

            barrBg.Position = Position;
            bar.Position = Position;

            target.Draw(barrBg, states);
            target.Draw(bar, states);
        }

        public FloatRect GetGlobalBounds() => bar.GetGlobalBounds();
        public FloatRect GetLocalBounds() => bar.GetLocalBounds();

        public void ConfigurePhysics(Action<PhysicalProperties> physicalProertyConfigurator)
        {
            Physics = new Physics();
            var physicalProerty = new PhysicalProperties();
            physicalProertyConfigurator(physicalProerty);
            Physics.Properties = physicalProerty;
        }


    }
}
