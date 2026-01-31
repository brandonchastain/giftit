namespace GiftServer.Contracts
{
    public record Gift(
        int Id,
        string Name,
        int PersonId,
        string? Link,
        string? Date,
        bool IsPurchased);
}