#region Copyright (C) 2005-2008 Team MediaPortal

/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
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

using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Threading;
using System.Windows.Forms;
using MediaPortal.Configuration;

namespace WebEPG_conf
{
  /// <summary>
  /// Summary description for Form1.
  /// </summary>
  public class fChannels : Form
  {
    private Panel panelSettings;
    private WebEPGConfigControl _settingsControl;

    public fChannels()
    {
      //
      // Required for Windows Form Designer support
      //
      InitializeComponent();
      Thread.CurrentThread.Name = "WebEPG Config";


      _settingsControl = new WebEPGConfigControl();
      panelSettings.Controls.Add(_settingsControl);
    }

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        //				if(components != null)
        //				{
        //					components.Dispose();
        //				}
      }
      base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.panelSettings = new System.Windows.Forms.Panel();
      this.SuspendLayout();
      // 
      // panel
      // 
      this.panelSettings.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.panelSettings.Location = new System.Drawing.Point(8, 12);
      this.panelSettings.Name = "panel";
      this.panelSettings.Size = new System.Drawing.Size(720, 500);
      this.panelSettings.TabIndex = 20;
      // 
      // fChannels
      // 
      this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
      this.ClientSize = new System.Drawing.Size(740, 524);
      this.Controls.Add(this.panelSettings);
      this.MaximizeBox = false;
      this.Name = "fChannels";
      this.Text = "WebEPG Config";
      this.ResumeLayout(false);
    }

    #endregion

    #region Main

    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    private static void Main()
    {
      NameValueCollection appSettings = ConfigurationManager.AppSettings;
      appSettings.Set("GentleConfigFile", Config.GetFile(Config.Dir.Config, "gentle.config"));

      Application.Run(new fChannels());
    }

    #endregion
  }
}