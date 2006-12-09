using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace MediaPortal.Player.DSP
{
  /// <summary>
  /// This class holds information about a BASS Effect
  /// </summary>
  [Serializable]
  public class BassEffect
  {
    /// <summary>
    /// The Name of the Effect
    /// </summary>
    [XmlAttribute]
    public string EffectName = String.Empty;

    /// <summary>
    /// List of Parameters for this Effect
    /// </summary>
    [XmlElement("Parameter", typeof(BassEffectParm))]
    public List<BassEffectParm> Parameter = new List<BassEffectParm>();
  }
}
