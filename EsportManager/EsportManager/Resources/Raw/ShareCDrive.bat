@echo off

echo Removing non-system shares...

for /f "skip=4 tokens=1" %%S in ('net share') do (
    if /I not "%%S"=="IPC$" if /I not "%%S"=="ADMIN$" if /I not "%%S"=="C$" (
        echo Deleting share: %%S
        net share %%S /delete /y >nul 2>&1
    )
)

echo.
echo Creating new C drive share...

net share CDrive=C:\ /grant:Everyone,FULL

echo.
echo Done.
echo Access via: \\%COMPUTERNAME%\CDrive