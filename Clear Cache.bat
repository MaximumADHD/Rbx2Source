@echo off
color 7c
echo This program will automatically remove the Rbx2Source Cache folder.
pause
rd %LOCALAPPDATA%\Rbx2Source /s
