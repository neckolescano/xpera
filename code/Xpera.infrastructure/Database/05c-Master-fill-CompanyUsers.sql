/*  ONE-TIME, part 3 of 3.  Run on: Xpera_Master  (your registered server Xpera_Master is fine)
    Run it AFTER Update-Database added the CompanyUsers table, and after parts 1 and 2.

    CompanyUsers is the Master's "phone book": which store each username belongs to.
    At sign-in:  username → CompanyUsers.CompanyId → CompanyDatabases (which database, which server, which login)
                 → the app opens that store's database and checks the password there.

    This login can only see the Master, not the store databases, so it can't read their Users tables.
    That's why the usernames are listed here by hand: the 12 sample accounts, plus any you added yourself
    (parts 1 and 2 showed them; for PixelZone run  SELECT Username FROM Users  on Xpera_TenantA).
    Safe to run again: a username already in the list is skipped.  */

SET NOCOUNT ON;

DECLARE @list TABLE (CompanyCode NVARCHAR(50), Username NVARCHAR(40));
INSERT INTO @list (CompanyCode, Username) VALUES
    -- PixelZone (Tenant A)
    (N'PIXEL01',  N'owner'),
    (N'PIXEL01',  N'cashier01'),
    (N'PIXEL01',  N'staff01'),
    (N'PIXEL01',  N'staff02'),
    -- Arcade Supply (Tenant B)
    (N'ARCADE01', N'arcade.owner'),
    (N'ARCADE01', N'arcade.cashier01'),
    (N'ARCADE01', N'arcade.staff01'),
    (N'ARCADE01', N'arcade.staff02'),
    -- Gaming Hub (Tenant C)
    (N'HUB01',    N'hub.owner'),
    (N'HUB01',    N'hub.cashier01'),
    (N'HUB01',    N'hub.staff01'),
    (N'HUB01',    N'hub.staff02');

-- Accounts you added yourself: one line each, store code + username exactly as it is in that store.
-- Remove the -- in front to use it. Example:
-- INSERT INTO @list (CompanyCode, Username) VALUES (N'PIXEL01', N'cashier02');

INSERT INTO CompanyUsers (CompanyId, Username, CreatedAt)
SELECT c.CompanyId, LOWER(l.Username), SYSUTCDATETIME()
FROM @list l
JOIN Companies c ON c.CompanyCode = l.CompanyCode
WHERE NOT EXISTS (SELECT 1 FROM CompanyUsers u WHERE u.Username = LOWER(l.Username));
PRINT CONCAT('Added ', @@ROWCOUNT, ' usernames to CompanyUsers');

-- The phone book now: each username, its store, and the database sign-in will open for it.
SELECT u.Username, c.CompanyCode, c.CompanyName, d.DatabaseName, d.CredentialKey
FROM CompanyUsers u
JOIN Companies c ON c.CompanyId = u.CompanyId
LEFT JOIN CompanyDatabases d ON d.CompanyId = c.CompanyId AND d.IsActive = 1
ORDER BY c.CompanyId, u.Username;
