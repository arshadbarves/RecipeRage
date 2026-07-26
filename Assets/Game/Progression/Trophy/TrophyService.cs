using System;
using System.Collections.Generic;
using Playcenter;
using Playcenter.Services;

namespace RecipeRage
{
    public interface ITrophyService
    {
        event Action<int> OnTrophiesChanged;
        int Trophies { get; }
        void ApplyMatchResult(bool won);
    }

    /// <summary>
    /// Brawl Stars-style trophies: win +15, loss -8 (floor 0). Separate from
    /// coins — coins are never lost, trophies rise and fall.
    /// </summary>
    public sealed class TrophyService : ITrophyService
    {
        private const string SaveKey = "trophies";
        private const int WinAmount = 15;
        private const int LossAmount = -8;

        private readonly ISaveService _save;
        private readonly IAnalyticsService _analytics;
        private readonly TrophyData _data;

        public event Action<int> OnTrophiesChanged;

        public int Trophies => _data.Trophies;

        public TrophyService(ISaveService save, IAnalyticsService analytics)
        {
            _save = save;
            _analytics = analytics;
            _data = _save.Load(SaveKey, new TrophyData());
        }

        public void ApplyMatchResult(bool won)
        {
            var delta = won ? WinAmount : LossAmount;
            _data.Trophies = Math.Max(0, _data.Trophies + delta);
            _save.Save(SaveKey, _data);

            _analytics.TrackEvent("trophies_changed", new Dictionary<string, object>
            {
                { "won", won },
                { "delta", delta },
                { "total", _data.Trophies }
            });
            OnTrophiesChanged?.Invoke(_data.Trophies);
        }
    }
}
