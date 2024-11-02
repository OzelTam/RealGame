using SFML.System;

namespace RealGame.GameEngine.ConfigurationObjects
{
    public class PhysicalProperties
    {
        public PhysicalProperties()
        {
        }
        public Vector2f AccumulatedForce = new(0, 0);
        public Vector2f Velocity { get; set; } = new(0, 0);
        public Vector2f AngularVelocity { get; set; } = new(0, 0);

        public float Mass { get; set; } = 1;
        public float Restitution { get; set; } = .5f;
        public float Damping { get; set; } = 0.05f;
        public bool IsStatic { get; set; }
        public Vector2f CollisionBoxScale { get; set; } = new(1, 1);
        public float CollisionBoxTrimHeight { get; set; } = 0;
        public float CollisionBoxTrimWidth { get; set; } = 0;
        public bool CollisionEnabled { get; set; } = true;
        public bool IsColliding { get; set; }
        public bool IsCollidingWithStatic { get; set; }

        public bool IsGhost { get; set; }
        //public bool UseOrientedBoundingBox { get; set; }
        public PhysicalProperties Clone()
        {
            return new PhysicalProperties()
            {
                AccumulatedForce = AccumulatedForce,
                Velocity = Velocity,
                Mass = Mass,
                Restitution = Restitution,
                Damping = Damping,
                IsStatic = IsStatic,
                CollisionBoxScale = CollisionBoxScale,
                CollisionBoxTrimHeight = CollisionBoxTrimHeight,
                CollisionBoxTrimWidth = CollisionBoxTrimWidth,
                CollisionEnabled = CollisionEnabled,
                IsGhost = IsGhost
            };
        }


    }
}
