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
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using MediaPortal.GUI.Library;
using MediaPortal.Dialogs;
using MediaPortal.Player;
using MediaPortal.Playlists;
using MediaPortal.Util;
using MediaPortal.TV.Recording;
using MediaPortal.TV.Database;
using MediaPortal.Video.Database;

using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Direct3D = Microsoft.DirectX.Direct3D;

namespace MediaPortal.GUI.Video
{
  /// <summary>
  /// Summary description for Class1.
  /// </summary>
    public class GUIDVDFullscreen : GUIWindow, ISetupForm, IShowPlugin
    {

    public GUIDVDFullscreen():base()
    {
      GetID = (int)GUIWindow.Window.WINDOW_DVD;
    }

    public override bool Init()
    {
        return true;
    }

    public override bool OnMessage(GUIMessage message)
        {
            Log.Write("GUIDVDFullscreen:message-{0}", message.Message.ToString());
            if (message.Message == GUIMessage.MessageType.GUI_MSG_WINDOW_INIT)
            {
                GUIWindowManager.ReplaceWindow((int)GUIWindow.Window.WINDOW_FULLSCREEN_VIDEO);
                if (!OnPlayDVD())
                {
                    GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT, this.GetID, 0, 0, GetID, 0, null);
                    return this.OnMessage(msg);	// Send a de-init msg
                }
                return true;
            }
            return base.OnMessage(message);
        }

        public override void Process()
        {
        }

        protected bool OnPlayDVD()
        {
            Log.Write("GUIDVDFullscreen playDVD");
            //check if dvd is inserted
            string[] drives = Environment.GetLogicalDrives();

            foreach (string drive in drives)
            {
                if (Util.Utils.getDriveType(drive) == 5) //cd or dvd drive
                {
                    string driverLetter = drive.Substring(0, 1);
                    string fileName = String.Format(@"{0}:\VIDEO_TS\VIDEO_TS.IFO", driverLetter);
                    if (System.IO.File.Exists(fileName))
                    {
                        IMDBMovie movieDetails = new IMDBMovie();
                        VideoDatabase.GetMovieInfo(fileName, ref movieDetails);
                        int idFile = VideoDatabase.GetFileId(fileName);
                        int idMovie = VideoDatabase.GetMovieId(fileName);
                        int timeMovieStopped = 0;
                        byte[] resumeData = null;
                        if ((idMovie >= 0) && (idFile >= 0))
                        {
                            timeMovieStopped = VideoDatabase.GetMovieStopTimeAndResumeData(idFile, out resumeData);
                            Log.Write("GUIDVDFullscreen::OnPlayBackStopped idFile={0} timeMovieStopped={1} resumeData={2}", idFile, timeMovieStopped, resumeData);
                            if (timeMovieStopped > 0)
                            {
                                string title = System.IO.Path.GetFileName(fileName);
                                VideoDatabase.GetMovieInfoById(idMovie, ref movieDetails);
                                if (movieDetails.Title != String.Empty) title = movieDetails.Title;

                                GUIDialogYesNo dlgYesNo = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_YES_NO);
                                if (null == dlgYesNo) return false;
                                dlgYesNo.SetHeading(GUILocalizeStrings.Get(900)); //resume movie?
                                dlgYesNo.SetLine(1, title);
                                dlgYesNo.SetLine(2, GUILocalizeStrings.Get(936) + Utils.SecondsToHMSString(timeMovieStopped));
                                dlgYesNo.SetDefaultToYes(true);
                                dlgYesNo.DoModal(GetID);

                                if (!dlgYesNo.IsConfirmed) timeMovieStopped = 0;
                            }
                        }

                        g_Player.PlayDVD();
                        if (g_Player.Playing && timeMovieStopped > 0)
                        {
                            if (g_Player.IsDVD)
                            {
                                g_Player.Player.SetResumeState(resumeData);
                            }
                            else
                            {
                                g_Player.SeekAbsolute(timeMovieStopped);
                            }
                        }
                        return true;
                    }
                }
            }
            //no disc in drive...
            GUIDialogOK dlgOk = (GUIDialogOK)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_OK);
            dlgOk.SetHeading(3);//my videos
            dlgOk.SetLine(1, 219);//no disc
            dlgOk.DoModal(GetID);
            return false;
        }
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
            return "My DVD";
        }

        public bool DefaultEnabled()
        {
            return false;
        }

        public int GetWindowId()
        {
            return GetID;
        }

        public bool GetHome(out string strButtonText, out string strButtonImage, out string strButtonImageFocus, out string strPictureImage)
        {
            strButtonText = GUILocalizeStrings.Get(341);
            strButtonImage = String.Empty;
            strButtonImageFocus = String.Empty;
            strPictureImage = "hover_my videos.png";
            return true;
        }

        public string Author()
        {
            return "Mosquiss";
        }

        public string Description()
        {
            return "Watch your DVD";
        }

        public void ShowPlugin()
        {
            // TODO:  Add GUIVideoFiles.ShowPlugin implementation
        }

        #endregion
        #region IShowPlugin Members

        public bool ShowDefaultHome()
        {
            return true;
        }

        #endregion


    }

}