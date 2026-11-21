using Microsoft.EntityFrameworkCore;
using Xpera.infrastructure.data;
using Xpera.domain.entities;
using Xpera.infrastructure.services;

var builder = WebApplication.CreateBuilder(args);

string master = builder.Configuration["Master"] ?? "LocalMaster";
builder.Services.AddDbContext<MasterCrmDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString(master)));

// The store tables. Every store has the same tables, so one DbContext serves all 3 store databases.
// Which store's line in appsettings.json to use. TenantA unless Update-Database passes another one:
//   Update-Database -Context TenantCrmDbContext -StartupProject Xpera.api -Args "--Tenant TenantB"

string tenant = builder.Configuration["Tenant"] ?? "TenantA";
builder.Services.AddDbContext<TenantCrmDbContext>(options =>  
    options.UseSqlServer(
        builder.Configuration.GetConnectionString(tenant)));


builder.Services.AddScoped<ITenantDatabaseResolver, TenantDatabaseResolver>();
builder.Services.AddScoped<ITenantDbContextFactory, TenantDbContextFactory>();
// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapPost("/companies", async (
    Company company,
    MasterCrmDbContext db) =>
{
    db.Companies.Add(company);
    await db.SaveChangesAsync();

    return Results.Created($"/companies/{company.CompanyId}", company);
});

app.MapPost("/devices", async (
    Device device,
    MasterCrmDbContext db) =>
{
    db.Devices.Add(device);
    await db.SaveChangesAsync();

    return Results.Created($"/devices/{device.DeviceId}", device);
});

app.MapPost("/company-databases", async (
    CompanyDatabase companyDatabase,
    MasterCrmDbContext db) =>
{
    db.CompanyDatabases.Add(companyDatabase);
    await db.SaveChangesAsync();

    return Results.Created(
        $"/company-databases/{companyDatabase.CompanyDatabaseId}",
        companyDatabase);
});

app.MapGet("/test-tenant/{companyId:int}", async (
    int companyId,
    ITenantDbContextFactory tenantFactory) =>
{
    await using var tenantDb = await tenantFactory.CreateAsync(companyId);

    var productCount = await tenantDb.Products.CountAsync();

    return Results.Ok(new
    {
        companyId,
        productCount
    });
});

app.MapPost("/tenant/{companyId:int}/products", async (
    int companyId,
    Product product,
    ITenantDbContextFactory tenantFactory) =>
{
    await using var tenantDb = await tenantFactory.CreateAsync(companyId);

    tenantDb.Products.Add(product);
    await tenantDb.SaveChangesAsync();

    return Results.Created(
        $"/tenant/{companyId}/products/{product.ProductId}",
        product);
});

app.MapGet("/tenant/{companyId:int}/products", async (
    int companyId,
    ITenantDbContextFactory tenantFactory) =>
{
    await using var tenantDb = await tenantFactory.CreateAsync(companyId);

    var products = await tenantDb.Products
        .AsNoTracking()
        .OrderBy(x => x.ProductId)
        .ToListAsync();

    return Results.Ok(products);
});

app.MapPost("/tenant/{companyId:int}/customers", async (
    int companyId,
    Customer customer,
    ITenantDbContextFactory tenantFactory) =>
{
    await using var tenantDb = await tenantFactory.CreateAsync(companyId);

    tenantDb.Customers.Add(customer);
    await tenantDb.SaveChangesAsync();

    return Results.Created(
        $"/tenant/{companyId}/customers/{customer.CustomerId}",
        customer);
});

app.MapGet("/tenant/{companyId:int}/customers", async (
    int companyId,
    ITenantDbContextFactory tenantFactory) =>
{
    await using var tenantDb = await tenantFactory.CreateAsync(companyId);

    var customers = await tenantDb.Customers
        .AsNoTracking()
        .OrderBy(x => x.CustomerId)
        .ToListAsync();

    return Results.Ok(customers);
});

app.Run();
