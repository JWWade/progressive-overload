@echo off
setlocal

pushd "%~dp0"

echo Building solution...
dotnet build ProgressiveOverload.slnx
if errorlevel 1 goto :build_failed

echo.
echo Starting console application...
dotnet run --project ProgressiveOverload.Console\ProgressiveOverload.Console.csproj --no-build
set "APP_EXIT_CODE=%ERRORLEVEL%"

popd
exit /b %APP_EXIT_CODE%

:build_failed
echo.
echo Build failed. Fix errors and retry.
popd
pause
exit /b 1
