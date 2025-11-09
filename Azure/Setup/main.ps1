Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$project = "flint"
$location = "polandcentral"
$dnsZone = "${project}.net"
$appZone = "app"
$storageZone = "storage"

$rg = "${project}-resource-group"

$vnet = "${project}-vnet"
$subnetApp = "app-subnet"
$subnetStorage = "storage-subnet"
$subnetGateway = "gateway-subnet"

$storageName = "${project}blob$(Get-Random)"

$appPlan = "${project}-app-plan"
$appName = "${project}app$(Get-Random)"
$appProbe = "${project}-hc-probe"

$gatewayName = "${project}-gateway"
$gatewayPolicy = "${project}-gateway-policy"
$gatewayIpAddr = "${project}-gateway-pip"
$gatewayBackendPool = "${project}-gateway-backend-pool"
$gatewayHttpSettings = "${project}-gateway-http-settings"

$logWorkspace = "${project}-log"

$subnetAppId = ""
$subnetStorageId = ""
$subnetGatewayId = ""
$storageId = ""
$storageIpAddr = ""
$appId = ""
$appIpAddr = ""

Write-Host "--- create resource group ---" -ForegroundColor Green
az group create --name $rg --location $location
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

. "$PSScriptRoot\vnet.ps1"
#. "$PSScriptRoot\storage.ps1"
#. "$PSScriptRoot\app.ps1"
. "$PSScriptRoot\gateway.ps1"

Write-Host "--- complete ---" -ForegroundColor Green
