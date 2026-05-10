namespace School_Management.Core.Enums
{
    public enum FileStatus
    {
        LocalOnly,
        Uploaded,
        /// <summary>
        /// Sync failed repeatedly
        /// </summary>
        Unavailable,
        PendingUpload,
        PendingDelete,
    }
}
