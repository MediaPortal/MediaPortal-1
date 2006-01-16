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
using System.Collections;
using MediaPortal.GUI.Library;
using MediaPortal.GUI.Pictures;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Direct3D = Microsoft.DirectX.Direct3D;
using MediaPortal.Picture.Database;

namespace MediaPortal.Dialogs
{
  /// <summary>
  /// Shows a dialog box with an OK button  
  /// </summary>
  public class GUIDialogExif : GUIWindow, IRenderLayer
  {
    [SkinControlAttribute(2)]
    protected GUILabelControl lblHeading = null;
    [SkinControlAttribute(3)]
    protected GUIImage imgPicture = null;
    [SkinControlAttribute(20)]
    protected GUILabelControl lblImgTitle = null;
    [SkinControlAttribute(21)]
    protected GUILabelControl lblImgDimensions = null;
    [SkinControlAttribute(22)]
    protected GUILabelControl lblResolutions = null;
    [SkinControlAttribute(23)]
    protected GUIFadeLabel lblFlash = null;
    [SkinControlAttribute(24)]
    protected GUIFadeLabel lblMeteringMode = null;
    [SkinControlAttribute(25)]
    protected GUIFadeLabel lblExposureCompensation = null;
    [SkinControlAttribute(26)]
    protected GUIFadeLabel lblShutterSpeed = null;
    [SkinControlAttribute(27)]
    protected GUILabelControl lblDateTakenLabel = null;
    [SkinControlAttribute(28)]
    protected GUILabelControl lblFstop = null;
    [SkinControlAttribute(29)]
    protected GUILabelControl lblExposureTime = null;
    [SkinControlAttribute(30)]
    protected GUIFadeLabel lblCameraModel = null;
    [SkinControlAttribute(31)]
    protected GUIFadeLabel lblEquipmentMake = null;
    [SkinControlAttribute(32)]
    protected GUILabelControl lblViewComments = null;

    #region Base Dialog Variables
    bool m_bRunning = false;
    int m_dwParentWindowID = 0;
    GUIWindow m_pParentWindow = null;
    #endregion

    int m_iTextureWidth, m_iTextureHeight;
    bool m_bPrevOverlay = true;
    string fileName;
    Texture m_pTexture = null;

    public GUIDialogExif()
    {
      GetID = (int)GUIWindow.Window.WINDOW_DIALOG_EXIF;
    }

    public override bool Init()
    {
      return Load(GUIGraphicsContext.Skin + @"\DialogPictureInfo.xml");
    }

    public override bool SupportsDelayedLoad
    {
      get { return true; }
    }
    public override void PreInit()
    {
    }


    public override void OnAction(Action action)
    {
      if (action.wID == Action.ActionType.ACTION_CLOSE_DIALOG || action.wID == Action.ActionType.ACTION_PREVIOUS_MENU || action.wID == Action.ActionType.ACTION_CONTEXT_MENU)
      {
        Close();
        return;
      }
      base.OnAction(action);
    }

    #region Base Dialog Members 

    void Close()
    {
      GUIWindowManager.IsSwitchingToNewWindow = true;
      lock (this)
      {
        GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT, GetID, 0, 0, 0, 0, null);
        OnMessage(msg);

        GUIWindowManager.UnRoute();
        m_pParentWindow = null;
        m_bRunning = false;
      }
      GUIWindowManager.IsSwitchingToNewWindow = false;
    }

    public void DoModal(int dwParentId)
    {
      m_dwParentWindowID = dwParentId;
      m_pParentWindow = GUIWindowManager.GetWindow(m_dwParentWindowID);
      if (null == m_pParentWindow)
      {
        m_dwParentWindowID = 0;
        return;
      }

      GUIWindowManager.IsSwitchingToNewWindow = true;
      GUIWindowManager.RouteToWindow(GetID);

      // active this window...
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_WINDOW_INIT, GetID, 0, 0, 0, 0, null);
      OnMessage(msg);

      GUIWindowManager.IsSwitchingToNewWindow = false;
      m_bRunning = true;
      while (m_bRunning && GUIGraphicsContext.CurrentState == GUIGraphicsContext.State.RUNNING)
      {
        GUIWindowManager.Process();
        System.Threading.Thread.Sleep(100);
      }
    }
    #endregion

    public override bool OnMessage(GUIMessage message)
    {
      switch (message.Message)
      {
        case GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT:
          {
            m_pParentWindow = null;
            m_bRunning = false;
            GUIGraphicsContext.Overlay = m_bPrevOverlay;
            FreeResources();
            DeInitControls();
            if (m_pTexture != null)
              m_pTexture.Dispose();
            m_pTexture = null;

            GUILayerManager.UnRegisterLayer(this);
            return true;
          }

        case GUIMessage.MessageType.GUI_MSG_WINDOW_INIT:
          {
            m_bPrevOverlay = GUIGraphicsContext.Overlay;
            base.OnMessage(message);
            GUIGraphicsContext.Overlay = false;
            Update();
            GUILayerManager.RegisterLayer(this, GUILayerManager.LayerType.Dialog);
          }
          return true;

        case GUIMessage.MessageType.GUI_MSG_CLICKED:
          {
            int iControl = message.SenderControlId;
            /*
              if ( GetControl((int)Controls.ID_BUTTON_YES) == null)
              {
                Close();
                return true;
              }*/
          }
          break;
      }

      return base.OnMessage(message);
    }



    public void SetHeading(string strLine)
    {
      LoadSkin();
      AllocResources();
      InitControls();

      lblHeading.Label = strLine;
    }

    public void SetHeading(int iString)
    {
      if (iString == 0) SetHeading(String.Empty);
      else SetHeading(GUILocalizeStrings.Get(iString));
    }


    public string FileName
    {
      get { return fileName; }
      set { fileName = value; }
    }


    void Update()
    {
      if (m_pTexture != null)
        m_pTexture.Dispose();


      PictureDatabase dbs = new PictureDatabase();
      int iRotate = dbs.GetRotation(FileName);

      m_pTexture = MediaPortal.Util.Picture.Load(FileName, iRotate, 512, 512, true, false, out m_iTextureWidth, out m_iTextureHeight);

      lblCameraModel.Label = String.Empty;
      lblDateTakenLabel.Label = String.Empty;
      lblEquipmentMake.Label = String.Empty;
      lblExposureCompensation.Label = String.Empty;
      lblExposureTime.Label = String.Empty;
      lblFlash.Label = String.Empty;
      lblFstop.Label = String.Empty;
      lblImgDimensions.Label = String.Empty;
      lblImgTitle.Label = String.Empty;
      lblMeteringMode.Label = String.Empty;
      lblResolutions.Label = String.Empty;
      lblShutterSpeed.Label = String.Empty;
      lblViewComments.Label = String.Empty;

      using (ExifMetadata extractor = new ExifMetadata())
      {
        ExifMetadata.Metadata metaData = extractor.GetExifMetadata(FileName);

        lblCameraModel.Label = metaData.CameraModel.DisplayValue;
        lblDateTakenLabel.Label = metaData.DatePictureTaken.DisplayValue;
        lblEquipmentMake.Label = metaData.EquipmentMake.DisplayValue;
        lblExposureCompensation.Label = metaData.ExposureCompensation.DisplayValue;
        lblExposureTime.Label = metaData.ExposureTime.DisplayValue;
        lblFlash.Label = metaData.Flash.DisplayValue;
        lblFstop.Label = metaData.Fstop.DisplayValue;
        lblImgDimensions.Label = metaData.ImageDimensions.DisplayValue;
        lblImgTitle.Label = System.IO.Path.GetFileNameWithoutExtension(FileName);
        lblMeteringMode.Label = metaData.MeteringMode.DisplayValue;
        lblResolutions.Label = metaData.Resolution.DisplayValue;
        lblShutterSpeed.Label = metaData.ShutterSpeed.DisplayValue;
        lblViewComments.Label = metaData.ViewerComments.DisplayValue;

        imgPicture.IsVisible = false;
      }
    }


    public override void Render(float timePassed)
    {
      base.Render(timePassed);
      if (null == m_pTexture) return;
      float x = (float)imgPicture.XPosition;
      float y = (float)imgPicture.YPosition;
      int width;
      int height;
      GUIGraphicsContext.Correct(ref x, ref y);

      GUIFontManager.Present();
      GUIGraphicsContext.GetOutputRect(m_iTextureWidth, m_iTextureHeight, imgPicture.Width, imgPicture.Height, out width, out height);
      MediaPortal.Util.Picture.RenderImage(ref m_pTexture, (int)x, (int)y, width, height, m_iTextureWidth, m_iTextureHeight, 0, 0, true);
    }


    #region IRenderLayer
    public bool ShouldRenderLayer()
    {
      return true;
    }

    public void RenderLayer(float timePassed)
    {
      Render(timePassed);
    }
    #endregion
  }
}
