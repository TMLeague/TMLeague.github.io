@echo off
SETLOCAL EnableExtensions

echo ======================================================
echo [1/3] Import started...
echo ======================================================

set ASPNETCORE_ENVIRONMENT=Bash && set DOTNET_ENVIRONMENT=Bash && dotnet run --project TMGameImporter.csproj -c Release --no-launch-profile

if %ERRORLEVEL% NEQ 0 (
    echo.
    echo [ERR] Import failed.
    pause
    exit /b %ERRORLEVEL%
)

echo.
echo ======================================================
echo [2/3] Preparing git changes...
echo ======================================================

git add .

git diff --cached --quiet
if %ERRORLEVEL% EQU 0 (
    echo [INFO] There are no changes in the repo.
    goto end
)

echo.
echo ======================================================
echo [3/3] Pushing changes...
echo ======================================================

git commit -m "Game imported automatically"
git push origin master

:end
echo.
echo Finished successfully!
timeout /t 5