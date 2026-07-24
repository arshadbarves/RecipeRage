# Live promotion runbook

1. Confirm SOFT_LAUNCH_GATE = GO and Stage soak done.
2. Single commit: all eos_*_config.json → Live SandboxId/DeploymentId.
3. bundleVersion bump (e.g. 0.1.0).
4. Build Android/iOS store binaries; smoke guest login + one match on Live.
5. Submit store; monitor crash + EOS error rates 24h.
6. Rollback = revert config commit to Stage and hotfix build.

## Live ids
- SandboxId: 19df6d3517a34ba480c2a65880c8567c
- DeploymentId: 70f48f125a80447688e18cd17aac17db
