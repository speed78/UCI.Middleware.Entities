using System.Runtime.Serialization;

namespace UCI.Middleware.Storage.Exceptions
{
    [Serializable]
    public class XmlValidationException : XmlStorageException
    {
        public XmlValidationException(string message) : base(message, "Validation") { }
        public XmlValidationException(string message, Exception innerException) : base(message, innerException) { }
        protected XmlValidationException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
