@echo off
rem DevEco Studio 定位顺序：DEVECO_HOME > D:\Program Files\Huawei\DevEco Studio > C:\Program Files\...
set "DEVECO=%DEVECO_HOME%"
if "%DEVECO%"=="" if exist "D:\Program Files\Huawei\DevEco Studio\tools\hvigor\bin\hvigorw.js" set "DEVECO=D:\Program Files\Huawei\DevEco Studio"
if "%DEVECO%"=="" if exist "C:\Program Files\Huawei\DevEco Studio\tools\hvigor\bin\hvigorw.js" set "DEVECO=C:\Program Files\Huawei\DevEco Studio"
if "%DEVECO%"=="" (
    echo 错误: 找不到 DevEco Studio。请设置环境变量 DEVECO_HOME 指向安装目录后重试
    exit /b 1
)
echo DevEco: %DEVECO%

set "PATH=%DEVECO%\jbr\bin;%DEVECO%\tools\node;%PATH%"
set "DEVECO_SDK_HOME=%DEVECO%\sdk"
set "NODE=%DEVECO%\tools\node\node.exe"
set "HVIGOR=%DEVECO%\tools\hvigor\bin\hvigorw.js"
cd /d "%~dp0..\samples\HarmonyHost"
"%NODE%" "%HVIGOR%" --mode module -p module=entry@default -p product=default assembleHap --no-daemon %*
