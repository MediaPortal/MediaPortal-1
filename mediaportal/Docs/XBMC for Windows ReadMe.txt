XBMC for Windows 2K/XP

This is a port of the Xbox MediaCenter or XBMC.
You can read all about XBMC on www.xboxmediacenter.com
As the title says this mediacenter is being developed for/on the xbox.
In my opinon its the best mediacenter out there. I looked at many others
but got spoiled with XBMC. However one big disadvantage of XBMC is that it 
doesnt do any PVR (tv recording). Since its virtually impossible to add
PVR functionality to the xbox, I ended up porting XBMC to the PC.


Don't expect a full-blown, bug-free & stable MediaCenter. 
I started porting this somewhere at the end of march 2004, so it still
in its early stages and a bit premature.  Good news is that new things are 
added & fixed every day. So each day its a bit better (we hope:-)

Requirements:
  - PC running Windows 2000 or Windows  XP
  - .NET Framework 1.1  ( www.windowsupdate.com )
  - DirectX 9 or higher ( www.windowsupdate.com )
  
Optional:
  - A TV-Capture card
  - Python 2.3 if you're gonna use the xml/tvguide.bat for the dutch tvguide.
    (see http://www.python.org/ )
  - XMLTV scripts/apps 
    
Keys:
  s   = controller start
  x,y = controller x,y
  a,b = controller a,b
  f1  = controller black
  f3  = controller white
  esc = controller back
  cursor keys 
  
  
TVGuide:  
  The TVGuide in XBMC is based on XMLTV. XMLTV is a defacto-standard which is used 
  by many HTPC's. Its a set of utilities which generates a tvguide file in a standard format
  There are many scripts/utilities (for different countries) which generate such a tvguide file
  More info about XMLTV:
    - http://sourceforge.net/projects/xmltv
    - http://membled.com/work/apps/xmltv/
    
  With this release we included a batch file for the dutch TVguide.
  Its called xmltv/tvguide.bat and each time you run it it creates a fresh xmltv/tvguide.xml
  which contains the dutch tvguide for the next 7 days. 
  It uses a python script, so first install Python (www.python.org) before running it!
  Offcourse you can run your own xmltv script for your own country. 
  Just make sure it outputs an xmltv/tvguide.xml file. 
  
  XBMC will detect any new versions automaticly and load/import it into its own tv database.
  But, you can always manually reload the xmltv/tvguide.xml file :
     1. Goto my tv->tvguide
     2. Hit F3 to reload the tvguide. 
  Note, you might have to delete your database/tvdatabase2.db once to remove the
  dutch tv-channels

  
Fullscreen/windowed
  use ALT+Enter to switch between fullscreen/windows mode
  

Players:
  xbmc/windows can use its internal players for playing DVD/movies or
  or it can use any external player
  You can specify if it should use the internal/external player in file->setup
  For DVD playback you need to have a DVD player installed like winDVD or powerdvd 
  You can change which players XBMC uses in the File->Setup menu
  
Setup
  Use File->Setup to set things up like shares/folders etc. 
  selecting Settings in XBMC isnt working yet
  
