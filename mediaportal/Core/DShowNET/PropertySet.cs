using System;
using System.Runtime.InteropServices;


namespace DShowNET
{
  public class IVac
  {
    static public readonly Guid IvacGuid=new Guid(0xd2185a40, 0x0398, 0x11d3, 0xa5, 0x3e, 0x0, 0xa0, 0xc9, 0xef, 0x50, 0x6a );
    public enum PropertyId
    {
      IVAC_OUTPUT_TYPE			= 0,		// Get & Set
      IVAC_BITRATE				= 1,		// Get & Set
      IVAC_VIDEO_INPUT_TYPE		= 2,		// Get & Set
      IVAC_VIDEO_RESOLUTION		= 3,		// Get & Set
      IVAC_TV_ENCODE_FORMAT		= 4,		// Get & Set
      IVAC_AUDIO_DATARATE			= 5,		// Get & Set
      IVAC_GOP_SIZE				= 6,		// Get & Set
      IVAC_CLOSED_GOP				= 7,		// Get & Set
      IVAC_SAMPLING_RATE			= 11,		// Get & Set
      IVAC_AUDIO_OUTPUT_MODE		= 12,		// Get & Set
      IVAC_AUDIO_CRC				= 13,		// Get & Set
      IVAC_CAPTURE_STATUS			= 16,		// Get only
      IVAC_RUN_STATUS				= 17,		// Get only
      IVAC_BOARD_DESCRIPTION		= 18,		// Get only
      IVAC_I2C_REGISTER_VALUE		= 20,		// Get & Set
      IVAC_TV_CHANNEL				= 21,		// Set only
      IVAC_GPIO_BIT_DIRECTION		= 22,		// Set only
      IVAC_GPIO_BYTE_DIRECTION 	= 23,		// Set only
      IVAC_GPIO_BIT_VALUE 		= 24,		// Set only
      IVAC_GPIO_BYTE_VALUE 		= 25,		// Set only
      IVAC_VBI_INFO				= 26,		// Set only
      IVAC_VERSION_INFO			= 27,		// Get only
      IVAC_GPIO_STATUS			= 28,		// Get only
      IVAC_I2C_INITIALIZE			= 34,		// Set only
      IVAC_GOP_STRUCTURE			= 39,		// Get & Set
      IVAC_TRICK_MODE				= 40,		// Get & Set
      IVAC_PREPARE_TO_STOP		= 41,		// Set Only
      IVAC_PREFILTER_SETTINGS     = 42,       // Get & Set
      IVAC_RUN_MODE				= 43,		// Set only _Shyam
      IVAC_INVERSE_TELECINE		= 50,		// Get & Set
      IVAC_VBI_LINE_INFO			= 51,		// Set Only _Shyam
      IVAC_ASPECT_RATIO			= 52,		// Set _Shyam
      IVAC_GPIO_WRITE_READ        = 53,       // Get & Set
      IVAC_TRANSPORT_OUTPUT_MODE	= 54,		// Set Only
      IVAC_DEVICE_STATE			= 55,		// Get & Set
      IVAC_SLOW_DECODER			= 56,		// Set only
      IVAC_FLUSH					= 57,		// Get only
      IVAC_NORMAL_PLAYMODE		= 58,		// set only
      IVAC_SET_NUMBER_BFRAMES		= 59,		// set only
      IVAC_OUTPUT_SIGNAL_STD		= 60,		// Get & Set
      IVAC_DONT_SENDAPI_STARTPLAY	= 61,		// Set only
      IVAC_VBI_INSERT_MODE		= 62,		// Set Only
      IVAC_BEGIN_CHANNEL_CHANGE	= 63,		// Set Only
      IVAC_END_CHANNEL_CHANGE		= 64,		// Set Only
      IVAC_CURRENT_FRAME_INFO		= 65,		// Get Only
      IVAC_DISPLAY_BUFFERS		= 66,		// Set Only
      IVAC_COPYPROTECTION			= 67,		// Set Only
      IVAC_PASS_THROUGH			= 68,		// Set Only
      IVAC_DEBUG					= 0xFF		// Set only
    } ;
  }

  enum KsPropertySupport:uint
  {
    Get=1,
    Set=2
  };


  // used by IKsPropertySet set AMPROPSETID_Pin
  enum AmPropertyPin:int
    {
      AMPROPERTY_PIN_CATEGORY,
      AMPROPERTY_PIN_MEDIUM
    } ;

  [ComVisible(true), ComImport,
  Guid("31EFAC30-515C-11d0-A9AA-00AA0061BE93"),
  InterfaceType( ComInterfaceType.InterfaceIsIUnknown )]
  public interface IKsPropertySet
  {
    [PreserveSig]
    int RemoteSet([In] ref Guid guidPropSet, 
                  [In] uint dwPropID, 
                  [In] IntPtr pInstanceData, 
                  [In] uint cbInstanceData, 
                  [In] IntPtr pPropData, 
                  [In] uint cbPropData);
    [PreserveSig]
    int RemoteGet([In] ref Guid guidPropSet, 
                  [In] uint dwPropID, 
                  [In] IntPtr pInstanceData, 
                  [In] uint cbInstanceData, 
                  [In]  IntPtr pPropData, 
                  [In] uint cbPropData, 
                  out uint pcbReturned);
    [PreserveSig]
    int QuerySupported([In] ref Guid guidPropSet, [In] uint dwPropID, out uint pTypeSupport);
  }
 


}
