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
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Windows.Forms;
using WaveLib;

namespace Yeti.MMedia

{
  /// <summary>
  /// 
  /// </summary>
  [Serializable]
  public class AudioWriterConfig : ISerializable

  {
    protected WaveFormat m_Format;


    /// <summary>
    /// A constructor with this signature must be implemented by descendants. 
    /// <see cref="System.Runtime.Serialization.ISerializable"/> for more information
    /// </summary>
    /// <param name="info">The <see cref="System.Runtime.Serialization.SerializationInfo"/> where is the serialized data.</param>
    /// <param name="context">The source (see <see cref="System.Runtime.Serialization.StreamingContext"/>) for this serialization.</param>
    protected AudioWriterConfig(SerializationInfo info, StreamingContext context)

    {
      int rate = info.GetInt32("Format.Rate");

      int bits = info.GetInt32("Format.Bits");

      int channels = info.GetInt32("Format.Channels");

      m_Format = new WaveFormat(rate, bits, channels);
    }


    public AudioWriterConfig(WaveFormat f)

    {
      m_Format = new WaveFormat(f.nSamplesPerSec, f.wBitsPerSample, f.nChannels);
    }


    public AudioWriterConfig()
      : this(new WaveFormat(44100, 16, 2)) {}


    [Browsable(false)]
    public WaveFormat Format

    {
      get { return m_Format; }

      set { m_Format = value; }
    }

    #region ISerializable Members

    public virtual void GetObjectData(SerializationInfo info, StreamingContext context)

    {
      info.AddValue("Format.Rate", m_Format.nSamplesPerSec);

      info.AddValue("Format.Bits", m_Format.wBitsPerSample);

      info.AddValue("Format.Channels", m_Format.nChannels);
    }

    #endregion
  }


  public interface IConfigControl

  {
    void DoApply();

    void DoSetInitialValues();

    /// <summary>
    /// Implementors must set [Browsable(false)] attribute to this property
    /// </summary>
    [Browsable(false)]
    Control ConfigControl { get; }

    /// <summary>
    /// Implementors must set [Browsable(false)] attribute to this property
    /// </summary>
    [Browsable(false)]
    string ControlName { get; }

    event EventHandler ConfigChange;
  }


  public interface IEditAudioWriterConfig : IConfigControl

  {
    /// <summary>
    /// Implementors must set [Browsable(false)] attribute to this property
    /// </summary>
    [Browsable(false)]
    AudioWriterConfig Config { get; set; }
  }


  public interface IEditFormat : IConfigControl

  {
    [Browsable(false)]
    WaveFormat Format { get; set; }
  }
}