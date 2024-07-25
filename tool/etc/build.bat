@ECHO off

SET NAME=%1

ECHO Building %NAME%...

dotnet publish "%~dp0/../Cascadium-Utility.csproj" --nologo ^
	-v quiet ^
	-r %NAME% ^
	-c Release ^
	--self-contained true ^
	-p:PublishReadyToRun=true ^
	-p:PublishTrimmed=true ^
	-p:PublishSingleFile=true ^
	-o "%~dp0bin/Build/cascadium-v0.6-%NAME%/"