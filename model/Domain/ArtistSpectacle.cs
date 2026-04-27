namespace Ticketing.Model.Domain;

public sealed class ArtistSpectacle : Entity<long>
{
    public long ArtistId { get; set; }
    public long SpectacleId { get; set; }

    public ArtistSpectacle(long id, long artistId, long spectacleId) : base(id)
    {
        ArtistId = artistId;
        SpectacleId = spectacleId;
    }

    public ArtistSpectacle(long artistId, long spectacleId) : base(0)
    {
        ArtistId = artistId;
        SpectacleId = spectacleId;
    }
}

