using System;
using System.Xml.Serialization;
using System.Drawing;

using MediaPortal.GUI.Library;
using MediaPortal.Util;

using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Direct3D=Microsoft.DirectX.Direct3D;

using Tetris;

namespace MediaPortal.Games.Tetris
{
	/// <summary>
	/// Summary description for MyTetris
	/// </summary>
	public class MyTetris : GUIWindow, ISetupForm
	{
		#region Construction, initialization & cleanup

		public MyTetris()
		{
			GetID = 19000;
		}

		~MyTetris()
		{
		}

		#endregion Construction, initialization & cleanup

		#region Serialization

		[Serializable]
		public class Settings
		{
			protected bool			m_bMusic;
			protected bool			m_bSound;
			protected int			m_nHighscore;

			public Settings()
			{
				m_bMusic = true;
				m_bSound = true;
				m_nHighscore = 0;
			}

			[XmlElement("Music")]
			public bool Music
			{
				get { return m_bMusic; }
				set { m_bMusic = value ;}
			}
      
			[XmlElement("Sound")]
			public bool Sound
			{
				get { return m_bSound; }
				set { m_bSound = value;}
			}

			[XmlElement("Highscore")]
			public int Highscore
			{
				get { return m_nHighscore; }
				set { m_nHighscore = value;}
			}

			public void Load()
			{
				using(AMS.Profile.Xml xmlreader = new AMS.Profile.Xml("MediaPortal.xml"))
				{
					m_bMusic = xmlreader.GetValueAsBool("tetris", "music", true);
					m_bSound = xmlreader.GetValueAsBool("tetris", "sound", true);

					m_nHighscore = xmlreader.GetValueAsInt("tetris", "highscore", -1);

					if(m_nHighscore == -1)
					{
						m_nHighscore = xmlreader.GetValueAsInt("tetris", "hiscore", 0);
					}
				}
			}

			public void Save()
			{
				using(AMS.Profile.Xml xmlwriter = new AMS.Profile.Xml("MediaPortal.xml"))
				{
					xmlwriter.SetValueAsBool("tetris", "music", m_bMusic);
					xmlwriter.SetValueAsBool("tetris", "sound", m_bSound);
					xmlwriter.SetValue("tetris", "highscore", m_nHighscore);
				}
			}
		}

		#endregion Serialization

		#region Overrides

		public override bool Init()
		{
			GUIControlFactory.RegisterControl("tetris", typeof(MyTetrisControl));
			_Settings.Load();
			return Load(GUIGraphicsContext.Skin + @"\mytetris.xml");
		}

		public override void OnAction(Action action)
		{
			if(action.wID == Action.ActionType.ACTION_PREVIOUS_MENU)
			{
				MyTetrisControl tetris = (MyTetrisControl)GetControl((int)Controls.CONTROL_TETRIS);

				if(tetris != null && tetris.Focus && tetris.State == State.Running)
				{
					tetris.State = State.Paused;
				}
				else
				{
					GUIWindowManager.PreviousWindow();
				}
			}
			else
			{
				base.OnAction(action);
			}
		}
		
		public override bool OnMessage(GUIMessage message)
		{
			switch(message.Message)
			{
				case GUIMessage.MessageType.GUI_MSG_WINDOW_INIT:
					base.OnMessage(message);
					GUIGraphicsContext.Overlay=false;

					_Settings.Load();

					if(_Settings.Music)
					{
						GUIControl.SelectControl(GetID, (int)Controls.CONTROL_BTNMUSIC);
					}
					else
					{
						GUIControl.DeSelectControl(GetID, (int)Controls.CONTROL_BTNMUSIC);
					}

					if(_Settings.Sound)
					{
						GUIControl.SelectControl(GetID, (int)Controls.CONTROL_BTNSOUND);
					}
					else
					{
						GUIControl.DeSelectControl(GetID, (int)Controls.CONTROL_BTNSOUND);
					}

					MyTetrisControl tetris = (MyTetrisControl)GetControl((int)Controls.CONTROL_TETRIS);

					int nScore = 0;
					int nLines = 0;
					int nLevel = 0;

					if(tetris != null)
					{
						nScore = tetris.Score;
						nLines = tetris.Lines;
						nLevel = tetris.Level;

						tetris.Sound = _Settings.Sound;
						tetris.Music = _Settings.Music;
					}

					GUIPropertyManager.SetProperty("#tetris_score", nScore.ToString());
					GUIPropertyManager.SetProperty("#tetris_lines", nLines.ToString());
					GUIPropertyManager.SetProperty("#tetris_level", nLevel.ToString());
					GUIPropertyManager.SetProperty("#tetris_highscore", (_Settings.Highscore == 0) ? "-" : _Settings.Highscore.ToString());

					return true;
				case GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT:
					break;
				case GUIMessage.MessageType.GUI_MSG_CLICKED:
					int iControl = message.SenderControlId;
					
					if(iControl == (int)Controls.CONTROL_BTNNEWGAME)
					{
						tetris = (MyTetrisControl)GetControl((int)Controls.CONTROL_TETRIS);

						if(tetris != null)
						{
							tetris.Start();

							GUIControl.FocusControl(GetID, (int)Controls.CONTROL_TETRIS);
						}
					}
					else if(iControl == (int)Controls.CONTROL_BTNMUSIC)
					{
						_Settings.Music = (message.Param1 == 1);
	 
						tetris = (MyTetrisControl)GetControl((int)Controls.CONTROL_TETRIS);

						if(tetris != null)
						{
							tetris.Music = _Settings.Music;
						}

						_Settings.Save();
					}
					else if(iControl == (int)Controls.CONTROL_BTNSOUND)
					{
						_Settings.Sound = (message.Param1 == 1);
	 
						tetris = (MyTetrisControl)GetControl((int)Controls.CONTROL_TETRIS);

						if(tetris != null)
						{
							tetris.Sound = _Settings.Sound;
						}

						_Settings.Save();
					}

					break;
			}
			
			return base.OnMessage(message);
		}

		public override void Render()
		{
			MyTetrisControl tetris = (MyTetrisControl)GetControl((int)Controls.CONTROL_TETRIS);

			if(tetris != null)
			{
				GUIPropertyManager.SetProperty("#tetris_score", tetris.Score.ToString());
				GUIPropertyManager.SetProperty("#tetris_lines", tetris.Lines.ToString());
				GUIPropertyManager.SetProperty("#tetris_level", tetris.Level.ToString());

				if(tetris.Score > _Settings.Highscore)
				{
					_Settings.Highscore = tetris.Score;
					_Settings.Save();

					GUIPropertyManager.SetProperty("#tetris_highscore", _Settings.Highscore.ToString());
				}
			}

			base.Render();
		}

		#endregion

		#region ISetupForm Members

		public bool CanEnable()
		{
			return true;
		}

		public bool HasSetup()
		{
			return false;
		}

		public string PluginName()
		{
			return "My Tetris";
		}

		public bool DefaultEnabled()
		{
			return true;
		}

		public int GetWindowId()
		{
			return GetID;
		}

		public bool GetHome(out string strButtonText, out string strButtonImage, out string strButtonImageFocus, out string strPictureImage)
		{
			// TODO:  Add GUITetris.GetHome implementation
			strButtonText = GUILocalizeStrings.Get(19001);
			strButtonImage = "";
			strButtonImageFocus = "";
			strPictureImage = @"\tetris\hover.png";
			return true;
		}

		public string Author()
		{
			return "Smirnoff";
		}

		public string Description()
		{
			return "Plugin to ...";
		}

		public void ShowPlugin()
		{
		}

		#endregion

		#region Helper enums

		enum Controls
		{
			CONTROL_BTNNEWGAME		= 2,
			CONTROL_BTNMUSIC		= 3,
			CONTROL_BTNSOUND		= 4,
			CONTROL_TETRIS			= 10,
			CONTROL_SCORE			= 202,
			CONTROL_LINES			= 204,
			CONTROL_LEVEL			= 206,
		};

		#endregion Helper enums

		#region Member variables

		Settings					_Settings = new Settings();

		#endregion Member variables
	}

	public class MyTetrisControl : GUIControl, IHostTetris
	{
		#region Construction
		
		public MyTetrisControl(int nParentID) : base(nParentID)
		{
		}

		#endregion Construction

		#region Properties

		public int Score
		{ get { return (m_theGame != null) ? m_theGame.Score : 0; } }

		public int Lines
		{ get { return (m_theGame != null) ? m_theGame.Lines : 0; } }

		public int Level
		{ get { return (m_theGame != null) ? m_theGame.Level : 0; } }

		public bool Music
		{ get { return m_bMusic; } set { m_bMusic = value; } }

		public bool Sound
		{  get { return m_bSounds; } set { m_bSounds = value; } }

		public State State
		{
			get { return (m_theGame != null) ? m_theGame.State : State.Stopped; }
			set { if(m_theGame != null) m_theGame.State = value; }
		}

		#endregion Properties

		#region IHostTetris implementation

		public void OnRenderBlock(float x, float y, Color color, int nHint)
		{
			if(nHint != 2)
			{
				int nX = m_nOffsetX + (int)(x * m_cxBlock);
				int nY = m_nOffsetY + (int)(y * m_cyBlock);

				int nImage = ColorToBlock(color);

				if(nHint == 3)
				{
					if(m_imgBlocksGlow != null && m_imgBlocksGlow[nImage] != null)
					{
						m_imgBlocksGlow[nImage].SetPosition(nX, nY);
						m_imgBlocksGlow[nImage].Render();
					}
				}
				else
				{
					if(m_imgBlocks != null && m_imgBlocks[nImage] != null)
					{
						m_imgBlocks[nImage].SetPosition(nX, nY);
						m_imgBlocks[nImage].Render();
					}
				}
			}
		}

		public void OnRenderSound(string strFilePath)
		{
			if(m_bSounds)
			{
				Utils.PlaySound(strFilePath, false, true);
			}
		}

		#endregion IHostTetris implementation

		#region Implementation

		void RenderTexture()
		{
			if(m_bHasFocus)
			{
				m_imgTextureFocused.SetPosition(m_dwPosX, m_dwPosY);
				m_imgTextureFocused.Render();
			}
			else
			{
				m_imgTexture.SetPosition(m_dwPosX, m_dwPosY);
				m_imgTexture.Render();
			}

			// render the guides also
			if(m_imgGuide != null)
			{
				if(m_imgGuide[0] != null)
				{
					m_imgGuide[0].SetPosition(((m_dwPosX + (this.Width / 2) - (m_cxBlock * 5))) - (m_imgGuide[0].Width + 2), (m_dwPosY + (21 * m_cyBlock)) - m_imgGuide[0].Height);
					m_imgGuide[0].Render();
				}

				if(m_imgGuide[1] != null)
				{
					m_imgGuide[1].SetPosition((m_dwPosX + (this.Width / 2) + (m_cxBlock * 5)), (m_dwPosY + (21 * m_cyBlock)) - m_imgGuide[1].Height);
					m_imgGuide[1].Render();
				}
			}
		}

		void RenderText()
		{
			if(m_Font == null)
			{
				return;
			}

			if(m_theGame != null)
			{
				// draw 'Paused' or 'Game Over' if needed
				if(m_theGame.State == State.Paused)
				{
					if(m_cxPaused == 0 || m_cyPaused == 0)
					{
						float fW = m_cxPaused;
						float fH = m_cyPaused;

						m_Font.GetTextExtent(m_strPaused, ref fW, ref fH);

						m_cxPaused = (int)fW;
						m_cyPaused = (int)fH;
					}

					int x = m_dwPosX + ((m_dwWidth - m_cxPaused) / 2);
					int y = m_dwPosY + ((m_dwHeight - m_cyPaused) / 2);

					m_Font.DrawText(x, y - m_cyPaused, m_dwTextColor, m_strPaused, Alignment.ALIGN_LEFT,-1);
				}
				else if(m_theGame.State == State.Stopped)
				{
					if(m_cxGameOver == 0 || m_cyGameOver == 0)
					{
						float fW = m_cxGameOver;
						float fH = m_cyGameOver;

						m_Font.GetTextExtent(m_strGameOver, ref fW, ref fH);

						m_cxGameOver = (int)fW;
						m_cyGameOver = (int)fH;
					}

					int x = m_dwPosX + ((m_dwWidth - m_cxGameOver) / 2);
					int y = m_dwPosY + ((m_dwHeight - m_cyGameOver) / 2);

					m_Font.DrawText(x, y - m_cyGameOver, m_dwTextColor, m_strGameOver, Alignment.ALIGN_LEFT,-1);
				}
			}
			else
			{
				if(m_cxPressToStart == 0 || m_cyPressToStart == 0)
				{
					float fW = m_cxPressToStart;
					float fH = m_cyPressToStart;

					m_Font.GetTextExtent(m_strStart, ref fW, ref fH);

					m_cxPressToStart = (int)fW;
					m_cyPressToStart = (int)fH;
				}

				int x = m_dwPosX + ((m_dwWidth - m_cxPressToStart) / 2);
				int y = m_dwPosY + ((m_dwHeight - m_cyPressToStart) / 2);

				m_Font.DrawText(x, y - m_cyPressToStart, m_dwTextColor, m_strStart, Alignment.ALIGN_LEFT,-1);
			}
		}

		int ColorToBlock(Color color)
		{
			if(color == Color.Red)
			{
				return 0;
			}

			if(color == Color.Blue)
			{
				return 1;
			}

			if(color == Color.Gray)
			{
				return 2;
			}

			if(color == Color.Yellow)
			{
				return 3;
			}

			if(color == Color.Cyan)
			{
				return 4;
			}

			if(color == Color.Orange)
			{
				return 5;
			}

			if(color == Color.Green)
			{
				return 6;
			}
			
			return 0;
		}

		#endregion Implementation

		#region Public methods

		public void Start()
		{
			if(m_theGame == null)
			{
				m_theGame = new Game(this);

				if(m_theGame != null)
				{
					m_theGame.Start();
				}
				else
				{
					Log.Write("MyTetris.Start: Failed in call to 'new Game()'");
				}
			}
			else
			{
				m_theGame.Start();
			}

			this.Focus = true;
		}

		#endregion Public methods

		#region Overrides

		public override void FinalizeConstruction()
		{
			base.FinalizeConstruction();

			m_Font = GUIFontManager.GetFont((m_strFont != "" && m_strFont != "-") ? m_strFont : "font18");

			if(m_strTexture != "" && m_strTexture != "-")
			{
				m_imgTexture = new GUIImage(m_dwParentID, 9998, m_dwPosX, m_dwPosY, this.Width, this.Height, m_strTexture, m_dwColorDiffuse);
			}

			if(m_strTextureFocused != "" && m_strTextureFocused != "-")
			{
				m_imgTextureFocused = new GUIImage(m_dwParentID, 9999, m_dwPosX, m_dwPosY, this.Width, this.Height, m_strTextureFocused, m_dwColorDiffuse);
			}

			m_imgGuide = new GUIImage[2];

			int cyBlock = this.Height / (Game.Height + 2);
			int cxBlock = cyBlock;

			if(m_strTextureLeft != "" && m_strTextureLeft != "-")
			{
				m_imgGuide[0] = new GUIImage(m_dwParentID, 9996, m_dwPosX, m_dwPosY, 0, 0, m_strTextureLeft, m_dwColorDiffuse);
			}

			if(m_strTextureRight != "" && m_strTextureRight != "-")
			{
				m_imgGuide[1] = new GUIImage(m_dwParentID, 9997, m_dwPosX, m_dwPosY, 0, 0, m_strTextureRight, m_dwColorDiffuse);
			}

			m_imgBlocks = new GUIImage[]
			{
				new GUIImage(m_dwParentID, 10001, m_dwPosX, m_dwPosY, cxBlock, cyBlock, GUIGraphicsContext.Skin + @"\media\tetris\block_red.png", m_dwColorDiffuse),
				new GUIImage(m_dwParentID, 10002, m_dwPosX, m_dwPosY, cxBlock, cyBlock, GUIGraphicsContext.Skin + @"\media\tetris\block_blue.png", m_dwColorDiffuse),
				new GUIImage(m_dwParentID, 10003, m_dwPosX, m_dwPosY, cxBlock, cyBlock, GUIGraphicsContext.Skin + @"\media\tetris\block_gray.png", m_dwColorDiffuse),
				new GUIImage(m_dwParentID, 10004, m_dwPosX, m_dwPosY, cxBlock, cyBlock, GUIGraphicsContext.Skin + @"\media\tetris\block_yellow.png", m_dwColorDiffuse),
				new GUIImage(m_dwParentID, 10005, m_dwPosX, m_dwPosY, cxBlock, cyBlock, GUIGraphicsContext.Skin + @"\media\tetris\block_cyan.png", m_dwColorDiffuse),
				new GUIImage(m_dwParentID, 10006, m_dwPosX, m_dwPosY, cxBlock, cyBlock, GUIGraphicsContext.Skin + @"\media\tetris\block_orange.png", m_dwColorDiffuse),
				new GUIImage(m_dwParentID, 10007, m_dwPosX, m_dwPosY, cxBlock, cyBlock, GUIGraphicsContext.Skin + @"\media\tetris\block_green.png", m_dwColorDiffuse),
			};

			m_imgBlocksGlow = new GUIImage[]
			{
				new GUIImage(m_dwParentID, 10011, m_dwPosX, m_dwPosY, cxBlock, cyBlock, GUIGraphicsContext.Skin + @"\media\tetris\block_red_glow.png", m_dwColorDiffuse),
				new GUIImage(m_dwParentID, 10012, m_dwPosX, m_dwPosY, cxBlock, cyBlock, GUIGraphicsContext.Skin + @"\media\tetris\block_blue_glow.png", m_dwColorDiffuse),
				new GUIImage(m_dwParentID, 10013, m_dwPosX, m_dwPosY, cxBlock, cyBlock, GUIGraphicsContext.Skin + @"\media\tetris\block_gray_glow.png", m_dwColorDiffuse),
				new GUIImage(m_dwParentID, 10014, m_dwPosX, m_dwPosY, cxBlock, cyBlock, GUIGraphicsContext.Skin + @"\media\tetris\block_yellow_glow.png", m_dwColorDiffuse),
				new GUIImage(m_dwParentID, 10015, m_dwPosX, m_dwPosY, cxBlock, cyBlock, GUIGraphicsContext.Skin + @"\media\tetris\block_cyan_glow.png", m_dwColorDiffuse),
				new GUIImage(m_dwParentID, 10016, m_dwPosX, m_dwPosY, cxBlock, cyBlock, GUIGraphicsContext.Skin + @"\media\tetris\block_orange_glow.png", m_dwColorDiffuse),
				new GUIImage(m_dwParentID, 10017, m_dwPosX, m_dwPosY, cxBlock, cyBlock, GUIGraphicsContext.Skin + @"\media\tetris\block_green_glow.png", m_dwColorDiffuse),
			};

			m_strStart = GUILocalizeStrings.Get(19010);
			m_strPaused = GUILocalizeStrings.Get(19011);
			m_strGameOver = GUILocalizeStrings.Get(19012);

			m_imgBlocksNext = new GUIImage[7];

			if(m_strTextureO != "" && m_strTextureO != "-")
			{
				m_imgBlocksNext[0] = new GUIImage(m_dwParentID, 10021, m_dwPosX, m_dwPosY, 0, 0, m_strTextureO, m_dwColorDiffuse);
			}

			if(m_strTextureI != "" && m_strTextureI != "-")
			{
				m_imgBlocksNext[1] = new GUIImage(m_dwParentID, 10022, m_dwPosX, m_dwPosY, 0, 0, m_strTextureI, m_dwColorDiffuse);
			}

			if(m_strTextureS != "" && m_strTextureS != "-")
			{
				m_imgBlocksNext[2] = new GUIImage(m_dwParentID, 10023, m_dwPosX, m_dwPosY, 0, 0, m_strTextureS, m_dwColorDiffuse);
			}
		
			if(m_strTextureZ != "" && m_strTextureZ != "-")
			{
				m_imgBlocksNext[3] = new GUIImage(m_dwParentID, 10024, m_dwPosX, m_dwPosY, 0, 0, m_strTextureZ, m_dwColorDiffuse);
			}
		
			if(m_strTextureL != "" && m_strTextureL != "-")
			{
				m_imgBlocksNext[4] = new GUIImage(m_dwParentID, 10025, m_dwPosX, m_dwPosY, 0, 0, m_strTextureL, m_dwColorDiffuse);
			}
		
			if(m_strTextureT != "" && m_strTextureT != "-")
			{
				m_imgBlocksNext[5] = new GUIImage(m_dwParentID, 10026, m_dwPosX, m_dwPosY, 0, 0, m_strTextureT, m_dwColorDiffuse);
			}
		
			if(m_strTextureJ != "" && m_strTextureJ != "-")
			{
				m_imgBlocksNext[6] = new GUIImage(m_dwParentID, 10027, m_dwPosX, m_dwPosY, 0, 0, m_strTextureJ, m_dwColorDiffuse);
			}
		}

		public override void AllocResources()
		{
			base.AllocResources();
			
			m_Font = GUIFontManager.GetFont((m_strFont != "" && m_strFont != "-") ? m_strFont : "font18");

			if(m_imgTexture != null)
			{
				m_imgTexture.AllocResources();
			}

			if(m_imgTextureFocused != null)
			{
				m_imgTextureFocused.AllocResources();
			}

			if(m_imgGuide != null)
			{
				if(m_imgGuide[0] != null)
				{
					m_imgGuide[0].AllocResources();
				}

				if(m_imgGuide[1] != null)
				{
					m_imgGuide[1].AllocResources();
				}
			}

			if(m_imgBlocks != null)
			{
				foreach(GUIImage image in m_imgBlocks)
				{
					image.AllocResources();
				}

				if(m_imgBlocks[0] != null)
				{
					// need to know the block dimensions for positioning and scaling
					m_cxBlock = m_imgBlocks[0].Width;
					m_cyBlock = m_imgBlocks[0].Height;
				}
			}

			if(m_imgBlocksGlow != null)
			{
				foreach(GUIImage image in m_imgBlocksGlow)
				{
					image.AllocResources();
				}
			}

			GUIGraphicsContext.ScalePosToScreenResolution(ref m_nNextBlockX, ref m_nNextBlockY);

			if(m_imgBlocksNext != null)
			{
				foreach(GUIImage image in m_imgBlocksNext)
				{
					image.AllocResources();

					if(m_nNextBlockAlign == Alignment.ALIGN_LEFT)
					{
						image.SetPosition(m_nNextBlockX, m_nNextBlockY - (image.Height / 2));
					}
					else if(m_nNextBlockAlign == Alignment.ALIGN_CENTER)
					{
						image.SetPosition(m_nNextBlockX - (image.Width / 2), m_nNextBlockY - (image.Height / 2));
					}
					else if(m_nNextBlockAlign == Alignment.ALIGN_RIGHT)
					{
						image.SetPosition(m_nNextBlockX - image.Width, m_nNextBlockY - (image.Height / 2));
					}
				}
			}

			// calculate offsets now to save time later (200+ times per render)
			m_nOffsetX = this.XPosition + ((this.Width - (m_cxBlock * Game.Width)) / 2);
			m_nOffsetY = this.YPosition + ((this.Height - (m_cyBlock * Game.Height)) / 2);
			m_nOffsetY = m_nOffsetY - m_cyBlock;
		}

		public override void Render()
		{
			if(GUIGraphicsContext.EditMode == false && m_bVisible == false)
			{
				return;
			}

			if(m_bHasFocus == false && (m_theGame != null && m_theGame.State == State.Running))
			{
				m_theGame.State = State.Paused;
			}

			bool bRenderTexture = true;

			if(m_theGame == null || (m_theGame != null && m_theGame.State == State.Running))
			{
				// draw the texture first so that it appears behind the blocks
				RenderTexture();

				bRenderTexture = false;
			}
			
			if(m_theGame != null)
			{
				m_theGame.Tick();
				m_theGame.Render();
			}

			if(bRenderTexture)
			{
				// draw the now so that the blocks appear faded
				RenderTexture();
			}

			RenderText();
			RenderNext();
		}

		public void RenderNext()
		{
			int nBlock = m_theGame != null ? (m_theGame.NextBlock - 1) : -1;

			if(nBlock != -1 && (nBlock < 0 || nBlock > 6))
			{
				return;
			}

			if(nBlock != -1 && m_imgBlocksNext != null && m_imgBlocksNext[nBlock] != null)
			{
				m_imgBlocksNext[nBlock].Render();
			}
		}

		public override void OnAction(Action action)
		{
			if(action.wID == Action.ActionType.ACTION_KEY_PRESSED && action.m_key.KeyChar == 0x1B)
			{
				if(m_theGame != null && m_theGame.State == State.Running)
				{
					m_theGame.State = State.Paused;
				}
				else
				{
					base.OnAction(action);
				}
			}
			else if(action.wID == Action.ActionType.ACTION_SELECT_ITEM || (action.wID == Action.ActionType.ACTION_KEY_PRESSED && action.m_key.KeyCode == 0x13))
			{
				if(m_theGame == null)
				{
					Start();
				}
				else if(m_theGame != null)
				{
					switch(m_theGame.State)
					{
						case State.Stopped:
							Start();
							break;
						case State.Paused:
							m_theGame.State = State.Running;
							break;
						case State.Running:
							m_theGame.MoveBlock(Game.Move.Drop);
							break;
					}
				}
			}
			else if(action.wID == Action.ActionType.ACTION_MOVE_UP)
			{
				if(m_theGame != null && m_theGame.State == State.Running)
				{
					m_theGame.MoveBlock(Game.Move.Rotate);
				}
				else
				{
					base.OnAction(action);
				}
			}
			else if(action.wID == Action.ActionType.ACTION_MOVE_DOWN)
			{
				if(m_theGame != null && m_theGame.State == State.Running)
				{
					m_theGame.MoveBlock(Game.Move.Down);
				}
				else
				{
					base.OnAction(action);
				}
			}
			else if(action.wID == Action.ActionType.ACTION_MOVE_LEFT)
			{
				if(m_theGame != null && m_theGame.State == State.Running)
				{
					m_theGame.MoveBlock(Game.Move.Left);
				}
				else
				{
					base.OnAction(action);
				}
			}
			else if(action.wID == Action.ActionType.ACTION_MOVE_RIGHT)
			{
				if(m_theGame != null && m_theGame.State == State.Running)
				{
					m_theGame.MoveBlock(Game.Move.Right);
				}
				else
				{
					base.OnAction(action);
				}
			}
			else if(m_bWarpEnabled && action.wID >= Action.ActionType.REMOTE_1 && action.wID <= Action.ActionType.REMOTE_9)
			{
				if(m_theGame != null)
				{
					m_theGame.Level = action.wID - Action.ActionType.REMOTE_1;
				}
			}
			else
			{
				base.OnAction(action);
			}
		}

		#endregion Overrides

		#region Skin variables

		[XMLSkinElement("font")]
		protected string			m_strFont = "font18";

		[XMLSkinElement("textcolor")]
		protected long  			m_dwTextColor = 0xFFFFFFFF;

		[XMLSkinElement("colordiffuse")]
		protected long  			m_dwColorDiffuse = 0xFFFFFFFF;

		[XMLSkinElement("texturefocus")]			
		protected string  			m_strTextureFocused = @"tetris\background_focus.png";

		[XMLSkinElement("texture")]
		protected string  			m_strTexture = @"tetris\background.png";

		[XMLSkinElement("textureO")]
		protected string			m_strTextureO = @"tetris\block_O.png";

		[XMLSkinElement("textureI")]
		protected string			m_strTextureI = @"tetris\block_I.png";

		[XMLSkinElement("textureS")]
		protected string			m_strTextureS = @"tetris\block_S.png";

		[XMLSkinElement("textureZ")]
		protected string			m_strTextureZ = @"tetris\block_Z.png";

		[XMLSkinElement("textureL")]
		protected string			m_strTextureL = @"tetris\block_L.png";

		[XMLSkinElement("textureT")]
		protected string			m_strTextureT = @"tetris\block_T.png";

		[XMLSkinElement("textureJ")]
		protected string			m_strTextureJ = @"tetris\block_J.png";

		[XMLSkinElement("nextblockx")]			
		protected int  				m_nNextBlockX = 60;

		[XMLSkinElement("nextblocky")]			
		protected int  				m_nNextBlockY = 60;

		[XMLSkinElement("nextblockalign")]
		protected Alignment  		m_nNextBlockAlign = Alignment.ALIGN_CENTER;

		[XMLSkinElement("textureLeft")]
		protected string  			m_strTextureLeft = @"tetris\guide.png";

		[XMLSkinElement("textureRight")]
		protected string  			m_strTextureRight = @"tetris\guide.png";

		#endregion

		#region Member variables
		
		Game						m_theGame = null;
		GUIFont						m_Font = null;
		GUIImage					m_imgTexture = null;
		GUIImage					m_imgTextureFocused = null;
		int							m_cxPaused = 0;
		int							m_cyPaused = 0;
		int							m_cxGameOver = 0;
		int							m_cyGameOver = 0;
		int							m_cxPressToStart = 0;
		int							m_cyPressToStart = 0;
		int							m_nOffsetX;
		int							m_nOffsetY;
		string						m_strStart;
		string						m_strPaused;
		string						m_strGameOver;
		bool						m_bWarpEnabled = true;
		bool						m_bSounds = true;
		bool						m_bMusic = false;
		GUIImage[]					m_imgBlocks;
		GUIImage[]					m_imgBlocksGlow;
		GUIImage[]					m_imgGuide;
		GUIImage[]					m_imgBlocksNext;
		int							m_cxBlock;
		int							m_cyBlock;

		#endregion Member variables
	}
}