using RealGame.GameEngine.Entities.Interfaces;
using SFML.Graphics;
using SFML.System;
/*
Ah Günahkar herif, nereye kaçacaksın?
Günahkar nereye kaçacaan!
nereye kaçacaan!
gün boyu?

Taşa kaçmalıyız, 
Sakla beni, Taşa sığınıyorum
Sakla beni, Taşa sığın
Sakla burada beni,
Gün boyuu.

Ama taş dile geldi!
Saklayamam seni, Taş haykırdı 
Saklayamam seni, Taş haykırdı 
Seni Saklamayacağım
Gün Boyunca

Haykırdım, "Ey Taş!"
Senin derdin ne Ey Taş!
Görmüyor musun muhtacım Ey Taş!
Ey Yüce Rabbim
Gün Bayounca.

Ben de nehre sığındım!
Oluk oluk kandan, Denize sığındım,
Oluk oluk kandan, Denize sığındım
Oluk oluk kandan,
Gün boyunca.

Ben de nehre sığındım,
Kaynar sudan denize sığındım
Kaynar sudan denize sığındım
Kaynar sudan,
Gün Boyunca.

Ben de Rabbime sığındım,
Ya Rabbim beni koru!
Duymuyor musun dualarımı?
Görmüyor musun dizlerimin üstündeyim?

Ama Rab buyurdu:
"Şeytana git!" dedi.
Rab dedi: "Şeytana git!"
Ve buyurdu: "Ona git!"
Gün boyunca.

Ben de İblis'e sıındım!
Beni bekliyordu, İblis'e sığındım
Beni bekliyordu, İblis'e sığındım
Beni bekliyordu,
Gün boyunca.

Haykırdım, "Kudret!", "Kudret!" (Rabbim Kudret)
"Kudret!" (Rabbim Kudret) x6...

(Scat)*/
namespace RealGame.GameEngine.Helpers
{
    public class Ray
    {
        public Vector2f StartingPosition { get; set; }
        public Vector2f EndingPosition { get; set; }
        public Vector2f Vector => EndingPosition - StartingPosition;
        public Vector2f Diection => Vector.Normalize();
        public float Magnitute => Vector.Length();
        public Ray(Vector2f startingPoint, Vector2f endingPoint)
        {
            StartingPosition = startingPoint;
            EndingPosition = endingPoint;
        }

    }
    public static class PhysicsHelper
    {
        /// <summary>
        /// Simulates Physics for first object relative to the other object
        /// </summary>
        /// <param name="drawable1"></param>
        /// <param name="other"></param>
        /// <returns>IsCollided</returns>
        public static void SimulatePhysics(this IGameDrawable drawable1)
        {
            if (drawable1.IsPhysicsConfigured())
            {
                var deltaT = GameWindow.DeltaTime.Milliseconds;
                var forceToApply = drawable1.Physics!.Properties!.AccumulatedForce;
                var mass = drawable1.Physics.Properties.Mass;

                // Calculate acceleration and update velocity
                var acceleration = forceToApply / mass;
                drawable1.Physics.Properties.Velocity += acceleration * deltaT;

                // Apply damping to smooth out movement over time and prevent jitter
                drawable1.Physics.Properties.Velocity *= 1 - drawable1.Physics.Properties.Damping;

                // Reset accumulated force
                drawable1.Physics.Properties.AccumulatedForce = new();
                // Update position
                drawable1.Position += drawable1.Physics.Properties.Velocity;
            }
        }

        public static bool CheckCollision(this IGameDrawable drawable, IGameDrawable other, out Vector2f normal, out float depth)
        {
            if (!drawable.IsPhysicsConfigured() || !other.IsPhysicsConfigured())
            { normal = new Vector2f(); depth = 0; return false; }

            var (box1, box2) = (drawable.GetOBBox(), other.GetOBBox());
            var (props1, props2) = (drawable.Physics.Properties, other.Physics.Properties);

            if (box1 == null || box2 == null || props1 == null || props2 == null || !drawable.IsCollidable() || !other.IsCollidable() || props1.IsStatic && props2.IsStatic)
            { normal = new Vector2f(); depth = 0; return false; }

            var result = box1.AreColliding(box2, out normal, out depth);
            if (result)
            {
                props1.IsColliding = true;
                props2.IsColliding = true;
                if (props1.IsStatic)
                    props2.IsCollidingWithStatic = true;
                if (props2.IsStatic)
                    props1.IsCollidingWithStatic = true;
            }
            return result;
        }

        public static void ResolveCollision(this IGameDrawable drawable, IGameDrawable other, Vector2f normal, float depth)
        {
            var (props1, props2) = (drawable.Physics.Properties, other.Physics.Properties);

            var relativeVelocity = props2!.Velocity - props1!.Velocity;

            float e = Math.Min(props1.Restitution, props2.Restitution);
            float j = -(1f + e) * relativeVelocity.Dot(normal);
            j /= 1 / props1.Mass + 1 / props2.Mass;
            //j /= 2;


            var impulse1 = j / props1.Mass * normal;
            var impulse2 = j / props2.Mass * normal;

            //(impulse1, impulse2) = AdjustEnergy(drawable, other, impulse1, impulse2);

            if (props1.IsStatic || props2.IsStatic)
            {
                if (props1.IsStatic)
                    props2.Velocity += impulse1;
                else
                    props1.Velocity -= impulse2;
            }
            else
            {

                props1.Velocity -= impulse1;
                props2.Velocity += impulse2;
            }

            var slop = 0.01f;
            var percent = 0.8f;
            var correction = Math.Max(depth - slop, 0) / (1 / props1.Mass + 1 / props2.Mass) * normal * percent;
            if (props1.IsStatic)
                other.Position += correction;
            else if (props2.IsStatic)
                drawable.Position -= correction;
            else
            {
                drawable.Position -= correction * (1 / props1.Mass) * 0.5f;
                other.Position += correction * (1 / props2.Mass) * 0.5f;
            }


        }

        public static bool CheckAndResolveCollision(this IGameDrawable drawable, IGameDrawable other)
        {
            if (drawable.CheckCollision(other, out var normal, out var depth))
            {
                if (!drawable.Physics.Properties!.IsGhost && !other.Physics.Properties!.IsGhost)
                    drawable.ResolveCollision(other, normal, depth);
                return true;
            }
            return false;
        }



        public static void ApplyForce(this IGameDrawable drawable, float x, float y)
        {
            drawable.ApplyForce(new(x, y));
        }
        public static void ApplyForce(this IGameDrawable drawable, Vector2f forceVector, Vector2f? forcePosition = null)
        {
            var properties = drawable.Physics.Properties;
            if (properties == null || properties.IsStatic)
                return;


            drawable.Physics.Properties!.AccumulatedForce += forceVector;


            if (drawable is not Transformable transformable || forcePosition == null)
                return;
            // Apply rotation if the force causes torque
            // Calculate torque based on the distance from the forcePosition to object's origin
            Vector2f forceToOrigin = forcePosition!.Value - transformable.Position;
            transformable.Origin = forceToOrigin;
            float torque = (forceToOrigin.X * forceVector.Y - forceToOrigin.Y * forceVector.X) / properties.Mass;
            //// I = mr^2 => float inertia 
            var r = (forcePosition.Value - transformable.GetOriginGlobalPosition()).Length();
            float inertia = properties.Mass * r * r;
            //// I = L/w, L = torque, w = angular acceleration
            float angularAcceleration = torque / inertia;

            transformable.Rotation += torque / properties.Mass * properties.Damping;

        }


        #region Vector Helpers
        public static float Length(this Vector2f vec)
        {
            return (float)Math.Sqrt(vec.X * vec.X + vec.Y * vec.Y);
        }
        public static Vector2f Normalize(this Vector2f vec)
        {

            float length = (float)Math.Sqrt(vec.X * vec.X + vec.Y * vec.Y);
            if (length == 0)
                return new Vector2f(0, 0);
            return new Vector2f(vec.X / length, vec.Y / length);
        }
        public static Vector2f Center(this FloatRect rect)
        {
            return new Vector2f(rect.Left + rect.Width / 2, rect.Top + rect.Height / 2);
        }
        public static Vector2f NormalTowards(this Vector2f pointA, Vector2f pointB)
        {
            var direction = pointB - pointA;
            return direction.Normalize();
        }
        public static float Dot(this Vector2f a, Vector2f b)
        {
            // a · b = ax × bx + ay × by
            return a.X * b.X + a.Y * b.Y;
        }
        public static float Cross(this Vector2f a, Vector2f b)
        {
            return a.X * b.Y - a.Y * b.X;
        }
        public enum GetRayPointOf
        {
            Enter,
            Exit,
        }

        public static Ray? GetRayFrom(this FloatRect rect, Vector2f fromPoint, GetRayPointOf pointOf)
        {
            var point = pointOf switch
            {
                GetRayPointOf.Enter => rect.GetRayEnterPoint(fromPoint),
                GetRayPointOf.Exit => rect.GetRayExitPoint(fromPoint),
                _ => null
            };

            if (point == null) return null;

            return new Ray(fromPoint, point.Value);
        }

        public static Ray? GetRayTowards(this FloatRect rect, Vector2f towardsPoint)
        {
            var point = rect.GetRayEnterPoint(towardsPoint);
            if (point == null) return null;
            var point2 = rect.Center();
            return new Ray(point2, point.Value);
        }
        private static Vector2f? GetRayEnterPoint(this FloatRect rect, Vector2f fromPoint)
        {
            // Calculate the center of the rectangle
            Vector2f rectCenter = new Vector2f(rect.Left + rect.Width / 2, rect.Top + rect.Height / 2);

            // Ray direction from point to the center of the rectangle
            Vector2f rayDirection = (rectCenter - fromPoint).Normalize();

            rayDirection = rayDirection.Normalize();

            // Initialize min time fact as infinity
            float tMin = float.PositiveInfinity;
            Vector2f? intersectionPoint = null;

            // Left 
            if (rayDirection.X != 0)
            {
                float t = (rect.Left - fromPoint.X) / rayDirection.X;
                if (t >= 0)
                {
                    float y = fromPoint.Y + t * rayDirection.Y;
                    if (y >= rect.Top && y <= rect.Top + rect.Height && t < tMin)
                    {
                        tMin = t;
                        intersectionPoint = new Vector2f(rect.Left, y);
                    }
                }
            }

            // Right 
            if (rayDirection.X != 0)
            {
                float t = (rect.Left + rect.Width - fromPoint.X) / rayDirection.X;
                if (t >= 0)
                {
                    float y = fromPoint.Y + t * rayDirection.Y;
                    if (y >= rect.Top && y <= rect.Top + rect.Height && t < tMin)
                    {
                        tMin = t;
                        intersectionPoint = new Vector2f(rect.Left + rect.Width, y);
                    }
                }
            }

            // Top 
            if (rayDirection.Y != 0)
            {
                float t = (rect.Top - fromPoint.Y) / rayDirection.Y;
                if (t >= 0)
                {
                    float x = fromPoint.X + t * rayDirection.X;
                    if (x >= rect.Left && x <= rect.Left + rect.Width && t < tMin)
                    {
                        tMin = t;
                        intersectionPoint = new Vector2f(x, rect.Top);
                    }
                }
            }

            // Bottom 
            if (rayDirection.Y != 0)
            {
                float t = (rect.Top + rect.Height - fromPoint.Y) / rayDirection.Y;
                if (t >= 0)
                {
                    float x = fromPoint.X + t * rayDirection.X;
                    if (x >= rect.Left && x <= rect.Left + rect.Width && t < tMin)
                    {
                        tMin = t;
                        intersectionPoint = new Vector2f(x, rect.Top + rect.Height);
                    }
                }
            }


            return intersectionPoint;
        }
        private static Vector2f? GetRayExitPoint(this FloatRect rect, Vector2f fromPoint)
        {
            // Calculate the center of the rectangle
            Vector2f rectCenter = new Vector2f(rect.Left + rect.Width / 2, rect.Top + rect.Height / 2);

            // Ray direction from point to the center of the rectangle
            Vector2f rayDirection = (rectCenter - fromPoint).Normalize();

            // Initialize min time fact as infinity
            float tMin = float.PositiveInfinity;
            Vector2f? intersectionPoint = null;


            // Left 
            if (rayDirection.X != 0)
            {
                // Calculate the time factor t for the ray to intersect the left side of the rectangle
                float t = (rect.Left - fromPoint.X) / rayDirection.X;
                if (t >= 0)
                {
                    // Calculate the y-coordinate of the intersection point
                    float y = fromPoint.Y + t * rayDirection.Y;
                    if (y >= rect.Top && y <= rect.Top + rect.Height && t < tMin)
                    {
                        tMin = t;
                        intersectionPoint = new Vector2f(rect.Left, y);
                    }
                }
            }

            // Right 
            if (rayDirection.X != 0)
            {
                float t = (rect.Left + rect.Width - fromPoint.X) / rayDirection.X;
                if (t >= 0)
                {
                    float y = fromPoint.Y + t * rayDirection.Y;
                    if (y >= rect.Top && y <= rect.Top + rect.Height && t < tMin)
                    {
                        tMin = t;
                        intersectionPoint = new Vector2f(rect.Left + rect.Width, y);
                    }
                }
            }

            // Top 
            if (rayDirection.Y != 0)
            {
                float t = (rect.Top - fromPoint.Y) / rayDirection.Y;
                if (t >= 0)
                {
                    float x = fromPoint.X + t * rayDirection.X;
                    if (x >= rect.Left && x <= rect.Left + rect.Width && t < tMin)
                    {
                        tMin = t;
                        intersectionPoint = new Vector2f(x, rect.Top);
                    }
                }
            }

            // Bottom 
            if (rayDirection.Y != 0)
            {
                float t = (rect.Top + rect.Height - fromPoint.Y) / rayDirection.Y;
                if (t >= 0)
                {
                    float x = fromPoint.X + t * rayDirection.X;
                    if (x >= rect.Left && x <= rect.Left + rect.Width && t < tMin)
                    {
                        tMin = t;
                        intersectionPoint = new Vector2f(x, rect.Top + rect.Height);
                    }
                }
            }

            // If no intersection found, return null
            if (intersectionPoint == null)
                return null;

            // Continue casting the ray to find the exit point
            Vector2f exitPoint = intersectionPoint.Value;

            // Now we will continue the ray and find the exit point
            float tMax = float.NegativeInfinity;

            // Check each of the four sides again for exit
            // Left side
            if (rayDirection.X != 0)
            {
                float t = (rect.Left - fromPoint.X) / rayDirection.X;
                if (t > tMin)
                {
                    float y = fromPoint.Y + t * rayDirection.Y;
                    if (y >= rect.Top && y <= rect.Top + rect.Height && t > tMax)
                    {
                        tMax = t;
                        exitPoint = new Vector2f(rect.Left, y);
                    }
                }
            }

            // Right side
            if (rayDirection.X != 0)
            {
                float t = (rect.Left + rect.Width - fromPoint.X) / rayDirection.X;
                if (t > tMin)
                {
                    float y = fromPoint.Y + t * rayDirection.Y;
                    if (y >= rect.Top && y <= rect.Top + rect.Height && t > tMax)
                    {
                        tMax = t;
                        exitPoint = new Vector2f(rect.Left + rect.Width, y);
                    }
                }
            }

            // Top side
            if (rayDirection.Y != 0)
            {
                float t = (rect.Top - fromPoint.Y) / rayDirection.Y;
                if (t > tMin)
                {
                    float x = fromPoint.X + t * rayDirection.X;
                    if (x >= rect.Left && x <= rect.Left + rect.Width && t > tMax)
                    {
                        tMax = t;
                        exitPoint = new Vector2f(x, rect.Top);
                    }
                }
            }

            // Bottom side
            if (rayDirection.Y != 0)
            {
                float t = (rect.Top + rect.Height - fromPoint.Y) / rayDirection.Y;
                if (t > tMin)
                {
                    float x = fromPoint.X + t * rayDirection.X;
                    if (x >= rect.Left && x <= rect.Left + rect.Width && t > tMax)
                    {
                        tMax = t;
                        exitPoint = new Vector2f(x, rect.Top + rect.Height);
                    }
                }
            }

            return exitPoint;
        }
        public static Vector2f? GetRayExitPointFrom(this FloatRect rect, Vector2f point)
        {
            // Calculate the center of the rectangle
            Vector2f rectCenter = new Vector2f(rect.Left + rect.Width / 2, rect.Top + rect.Height / 2);

            // Ray direction from point to the center of the rectangle
            Vector2f rayDir = point - rectCenter; // Reverse direction for exit
                                                  // Normalize the ray direction
            float length = (float)Math.Sqrt(rayDir.X * rayDir.X + rayDir.Y * rayDir.Y);
            rayDir.X /= length;
            rayDir.Y /= length;

            // Initialize maximum t (time factor) as negative infinity
            float tMax = float.NegativeInfinity;
            Vector2f? exitPoint = null;

            // Check each of the four sides of the rectangle
            // Left side
            if (rayDir.X != 0)
            {
                float t = (rect.Left - point.X) / rayDir.X;
                if (t >= 0)
                {
                    float y = point.Y + t * rayDir.Y;
                    if (y >= rect.Top && y <= rect.Top + rect.Height && t > tMax)
                    {
                        tMax = t;
                        exitPoint = new Vector2f(rect.Left, y);
                    }
                }
            }

            // Right side
            if (rayDir.X != 0)
            {
                float t = (rect.Left + rect.Width - point.X) / rayDir.X;
                if (t >= 0)
                {
                    float y = point.Y + t * rayDir.Y;
                    if (y >= rect.Top && y <= rect.Top + rect.Height && t > tMax)
                    {
                        tMax = t;
                        exitPoint = new Vector2f(rect.Left + rect.Width, y);
                    }
                }
            }

            // Top side
            if (rayDir.Y != 0)
            {
                float t = (rect.Top - point.Y) / rayDir.Y;
                if (t >= 0)
                {
                    float x = point.X + t * rayDir.X;
                    if (x >= rect.Left && x <= rect.Left + rect.Width && t > tMax)
                    {
                        tMax = t;
                        exitPoint = new Vector2f(x, rect.Top);
                    }
                }
            }

            // Bottom side
            if (rayDir.Y != 0)
            {
                float t = (rect.Top + rect.Height - point.Y) / rayDir.Y;
                if (t >= 0)
                {
                    float x = point.X + t * rayDir.X;
                    if (x >= rect.Left && x <= rect.Left + rect.Width && t > tMax)
                    {
                        tMax = t;
                        exitPoint = new Vector2f(x, rect.Top + rect.Height);
                    }
                }
            }

            // Return the exit point or null if no exit was found
            return exitPoint;
        }

        #endregion







    }
}
