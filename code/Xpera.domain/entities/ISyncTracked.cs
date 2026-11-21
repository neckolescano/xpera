namespace Xpera.domain.entities
{
    // A table whose rows are backed up to the cloud one by one and can say if they made it there.
    // Only tables with records of their own have it: SaleItems doesn't, because an item is part of its sale
    // (saved together, backed up together), so the Sale's flag already says whether the whole sale is in the cloud.
    //
    // IsSynced = false: added or changed on this PC since the last cloud backup.
    // IsSynced = true:  the cloud has this exact version.
    public interface ISyncTracked
    {
        bool IsSynced { get; set; }
    }
}
