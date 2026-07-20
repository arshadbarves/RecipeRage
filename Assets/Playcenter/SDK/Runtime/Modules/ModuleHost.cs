using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Playcenter.SDK
{
    public sealed class ModuleHost
    {
        /// <summary>
        /// Runs each module in order. Returns null on full success, or a BootFailure on the
        /// first failure or cancellation.
        /// </summary>
        public async Task<BootFailure> RunAsync(
            IReadOnlyList<IPlaycenterModule> modules,
            ModuleContext context,
            CancellationToken ct)
        {
            if (ct.IsCancellationRequested)
                return new BootFailure(BootFailureCode.Cancelled, "Cancelled before start.", null);

            foreach (var module in modules)
            {
                if (ct.IsCancellationRequested)
                    return new BootFailure(BootFailureCode.Cancelled, "Cancelled before module.", null);

                try
                {
                    var result = await module.InitializeAsync(context, ct);
                    if (!result.Success)
                    {
                        var f = result.Failure;
                        return new BootFailure(f.Code, f.Message, module.Id);
                    }
                }
                catch (OperationCanceledException)
                {
                    return new BootFailure(BootFailureCode.Cancelled, "Cancelled during module.", module.Id);
                }
                catch (Exception ex)
                {
                    return new BootFailure(BootFailureCode.Unknown, ex.Message, module.Id);
                }
            }

            return null;
        }

        /// <summary>
        /// Re-runs modules starting from the module with <paramref name="moduleId"/>.
        /// Prior modules in the list are assumed already complete and are skipped.
        /// </summary>
        public Task<BootFailure> RetryFromAsync(
            string moduleId,
            IReadOnlyList<IPlaycenterModule> modules,
            ModuleContext context,
            CancellationToken ct)
        {
            int startIndex = -1;
            for (int i = 0; i < modules.Count; i++)
            {
                if (modules[i].Id == moduleId)
                {
                    startIndex = i;
                    break;
                }
            }

            if (startIndex < 0)
                throw new ArgumentException($"Module '{moduleId}' not found in the provided list.", nameof(moduleId));

            var slice = new List<IPlaycenterModule>(modules.Count - startIndex);
            for (int i = startIndex; i < modules.Count; i++)
                slice.Add(modules[i]);

            return RunAsync(slice, context, ct);
        }
    }
}
