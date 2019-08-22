using System;

namespace TsPacketChecker
{
  public class Descriptor
  {
    #region Private Fields
    private int _lastIndex = -1;
    private int _descriptorTag;
    private int _descriptorLength;
    private byte[] _descriptorData;
    private bool _isUndefined;
    #endregion
    #region Constructor
    #endregion
    #region Properties
    public virtual int Index
    {
      get
      {
        if (_lastIndex == -1)
        {
          throw (new InvalidOperationException());
        }
        return (_lastIndex);
      }
    }
    public int MinimumDescriptorLength { get => 2; }
    public int MaximumDescriptorLength { get => DescriptorLength + 2; }
    public int DescriptorLength { get => _descriptorLength; set => _descriptorLength = value; }
    public int DescriptorTag { get => _descriptorTag; set => _descriptorTag = value; }
    public byte[] DescriptorData { get => _descriptorData; set => _descriptorData = value; }
    public bool IsEmpty { get { return (DescriptorLength == 0); } }
    public bool IsUndefined { get => _isUndefined; set => _isUndefined = value; }
    #endregion
    #region Static
    public static Descriptor Instance(byte[] buffer, int index)
    {
      Descriptor descriptor = null;
      switch((int)buffer[index])
      {
        case 0x05: // Registration Descriptor
          descriptor = new RegistrationDescriptor();
          break;
        case 0x09: // Ca Descriptor
          descriptor = new CaDescriptor();
          break;
        case 0x0A: // ISO 639 Language
          descriptor = new ISO639LanguageDescriptor();
          break;
        case 0x40: // Network Name Descriptor
          descriptor = new NetworkNameDescriptor();
          break;
        case 0x41: // Service List Descriptor
          descriptor = new ServiceListDescriptor();
          break;
        case 0x42: // Stuffing Descriptor
          break;
        case 0x43: // Satellite Delivery System Descriptor
          descriptor = new SatelliteDeliverySystemDescriptor();
          break;
        case 0x44: // Cable Delivery System Descriptor
          descriptor = new CableDeliverySystemDescriptor();
          break;
        case 0x45: // VBI Data Descriptor
          break;
        case 0x46: // VBI Teletext Descriptor
          break;
        case 0x47: // Bouquet Name Descriptor
          descriptor = new BouquetNameDescriptor();
          break;
        case 0x48: // Service Descriptor
          descriptor = new ServiceDescriptor();
          break;
        case 0x49: // Country Availability Descriptor
          descriptor = new CountryAvailabilityDescriptor();
          break;
        case 0x4a: // Linkage Descriptor
          descriptor = new LinkageDescriptor();
          break;
        case 0x4b: // NVOD Reference Descriptor
          break;
        case 0x4c: // Time Shifted Service Descriptor
          break;
        case 0x4d: // Short Event Descriptor
          descriptor = new ShortEventDescriptor();
          break;
        case 0x4e: // Extended Event Descriptor
          descriptor = new ExtendedEventDescriptor();
          break;
        case 0x4f: // Time Shifted Event Descriptor        
          break;
        case 0x50: // Component Descriptor
          descriptor = new ComponentDescriptor();
          break;
        case 0x51: // Mosaic Descriptor 
          break;
        case 0x52: // Stream Identifier Descriptor
          descriptor = new StreamIdentifierDescriptor();
          break;
        case 0x53: // CA Identifier Descriptor
          descriptor = new CaIdentifierDescriptor();
          break;
        case 0x54: // Content Descriptor
          descriptor = new ContentDescriptor();
          break;
        case 0x55: // Parental Rating Descriptor
          descriptor = new ParentalRatingDescriptor();
          break;
        case 0x56: // Teletext Descriptor
          descriptor = new TeletextDescriptor();
          break;
        case 0x57: // Telephone Descriptor 
          break;
        case 0x58: // Local Time Offset Descriptor
          descriptor = new LocalTimeOffsetDescriptor();
          break;
        case 0x59: // Subtitling Descriptor
          descriptor = new SubtitlingDescriptor();
          break;
        case 0x5a: // Terrestrial Delivery System Descriptor
          descriptor = new TerrestrialDeliverySystemDescriptor();
          break;
        case 0x5b: // Multilingual Network Name Descriptor 
          break;
        case 0x5c: // Multilingual Bouquet Name Descriptor
          break;
        case 0x5d: // Multilingual Service Name Descriptor
          break;
        case 0x5e: // Multilingual Component Descriptor
          break;
        case 0x5f: // Private Data Specifier Descriptor
          descriptor = new PrivateDataSpecifierDescriptor();
          break;
        case 0x60: // Service Move Descriptor
          descriptor = new ServiceMoveDescriptor();
          break;
        case 0x61: // Short_smoothing_buffer_descriptor 
          break;
        case 0x62: // Frequency List Descriptor 
          break;
        case 0x63: // Partial Transport Stream Descriptor ( Only found in Partial Transport Streams) 
          break;
        case 0x64: // Data Broadcast Descriptor
          descriptor = new DataBroadcastDescriptor();
          break;
        case 0x65: // Scrambling Descriptor
          descriptor = new ScramblingDescriptor();
          break;
        case 0x66: // Data Broadcast Id Descriptor
          descriptor = new DataBroadcastIdDescriptor();
          break;
        case 0x67: // Transport Stream Descriptor (Only in the TSDT (Transport Streams Description Table).)
          break;
        case 0x68: // DSNG Descriptor (Only in the TSDT (Transport Streams Description Table).) 
          break;
        case 0x69: // PDC Descriptor 
          break;
        case 0x6a: // AC-3 Descriptor (see annex D)
          descriptor = new AC3Descriptor();
          break;
        case 0x6b: // Ancillary Data Descriptor
          break;
        case 0x6c: // Cell List Descriptor 
          break;
        case 0x6d: // Cell Frequency Link Descriptor 
          break;
        case 0x6e: // Announcement Support Descriptor 
          break;
        case 0x6f: // Application Signalling Descriptor (see [56])          
          break;
        case 0x70: // Adaptation Field Data Descriptor
          descriptor = new AdaptationFieldDataDescriptor();
          break;
        case 0x71: // Service Identifier Descriptor (see [15])
          descriptor = new ServiceIdentifierDescriptor();
          break;
        case 0x72: // Service Availability Descriptor
          descriptor = new ServiceAvailabilityDescriptor();
          break;
        case 0x73: // Default Authority Descriptor (ETSI TS 102 323 [13])
          descriptor = new DefaultAuthorityDescriptor();
          break;
        case 0x74: // Related Content Descriptor (ETSI TS 102 323 [13])          
          break;
        case 0x75: // TVA Id Descriptor (ETSI TS 102 323 [13]) 
          break;
        case 0x76: // Content Identifier Descriptor (ETSI TS 102 323 [13]) 
          break;
        case 0x77: // Time Slice Fec Identifier Descriptor (ETSI EN 301 192 [4]) (May also be located in the CAT (ISO/IEC 13818-1 [18]) and INT (ETSI TS 102 006 [11]).) 
          break;
        case 0x78: // ECM Repetition Rate Descriptor (ETSI EN 301 192 [4])          
          break;
        case 0x79: // S2 Satellite Delivery System Descriptor
          //descriptor = new S2DeliverySystemDescriptor();
          break;
        case 0x7a: // Enhanced AC-3 Descriptor (see annex D)
          descriptor = new EnhancedAC3Descriptor();
          break;
        case 0x7b: // DTS® Descriptor (see annex G)
          descriptor = new DTSDescriptor();
          break;
        case 0x7c: // AAC Descriptor (see annex H)
          descriptor = new AACDescriptor();
          break;
        case 0x7d: // XAIT Location Descriptor (see [i.3]) 
          break;
        case 0x7e: // FTA Content Management Descriptor 
          break;
        case 0x7f: // Extension Descriptor (See also clauses 6.3 and 6.4.)
          switch(buffer[index+2])
          {
            case 0x00: // Image Icon Descriptor 
              break;
            case 0x01: // Cpcm Delivery Signalling Descriptor (ETSI TS/TR 102 825 [46] and [i.4]) 
              break;
            case 0x02: // CP Descriptor (ETSI TS/TR 102 825 [46] and [i.4]) 
              break;
            case 0x03: // CP Identifier Descriptor  (ETSI TS/TR 102 825 [46] and [i.4]) 
              break;
            case 0x04: // T2 Delivery System Descriptor
              //descriptor = new T2DeliverySystemDescriptor();
              break;
            case 0x05: // SH Delivery System Descriptor 
              break;
            case 0x06: // Supplementary Audio Descriptor
              break;
            case 0x07: // Network Change Notify Descriptor 
              break;
            case 0x08: // Message Descriptor  
              break;
            case 0x09: // Target Region Descriptor  
              break;
            case 0x0a: // Target Region Name Descriptor
              break;
            case 0x0b: // Service Relocated Descriptor  
              break;
            case 0x0c: // XAIT PID Descriptor 
              break;
            case 0x0d: // C2 Delivery System Descriptor
              //descriptor = new C2DeliverySystemDescriptor();
              break;
            case 0x0e: // DTS-HD Audio Stream Descriptor (annex G)
              break;
            case 0x0f: // DTS Neural Descriptor (annex L) 
              break;
          }
          break;
        default:
          break;
      }
      if (descriptor == null)
      {
        descriptor = new Descriptor();
      }
      descriptor.DescriptorTag = (int)buffer[index];
      index++;
      descriptor.DescriptorLength = (int)buffer[index];
      index++;
      if (descriptor.DescriptorLength != 0)
        descriptor.Process(buffer, index);
      return (descriptor);     
    }    

    #endregion
    internal virtual void Process(byte[] buffer, int index)
    {
      _lastIndex = index;

      if (DescriptorLength != 0)
      {
        _descriptorData = Utils.GetBytes(buffer, index, DescriptorLength);
        _lastIndex += DescriptorLength;
      }
      _isUndefined = true;
    }
    
  }
  
}
