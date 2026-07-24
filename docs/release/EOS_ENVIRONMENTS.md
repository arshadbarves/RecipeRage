# EOS Environments

| Name  | SandboxId                          | DeploymentId                         |
|-------|------------------------------------|--------------------------------------|
| Dev   | p-3e949b6n57y7qcjyg5sccpyatyzser   | 146b53cc89584a8d9586e9dd1f0caf91     |
| Stage | p-uvb48fad3qb2tza5wyetcx2hyxnrpt   | b27af9e630504620a05e5794e16ce190     |
| Live  | 19df6d3517a34ba480c2a65880c8567c   | 70f48f125a80447688e18cd17aac17db     |

## Development builds
All `Assets/StreamingAssets/EOS/eos_*_config.json` `deployment.SandboxId` + `deployment.DeploymentId` MUST match **Dev**.

## Promotion
Before Stage/Live store builds, set every platform file to Stage then Live as a single commit pair. Never mix sandboxes across platforms in one build train.
