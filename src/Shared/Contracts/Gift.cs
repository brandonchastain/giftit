namespace GiftServer.Contracts
{
    public record Gift(
        int Id,
        string Name,
        string? PersonName,
        string? Link,
        string? Date,
        bool IsPurchased);
}