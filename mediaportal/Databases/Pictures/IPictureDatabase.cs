using System;
using System.Collections.Generic;
using System.Text;

using MediaPortal.Util;
using MediaPortal.GUI.Library;
using MediaPortal.GUI.Pictures;
using MediaPortal.Database;

namespace MediaPortal.Picture.Database
{
  public interface IPictureDatabase
  {
    int AddPicture(string strPicture, int iRotation);
    void DeletePicture(string strPicture);
    int GetRotation(string strPicture);
    void SetRotation(string strPicture, int iRotation);
    DateTime GetDateTaken(string strPicture);
    int EXIFOrientationToRotation(int orientation);
    void Dispose();
  }
}
