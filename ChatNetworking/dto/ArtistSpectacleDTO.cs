using System;

namespace Ticketing.Networking.Dto
{
    [Serializable]
    public class ArtistSpectacleDto
    {
        public long? Id { get; set; }
        public long? ArtistId { get; set; }
        public long? SpectacleId { get; set; }

        public ArtistSpectacleDto() { }

        public ArtistSpectacleDto(long? id, long? artistId, long? spectacleId)
        {
            Id = id;
            ArtistId = artistId;
            SpectacleId = spectacleId;
        }

        public ArtistSpectacleDto(long? artistId, long? spectacleId) : this(null, artistId, spectacleId) { }

        public override string ToString() => $"ArtistSpectacleDTO{{id={Id}, artistId={ArtistId}, spectacleId={SpectacleId}}}";
    }
}