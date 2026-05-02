using System;
using KitchenClash.Domain;
using KitchenClash.Application;

namespace KitchenClash.Application.Services
{
    public class TrophyService : ITrophyService
    {
        private readonly IConfigService _cfg;
        private readonly ISaveService _saveService;
        private int _trophies;
        private const string StorageKey = "trophy_count";

        public int Trophies => _trophies;

        public TrophyService(IConfigService cfg, ISaveService saveService)
        {
            _cfg = cfg;
            _saveService = saveService;
        }

        public void Initialize()
        {
            _trophies = _saveService.Load(StorageKey, 0);
        }

        public int CalculateChange(MatchOutcome outcome)
        {
            return outcome switch
            {
                MatchOutcome.WinDominant => _cfg.Get("trophy_win_dominant", 35),
                MatchOutcome.WinStandard => _cfg.Get("trophy_win_standard", 25),
                MatchOutcome.WinClose => _cfg.Get("trophy_win_close", 20),
                MatchOutcome.LossClose => _cfg.Get("trophy_loss_close", -15),
                MatchOutcome.LossStandard => _cfg.Get("trophy_loss_standard", -20),
                MatchOutcome.Disconnect => _cfg.Get("trophy_disconnect", -30),
                _ => 0
            };
        }

        public void ApplyChange(int delta)
        {
            _trophies = Math.Max(0, _trophies + delta);
            _saveService.Save(StorageKey, _trophies);
        }
    }
}
