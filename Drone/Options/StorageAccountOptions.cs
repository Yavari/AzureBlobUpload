namespace Drone.Options
{
    public class StorageAccountOptions
    {
        private const string ContainerTemplate = "https://{0}.blob.core.windows.net/{1}";
        public const string Position = "StorageAccount";
        public string AccountName { get; set; }
        public string AccountKey { get; set; }
        public string Container { get; set; }

        public string ContainerPath => string.Format(ContainerTemplate, AccountName, Container);
    }
}
