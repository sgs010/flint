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
