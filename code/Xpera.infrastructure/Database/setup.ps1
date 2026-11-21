# Builds every xpera database from scratch: the Master plus one database per store.
# WARNING: running it again deletes everything typed into them.
#
# The tables come from the EF Core migrations (Xpera.infrastructure\Migrations). 00-master-tables.sql and
# 02-store-tables.sql are those migrations as SQL (Script-Migration -Idempotent), so this script makes exactly
# what Update-Database makes. After adding a migration, make them again (see "Remaking the table scripts" below).
# Then 01, 03, and 04 fill the tables with sample data.
#
# On this PC (SQL Server LocalDB, comes with Visual Studio):
#   .\setup.ps1
# Another server on this PC:
#   .\setup.ps1 -Server ".\SQLEXPRESS"
# Online (MonsterASP). The databases must already exist there, and each one needs its line in
# %USERPROFILE%\.xpera\databases.txt (the same file the app reads). Databases without a line are skipped.
#   .\setup.ps1 -Online
param([string]$Server = "(localdb)\MSSQLLocalDB", [switch]$Online)

$ErrorActionPreference = "Stop"
$here = Split-Path -Parent $MyInvocation.MyCommand.Path
$names = "Xpera_Master", "Xpera_TenantA", "Xpera_TenantB", "Xpera_TenantC"

# Reads "name = connection string" lines, skipping comments and passwords still shown as ********.
function Read-Settings {
    $file = Join-Path $env:USERPROFILE ".xpera\databases.txt"
    if (-not (Test-Path $file)) { throw "No settings file yet: $file" }
    $settings = @{}
    foreach ($line in Get-Content $file) {
        $at = $line.IndexOf("=")
        if ($line.TrimStart().StartsWith("#") -or $at -lt 0) { continue }
        $connection = $line.Substring($at + 1).Trim()
        if ($connection -eq "" -or $connection.Contains("********")) { continue }
        $parts = @{}
        foreach ($pair in $connection.Split(";")) {
            $eq = $pair.IndexOf("=")
            if ($eq -gt 0) { $parts[$pair.Substring(0, $eq).Trim().ToLower()] = $pair.Substring($eq + 1).Trim() }
        }
        $settings[$line.Substring(0, $at).Trim()] = $parts
    }
    return $settings
}

# Runs one script against one database. Online, the password goes through SQLCMDPASSWORD,
# so it is never typed on the command line.
function Run($name, $file, $target) {
    if ($target) {
        $env:SQLCMDPASSWORD = $target["password"]
        $sqlArgs = @("-S", $target["server"], "-d", $target["database"], "-U", $target["user id"], "-N", "-C")
    } else {
        $sqlArgs = @("-S", $Server, "-d", $name, "-E")
    }
    & sqlcmd @sqlArgs -b -I -i (Join-Path $here $file)   # -I = QUOTED_IDENTIFIER ON, as EF Core and SSMS expect
    $code = $LASTEXITCODE
    $env:SQLCMDPASSWORD = $null
    if ($code -ne 0) { throw "Failed while running $file on $name" }
}

if ($Online) {
    $settings = Read-Settings
} else {
    if ($Server -like "(localdb)*") { & sqllocaldb start MSSQLLocalDB | Out-Null }
    # On this PC, setup makes empty databases first. Online, MonsterASP makes them in its panel.
    foreach ($name in $names) {
        & sqlcmd -S $Server -E -b -Q "IF DB_ID('$name') IS NOT NULL BEGIN ALTER DATABASE [$name] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [$name]; END; CREATE DATABASE [$name];"
        if ($LASTEXITCODE -ne 0) { throw "Could not create $name" }
    }
}

foreach ($name in $names) {
    $target = $null
    if ($Online) {
        $target = $settings[$name]
        if (-not $target) { Write-Host "Skipping $name (no line in databases.txt yet)."; continue }
    }
    Write-Host "Building $name..."
    if ($name -eq "Xpera_Master") {
        Run $name "00-master-tables.sql" $target
        Run $name "01-master-data.sql" $target
    } else {
        Run $name "02-store-tables.sql" $target
        Run $name "03-store-sample-data.sql" $target
        Run $name "04-store-transactions.sql" $target
    }
}

if ($Online) { Write-Host "Done. The online databases are ready." }
else { Write-Host "Done. Open SSMS and connect to $Server to see the databases." }

# Remaking the table scripts after a new migration (Package Manager Console, Default project = Xpera.infrastructure):
#   Script-Migration -Idempotent -Context MasterCrmDbContext -StartupProject Xpera.api -Output Xpera.infrastructure/Database/00-master-tables.sql
#   Script-Migration -Idempotent -Context TenantCrmDbContext -StartupProject Xpera.api -Output Xpera.infrastructure/Database/02-store-tables.sql
