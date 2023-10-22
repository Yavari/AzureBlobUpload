using System.Xml.Serialization;

namespace Drone.Services.AzureBlob
{
    [XmlRoot(ElementName = "EnumerationResults")]
    public class ListBlobResult
    {
        [XmlElement(ElementName = "Blobs")]
        public InnerBlobs Blobs { get; set; }

        [XmlElement(ElementName = "NextMarker")]
        public object NextMarker { get; set; }

        [XmlAttribute(AttributeName = "ServiceEndpoint")]
        public string ServiceEndpoint { get; set; }

        [XmlAttribute(AttributeName = "ContainerName")]
        public string ContainerName { get; set; }

        [XmlText]
        public string Text { get; set; }

        [XmlRoot(ElementName = "Blobs")]
        public class InnerBlobs
        {
            [XmlElement(ElementName = "Blob")]
            public List<InnerBlob> Blob { get; set; }

            [XmlRoot(ElementName = "Blob")]
            public class InnerBlob
            {

                [XmlElement(ElementName = "Name")]
                public string Name { get; set; }

                [XmlElement(ElementName = "Properties")]
                public InnerProperties Properties { get; set; }

                [XmlElement(ElementName = "OrMetadata")]
                public object OrMetadata { get; set; }

                [XmlRoot(ElementName = "Properties")]
                public class InnerProperties
                {

                    [XmlElement(ElementName = "Content-Length")]
                    public int ContentLength { get; set; }

                    [XmlElement(ElementName = "BlobType")]
                    public string BlobType { get; set; }

                    [XmlElement(ElementName = "AccessTier")]
                    public string AccessTier { get; set; }

                    [XmlElement(ElementName = "AccessTierInferred")]
                    public bool AccessTierInferred { get; set; }

                    [XmlElement(ElementName = "LeaseStatus")]
                    public string LeaseStatus { get; set; }

                    [XmlElement(ElementName = "LeaseState")]
                    public string LeaseState { get; set; }

                    [XmlElement(ElementName = "ServerEncrypted")]
                    public bool ServerEncrypted { get; set; }

                    [XmlElement(ElementName = "Creation-Time")]
                    public string CreationTime { get; set; }

                    [XmlElement(ElementName = "Last-Modified")]
                    public string LastModified { get; set; }

                    [XmlElement(ElementName = "Etag")]
                    public string Etag { get; set; }

                    [XmlElement(ElementName = "Content-Type")]
                    public string ContentType { get; set; }

                    [XmlElement(ElementName = "Content-Encoding")]
                    public object ContentEncoding { get; set; }

                    [XmlElement(ElementName = "Content-Language")]
                    public object ContentLanguage { get; set; }

                    [XmlElement(ElementName = "Content-CRC64")]
                    public object ContentCRC64 { get; set; }

                    [XmlElement(ElementName = "Content-MD5")]
                    public object ContentMD5 { get; set; }

                    [XmlElement(ElementName = "Cache-Control")]
                    public object CacheControl { get; set; }

                    [XmlElement(ElementName = "Content-Disposition")]
                    public object ContentDisposition { get; set; }
                }
            }
        }
    }
}