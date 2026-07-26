using System;

namespace KitchenClash.Application.Services
{
    public interface INetworkingServices : IDisposable
    {
        IBotSpawner BotSpawner { get; }
        IGameStarter GameStarter { get; }
    }
}
