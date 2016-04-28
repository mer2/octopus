@echo off
set ProjectName=octopus.AdminWeb
set PublishDestination=D:\deploy\huyu\www.admin.huyu123.com

:build
echo Publishing website...
rd /s /q %PublishDestination%
C:\Windows\Microsoft.NET\Framework\v4.0.30319\msbuild.exe %ProjectName%.csproj /p:Platform=AnyCPU;AutoParameterizationWebConfigConnectionStrings=False;Configuration=Release;PublishDestination=%PublishDestination% /t:PublishToFileSystem
if not errorlevel 0 goto error
goto end

:error
echo Build error detected, stopping.

:end
echo Done.

pause