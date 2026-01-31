using GiftServer.Contracts;

namespace GiftServer.Data;

public interface IStoreRepository
{
    Task<Store[]> GetStoresForPerson(int personId);
    Task AddStoreAsync(int personId, string name, string link);
    Task DeleteStoreAsync(int id);
}