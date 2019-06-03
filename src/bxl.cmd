@ECHO OFF
SETLOCAL

set localCache=%~dp0..\artifacts\tmp\native-windows.build.cache
set additionalArgs=
REM if exist %localCache%  (
REM     set additionalArgs=%additionalArgs% -inputResultsCaches:%localCache%
REM )

echo Running restore...
"%ProgramFiles(x86)%\Microsoft Visual Studio\2019\Enterprise\MSBuild\Current\Bin\msbuild.exe" ^
    %~dp0native-windows.builds ^
    -t:Restore ^
    -v:m ^
    -m

echo Running build...
"%ProgramFiles(x86)%\Microsoft Visual Studio\2019\Enterprise\MSBuild\Current\Bin\msbuild.exe" ^
    %~dp0native-windows.builds ^
    -graph ^
    -isolate ^
    -outputResultsCache:%~dp0..\artifacts\tmp\native-windows.build.cache ^
    -v:m ^
    -m ^
    %additionalArgs%
