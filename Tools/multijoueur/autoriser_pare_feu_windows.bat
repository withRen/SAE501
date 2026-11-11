@echo off
rem Autorise le multijoueur LAN de The Last Escapor dans le pare-feu Windows (UDP 7777 : jeu, UDP 47777 : découverte des parties).
rem A lancer UNE FOIS sur le PC qui héberge, clic droit > "Exécuter en tant qu'administrateur".
netsh advfirewall firewall delete rule name="Last Escapor jeu" >nul 2>&1
netsh advfirewall firewall delete rule name="Last Escapor decouverte" >nul 2>&1
netsh advfirewall firewall add rule name="Last Escapor jeu" dir=in action=allow protocol=UDP localport=7777 profile=any
netsh advfirewall firewall add rule name="Last Escapor decouverte" dir=in action=allow protocol=UDP localport=47777 profile=any
echo.
echo Regles ajoutees. Relancez le jeu puis HEBERGER.
pause
