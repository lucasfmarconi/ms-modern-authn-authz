Connect-AzureAD
$cert = New-SelfSignedCertificate -certstorelocation cert:\currentuser\my -subject "CN=adalconsoleapp" -KeySpec KeyExchange
$bin = $cert.RawData
$base64Value = [System.Convert]::ToBase64String($bin)
$bin = $cert.GetCertHash()
$thumbprint = [System.Convert]::ToBase64String($bin)
Export-Certificate -Cert $cert -FilePath C:\temp\adalconsole.cer



# Ignore
$keyid = [System.Guid]::NewGuid().ToString()
$jsonObj = @{customKeyIdentifier=$base64Thumbprint;keyId=$keyid;type="AsymmetricX509Cert";usage="Verify";value=$base64Value}
$keyCredentials=ConvertTo-Json @($jsonObj) | Out-File "keyCredentials.txt"

$TenantId = (Get-AzureRmSubscription -SubscriptionName "Contoso Default").TenantId
$ApplicationId = (Get-AzureADApplication -DisplayNameStartWith ADALConsoleApp).ApplicationId

$Thumbprint = (Get-ChildItem cert:\CurrentUser\My\ | Where-Object {$_.Subject -match "CN=exampleappScriptCert" }).Thumbprint
