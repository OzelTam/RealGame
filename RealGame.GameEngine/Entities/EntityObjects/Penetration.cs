using SFML.System;

namespace RealGame.GameEngine.Entities.EntityObjects
{
    public struct Penetration
    {
        public Vector2f Position { get; set; }
        public float Depth { get; set; }
        public bool IsIntersecting { get; set; }
    }
}
