namespace GiftServer
{
    public record Store(
        Guid Id,
        Guid PersonId,
        string Name,
        string Url
    );
}