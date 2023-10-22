@ECHO off

CALL "%~dp0/etc/build.bat" win-x64
CALL "%~dp0/etc/build.bat" win-arm64

PAUSE