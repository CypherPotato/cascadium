@ECHO off

CALL "%~dp0/build.bat" win-x64
CALL "%~dp0/build.bat" win-arm64

PAUSE