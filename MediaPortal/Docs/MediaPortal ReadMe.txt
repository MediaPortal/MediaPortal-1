MediaPortal

Information:
    sources / binaries : http://sourceforge.net/projects/mediaportal
    forums & support   : http://nolanparty.com/mediaportal.sourceforge.net/phpBB2/
    Chatting & support : IRC efnet #MediaPortal

History:
    Mediaportal is a port of the Xbox MediaCenter or XBMC.
    You can read all about XBMC on www.xboxmediacenter.com
    As the title says this mediacenter is being developed for/on the xbox.
    In my opinon its the best mediacenter out there. I looked at many others
    but got spoiled with XBMC. One big disadvantage of XBMC is that it 
    doesnt do any PVR (tv recording). Since its virtually impossible to add
    PVR functionality to the xbox, I ended up porting XBMC to the PC.
    Thats how Mediaportal was born;-)

    Don't expect a full-blown, bug-free & stable MediaCenter yet. 
    I started porting this somewhere at the end of March 2004, so it still
    in its early stages and a bit premature.  Good news is that new things are 
    added & fixed every day. So each day its a bit better (we hope:-)


Requirements:
    - PC running Windows 2000 or Windows  XP
    - .NET Framework 1.1   ( www.windowsupdate.com )
    - DirectX 9c or higher ( www.windowsupdate.com )
    - Windows MediaPlayer 9/10
  
Optional:
    - A TV-Capture card ( We suggest one with hardware mpeg2 encoding to free the CPU)
    - Python 2.3 if you're gonna use the xml/tvguide.bat for the dutch tvguide.
    (see http://www.python.org/ )
    - XMLTV scripts/apps 
    - remote control (use Girder)
    - extra codecs like dscaler, ffdshow to play more formats
    
Keys:
    look in keymapping.txt!

  
About the TVGuide:  
    The TVGuide in Mediaportal is based on XMLTV. XMLTV is a defacto-standard which is used 
    by many HTPC's. Its a set of utilities which generates a tvguide file in a standard format
    There are many scripts/utilities (for different countries) which generate such a tvguide file
    More info about XMLTV:
    - http://sourceforge.net/projects/xmltv
    - http://membled.com/work/apps/xmltv/

    With this release we included a batch file for the dutch TVguide.
    Its called xmltv/tvguide.bat and each time you run it it creates a fresh xmltv/tvguide.xml
    which contains the dutch tvguide for the next 7 days. 
    It uses a python script, so first install Python (www.python.org) before running it!
    Offcourse you can run your own xmltv script/util for your own country. 
    Just make sure it outputs an tvguide.xml file. (setup the correct folder in MediaPortals setup)

    MediaPortal will detect any new versions automaticly and load/import it into its own tv database.
    But, you can always manually reload the tvguide.xml file :
        1. Goto my tv->tvguide
        2. Hit F3 to reload the tvguide. 
    Note, you might have to delete your database/tvdatabase3.db once to remove the
    old dutch tv-channels

  
Fullscreen/windowed
    use ALT+Enter to switch between fullscreen/windows mode
  

Audio/Video/DVD Players:
    Mediaportal can use its internal players for playing DVD/movies/audio or it 
    can use any external player. The internal players are fully integrated in MediaPortal and
    will give you the best user experience. I suggest you try these first (they are enabled by default) 
    If you really want to use an external player, then you can specify which ones to use in file->setup
    Please note. For (internal) DVD playback you still need to have a DVD player installed 
    like WinDVD or PowerDVD!!
  
Setup
    Use File->Setup to set things up like shares/folders etc. 
    In Mediaportal home>Settings r GUI/movie specific settings. Not all are here yet.
  
