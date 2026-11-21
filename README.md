# xpera CRM

**Level up every customer.** xpera (XP + era) is a desktop CRM for small stores that sell gaming PC parts:
customers, sales, payments, inquiries, complaints, and feedback, one private database per store.
IT13L individual project.

## Projects (`code/Xpera.slnx`)
| Project | What it holds |
|---|---|
| `Xpera` | Windows Forms app (.NET 10): the screens |
| `Xpera.domain` | entities and input rules |
| `Xpera.infrastructure` | EF Core DbContexts (Master + store), migrations, services (tenant resolver/factory, cloud backup) |
| `Xpera.api` | ASP.NET Core Web API (.NET 10) |

## Databases
- **Xpera_Master**: companies (stores), their databases and login keys, plans, subscriptions, devices, usernames.
- **Xpera_TenantA / B / C**: one database per store, same tables.
- Cloud copies on MonsterASP (one-way backup, local → cloud).

## Run it
1. Copy `code/Xpera.api/appsettings.example.json` to `appsettings.json` and fill in your own logins and passwords.
2. Package Manager Console (default project `Xpera.infrastructure`):
   `Update-Database -Context MasterCrmDbContext -StartupProject Xpera.api`, then
   `Update-Database -Context TenantCrmDbContext -StartupProject Xpera.api -Args "--Tenant TenantA"` (and TenantB, TenantC).
3. Fill sample data with `code/Xpera.infrastructure/Database/01-master-data.sql` (Master) and `03`, `04` (each store).
4. Start `Xpera`. Sample sign-ins (test data only, password `xpera123`): `owner`, `arcade.owner`, `hub.owner`.

More: `docs/xpera - Presentation Guide (Nov 19).pdf`.
