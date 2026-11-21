const fs = require('fs');
const {
  Document, Packer, Paragraph, TextRun, Table, TableRow, TableCell, WidthType, ShadingType,
  HeadingLevel, AlignmentType, LevelFormat, BorderStyle, PageBreak, Footer, PageNumber,
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

// ---------- content ----------
const body = [];

body.push(new Paragraph({ alignment: AlignmentType.LEFT, spacing: { after: 60 }, children: [new TextRun({ text: 'xpera CRM · Lab 1', size: 44, bold: true, color: INK })] }));
body.push(new Paragraph({ spacing: { after: 60 }, children: [new TextRun({ text: 'Presentation guide: what each page does, how it works, and how to explain it', size: 24, color: GRAY })] }));
body.push(new Paragraph({ spacing: { after: 240 }, border: { bottom: { style: BorderStyle.SINGLE, size: 8, color: RED, space: 6 } }, children: [new TextRun({ text: 'Nicho Lescano · BSIT 3 · IT13L (3462) · Instructor: Renz Arriola', size: 20, color: GRAY })] }));

// 1
body.push(H1('1. What xpera is, in one minute'));
body.push(P('xpera is a desktop CRM (customer relationship management system) for small stores that sell gaming PC parts. It helps a store remember every customer: who they are, what they buy, what they owe, and what they asked or complained about.'));
body.push(P('xpera is sold as a subscription. Each store that subscribes is a **tenant**, and every tenant gets **its own database**, so one store can never see another store\'s data. There is also a **Master** database that knows every store and its plan.'));
body.push(P('**Lab 1 = Tenant A (micro store):** the main transactions (customers, sales, payments) and data collection (inquiries, complaints, feedback). Lab 2 adds the dashboard, reports, and retention; the Final adds branches and the Super Admin panel.'));
body.push(SAY('"xpera helps a gaming parts store remember every customer. Lab 1 is the base plan: the store can record sales and payments under a customer, and collect their inquiries, complaints, and feedback."'));

// 2
body.push(H1('2. Before the demo (5 minutes)'));
body.push(N('Open `code\\Xpera.slnx` in Visual Studio 2026.'));
body.push(N('If you want fresh sample data, run the setup once in your own PowerShell (it erases data you typed): `powershell -ExecutionPolicy Bypass -File "...\\code\\Xpera.infrastructure\\Database\\setup.ps1"`'));
body.push(N('**For the backup part:** make sure the PC is online. The cloud settings are already in `C:\\Users\\<me>\\.xpera\\cloud.json` (never open it on the projector: it has passwords). Close any file with passwords, like the MonsterASP notes.'));
body.push(N('Set **Xpera** (not Xpera.api) as the startup project, then press **F5**. The sign-in window opens.'));
body.push(N('Sign in with store code `PIXEL01`, username `owner`, password `xpera123`.'));
body.push(gap());
body.push(table(['Store code', 'Username', 'Role', 'What they see'], [
  ['PIXEL01', 'owner', 'Admin (store owner)', 'Every screen'],
  ['PIXEL01', 'cashier01', 'Cashier', 'Sales, Payments, Customers'],
  ['PIXEL01', 'staff01 / staff02', 'Staff', 'Products, Customers, Inquiries, Complaints, Feedback'],
], [1600, 1900, 2200, 3660]));
body.push(P('All sample passwords are `xpera123` (test accounts only). Each store has 40 customers and 265 transactions (210 sales + 55 payments), more than the 200 the project needs.', { spacing: { before: 120, after: 120 } }));

// 3
body.push(pageBreak());
body.push(H1('3. Screen by screen'));

body.push(H2('Sign in'));
body.push(B('Type the **store code** first. xpera looks it up in the Master database and shows the store\'s name under the logo. The code tells xpera **which store\'s database** to open.'));
body.push(B('Then the username and password. The password is never saved as text: the database keeps only a scrambled version (a SHA-256 **hash**).'));
body.push(B('Three wrong passwords **lock** the account. The last store code is remembered for next time.'));
body.push(SAY('"The store code picks the tenant. That is how one app serves many stores, each with its own private database."'));

body.push(H2('The main window'));
body.push(B('**Sidebar (left):** the menu, grouped as Sell, Customer care, and Store. The open page has a red bar. The menu only shows pages your **role** may use.'));
body.push(B('**Bottom of the sidebar:** **Back up now** with the last backup time (Admin only), the Dark mode switch, then a card with who is signed in, which store and plan this is, and a small **sign-out** arrow in its corner.'));
body.push(B('**Header (right):** breadcrumb (Store › Group › Page), the page title, a one-line description, and the page\'s main buttons on the right.'));
body.push(B('**Number tiles:** most lists start with four tiles that answer the first question on that page (for example, how much is owed and how much is overdue).'));
body.push(B('**Lists:** search box, 8 rows per page, and a short pager like ‹ Previous 1 2 3 … 27 Next ›. The page scrolls when the window is small.'));

body.push(H2('Sales (Cashier, Admin) · UC-04 Record Sale'));
body.push(N('**Pick the customer first.** Click the customer card (or Change) to search by name. The card shows what they spent, their last visit, and their balance (red if they owe).', 'sales'));
body.push(N('**Add items.** Use the Quick add chips or "+ Add item by name". Products that are not available are grayed out and cannot be sold.', 'sales'));
body.push(N('**Adjust the cart** with −, +, and ×. Subtotal and total update by themselves.', 'sales'));
body.push(N('**Choose Cash or Credit.** Credit asks for a due date and shows how much it adds to the customer\'s balance.', 'sales'));
body.push(N('**Save sale.** The receipt number (OR-000480 and up) is given automatically, and a credit sale appears on the Payments page.', 'sales'));
body.push(P('The **Sales history** tab lists every sale, newest first, with search and pages.'));
body.push(SAY('"Every sale goes under a customer. That is what makes it a CRM and not just a cash register."'));

body.push(H2('Payments (Cashier, Admin) · UC-05 Record Payment'));
body.push(B('Lists everyone who still owes from credit sales, overdue ones first. Tiles show the total owed and how much is overdue.'));
body.push(B('**+ Record payment:** pick the customer (their balance and due date show under the field), type the amount, save. You cannot pay more than they owe; the form says so in red.'));
body.push(B('The balance is **calculated**, not typed: credit sales minus payments. Payments pay off the oldest sale first.'));

body.push(H2('Products (Staff, Admin) · UC-02 Manage Products'));
body.push(B('Name, category, price, and **Available / Not available**. There are no stock counts on purpose (that is inventory, an ERP feature).'));
body.push(B('Click the availability tag to change it. When an item becomes available again, xpera tells you how many customers were waiting for it.'));

body.push(H2('Customers (everyone) · UC-03 Manage Customer Records'));
body.push(B('Name, contact, what they usually buy, total spent, last visit. Tiles show total customers, total spent, average spend, and the top customer.'));
body.push(B('**+ Add customer** opens a short form.'));

body.push(H2('Inquiries (Staff, Admin) · UC-06 Manage Inquiries'));
body.push(B('A customer\'s question, linked to a product if they asked about one. If that product is not available, the inquiry is saved as **Waiting for product**, so staff can follow up when it comes back.'));
body.push(B('Change the status right in the list: Open, In progress, Waiting for product, Resolved.'));

body.push(H2('Complaints (Staff, Admin) · UC-07 Manage Complaints'));
body.push(B('Complaints and **warranty claims**, moved from Open to In progress to Resolved.'));

body.push(H2('Feedback (Staff, Admin) · UC-08 Record Feedback'));
body.push(B('A 1–5 star rating and a comment after a visit. Tiles show the average rating and how many were unhappy. In Lab 2 this becomes the satisfaction graph.'));

body.push(H2('Users (Admin) · UC-01 Manage Staff Accounts'));
body.push(B('The Admin adds Cashier and Staff accounts with a temporary password. Tiles show how many accounts are locked.'));

body.push(H2('Back up now and Sign out (Admin, everyone) · cloud backup'));
body.push(P('Each store works on **its own PC** (LocalDB), so xpera keeps working without internet. Each store also has a database on **MonsterASP**. xpera copies the store\'s data **up** to it, one way (local → cloud), as a backup.'));
body.push(B('**Back up now** (Admin, bottom of the sidebar): a "Backing up to the cloud" pop-up, then **Backed up: 6xx rows in 9 tables**. The line under the button changes to **Last backup · 1:05 PM**. Hover over it for the full date and row count.'));
body.push(B('**Sign out** (the arrow on the profile card, any role): backs up first, then the sign-in window comes back. Closing the window with **×** also backs up, then closes xpera.'));
body.push(B('**No internet?** It says so after about 4 seconds and still signs out. The data is safe on the PC, and the next sign-out tries again. The sidebar shows **Failed · no internet** in red.'));
body.push(B('**All or nothing:** the copy is one transaction. If anything fails halfway, the cloud keeps its last complete copy.'));
body.push(SAY('"xpera works offline on the store\'s PC. When the owner clicks Back up now, or anyone signs out, it copies that store\'s data to its own cloud database on MonsterASP. If the internet is down, nothing breaks: it tries again next time."'));

body.push(H2('All pop-ups'));
body.push(B('Every form and message uses the same xpera pop-up: title, short description, × to close, Esc to cancel. The window behind dims.'));
body.push(B('Mistakes show **inside the form** in red ("Fill in amount paid."), so nothing you typed is lost.'));

// 4
body.push(pageBreak());
body.push(H1('4. How it works behind the screen'));
body.push(P('Think of it as three layers. Each layer only talks to the one below it:'));
body.push(table(['Layer', 'Files', 'Job'], [
  ['1. Screens (what you see)', 'Xpera: LoginForm, MainForm, Screens\\*.cs, Controls\\*.cs', 'Show data and buttons; react to clicks.'],
  ['2. Data actions', 'Xpera.infrastructure\\data: StoreDb.cs, MasterDb.cs; services\\CloudBackup.cs', 'One method per action, like SaveSale or RecordPayment, through the EF Core DbContexts. Screens never write SQL.'],
  ['3. Database connection', 'Xpera.infrastructure\\data: Db.cs, DbContexts.cs; services\\ (resolver + factory)', 'Opens the right database for the signed-in store.'],
  ['Database', 'SQL Server (LocalDB): Xpera_Master + Xpera_TenantA/B/C; backup copies on MonsterASP', 'Built by EF Core migrations (Update-Database).'],
], [2300, 3400, 3660]));
body.push(gap());
body.push(H2('What happens when the cashier clicks Save sale'));
body.push(N('`SalesScreen` collects the customer, the items, and Cash or Credit.', 'save'));
body.push(N('It calls `StoreDb.SaveSale(...)`.', 'save'));
body.push(N('`SaveSale` gets the next receipt number and saves the sale and all its items inside one **transaction**: either everything is saved, or nothing is (no half-saved sales).', 'save'));
body.push(N('The screen clears the cart, refreshes the customer card, and shows "Sale saved".', 'save'));
body.push(H2('Lab 1 tables (in each store\'s database)'));
body.push(P('**Users, Products, Customers, Sales, SaleItems, Payments, Inquiries, Complaints, Feedback.** Two **views** do the math so nothing is stored twice: `CustomerBalances` (what each customer owes and when it is due) and `CustomerSummary` (total spent and last visit).'));
body.push(H2('Security, in plain words'));
body.push(B('**Parameterized queries:** typed text is sent separately from the SQL, so a trick like `\'; DROP TABLE` is just text (stops SQL injection).'));
body.push(B('**Hashed passwords** and **account lockout** after 3 wrong tries.'));
body.push(B('**Roles:** each role only sees its own pages. **Tenants:** each store has its own database.'));

// 5
body.push(H1('5. "Is this drag and drop?"'));
body.push(P('**Both.** Visual Studio has two halves for every window or screen:'));
body.push(table(['File', 'Made by', 'What it holds'], [
  ['`SalesScreen.Designer.cs`', 'The Visual Studio Designer (drag and drop)', 'Where each panel, label, and button sits, and its size.'],
  ['`SalesScreen.cs`', 'Me, by writing code', 'What happens: loading data, clicks, totals, saving.'],
], [2900, 3000, 3460]));
body.push(gap());
body.push(P('They are two parts of **one class** (a `partial class`). The layout was placed with the Designer, then the behavior was written in code.'));
body.push(P('Some parts are **custom controls written in code**, because the default Windows controls cannot look like the xpera design: rounded cards and buttons (`Card`, `RoundButton`), rounded text fields (`InputBox`), the searchable picker (`PickerPopup`), the number tiles (`StatStrip`), and the sidebar items with icons (`MenuButton`). Once built, they appear in the Designer\'s Toolbox and can be dragged in like any other control.'));
body.push(SAY('"The screens are laid out with the Designer, which is drag and drop. Things Windows can\'t draw by default, like the rounded buttons and the number tiles, I made as my own controls in C#, and they can be dragged in like normal controls."'));

// 6
body.push(H1('6. C# vs VB.NET'));
body.push(P('Both are Microsoft languages for the **same .NET platform**. They use the same Windows Forms, the same Designer, and the same SQL Server tools. The difference is mostly **how the code is written**:'));
body.push(table(['', 'C# (what xpera uses)', 'VB.NET'], [
  ['Blocks', 'Braces `{ }`', 'Words: `End If`, `End Sub`'],
  ['End of a line', 'Semicolon `;`', 'Nothing'],
  ['If', '`if (total > 0) { ... }`', '`If total > 0 Then ... End If`'],
  ['Inheritance', '`class Card : Panel`', '`Class Card` + `Inherits Panel`'],
  ['This object', '`this`', '`Me`'],
  ['Comments', '`// note`', '`\' note`'],
  ['Upper/lower case', 'Matters (`Total` ≠ `total`)', 'Does not matter'],
  ['Feels like', 'Java, C++, JavaScript', 'Plain English, older BASIC'],
], [1900, 3730, 3730]));
body.push(gap());
body.push(P('**Why C#:** it is the main language of .NET today (most new examples, tutorials, and jobs use it), and it looks like Java and C++, which are taught in BSIT. Both would work; C# simply has more support.'));

// 7
body.push(pageBreak());
body.push(H1('7. OOP in xpera (with real examples)'));
body.push(P('OOP (object-oriented programming) means building the program from **classes**: blueprints that hold data and the actions on that data. Here is where each idea appears in the code.'));
body.push(table(['OOP idea', 'Plain meaning', 'Where it is in xpera'], [
  ['Class and object', 'A blueprint, and a real thing made from it.', '`LoginForm`, `MainForm`, and the 8 screens are classes. `new SalesScreen()` makes an object.'],
  ['Encapsulation', 'Keep the data private; let others use it only through safe methods or properties.', '`Palette.cs` keeps colors in **private fields** with **getters and setters**. `Tints.cs` uses `{ get; private set; }` so colors can be read but not changed from outside. `EntryForm` keeps its fields private and shares only `Values` and `ChoiceIndex`.'],
  ['Inheritance', 'A new class reuses an existing one and adds to it.', '`RoundButton : Button`, `Card : Panel`, `SearchBox : InputBox` (a text box plus a magnifying glass), `EntryForm : ModalForm` and `Notice : ModalForm` (they share one pop-up frame).'],
  ['Polymorphism', 'Different classes answer the same call in their own way.', 'Each control **overrides** `OnPaint` to draw itself. `MainForm` treats every screen the same through **interfaces**: `IHeaderActions` (screens give their header buttons), `IThemeAware`, `IContentHeight`.'],
  ['Abstraction', 'Hide the details behind a simple name.', 'Screens call `StoreDb.RecordPayment(customer, amount)` and never see the SQL. `Db.cs` hides how the connection is opened.'],
  ['Interfaces', 'A promise of what a class can do.', '`IHeaderActions`, `IThemeAware`, `IContentHeight` in `MainForm.cs`.'],
  ['Static classes', 'Shared tools; no object needed.', '`Ui` (styles), `Typography` (fonts), `Sizes`, `Choices`, `StoreDb`, `Db`.'],
  ['Enums', 'A fixed list of named choices.', '`SignInResult` { Ok, WrongPassword, Locked }, `NoticeKind`, `Weight`.'],
  ['Events', 'Code that runs when something happens.', '`button.Click += ...`, the pager\'s `PageChanged` event.'],
], [1800, 2700, 4860]));
body.push(gap());
body.push(SAY('"Encapsulation: my color palette keeps its values in private fields and only shows them through getters and setters. Inheritance: my RoundButton is a normal Windows Button with rounded drawing added. Polymorphism: the main window works with any screen through an interface, so adding a new screen is one line."'));

// 8
body.push(H1('8. Questions he might ask'));
body.push(table(['Question', 'Short answer'], [
  ['Why does each store have its own database?', 'Privacy and safety: one store\'s data can never mix with another\'s. The Master database only keeps the list of stores and plans.'],
  ['How do you stop SQL injection?', 'Every query uses parameters (@name). Typed text is never glued into the SQL.'],
  ['Where are passwords stored?', 'Only as a SHA-256 hash of "username:password". The real password is never saved.'],
  ['Why no stock counts?', 'Inventory is ERP. xpera is a CRM: it tracks customers. Products only say Available or Not available.'],
  ['How is the balance computed?', 'A database view: credit sales minus payments, oldest sale paid first. It is never typed by hand, so it cannot get out of sync.'],
  ['What if the save fails halfway?', 'SaveSale uses a transaction: the sale and its items are saved together or not at all.'],
  ['Can you edit or delete records?', 'Not yet. Lab 1 adds and updates status; edit and delete come next.'],
  ['Why a desktop app?', 'The teacher\'s plan uses C# Windows Forms and SQL Server; a store counter PC runs it directly.'],
  ['Is the cloud your main database?', 'No. Each store works on its own PC; the cloud (MonsterASP) is its backup. The copy goes one way, local to cloud.'],
  ['When does it back up?', 'When the owner clicks Back up now, and whenever anyone signs out or closes xpera. My teacher said the timing was up to my logic: sign-out saves the day\'s work without anyone having to remember.'],
  ['What if the internet is down?', 'It says so after about 4 seconds and still signs out. The data stays safe on the PC and the next sign-out tries again.'],
  ['Can the backup be half done?', 'No. It is one transaction: all tables are copied and committed together, or the cloud keeps its last complete copy.'],
  ['Where are the cloud passwords?', 'In a private cloud.json in my user folder, not in the code or the database. The database only stores a key name like CloudTenantA.'],
  ['What comes in Lab 2?', 'Tenant B: dashboard with 6 graphs, printable reports, and retention campaigns.'],
], [3200, 6160]));

// 9
body.push(H1('9. Demo order (about 3 minutes)'));
['Sign in as `owner` at `PIXEL01`. Point out the store name appearing under the logo.',
 'Show the sidebar groups, the breadcrumb, and dark mode.',
 'Sales: pick a customer, quick-add two items, switch to Credit, save. Read the receipt number.',
 'Payments: show the new balance, record part of it, show the red message if you type too much.',
 'Products: set an item to Not available, then log an inquiry about it on Inquiries (it waits).',
 'Feedback or Complaints: add one, change a status in the list.',
 'Click **Back up now**: show the pop-up, "Backed up", and the new **Last backup** time. (Optional: in SSMS, Cloud_TenantA → `SELECT COUNT(*) FROM Sales` shows the same count as local.)',
 'Click the **sign-out arrow**: it backs up, then the sign-in window comes back. Sign in as `cashier01` to show the shorter menu (roles) and that a cashier has no Back up button.',
].forEach(s => body.push(N(s, 'demo')));

// ---------- document ----------
const numbered = (ref) => ({ reference: ref, levels: [{ level: 0, format: LevelFormat.DECIMAL, text: '%1.', alignment: AlignmentType.LEFT, style: { paragraph: { indent: { left: 440, hanging: 300 } } } }] });
const doc = new Document({
  creator: 'Nicho Lescano', title: 'xpera CRM Lab 1 Presentation Guide',
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
    numbered('steps'), numbered('sales'), numbered('save'), numbered('demo'),
  ] },
  sections: [{
    properties: { page: { size: { width: 11906, height: 16838 }, margin: { top: 1134, bottom: 1134, left: 1134, right: 1134 } } },
    footers: { default: new Footer({ children: [new Paragraph({ alignment: AlignmentType.RIGHT, children: [new TextRun({ text: 'xpera CRM · Lab 1 guide · page ', size: 16, color: GRAY }), new TextRun({ children: [PageNumber.CURRENT], size: 16, color: GRAY })] })] }) },
    children: body,
  }],
});
Packer.toBuffer(doc).then(buf => {
  fs.writeFileSync(process.argv[2], buf);
  console.log('written', process.argv[2]);
});
