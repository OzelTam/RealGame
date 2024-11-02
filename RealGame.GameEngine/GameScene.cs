using RealGame.GameEngine.Entities;
using RealGame.GameEngine.Entities.EntityObjects;
using RealGame.GameEngine.Entities.Interfaces;
using SFML.Graphics;

namespace RealGame.GameEngine
{
    public interface IDentifiableDrawable : IIdentifiable, Drawable
    {
    }


    public class GameScene
    {
        public DateTime StartingTime { get; } = DateTime.Now;
        public ResourceRepository<IGameDrawable> GameDrawings { get; set; } = new();
        public ResourceRepository<GameTexture> Textures { get; } = new();
        public ResourceRepository<Animation> Animations { get; } = new();
        public Queue<Drawable> RawDrawingspPrior { get; } = new();
        public Queue<Drawable> RawDrawingspLater { get; } = new();

        public void Add(IGameDrawable drawable) => GameDrawings.Add(drawable);
        public void Enqueue(Drawable drawable, bool isPrior = false)
        {
            if (isPrior)
            {
                RawDrawingspPrior.Enqueue(drawable);
            }
            else
            {
                RawDrawingspLater.Enqueue(drawable);
            }
        }
        public void ResetAllIsCollidedProperties()
        {
            GameDrawings.GetAll().ToList().ForEach(a =>
            {
                if (a.Physics.Properties != null)
                {
                    a.Physics.Properties.IsColliding = false;
                    a.Physics.Properties.IsCollidingWithStatic = false;
                }
            });
        }

        public T? Get<T>(string id) where T : IGameDrawable
        {
            var drawable = GameDrawings.Get(id);
            if (drawable is T)
            {
                return (T)drawable;
            }
            return default;
        }
        public IEnumerable<T> GetAll<T>() where T : IGameDrawable
        {
            return GameDrawings.GetAll().Where(a => a is T).Select(a => (T)a);
        }
    }
}
