namespace Ticketing.Model.Domain
{
    public interface IIdentifiable<TId>
    {
        TId Id { get; set; }
    }
}