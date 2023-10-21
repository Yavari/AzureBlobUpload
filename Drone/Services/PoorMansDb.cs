namespace Drone.Services
{
    public class PoorMansDb
    {
        public Dictionary<string, Queue<string>> Db { get; } = new();
    }
}
