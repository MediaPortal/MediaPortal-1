#region Copyright (C) 2005-2006 Team MediaPortal

/* 
 *	Copyright (C) 2005-2006 Team MediaPortal
 *	http://www.team-mediaportal.com
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

#endregion

#region Usings
using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Windows.Forms; // used for Keys definition
using System.Windows.Media.Animation;
using MediaPortal.Utils.Services;
using MediaPortal.GUI.Library;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Direct3D = Microsoft.DirectX.Direct3D;
#endregion

namespace MediaPortal.GUI.Library
{
	public class GUIMenuControl : GUIControl
	{
		#region Properties (Skin)
		[XMLSkinElement("spaceBetweenButtons")]	 protected int    _spaceBetweenButtons  = 8;
		[XMLSkinElement("textcolor")]        		 protected long   _textColor = 0xFFFFFFFF;
		[XMLSkinElement("align")]                Alignment        _textAlignment = Alignment.ALIGN_LEFT;
		[XMLSkinElement("buttonHeight")]         protected int    _buttonHeight         = 30;
		[XMLSkinElement("buttonTextXOff")] 		   protected int    _buttonTextXOffset    = 10;
		[XMLSkinElement("buttonTextYOff")]    	 protected int    _buttonTextYOffset    = 8;
		[XMLSkinElement("buttonFont")]  		     protected string _buttonFont           = "font16";
		[XMLSkinElement("numberOfButtons")]		   protected int    _numberOfButtons      = 5;
		[XMLSkinElement("textureBackground")] 	 protected string _textureBackground    = String.Empty;
		[XMLSkinElement("textureButtonFocus")]	 protected string _textureButtonFocus   = String.Empty;
		[XMLSkinElement("textureButtonNoFocus")] protected string _textureButtonNoFocus = String.Empty;
    [XMLSkinElement("hoverX")]               protected int    _hoverPositionX       = 0;
    [XMLSkinElement("hoverY")]               protected int    _hoverPositionY       = 0;
    [XMLSkinElement("hoverWidth")]           protected int    _hoverWidth           = 0;
    [XMLSkinElement("hoverHeight")]          protected int    _hoverHeight          = 0;

		#endregion

		#region Private Enumerations
		protected enum State
		{
			Idle,
			ScrollUp,
			ScrollDown,
		}
		#endregion

		#region Variables
		protected List<PlugInInfo> _buttonInfos = new List<PlugInInfo>();
		protected List<GUIButtonControl> _buttonList = new List<GUIButtonControl>();
    protected List<GUIAnimation> _hoverList = new List<GUIAnimation>();
    protected GUIAnimation _backgroundImage = null;
    protected GUIAnimation _focusImage = null; 
		protected int _buttonOffset         = 25;         // offset to the from the border to the buttons
		protected int _buttonWidth          = 0;          // width of one button 
		protected State _currentState       = State.Idle; // current State of the animation
		protected State _nextState          = State.Idle; // what follows when _currentState ends
		protected State _mouseState         = State.Idle; // autoscrolling with the mouse 
		protected Viewport _newViewport     = new Viewport(); // own viewport for scrolling
		protected Viewport _oldViewport;                  // storage of the currrent Viewport 
		ScrollAnimator _scrollLabel         = null;
		protected int _animationTime        = 0;          // duration for a scroll animation
		protected int _animationTimeMin     = 100;        // min duration for a scrolling - speedup
		protected int _animationTimeMax     = 160;        // max. duration for a scrolling - normal
		protected int _focusPosition        = 0;          // current position of the focus bar 
		protected bool _fixedScroll         = false; 		  // fix scrollbar in the middle of menu
		protected bool _useMyPlugins        = true;
		protected bool _ignoreFirstUpDown   = true;
    protected GUIAnimation _hoverImage  = null;
    #endregion

		#region Properties
		public int FocusedButton
		{
			get { return _focusPosition; }
			set
			{
				bool oldFocus = _buttonList[_focusPosition].Focus;
				_buttonList[_focusPosition].Focus = false;
				_focusPosition = value;
				_buttonList[_focusPosition].Focus = oldFocus;
				_focusImage.SetPosition(_focusImage._positionX, _buttonList[_focusPosition]._positionY);
			}
		}

		public List<PlugInInfo> ButtonInfos
		{
			get { return _buttonInfos; }
		}

		#endregion

		#region Constructor & OnInit
		public GUIMenuControl(int dwParentID) : base(dwParentID)
		{
	  }

		public override void FinalizeConstruction()
		{
			_focusPosition = (_numberOfButtons / 2) + 1; // +1, because of the hidden button for scrolling
			Height = ((_buttonHeight + _spaceBetweenButtons) * _numberOfButtons) + 2 * _buttonOffset;
			_buttonWidth = Width - 2 * _buttonOffset;
			base.FinalizeConstruction();
		}

		public override void OnInit()
		{
			_animationTime = _animationTimeMax;
			LoadSetting();
      LoadHoverImage(FocusedButton);
			base.OnInit();
		}
				
		#endregion

		#region Serialisation
		protected void LoadSetting()
		{
			using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings("MediaPortal.xml"))
			{
				string section = "Menu"+this.ParentID.ToString();
				int focus    = xmlreader.GetValueAsInt(section, "focus", 3);
				string label = xmlreader.GetValue(section, "label");
			
			  if (label == null) return;

			  for (int i = 0; i < _buttonList.Count; i++)
			  {
					if (_buttonList[focus].Label.Equals(label))
					{
						FocusedButton = focus;
						break;
					}
				  // move top button to the end of the list
				  foreach (GUIButtonControl btn in _buttonList)
				  {
					  // move all buttons up
					  btn.SetPosition(btn._positionX, btn._positionY - (_buttonHeight + _spaceBetweenButtons));
				  }
				  // move top button to the end of the list
				  GUIButtonControl button = _buttonList[0];
				  _buttonList.RemoveAt(0);
				  button._positionY = _buttonList[_buttonList.Count - 1]._positionY + _spaceBetweenButtons + _buttonHeight;
				  _buttonList.Add(button);
		  	}
			}
		}

		protected void SaveSetting()
		{
			using (MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.Settings("MediaPortal.xml"))
			{
				string section = "Menu" + this.ParentID.ToString();
				xmlwriter.SetValue(section, "focus", FocusedButton.ToString());
				if (_buttonList.Count > 0) xmlwriter.SetValue(section, "label", _buttonList[FocusedButton].Label);
			}
		}
		#endregion

		#region Resources functions
		public override void AllocResources()
		{
			int buttonX = _positionX + _buttonOffset;
			int buttonY = _positionY + _spaceBetweenButtons + _buttonOffset;
			buttonY -= (_buttonHeight + _spaceBetweenButtons);  // one invisible button for scrolling 

			int controlID = 1;
			_buttonList.Clear();
      _hoverList.Clear();
			while ((_buttonInfos.Count > 0) && (_buttonList.Count < _numberOfButtons))
			{
				for (int i = 0; i < _buttonInfos.Count; i++)
				{
					PlugInInfo info = _buttonInfos[i];
					GUIButtonControl button = new GUIButtonControl(GetID, controlID, buttonX, buttonY, _buttonWidth, _buttonHeight,
																		_textureButtonFocus, _textureButtonNoFocus);
					button.Label = info.Text;
					button.Data = info;
					button.ParentControl = this;
					button.FontName = _buttonFont;
					button.TextOffsetX = _buttonTextXOffset;
					button.TextOffsetY = _buttonTextYOffset;
					button.AllocResources();
					_buttonList.Add(button);
					buttonY += (_buttonHeight + _spaceBetweenButtons);
					controlID++;
				}
			}
      _backgroundImage = LoadAnimationControl(GetID, controlID, _positionX, _positionY, _width, Height, _textureBackground);
			_backgroundImage.AllocResources();
			controlID++;

      _focusImage = LoadAnimationControl(GetID, controlID, buttonX, _buttonList[FocusedButton]._positionY, _buttonWidth, _buttonHeight, _textureButtonFocus);
			_focusImage.AllocResources();
			_focusImage.Visible = false;

      if ((_hoverHeight > 0) && (_hoverWidth > 0))
      {
        foreach (GUIButtonControl btn in _buttonList)
        {
          string fileName = GetHoverFileName(btn.Label);
          if (fileName != null)
          {
            GUIAnimation hover = LoadAnimationControl(GetID, btn.GetID, _hoverPositionX, _hoverPositionY, _hoverWidth, _hoverHeight, fileName);
            hover.AllocResources();
            _hoverList.Add(hover);
          }
        }
      }
			base.AllocResources();
		}

		public override void FreeResources()
		{
			SaveSetting();
			foreach (GUIControl control in _buttonList)	control.FreeResources();
      foreach (GUIAnimation hover in _hoverList)  hover.FreeResources();
			_buttonList.Clear();
      _hoverList.Clear();
			if (_backgroundImage != null) _backgroundImage.FreeResources();
			if (_focusImage != null) _focusImage.FreeResources();
			base.FreeResources();
		}

    protected string GetHoverFileName(string name)
    {
      name = String.Format(@"{0}\media\hover_{1}", GUIGraphicsContext.Skin, name);
      string filename = name + ".png";
      if (System.IO.File.Exists(filename)) return filename;

      filename = name + ".gif";
      if (System.IO.File.Exists(filename)) return filename;

      filename = name + ".bmp";
      if (System.IO.File.Exists(filename)) return filename;

      filename = name + ".xml";
      if (System.IO.File.Exists(filename)) return filename;

      return null;
    }

		#endregion

		#region OnAction function
		public override void  OnAction(Action action)
		{
      switch (action.wID) 
			{
				case Action.ActionType.ACTION_MOUSE_MOVE:
			  {
				  int x = (int)action.fAmount1;
				  int y = (int)action.fAmount2;
				  int controlID = 0;
				  bool focused = false;
					
					_ignoreFirstUpDown = false;
					if (HitTest(x, y, out controlID, out focused))
					{
						double middlePos = YPosition + Height / 2;
						if (_fixedScroll)  // we can not move the scrollbar
						{
							if (y < middlePos - _buttonHeight / 2 - _spaceBetweenButtons)
							{
								middlePos -= _buttonHeight / 2;
								_animationTime = _animationTimeMax - (int)((_animationTimeMax - _animationTimeMin) *
																 (Math.Abs(middlePos - y) / (middlePos - YPosition)));
								_mouseState = State.ScrollUp;
								if (_currentState == State.Idle) OnUp();
							}
							else if (y > middlePos + _buttonHeight / 2 + _spaceBetweenButtons)
							{
								middlePos += _buttonHeight / 2;
								_animationTime = _animationTimeMax - (int)((_animationTimeMax - _animationTimeMin) *
																 (Math.Abs(middlePos - y) / (middlePos - YPosition)));
								_mouseState = State.ScrollDown;
								if (_currentState == State.Idle) OnDown();
							}
							else
							{
								_mouseState = State.Idle;
								if (_currentState != State.Idle) _scrollLabel.StopAnimation = false;
							}
						}
						else   // scrollbar can move
						{
							if (y < YPosition + _buttonOffset + _spaceBetweenButtons + _buttonHeight)
							{
								_mouseState = State.ScrollUp;
								if (_currentState == State.Idle) OnUp();
							}
							else if (y > YPosition + Height - _buttonOffset - 2*_spaceBetweenButtons - _buttonHeight)
							{
								_mouseState = State.ScrollDown;
								if (_currentState == State.Idle) OnDown();
							}
							else  // direct selection in the middle
							{
								_mouseState = State.Idle;
								if (_currentState != State.Idle) _scrollLabel.StopAnimation = false;
								else
								{  // move scroll bar only when idle
									double button = _buttonHeight + _spaceBetweenButtons;
									int position = 1 + (int)Math.Round(((double)y - YPosition - 2*_buttonOffset) / button);
									if (Math.Abs(FocusedButton - position) > 1) FocusedButton = position;
									else
									{
										if (position > FocusedButton) OnDown();
										else if (position < FocusedButton) OnUp();
									}
								}
							}
						}
					}
					break;
				}

				case Action.ActionType.ACTION_MOVE_UP:
			  {
					if (_ignoreFirstUpDown)
					{
						_ignoreFirstUpDown = false;
						return;
					}
					if (_currentState == State.Idle) _animationTime = _animationTimeMax;
					_mouseState = State.Idle;
				  OnUp();
					return;
			  }
				
				case Action.ActionType.ACTION_MOVE_DOWN:
			  {
					if (_ignoreFirstUpDown)
					{
						_ignoreFirstUpDown = false;
						return;
					}
					if (_currentState == State.Idle) _animationTime = _animationTimeMax;
					_mouseState = State.Idle;
					OnDown();
					return;
			  }
			  
        case Action.ActionType.ACTION_SELECT_ITEM:
				case Action.ActionType.ACTION_MOUSE_CLICK:
				{
					if (_currentState != State.Idle) return;
					PlugInInfo info = _buttonList[FocusedButton].Data as PlugInInfo;
					if (info != null)
					{
						// button selected.
						// send a message to the parent window
						GUIMessage message = new GUIMessage(GUIMessage.MessageType.GUI_MSG_CLICKED, WindowId, GetID, ParentID, info.PluginID, 0, info);
						GUIGraphicsContext.SendMessage(message);
					}
					break;
				}

		  }
	
			base.OnAction(action);
		}
		#endregion

		#region OnMessage function
		public override bool  OnMessage(GUIMessage message)
		{
		
			return base.OnMessage(message);
		}
		#endregion

		#region Reder functions
		public override void Render(float timePassed)
		{
			if (_backgroundImage != null) _backgroundImage.Render(timePassed);
      if (_hoverImage != null) _hoverImage.Render(timePassed);
			ShowDebugInfo();	
			_oldViewport = GUIGraphicsContext.DX9Device.Viewport;
			_newViewport.X = _positionX;
			_newViewport.Y = _positionY + _buttonOffset;
			_newViewport.Width = Width;
			_newViewport.Height = Height - 2 * _buttonOffset;
			_newViewport.MinZ = 0.0f;
			_newViewport.MaxZ = 1.0f;
			GUIGraphicsContext.DX9Device.Viewport = _newViewport;
			
			if (_currentState != State.Idle) AnimationMovement(timePassed);
			_focusImage.Render(timePassed);
			foreach (GUIButtonControl button in _buttonList)
			{
				button.Render(timePassed);
			}
			GUIGraphicsContext.DX9Device.Viewport = _oldViewport;
			if (_currentState != State.Idle) AnimationFinished(timePassed);
		}

		protected void AnimationFinished(float timePassed)
		{
			if (_scrollLabel == null) return;
			if (_scrollLabel.Running != false) return;

			_buttonList[FocusedButton].Focus = false;  // unfocus before any changes
			_focusImage.Visible = false;
			if (_fixedScroll)
			{
				// Final State is reached let us have always one invisible button on top
				if (_currentState == State.ScrollDown)  // current state: invisible button is now visible
				{
					// move one button from the end of the list to the beginning
					GUIButtonControl button = _buttonList[_buttonList.Count - 1];
					_buttonList.RemoveAt(_buttonList.Count - 1);
					button._positionY = _positionY + _buttonOffset - _buttonHeight;
					_buttonList.Insert(0, button);
				}
				else if (_currentState == State.ScrollUp) // current state: two invisible buttons on top
				{
					// move the top element to the end
					GUIButtonControl button = _buttonList[0];
					_buttonList.RemoveAt(0);
					button._positionY = _buttonList[_buttonList.Count - 1]._positionY + _spaceBetweenButtons + _buttonHeight;
					_buttonList.Add(button);
				}
			}
			else
			{
				if (((_numberOfButtons-1 > FocusedButton) || (_currentState == State.ScrollUp)) &&
					  ((2 < FocusedButton) || (_currentState == State.ScrollDown)))
				{
					if (_currentState == State.ScrollDown) FocusedButton++;
					else if (_currentState == State.ScrollUp) FocusedButton--;
				}
				else 
				{
						// Final State is reached let us have always one invisible button on top
				  if (_currentState == State.ScrollUp)  // current state: invisible button is now visible
				  {
					  // move one button from the end of the list to the beginning
					  GUIButtonControl button = _buttonList[_buttonList.Count - 1];
					  _buttonList.RemoveAt(_buttonList.Count - 1);
					  button._positionY = _positionY + _buttonOffset - _buttonHeight;
					  _buttonList.Insert(0, button);
				  }
				  else if (_currentState == State.ScrollDown) // current state: two invisible buttons on top
				  {
					  // move the top element to the end
					  GUIButtonControl button = _buttonList[0];
					  _buttonList.RemoveAt(0);
					  button._positionY = _buttonList[_buttonList.Count - 1]._positionY + _spaceBetweenButtons + _buttonHeight;
					  _buttonList.Add(button);
				  }
				}
			}
			_focusImage.SetPosition(_focusImage._positionX, _buttonList[FocusedButton]._positionY);
			_buttonList[FocusedButton].Focus = true; // focus after all changes
      LoadHoverImage(FocusedButton);
      _scrollLabel = null;
			_currentState = State.Idle;
			if ((_nextState == State.ScrollDown) || (_mouseState == State.ScrollDown)) OnDown();
			else if ((_nextState == State.ScrollDown) || (_mouseState == State.ScrollUp)) OnUp();
		}

		protected void AnimationMovement(float timePassed)
		{
			if (_scrollLabel == null) return;
			if (_scrollLabel.Running == false) return;
		
			int increment = _scrollLabel.DeltaValue;
			if (_currentState == State.ScrollUp) increment = -increment;

			if (_fixedScroll) // we have a fixed scrollbar -> move the buttons
			{
				foreach (GUIControl control in _buttonList)
					control.SetPosition(control._positionX, control._positionY + increment);
			}
			else // scrollbar can move -> move scrollbar until it reaches the top or button
			{
				if (((_numberOfButtons-1 > FocusedButton) || (_currentState == State.ScrollUp)) &&
					  ((2 < FocusedButton) || (_currentState == State.ScrollDown)))
			  {
				  _focusImage.SetPosition(_focusImage._positionX, _focusImage._positionY + increment);
			  }
				else  // we reched the top or the button -> move the buttons  
				{
					foreach (GUIControl control in _buttonList)
						control.SetPosition(control._positionX, control._positionY - increment);
				}
			}
		}

    protected void LoadHoverImage(int position)
    {
      _hoverImage = null;
      if ((position < 0) || (position >= _hoverList.Count)) return;

      foreach (GUIAnimation hover in _hoverList)
      {
        if (hover.GetID == _buttonList[position].GetID) _hoverImage = hover;
      }
    }

		private void LogButtons(string text)
		{
			/*_log.Debug(text);
			foreach (GUIControl control in _buttonList)
			{
				_log.Debug("  Button{0}: Y = {1}", control.GetID, control.YPosition);
			}
			*/
		}

		private void ShowDebugInfo()
		{
			//GUIPropertyManager.SetProperty("#test1", Focus.ToString());
			//GUIPropertyManager.SetProperty("#test2", FocusedButton.ToString());
		}


    #endregion

		#region Overrides
		public override bool Focus
		{
			get { return base.Focus;	}
			set
			{
        if (value == false)
        {
          _mouseState = State.Idle;  // stop scrolling
        }
        else
        {
          if (_backgroundImage != null) _backgroundImage.Begin();
          if (_focusImage != null) _focusImage.Begin();
        }
				//else _ignoreFirstUpDown = true;
				base.Focus = value;
				if (_buttonList.Count > FocusedButton) _buttonList[FocusedButton].Focus = value;
			}
		}

		public override void ScaleToScreenResolution()
		{
			base.ScaleToScreenResolution();
			GUIGraphicsContext.ScaleVertical(ref _buttonOffset);
			GUIGraphicsContext.ScaleVertical(ref _spaceBetweenButtons);
			GUIGraphicsContext.ScaleVertical(ref _buttonHeight);
			GUIGraphicsContext.ScalePosToScreenResolution(ref _buttonTextXOffset, ref _buttonTextYOffset);
		}

		#endregion

		#region OnUp, OnDown
		protected void OnUp()
		{
			if (_currentState != State.Idle)
			{
				_nextState = State.ScrollUp;
				_scrollLabel.StopAnimation = true;
				if (_animationTime > _animationTimeMin) _animationTime -= 5;
				return;
			}
			_currentState = State.ScrollUp;
			_nextState = State.Idle;
			_buttonList[_focusPosition].Focus = false;  // hide button focus for animation
			_focusImage.Visible = true;                 // show Image for animation 
			_scrollLabel = new ScrollAnimator(_animationTime, 0, _buttonHeight + _spaceBetweenButtons);
			_scrollLabel.Begin();
			if (_mouseState != State.Idle) _scrollLabel.StopAnimation = true; // speed up
      LoadHoverImage(FocusedButton-1);
		}

		protected void OnDown()
		{
			if (_currentState != State.Idle)
			{
				_nextState = State.ScrollDown;
				_scrollLabel.StopAnimation = true;
				if (_animationTime > _animationTimeMin) _animationTime -= 5;
				return;
			}
			_currentState = State.ScrollDown;
			_nextState = State.Idle;
			_buttonList[_focusPosition].Focus = false;  // hide button focus for animation
			_focusImage.Visible = true;                 // show Image for animation 
			_scrollLabel = new ScrollAnimator(_animationTime, 0, _buttonHeight + _spaceBetweenButtons);
			_scrollLabel.Begin();
			if (_mouseState != State.Idle) _scrollLabel.StopAnimation = true;  // speed up
      LoadHoverImage(FocusedButton+1);
		}
		#endregion

	}


	#region PlugInInfo
	public class PlugInInfo
	{
		protected string _text;
		protected string _image;
		protected string _imageFocus;
		protected string _pictureImage;
		protected int    _pluginID;

		public PlugInInfo(string Text, string Image, string ImageFocus, string PicImage, int PlugInID)
		{
			_text = Text;
			_image = Image;
			_imageFocus = ImageFocus;
			_pictureImage = PicImage;
			_pluginID = PlugInID;      
		}

		public string Text
		{ get { return _text; } }

		public string Image
		{ get { return _image; } }

		public string ImageFocus
		{ get { return _imageFocus; } }

		public string PictureImage
		{ get { return _pictureImage; } }

		public int PluginID
		{ get { return _pluginID; } }
	}

	#endregion


	#region ScrollAnimator
	public class ScrollAnimator
	{
		#region fields
		protected int _duration;
		protected int _start;
		protected int _stop;
		protected int _lastValue;
		protected double _startTicks = 0;
		protected double _valuePerDuration = 0;
		protected bool _animation = false;
		protected int[] _upDown;
		protected int _count;
		protected bool _stopAnimation;
		protected ILog _log;
		#endregion

		public ScrollAnimator(int duration, int StartValue, int StopValue)
		{
			ServiceProvider services = GlobalServiceProvider.Instance;
			_log = services.Get<ILog>();

			_duration = duration;
			_start = StartValue;
			_stop = StopValue;
			_valuePerDuration = ((double)(_stop - _start)) / (double)duration;
			_upDown = new int[12];
			int delta = 2;
			_upDown[0] = delta;
			_upDown[1] = delta;
			_upDown[2] = delta;
			_upDown[3] = delta;
			_upDown[4] = -delta;
			_upDown[5] = -delta;
			_upDown[6] = -delta;
			_upDown[7] = -delta;
			_upDown[8] = -delta;
			_upDown[9] = -delta;
			_upDown[10] = delta;
			_upDown[11] = delta;
			_count = -1;
			_stopAnimation = false;
		}

		public void Begin()
		{
			_startTicks = AnimationTimer.TickCount;
			_animation = true;
			_lastValue = 0;
			_count = -1;
			_stopAnimation = false;
		}

		public virtual int Value
		{
			get
			{
				double elapsedTicks = AnimationTimer.TickCount - _startTicks;
				if (elapsedTicks >= _duration)
				{
					if (_count == -1) _lastValue = _stop;
					_count++;
					if ((!_stopAnimation) && (_count < _upDown.Length))
					{
						_lastValue += _upDown[_count];
						return _lastValue;
					}
					else
					{
						_animation = false;
						return _stop;
					}
				}
				_lastValue = (int)Math.Round(_valuePerDuration * elapsedTicks);
				return (_lastValue);
			}
		}

		public virtual int DeltaValue
		{
			get
			{
				int tmp = _lastValue;
				//_log.Debug("Ticks= {0}, Duration = {1}, Value = {2}", AnimationTimer.TickCount - _startTicks, _duration, _lastValue);
				return (Value - tmp);
			}
		}

		public bool Running
		{
			get { return _animation; }
		}

		public bool StopAnimation
		{
			get { return _stopAnimation; }
			set { _stopAnimation = value; }
		}
	}
	#endregion
}
