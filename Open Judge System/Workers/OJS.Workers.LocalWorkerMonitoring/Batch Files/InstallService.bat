NET STOP "OJS Local Worker Monitoring Service"
sc delete "OJS Local Worker Monitoring Service"
timeout 10
CD %~dp0
C:\Windows\Microsoft.NET\Framework\v4.0.30319\installutil "..\OJS.Workers.LocalWorkerMonitoring.exe"
NET START "OJS Local Worker Monitoring Service"
pause