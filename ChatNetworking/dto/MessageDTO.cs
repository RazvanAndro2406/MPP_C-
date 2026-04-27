using System;

namespace Ticketing.Networking.Dto
{
    [Serializable]
    public class MessageDto
    {
        public string SenderId { get; set; }
        public string ReceiverId { get; set; }
        public string Text { get; set; }

        public MessageDto(string senderId, string text, string receiverId)
        {
            SenderId = senderId;
            Text = text;
            ReceiverId = receiverId;
        }

        public override string ToString() => $"MessageDTO[{SenderId} --> {ReceiverId} : {Text}]";
    }
}