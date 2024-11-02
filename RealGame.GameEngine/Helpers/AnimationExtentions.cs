using RealGame.GameEngine.Entities.EntityObjects;
using RealGame.GameEngine.Entities.Interfaces;

namespace RealGame.GameEngine.Helpers
{
    public static partial class Extensions
    {
        public static int HierarchyIndex(this Animation? animation)
        {
            return (int)(animation?.AnimationType ?? AnimationType.None);
        }

        public static int AnimationHierarchyIndex(this IAnimated animated)
        {
            return (int)(animated.Animation?.AnimationType ?? AnimationType.None);
        }



    }
}
