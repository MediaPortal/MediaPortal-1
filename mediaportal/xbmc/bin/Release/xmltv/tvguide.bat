:start
del tvguidenew.xml
del tvguideold.xml
Sleep 10
tv_grab_nl.py  --output=tvguidenew.xml
if errorlevel 10 goto start

ren tvguide.xml tvguideold.xml
ren tvguidenew.xml tvguide.xml