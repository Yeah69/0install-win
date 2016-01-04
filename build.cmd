@echo off
::Compiles the source code and then creates an installer.

echo.
call "%~dp0src\build.cmd" Release
if errorlevel 1 pause

if defined signing_cert_path (
echo.
call "%~dp0src\sign.cmd"
if errorlevel 1 pause
)

::Auto-download solver if missing
if not exist "%~dp0bundled\Solver" (
  echo.
  pushd "%~dp0bundled"
  powershell -NonInteractive -Command - < download-solver.ps1
  popd
)

echo.
call "%~dp0nuget\build.cmd" %*
if errorlevel 1 pause

echo.
call "%~dp0installer\build.cmd" %*
if errorlevel 1 pause

if defined signing_cert_path (
echo.
call "%~dp0installer\sign.cmd"
if errorlevel 1 pause
)

echo.
call "%~dp0doc\build.cmd" %*
if errorlevel 1 pause