@ECHO OFF
SETLOCAL


echo Running restore...
"%ProgramFiles(x86)%\Microsoft Visual Studio\2019\Enterprise\MSBuild\Current\Bin\msbuild.exe" ^
    %~dp0native-windows.builds ^
    -t:Restore ^
    -v:m ^
    -m

REM echo Running build...
REM "%ProgramFiles(x86)%\Microsoft Visual Studio\2019\Enterprise\MSBuild\Current\Bin\msbuild.exe" ^
REM     %~dp0native-windows.builds ^
REM     -graph ^
REM     -isolate ^
REM     -v:m ^
REM     -m -bl ^
REM     -clp:Summary


C:\Users\namc\Downloads\buildxl.net472.0.1.0-20190603.2\bxl.exe /c:%~dp0..\config.dsc ^
    /disableProcessRetryOnResourceExhaustion+ ^
    /useHardlinks-

REM using /useHardlinks to workaround ACLs issue
