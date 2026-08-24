@echo off
setlocal EnableDelayedExpansion
REM ===========================================================================
REM  RSAF VR Warehouse Safety - copy trainee results off a Quest headset.
REM
REM  The app records every run into an Excel spreadsheet stored on the headset.
REM  Double-click this file to copy those spreadsheets to your Desktop.
REM ===========================================================================

set "PACKAGE_ID=com.sp.fyp3a25.rsafvrwarehouse"
set "REMOTE_DIR=/storage/emulated/0/Android/data/%PACKAGE_ID%/files/Output"

cd /d "%~dp0"
cls
echo.
echo   Collect trainee results from a Quest headset
echo.

set "ADB="
where adb >nul 2>&1 && for /f "delims=" %%A in ('where adb') do if not defined ADB set "ADB=%%A"
if not defined ADB if exist "%~dp0platform-tools\adb.exe" set "ADB=%~dp0platform-tools\adb.exe"
if not defined ADB if exist "%LOCALAPPDATA%\Android\Sdk\platform-tools\adb.exe" set "ADB=%LOCALAPPDATA%\Android\Sdk\platform-tools\adb.exe"

if not defined ADB (
    call :die "The Android tools are not set up on this PC." ^
        "Run 'install-on-quest-windows.bat' first - it downloads them automatically."
)

echo ==^> Connecting to the headset
"!ADB!" start-server >nul 2>&1
"!ADB!" wait-for-device shell true >nul 2>&1
if errorlevel 1 (
    call :die "No headset is connected." ^
        "Plug the headset into this PC with a USB-C cable." ^
        "Put it on and approve the 'Allow USB debugging' prompt."
)
echo     Connected.

echo.
echo ==^> Looking for results
set "FOUND="
for /f "delims=" %%F in ('"!ADB!" shell "ls %REMOTE_DIR% 2^>/dev/null" 2^>nul') do set "FOUND=1"
if not defined FOUND (
    call :die "No results were found on this headset." ^
        "Results only appear after somebody completes a training run." ^
        "Check the app has been used on this particular headset."
)

REM Stamp each collection into its own folder so repeat runs never overwrite.
for /f "tokens=2 delims==" %%T in ('wmic os get localdatetime /value 2^>nul ^| find "="') do set "TS=%%T"
set "STAMP=!TS:~0,4!-!TS:~4,2!-!TS:~6,2! !TS:~8,2!!TS:~10,2!"
set "DEST=%USERPROFILE%\Desktop\RSAF Results !STAMP!"
mkdir "!DEST!" 2>nul

set /a COPIED=0
for /f "delims=" %%F in ('"!ADB!" shell "ls %REMOTE_DIR% 2^>/dev/null" 2^>nul') do (
    set "NAME=%%F"
    set "NAME=!NAME:`r=!"
    echo     !NAME!
    "!ADB!" pull "%REMOTE_DIR%/!NAME!" "!DEST!\!NAME!" >nul 2>&1
    if not errorlevel 1 (
        set /a COPIED+=1
    ) else (
        echo       ^(could not copy this one^)
    )
)

if !COPIED! EQU 0 (
    rmdir "!DEST!" 2>nul
    call :die "Results were listed but none could be copied." ^
        "Make sure the headset stayed connected, then try again."
)

echo.
echo   [OK]  !COPIED! file(s) copied
echo.
echo   Saved to:
echo     !DEST!
echo.
echo   Open the .xls files with Excel or Google Sheets.
echo.
start "" "!DEST!"
pause
exit /b 0

:die
echo.
echo   ------ PROBLEM ------
echo     %~1
echo.
echo     What to do:
if not "%~2"=="" echo       - %~2
if not "%~3"=="" echo       - %~3
if not "%~4"=="" echo       - %~4
echo.
pause
exit 1
