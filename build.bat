@echo off
title The Voider build
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0tools\build.ps1" %*
echo.
pause
