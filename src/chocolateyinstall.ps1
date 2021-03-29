$ErrorActionPreference = 'Stop';

$packageArgs = @{
  packageName    = $env:ChocolateyPackageName
  softwareName   = "dolt"
  unzipLocation  = "$(Split-Path -parent $MyInvocation.MyCommand.Definition)"
  url64bit       = "${url64bit}"
  checksum64     = "${checksum64}"
  checksumType64 = "${checksumType64}"
}

Install-ChocolateyZipPackage @packageArgs
