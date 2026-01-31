namespace GiftServer.Contracts
{
    public record Store(
        int Id,
        int PersonId,
        string Name,
        string Url
    );
}