using System;
using System.Runtime.Serialization;

namespace XPBurn
{
  /// <summary>
  /// This is the base exception type from which all burner exceptions are thrown.  No other burner 
  /// specific exceptions are currently thrown.
  /// </summary>
  [Serializable]
  public class XPBurnException : ApplicationException
  {
    /// <summary>
    /// Creates the burner exception.
    /// </summary>
    public XPBurnException() {}

    /// <summary>
    /// Creates the burner exception with a message.
    /// </summary>
    /// <param name="message">The message that the dialog box containing the exception will display.</param>
    public XPBurnException(string message) : base(message) {}

    /// <summary>
    /// Creates the burner exception with a message and an inner exception.
    /// </summary>
    /// <param name="message">The message that the dialog box contianing the excpetion will display.</param>
    /// <param name="inner">The inner exception to be stored.</param>
    public XPBurnException(string message, Exception inner) : base(message, inner) {}

    /// <summary>
    /// Creates the burner exception with serialization info.
    /// </summary>
    /// <param name="info">The serialization info to be passed.</param>
    /// <param name="context">The streaming context to be passed.</param>
    public XPBurnException(SerializationInfo info, StreamingContext context) : base(info, context) {}
  }
}