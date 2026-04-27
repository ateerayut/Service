param(
    [string]$MigrationName = "Init"
)

Write-Host "Running EF Migration..."

$infra = "src/Service.Infrastructure"
$api = "src/Service.Api"

dotnet ef migrations add $MigrationName `
 --project $infra `
 --startup-project $api

dotnet ef database update `
 --project $infra `
 --startup-project $api

Write-Host "Migration Completed."