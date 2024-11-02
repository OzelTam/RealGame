using RealGame.GameEngine.Entities.EntityObjects;

namespace RealGame.GameEngine.Entities.Interfaces
{
    public interface IAnimated : IIdentifiable
    {

        public Animation? Animation { get; set; }
        public bool HasAnimation { get; }
        public void UpdateAnimation();

    }
}
