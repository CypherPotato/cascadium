@echo off

:: create the output directory
mkdir bin

:: build runtime assembly and copy lib
dotnet build -c Release ./tool/Cascadium-Utility.csproj

copy tool\bin\Release\net7.0\Cascadium.Core.dll bin\Cascadium.Core.dll
copy tool\bin\Release\net7.0\LightJson.dll bin\LightJson.dll

mkdir bin\tool

:: build executeables

for %%i in (windows linux) do (
	mkdir bin\tool\%%i

    call bflat build ^
		tool/Compiler.cs ^
		tool/CommandLineArguments.cs ^
		tool/CommandLineParser.cs ^
		tool/JsonCompilerOptions.cs ^
		tool/Log.cs ^
		tool/PathUtils.cs ^
		tool/Program.cs ^
		tool/Watcher.cs ^
		-r bin/Cascadium.Core.dll ^
		-r bin/LightJson.dll ^
		-Ot ^
		--no-stacktrace-data ^
		--no-globalization ^
		-m native ^
		--os %%i ^
		--arch x64 ^
		-o bin/tool/%%i/cascadium
)