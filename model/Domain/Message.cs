using System;

namespace Ticketing.Model.Domain
{
    [Serializable]
    public class Message : IIdentifiable<int>
    {
        private User sender;
        private User receiver;
        private string text;
        private int id;

        // Java: public Message(User sender, String text, User receiver)
        public Message(User sender, string text, User receiver)
        {
            this.sender = sender;
            this.text = text;
            this.receiver = receiver;
        }

        // Required for serialization/proxy layers
        public Message() 
        { 
            this.text = string.Empty;
        }

        // --- Getters (Properties with only get to match Java) ---

        public User Sender => sender;

        public User Receiver => receiver;

        public string Text => text;

        // --- IIdentifiable<int> Implementation ---

        // Java: public Integer getId() and public void setId(Integer id)
        public int Id
        {
            get => id;
            set => id = value;
        }

        // --- Overrides ---

        public override string ToString()
        {
            // Java: "Message{sender=" + sender.getId() + ", receiver=" + receiver.getId() + ", text='" + text + '\'' + '}'
            // We use .Id here because Sender/Receiver are Identifiable
            return $"Message{{sender={sender?.Id}, receiver={receiver?.Id}, text='{text}'}}";
        }

        // Standard C# Equals/GetHashCode to ensure the model works in Collections
        public override bool Equals(object? obj)
        {
            if (obj is Message other)
            {
                return id == other.id;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return id.GetHashCode();
        }
    }
}