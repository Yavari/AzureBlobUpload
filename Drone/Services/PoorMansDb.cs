namespace Drone.Services
{
    public class PoorMansDb
    {
        public Queue<string> Db { get; } = new();
    }
}
