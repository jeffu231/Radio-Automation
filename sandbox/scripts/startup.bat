REM Download windowsdesktop-runtime
curl -L "https://builds.dotnet.microsoft.com/dotnet/WindowsDesktop/10.0.3/windowsdesktop-runtime-10.0.3-win-x64.exe" --output C:\users\WDAGUtilityAccount\Downloads\windowsdesktop-runtime.exe

REM Install windowsdesktop-runtime
C:\users\WDAGUtilityAccount\Downloads\windowsdesktop-runtime.exe /norestart /quiet

REM Download aspcore-runtime
REM curl -L "https://builds.dotnet.microsoft.com/dotnet/aspnetcore/Runtime/10.0.3/aspnetcore-runtime-10.0.3-win-x64.exe" --output C:\users\WDAGUtilityAccount\Downloads\aspcore-runtime.exe

REM Install aspcore-runtime
REM C:\users\WDAGUtilityAccount\Downloads\aspcore-runtime.exe /norestart /quiet

REM Install VC Redist
REM C:\Dev\Vixen\Installer\Redist\VC_redist.x64.exe /install /passive /norestart

REM PowerShell Start-Process PowerShell -WindowStyle Normal -WorkingDirectory 'C:\Dev\Radio-Automation\scripts' -ArgumentList '-ExecutionPolicy Bypass -NoExit -NoLogo -File pre-install.ps1'

REM Launch App install panel
appwiz.cpl

REM open output folder
explorer "c:\Dev\Radio-Automation"