$ErrorActionPreference = 'Stop';

$packageArgs = @{
  packageName    = $env:ChocolateyPackageName
  softwareName   = "dolt*"
  unzipLocation  = "$(Split-Path -parent $MyInvocation.MyCommand.Definition)"
  url64bit       = "https://github.com/dolthub/dolt/releases/download/v0.23.9/dolt-windows-amd64.zip"
  checksum64     = "FB217C59B189FCE2F7EF02F849DC908D8AE38F2F0E80F6ECCD267E4B8DD1B323"
  checksumType64 = "sha256"
}

Install-ChocolateyZipPackage @packageArgs
