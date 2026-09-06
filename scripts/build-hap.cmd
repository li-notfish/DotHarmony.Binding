@echo off
set "PATH=C:\Program Files\Huawei\DevEco Studio\jbr\bin;C:\Program Files\Huawei\DevEco Studio\tools\node;%PATH%"
set "DEVECO_SDK_HOME=C:\Program Files\Huawei\DevEco Studio\sdk"
set "NODE=C:\Program Files\Huawei\DevEco Studio\tools\node\node.exe"
set "HVIGOR=C:\Program Files\Huawei\DevEco Studio\tools\hvigor\bin\hvigorw.js"
cd /d "%~dp0..\samples\HarmonyHost"
"%NODE%" "%HVIGOR%" --mode module -p module=entry@default -p product=default assembleHap --no-daemon %*
