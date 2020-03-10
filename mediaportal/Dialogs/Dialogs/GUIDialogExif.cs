#region Copyright (C) 2005-2020 Team MediaPortal

// Copyright (C) 2005-2020 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

using System.IO;

using MediaPortal.GUI.Library;
using MediaPortal.GUI.Pictures;
using MediaPortal.Picture.Database;
using MediaPortal.Util;

using Microsoft.DirectX.Direct3D;

namespace MediaPortal.Dialogs
{
  /// <summary>
  /// Shows a dialog box with an OK button  
  /// </summary>
  public class GUIDialogExif : GUIDialogWindow
  {
    [SkinControl(2)] protected GUILabelControl lblHeading = null;
    [SkinControl(3)] protected GUIImage imgPicture = null;
    [SkinControl(20)] protected GUIControl lblImgTitle = null;
    [SkinControl(21)] protected GUIControl lblImgDimensions = null;
    [SkinControl(22)] protected GUIControl lblResolutions = null;
    [SkinControl(23)] protected GUIControl lblFlash = null;
    [SkinControl(24)] protected GUIControl lblMeteringMode = null;
    [SkinControl(25)] protected GUIControl lblExposureCompensation = null;
    [SkinControl(26)] protected GUIControl lblShutterSpeed = null;
    [SkinControl(27)] protected GUIControl lblDateTakenLabel = null;
    [SkinControl(28)] protected GUIControl lblFstop = null;
    [SkinControl(29)] protected GUIControl lblExposureTime = null;
    [SkinControl(30)] protected GUIControl lblCameraModel = null;
    [SkinControl(31)] protected GUIControl lblEquipmentMake = null;
    [SkinControl(32)] protected GUIControl lblViewComments = null;

    private int m_iTextureWidth, m_iTextureHeight;
    private string fileName;
    private Texture m_pTexture = null;

    public GUIDialogExif()
    {
      GetID = (int)Window.WINDOW_DIALOG_EXIF;
    }

    public override bool Init()
    {
      return Load(GUIGraphicsContext.GetThemedSkinFile(@"\DialogPictureInfo.xml"));
    }

    public override bool OnMessage(GUIMessage message)
    {
      switch (message.Message)
      {
        case GUIMessage.MessageType.GUI_MSG_WINDOW_INIT:
          {
            base.OnMessage(message);
            Update();
            return true;
          }
        case GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT:
          {
            if (m_pTexture != null)
            {
              m_pTexture.Dispose();
            }
            m_pTexture = null;
            base.OnMessage(message);
            // Fix for Mantis issue: 0001709: Background not correct after viewing pictures properties twice
            Restore();
            return true;
          }
      }
      return base.OnMessage(message);
    }

    public void SetHeading(string strLine)
    {
      //LoadSkin();
      AllocResources();
      InitControls();

      lblHeading.Label = strLine;
    }

    public void SetHeading(int iString)
    {
      if (iString == 0)
      {
        SetHeading(string.Empty);
      }
      else
      {
        SetHeading(GUILocalizeStrings.Get(iString));
      }
    }

    public string FileName
    {
      get { return fileName; }
      set { fileName = value; }
    }

    private void setLabel(GUIControl control, string value, bool translate = false)
    {
      if (translate && !string.IsNullOrEmpty(value))
      {
        value = value.ToValue() ?? value;
      }

      var cf = control as GUIFadeLabel;
      if (cf != null) cf.Label = value;
      var cl = control as GUILabelControl;
      if (cl != null) cl.Label = value;
    }

    private void Update()
    {
      if (m_pTexture != null)
      {
        m_pTexture.Dispose();
      }

      setLabel(lblCameraModel, string.Empty);
      setLabel(lblDateTakenLabel, string.Empty);
      setLabel(lblEquipmentMake, string.Empty);
      setLabel(lblExposureCompensation, string.Empty);
      setLabel(lblExposureTime, string.Empty);
      setLabel(lblFlash, string.Empty);
      setLabel(lblFstop, string.Empty);
      setLabel(lblImgDimensions, string.Empty);
      setLabel(lblImgTitle, string.Empty);
      setLabel(lblMeteringMode, string.Empty);
      setLabel(lblResolutions, string.Empty);
      setLabel(lblShutterSpeed, string.Empty);
      setLabel(lblViewComments, string.Empty);

      if (!File.Exists(FileName))
      {
        GUIPropertyManager.SetProperty("#selectedthumb", string.Empty);
        return;
      }

      int iRotate = PictureDatabase.GetRotation(FileName);
      m_pTexture = Util.Picture.Load(FileName, iRotate, (int)Thumbs.LargeThumbSize.uhd, (int)Thumbs.LargeThumbSize.uhd, 
                                     true, false, out m_iTextureWidth, out m_iTextureHeight);

      ExifMetadata.Metadata metaData = PictureDatabase.GetExifFromDB(FileName);
      if (metaData.IsEmpty())
      {
        metaData = PictureDatabase.GetExifFromFile(FileName);
      }
      if (!metaData.IsEmpty())
      {
        setLabel(lblCameraModel, metaData.CameraModel.DisplayValue);
        setLabel(lblDateTakenLabel, metaData.DatePictureTaken.DisplayValue);
        setLabel(lblEquipmentMake, metaData.EquipmentMake.DisplayValue);
        setLabel(lblExposureCompensation, metaData.ExposureCompensation.DisplayValue);
        setLabel(lblExposureTime, metaData.ExposureTime.DisplayValue);
        setLabel(lblFlash, metaData.Flash.DisplayValue, true);
        setLabel(lblFstop, metaData.Fstop.DisplayValue);
        setLabel(lblImgDimensions, metaData.ImageDimensionsAsString());
        setLabel(lblImgTitle, Path.GetFileNameWithoutExtension(FileName));
        setLabel(lblMeteringMode, metaData.MeteringMode.DisplayValue, true);
        setLabel(lblResolutions, metaData.ResolutionAsString());
        setLabel(lblShutterSpeed, metaData.ShutterSpeed.DisplayValue);
        setLabel(lblViewComments, metaData.ViewerComments.DisplayValue);

        imgPicture.IsVisible = false;
      }
      metaData.SetExifProperties();
      GUIPropertyManager.SetProperty("#selectedthumb", FileName);
    }

    public override void Render(float timePassed)
    {
      base.Render(timePassed);
      if (null == m_pTexture)
      {
        return;
      }
      float x = imgPicture.XPosition;
      float y = imgPicture.YPosition;
      int width;
      int height;
      GUIGraphicsContext.Correct(ref x, ref y);

      GUIFontManager.Present();
      GUIGraphicsContext.GetOutputRect(m_iTextureWidth, m_iTextureHeight, imgPicture.Width, imgPicture.Height, 
                                       out width, out height);
      Util.Picture.RenderImage(m_pTexture, (int)x, (int)y, width, height, m_iTextureWidth, m_iTextureHeight, 
                               0, 0, true);
    }
  }
}