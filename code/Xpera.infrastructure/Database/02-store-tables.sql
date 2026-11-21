IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261111094514_InitialTenantCrm'
)
BEGIN
    CREATE TABLE [Customers] (
        [CustomerId] int NOT NULL IDENTITY,
        [FullName] nvarchar(80) NOT NULL,
        [Contact] nvarchar(30) NOT NULL,
        [UsuallyBuys] nvarchar(80) NOT NULL DEFAULT N'',
        [CreatedOn] date NOT NULL DEFAULT (CAST(GETDATE() AS DATE)),
        CONSTRAINT [PK_Customers] PRIMARY KEY ([CustomerId])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261111094514_InitialTenantCrm'
)
BEGIN
    CREATE TABLE [Products] (
        [ProductId] int NOT NULL IDENTITY,
        [Name] nvarchar(80) NOT NULL,
        [Category] nvarchar(40) NOT NULL,
        [Price] decimal(10,2) NOT NULL,
        [IsAvailable] bit NOT NULL DEFAULT CAST(1 AS bit),
        CONSTRAINT [PK_Products] PRIMARY KEY ([ProductId]),
        CONSTRAINT [CK_Products_Price] CHECK (Price >= 0)
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261111094514_InitialTenantCrm'
)
BEGIN
    CREATE TABLE [Users] (
        [UserId] int NOT NULL IDENTITY,
        [FullName] nvarchar(80) NOT NULL,
        [Username] nvarchar(40) NOT NULL,
        [Role] nvarchar(20) NOT NULL,
        [Status] nvarchar(20) NOT NULL DEFAULT N'Active',
        [FailedLogins] int NOT NULL DEFAULT 0,
        [PasswordHash] varbinary(32) NOT NULL,
        CONSTRAINT [PK_Users] PRIMARY KEY ([UserId]),
        CONSTRAINT [CK_Users_Role] CHECK (Role IN (N'Admin', N'Cashier', N'Staff')),
        CONSTRAINT [CK_Users_Status] CHECK (Status IN (N'Active', N'Locked'))
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261111094514_InitialTenantCrm'
)
BEGIN
    CREATE TABLE [Complaints] (
        [ComplaintId] int NOT NULL IDENTITY,
        [ComplaintDate] date NOT NULL DEFAULT (CAST(GETDATE() AS DATE)),
        [CustomerId] int NOT NULL,
        [Details] nvarchar(200) NOT NULL,
        [Type] nvarchar(20) NOT NULL,
        [Status] nvarchar(20) NOT NULL DEFAULT N'Open',
        CONSTRAINT [PK_Complaints] PRIMARY KEY ([ComplaintId]),
        CONSTRAINT [CK_Complaints_Status] CHECK (Status IN (N'Open', N'In progress', N'Resolved')),
        CONSTRAINT [CK_Complaints_Type] CHECK (Type IN (N'Complaint', N'Warranty claim')),
        CONSTRAINT [FK_Complaints_Customers_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [Customers] ([CustomerId]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261111094514_InitialTenantCrm'
)
BEGIN
    CREATE TABLE [Feedback] (
        [FeedbackId] int NOT NULL IDENTITY,
        [FeedbackDate] date NOT NULL DEFAULT (CAST(GETDATE() AS DATE)),
        [CustomerId] int NOT NULL,
        [Rating] int NOT NULL,
        [Comment] nvarchar(300) NOT NULL DEFAULT N'',
        CONSTRAINT [PK_Feedback] PRIMARY KEY ([FeedbackId]),
        CONSTRAINT [CK_Feedback_Rating] CHECK (Rating BETWEEN 1 AND 5),
        CONSTRAINT [FK_Feedback_Customers_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [Customers] ([CustomerId]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261111094514_InitialTenantCrm'
)
BEGIN
    CREATE TABLE [Inquiries] (
        [InquiryId] int NOT NULL IDENTITY,
        [InquiryDate] date NOT NULL DEFAULT (CAST(GETDATE() AS DATE)),
        [CustomerId] int NOT NULL,
        [Question] nvarchar(200) NOT NULL,
        [ProductId] int NULL,
        [Status] nvarchar(30) NOT NULL DEFAULT N'Open',
        CONSTRAINT [PK_Inquiries] PRIMARY KEY ([InquiryId]),
        CONSTRAINT [CK_Inquiries_Status] CHECK (Status IN (N'Open', N'In progress', N'Waiting for product', N'Resolved')),
        CONSTRAINT [FK_Inquiries_Customers_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [Customers] ([CustomerId]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Inquiries_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([ProductId]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261111094514_InitialTenantCrm'
)
BEGIN
    CREATE TABLE [Payments] (
        [PaymentId] int NOT NULL IDENTITY,
        [CustomerId] int NOT NULL,
        [Amount] decimal(10,2) NOT NULL,
        [PaidOn] date NOT NULL DEFAULT (CAST(GETDATE() AS DATE)),
        [CashierId] int NULL,
        CONSTRAINT [PK_Payments] PRIMARY KEY ([PaymentId]),
        CONSTRAINT [CK_Payments_Amount] CHECK (Amount > 0),
        CONSTRAINT [FK_Payments_Customers_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [Customers] ([CustomerId]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Payments_Users_CashierId] FOREIGN KEY ([CashierId]) REFERENCES [Users] ([UserId]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261111094514_InitialTenantCrm'
)
BEGIN
    CREATE TABLE [Sales] (
        [SaleId] int NOT NULL IDENTITY,
        [ReceiptNo] nvarchar(20) NOT NULL,
        [SaleDate] date NOT NULL DEFAULT (CAST(GETDATE() AS DATE)),
        [CustomerId] int NOT NULL,
        [CashierId] int NULL,
        [PaymentType] nvarchar(10) NOT NULL,
        [Total] decimal(10,2) NOT NULL,
        [DueDate] date NULL,
        CONSTRAINT [PK_Sales] PRIMARY KEY ([SaleId]),
        CONSTRAINT [CK_Sales_PaymentType] CHECK (PaymentType IN (N'Cash', N'Credit')),
        CONSTRAINT [CK_Sales_Total] CHECK (Total >= 0),
        CONSTRAINT [FK_Sales_Customers_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [Customers] ([CustomerId]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Sales_Users_CashierId] FOREIGN KEY ([CashierId]) REFERENCES [Users] ([UserId]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261111094514_InitialTenantCrm'
)
BEGIN
    CREATE TABLE [SaleItems] (
        [SaleItemId] int NOT NULL IDENTITY,
        [SaleId] int NOT NULL,
        [ProductId] int NOT NULL,
        [Quantity] int NOT NULL,
        [UnitPrice] decimal(10,2) NOT NULL,
        CONSTRAINT [PK_SaleItems] PRIMARY KEY ([SaleItemId]),
        CONSTRAINT [CK_SaleItems_Quantity] CHECK (Quantity > 0),
        CONSTRAINT [FK_SaleItems_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([ProductId]) ON DELETE NO ACTION,
        CONSTRAINT [FK_SaleItems_Sales_SaleId] FOREIGN KEY ([SaleId]) REFERENCES [Sales] ([SaleId]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261111094514_InitialTenantCrm'
)
BEGIN
    CREATE INDEX [IX_Complaints_CustomerId] ON [Complaints] ([CustomerId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261111094514_InitialTenantCrm'
)
BEGIN
    CREATE INDEX [IX_Feedback_CustomerId] ON [Feedback] ([CustomerId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261111094514_InitialTenantCrm'
)
BEGIN
    CREATE INDEX [IX_Inquiries_CustomerId] ON [Inquiries] ([CustomerId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261111094514_InitialTenantCrm'
)
BEGIN
    CREATE INDEX [IX_Inquiries_ProductId] ON [Inquiries] ([ProductId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261111094514_InitialTenantCrm'
)
BEGIN
    CREATE INDEX [IX_Payments_CashierId] ON [Payments] ([CashierId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261111094514_InitialTenantCrm'
)
BEGIN
    CREATE INDEX [IX_Payments_CustomerId] ON [Payments] ([CustomerId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261111094514_InitialTenantCrm'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Products_Name] ON [Products] ([Name]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261111094514_InitialTenantCrm'
)
BEGIN
    CREATE INDEX [IX_SaleItems_ProductId] ON [SaleItems] ([ProductId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261111094514_InitialTenantCrm'
)
BEGIN
    CREATE INDEX [IX_SaleItems_SaleId] ON [SaleItems] ([SaleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261111094514_InitialTenantCrm'
)
BEGIN
    CREATE INDEX [IX_Sales_CashierId] ON [Sales] ([CashierId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261111094514_InitialTenantCrm'
)
BEGIN
    CREATE INDEX [IX_Sales_CustomerId] ON [Sales] ([CustomerId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261111094514_InitialTenantCrm'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Sales_ReceiptNo] ON [Sales] ([ReceiptNo]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261111094514_InitialTenantCrm'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Users_Username] ON [Users] ([Username]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261111094514_InitialTenantCrm'
)
BEGIN
    EXEC(N'CREATE VIEW CustomerBalances AS
    WITH credit AS (
        SELECT CustomerId, DueDate, Total,
               SUM(Total) OVER (PARTITION BY CustomerId ORDER BY SaleDate, SaleId) AS RunningTotal
        FROM Sales WHERE PaymentType = N''Credit''
    ), paid AS (
        SELECT CustomerId, SUM(Amount) AS Total FROM Payments GROUP BY CustomerId
    )
    SELECT c.CustomerId,
           c.FullName,
           ISNULL((SELECT SUM(Total) FROM credit WHERE credit.CustomerId = c.CustomerId), 0) - ISNULL(paid.Total, 0) AS Balance,
           (SELECT MIN(DueDate) FROM credit
             WHERE credit.CustomerId = c.CustomerId AND credit.RunningTotal > ISNULL(paid.Total, 0)) AS DueDate
    FROM Customers c
    LEFT JOIN paid ON paid.CustomerId = c.CustomerId;')
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261111094514_InitialTenantCrm'
)
BEGIN
    EXEC(N'CREATE VIEW CustomerSummary AS
    SELECT c.CustomerId, c.FullName, c.Contact, c.UsuallyBuys,
           ISNULL(SUM(s.Total), 0) AS Spent,
           MAX(s.SaleDate) AS LastVisit
    FROM Customers c
    LEFT JOIN Sales s ON s.CustomerId = c.CustomerId
    GROUP BY c.CustomerId, c.FullName, c.Contact, c.UsuallyBuys;')
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261111094514_InitialTenantCrm'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261111094514_InitialTenantCrm', N'10.0.14');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261117124355_AddSyncTracking'
)
BEGIN
    ALTER TABLE [Users] ADD [IsSynced] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261117124355_AddSyncTracking'
)
BEGIN
    ALTER TABLE [Sales] ADD [IsSynced] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261117124355_AddSyncTracking'
)
BEGIN
    ALTER TABLE [Products] ADD [IsSynced] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261117124355_AddSyncTracking'
)
BEGIN
    ALTER TABLE [Payments] ADD [IsSynced] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261117124355_AddSyncTracking'
)
BEGIN
    ALTER TABLE [Inquiries] ADD [IsSynced] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261117124355_AddSyncTracking'
)
BEGIN
    ALTER TABLE [Feedback] ADD [IsSynced] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261117124355_AddSyncTracking'
)
BEGIN
    ALTER TABLE [Customers] ADD [IsSynced] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261117124355_AddSyncTracking'
)
BEGIN
    ALTER TABLE [Complaints] ADD [IsSynced] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261117124355_AddSyncTracking'
)
BEGIN
    CREATE TABLE [ActivityLogs] (
        [ActivityLogId] int NOT NULL IDENTITY,
        [LoggedAt] datetime2(0) NOT NULL DEFAULT (SYSDATETIME()),
        [UserId] int NULL,
        [Action] nvarchar(200) NOT NULL,
        [TableName] nvarchar(40) NULL,
        [RecordId] int NULL,
        CONSTRAINT [PK_ActivityLogs] PRIMARY KEY ([ActivityLogId]),
        CONSTRAINT [FK_ActivityLogs_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([UserId]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261117124355_AddSyncTracking'
)
BEGIN
    CREATE INDEX [IX_ActivityLogs_TableName_RecordId] ON [ActivityLogs] ([TableName], [RecordId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261117124355_AddSyncTracking'
)
BEGIN
    CREATE INDEX [IX_ActivityLogs_UserId] ON [ActivityLogs] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261117124355_AddSyncTracking'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261117124355_AddSyncTracking', N'10.0.14');
END;

COMMIT;
GO

