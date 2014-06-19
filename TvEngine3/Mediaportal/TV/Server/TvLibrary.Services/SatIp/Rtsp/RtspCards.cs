using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVService.Interfaces;
using Mediaportal.TV.Server.TVService.Interfaces.Services;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;

namespace Mediaportal.TV.Server.TVLibrary.SatIp.Rtsp
{
  class RtspCards
  {
    private static Int32 counter = 0;
    // 8 is the number of maximal available slots. This number should be equal with the number defined in the filter
    // the defaul falue for a int array is "0" so we don't have to care about initalisation
    // 0 = slot unused; anything else = slot used
    private int[] slotMatrix = new int[8];
    private Int32 _id;
    private int _owner;
    private ArrayList _slaves = new ArrayList();
    private static int _streamCount = 0;
    private int _freq;
    private string _msys;
    private TuningDetail _tuningDetail;
    private IVirtualCard _card;
    private IUser _user;
    private string _devicePath;

    public RtspCards()
    {
      lock (this)
      {
        counter++;
        _id = counter;
        _streamCount = 1;
      }
    }

    /// <summary>
    /// Add the session id of a slave user who is not the owner of the card, but using this card to deliver its stream
    /// </summary>
    public void AddSlave(int sessionId)
    {
      lock (this)
      {
        _slaves.Add(sessionId);
        _streamCount++;
      }
    }

    /// <summary>
    /// remove a user from the card
    /// </summary>
    public void removeUser(int sessionId)
    {
      lock (this)
      {
        if (_owner == sessionId)
        {
          if (_slaves.Count == 0)
          {
            _owner = -1;
            return;
          }
          _owner = (int)_slaves[0];
          _slaves.Remove(_slaves[0]);
        }
        else
        {
          _slaves.Remove(sessionId);
        }

        for (int i = 0; i < slotMatrix.Length; ++i)
        {
          if (slotMatrix[i] == sessionId)
          {
            slotMatrix[i] = 0;  // free slot again
            break;
          }
        }
      }
    }

    public int getSlot(int sessionId)
    {
      lock (this)
      {
        for (int i = 0; i < slotMatrix.Length; ++i)
        {
          if (slotMatrix[i] == 0)
          {
            slotMatrix[i] = sessionId;
            return i;
          }
        }
      }

      this.LogError("SAT>IP: Error determining free slot for card on freq = {0}", _freq);
      return -1;
    }

    public int getNumberOfFreeSlots()
    {
      int count = 0;

      lock (this)
      {
        for (int i = 0; i < slotMatrix.Length; ++i)
        {
          if (slotMatrix[i] == 0)
          {
            ++count;
          }
        }
      }

      return count;
    }

    /// <summary>
    /// Get the id of the instance.
    /// </summary>
    public int id
    {
      get
      {
        return _id;
      }
    }
    
    /// <summary>
    /// Get/Set the owner session id.
    /// </summary>
    public int ownerId
    {
      get
      {
        return _owner;
      }
      set
      {
        _owner = value;
      }
    }

    /// <summary>
    /// Get the number of running streams from this card.
    /// </summary>
    public int streams
    {
      get
      {
        return _streamCount;
      }
    }

    /// <summary>
    /// Get/Set the devicepath of the card.
    /// </summary>
    public string devicePath
    {
      get
      {
        return _devicePath;
      }
      set
      {
        _devicePath = value;
      }
    }

    /// <summary>
    /// Get/Set the frequency the card is tuned to.
    /// </summary>
    public int freq
    {
      get
      {
        return _freq;
      }
      set
      {
        _freq = value;
      }
    }

    /// <summary>
    /// Get/Set the modulation system of the card.
    /// </summary>
    public string msys
    {
      get
      {
        return _msys;
      }
      set
      {
        _msys = value;
      }
    }

    /// <summary>
    /// Get/Set the tuning detail the card is tuned to.
    /// </summary>
    public TuningDetail tuningDetail
    {
      get
      {
        return _tuningDetail;
      }
      set
      {
        _tuningDetail = value;
      }
    }

    /// <summary>
    /// Get/Set the card handler of the card which is in use.
    /// </summary>
    public IVirtualCard card
    {
      get
      {
        return _card;
      }
      set
      {
        _card = value;
      }
    }

    /// <summary>
    /// Get/Set the TVE user which is the owner of the card from the point of the TvLibraray.
    /// </summary>
    public IUser user
    {
      get
      {
        return _user;
      }
      set
      {
        _user = value;
      }
    }

  }
}
