using Microsoft.Extensions.DependencyInjection;
using SchoolManagement.Core.Infrastructure.Interfaces;
using System.Diagnostics;

namespace SchoolManagement.Application.Workers
{
    public class FileSyncBackgroundWorker
    {
        private readonly IServiceProvider _scopeFactory;

        public FileSyncBackgroundWorker(IServiceProvider scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        private CancellationTokenSource? _cts;
        private Task? _workerTask;

        public async Task Start()
        {
            _cts = new CancellationTokenSource();
            CancellationToken token = _cts.Token;

            _workerTask = Task.Run(() => ExecuteAsync(token));
        }

        public async Task Stop()
        {
            if (_cts == null || _workerTask == null)
                return;

            _cts.Cancel();

            try
            {
                await _workerTask;
            }
            catch (OperationCanceledException)
            {
            }
        }

        private async Task ExecuteAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();

                    IPhotoSyncService syncService = scope.ServiceProvider.GetRequiredService<IPhotoSyncService>();

                    await syncService.ProcessPendingUploads(token);
                    await syncService.ProcessPendingDeletes(token);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception)
                {
                    throw;
                }

                Debug.WriteLine("Ran photo sync service");
                await Task.Delay(TimeSpan.FromSeconds(60), token);
            }
        }
        
    }
}
