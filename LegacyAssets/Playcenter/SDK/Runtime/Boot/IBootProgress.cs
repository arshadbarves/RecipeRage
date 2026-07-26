using System;

namespace Playcenter.SDK
{
    public interface IBootProgress
    {
        float Overall01 { get; }
        void Report(string moduleId, float local01);
        event Action<float, string> Changed;
    }
}
