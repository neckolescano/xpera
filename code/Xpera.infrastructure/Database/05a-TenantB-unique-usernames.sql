/*  ONE-TIME, part 1 of 3.  Run on: Xpera_TenantB  (your registered server Xpera_TenantB is fine)

    Sign-in has no store code anymore, so a username must be unique in ALL of xpera.
    PixelZone (TenantA) keeps owner, cashier01, staff01, staff02.
    Arcade Supply (this database) gets "arcade." in front: arcade.owner, arcade.cashier01, ...

    Only sample accounts that still have the password xpera123 are renamed. The password hash is made from
    "username:password", so it is remade for the new name; the password itself stays xpera123.
    Safe to run again: renamed accounts are skipped.  */

SET NOCOUNT ON;

UPDATE Users
SET Username = N'arcade.' + Username,
    PasswordHash = HASHBYTES('SHA2_256', N'arcade.' + Username + N':xpera123')
WHERE Username IN (N'owner', N'cashier01', N'staff01', N'staff02')
  AND PasswordHash = HASHBYTES('SHA2_256', Username + N':xpera123');
PRINT CONCAT('Renamed ', @@ROWCOUNT, ' sample accounts to arcade.*');

-- The changed accounts go to the cloud at the next backup.
IF COL_LENGTH('Users', 'IsSynced') IS NOT NULL
    EXEC (N'UPDATE Users SET IsSynced = 0 WHERE Username LIKE N''arcade.%''');

-- Every account in this store now. Any name WITHOUT "arcade." in front is one you added yourself:
-- write it down, it goes into part 3 (05c, on the Master).
SELECT Username, FullName, Role FROM Users ORDER BY Username;
