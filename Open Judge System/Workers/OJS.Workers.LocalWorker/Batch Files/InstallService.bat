NET STOP "OJS Local Worker Service"
sc delete "OJS Local Worker Service"
timeout 10
CD %~dp0
C:\Windows\Microsoft.NET\Framework\v4.0.30319\installutil "..\OJS.Workers.LocalWorker.exe"
sc failure "OJS Local Worker Service" actions= restart/60000/restart/60000/""/60000 reset= 7200
NET START "OJS Local Worker Service"

@echo off
SC QUERY "OJS Local Worker Monitoring Service" > NUL
IF ERRORLEVEL 1060 GOTO MISSING
	for /F "tokens=3 delims=: " %%H in ('sc query "OJS Local Worker Monitoring Service" ^| findstr "STATE"') do (
	  if /I "%%H" NEQ "RUNNING" (
		NET START "OJS Local Worker Monitoring Service"
	  )
	)
GOTO END

:MISSING
	CD %~dp0
	C:\Windows\Microsoft.NET\Framework\v4.0.30319\installutil "..\..\..\..\OJS.Workers.LocalWorkerMonitoring\bin\Debug\OJS.Workers.LocalWorkerMonitoring.exe"
	NET START "OJS Local Worker Monitoring Service"
:END

pause