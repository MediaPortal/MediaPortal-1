// JScript File

		var swapping=false;
		function handleButton(obj,on,css)
		{
		   
		    //info.innerHTML=obj.name + " - " + on + " - " + obj.style.visibility;
		    if(!on)
			{
			
				if(outTimer==null || outItem_name!=obj)
				{
					outItem_name=obj;
					//oUPnP.addToLog("handleMouseItem (" + item_type + " - " + which_event + " - " + item_name + " - " + window.event.srcElement.tagName );
					outTimer=window.setTimeout("doSwap('" + obj + "','" + on + "','" + css + "')",75);
				}
			}
			else
			{
			    if(outTimer!=null && outItem_name==obj)
				{
					//oUPnP.addToLog("cleared timer");
					window.clearTimeout(outTimer);
					outItem_name="";
				}
				
		            inTimer=window.setTimeout("doSwap('" + obj + "','true','" + css + "')",1);
		       
		      }
		}
		
		
        function showWait()
        {
            try
            {
                wait_div.style.display="block";
            }
             catch(e){
             try
             {
                parent.wait_div.style.display="block";
             }
             catch(e){}
             }
        }
        function hideWait()
        {
            try
            {
                wait_div.style.display="none";
            }
             catch(e){
             try
             {
                parent.wait_div.style.display="none";
             }
             catch(e){}
             }
        }
		
		var outTimer=null;
		var inTimer=null;
	    var outItem_name="";
		
		function doSwap(obj,on,css)
		{
		//alert(obj);
		    if(!swapping)
		    {
		        swapping=true;
		        try{
		            //eval('document.all.td_' + obj).style.filter="progid:DXImageTransform.Microsoft.Fade(duration=.2)";
		            //eval('document.all.td_' + obj).filters[0].Apply();
		        }catch(e){}
		        if (on=='true')
		        {
		            try{eval('document.all.text_' + obj).className=css + '_text_on';}catch(e){}
		            try{eval('document.all.over_image_' + obj).style.visibility='visible';}catch(e){}
		        }
		        else
		        {
		            try{eval('document.all.over_image_' + obj).style.visibility='hidden';}catch(e){}
		            try{eval('document.all.text_' + obj).className=css + '_text_off';}catch(e){}
		            
		        }
		         try{
		        //eval('document.all.td_' + obj).filters[0].Play();
		        }catch(e){}
		        swapping=false;
		     }
		}


function highLight(obj, on)
{
    if(on)
    {
        obj.style.backgroundImage='url(/images/cell_over.png)';
    }
    else
    {
        obj.style.backgroundImage='none';
    }
}


//page clicks

function loadInfo(type,args)
{

    info_row.style.display="block";
    info_row.style.visibility="visible";
    info_close_button.style.display="block";
    if(type=="music")
    {
        document.all.info_frame.style.height="225px";
        document.all.info_frame.src="info_music.aspx?" + args
       
    }
    else if(type=="recorded")
    {
    document.all.info_frame.style.height="155px";
        document.all.info_frame.src='info_tv.aspx?type=recorded&id=' + args;
    }
    else if(type=="scheduled")
    {
        document.all.info_frame.style.height="155px";
        document.all.info_frame.src='info_tv.aspx?type=scheduled&id=' + args;
    }
    else if(type=="new_recording")
    {
        document.all.info_frame.style.height="155px";
        document.all.info_frame.src='info_tv_edit.aspx?type=new_recording';
    }
    else if(type=="entry")
    {
        document.all.info_frame.style.height="155px";
        document.all.info_frame.src='info_tv.aspx?type=entry&id=' + args;
    }
    else if(type=="picture")
    {
        document.all.info_frame.style.height="225px";
        //alert(URLEncode(args));
        document.all.info_frame.src='info_picture.aspx?path=' + args; //URLEncode(args);
    }
    else if(type=="video")
    {
        document.all.info_frame.style.height="165px";
        //alert(URLEncode(args));
        document.all.info_frame.src='info_video.aspx?path=' + args; //URLEncode(args);
    }
    //window.setTimeout("document.all.info_frame.style.visibility='visible';",50);
     resizeScroller();
}

	
	function closeInfo()
	{
	    if(info_row.style.display=="block")
	    {
	        info_row.style.visibility="hidden";
	        //history.back();
	        document.all.info_frame.src='/blank.htm';
	        document.all.info_frame.style.height="0px";
	        info_close_button.style.display="none";
		    //
    		
		    resizeScroller();
		    
		 }
	}



//draggable stuff

var oActiveSel="";
	var topZ = 1;
	function fxPoint(oPoint){
	if(event.srcElement.tagName!="SELECT"){
		oPoint=event.srcElement;
		//oLab1.innerText="Point 1: " + oPoint.tagName;
		}
	}
	
	function fxTrapSelect(){
		event.returnValue=false;
	}
	
	function fxSetCapture(oPoint){
		var cmp=oPoint.componentFromPoint(event.clientX,event.clientY);
		if (cmp.indexOf("scrollbar")==-1){
		if(event.y < oPoint.offsetTop+35){
			topZ++;
			if(oPoint.style.position!='absolute')
			{
			    oPoint.style.position='absolute';
			    oPoint.style.left=oPoint.offsetLeft-oPoint.offsetWidth/2;
			}
			oPoint.style.zIndex=topZ;
			oPoint.setCapture(true);
			oPoint.dx=event.clientX - oPoint.offsetLeft;// + oPoint.offsetWidth;
			oPoint.dy=event.clientY - oPoint.offsetTop;
			
			oPoint.isMoving=true;
			if(oActiveSel){
				oLatitude.style.display="none";
				oLongitude.style.display="none";
			}
		}}
	}
		
	function fxMove(oPoint){
		//alert(oPoint);
		if(oPoint.isMoving){
			
			offsetleft=0;
			offsettop=0;
			offsetright=0;
			offsetbottom=0;
		
			var iNewX=event.x - oPoint.dx;
			var iNewY=event.y - oPoint.dy;
			
			if (iNewX+oPoint.offsetWidth+25>document.body.offsetWidth )
			{
			    iNewX=document.body.offsetWidth-oPoint.offsetWidth-25;
			}
			if (iNewX<0){iNewX=0;}
			if (iNewY+oPoint.offsetHeight+5>document.body.offsetHeight )
			{
			    iNewY=document.body.offsetHeight-oPoint.offsetHeight-5;
			}
			if (iNewY<0){iNewY=0;}
			
			oPoint.style.left=iNewX;
			oPoint.style.top=iNewY;
		}
	}
	function fxReleaseCapture(oPoint){
		//oPoint.style.zIndex=10
		if(oActiveSel){
			oActiveSel.selectedIndex=iLastIndex;
			fxCalcPoints(oActiveSel);
		}
		
		document.releaseCapture();
		oPoint.isMoving=false;
	}
	
	
// ====================================================================
//       URLEncode and URLDecode functions
//
// Copyright Albion Research Ltd. 2002
// http://www.albionresearch.com/
//
// You may copy these functions providing that 
// (a) you leave this copyright notice intact, and 
// (b) if you use these functions on a publicly accessible
//     web site you include a credit somewhere on the web site 
//     with a link back to http://www.albionresarch.com/
//
// If you find or fix any bugs, please let us know at albionresearch.com
//
// SpecialThanks to Neelesh Thakur for being the first to
// report a bug in URLDecode() - now fixed 2003-02-19.
// ====================================================================
function URLEncode(val)
{
	// The Javascript escape and unescape functions do not correspond
	// with what browsers actually do...
	var SAFECHARS = "0123456789" +					// Numeric
					"ABCDEFGHIJKLMNOPQRSTUVWXYZ" +	// Alphabetic
					"abcdefghijklmnopqrstuvwxyz" +
					"-_.!~*'()";					// RFC2396 Mark characters
	var HEX = "0123456789ABCDEF";

	var plaintext = val;
	var encoded = "";
	for (var i = 0; i < plaintext.length; i++ ) {
		var ch = plaintext.charAt(i);
	    if (ch == " ") {
		    encoded += "+";				// x-www-urlencoded, rather than %20
		} else if (SAFECHARS.indexOf(ch) != -1) {
		    encoded += ch;
		} else {
		    var charCode = ch.charCodeAt(0);
			if (charCode > 255) {
			    alert( "Unicode Character '" 
                        + ch 
                        + "' cannot be encoded using standard URL encoding.\n" +
				          "(URL encoding only supports 8-bit characters.)\n" +
						  "A space (+) will be substituted." );
				encoded += "+";
			} else {
				encoded += "%";
				encoded += HEX.charAt((charCode >> 4) & 0xF);
				encoded += HEX.charAt(charCode & 0xF);
			}
		}
	} // for

	//document.URLForm.F2.value = encoded;
	return encoded;
};

function URLDecode( )
{
   // Replace + with ' '
   // Replace %xx with equivalent character
   // Put [ERROR] in output if %xx is invalid.
   var HEXCHARS = "0123456789ABCDEFabcdef"; 
   var encoded = document.URLForm.F2.value;
   var plaintext = "";
   var i = 0;
   while (i < encoded.length) {
       var ch = encoded.charAt(i);
	   if (ch == "+") {
	       plaintext += " ";
		   i++;
	   } else if (ch == "%") {
			if (i < (encoded.length-2) 
					&& HEXCHARS.indexOf(encoded.charAt(i+1)) != -1 
					&& HEXCHARS.indexOf(encoded.charAt(i+2)) != -1 ) {
				plaintext += unescape( encoded.substr(i,3) );
				i += 3;
			} else {
				alert( 'Bad escape combination near ...' + encoded.substr(i) );
				plaintext += "%[ERROR]";
				i++;
			}
		} else {
		   plaintext += ch;
		   i++;
		}
	} // while
   document.URLForm.F1.value = plaintext;
   return false;
};