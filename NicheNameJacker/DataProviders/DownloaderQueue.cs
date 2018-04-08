using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NicheNameJacker.DataProviders
{
    class DownloaderQueue
    {
        private readonly CancellationToken _token;

        private int _count;

        public ConcurrentQueue<Action> Actions { get; private set; }

        public DownloaderQueue(CancellationToken token)
        {
            _token = token;
            Actions = new ConcurrentQueue<Action>();
            Task.Run(() =>
            {
                while (!_token.IsCancellationRequested)
                {
                    List<Action> actions = new List<Action>();
                    Action act = null;
                    while (actions.Count < 20 && Actions.TryDequeue(out act))
                    {
                        actions.Add(act);
                    }

                    _count += actions.Count;
                    actions.ForEach(action => action());
                    Console.WriteLine(@"Requests performed so far: {0}", _count);

                    Task.Delay(TimeSpan.FromSeconds(1.5)).Wait();
                }
            }, _token);
        }

        public async Task<string> Enqueu(Task<string> task)
        {
            TaskCompletionSource<string> source = new TaskCompletionSource<string>();
            Actions.Enqueue(async () =>
            {
                var result = await task;
                source.TrySetResult(result);
            });

            return await source.Task;
        }
    }
}
