Update from MPC-HC sources guide
-----------------------------------------------------------------
The following MPC-HC projects are used: BaseClasses, DSUtil, SubPic, subtitles, libssf, system.

BaseClasses, subtitles and libssf are taken as is without any modifications from MPC-HC.
SubPic is the same as in MPC-HC sources besides DX7SubPic.cpp which is excluded.
DSUtil has deinterlace.cpp excluded, additionally in vd.cpp all BitBltFrom* methods 
are commented out (to exclude dependency on yet another MPC-HC project).
system  is the same as in MPC-HC sources but only includes few needed files.

-----------------------------------------------------------------

Building System project using vsyasm

The Yasm Modular Assembler Project

Author : Peter Johnson
url : https://yasm.tortall.net/
Version used : 1.2.0
