@ECHO off

SET NAME=%1

ECHO Building %NAME%...

dotnet publish "%~dp0Cascadium-Utility.csproj" --nologo -v quiet -r %NAME% -c Release -o "%~dp0bin/Build/%NAME%/"