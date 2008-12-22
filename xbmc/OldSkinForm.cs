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

using System.IO;
using System.Xml;
using MediaPortal.Util;
using MediaPortal.Configuration;

namespace MediaPortal
{
	/// <summary>
	/// Summary description for OldSkinForm.
	/// </summary>
	public class OldSkinForm : MediaPortal.UserInterface.Controls.MPForm
	{
		private MediaPortal.UserInterface.Controls.MPLabel label1;
		private MediaPortal.UserInterface.Controls.MPCheckBox checkBoxIgnoreMsg;
    private MediaPortal.UserInterface.Controls.MPButton bClose;
    private System.ComponentModel.IContainer components;
    private System.Windows.Forms.Timer CloseTimer;

    private int _nagCount = 0;
    private int _CloseInSeconds = 15;

		public OldSkinForm()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
      this.components = new System.ComponentModel.Container();
      this.label1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.checkBoxIgnoreMsg = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.bClose = new MediaPortal.UserInterface.Controls.MPButton();
      this.CloseTimer = new System.Windows.Forms.Timer(this.components);
      this.SuspendLayout();
      // 
      // label1
      // 
      this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label1.Location = new System.Drawing.Point(29, 19);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(291, 106);
      this.label1.TabIndex = 0;
      this.label1.Text = "The currently selected skin is outdated.\r\n\r\nThis will cause problems when using M" +
          "P!\r\n\r\n(Do NOT file bugreports using this skin)";
      // 
      // checkBoxIgnoreMsg
      // 
      this.checkBoxIgnoreMsg.AutoSize = true;
      this.checkBoxIgnoreMsg.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxIgnoreMsg.Location = new System.Drawing.Point(32, 131);
      this.checkBoxIgnoreMsg.Name = "checkBoxIgnoreMsg";
      this.checkBoxIgnoreMsg.Size = new System.Drawing.Size(177, 17);
      this.checkBoxIgnoreMsg.TabIndex = 0;
      this.checkBoxIgnoreMsg.Text = "Do not show this message again";
      this.checkBoxIgnoreMsg.UseVisualStyleBackColor = true;
      // 
      // bClose
      // 
      this.bClose.Location = new System.Drawing.Point(206, 128);
      this.bClose.Name = "bClose";
      this.bClose.Size = new System.Drawing.Size(114, 23);
      this.bClose.TabIndex = 1;
      this.bClose.Text = "Ignore and try (15) ...";
      this.bClose.UseVisualStyleBackColor = true;
      this.bClose.Click += new System.EventHandler(this.bClose_Click);
      // 
      // CloseTimer
      // 
      this.CloseTimer.Enabled = true;
      this.CloseTimer.Interval = 1000;
      this.CloseTimer.Tick += new System.EventHandler(this.CloseTimer_Tick);
      // 
      // OldSkinForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.BackColor = System.Drawing.Color.OrangeRed;
      this.ClientSize = new System.Drawing.Size(345, 163);
      this.Controls.Add(this.bClose);
      this.Controls.Add(this.checkBoxIgnoreMsg);
      this.Controls.Add(this.label1);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
      this.Location = new System.Drawing.Point(100, 70);
      this.Name = "OldSkinForm";
      this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
      this.Text = "Warning! Outdated skin!";
      this.ResumeLayout(false);
      this.PerformLayout();

		}
		#endregion

		private void bClose_Click(object sender, System.EventArgs e)
		{
      _nagCount++;
      using (MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
			{
        xmlwriter.SetValueAsBool("general", "dontshowskinversion", checkBoxIgnoreMsg.Checked);
        xmlwriter.SetValue("general", "skinobsoletecount", _nagCount);
			}
			this.Close();
		}
		
		public bool CheckSkinVersion(string skin)
		{
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
			{
				bool ignoreErrors=false;
				ignoreErrors=xmlreader.GetValueAsBool("general", "dontshowskinversion", false);
        _nagCount = xmlreader.GetValueAsInt("general", "skinobsoletecount", 0);
				if (ignoreErrors) return true;
			}
      checkBoxIgnoreMsg.Visible = _nagCount > 4 ? true : false;

			string versionBlue3Skin="";
			string versionSkin="";
			string filename= Config.GetFile(Config.Dir.Skin, "Blue3", "references.xml");
			if(File.Exists(filename))
			{	
				XmlDocument doc=new XmlDocument();
				doc.Load(filename);
				XmlNode node=doc.SelectSingleNode("/controls/skin/version");
				if (node!=null && node.InnerText!=null)
					versionBlue3Skin=node.InnerText;
			}
      filename = Config.GetFile(Config.Dir.Skin, skin, "references.xml");
			if(File.Exists(filename))
			{	
				XmlDocument doc=new XmlDocument();
				doc.Load(filename);
				XmlNode node=doc.SelectSingleNode("/controls/skin/version");
				if (node!=null && node.InnerText!=null)
					versionSkin=node.InnerText;
			}
			if (versionBlue3Skin==versionSkin) return true;
			return false;
		}

    private void CloseTimer_Tick(object sender, System.EventArgs e)
    {
      _CloseInSeconds--;
      if (_CloseInSeconds > 0)
      {
        bClose.Text = string.Format("Ignore and try ({0}) ...", _CloseInSeconds);
      }
      else
      {
        CloseTimer.Enabled = false;
        bClose_Click(sender, e);
      }
    }
	}
}
