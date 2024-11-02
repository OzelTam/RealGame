using RealGame.GameEngine;
using RealGame.GameEngine.Entities.EntityObjects;
using RealGame.GameEngine.Entities.GameEntities;

namespace RealGame.Application.GameObjects
{
    public enum BombState
    {
        Falling,
        Exploding,
    }
    public class Bomb : StatefulGameSprite<BombState>
    {
        private float _fallingRate = 1;
        public float Damage { get; set; } = 10;
        private void InitializeTexture()
        {
            if (GameWindow.ActiveScene.Textures.Contains("bomb1_texture"))
                return;
            var bombTexture = new GameTexture(Properties.Resources.bomb1, id: "bomb1_texture");
            var explosionTexture = new GameTexture(Properties.Resources.explosion1, id: "explosion1_texture");

            GameWindow.ActiveScene.Textures.Add(bombTexture);
            GameWindow.ActiveScene.Textures.Add(explosionTexture);
        }

        private void InitializeAnimation()
        {
            var txBomb = GameWindow.ActiveScene.Textures.Get("bomb1_texture");
            var txExplosion = GameWindow.ActiveScene.Textures.Get("explosion1_texture");

            if (txBomb == null || txExplosion == null)
                throw new Exception("Textures not found");


            var bombAnimation = new Animation();
            bombAnimation.SetLoop(txBomb, new(16, 16), TimeSpan.FromMilliseconds(300));
            bombAnimation.RectIndexRange = (0, 5);
            GameWindow.ActiveScene.Animations.Add(bombAnimation);

            var explosionAnimation = new Animation();
            explosionAnimation.SetOnce(txExplosion, new(64, 64), TimeSpan.FromMilliseconds(100), (a) =>
            {
                this.IsDestroyed = true;
                GameWindow.ActiveScene.Animations.Remove(a.Id);
                GameWindow.ActiveScene.Animations.Remove(bombAnimation.Id);
            });
            explosionAnimation.RectIndexRange = (0, 10);
            AddState(BombState.Falling, bombAnimation);
            AddState(BombState.Exploding, explosionAnimation);
        }
        public Bomb(float fallingRate = 3, string tag = "bomb") : base(Guid.NewGuid().ToString(), tag)
        {
            this.Scale = new(2, 2);
            _fallingRate = fallingRate;
            this.Rotation = 1;
            InitializeTexture();
            InitializeAnimation();
            OnDrawing += Bomb_OnDrawing;
            this.ConfigurePhysics(o =>
            {
                o.IsGhost = true;
            });
            this.MapStateCondition(BombState.Exploding, () => this.Physics.Properties!.IsCollidingWithStatic);

        }

        private void Bomb_OnDrawing(object? sender, GameEngine.Entities.Interfaces.IGameDrawable e)
        {
            if (CurrentState?.Type == BombState.Falling)
            {
                this.Position += new SFML.System.Vector2f(0, _fallingRate);
            }


        }
    }
}
