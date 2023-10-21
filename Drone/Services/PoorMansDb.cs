namespace Drone.Services
{
    public class PoorMansDb
    {
        public record Chunk(long Start, long End, long TotalSize);
        public record MetaData(Chunk Chunk, string Hash);
        public Dictionary<string, List<MetaData>> Db { get; } = new();
    }
}
