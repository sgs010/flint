Set-StrictMode -Version Latest

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

#--------------------------------------------------------------------------------------------------
# resource group
#--------------------------------------------------------------------------------------------------
Write-Host "--- create resource group ---" -ForegroundColor Green
az group create `
	--name $rg `
	--location $location
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

#--------------------------------------------------------------------------------------------------
# virtual network
#--------------------------------------------------------------------------------------------------
Write-Host "--- create virtual network ---" -ForegroundColor Green
az network vnet create `
	--resource-group $rg `
	--name $vnet `
	--address-prefix 10.0.0.0/16
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

Write-Host "--- create app subnet ---" -ForegroundColor Green
az network vnet subnet create `
	--resource-group $rg `
	--vnet-name $vnet `
	--name $subnetApp `
	--address-prefix 10.0.1.0/24 `
	--private-endpoint-network-policies Enabled
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

$subnetAppId = az network vnet subnet show `
	--resource-group $rg `
	--vnet-name $vnet `
	--name $subnetApp `
	--query id -o tsv
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
Write-Host "app subnet ID is ${subnetAppId}" -ForegroundColor DarkCyan

Write-Host "--- create storage subnet ---" -ForegroundColor Green
az network vnet subnet create `
	--resource-group $rg `
	--vnet-name $vnet `
	--name $subnetStorage `
	--address-prefix 10.0.2.0/24 `
	--private-endpoint-network-policies Enabled
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

$subnetStorageId = az network vnet subnet show `
	--resource-group $rg `
	--vnet-name $vnet `
	--name $subnetStorage `
	--query id -o tsv
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
Write-Host "storage subnet ID is ${subnetStorageId}" -ForegroundColor DarkCyan

Write-Host "--- create gateway subnet ---" -ForegroundColor Green
az network vnet subnet create `
	--resource-group $rg `
	--vnet-name $vnet `
	--name $subnetGateway `
	--address-prefix 10.0.3.0/24 `
	--private-endpoint-network-policies Enabled
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

$subnetGatewayId = az network vnet subnet show `
	--resource-group $rg `
	--vnet-name $vnet `
	--name $subnetGateway `
	--query id -o tsv
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
Write-Host "gateway subnet ID is ${subnetGatewayId}" -ForegroundColor DarkCyan

Write-Host "--- create private DNS zone ---" -ForegroundColor Green
az network private-dns zone create `
	--resource-group $rg `
	--name $dnsZone
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

Write-Host "--- link private DNS zone to virtual network ---" -ForegroundColor Green
az network private-dns link vnet create `
	--resource-group $rg `
	--name "${dnsZone}-link" `
	--zone-name $dnsZone `
	--virtual-network $vnet `
	--registration-enabled false
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

#--------------------------------------------------------------------------------------------------
# storage
#--------------------------------------------------------------------------------------------------
Write-Host "--- create storage ---" -ForegroundColor Green
az storage account create `
	--resource-group $rg `
	--name $storageName `
	--location $location `
	--sku Standard_LRS `
	--kind StorageV2 `
	--public-network-access Disabled
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

$storageId = az storage account show `
	--resource-group $rg `
	--name $storageName `
	--query id -o tsv
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }	
Write-Host "gateway subnet ID is ${storageId}" -ForegroundColor DarkCyan

Write-Host "--- create private endpoint for storage ---" -ForegroundColor Green
az network private-endpoint create `
	--resource-group $rg `
	--name "${storageName}-pe" `
	--location $location `
	--subnet $subnetStorageId `
	--private-connection-resource-id $storageId `
	--group-id blob `
	--connection-name "${storageName}-pe-conn"
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

$storageIpAddr = az network private-endpoint list `
	--resource-group $rg `
	--query "[?name=='${storageName}-pe'].customDnsConfigs[0].ipAddresses[0]" -o tsv
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
Write-Host "storage IP address is ${storageIpAddr}" -ForegroundColor DarkCyan

Write-Host "--- create A record set for storage ---" -ForegroundColor Green
az network private-dns record-set a create `
	--resource-group $rg `
	--name $storageZone `
	--zone-name $dnsZone
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

Write-Host "--- add A record for storage IP address ---" -ForegroundColor Green
az network private-dns record-set a add-record `
	--resource-group $rg `
	--record-set-name $storageZone `
	--zone-name $dnsZone `
	--ipv4-address $storageIpAddr
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

exit

#--------------------------------------------------------------------------------------------------
# webapp
#--------------------------------------------------------------------------------------------------
Write-Host "--- create linux app service plan B3 ---" -ForegroundColor Green
az appservice plan create `
	--resource-group $rg `
	--name $appPlan `
	--location $location `
	--sku B3 `
	--is-linux
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

Write-Host "--- create webapp for ASP.NET Core 9 ---" -ForegroundColor Green
az webapp create `
	--resource-group $rg `
	--name $appName `
	--plan $appPlan `
	--runtime "DOTNETCORE:9.0"
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

$appId = az webapp show `
	--resource-group $rg `
	--name $appName `
	--query id -o tsv
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }	
Write-Host "webapp ID is ${appId}" -ForegroundColor DarkCyan

Write-Host "--- create private endpoint for webapp ---" -ForegroundColor Green
az network private-endpoint create `
	--resource-group $rg `
	--name "${appName}-pe" `
	--location $location `
	--subnet $subnetAppId `
	--private-connection-resource-id $appId `
	--group-ids sites `
	--connection-name "${appName}-pe-conn"
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

$appIpAddr = az network private-endpoint list `
	--resource-group $rg `
	--query "[?name=='${appName}-pe'].customDnsConfigs[0].ipAddresses[0]" -o tsv
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
Write-Host "webapp IP address is ${appIpAddr}" -ForegroundColor DarkCyan

Write-Host "--- create A record set for webapp ---" -ForegroundColor Green
az network private-dns record-set a create `
	--resource-group $rg `
	--name $appZone `
	--zone-name $dnsZone
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

Write-Host "--- add A record for webapp IP address ---" -ForegroundColor Green
az network private-dns record-set a add-record `
	--resource-group $rg `
	--record-set-name $appZone `
	--zone-name $dnsZone `
	--ipv4-address $appIpAddr
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

# # # Write-Host "--- deny all public traffic to webapp ---" -ForegroundColor Green
# # # az webapp config access-restriction add `
# # # 	--resource-group $rg `
# # # 	--name $appName `
# # # 	--rule-name DenyAll `
# # # 	--priority 100 `
# # # 	--action Deny `
# # # 	--ip-address 0.0.0.0/0 `
# # # 	--description "Deny public access" `
# # # 	--scm-site false
# # # if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

# # # Write-Host "--- allow trafic from private endpoint for webapp ---" -ForegroundColor Green
# # # az webapp config access-restriction add `
# # # 	--resource-group $rg `
# # # 	--name $appName `
# # # 	--rule-name AllowPrivateEndpoint `
# # # 	--priority 50 `
# # # 	--action Allow `
# # # 	--ip-address $appPrivateIp `
# # # 	--description "Allow private endpoint"
# # # if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

#--------------------------------------------------------------------------------------------------
# gateway
#--------------------------------------------------------------------------------------------------
Write-Host "--- create gateway policy ---" -ForegroundColor Green
az network application-gateway waf-policy create `
	--resource-group $rg `
	--name $gatewayPolicy `
	--location $location `
	--managed-rules threat-protection=OWASP_CRS_3.2 `
	--policy-settings mode=Prevention
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

Write-Host "--- create public IP ---" -ForegroundColor Green
az network public-ip create `
	--resource-group $rg `
	--name $gatewayIpAddr `
	--sku Standard
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

Write-Host "--- create gateway ---" -ForegroundColor Green
az network application-gateway create `
	--resource-group $rg `
	--name $gatewayName `
	--location $location `
	--sku WAF_v2 `
	--capacity 1 `
	--waf-policy $gatewayPolicy `
	--public-ip-address $gatewayIpAddr `
	--vnet $vnet `
	--subnet $subnetGateway `
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

Write-Host "--- create gateway backend pool ---" -ForegroundColor Green
az network application-gateway address-pool create `
	--resource-group $rg `
	--gateway-name $gatewayName `
	--name $gatewayBackendPool `
	--servers $appFqdn
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

Write-Host "--- create gateway HTTP settings ---" -ForegroundColor Green
az network application-gateway http-settings create `
	--resource-group $rg `
	--gateway-name $gatewayName `
	--name $gatewayHttpSettings `
	--port 80 `
	--protocol Http `
	--cookie-based-affinity Disabled `
	--pick-hostname-from-backend-pool true
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

# Write-Host "--- create webapp healthcheck probe ---" -ForegroundColor Green
# az network application-gateway probe create `
# 	--resource-group $rg `
# 	--gateway-name $gatewayName `
# 	--name $appProbe `
# 	--protocol Http `
# 	--host $appFqdn `
# 	--port 80 `
# 	--path /hc `
# 	--interval 30 `
# 	--threshold 3 `
# 	--timeout 30
# if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

# Write-Host "--- attach webapp healthcheck probe to gateway ---" -ForegroundColor Green
# az network application-gateway http-settings update `
# 	--resource-group $rg `
# 	--gateway-name $gatewayName `
# 	--name $gatewayHttpSettings `
# 	--probe $appProbe
# if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }






# Write-Host "--- create log analytics workspace ---" -ForegroundColor Green
# az monitor log-analytics workspace create `
# 	--resource-group $rg `
# 	--workspace-name $workspace `
# 	--location $location
# if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

# Write-Host "--- enable diagnostic logs for web app ---" -ForegroundColor Green
# $workspaceResId = az monitor log-analytics workspace show `
# 	--resource-group $rg `
# 	--workspace-name $workspace `
# 	--query id -o tsv
# if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

# az monitor diagnostic-settings create `
# 	--name "${appName}-app-logs" `
# 	--resource "/subscriptions/$(az account show --query id -o tsv)/resourceGroups/$rg/providers/Microsoft.Web/sites/$appName" `
# 	--workspace $workspaceResId `
# 	--logs '[{"category":"AppServiceAppLogs","enabled":true},{"category":"AppServiceHTTPLogs","enabled":true}]'
# if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
