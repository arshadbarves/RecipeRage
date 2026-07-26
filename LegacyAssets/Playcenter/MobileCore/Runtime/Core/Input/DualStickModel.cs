using Playcenter.Services;

namespace Playcenter.MobileCore
{
    /// <summary>
    /// Engine-free dual-stick state machine. Left half of screen = move stick,
    /// right half = aim stick. Feed raw PointerEvents; read one InputFrame per Tick().
    /// Chop taps are right-side quick taps tracked by the embedded TapGestureDetector.
    /// </summary>
    public sealed class DualStickModel
    {
        private readonly DualStickConfig _config;
        private readonly IGameClock _clock;
        private readonly TapGestureDetector _chopTaps;

        private int _movePointerId = -1;
        private int _aimPointerId = -1;
        private InputAxis2 _move;
        private InputAxis2 _aim;
        private bool _aimReleasedPending;
        private uint _sequence;

        public DualStickModel(DualStickConfig config, IGameClock clock)
        {
            _config = config;
            _clock = clock;
            _chopTaps = new TapGestureDetector(config.TapWindowSeconds, config.TapIdleResetSeconds, clock);
        }

        public int ChopTapCount => _chopTaps.TapCount;
        public bool AimActive => _aimPointerId >= 0;

        public void OnPointer(in PointerEvent e)
        {
            bool isLeftSide = e.X < e.HalfWidth;

            switch (e.Phase)
            {
                case PointerPhase.Began:
                    if (isLeftSide && _movePointerId < 0)
                    {
                        _movePointerId = e.PointerId;
                        _move = Normalize(e, isLeftSide);
                    }
                    else if (!isLeftSide && _aimPointerId < 0)
                    {
                        _aimPointerId = e.PointerId;
                        _aim = Normalize(e, isLeftSide);
                    }
                    break;

                case PointerPhase.Moved:
                    if (e.PointerId == _movePointerId)
                    {
                        _move = Normalize(e, isLeftSide);
                    }
                    else if (e.PointerId == _aimPointerId)
                    {
                        _aim = Normalize(e, isLeftSide);
                    }
                    break;

                case PointerPhase.Ended:
                case PointerPhase.Cancelled:
                    if (e.PointerId == _movePointerId)
                    {
                        _movePointerId = -1;
                        _move = InputAxis2.Zero;
                    }
                    else if (e.PointerId == _aimPointerId)
                    {
                        _aimPointerId = -1;
                        _aimReleasedPending = true;
                        _aim = InputAxis2.Zero;
                    }
                    break;
            }
        }

        /// <summary>Registers one chop tap (right-side quick tap, game decides what counts).</summary>
        public void RegisterChopTap()
        {
            _chopTaps.OnTap();
        }

        public InputFrame Tick()
        {
            InputButtons buttons = InputButtons.None;
            if (_aimReleasedPending)
            {
                buttons |= InputButtons.AimReleased;
                _aimReleasedPending = false;
            }

            return new InputFrame(
                sequenceNumber: _sequence++,
                deltaTime: _clock.DeltaTime,
                move: ApplyDeadzone(_move),
                aim: ApplyDeadzone(_aim),
                buttons: buttons);
        }

        private InputAxis2 Normalize(in PointerEvent e, bool isLeftSide)
        {
            float centerX = isLeftSide ? e.HalfWidth * 0.5f : e.HalfWidth * 1.5f;
            float nx = (e.X - centerX) / (e.HalfWidth * 0.5f);
            float ny = (e.Y - e.HalfHeight) / e.HalfHeight;
            return ClampToUnit(new InputAxis2(nx, ny));
        }

        private InputAxis2 ApplyDeadzone(InputAxis2 axis)
        {
            return axis.Magnitude < _config.Deadzone ? InputAxis2.Zero : axis;
        }

        private static InputAxis2 ClampToUnit(InputAxis2 axis)
        {
            return axis.SqrMagnitude > 1f ? axis.Normalized : axis;
        }
    }
}
