namespace SchoolManagement.Core.Features.Files.Contracts
{
    public interface IPhotoSyncService
    {
        Task ProcessPendingUploads(CancellationToken token);
        Task ProcessPendingDeletes(CancellationToken token);
    }
}

