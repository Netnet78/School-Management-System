using System;
using System.Collections.Generic;
using System.Text;

namespace SchoolManagement.Core.Infrastructure.Interfaces
{
    public interface IPhotoSyncService
    {
        Task ProcessPendingUploads(CancellationToken token);
        Task ProcessPendingDeletes(CancellationToken token);
    }
}
