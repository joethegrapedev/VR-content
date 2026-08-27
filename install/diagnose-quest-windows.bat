@echo off
setlocal EnableDelayedExpansion
REM ===========================================================================
REM  RSAF VR Warehouse Safety - collect diagnostics from a Quest headset.
REM
REM  Run this if the app will not install or will not launch. It gathers what
REM  is needed to work out why and saves it to a single file on your Desktop,
REM  which you can send on. It changes nothing on the headset.
REM ===========================================================================

set "PACKAGE_ID=com.sp.fyp3a25.rsafvrwarehouse"
set "CAPTURE_SECONDS=25"

cd /d "%~dp0"
cls
echo.
echo   RSAF VR - headset diagnostics
echo.
echo   This reads information from the headset. It changes nothing.
echo.

set "ADB="
where adb >nul 2>&1 && for /f "delims=" %%A in ('where adb') do if not defined ADB set "ADB=%%A"
if not defined ADB if exist "%~dp0platform-tools\adb.exe" set "ADB=%~dp0platform-tools\adb.exe"
if not defined ADB if exist "%LOCALAPPDATA%\Android\Sdk\platform-tools\adb.exe" set "ADB=%LOCALAPPDATA%\Android\Sdk\platform-tools\adb.exe"
if not defined ADB (
    call :die "The Android tools are not set up on this PC." ^
        "Run the installer first - it downloads them automatically."
)

for /f "tokens=2 delims==" %%T in ('wmic os get localdatetime /value 2^>nul ^| find "="') do set "TS=%%T"
set "REPORT=%USERPROFILE%\Desktop\RSAF-diagnostics-!TS:~0,4!-!TS:~4,2!-!TS:~6,2!-!TS:~8,4!.txt"

echo RSAF VR Warehouse - diagnostic report> "!REPORT!"
echo Generated: %DATE% %TIME%>> "!REPORT!"
echo.>> "!REPORT!"

echo ==^> Checking the headset connection
"!ADB!" start-server >nul 2>&1

set "STATE="
for /f "skip=1 tokens=1,2" %%A in ('"!ADB!" devices 2^>nul') do if not defined STATE if not "%%B"=="" set "STATE=%%B"

if "!STATE!"=="unauthorized" (
    call :die "The headset is connected but USB debugging is not approved." ^
        "Put the headset on and tap Allow on the prompt inside it, then run this again."
)
if not "!STATE!"=="device" (
    call :die "No headset detected." ^
        "Plug the headset into this PC with a USB-C cable that carries data." ^
        "Many charge-only cables give exactly this result."
)
echo     Connected.
echo Headset connection: OK>> "!REPORT!"

echo.>> "!REPORT!"
echo -- Headset -->> "!REPORT!"
for %%P in (ro.product.model ro.product.device ro.build.version.release ro.build.version.sdk) do (
    for /f "delims=" %%V in ('"!ADB!" shell getprop %%P 2^>nul') do echo %%P = %%V>> "!REPORT!"
)

echo.>> "!REPORT!"
echo -- Storage -->> "!REPORT!"
"!ADB!" shell df -h /storage/emulated/0 >> "!REPORT!" 2>&1

echo.
echo ==^> Checking the app
echo.>> "!REPORT!"
echo -- Installed app -->> "!REPORT!"

set "INSTALLED="
for /f "delims=" %%L in ('"!ADB!" shell pm list packages 2^>nul ^| findstr /c:"%PACKAGE_ID%"') do set "INSTALLED=1"

if not defined INSTALLED (
    echo Installed: NO ^<-- the app is not on this headset>> "!REPORT!"
    echo.
    echo   The app is not installed on this headset.
    echo   Run the installer first, then run this again.
    echo.
    echo   Report saved to:
    echo     !REPORT!
    echo.
    pause
    exit /b 0
)

echo Installed: YES>> "!REPORT!"
"!ADB!" shell dumpsys package %PACKAGE_ID% 2>nul | findstr /r "versionName versionCode primaryCpuAbi firstInstallTime lastUpdateTime codePath" >> "!REPORT!"
echo     Installed.

echo.
echo ==^> Launching the app and recording what happens
echo     Put the headset ON now and watch it.
echo     Recording for %CAPTURE_SECONDS% seconds...

echo.>> "!REPORT!"
echo -- Launch attempt -->> "!REPORT!"
"!ADB!" shell am force-stop %PACKAGE_ID% >nul 2>&1
"!ADB!" logcat -c >nul 2>&1

REM Starting through the activity manager bypasses the Oculus launcher. If the
REM app runs this way but not from the library, the launcher is refusing it
REM rather than the app crashing - the most useful thing to establish.
"!ADB!" shell monkey -p %PACKAGE_ID% -c android.intent.category.LAUNCHER 1 >> "!REPORT!" 2>&1

start /b "" "!ADB!" logcat -v time > "%TEMP%\rsaf_logcat.txt" 2>&1
timeout /t %CAPTURE_SECONDS% /nobreak >nul
taskkill /f /im adb.exe >nul 2>&1
"!ADB!" start-server >nul 2>&1

echo.>> "!REPORT!"
echo -- Is it running now? -->> "!REPORT!"
set "PID="
for /f "delims=" %%P in ('"!ADB!" shell pidof %PACKAGE_ID% 2^>nul') do set "PID=%%P"
if defined PID (
    echo RUNNING ^(pid !PID!^) - the app started via the activity manager.>> "!REPORT!"
    echo If it will not start from the headset's library, the launcher is refusing it,>> "!REPORT!"
    echo not the app crashing.>> "!REPORT!"
) else (
    echo NOT RUNNING - the app did not stay up.>> "!REPORT!"
)

echo.>> "!REPORT!"
echo -- Relevant log lines -->> "!REPORT!"
findstr /i /r "rsaf unity il2cpp oculus FATAL AndroidRuntime ActivityManager crash abort" "%TEMP%\rsaf_logcat.txt" >> "!REPORT!" 2>nul
del "%TEMP%\rsaf_logcat.txt" >nul 2>&1

echo.
echo   [OK]  Report saved
echo.
echo     !REPORT!
echo.
echo   Send that file back. It contains no personal information -
echo   just the headset model, the app version and the launch log.
echo.
explorer /select,"!REPORT!"
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
echo.
pause
exit 1
