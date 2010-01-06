using System;
using MediaPortal.GUI.Library;
using Un4seen.Bass;
using Un4seen.Bass.Misc;

namespace MediaPortal.Player
{
  internal class StreamCopy : BaseDSP
  {
    private int _stream;
    private BASSBuffer _streamBuffer;
    private BASSFlag _streamFlags;

    public StreamCopy()
      : base() {}

    public StreamCopy(int channel, int priority)
      : base(channel, priority, IntPtr.Zero) {}

    public override void OnChannelChanged()
    {
      this.OnStopped();
      if (base.IsAssigned)
      {
        this.OnStarted();
      }
    }

    public override void OnStarted()
    {
      int channelBitwidth = base.ChannelBitwidth;
      switch (channelBitwidth)
      {
        case 0x20:
          this._streamFlags &= ~BASSFlag.BASS_SAMPLE_8BITS;
          this._streamFlags |= BASSFlag.BASS_SAMPLE_FLOAT;
          channelBitwidth = 4;
          break;

        case 8:
          this._streamFlags &= ~BASSFlag.BASS_SAMPLE_FLOAT;
          this._streamFlags |= BASSFlag.BASS_SAMPLE_8BITS;
          channelBitwidth = 1;
          break;

        default:
          this._streamFlags &= ~BASSFlag.BASS_SAMPLE_FLOAT;
          this._streamFlags &= ~BASSFlag.BASS_SAMPLE_8BITS;
          channelBitwidth = 2;
          break;
      }
      this._streamBuffer = new BASSBuffer(2f, base.ChannelSampleRate, base.ChannelNumChans, channelBitwidth);
      this._stream = Un4seen.Bass.Bass.BASS_StreamCreate(base.ChannelSampleRate, base.ChannelNumChans, this._streamFlags,
                                                         null, IntPtr.Zero);
      Un4seen.Bass.Bass.BASS_ChannelSetLink(base.ChannelHandle, this._stream);
      if (Un4seen.Bass.Bass.BASS_ChannelIsActive(base.ChannelHandle) == BASSActive.BASS_ACTIVE_PLAYING)
      {
        Bass.BASS_ChannelPlay(this._stream, false);
      }
    }

    public override void OnStopped()
    {
      Bass.BASS_ChannelRemoveLink(base.ChannelHandle, this._stream);
      Bass.BASS_StreamFree(this._stream);
      this._stream = 0;
      this.ClearBuffer();
    }

    public void ClearBuffer()
    {
      if (this._streamBuffer != null)
      {
        this._streamBuffer.Clear();
      }
    }

    public override void DSPCallback(int handle, int channel, IntPtr buffer, int length, IntPtr user)
    {
      try
      {
        this._streamBuffer.Write(buffer, length);
      }
      catch (Exception ex)
      {
        Log.Error("Caught Exception in DSPCallBack. {0}", ex.Message);
      }
    }

    public override string ToString()
    {
      return "StreamCopy";
    }

    // Properties
    public int Stream
    {
      get { return this._stream; }
    }

    public BASSFlag StreamFlags
    {
      get { return this._streamFlags; }
      set { this._streamFlags = value; }
    }
  }
}