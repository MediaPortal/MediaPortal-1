using System;
using System.Runtime.Serialization;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVService.Interfaces.Services;

namespace Mediaportal.TV.Server.TVDatabase.Presentation
{  
  [DataContract]
  public class StreamPresentation
  {
    [DataMember]
    private readonly Channel _channel;

    [DataMember]
    private readonly IUser _user;

    [DataMember]
    private readonly bool _isParked;

    [DataMember]
    private readonly double _parkedDuration;

    [DataMember]
    private readonly DateTime _parkedAt;

    [DataMember]
    private readonly bool _isRecording;

    [DataMember]
    private readonly bool _isTimeshifting;

    private readonly bool _isScrambled;



    public StreamPresentation(Channel channel, IUser user, bool isParked, bool isRecording, bool isTimeshifting, double parkedDuration, DateTime parkedAt, bool isScrambled)
    {
      _channel = channel;
      _user = user;
      _isParked = isParked;
      _isRecording = isRecording;
      _isTimeshifting = isTimeshifting;
      _parkedDuration = parkedDuration;
      _parkedAt = parkedAt;
      _isScrambled = isScrambled;
    }

    public Channel Channel
    {
      get { return _channel; }
    }

    public IUser User
    {
      get { return _user; }
    }

    public bool IsParked
    {
      get { return _isParked; }
    }

    public bool IsRecording
    {
      get { return _isRecording; }
    }

    public bool IsTimeshifting
    {
      get { return _isTimeshifting; }
    }

    public double ParkedDuration
    {
      get { return _parkedDuration; }
    }

    public DateTime ParkedAt
    {
      get { return _parkedAt; }
    }

    public bool IsScrambled
    {
      get { return _isScrambled; }
    }
  }
}
