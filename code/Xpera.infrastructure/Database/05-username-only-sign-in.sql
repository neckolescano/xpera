/*  ONE-TIME fix for databases made before "sign in with only a username" (2026-11-18).
    Run it ONCE in SSMS on (localdb)\MSSQLLocalDB with Windows Authentication (not the per-store logins:
    this script reads all three store databases and writes to the Master), AFTER Update-Database has
    added the CompanyUsers table to Xpera_Master.

    1. The sample usernames were the same in every store (owner, cashier01, ...). Sign-in now has no store code,
       so a username must be unique in all of xpera. PixelZone keeps the plain names; Arcade Supply's become
       arcade.owner, arcade.cashier01, ...; Gaming Hub's become hub.owner, hub.cashier01, ...
       Only accounts that still have the sample password xpera123 are renamed (the password hash includes
       the username, so it is remade for the new name; the password stays xpera123).
    2. Every username in the three stores is listed in Xpera_Master.CompanyUsers with its store.
       A username found in two stores is NOT listed; the script prints it so you can rename one of them.
    Safe to run again: renamed users and listed usernames are skipped.  */

SET NOCOUNT ON;

-- 1. Unique sample usernames (IsSynced = 0: the changed accounts go to the cloud at the next backup).
UPDATE Xpera_TenantB.dbo.Users
SET Username = N'arcade.' + Username,
    PasswordHash = HASHBYTES('SHA2_256', N'arcade.' + Username + N':xpera123'),
    IsSynced = 0
WHERE Username IN (N'owner', N'cashier01', N'staff01', N'staff02')
  AND PasswordHash = HASHBYTES('SHA2_256', Username + N':xpera123');
PRINT CONCAT('Arcade Supply: ', @@ROWCOUNT, ' sample accounts renamed to arcade.*');

UPDATE Xpera_TenantC.dbo.Users
SET Username = N'hub.' + Username,
    PasswordHash = HASHBYTES('SHA2_256', N'hub.' + Username + N':xpera123'),
    IsSynced = 0
WHERE Username IN (N'owner', N'cashier01', N'staff01', N'staff02')
  AND PasswordHash = HASHBYTES('SHA2_256', Username + N':xpera123');
PRINT CONCAT('Gaming Hub: ', @@ROWCOUNT, ' sample accounts renamed to hub.*');

-- 2. Every store's usernames, with the store they belong to (from CompanyDatabases).
DECLARE @all TABLE (CompanyId INT, Username NVARCHAR(40));
INSERT INTO @all
SELECT d.CompanyId, u.Username FROM Xpera_TenantA.dbo.Users u
JOIN Xpera_Master.dbo.CompanyDatabases d ON d.DatabaseName = N'Xpera_TenantA' AND d.IsActive = 1
UNION ALL
SELECT d.CompanyId, u.Username FROM Xpera_TenantB.dbo.Users u
JOIN Xpera_Master.dbo.CompanyDatabases d ON d.DatabaseName = N'Xpera_TenantB' AND d.IsActive = 1
UNION ALL
SELECT d.CompanyId, u.Username FROM Xpera_TenantC.dbo.Users u
JOIN Xpera_Master.dbo.CompanyDatabases d ON d.DatabaseName = N'Xpera_TenantC' AND d.IsActive = 1;

-- Usernames used in more than one store: listed here for you to fix, and left out of CompanyUsers.
SELECT Username AS [Used in more than one store - rename one], COUNT(*) AS Stores
FROM @all GROUP BY Username HAVING COUNT(*) > 1;

INSERT INTO Xpera_Master.dbo.CompanyUsers (CompanyId, Username, CreatedAt)
SELECT a.CompanyId, a.Username, SYSUTCDATETIME()
FROM @all a
WHERE a.Username IN (SELECT Username FROM @all GROUP BY Username HAVING COUNT(*) = 1)
  AND NOT EXISTS (SELECT 1 FROM Xpera_Master.dbo.CompanyUsers c WHERE c.Username = a.Username);
PRINT CONCAT('Master: ', @@ROWCOUNT, ' usernames added to CompanyUsers');

-- What the Master knows now: each username and its store.
SELECT c.CompanyCode, c.CompanyName, u.Username
FROM Xpera_Master.dbo.CompanyUsers u
JOIN Xpera_Master.dbo.Companies c ON c.CompanyId = u.CompanyId
ORDER BY c.CompanyId, u.Username;
