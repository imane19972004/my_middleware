@echo off
chcp 65001 >nul
color 0A
title Let's Go Biking - Launcher

echo.
echo ╔════════════════════════════════════════════════════════╗
echo ║     LET'S GO BIKING - LANCEMENT DE TOUS LES SERVEURS  ║
echo ╚════════════════════════════════════════════════════════╝
echo.

REM Configuration des chemins (ADAPTEZ SELON VOTRE STRUCTURE)
set CACHE_SERVER=CachingServer\bin\Debug\CachingServer.exe
set PROXY_SERVER=ProxyServer\bin\Debug\ProxyServer.exe
set ROUTING_SERVER=RoutingServer\bin\Debug\RoutingServer.exe
set NOTIF_SERVICE=NotificationService\bin\Debug\NotificationService.exe

REM Vérification de l'existence des fichiers
if not exist "%CACHE_SERVER%" (
    echo [ERREUR] CachingServer.exe introuvable !
    echo Chemin: %CACHE_SERVER%
    pause
    exit /b 1
)

echo [1/5] Lancement CachingServer...
start "CachingServer" "%CACHE_SERVER%"
timeout /t 3 >nul

echo [2/5] Lancement ProxyServer...
start "ProxyServer" "%PROXY_SERVER%"
timeout /t 3 >nul

echo [3/5] Lancement RoutingServer...
start "RoutingServer" "%ROUTING_SERVER%"
timeout /t 3 >nul

echo [4/5] Lancement NotificationService...
start "NotificationService" "%NOTIF_SERVICE%"
timeout /t 2 >nul

echo [5/5] Ouverture du navigateur...
start "" "LetsGoBikingWeb\index.html"

echo.
echo ╔════════════════════════════════════════════════════════╗
echo ║                  TOUS LES SERVEURS LANCÉS !           ║
echo ║  Vous pouvez maintenant utiliser l'application web    ║
echo ╚════════════════════════════════════════════════════════╝
echo.
echo Appuyez sur une touche pour fermer tous les serveurs...
pause >nul

REM Fermer toutes les fenêtres
taskkill /FI "WINDOWTITLE eq CachingServer*" /F >nul 2>&1
taskkill /FI "WINDOWTITLE eq ProxyServer*" /F >nul 2>&1
taskkill /FI "WINDOWTITLE eq RoutingServer*" /F >nul 2>&1
taskkill /FI "WINDOWTITLE eq NotificationService*" /F >nul 2>&1

echo Tous les serveurs ont été arrêtés.