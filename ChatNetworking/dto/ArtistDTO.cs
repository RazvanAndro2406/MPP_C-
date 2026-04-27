using System;

namespace Ticketing.Networking.Dto
{
    [Serializable]
    public class ArtistDto
    {
        public long? Id { get; set; }
        public string Name { get; set; }

        public ArtistDto() { }

        public ArtistDto(long? id, string name)
        {
            Id = id;
            Name = name;
        }

        public ArtistDto(string name) : this(null, name) { }

        public override string ToString() => $"ArtistDTO{{id={Id}, name='{Name}'}}";
    }
}