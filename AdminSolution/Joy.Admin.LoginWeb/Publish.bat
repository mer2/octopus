@echo off
set ProjectName=Joy.Admin.LoginWeb
set PublishDestination=D:\deploy\octopus\login.admin.devfx.net

:build
echo Publishing website...
C:\Windows\Microsoft.NET\Framework\v4.0.30319\msbuild.exe %ProjectName%.csproj /p:Platform=AnyCPU;AutoParameterizationWebConfigConnectionStrings=False;Configuration=Release;PublishDestination=%PublishDestination% /t:PublishToFileSystem
if not errorlevel 0 goto error
goto end

:error
echo Build error detected, stopping.

:end
echo Done.

pause