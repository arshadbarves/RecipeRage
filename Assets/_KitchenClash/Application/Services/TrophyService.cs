using System;
using KitchenClash.Application;
using KitchenClash.Domain;

namespace KitchenClash.Application.Services
{
    /// <summary>
    /// GDD v3 trophy system. Uses RC keys with hardcoded defaults.
    /// Win types: dominant (diff > 30%), standard (10-30%), close (&lt;10%).
    /// </summary>
    public sealed class TrophyService : ITrophyService
    {
        private const string SaveKey = "player_trophies";

        // RC key names and defaults per GDD
        private const int DefaultWinDominant = 35;
        private const int DefaultWinStandard = 25;
        private const int DefaultWinClose = 20;
        private const int DefaultLossClose = -15;
        private const int DefaultLossStandard = -20;
        private const int DefaultDisconnect = -30;

        private readonly ISaveService _saveService;
        private int _currentTrophies;

        public int CurrentTrophies => _currentTrophies;

        public TrophyService(ISaveService saveService)
        {
            _saveService = saveService;
            _currentTrophies = _saveService.Load(SaveKey, 0);
        }

        public TrophyResult CalculateMatchResult(bool won, int scoreDifference, bool disconnected)
        {
            if (disconnected)
            {
                int dcDelta = DefaultDisconnect;
                _currentTrophies = Math.Max(0, _currentTrophies + dcDelta);
                Persist();
                return new TrophyResult(dcDelta, _currentTrophies, WinType.Standard, false, true);
            }

            // Determine win type by score diff percentage (treat scoreDifference as absolute %)
            int absDiff = Math.Abs(scoreDifference);
            WinType winType;
            if (absDiff > 30)
                winType = WinType.Dominant;
            else if (absDiff >= 10)
                winType = WinType.Standard;
            else
                winType = WinType.Close;

            int delta;
            if (won)
            {
                delta = winType switch
                {
                    WinType.Dominant => DefaultWinDominant,
                    WinType.Standard => DefaultWinStandard,
                    WinType.Close => DefaultWinClose,
                    _ => DefaultWinStandard,
                };
            }
            else
            {
                delta = winType switch
                {
                    WinType.Close => DefaultLossClose,
                    _ => DefaultLossStandard,
                };
            }

            _currentTrophies = Math.Max(0, _currentTrophies + delta);
            Persist();
            return new TrophyResult(delta, _currentTrophies, winType, won, false);
        }

        private void Persist()
        {
            _saveService.Save(SaveKey, _currentTrophies);
        }
    }
}
