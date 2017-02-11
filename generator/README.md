## Steps to generate the .d.ts file

1. Open with VS2015 (must be VS2015!):
  - `c2.tools.ExtTS/c2.tools.ExtTS.sln`
2. Built (and just build)
  - So that `bin/Debug/c2.tools.ExtTS.exe` file is created.
3. Run the `generate.cmd` script
  - Specify in command line the path to ExtJS package
    - must be the path where ext-6.2.1 directory is
    - the script will complete the path
      `<specified-path>/ext-6.2.1/classic/classic/src`
    - specify in command line (1st argument)
    - hardcode on the script line 5
  - Optional: specify the ExtJS version.
    - defaults to 6.2.1
    - 2nd argument or hardcode on line 10
  - Optional: specify the ExtJS toolkit.
    - defaults to classic
    - 3rd argument or hardcode on line 14
4. Check out the output file
  - path: `3.out\ext-6.2.1-classic.d.ts`
  - will match the version and toolkit specified in commandline/hardcoded

## Manual steps

1. Create base directories:
  - `1.src`
  - `2.docs`
  - `3.out`
2. Copy over ExtJS sources 
  - from (example): `ext-6.2.1/classic/classic/src/*`
  - to: `1.src/ext-6.2.1-classic`
3. Run JSduck and create docs at `2.docs`
  - `0.tools/jsduck-6.0.0-beta.exe 1.src/ext-6.2.1-classic --output 2.docs/ext-6.2.1-classic`
4. Build `c2.tools.ExtTS/c2.tools.ExtTS.sln`
  - Project requires to be run at least on VS2015 due to C#6 syntax
5. Run `c2.tools.ExtTS/bin/Debug/c2.tools.ExtTS.exe`
6. Inspect `3.out/ext-6.2.1-classic.d.ts`