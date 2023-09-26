$version = "v.1.0.5.12"

$archs = @(
    "win-x64",
    "win-arm64",
    "linux-x64",
    "linux-arm64",
    "osx-x64"
)

$dir = Split-Path -Parent $MyInvocation.MyCommand.Path
$dir = $dir.Replace('\', '/')

foreach ($arch in $archs) {
    $nameTag = "xcss-$version-$arch"
    Write-Host "Building $nameTag..."

    # build
    & dotnet publish "$dir/xcss.csproj" --nologo -v quiet -r $arch -c Release `
        -o ""$dir/bin/dist/$nameTag/"" --self-contained false -p:DebugType=None -p:DebugSymbols=false
    
    # zip 
    & 7z a "$dir/bin/dist/$nameTag.zip" "$dir/bin/dist/$nameTag/" | Select-String "Error" -Context 10
}

Write-Host "Building done!"