# Teacher's lab steps 1–5 (ERP demo) and where each one is in xpera

Pasted by the user on 2026-11-18. The teacher's demo is an ERP ("MasterErp", "TenantErp", "ERP.api");
xpera is a CRM, so names were changed (Erp → Crm, ERP → Xpera) and fields fit the CRM.
Passwords from the original notes are left out on purpose.

| Lab | Teacher's step | In xpera |
|---|---|---|
| 1 | WinForms App (.NET Framework 4.7) | `Xpera` = WinForms on **.NET 10** (net10.0-windows). A .NET Framework 4.7 app can't reference .NET 10 libraries or EF Core 10, which the teacher's other projects use |
| 1 | ASP.NET Core Web API ".api", .NET 10 | `Xpera.api` (net10.0) |
| 1 | Class libraries ".domain", ".infrastructure", .NET 10 | `Xpera.domain`, `Xpera.infrastructure` (net10.0) |
| 1 | Packages: Identity.EntityFrameworkCore, SqlServer, Tools, Design → infrastructure; OpenApi, Design → api | All installed, 10.0.x |
| 2 | domain/entities: Company, CompanyDatabase | Same fields (+ Devices list, + CredentialKey from lab 4) |
| 2 | infrastructure/data: MasterErpDbContext : IdentityDbContext | `MasterCrmDbContext : IdentityDbContext`, same config |
| 2 | Program.cs AddDbContext, appsettings "MasterErp" | Connection "LocalMaster" (+ "CloudMaster"), chosen with `-Args "--Master ..."` |
| 2 | Add-Migration InitialMasterErp, Update-Database | `InitialMasterCrm` |
| 3 | SSMS registered server with SQL login; appsettings with User Id/Password | Registered servers per DB with SQL logins (xpera_master_login, xpera_tenantX_login) |
| 3 | Device entity, Company.Devices, DbSet + config, Add-Migration AddDeviceToMasterErp | `Device`, `AddDeviceToMasterCrm` |
| 3 | MonsterASP account, cloud server in SSMS | db77928 (Master), db79610/db79617/db79621 (stores) |
| 3–4 | MapPost /companies, /devices + .http calls; /openapi/v1.json | In Program.cs and Xpera.api.http |
| 4 | TenantErpDbContext : DbContext, Product entity, "TenantErp" connection, InitialTenantErp | `TenantCrmDbContext`, Product = Name/Category/Price/IsAvailable (CRM fields), connections TenantA/B/C + CloudTenantA/B/C, `InitialTenantCrm` |
| 4 | MapPost /company-databases | Yes |
| 4 | services/: ITenantDatabaseResolver, TenantDatabaseResolver, TenantDatabaseInfo, ITenantDbContextFactory, TenantDbContextFactory | All, same code; AddScoped for both (before builder.Build()) |
| 4 | CompanyDatabase.CredentialKey + AddCredentialKeyToCompanyDatabase; TenantCredentials in appsettings | Yes (TenantA/B/C + CloudTenantA/B/C) |
| 4 | /test-tenant/{id}, GET/POST /tenant/{id}/products | Yes |
| 5 | Second tenant (company 3 → TenantB) in CompanyDatabases + credentials | 3 stores: 1 PIXEL01 → TenantA, 2 ARCADE01 → TenantB, 3 HUB01 → TenantC |
| 5 | Customer entity + AddCustomersToTenantErp; GET/POST /tenant/{id}/customers | Customer (FullName/Contact/UsuallyBuys/CreatedOn) is in InitialTenantCrm; endpoints yes |
| 5 | Supplier, Inventory | **Skipped on purpose**: suppliers and stock counts are ERP, outside xpera's CRM scope (see CLAUDE.md "Not included") |
| 5 | Update-Database -Connection "..." | `-Args "--Tenant X"` instead (`-Connection "Name=..."` doesn't work in EF) |

Beyond the labs (xpera's own): username-only sign-in (CompanyUsers), cloud backup + IsSynced + ActivityLogs,
the desktop screens and controllers.
