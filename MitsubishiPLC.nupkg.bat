@echo off

REM 如果有輸入參數就使用參數，否則自動產生版本號
if "%~1"=="" (
    REM 使用 PowerShell 產生版本號：YYYY.M.D
    for /f %%i in ('powershell -Command "Get-Date -Format 'yyyy.M.d'"') do set VERSION=%%i
) else (
    set VERSION=%~1
)

echo.
echo Building MitsubishiPLC package version: %VERSION%
echo.

dotnet pack MitsubishiPLC/MitsubishiPLC.csproj ^
  -c Release ^
  -o ./ ^
  /p:PackageVersion=%VERSION% ^
  /p:PackageId=MitsubishiPLC ^
  /p:PackageLicenseExpression=MIT ^
  /p:RepositoryType=git ^
  /p:PackageTags="Mitsubishi PLC MELSEC MX-Component 三菱"

dir *.nupkg

@if not defined VisualStudioVersion (Pause)

