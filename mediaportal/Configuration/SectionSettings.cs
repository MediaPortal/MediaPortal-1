#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
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
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Xml;

namespace MediaPortal.Configuration
{
  public partial class SectionSettings : UserControl
  {
    public readonly string Windows7Codec = "Microsoft DTV-DVD Video Decoder";

    public SectionSettings()
    {
      this.AutoScroll = true;
      InitializeComponent();
    }

    public SectionSettings(string text)
    {
      this.AutoScroll = true;
      Text = text;
    }


    public virtual void SaveSettings() {}

    public virtual void LoadSettings() {}

    public virtual void LoadWizardSettings(XmlNode node) {}


    /// <summary>
    /// Returns the current setting for the given setting name
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public virtual object GetSetting(string name)
    {
      return null;
    }

    public static SectionSettings GetSection(string name)
    {
      SectionSettings sectionSettings = null;
      SectionTreeNode sectionTreeNode = null;
      try
      {
        sectionTreeNode = new SectionTreeNode(SettingsForm.SettingSections[name].ConfigSection);
      }
      catch (KeyNotFoundException)
      {
        MessageBox.Show("Someone broke section handling specifying a non existing name for {0}!", name);
      }

      if (sectionTreeNode != null)
      {
        sectionSettings = sectionTreeNode.Section;
      }
      else
      {
        //
        // Failed to locate the specified section, loop through and try to match
        // a section against the type name instead, as this is the way the wizard names
        // its sections.
        //
        IDictionaryEnumerator enumerator = SettingsForm.SettingSections.GetEnumerator();

        while (enumerator.MoveNext())
        {
          sectionTreeNode = enumerator.Value as SectionTreeNode;

          if (sectionTreeNode != null)
          {
            Type sectionType = sectionTreeNode.Section.GetType();

            if (sectionType.Name.Equals(name))
            {
              sectionSettings = sectionTreeNode.Section;
              break;
            }
          }
        }
      }

      return sectionSettings;
    }


    public virtual void OnSectionActivated() {}

    public virtual void OnSectionDeActivated() {}

    public virtual bool CanActivate
    {
      get { return true; }
    }
  }
}