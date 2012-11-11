namespace Mediaportal.TV.Server.TVDatabase.Entities
{
  public partial class LnbType 
  {
    public LnbType Clone()
    {
      var l = new LnbType
                {
                  IdLnbType = IdLnbType,
                  IsBandStacked = IsBandStacked,
                  IsToroidal = IsToroidal,
                  LowBandFrequency = LowBandFrequency,
                  Name = Name,
                  SwitchFrequency = SwitchFrequency,
                  HighBandFrequency = HighBandFrequency
                };


      return l;
    }
   
  }
}
