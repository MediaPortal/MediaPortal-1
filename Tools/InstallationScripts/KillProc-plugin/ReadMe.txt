KillProcDLL ©2003 by DITMan, based upon the KILL_PROC_BY_NAME function programmed by Ravi, reach him at: http://www.physiology.wisc.edu/ravi/


-> Introduction

  This NSIS DLL Plug-In provides one function that has the ability to close any process running, without the need to have the 'class name' or 'window handle' you used to need when using the Windows API TerminateProcess or ExitProcess, just with the name of its .exe file.

    KillProc "process_name.exe"

  the parameter "process_name.exe" is passed through the stack, and the return code is stored in the $R0 variable. 

The meaning of the return codes is this:

   0   = Process was successfully terminated
   603 = Process was not currently running
   604 = No permission to terminate process
   605 = Unable to load PSAPI.DLL
   602 = Unable to terminate process for some other reason
   606 = Unable to identify system type
   607 = Unsupported OS
   632 = Invalid process name
   700 = Unable to get procedure address from PSAPI.DLL
   701 = Unable to get process list, EnumProcesses failed
   702 = Unable to load KERNEL32.DLL
   703 = Unable to get procedure address from KERNEL32.DLL
   704 = CreateToolhelp32Snapshot failed


-> Usage with NSIS

  Just copy the 'KillProcDLL.dll' in your NSIS plugins directory, and call the function with one of the two suggested syntax in the NSIS documentation:

    KillProcDLL::KillProc "process_name.exe"

OR

     ; Pre 2.0a4 syntax
    SetOutPath $TEMP
    GetTempFileName $8
    File /oname=$8 KillProcDLL.dll
    Push "process_name.exe"
    CallInstDLL KillProc

  then check out $R0 to get the return value if you need it.


-> Warning:
  According to MSDN (MicroSoft Developers Network):
  
  The TerminateProcess function is used to unconditionally cause a process to exit. Use it only in extreme circumstances. The state of global data maintained by dynamic-link libraries (DLLs) may be compromised if TerminateProcess is used rather than ExitProcess.

  So use this DLL as your LAST and EXTREME option :)


-> Copyrights and all that stuff:

  The original source file for the KILL_PROC_BY_NAME function is provided, the file is: exam28.cpp, and it MUST BE in this zip file.

  You can redistribute this archive if you do it without changing anything on it, otherwise you're NOT allowed to do so.

  You may use this source code in any of your projects, while you keep all the files intact, otherwise you CAN NOT use this code.


-> Contact information:

My homepage:
   http://petra.uniovi.es/~i6948857/index.php


-> Greetings:

  First of all, thanks to Ravi for his great function...
  Then all the winamp.com forums people who helped me doing this (kichik, Joost Verburg, Afrow UK...)
  Last but not least, I want to devote this source code to my Girl, Natalia... :)



Compiled in La Felguera, Spain, June-7th-2003
while listening to 'The Cure - Greatest Hits'
(in Winamp 2.91, of course :D)

·-EOF-·