@echo off
setlocal EnableDelayedExpansion
rem Locate DevEco Studio: DEVECO_HOME > D:\Program Files\Huawei\DevEco Studio > C:\Program Files\...
rem Host project dir: HOST_DIR override, sample name/path argument.
rem The shared template samples\HarmonyHost is NOT deployable (see AGENTS.md); staging into
rem <sample>\obj\harmony\host is done by the MSBuild HarmonyStageHost target.
if not "%~2"=="" (
    echo Error: build-hap.cmd accepts only one optional sample name or host directory.
    exit /b 2
)
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
set "NODE_HOME=%DEVECO%\tools\node"
set "NODE=%DEVECO%\tools\node\node.exe"
set "HVIGOR=%DEVECO%\tools\hvigor\bin\hvigorw.js"
set "OHPM=%DEVECO%\tools\ohpm\bin\ohpm.bat"
if not exist "%NODE%" (
    echo Error: node.exe not found at "%NODE%".
    exit /b 1
)
if not exist "%HVIGOR%" (
    echo Error: hvigorw.js not found at "%HVIGOR%".
    exit /b 1
)

rem Resolve host dir:
rem   1) HOST_DIR environment variable (set by HarmonyBuildHap/HarmonyDeploy)
rem   2) first argument as a sample name, sample project dir, or staged host dir
rem   3) otherwise fail: never fall back to the shared, non-deployable template
set "HOST=%HOST_DIR%"
if "%HOST%"=="" if not "%~1"=="" (
  if exist "%~dp0..\samples\dotnet\%~1\obj\harmony\host\AppScope\app.json5" (
    set "HOST=%~dp0..\samples\dotnet\%~1\obj\harmony\host"
  ) else if exist "%~1\obj\harmony\host\AppScope\app.json5" (
    set "HOST=%~1\obj\harmony\host"
  ) else if exist "%~1\AppScope\app.json5" (
    set "HOST=%~1"
  ) else (
    echo Error: "%1" is neither a sample name with a staged host nor a host directory.
    echo        Run: dotnet build samples\dotnet\%1 -t:HarmonyStageHost
    exit /b 2
  )
)
if "%HOST%"=="" (
  echo Error: no staged host given.
  echo.
  echo   Use one of:
  echo     dotnet build samples\dotnet^<Sample^> -t:HarmonyRun     ^(recommended^)
  echo     dotnet build samples\dotnet^<Sample^> -t:HarmonyStageHost
  echo     set HOST_DIR=samples\dotnet^<Sample^>\obj\harmony\host
  echo     scripts\build-hap.cmd ^<Sample^>
  echo.
  echo   The shared template samples\HarmonyHost is not deployable by itself.
  exit /b 2
)

if not exist "%HOST%\AppScope\app.json5" (
  echo Error: "%HOST%" is not a staged host ^(missing AppScope\app.json5^).
  echo        Run: dotnet build ^<project^> -t:HarmonyStageHost
  exit /b 2
)

rem Refuse the raw shared template unless explicitly allowed (debugging only).
for %%I in ("%HOST%") do set "HOST_ABS=%%~fI"
for %%I in ("%~dp0..\samples\HarmonyHost") do set "TEMPLATE_ABS=%%~fI"
if /I "%HOST_ABS%"=="%TEMPLATE_ABS%" if not "%HARMONY_ALLOW_SHARED_HOST%"=="1" (
  echo Error: samples\HarmonyHost is the shared template, not a per-app host.
  echo        Use -t:HarmonyRun, or set HARMONY_ALLOW_SHARED_HOST=1 for a deliberate template build.
  exit /b 2
)

echo Host: %HOST%
if exist "%HOST%\build-profile.json5" (
  for /f "tokens=2 delims=:" %%A in ('findstr /c:"compatibleSdkVersion" "%HOST%\build-profile.json5"') do echo SDK compatible:%%A
)

rem ArkTS 依赖：模板不携带 oh_modules（.gitignore），首次构建必须 ohpm install。
if not exist "%HOST%\oh_modules" if not "%HARMONY_SKIP_OHPM_INSTALL%"=="1" (
  if not exist "%OHPM%" (
    echo Error: "%HOST%\oh_modules" is missing and ohpm was not found at "%OHPM%".
    echo        Install DevEco Studio which bundles ohpm, or run ohpm install manually.
    exit /b 2
  )
  echo oh_modules missing; running ohpm install in "%HOST%" ...
  pushd "%HOST%"
  call "%OHPM%" install --all
  set "OHPM_EXIT=!ERRORLEVEL!"
  popd
  if not "!OHPM_EXIT!"=="0" (
    echo Error: ohpm install failed with exit code !OHPM_EXIT!.
    exit /b 1
  )
)

cd /d "%HOST%"
"%NODE%" "%HVIGOR%" --mode module -p module=entry@default -p product=default assembleHap --no-daemon
set "HVIGOR_EXIT=%ERRORLEVEL%"
exit /b %HVIGOR_EXIT%
