Write-Host "--- create gateway policy ---" -ForegroundColor Green
az network application-gateway waf-policy create `
	--resource-group $rg `
	--name $gatewayPolicy `
	--location $location `
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

Write-Host "--- add OWASP 4.19 rule set to the gateway policy ---" -ForegroundColor Green
az network application-gateway waf-policy managed-rule rule-set add `
	--resource-group $rg `
	--policy-name $gatewayPolicy `
    --type OWASP `
    --version 4.19
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
	--servers "${appZone}.${dnsZone}"
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

Write-Host "--- create webapp healthcheck probe ---" -ForegroundColor Green
az network application-gateway probe create `
	--resource-group $rg `
	--gateway-name $gatewayName `
	--name $appProbe `
	--protocol Http `
	--host "${appZone}.${dnsZone}" `
	--port 80 `
	--path /hc `
	--interval 30 `
	--threshold 3 `
	--timeout 30
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

Write-Host "--- attach webapp healthcheck probe to gateway ---" -ForegroundColor Green
az network application-gateway http-settings update `
	--resource-group $rg `
	--gateway-name $gatewayName `
	--name $gatewayHttpSettings `
	--probe $appProbe
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
