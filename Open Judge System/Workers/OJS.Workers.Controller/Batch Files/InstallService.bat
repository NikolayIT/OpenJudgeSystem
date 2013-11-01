NET STOP "TOJS Controller Service"
sc delete "TOJS Controller Service"
timeout 10
CD %~dp0
C:\Windows\Microsoft.NET\Framework\v4.0.30319\installutil "..\OJS.Workers.Controller.exe"
NET START "TOJS Controller Service"
pause