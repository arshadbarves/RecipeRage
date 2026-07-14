using KitchenClash.Domain;
using Playcenter.GameFlow;
using VContainer.Unity;
using Playcenter.Shell;

namespace KitchenClash.Composition
{
    public class GameBootstrapper : IStartable
    {
        private readonly IAppFlow _appFlow;

        public GameBootstrapper(IAppFlow appFlow)
        {
            _appFlow = appFlow;
        }

        public void Start()
        {
            GameLogger.Log("GameBootstrapper starting AppFlow cold boot...");
            _appFlow.StartColdBoot();
        }
    }
}
