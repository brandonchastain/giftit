using GiftServer.Contracts;

namespace GiftServer.Data;

public interface IGiftRepository
{

    Task<Gift[]> GetGiftIdeasForPerson(int personId);
    Task<Gift> GetGift(int giftId);
    Task SetIsPurchased(int giftId);
    Task<Gift> AddNewGift(string name, int personId, string link, string date);
    Task DeleteGift(int id);
}