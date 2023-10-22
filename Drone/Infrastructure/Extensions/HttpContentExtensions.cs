using System.Xml.Serialization;

namespace Drone.Infrastructure.Extensions
{
    public static class HttpContentExtensions
    {
        public static async Task<T?> ReadAsXmlAsync<T>(this HttpContent httpContent) where T : class
        {
            try
            {
                var serializer = new XmlSerializer(typeof(T));
                return serializer.Deserialize(await httpContent.ReadAsStreamAsync()) as T;
            }
            catch (Exception ex)
            {
                var xml = await httpContent.ReadAsStringAsync();
                throw new Exception(xml, ex);
            }

        }
    }
}
