using System;

namespace Ticketing.Services
{
    /// <summary>
    /// Custom exception class for chat-related service errors.
    /// </summary>
    [Serializable]
    public class ChatException : Exception
    {
        // Java: public ChatException()
        public ChatException() : base() 
        { 
        }

        // Java: public ChatException(String message)
        public ChatException(string message) : base(message) 
        { 
        }

        // Java: public ChatException(String message, Throwable cause)
        public ChatException(string message, Exception innerException) 
            : base(message, innerException) 
        { 
        }
    }
}