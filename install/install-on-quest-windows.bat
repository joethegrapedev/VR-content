@echo off
setlocal EnableDelayedExpansion
REM ===========================================================================
REM  RSAF VR Warehouse Safety - one-click installer for Windows.
REM
REM  Double-click this file. It finds (or downloads) the Android tools, finds
REM  (or downloads) the app, then installs it onto a connected Quest headset.
REM
REM  Nothing is installed system-wide. Anything downloaded lands in a
REM  "platform-tools" folder next to this script and can be deleted afterwards.
REM ===========================================================================

set "APP_NAME=RSAF VR Warehouse Safety"
set "APK_FILENAME=RSAF_VRWarehouse.apk"
set "PACKAGE_ID=com.sp.fyp3a25.rsafvrwarehouse"
set "RELEASE_REPO=joethegrapedev/VR-content"
set "APK_URL=https://github.com/%RELEASE_REPO%/releases/latest/download/%APK_FILENAME%"
set "TOOLS_URL=https://dl.google.com/android/repository/platform-tools-latest-windows.zip"
set "DEVICE_WAIT_TRIES=60"

cd /d "%~dp0"

cls
echo.
echo   %APP_NAME%
echo   Installer for Meta Quest - Windows
echo.
echo   Before you start:
echo     1. Developer Mode must be turned on for the headset
echo        (done once, in the Meta Horizon phone app)
echo     2. The headset must be plugged into this PC with a USB-C cable
echo     3. Put the headset on when prompted, to approve the connection
echo.

REM ------------------------------------------------------------ android tools
echo ==^> Checking for Android device tools
set "ADB="
where adb >nul 2>&1 && for /f "delims=" %%A in ('where adb') do if not defined ADB set "ADB=%%A"
if not defined ADB if exist "%~dp0platform-tools\adb.exe" set "ADB=%~dp0platform-tools\adb.exe"
if not defined ADB if exist "%LOCALAPPDATA%\Android\Sdk\platform-tools\adb.exe" set "ADB=%LOCALAPPDATA%\Android\Sdk\platform-tools\adb.exe"

if not defined ADB (
    echo     Not found on this PC.
    echo.
    echo ==^> Downloading Android device tools ^(about 10 MB, one time only^)
    powershell -NoProfile -ExecutionPolicy Bypass -Command ^
        "try { [Net.ServicePointManager]::SecurityProtocol = 'Tls12'; Invoke-WebRequest -Uri '%TOOLS_URL%' -OutFile 'platform-tools.zip' -UseBasicParsing; Expand-Archive -Path 'platform-tools.zip' -DestinationPath '.' -Force; Remove-Item 'platform-tools.zip' -Force } catch { exit 1 }"
    if errorlevel 1 (
        call :die "Could not download the Android tools from Google." ^
            "Check that this PC is connected to the internet." ^
            "If you are on a restricted work network, try a home or mobile connection."
    )
    if exist "%~dp0platform-tools\adb.exe" set "ADB=%~dp0platform-tools\adb.exe"
)

if not defined ADB (
    call :die "Android tools could not be set up." ^
        "Delete the 'platform-tools' folder next to this script and run it again."
)
echo     Found: !ADB!

REM ------------------------------------------------------------ the app file
echo.
echo ==^> Locating the app
set "APK="
if exist "%~dp0%APK_FILENAME%" set "APK=%~dp0%APK_FILENAME%"
if not defined APK if exist "%~dp0..\%APK_FILENAME%" set "APK=%~dp0..\%APK_FILENAME%"
if not defined APK if exist "%USERPROFILE%\Downloads\%APK_FILENAME%" set "APK=%USERPROFILE%\Downloads\%APK_FILENAME%"

if not defined APK (
    echo     No local copy of %APK_FILENAME% found.
    echo.
    echo ==^> Downloading %APP_NAME% ^(about 1 GB - this can take a while^)
    echo     Source: %APK_URL%
    powershell -NoProfile -ExecutionPolicy Bypass -Command ^
        "try { [Net.ServicePointManager]::SecurityProtocol = 'Tls12'; $ProgressPreference='Continue'; Invoke-WebRequest -Uri '%APK_URL%' -OutFile '%APK_FILENAME%' -UseBasicParsing } catch { exit 1 }"
    if errorlevel 1 (
        call :die "Could not download the app." ^
            "Check this PC's internet connection." ^
            "Or download %APK_FILENAME% manually from https://github.com/%RELEASE_REPO%/releases/latest" ^
            "then put it in the same folder as this script and run it again."
    )
    set "APK=%~dp0%APK_FILENAME%"
)
echo     Found: !APK!

for %%I in ("!APK!") do set /a APK_MB=%%~zI / 1048576
echo     Size: !APK_MB! MB
if !APK_MB! LSS 100 (
    call :die "%APK_FILENAME% is only !APK_MB! MB, far too small to be the real app." ^
        "The download was probably interrupted." ^
        "Delete that file and run this script again."
)

REM ------------------------------------------------------------ headset
echo.
echo ==^> Connecting to the headset
"!ADB!" start-server >nul 2>&1
echo     Waiting for a headset... plug it into this PC with a USB-C cable.

set /a TRIES=0
:wait_loop
set "STATE="
for /f "skip=1 tokens=1,2" %%A in ('"!ADB!" devices 2^>nul') do if not defined STATE if not "%%B"=="" set "STATE=%%B"

if "!STATE!"=="device" goto device_ready
if "!STATE!"=="unauthorized" (
    set /a REMIND=!TRIES! %% 5
    if !REMIND! EQU 0 (
        echo     Headset found, but debugging has not been approved yet.
        echo     Put the headset ON and tap 'Allow' on the prompt inside it.
    )
)
set /a TRIES+=1
if !TRIES! GEQ %DEVICE_WAIT_TRIES% goto device_timeout
timeout /t 2 /nobreak >nul
goto wait_loop

:device_timeout
call :die "No headset was ready in time." ^
    "Check the USB-C cable is firmly connected to both the PC and the headset." ^
    "Use a cable that supports data - some charge-only cables will not work." ^
    "Confirm Developer Mode is on for this headset in the Meta Horizon phone app." ^
    "Put the headset on and look for an 'Allow USB debugging' prompt."

:device_ready
echo     Headset connected and authorised.

REM ------------------------------------------------------------ install
echo.
echo ==^> Installing %APP_NAME%
echo     This takes several minutes for a file this size. Do not unplug the cable.
echo.
"!ADB!" install -r -g "!APK!" > "%TEMP%\rsaf_install.txt" 2>&1
type "%TEMP%\rsaf_install.txt"

findstr /i "Success" "%TEMP%\rsaf_install.txt" >nul 2>&1
if errorlevel 1 (
    findstr /i "INSTALL_FAILED_INSUFFICIENT_STORAGE" "%TEMP%\rsaf_install.txt" >nul 2>&1
    if not errorlevel 1 (
        call :die "The headset does not have enough free space." ^
            "The app needs about 2 GB free. Delete some apps or videos on the headset and try again."
    )
    findstr /i "INSTALL_FAILED_UPDATE_INCOMPATIBLE INSTALL_FAILED_VERSION_DOWNGRADE" "%TEMP%\rsaf_install.txt" >nul 2>&1
    if not errorlevel 1 (
        call :die "A different build of this app is already installed." ^
            "Remove the old one first, then run this script again:" ^
            "  adb uninstall %PACKAGE_ID%"
    )
    call :die "The installation did not complete." ^
        "Check the messages above for the reason." ^
        "Make sure the headset stayed awake and connected throughout."
)
del "%TEMP%\rsaf_install.txt" >nul 2>&1

echo.
echo   [OK]  INSTALLED SUCCESSFULLY
echo.
echo   To open it in the headset:
echo     1. Put the headset on
echo     2. Open the App Library
echo     3. Change the filter at the top right to "Unknown Sources"
echo     4. Launch RSAF_VRWarehouse
echo.
echo   You can unplug the cable now - the app runs entirely on the headset.
echo.
pause
exit /b 0

REM ------------------------------------------------------------ helpers
:die
echo.
echo   ------ PROBLEM ------
echo     %~1
echo.
echo     What to do:
if not "%~2"=="" echo       - %~2
if not "%~3"=="" echo       - %~3
if not "%~4"=="" echo       - %~4
if not "%~5"=="" echo       - %~5
echo.
pause
exit 1
