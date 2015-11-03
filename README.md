# How to use
1. Install Wix 3.10.1
2. Load the solution up in Visual Studio 2015
3. Run CreateInstallers
   This creates two versions in temp:
   - %temp%\KungFu\0.0.6.0\KungFu.exe
   - %temp%\KungFu\1.0.0.0\KungFu.exe
4. Navigate to %temp%\KungFu\0.0.6.0
5. Run KungFu.exe
6. It should say an update is available, ignore this. Install and then exit.
7. Run KungFu.exe again
8. Click the Update Available button

  **This is where it goes awry**

9. 0.0.6.0 is run again
  checking the logs it appears its run as an update (when it should be an upgrade and be silent)
10. Click Update Available again
11. 1.0.0.0 is now running, install it and then exit
12. Check Programs and Features - the old version has not been uninstalled correctly

Changing the KungFu setup project to be perMachine instead of perUser fixes this behavior and step 9 becomes:

1. 1.0.0.0 is run, install it and then exit
2. Check Programs and Features - everything is sweet