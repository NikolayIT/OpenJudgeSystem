NET STOP "OJS Local Worker Service"
sc delete "OJS Local Worker Service"
timeout 10
CD %~dp0
C:\Windows\Microsoft.NET\Framework\v4.0.30319\installutil "..\OJS.Workers.LocalWorker.exe"
NET START "OJS Local Worker Service"
pause