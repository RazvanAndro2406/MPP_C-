using System;

namespace Ticketing.Networking.Dto
{
    [Serializable]
    public class BuyerDto
    {
        public long? Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }

        public BuyerDto() { }

        public BuyerDto(long? id, string name, string email)
        {
            Id = id;
            Name = name;
            Email = email;
        }
    }
}