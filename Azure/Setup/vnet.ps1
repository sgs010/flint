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

# Write-Host "--- create private DNS zone ---" -ForegroundColor Green
# az network private-dns zone create `
# 	--resource-group $rg `
# 	--name $dnsZone
# if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

# Write-Host "--- link private DNS zone to virtual network ---" -ForegroundColor Green
# az network private-dns link vnet create `
# 	--resource-group $rg `
# 	--name "${dnsZone}-link" `
# 	--zone-name $dnsZone `
# 	--virtual-network $vnet `
# 	--registration-enabled false
# if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
