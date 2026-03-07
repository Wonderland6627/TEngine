Cd /d %~dp0
echo %CD%

set WORKSPACE=../../
set LUBAN_DLL=%WORKSPACE%/Tools/Luban/Luban.dll
set CONF_ROOT=.
set DATA_OUTPATH=%WORKSPACE%/../piratecat_slime_express/config/luban/data
set CODE_OUTPATH=%WORKSPACE%/../piratecat_slime_express/config/luban/gen

dotnet %LUBAN_DLL% ^
    -t server ^
    -c javascript-json ^
    -d json ^
    --conf %CONF_ROOT%\luban.conf ^
    -x outputCodeDir=%CODE_OUTPATH% ^
    -x outputDataDir=%DATA_OUTPATH% 
pause

