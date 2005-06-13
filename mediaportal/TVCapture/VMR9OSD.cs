using System;
using System.Drawing.Text;
using System.Drawing.Imaging;
using System.Drawing;
using MediaPortal.GUI.Library;
using DShowNET;
using MediaPortal.TV.Database;
using MediaPortal.Player;
using MediaPortal.Util;

namespace MediaPortal.TV.Recording
{
	/// <summary>
	/// Zusammenfassung für VMR9OSD.
	/// </summary>
	public class VMR9OSD
	{
		#region constructor / destructor
		public VMR9OSD()
		{
			//
			// TODO: Fügen Sie hier die Konstruktorlogik hinzu
			//
			ReadSkinFile();
			using (MediaPortal.Profile.Xml xmlreader = new MediaPortal.Profile.Xml(System.Windows.Forms.Application.StartupPath+@"\MediaPortal.xml"))
			{
				int alpha=xmlreader.GetValueAsInt("vmr9OSDSkin","alphaValue",10);
				if(alpha>0)
					m_renderOSDAlpha=(float)alpha/10;
				else 
					m_renderOSDAlpha=0.8f;// default
			}
		}
		#endregion
		// structs
		#region structs / enums
		enum OSD
		{
			ZapOSD=1,
			ZapList,
			VolumeOSD,
			CurrentTVShowInfo,
			OtherBitmap,
			None
		}

		struct OSDSkin
		{
			public string[] rects;
			public string chName;
			public string chNow;
			public string chNext;
			public string chProgress;
			public string sigBar;
			public string time;
			public string mute;
			public string chLogo;
		}
		struct OSDChannelList
		{
			public string baseRect;
		}
		#endregion
		//
		#region globals
		DateTime m_timeDisplayed=DateTime.Now;
		bool m_muteState=false;
		string m_mediaPath=System.Windows.Forms.Application.StartupPath+@"\osdskin-media\";
		TVChannel m_actualChannel=null;
		int m_channelSNR=0;
		// osd skin
		OSDSkin m_osdSkin;
		OSDChannelList m_osdChannels;
		bool m_bitmapIsVisible=false;
		int m_timeout=0;
		OSD m_osdRendered=OSD.None;
		Bitmap m_volumeBitmap;
		Bitmap m_muteBitmap;
		float m_renderOSDAlpha=0.8f;

		#endregion

		#region properties
		public bool Mute
		{
			get{return m_muteState;}
			set{m_muteState=value;}
		}
		#endregion

		#region osd render functions
		public void RenderCurrentShowInfo()
		{
			int gWidth=GUIGraphicsContext.Width;
			int gHeight=GUIGraphicsContext.Height;

			if(m_osdRendered==OSD.CurrentTVShowInfo)
			{
				m_bitmapIsVisible=true;
				HideBitmap();
				m_timeout=0;
				return;
			}
			if(m_actualChannel==null)
				return;

			TVChannel channel=m_actualChannel;
			TVProgram prog=channel.GetProgramAt(DateTime.Now);
			
			if(prog==null)
				return;
			
			m_timeout=10000;// ten seconds
			m_osdRendered=OSD.CurrentTVShowInfo;
			// render list
			Bitmap bm=new Bitmap(gWidth,gHeight);//m_mediaPath+@"bgimage.png");
			Graphics gr=Graphics.FromImage(bm);
			int x=60;
			int y=20;
			if(bm==null || gr==null || m_osdChannels.baseRect==null)
				return;
			
			string[] seg=m_osdChannels.baseRect.Split(new char[]{':'});
			if(seg==null) return;
			if(seg.Length!=7) return;
			if(seg[0]!="nsrect") return;
			Color headColor=GetColor(seg[1]);
			Color nBoxColor=GetColor(seg[2]);
			Color sBoxColor=GetColor(seg[3]);
			Color textColor=GetColor(seg[4]);
			//
			Font drawFont=new Font(seg[5],Convert.ToInt16(seg[6]));
			SolidBrush textBrush=new SolidBrush(textColor);
			RectangleF layoutRect=new RectangleF(x,y,gWidth-(x*2),gHeight-(y*2));
			//
			SizeF textSize=gr.MeasureString("AAA",drawFont);
			int textHeight=2+((int)textSize.Height);

			string headerText=String.Format("{0}: {1} ({2}-{3})",channel.Name,prog.Title,prog.StartTime.ToShortTimeString(),prog.EndTime.ToShortTimeString());

			gr.FillRectangle(new SolidBrush(headColor),x,y,gWidth-(2*x),textHeight);
			gr.DrawString(headerText,drawFont,textBrush,layoutRect,StringFormat.GenericTypographic);
			layoutRect.Y+=textHeight;
			layoutRect.Height-=textHeight*2;
			// draw
			gr.FillRectangle(new SolidBrush(nBoxColor),layoutRect);
			layoutRect.Width-=20;// ten pixel offset
			layoutRect.X+=10;
			gr.DrawString(prog.Description,drawFont,textBrush,layoutRect,StringFormat.GenericTypographic);
			// display and release
			m_bitmapIsVisible=false;
			SaveBitmap(bm,true,true,m_renderOSDAlpha);
			bm.Dispose();
			gr.Dispose();
			drawFont.Dispose();
			textBrush.Dispose();
			m_timeDisplayed=DateTime.Now;
		}

		public void RenderChannelList(TVGroup group,string currentChannel)
		{
			int gWidth=GUIGraphicsContext.Width;
			int gHeight=GUIGraphicsContext.Height;

			if(group==null)
				return;
			if(group.tvChannels.Count<2)
				return;
			int positionActChannel=0;
			int counter=0;
			bool logosFound=false;
			m_timeout=10000;
			m_osdRendered=OSD.ZapList;
			
			foreach(TVChannel chan in group.tvChannels)
			{
				string tvlogo=Utils.GetCoverArt(Thumbs.TVChannel,chan.Name);				
				if(System.IO.File.Exists(tvlogo))
				{
					logosFound=true;
				}
				if(chan.Name==currentChannel)
				{
					positionActChannel=counter;
					//break;
				}
				counter++;
			}

			// render list
			Bitmap bm=new Bitmap(gWidth,gHeight);//m_mediaPath+@"bgimage.png");
			Graphics gr=Graphics.FromImage(bm);
			int x=60;
			int y=20;
			if(bm==null || gr==null || m_osdChannels.baseRect==null)
				return;
			
			string[] seg=m_osdChannels.baseRect.Split(new char[]{':'});
			if(seg==null) return;
			if(seg.Length!=7) return;
			if(seg[0]!="nsrect") return;
			Color headColor=GetColor(seg[1]);
			Color nBoxColor=GetColor(seg[2]);
			Color sBoxColor=GetColor(seg[3]);
			Color textColor=GetColor(seg[4]);
			//
			Font drawFont=new Font(seg[5],Convert.ToInt16(seg[6]));
			SolidBrush textBrush=new SolidBrush(textColor);
			RectangleF layoutRect=new RectangleF(x,y,gWidth-(x*2),gHeight-(y*2));
			//
			SizeF textSize=gr.MeasureString("AAA",drawFont);
			int textHeight;
			if(logosFound)
			{
				textHeight=50;
				layoutRect.X+=50;
				layoutRect.Width-=100;
			}
			else
				textHeight=2+((int)textSize.Height);


			string headText=group.GroupName;
			
			gr.FillRectangle(new SolidBrush(headColor),x,y,gWidth-(2*x),textHeight);
			gr.DrawString(headText,drawFont,textBrush,layoutRect,StringFormat.GenericTypographic);
			layoutRect.Y+=textHeight;
			int yMax=gHeight-(y*2);
			int channelCount=yMax/textHeight;
			channelCount--;
			int pos=y+textHeight;
			int startAt=positionActChannel-(channelCount/2);
			Log.Write("start list at={0} position={1}",startAt,positionActChannel);
			// draw
			if(group.tvChannels.Count<channelCount || positionActChannel<(channelCount/2))
				startAt=0;
			for(int i=startAt;i<group.tvChannels.Count;i++)
			{
				// stop render / continue
				if(i<0) break;
				if(i>positionActChannel+channelCount) continue;
				if(i>=group.tvChannels.Count) break;
				TVChannel chan=(TVChannel)group.tvChannels[i];
				if(chan==null) break;

				TVProgram prog=chan.GetProgramAt(DateTime.Now);
				string channelText="";
				if(prog!=null)
					channelText=chan.Name+" "+"\""+prog.Title+"\"";
				else
					channelText=chan.Name;

				if(chan.Name==currentChannel)
				{
					gr.FillRectangle(new SolidBrush(sBoxColor),x,pos,gWidth-(2*x),textHeight);
					gr.DrawString(channelText,drawFont,textBrush,layoutRect,StringFormat.GenericTypographic);
				}
				else
				{
					gr.FillRectangle(new SolidBrush(nBoxColor),x,pos,gWidth-(2*x),textHeight);
					gr.DrawString(channelText,drawFont,textBrush,layoutRect,StringFormat.GenericTypographic);
				}
				if(logosFound==true)
				{
					string tvlogo=Utils.GetCoverArt(Thumbs.TVChannel,chan.Name);				
					if(System.IO.File.Exists(tvlogo))
					{
						Bitmap logo=new Bitmap(tvlogo);
						Util.BitmapResize.Resize(ref logo,48,48,true,true);
						gr.FillRectangle(new SolidBrush(Color.FromArgb(144,144,144)),x,pos+1,48,48);
						gr.DrawImage(logo,x,pos+1,48,48);
						logo.Dispose();
					}
				}

				pos+=textHeight;
				layoutRect.Y+=textHeight;

				if(pos>=yMax-textHeight)
					break;
			}
			m_bitmapIsVisible=false;
			SaveBitmap(bm,true,true,m_renderOSDAlpha);
			bm.Dispose();
			gr.Dispose();
			drawFont.Dispose();
			textBrush.Dispose();
	
		}
		public void RenderVolumeOSD()
		{
			int gWidth=GUIGraphicsContext.Width;
			int gHeight=GUIGraphicsContext.Height;
			int max;
			int min;
			int currentVolume=AudioMixerHelper.GetMinMaxVolume(out min,out max);
			int volume=0;
			if(currentVolume>0)
			{
				volume=((currentVolume*100)/max)/10;
			}
			if(volume<1)
				m_muteState=true;
			else
				m_muteState=false;
			
			int[] drawWidth=new int[]{0,25,43,62,82,99,117,137,155,173,200};

			m_osdRendered=OSD.VolumeOSD;
			m_bitmapIsVisible=false;
			m_timeout=3000; // 3 sec for volume osd
			if(System.IO.File.Exists(m_mediaPath+String.Format("volume_level_{0}.png",volume))==true)
			{
				if(m_osdSkin.mute!=null)
				{
					string[] seg =m_osdSkin.mute.Split(new char[]{':'});
					if(seg!=null)
					{
						if(seg[0]=="icon" && seg.Length==4)
						{
							Bitmap osd=new Bitmap(gWidth,gHeight);
							Graphics gr=Graphics.FromImage(osd);
							
							//Bitmap gfx=new Bitmap(m_mediaPath+String.Format("volume_level_{0}.png",volume));
							//gfx.MakeTransparent(Color.White);
							int xPos=0;
							int yPos=0;

							if(seg[1].StartsWith("m"))
								xPos=GetPosition(gWidth,seg[1]);
							else
								xPos=Convert.ToInt16(seg[1]);

							if(seg[2].StartsWith("m"))
								yPos=GetPosition(gHeight,seg[2]);
							else
								yPos=Convert.ToInt16(seg[2]);
							
							if(volume>0)
							{
								if(m_volumeBitmap!=null)
									gr.DrawImage(m_volumeBitmap,xPos,yPos,new RectangleF(0f,0f,drawWidth[volume],m_volumeBitmap.Height),System.Drawing.GraphicsUnit.Pixel);
							}
							else
								if(m_muteBitmap!=null)
									gr.DrawImageUnscaled(m_muteBitmap,xPos,yPos,m_muteBitmap.Width,m_muteBitmap.Height);

							SaveBitmap(osd,true,true,0.9f);
							gr.Dispose();
							osd.Dispose();
							m_timeDisplayed=DateTime.Now;
						}
					}
				
				}
			}
		}
		public void RenderZapOSD(TVChannel channel,int signalLevel)
		{
			int gWidth=GUIGraphicsContext.Width;
			int gHeight=GUIGraphicsContext.Height;
			Bitmap bm=new Bitmap(gWidth,gHeight);//m_mediaPath+@"bgimage.png");
			Graphics gr=Graphics.FromImage(bm);
			int x=60;
			int y=0;
			if(bm==null || gr==null || channel==null)
			{
				Log.Write("end rendering zaposd: no bitmap (memory problem?)");
				return ;
			}
			m_osdRendered=OSD.ZapOSD;
			m_timeout=0;
			m_actualChannel=channel;
			m_channelSNR=signalLevel;
			// set the tvchannel data
			string serviceName=channel.Name;
			TVProgram tvNext=null;
			TVProgram tvNow=channel.GetProgramAt(DateTime.Now);
			string nowStart="";
			string nowDur="";
			string nowDescr="";
			string nowTitle="";
			string nextTitle="";
			string nextStart="";
			string nextDur="";
			double done=0;
			double signalLev=(double)signalLevel;

			if(tvNow!=null)
			{
				tvNext=channel.GetProgramAt(tvNow.EndTime.AddMinutes(1));		
				nowStart=tvNow.StartTime.ToShortTimeString();
				nowDur=tvNow.Duration.ToString();
				double nowDone=tvNow.EndTime.Subtract(DateTime.Now).TotalMinutes;
				double nowTotal=tvNow.EndTime.Subtract(tvNow.StartTime).TotalMinutes;
				done=(nowDone*100)/nowTotal;
				nowTitle=tvNow.Title;
				nowDescr=tvNow.Description;
			}
			if(tvNext!=null)
			{
				nextStart=tvNext.StartTime.ToShortTimeString();
				nextTitle=tvNext.Title;
				nextDur=tvNext.Duration.ToString();
			}
			
			// first graphic elements and pictures
			// rects
			for(int i=0;i<99;i++)
			{
				string rect=m_osdSkin.rects[i];
				if(rect=="" || rect==null)
					break;
				string[] seg=rect.Split(new char[]{':'});

				if(seg!=null)
				{
					if(seg.Length==6)
					{
						int xpos=0;
						int ypos=0;
						int width=0;
						int height=0;
						try
						{
							if(seg[1].StartsWith("m"))
								xpos=GetPosition(gWidth,seg[1]);
							else
								xpos=Convert.ToInt16(seg[1]);
							
							if(seg[2].StartsWith("m"))
								ypos=GetPosition(gHeight,seg[2]);
							else
								ypos=Convert.ToInt16(seg[2]);
							
							if(seg[3]=="max")
								width=gWidth;
							else
								width=Convert.ToInt16(seg[3]);
							
							if(seg[4]=="max")
								height=gHeight;
							else
								height=Convert.ToInt16(seg[4]);
						}
						catch{ break;}
						if(seg[0]=="frect")
							gr.FillRectangle(new SolidBrush(GetColor(seg[5])),xpos,ypos,width,height);
						if(seg[0]=="rect")
							gr.DrawRectangle(new Pen(GetColor(seg[5])),xpos,ypos,width,height);				
					}
				}
				else break;
			}
			// text always gets an x-offset 40 pix.
			// tv channel logo
			string chLogo=m_osdSkin.chLogo;
			if(chLogo!=null)
			{
				string[] seg =chLogo.Split(new char[]{':'});
				if(seg!=null)
				{
					if(seg[0]=="chLogo" && seg.Length==7)
					{
						int xPos=0;
						int yPos=0;
						int width=0;
						int height=0;

						if(seg[1].StartsWith("m"))
							xPos=GetPosition(gWidth,seg[1]);
						else
							xPos=x+Convert.ToInt16(seg[1]);

						if(seg[2].StartsWith("m"))
							yPos=GetPosition(gHeight,seg[2]);
						else
							yPos=y+Convert.ToInt16(seg[2]);
						if(seg[3]=="max")
							width=gWidth;
						else
							width=Convert.ToInt16(seg[3]);
						if(seg[4]=="max")
							height=gHeight;
						else
							height=Convert.ToInt16(seg[4]);
						
						Color oColor=GetColor(seg[5]);
						int outline=Convert.ToInt16(seg[6]);
						string tvlogo=Utils.GetCoverArt(Thumbs.TVChannel,serviceName);				
						if(System.IO.File.Exists(tvlogo))
						{
							gr.FillRectangle(new SolidBrush(oColor),xPos,yPos,width+outline,height+outline);
							Bitmap logo=new Bitmap(tvlogo);
							gr.DrawImage(logo,xPos+(outline/2),yPos+(outline/2),64,64);
							logo.Dispose();
						}
					}
				}
			}
			
			//channel name (chName)
			string chName=m_osdSkin.chName;
			if(chName!=null)
			{
				string[] seg =chName.Split(new char[]{':'});
				if(seg!=null)
				{
					if(seg[0]=="text" && seg.Length==7)
					{
						int xPos=0;
						int yPos=0;

						if(seg[1].StartsWith("m"))
							xPos=GetPosition(gWidth,seg[1]);
						else
							xPos=x+Convert.ToInt16(seg[1]);

						if(seg[2].StartsWith("m"))
							yPos=GetPosition(gHeight,seg[2]);
						else
							yPos=y+Convert.ToInt16(seg[2]);

						Color fColor=GetColor(seg[4]);
						Color bColor=GetColor(seg[5]);
						gr.DrawString(serviceName,new System.Drawing.Font(seg[3],Convert.ToInt16(seg[6])),new SolidBrush(fColor),xPos,yPos,StringFormat.GenericTypographic);
					}
				}
			}
			//now on tv (chNow)
			string chNow=m_osdSkin.chNow;
			if(chNow!=null)
			{
				string[] seg =chNow.Split(new char[]{':'});
				if(seg!=null)
				{
					if(seg[0]=="text" && seg.Length==7)
					{
						int xPos=0;
						int yPos=0;

						if(seg[1].StartsWith("m"))
							xPos=GetPosition(gWidth,seg[1]);
						else
							xPos=x+Convert.ToInt16(seg[1]);

						if(seg[2].StartsWith("m"))
							yPos=GetPosition(gHeight,seg[2]);
						else
							yPos=y+Convert.ToInt16(seg[2]);

						Color fColor=GetColor(seg[4]);
						Color bColor=GetColor(seg[5]);
						Font drawFont=new System.Drawing.Font(seg[3],Convert.ToInt16(seg[6]));
						Brush drawBrush=new SolidBrush(fColor);
						SizeF xEnd=gr.MeasureString(nowDur,drawFont);
						int xPosEnd=(gWidth-70)-((int)xEnd.Width);
						gr.DrawString(nowDur,drawFont,drawBrush,xPosEnd,yPos,StringFormat.GenericTypographic);
						gr.DrawString(nowStart+"  "+nowTitle,drawFont,drawBrush,new RectangleF(xPos,yPos,xPosEnd-xPos-5,xEnd.Height),StringFormat.GenericTypographic);
						drawFont.Dispose();
						drawBrush.Dispose();
					}
				}
			}
			//next on tv (chNow)
			string chNext=m_osdSkin.chNext;
			if(chNext!=null)
			{
				string[] seg =chNext.Split(new char[]{':'});
				if(seg!=null)
				{
					if(seg[0]=="text" && seg.Length==7)
					{
						int xPos=0;
						int yPos=0;

						if(seg[1].StartsWith("m"))
							xPos=GetPosition(gWidth,seg[1]);
						else
							xPos=x+Convert.ToInt16(seg[1]);

						if(seg[2].StartsWith("m"))
							yPos=GetPosition(gHeight,seg[2]);
						else
							yPos=y+Convert.ToInt16(seg[2]);
						Color fColor=GetColor(seg[4]);
						Color bColor=GetColor(seg[5]);
						Font drawFont=new System.Drawing.Font(seg[3],Convert.ToInt16(seg[6]));
						Brush drawBrush=new SolidBrush(fColor);
						SizeF xEnd=gr.MeasureString(nextDur,drawFont);
						int xPosEnd=(gWidth-70)-((int)xEnd.Width);
						gr.DrawString(nextDur,drawFont,drawBrush,xPosEnd,yPos,StringFormat.GenericTypographic);
						gr.DrawString(nextStart+"  "+nextTitle,drawFont,drawBrush,new RectangleF(xPos,yPos,xPosEnd-xPos-5,xEnd.Height),StringFormat.GenericTypographic);
						drawFont.Dispose();
						drawBrush.Dispose();
					}
				}
			}
			//progress tv (chNow)
			string chProgress=m_osdSkin.chProgress;
			if(chProgress!=null)
			{
				string[] seg =chProgress.Split(new char[]{':'});
				if(seg!=null)
				{
					if(seg[0]=="progressbar" && seg.Length==7)
					{
						int xPos=0;
						int yPos=0;
						int width=0;
						int height=0;

						if(seg[1].StartsWith("m"))
							xPos=GetPosition(gWidth,seg[1]);
						else
							xPos=x+Convert.ToInt16(seg[1]);

						if(seg[2].StartsWith("m"))
							yPos=GetPosition(gHeight,seg[2]);
						else
							yPos=y+Convert.ToInt16(seg[2]);
						if(seg[3]=="max")
							width=gWidth;
						else
							width=Convert.ToInt16(seg[3]);
						if(seg[4]=="max")
							height=gHeight;
						else
							height=Convert.ToInt16(seg[4]);
						Color fColor=GetColor(seg[5]);
						Color bColor=GetColor(seg[6]);
						gr.FillRectangle(new SolidBrush(bColor),xPos,yPos,width,height);
						gr.FillRectangle(new SolidBrush(fColor),xPos+2,yPos+2,width-((int)((done/100)*width))-4,height-4);
					}
				}
			}
			//signal level
			if(signalLevel>0)
			{
				string sigBar=m_osdSkin.sigBar;
				if(sigBar!=null)
				{
					string[] seg=sigBar.Split(new char[]{':'});
					if(seg!=null)
					{
						if(seg[0]=="progressbar" && seg.Length==11)
						{
							int xPos=0;
							int yPos=0;
							int width=0;
							int height=0;

							if(seg[1].StartsWith("m"))
								xPos=GetPosition(gWidth,seg[1]);
							else
								xPos=x+Convert.ToInt16(seg[1]);

							if(seg[2].StartsWith("m"))
								yPos=GetPosition(gHeight,seg[2]);
							else
								yPos=y+Convert.ToInt16(seg[2]);
							if(seg[3]=="max")
								width=gWidth;
							else
								width=Convert.ToInt16(seg[3]);
							if(seg[4]=="max")
								height=gHeight;
							else
								height=Convert.ToInt16(seg[4]);
							Color fColor=GetColor(seg[5]);
							Color bColor=GetColor(seg[6]);
							Color tColor=GetColor(seg[7]);
							Font drawFont=new Font(seg[8],Convert.ToInt16(seg[9]));
							SizeF xEnd=gr.MeasureString(seg[10],drawFont);
							gr.DrawString(seg[10],drawFont,new SolidBrush(tColor),xPos,yPos,StringFormat.GenericTypographic);
							xPos+=5+((int)xEnd.Width);
							gr.FillRectangle(new SolidBrush(bColor),xPos,yPos,width,height);
							gr.FillRectangle(new SolidBrush(fColor),xPos+2,yPos+2,width-((int)(((signalLev/100)*width)))-4,height-4);
						}
					}
				}
			}
			// time display
			string time=m_osdSkin.time;
			if(time!=null)
			{
				string[] seg =time.Split(new char[]{':'});
				if(seg!=null)
				{
					if(seg[0]=="time" && seg.Length==7)
					{
						int xPos=0;
						int yPos=0;

						if(seg[1].StartsWith("m"))
							xPos=GetPosition(gWidth,seg[1]);
						else
							xPos=x+Convert.ToInt16(seg[1]);

						if(seg[2].StartsWith("m"))
							yPos=GetPosition(gHeight,seg[2]);
						else
							yPos=y+Convert.ToInt16(seg[2]);
						Color fColor=GetColor(seg[4]);
						Color bColor=GetColor(seg[5]);
						gr.DrawString(DateTime.Now.ToShortTimeString(),new System.Drawing.Font(seg[3],Convert.ToInt16(seg[6])),new SolidBrush(fColor),xPos,yPos,StringFormat.GenericTypographic);
					}
				}
			}
			// mute
			string mute=m_osdSkin.mute;
			if(mute!=null && m_muteState==true)
			{
				string[] seg =mute.Split(new char[]{':'});
				if(seg!=null)
				{
					if(seg[0]=="icon" && seg.Length==4)
					{
						if(System.IO.File.Exists(m_mediaPath+"volume_level_0.png"))
						{
							int xPos=0;
							int yPos=0;

							if(seg[1].StartsWith("m"))
								xPos=GetPosition(gWidth,seg[1]);
							else
								xPos=x+Convert.ToInt16(seg[1]);

							if(seg[2].StartsWith("m"))
								yPos=GetPosition(gHeight,seg[2]);
							else
								yPos=y+Convert.ToInt16(seg[2]);
							if(m_muteBitmap!=null)
								gr.DrawImageUnscaled(m_muteBitmap,xPos,yPos,60,60);
						}
					}
				}
			}
			m_bitmapIsVisible=true;
			SaveBitmap(bm,true,true,m_renderOSDAlpha);
		}
		#endregion

		#region private helper functions
		int GetPosition(int baseVal,string val)
		{
			string val1=val.Substring(1,val.Length-1);
			int val2=Convert.ToInt16(val1);
			return (baseVal-val2<0)?0:baseVal-val2;
		}
		Color GetColor(string colString)
		{
			Color col=new Color();
			if(colString!=null)
			{
				string[] values=colString.Split(new char[]{','});
				if(values!=null)
				{
					if(values.Length==3)
					{
						int red=Convert.ToInt16(values[0]);
						int green=Convert.ToInt16(values[1]);
						int blue=Convert.ToInt16(values[2]);
						if(red<0 || red>255) red=0;
						if(green<0 || green>255) green=0;
						if(blue<0 || blue>255) blue=0;
						col=System.Drawing.Color.FromArgb(red,green,blue);
					}
				}
			}
			return col;
		}
		#endregion

		#region public functions
		public void RefreshCurrentChannel(int signal)
		{
			if(signal!=m_channelSNR && m_bitmapIsVisible==true)
			{
				m_channelSNR=signal;
				RenderZapOSD(m_actualChannel,m_channelSNR);
			}
		}

		public void RefreshCurrentChannel()
		{
			RenderZapOSD(m_actualChannel,m_channelSNR);
		}
		public void CheckTimeOuts()
		{
			TimeSpan ts=DateTime.Now-m_timeDisplayed;
			if(ts.TotalMilliseconds>m_timeout && m_timeout>0)
			{
				
				if(m_osdRendered==OSD.VolumeOSD)
				{
					if(m_muteState==true)
						return;
				}
				else
				{
					m_bitmapIsVisible=true; // force clear
					HideBitmap();
					m_timeout=0;
				}
			}
		}
		public void ShowBitmap(Bitmap bmp)
		{
			if(bmp==null)
				return;
			m_timeout=0;
			m_osdRendered=OSD.OtherBitmap;
			SaveBitmap(bmp,true,true,1.0f);
		}
		public void ShowBitmap(Bitmap bmp,int timeout)
		{
			if(bmp==null)
				return;
			m_timeout=timeout;
			m_osdRendered=OSD.OtherBitmap;
			SaveBitmap(bmp,true,true,1.0f);
		}
		public void ShowBitmap(Bitmap bmp,float alpha,int timeout)
		{
			if(bmp==null)
				return;
			m_timeout=timeout;
			m_osdRendered=OSD.OtherBitmap;
			SaveBitmap(bmp,true,true,alpha);
		}

		public void ShowBitmap(Bitmap bmp,float alpha)
		{
			if(bmp==null)
				return;
			m_timeout=0;
			m_osdRendered=OSD.OtherBitmap;
			SaveBitmap(bmp,true,true,alpha);
		}
		public void HideBitmap()
		{
			SaveBitmap(null,false,true,0f);
		}
		#endregion

		#region private functions
		void ReadSkinFile()
		{
			m_osdSkin.rects=new string[99];
			using (MediaPortal.Profile.Xml   xmlreader=new MediaPortal.Profile.Xml(System.Windows.Forms.Application.StartupPath+@"\osdskin.xml"))
			{
				// first graphic elements and pictures
				// rects
				
				for(int i=0;i<99;i++)
				{
					string rect=xmlreader.GetValueAsString("zaposdSkin",String.Format("rect{0}",i),"");
					if(rect=="")
						break;
					else
						m_osdSkin.rects[i]=rect;
				}
				// text always gets an x-offset 40 pix.
				//channel name (chName)
				string chName=xmlreader.GetValueAsString("zaposdSkin","chName","");
				if(chName!=null)
				{
					if(chName!="")
						m_osdSkin.chName=chName;
				}
				//now on tv (chNow)
				string chNow=xmlreader.GetValueAsString("zaposdSkin","chNow","");
				if(chNow!=null)
				{
					if(chNow!="")
						m_osdSkin.chNow=chNow;
				}
				//next on tv (chNow)
				string chNext=xmlreader.GetValueAsString("zaposdSkin","chNext","");
				if(chNext!=null)
				{
					if(chNext!="")
						m_osdSkin.chNext=chNext;

				}
				//progress tv (chNow)
				string chProgress=xmlreader.GetValueAsString("zaposdSkin","chProgress","");
				if(chProgress!=null)
				{
					if(chProgress!="")
						m_osdSkin.chProgress=chProgress;

				}
				//signal level
				string sigBar=xmlreader.GetValueAsString("zaposdSkin","signalBar","");
				if(sigBar!=null)
				{
					if(sigBar!="")
						m_osdSkin.sigBar=sigBar;

				}
				
				// time display
				string time=xmlreader.GetValueAsString("zaposdSkin","time","");
				if(time!=null)
				{
					if(time!="")
						m_osdSkin.time=time;
				}
				// mute
				string mute=xmlreader.GetValueAsString("zaposdSkin","mute","");
				if(mute!=null)
				{
					if(mute!="")
						m_osdSkin.mute=mute;
				}
				// mute
				string chLogo=xmlreader.GetValueAsString("zaposdSkin","chLogo","");
				if(chLogo!=null)
				{
					if(chLogo!="")
						m_osdSkin.chLogo=chLogo;
				}
				// channel list
				m_osdChannels.baseRect=xmlreader.GetValueAsString("zaposdChannels","rect","");
			}
			try
			{
				m_volumeBitmap=new Bitmap(m_mediaPath+"volume_level_10.png");
				m_volumeBitmap.MakeTransparent(Color.White);
				m_muteBitmap=new Bitmap(m_mediaPath+"volume_level_0.png");
				m_muteBitmap.MakeTransparent(Color.White);
			}
			catch{}
		}

		bool SaveBitmap(System.Drawing.Bitmap bitmap,bool show,bool transparent,float alphaValue)
		{
			if (VMR9Util.g_vmr9!=null)
			{
				System.IO.MemoryStream mStr=new System.IO.MemoryStream();
				// transparent image?
				if(bitmap!=null)
				{
					if(transparent==true)
						bitmap.MakeTransparent(Color.Black);
					bitmap.Save(mStr,System.Drawing.Imaging.ImageFormat.Bmp);
					mStr.Position=0;
				}
				if (show==true)
				{
					VMR9Util.g_vmr9.SaveBitmap(bitmap,show,transparent,alphaValue);
					m_timeDisplayed=DateTime.Now;
					return true;
				}
				else
				{
					if(m_bitmapIsVisible==true)
					{
						if(m_muteState==true)
						{
							RenderVolumeOSD();
						}
						else
						{
							VMR9Util.g_vmr9.SaveBitmap(bitmap,show,transparent,alphaValue);
							m_bitmapIsVisible=false;
							m_osdRendered=OSD.None;
						}
					}
				}
				// dispose
				return true;
			}


			if (VMR7Util.g_vmr7!=null)
			{
				System.IO.MemoryStream mStr=new System.IO.MemoryStream();
				// transparent image?
				if(bitmap!=null)
				{
					if(transparent==true)
						bitmap.MakeTransparent(Color.Black);
					bitmap.Save(mStr,System.Drawing.Imaging.ImageFormat.Bmp);
					mStr.Position=0;
				}
				if (show==true)
				{
					VMR7Util.g_vmr7.SaveBitmap(bitmap,show,transparent,alphaValue);
					m_timeDisplayed=DateTime.Now;
					return true;
				}
				else
				{
					if(m_bitmapIsVisible==true)
					{
						if(m_muteState==true)
						{
							RenderVolumeOSD();
						}
						else
						{
							VMR7Util.g_vmr7.SaveBitmap(bitmap,show,transparent,alphaValue);
							m_bitmapIsVisible=false;
							m_osdRendered=OSD.None;
						}
					}
				}
				// dispose
				return true;
			}

			return false;
		}// savevmr9bitmap

		
		#endregion
	}// class
}// namespace
