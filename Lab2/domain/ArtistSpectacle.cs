using System;
using Org.Example.Domain;

namespace Lab2.domain
{
    public class ArtistSpectacle : Entity<long?>
    {
        public long ArtistId { get; set; }
        public long SpectacleId { get; set; }
        public Artist? Artist { get; set; }
        public Spectacle? Spectacle { get; set; }

        public ArtistSpectacle(long? id, long artistId, long spectacleId) : base(id)
        {
            ArtistId = artistId;
            SpectacleId = spectacleId;
        }

        public ArtistSpectacle(long artistId, long spectacleId) : base(null)
        {
            ArtistId = artistId;
            SpectacleId = spectacleId;
        }

        public ArtistSpectacle(long? id, Artist artist, Spectacle spectacle) : base(id)
        {
            ArtistId = artist.Id;
            SpectacleId = spectacle.Id ?? throw new ArgumentException("Spectacle ID nu poate fi null.", nameof(spectacle));
            Artist = artist;
            Spectacle = spectacle;
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(this, obj)) return true;
            if (obj is not ArtistSpectacle other) return false;
            if (!base.Equals(obj)) return false;

            return ArtistId == other.ArtistId && SpectacleId == other.SpectacleId;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), ArtistId, SpectacleId);
        }

        public override string ToString()
        {
            return $"ArtistSpectacle{{id={Id}, artistId={ArtistId}, spectacleId={SpectacleId}}}";
        }
    }
}

