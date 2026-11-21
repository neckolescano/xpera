/*  200 more sales for one store database, spread over the last 11 months, plus the payments for them.
    With the 10 sales and 3 payments from 03-store-sample-data.sql, each store has 260+ transactions,
    enough history for the Lab 2 dashboard graphs (monthly sales, new vs returning, top customers).
    The numbers look random but are worked out from each sale's number, so a run always gives the same data.
    Run after 03-store-sample-data.sql.  */

DECLARE @today DATE = CAST(GETDATE() AS DATE);
DECLARE @cashier INT = (SELECT TOP 1 UserId FROM Users WHERE Role = N'Cashier' ORDER BY UserId);   -- cashier01 / arcade.cashier01 / hub.cashier01
DECLARE @seed INT = ABS(CHECKSUM(DB_NAME())) % 50;     -- each store gets slightly different sales

-- 32 more customers. Their first sale sets their "customer since" date further down.
INSERT INTO Customers (FullName, Contact, UsuallyBuys, CreatedOn) VALUES
    (N'Carlo Bautista',   N'0917 555 0201', N'GPUs',          @today), (N'Mika Flores',     N'0918 555 0202', N'Headsets',      @today),
    (N'Jerome Aquino',    N'0919 555 0203', N'Full builds',   @today), (N'Trish Navarro',   N'0927 555 0204', N'Mice, pads',    @today),
    (N'Enzo Castillo',    N'0995 555 0205', N'Keyboards',     @today), (N'Lara Villareal',  N'0915 555 0206', N'Monitors',      @today),
    (N'Kevin Ocampo',     N'0917 555 0207', N'RAM, storage',  @today), (N'Janine Robles',   N'0918 555 0208', N'Peripherals',   @today),
    (N'Migs Dela Cruz',   N'0919 555 0209', N'Processors',    @today), (N'Andrea Pascual',  N'0927 555 0210', N'Headsets',      @today),
    (N'Rafael Gomez',     N'0995 555 0211', N'GPUs, cooling', @today), (N'Sofia Lim',       N'0915 555 0212', N'Keyboards',     @today),
    (N'Gab Mercado',      N'0917 555 0213', N'Mice, pads',    @today), (N'Hannah Uy',       N'0918 555 0214', N'Monitors',      @today),
    (N'Ivan Salazar',     N'0919 555 0215', N'Full builds',   @today), (N'Patrice Yu',      N'0927 555 0216', N'Peripherals',   @today),
    (N'Dominic Rivera',   N'0995 555 0217', N'RAM, storage',  @today), (N'Camille Reyes',   N'0915 555 0218', N'Headsets',      @today),
    (N'Luis Fernandez',   N'0917 555 0219', N'Processors',    @today), (N'Bianca Soriano',  N'0918 555 0220', N'Mice, pads',    @today),
    (N'Nathan Co',        N'0919 555 0221', N'GPUs',          @today), (N'Erika Javier',    N'0927 555 0222', N'Keyboards',     @today),
    (N'Joshua Manalo',    N'0995 555 0223', N'Monitors',      @today), (N'Kristine Ong',    N'0915 555 0224', N'Peripherals',   @today),
    (N'Arvin Padilla',    N'0917 555 0225', N'Full builds',   @today), (N'Denise Chua',     N'0918 555 0226', N'Headsets',      @today),
    (N'Renz Morales',     N'0919 555 0227', N'RAM, storage',  @today), (N'Faith Tolentino', N'0927 555 0228', N'Mice, pads',    @today),
    (N'Bryan Sy',         N'0995 555 0229', N'GPUs, cooling', @today), (N'Ella Marquez',    N'0915 555 0230', N'Keyboards',     @today),
    (N'Vince Galang',     N'0917 555 0231', N'Processors',    @today), (N'Jessa Lopez',     N'0918 555 0232', N'Monitors',      @today);

-- Every customer gets a number k: 1 to 8 are the regulars from the sample data, 9 to 40 are the new ones.
DECLARE @customers TABLE (k INT PRIMARY KEY, CustomerId INT);
INSERT INTO @customers SELECT ROW_NUMBER() OVER (ORDER BY CustomerId), CustomerId FROM Customers;
DECLARE @newCount INT = (SELECT COUNT(*) FROM @customers) - 8;

-- How often each product sells, out of 20: small peripherals sell most, graphics cards least.
DECLARE @pick TABLE (Lo INT, Hi INT, Product NVARCHAR(80));
INSERT INTO @pick VALUES
    (0, 6, N'Gaming mouse'), (7, 10, N'Mechanical keyboard'), (11, 13, N'Wireless headset'), (14, 15, N'27" 165Hz monitor'),
    (16, 16, N'32GB DDR5 RAM'), (17, 17, N'Ryzen 7 7800X3D'), (18, 18, N'RTX 4070 Super 12GB'), (19, 19, N'RTX 5070 Ti 16GB');

-- Sale numbers 1 to 200. Sale 1 is about 330 days ago and sale 200 is a few days ago,
-- with more sales in recent months, like a store that is growing.
DECLARE @n TABLE (n INT PRIMARY KEY, DaysAgo INT, k INT, PaymentType NVARCHAR(10));
INSERT INTO @n
SELECT n,
       4 + CAST(326 * (1 - SQRT((n - 1) / 200.0)) AS INT),
       -- almost half of the sales go to the 8 regulars, the rest to the newer customers
       CASE WHEN (n * 37 + @seed) % 100 < 45 THEN (n * 5 + @seed) % 8 + 1 ELSE (n * 13 + @seed) % @newCount + 9 END,
       CASE WHEN (n + @seed) % 6 = 0 THEN N'Credit' ELSE N'Cash' END
FROM (SELECT TOP 200 ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS n FROM sys.all_objects) numbers;

-- A temporary receipt number; the real ones are given out in date order at the end.
INSERT INTO Sales (ReceiptNo, SaleDate, CustomerId, CashierId, PaymentType, Total, DueDate)
SELECT N'TMP-' + CAST(x.n AS NVARCHAR(10)), DATEADD(DAY, -x.DaysAgo, @today), c.CustomerId, @cashier, x.PaymentType, 0,
       CASE WHEN x.PaymentType = N'Credit' THEN DATEADD(DAY, 30 - x.DaysAgo, @today) END
FROM @n x JOIN @customers c ON c.k = x.k;

-- 1 to 3 items per sale. Now and then someone buys 2 mice.
INSERT INTO SaleItems (SaleId, ProductId, Quantity, UnitPrice)
SELECT sa.SaleId, p.ProductId,
       CASE WHEN pk.Product = N'Gaming mouse' AND x.n % 5 = 0 THEN 2 ELSE 1 END,
       p.Price
FROM @n x
JOIN (VALUES (1), (2), (3)) slot(j) ON slot.j = 1 OR (slot.j = 2 AND x.n % 3 = 0) OR (slot.j = 3 AND x.n % 7 = 0)
JOIN @pick pk ON (x.n * 31 + slot.j * 17 + @seed) % 20 BETWEEN pk.Lo AND pk.Hi
JOIN Products p ON p.Name = pk.Product
JOIN Sales sa ON sa.ReceiptNo = N'TMP-' + CAST(x.n AS NVARCHAR(10));

UPDATE sa SET Total = t.Total
FROM Sales sa JOIN (SELECT SaleId, SUM(Quantity * UnitPrice) AS Total FROM SaleItems GROUP BY SaleId) t ON t.SaleId = sa.SaleId
WHERE sa.ReceiptNo LIKE N'TMP-%';

-- Credit sales older than 45 days were paid off in two payments: half after 10 days, the rest after 25.
-- Newer credit sales are still open, so they show up on the Payments screen.
INSERT INTO Payments (CustomerId, Amount, PaidOn, CashierId)
SELECT sa.CustomerId, part.Amount, DATEADD(DAY, part.After, sa.SaleDate), @cashier
FROM Sales sa
CROSS APPLY (VALUES (FLOOR(sa.Total / 2), 10), (sa.Total - FLOOR(sa.Total / 2), 25)) part(Amount, After)
WHERE sa.ReceiptNo LIKE N'TMP-%' AND sa.PaymentType = N'Credit' AND sa.SaleDate <= DATEADD(DAY, -45, @today);

-- A customer's "customer since" date is their first sale.
UPDATE c SET CreatedOn = f.FirstSale
FROM Customers c JOIN (SELECT CustomerId, MIN(SaleDate) AS FirstSale FROM Sales GROUP BY CustomerId) f ON f.CustomerId = c.CustomerId
WHERE f.FirstSale < c.CreatedOn;

-- Receipt numbers in date order, so the newest sale has the highest number (OR-000270 up to OR-000479).
WITH ordered AS (SELECT ReceiptNo, ROW_NUMBER() OVER (ORDER BY SaleDate, SaleId) AS k FROM Sales)
UPDATE ordered SET ReceiptNo = N'OR-' + RIGHT(N'000000' + CAST(269 + k AS NVARCHAR(10)), 6);
GO
