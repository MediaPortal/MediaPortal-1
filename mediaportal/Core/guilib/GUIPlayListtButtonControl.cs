/* 
 *	Copyright (C) 2005 Team MediaPortal
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
using System;

namespace MediaPortal.GUI.Library
{
    /// <summary>
    /// The implementation of a PlayList item button with move up, move down, and delete playlist item buttons.
    /// </summary>
    public class GUIPlayListButtonControl : GUIButtonControl
	{
        #region Variables

        [XMLSkinElement("textureMoveUp")]
        string TextureMoveUpFileName = "playlist_item_up_nofocus.png";

        [XMLSkinElement("textureMoveUpFocused")]
        string TextureMoveUpFocusedFileName = "playlist_item_up_focus.png";

        [XMLSkinElement("textureMoveDown")]
        string TextureMoveDownFileName = "playlist_item_down_nofocus.png";

        [XMLSkinElement("textureMoveDownFocused")]
        string TextureMoveDownFocusedFileName = "playlist_item_down_focus.png";

        [XMLSkinElement("textureDelete")]
        string TextureDeleteFileName = "playlist_item_delete_nofocus.png";

        [XMLSkinElement("textureDeleteFocused")]
        string TextureDeleteFocusedFileName = "playlist_item_delete_focus.png";

        bool _isAscending = true;
        bool IsEditImageHot = false;

        [XMLSkinElement("upBtnWidth")]
        int UpBtnWidth = 35;

        [XMLSkinElement("downBtnWidth")]
        int DownBtnWidth = 35;

        [XMLSkinElement("deleteBtnWidth")]
        int DeleteBtnWidth = 35;

        [XMLSkinElement("upBtnHeight")]
        int UpBtnHeight = 38;

        [XMLSkinElement("downBtnHeight")]
        int DownBtnHeight = 38;

        [XMLSkinElement("deleteBtnHeight")]
        int DeleteBtnHeight = 38;

        [XMLSkinElement("upBtnXOffset")]
        int UpBtnXOffset = 0;

        [XMLSkinElement("downBtnXOffset")]
        int DownBtnXOffset = 0;

        [XMLSkinElement("deleteBtnXOffset")]
        int DeleteBtnXOffset = 0;

        [XMLSkinElement("upBtnYOffset")]
        int UpBtnYOffset = 0;

        [XMLSkinElement("downBtnYOffset")]
        int DownBtnYOffset = 0;

        [XMLSkinElement("deleteBtnYOffset")]
        int DeleteBtnYOffset = 0;

        public enum ActiveButton { None, Main, Up, Down, Delete }
        private ActiveButton _CurrentActiveButton = ActiveButton.None;

        private static ActiveButton _LastActiveButton = ActiveButton.None;
        private static bool _SuppressActiveButtonReset = false;

        private GUIImage ImgUpButtonNormal;
        private GUIImage ImgUpButtonFocused;

        private GUIImage ImgDownButtonNormal;
        private GUIImage ImgDownButtonFocused;

        private GUIImage ImgDeleteButtonNormal;
        private GUIImage ImgDeleteButtonFocused;

        private int _NavigateLeft;
        private int _NavigateRight;
        
        #endregion Fields
        
        #region Properties

        public static ActiveButton LastActiveButton
        {
            get { return _LastActiveButton; }
            set { _LastActiveButton = value; }
        }

        public static bool SuppressActiveButtonReset
        {
            get { return _SuppressActiveButtonReset; }
            set { _SuppressActiveButtonReset = value; }
        }

        public ActiveButton CurrentActiveButton
        {
            get { return _CurrentActiveButton; }
            set 
            {
                if (_SuppressActiveButtonReset)
                    _CurrentActiveButton = _LastActiveButton;

                else
                    _CurrentActiveButton = value; 
            }

            //set { _CurrentActiveButton = value; }
        }

        #endregion Properties

        #region Constructors

		public GUIPlayListButtonControl(int parentId) : base(parentId)
		{
			
		}


        ////        ///// <summary>
        ////        ///// The constructor of the GUIPlayListButtonControl class.
        ////        ///// </summary>
        ////        ///// <param name="dwParentID">The parent of this control.</param>
        ////        ///// <param name="dwControlId">The ID of this control.</param>
        ////        ///// <param name="dwPosX">The X position of this control.</param>
        ////        ///// <param name="dwPosY">The Y position of this control.</param>
        ////        ///// <param name="dwWidth">The width of this control.</param>
        ////        ///// <param name="dwHeight">The height of this control.</param>
        ////        ///// <param name="strTextureFocus">The filename containing the texture of the butten, when the button has the focus.</param>
        ////        ///// <param name="strTextureNoFocus">The filename containing the texture of the butten, when the button does not have the focus.</param>
        ////        //public GUIPlayListButtonControl(int dwParentID, int dwControlId, int dwPosX, int dwPosY, int dwWidth, int dwHeight, string strTextureFocus, string strTextureNoFocus)
        ////        //    : base(dwParentID, dwControlId, dwPosX, dwPosY, dwWidth, dwHeight)
        ////        //{
        ////        //    _focusedTextureName = strTextureFocus;
        ////        //    _nonFocusedTextureName = strTextureNoFocus;
        ////        //    FinalizeConstruction();
        ////        //}


        public GUIPlayListButtonControl(int dwParentID, int dwControlId, int dwPosX, int dwPosY, int dwWidth, int dwHeight, string strTextureFocus, string strTextureNoFocus,
            int upBtnWidth,
            int downBtnWidth,
            int deleteBtnWidth,
            int upBtnHeight,
            int downBtnHeight,
            int deleteBtnHeight,
            string strUp,
            string strDown,
            string strDelete,
            string strUpFocus,
            string strDownFocus,
            string strDeleteFocus,
            int upBtnXOffset,
            int downBtnXOffset,
            int deleteBtnXOffset,
            int upBtnYOffset,
            int downBtnYOffset,
            int deleteBtnYOffset)
            : base(dwParentID, dwControlId, dwPosX, dwPosY, dwWidth, dwHeight, strTextureFocus, strTextureNoFocus)
        {
            _focusedTextureName = strTextureFocus;
            _nonFocusedTextureName = strTextureNoFocus;

            UpBtnWidth = upBtnWidth;
            DownBtnWidth = downBtnWidth;
            DeleteBtnWidth = deleteBtnWidth;
            UpBtnHeight = upBtnHeight;
            DownBtnHeight = downBtnHeight;
            DeleteBtnHeight = deleteBtnHeight;
            TextureMoveUpFileName = strUp;
            TextureMoveDownFileName = strDown;
            TextureDeleteFileName = strDelete;
            TextureMoveUpFocusedFileName = strUpFocus;
            TextureMoveDownFocusedFileName = strDownFocus;
            TextureDeleteFocusedFileName = strDeleteFocus;
            UpBtnXOffset = upBtnXOffset;
            DownBtnXOffset = downBtnXOffset;
            DeleteBtnXOffset = deleteBtnXOffset;
            UpBtnYOffset = upBtnYOffset;
            DownBtnYOffset = downBtnYOffset;
            DeleteBtnYOffset = deleteBtnYOffset;
            FinalizeConstruction();
        }
		#endregion Constructors

		#region Events

		public event SortEventHandler	SortChanged;

		#endregion Events

		#region Methods

		public override void AllocResources()
		{
			base.AllocResources();

            ImgUpButtonNormal.AllocResources();
            ImgUpButtonFocused.AllocResources();

            ImgDownButtonNormal.AllocResources();
            ImgDownButtonFocused.AllocResources();

            ImgDeleteButtonNormal.AllocResources();
            ImgDeleteButtonFocused.AllocResources();
		}

        public override void FinalizeConstruction()
        {
            base.FinalizeConstruction();
            ImgUpButtonNormal = new GUIImage(WindowId, WindowId + 10000, UpBtnXOffset, UpBtnYOffset, UpBtnWidth, UpBtnHeight, TextureMoveUpFileName, 0);
            ImgUpButtonFocused = new GUIImage(WindowId, WindowId + 10001, 0, 0, UpBtnWidth, UpBtnHeight, TextureMoveUpFocusedFileName, 0);
            ImgUpButtonNormal.ParentControl = this;
            ImgUpButtonFocused.ParentControl = this;
            ImgUpButtonNormal.BringIntoView();
            ImgUpButtonFocused.BringIntoView();

            ImgDownButtonNormal = new GUIImage(WindowId, WindowId + 10002, DownBtnXOffset, DownBtnYOffset, DownBtnWidth, DownBtnHeight, TextureMoveDownFileName, 0);
            ImgDownButtonFocused = new GUIImage(WindowId, WindowId + 10003, 0, 0, DownBtnWidth, DownBtnHeight, TextureMoveDownFocusedFileName, 0);
            ImgDownButtonNormal.ParentControl = this;
            ImgDownButtonFocused.ParentControl = this;
            ImgDownButtonNormal.BringIntoView();
            ImgDownButtonFocused.BringIntoView();

            ImgDeleteButtonNormal = new GUIImage(WindowId, WindowId + 10004, DeleteBtnXOffset, DeleteBtnYOffset, DeleteBtnWidth, DeleteBtnHeight, TextureDeleteFileName, 0);
            ImgDeleteButtonFocused = new GUIImage(WindowId, WindowId + 10005, 0, 0, DeleteBtnWidth, DeleteBtnHeight, TextureDeleteFocusedFileName, 0);
            ImgDeleteButtonNormal.ParentControl = this;
            ImgDeleteButtonFocused.ParentControl = this;
            ImgDeleteButtonNormal.BringIntoView();
            ImgDeleteButtonFocused.BringIntoView();

            // Keep track of the original NavigateLeft and NavigateRight values...
            _NavigateLeft = NavigateLeft;
            _NavigateRight = NavigateRight;

            this.Label = "Test Text";
        }

		public override void ScaleToScreenResolution()
		{
			base.ScaleToScreenResolution();
		}

		public override void FreeResources()
		{
			base.FreeResources();

            ImgUpButtonNormal.FreeResources();
            ImgUpButtonFocused.FreeResources();

            ImgDownButtonNormal.FreeResources();
            ImgDownButtonFocused.FreeResources();

            ImgDeleteButtonNormal.FreeResources();
            ImgDeleteButtonFocused.FreeResources();
		}

		public override void PreAllocResources()
		{
			base.PreAllocResources();

            ImgUpButtonNormal.PreAllocResources();
            ImgUpButtonFocused.PreAllocResources();

            ImgDownButtonNormal.PreAllocResources();
            ImgDownButtonFocused.PreAllocResources();

            ImgDeleteButtonNormal.PreAllocResources();
            ImgDeleteButtonFocused.PreAllocResources();
		}

        public override void Render(float timePassed)
        {
            bool isFocused = this.Focus;

            if (!isFocused)
                _CurrentActiveButton = ActiveButton.Main;

            if (IsEditImageHot)
                Focus = false;

            int xPos = 0;
            int yPos = 0;

            //if (Focus && _CurrentActiveButton == ActiveButton.Main)
            if (!_SuppressActiveButtonReset && Focus && _CurrentActiveButton == ActiveButton.Main)
                _imageFocused.Render(timePassed);

            else
                _imageNonFocused.Render(timePassed);

            xPos = _imageNonFocused.XPosition + UpBtnXOffset;
            yPos = _imageNonFocused.YPosition + UpBtnYOffset;
            ImgUpButtonFocused.SetPosition(xPos, yPos);
            ImgUpButtonNormal.SetPosition(xPos, yPos);

            if (isFocused && _CurrentActiveButton == ActiveButton.Up)
                ImgUpButtonFocused.Render(timePassed);

            else
                ImgUpButtonNormal.Render(timePassed);

            xPos = _imageNonFocused.XPosition + DownBtnXOffset;
            yPos = _imageNonFocused.YPosition + DownBtnYOffset;
            ImgDownButtonFocused.SetPosition(xPos, yPos);
            ImgDownButtonNormal.SetPosition(xPos, yPos);

            if (isFocused && _CurrentActiveButton == ActiveButton.Down)
                ImgDownButtonFocused.Render(timePassed);

            else
                ImgDownButtonNormal.Render(timePassed);

            xPos = _imageNonFocused.XPosition + DeleteBtnXOffset;
            yPos = _imageNonFocused.YPosition + DeleteBtnYOffset;
            ImgDeleteButtonFocused.SetPosition(xPos, yPos);
            ImgDeleteButtonNormal.SetPosition(xPos, yPos);

            if (isFocused && _CurrentActiveButton == ActiveButton.Delete)
                ImgDeleteButtonFocused.Render(timePassed);

            else
                ImgDeleteButtonNormal.Render(timePassed);
        }

        protected override void Update()
        {
            base.Update();
        }

        public override void OnAction(Action action)
        {
            Console.WriteLine("Action:{0} ActiveButton:{1}", (Action.ActionType)action.wID, _CurrentActiveButton);

            //if (this.Focus)
            //{
                if (action.wID == Action.ActionType.ACTION_MOUSE_CLICK || action.wID == Action.ActionType.ACTION_SELECT_ITEM)
                {
                    Console.WriteLine("ACTION_MOUSE_CLICK ActiveButton:{0}", _CurrentActiveButton);

                    switch (_CurrentActiveButton)
                    {
                        case ActiveButton.Main:
                            {
                                Console.WriteLine("Clicked ActiveButton.Main");
                                break;
                            }

                        case ActiveButton.Up:
                            {
                                Console.WriteLine("Clicked ActiveButton.Up");
                                break;
                            }

                        case ActiveButton.Down:
                            {
                                Console.WriteLine("Clicked ActiveButton.Down");
                                break;
                            }

                        case ActiveButton.Delete:
                            {
                                Console.WriteLine("Clicked ActiveButton.Delete");
                                break;
                            }

                        case ActiveButton.None:
                            {
                                // We should never get here!
                                Console.WriteLine("Clicked ActiveButton.None");
                                break;
                            }
                    }
                }

                else if (action.wID == Action.ActionType.ACTION_MOVE_LEFT)
                {
                    if (_CurrentActiveButton != ActiveButton.Main)
                    {
                        FocusPreviousButton();
                        return;
                    }

                    else
                    {
                        if (NavigateLeft != _windowId)
                            _CurrentActiveButton = ActiveButton.None;

                        else
                            return;
                    }
                }

                else if (action.wID == Action.ActionType.ACTION_MOVE_RIGHT)
                {
                    if (_CurrentActiveButton != ActiveButton.Delete)
                    {
                        FocusNextButton();
                        return;
                    }


                    else
                    {
                        if (NavigateRight != _windowId)
                            _CurrentActiveButton = ActiveButton.None;

                        else
                            return;
                    }
                }
            //}

            //Console.WriteLine("Action:{0} ActiveButton:{1}", (Action.ActionType)action.wID, _ActiveButton);
            base.OnAction(action);
        }

        private void FocusNextButton()
        {
            if (_CurrentActiveButton == ActiveButton.Main)
                _CurrentActiveButton = ActiveButton.Up;

            else if (_CurrentActiveButton == ActiveButton.Up)
                _CurrentActiveButton = ActiveButton.Down;

            else if (_CurrentActiveButton == ActiveButton.Down)
                _CurrentActiveButton = ActiveButton.Delete;

            else if (_CurrentActiveButton == ActiveButton.Delete)
                _CurrentActiveButton = ActiveButton.Delete;
        }

        private void FocusPreviousButton()
        {
            if (_CurrentActiveButton == ActiveButton.Delete)
                _CurrentActiveButton = ActiveButton.Down;

            else if (_CurrentActiveButton == ActiveButton.Down)
                _CurrentActiveButton = ActiveButton.Up;

            else if (_CurrentActiveButton == ActiveButton.Up)
                _CurrentActiveButton = ActiveButton.Main;

            else if (_CurrentActiveButton == ActiveButton.Main)
                _CurrentActiveButton = ActiveButton.Main;
        }
        
        public override bool OnMessage(GUIMessage message)
        {
            if(message.Message == GUIMessage.MessageType.GUI_MSG_SETFOCUS || message.Message == GUIMessage.MessageType.GUI_MSG_LOSTFOCUS)
                IsEditImageHot = false;

            else if (message.Message == GUIMessage.MessageType.GUI_MSG_CLICKED)
            {
                Console.WriteLine("A Control was clicked!");
            }

            else if (message.Message == GUIMessage.MessageType.GUI_MSG_LOSTFOCUS)
            {
                _CurrentActiveButton = ActiveButton.None;
            }

            else if (message.Message == GUIMessage.MessageType.GUI_MSG_SETFOCUS)
            {
                _CurrentActiveButton = ActiveButton.Main;
            }

            return base.OnMessage (message);
        }

        public override bool HitTest(int x, int y, out int controlID, out bool focused)
        {
            int focusedControlID = 0;

            if (ImgUpButtonNormal.InControl(x, y, out focusedControlID))
            {
                _CurrentActiveButton = ActiveButton.Up;
            }

            else if (ImgDownButtonNormal.InControl(x, y, out focusedControlID))
            {
                _CurrentActiveButton = ActiveButton.Down;
            }

            else if (ImgDeleteButtonNormal.InControl(x, y, out focusedControlID))
            {
                _CurrentActiveButton = ActiveButton.Delete;
            }

            else
            {
                _CurrentActiveButton = ActiveButton.Main;
            }

            Console.WriteLine("X:{0} Y:{1} ActiveButton:{2}", x, y, _CurrentActiveButton);
            return base.HitTest(x, y, out controlID, out focused);
       }

        ////public override void Render(float timePassed)
        ////{
        ////    base.Render(timePassed);
        ////}

        ////public override void OnAction(Action action)
        ////{
        ////    Console.WriteLine("ActionType: {0}", action.wID.ToString());
        ////    base.OnAction(action);
        ////}

        //////public override bool OnMessage(GUIMessage message)
        ////public override bool OnMessage(GUIMessage message)
        ////{
        ////    Console.WriteLine("Message: {0}", message.Message.ToString());
        ////    return base.OnMessage(message);
        ////}

		#endregion Methods
    }
}