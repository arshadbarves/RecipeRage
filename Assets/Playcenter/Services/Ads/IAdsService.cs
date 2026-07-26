using System;
using System.Collections;

namespace Playcenter.Services
{
    public interface IAdsService
    {
        bool IsReady { get; }
        bool IsRewardedReady { get; }
        IEnumerator Initialize();
        void ShowRewardedAd(string placement, Action<bool> onComplete);
        void ShowInterstitial();
    }
}
