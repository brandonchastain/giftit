
using GiftServer.Contracts;

namespace GiftServer.Data;

public interface IUserRepository
{

        Task<User> GetUser(string email);

        Task AddNewUser(string email, string name);

        Task SetReminderDurAsync(Guid userId, TimeSpan reminderDur);
}