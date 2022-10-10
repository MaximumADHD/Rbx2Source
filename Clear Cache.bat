@echo off
color 7c
echo This script will remove the Rbx2Source Cache folder.
:choice
set /P c=Are you sure you want to continue? (Y/N)
if /I "%c%" EQU "Y" goto :yes
if /I "%c%" EQU "N" goto :No
goto :choice

:yes
rd %LOCALAPPDATA%\Rbx2Source /s
goto :end

:no
echo Cancelled.
pause
goto :end

:end
