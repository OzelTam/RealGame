using RealGame.GameEngine.Entities.Interfaces;
using RealGame.GameEngine.Helpers;
using SFML.Graphics;
using SFML.System;

namespace RealGame.GameEngine.Entities.EntityObjects
{
    public enum AnimationType
    {
        None = -1,
        Static = 0,
        Loop = 1,
        FiniteLooped = 2,
        Once = 3,
    }

    public class Animation : IIdentifiable
    {
        public static bool operator <(Animation? a, Animation? b)
        {
            return a.HierarchyIndex() < b.HierarchyIndex();
        }

        public static bool operator >(Animation? a, Animation? b)
        {
            return a.HierarchyIndex() > b.HierarchyIndex();
        }

        public static bool operator <=(Animation? a, Animation? b)
        {
            return a.HierarchyIndex() <= b.HierarchyIndex();
        }

        public static bool operator >=(Animation? a, Animation? b)
        {
            return a.HierarchyIndex() >= b.HierarchyIndex();
        }







        public AnimationType AnimationType { get; private set; }
        public Vector2u FrameRectSize { get; private set; } = new();
        public string? GameTextureId { get; private set; }

        private TimeSpan FrameDuration { get; set; } = TimeSpan.FromMilliseconds(100);
        private TimeSpan CurrentFrameDuration { get; set; } = TimeSpan.Zero;
        private Vector2i CurrentFramePosition { get; set; } = new();
        private int CurrentFrameIndex { get; set; } = 0;
        public bool IsDone { get; set; }

        private (int, int) rectIndexRange = (0, 0);
        public (int, int) RectIndexRange
        {
            get => rectIndexRange;
            set
            {
                if (value.Item1 < 0 || value.Item2 < 0)
                    throw new ArgumentOutOfRangeException("RectIndexRange", "RectIndexRange values must be greater than 0");
                if (value.Item1 > value.Item2)
                    throw new ArgumentOutOfRangeException("RectIndexRange", "RectIndexRange values must be in ascending order");
                rectIndexRange = value;
            }
        }


        private int finiteLoopCount = -1;

        private Action<Animation>? DoneAction { get; set; }

        public void Reset()
        {

            CurrentFrameDuration = TimeSpan.Zero;
            CurrentFrameIndex = 0;
            CurrentFramePosition = new Vector2i(0, 0);
            IsDone = false;
            currentLoopCount = 0;
        }

        public void Reset(TimeSpan delay)
        {
            Task.Delay(delay).ContinueWith((task) =>
            {
                Reset();
            });
        }


        public void SetStatic(GameTexture texture, Vector2u? frameRectSize = null)
        {
            AnimationType = AnimationType.Static;
            GameTextureId = texture.Id;
            FrameRectSize = frameRectSize ?? FrameRectSize;
            Initialize(texture);
        }

        public void SetFiniteLoop(GameTexture texture, Vector2u frameRectSize, TimeSpan frameDuration, int maxLoopCount, Action<Animation>? doneAction = null)
        {
            DoneAction = doneAction;
            AnimationType = AnimationType.FiniteLooped;
            GameTextureId = texture.Id;
            FrameRectSize = frameRectSize;
            FrameDuration = frameDuration;
            finiteLoopCount = maxLoopCount;
            Initialize(texture);
        }

        public void SetLoop(GameTexture texture, Vector2u frameRectSize, TimeSpan frameDuration)
        {
            AnimationType = AnimationType.Loop;
            GameTextureId = texture.Id;
            FrameRectSize = frameRectSize;
            FrameDuration = frameDuration;
            Initialize(texture);
        }

        public void SetOnce(GameTexture texture, Vector2u frameRectSize, TimeSpan frameDuration, Action<Animation>? doneAction = null)
        {
            DoneAction = doneAction;
            AnimationType = AnimationType.Once;
            GameTextureId = texture.Id;
            FrameRectSize = frameRectSize;
            FrameDuration = frameDuration;
            Initialize(texture);
        }

        private void EnsureTextureInRepository(GameTexture gameTexture)
        {
            if (!GameWindow.ActiveScene.Textures.Contains(gameTexture.Id))
            {
                GameWindow.ActiveScene.Textures.Add(gameTexture);
            }


        }
        public GameTexture? GameTexture
        {
            get
            {
                return !string.IsNullOrEmpty(GameTextureId) ? GameWindow.ActiveScene.Textures.Get(GameTextureId) : null;
            }
            //set
            //{
            //    if (value != null)
            //    {
            //        GameTextureId = value.Id;
            //        Initialize(value);
            //    }
            //}
        }


        public string Id { get; init; } = Guid.NewGuid().ToString();
        public string Tag { get; init; } = "Animation";

        int currentLoopCount = 0;
        private void UpdateFiniteLooped()
        {

            if (finiteLoopCount <= currentLoopCount)
            {
                IsDone = true;
                DoneAction?.Invoke(this);
                return;
            }

            UpdateLoop(true);
        }

        private void UpdateLoop(bool incrementLoop = false)
        {
            var texture = GameTexture;
            if (texture == null)
            {
                Logger.Log($"GameTexture not found '{GameTextureId}'", LogLevel.Error);
                return;
            }
            var textureSize = texture.Size;
            var rectRowCount = textureSize.X / FrameRectSize.X;
            var rectColumnCount = textureSize.Y / FrameRectSize.Y;

            var rectWidth = FrameRectSize.X;
            var rectHeight = FrameRectSize.Y;

            if (CurrentFrameDuration >= FrameDuration)
            {
                CurrentFrameDuration = TimeSpan.Zero;
                var (start, end) = RectIndexRange;
                var maximumFrameIndex = rectRowCount * rectColumnCount - 1;

                var indexRangeSet = !(start == 0 && end == 0);
                var loopDone = CurrentFrameIndex > maximumFrameIndex || indexRangeSet && CurrentFrameIndex >= end; // If CurrentFrameIndex is greater than the maximum frame index or the end of the range

                if (start == end && indexRangeSet) // If the range is set to a single frame
                {
                    CurrentFrameIndex = start;
                }
                else if (!indexRangeSet) // If the range is not set
                {
                    CurrentFrameIndex++;
                }
                else if (indexRangeSet) // If the range is set to a range of frames
                {
                    if (loopDone && incrementLoop)
                        currentLoopCount++;
                    CurrentFrameIndex++;
                    CurrentFrameIndex = start + CurrentFrameIndex % (end - start + 1); // Modulo operation to keep the index in the range, it Loops back to the start of the range
                }

                CurrentFrameIndex = loopDone ? 0 : CurrentFrameIndex;

                var left = CurrentFrameIndex % rectRowCount * rectWidth;
                var top = CurrentFrameIndex / rectRowCount * rectHeight;

                CurrentFramePosition = new Vector2i((int)left, (int)top);

            }

            CurrentFrameDuration += GameWindow.DeltaTime;
        }

        private void UpdateOnce()
        {
            if (currentLoopCount > 0)
            {
                IsDone = true;
                DoneAction?.Invoke(this);
                return;
            }
            UpdateLoop(true);
        }



        private void Initialize(GameTexture gameTexture)
        {
            EnsureTextureInRepository(gameTexture);
            if (FrameRectSize == new Vector2u())
            {
                var texture = GameTexture;
                if (texture == null)
                {
                    Logger.Log($"GameTexture not found '{GameTextureId}'", LogLevel.Error);
                    return;
                }
                FrameRectSize = new Vector2u(texture.Size.X, texture.Size.Y);
            }
        }
        public void UpdateFrame()
        {

            if (AnimationType == AnimationType.FiniteLooped)
            {
                UpdateFiniteLooped();
            }
            else if (AnimationType == AnimationType.Loop)
            {
                UpdateLoop();
            }
            else if (AnimationType == AnimationType.Once)
            {
                UpdateOnce();
            }
        }

        public IntRect GetCurrentRect()
        {
            var resultRect = new IntRect(CurrentFramePosition.X, CurrentFramePosition.Y, (int)FrameRectSize.X, (int)FrameRectSize.Y);
            if (flip.Item1)
            {
                resultRect.Left += (int)FrameRectSize.X;
                resultRect.Width *= -1;
            }
            if (flip.Item2)
            {
                resultRect.Top += (int)FrameRectSize.Y;
                resultRect.Height *= -1;
            }
            return resultRect;
        }

        (bool, bool) flip = (false, false);
        public void Flipped(bool flipX, bool flipY)
        {
            flip = (flipX, flipY);
        }

        public Animation(string? id = null)
        {
            Id = string.IsNullOrEmpty(id) ? Id : id;
        }
    }
}
