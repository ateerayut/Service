param(
    [ValidateSet("add", "update", "script", "remove", "list")]
    [string]$Action = "update",

    [string]$MigrationName,

    [string]$FromMigration = "0",

    [string]$ToMigration,

    [string]$Output = "artifacts/migrations/migration.sql"
)

$ErrorActionPreference = "Stop"

$InfrastructureProject = "src/Service.Infrastructure"
$StartupProject = "src/Service.Api"
$MigrationsOutputDir = "Migrations"

function Invoke-DotNetEf {
    param(
        [Parameter(Mandatory = $true)]
        [string[]]$Arguments
    )

    dotnet ef @Arguments `
        --project $InfrastructureProject `
        --startup-project $StartupProject
}

function Assert-MigrationName {
    if ([string]::IsNullOrWhiteSpace($MigrationName)) {
        throw "MigrationName is required for action '$Action'. Example: .\migrate.ps1 -Action add -MigrationName AddCustomers"
    }
}

Write-Host "EF Core migration helper"
Write-Host "Action: $Action"
Write-Host "Infrastructure project: $InfrastructureProject"
Write-Host "Startup project: $StartupProject"

switch ($Action) {
    "add" {
        Assert-MigrationName

        Invoke-DotNetEf @(
            "migrations",
            "add",
            $MigrationName,
            "--output-dir",
            $MigrationsOutputDir
        )

        Write-Host "Migration '$MigrationName' created."
    }

    "update" {
        $arguments = @("database", "update")

        if (-not [string]::IsNullOrWhiteSpace($ToMigration)) {
            $arguments += $ToMigration
        }

        Invoke-DotNetEf $arguments

        Write-Host "Database update completed."
    }

    "script" {
        $outputDirectory = Split-Path -Parent $Output

        if (-not [string]::IsNullOrWhiteSpace($outputDirectory)) {
            New-Item -ItemType Directory -Path $outputDirectory -Force | Out-Null
        }

        $arguments = @(
            "migrations",
            "script",
            $FromMigration,
            "--idempotent",
            "--output",
            $Output
        )

        if (-not [string]::IsNullOrWhiteSpace($ToMigration)) {
            $arguments = @(
                "migrations",
                "script",
                $FromMigration,
                $ToMigration,
                "--idempotent",
                "--output",
                $Output
            )
        }

        Invoke-DotNetEf $arguments

        Write-Host "Migration SQL script generated: $Output"
    }

    "remove" {
        Invoke-DotNetEf @("migrations", "remove")

        Write-Host "Last migration removed."
    }

    "list" {
        Invoke-DotNetEf @("migrations", "list")
    }
}
