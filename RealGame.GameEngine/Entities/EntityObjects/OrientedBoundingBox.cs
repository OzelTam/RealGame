using SFML.Graphics;
using SFML.System;

namespace RealGame.GameEngine.Entities.EntityObjects
{
    public class OrientedBoundingBox
    {
        public Vector2f Center { get; set; }
        public Vector2f HalfSize { get; set; }
        public float Rotation { get; set; }
        public OrientedBoundingBox(Vector2f center, Vector2f halfSize, float rotation)
        {
            Center = center;
            HalfSize = halfSize;
            Rotation = rotation;
        }

        // Get the four corners of the OBB
        public Vector2f[] GetCorners()
        {
            Vector2f[] corners = new Vector2f[4];

            // Convert the Rotation from degrees to radians
            float rotationInRadians = Rotation * (float)Math.PI / 180.0f;

            // Calculate the rotated axes (cos and sin for rotation in radians)
            Vector2f right = new Vector2f((float)Math.Cos(rotationInRadians), (float)Math.Sin(rotationInRadians));
            Vector2f up = new Vector2f(-right.Y, right.X);

            // Calculate corners based on the half size and rotation
            corners[0] = Center + right * HalfSize.X + up * HalfSize.Y;
            corners[1] = Center - right * HalfSize.X + up * HalfSize.Y;
            corners[2] = Center - right * HalfSize.X - up * HalfSize.Y;
            corners[3] = Center + right * HalfSize.X - up * HalfSize.Y;

            return corners;
        }

        public void Scale(float scaleX, float scaleY)
        {
            HalfSize = new Vector2f(HalfSize.X * scaleX, HalfSize.Y * scaleY);
        }

        public void Scale(float factor) => Scale(factor, factor);

        public void Trim(float trimX, float trimY)
        {
            Center += new Vector2f(trimX / 2, trimY / 2);
            HalfSize = new Vector2f(HalfSize.X - trimX, HalfSize.Y - trimY);
        }
        public RectangleShape AsRectangleShape()
        {
            RectangleShape shape = new RectangleShape();
            shape.Position = Center;
            shape.Size = HalfSize * 2;
            shape.Origin = HalfSize;
            shape.Rotation = Rotation;
            return shape;
        }


    }
}
