/*  ONE-TIME, part 2 of 3.  Run on: Xpera_TenantC  (your registered server Xpera_TenantC is fine)

    Same as part 1, for Gaming Hub: its sample accounts get "hub." in front (hub.owner, hub.cashier01, ...).
    Only accounts still on the sample password xpera123 are renamed; the password stays xpera123.
    Safe to run again.  */

SET NOCOUNT ON;

UPDATE Users
SET Username = N'hub.' + Username,
    PasswordHash = HASHBYTES('SHA2_256', N'hub.' + Username + N':xpera123')
WHERE Username IN (N'owner', N'cashier01', N'staff01', N'staff02')
  AND PasswordHash = HASHBYTES('SHA2_256', Username + N':xpera123');
PRINT CONCAT('Renamed ', @@ROWCOUNT, ' sample accounts to hub.*');

IF COL_LENGTH('Users', 'IsSynced') IS NOT NULL
    EXEC (N'UPDATE Users SET IsSynced = 0 WHERE Username LIKE N''hub.%''');

-- Every account in this store now. Any name WITHOUT "hub." in front is one you added yourself:
-- write it down, it goes into part 3 (05c, on the Master).
SELECT Username, FullName, Role FROM Users ORDER BY Username;
