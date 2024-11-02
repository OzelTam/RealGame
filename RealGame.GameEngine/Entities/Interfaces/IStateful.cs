namespace RealGame.GameEngine.Entities.Interfaces
{
    public interface IStateful
    {
        public string CurrentStateName { get; }
        public IEnumerable<string> StateNames { get; }
    }

}
