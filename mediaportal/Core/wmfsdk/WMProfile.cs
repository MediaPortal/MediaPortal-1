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
using System.Text;
using System.Runtime.InteropServices;


namespace Yeti.WMFSdk

{
  /// <summary>
  /// Helper class to encapsulate IWMProfile interface
  /// </summary>
  public class WMProfile

  {
    private IWMProfile m_Profile;


    /// <summary>
    /// WMProfile constructor
    /// </summary>
    /// <param name="profile">Profile object to wrap</param>
    public WMProfile(IWMProfile profile)

    {
      m_Profile = profile;
    }


    /// <summary>
    /// Wraps IWMProfile.GetStream
    /// </summary>
    /// <param name="index">Index of desired stream</param>
    /// <returns>IWMStreamConfig</returns>
    public IWMStreamConfig GetStream(int index)

    {
      IWMStreamConfig res;

      m_Profile.GetStream((uint)index, out res);

      return res;
    }


    /// <summary>
    /// Wraps IWMProfile.GetStreamByNumber
    /// </summary>
    /// <param name="number">Stream number</param>
    /// <returns>IWMStreamConfig</returns>
    public IWMStreamConfig GetStreamByNumber(int number)

    {
      IWMStreamConfig res;

      m_Profile.GetStreamByNumber((ushort)number, out res);

      return res;
    }


    /// <summary>
    /// Wraps IWMProfile.RemoveStream
    /// </summary>
    /// <param name="strconfig">IWMStreamConfig stream to remove</param>
    public void RemoveStream(IWMStreamConfig strconfig)

    {
      m_Profile.RemoveStream(strconfig);
    }


    /// <summary>
    /// Wraps IWMProfile.RemoveStreamByNumber
    /// </summary>
    /// <param name="number">Stream number to remove</param>
    public void RemoveStreamByNumber(int number)

    {
      m_Profile.RemoveStreamByNumber((ushort)number);
    }


    /// <summary>
    /// Wraps IWMProfile.ReconfigStream
    /// </summary>
    /// <param name="strconfig">IWMStreamConfig stream to reconfig</param>
    public void ReconfigStream(IWMStreamConfig strconfig)

    {
      m_Profile.ReconfigStream(strconfig);
    }


    /// <summary>
    /// Wrapped IWMProfile object
    /// </summary>
    public IWMProfile Profile

    {
      get { return m_Profile; }
    }


    /// <summary>
    /// Profile name. Wraps IWMProfile.GetName
    /// </summary>
    public string Name

    {
      get

      {
        uint len = 0;

        StringBuilder s;

        m_Profile.GetName(null, ref len);

        s = new StringBuilder((int)len);

        m_Profile.GetName(s, ref len);

        return s.ToString();
      }

      set { m_Profile.SetName(value); }
    }


    /// <summary>
    /// Profile description. Wraps IWMProfile.GetDescription
    /// </summary>
    public string Description

    {
      get

      {
        uint len = 0;

        StringBuilder s;

        m_Profile.GetDescription(null, ref len);

        s = new StringBuilder((int)len);

        m_Profile.GetName(s, ref len);

        return s.ToString();
      }

      set { m_Profile.SetDescription(value); }
    }


    /// <summary>
    /// String in XML representing the profile. Wraps IProfileManager.SaveProfile
    /// </summary>
    public string ProfileData

    {
      get

      {
        uint len = 0;

        StringBuilder s;

        WM.ProfileManager.SaveProfile(m_Profile, null, ref len);

        s = new StringBuilder((int)len);

        WM.ProfileManager.SaveProfile(m_Profile, s, ref len);

        return s.ToString();
      }
    }


    /// <summary>
    /// Number of streams in the profile. Wraps IWMProfile.GetStreamCount
    /// </summary>
    public uint StreamCount

    {
      get

      {
        uint res;

        m_Profile.GetStreamCount(out res);

        return res;
      }
    }
  }
}