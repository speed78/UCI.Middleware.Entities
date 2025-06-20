using System.Runtime.Serialization;

namespace UCI.Middleware.Storage.Exceptions
{
    [Serializable]
    public class XmlStorageException : Exception
    {
        public string? Operation { get; }
        public string? ContainerName { get; }
        public string? FileName { get; }

        public XmlStorageException() { }
        public XmlStorageException(string message) : base(message) { }
        public XmlStorageException(string message, Exception innerException) : base(message, innerException) { }

        public XmlStorageException(string message, string operation, string? containerName = null, string? fileName = null)
            : base(message)
        {
            Operation = operation;
            ContainerName = containerName;
            FileName = fileName;
        }

        protected XmlStorageException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
