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
        RoadComponentValue<T> Result { get; }
    }

    public struct RoadComponentValue<T>
    {

        public RoadComponentValue(Task<T> task, CancellationToken token) : this()
        {
            this.Task = task;
            this.Validity = token;
        }

        public Task<T> Task { get; }
        public CancellationToken Validity { get; }
    }

    public struct InternalStore<T>
    {
        public InternalStore(Task<T> task, CancellationTokenSource cts) : this()
        {
            Value = task;
            CancelSource = cts;
        }

        public Task<T> Value { get; }
        public CancellationTokenSource CancelSource { get; }
    }

    public abstract class RoadComponent<T> : IRoadComponent<T>
        where T : class
    {


        public RoadComponent()
        {
            reset();
        }

        private void reset()
        {
            lazy = new Lazy<InternalStore<T>>(initialize, true);
        }

        private Lazy<InternalStore<T>> lazy;

        public event Action Invalidated;

        public RoadComponentValue<T> Result
        {
            get
            {
                var value = lazy.Value;
                return new RoadComponentValue<T>(value.Value, value.CancelSource.Token);
            }
        }

        private InternalStore<T> initialize()
        {
            var cts = new CancellationTokenSource();
            return new InternalStore<T>(GetValueAsync(cts.Token), cts);
        }

        public void Invalidate()
        {
            if (lazy.IsValueCreated)
            {
                lazy.Value.CancelSource.Cancel();
                reset();
            }
        }

        protected abstract Task<T> GetValueAsync(CancellationToken cancel);

        public Task ValidateAsync()
        {
            throw new NotImplementedException();
        }

        /*
        private CancellationTokenSource cancelSource;

        private Task<T> value;
        private SemaphoreSlim semaphore = new SemaphoreSlim(1);

        public event Action Invalidated;

        public RoadComponentValue<T> Value => new RoadComponentValue<T>(GetValueInternal( ), cancelSource.Token);

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


        public void Invalidate()
        {
            value = null;
            cancelSource?.Cancel();
            Invalidated?.Invoke();
        }

        public Task ValidateAsync() => GetValueInternal();
        */
    }
}
