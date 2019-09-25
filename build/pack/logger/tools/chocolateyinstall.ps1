$ErrorActionPreference = 'SilentlyContinue';
Import-Module WebAdministration
$arguments = @{}
$packageName= 'logger'
$toolsDir   = "$(Split-Path $MyInvocation.MyCommand.Definition -Parent)"
$package = "$(Split-Path -Path $toolsDir -Parent)"
$packageParameters = $env:chocolateyPackageParameters

if ($packageParameters) {
  $match_pattern = "\/(?<option>([a-zA-Z]+)):(?<value>([`"'])?([a-zA-Z0-9- _\\:\.]+)([`"'])?)|\/(?<option>([a-zA-Z]+))"
  $option_name = 'option'
  $value_name = 'value'

  if ($packageParameters -match $match_pattern ){
      $results = $packageParameters | Select-String $match_pattern -AllMatches
      $results.matches | % {
        $arguments.Add(
            $_.Groups[$option_name].Value.Trim(),
            $_.Groups[$value_name].Value.Trim())
    }
  }
  if ($arguments.ContainsKey("environment")) {
      Write-Host "Environment Argument Found"
      $environment = $arguments["environment"]
  }
}
# folders Cleanup
     if (Test-Path -path "D:\Inetpub\LoggerAPI") {
    
    Remove-Item "D:\Inetpub\LoggerAPI" -Force -Recurse
   }
  
#folder copy
try {
    Copy-Item "${package}\source\logger\" "D:\Inetpub\LoggerAPI" -Recurse -Force
    if(-not $?) {
        throw "Failure occured on Copy-Item:$_" 
        
    }
  }

catch [Exception]{
    Write-Host "Exception occured on copy : $_"

    Exit(1)  
}
if(!(Test-Path IIS:\AppPools\loggerservice))
{
  New-Item IIS:\AppPools\loggerservice
  Set-ItemProperty "IIS:\AppPools\loggerservice" -Name "managedPipelineMode" -value "Integrated"
  Set-ItemProperty "IIS:\AppPools\crosui.thomsononeadvisor.com" -name "managedRuntimeVersion" -value "No Managed Code"
}



if(!(Test-Path IIS:\Sites\'API'))
{
  New-WebSite -Name "API" -PhysicalPath "D:\Inetpub" -ApplicationPool "loggerservice" -Port 9000
}

if(!(Test-Path IIS:\Sites\'API'\'LoggerWebApi'))
{
  New-WebApplication -Name "LoggerWebApi" -PhysicalPath "D:\Inetpub\LoggerAPI" -ApplicationPool "loggerservice" -Site "API"
}


Set-ItemProperty 'IIS:\Sites\API' -name physicalPath -value "D:\Inetpub"
Set-ItemProperty 'IIS:\Sites\API\LoggerWebApi' -name physicalPath -value "D:\Inetpub\LoggerAPI"

Restart-WebAppPool loggerservice