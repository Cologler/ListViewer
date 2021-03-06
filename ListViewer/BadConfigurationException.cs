﻿using System;
using System.Runtime.Serialization;

namespace ListViewer
{
    public class BadConfigurationException : Exception
    {
        public BadConfigurationException()
        {
        }

        public BadConfigurationException(string? message) : base(message)
        {
        }

        public BadConfigurationException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected BadConfigurationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
