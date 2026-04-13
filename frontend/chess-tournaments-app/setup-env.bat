@echo off
REM Chess Tournaments - Environment Setup Script (Windows)
REM This script helps you set up your local environment configuration

echo.
echo ^♔ Chess Tournaments - Environment Setup ^♔
echo.

set ENV_DIR=src\environments
set EXAMPLE_FILE=%ENV_DIR%\environment.example.ts
set LOCAL_FILE=%ENV_DIR%\environment.local.ts

REM Check if example file exists
if not exist "%EXAMPLE_FILE%" (
    echo ❌ Error: %EXAMPLE_FILE% not found!
    exit /b 1
)

REM Check if local file already exists
if exist "%LOCAL_FILE%" (
    echo ⚠️  Warning: %LOCAL_FILE% already exists!
    set /p REPLY="Do you want to overwrite it? (y/N): "
    if /i not "%REPLY%"=="y" (
        echo Setup cancelled.
        exit /b 0
    )
)

REM Copy example to local
echo 📄 Creating %LOCAL_FILE% from example...
copy "%EXAMPLE_FILE%" "%LOCAL_FILE%" >nul

echo.
echo ✅ Environment file created successfully!
echo.
echo 📝 Next steps:
echo 1. Edit %LOCAL_FILE%
echo 2. Configure your OIDC settings (issuer, clientId, etc.)
echo 3. Run: npm run start:local
echo.
echo 📖 For detailed configuration guide, see ENVIRONMENT.md
echo.
pause
