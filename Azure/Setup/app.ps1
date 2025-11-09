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
