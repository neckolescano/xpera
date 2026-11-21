/*  Sample data for one store database.
    Dates count back from today, so the data always looks current.
    Sample sign-ins for testing only, all with password xpera123:
      owner (Admin), cashier01 (Cashier), staff01 and staff02 (Staff).
    People sign in with only a username, so a username must be unique in ALL of xpera:
    PixelZone (Xpera_TenantA) keeps the plain names, Arcade Supply (TenantB) gets "arcade." in front
    (arcade.owner), Gaming Hub (TenantC) gets "hub." (hub.owner). 01-master-data.sql lists the same names.
    UC-09 Back-End adds the 200+ transactions the dashboard needs.  */

DECLARE @today DATE = CAST(GETDATE() AS DATE);
DECLARE @p NVARCHAR(10) = CASE DB_NAME() WHEN N'Xpera_TenantB' THEN N'arcade.' WHEN N'Xpera_TenantC' THEN N'hub.' ELSE N'' END;

-- The password hash is SHA-256 of "username:password", the same as Db.HashPassword in the app.
INSERT INTO Users (FullName, Username, PasswordHash, Role)
SELECT FullName, @p + Username, HASHBYTES('SHA2_256', @p + Username + N':xpera123'), Role
FROM (VALUES
    (N'Mark Villanueva', N'owner',     N'Admin'),
    (N'Alyssa Ramos',    N'cashier01', N'Cashier'),
    (N'Ben Torres',      N'staff01',   N'Staff'),
    (N'Joy Santiago',    N'staff02',   N'Staff')) v(FullName, Username, Role);

INSERT INTO Products (Name, Category, Price, IsAvailable) VALUES
    (N'RTX 5070 Ti 16GB',    N'Graphics cards', 48995, 0),
    (N'RTX 4070 Super 12GB', N'Graphics cards', 38995, 1),
    (N'Ryzen 7 7800X3D',     N'Processors',     24950, 1),
    (N'32GB DDR5 RAM',       N'Memory',          5990, 0),
    (N'27" 165Hz monitor',   N'Monitors',        9990, 1),
    (N'Mechanical keyboard', N'Peripherals',     4590, 1),
    (N'Gaming mouse',        N'Peripherals',     2990, 1),
    (N'Wireless headset',    N'Peripherals',     6450, 1);

INSERT INTO Customers (FullName, Contact, UsuallyBuys, CreatedOn) VALUES
    (N'Nina Go',     N'0917 555 0188', N'Mice, pads',    DATEADD(DAY, -120, @today)),
    (N'Ria Mendoza', N'0918 555 0164', N'Headsets',      DATEADD(DAY, -90,  @today)),
    (N'Marco Reyes', N'0917 555 0142', N'GPUs, cooling', DATEADD(DAY, -80,  @today)),
    (N'Alyssa Tan',  N'0918 555 0193', N'Peripherals',   DATEADD(DAY, -70,  @today)),
    (N'Jomar Cruz',  N'0927 555 0108', N'RAM, storage',  DATEADD(DAY, -60,  @today)),
    (N'Bea Santos',  N'0915 555 0176', N'Monitors',      DATEADD(DAY, -50,  @today)),
    (N'Paolo Diaz',  N'0919 555 0121', N'Full builds',   DATEADD(DAY, -45,  @today)),
    (N'Kyle Lim',    N'0995 555 0130', N'Keyboards',     DATEADD(DAY, -40,  @today));

-- Sales: receipt, days ago, customer, cash or credit, days until due (credit only).
DECLARE @sales TABLE (ReceiptNo NVARCHAR(20), DaysAgo INT, Customer NVARCHAR(80), PaymentType NVARCHAR(10), DueIn INT NULL);
INSERT INTO @sales VALUES
    (N'OR-000470', 85, N'Nina Go',     N'Cash',   NULL),
    (N'OR-000471', 40, N'Paolo Diaz',  N'Credit', 15),
    (N'OR-000472', 35, N'Jomar Cruz',  N'Credit', 15),
    (N'OR-000473', 20, N'Bea Santos',  N'Cash',   NULL),
    (N'OR-000474', 16, N'Kyle Lim',    N'Cash',   NULL),
    (N'OR-000475', 14, N'Alyssa Tan',  N'Cash',   NULL),
    (N'OR-000476', 10, N'Ria Mendoza', N'Cash',   NULL),
    (N'OR-000477',  3, N'Marco Reyes', N'Credit', 30),
    (N'OR-000478',  2, N'Alyssa Tan',  N'Cash',   NULL),
    (N'OR-000479',  1, N'Kyle Lim',    N'Cash',   NULL);

-- What was in each sale.
DECLARE @items TABLE (ReceiptNo NVARCHAR(20), Product NVARCHAR(80), Quantity INT);
INSERT INTO @items VALUES
    (N'OR-000470', N'Gaming mouse', 1),
    (N'OR-000471', N'RTX 4070 Super 12GB', 1), (N'OR-000471', N'Ryzen 7 7800X3D', 1),
    (N'OR-000471', N'32GB DDR5 RAM', 1),       (N'OR-000471', N'27" 165Hz monitor', 1),
    (N'OR-000472', N'32GB DDR5 RAM', 2),
    (N'OR-000473', N'27" 165Hz monitor', 1),
    (N'OR-000474', N'Mechanical keyboard', 1),
    (N'OR-000475', N'Mechanical keyboard', 1), (N'OR-000475', N'Gaming mouse', 1),
    (N'OR-000476', N'Wireless headset', 1),
    (N'OR-000477', N'RTX 4070 Super 12GB', 1),
    (N'OR-000478', N'Wireless headset', 1),
    (N'OR-000479', N'Gaming mouse', 1);

DECLARE @cashier INT = (SELECT UserId FROM Users WHERE Username = @p + N'cashier01');

INSERT INTO Sales (ReceiptNo, SaleDate, CustomerId, CashierId, PaymentType, Total, DueDate)
SELECT s.ReceiptNo, DATEADD(DAY, -s.DaysAgo, @today), c.CustomerId, @cashier, s.PaymentType, 0,
       CASE WHEN s.DueIn IS NULL THEN NULL ELSE DATEADD(DAY, s.DueIn - s.DaysAgo, @today) END
FROM @sales s JOIN Customers c ON c.FullName = s.Customer;

INSERT INTO SaleItems (SaleId, ProductId, Quantity, UnitPrice)
SELECT sa.SaleId, p.ProductId, i.Quantity, p.Price
FROM @items i
JOIN Sales sa ON sa.ReceiptNo = i.ReceiptNo
JOIN Products p ON p.Name = i.Product;

-- Each sale's total is the sum of its items.
UPDATE sa SET Total = t.Total
FROM Sales sa JOIN (SELECT SaleId, SUM(Quantity * UnitPrice) AS Total FROM SaleItems GROUP BY SaleId) t ON t.SaleId = sa.SaleId;

-- Payments toward the credit sales, leaving balances of 8,500 / 4,000 / 8,500.
INSERT INTO Payments (CustomerId, Amount, PaidOn, CashierId)
SELECT c.CustomerId, p.Amount, DATEADD(DAY, -p.DaysAgo, @today), @cashier
FROM (VALUES (N'Paolo Diaz', 71425, 30), (N'Jomar Cruz', 7980, 25), (N'Marco Reyes', 30495, 3)) p(Customer, Amount, DaysAgo)
JOIN Customers c ON c.FullName = p.Customer;

INSERT INTO Inquiries (InquiryDate, CustomerId, Question, ProductId, Status)
SELECT DATEADD(DAY, -q.DaysAgo, @today), c.CustomerId, q.Question, p.ProductId, q.Status
FROM (VALUES
        (0, N'Marco Reyes', N'Is the RTX 5070 Ti available?',        N'RTX 5070 Ti 16GB', N'Waiting for product'),
        (0, N'Kyle Lim',    N'When is the RTX 5070 Ti coming back?', N'RTX 5070 Ti 16GB', N'Waiting for product'),
        (1, N'Alyssa Tan',  N'Do you have 32GB DDR5 RAM?',           N'32GB DDR5 RAM',    N'Waiting for product'),
        (1, N'Bea Santos',  N'Can I reserve a 27" 165Hz monitor?',   NULL,                N'Open'),
        (3, N'Paolo Diaz',  N'PSU wattage for a full build',         NULL,                N'Resolved')
     ) q(DaysAgo, Customer, Question, Product, Status)
JOIN Customers c ON c.FullName = q.Customer
LEFT JOIN Products p ON p.Name = q.Product;

INSERT INTO Complaints (ComplaintDate, CustomerId, Details, Type, Status)
SELECT DATEADD(DAY, -k.DaysAgo, @today), c.CustomerId, k.Details, k.Type, k.Status
FROM (VALUES
        (0, N'Kyle Lim',   N'Mouse double-clicking after 2 weeks', N'Warranty claim', N'Open'),
        (1, N'Jomar Cruz', N'Wrong RAM speed was delivered',       N'Complaint',      N'In progress'),
        (4, N'Paolo Diaz', N'Monitor has a dead pixel',            N'Warranty claim', N'Resolved')
     ) k(DaysAgo, Customer, Details, Type, Status)
JOIN Customers c ON c.FullName = k.Customer;

INSERT INTO Feedback (FeedbackDate, CustomerId, Rating, Comment)
SELECT DATEADD(DAY, -f.DaysAgo, @today), c.CustomerId, f.Rating, f.Comment
FROM (VALUES
        (0, N'Alyssa Tan', 5, N'Fast checkout, and they tested the keyboard for me.'),
        (1, N'Paolo Diaz', 3, N'Good build, but I waited 2 days longer than promised.'),
        (2, N'Bea Santos', 5, N'Best monitor price in the area.'),
        (3, N'Jomar Cruz', 2, N'Received the wrong RAM speed.')
     ) f(DaysAgo, Customer, Rating, Comment)
JOIN Customers c ON c.FullName = f.Customer;
GO
