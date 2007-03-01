

     //IE Code
     document.write ('<OBJECT ID=FlashWait height="90" width="90"  ');
     document.write ('CLASSID=clsid:D27CDB6E-AE6D-11cf-96B8-444553540000 ');
     document.write ('CODEBASE=http://download.macromedia.com/pub/shockwave/cabs/flash/swflash.cab#version=6,0,0,0 ');
     document.write ('<PARAM NAME="Movie" VALUE="images/wait.swf">');
     document.write ('<PARAM NAME="Src" VALUE="images/wait.swf">');
      document.write ('<PARAM NAME="WMode" VALUE="Transparent">');
                                
     //Netscape code
     document.write ('    <Embed type="application/x-shockwave-flash" src="images/wait.swf" quality="high" wmode="transparent" id=FlashWait');
     document.write ('        pluginspage="http://www.macromedia.com/go/getflashplayer"');
     document.write ('        Movie="images/wait.swf"');
     document.write ('        src="images/wait.swf"');
     document.write ('        Name=FlashWait');
     document.write ('        width=90');
     document.write ('        height=90>');
     document.write ('    </embed>');
     document.write ('</OBJECT>');
