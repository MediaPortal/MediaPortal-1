using System;
using DirectShowLib.BDA;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channels;

namespace Mediaportal.TV.Server.SetupTV.Sections.Helpers
{
  [Serializable]
  public class Transponder : IComparable<Transponder>
  {
    public int CarrierFrequency; // frequency
    public Polarisation Polarisation; // polarisation 0=hori, 1=vert
    public int SymbolRate; // symbol rate
    public ModulationType Modulation = ModulationType.ModNotSet;
    public BinaryConvolutionCodeRate InnerFecRate = BinaryConvolutionCodeRate.RateNotSet;
    public Pilot Pilot = Pilot.NotSet;
    public RollOff Rolloff = RollOff.NotSet;

    public DVBSChannel toDVBSChannel
    {
      get
      {
        DVBSChannel tuneChannel = new DVBSChannel();
        tuneChannel.Frequency = CarrierFrequency;
        tuneChannel.Polarisation = Polarisation;
        tuneChannel.SymbolRate = SymbolRate;
        tuneChannel.ModulationType = Modulation;
        tuneChannel.InnerFecRate = InnerFecRate;
        //Grab the Pilot & Roll-off settings
        tuneChannel.Pilot = Pilot;
        tuneChannel.RollOff = Rolloff;
        return tuneChannel;
      }
    }

    public int CompareTo(Transponder other)
    {
      if (Polarisation < other.Polarisation)
        return 1;
      if (Polarisation > other.Polarisation)
        return -1;
      if (CarrierFrequency > other.CarrierFrequency)
        return 1;
      if (CarrierFrequency < other.CarrierFrequency)
        return -1;
      if (SymbolRate > other.SymbolRate)
        return 1;
      if (SymbolRate < other.SymbolRate)
        return -1;
      return 0;
    }

    public override string ToString()
    {
      return String.Format("{0} {1} {2} {3} {4}", CarrierFrequency, SymbolRate, Polarisation, Modulation, InnerFecRate);
    }
  }
}