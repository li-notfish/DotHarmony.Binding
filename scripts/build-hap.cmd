@echo off
rem Locate DevEco Studio: DEVECO_HOME > D:\Program Files\Huawei\DevEco Studio > C:\Program Files\...
rem Host project dir: HOST_DIR override, sample name/path argument, or default samples/HarmonyHost
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

rem Resolve host dir:
rem   1) HOST_DIR environment variable
rem   2) first argument as a sample name, sample project dir, or staged host dir
rem   3) repo default samples/HarmonyHost
set "HOST=%HOST_DIR%"
if "%HOST%"=="" if not "%~1"=="" (
  if exist "%~dp0..\samples\dotnet\%~1\obj\harmony\host\AppScope\app.json5" (
    set "HOST=%~dp0..\samples\dotnet\%~1\obj\harmony\host"
  ) else if exist "%~1\obj\harmony\host\AppScope\app.json5" (
    set "HOST=%~1\obj\harmony\host"
  ) else if exist "%~1\AppScope\app.json5" (
    set "HOST=%~1"
  )
)
if "%HOST%"=="" set "HOST=%~dp0..\samples\HarmonyHost"
echo Host: %HOST%
cd /d "%HOST%"
"%NODE%" "%HVIGOR%" --mode module -p module=entry@default -p product=default assembleHap --no-daemon %*
