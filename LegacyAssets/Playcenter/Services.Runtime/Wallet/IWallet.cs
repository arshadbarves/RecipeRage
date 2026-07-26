namespace Playcenter.Services
{
    public interface IWallet
    {
        int GetBalance(CurrencyId currency);
        bool HasItem(string itemId);
    }
}
