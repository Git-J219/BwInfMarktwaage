using System;
using System.Runtime.Serialization;

namespace MarktwaageCs
{
    [Serializable]
    internal class UngueltigesErgebnisException : Exception
    {
        public UngueltigesErgebnisException()
        {
        }

        public UngueltigesErgebnisException(string message) : base(message)
        {
        }

        public UngueltigesErgebnisException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected UngueltigesErgebnisException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}