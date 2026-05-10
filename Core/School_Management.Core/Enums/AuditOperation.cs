namespace School_Management.Core.Enums
{
    [Flags]
    public enum AuditOperation
    {
        None = 0,
        Insert = 1,
        Update = 2,
        Delete = 4,

        All = Insert | Update | Delete
    }
}
