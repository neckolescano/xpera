/*  Health check. Changes NOTHING, only reads.
    Run it on each registered server: Xpera_Master, Xpera_TenantA, Xpera_TenantB, Xpera_TenantC
    (and the Cloud_ ones if you like). It sees which kind of database it is on and checks that one.
    Each line says OK or what to fix.  */

SET NOCOUNT ON;

DECLARE @r TABLE (Area NVARCHAR(40), [Check] NVARCHAR(120), Result NVARCHAR(200));

IF OBJECT_ID('Companies') IS NOT NULL
BEGIN
    -- ---------- the Master ----------
    INSERT INTO @r SELECT 'Master', 'Newest migration',
        ISNULL((SELECT MAX(MigrationId) FROM __EFMigrationsHistory), 'none')
        + CASE WHEN EXISTS (SELECT 1 FROM __EFMigrationsHistory WHERE MigrationId LIKE '%AddCompanyUsers') THEN '  OK' ELSE '  -> run Update-Database (Master)' END;

    INSERT INTO @r SELECT 'Master', 'Super Admin account',
        CASE WHEN EXISTS (SELECT 1 FROM AspNetUsers WHERE NormalizedUserName = 'SUPERADMIN') THEN 'superadmin  OK' ELSE 'missing -> run 01-master-data.sql' END;

    -- Every store needs: an active database row with a login key, an active plan, and usernames.
    INSERT INTO @r
    SELECT 'Store ' + c.CompanyCode, c.CompanyName,
        CONCAT(ISNULL(d.DatabaseName, 'NO DATABASE'), ' / key ', ISNULL(NULLIF(d.CredentialKey, ''), 'NONE'),
               ' / plan ', ISNULL(s.PlanCode, 'NONE'),
               ' / ', (SELECT COUNT(*) FROM CompanyUsers u WHERE u.CompanyId = c.CompanyId), ' usernames',
               CASE WHEN d.DatabaseName IS NULL OR ISNULL(d.CredentialKey, '') = '' OR s.PlanCode IS NULL
                         OR NOT EXISTS (SELECT 1 FROM CompanyUsers u WHERE u.CompanyId = c.CompanyId)
                    THEN '  -> fix' ELSE '  OK' END)
    FROM Companies c
    OUTER APPLY (SELECT TOP 1 * FROM CompanyDatabases d WHERE d.CompanyId = c.CompanyId AND d.IsActive = 1) d
    OUTER APPLY (SELECT TOP 1 * FROM Subscriptions s WHERE s.CompanyId = c.CompanyId AND s.IsActive = 1 ORDER BY s.StartedOn DESC) s;
END
ELSE IF OBJECT_ID('Users') IS NOT NULL
BEGIN
    -- ---------- a store database ----------
    DECLARE @store NVARCHAR(40) = DB_NAME();

    INSERT INTO @r SELECT @store, 'Newest migration',
        ISNULL((SELECT MAX(MigrationId) FROM __EFMigrationsHistory), 'none')
        + CASE WHEN EXISTS (SELECT 1 FROM __EFMigrationsHistory WHERE MigrationId LIKE '%AddSyncTracking') THEN '  OK' ELSE '  -> run Update-Database for this store' END;

    INSERT INTO @r SELECT @store, 'Transactions (sales + payments, need 200+)',
        CONCAT((SELECT COUNT(*) FROM Sales), ' sales + ', (SELECT COUNT(*) FROM Payments), ' payments',
               CASE WHEN (SELECT COUNT(*) FROM Sales) + (SELECT COUNT(*) FROM Payments) >= 200 THEN '  OK' ELSE '  -> run 03/04 seed scripts' END);

    INSERT INTO @r SELECT @store, 'Rows per table',
        CONCAT((SELECT COUNT(*) FROM Customers), ' customers, ', (SELECT COUNT(*) FROM Products), ' products, ',
               (SELECT COUNT(*) FROM Inquiries), ' inquiries, ', (SELECT COUNT(*) FROM Complaints), ' complaints, ',
               (SELECT COUNT(*) FROM Feedback), ' feedback, ', (SELECT COUNT(*) FROM Users), ' users');

    INSERT INTO @r SELECT @store, 'Usernames', STRING_AGG(Username, ', ') WITHIN GROUP (ORDER BY Username) FROM Users;

    INSERT INTO @r SELECT @store, 'Accounts locked', CONCAT(COUNT(*), CASE WHEN COUNT(*) = 0 THEN '  OK' ELSE '  (unlock if needed)' END)
    FROM Users WHERE Status = 'Locked';

    IF COL_LENGTH('Customers', 'IsSynced') IS NOT NULL
        INSERT INTO @r EXEC (N'
            SELECT DB_NAME(), ''Not yet in the cloud'',
                CONCAT((SELECT COUNT(*) FROM Customers WHERE IsSynced = 0) + (SELECT COUNT(*) FROM Sales WHERE IsSynced = 0)
                     + (SELECT COUNT(*) FROM Payments WHERE IsSynced = 0) + (SELECT COUNT(*) FROM Users WHERE IsSynced = 0)
                     + (SELECT COUNT(*) FROM Products WHERE IsSynced = 0) + (SELECT COUNT(*) FROM Inquiries WHERE IsSynced = 0)
                     + (SELECT COUNT(*) FROM Complaints WHERE IsSynced = 0) + (SELECT COUNT(*) FROM Feedback WHERE IsSynced = 0),
                     '' records (0 right after a backup)'')');

    -- Mobile numbers: the app saves them as 0917 555 0142 (11 digits starting with 09).
    INSERT INTO @r
    SELECT @store, 'Customer with a wrong mobile number', CONCAT('#', CustomerId, ' ', FullName, ': "', Contact, '"  -> fix')
    FROM Customers
    WHERE REPLACE(Contact, ' ', '') NOT LIKE '09[0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9]'
       OR Contact NOT LIKE '09[0-9][0-9] [0-9][0-9][0-9] [0-9][0-9][0-9][0-9]';

    INSERT INTO @r
    SELECT @store, 'Two customers with the same mobile', CONCAT(REPLACE(Contact, ' ', ''), ' used ', COUNT(*), ' times  -> fix')
    FROM Customers GROUP BY REPLACE(Contact, ' ', '') HAVING COUNT(*) > 1;
END
ELSE
    INSERT INTO @r VALUES ('?', 'Database', DB_NAME() + ' has no xpera tables');

SELECT * FROM @r;
