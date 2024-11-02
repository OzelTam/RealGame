using RealGame.GameEngine.Entities.EntityObjects;
using RealGame.GameEngine.Entities.Interfaces;
using RealGame.GameEngine.Helpers;

namespace RealGame.GameEngine.Entities.GameEntities
{
    public class StatefulGameSprite<T> : GameSprite, IStateful where T : Enum
    {
        private T? CurrentStateType { get; set; }
        private Dictionary<T, State<T>> states = new();
        public StatefulGameSprite(string id, string tag = "stateSprite") : base(id, tag) { }
        public StatefulGameSprite(string id, T initialState, Animation initialStateAnimation, string tag = "stateSprite") : base(id, tag)
        {
            CurrentStateType = initialState;
            states.Add(initialState, new State<T>(initialState, initialStateAnimation));
        }
        public StatefulGameSprite(State<T> initialState, string id, string tag = "stateSprite") : base(id, tag)
        {
            CurrentStateType = initialState.Type;
            states.Add(initialState.Type, initialState);
        }
        public StatefulGameSprite(StatefulGameSprite<T> copySprite, string id, string tag = "stateSprite") : base(copySprite, id, tag)
        {
            CurrentStateType = copySprite.CurrentStateType;
            Physics = copySprite.Physics.Clone();
            states = copySprite.states;
        }
        public State<T>? CurrentState => CurrentStateType == null ? null : states[CurrentStateType];
        public override bool HasAnimation => CurrentState?.HasAnimation ?? false;

        /// <summary>
        /// Sets the state of the sprite according to the condition.
        /// </summary>
        /// <param name="stateType"></param>
        /// <param name="condition"></param>
        public void MapStateCondition(T stateType, Func<bool> condition)
        {
            OnDrawing += (_, _) =>
            {
                if (condition())
                    SetState(stateType);
            };
        }
        public override Animation? Animation
        {
            get
            {
                if (CurrentStateType == null)
                    return null;
                if (states.ContainsKey(CurrentStateType))
                {
                    var state = states[CurrentStateType];
                    if (state.Animation?.GameTexture != Texture)
                    {
                        Texture = state.Animation?.GameTexture;
                    }

                    return state.Animation;
                }
                return null;
            }
            set
            {
                var hasMatchingState = states.Any(x => x.Value.Animation?.Id == value?.Id);
                if (hasMatchingState)
                {
                    CurrentStateType = states.First(x => x.Value.Animation?.Id == value?.Id).Value.Type;
                    Texture = value?.GameTexture;

                    return;
                }

            }
        }

        public string CurrentStateName => CurrentStateType?.ToString() ?? string.Empty;

        public IEnumerable<string> StateNames => Enum.GetValues(typeof(T)).Cast<T>().Select(t => t.ToString());

        ///<inheritdoc cref="SetState(State{T}, bool)"/>
        public void SetState(T stateType, bool considerHierarchy = true)
        {
            if (stateType == null)
            {
                Logger.Log("StateType sent null.", LogLevel.Warn);
                return;
            }

            if (CurrentStateType == null)
            {
                CurrentStateType = stateType;
                return;
            }

            if (CurrentStateType.Equals(stateType))
                return;

            if (!states.ContainsKey(stateType))
            {
                Logger.Log($"{stateType} State not found at {Id}.", LogLevel.Warn);
                return;
            }


            var currentState = states[CurrentStateType];
            var newState = states[stateType];

            // If the current state is done, no need to check hierarchy
            if (currentState.AnimationIsDone)
            {
                CurrentStateType = stateType;
                return;
            }

            if (considerHierarchy && currentState.Animation > newState.Animation)
            {
                Logger.Log($"State {stateType} is less important than {CurrentStateType}.", LogLevel.Warn, "state_block");
                return;
            }


            CurrentStateType = stateType;
        }

        public void SetState(State<T> state, bool considerHierarchy = true)
        {
            if (state == null)
            {
                Logger.Log("State sent null.", LogLevel.Warn);
                return;
            }

            if (!states.ContainsKey(state.Type))
            {
                AddState(state);
                Logger.Log($"State {state.Type} added to {Id}.", LogLevel.Warn);
                return;
            }

            SetState(state.Type, considerHierarchy);
        }
        public Animation? this[T stateType]
        {
            get
            {
                if (CurrentStateType == null)
                    return null;

                if (states.ContainsKey(stateType))
                    return states[stateType].Animation;
                return null;
            }
            set
            {
                if (value == null)
                {
                    RemoveState(stateType);
                    return;
                }

                if (states.ContainsKey(stateType))
                    states[stateType] = new(stateType, value);
                else
                    states.Add(stateType, new(stateType, value));
            }

        }
        public void AddState(State<T> state) => this[state.Type] = state.Animation;
        public void AddState(T stateType, Animation animation) => this[stateType] = animation;
        public void RemoveState(T stateType)
        {
            if (states.ContainsKey(stateType))
                states.Remove(stateType);
        }
        public void RemoveState(State<T> state)
        {
            RemoveState(state.Type);
        }
        public void ClearStates()
        {
            states.Clear();
        }


    }

}
