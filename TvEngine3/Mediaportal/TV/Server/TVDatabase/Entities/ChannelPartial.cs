using System.Collections.Generic;
using System.Linq;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;

namespace Mediaportal.TV.Server.TVDatabase.Entities
{
  public partial class Channel
  {
    public override string ToString()
    {
      return DisplayName;
    }
    

    public bool IsIP
    {
      get
      {
        bool isIP = false;          
        foreach (ServiceDetail serviceDetail in ServiceDetails)
        {
          isIP = (serviceDetail is ServiceDvb && serviceDetail.TuningDetail is TuningDetailStream);
          if (isIP)
          {
            break;
          }                          
        }
        return isIP;
      }
    }

    public void GetEncrytionState(out bool isFree, out bool encrypted, out bool sometimesEncrypted)
    {
      isFree = false;
      encrypted = false;
      sometimesEncrypted = false;

      foreach (ServiceDetail detail in ServiceDetails)
      {
        if (detail.EncryptionScheme == (int)(EncryptionSchemeEnum.Free))
        {
          isFree = true;
        }
        else if (detail.EncryptionScheme == (int)(EncryptionSchemeEnum.Encrypted))
        {
          encrypted = true;
        }
        else if (detail.EncryptionScheme == (int)(EncryptionSchemeEnum.SometimesEncrypted))
        {
          sometimesEncrypted = true;
        }
      }
    }

    public bool IsWebstream
    {
      get
      {
        bool isWebstream = false;          
        /*foreach (ServiceDetail serviceDetail in ServiceDetails)
        {
          //isIP = (serviceDetail is ServiceDvb && serviceDetail.TuningDetail is TuningDetailStream);
          isWebstream = (serviceDetail is ??? && serviceDetail.TuningDetail is TuningDetailStream);

          if (isWebstream)
          {
            break;
          }                          
        }*/
        return isWebstream;
      }
    }

    //todo: rework for new tuningdetail structure
    /*
    public bool IsWebstream()
    {
      IList<TuningDetail> details = TuningDetails;
      if (details == null)
      {
        return false;
      }
      return details.Any(detail => detail.ChannelType == 5);
    }*/
  }
}
