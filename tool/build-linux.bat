@ECHO off

CALL "%~dp0/etc/build.bat" linux-x64
CALL "%~dp0/etc/build.bat" linux-arm64

PAUSE