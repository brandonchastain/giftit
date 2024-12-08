namespace GiftServer.Notification
{
    public interface INotifier
    {
        Task Notify(string destination, string message);
    }
}