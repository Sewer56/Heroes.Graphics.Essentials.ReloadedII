
# Set Working Directory
Split-Path $MyInvocation.MyCommand.Path | Push-Location
[Environment]::CurrentDirectory = $PWD

./Publish.ps1 -ProjectPath "Heroes.Graphics.Essentials/Heroes.Graphics.Essentials.csproj" `
              -PackageName "Heroes.Graphics.Essentials" `

Pop-Location