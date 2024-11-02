using RealGame.GameEngine;
using RealGame.GameEngine.Entities.EntityObjects;
using RealGame.GameEngine.Entities.GameEntities;
using RealGame.GameEngine.Entities.Interfaces;
using RealGame.GameEngine.Helpers;

namespace RealGame.Application.GameObjects
{
    public class SpikeRoll : GameSprite
    {
        public Func<Drone, bool>? DamageFilter { private get; set; }
        public float CollisionVelocityDamage { get; }
        public float ContactDamage { get; }

        private List<string> _firendlyWith = new();
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="collisionVelocityDamage">Multiply collided drones velocity with that factor and inflict as damage</param>
        /// <param name="contactDamage">Inflict damage each frame of contact</param>
        /// <param name="tag"></param>
        public SpikeRoll(string id, float collisionVelocityDamage = 2, float contactDamage = 0.1f, string tag = "spike") : base(id, tag)
        {
            if (!GameWindow.ActiveScene.Textures.Contains("spike"))
            {
                var texture = new GameTexture(Properties.Resources.spikes);
                var animation = new Animation("spike_animation");
                animation.SetLoop(texture, new(64, 64), TimeSpan.FromMilliseconds(200));
                animation.RectIndexRange = (0, 9);
                this.Animation = animation;
            }
            else
            {
                this.Animation = GameWindow.ActiveScene.Animations.Get("spike_animation");
            }

            CollisionVelocityDamage = collisionVelocityDamage;
            ContactDamage = contactDamage;

            this.ConfigurePhysics(P =>
            {
                P.IsStatic = true;
                P.CollisionBoxTrimWidth = 16;
            });
            Physics.OnCollision += Physics_OnCollision;
            Physics.OnSeparation += (_, obj) => collidedIds.Remove(obj.Id);
            OnDrawed += SpikeRoll_OnDrawed;
        }

        private HashSet<string> collidedIds = new();

        public void AddFriendlyWith(Drone drone)
        {
            _firendlyWith.Add(drone.Id);
        }
        private void SpikeRoll_OnDrawed(object? sender, IGameDrawable e)
        {
            foreach (var collidedId in collidedIds)
            {
                var collidedDrone = GameWindow.ActiveScene.Get<Drone>(collidedId);
                if (collidedDrone != null)
                {
                    collidedDrone.Health.Damage(ContactDamage);
                }
            }
        }

        private void Physics_OnCollision(object? sender, IGameDrawable e)
        {
            if (_firendlyWith.Contains(e.Id))
                return;

            if (e is Drone drone && (DamageFilter?.Invoke(drone) ?? true))
            {
                drone.Health.Damage(drone.Physics.Properties!.Velocity.Length() * CollisionVelocityDamage);
                collidedIds.Add(drone.Id);
            }
        }


    }
}
