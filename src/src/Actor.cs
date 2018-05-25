using System;
using System.Threading;
using System.Threading.Tasks;

namespace Kelson.Async
{   
    public abstract class Actor<T> : IDisposable
    {
        private readonly T asset;

        private readonly CancellationTokenSource cancelTokenSource;
        private readonly Task owner;

        public bool IsClosed { get; private set; }

        protected Actor(T value)
        {
            cancelTokenSource = new CancellationTokenSource();
            owner = Task.Run(() => { }, cancelTokenSource.Token);
            asset = value;
        }

        protected async Task EnqueueAction(Action<T> action)
        {
            await owner.ContinueWith(t =>
            {
                if (!IsClosed || cancelTokenSource.IsCancellationRequested)
                    action(asset);
            });
        }

        protected void Cancel()
        {
            cancelTokenSource.Cancel();
        }

        public abstract void Dispose();

        protected async Task WithDisposedAssetAsync(Action<T> action)
        {
            IsClosed = true;
            await owner;
            action(asset);
        }
    }
}
