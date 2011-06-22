@echo off
if exist "%ProgramFiles%\Fiddler2\Fiddler.exe" set "FIDDLERPATH=%ProgramFiles%\Fiddler2"
if exist "%ProgramFiles(x86)%\Fiddler2\Fiddler.exe" set "FIDDLERPATH=%ProgramFiles(x86)%\Fiddler2"
if not defined FIDDLERPATH echo Can't find Fiddler in Program Files & exit

csc /d:TRACE /target:library /out:"%USERPROFILE%\Documents\fiddler2\scripts\AutoCapture.dll" AutoCapture.cs Win32File.cs /reference:"%FIDDLERPATH%\Fiddler.exe"
