namespace Playcenter.Services
{
    public sealed class WalletSnapshot
    {
        public int Coins { get; set; }
        public int Gems { get; set; }
        public string[] OwnedItemIds { get; set; }
    }
}
