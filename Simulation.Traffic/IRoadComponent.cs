using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Simulation.Traffic
{
    public interface IRoadComponent
    {
        void Invalidate();
        Task ValidateAsync();
        event Action Invalidated;
    }
    public interface IRoadComponent<T> : IRoadComponent
    {
        Task<T> Value { get; }
    }
    

    public abstract class RoadComponent<T> : IRoadComponent<T>
        where T : class
    {

        private CancellationTokenSource cancelSource;

        private Task<T> value;
        private SemaphoreSlim semaphore = new SemaphoreSlim(1);

        public event Action Invalidated;

        public Task<T> Value => GetValueInternal();

        private async Task<T> GetValueInternal()
        {
            if (value?.IsCompleted == true) return await value; // completed
            if (value?.IsCompleted == false) return await value; // still running, not cancelled

            // null value or cancelled

            try
            {
                await semaphore.WaitAsync();

                if (cancelSource?.Token.IsCancellationRequested != false)
                {
                    if (value?.IsCompleted == true) return await value; // completed
                    if (value?.IsCompleted == false) return await value; // still running, not cancelled
                }

                var cts = new CancellationTokenSource();
                value = GetValue(cts.Token);
                cancelSource = cts;
            }
            finally
            {
                semaphore.Release();
            }
            return await value;
        }

        protected abstract Task<T> GetValue(CancellationToken cancel);

        public void Invalidate()
        {
            value = null;
            cancelSource?.Cancel();
            Invalidated?.Invoke();
        }

        public Task ValidateAsync() => GetValueInternal();
    }
}
