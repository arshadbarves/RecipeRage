namespace Playcenter.Services
{
    public interface IWalletStore
    {
        System.Threading.Tasks.Task<WalletSnapshot> LoadAsync(System.Threading.CancellationToken ct = default);
        System.Threading.Tasks.Task SaveAsync(WalletSnapshot snapshot, System.Threading.CancellationToken ct = default);
    }
}
