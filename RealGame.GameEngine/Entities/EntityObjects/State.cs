namespace RealGame.GameEngine.Entities.EntityObjects
{
    public class State<T> where T : Enum
    {
        public T Type { get; set; }
        public string? AnimationId { get; private set; }
        public bool HasAnimation => !string.IsNullOrEmpty(AnimationId) && GameWindow.ActiveScene.Animations.Contains(AnimationId);
        public bool AnimationIsDone => Animation?.IsDone ?? true;

        public Animation? Animation
        {
            get => string.IsNullOrEmpty(AnimationId) ? null : GameWindow.ActiveScene.Animations.Get(AnimationId);
            set
            {
                if (value != null)
                {
                    if (GameWindow.ActiveScene.Animations.Contains(value.Id))
                    {
                        AnimationId = value.Id;
                        return;
                    }

                    GameWindow.ActiveScene.Animations.Add(value);
                    AnimationId = value.Id;
                }
                else
                    AnimationId = null;
            }
        }

        public State(T statusType, Animation animation)
        {
            Type = statusType;
            Animation = animation;
        }


    }

}
