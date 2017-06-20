@echo off
if exist bin\Release\EdBPrepareCarefully.dll (
	robocopy Resources dist\EdBPrepareCarefully\ /e /MIR
	xcopy LICENSE dist\EdBPrepareCarefully\ /Y
	xcopy bin\Release\EdBPrepareCarefully.dll dist\EdBPrepareCarefully\Assemblies\ /Y
	xcopy Libraries\0Harmony.dll dist\EdBPrepareCarefully\Assemblies\ /Y
	xcopy THIRD-PARTY-LICENSES dist\EdBPrepareCarefully\Assemblies\ /Y
	exit /b 0
) else (
	echo Cannot build mod distribution directory. Release build not found in bin\Release directory.
	pause
)
