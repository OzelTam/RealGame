using RealGame.GameEngine.Entities.EntityObjects;
using RealGame.GameEngine.Entities.Interfaces;
using SFML.Graphics;
using SFML.System;

namespace RealGame.GameEngine.Helpers
{
    public static partial class Extensions
    {
        public static (float min, float max) ProjectOntoAxis(this Vector2f axis, Vector2f[] corners)
        {
            float min = corners[0].Dot(axis);
            float max = min;

            for (int i = 1; i < corners.Length; i++)
            {
                float projection = corners[i].Dot(axis);
                if (projection < min)
                    min = projection;
                else if (projection > max)
                    max = projection;
            }

            return (min, max);
        }
        public static (float min, float max) ProjectOntoAxis(OrientedBoundingBox box, Vector2f axis) => ProjectOntoAxis(axis, box.GetCorners());

        public static bool Overlaps(this (float min, float max) projection1, (float min, float max) projection2)
        {
            return projection1.max >= projection2.min && projection2.max >= projection1.min;
        }

        public static float GetOverlap(this (float min, float max) projection1, (float min, float max) projection2)
        {
            if (projection1.max >= projection2.min && projection2.max >= projection1.min)
            {
                float overlap = Math.Min(projection1.max, projection2.max) - Math.Max(projection1.min, projection2.min);
                return overlap;
            }

            return 0f; // No overlap
        }

        public static bool AreColliding(this OrientedBoundingBox obb1, OrientedBoundingBox obb2)
        {
            Vector2f[] corners1 = obb1.GetCorners();
            Vector2f[] corners2 = obb2.GetCorners();

            // Get the axes to test (axes are perpendicular to the edges of the OBB)
            Vector2f[] axes = new Vector2f[4];
            axes[0] = (corners1[1] - corners1[0]).Normalize(); // Edge 1 of OBB1
            axes[1] = (corners1[2] - corners1[1]).Normalize(); // Edge 2 of OBB1
            axes[2] = (corners2[1] - corners2[0]).Normalize(); // Edge 1 of OBB2
            axes[3] = (corners2[2] - corners2[1]).Normalize(); // Edge 2 of OBB2

            // Check for a separating axis by projecting onto each axis
            foreach (var axis in axes)
            {
                var projection1 = axis.ProjectOntoAxis(corners1);
                var projection2 = axis.ProjectOntoAxis(corners2);

                if (!projection1.Overlaps(projection2))
                {
                    // If there's a separating axis, the OBBs are not colliding
                    return false;
                }
            }

            // If all projections overlap, the OBBs are colliding
            return true;
        }

        public static Vector2f[] GetAxes(this OrientedBoundingBox obb)
        {

            Vector2f[] axes = new Vector2f[4];
            Vector2f[] corners = obb.GetCorners();

            for (var i = 0; i < corners.Length; i++)
            {
                var corner1 = corners[i];
                var corner2 = corners[(i + 1) % corners.Length];
                var edge = corner2 - corner1;
                axes[i] = edge.Normalize();
            }
            return axes;
        }

        public static void ProjectVertices(this OrientedBoundingBox obb, Vector2f axis, out float min, out float max)
        {
            (min, max) = (float.MaxValue, float.MinValue);
            var corners = obb.GetCorners();
            for (var i = 0; i < corners.Length; i++)
            {
                var projection = corners[i].Dot(axis);
                if (projection < min)
                    min = projection;
                if (projection > max)
                    max = projection;
            }
        }

        public static bool AreColliding(this OrientedBoundingBox obb1, OrientedBoundingBox obb2, out Vector2f normal, out float depth)
        {
            normal = new Vector2f();
            depth = float.MaxValue;

            var corners1 = obb1.GetCorners();

            float min1, max1, min2, max2;
            for (int i = 0; i < corners1.Length; i++)
            {
                var va = corners1[i];
                var vb = corners1[(i + 1) % corners1.Length];
                var axis = vb - va;
                axis = new(-axis.Y, axis.X);
                axis = axis.Normalize();

                obb1.ProjectVertices(axis, out min1, out max1);
                obb2.ProjectVertices(axis, out min2, out max2);

                if (max1 < min2 || max2 < min1)
                    return false;

                float o = Math.Min(max2 - min1, max1 - min2);
                if (o < depth)
                {
                    depth = o;
                    normal = axis;
                }
            }
            var normalCorrenction = obb2.Center - obb1.Center;
            if (normalCorrenction.Dot(normal) < 0)
                normal = -normal;


            return true;
        }




        public static OrientedBoundingBox? GetOBBox(this IGameDrawable entity)
        {

            var props = entity.Physics.Properties;

            if (entity is IAnimated animated && animated.Animation != null && entity is Transformable tf)
            {
                var size = animated.Animation.FrameRectSize;
                tf.Origin = new(size.X / 2, size.Y / 2);
                var sizef = new Vector2f(size.X, size.Y);
                var _center = new Vector2f((float)tf.Position.X, (float)tf.Position.Y);
                var _result = new OrientedBoundingBox(_center, sizef / 2f, tf.Rotation);
                if (props!.CollisionBoxScale.Length() != 1)
                    _result.Scale(props.CollisionBoxScale.X * tf.Scale.X, props.CollisionBoxScale.Y * tf.Scale.Y);
                if (props!.CollisionBoxTrimWidth > 0 || props!.CollisionBoxTrimHeight > 0)
                    _result.Trim(props.CollisionBoxTrimWidth, props.CollisionBoxTrimHeight);
                return _result;
            }



            var bounds = entity.GetGlobalBounds();

            if (entity.Physics.Properties?.CollisionBoxScale != null && entity.Physics.Properties!.CollisionBoxScale.Length() > 0)
                bounds = bounds.Scale(entity.Physics.Properties.CollisionBoxScale);
            if (entity.Physics.Properties?.CollisionBoxTrimHeight > 0 || entity.Physics.Properties?.CollisionBoxTrimWidth > 0)
                bounds = bounds.Trim(entity.Physics.Properties.CollisionBoxTrimHeight, entity.Physics.Properties.CollisionBoxTrimWidth);

            var fullSize = new Vector2f(bounds.Width, bounds.Height);
            var center = new Vector2f(bounds.Left + fullSize.X / 2, bounds.Top + fullSize.Y / 2);
            var result = new OrientedBoundingBox(center, fullSize / 2, 0);
            if (props!.CollisionBoxScale.Length() != 1)
                result.Scale(props.CollisionBoxScale.X, props.CollisionBoxScale.Y);
            if (props!.CollisionBoxTrimWidth > 0 || props!.CollisionBoxTrimHeight > 0)
                result.Trim(props.CollisionBoxTrimWidth, props.CollisionBoxTrimHeight);
            return result;



        }
    }
}
