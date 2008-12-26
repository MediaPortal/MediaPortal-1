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

using System;
using System.Collections;
using System.Xml;

using SetupControls;

namespace SetupTv
{
  public partial class SectionSettings : System.Windows.Forms.UserControl
  {
    public SectionSettings()
    {
      Init();
      InitializeComponent();
    }

    public SectionSettings(string text)
    {
      Init(text);
    }

    private void Init()
    {
      AutoScroll = true;
    }
    private void Init(string text)
    {
      Init();
      Text = text;
    }
    public virtual void SaveSettings()
    {
    }

    public virtual void LoadSettings()
    {
    }

    public virtual void LoadWizardSettings(XmlNode node)
    {
    }


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
      SectionTreeNode sectionTreeNode = SettingsForm.SettingSections[name] as SectionTreeNode;

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
          SectionTreeNode treeNode = enumerator.Value as SectionTreeNode;

          if (treeNode != null)
          {
            Type sectionType = treeNode.Section.GetType();

            if (sectionType.Name.Equals(name))
            {
              sectionSettings = treeNode.Section;
              break;
            }
          }
        }
      }

      return sectionSettings;
    }


    public virtual void OnSectionActivated()
    {
    }

    public virtual void OnSectionDeActivated()
    {
    }

    public virtual bool CanActivate
    {
      get { return true; }
    }
  }
}
