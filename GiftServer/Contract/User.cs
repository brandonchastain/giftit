namespace GiftServer
{
    public record User(Guid Id, string Name, string Email, bool IsAdmin);
}