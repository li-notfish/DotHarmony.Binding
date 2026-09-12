@echo off
rem Locate DevEco Studio: DEVECO_HOME > D:\Program Files\Huawei\DevEco Studio > C:\Program Files\...
rem Host project dir: HOST_DIR override (per-app staged host from targets), default samples/HarmonyHost
set "DEVECO=%DEVECO_HOME%"
if "%DEVECO%"=="" if exist "D:\Program Files\Huawei\DevEco Studio\tools\hvigor\bin\hvigorw.js" set "DEVECO=D:\Program Files\Huawei\DevEco Studio"
if "%DEVECO%"=="" if exist "C:\Program Files\Huawei\DevEco Studio\tools\hvigor\bin\hvigorw.js" set "DEVECO=C:\Program Files\Huawei\DevEco Studio"
if "%DEVECO%"=="" (
    echo Error: DevEco Studio not found. Set DEVECO_HOME to the install dir and retry.
    exit /b 1
)
echo DevEco: %DEVECO%

set "PATH=%DEVECO%\jbr\bin;%DEVECO%\tools\node;%PATH%"
set "DEVECO_SDK_HOME=%DEVECO%\sdk"
set "NODE=%DEVECO%\tools\node\node.exe"
set "HVIGOR=%DEVECO%\tools\hvigor\bin\hvigorw.js"

rem Host dir: repo default samples/HarmonyHost, or HOST_DIR from environment
set "HOST=%HOST_DIR%"
if "%HOST%"=="" set "HOST=%~dp0..\samples\HarmonyHost"
cd /d "%HOST%"
"%NODE%" "%HVIGOR%" --mode module -p module=entry@default -p product=default assembleHap --no-daemon %*
