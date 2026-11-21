const fs = require('fs');
const {
  Document, Packer, Paragraph, TextRun, Table, TableRow, TableCell, WidthType, ShadingType,
  HeadingLevel, AlignmentType, LevelFormat, BorderStyle, PageBreak, Footer, PageNumber, ImageRun,
} = require('docx');

const RED = 'C3324B', INK = '121212', GRAY = '595959', LINE = 'D0D0D0', TINT = 'F4F1EA';
const W = 9360; // content width in DXA (A4 with 1" margins is 9026; we use Letter-ish margins below)

// ---------- small helpers ----------
const runs = (text) => {
  // **bold** and `code` inside a string
  const out = [];
  const re = /(\*\*[^*]+\*\*|`[^`]+`)/g;
  let last = 0, m;
  while ((m = re.exec(text))) {
    if (m.index > last) out.push(new TextRun(text.slice(last, m.index)));
    const t = m[0];
    if (t.startsWith('**')) out.push(new TextRun({ text: t.slice(2, -2), bold: true }));
    else out.push(new TextRun({ text: t.slice(1, -1), font: 'Consolas', size: 19, color: RED }));
    last = m.index + t.length;
  }
  if (last < text.length) out.push(new TextRun(text.slice(last)));
  return out;
};
const P = (text, opts = {}) => new Paragraph({ children: runs(text), spacing: { after: 120 }, ...opts });
const H1 = (text) => new Paragraph({ heading: HeadingLevel.HEADING_1, children: [new TextRun(text)] });
const H2 = (text) => new Paragraph({ heading: HeadingLevel.HEADING_2, children: [new TextRun(text)] });
const B = (text, level = 0) => new Paragraph({ numbering: { reference: 'bullets', level }, children: runs(text), spacing: { after: 60 } });
const N = (text, ref = 'steps') => new Paragraph({ numbering: { reference: ref, level: 0 }, children: runs(text), spacing: { after: 60 } });
const SAY = (text) => new Paragraph({
  children: [new TextRun({ text: 'Say it like this: ', bold: true, color: RED }), ...runs(text)],
  shading: { type: ShadingType.CLEAR, fill: TINT, color: 'auto' },
  border: { left: { style: BorderStyle.SINGLE, size: 18, color: RED, space: 8 } },
  spacing: { before: 80, after: 160 }, indent: { left: 160, right: 160 },
});
const border = { style: BorderStyle.SINGLE, size: 4, color: LINE };
const borders = { top: border, bottom: border, left: border, right: border };
function table(headers, rows, widths) {
  const cell = (text, w, head) => new TableCell({
    width: { size: w, type: WidthType.DXA }, borders,
    shading: head ? { type: ShadingType.CLEAR, fill: INK, color: 'auto' } : undefined,
    margins: { top: 60, bottom: 60, left: 100, right: 100 },
    children: [new Paragraph({ children: head ? [new TextRun({ text, bold: true, color: 'FFFFFF', size: 19 })] : runs(text).map(r => r), spacing: { after: 0 } })],
  });
  return new Table({
    width: { size: widths.reduce((a, b) => a + b, 0), type: WidthType.DXA }, columnWidths: widths,
    rows: [new TableRow({ tableHeader: true, children: headers.map((h, i) => cell(h, widths[i], true)) }),
           ...rows.map(r => new TableRow({ cantSplit: true, children: r.map((c, i) => cell(c, widths[i], false)) }))],
  });
}
const gap = () => new Paragraph({ children: [], spacing: { after: 80 } });
const pageBreak = () => new Paragraph({ children: [new PageBreak()] });

// ---------- extra helpers for this guide ----------
const CODE = (lines) => lines.split('\n').map((line, i, all) => new Paragraph({
  children: [new TextRun({ text: line || ' ', font: 'Consolas', size: 18, color: INK })],
  shading: { type: ShadingType.CLEAR, fill: 'EFEFEF', color: 'auto' },
  spacing: { before: i === 0 ? 60 : 0, after: i === all.length - 1 ? 140 : 0 },
  indent: { left: 160, right: 160 },
}));
const NOTE = (label, text) => new Paragraph({
  children: [new TextRun({ text: label + ' ', bold: true }), ...runs(text)],
  shading: { type: ShadingType.CLEAR, fill: 'FFF4E5', color: 'auto' },
  border: { left: { style: BorderStyle.SINGLE, size: 18, color: 'E8A33D', space: 8 } },
  spacing: { before: 80, after: 160 }, indent: { left: 160, right: 160 },
});

// ---------- content ----------
const body = [];

body.push(new Paragraph({ spacing: { after: 60 }, children: [new TextRun({ text: 'xpera CRM · Project structure and migrations', size: 44, bold: true, color: INK })] }));
body.push(new Paragraph({ spacing: { after: 60 }, children: [new TextRun({ text: 'What each project holds, how the code works (with pictures), and how I built the databases with EF Core migrations', size: 24, color: GRAY })] }));
body.push(new Paragraph({ spacing: { after: 240 }, border: { bottom: { style: BorderStyle.SINGLE, size: 8, color: RED, space: 6 } }, children: [new TextRun({ text: 'Nicho Lescano · BSIT 3 · IT13L (3462) · Instructor: Renz Arriola · November 2026', size: 20, color: GRAY })] }));

// 1
body.push(H1('1. The big picture'));
body.push(P('The solution `Xpera.slnx` is split into **four projects**. Each one has one job, so a change in one place does not break the others. This is called **separation of concerns** (also known as a layered or "clean" architecture).'));
body.push(table(['Project', 'Its one job', 'Holds'], [
  ['**Xpera**', 'The user interface (what people see and click)', 'Forms, screens, custom controls, colors, fonts'],
  ['**Xpera.domain**', 'The models: what a Customer, Sale, Company… is', 'Entity classes, fixed choices, peso format'],
  ['**Xpera.infrastructure**', 'Everything that talks to the database', 'DbContexts, migrations, data access, SQL scripts'],
  ['**Xpera.api**', 'A web API so other apps can use the same data', 'Controllers (one per feature), Program.cs, appsettings.json'],
], [2300, 3600, 3460]));
body.push(gap());
body.push(H2('Who uses whom (project references)'));
body.push(table(['Project', 'References', 'Why'], [
  ['Xpera.domain', '(nothing)', 'The models are plain C#. They do not need to know about screens or databases.'],
  ['Xpera.infrastructure', 'Xpera.domain', 'It saves and loads the models, so it must know them.'],
  ['Xpera.api', 'Xpera.infrastructure + Xpera.domain', 'Controllers load models through the infrastructure and send them out as JSON.'],
  ['Xpera (UI)', 'Xpera.infrastructure + Xpera.domain', 'Screens show models and call the infrastructure to load and save.'],
], [2300, 3000, 4060]));
body.push(P('The arrows only point **toward** the domain. The domain never points back. That is why the domain has no packages at all.', { spacing: { before: 120, after: 120 } }));
body.push(P('The desktop app calls the infrastructure **directly**, so it does not need the API running. The API uses the **same** infrastructure, so both always read the same data with the same rules.'));
body.push(SAY('"Xpera shows things, Xpera.domain describes things, Xpera.infrastructure stores things, and Xpera.api shares things. The domain is in the middle and depends on nothing."'));

// 2
body.push(pageBreak());
body.push(H1('2. Xpera (the UI project)'));
body.push(P('A **Windows Forms** app (.NET 10). It only shows data and reacts to clicks. It never writes SQL itself.'));
body.push(table(['Folder / file', 'What it holds', 'What it does'], [
  ['`Program.cs`', 'The start of the app', 'Loads the Inter font and opens the sign-in window.'],
  ['`LoginForm`', 'Sign-in window (+ Designer file)', 'Store code → finds the store in the Master; username + password → signs in.'],
  ['`MainForm`', 'Sidebar + page area (+ Designer)', 'Builds the menu for the signed-in role and swaps the screens.'],
  ['`Screens/`', 'One UserControl per page', 'Sales, Payments, Products, Customers, Inquiries, Complaints, Feedback, Users.'],
  ['`Controls/`', 'Custom controls', 'Card, RoundButton, InputBox, SearchBox, Pager, StatStrip, ModalForm, PickerPopup…'],
  ['`EntryForm`, `Notice`', 'Pop-up form and pop-up message', 'Every "Add…" form and every message uses these two.'],
  ['`Ui.cs`, `Typography.cs`, `Sizes.cs`', 'Shared styles', 'Colors, dark mode, fonts, sizes in one place, reused by every screen.'],
  ['`Theme/` (linked)', 'Brand color model', 'Tints, Tones, Shades of Signal Red and Carbon.'],
  ['`Fonts/`', 'Inter font files', 'Copied next to the app so it looks the same on every PC.'],
], [2500, 2900, 3960]));
body.push(P('Packages: **none**. It gets the database packages through Xpera.infrastructure.', { spacing: { before: 120, after: 120 } }));

body.push(H1('3. Xpera.domain (the models)'));
body.push(P('Plain C# classes. Each **entity** describes one table: one property = one column. This is where the user\'s own `Company` and `CompanyDatabase` (the teacher\'s format) live, and the other models follow the same style.'));
body.push(table(['Entity (entities/)', 'Table', 'Database', 'Use case'], [
  ['`Company`', 'Companies', 'Xpera_Master', 'UC-13 Manage Tenants (each store)'],
  ['`CompanyDatabase`', 'CompanyDatabases', 'Xpera_Master', 'Which database belongs to which store'],
  ['`Plan`', 'Plans', 'Xpera_Master', 'Plan A, B, C'],
  ['`Subscription`', 'Subscriptions', 'Xpera_Master', 'UC-14 Manage Subscriptions'],
  ['`User`', 'Users', 'each store', 'UC-01 Staff accounts'],
  ['`Product`', 'Products', 'each store', 'UC-02 Products'],
  ['`Customer`', 'Customers', 'each store', 'UC-03 Customers'],
  ['`Sale`, `SaleItem`', 'Sales, SaleItems', 'each store', 'UC-04 Record Sale'],
  ['`Payment`', 'Payments', 'each store', 'UC-05 Record Payment'],
  ['`Inquiry`', 'Inquiries', 'each store', 'UC-06 Inquiries'],
  ['`Complaint`', 'Complaints', 'each store', 'UC-07 Complaints'],
  ['`Feedback`', 'Feedback', 'each store', 'UC-08 Feedback'],
  ['`StoreInfo`, `SignInResult`', '(not tables)', '–', 'The signed-in store, and the result of a sign-in'],
], [2400, 1900, 1700, 3360]));
body.push(P('Also here: `Choices.cs` (the fixed dropdown options, like the inquiry statuses) and `Money.cs` (how pesos are written, ₱45,200). Packages: **none**.', { spacing: { before: 120, after: 120 } }));
body.push(SAY('"An entity is the blueprint of a table. EF Core reads these classes and builds the real tables from them."'));

// 4
body.push(H1('4. Xpera.infrastructure (the database layer)'));
body.push(H2('data/ (the C# that talks to SQL Server)'));
body.push(table(['File', 'What it does'], [
  ['`MasterCrmDbContext`', 'The teacher-format DbContext for the Master. Inherits **IdentityDbContext**, so it also has the AspNet login tables. Lists Companies, CompanyDatabases, Plans, Subscriptions and their rules (keys, lengths, unique codes, foreign keys).'],
  ['`TenantCrmDbContext`', 'The DbContext for **one store\'s** database. Lists Users, Products, Customers, Sales, SaleItems, Payments, Inquiries, Complaints, Feedback, with defaults (e.g. today\'s date) and CHECK rules (e.g. rating 1 to 5).'],
  ['`DbContexts`', 'Opens either DbContext from the desktop app (WinForms has no Program.cs services), with the connection from Db.cs. Migrations do not use it: they go through Program.cs in the API.'],
  ['`Db.cs`', 'The connection rules only: which server and database to open (LocalDB on this PC, or an online server from `%USERPROFILE%\\.xpera\\databases.txt`), and the password hash. A wrong online password is tried only once.'],
  ['`MasterDb`', 'Uses **MasterCrmDbContext**: finds a store by its code (Companies + CompanyDatabases + its active Subscription) and checks the Super Admin\'s password with Identity\'s PasswordHasher.'],
  ['`StoreDb`', 'Uses **TenantCrmDbContext** for everything the screens do: SignIn, SaveSale (one transaction), RecordPayment, AddInquiry… with LINQ (`db.Customers.OrderBy(...)`, `db.Sales.Add(...)`, `SaveChanges()`). Reads the two views with `SqlQuery`. Holds `Session`: who is signed in, to which store.'],
  ['`StoreRecords`', 'Uses **TenantCrmDbContext** to read whole models (with LINQ) for the API.'],
], [2500, 6860]));
body.push(H2('Migrations/ and Database/'));
body.push(B('`Migrations/` = the Master migrations: `InitialMasterCrm` (mine) and `AddPlansAndSubscriptions`, plus `MasterCrmDbContextModelSnapshot` (EF\'s memory of the last model).'));
body.push(B('`Migrations/Tenant/` = `InitialTenantCrm` for the store databases (it also creates the two views CustomerBalances and CustomerSummary).'));
body.push(B('`Database/` = sample-data scripts: `01-master-data.sql` (stores, plans, subscriptions, Super Admin), `03-store-sample-data.sql` and `04-store-transactions.sql` (staff, products, customers, 210 sales per store). `00` and `02` are the migrations as SQL, used only by `setup.ps1`.'));
body.push(H2('Packages'));
body.push(table(['Package', 'Why it is here'], [
  ['Microsoft.EntityFrameworkCore.SqlServer', 'EF Core for SQL Server (the DbContexts).'],
  ['Microsoft.EntityFrameworkCore.Design + Tools', 'Add-Migration, Update-Database, Drop-Database commands.'],
  ['Microsoft.AspNetCore.Identity.EntityFrameworkCore', 'IdentityDbContext and the AspNet login tables (Super Admin).'],
  ['System.Security.Cryptography.Pkcs 10.0.14', 'Pins a safe version. EF SqlServer pulls in an older one with a known vulnerability.'],
], [4200, 5160]));

// 5
body.push(H1('5. Xpera.api (the web API)'));
body.push(B('`Program.cs` registers **MasterCrmDbContext** with `AddDbContext` and reads the connection string `MasterCrm` from `appsettings.json` (`Xpera_Master` on `(localdb)\\MSSQLLocalDB`). It also registers **TenantCrmDbContext** with one of the lines `TenantA`, `TenantB`, `TenantC` (TenantA unless a command passes `--Tenant`). This is why the API is the **startup project** for migrations.'));
body.push(B('`Controllers/`: one controller per feature, all built on `StoreControllerBase`. Every request sends three headers (`X-Store-Code`, `X-Username`, `X-Password`); the API signs in exactly like the app and checks the role.'));
body.push(table(['Controller', 'Address', 'Who may change data'], [
  ['Account', 'GET api/me', 'anyone signed in'],
  ['Users', 'api/users', 'Admin only'],
  ['Products', 'api/products, PUT …/{id}/availability', 'Admin adds; Admin or Staff change availability'],
  ['Customers', 'api/customers, GET …/{id}/card', 'anyone signed in'],
  ['Sales', 'api/sales', 'Admin or Cashier (prices always come from the database)'],
  ['Payments', 'api/payments', 'Admin or Cashier'],
  ['Inquiries, Complaints', 'api/inquiries, api/complaints, PUT …/{id}/status', 'anyone signed in'],
  ['Feedback', 'api/feedback', 'anyone signed in'],
], [2300, 3900, 3160]));
body.push(P('`Xpera.api.http` has ready-made sample requests you can send from Visual Studio.', { spacing: { before: 120, after: 120 } }));

// 6
body.push(H1('6. The databases (multi-tenant)'));
body.push(table(['Database', 'Made by', 'Holds'], [
  ['**Xpera_Master**', 'MasterCrmDbContext', 'Companies, CompanyDatabases, Plans, Subscriptions, AspNet… (Identity), __EFMigrationsHistory'],
  ['**Xpera_TenantA**', 'TenantCrmDbContext', 'PixelZone (PIXEL01, Plan A): Users, Products, Customers, Sales, SaleItems, Payments, Inquiries, Complaints, Feedback + 2 views'],
  ['**Xpera_TenantB**', 'TenantCrmDbContext', 'Arcade Supply (ARCADE01, Plan B): same tables'],
  ['**Xpera_TenantC**', 'TenantCrmDbContext', 'Gaming Hub (HUB01, Plan C): same tables'],
], [2200, 2300, 4860]));
body.push(H2('What happens at sign-in'));
['The user types `PIXEL01`. `MasterDb` opens **Xpera_Master** through MasterCrmDbContext.',
 'It finds PixelZone in **Companies**, its database (`Xpera_TenantA`) in **CompanyDatabases**, and its plan (A, Micro) in **Subscriptions**.',
 'The username and password are checked in **Xpera_TenantA**\'s Users table (3 wrong tries lock the account).',
 'From then on, every screen reads and writes only **Xpera_TenantA**. Another store can never see this data.',
].forEach(s => body.push(N(s, 'signin')));
body.push(SAY('"There is one Master that knows every store, and every store has its own database with the same tables. The store code at sign-in decides which database opens."'));

// ---------- 7. Reading the code (picture + explanation) ----------
const SIZES = JSON.parse(fs.readFileSync(__dirname + '/code/sizes.json', 'utf8'));
const K = 0.349; // one scale for every picture, so the code text is the same size everywhere
const PIC = (id, caption) => {
  const [w, h] = SIZES[id];
  return [
    new Paragraph({ keepNext: true, spacing: { before: 120, after: 60 }, children: [new TextRun({ text: caption, bold: true, size: 20, color: GRAY })] }),
    new Paragraph({ keepNext: true, spacing: { after: 100 }, children: [new ImageRun({ type: 'png', data: fs.readFileSync(__dirname + '/code/' + id + '.png'), transformation: { width: Math.round(w * K), height: Math.round(h * K) } })] }),
    new Paragraph({ keepNext: true, spacing: { after: 60 }, children: [new TextRun({ text: 'How it works', bold: true, color: RED })] }),
  ];
};
const L = (lines, text) => B('**' + lines + '** ' + text);

body.push(H1('7. Reading the code (picture + explanation)'));
body.push(P("Each picture below is copied from the real file. The **line numbers are the same as in Visual Studio**, so you can open the file and jump to a line with **Ctrl + G**. A **⋮** row means some lines in between were skipped. Under every picture, **How it works** explains it line by line."));
body.push(P("The pictures follow the order the system is built in: the **entities** (what things are), the **DbContexts** (the rules), the **migration** (the tables EF made), the **connection** (how a database is found), then two real trips through the code: **signing in** and **saving a sale**, and last the **API**."));

// 7.1 entities
body.push(H2('7.1 Entities (Xpera.domain): what a thing is'));
body.push(...PIC('company', 'Picture 1. Company.cs: one store in the Master (your file)'));
body.push(L('Line 7', "`public class Company`: one class describes one table. The table is called **Companies** because that is the name of its DbSet in MasterCrmDbContext."));
body.push(L('Line 9', "`CompanyId`: a property named after the class + `Id` becomes the **primary key**. Because it is an `int`, SQL Server numbers it by itself (1, 2, 3…)."));
body.push(L('Lines 11, 13', "`= string.Empty` means the text is never null. How long it may be (50 and 200 letters) is set later in the DbContext."));
body.push(L('Line 15', "`IsActive = true`: a store can be **suspended** without deleting it. The sign-in screen refuses a suspended store."));
body.push(L('Line 17', "`CreatedAt = DateTime.UtcNow`: the date is filled in by itself when a new Company is made."));

body.push(...PIC('companydb', "Picture 2. CompanyDatabase.cs: which database belongs to which store (your file)"));
body.push(L('Line 11', "`CompanyId` is a **foreign key**: it points to the store in Companies that owns this database."));
body.push(L('Lines 13, 15', "`ServerName` and `DatabaseName` are the tenant's “connection line”. The app builds the full connection string from these two values (Picture 11), which is why the app needs no line per tenant in a config file."));
body.push(L('Line 19', "`Company? Company` is a **navigation property**: from a CompanyDatabase row, C# can reach its Company. The `?` means it may be empty when it was not loaded."));

body.push(...PIC('sale', 'Picture 3. Sale.cs: one checkout in a store'));
body.push(L('Lines 6–13', "One property = one column. `decimal` is used for money (never `double`, which can round 0.1 + 0.2 wrongly). `int?` and `DateTime?` (with `?`) may be empty: a sale may have no cashier, and only **credit** sales have a due date."));
body.push(L('Line 14', "`List<SaleItem> Items`: one sale has **many** items. Because they are inside the Sale object, EF can save the sale and all its items in one go (Picture 18)."));
body.push(SAY('"An entity is a plain C# class. It does not know about the database; it only describes the thing. That is why Xpera.domain has no packages."'));

// 7.2 DbContexts
body.push(H2('7.2 DbContexts (Xpera.infrastructure): the rules for the tables'));
body.push(...PIC('mastercontext', 'Picture 4. MasterCrmDbContext.cs: the Master database (your file, plus Plans and Subscriptions)'));
body.push(L('Line 10', "`: IdentityDbContext` means it **inherits** everything IdentityDbContext has: the ready-made login tables (AspNetUsers, AspNetRoles…). The Super Admin is a row in AspNetUsers."));
body.push(L('Lines 12–18', "Each `DbSet<…>` is **one table**: Companies, CompanyDatabases, Plans, Subscriptions. `=> Set<Company>()` is a short way to write a getter."));
body.push(L('Line 20', "The **constructor** receives the `options`, which say which server and database to use. The class never decides that itself: the API's Program.cs (Picture 10) or DbContexts.cs (Picture 12) hands the options in. That is why the same class works in both programs."));
body.push(L('Lines 22–24', "`OnModelCreating` holds the rules EF cannot guess from the class. `base.OnModelCreating(builder)` must come first, or the Identity tables would be missing."));
body.push(L('Lines 27–38', "`HasKey` = primary key. `HasMaxLength(50).IsRequired()` becomes `nvarchar(50) NOT NULL`. `HasIndex(...).IsUnique()` = no two stores can have the same code."));
body.push(L('Lines 54–57', "`HasOne(...).WithMany().HasForeignKey(...)` = one Company has many CompanyDatabase rows. `DeleteBehavior.Restrict` = SQL Server refuses to delete a company that still has a database row, so data is never left pointing at nothing."));

body.push(...PIC('tenantcontext', "Picture 5. TenantCrmDbContext.cs: the tables inside every store's database"));
body.push(L('Line 11', "A plain `DbContext` (not Identity): store staff sign in through the store's own **Users** table."));
body.push(L('Lines 13–29', "Nine DbSets = **nine tables**, the same in Xpera_TenantA, B and C."));
body.push(L('Line 31', "Same idea as the Master: the options decide **which store** database. One class, three databases."));
body.push(L('Line 38', "`today` is SQL text (`CAST(GETDATE() AS DATE)`) used as the default for every date column, so SQL Server fills in today's date by itself."));

body.push(...PIC('tenantsale', 'Picture 6. TenantCrmDbContext.cs: the rules for the Sales table'));
body.push(L('Lines 131–136', "A receipt number is required, at most 20 letters, and **unique**: two sales can never share OR-000483."));
body.push(L('Lines 138–140', "`HasColumnType(\"date\")` keeps only the date (no time). `HasDefaultValueSql(today)` = today, if nothing is given."));
body.push(L('Lines 146–147', "`HasPrecision(10, 2)` = `decimal(10,2)`: up to 99,999,999.99, always with centavos."));
body.push(L('Lines 152–160', "Every sale must point to a **real customer** and a **real cashier** (foreign keys). `HasOne<Customer>()` with no property inside just adds the rule."));
body.push(L('Lines 162–165', "`HasMany(x => x.Items)`: one sale, many SaleItems, joined by SaleId."));
body.push(L('Lines 167–171', "**Check constraints**: SQL Server itself refuses a PaymentType other than Cash or Credit, or a negative total, even if a bug in the code or a typo in SSMS tries."));

// 7.3 migration
body.push(H2('7.3 The migration: what Add-Migration wrote'));
body.push(...PIC('migration', 'Picture 7. InitialTenantCrm.cs: the Sales table, written by Add-Migration'));
body.push(L('Whole picture', "You did not type this. **Add-Migration** read Picture 6 and wrote it. Compare them: `HasMaxLength(20)` → `nvarchar(20)` (line 180), `HasDefaultValueSql(today)` → `defaultValueSql` (line 181), `HasPrecision(10, 2)` → `decimal(10,2)` (line 185)."));
body.push(L('Line 179', "`SqlServer:Identity, 1, 1` = the SaleId numbers itself, starting at 1, going up by 1."));
body.push(L('Lines 190–204', "The primary key, the two check constraints and the two foreign keys from Picture 6."));
body.push(L('When it runs', "**Update-Database** turns this into `CREATE TABLE Sales (...)`, runs it, and writes `InitialTenantCrm` into **__EFMigrationsHistory** so it never runs twice."));

body.push(...PIC('view', 'Picture 8. InitialTenantCrm.cs: a view added by hand'));
body.push(L('Line 325', "EF has no command for views, so this line was added by hand. `migrationBuilder.Sql(...)` runs any SQL during the migration."));
body.push(L('Lines 326–331', "A **view** is a saved SELECT. For each customer it adds up what they spent (`SUM`) and finds their last visit (`MAX`). `LEFT JOIN` keeps customers who never bought anything (they show ₱0). Nothing is stored twice: the numbers are always worked out fresh from Sales."));
body.push(L('EXEC(N’...’)', "The wrapper lets the same view also run inside the generated script `02-store-tables.sql` that setup.ps1 uses."));

// 7.4 connection
body.push(H2('7.4 The connection: how a database is found'));
body.push(...PIC('appsettings', "Picture 9. appsettings.json: the API's connection strings"));
body.push(L('Line 8', "`ConnectionStrings` is a list of **name: value** pairs."));
body.push(L('Line 9', "`MasterCrm` is read from SSMS: `Server=` is the **Server name** box (written with `\\\\` in JSON because one `\\` is special there), `Database=` is the name under **Databases**, `Trusted_Connection=True` is **Windows Authentication**, `TrustServerCertificate=true` is the **Trust server certificate** tick."));
body.push(L('Lines 10–12', "`TenantA`, `TenantB`, `TenantC` are the same, only the database name changes."));
body.push(L('Who reads it', "Only **Xpera.api** (and the migration commands, which start Xpera.api). The desktop app never reads this file."));

body.push(...PIC('apiprogram', "Picture 10. Program.cs (Xpera.api): where the API registers the DbContexts"));
body.push(L('Line 4', "`builder` sets up the API. It reads appsettings.json by itself."));
body.push(L('Lines 6–8', "`AddDbContext<MasterCrmDbContext>`: “whenever something needs a MasterCrmDbContext, make one with the `MasterCrm` line.” This is called **dependency injection**."));
body.push(L('Line 13', "`builder.Configuration[\"Tenant\"] ?? \"TenantA\"`: read a setting called Tenant. `??` means “if it is empty, use TenantA”. The command `-Args \"--Tenant TenantB\"` fills it in."));
body.push(L('Lines 14–16', "Registers TenantCrmDbContext with that line."));
body.push(L('Why it matters', "Update-Database **starts this Program.cs** (that is what `-StartupProject Xpera.api` means) and uses these two registrations to know which database to build."));

body.push(...PIC('db', 'Picture 11. Db.cs: opening a connection (used by the desktop app and the API controllers)'));
body.push(L('Line 38', "`Open(database)` makes a `SqlConnection` and opens it."));
body.push(L('Line 40', "If this database's password was wrong before, it stops right away. Online hosts like MonsterASP block a PC after repeated failed logins, so one wrong try must stay one."));
body.push(L('Lines 47–51', "`catch ... when (ex.Number == 18456)` catches only **login failed**, remembers it, and passes the error on."));
body.push(L('Lines 60–65', "If `databases.txt` (in your user folder) has a line for this database, that online connection string is used."));
body.push(L('Lines 66–69', "Otherwise it **builds** the string: `Server=` from CompanyDatabases.ServerName (or LocalDB), `Database=` the name, `Integrated Security=true` (= Windows Authentication). This is how a tenant connects without a line in appsettings.json."));

body.push(...PIC('dbcontexts', 'Picture 12. DbContexts.cs: the desktop version of AddDbContext'));
body.push(L('Why it exists', "WinForms has no Program.cs services, so this file does by hand what `AddDbContext` does in the API."));
body.push(L('Lines 9–13', "Makes a MasterCrmDbContext with `UseSqlServer(Db.Open(...))`: an already-open connection from Picture 11. `contextOwnsConnection: true` = closing the context also closes the connection."));
body.push(L('Lines 16–20', "`Tenant(database)` does the same for one store. StoreDb passes the signed-in store's database (Picture 16)."));

// 7.5 sign in
body.push(H2('7.5 Trip 1: signing in with PIXEL01'));
body.push(...PIC('login', 'Picture 13. LoginForm.cs (Xpera, the UI): the Sign in button'));
body.push(L('Line 79', "Asks the Master for the store: `MasterDb.FindStore(code)` (Picture 14). No store → a message, and stop."));
body.push(L('Lines 86–92', "The code `MASTER` is the Super Admin, checked by Identity in the Master database."));
body.push(L('Line 94', "A suspended store cannot sign in."));
body.push(L('Line 101', "`Session.Store = store`: **from here on, everything uses this store's database** (Picture 15)."));
body.push(L('Lines 102–113', "`StoreDb.SignIn(...)` checks the username and password and answers Ok, Locked or WrongPassword."));
body.push(L('Lines 127–131', "Opens MainForm, shows the store name and plan in the sidebar, and hides the login window."));
body.push(L('Notice', "The UI never writes SQL. It only calls the infrastructure (MasterDb, StoreDb). That is the layering from section 1."));

body.push(...PIC('finddstore', 'Picture 14. MasterDb.cs: finding the store in the Master'));
body.push(L('Line 13', "`Trim().ToUpper()` so “ pixel01 ” still works."));
body.push(L('Line 17', "`using ... = DbContexts.Master()` opens Xpera_Master. `using` closes it again at the end of the method."));
body.push(L('Line 19', "A **LINQ** query. EF turns it into `SELECT ... FROM Companies WHERE CompanyCode = @p0`, with the typed code sent as a **parameter**, so typed text can never become SQL."));
body.push(L('Lines 22–30', "Gets the store's **database row** and its **active subscription** (newest first), which says the plan: A, B or C."));
body.push(L('Line 33', "Remembers which server that database lives on (used by Db.cs, Picture 11)."));
body.push(L('Lines 35–44', "Packs everything into a `StoreInfo` (a domain class), for example Database = `Xpera_TenantA`, Plan = `A`."));

body.push(...PIC('session', 'Picture 15. StoreDb.cs: Session, who is signed in'));
body.push(L('static', "A `static` class has **one shared copy** for the whole program: the store, the user's id and name, and the role."));
body.push(L('[ThreadStatic]', "Gives each thread its own copy. The desktop app has one thread, so nothing changes there. The API serves many people at once, so each request gets its own copy and clears it afterwards (`Clear()`, lines 16–22)."));
body.push(L('Role', "MainForm reads `Session.Role` to show only the menu items for Admin, Cashier or Staff."));

body.push(...PIC('signin', 'Picture 16. StoreDb.cs: Open() and SignIn()'));
body.push(L('Lines 51–54', "`Open()` is used by **every** StoreDb method. It opens the signed-in store's database (`Session.Store.Database`). This one line is what keeps every store's data apart."));
body.push(L('Lines 73–74', "Finds the user. An unknown user gets the same answer as a wrong password, so nobody can guess which usernames exist."));
body.push(L('Line 77', "Compares **hashes**, not passwords. `Db.HashPassword` makes SHA-256 of “username:password”; the database only stores the hash."));
body.push(L('Lines 79–82', "Wrong password: add 1 to FailedLogins; at 3 the account is **Locked**. `SaveChanges()` sends the UPDATE to SQL Server."));
body.push(L('Lines 85–90', "Right password: reset the counter and fill in Session."));

// 7.6 sale
body.push(H2('7.6 Trip 2: saving a sale'));
body.push(...PIC('salesbutton', 'Picture 17. SalesScreen.cs (Xpera, the UI): the Save sale button'));
body.push(L('Lines 303–307', "Checks that there is a customer and at least one item."));
body.push(L('Lines 310–315', "Turns every row of the cart into (ProductId, Quantity, Price)."));
body.push(L('Line 319', "**One call** to the infrastructure: `StoreDb.SaveSale(...)`. It gives back the receipt number."));
body.push(L('Lines 323–327', "Clears the cart, refreshes Sales history and shows “Sale saved”."));

body.push(...PIC('savesale', 'Picture 18. StoreDb.cs: SaveSale()'));
body.push(L('Lines 231–236', "The next receipt: the highest ReceiptNo + 1, written as OR-000484."));
body.push(L('Line 242', "`BeginTransaction()`: **all or nothing**. Either the sale and all its items are saved, or nothing is."));
body.push(L('Lines 245–254', "Builds the Sale object with its Items. The cashier comes from Session, the total is worked out from the items, and the due date is kept only for credit."));
body.push(L('Lines 255–256', "`Add` + `SaveChanges()`: EF sends `INSERT INTO Sales`, gets the new SaleId, then `INSERT INTO SaleItems` with that SaleId."));
body.push(L('Line 258', "`Commit()` makes it permanent. If anything failed before this line, `using` throws the transaction away and SQL Server undoes everything (a **rollback**)."));

body.push(...PIC('readview', 'Picture 19. StoreDb.cs: reading a view, and the simplest save'));
body.push(L('Line 169', "A view is not a table, so it has no DbSet. `Database.SqlQuery<CustomerSummaryRow>(...)` runs the SELECT and fills a CustomerSummaryRow for each row, matching columns to properties by name. Values put inside `$\"...{x}\"` are sent as parameters too."));
body.push(L('Lines 170–171', "Sorts by name and turns each row into the cells of the Customers table (with ₱ money format)."));
body.push(L('Lines 278–283', "`RecordPayment`: the simplest write. `Add` a Payment, `SaveChanges()` → one INSERT. The check constraint makes sure the amount is more than 0."));

// 7.7 api
body.push(H2('7.7 The API: the same code, over the web'));
body.push(...PIC('controllerbase', 'Picture 20. StoreControllerBase.cs: the parent of every controller'));
body.push(L('Line 15', "`InStore(work, roles)`: `work` is the code to run if the caller is allowed; `roles` says who may (empty = anyone signed in)."));
body.push(L('Lines 17–21', "Reads the three headers. Missing → **401 Unauthorized**."));
body.push(L('Lines 23–30', "Exactly the desktop sign-in: FindStore (Picture 14), then Session.Store and StoreDb.SignIn (Picture 16)."));
body.push(L('Lines 33–34', "Wrong role → **403 Forbidden**."));
body.push(L('Lines 36–37', "Runs the work. An answer → **200 OK** with JSON; nothing → **204 No Content**."));
body.push(L('Lines 39–46', "A `Require` that failed → **400 Bad Request**. `finally` always clears Session, so the next request starts signed out."));

body.push(...PIC('payments', 'Picture 21. PaymentsController.cs: one controller'));
body.push(L('Line 8', "`[Route(\"api/payments\")]` = the web address."));
body.push(L('Lines 11–12', "`GET api/payments` lists the payments."));
body.push(L('Lines 14–21', "`POST api/payments`: the JSON body becomes a `Payment` object by itself. `Require` checks it, then it calls **the same** `StoreDb.RecordPayment` the desktop Payments screen uses. Only Admin or Cashier."));

body.push(H2('7.8 The whole trip of one sale, in one list'));
['The cashier clicks **Save sale** (Picture 17, UI).',
 'The screen calls `StoreDb.SaveSale` (Picture 18, infrastructure).',
 '`Open()` asks `DbContexts.Tenant(Session.Store.Database)` for **Xpera_TenantA** (Pictures 16 and 12).',
 '`Db.Open` builds the connection string and opens it (Picture 11).',
 'EF uses the rules in **TenantCrmDbContext** (Picture 6) to write the INSERTs for the **Sale** and its **SaleItems** (Picture 3).',
 'SQL Server checks the constraints the **migration** made (Picture 7) and saves the rows.',
 '`Commit()`, then the receipt number goes back to the screen, which shows “Sale saved”.',
].forEach(s => body.push(N(s, 'trip')));
body.push(SAY('"The screen only asks. The infrastructure does the work through the DbContext. The domain class carries the data. And the database was built by the migration, from the same DbContext rules."'));

// ---------- 8. Cloud backup and sign out (MonsterASP) ----------
const SIZES2 = JSON.parse(fs.readFileSync(__dirname + '/code2/sizes.json', 'utf8'));
const PIC2 = (id, caption) => {
  const [w, h] = SIZES2[id];
  return [
    new Paragraph({ keepNext: true, spacing: { before: 120, after: 60 }, children: [new TextRun({ text: caption, bold: true, size: 20, color: GRAY })] }),
    new Paragraph({ keepNext: true, spacing: { after: 100 }, children: [new ImageRun({ type: 'png', data: fs.readFileSync(__dirname + '/code2/' + id + '.png'), transformation: { width: Math.round(w * K), height: Math.round(h * K) } })] }),
    new Paragraph({ keepNext: true, spacing: { after: 60 }, children: [new TextRun({ text: 'How it works', bold: true, color: RED })] }),
  ];
};

body.push(H1('8. Cloud backup and sign out (MonsterASP)'));
body.push(P("**The idea:** each store works on its **own PC** (LocalDB). That stays the main database: it is fast and keeps working when the internet is down. Each store also has a database on **MonsterASP** (the cloud). xpera copies the store's local data **up** to it. The copy only goes one way, **local → cloud**, so the cloud is the store's backup: if the PC breaks, the data is still safe online."));
body.push(P("**When it copies** (my teacher left this to my own logic):"));
body.push(table(['When', 'Who', 'Why'], [
  ['**Back up now** button in the sidebar', 'Admin (store owner) only', 'The owner decides; easy to show in a demo.'],
  ['**Sign out** (the arrow icon on the profile card)', 'Any role', 'Cashiers add most of the day\'s sales, so every sign-out saves the day\'s work.'],
  ['**Closing the window (×)**', 'Any role', 'Closing xpera counts as signing out, so nothing is forgotten.'],
], [3300, 2200, 3860]));
body.push(P("The sidebar shows **Last backup · 1:05 PM** (or **Failed · no internet** in red). Hovering over it shows the full date, time and number of rows.", { spacing: { before: 120, after: 120 } }));
body.push(SAY('"xpera works offline on the store\'s PC. When the owner clicks Back up now, or anyone signs out, it copies that store\'s data to its own cloud database. If the internet is down, nothing breaks: it simply tries again next time."'));

body.push(H2('8.1 The cloud databases'));
body.push(table(['Local (main)', 'MonsterASP (backup)', 'Login key'], [
  ['Xpera_Master', 'db77928 (master_xpera)', 'CloudMaster'],
  ['Xpera_TenantA · PixelZone', 'db79610 (tenanta_xpera)', 'CloudTenantA'],
  ['Xpera_TenantB · Arcade Supply', 'db79617 (tenantb_xpera)', 'CloudTenantB'],
  ['Xpera_TenantC · Gaming Hub', 'db79621 (tenantc_xpera)', 'CloudTenantC'],
], [3300, 3300, 2760]));
body.push(P('The cloud tables were built **the same way as the local ones**, with the same migrations. `Program.cs` picks the database by name from `appsettings.json`:', { spacing: { before: 120, after: 80 } }));
body.push(...CODE(`Update-Database -Context MasterCrmDbContext -StartupProject Xpera.api -Args "--Master CloudMaster"
Update-Database -Context TenantCrmDbContext -StartupProject Xpera.api -Args "--Tenant CloudTenantA"
Update-Database -Context TenantCrmDbContext -StartupProject Xpera.api -Args "--Tenant CloudTenantB"
Update-Database -Context TenantCrmDbContext -StartupProject Xpera.api -Args "--Tenant CloudTenantC"`));
body.push(P('Then the cloud Master got its data in SSMS: `01-master-data.sql`, plus 3 `UPDATE` lines so each store points to **its cloud database** with the key `CloudTenantA/B/C`. The local Master keeps pointing to Xpera_TenantA/B/C with the keys TenantA/B/C. Each Master is the "address book" of its own world.'));
body.push(NOTE('SQL logins everywhere:', 'MonsterASP only accepts SQL logins (it does not know my Windows account). Locally I also use one SQL login per database, so local and cloud work the same way and each login opens only its own store.'));

body.push(H2('8.2 Where the cloud passwords live'));
body.push(...PIC2('cloudjson', 'Picture 22. cloud.json: the desktop app\'s cloud settings (sample, passwords hidden)'));
body.push(L('Where', "In my **user folder** (`C:\\Users\\<me>\\.xpera\\cloud.json`), **not in the project**. So the passwords are never in the code, never in a screenshot of Visual Studio, and never in a zip I hand in."));
body.push(L('Line 3', "`CloudMaster`: how to open the **cloud Master** (db77928)."));
body.push(L('Lines 5\u20139', "`TenantCredentials`: the cloud login of each store, in the **same format** as the API\'s `appsettings.json`, so the teacher\'s `TenantDbContextFactory` can read it unchanged."));

body.push(H2('8.3 Trip 3: clicking Back up now'));
body.push(...PIC2('bkwire', 'Picture 23. MainForm.cs: connecting the sidebar parts'));
body.push(L('Line 154', "Only the **Admin** sees the Back up now row."));
body.push(L('Line 155', "Clicking it runs `BackUpNow()` (Picture 24)."));
body.push(L('Line 156', "Clicking the sign-out icon on the profile card runs `SignOut(...)` (Picture 26)."));
body.push(L('Line 157', "Shows the line under the button: **Last backup · …**, **Failed · …** or **Not backed up yet**."));

body.push(...PIC2('bkrun', 'Picture 24. MainForm.cs: RunBackup() and BackUpNow()'));
body.push(L('Lines 216\u2013218', "Remembers which store is signed in and turns the row into **Backing up…**."));
body.push(L('Lines 219\u2013220', "Opens the **Backing up** pop-up (Picture 25) and gives it the work to do: `CloudBackup.RunAsync(store)`."));
body.push(L('Lines 221\u2013223', "When it is done: refresh the sidebar line and hand back the result."));
body.push(L('Lines 228\u2013232', "`BackUpNow()` runs it and shows **Backed up** (with the number of rows) or **Not backed up** (with the reason)."));

body.push(...PIC2('bkdialog', 'Picture 25. BackupDialog.cs: the "Backing up…" pop-up'));
body.push(L('Line 35', "`Task.Run`: the copy runs on a **background thread**, so the window keeps drawing and does not look frozen while it waits for the internet."));
body.push(L('Lines 36\u201337', "When the copy is done, the pop-up closes by itself."));
body.push(L('Lines 41\u201345', "The × and Esc do nothing **until it is done**. The copy is one transaction, so it must finish (or fail) on its own."));

body.push(H2('8.4 Trip 4: signing out'));
body.push(...PIC2('bksignout', 'Picture 26. MainForm.cs: SignOut() and the window\'s ×'));
body.push(L('Lines 239\u2013240', "`_leaving` makes sure it only runs **once**, even if someone clicks twice."));
body.push(L('Line 241', "Backs up only if it should (Picture 28, lines 70\u201376): cloud.json exists, and the last try did not fail with a **wrong password**."));
body.push(L('Lines 243\u2013245', "Runs the backup. If it did not work, it says why, and that **the data is still safe on this PC**. It signs out anyway."));
body.push(L('Lines 247\u2013249', "Remembers whether this was a **sign-out** or **closing xpera**, forgets who was signed in (`Session.Clear()`), and closes the main window."));
body.push(L('Lines 253\u2013262', "Clicking the window\'s **×**: instead of closing at once, it cancels the close and calls `SignOut(exitApp: true)`, so closing xpera also backs up."));

body.push(...PIC2('bklogin', 'Picture 27. LoginForm.cs: what happens after the main window closes'));
body.push(L('Line 132', "Closed with the **×** \u2192 the whole app ends."));
body.push(L('Lines 133\u2013135', "**Signed out** \u2192 the sign-in window comes back, with the password cleared and the cursor in Username. The store code stays filled in."));

body.push(H2('8.5 The copy itself (Xpera.infrastructure)'));
body.push(...PIC2('bkentry', 'Picture 28. CloudBackup.cs: RunAsync() and ShouldRunOnSignOut()'));
body.push(L('Line 50', "Does the real work in `CopyAsync` (Pictures 29 and 30)."));
body.push(L('Lines 52\u201363', "Turns errors into plain reasons: **wrong password** (error 18456), **no internet**, or anything else."));
body.push(L('Line 64', "Saves the result in a small file per store (`%LOCALAPPDATA%\\Xpera\\backup-PIXEL01.txt`). That is where **Last backup** comes from."));
body.push(L('Lines 70\u201376', "After a **wrong password**, signing out does **not** try again until cloud.json is changed. MonsterASP blocks a PC after repeated failed logins."));

body.push(...PIC2('bkfind', 'Picture 29. CloudBackup.cs: finding and opening the cloud database'));
body.push(L('Lines 80\u201386', "Reads `cloud.json` with `ConfigurationBuilder`, the same way ASP.NET reads `appsettings.json`."));
body.push(L('Lines 89\u201390', "A **4-second internet check** first, so signing out without internet does not hang."));
body.push(L('Lines 93\u201397', "Opens the **cloud Master** and finds the store by its code (PIXEL01)."));
body.push(L('Lines 100\u2013101', "**The teacher\'s resolver and factory** (Session 3) open that store\'s cloud database with its cloud login. The same code the API uses."));
body.push(L('Lines 106\u2013109', "Opens both connections **by hand, one login try each**. (EF\'s own migration check retries a failed login for about a minute, which on MonsterASP would count as many wrong passwords. I found this while testing.)"));
body.push(L('Lines 112\u2013116', "Both databases must have the **same last migration** (same tables). If the cloud is behind, it says to run `Update-Database` first."));

body.push(...PIC2('bkcopy', 'Picture 30. CloudBackup.cs: replacing the cloud copy in one transaction'));
body.push(L('Line 122', "**One transaction**: either the whole copy is saved, or the cloud stays exactly as it was."));
body.push(L('Lines 124\u2013128', "Empties the cloud tables, **children first** (SaleItems before Sales before Customers), so no foreign key complains."));
body.push(L('Lines 130\u2013147', "Copies every table, **parents first**, with `SqlBulkCopy`: it streams all rows in one go, which is much faster than one INSERT per row. `KeepIdentity` keeps the **same IDs**, so a receipt points to the same sale in both places."));
body.push(L('Lines 150\u2013153', "Moves the cloud\'s ID counter past the copied rows, so a new row there would not clash."));
body.push(L('Line 156', "`Commit`: the cloud copy is now an exact copy of local. Running it again replaces it again, so deleted rows also disappear from the cloud."));

body.push(...PIC2('bkorder', 'Picture 31. CloudBackup.cs: which table goes first'));
body.push(L('Line 169', "Asks the **DbContext** for every table it knows (the views are skipped)."));
body.push(L('Lines 171\u2013178', "Repeats: take a table whose parents (foreign keys) are **already in the list**. That gives Users and Products and Customers first, then Sales, then SaleItems."));
body.push(L('Why', "Nothing is typed by hand, so the **Lab 2 tables** (campaigns) will be backed up automatically once their migration exists."));

body.push(H2('8.6 What can go wrong, and what xpera does'));
body.push(table(['Situation', 'What the user sees', 'Data'], [
  ['No internet', '"Can\'t reach the cloud right now…" after about 4 seconds; sidebar: **Failed · no internet**', 'Safe on the PC; next sign-out tries again'],
  ['Wrong cloud password', '"The cloud refused the login…"; sidebar: **Failed · login refused**', 'Safe; no automatic retry until cloud.json is fixed'],
  ['Cloud missing a migration', '"…doesn\'t have the newest tables yet. Run Update-Database…"', 'Cloud not touched'],
  ['cloud.json missing', 'Sidebar: **Not set up on this PC**', 'Signing out works as before'],
  ['Error halfway through', '"The backup stopped: …"', 'Transaction rolled back: the cloud keeps the last good copy'],
], [2500, 4300, 2560]));

body.push(H2('8.7 How I tested it'));
body.push(B('On a **test copy** (a second LocalDB acting as the "cloud", with a SQL login like MonsterASP): 635 rows in 9 tables, every table **identical by checksum** (IDs included), the views work in the copy, and running it twice gives the same result.'));
body.push(B('A new customer and a deleted feedback showed up in the copy after the next backup.'));
body.push(B('Wrong password: **one** login try, fails in about 1 second. No internet: fails in 4 seconds. Missing migration and missing cloud.json: clear messages.'));
body.push(B('Then the real one: **Back up now** on PIXEL01 → db79610 on MonsterASP.'));

body.push(H2('8.8 The whole backup trip, in one list'));
['The owner clicks **Back up now**, or anyone **signs out** (Pictures 23, 26).',
 'The **Backing up** pop-up runs the copy in the background (Pictures 24, 25).',
 '`CloudBackup` reads `cloud.json` and checks the internet (Picture 29).',
 'The **cloud Master** says which cloud database is PIXEL01\'s: db79610, key CloudTenantA.',
 'The **teacher\'s factory** opens db79610 with the CloudTenantA login.',
 'Same migration on both? Then, in **one transaction**: empty the cloud tables, copy every local table, commit (Pictures 30, 31).',
 'The sidebar shows **Last backup · 1:05 PM**; after a sign-out, the sign-in window comes back (Picture 27).',
].forEach(s => body.push(N(s, 'trip2')));
body.push(SAY('"The backup reuses the same multi-tenant pieces as the API: the cloud Master finds the store\'s cloud database, the tenant factory opens it with that store\'s own login, and the copy is one transaction, so the cloud always holds a complete copy."'));

// 9
body.push(H1('9. How I did the migrations, step by step'));
body.push(H2('Step 1. Write the entities'));
body.push(P('A class per table in `Xpera.domain/entities`, for example:'));
body.push(...CODE(`public class Company
{
    public int CompanyId { get; set; }
    public string CompanyCode { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}`));
body.push(H2('Step 2. Write the DbContext'));
body.push(P('List each table as a `DbSet`, then set the rules in `OnModelCreating`:'));
body.push(...CODE(`public DbSet<Company> Companies => Set<Company>();

builder.Entity<Company>(entity =>
{
    entity.HasKey(x => x.CompanyId);                 // primary key
    entity.Property(x => x.CompanyCode).HasMaxLength(50).IsRequired();
    entity.HasIndex(x => x.CompanyCode).IsUnique();  // no two stores with the same code
});`));
body.push(H2('Step 3. Add-Migration (write the change as C#)'));
body.push(P('Package Manager Console, **Default project = Xpera.infrastructure**:'));
body.push(...CODE(`Add-Migration InitialMasterCrm -Context MasterCrmDbContext -StartupProject Xpera.api
Add-Migration AddPlansAndSubscriptions -Context MasterCrmDbContext -StartupProject Xpera.api
Add-Migration InitialTenantCrm -Context TenantCrmDbContext -StartupProject Xpera.api -OutputDir Migrations/Tenant`));
body.push(P('EF compares the classes with the last snapshot and writes a migration file. Its `Up()` method holds `CreateTable(...)` for each new table; `Down()` undoes it.'));
body.push(H2('Step 4. Update-Database (run it on SQL Server)'));
body.push(...CODE(`Update-Database -Context MasterCrmDbContext -StartupProject Xpera.api
Update-Database -Context TenantCrmDbContext -StartupProject Xpera.api -Args "--Tenant TenantA"
Update-Database -Context TenantCrmDbContext -StartupProject Xpera.api -Args "--Tenant TenantB"
Update-Database -Context TenantCrmDbContext -StartupProject Xpera.api -Args "--Tenant TenantC"`));
body.push(B('EF creates the database if it does not exist, runs each migration, and writes it into **__EFMigrationsHistory**, so it never runs twice.'));
body.push(B('The three tenant lines are the same command with a different store name. `-Args "--Tenant TenantB"` makes Program.cs use the `TenantB` line in appsettings.json, so the **same** migration is built into each store database. (Leaving out `-Args` also means TenantA, because TenantA is the default in Program.cs.) The first time, we typed the full connection string with `-Connection "..."`; that still works, but the names are shorter.'));
body.push(H2('Step 5. Fill the tables with sample data (SSMS)'));
['Open the script with **File → Open → File** (Ctrl + O). A newly opened file starts **disconnected**: click the plug icon and connect to `(localdb)\\MSSQLLocalDB` (Windows Authentication, Trust Server Certificate).',
 'Pick the database in the toolbar dropdown next to Execute, then press **Execute**.',
 '`01-master-data.sql` on **Xpera_Master**.',
 '`03-store-sample-data.sql` on **Xpera_TenantA**, then B, then C.',
 '`04-store-transactions.sql` on **Xpera_TenantA**, then B, then C (always after 03).',
 'Check: `SELECT COUNT(*) FROM Sales` on TenantA gave **210** sales (with 53 payments and 40 customers).',
].forEach(s => body.push(N(s, 'seed')));
body.push(H2('Step 6. One login per database (SSMS)'));
body.push(P('In a New Query on `master` I made a SQL login for each database and allowed it into **only** that database:'));
body.push(...CODE(`CREATE LOGIN xpera_tenantA_login WITH PASSWORD = '<my password>', CHECK_POLICY = OFF;
USE Xpera_TenantA;
CREATE USER xpera_tenantA_login FOR LOGIN xpera_tenantA_login;
ALTER ROLE db_owner ADD MEMBER xpera_tenantA_login;`));
body.push(P('Then each **Registered Server** (MasterXpera, TenantA_Xpera, TenantB_Xpera, TenantC_Xpera) got: Server name `(localdb)\\MSSQLLocalDB`, **SQL Server Authentication** with its login, **Connect to database** = its own database, and Trust server certificate ticked. The registered server name is only a nickname for the list.'));
body.push(P('**Test:** connected as TenantA_Xpera, opening Xpera_TenantB says **"The database Xpera_TenantB is not accessible."** That is tenant isolation working.'));
body.push(SAY('"A login gets you into the server; a user inside a database lets that login into that one database. TenantA\'s login simply has no user in TenantB, so it is refused."'));
body.push(H2('Step 7. Run the app'));
body.push(P('Right-click **Xpera** → Debug → Start New Instance, then sign in with `PIXEL01` / `owner` / `xpera123`. The sidebar shows **PixelZone · TENANT A · MICRO**, read from the new Master.'));

// 9
body.push(H1('10. Problems I hit, and what they taught me'));
body.push(table(['What happened', 'Why', 'Fix / lesson'], [
  ['"Your startup project \'Xpera\' doesn\'t reference Microsoft.EntityFrameworkCore.Design"', 'EF commands run through the startup project. Xpera (WinForms) has no Design package, on purpose.', 'Add `-StartupProject Xpera.api`, or set Xpera.api as the startup project while running EF commands.'],
  ['"There is already an object named \'Plans\'"', 'SQL Server ignores upper/lower case in names, so `Xpera_Master` was the same as my old `xpera_master`, which already had a Plans table.', 'Delete the old databases first. With migrations, **EF must own the database**; never make the same tables by hand.'],
  ['SSMS "network path was not found" (error 53)', 'The registered servers pointed to a server name this PC does not have.', 'Use `(localdb)\\MSSQLLocalDB`, the SQL Server that comes with Visual Studio.'],
  ['The database dropdown was greyed out', 'A file opened with Ctrl + O starts disconnected; each tab has its own connection.', 'Click the plug icon to connect that tab once.'],
  ['TenantA\'s login can see the names of other databases', 'SQL Server shows every login the list of names, but not what is inside.', 'Opening one gives "not accessible". Seeing the door is not opening it.'],
  ['The first store got CompanyId **0**', 'My seed script reset the ID counter with RESEED 0. On a table that never had rows, that makes the first ID 0.', 'The script now resets only tables that had rows before. IDs are 1, 2, 3 again.'],
  ['A wrong cloud password took a full minute to fail', 'The EF Core migration check quietly retries a failed login for about a minute (many wrong logins on MonsterASP).', 'The backup opens each connection once and reads __EFMigrationsHistory itself: one try, about 1 second.'],
], [2900, 3300, 3160]));

// 9
body.push(H1('11. Command cheat sheet'));
body.push(table(['Command (Package Manager Console)', 'What it does'], [
  ['`Add-Migration <Name> -Context <X> -StartupProject Xpera.api`', 'Writes a new migration from the changes in the entities and DbContext.'],
  ['`Update-Database -Context <X> -StartupProject Xpera.api`', 'Runs every migration not yet in __EFMigrationsHistory.'],
  ['`… -Args "--Tenant TenantB"`', 'Picks the store database (TenantA, TenantB or TenantC), using that line in appsettings.json.'],
  ['`Remove-Migration -Context <X> -StartupProject Xpera.api`', 'Deletes the last migration if it was not applied yet.'],
  ['`Drop-Database -Context <X> -StartupProject Xpera.api`', 'Deletes that database completely (asks first).'],
  ['`Get-Migration -Context <X> -StartupProject Xpera.api`', 'Lists the migrations and which ones are applied.'],
], [4900, 4460]));
body.push(H2('Adding a new table later (for example Lab 2 retention campaigns)'));
['Add the entity class in `Xpera.domain/entities` (e.g. `Campaign.cs`).',
 'Add `public DbSet<Campaign> Campaigns => Set<Campaign>();` and its rules in `TenantCrmDbContext`.',
 '`Add-Migration AddCampaigns -Context TenantCrmDbContext -StartupProject Xpera.api -OutputDir Migrations/Tenant`',
 '`Update-Database ... -Args "--Tenant TenantA"`, then the same with TenantB and TenantC.',
 'Refresh SSMS: the new table is there, in every store.',
].forEach(s => body.push(N(s, 'addtable')));

// 10
body.push(H1('12. Questions the teacher may ask'));
body.push(table(['Question', 'Answer'], [
  ['What is a migration?', 'C# code that describes a change to the database. Add-Migration writes it; Update-Database runs it. The database can be rebuilt from code at any time.'],
  ['What is a DbContext?', 'The class that connects my entities to the database: which tables exist and their rules. I have two: MasterCrmDbContext and TenantCrmDbContext.'],
  ['Why two DbContexts?', 'The Master and the stores hold different tables. One store migration builds every store\'s database.'],
  ['What is __EFMigrationsHistory?', 'EF\'s list of migrations already applied, so it never runs one twice.'],
  ['Why is the API the startup project for migrations?', 'EF reads the connection string and services from the startup project. The API has appsettings.json, Program.cs (AddDbContext), and the Design package.'],
  ['Why IdentityDbContext?', 'It adds ready-made, secure login tables (AspNetUsers…). The Super Admin signs in through them, and passwords are hashed by Identity.'],
  ['How are tenants kept apart?', 'Each store has its own database, the Master says which one, and each SQL login can only open its own database.'],
  ['Where is the data access?', 'Only in Xpera.infrastructure, and all of it goes through the two DbContexts with LINQ. EF Core sends every value as a parameter, so typed text can never become SQL. The UI and the API never write SQL themselves.'],
  ['How do you read the balance and summary views?', 'They are views made by the migration, so StoreDb reads them with `db.Database.SqlQuery<CustomerBalanceRow>(...)`. A view is not a table, so it is not a DbSet and needs no migration of its own.'],
  ['Is the cloud database your main database?', 'No. Each store works on its own PC (LocalDB); the cloud is its backup. The copy goes one way, local to cloud, when the owner clicks Back up now or anyone signs out.'],
  ['What if there is no internet when they sign out?', 'xpera checks for 4 seconds, says it could not back up, and signs out anyway. The data is safe on the PC and the next sign-out tries again.'],
  ['Can a backup leave the cloud half-copied?', 'No. The whole copy is one transaction: it commits completely or rolls back, so the cloud always keeps the last complete copy.'],
  ['Where are the cloud passwords?', 'In cloud.json in my user folder, not in the project. The database only stores a key name like CloudTenantA, never a password.'],
  ['Why not let EF check the migrations?', 'EF retries a failed login for about a minute. On MonsterASP that counts as many wrong passwords and can block the PC, so xpera opens each connection once and reads __EFMigrationsHistory itself.'],
  ['Why does the domain have no packages?', 'It only describes things. Keeping it free of database and UI code means it can be reused anywhere.'],
], [3200, 6160]));

// ---------- document ----------
const numbered = (ref) => ({ reference: ref, levels: [{ level: 0, format: LevelFormat.DECIMAL, text: '%1.', alignment: AlignmentType.LEFT, style: { paragraph: { indent: { left: 440, hanging: 300 } } } }] });
const doc = new Document({
  creator: 'Nicho Lescano', title: 'xpera CRM Project Structure and Migrations Guide',
  styles: {
    default: { document: { run: { font: 'Arial', size: 21, color: INK } } },
    paragraphStyles: [
      { id: 'Heading1', name: 'Heading 1', basedOn: 'Normal', next: 'Normal', quickFormat: true,
        run: { font: 'Arial', size: 30, bold: true, color: INK }, paragraph: { spacing: { before: 300, after: 140 }, keepNext: true, outlineLevel: 0 } },
      { id: 'Heading2', name: 'Heading 2', basedOn: 'Normal', next: 'Normal', quickFormat: true,
        run: { font: 'Arial', size: 24, bold: true, color: RED }, paragraph: { spacing: { before: 220, after: 80 }, keepNext: true, outlineLevel: 1 } },
    ],
  },
  numbering: { config: [
    { reference: 'bullets', levels: [{ level: 0, format: LevelFormat.BULLET, text: '•', alignment: AlignmentType.LEFT, style: { paragraph: { indent: { left: 440, hanging: 260 } } } }] },
    numbered('signin'), numbered('seed'), numbered('addtable'), numbered('trip'), numbered('trip2'),
  ] },
  sections: [{
    properties: { page: { size: { width: 11906, height: 16838 }, margin: { top: 1134, bottom: 1134, left: 1134, right: 1134 } } },
    footers: { default: new Footer({ children: [new Paragraph({ alignment: AlignmentType.RIGHT, children: [new TextRun({ text: 'xpera CRM · Structure and migrations guide · page ', size: 16, color: GRAY }), new TextRun({ children: [PageNumber.CURRENT], size: 16, color: GRAY })] })] }) },
    children: body,
  }],
});
Packer.toBuffer(doc).then(buf => {
  fs.writeFileSync(process.argv[2], buf);
  console.log('written', process.argv[2]);
});
