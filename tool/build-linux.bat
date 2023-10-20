@ECHO off

CALL "%~dp0/build.bat" linux-x64
CALL "%~dp0/build.bat" linux-arm64

PAUSE