/* 
 *	Copyright (C) 2005 Media Portal
 *	http://mediaportal.sourceforge.net
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */
using System;
using System.Runtime.InteropServices;
using System.Collections;
using MediaPortal.GUI.Library;


namespace MediaPortal.TV.Recording
{
	/// <summary>
	/// Zusammenfassung für DVBTeletext.
	/// </summary>
	public class DVBTeletext
	{
		public delegate void		 PageUpdated();
		public event PageUpdated PageUpdatedEvent;
		IntPtr[,]								 m_cacheTable=new IntPtr[0x900,0x80];
		int[]										 m_currentSubPage=new int[10];
		byte[]									 m_subPageTable=new byte[0x900];
		int[]										 m_currentPage=new int[10];
		int[]										 m_lastRow=new int[10];
		int											 m_actualPage=0;
		int											 m_actualSubPage=0;
		int											 m_pageCount=0;
		System.Drawing.Bitmap		 m_pageBitmap=null;
		System.Drawing.Graphics  m_renderGraphics=null;
		int											 m_txtLanguage=0;
		int											 m_pageWidth=0;
		int											 m_pageHeight=0;
		bool										 m_hiddenMode=true;
		bool										 m_transparentMode=false;
		string									 m_pageSelectText="";
		System.Drawing.Font			 m_teletextFont=null;
		int[]										 m_aitbuffer=new int[10];
		int[]										 m_basicTableOfPages=new int[2352];
		int											 m_aitCount=-1;
		string[]								 m_aitTable=new string[2352];
		int[]										 m_topNavigationPages=new int[]{256,256,256,256};
		string[]								 m_flofAIT=new string[2352];
		int[,]									 m_flofTable=new int[2352,4];
		bool										 m_fastTextDecode=false;
		byte[]									 analogBuffer = new byte[2048];
		byte[]									 tmpBuffer=new byte[46];
		
		//
		//
		string[]				m_mpPage=new string[]
			{
				"0000000000000000000000000000000000000000",
				"0000000000000000000000000000000000000000",
				"0000000000000000000000000000000000000000",
				"0000000001111100000111110000111111100000",
				"0000000011111110001111111001111111110000",
				"0000000011111110001111111001110001110000",
				"0000000011111110001111111001110001110000",
				"0000000011111110001111111001111111110000",
				"0000000011111110001111111000111111100000",
				"0000000011111110001111111000000000000000",
				"0000000011111110001111111000000000000000",
				"0000000011111110001111111000000000000000",
				"0000000001111100000111110000000000000000",
				"0000000000000000000000000000000000000000",
				"0000000000000000000000000000000000000000",
				"0000000000000000000000000000000000000000",
				"0000000000000000000000000000000000000000",
				"0000000000000000000000000000000000000000",
				"0000000000000000000000000000000000000000",
				"0000000000000000000000000000000000000000",
				"0000000000000000000000000000000000000000",


		};
			enum TextColors
		{
			None,
			Black,
			Red   ,
			Green ,
			Yellow,
			Blue   ,
			Magenta,
			Cyan   ,
			White  ,
			Trans1 ,
			Trans2
		}
		enum Attributes
		{
			AlphaBlack,
			AlphaRed,
			AlphaGreen,
			AlphaYellow ,
			AlphaBlue,
			AlphaMagenta,
			AlphaCyan ,
			AlphaWhite,
			Flash,
			Steady,
			EndBox,
			StartBox,
			NormalSize,
			DoubleHeight,
			DoubleWidth,
			DoubleSize,
			MosaicBlack,
			MosaicRed,
			MosaicGreen,
			MosaicYellow,
			MosaicBlue,
			MosaicMagenta,
			MosaicCyan,
			MosaicWhite,
			Conceal,
			ContiguousMosaic,
			SeparatedMosaic,
			Esc,
			BlackBackground,
			NewBackground,
			HoldMosaic,
			ReleaseMosaic
		}
		//
		char[,] m_charTableA =new char[,]{{ '#', '\u016F' },{ '£', '$' }, 
	{ '#', 'õ' },{ 'é', 'ï' }, { '#', '$' }, { '£', '$' },{ '#', '$' },
	{ '#', '\u0149' },{ 'ç', '$' }, { '#', '¤' },{ '#', 'Ë' }, { '#', '¤' },{ '£', '\u011F' }
	};
		char[] m_charTableB =new char[]{'\u010D','@','\u0160','à', '§','é','\u0160','\u0105','¡','\u0162','\u010C','É','\u0130'};
		char[,] m_charTableC =new char[,]{{ '\u0165', '\u017E', 'ý', 'í', '\u0159', 'é' },{'\u2190', '½','\u2192','\u2191', '#', '\u0336' },
	{ 'Ä', 'Ö', '\u017D', 'Ü', 'Õ', '\u0161' },{ 'ë', 'ê', 'ù', 'î', '#', 'è' },
	{ 'Ä', 'Ö', 'Ü', '^', '_', '°' },{ '°', 'ç','\u2192','\u2191', '#', 'ù' },
	{ 'é', '\u0229', '\u017D', '\u010D', '\u016B', '\u0161' },{ '\u01B5', '\u015A', '\u0141', '\u0107', 'ó', '\u0119' },
	{ 'á', 'é', 'í', 'ó', 'ú', '¿' },{ 'Â', '\u015E', '\u01CD', 'Î', '\u0131', '\u0163' },
	{ '\u0106', '\u017D', '\u0110', '\u0160', 'ë', '\u010D' },{ 'Ä', 'Ö', 'Å', 'Ü', '_', 'é' },
	{ '\u015E', 'Ö', 'Ç', 'Ü', '\u01E6', '\u0131' }};
		
		char[,] m_charTableD =new char[,]{{ 'á', '\u011B', 'ú', '\u0161' },{ '¼','\u2016', '¾', '÷' },
	{ 'ä', 'ö', '\u017E', 'ü' },{ 'â', 'ô', 'û', 'ç' },{ 'ä', 'ö', 'ü', 'ß' },
	{ 'à', 'ò', 'è', 'ì' },{ '\u0105', '\u0173', '\u017E', '\u012F' },{ '\u017C', '\u015B', '\u0142', '\u017A' },
	{ 'ü', 'ñ', 'è', 'à' },{ 'â', '\u015F', '\u01CE', 'î' },{ '\u0107', '\u017E', '\u0111', '\u0161' },
	{ 'ä', 'ö', 'å', 'ü' },{ '\u015F', 'ö', 'ç', 'ü' }};
		char[] m_charTableE =new char[] {'\u2190', '\u2192', '\u2191', '\u2193', 'O', 'K', '\u2190', '\u2190', '\u2190'};
		byte[] m_lutTable=new byte[] {0x00,0x08,0x04,0x0c,0x02,0x0a,0x06,0x0e,
										 0x01,0x09,0x05,0x0d,0x03,0x0b,0x07,0x0f,
										 0x00,0x80,0x40,0xc0,0x20,0xa0,0x60,0xe0,
										 0x10,0x90,0x50,0xd0,0x30,0xb0,0x70,0xf0
									 };

		byte[] m_deHamTable =new byte[]{0x01, 0xFF, 0x01, 0x01, 0xFF, 0x00, 0x01, 0xFF, 0xFF, 0x02, 0x01, 0xFF, 0x0A, 0xFF, 0xFF, 0x07,
										   0xFF, 0x00, 0x01, 0xFF, 0x00, 0x00, 0xFF, 0x00, 0x06, 0xFF, 0xFF, 0x0B, 0xFF, 0x00, 0x03, 0xFF,
										   0xFF, 0x0C, 0x01, 0xFF, 0x04, 0xFF, 0xFF, 0x07, 0x06, 0xFF, 0xFF, 0x07, 0xFF, 0x07, 0x07, 0x07,
										   0x06, 0xFF, 0xFF, 0x05, 0xFF, 0x00, 0x0D, 0xFF, 0x06, 0x06, 0x06, 0xFF, 0x06, 0xFF, 0xFF, 0x07,
										   0xFF, 0x02, 0x01, 0xFF, 0x04, 0xFF, 0xFF, 0x09, 0x02, 0x02, 0xFF, 0x02, 0xFF, 0x02, 0x03, 0xFF,
										   0x08, 0xFF, 0xFF, 0x05, 0xFF, 0x00, 0x03, 0xFF, 0xFF, 0x02, 0x03, 0xFF, 0x03, 0xFF, 0x03, 0x03,
										   0x04, 0xFF, 0xFF, 0x05, 0x04, 0x04, 0x04, 0xFF, 0xFF, 0x02, 0x0F, 0xFF, 0x04, 0xFF, 0xFF, 0x07,
										   0xFF, 0x05, 0x05, 0x05, 0x04, 0xFF, 0xFF, 0x05, 0x06, 0xFF, 0xFF, 0x05, 0xFF, 0x0E, 0x03, 0xFF,
										   0xFF, 0x0C, 0x01, 0xFF, 0x0A, 0xFF, 0xFF, 0x09, 0x0A, 0xFF, 0xFF, 0x0B, 0x0A, 0x0A, 0x0A, 0xFF,
										   0x08, 0xFF, 0xFF, 0x0B, 0xFF, 0x00, 0x0D, 0xFF, 0xFF, 0x0B, 0x0B, 0x0B, 0x0A, 0xFF, 0xFF, 0x0B,
										   0x0C, 0x0C, 0xFF, 0x0C, 0xFF, 0x0C, 0x0D, 0xFF, 0xFF, 0x0C, 0x0F, 0xFF, 0x0A, 0xFF, 0xFF, 0x07,
										   0xFF, 0x0C, 0x0D, 0xFF, 0x0D, 0xFF, 0x0D, 0x0D, 0x06, 0xFF, 0xFF, 0x0B, 0xFF, 0x0E, 0x0D, 0xFF,
										   0x08, 0xFF, 0xFF, 0x09, 0xFF, 0x09, 0x09, 0x09, 0xFF, 0x02, 0x0F, 0xFF, 0x0A, 0xFF, 0xFF, 0x09,
										   0x08, 0x08, 0x08, 0xFF, 0x08, 0xFF, 0xFF, 0x09, 0x08, 0xFF, 0xFF, 0x0B, 0xFF, 0x0E, 0x03, 0xFF,
										   0xFF, 0x0C, 0x0F, 0xFF, 0x04, 0xFF, 0xFF, 0x09, 0x0F, 0xFF, 0x0F, 0x0F, 0xFF, 0x0E, 0x0F, 0xFF,
										   0x08, 0xFF, 0xFF, 0x05, 0xFF, 0x0E, 0x0D, 0xFF, 0xFF, 0x0E, 0x0F, 0xFF, 0x0E, 0x0E, 0xFF, 0x0E
									   };

		public DVBTeletext()
		{
			m_pageWidth=1920;
			m_pageHeight=1080;
			//
			// TODO: Fügen Sie hier die Konstruktorlogik hinzu
			//
		}
		~DVBTeletext()
		{
			ClearAllTeletext();
		}
		public void ClearBuffer()
		{
			try
			{
				m_aitbuffer=new int[10];
				m_basicTableOfPages=new int[2352];
				m_aitCount=-1;
				m_aitTable=new string[2352];
				m_topNavigationPages=new int[]{256,512,768,1024};
				m_fastTextDecode=false;
				m_flofAIT=new string[2352];
				m_flofTable=new int[2352,4];

				// free alloctated memory
			
				for(int t=0;t<0x900;t++)
					for(int n=0;n<0x80;n++)
						if((int)m_cacheTable[t,n]!=0)
						{
							Marshal.FreeHGlobal(m_cacheTable[t,n]);
							m_cacheTable[t,n]=(IntPtr)0;
						}
				if(m_pageBitmap!=null)
					m_pageBitmap.Dispose();
				m_pageBitmap=null;

				if(m_renderGraphics!=null)
					m_renderGraphics.Dispose();
				m_renderGraphics=null;

			}
			catch{}
		}
		void ClearAllTeletext()
		{
			// free graphics and bitmap object
			//
			if(m_pageBitmap!=null)
				m_pageBitmap.Dispose();
			m_pageBitmap=null;

			if(m_renderGraphics!=null)
				m_renderGraphics.Dispose();
			m_renderGraphics=null;

			m_aitbuffer=new int[10];
			m_basicTableOfPages=new int[2352];
			m_aitCount=-1;
			m_aitTable=new string[2352];
			m_topNavigationPages=new int[]{256,512,768,1024};

			// free alloctated memory
			for(int t=0;t<0x900;t++)
				for(int n=0;n<0x80;n++)
					if((int)m_cacheTable[t,n]!=0)
						Marshal.FreeHGlobal(m_cacheTable[t,n]);

		}
		// top-text navigation
		public int PageRed
		{
			get
			{
				string hexVal="100";
				try
				{
					hexVal=String.Format("{0:X}",m_topNavigationPages[0]);
				}
				catch
				{
					hexVal="100";
				}
				return Convert.ToInt16(hexVal);
			}
		}
		public int PageGreen
		{
			get
			{
				string hexVal="100";
				try
				{
					hexVal=String.Format("{0:X}",m_topNavigationPages[1]);
				}
				catch
				{
					hexVal="100";
				}
				return Convert.ToInt16(hexVal);
			}
		}
		public int PageYellow
		{
			get
			{
				string hexVal="100";
				try
				{
					hexVal=String.Format("{0:X}",m_topNavigationPages[2]);
				}
				catch
				{
					hexVal="100";
				}
				return Convert.ToInt16(hexVal);
			}
		}
		public int PageBlue
		{
			get
			{
				string hexVal="100";
				try
				{
					hexVal=String.Format("{0:X}",m_topNavigationPages[3]);
				}
				catch
				{
					hexVal="100";
				}
				return Convert.ToInt16(hexVal);
			}
		}
		public string GetPageTitle(int page)
		{
			if(m_aitTable[page]!=null)
				return m_aitTable[page];
			else
				return "";
		}
		//
		public string PageSelectText
		{
			get
			{
				return m_pageSelectText;
			}
			set
			{
				m_pageSelectText="";
				if(value.Length>0 && value.Length<3)
					m_pageSelectText=value+(new string('-',3-value.Length));
			}
		}
		public bool HiddenMode
		{
			get
			{
				return m_hiddenMode;
			}
			set
			{
				m_hiddenMode=value;
			}
		}
		public bool TransparentMode
		{
			get{return m_transparentMode;}
			set{m_transparentMode=value;}
		}
		public int PageLanguage
		{
			get
			{
				return m_txtLanguage;
			}
			set
			{
				m_txtLanguage=value;
			}
		}
		//
		//
		public System.Drawing.Bitmap GetPage(int page,int subpage)
		{
			
			try
			{
				if (m_pageBitmap==null)
				{
						m_pageBitmap=new System.Drawing.Bitmap(m_pageWidth,m_pageHeight);
						m_pageBitmap.MakeTransparent(System.Drawing.Color.HotPink);
				}
				if (m_renderGraphics==null)
					m_renderGraphics=System.Drawing.Graphics.FromImage(m_pageBitmap);

				string sPage="0x"+page.ToString();
				string sSubPage="0x"+subpage.ToString();

				if(sPage==null || sSubPage==null)
					return null;

				m_actualPage=Convert.ToInt16(sPage,16);
				m_actualSubPage=Convert.ToInt16(sSubPage,16);
				

				if (m_actualPage<0x100)
					m_actualPage=0x100;
				if(m_actualPage>0x899)
					m_actualPage=0x899;


				if((int)m_cacheTable[m_actualPage,m_actualSubPage]!=0)
					DecodePage(m_actualPage,m_actualSubPage);
				else 
				{
					for(int sub=0;sub<35;sub++)
						if((int)m_cacheTable[m_actualPage,sub]!=0)//return first aval. subpage
						{
							DecodePage(m_actualPage,sub);
							return m_pageBitmap;
						}
					DecodePage(0xFFFF,0xFFFF);
					return m_pageBitmap;; // nothing found
				}
				return m_pageBitmap;
			}
			catch(Exception)
			{
				return m_pageBitmap;
			}
		}

		//
		// returns the last subpage in cache
		public int PageExists(int page)
		{
			int maxSubPages=-1;
			for(int subpage=0;subpage<50;subpage++)
			{
				if((int)m_cacheTable[page,subpage]!=0)
				{
					maxSubPages=subpage;
				}
			}
			return maxSubPages;
		}
		//
		// returns true if the page and the subpage exists
		public bool PageSubpageExists(int page,int subpage)
		{
			if((int)m_cacheTable[page,subpage]!=0)
				return true;
			return false;
		}
		//
		//
		public System.Drawing.Bitmap PageBitmap
		{
			get
			{
				if (m_pageBitmap==null)
				{
					m_pageBitmap=new System.Drawing.Bitmap(m_pageWidth,m_pageHeight);
					m_pageBitmap.MakeTransparent(System.Drawing.Color.HotPink);
				}
				if (m_renderGraphics==null)
					m_renderGraphics=System.Drawing.Graphics.FromImage(m_pageBitmap);

				DecodePage(m_actualPage,m_actualSubPage);
				return m_pageBitmap;
			}
		}
		//
		//
		bool SetMemory(int size,int page,int subpage)
		{
			if(((int)m_cacheTable[page,subpage])==0)
			{
				byte[] emptyPage = new byte[size];
				for (int i=0; i < size;++i)
					emptyPage[i]=32;
				m_cacheTable[page,subpage]=IntPtr.Zero;
				m_cacheTable[page,subpage]=Marshal.AllocHGlobal(size);
				Marshal.Copy(emptyPage,0,m_cacheTable[page,subpage],size);
				if(m_cacheTable[page,subpage]==IntPtr.Zero)
					return false;
				m_pageCount++;
			}
			return true;
		}

		void FreeMemory(int size,int page,int subpage)
		{
			if((int)m_cacheTable[page,subpage]!=0)
				Marshal.FreeHGlobal(m_cacheTable[page,subpage]);
		}

		//

		public void SetPageSize(int width,int height)
		{
			m_pageWidth=width;
			m_pageHeight=height;
			if(m_pageBitmap!=null)
				m_pageBitmap.Dispose();
			m_pageBitmap=null;

			if(m_renderGraphics!=null)
				m_renderGraphics.Dispose();
			m_renderGraphics=null;
		}

		
		public void SaveAnalogData(IntPtr dataPtr,int bufferLen)
		{
			if (dataPtr==IntPtr.Zero) return;
			if (bufferLen<43) return;
			int maxLines=bufferLen/43;
			int line=0;
			int b=0,byte1=0, byte2=0, byte3=0, byte4=0;
			int actualTransmittingPage=0;
			
			int packetNumber;
			byte magazine;
		//	int pointer=0;
			int dataAdd=(int)dataPtr;

			Marshal.Copy(dataPtr,analogBuffer,0,bufferLen);

			try
			{
				for (line = 0; line < maxLines; line++)
				{
					bool copyData=false;
					for (b=0;b<42;++b)
						tmpBuffer[b]=analogBuffer[line*43+b];
					if (tmpBuffer[0]==0 && tmpBuffer[1]==0 && tmpBuffer[2]==0 && tmpBuffer[3]==0 && tmpBuffer[4]==0)
						continue;
					
						byte1 = m_deHamTable[tmpBuffer[0]];
						byte2 = m_deHamTable[tmpBuffer[1]];

						//hamming error?
						if (byte1 == 0xFF || byte2 == 0xFF)
							continue;

						byte1 &= 8;
						packetNumber = byte1>>3 | byte2<<1;
						//  mag number
						magazine =(byte)(m_deHamTable[tmpBuffer[0]] & 7);
					  if (packetNumber<0 || packetNumber==25 || packetNumber==26 || packetNumber>27) continue;
						if (packetNumber == 0)
						{
							m_lastRow[magazine]=0;
							//pagenumber
							byte1 = m_deHamTable[tmpBuffer[0]];
							byte2 = m_deHamTable[tmpBuffer[3]];
							byte3 = m_deHamTable[tmpBuffer[2]];
							if (byte1 == 0xFF || byte2 == 0xFF || byte3 == 0xFF)
							{
								m_currentPage[magazine] = -1;
								continue;
							}

							byte1 &= 7;
							if (byte1==0)
								byte1 = 8;
							actualTransmittingPage = byte1<<8 | byte2<<4 | byte3;

//							Log.Write("page:{0:X}",actualTransmittingPage);
							if (byte2 > 9 || byte3 > 9) 
							{
//								Log.Write("page:{0:X} ignored",actualTransmittingPage);
								m_currentPage[magazine] = -1;
								continue;
							}
							m_currentPage[magazine] = actualTransmittingPage;

							//subpage number
							byte1 = m_deHamTable[tmpBuffer[7]]; //3
							byte2 = m_deHamTable[tmpBuffer[6]]; //f
							byte3 = m_deHamTable[tmpBuffer[5]]; //7
							byte4 = m_deHamTable[tmpBuffer[4]]; //f

//							Log.Write("page:{0:X} subpage:{1:X} {2:X} {3:X} {4:X}",actualTransmittingPage,byte1,byte2,byte3,byte4);
							if (byte1 == 0xFF || byte2 == 0xFF || byte3 == 0xFF || byte4 == 0xFF)
							{
//								Log.Write("page:{0:X} subpage:{1:X} {2:X} {3:X} {4:X} ignored",actualTransmittingPage,byte1,byte2,byte3,byte4);
								m_currentPage[magazine] = -1;
								continue;
							}

							byte1 &= 3;
							byte3 &= 7;

							if (byte1 != 0 || byte2 != 0 || byte4 > 9)
							{
//								Log.Write("page:{0:X} subpage:{1:X} {2:X} {3:X} {4:X} ignored2",actualTransmittingPage,byte1,byte2,byte3,byte4);
								m_currentPage[magazine] = -1;
								continue;
							}
							m_currentSubPage[magazine] = byte3<<4 | byte4;

							//language
							int languageCode=0;
							byte1 = m_deHamTable[tmpBuffer[9]];
							if (byte1 == 0xFF)
								languageCode = 0;
							else
								languageCode =((byte1 >> 3) & 0x01) | (((byte1 >> 2) & 0x01) << 1) | (((byte1 >> 1) & 0x01) << 2);

							switch(languageCode)
							{
								case 0:
									m_txtLanguage=1;
									break;
								case 1:
									m_txtLanguage=4;
									break;
								case 2:
									m_txtLanguage=11;
									break;
								case 3:
									m_txtLanguage=5;
									break;
								case 4:
									m_txtLanguage=3;
									break;
								case 5:
									m_txtLanguage=8;
									break;
								case 6:
									m_txtLanguage=0;
									break;
								default:
									m_txtLanguage=1;
									break;

							}

							if(SetMemory(1000,m_currentPage[magazine],m_currentSubPage[magazine])==false)
								return;
							m_subPageTable[m_currentPage[magazine]] = (byte)m_currentSubPage[magazine];

							for (b = 10; b < 42; b++)
							{
								tmpBuffer[b] &= 127;
							}

							if ((m_deHamTable[tmpBuffer[5]] & 8)!=0)   /* C4 -> erase page */
							{
								for(int t=0;t<960;t++)
									Marshal.WriteByte(m_cacheTable[m_currentPage[magazine],m_currentSubPage[magazine]],t,32);
							}
							copyData=true;
						}
						else if (packetNumber <= 24)
						{
							if (m_currentPage[magazine] == -1) continue;
							if (m_lastRow[magazine]!=27)
							{
								if (packetNumber< m_lastRow[magazine] ) continue;
							}

							m_lastRow[magazine]=packetNumber;
//							Log.Write("mag:{0} row:{1}",magazine,packetNumber);
							if(SetMemory(1000,m_currentPage[magazine],m_currentSubPage[magazine])==false)
								return;

							if ((m_currentPage[magazine] & 0x0F0) <= 0x090 && (m_currentPage[magazine] & 0x00F) <= 0x009)
							{  
								for (b = 2; b < 42; b++)
								{
									tmpBuffer[b] &= 127;
								}
							}
							copyData=true;
						}
						else if(packetNumber==27)
						{
							if (packetNumber< m_lastRow[magazine] ) continue;
							m_lastRow[magazine]=packetNumber;
							if (m_currentPage[magazine] ==-1) continue;
							int pageNumber=m_currentPage[magazine];
							int subPageNumber=m_currentSubPage[magazine];
							if(m_deHamTable[tmpBuffer[2]]==0)
							{
								byte1 = m_deHamTable[tmpBuffer[0]];
								if (byte1!=255)
								{
									byte1 &= 7;
									for (b = 0; b < 4; b++)
									{
										m_flofTable[pageNumber,b]=0;
										byte2 = m_deHamTable[tmpBuffer[b*6+4]];
										byte3 = m_deHamTable[tmpBuffer[b*6+3]];

										if (byte2!=255 && byte3!=255)
										{
											byte4 = ((byte1 & 4)^((m_deHamTable[tmpBuffer[b*6+8]]>>1) & 4)) | ((byte1 & 2)^((m_deHamTable[tmpBuffer[b*6+8]]>>1) & 2)) |  ((byte1 & 1)^((m_deHamTable[tmpBuffer[b*6+6]]>>3) & 1));
											if (byte4 == 0) byte4 = 8;
											if (byte2<=9 && byte3<= 9)
												m_flofTable[pageNumber,b] = byte4<<8 | byte2<<4 | byte3;
										}
									}
								}
							}
							copyData=true;
						}
						
					if (copyData)
					{
						if (m_currentPage[magazine]!= -1 && m_currentSubPage[magazine] != -1)
						{
							if (packetNumber <= 24 && ((int)m_cacheTable[m_currentPage[magazine],m_currentSubPage[magazine]]!=0))
							{
								IntPtr adr=m_cacheTable[m_currentPage[magazine],m_currentSubPage[magazine]];
								int offset=packetNumber*40;
								adr=(IntPtr)(((int)adr)+offset);
								Marshal.Copy(tmpBuffer,2,adr,40);
								if (m_currentPage[magazine]==m_actualPage && m_currentSubPage[magazine]==m_actualSubPage)
								{
									if (PageUpdatedEvent!=null)
										PageUpdatedEvent();
								}
							} 
						}
					}
				}// for(line=0
			}
			catch(Exception )
			{ 
			}
		}

		int logcount=0;
		public void SaveData(IntPtr dataPtr)
		{
			if (dataPtr==IntPtr.Zero) return;
			
			int line=0;
			int b=0,byte1=0, byte2=0, byte3=0, byte4=0;
			
			int packetNumber;
			byte magazine;
			int pointer=0;
			int dataAdd=(int)dataPtr;
			try
			{
				for (line = 0; line < 4; line++)
				{
					bool copyData=false;
					Marshal.Copy((IntPtr)((dataAdd+4)+(line*0x2e)),tmpBuffer,0,46);

					pointer = line*0x2e;
					if ((tmpBuffer[0]==0x02 || tmpBuffer[0]==0x03) && (tmpBuffer[1]==0x2C))
					{
						for (b=4;b<46;b++)
						{
							byte upper=0;
							byte lower=0;
							upper = (byte)((tmpBuffer[b] >> 4) & 0xf);
							lower = (byte)(tmpBuffer[b] & 0xf);
							tmpBuffer[b-4] = (byte)((m_lutTable[upper]) | (m_lutTable[lower+16]));
						}//for(b=4;

						byte1 = m_deHamTable[tmpBuffer[0]];
						byte2 = m_deHamTable[tmpBuffer[1]];

						if (byte1 == 0xFF || byte2 == 0xFF)
							continue;

						byte1 &= 8;
						packetNumber = byte1>>3 | byte2<<1;
						if (packetNumber<0 || packetNumber==25 || packetNumber==26 || packetNumber>27) continue;
						//  mag number
						magazine =(byte)(m_deHamTable[tmpBuffer[0]] & 7);
						if (packetNumber == 0)
						{
							m_lastRow[magazine]=0;
							byte1 = m_deHamTable[tmpBuffer[0]];
							byte2 = m_deHamTable[tmpBuffer[3]];
							byte3 = m_deHamTable[tmpBuffer[2]];

							byte g1 = m_deHamTable[tmpBuffer[7]];
							byte g2 = m_deHamTable[tmpBuffer[6]];
							byte g3 = m_deHamTable[tmpBuffer[5]];
							byte g4 = m_deHamTable[tmpBuffer[4]];
							if (logcount>0) 
								Log.Write("page:{0:x} {1:X} {2:X} {3:X} {4:X}",( byte1<<8 | byte2<<4), g1 ,g2,g3,g4);
							if (byte1 == 0xFF || byte2 == 0xFF || byte3 == 0xFF)
							{
								if (logcount>0) Log.Write("ignore");
								m_currentPage[magazine] = -1;
								continue;
							}

							byte1 &= 7;
							if (byte1==0)
								byte1 = 8;

							if (byte2 > 9 || byte3 > 9) 
							{
								if (logcount>0) Log.Write("ignore");
								m_currentPage[magazine] = -1;
								continue;
							}
							m_currentPage[magazine] = byte1<<8 | byte2<<4 | byte3;

							byte1 = m_deHamTable[tmpBuffer[7]];
							byte2 = m_deHamTable[tmpBuffer[6]];
							byte3 = m_deHamTable[tmpBuffer[5]];
							byte4 = m_deHamTable[tmpBuffer[4]];

							if (byte1 == 0xFF || byte2 == 0xFF || byte3 == 0xFF || byte4 == 0xFF)
							{
								if (logcount>0) Log.Write("ignore");
								m_currentPage[magazine] = -1;
								continue;
							}

							byte1 &= 3;
							byte3 &= 7;

							if (byte1 != 0 || byte2 != 0 || byte4 > 9)
							{
								if (logcount>0) Log.Write("ignore");
								m_currentPage[magazine] = -1;
								continue;
							}
							m_currentSubPage[magazine] = byte3<<4 | byte4;
/*
							if (m_currentPage[magazine]==0x100)
							{
								Log.Write("page {0:x} {1:x}", m_currentPage[magazine],m_currentSubPage[magazine]);
								logcount=50;
							}*/
							int languageCode=0;
							byte1 = m_deHamTable[tmpBuffer[9]];
							if (byte1 == 0xFF)
								languageCode = 0;
							else
								languageCode =((byte1 >> 3) & 0x01) | (((byte1 >> 2) & 0x01) << 1) | (((byte1 >> 1) & 0x01) << 2);

							switch(languageCode)
							{
								case 0:
									m_txtLanguage=1;
									break;
								case 1:
									m_txtLanguage=4;
									break;
								case 2:
									m_txtLanguage=11;
									break;
								case 3:
									m_txtLanguage=5;
									break;
								case 4:
									m_txtLanguage=3;
									break;
								case 5:
									m_txtLanguage=8;
									break;
								case 6:
									m_txtLanguage=0;
									break;
								default:
									m_txtLanguage=1;
									break;

							}

							if(SetMemory(1000,m_currentPage[magazine],m_currentSubPage[magazine])==false)
								return;
							m_subPageTable[m_currentPage[magazine]] = (byte)m_currentSubPage[magazine];

							for (b = 10; b < 42; b++)
							{
								if ( ((tmpBuffer[b]&1)>0) ^ (((tmpBuffer[b]>>1)&1)>0) ^
									((tmpBuffer[b]>>2)&1)>0 ^ ((tmpBuffer[b]>>3)&1)>0 ^
									((tmpBuffer[b]>>4)&1)>0 ^ ((tmpBuffer[b]>>5)&1)>0 ^
									((tmpBuffer[b]>>6)&1)>0 ^ (tmpBuffer[b]>>7)>0)
									tmpBuffer[b] &= 127;
								else
									tmpBuffer[b] = 32;
							}

							if ((m_deHamTable[tmpBuffer[5]] & 8)!=0)   /* C4 -> erase page */
							{
								for(int t=0;t<960;t++)
									Marshal.WriteByte(m_cacheTable[m_currentPage[magazine],m_currentSubPage[magazine]],t,32);
							}
							 copyData=true;
						}
						else if (packetNumber <= 24)
						{
							if (logcount>0) 
								Log.Write("mag:{0:x} {1}",magazine,packetNumber);
							if (m_currentPage[magazine] == -1) 
							{
								if (logcount>0) Log.Write("ignore p");
								continue;
							}
							if (m_lastRow[magazine]!=27)
							{
								if (packetNumber< m_lastRow[magazine] )  
								{
									if (logcount>0) Log.Write("ignore p2");
									continue;
								}
							}

							m_lastRow[magazine]=packetNumber;
							if(SetMemory(1000,m_currentPage[magazine],m_currentSubPage[magazine])==false)
								return;

							if (m_currentSubPage[magazine]==-1) 
							{
								if (logcount>0) Log.Write("ignore p3");
								continue;
							}

							for (b = 2; b < 42; b++)
							{
								tmpBuffer[b] &= 127;
							}
							copyData=true;
						}
						else if(packetNumber==27)
						{
							if (logcount>0) 
								Log.Write("mag:{0:x} {1}",magazine,packetNumber);
							if (m_currentPage[magazine] == -1) continue;
							if (packetNumber< m_lastRow[magazine] ) continue;
							m_lastRow[magazine]=packetNumber;
							
							if (m_currentPage[magazine]==-1) continue;
							int pageNumber=m_currentPage[magazine];
							int subPageNumber=m_currentSubPage[magazine];
							if(m_deHamTable[tmpBuffer[2]]==0)
							{

								byte1 = m_deHamTable[tmpBuffer[0]];
								if (byte1!=255)
								{
									byte1 &= 7;
									for (b = 0; b < 4; b++)
									{
										m_flofTable[pageNumber,b]=0;
										byte2 = m_deHamTable[tmpBuffer[b*6+4]];
										byte3 = m_deHamTable[tmpBuffer[b*6+3]];

										if (byte2!=255 && byte3!=255)
										{
											byte4 = ((byte1 & 4)^((m_deHamTable[tmpBuffer[b*6+8]]>>1) & 4)) | ((byte1 & 2)^((m_deHamTable[tmpBuffer[b*6+8]]>>1) & 2)) |  ((byte1 & 1)^((m_deHamTable[tmpBuffer[b*6+6]]>>3) & 1));
											if (byte4 == 0) byte4 = 8;
											if (byte2<=9 && byte3<= 9)
												m_flofTable[pageNumber,b] = byte4<<8 | byte2<<4 | byte3;
										}
									}

								}
							}
							copyData=true;
						}
						
						if (logcount>0) logcount--;
						if (copyData)
						{
							if (m_currentPage[magazine]!= -1 && m_currentSubPage[magazine] != -1)
							{
								if (packetNumber <= 24 && ((int)m_cacheTable[m_currentPage[magazine],m_currentSubPage[magazine]]!=0))
								{
									IntPtr adr=m_cacheTable[m_currentPage[magazine],m_currentSubPage[magazine]];
									int offset=packetNumber*40;
									adr=(IntPtr)(((int)adr)+offset);
									Marshal.Copy(tmpBuffer,2,adr,40);
									if (m_currentPage[magazine]==m_actualPage && m_currentSubPage[magazine]==m_actualSubPage)
									{
										if (PageUpdatedEvent!=null)
											PageUpdatedEvent();
									}
								} 
							}
						}
					}//if ((tmpBuffer
				}// for(line=0
			}
			catch(Exception )
			{ 
			}
		}

		bool IsDEC(int i)
		{
			return (bool)(((i & 0x00F) <= 9) && ((i & 0x0F0) <= 0x90));
		}

		//
		// decode and render the page
		//
		void BasicTopTable()
		{
			/* basic top table */
			int page=0;
			int byte1=0;
			int byte2=0;
			int byte3=0;
			int byte4=0;
			int i=0;
			byte[] page496=new byte[1024];
			
			m_aitCount=-1;
			if((int)m_cacheTable[496,0]==0)
				return;
			Marshal.Copy(m_cacheTable[496,0],page496,0,960);
			
			page = 0x100;
			for (i = 0; i < 799; i++)
			{
				byte1 = page496[i+40];
				if (byte1 == ' ')
					byte1 = 0;
				else
				{
					byte1 = m_deHamTable[byte1];
					if (byte1 == 0xFF) /* hamming error in btt */
					{
						page496[40+799] = 0; /* mark btt as not received */
						return;
					}
				}
				m_basicTableOfPages[page] = byte1;
				page=GetNextDecimal(page);
			}
			int ait = 0; 
			for (i = 0; i < 10; i++)
			{
				int offset=840+8*i;
				byte1 = m_deHamTable[page496[offset]];

				if (byte1 == 0xE)
					continue;
				else if (byte1 == 0xF)
					break; 

				byte4 = m_deHamTable[page496[offset+7]];

				if (byte4!=2)
					continue;

				byte2 = m_deHamTable[page496[offset+1]];
				byte3 = m_deHamTable[page496[offset+2]];

				if (byte1 == 0xFF || byte2 == 0xFF || byte3 == 0xFF)
					return;

				byte1 = byte1<<8 | byte2<<4 | byte3;
				m_aitbuffer[ait] = byte1;
				ait++;
			}
			m_aitCount=ait;
		}
		//
		//
		void FastTextTable(int pageNumber,int subPage)
		{
			if (pageNumber<0x100 || pageNumber>=0x900) return;
			if (subPage<0 || subPage>0x79) return;
			m_flofAIT=new string[2352];

			if((int)m_cacheTable[pageNumber,subPage]==0)
				return;

			byte[] buffer=new byte[1024];

			if(m_flofTable[pageNumber,0]!=0 || m_flofTable[pageNumber,1]!=0 || m_flofTable[pageNumber,2]!=0 || m_flofTable[pageNumber,3]!=0)
				m_fastTextDecode=true;

			Marshal.Copy(m_cacheTable[pageNumber,subPage],buffer,0,1000);
			int pointer=960;

			string table=System.Text.Encoding.ASCII.GetString(buffer,pointer,40);
			int button=1;
			int buttonNumber=-1;
			do			
			{
				
				string tmpLink="";
				for(;IsText(buffer[pointer])==false;pointer++)
				{ 
					if(buttonNumber==-1)
						buttonNumber=buffer[pointer];
					if(buttonNumber>4) buttonNumber=4;
					// search for the start
					if(pointer>=1000)
						break;
				}
				if(pointer>=1000)
					break;
				for(;IsText(buffer[pointer])==true;pointer++)
				{
					tmpLink+=""+((char)buffer[pointer]); // add the char
				}

				if (buttonNumber>=1 && buttonNumber<=4)
				{
					int off=m_flofTable[pageNumber,buttonNumber-1];
					if (off >=0 && off < 2352)
					{
						m_flofAIT[off]=tmpLink;
					}
				}
				button++;
				buttonNumber=-1;
			}
			while(pointer<1000 && button<=4);
		}
		void AdditionalInformationTable()
		{
			int page=0;
			int byte1=0;
			int byte2=0;
			int byte3=0; 
			bool found=false;

			for (int i = 0; i <= m_aitCount; i++)
			{
				page = m_aitbuffer[i];
				if ((int)m_cacheTable[page,0]!=0)
				{
					int tmpValue=Marshal.ReadByte((IntPtr)(((int)m_cacheTable[page,0])+40+20*43));
					if (tmpValue!=1)
					{
						for (int j = 0; j < 44; j++)
						{
							byte1 = m_deHamTable[Marshal.ReadByte((IntPtr)(((int)m_cacheTable[page,0])+40+20*j))];

							if (byte1 == 0xE)
								continue; 

							if (byte1 == 0xF)
								break;

							byte2 = m_deHamTable[Marshal.ReadByte((IntPtr)(((int)m_cacheTable[page,0])+40+20*j+1))];
							byte3 = m_deHamTable[Marshal.ReadByte((IntPtr)(((int)m_cacheTable[page,0])+40+20*j+2))];

							if (byte1 == 0xFF || byte2 == 0xFF || byte3 == 0xFF)
							{
								return;
							}

							if (byte1>8 || byte2>9 || byte3>9)
							{
								continue;
							}

							byte1 = byte1<<8 | byte2<<4 | byte3; 
							found = false;

							for (byte2 = 0; byte2 <11; byte2++)
							{
								byte3 = Marshal.ReadByte((IntPtr)(((int)m_cacheTable[page,0])+40+20*j+byte2+8));
								if (((byte3&1) ^ ((byte3>>1)&1) ^ ((byte3>>2)&1) ^ ((byte3>>3)&1) ^
									((byte3>>4)&1) ^ ((byte3>>5)&1) ^ ((byte3>>6)&1) ^ (byte3>>7))!=0)
									byte3 &= 0x7F;
								else
									byte3 = ' ';

								if (byte3 < ' ')
									byte3 = ' ';

								if (byte3 == ' ' && found==false)
									m_aitTable[byte1]= new string(' ',10);
								else
								{
									m_aitTable[byte1]+=""+((char)byte3);
									found = true;
								}
							}
						}
						m_aitbuffer[i] = 0;
					}
				}
			}

		}
		//
		// helper
		int GetNextDecimal(int val)
		{
			int ret=val;
			ret++;

			if ((ret & 15)>9)
				ret+=6;

			if ((ret & 240)>144)
				ret+=96;

			if (ret>2201)
				ret=256;

			return ret;
		}
		bool IsText(byte val)
		{
			if(val>=' ') 
				return true;
			return false;

		}
		bool IsAlphaNumeric(byte val)
		{
			if(val>='A' && val<='Z')
				return true;
			if(val>='a' && val<='z')
				return true;
			if(val>='0' && val<='9')
				return true;
			return false;
		}
		int GetPreviousDecimal(int val)           /* counting down */
		{
			int ret=val;
			ret--;

			if ((ret & 15)>0x09)
				ret-=6;

			if ((ret & 240)>144)
				ret-=96;

			if (ret < 256)
				ret = 2201;

			return ret;
		}
		int TopNavigation(int page,bool getGroup,bool upDown)
		{
			int actPage=page;
			int group=0;
			int block=0;

			do 
			{
				if (upDown==true)
					actPage=GetNextDecimal(actPage);
				else
					actPage=GetPreviousDecimal(actPage);

				if (m_basicTableOfPages[actPage]!=0)
				{
					if (getGroup==true)
					{
						if (m_basicTableOfPages[actPage]>=6 && m_basicTableOfPages[actPage]<=7)
							return actPage;

						if (group!=0 && (actPage & 15) == 0)
							return actPage;
					}
					if (m_basicTableOfPages[actPage]>=2 && m_basicTableOfPages[actPage]<=5)
						return actPage;

					if (block!=0 && (actPage & 15) == 0)
						return actPage;
				}
			} while (actPage!=page);

			if (group!=0) 
				return group;
			else if (block!=0)
				return block;
			else
				return page;
		}
		//
		//
		bool DecodePage(int mPage,int sPage)
		{
			int row, col;
			int hold;
			int foreground, background, doubleheight, charset, mosaictype;
			byte held_mosaic;
			bool flag=false;
			int boxed=0;
			byte[] pageChars=new byte[1024];
			int[] pageAttribs=new int[1024];

			bool isSubtitlePage=false;
			if(mPage<0xffff)
			{
				if((int)m_cacheTable[mPage,sPage]==0)
					return false;
			
				Marshal.Copy(m_cacheTable[mPage,sPage],pageChars,0,960);

				if ((m_deHamTable[pageChars[5]] & 12)>0)
					boxed = 1;
				else
					boxed = 0;

				if ((m_deHamTable[pageChars[7-2]] & 0x4)>0 && m_transparentMode)
					isSubtitlePage=true;

				for (row = 0; row < 24; row++)
				{
					if (row==24 && isSubtitlePage)
					{
						for (i=0; i < 40; ++i)
						{
							pageChars[row*40+i]=32;
							pageAttribs[row*40+i]=((int)TextColors.Black<<4) | ((int)TextColors.White);
						}
					}
					else
					{
						foreground   = (int)TextColors.White;
						if(isSubtitlePage==false&&m_transparentMode==false)
							background  = (int)TextColors.Black;
						else
							background  = (int)TextColors.Trans1;

						doubleheight = 0;
						charset      = 0;
						mosaictype   = 0;
						hold         = 0;
						held_mosaic  = 32;
					
						for(int loop1=0;loop1<40;loop1++)
							if(pageChars[(row*40)+loop1]==(int)Attributes.StartBox)
							{
								flag=true;
								break;
							}

						if (boxed!=0 && flag==false)
						{
							foreground = (int)TextColors.Trans1;
							background = (int)TextColors.Trans1;
						}
					
						for (col = 0; col < 40; col++)
						{
							int index = row*40 + col;
				
							pageAttribs[index] = (doubleheight<<10 | charset<<8 | background<<4 | foreground);
				
							if (pageChars[index] < 32)
							{
								switch (pageChars[index])
								{
									case (int)Attributes.AlphaBlack:
										foreground = (int)TextColors.Black;
										charset = 0;
										break;

									case (int)Attributes.AlphaRed:
										foreground = (int)TextColors.Red;
										charset = 0;
										break;

									case (int)Attributes.AlphaGreen:
										foreground = (int)TextColors.Green;
										charset = 0;
										break;

									case (int)Attributes.AlphaYellow:
										foreground = (int)TextColors.Yellow;
										charset = 0;
										break;

									case (int)Attributes.AlphaBlue:
										foreground = (int)TextColors.Blue;
										charset = 0;
										break;

									case (int)Attributes.AlphaMagenta:
										foreground = (int)TextColors.Magenta;
										charset = 0;
										break;

									case (int)Attributes.AlphaCyan:
										foreground = (int)TextColors.Cyan;
										charset = 0;
										break;

									case (int)Attributes.AlphaWhite:
										foreground = (int)TextColors.White;
										charset = 0;
										break;

									case (int)Attributes.Flash:
										break;

									case (int)Attributes.Steady:
										break;

									case (int)Attributes.EndBox:
										if (boxed>0)
										{
											foreground = (int)TextColors.Trans1;
											background = (int)TextColors.Trans1;
										}
										break;

									case (int)Attributes.StartBox:
										if (boxed>0)
										{
											if (col > 0)
												for(int loop1=0;loop1<col;loop1++)
													pageChars[(row*40)+loop1]=32;
											for (int clear = 0; clear < col; clear++)
												pageAttribs[row*40 + clear] = doubleheight<<10 |charset<<8|(int)TextColors.Trans1<<4 | (int)TextColors.Trans1;
										}
										break;

									case (int)Attributes.NormalSize:
										doubleheight = 0;
										pageAttribs[index] =( doubleheight<<10 | charset<<8 | background<<4 | foreground);
										break;

									case (int)Attributes.DoubleHeight:
										if (row < 23)
											doubleheight = 1;
										break;

									case (int)Attributes.MosaicBlack:
										foreground = (int)TextColors.Black;
										charset = 1 + mosaictype;
										break;

									case (int)Attributes.MosaicRed:
										foreground = (int)TextColors.Red;
										charset = 1 + mosaictype;
										break;

									case (int)Attributes.MosaicGreen:
										foreground = (int)TextColors.Green;
										charset = 1 + mosaictype;
										break;

									case (int)Attributes.MosaicYellow:
										foreground = (int)TextColors.Yellow;
										charset = 1 + mosaictype;
										break;

									case (int)Attributes.MosaicBlue:
										foreground = (int)TextColors.Blue;
										charset = 1 + mosaictype;
										break;

									case (int)Attributes.MosaicMagenta:
										foreground = (int)TextColors.Magenta;
										charset = 1 + mosaictype;
										break;

									case (int)Attributes.MosaicCyan:
										foreground = (int)TextColors.Cyan;
										charset = 1 + mosaictype;
										break;

									case (int)Attributes.MosaicWhite:
										foreground = (int)TextColors.White;
										charset = 1 + mosaictype;
										break;

									case (int)Attributes.Conceal:
										if (m_hiddenMode==true) 
										{
											foreground = background;
											pageAttribs[index] = (doubleheight<<10 | charset<<8 | background<<4 | foreground);
										}
										break;

									case (int)Attributes.ContiguousMosaic:
										mosaictype = 0;
										if (charset>0)
										{
											charset = 1;
											pageAttribs[index] = (doubleheight<<10 | charset<<8 | background<<4 | foreground);
										}
										break;

									case (int)Attributes.SeparatedMosaic:
										mosaictype = 1;
										if (charset>0)
										{
											charset = 2;
											pageAttribs[index] = (doubleheight<<10 | charset<<8 | background<<4 | foreground);
										}
										break;

									case (int)Attributes.Esc:
										break;

									case (int)Attributes.BlackBackground:
										background = (int)TextColors.Black;
										pageAttribs[index] = (doubleheight<<10 | charset<<8 | background<<4 | foreground);
										break;

									case (int)Attributes.NewBackground:
										background = foreground;
										pageAttribs[index] = (doubleheight<<10 | charset<<8 | background<<4 | foreground);
										break;

									case (int)Attributes.HoldMosaic:
										hold = 1;
										break;

									case (int)Attributes.ReleaseMosaic:
										hold = 2;
										break;
								}

								if (hold>0 && charset>0)
									pageChars[index] = held_mosaic;
								else
									pageChars[index] = 32;

								if (hold == 2)
									hold = 0;
							}
							else 
							{
								if (charset>0)
									held_mosaic = pageChars[index];

								if (doubleheight>0)
									pageChars[index + 40] = 0xFF;
							}
						}

					
						for(int count=(row+1)*40;count<((row+1)*40)+40;count++)
						{
							if(pageChars[count]==255)
							{
								for(int loop1=0;loop1<40;loop1++)
									pageAttribs[(row+1)*40 + loop1] = ((pageAttribs[(row*40) + loop1] & 0xF0) | ((pageAttribs[(row*40) + loop1] & 0xF0)>>4));
						
								row++;
								break;
							}
						}
					}
				}
			
				if (IsDEC(mPage))
				{
					if (isSubtitlePage)
					{
						for (int i=0; i < 40;++i)
						{
							pageChars[i]=0;
							pageAttribs[i]=((int)TextColors.Black<<4) | ((int)TextColors.White);
						}
					}
					else
					{
						int i;
						string pageNumber="";
						int lineColor=0;
						if(m_pageSelectText.IndexOf("-")==-1)
						{
							lineColor=(int)TextColors.Green;
							pageNumber=Convert.ToString(mPage,16)+"/"+Convert.ToString(sPage,16);
						}
						else
						{
							lineColor=(int)TextColors.Red;
							pageNumber=m_pageSelectText;
						}
						string headline="MediaPortal P."+pageNumber;
						headline+=new string((char)32,32-headline.Length);
						byte[] mpText=System.Text.Encoding.ASCII.GetBytes(headline);
						System.Array.Copy(mpText,0,pageChars,0,mpText.Length);
						for (i = 0; i < 11; i++)
							pageAttribs[i] = ((int)TextColors.Black<<4) | lineColor;
						for (i = 12; i < 40; i++)
							pageAttribs[i] = ((int)TextColors.Black<<4) | ((int)TextColors.White);
					}

				}
			}
			else // waiting page...
			{
				int i;
				string pageNumber="";
				int lineColor=0;
				if(m_pageSelectText.IndexOf("-")==-1)
				{
					lineColor=(int)TextColors.Green;
					pageNumber=Convert.ToString(m_actualPage,16)+"/"+Convert.ToString(m_actualSubPage,16);
				}
				else
				{
					lineColor=(int)TextColors.Red;
					pageNumber=m_pageSelectText;
				}
				string headline="MediaPortal P."+pageNumber;
				string hintLine=String.Format(GUILocalizeStrings.Get(25001),m_actualPage);
				headline+=new string((char)32,32-headline.Length);
				byte[] mpText=System.Text.Encoding.ASCII.GetBytes(headline);
				System.Array.Copy(mpText,0,pageChars,0,mpText.Length);
				mpText=System.Text.Encoding.ASCII.GetBytes(hintLine);
				System.Array.Copy(mpText,0,pageChars,40,mpText.Length);
				for (i = 0; i < 11; i++)
					pageAttribs[i] = ((int)TextColors.Black<<4) | lineColor;
				for (i = 12; i < 40; i++)
					pageAttribs[i] = ((int)TextColors.Black<<4) | ((int)TextColors.White);
				for(i=40;i<80;i++)
					pageAttribs[i] = ((int)TextColors.Black<<4) | (int)TextColors.Yellow;
				int pos=80;
				foreach(string line in m_mpPage)
				{
					for(i=0;i<line.Length;i++)
					{
						if(m_hiddenMode==false)
							pageAttribs[pos+i] = ((int)TextColors.Black<<4) | (int)TextColors.Black;
						else
							pageAttribs[pos+i] = ((int)TextColors.Blue<<4) | (int)TextColors.White;

						if(line.Substring(i,1)=="1")
							pageChars[pos+i] = 0xEC;
						else
							pageChars[pos+i]=0;

					}
					pos+=40;
				}
			}
			// render
			bool hasTopText=true;


			if((int)m_cacheTable[496,0]!=0)
				BasicTopTable();
			if((int)m_cacheTable[498,0]!=0)
				AdditionalInformationTable();
			if((int)m_cacheTable[496,0]==0)
			{
				hasTopText=false;
				FastTextTable(mPage,sPage);
			}
			int y = 0;
			int x;
			int width=m_pageWidth/40;
			int height=(m_pageHeight-2)/25;
			int fntSize=(width-2<10)?10:width-2;
			m_teletextFont=new System.Drawing.Font("Courier New",fntSize,System.Drawing.FontStyle.Bold);
			if (m_renderGraphics!=null)
			{
				if (isSubtitlePage)
					m_renderGraphics.FillRectangle(new System.Drawing.SolidBrush(System.Drawing.Color.HotPink),0,0,m_pageWidth,m_pageHeight);
				else
					m_renderGraphics.FillRectangle(new System.Drawing.SolidBrush(System.Drawing.Color.Black),0,0,m_pageWidth,m_pageHeight);
			}
			int[] topColors=new int[]{(int)TextColors.Red,(int)TextColors.Green,(int)TextColors.Yellow,(int)TextColors.Cyan};
			int colorCounter=0;
			if(mPage==0xFFFF)
			{
				m_topNavigationPages[0]=256;
				m_topNavigationPages[1]=512;
				m_topNavigationPages[2]=768;
				m_topNavigationPages[3]=1024;
			}
			else if(hasTopText)
			{
				int redButton= TopNavigation(mPage, true, false); /* arguments: startpage, up, findgroup */
				int greenButton= TopNavigation(redButton, false, false);
				int yellowButton = TopNavigation(mPage, true, true);
				int blueButton = TopNavigation(yellowButton, false, true);

				m_topNavigationPages[0]=redButton;
				m_topNavigationPages[1]=greenButton;
				m_topNavigationPages[2]=yellowButton;
				m_topNavigationPages[3]=blueButton;
			}
			else if(m_fastTextDecode==true)
			{
				for(int button=0;button<4;button++)
				{
					if(m_flofTable[mPage,button]!=0)
						m_topNavigationPages[button]=m_flofTable[mPage,button];
					else
						m_topNavigationPages[button]=mPage;
				}
			}
			//
			// build control line
			if(mPage!=0xFFFF) // no top or fast text for not found page
			{
				if(hasTopText && m_fastTextDecode==false)
				{
					for(int lastLine=0;lastLine<40;lastLine+=10)
					{
						for(int i=0;i<10;i++)
						{
							pageAttribs[960+lastLine+i]=topColors[colorCounter]<<4 | (int)TextColors.Black;
						}
						if(m_aitTable[m_topNavigationPages[colorCounter]]!=null)
							System.Text.Encoding.ASCII.GetBytes(m_aitTable[m_topNavigationPages[colorCounter]],0,10,pageChars,960+lastLine);

						colorCounter++;
					}
				}
				else
					if(m_fastTextDecode==true && hasTopText==false)
				{
					int charCounter=0;
					for(int button=0;button<4;button++)
					{
						string text=m_flofAIT[m_flofTable[mPage,button]];
						if(text==null)
						{
							text="";
						}
						text+=" ";
						if(charCounter>=40)
							break;
						for(int l=0;l<text.Length;l++)
						{
							System.Text.Encoding.ASCII.GetBytes(text,l,1,pageChars,960+charCounter+l);
							pageAttribs[960+charCounter+l]=(int)TextColors.Black<<4 | topColors[button];
							
						}
						charCounter+=text.Length;
					}

				}
			}
			if (m_renderGraphics!=null && m_pageBitmap!=null)
			{
				for (row = 0; row < 25; row++)
				{
					x = 0;

					for (col = 0; col < 40; col++)
						Render(m_renderGraphics,pageChars[row*40 + col], pageAttribs[row*40 + col],ref x,ref y,width,height,isSubtitlePage);

					y+=height+(row==23?2:0);
					
				}
			}
			m_teletextFont.Dispose();
			return true;
			// send the bitmap to the callback
		}

		void Render(System.Drawing.Graphics graph,byte chr,int attrib,ref int x,ref int y,int w,int h, bool isSubtitlePage)
		{
			bool charReady=false;
			char chr2='?';
			
			if (chr == 0xFF)
			{
				x += w;
				return;
			}
			int[] mosaicY=new int[4];
			mosaicY[0] = 0;
			mosaicY[1] = (h + 1) / 3;
			mosaicY[2] = (h * 2 + 1) / 3;
			mosaicY[3] = h;

			/* get colors */
			int fColor = attrib & 0x0F;
			int bColor = (attrib>>4) & 0x0F;
			System.Drawing.Color bgColor=GetColor(bColor);
			if (bgColor==System.Drawing.Color.Black && isSubtitlePage)
				bgColor=System.Drawing.Color.HotPink;
			System.Drawing.Brush backBrush=new System.Drawing.SolidBrush(bgColor);
			System.Drawing.Brush foreBrush=new System.Drawing.SolidBrush(GetColor(fColor));
			System.Drawing.Pen	backPen=new System.Drawing.Pen(backBrush,1);
			System.Drawing.Pen	forePen=new System.Drawing.Pen(foreBrush,1);
			
			if (((attrib & 0x300)>0) && ((chr&0xA0) == 0x20))
			{
				int w1 = w / 2;
				int w2 = w - w1;
				int y1;
		
				chr = (byte)((chr & 0x1f) | ((chr & 0x40) >> 1));
				if ((attrib & 0x200)>0) 
					for (y1 = 0; y1 < 3; y1++)
					{
						graph.FillRectangle(backBrush,x,y+mosaicY[y1],w1,mosaicY[y1+1] - mosaicY[y1]);
						if((chr& 1)>0)
							graph.FillRectangle(foreBrush,x+1,y+mosaicY[y1]+1,w1-2,mosaicY[y1+1] - mosaicY[y1]-2);
						graph.FillRectangle(backBrush,x+w1,y+mosaicY[y1],w2,mosaicY[y1+1] - mosaicY[y1]);
						if((chr& 2)>0)
							graph.FillRectangle(foreBrush,x+w1+1,y+mosaicY[y1]+1,w2-2,mosaicY[y1+1] - mosaicY[y1]-2);
						chr >>= 2;
					}
				else
					for (y1 = 0; y1 < 3; y1++)
					{
						if((chr&1)>0)
							graph.FillRectangle(foreBrush,x,y+mosaicY[y1],w1,mosaicY[y1+1] - mosaicY[y1]);
						else
							graph.FillRectangle(backBrush,x,y+mosaicY[y1],w1,mosaicY[y1+1] - mosaicY[y1]);
						if((chr&2)>0)
							graph.FillRectangle(foreBrush,x+w1,y+mosaicY[y1],w2,mosaicY[y1+1] - mosaicY[y1]);
						else
							graph.FillRectangle(backBrush,x+w1,y+mosaicY[y1],w2,mosaicY[y1+1] - mosaicY[y1]);

						chr >>= 2;
					}
		
				x += w;
				return;
			}
			int factor=0;

			if ((attrib & 1<<10)>0)
				factor = 2;
			else
				factor = 1;

			charReady=false;

			switch (chr)
			{
				case 0x00:
				case 0x20:
					graph.FillRectangle(backBrush,x,y, w, h);
					if(factor==2)
						graph.FillRectangle(backBrush,x,y+h, w, h);
					x+=w;
					charReady=true;
					break;
				case 0x23:
				case 0x24:
					chr2 = m_charTableA[m_txtLanguage,chr-0x23];
					break;
				case 0x40:
					chr2 = m_charTableB[m_txtLanguage];
					break;
				case 0x5B:
				case 0x5C:
				case 0x5D:
				case 0x5E:
				case 0x5F:
				case 0x60:
					chr2 = m_charTableC[m_txtLanguage,chr-0x5B];
					break;
				case 0x7B:
				case 0x7C:
				case 0x7D:
				case 0x7E:
					chr2 = m_charTableD[m_txtLanguage,chr-0x7B];
					break;
				case 0x7F:
					graph.FillRectangle(backBrush,x, y, w, factor*h);
					graph.FillRectangle(foreBrush,x+(w/12), y+factor*(h*5/20), w*10/12, factor*(h*11/20));
					x+= w;
					charReady=true;
					break;
				case 0xE0: 
					graph.FillRectangle(backBrush,x+1, y+1, w-1, h-1);
					graph.DrawLine(forePen,x,y,x+w,y);
					graph.DrawLine(forePen,x,y,x,y+h);
					x += w;
					charReady=true;
					break;
				case 0xE1:
					graph.FillRectangle(backBrush,x, y+1, w, h-1);
					graph.DrawLine(forePen,x,y,x+w,y);
					x+= w;
					charReady=true;
					break;
				case 0xE2:
					graph.FillRectangle(backBrush,x, y+1, w-1, h-1);
					graph.DrawLine(forePen,x,y,x+w,y);
					graph.DrawLine(forePen,x+w-1,y+1,x+w-1,y+h-1);
					x+= w;
					charReady=true;
					break;
				case 0xE3: 
					graph.FillRectangle(backBrush,x+1, y, w-1, h);
					graph.DrawLine(forePen,x,y,x,y+h);
					x+= w;
					charReady=true;
					break;
				case 0xE4: 
					graph.FillRectangle(backBrush,x, y, w-1, h);
					graph.DrawLine(forePen,x+w-1,y,x+w-1,y+h);
					x+= w;
					charReady=true;
					break;
				case 0xE5: 
					graph.FillRectangle(backBrush,x+1, y, w-1, h-1);
					graph.DrawLine(forePen,x,y+h-1,x+w,y+h-1);
					graph.DrawLine(forePen,x,y,x,y+h-1);
					x+= w;
					charReady=true;
					break;
				case 0xE6: 
					graph.FillRectangle(backBrush,x, y, w, h-1);
					graph.DrawLine(forePen,x,y+h-1,x+w,y+h-1);
					x+= w;
					charReady=true;
					break;
				case 0xE7: 
					graph.FillRectangle(backBrush,x, y, w-1, h-1);
					graph.DrawLine(forePen,x,y+h-1,x+w,y+h-1);
					graph.DrawLine(forePen,x+w-1,y,x+w-1,y+h-1);
					x+= w;
					charReady=true;
					break;
				case 0xE8: 
					graph.FillRectangle(backBrush,x+1, y, w-1, h);
					for (int r = 0; r < w/2;r++)
						graph.DrawLine(forePen,x+r,y+r,x+r,y+h-r);
					x+= w;
					charReady=true;
					break;
				case 0xE9:
					graph.FillRectangle(backBrush,x+w/2, y, (w+1)/2, h);
					graph.FillRectangle(foreBrush,x, y, w/2, h);
					x+= w;
					charReady=true;
					break;
				case 0xEA:
					graph.FillRectangle(backBrush,x, y, w, h);
					graph.FillRectangle(foreBrush,x, y, w/2, h/2);
					x+= w;
					charReady=true;
					break;
				case 0xEB:
					graph.FillRectangle(backBrush,x, y+1, w, h-1);
					for (int r=0; r < w/2; r++)
						graph.DrawLine(forePen,x+r,y+r,x+w-r,y+r);
					x+= w;
					charReady=true;
					break;
				case 0xEC: 
					graph.FillRectangle(backBrush,x, y+(w/2), w, h-(w/2));
					graph.FillRectangle(foreBrush,x, y, w, h/2);
					x+= w;
					charReady=true;
					break;
				case 0xED:
				case 0xEE:
				case 0xEF:
				case 0xF0:
				case 0xF1:
				case 0xF2:
				case 0xF3:
				case 0xF4:
				case 0xF5:
				case 0xF6:
					chr2 = m_charTableE[chr - 0xED];
					break;
				default:
					chr2 = (char)chr;
					break;
			}
			if(charReady==false)
			{
				string text=""+chr2;
				graph.FillRectangle(backBrush,x, y, w, h);
				System.Drawing.SizeF width=graph.MeasureString(text,m_teletextFont);
				System.Drawing.PointF xyPos=new System.Drawing.PointF((float)x+((w-((int)width.Width))/2),(float)y);
				graph.DrawString(text,m_teletextFont,foreBrush,xyPos);
				if(factor==2)
				{
					graph.FillRectangle(backBrush,x, y+h, w, h);
					System.Drawing.Color[,] pixelColor=new System.Drawing.Color[w+1,h+1];
					// save char
					for(int ypos=0;ypos<h;ypos++)
					{
						for(int xpos=0;xpos<w;xpos++)
						{
							pixelColor[xpos,ypos]=m_pageBitmap.GetPixel(xpos+x,ypos+y); // backup old line
						}
					}
					// draw doubleheight
					for(int ypos=0;ypos<h;ypos++)
					{
						
						for(int xpos=0;xpos<w;xpos++)
						{
						
							try
							{
								if(y+(ypos*2)+1<m_pageBitmap.Height)
								{
									m_pageBitmap.SetPixel(x+xpos,y+(ypos*2),pixelColor[xpos,ypos]); // backup old line
									m_pageBitmap.SetPixel(x+xpos,y+(ypos*2)+1,pixelColor[xpos,ypos]);
								}
								}
							catch{}
						}
					}

				}
				x+=w;
			}
			foreBrush.Dispose();
			backBrush.Dispose();
			forePen.Dispose();
			backPen.Dispose();
			return;

		}
		System.Drawing.Color GetColor(int colorNumber)
		{

			switch(colorNumber)
			{
				case (int)TextColors.Black:
					return System.Drawing.Color.Black;
				case (int)TextColors.Red:
					return System.Drawing.Color.Red;
				case (int)TextColors.Green:
					return System.Drawing.Color.FromArgb(0,255,0);
				case (int)TextColors.Yellow:
					return System.Drawing.Color.Yellow;
				case (int)TextColors.Blue:
					return System.Drawing.Color.Blue;
				case (int)TextColors.Magenta:
					return System.Drawing.Color.Magenta;
				case (int)TextColors.White:
					return System.Drawing.Color.White;
				case (int)TextColors.Cyan:
					return System.Drawing.Color.Cyan;
				case (int)TextColors.Trans1:
					return System.Drawing.Color.HotPink;
				case (int)TextColors.Trans2:
					return System.Drawing.Color.HotPink;
			}
			return System.Drawing.Color.Black;
		}
	}// class
}// namespace
