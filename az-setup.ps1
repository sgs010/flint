$project = "flint"
$location = "polandcentral"
$dnsZone = "privatelink.azurewebsites.net"

$rg = "${project}-resource-group"

$vnet = "${project}-vnet"
$subnetApp = "app-subnet"
$subnetStorage = "storage-subnet"
$subnetGateway = "gateway-subnet"

$storage = "${project}blob$(Get-Random)"

$appPlan = "${project}-app-plan"
$appName = "${project}app$(Get-Random)"
$appFqdn = "${appName}.azurewebsites.net"
$appProbe = "${project}-hc-probe"

$gateway = "${project}-gateway"
$gatewayPolicy = "${project}-gateway-policy"
$gatewayPublicIp = "${project}-gateway-pip"
$gatewayBackendPool = "${project}-gateway-backend-pool"
$gatewayHttpSettings = "${project}-gateway-http-settings"

$logsWorkspace = "${project}-logs"

$subnetAppId = ""
$subnetStorageId = ""
$subnetGatewayId = ""
$appId = ""
$appPrivateIp = ""

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

#--------------------------------------------------------------------------------------------------
# storage
#--------------------------------------------------------------------------------------------------
# Write-Host "--- create storage account ---" -ForegroundColor Green
# az storage account create `
# 	--name $storage `
# 	--resource-group $rg `
# 	--location $location `
# 	--sku Standard_LRS `
# 	--kind StorageV2 `
# 	--public-network-access Disabled
# if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

# Write-Host "--- create private endpoint for blob ---" -ForegroundColor Green
# $blobId = az storage account show `
# 	--name $storage `
# 	--resource-group $rg `
# 	--query id -o tsv
# if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }	

# $subnetIdStorage = az network vnet subnet show `
# 	--resource-group $rg `
# 	--vnet-name $vnet `
# 	--name $subnetStorage `
# 	--query id -o tsv
# if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

# az network private-endpoint create `
# 	--name "${storage}-pe" `
# 	--resource-group $rg `
# 	--location $location `
# 	--subnet $subnetIdStorage `
# 	--private-connection-resource-id $blobId `
# 	--group-id blob `
# 	--connection-name "${storage}-blob-conn"
# if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

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

Write-Host "--- create web app for ASP.NET Core 9 ---" -ForegroundColor Green
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

$appPrivateIp = az network private-endpoint list `
	--resource-group $rg `
	--query "[?name=='${appName}-pe'][0].customDnsConfigs[0].ipAddresses[0]" -o tsv
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

Write-Host "--- create webapp private DNS zone ---" -ForegroundColor Green
az network private-dns zone create `
	--resource-group $rg `
	--name $dnsZone
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

Write-Host "--- link webapp private DNS zone to VNet ---" -ForegroundColor Green
az network private-dns link vnet create `
	--resource-group $rg `
	--name "${dnsZone}-link" `
	--zone-name $dnsZone `
	--virtual-network $vnet `
	--registration-enabled false
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

Write-Host "--- create A record for the azurewebsites.net name pointing to the private IP ---" -ForegroundColor Green
az network private-dns record-set a create `
	--resource-group $rg `
	--name $appName `
	--zone $dnsZone
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }


az network private-dns record-set a add-record `
	--resource-group $rg `
	--name $appName `
	--zone $dnsZone `
	--ipv4-address $appPrivateIp
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

!!!!! ERROR argument --ipv4-address/-a: expected one argument

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

exit

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
	--name $gatewayPublicIp `
	--sku Standard
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

Write-Host "--- create gateway ---" -ForegroundColor Green
az network application-gateway create `
	--resource-group $rg `
	--name $gateway `
	--location $location `
	--sku WAF_v2 `
	--capacity 1 `
	--waf-policy $gatewayPolicy `
	--public-ip-address $gatewayPublicIp `
	--vnet $vnet `
	--subnet $subnetGateway `
	--frontend-port 80 `
	--http-settings-port 80 `
	--http-settings-protocol Http `
	--http-settings-cookie-based-affinity Disabled
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

Write-Host "--- create gateway backend pool ---" -ForegroundColor Green
az network application-gateway address-pool create `
	--resource-group $rg `
	--gateway-name $gateway `
	--name $gatewayBackendPool `
	--servers $appFqdn
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

Write-Host "--- create gateway HTTP settings ---" -ForegroundColor Green
az network application-gateway http-settings create `
	--resource-group $rg `
	--gateway-name $gateway `
	--name $gatewayHttpSettings `
	--port 80 `
	--protocol Http `
	--cookie-based-affinity Disabled `
	--pick-hostname-from-backend-pool true
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

Write-Host "--- create webapp healthcheck probe ---" -ForegroundColor Green
az network application-gateway probe create `
	--resource-group $rg `
	--gateway-name $gateway `
	--name $appProbe `
	--protocol Http `
	--host $appFqdn `
	--port 80 `
	--path /hc `
	--interval 30 `
	--threshold 3 `
	--timeout 30
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

Write-Host "--- attach webapp healthcheck probe to gateway ---" -ForegroundColor Green
az network application-gateway http-settings update `
	--resource-group $rg `
	--gateway-name $gateway `
	--name $gatewayHttpSettings `
	--probe $appProbe
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }






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
