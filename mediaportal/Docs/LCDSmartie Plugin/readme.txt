LCDSmartie plugin for Mediaportal

needed:
 - mediaportal
 - lcdsmartie 5.3 (http://lcdsmartie.sourceforge.net/)

Installation:
-copy MediaportalPlugin.dll to lcdsmartie plugins folder
-Enable the Network Controller plugin in Mediaportal


Configuration:
-Setup lcd smartie for your LCD
add following line to LCD smartie setup:
$dll(MediaportalPlugin.dll,1,[TAGS],-)
where [TAGS] is one or more of the tags specified in docs/tags.txt

Some examples:
$dll(MediaportalPlugin.dll,1,#title-#artist-#currentplaytime,-)
$dll(MediaportalPlugin.dll,1,#TV.View.channel #TV.View.title #TV.View.start-#TV.View.stop,-)
