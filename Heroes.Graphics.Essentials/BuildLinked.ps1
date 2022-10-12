# Set Working Directory
Split-Path $MyInvocation.MyCommand.Path | Push-Location
[Environment]::CurrentDirectory = $PWD

Remove-Item "$env:RELOADEDIIMODS/sonicheroes.essentials.graphics/*" -Force -Recurse
dotnet publish "./Heroes.Graphics.Essentials.csproj" -c Release -o "$env:RELOADEDIIMODS/sonicheroes.essentials.graphics" /p:OutputPath="./bin/Release" /p:ReloadedILLink="true"

# Restore Working Directory
Pop-Location