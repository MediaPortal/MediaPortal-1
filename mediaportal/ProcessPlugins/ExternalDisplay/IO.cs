using System.Runtime.InteropServices;

namespace ProcessPlugins.ExternalDisplay
{
  /// <summary>
  /// This is a wrapper class for direct communication with IO ports.
  /// It wraps the methods in the interop assembly and exposes them in a more 
  /// developer friendly way.
  /// </summary>
  /// <remarks>
  /// This class implements the Visitor pattern.
  /// Use the static Port property to get access to the single instance
  /// </remarks>
  /// <author>JoeDalton</author>
  public class IO
  {
    private static IOPort m_Port = new IOPort();

    /// <summary>
    /// Provides access to the single instance
    /// </summary>
    /// <value>
    /// Gets the single instance of</value>
    public static IOPort Port
    {
      get { return m_Port; }
    }

    public class IOPort
    {
      internal IOPort()
      {
      }

      /* For sending to a port */
      [DllImport("inpout32.dll", EntryPoint="Out32")]
      private static extern void Output(uint _addres, int _value);
      /* For receiving from a port */
      [DllImport("inpout32.dll", EntryPoint="Inp32")]
      private static extern int Input(uint _addres);

      /// <summary>
      /// The indexer for this class
      /// </summary>
      /// <value>
      /// Reads or writes to the specified port address
      /// </value>
      public int this[uint _addres]
      {
        get { return Input(_addres); }
        set { Output(_addres,value); }
      }
    
    }
  }
}
