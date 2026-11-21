# Presentation: xpera, how it's built and how it works

For the peer-to-peer evaluation and the teacher's check (Nov 19). About 5 minutes of talking + a walkthrough of the app. Speak to the class, not the paper.
**Bold** = say it with emphasis. (Brackets) = what to do on screen.

Sample sign-ins (testing only, all password `xpera123`):

| Store | Plan | Admin | Cashier | Staff |
|---|---|---|---|---|
| PixelZone | Tenant A | `owner` | `cashier01` | `staff01`, `staff02` |
| Arcade Supply | Tenant B | `arcade.owner` | `arcade.cashier01` | `arcade.staff01` |
| Gaming Hub | Tenant C | `hub.owner` | `hub.cashier01` | `hub.staff01` |
| Master | Super Admin | `superadmin` | | |

---

## Before you start (5 minutes before)

- Open Visual Studio with `code/Xpera.slnx`, build once (0 errors).
- Open SSMS with your registered servers (Xpera_Master, Xpera_TenantA/B/C, Cloud_…).
- Check the internet (the backup needs MonsterASP).
- Start xpera and leave it on the login screen.
- Keep `docs/teacher-labs-1-5.md` open in case Sir asks "where is Lab X?".
- Do NOT show `appsettings.json` or `docs/MONSTER CREDENTIALS 4 DB.txt` on the projector (real passwords).

---

## 1. Opening (20 sec)
"Good morning, Sir. I'm Nicho, and this is **xpera**, a CRM for stores that sell gaming PC parts.
**xpera = XP + era**: the era where stores level up by knowing their customers. Our tagline: **Level up every customer.**

Today I'll show how it's **built**, following your labs, and how it **works**, from signing in to backing up to the cloud."

## 2. How it's built: the 4 projects (45 sec)
(Show Solution Explorer.)

"Like in Lab 1, the solution has **four projects**:

- **Xpera** is the desktop app, the screens the store uses.
- **Xpera.domain** holds the **entities**, the shapes of my data: Company, Customer, Sale, and so on.
- **Xpera.infrastructure** talks to the database: the **DbContexts**, the **migrations**, and the **services** folder with the resolver and the factory from Lab 4.
- **Xpera.api** is the Web API with the endpoints from Labs 3 to 5.

The app and the API both use the **same domain and infrastructure**, so the rules are written once."

## 3. Two kinds of databases (45 sec)
(Show SSMS Registered Servers.)

"xpera is **multi-tenant**. Each store is a **tenant** with its **own database**:
PixelZone uses Xpera_TenantA, Arcade Supply TenantB, Gaming Hub TenantC.

Above them is the **Master**. It holds the stores (**Companies**), which database each store uses (**CompanyDatabases** with the **CredentialKey**), the **Plans** and **Subscriptions**, the **Devices**, and the Super Admin.

Every table comes from **code-first migrations**, like in Lab 2. I write the entity and the DbContext, then **Add-Migration** and **Update-Database**. SSMS is only for viewing.

Each database also has its **own SQL login**. The TenantA login **cannot open TenantB**. Even the Master's login can't read a store. That's **tenant isolation**."

## 4. Signing in with only a username (45 sec)
(Show the login screen.)

"There's **no store code**, only a username and password. So how does xpera know which store to open?

The **Master is the phone book**. Its **CompanyUsers** table says which store each username belongs to:
**username → CompanyUsers → CompanyDatabases → that store's database**.
Then the password is checked **inside the store's own Users table**. The Master never holds store passwords.

That's why a username must be **unique across all stores**: PixelZone has `owner`, Arcade Supply has `arcade.owner`, Gaming Hub has `hub.owner`."

(Sign in as `owner`.)

## 5. Saving data: IsSynced and the Activity log (45 sec)
(Add a customer, then open Store › Activity log.)

"The **local database is the main one**, and the cloud on **MonsterASP** is the **backup**.
To know what's not backed up yet, eight tables have **IsSynced**: Users, Products, Customers, Sales, Payments, Inquiries, Complaints, Feedback.

**SaleItems doesn't have it**, because an item can't exist without its sale. They're saved together and backed up together, so the Sale's flag already covers them. Putting it twice would **break normalization**.

Every save marks the record **Not yet synced** in one place, the DbContext's **SaveChanges**, so no screen can forget. The **Activity log** shows what was done, who did it, and whether it's in the cloud. The log doesn't store the status itself; it **reads the record's IsSynced**, so the status lives in one place."

## 6. Backing up to the cloud (30 sec)
(Click **Back up now**.)

"The backup copies this store's database to its **cloud database** on MonsterASP, using the **resolver and factory from Lab 4** with the cloud **CredentialKey**. It's **one transaction**: either everything copies or nothing changes.
Only after it succeeds are the records marked **Synced to cloud**."

(The lines turn to "Synced to cloud".)

"Signing out also backs up by itself, so a cashier's sales still reach the cloud."

## 7. CRUD: Create, Read, Update, Delete (40 sec)
(Open Customers. Click **•••** at the end of a row.)

"Every list has all four: **Create** with the + button, **Read** with the list and search, and **Update** and **Delete** in the **•••** menu at the end of each row, or by **double-clicking** a row to edit it.

Deleting is **safe**. A customer who already has **sales** can't be deleted, because the sales would lose who bought them. The app says why **before** asking 'are you sure?'. A product that was sold can't be deleted either; it's set to **Not available** instead. An account that already has history is **locked**, not deleted, and the Admin can **unlock** it or **reset the password**.

**Sales and payments are never deleted**, on purpose: in a real store, money records stay. Every edit and delete also shows in the **Activity log** as **Not yet synced**, and the next backup removes deleted rows from the cloud too."

## 8. Validation (20 sec)
(Open Customers › Add customer, type more than 11 digits in Mobile number.)

"Inputs are checked. A mobile number **stops at 11 digits** and must start with **09**; names take only letters; usernames and product names can't repeat. The rules live in **one file, Rules.cs**, used by both the app and the API."

## 9. Closing (15 sec)
"So xpera follows your labs: **4 projects, code-first Master and tenant databases, the resolver and factory, the API, and MonsterASP**. On top of that: username-only sign-in, cloud backup with IsSynced, and the Activity log.

Other stores just sell PC parts. With xpera, they **level up every customer**. Thank you, Sir."

---

## Demo order (if Sir asks to see it run)

1. Login → `owner` / `xpera123` → PixelZone opens (sidebar: "Tenant A · Micro").
2. **Sales**: pick a customer, add an item, Save sale.
3. **Customers › Add customer**: show the 11-digit mobile, then a duplicate number ("already has this mobile number").
4. **Activity log**: the new lines say **Not yet synced** → **Back up now** → **Synced to cloud**.
5. SSMS → Cloud_TenantA → `SELECT * FROM Customers` → the new customer is there with IsSynced = 1.
6. Sign out → sign in as `hub.owner` → **Gaming Hub** opens (another database, same app).
7. Try `nobody` / anything → "Wrong username or password".

---

## Every database connection (what opens what, and with which login)

| # | When | Program | Opens | Logs in with | Set in |
|---|---|---|---|---|---|
| 1 | Sign in | Desktop app | **Xpera_Master** (CompanyUsers, Companies, CompanyDatabases, Subscriptions, AspNetUsers) | Windows login (LocalDB on this PC) | `Db.cs` → `DbContexts.Master()` |
| 2 | Every screen after sign-in | Desktop app | **The store's own database** (Xpera_TenantA / B / C), picked from CompanyDatabases | Windows login | `Db.cs` → `DbContexts.Tenant(...)` |
| 3 | Back up now / sign out | Desktop app | **Cloud Master** db77928 on MonsterASP (to find the store's cloud database) | SQL login `CloudMaster` | `%USERPROFILE%\.xpera\cloud.json` |
| 4 | Back up now / sign out | Desktop app | **The store's cloud database** (db79610 / db79617 / db79621) | SQL login `TenantCredentials:CloudTenantA/B/C`, through the **TenantDatabaseResolver + TenantDbContextFactory** (Lab 4) | `cloud.json` |
| 5 | API `/companies`, `/devices`, `/company-databases` | Xpera.api | **Xpera_Master** | `xpera_master_login` (connection `LocalMaster`) | `appsettings.json` (Lab 2–3) |
| 6 | API `/test-tenant/{id}`, `/tenant/{id}/products`, `/tenant/{id}/customers` | Xpera.api | **The store's database** for that CompanyId | `TenantCredentials:TenantA/B/C` = `xpera_tenantX_login`, through the **resolver + factory** | `appsettings.json` (Lab 4–5) |
| 7 | API `/api/...` controllers (customers, sales, ...) | Xpera.api | Master to find the user's store, then that store | Windows login, same code as the desktop app | `Db.cs` |
| 8 | `Update-Database` | Package Manager Console | The database named by `-Args` (`--Master LocalMaster/CloudMaster`, `--Tenant TenantA/.../CloudTenantC`) | the connection string with that name | `appsettings.json` |
| 9 | Viewing data | SSMS | One database per registered server | its own SQL login (`xpera_master_login`, `xpera_tenantA_login`, ...) | SSMS Registered Servers |

**How to explain it in one breath:** "The desktop app runs on the same PC as LocalDB, so it signs in with Windows. Everything that goes **through the API or to the cloud** uses the **per-store SQL logins**, found through the **CredentialKey** in CompanyDatabases, exactly like Lab 4. And in SSMS each registered server can only open its own database."

---

## Peer walkthrough: screen by screen (what each screen reads and writes)

Go through the app in this order. After each step you can prove it in SSMS with the query in the last column.

| Screen / action | Who can do it | Database | Reads | Writes | Prove it in SSMS |
|---|---|---|---|---|---|
| **Login** (`owner`) | everyone | Master, then the store | CompanyUsers → CompanyDatabases; the store's Users (password hash) | Users.FailedLogins (wrong password), ActivityLogs "Signed in" | Master: `SELECT * FROM CompanyUsers` |
| **Sales** › Save sale | Admin, Cashier | store | Customers, Products, CustomerSummary/CustomerBalances (views) | **Sales + SaleItems** (one transaction), ActivityLogs | `SELECT TOP 5 * FROM Sales ORDER BY SaleId DESC` |
| **Sales history** tab | Admin, Cashier | store | Sales, SaleItems, Products, Customers | nothing | same as above |
| **Payments** › Record payment | Admin, Cashier | store | CustomerBalances (view: what each customer owes) | **Payments**, ActivityLogs | `SELECT TOP 5 * FROM Payments ORDER BY PaymentId DESC` |
| **Products** › Add / set availability | Admin, Staff | store | Products, Inquiries (who's waiting) | **Products**, ActivityLogs | `SELECT * FROM Products` |
| **Customers** › Add customer | everyone | store | CustomerSummary (spent, last visit) | **Customers**, ActivityLogs | `SELECT TOP 5 * FROM Customers ORDER BY CustomerId DESC` |
| **Inquiries** › Log / change status | Admin, Staff | store | Inquiries, Customers, Products | **Inquiries**, ActivityLogs | `SELECT * FROM Inquiries` |
| **Complaints** › Log / change status | Admin, Staff | store | Complaints, Customers | **Complaints**, ActivityLogs | `SELECT * FROM Complaints` |
| **Feedback** › Record | Admin, Staff | store | Feedback, Customers | **Feedback**, ActivityLogs | `SELECT * FROM Feedback` |
| **Customers / Products / Inquiries / Complaints / Feedback** › ••• › Edit (or double-click) | same as Add | store | the record (to fill the form) | **that table**, ActivityLogs "Edited …" | run the same SELECT again: the row changed, IsSynced = 0 |
| **•••** › Delete | same as Add | store | other tables (is the record still used?) | **removes the row** if nothing uses it, ActivityLogs "Deleted …" | the row is gone; the Activity line says "Not yet synced" until the next backup |
| **Users** › Add user | Admin | **Master + store** | Master CompanyUsers + AspNetUsers (is the username free?) | **Master CompanyUsers**, then the store's **Users**, ActivityLogs | Master: `SELECT * FROM CompanyUsers`; store: `SELECT * FROM Users` |
| **Users** › ••• › Edit / Lock / Unlock / Reset password / Delete | Admin | store (+ **Master** for Delete) | Users, Sales, Payments, ActivityLogs (is the account used?) | **Users** (name, role, Status, FailedLogins, PasswordHash); Delete also removes the username from **Master CompanyUsers** | store: `SELECT Username, Status, FailedLogins FROM Users` |
| **Activity log** | everyone | store | ActivityLogs + each record's IsSynced | nothing | `SELECT TOP 10 * FROM ActivityLogs ORDER BY ActivityLogId DESC` |
| **Back up now** | Admin | store + cloud Master + cloud store | every table | local IsSynced = 1; the cloud copy of every table | Cloud_TenantA: `SELECT COUNT(*) FROM Sales` = the same as local |
| **Sign out / ×** | everyone | same as Back up now | | ActivityLogs "Signed out" + the backup | |

Every save also sets **IsSynced = 0** on that record (in `TenantCrmDbContext.SaveChanges`), except SaleItems, which has no flag (it follows its Sale).

---

## How everything works (for you to understand, not to read out)

### The 4 projects and who uses whom
```
Xpera (desktop screens) ──┐
                          ├──► Xpera.infrastructure ──► Xpera.domain
Xpera.api (Web API) ──────┘    DbContexts, migrations,   entities,
                               services, StoreDb         Rules
```

- The desktop app does NOT need the API running; it calls infrastructure directly.
- The API is for other programs (and for the lab endpoints you test with `Xpera.api.http`).

### The Master database (Xpera_Master)
| Table | What it holds | From |
|---|---|---|
| Companies | the stores: PIXEL01, ARCADE01, HUB01 | Lab 2 |
| CompanyDatabases | each store's ServerName, DatabaseName, CredentialKey | Labs 2 + 4 |
| Devices | a store's registered PCs | Lab 3 |
| Plans, Subscriptions | Plan A/B/C and which one each store pays for | yours |
| CompanyUsers | which store each username belongs to | yours (no store code) |
| AspNet… tables | Identity; the Super Admin lives here | Lab 2 (IdentityDbContext) |

### A store database (Xpera_TenantA / B / C, all the same tables)
Users, Products, Customers, Sales + SaleItems, Payments, Inquiries, Complaints, Feedback, ActivityLogs
(+ 2 views: CustomerSummary, CustomerBalances).

### The sign-in trip

1. You type `hub.owner` + password.
2. `MasterDb.FindStoreForUser` → **CompanyUsers** says company 3 (Gaming Hub).
3. **CompanyDatabases** for company 3 → `Xpera_TenantC`.
4. `StoreDb.SignIn` opens Xpera_TenantC and compares the password hash in its **Users** table.
5. 3 wrong passwords → the account is **Locked**.
6. An unknown username gets the same message as a wrong password, so nobody can guess which usernames exist.

### The save trip (example: Save sale)

1. The Sales screen calls `StoreDb.SaveSale`.
2. EF adds the Sale and its SaleItems in **one transaction** (both or nothing).
3. `TenantCrmDbContext.SaveChanges` sets **IsSynced = false** on the sale.
4. `StoreDb` writes an **ActivityLogs** line: "Recorded cash sale OR-000480 · ₱77,990", pointing at Sales + the sale's ID.

### The backup trip (Back up now / sign out)

1. Reads the cloud settings from `%USERPROFILE%\.xpera\cloud.json` (passwords never in the code).
2. Cloud Master → finds this store's cloud database → **TenantDatabaseResolver** + **TenantDbContextFactory** (Lab 4) open it with the cloud CredentialKey.
3. Checks both databases have the **same last migration** (otherwise: "update cloud").
4. Locally: marks the not-synced records **IsSynced = 1**, inside a transaction that waits.
5. Cloud: empties the tables and copies every row with the **same IDs**, in **one transaction**.
6. Only when the cloud copy succeeds does the local transaction commit. If anything fails, nothing is marked synced.

### Adding a staff account

1. Admin → Users → Add user.
2. The form checks the username against the **Master** (unique in all of xpera).
3. The username goes into the Master's **CompanyUsers** first, then into the store's **Users**. If the store save fails, the Master row is removed again.

### Validation (Rules.cs)
| Field | Rule |
|---|---|
| Mobile | 11 digits, starts with 09; saved as `0956 563 4228` |
| Name | letters, spaces, `. - '` |
| Username | 3–40 small letters/numbers/`.`/`_`, starts with a letter, unique |
| Password | 6–50 characters |
| Money | more than ₱0, numbers and one dot |
| Text | at least 2 characters, not longer than the table column |

---

## Likely questions (short answers)

**"Where's your Update and Delete?"**
In the ••• menu at the end of every row (and double-click to edit), on Customers, Products, Users, Inquiries, Complaints and Feedback. The code is in `StoreDb.Edit.cs`.

**"Why can't I delete this customer / product?"**
Because other records use it: the customer has sales, or the product was sold. Deleting it would leave those sales pointing at nothing, and the database's foreign keys also refuse it. The app explains this before asking "are you sure?".

**"Why can't you delete a sale or payment?"**
Money records are never deleted in a real store; they're the store's history and the customer's balance depends on them. That's why Sales and Payments only have Create and Read.

**"What happens to a deleted record in the cloud?"**
It stays there until the next backup. Its Activity log line says "Not yet synced" until then. The backup replaces the cloud tables, so the deleted row disappears from the cloud too.

**"Why is your WinForms .NET 10, not .NET Framework 4.7 like Lab 1?"**
A .NET Framework 4.7 app can't use .NET 10 libraries, and my domain and infrastructure are .NET 10, as the lab says. So the app has to be .NET 10 too to use them.

**"Where are Supplier and Inventory from Lab 5?"**
They're ERP features: buying from suppliers and counting stock. xpera is a CRM, about customers, so I left them out, like my paper says. I can add them if you want the exact steps.

**"Why don't all tables have IsSynced?"**
Only tables with records of their own need it. SaleItems belong to a sale and are backed up with it, so the Sale's flag covers them. Repeating it would break normalization.

**"How does the system know which store to open without a store code?"**
The Master's CompanyUsers table works like a phone book: username → store → database. The password is checked in the store's own database.

**"What if two stores want the same username?"**
They can't. Usernames are unique across all of xpera, which is why the other stores use `arcade.` and `hub.` in front. The Add user form checks the Master before saving.

**"Is the cloud the main database?"**
No. The local database is the main one, and MonsterASP is a one-way backup (local → cloud). It runs when the Admin clicks Back up now, and every time someone signs out.

**"What happens if there's no internet when backing up?"**
It says it can't reach the cloud, and the data stays safe on the PC. The records stay "Not yet synced" and go up at the next backup.

**"Can one store see another store's data?"**
No. Each store has its own database and its own SQL login, and the TenantA login gets "not accessible" on TenantB. Even the Master's login can't open a store.

**"Where are the passwords?"**
User passwords are stored as SHA-256 hashes in each store's Users table. The Super Admin's is an Identity hash. Database passwords are only in appsettings.json and cloud.json on this PC, never in the code.

**"Your desktop app uses Windows login. Where are your per-store SQL logins used?"**
The app runs on the same PC as LocalDB, so Windows login is enough there. The per-store SQL logins are used by the API's tenant endpoints and by the cloud backup, through the resolver and the factory with the CredentialKey. In SSMS each registered server uses its own login too, which is how I showed the isolation.

**"Where is your resolver and factory used?"**
In the API's minimal endpoints (/test-tenant, /tenant/{id}/products, /customers) and in the cloud backup, which opens each store's cloud database through them.

**"Where's the BI dashboard (Tenant B)?"**
Be honest: "That's the next part I'm building: the KPIs and six graphs, reports, and retention campaigns." (Only say this if the dashboard isn't done yet by the time you present.)
