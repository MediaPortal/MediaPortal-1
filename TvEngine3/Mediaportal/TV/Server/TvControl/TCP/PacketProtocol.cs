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
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;

namespace Mediaportal.TV.Server.TVControl.TCP
{
  /// <summary>
  /// Maintains the necessary buffers for applying a length-prefix message framing protocol over a stream.
  /// </summary>
  /// <remarks>
  /// <para>Create one instance of this class for each incoming stream, and assign a handler to <see cref="MessageArrived"/>. As bytes arrive at the stream, pass them to <see cref="DataReceived"/>, which will invoke <see cref="MessageArrived"/> as necessary.</para>
  /// <para>If <see cref="DataReceived"/> raises <see cref="System.Net.ProtocolViolationException"/>, then the stream data should be considered invalid. After that point, no methods should be called on that <see cref="PacketProtocol"/> instance.</para>
  /// <para>This class uses a 4-byte signed integer length prefix, which allows for message sizes up to 2 GB. Keepalive messages are supported as messages with a length prefix of 0 and no message data.</para>
  /// <para>This is EXAMPLE CODE! It is not particularly efficient; in particular, if this class is rewritten so that a particular interface is used (e.g., Socket's IAsyncResult methods), some buffer copies become unnecessary and may be removed.</para>
  /// </remarks>
  public class PacketProtocol
  {
    #region private vars

    private readonly object _lockBuffer = new object();

    /// <summary>
    /// The buffer for the length prefix; this is always 4 bytes long.
    /// </summary>
    private readonly byte[] _lengthBuffer;

    /// <summary>
    /// The buffer for the data; this is null if we are receiving the length prefix buffer.
    /// </summary>
    private byte[] _dataBuffer;

    /// <summary>
    /// The number of bytes already read into the buffer (the length buffer if <see cref="_dataBuffer"/> is null, otherwise the data buffer).
    /// </summary>
    private int _bytesReceived;

    /// <summary>
    /// The maximum size of messages allowed.
    /// </summary>
    private readonly int _maxMessageSize;

    #endregion
    
    #region public static helper methods

    /// <summary>
    /// Wraps a message. The wrapped message is ready to send to a stream.
    /// </summary>
    /// <remarks>
    /// <para>Generates a length prefix for the message and returns the combined length prefix and message.</para>
    /// </remarks>
    /// <param name="message">The message to send.</param>
    public static byte[] WrapMessage(byte[] message)
    {
      // Get the length prefix for the message
      byte[] lengthPrefix = BitConverter.GetBytes(message.Length);

      // Concatenate the length prefix and the message
      byte[] ret = new byte[lengthPrefix.Length + message.Length];
      lengthPrefix.CopyTo(ret, 0);
      message.CopyTo(ret, lengthPrefix.Length);

      return ret;
    }

    /// <summary>
    /// Wraps a keepalive (0-length) message. The wrapped message is ready to send to a stream.
    /// </summary>
    public static byte[] WrapKeepaliveMessage()
    {
      return BitConverter.GetBytes(0);
    }

    #endregion

    #region events & delegates

    /// <summary>
    /// Indicates the completion of a message read from the stream.
    /// </summary>
    /// <remarks>
    /// <para>This may be called with an empty message, indicating that the other end had sent a keepalive message. This will never be called with a null message.</para>
    /// <para>This event is invoked from within a call to <see cref="DataReceived"/>. Handlers for this event should not call <see cref="DataReceived"/>.</para>
    /// </remarks>
    //public Action<byte[]> MessageArrived { get; set; }

    public delegate void MessageArrivedDelegate(byte[] msg);
    public event MessageArrivedDelegate MessageArrived;

    #endregion

    #region public methods


    /// <summary>
    /// Initializes a new <see cref="PacketProtocol"/>, limiting message sizes to the given maximum size.
    /// </summary>
    /// <param name="maxMessageSize">The maximum message size supported by this protocol. This may be less than or equal to zero to indicate no maximum message size.</param>
    public PacketProtocol(int maxMessageSize)
    {
      ResetBuffer();
      // We allocate the buffer for receiving message lengths immediately
      _lengthBuffer = new byte[sizeof(int)];
      _maxMessageSize = maxMessageSize;
    }


    /// <summary>
    /// Notifies the <see cref="PacketProtocol"/> instance that incoming data has been received from the stream. This method will invoke <see cref="MessageArrived"/> as necessary.
    /// </summary>
    /// <remarks>
    /// <para>This method may invoke <see cref="MessageArrived"/> zero or more times.</para>
    /// <para>Zero-length receives are ignored. Many streams use a 0-length read to indicate the end of a stream, but <see cref="PacketProtocol"/> takes no action in this case.</para>
    /// </remarks>
    /// <param name="data">The data received from the stream. Cannot be null.</param>
    /// <exception cref="System.Net.ProtocolViolationException">If the data received is not a properly-formed message.</exception>
    public void DataReceived(byte[] data)
    {

      Log.Debug("PacketProtocol.DataReceived : len={0} - contents={1}", data.Length, BitConverter.ToString(data));
      

      // Process the incoming data in chunks, as the ReadCompleted requests it

      // Logically, we are satisfying read requests with the received data, instead of processing the
      // incoming buffer looking for messages.

      int i = 0;
      while (i != data.Length)
      {
        lock (_lockBuffer)
        {
          // Determine how many bytes we want to transfer to the buffer and transfer them
          int bytesAvailable = data.Length - i;
          if (_dataBuffer != null)
          {
            // We're reading into the data buffer
            int bytesRequested = _dataBuffer.Length - _bytesReceived;

            // Copy the incoming bytes into the buffer
            int bytesTransferred = Math.Min(bytesRequested, bytesAvailable);
            Array.Copy(data, i, _dataBuffer, _bytesReceived, bytesTransferred);
            i += bytesTransferred;

            // Notify "read completion"
            ReadCompleted(bytesTransferred);
          }
          else
          {            

            // We're reading into the length prefix buffer
            int bytesRequested = _lengthBuffer.Length - _bytesReceived;            

            // Copy the incoming bytes into the buffer
            int bytesTransferred = Math.Min(bytesRequested, bytesAvailable);
            Array.Copy(data, i, _lengthBuffer, _bytesReceived, bytesTransferred);
            i += bytesTransferred;

            int bufferSize = BitConverter.ToInt32(_lengthBuffer, 0);
            if (bufferSize > 0)
            {
              Log.Debug("PacketProtocol.DataReceived : new prefix buffer size found={0} - hex={1}", bufferSize, BitConverter.ToString(_lengthBuffer));
            }

            // Notify "read completion"
            ReadCompleted(bytesTransferred);
          }
        }
      }
      _bytesReceived = 0;      
    }

    #endregion

    #region private methods

    /// <summary>
    /// Called when a read completes. Parses the received data and calls <see cref="MessageArrived"/> if necessary.
    /// </summary>
    /// <param name="count">The number of bytes read.</param>
    /// <exception cref="System.Net.ProtocolViolationException">If the data received is not a properly-formed message.</exception>
    private void ReadCompleted(int count)
    {
      // Get the number of bytes read into the buffer
      _bytesReceived += count;

      if (_dataBuffer == null)
      {
        // We're currently receiving the length buffer

        if (_bytesReceived != sizeof(int))
        {
          // We haven't gotten all the length buffer yet: just wait for more data to arrive
        }
        else
        {
          // We've gotten the length buffer
          int length = BitConverter.ToInt32(_lengthBuffer, 0);

          // Sanity check for length < 0
          if (length < 0)
          {
            throw new System.Net.ProtocolViolationException("Message length is less than zero");
          }

          // Another sanity check is needed here for very large packets, to prevent denial-of-service attacks
          if (_maxMessageSize > 0 && length > _maxMessageSize)
          {
            throw new System.Net.ProtocolViolationException("Message length " +
                                                            length.ToString(
                                                              System.Globalization.CultureInfo.InvariantCulture) +
                                                            " is larger than maximum message size " +
                                                            _maxMessageSize.ToString(
                                                              System.Globalization.CultureInfo.InvariantCulture));
          }

          // Zero-length packets are allowed as keepalives
          if (length == 0)
          {            
            _bytesReceived = 0;
            if (MessageArrived != null)
            {
              MessageArrived(new byte[0]);
            }
          }
          else
          {
            // Create the data buffer and start reading into it
            _dataBuffer = new byte[length];
            _bytesReceived = 0;
          }
        }
      }
      else
      {
        if (_bytesReceived == _dataBuffer.Length)                          
        {
          Log.Debug("PacketProtocol.ReadCompleted entire packet found : len={0} - contents={1}", _dataBuffer.Length, BitConverter.ToString(_dataBuffer));
          // We've gotten an entire packet
          if (MessageArrived != null)
          {
            MessageArrived(_dataBuffer);
          }

          // Start reading the length buffer again
          ResetBuffer();
        }        
      }
    }

    private void ResetBuffer() 
    {
      _dataBuffer = null;
      _bytesReceived = 0;
    }

    #endregion
  }
}
