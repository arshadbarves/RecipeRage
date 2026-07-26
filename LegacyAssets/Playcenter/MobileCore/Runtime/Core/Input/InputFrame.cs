using Playcenter.Services;

namespace Playcenter.MobileCore
{
    /// <summary>
    /// Versioned wire-ready input snapshot. Bump Version on any layout change.
    /// Uses InputAxis2 (Playcenter.Services) — no UnityEngine types.
    /// </summary>
    public readonly struct InputFrame
    {
        public const byte CurrentVersion = 1;

        public byte Version => CurrentVersion;
        public uint SequenceNumber { get; }
        public float DeltaTime { get; }
        public InputAxis2 Move { get; }
        public InputAxis2 Aim { get; }
        public InputButtons Buttons { get; }

        public InputFrame(uint sequenceNumber, float deltaTime, InputAxis2 move, InputAxis2 aim, InputButtons buttons)
        {
            SequenceNumber = sequenceNumber;
            DeltaTime = deltaTime;
            Move = move;
            Aim = aim;
            Buttons = buttons;
        }
    }
}
