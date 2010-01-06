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
using System.IO;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using Yeti.MMedia;
using WaveLib;


namespace Yeti.WMFSdk

{
  /// <summary>
  /// WmaWriterConfig is used to persist writer config. 
  /// </summary>
  [Serializable]
  public class WmaWriterConfig : Yeti.MMedia.AudioWriterConfig

  {
    protected string m_ProfileData;

    protected const string VALUE_NAME = "ProfileData";


    /// <summary>
    /// A constructor with this signature must be implemented by descendants. 
    /// <see cref="System.Runtime.Serialization.ISerializable"/> for more information
    /// </summary>
    /// <param name="info">The <see cref="System.Runtime.Serialization.SerializationInfo"/> where is the serialized data.</param>
    /// <param name="context">The source (see <see cref="System.Runtime.Serialization.StreamingContext"/>) for this serialization.</param>
    protected WmaWriterConfig(SerializationInfo info, StreamingContext context)
      : base(info, context)

    {
      m_ProfileData = info.GetString(VALUE_NAME);
    }


    /// <summary>
    /// Create an instance of WmaWriterConfig specifying the writer input format and the ouput profile
    /// </summary>
    /// <param name="format">Input data format</param>
    /// <param name="profile">Output profile</param>
    public WmaWriterConfig(WaveFormat format, IWMProfile profile)
      : base(format)

    {
      WMProfile prf = new WMProfile(profile);

      m_ProfileData = prf.ProfileData;
    }


    /// <summary>
    /// Used to serialize this class.
    /// </summary>
    /// <param name="info"><see cref="System.Runtime.Serialization.SerializationInfo"/></param>
    /// <param name="context"><see cref="System.Runtime.Serialization.StreamingContext"/></param>
    public override void GetObjectData(SerializationInfo info, StreamingContext context)

    {
      base.GetObjectData(info, context);

      info.AddValue(VALUE_NAME, m_ProfileData);
    }


    [Browsable(false)]
    public IWMProfile Profile

    {
      get

      {
        IWMProfile res;

        WM.ProfileManager.LoadProfileByData(m_ProfileData, out res);

        return res;
      }
    }
  }
}