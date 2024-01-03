#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
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

using System;
using MediaPortal.Services;
using MediaPortal.Profile;

namespace MediaPortal.Configuration.Sections
{
  public partial class MediaInfo : SectionSettings
  {
    public MediaInfo()
      : this("Media Info") {}

    public MediaInfo(string name)
      : base(name)
    {
      InitializeComponent();
    }

    public override void LoadSettings()
    {
      using (Settings xmlreader = new MPSettings())
      {
        this.mpCheckBoxEnBluray.Checked = xmlreader.GetValueAsBool("MediaInfo", "DatabaseEnabled", true);

        this.mpCheckBoxEnBluray.Checked = xmlreader.GetValueAsBool("MediaInfo", "EnableCachingForBluray", true);
        this.mpCheckBoxEnDVD.Checked = xmlreader.GetValueAsBool("MediaInfo", "EnableCachingForDVD", true);
        this.mpCheckBoxEnVideo.Checked = xmlreader.GetValueAsBool("MediaInfo", "EnableCachingForVideo", true);
        this.mpCheckBoxEnAudio.Checked = xmlreader.GetValueAsBool("MediaInfo", "EnableCachingForAudio", false);
        this.mpCheckBoxEnPicture.Checked = xmlreader.GetValueAsBool("MediaInfo", "EnableCachingForPicture", false);
        this.mpCheckBoxEnImage.Checked = xmlreader.GetValueAsBool("MediaInfo", "EnableCachingForImage", false);
        this.mpCheckBoxEnAudioCD.Checked = xmlreader.GetValueAsBool("MediaInfo", "EnableCachingForAudioCD", false);

        int iDays = xmlreader.GetValueAsInt("MediaInfo", "RecordLifeTime", 0);
        if (iDays > 0)
        {
          this.nudDays.Value = Math.Min(this.nudDays.Maximum, iDays);
          this.mpCheckBoxDeleteOlder.Checked = true;
        }
      }
    }

    public override void SaveSettings()
    {
      using (Settings xmlwriter = new MPSettings())
      {
        xmlwriter.SetValueAsBool("MediaInfo", "EnableCachingForBluray", this.mpCheckBoxEnBluray.Checked);
        xmlwriter.SetValueAsBool("MediaInfo", "EnableCachingForDVD", this.mpCheckBoxEnDVD.Checked);
        xmlwriter.SetValueAsBool("MediaInfo", "EnableCachingForVideo", this.mpCheckBoxEnVideo.Checked);
        xmlwriter.SetValueAsBool("MediaInfo", "EnableCachingForAudio", this.mpCheckBoxEnAudio.Checked);
        xmlwriter.SetValueAsBool("MediaInfo", "EnableCachingForPicture", this.mpCheckBoxEnPicture.Checked);
        xmlwriter.SetValueAsBool("MediaInfo", "EnableCachingForImage", this.mpCheckBoxEnImage.Checked);
        xmlwriter.SetValueAsBool("MediaInfo", "EnableCachingForAudioCD", this.mpCheckBoxEnAudioCD.Checked);
        xmlwriter.SetValue("MediaInfo", "RecordLifeTime", this.mpCheckBoxDeleteOlder.Checked ? (int)this.nudDays.Value : 0);
      }
    }

    private void mpButtonClear_Click(object sender, EventArgs e)
    {
      GlobalServiceProvider.Get<IMediaInfoService>().Clear();
    }

    private void mpCheckBoxDeleteOlder_CheckedChanged(object sender, EventArgs e)
    {
      this.nudDays.Enabled = this.mpCheckBoxDeleteOlder.Checked;
    }
  }
}