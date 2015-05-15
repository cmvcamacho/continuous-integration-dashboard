@echo off

:: Check command syntax
IF '%1'=='' (
	echo Usage: Install.bat [environment]
	echo
	exit
)


:: Gets the current path
set rootDir=%~dp0

:: Determin what architecture EXEs should run on.
if %PROCESSOR_ARCHITECTURE%==x86 (
 set msbuild=C:\Windows\Microsoft.NET\Framework\v4.0.30319\msbuild.exe
) else (
 set msbuild=C:\Windows\Microsoft.NET\Framework64\v4.0.30319\msbuild.exe
)

%msbuild% default.build /t:Install /p:Environment=%1 /p:VisualStudioVersion=12.0
