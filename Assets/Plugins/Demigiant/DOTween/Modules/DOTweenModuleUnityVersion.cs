﻿// Author: Daniele Giardini - http://www.demigiant.com
// Created: 2018/07/13

using System.Threading.Tasks;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;
//#if UNITY_2018_1_OR_NEWER && (NET_4_6 || NET_STANDARD_2_0)
//using Task = System.Threading.Tasks.Task;
//#endif

#pragma warning disable 1591
namespace DG.Tweening
{
    /// <summary>
    ///     Shortcuts/functions that are not strictly related to specific Modules
    ///     but are available only on some Unity versions
    /// </summary>
    public static class DoTweenModuleUnityVersion
    {
        #region Material

        /// <summary>
        ///     Tweens a Material's color using the given gradient
        ///     (NOTE 1: only uses the colors of the gradient, not the alphas - NOTE 2: creates a Sequence, not a Tweener).
        ///     Also stores the image as the tween's target so it can be used for filtered operations
        /// </summary>
        /// <param name="gradient">The gradient to use</param>
        /// <param name="duration">The duration of the tween</param>
        public static Sequence DoGradientColor(this Material target, Gradient gradient, float duration)
        {
            Sequence s = DOTween.Sequence();
            GradientColorKey[] colors = gradient.colorKeys;
            int len = colors.Length;
            for (int i = 0; i < len; ++i)
            {
                GradientColorKey c = colors[i];
                if (i == 0 && c.time <= 0)
                {
                    target.color = c.color;
                    continue;
                }
                float colorDuration = i == len - 1
                    ? duration - s.Duration(false) // Verifies that total duration is correct
                    : duration * (i == 0 ? c.time : c.time - colors[i - 1].time);
                s.Append(target.DOColor(c.color, colorDuration).SetEase(Ease.Linear));
            }
            s.SetTarget(target);
            return s;
        }
        /// <summary>
        ///     Tweens a Material's named color property using the given gradient
        ///     (NOTE 1: only uses the colors of the gradient, not the alphas - NOTE 2: creates a Sequence, not a Tweener).
        ///     Also stores the image as the tween's target so it can be used for filtered operations
        /// </summary>
        /// <param name="gradient">The gradient to use</param>
        /// <param name="property">The name of the material property to tween (like _Tint or _SpecColor)</param>
        /// <param name="duration">The duration of the tween</param>
        public static Sequence DoGradientColor(this Material target, Gradient gradient, string property, float duration)
        {
            Sequence s = DOTween.Sequence();
            GradientColorKey[] colors = gradient.colorKeys;
            int len = colors.Length;
            for (int i = 0; i < len; ++i)
            {
                GradientColorKey c = colors[i];
                if (i == 0 && c.time <= 0)
                {
                    target.SetColor(property, c.color);
                    continue;
                }
                float colorDuration = i == len - 1
                    ? duration - s.Duration(false) // Verifies that total duration is correct
                    : duration * (i == 0 ? c.time : c.time - colors[i - 1].time);
                s.Append(target.DOColor(c.color, property, colorDuration).SetEase(Ease.Linear));
            }
            s.SetTarget(target);
            return s;
        }

        #endregion

        #region CustomYieldInstructions

        /// <summary>
        ///     Returns a <see cref="CustomYieldInstruction" /> that waits until the tween is killed or complete.
        ///     It can be used inside a coroutine as a yield.
        ///     <para>Example usage:</para>
        ///     <code>yield return myTween.WaitForCompletion(true);</code>
        /// </summary>
        public static CustomYieldInstruction WaitForCompletion(this Tween t, bool returnCustomYieldInstruction)
        {
            if (!t.active)
            {
                if (Debugger.logPriority > 0) Debugger.LogInvalidTween(t);
                return null;
            }
            return new DoTweenCyInstruction.WaitForCompletion(t);
        }

        /// <summary>
        ///     Returns a <see cref="CustomYieldInstruction" /> that waits until the tween is killed or rewinded.
        ///     It can be used inside a coroutine as a yield.
        ///     <para>Example usage:</para>
        ///     <code>yield return myTween.WaitForRewind();</code>
        /// </summary>
        public static CustomYieldInstruction WaitForRewind(this Tween t, bool returnCustomYieldInstruction)
        {
            if (!t.active)
            {
                if (Debugger.logPriority > 0) Debugger.LogInvalidTween(t);
                return null;
            }
            return new DoTweenCyInstruction.WaitForRewind(t);
        }

        /// <summary>
        ///     Returns a <see cref="CustomYieldInstruction" /> that waits until the tween is killed.
        ///     It can be used inside a coroutine as a yield.
        ///     <para>Example usage:</para>
        ///     <code>yield return myTween.WaitForKill();</code>
        /// </summary>
        public static CustomYieldInstruction WaitForKill(this Tween t, bool returnCustomYieldInstruction)
        {
            if (!t.active)
            {
                if (Debugger.logPriority > 0) Debugger.LogInvalidTween(t);
                return null;
            }
            return new DoTweenCyInstruction.WaitForKill(t);
        }

        /// <summary>
        ///     Returns a <see cref="CustomYieldInstruction" /> that waits until the tween is killed or has gone through the given
        ///     amount of loops.
        ///     It can be used inside a coroutine as a yield.
        ///     <para>Example usage:</para>
        ///     <code>yield return myTween.WaitForElapsedLoops(2);</code>
        /// </summary>
        /// <param name="elapsedLoops">Elapsed loops to wait for</param>
        public static CustomYieldInstruction WaitForElapsedLoops(this Tween t, int elapsedLoops, bool returnCustomYieldInstruction)
        {
            if (!t.active)
            {
                if (Debugger.logPriority > 0) Debugger.LogInvalidTween(t);
                return null;
            }
            return new DoTweenCyInstruction.WaitForElapsedLoops(t, elapsedLoops);
        }

        /// <summary>
        ///     Returns a <see cref="CustomYieldInstruction" /> that waits until the tween is killed
        ///     or has reached the given time position (loops included, delays excluded).
        ///     It can be used inside a coroutine as a yield.
        ///     <para>Example usage:</para>
        ///     <code>yield return myTween.WaitForPosition(2.5f);</code>
        /// </summary>
        /// <param name="position">Position (loops included, delays excluded) to wait for</param>
        public static CustomYieldInstruction WaitForPosition(this Tween t, float position, bool returnCustomYieldInstruction)
        {
            if (!t.active)
            {
                if (Debugger.logPriority > 0) Debugger.LogInvalidTween(t);
                return null;
            }
            return new DoTweenCyInstruction.WaitForPosition(t, position);
        }

        /// <summary>
        ///     Returns a <see cref="CustomYieldInstruction" /> that waits until the tween is killed or started
        ///     (meaning when the tween is set in a playing state the first time, after any eventual delay).
        ///     It can be used inside a coroutine as a yield.
        ///     <para>Example usage:</para>
        ///     <code>yield return myTween.WaitForStart();</code>
        /// </summary>
        public static CustomYieldInstruction WaitForStart(this Tween t, bool returnCustomYieldInstruction)
        {
            if (!t.active)
            {
                if (Debugger.logPriority > 0) Debugger.LogInvalidTween(t);
                return null;
            }
            return new DoTweenCyInstruction.WaitForStart(t);
        }

        #endregion

#if UNITY_2018_1_OR_NEWER
        #region Unity 2018.1 or Newer

        #region Material

        /// <summary>
        ///     Tweens a Material's named texture offset property with the given ID to the given value.
        ///     Also stores the material as the tween's target so it can be used for filtered operations
        /// </summary>
        /// <param name="endValue">The end value to reach</param>
        /// <param name="propertyID">The ID of the material property to tween (also called nameID in Unity's manual)</param>
        /// <param name="duration">The duration of the tween</param>
        public static TweenerCore<Vector2, Vector2, VectorOptions> DoOffset(this Material target, Vector2 endValue, int propertyID, float duration)
        {
            if (!target.HasProperty(propertyID))
            {
                if (Debugger.logPriority > 0) Debugger.LogMissingMaterialProperty(propertyID);
                return null;
            }
            TweenerCore<Vector2, Vector2, VectorOptions> t = DOTween.To(() => target.GetTextureOffset(propertyID), x => target.SetTextureOffset(propertyID, x), endValue, duration);
            t.SetTarget(target);
            return t;
        }

        /// <summary>
        ///     Tweens a Material's named texture scale property with the given ID to the given value.
        ///     Also stores the material as the tween's target so it can be used for filtered operations
        /// </summary>
        /// <param name="endValue">The end value to reach</param>
        /// <param name="propertyID">The ID of the material property to tween (also called nameID in Unity's manual)</param>
        /// <param name="duration">The duration of the tween</param>
        public static TweenerCore<Vector2, Vector2, VectorOptions> DoTiling(this Material target, Vector2 endValue, int propertyID, float duration)
        {
            if (!target.HasProperty(propertyID))
            {
                if (Debugger.logPriority > 0) Debugger.LogMissingMaterialProperty(propertyID);
                return null;
            }
            TweenerCore<Vector2, Vector2, VectorOptions> t = DOTween.To(() => target.GetTextureScale(propertyID), x => target.SetTextureScale(propertyID, x), endValue, duration);
            t.SetTarget(target);
            return t;
        }

        #endregion

        #region .NET 4.6 or Newer

#if UNITY_2018_1_OR_NEWER && (NET_4_6 || NET_STANDARD_2_0)

        #region Async Instructions

        /// <summary>
        ///     Returns an async <see cref="System.Threading.Tasks.Task" /> that waits until the tween is killed or complete.
        ///     It can be used inside an async operation.
        ///     <para>Example usage:</para>
        ///     <code>await myTween.WaitForCompletion();</code>
        /// </summary>
        public static async Task AsyncWaitForCompletion(this Tween t)
        {
            if (!t.active)
            {
                if (Debugger.logPriority > 0) Debugger.LogInvalidTween(t);
                return;
            }
            while (t.active && !t.IsComplete()) await Task.Yield();
        }

        /// <summary>
        ///     Returns an async <see cref="System.Threading.Tasks.Task" /> that waits until the tween is killed or rewinded.
        ///     It can be used inside an async operation.
        ///     <para>Example usage:</para>
        ///     <code>await myTween.AsyncWaitForRewind();</code>
        /// </summary>
        public static async Task AsyncWaitForRewind(this Tween t)
        {
            if (!t.active)
            {
                if (Debugger.logPriority > 0) Debugger.LogInvalidTween(t);
                return;
            }
            while (t.active && (!t.playedOnce || t.position * (t.CompletedLoops() + 1) > 0)) await Task.Yield();
        }

        /// <summary>
        ///     Returns an async <see cref="System.Threading.Tasks.Task" /> that waits until the tween is killed.
        ///     It can be used inside an async operation.
        ///     <para>Example usage:</para>
        ///     <code>await myTween.AsyncWaitForKill();</code>
        /// </summary>
        public static async Task AsyncWaitForKill(this Tween t)
        {
            if (!t.active)
            {
                if (Debugger.logPriority > 0) Debugger.LogInvalidTween(t);
                return;
            }
            while (t.active) await Task.Yield();
        }

        /// <summary>
        ///     Returns an async <see cref="System.Threading.Tasks.Task" /> that waits until the tween is killed or has gone
        ///     through the given amount of loops.
        ///     It can be used inside an async operation.
        ///     <para>Example usage:</para>
        ///     <code>await myTween.AsyncWaitForElapsedLoops();</code>
        /// </summary>
        /// <param name="elapsedLoops">Elapsed loops to wait for</param>
        public static async Task AsyncWaitForElapsedLoops(this Tween t, int elapsedLoops)
        {
            if (!t.active)
            {
                if (Debugger.logPriority > 0) Debugger.LogInvalidTween(t);
                return;
            }
            while (t.active && t.CompletedLoops() < elapsedLoops) await Task.Yield();
        }

        /// <summary>
        ///     Returns an async <see cref="System.Threading.Tasks.Task" /> that waits until the tween is killed or started
        ///     (meaning when the tween is set in a playing state the first time, after any eventual delay).
        ///     It can be used inside an async operation.
        ///     <para>Example usage:</para>
        ///     <code>await myTween.AsyncWaitForPosition();</code>
        /// </summary>
        /// <param name="position">Position (loops included, delays excluded) to wait for</param>
        public static async Task AsyncWaitForPosition(this Tween t, float position)
        {
            if (!t.active)
            {
                if (Debugger.logPriority > 0) Debugger.LogInvalidTween(t);
                return;
            }
            while (t.active && t.position * (t.CompletedLoops() + 1) < position) await Task.Yield();
        }

        /// <summary>
        ///     Returns an async <see cref="System.Threading.Tasks.Task" /> that waits until the tween is killed.
        ///     It can be used inside an async operation.
        ///     <para>Example usage:</para>
        ///     <code>await myTween.AsyncWaitForKill();</code>
        /// </summary>
        public static async Task AsyncWaitForStart(this Tween t)
        {
            if (!t.active)
            {
                if (Debugger.logPriority > 0) Debugger.LogInvalidTween(t);
                return;
            }
            while (t.active && !t.playedOnce) await Task.Yield();
        }

        #endregion
#endif

        #endregion

        #endregion
#endif
    }

    // █████████████████████████████████████████████████████████████████████████████████████████████████████████████████████
    // ███ CLASSES █████████████████████████████████████████████████████████████████████████████████████████████████████████
    // █████████████████████████████████████████████████████████████████████████████████████████████████████████████████████

    public static class DoTweenCyInstruction
    {
        public class WaitForCompletion : CustomYieldInstruction
        {
            private readonly Tween _t;
            public WaitForCompletion(Tween tween)
            {
                _t = tween;
            }
            public override bool keepWaiting => _t.active && !_t.IsComplete();
        }

        public class WaitForRewind : CustomYieldInstruction
        {
            private readonly Tween _t;
            public WaitForRewind(Tween tween)
            {
                _t = tween;
            }
            public override bool keepWaiting => _t.active && (!_t.playedOnce || _t.position * (_t.CompletedLoops() + 1) > 0);
        }

        public class WaitForKill : CustomYieldInstruction
        {
            private readonly Tween _t;
            public WaitForKill(Tween tween)
            {
                _t = tween;
            }
            public override bool keepWaiting => _t.active;
        }

        public class WaitForElapsedLoops : CustomYieldInstruction
        {
            private readonly int _elapsedLoops;
            private readonly Tween _t;
            public WaitForElapsedLoops(Tween tween, int elapsedLoops)
            {
                _t = tween;
                _elapsedLoops = elapsedLoops;
            }
            public override bool keepWaiting => _t.active && _t.CompletedLoops() < _elapsedLoops;
        }

        public class WaitForPosition : CustomYieldInstruction
        {
            private readonly float _position;
            private readonly Tween _t;
            public WaitForPosition(Tween tween, float position)
            {
                _t = tween;
                _position = position;
            }
            public override bool keepWaiting => _t.active && _t.position * (_t.CompletedLoops() + 1) < _position;
        }

        public class WaitForStart : CustomYieldInstruction
        {
            private readonly Tween _t;
            public WaitForStart(Tween tween)
            {
                _t = tween;
            }
            public override bool keepWaiting => _t.active && !_t.playedOnce;
        }
    }
}