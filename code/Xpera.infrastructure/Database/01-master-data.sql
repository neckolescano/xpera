/*  Sample data for the Master database (Xpera_Master).
    The tables come from the EF Core migrations (MasterCrmDbContext); this script only fills them.
    Run it with setup.ps1, or open it in SSMS on Xpera_Master and press Execute.  */

-- Start clean, children before parents.
IF OBJECT_ID('Devices') IS NOT NULL DELETE FROM Devices;
IF OBJECT_ID('CompanyUsers') IS NOT NULL DELETE FROM CompanyUsers;
DELETE FROM Subscriptions;
DELETE FROM CompanyDatabases;
DELETE FROM Companies;
DELETE FROM Plans;
DELETE FROM AspNetUsers WHERE NormalizedUserName = N'SUPERADMIN';

-- Make the IDs start at 1 again. A table that never had rows already starts at 1 by itself;
-- RESEED 0 is only for a table that had rows before (on a brand-new table it would make the first ID 0).
DECLARE @t NVARCHAR(50);
DECLARE tables CURSOR LOCAL FOR
    SELECT name FROM (VALUES (N'Companies'), (N'CompanyDatabases'), (N'Subscriptions'), (N'Devices'), (N'CompanyUsers')) v(name)
    WHERE EXISTS (SELECT 1 FROM sys.identity_columns WHERE object_id = OBJECT_ID(v.name) AND last_value IS NOT NULL);
OPEN tables;
FETCH NEXT FROM tables INTO @t;
WHILE @@FETCH_STATUS = 0
BEGIN
    DBCC CHECKIDENT (@t, RESEED, 0) WITH NO_INFOMSGS;
    FETCH NEXT FROM tables INTO @t;
END
CLOSE tables;
DEALLOCATE tables;

DECLARE @today DATE = CAST(GETDATE() AS DATE);

-- The three plans. Each plan includes everything in the plan before it.
INSERT INTO Plans (PlanCode, PlanName, MonthlyFee, Includes) VALUES
    (N'A', N'Micro',  1500, N'Customers, sales, payments, inquiries, complaints, feedback'),
    (N'B', N'Small',  3000, N'Plan A + dashboard, reports, retention campaigns'),
    (N'C', N'Medium', 5000, N'Plan B + branches');

-- The stores (companies). People sign in by username only; the code is the store's short name.
INSERT INTO Companies (CompanyCode, CompanyName, IsActive, CreatedAt) VALUES
    (N'PIXEL01',  N'PixelZone',     1, DATEADD(MONTH, -5, @today)),
    (N'ARCADE01', N'Arcade Supply', 1, DATEADD(MONTH, -3, @today)),
    (N'HUB01',    N'Gaming Hub',    1, DATEADD(MONTH, -2, @today));

-- Each store has its own database (multi-tenant).
-- CredentialKey names the store's login in appsettings.json (TenantCredentials:TenantA:UserId / Password);
-- the password itself is never stored in this table.
INSERT INTO CompanyDatabases (CompanyId, ServerName, DatabaseName, IsActive, CredentialKey)
SELECT CompanyId, N'(localdb)\MSSQLLocalDB',
       CASE CompanyCode WHEN N'PIXEL01' THEN N'Xpera_TenantA' WHEN N'ARCADE01' THEN N'Xpera_TenantB' ELSE N'Xpera_TenantC' END, 1,
       CASE CompanyCode WHEN N'PIXEL01' THEN N'TenantA' WHEN N'ARCADE01' THEN N'TenantB' ELSE N'TenantC' END
FROM Companies;

-- Each store's plan: PixelZone = Tenant A, Arcade Supply = Tenant B, Gaming Hub = Tenant C.
INSERT INTO Subscriptions (CompanyId, PlanCode, MonthlyFee, StartedOn, RenewsOn, IsActive)
SELECT c.CompanyId, p.PlanCode, p.MonthlyFee, CAST(c.CreatedAt AS DATE), DATEADD(MONTH, 1, DATEADD(DAY, c.CompanyId * 6, @today)), 1
FROM Companies c
JOIN Plans p ON p.PlanCode = CASE c.CompanyCode WHEN N'PIXEL01' THEN N'A' WHEN N'ARCADE01' THEN N'B' ELSE N'C' END;

-- Which store each username belongs to, so sign-in needs no store code (the same names as 03-store-sample-data.sql).
-- Passwords are NOT here; they stay in each store's own Users table.
IF OBJECT_ID('CompanyUsers') IS NOT NULL
    INSERT INTO CompanyUsers (CompanyId, Username, CreatedAt)
    SELECT c.CompanyId,
           CASE c.CompanyCode WHEN N'ARCADE01' THEN N'arcade.' WHEN N'HUB01' THEN N'hub.' ELSE N'' END + v.Username,
           c.CreatedAt
    FROM Companies c
    CROSS JOIN (VALUES (N'owner'), (N'cashier01'), (N'staff01'), (N'staff02')) v(Username);

-- The Super Admin, as an Identity user (AspNetUsers). Sample sign-in for testing only: superadmin / xpera123.
-- PasswordHash is Identity's own format (made by PasswordHasher), never the password itself.
INSERT INTO AspNetUsers (Id, UserName, NormalizedUserName, Email, NormalizedEmail, EmailConfirmed, PasswordHash,
                         SecurityStamp, ConcurrencyStamp, PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnabled, AccessFailedCount)
VALUES (N'd1109054-653b-47b0-be93-bc8b6055c3da', N'superadmin', N'SUPERADMIN', NULL, NULL, 0,
        N'AQAAAAIAAYagAAAAEMg8b32B+sisruRzdF16I2F4j9HHRk2fSdvadHsf4/Mse3AhAWwRU5zE0HMX2Oummw==',
        N'235b9121-deac-499c-b5ce-41a2abbc017a', N'6083c438-5315-4b24-8fbe-74a321ce8458', 0, 0, 1, 0);
GO
