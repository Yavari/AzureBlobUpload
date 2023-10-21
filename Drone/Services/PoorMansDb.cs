namespace Drone.Services
{
    public class PoorMansDb
    {
        public record Chunk(long Start, long End, long Size, long TotalSize);
        public record MetaData(Chunk Chunk, string[] Hashes);
        public Dictionary<string, List<MetaData>> Db { get; } = new();
    }
}
