# wintools build script
# Copyright Citrix 2009 
#
# USAGE: do_build.ps1 BuildType=Configuration
# 
# EX: do_build.ps1 BuildType=Debug
#     do_build.ps1 BuildType=Release
#

#
# Permission is hereby granted, free of charge, to any person obtaining a copy
# of this software and associated documentation files (the "Software"), to deal
# in the Software without restriction, including without limitation the rights
# to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
# copies of the Software, and to permit persons to whom the Software is
# furnished to do so, subject to the following conditions:
# 
# The above copyright notice and this permission notice shall be included in
# all copies or substantial portions of the Software.
# 
# THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
# IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
# FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
# AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
# LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
# OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
# THE SOFTWARE.
#

$ErrorActionPreference = 'stop'
$ScriptDir = Split-Path -parent $MyInvocation.MyCommand.Path
Import-Module $ScriptDir\..\BuildSupport\invoke.psm1
Import-Module $ScriptDir\..\BuildSupport\checked-copy.psm1

#Get parameters
$args | Foreach-Object {$argtable = @{}} {if ($_ -Match "(.*)=(.*)") {$argtable[$matches[1]] = $matches[2];}}
$BuildType = $argtable["BuildType"]
$CertName = $argtable["CertName"]
$codeVersion = $argtable["VerString"]
$CompanyName = $argtable["CompanyName"]
$MSBuild = $argtable["MSBuild"]

$DoXenClientSign=''
if ($BuildType -eq "Release")
{
    $DoXenClientSign = 'true'
}

# set a couple of environment variables to propagate to msbuild
$env:CERTNAME = $CertName
$env:SIGNTOOLPATH = $signtool+'\signtool.exe'

#Set some important variables
$mywd = Split-Path -Parent $MyInvocation.MyCommand.Path
cd $mywd


Checked-Copy ..\xc-windows\inc\*.h  .\XenGuestAgent
Checked-Copy ..\xc-windows\inc\v4v*.h  .\XenGuestAgent
Checked-Copy ..\xc-windows\xenhdrs\*.h .\XenGuestAgent
Checked-Copy ..\xc-windows\xenuser\xs2\*.h .\XenGuestAgent
rm .\XenGuestAgent\memory.h -Force -V
if (-Not ($?)) {
   throw "unable to delete XenGuestAgent\memory.h"
}


#Prepare for MSM creation
Push-Location .\MSMs
New-Item -Path obj -Type Directory -Force
New-Item -Path bin -Type Directory -Force
Pop-Location

# Prepare Common Assembly information
Push-Location .\props
$Year = (Get-Date).Year
(Get-Content .\CommonAssemblyInfo.cs) | 
Foreach-Object {$_ -replace "//AssemblyVersion", "[assembly: AssemblyVersion(`"$codeVersion`")]"} | 
Foreach-Object {$_ -replace "//AssemblyFileVersion", "[assembly: AssemblyFileVersion(`"$codeVersion`")]"} | 
Foreach-Object {$_ -replace "//AssemblyCopyright", ("[assembly: AssemblyCopyright(`"Copyright © $Year $CompanyName`")]")} | 
Foreach-Object {$_ -replace "//AssemblyCompany", "[assembly: AssemblyCompany(`"$CompanyName`")]"} | 
Set-Content .\CommonAssemblyInfo.cs

$CodeVersion2 = $CodeVersion.Replace(".", ",")
(Get-Content .\common_properties.h) | 
Foreach-Object {$_ -replace "#define COMMON_VERSION `"`"", "#define COMMON_VERSION $codeVersion2"} |
Foreach-Object {$_ -replace "#define COMMON_VERSION_STR `"`"", "#define COMMON_VERSION_STR `"$codeVersion`""} |
Foreach-Object {$_ -replace "#define COMMON_COPYRIGHT `"`"", ("#define COMMON_COPYRIGHT `"Copyright © $Year $CompanyName`"")} | 
Foreach-Object {$_ -replace "#define COMMON_COMPANY `"`"", "#define COMMON_COMPANY `"$CompanyName`""} | 
Set-Content .\common_properties.h
Pop-Location

#Build XenClientGuestService
Invoke-CommandChecked "Build XenClientGuestService" $MSBuild XenClientGuestService\XenClientGuestServiceVS2012.sln /p:Configuration=$BuildType /p:DoXenClientSign=$DoXenClientSign /p:CertName='\"'$Certname'\"' /p:TargetFrameworkVersion=v4.0 /m

#Have to build XenClientGuestService MSMs before the other bits because it does the signing of DLLs as part of the MSBuild
#As building XenGuestPlugin rebuilds a few of these DLLs without signing it would break our install
Push-Location .\MSMs
Invoke-CommandChecked "XenClient Guest Service MSM Candle" ($env:WIX + "bin\candle.exe") .\XenClientGuestService.wxs ("-dConfiguration=" + $BuildType) ("-dVersion=" + $codeVersion) "-dInstallService=yes" -ext WixUtilExtension -out .\obj\
Invoke-CommandChecked "XenClient Guest Service MSM Light" ($env:WIX + "bin\light.exe") .\obj\XenClientGuestService.wixobj -out .\bin\XenClientGuestService.msm -pdbout .\obj\XenClientGuestService.wixpdb -ext WixUtilExtension

#XenClient Guest Service SDK MSM
Invoke-CommandChecked "Candle - XenClientGuestServiceSDK" ($env:WIX + "bin\candle.exe") .\XenClientGuestService.wxs ("-dConfiguration=" + $BuildType) ("-dVersion=" + $codeVersion) "-dInstallService=no" -out .\obj\XenClientGuestServiceSDK.wixobj
Invoke-CommandChecked "Light - XenClientGuestServiceSDK" ($env:WIX + "bin\light.exe") .\obj\XenClientGuestServiceSDK.wixobj -out .\bin\XenClientGuestServiceSDK.msm -pdbout .\obj\XenClientGuestServiceSDK.wixpdb


#Build our own Udbus merge modules outside of visual studio
Invoke-CommandChecked "udbus client candle" ($env:WIX + "bin\candle.exe") Udbus.Bindings.Client.Libraries.InstallerMSM.wxs ("-dConfiguration=" + $BuildType) ("-dVersion=" + $codeVersion) -out .\obj\
Invoke-CommandChecked "udbus client light" ($env:WIX + "bin\light.exe") .\obj\Udbus.Bindings.Client.Libraries.InstallerMSM.wixobj -out .\bin\Udbus.Bindings.Client.Libraries.Installer.msm -pdbout .\obj\Udbus.Bindings.Client.Libraries.Installer.wixpdb
Invoke-CommandChecked "udbus candle" ($env:WIX + "bin\candle.exe") Udbus.Bindings.Interfaces.Libraries.InstallerMSM.wxs ("-dConfiguration=" + $BuildType) ("-dVersion=" + $codeVersion) -out .\obj\
Invoke-CommandChecked "udbus interfaces light" ($env:WIX + "bin\light.exe") .\obj\Udbus.Bindings.Interfaces.Libraries.InstallerMSM.wixobj -out .\bin\Udbus.Bindings.Interfaces.Libraries.Installer.msm -pdbout .\obj\Udbus.Bindings.Interfaces.Libraries.Installer.wixpdb
Invoke-CommandChecked "udbus service candle" ($env:WIX + "bin\candle.exe") Udbus.Bindings.Service.Libraries.InstallerMSM.wxs ("-dConfiguration=" + $BuildType) ("-dVersion=" + $codeVersion) -out .\obj\
Invoke-CommandChecked "udbus service light" ($env:WIX + "bin\light.exe") .\obj\Udbus.Bindings.Service.Libraries.InstallerMSM.wixobj -out .\bin\Udbus.Bindings.Service.Libraries.Installer.msm -pdbout .\obj\Udbus.Bindings.Interfaces.Libraries.Installer.wixpdb

Pop-Location

Invoke-CommandChecked "XenGuestAgent build" $MSBuild .\XenGuestAgent\XenGuestAgent.sln  /p:Configuration=$BuildType /t:Rebuild /m

#Build XenGuestPlugin. Note, rebuilding the bindings generated by XenClientGuestService fails, so can't rebuild XenGuestPlugin.
Invoke-CommandChecked "XenGuestPlugin build" $MSBuild .\XenGuestPlugin\XenGuestPlugin_40.sln /p:Configuration=$BuildType /p:TargetFrameworkVersion=v4.0

#Sign XenGuestPlugin bits
if ($BuildType -eq "Release")
{
	Invoke-CommandChecked "sign XenGuestAgent EXEs" signtool.exe sign /a /s my /n $CertName /t http://timestamp.verisign.com/scripts/timestamp.dll XenGuestAgent\$BuildType\*.exe
	Invoke-CommandChecked "sign XenGuestPlugin DLLs" signtool.exe sign /a /s my /n $CertName /t http://timestamp.verisign.com/scripts/timestamp.dll XenGuestPlugin\XenGuestPlugin\bin\$BuildType\*.dll
	Invoke-CommandChecked "sign XenGuestPlugin EXEs" signtool.exe sign /a /s my /n $CertName /t http://timestamp.verisign.com/scripts/timestamp.dll XenGuestPlugin\XenGuestPlugin\bin\$BuildType\*.exe
}

#Build remaining MSMs
Push-Location .\MSMs

#XenGuestPlugin
Invoke-CommandChecked "Candle - XenGuestPlugin x32" ($env:WIX + "bin\candle.exe") .\XenGuestPlugin.wxs ("-dConfiguration=" + $BuildType) ("-dVersion=" + $codeVersion) ("-dPlatform=x86") -out .\obj\
Invoke-CommandChecked "Light - XenGuestPlugin x32" ($env:WIX + "bin\light.exe") .\obj\XenGuestPlugin.wixobj -out .\bin\XenGuestPlugin.msm -pdbout .\obj\XenGuestPlugin.wixpdb
Invoke-CommandChecked "Candle - XenGuestPlugin x64" ($env:WIX + "bin\candle.exe") .\XenGuestPlugin.wxs ("-dConfiguration=" + $BuildType) ("-dVersion=" + $codeVersion) ("-dPlatform=x64") -out .\obj\
Invoke-CommandChecked "Light - XenGuestPlugin x64" ($env:WIX + "bin\light.exe") .\obj\XenGuestPlugin.wixobj -out .\bin\XenGuestPlugin64.msm -pdbout .\obj\XenGuestPlugin64.wixpdb

#XenGuestAgent
Invoke-CommandChecked "Candle - XenGuestAgent" ($env:WIX + "bin\candle.exe") .\XenGuestAgent.wxs ("-dConfiguration=" + $BuildType) ("-dVersion=" + $codeVersion) -out .\obj\
Invoke-CommandChecked "Light - XenGuestAgent" ($env:WIX + "bin\light.exe") .\obj\XenGuestAgent.wixobj -out .\bin\XenGuestAgent.msm -pdbout .\obj\XenGuestAgent.wixpdb

#SDK Installer
Invoke-CommandChecked "Candle - XenClientGuestClientConsoleTestInstaller" ($env:WIX + "bin\candle.exe") .\XenClientGuestClientConsoleTestInstaller.wxs ("-dConfiguration=" + $BuildType) ("-dVersion=" + $codeVersion) -out .\obj\
Invoke-CommandChecked "Light - XenClientGuestClientConsoleTestInstaller" ($env:WIX + "bin\light.exe") .\obj\XenClientGuestClientConsoleTestInstaller.wixobj -out .\bin\XenClientGuestClientConsoleTestInstaller.msi -pdbout .\obj\XenGuestAgent.wixpdb -ext WixUIExtension

Pop-Location

#Test merge modules built
$output = @(0..3)
$output[0] = $mywd + "\MSMs\bin\XenGuestAgent.msm"
$output[1] = $mywd + "\MSMs\bin\XenGuestPlugin64.msm"
$output[2] = $mywd + "\MSMs\bin\XenGuestPlugin.msm"
$output[3] = $mywd + "\MSMs\bin\XenClientGuestService.msm"
$output | Foreach-Object {
	if (!(Test-Path -Path $_ -PathType Leaf))
	{
		Write-Host ("Failed to make {0} installer package" -f $_)
		$host.SetShouldExit(2)
		Exit
	}
}


