namespace Drone.Options
{
    public class StorageAccountOptions
    {
        public const string Position = "StorageAccount";
        public string AccountName { get; set; }
        public string AccountKey { get; set; }
        public string Container { get; set; }
    }
}
