# Load the ADFS PowerShell snap-in
Add-PSSnapin Microsoft.Adfs.PowerShell

# The directory where the relying parties should be extracted
$filePathBase = "C:\ADFS-RP-Output\"
mkdir $filePathBase

$AdfsRelyingPartyTrusts = Get-AdfsRelyingPartyTrust
foreach ($AdfsRelyingPartyTrust in $AdfsRelyingPartyTrusts)
{
  # The identifier is actually an array of identifiers, we will just use the first one
  $rpIdentifier = $AdfsRelyingPartyTrust.Identifier[0]

  # We want a filename for this so we will try to make the identifier safe
  # Replace all of the following characters with a - 
  #  : " / \ | ? *
  $fileNameSafeIdentifier = $rpIdentifier `
    -replace '', '-' `
    -replace ':', '-' `
    -replace '"', '-' `
    -replace '/', '-' `
    -replace '\\', '-' `
    -replace '\|', '-' `
    -replace '\?', '-' `
    -replace '\*', '-'

  # Create the filename of the XML file we will export
  $filePath = $filePathBase + $fileNameSafeIdentifier + '.xml'

  # Use Export-Clixml to export the object to an XML file
  $AdfsRelyingPartyTrust | Export-Clixml $filePath

}