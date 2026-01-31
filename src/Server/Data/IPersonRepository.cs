using GiftServer.Contracts;

namespace GiftServer.Data;

public interface IPersonRepository
{
    Task<Person[]> GetMyPeople(int userId);
    Task<Person?> GetPerson(int id);
    Task AddNewPerson(string name, string birthday, int userId);
    Task DeletePerson(int id);
}