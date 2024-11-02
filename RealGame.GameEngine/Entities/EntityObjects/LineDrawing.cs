using RealGame.GameEngine.Helpers;
using SFML.Graphics;
using SFML.System;

namespace RealGame.GameEngine.Entities.EntityObjects
{
    public class LineDrawing : RectangleShape, Drawable
    {
        private CircleShape? startBall;
        private ConvexShape? endTriangle;
        public LineDrawing(Vector2f start, Vector2f end, float thickness = 3, bool showStart = true, bool showEnd = true, Color? color = null)
        {
            // Calculate the length and angle of the line
            Vector2f direction = end - start;
            float length = direction.Length();
            float angle = (float)Math.Atan2(direction.Y, direction.X) * 180 / (float)Math.PI;

            // Set the size of the rectangle to match the line's length and thickness
            Size = new Vector2f(length, thickness);

            // Set the position to the start point
            Position = start;

            // Set the origin to the start of the line to ensure it draws from start to end
            Origin = new Vector2f(0, thickness / 2);

            // Rotate the rectangle to match the angle of the line
            Rotation = angle;

            // Apply the color, defaulting to white if none is provided
            FillColor = color ?? Color.Blue;

            // Optionally show start or end points (could be implemented as small circles, dots, etc.)
            if (showStart)
            {
                startBall = new CircleShape(thickness + 0.5f);
                startBall.Origin = new Vector2f(thickness + .5f, thickness + .5f); // Center the ball
                startBall.Position = start; // Place the ball at the start point
                startBall.FillColor = color ?? Color.Blue;
            }

            if (showEnd)
            {
                endTriangle = new ConvexShape(3); // Triangle with 3 points
                endTriangle.SetPoint(0, new Vector2f(0, 0));                     // Tip of the triangle (pointing forward)
                endTriangle.SetPoint(1, new Vector2f(-thickness * 2.3f, thickness));  // Bottom left corner
                endTriangle.SetPoint(2, new Vector2f(-thickness * 2.3f, -thickness)); // Bottom right corner
                endTriangle.FillColor = color ?? Color.Blue;

                // Place the triangle at the end point
                endTriangle.Position = end;

                // Rotate the triangle to point towards the direction of the line
                endTriangle.Rotation = angle;
            }
        }

        public new void Draw(RenderTarget target, RenderStates states)
        {
            base.Draw(target, states);
            if (startBall != null)
                startBall.Draw(target, states);
            if (endTriangle != null)
                endTriangle.Draw(target, states);
        }
    }
}

