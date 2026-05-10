using System;
using System.Collections.Generic;
using System.Text;

namespace School_Management.Core.Interfaces.Infrastructure
{
    public interface IPhotoSyncService
    {
        Task ProcessPendingUploads(CancellationToken token);
        Task ProcessPendingDeletes(CancellationToken token);
    }
}
