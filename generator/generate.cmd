@echo off

set jsduckbin=0.tools\jsduck-6.0.0-beta.exe
set etsbin=c2.tools.ExtTS\bin\Debug\c2.tools.ExtTS.exe
set ejspath=%~1
set ejsver=%~2
set ejstoolkit=%~3

if [%ejsver%]==[] (
 set ejsver=6.2.1
)

if [%ejstoolkit%]==[] (
 set ejstoolkit=classic
)

if [%ejspath%]==[] (
 echo Usage: %0 ^<extjs_root^> [^<extjs_version^>] [^<extjs_toolkit^>]
 echo.
 echo Point extjs_root to the folder where the extjs package was extracted i.e.
 echo where the ext-^<version^> folder is.
 echo.
 echo examples:
 echo %0 c:\projects\extjs
 echo   will search for ExtJS sources v6.2.1 classic at 
 echo   c:\projects\extjs\ext-6.2.1\classic\classic\src
 echo.
 echo %0 c:\projects\extjs 6.0.2 modern
 echo   will search for ExtJS sources v6.2.1 classic at 
 echo   c:\projects\extjs\ext-6.0.2\modern\modern\src
 echo.
 pause
 exit /b 0
)

:: ExtJS Toolkit/sources path
set ejstkpath=%ejspath%\ext-%ejsver%\%ejstoolkit%\%ejstoolkit%\src

set ejsdir=ext-%ejsver%-%ejstoolkit%
set srcdir=1.src\%ejsdir%
set jsdocdir=2.docs\%ejsdir%
set outfile=3.out\%ejsdir%.d.ts

:: JSDuck's Version file required for the generator to work
set jsdocversfile=%jsdocdir%\source\Version.html

if not exist %jsduckbin% (
 echo *** Error: JSDuck binary was not found at: %jsduckbin%
 pause
 exit /b 1
)

if not exist %etsbin% (
 echo *** Error: ExtTS binary was not found at: %etsbin%
 echo Please open c2.tools.ExtTS\c2.tools.ExtTS.csproj with Visual Studio and build
 echo  the project before trying to run this script.
 echo Notice the project uses a syntax that requires using VS2015+.
 rem could open the project with: start explorer c2.tools.ExtTS\c2.tools.ExtTS.csproj
 pause
 exit /b 1
)

echo ExtJS root path: %ejspath%
echo ExtJS sources path: %ejstkpath%
echo Source files path: %srcdir%
echo JSDuck output path: %jsdocdir%
echo Output file: %outfile%
echo.
echo Process:
echo * 1^) Cleanup existing files ^(if any^)
echo   2^) Copy source files over
echo   3^) Build documentation with JSDuck
echo   4^) Generate .d.ts off documentation
echo.
echo Press ENTER to cleanup old files ^(if any^) ^(1/4^) . . .
set /p null=

if not exist %ejstkpath% (
 echo *** Error: can't find ExtJS sources on specified path.
 pause
 exit /b 1
)

if not exist 1.src (
 mkdir 1.src
)

if not exist 2.docs (
 mkdir 2.docs
)

if not exist 3.out (
 mkdir 3.out
)

if exist %srcdir% (
 echo *** Warning: src dir already exists at: %srcdir%
 echo Press ENTER to confirm removing it ^(ctrl+c cancels^) . . .
 set /p null=
 
 echo Removing it.
 rmdir /q /s %srcdir%
 echo Removed.
)

if exist %jsdocdir% (
 echo *** Warning: Previous docs build already exists at: %jsdocdir%
 echo Press ENTER to confirm removing it ^(ctrl+c cancels^) . . .
 set /p null=

 echo Removing it.
 rmdir /q /s %jsdocdir%
 echo Removed.
)

if exist %outfile% (
 echo *** Warning: Output file already exists: %outfile%
 echo Press ENTER to confirm removing it ^(ctrl+c cancels^) . . .
 set /p null=

 echo Removing it.
 del /q %outfile%
 echo Removed.
)

echo Process:
echo   1^) Cleanup existing files ^(if any^)
echo * 2^) Copy source files over
echo   3^) Build documentation with JSDuck
echo   4^) Generate .d.ts off documentation
echo.
echo Press ENTER to copy over ExtJS sources ^(2/4^) . . .
set /p null=

mkdir %srcdir%
xcopy /s /y %ejstkpath% %srcdir%

echo Process:
echo   1^) Cleanup existing files ^(if any^)
echo   2^) Copy source files over
echo * 3^) Build documentation with JSDuck
echo   4^) Generate .d.ts off documentation
echo.
echo Press ENTER to run the JSDuck docs generator ^(3/4^) . . .
set /p null=

%jsduckbin% %srcdir% --output %jsdocdir%

if not exist %jsdocversfile% (
 echo Generating %jsdocversfile% . . .
 echo var version = '%ejsver%'; > %jsdocversfile%
 echo Done.
)

echo Done running JSDuck docs generator.
echo.
echo Process:
echo   1^) Cleanup existing files ^(if any^)
echo   2^) Copy source files over
echo   3^) Build documentation with JSDuck
echo * 4^) Generate .d.ts off documentation
echo.
echo Press ENTER to run the .d.ts generator ^(4/4^) . . .
set /p null=

%etsbin%

echo Done running the generator.

if not exist %outfile% (
 echo *** Error: the output .d.ts file was not generated at all.
 echo  Either JSDuck or the ExtTS generator application have crashed.
 pause
 exit /b 1
) else (
 echo File is available at: %outfile%
)

pause