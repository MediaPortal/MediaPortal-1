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
		IntPtr[,]				m_cacheTable=new IntPtr[0x900,0x80];
		int[]					m_currentSubPage=new int[10];
		byte[]					m_subPageTable=new byte[0x900];
		int[]					m_currentPage=new int[10];
		int						m_actualPage=0;
		int						m_actualSubPage=0;
		int						m_pageCount=0;
		//Hashtable				pages=new Hashtable();
		System.Drawing.Bitmap	m_pageBitmap=new System.Drawing.Bitmap(1920,1080);
		System.Drawing.Graphics m_renderGraphics;
		int						m_txtLanguage=4;// german
		public delegate void PageUpdated();
		public event PageUpdated PageUpdatedEvent;
		int						m_pageWidth=0;
		int						m_pageHeight=0;
		bool					m_hiddenMode=true;
		bool					m_transparentMode=false;
		string					m_pageSelectText="";
		System.Drawing.Font		m_teletextFont=null;
		//
		//
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
		int[,] m_charTableA =new int[,]{{ '#', 367 },{ '£', '$' }, 
	{ '#', 'õ' },{ 'é', 'ï' }, { '#', '$' }, { '£', '$' },{ '#', '$' },
	{ '#', 329 },{ 'ç', '$' }, { '#', '¤' },{ '#', 'Ë' }, { '#', '¤' },{ '£', 287 }
	};
		int[] m_charTableB =new int[]{269,'@',352,'à', '§','é',352,261,'¡',354,268,'É',304};
		int[,] m_charTableC =new int[,]{{ 357, 382, 'ý', 'í', 345, 'é' },{8592, '½',8594,8593, '#', 822 },
	{ 'Ä', 'Ö', 381, 'Ü', 'Õ', 353 },{ 'ë', 'ê', 'ù', 'î', '#', 'è' },
	{ 'Ä', 'Ö', 'Ü', '^', '_', '°' },{ '°', 'ç',8594,8593, '#', 'ù' },
	{ 'é', 553, 381, 269, 363, 353 },{ 437, 346, 321, 263, 'ó', 281 },
	{ 'á', 'é', 'í', 'ó', 'ú', '¿' },{ 'Â', 350, 461, 'Î', 305, 355 },
	{ 262, 381, 272, 352, 'ë', 269 },{ 'Ä', 'Ö', 'Å', 'Ü', '_', 'é' },
	{ 350, 'Ö', 'Ç', 'Ü', 486, 305 }};
		
		int[,] m_charTableD =new int[,]{{ 'á', 283, 'ú', 353 },{ '¼',8214, '¾', '÷' },
	{ 'ä', 'ö', 382, 'ü' },{ 'â', 'ô', 'û', 'ç' },{ 'ä', 'ö', 'ü', 'ß' },
	{ 'à', 'ò', 'è', 'ì' },{ 261, 371, 382, 303 },{ 380, 347, 322, 378 },
	{ 'ü', 'ñ', 'è', 'à' },{ 'â', 351, 462, 'î' },{ 263, 382, 273, 353 },
	{ 'ä', 'ö', 'å', 'ü' },{ 351, 'ö', 231, 'ü' }};
		int[] m_charTableE =new int[] {8592, 8594, 8593, 8595, 'O', 'K', 8592, 8592};
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
			m_pageBitmap=new System.Drawing.Bitmap(m_pageWidth,m_pageHeight);
			m_renderGraphics=System.Drawing.Graphics.FromImage(m_pageBitmap);
			//
			// TODO: Fügen Sie hier die Konstruktorlogik hinzu
			//
		}
		~DVBTeletext()
		{
			// free graphics and bitmap object
			//
			if(m_renderGraphics!=null)
				m_renderGraphics.Dispose();
			if(m_pageBitmap!=null)
				m_pageBitmap.Dispose();

			// free alloctated memory
			for(int t=0;t<0x900;t++)
				for(int n=0;n<0x80;n++)
					if((int)m_cacheTable[t,n]!=0)
						Marshal.FreeHGlobal(m_cacheTable[t,n]);
		}
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
			
			string sPage="0x"+page.ToString();
			string sSubPage="0x"+subpage.ToString();

			if(sPage==null || sSubPage==null)
				return null;

			m_actualPage=Convert.ToInt16(sPage,16);
			m_actualSubPage=Convert.ToInt16(sSubPage,16);
			
			if (m_actualPage<0x100)
				return null;

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
				return null; // nothing found
			}
			return m_pageBitmap;
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
				m_cacheTable[page,subpage]=IntPtr.Zero;
				m_cacheTable[page,subpage]=Marshal.AllocHGlobal(size);
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
			if(m_renderGraphics!=null)
				m_renderGraphics.Dispose();
			m_pageBitmap=new System.Drawing.Bitmap(m_pageWidth,m_pageHeight);
			m_renderGraphics=System.Drawing.Graphics.FromImage(m_pageBitmap);

			
		}

		public void SaveData(IntPtr dataPtr)
		{
			if (dataPtr==IntPtr.Zero) return;
			byte[] txtRow=new byte[42];
			int line=0;
			int b=0,b1=0, b2=0, b3=0, b4=0;
			int actualTransmittingPage=0;
			byte[] tmpBuffer=new byte[184];
			int packetNumber;
			byte magazine;
			int pointer=0;
			int dataAdd=(int)dataPtr;
			try
			{
				for (line = 0; line < 4; line++)
				{

					Marshal.Copy((IntPtr)((dataAdd+4)+(line*0x2e)),tmpBuffer,0,184);

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

						b1 = m_deHamTable[tmpBuffer[0]];
						b2 = m_deHamTable[tmpBuffer[1]];

						if (b1 == 0xFF || b2 == 0xFF)
						{
							continue;
						}

						b1 &= 8;
						packetNumber = b1>>3 | b2<<1;
						//  mag number
						magazine =(byte)(m_deHamTable[tmpBuffer[0]] & 7);
						if (packetNumber == 0)
						{
							b1 = m_deHamTable[tmpBuffer[0]];
							b2 = m_deHamTable[tmpBuffer[3]];
							b3 = m_deHamTable[tmpBuffer[2]];

							if (b1 == 0xFF || b2 == 0xFF || b3 == 0xFF)
							{
								m_currentPage[magazine] = -1;
								actualTransmittingPage = -1;
								continue;
							}

							b1 &= 7;
							if (b1==0)
								b1 = 8;
							actualTransmittingPage = b1<<8 | b2<<4 | b3;
							m_currentPage[magazine] = actualTransmittingPage;

							if (b2 > 9 || b3 > 9) 
							{
								m_currentSubPage[magazine] = 0;
								m_subPageTable[m_currentPage[magazine]] = 0;
								continue;
							}

							b1 = m_deHamTable[tmpBuffer[7]];
							b2 = m_deHamTable[tmpBuffer[6]];
							b3 = m_deHamTable[tmpBuffer[5]];
							b4 = m_deHamTable[tmpBuffer[4]];

							if (b1 == 0xFF || b2 == 0xFF || b3 == 0xFF || b4 == 0xFF)
							{
								m_currentSubPage[magazine] = -1;
								continue;
							}

							b1 &= 3;
							b3 &= 7;

							if (b1 != 0 || b2 != 0 || b4 > 9)
							{
								m_currentSubPage[magazine] = -1;
								continue;
							}
							else
								m_currentSubPage[magazine] = b3<<4 | b4;

							b1 = m_deHamTable[tmpBuffer[9]];


							if(SetMemory(960,m_currentPage[magazine],m_currentSubPage[magazine])==false)
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
						}
						else if (packetNumber < 24)
						{
							if ((m_currentPage[magazine] & 0x0F0) <= 0x090 && (m_currentPage[magazine] & 0x00F) <= 0x009)
							{  
								for (b = 2; b < 42; b++)
								{
									if ((tmpBuffer[b]&1)>0 ^ ((tmpBuffer[b]>>1)&1)>0 ^
										((tmpBuffer[b]>>2)&1)>0 ^ ((tmpBuffer[b]>>3)&1)>0 ^
										((tmpBuffer[b]>>4)&1)>0 ^ ((tmpBuffer[b]>>5)&1)>0 ^
										((tmpBuffer[b]>>6)&1)>0 ^ (tmpBuffer[b]>>7)>0)
										tmpBuffer[b] &= 127;
									else
										tmpBuffer[b] = 32;
								}
							}
						}

						if (m_currentPage[magazine] != -1 && m_currentSubPage[magazine] != -1 &&
							packetNumber < 24 && ((int)m_cacheTable[m_currentPage[magazine],m_currentSubPage[magazine]]!=0)) /* avoid segfault */
						{
							IntPtr adr=m_cacheTable[m_currentPage[magazine],m_currentSubPage[magazine]];
							int offset=packetNumber*40;
							adr=(IntPtr)(((int)adr)+offset);
							Marshal.Copy(tmpBuffer,2,adr,40);
							if (m_currentPage[magazine]==m_actualPage &&
								m_currentSubPage[magazine]==m_actualSubPage)
							{
								//DecodePage(m_actualPage,0);
								PageUpdatedEvent();
							}

							
						}


					}//if ((tmpBuffer

				}// for(line=0
			}
			catch(Exception )
			{ 
				//int a=0;
			}
		}
		bool IsDEC(int i)
		{
			return (bool)(((i & 0x00F) <= 9) && ((i & 0x0F0) <= 0x90));
		}

		//
		// decode and render the page
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

			if((int)m_cacheTable[mPage,sPage]==0)
				return false;
			
			Marshal.Copy(m_cacheTable[mPage,sPage],pageChars,0,960);

			if ((m_deHamTable[pageChars[5]] & 12)>0)
				boxed = 1;
			else
				boxed = 0;


			for (row = 0; row < 24; row++)
			{
				foreground   = (int)TextColors.White;
				if(m_transparentMode==false)
					background  = (int)TextColors.Black;
				else
					background  = (int)TextColors.Trans1;

				doubleheight = 0;
				charset      = 0;
				mosaictype   = 0;
				hold         = 0;
				held_mosaic  = 32;
				
				for(int loop1=0;loop1<40;loop1++)
					if(pageChars[(row*40)+loop1]==12)
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
			
			if (IsDEC(mPage))
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

			// render
			int y = 0;
			int x;
			int width=m_pageWidth/40;
			int height=m_pageHeight/24;
			int fntSize=(width-1<10)?10:width-1;
			m_teletextFont=new System.Drawing.Font("Courier New",fntSize,System.Drawing.FontStyle.Bold);
			m_renderGraphics.FillRectangle(new System.Drawing.SolidBrush(System.Drawing.Color.Black),0,0,m_pageWidth,m_pageHeight);
			for (row = 0; row < 24; row++)
			{
				x = 0;

				for (col = 0; col < 40; col++)
					Render(m_renderGraphics,pageChars[row*40 + col], pageAttribs[row*40 + col],ref x,ref y,width,height);

				y+=height;
			}
			m_teletextFont.Dispose();
			return true;
			// send the bitmap to the callback
		}

		void Render(System.Drawing.Graphics graph,byte chr,int attrib,ref int x,ref int y,int w,int h)
		{
			bool charReady=false;
			
			if (chr == 0xFF)
			{
				x += w;
				return;
			}
			int[] ymosaic=new int[4];
			ymosaic[0] = 0;
			ymosaic[1] = (h + 1) / 3;
			ymosaic[2] = (h * 2 + 1) / 3;
			ymosaic[3] = h;

			/* get colors */
			int fColor = attrib & 0x0F;
			int bColor = (attrib>>4) & 0x0F;
			System.Drawing.Brush backBrush=new System.Drawing.SolidBrush(GetColor(bColor));
			System.Drawing.Brush foreBrush=new System.Drawing.SolidBrush(GetColor(fColor));
			System.Drawing.Pen	backPen=new System.Drawing.Pen(backBrush,1);
			System.Drawing.Pen	forePen=new System.Drawing.Pen(foreBrush,1);
			
			if (((attrib & 0x300)>0) && ((chr&0xA0) == 0x20))
			{
				int w1 = w / 2;
				int w2 = w - w1;
				int y1;
		
				chr = (byte)((chr & 0x1f) | ((chr & 0x40) >> 1));
				if ((attrib & 0x200)>0) /* separated mosaic */
					for (y1 = 0; y1 < 3; y1++)
					{
						graph.FillRectangle(backBrush,x,y+ymosaic[y1],w1,ymosaic[y1+1] - ymosaic[y1]);
						if((chr& 1)>0)
							graph.FillRectangle(backBrush,x+1,y+ymosaic[y1]+1,w1-2,ymosaic[y1+1] - ymosaic[y1]-2);
						graph.FillRectangle(backBrush,x+w1,y+ymosaic[y1],w2,ymosaic[y1+1] - ymosaic[y1]);
						if((chr& 2)>0)
							graph.FillRectangle(backBrush,x+w1+1,y+ymosaic[y1]+1,w2-2,ymosaic[y1+1] - ymosaic[y1]-2);
						chr >>= 2;
					}
				else
					for (y1 = 0; y1 < 3; y1++)
					{
						if((chr&1)>0)
							graph.FillRectangle(foreBrush,x,y+ymosaic[y1],w1,ymosaic[y1+1] - ymosaic[y1]);
						else
							graph.FillRectangle(backBrush,x,y+ymosaic[y1],w1,ymosaic[y1+1] - ymosaic[y1]);
						if((chr&2)>0)
							graph.FillRectangle(foreBrush,x+w1,y+ymosaic[y1],w2,ymosaic[y1+1] - ymosaic[y1]);
						else
							graph.FillRectangle(backBrush,x+w1,y+ymosaic[y1],w2,ymosaic[y1+1] - ymosaic[y1]);

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
					chr = (byte)m_charTableA[m_txtLanguage,chr-0x23];
					break;
				case 0x40:
					chr = (byte)m_charTableB[m_txtLanguage];
					break;
				case 0x5B:
				case 0x5C:
				case 0x5D:
				case 0x5E:
				case 0x5F:
				case 0x60:
					chr = (byte)m_charTableC[m_txtLanguage,chr-0x5B];
					break;
				case 0x7B:
				case 0x7C:
				case 0x7D:
				case 0x7E:
					chr = (byte)m_charTableD[m_txtLanguage,chr-0x7B];
					break;
				case 0x7F:
					graph.FillRectangle(backBrush,x, y+factor*4, w, factor*(h-4));
					graph.FillRectangle(foreBrush,x, y, w, factor*4);
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
					chr = (byte)m_charTableE[chr - 0xED];
					break;
				default:
					break;
			}
			if(charReady==false)
			{
				string text=""+((char)chr);
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
					return System.Drawing.Color.Transparent;
				case (int)TextColors.Trans2:
					return System.Drawing.Color.Transparent;
			}
			return System.Drawing.Color.Black;
		}
	}// class
}// namespace
