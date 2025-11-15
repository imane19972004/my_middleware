@echo off
echo ================================================
echo   LET'S GO BIKING - DEMARRAGE COMPLET
echo ================================================
echo.

REM 1. CachingServer
echo [1/5] Lancement CachingServer...
cd CachingServer\bin\Debug
start "CachingServer" CachingServer.exe
cd ..\..\..
timeout /t 2 >nul

REM 2. ProxyServer
echo [2/5] Lancement ProxyServer...
cd ProxyServer\bin\Debug
start "ProxyServer" ProxyServer.exe
cd ..\..\..
timeout /t 2 >nul

REM 3. RoutingServer
echo [3/5] Lancement RoutingServer...
cd RoutingServer\bin\Debug
start "RoutingServer" RoutingServer.exe
cd ..\..\..
timeout /t 2 >nul

REM 4. NotificationService
echo [4/5] Lancement NotificationService...
cd NotificationService\bin\Debug
start "NotificationService" NotificationService.exe
cd ..\..\..
timeout /t 2 >nul

REM 5. WebServer
echo [5/5] Lancement WebServer...
cd WebServer\bin\Debug
start "WebServer" WebServer.exe
cd ..\..\..

timeout /t 3 >nul

echo.
echo ================================================
echo   TOUS LES SERVEURS SONT LANCES !
echo ================================================
echo.
echo Ouvrez dans votre navigateur :
echo    http://localhost:8080/
echo.
echo Services actifs :
echo    - CachingServer    : http://localhost:8002
echo    - ProxyServer      : http://localhost:8001
echo    - RoutingServer    : http://localhost:8003
echo    - NotificationSrv  : tcp://localhost:61616
echo    - WebServer (HTML) : http://localhost:8080
echo.
echo Pour tout arreter : fermez toutes les fenetres
echo.

REM Ouvrir automatiquement le navigateur
timeout /t 2 >nul
start http://localhost:8080/

pause