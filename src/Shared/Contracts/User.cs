namespace GiftServer.Contracts
{
    public record User(int Id, string Name, string Email, bool IsAdmin, TimeSpan ReminderDuration);
}