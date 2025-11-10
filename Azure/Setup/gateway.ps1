Write-Host "--- create gateway policy ---" -ForegroundColor Green
az network application-gateway waf-policy create `
	--resource-group $rg `
	--name $gatewayPolicy `
	--location $location
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

Write-Host "--- configure gateway policy ---" -ForegroundColor Green
az network application-gateway waf-policy policy-setting update `
	--resource-group $rg `
	--policy-name $gatewayPolicy `
	--mode Prevention `
	--state Enabled
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
	--vnet-name $vnet `
	--subnet $subnetGateway `
	--priority 100 `
	--frontend-port 80 `
	--http-settings-cookie-based-affinity Disabled `
	--http-settings-port 80 `
	--http-settings-protocol Http `
	--servers $appIpAddr
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

Write-Host "--- override hostname ---" -ForegroundColor Green
az network application-gateway http-settings update `
	--resource-group $rg `
	--name "appGatewayBackendHttpSettings" `
	--gateway-name $gatewayName `
	--host-name "${appName}.azurewebsites.net"
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

Write-Host "--- create webapp healthcheck probe ---" -ForegroundColor Green
az network application-gateway probe create `
	--resource-group $rg `
	--gateway-name $gatewayName `
	--name $appProbe `
	--protocol Http `
	--host ${appIpAddr} `
	--port 80 `
	--path / `
	--interval 30 `
	--threshold 3 `
	--timeout 30
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
