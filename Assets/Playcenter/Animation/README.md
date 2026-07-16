# Playcenter.Animation

Unity-thin DOTween animation service for multi-title shells.

## Rules
1. May reference Playcenter.Shell, DOTween, UniTask, Unity.
2. Must NOT reference Playcenter.UI, Services, GameFlow, EOS, or KitchenClash.
3. Public async API uses UniTask (Unity-thin lock).
4. Composition registration stays in the game.
