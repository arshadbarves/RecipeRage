namespace Playcenter.Services
{
    public interface IWalletLedger
    {
        bool TryDebit(CurrencyId currency, int amount, string reason);
        void Credit(CurrencyId currency, int amount, string reason);
        bool TryPurchase(string itemId, CurrencyId currency, int cost, string reason);
    }
}
