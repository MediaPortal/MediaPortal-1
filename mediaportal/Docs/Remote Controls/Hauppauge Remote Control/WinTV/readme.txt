see http://nolanparty.com/mediaportal.sourceforge.net/phpBB2/viewtopic.php?t=621

Hi folks,

this is a small tutorial which describes how to use your Hauppauge Remote with Media Portal using the remote software from Hauppauge.

With the remote software from Hauppauge, you can control most of the software in windows. You just have to know what title the application you want control has and to edit the Irremote.ini file. This file can be found in your main windows directory (usually "C:\WINDOWS"). Make a copy of your current file! We don't want to damage your system Wink. The name of your application can be seen in the titlebar of it's window or in the taskbar. For this tutorial it is "Medial Portal". Smile


Open Irremote.ini and search for "[Applications]". In this section all the applications are listed which can be controlled. Add at the end "Media Portal=":
Code:

[Applications]
Default=
.
.
.
Windows Media Player=
ActiveMovie Window=
Media Portal=


Now we have to add a new section for our application. You can insert it where you want, for example at the end of the file. The section must begin with the title of the application in edged brackets "[Media Portal]". Beneath that you can define keys for the buttons of your remote. Normal chars can be entered directly. Function / special keys have to be set in brackets. A list of the remote control buttons can be found in section "[HCWPVR]". A small example:
Code:

BACK={esc}
MENU=y


A complete list of all function and special keys can be found here http://www.dschnabel.de/irdoku.htm (german!)

For the lazy boys and girls Wink:
Code:

[Media Portal]
CHNLUP={UP}
CHNLDOWN={DOWN}
VOLUP={RIGHT}
VOLDOWN={LEFT}
OK={enter}
BACK={esc}
MENU=y
0=0
1=1
2=2
3=3
4=4
5=5
6=6
7=7
8=8
9=9
RED={f1}
GREEN={PGUP}
YELLOW={f3}
BLUE={PGDN}
FULLSCREEN=x
STOP=b
PAUSE={space}
PLAY=p
REWIND={f5}
FASTFWD={f6}
SKIPFWD={f8}
SKIPREV={f7}
GRNPOWER={alt}{f4}
FUNC=m
GO={home}
REC=
MUTE=


And for the really lazy boys and girls here is a download of my Irremote.ini. It is the original Irremote.ini just with my Media Portal extensions. http://www.planetgrafe.net/mp/Irremote.zip

After changing the file, you have to restart the remote software. That can be easily done under "Start->Programs->Hauppauge Win TV->Restart IR".

This example works without making any changes to the keymap.xml from Media Portal. Keys currently not supported are "r", "s" and "u". I think some things can be done better, if you also change your keymap.xml. Since there are often new Builds of Media Portal i don't want to do that. Very Happy

Any comments, suggestions and critics are welcome! 