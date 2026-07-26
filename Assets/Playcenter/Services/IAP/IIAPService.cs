using System;
using System.Collections;

namespace Playcenter.Services
{
    public interface IIAPService
    {
        bool IsReady { get; }
        event Action<string> OnPurchaseCompleted;
        IEnumerator Initialize();
        void Purchase(string productId);
        bool IsProductAvailable(string productId);
        string GetLocalizedPrice(string productId);
    }
}
