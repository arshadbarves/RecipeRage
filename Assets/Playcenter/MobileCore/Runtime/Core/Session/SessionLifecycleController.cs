using System;
using System.Threading.Tasks;

namespace Playcenter.MobileCore
{
    /// <summary>
    /// Fail-closed session FSM: None → Creating → Active → TearingDown → None.
    /// Illegal transitions throw. Mirrors AppFlowController's fail-closed discipline.
    /// </summary>
    public sealed class SessionLifecycleController
    {
        private readonly ISessionScopeFactory _factory;
        private readonly ISessionScopeInstaller _installer;

        private ISessionScopeHandle _scope;

        public SessionState State { get; private set; } = SessionState.None;
        public ISessionScopeHandle Scope => _scope;
        public event Action<SessionState, SessionState> Transitioned;

        public SessionLifecycleController(ISessionScopeFactory factory, ISessionScopeInstaller installer)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            _installer = installer;
        }

        public Task CreateAsync()
        {
            if (_installer == null)
            {
                throw new InvalidOperationException(
                    "ISessionScopeInstaller is required. Register a session installer before CreateAsync.");
            }

            if (State != SessionState.None)
            {
                throw new InvalidOperationException($"Cannot create session from state {State}.");
            }

            Transition(SessionState.Creating);
            _scope = _factory.Create(_installer);
            Transition(SessionState.Active);
            return Task.CompletedTask;
        }

        public Task TeardownAsync()
        {
            if (State != SessionState.Active)
            {
                throw new InvalidOperationException($"Cannot tear down session from state {State}.");
            }

            Transition(SessionState.TearingDown);
            _scope?.Dispose();
            _scope = null;
            Transition(SessionState.None);
            return Task.CompletedTask;
        }

        private void Transition(SessionState next)
        {
            SessionState previous = State;
            State = next;
            Transitioned?.Invoke(previous, next);
        }
    }
}
