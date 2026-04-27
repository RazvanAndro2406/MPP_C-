namespace Ticketing.Model.Domain;

public sealed class Artist : Entity<long>
{
    public string Name { get; set; }

    public Artist(long id, string name) : base(id)
    {
        Name = name;
    }

    public Artist(string name) : base(0)
    {
        Name = name;
    }
}

