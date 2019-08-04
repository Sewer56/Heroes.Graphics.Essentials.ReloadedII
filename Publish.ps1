# Project Output Paths
$essentialsOutputPath = "Heroes.Graphics.Essentials/bin"
$publishDirectory = "Publish"

if ([System.IO.Directory]::Exists($publishDirectory)) {
	Get-ChildItem $publishDirectory -Include * -Recurse | Remove-Item -Force -Recurse
}


# Build
dotnet clean Heroes.Graphics.Essentials.sln
dotnet build -c Release Heroes.Graphics.Essentials.sln

# Cleanup
Get-ChildItem $essentialsOutputPath -Include *.pdb -Recurse | Remove-Item -Force -Recurse
Get-ChildItem $essentialsOutputPath -Include *.xml -Recurse | Remove-Item -Force -Recurse

# Make compressed directory
if (![System.IO.Directory]::Exists($publishDirectory)) {
    New-Item $publishDirectory -ItemType Directory
}

# Compress
Add-Type -A System.IO.Compression.FileSystem
[IO.Compression.ZipFile]::CreateFromDirectory( $essentialsOutputPath + '/Release', 'Publish/HeroesGraphicsEssentials.zip')