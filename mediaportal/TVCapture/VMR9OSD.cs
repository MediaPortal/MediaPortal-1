using System;
using System.Drawing.Text;
using System.Drawing.Imaging;
using System.Drawing;
using MediaPortal.GUI.Library;
using DShowNET;
using MediaPortal.TV.Database;


namespace MediaPortal.TV.Recording
{
	/// <summary>
	/// Zusammenfassung für VMR9OSD.
	/// </summary>
	public class VMR9OSD
	{
		public VMR9OSD()
		{
			//
			// TODO: Fügen Sie hier die Konstruktorlogik hinzu
			//
			ReadSkinFile();
		}
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
				// channel list
				m_osdChannels.baseRect=xmlreader.GetValueAsString("zaposdChannels","rect","");
			}
		}
		// structs
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
		}
		struct OSDChannelList
		{
			public string baseRect;
		}
		//
		Player.VMR9Util Vmr9=null;
		DateTime m_timeDisplayed=DateTime.Now;
		bool m_muteState=false;
		string m_mediaPath=System.Windows.Forms.Application.StartupPath+@"\osdskin-media\";
		TVChannel m_actualChannel=null;
		int m_channelSNR=0;
		// osd skin
		OSDSkin m_osdSkin;
		OSDChannelList m_osdChannels;

		public bool Mute
		{
			get{return m_muteState;}
			set{m_muteState=value;}
		}
		public Player.VMR9Util VMR9
		{
			set
			{
				if(value!=null)
					Vmr9=value;
			}
		}
		public void RenderChannelList(TVGroup group,string currentChannel)
		{
			if(group==null)
				return;
			if(group.tvChannels.Count<2)
				return;
			//HideBitmap();
			string nextChannel="";
			string prevChannel="";
			bool breakLoop=false;
			int positionActChannel=0;
			int counter=0;
			foreach(TVChannel chan in group.tvChannels)
			{
				if(chan.Name==currentChannel)
				{
					positionActChannel=counter;
					break;
				}
				counter++;

			}

			// render screen
			Bitmap bm=new Bitmap(720,576);//m_mediaPath+@"bgimage.png");
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
			RectangleF layoutRect=new RectangleF(x,y,720-(x*2),576-(y*2));
			//
			SizeF textSize=gr.MeasureString("AAA",drawFont);
			int textHeight=(int)textSize.Height;
			textHeight+=2;
			string headText=group.GroupName;
			
			gr.FillRectangle(new SolidBrush(headColor),x,y,720-(2*x),textHeight);
			gr.DrawString(headText,drawFont,textBrush,layoutRect,StringFormat.GenericTypographic);
			layoutRect.Y+=textHeight;
			int yMax=576-(y*2);
			int channelCount=yMax/textHeight;
			channelCount--;
			int pos=y+textHeight;
			int startAt=positionActChannel-(channelCount/2);
			Log.Write("start list at={0} position={1}",startAt,positionActChannel);
			// draw
			if(group.tvChannels.Count<channelCount)
				startAt=0;
			for(int i=startAt;i<group.tvChannels.Count;i++)
			{
				
				
				if(i>positionActChannel+channelCount)
					continue;
				if(i>=group.tvChannels.Count)
					break;
				TVChannel chan=(TVChannel)group.tvChannels[i];
				if(chan==null)
					break;
				TVProgram prog=chan.GetProgramAt(DateTime.Now);
				string channelText="";
				if(prog!=null)
					channelText=chan.Number.ToString()+" "+chan.Name+" "+"\""+prog.Title+"\"";
				else
					channelText=chan.Number.ToString()+" "+chan.Name;

				if(chan.Name==currentChannel)
				{
					gr.FillRectangle(new SolidBrush(sBoxColor),x,pos,720-(2*x),textHeight);
					gr.DrawString(channelText,drawFont,textBrush,layoutRect,StringFormat.GenericTypographic);
				}
				else
				{
					gr.FillRectangle(new SolidBrush(nBoxColor),x,pos,720-(2*x),textHeight);
					gr.DrawString(channelText,drawFont,textBrush,layoutRect,StringFormat.GenericTypographic);
				}
				pos+=textHeight;
				layoutRect.Y+=textHeight;

				if(pos>=yMax-textHeight)
					break;
			}
			
			SaveVMR9Bitmap(bm,true,true,0.8f);
			bm.Dispose();
			gr.Dispose();
			drawFont.Dispose();
			textBrush.Dispose();
	
		}
		public void RefreshCurrentChannel()
		{
			ShowBitmap(RenderZapOSD(m_actualChannel,m_channelSNR),0.8f);
		}
		public Bitmap RenderZapOSD(TVChannel channel,int signalLevel)
		{
			Bitmap bm=new Bitmap(720,576);//m_mediaPath+@"bgimage.png");
			Graphics gr=Graphics.FromImage(bm);
			int x=60;
			int y=0;
			if(bm==null || gr==null || channel==null)
				return null;
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
							xpos=Convert.ToInt16(seg[1]);
							ypos=Convert.ToInt16(seg[2]);
							width=Convert.ToInt16(seg[3]);
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
			//channel name (chName)
			string chName=m_osdSkin.chName;
			if(chName!=null)
			{
				string[] seg =chName.Split(new char[]{':'});
				if(seg!=null)
				{
					if(seg[0]=="text" && seg.Length==7)
					{
						int xPos=x+Convert.ToInt16(seg[1]);
						int yPos=y+Convert.ToInt16(seg[2]);
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
						int xPos=x+Convert.ToInt16(seg[1]);
						int yPos=y+Convert.ToInt16(seg[2]);
						Color fColor=GetColor(seg[4]);
						Color bColor=GetColor(seg[5]);
						Font drawFont=new System.Drawing.Font(seg[3],Convert.ToInt16(seg[6]));
						Brush drawBrush=new SolidBrush(fColor);
						SizeF xEnd=gr.MeasureString(nowDur,drawFont);
						int xPosEnd=650-((int)xEnd.Width);
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
						int xPos=x+Convert.ToInt16(seg[1]);
						int yPos=y+Convert.ToInt16(seg[2]);
						Color fColor=GetColor(seg[4]);
						Color bColor=GetColor(seg[5]);
						Font drawFont=new System.Drawing.Font(seg[3],Convert.ToInt16(seg[6]));
						Brush drawBrush=new SolidBrush(fColor);
						SizeF xEnd=gr.MeasureString(nextDur,drawFont);
						int xPosEnd=650-((int)xEnd.Width);
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
						int xPos=x+Convert.ToInt16(seg[1]);
						int yPos=y+Convert.ToInt16(seg[2]);
						int width=Convert.ToInt16(seg[3]);
						int height=Convert.ToInt16(seg[4]);
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
							int xPos=x+Convert.ToInt16(seg[1]);
							int yPos=y+Convert.ToInt16(seg[2]);
							int width=Convert.ToInt16(seg[3]);
							int height=Convert.ToInt16(seg[4]);
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
						int xPos=x+Convert.ToInt16(seg[1]);
						int yPos=y+Convert.ToInt16(seg[2]);
						Color fColor=GetColor(seg[4]);
						Color bColor=GetColor(seg[5]);
						gr.DrawString(DateTime.Now.ToShortTimeString(),new System.Drawing.Font(seg[3],Convert.ToInt16(seg[6])),new SolidBrush(fColor),xPos,yPos,StringFormat.GenericTypographic);
					}
				}
			}
			// mute
			string mute=m_osdSkin.mute;
			if(mute!=null)
			{
				string[] seg =mute.Split(new char[]{':'});
				if(seg!=null)
				{
					if(seg[0]=="icon" && seg.Length==4)
					{
						if(System.IO.File.Exists(m_mediaPath+seg[3]))
						{
							Bitmap muteBmp=new Bitmap(m_mediaPath+seg[3]);
							muteBmp.MakeTransparent(Color.White);
							int xPos=Convert.ToInt16(seg[1]);
							int yPos=Convert.ToInt16(seg[2]);
							gr.DrawImage(muteBmp,xPos,yPos,60,60);
							muteBmp.Dispose();
						}
					}
				}
			}
			return bm;
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
		public void ShowBitmap(Bitmap bmp)
		{
			SaveVMR9Bitmap(bmp,true,true,1.0f);
		}
		public void ShowBitmap(Bitmap bmp,float alpha)
		{
			SaveVMR9Bitmap(bmp,true,true,alpha);
		}
		public void HideBitmap()
		{
			SaveVMR9Bitmap(null,false,true,0f);
		}
		bool SaveVMR9Bitmap(System.Drawing.Bitmap bitmap,bool show,bool transparent,float alphaValue)
		{
			
			if(Vmr9!=null)
			{
				if(Vmr9.IsVMR9Connected==false)
					return false;
				System.IO.MemoryStream mStr=new System.IO.MemoryStream();
				int hr=0;
				// transparent image?
				if(bitmap!=null)
				{
					if(transparent==true)
						bitmap.MakeTransparent(Color.Black);
					bitmap.Save(mStr,System.Drawing.Imaging.ImageFormat.Bmp);
					mStr.Position=0;
				}
				VMR9AlphaBitmap bmp=new VMR9AlphaBitmap();

				if(show==true)
				{
					Microsoft.DirectX.Direct3D.Surface surface=GUIGraphicsContext.DX9Device.CreateOffscreenPlainSurface(GUIGraphicsContext.Width,GUIGraphicsContext.Height,Microsoft.DirectX.Direct3D.Format.X8R8G8B8,Microsoft.DirectX.Direct3D.Pool.SystemMemory);
					Microsoft.DirectX.Direct3D.SurfaceLoader.FromStream(surface,mStr,Microsoft.DirectX.Direct3D.Filter.None,0);
					bmp.dwFlags=4|8;
					bmp.color.blu=0;
					bmp.color.green=0;
					bmp.color.red=0;
					unsafe
					{
						bmp.pDDS=(System.IntPtr)surface.UnmanagedComPointer;
					}
					bmp.rDest=new VMR9NormalizedRect();
					bmp.rDest.top=0.0f;
					bmp.rDest.left=0.0f;
					bmp.rDest.bottom=1.0f;
					bmp.rDest.right=1.0f;
					bmp.fAlpha=alphaValue;
					hr=Vmr9.MixerBitmapInterface.SetAlphaBitmap(bmp);
					if(hr!=0)
					{
						return false;
					}
					surface.Dispose();
					m_timeDisplayed=DateTime.Now;
				}
				else
				{
					hr=Vmr9.MixerBitmapInterface.UpdateAlphaBitmapParameters(bmp);
					if(hr!=0)
					{
						return false;
					}		
				}
				// dispose
				return true;
			}
			return false;
		}// savevmr9bitmap
	}// class
}// namespace
