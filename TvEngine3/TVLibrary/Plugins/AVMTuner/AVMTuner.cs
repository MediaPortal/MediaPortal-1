using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Web;
using System.Windows.Forms;
using MediaPortal.Playlists;
using SetupTv;
using TvControl;
using TvDatabase;
using TvLibrary.Channels;
using TvLibrary.Interfaces;
using TvLibrary.Log;
using UPnP.Infrastructure.CP;
using UPnP.Infrastructure.CP.Description;
using UPnP.Infrastructure.CP.DeviceTree;

namespace AVMTuner
{
  public partial class AVMTuner : SectionSettings
  {
    string PLAYLIST_NAME = "Fritz.DVBC.m3u";

    private UPnPNetworkTracker _networkTracker;
    private UPnPControlPoint _cp;
    private DeviceConnection _connection;
    private CpService _avmTunerService;
    private AvmProxy _avmProxy;
    private bool _isScanning;
    private int _cardNumber;
    private bool _stopScanning;
    private PlayList _playlist;
    private Dictionary<string, string> _tuningUrls;

    private class DescriptorBag
    {
      public RootDescriptor RootDescriptor;
      public DeviceDescriptor DeviceDescriptor;
    }
    public AVMTuner()
    {
      InitializeComponent();
    }

    public override void OnSectionActivated()
    {
      base.OnSectionActivated();
      StartDetection();
    }

    public override void OnSectionDeActivated()
    {
      base.OnSectionDeActivated();
      CloseConnection();
    }

    private void StartDetection()
    {
      if (_cp != null)
        return;

      CPData cpData = new CPData();
      _networkTracker = new UPnPNetworkTracker(cpData);
      _networkTracker.RootDeviceRemoved += NetworkTrackerOnRootDeviceRemoved;
      _networkTracker.RootDeviceAdded += NetworkTrackerOnRootDeviceAdded;
      _networkTracker.Start();
      _cp = new UPnPControlPoint(_networkTracker);
      _cp.Start();
    }

    private void OnUPnPDeviceDisconnected(DeviceConnection connection)
    {

    }


    private void NetworkTrackerOnRootDeviceRemoved(RootDescriptor rootdescriptor)
    {
      List<ListViewItem> toRemove =
        (from ListViewItem item in listDevices.Items
         let bag = item.Tag as DescriptorBag
         where bag != null && bag.RootDescriptor == rootdescriptor
         select item).ToList();

      toRemove.ForEach(listDevices.Items.Remove);
    }

    private void NetworkTrackerOnRootDeviceAdded(RootDescriptor rootDescriptor)
    {
      DeviceDescriptor rootDeviceDescriptor = DeviceDescriptor.CreateRootDeviceDescriptor(rootDescriptor);
      if (rootDeviceDescriptor.TypeVersion_URN != "urn:ses-com:device:SatIPServer:1")
        return;

      DeviceDescriptor deviceDescriptor = rootDeviceDescriptor.FindFirstDevice("ses-com:device:SatIPServer", 1);
      if (deviceDescriptor == null)
        return;

      ListViewItem deviceItem = new ListViewItem(rootDeviceDescriptor.FriendlyName);
      deviceItem.Tag = new DescriptorBag { RootDescriptor = rootDescriptor, DeviceDescriptor = deviceDescriptor };
      listDevices.Items.Add(deviceItem);
      // Auto select
      if (listDevices.SelectedItem == null)
      {
        listDevices.SetSelected(0, true);
        listDevices_SelectedIndexChanged(listDevices, EventArgs.Empty);
      }
    }

    private void CombineM3Us(List<string> m3uLists)
    {
      List<string> playListLines = new List<string>();
      using (var webClient = new WebClient())
      {
        foreach (var m3UList in m3uLists)
        {
          string playlist = Encoding.UTF8.GetString(webClient.DownloadData(m3UList));
          string line;
          using (var sr = new StringReader(playlist))
          {
            while ((line = sr.ReadLine()) != null)
            {
              playListLines.Add(line);
            }
          }
        }
      }

      Dictionary<string, HashSet<int>> transponderUrls = new Dictionary<string, HashSet<int>>();
      // Key is the Frequencey.pmtPID, which is the 6th pid in the source url
      _tuningUrls = new Dictionary<string, string>();
      foreach (var rtspLine in playListLines.Where(l => l.StartsWith("rtsp://", StringComparison.InvariantCultureIgnoreCase)))
      {
        var parts = rtspLine.Split(new[] { "&pids=" }, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 2)
        {
          if (!transponderUrls.ContainsKey(parts[0]))
            transponderUrls.Add(parts[0], new HashSet<int>());

          foreach (int pid in parts[1].Split(',').Select(int.Parse))
            transponderUrls[parts[0]].Add(pid);

          _tuningUrls.Add(GetUrlKey(rtspLine), rtspLine);
        }
      }

      List<string> combinedTransponderList = new List<string>();
      combinedTransponderList.Add("#EXTM3U");
      int i = 0;
      PlayList playList = new PlayList();
      foreach (var url in transponderUrls)
      {
        i++;
        string transponderName = string.Format("Transponder {0}", i);
        string transponderText = string.Format("#EXTINF:0,{0}", transponderName);
        var rtspUrl = string.Format("{0}&pids={1}", url.Key, string.Join(",", url.Value.ToArray()));

        combinedTransponderList.Add(transponderText);
        combinedTransponderList.Add(rtspUrl);
        playList.Add(new PlayListItem(transponderName, rtspUrl));
      }
      File.WriteAllLines(PLAYLIST_NAME, combinedTransponderList);

      _playlist = playList;
    }

    private string GetUrlKey(string rtspLine)
    {
      //# EXTINF:0,Das Erste
      //# EXTVLCOPT:network-caching=1000
      //rtsp://192.168.2.80:554/?freq=786&bw=8&msys=dvbc&mtype=64qam&sr=6900&specinv=1&pids=0,16,17,18,20,100,101,102,103,104,106,84,105,1176,2070,2171
      Uri myUri = new Uri(rtspLine);
      NameValueCollection parameters = HttpUtility.ParseQueryString(myUri.Query);
      int freq;
      if (int.TryParse(parameters["freq"], out freq))
      {
        var pidString = parameters["pids"];
        var pids = pidString.Split(',').Select(int.Parse).ToArray();
        if (pids.Length >= 6)
        {
          var pmtPid = pids[5];
          return string.Format("{0}.{1}", freq, pmtPid);
        }
      }
      return string.Empty;
    }

    private string GetFrequency(string rtspLine)
    {
      //# EXTINF:0,Das Erste
      //# EXTVLCOPT:network-caching=1000
      //rtsp://192.168.2.80:554/?freq=786&bw=8&msys=dvbc&mtype=64qam&sr=6900&specinv=1&pids=0,16,17,18,20,100,101,102,103,104,106,84,105,1176,2070,2171
      Uri myUri = new Uri(rtspLine);
      NameValueCollection parameters = HttpUtility.ParseQueryString(myUri.Query);
      int freq;
      return int.TryParse(parameters["freq"], out freq) ? freq.ToString() : string.Empty;
    }

    private void listDevices_SelectedIndexChanged(object sender, EventArgs args)
    {
      if (_isScanning)
        return;

      mpButtonScanTv.Enabled = false;
      lblTunerNumber.Text = "";
      var selectedItem = (ListViewItem)listDevices.SelectedItems[0];

      DescriptorBag bag = (DescriptorBag)selectedItem.Tag;
      if (bag == null)
        return;

      DeviceConnection connection;
      string deviceUuid = bag.DeviceDescriptor.DeviceUUID;
      try
      {
        CloseConnection();
        connection = _connection = _cp.Connect(bag.RootDescriptor, deviceUuid, null /*UPnPExtendedDataTypes.ResolveDataType*/);
      }
      catch (Exception e)
      {
        MessageBox.Show(string.Format("Error connecting the device {0}", deviceUuid));
        return;
      }
      connection.DeviceDisconnected += OnUPnPDeviceDisconnected;
      _avmTunerService = connection.Device.FindServiceByServiceId("urn:ses-com:serviceId:satip");
      if (_avmTunerService != null)
      {
        _avmProxy = new AvmProxy(_avmTunerService);
        mpButtonScanTv.Enabled = true;

        var tuners = _avmProxy.GetNumberOfTuners();
        lblTunerNumber.Text = tuners.ToString();
      }

    }

    private void CloseConnection()
    {
      if (_connection != null)
        _connection.Disconnect();
      _avmTunerService = null;
      _avmProxy = null;
      _connection = null;
    }

    private void btnImport_Click(object sender, EventArgs e)
    {
      if (_isScanning == false)
      {
        if (_avmProxy == null)
          return;

        List<string> m3uLists = new List<string>();
        var listHD = _avmProxy.GetChannelListHD();
        if (chkScanHD.Checked && !string.IsNullOrEmpty(listHD))
          m3uLists.Add(listHD);
        var listSD = _avmProxy.GetChannelListSD();
        if (chkScanSD.Checked && !string.IsNullOrEmpty(listSD))
          m3uLists.Add(listSD);
        var listRadio = _avmProxy.GetChannelListRadio();
        if (chkScanRadio.Checked && !string.IsNullOrEmpty(listRadio))
          m3uLists.Add(listRadio);

        if (m3uLists.Count == 0)
        {
          MessageBox.Show(this, "Please select at least one type of channels you want to import.");
          return;
        }

        CombineM3Us(m3uLists);

        //bool isUsed;
        //bool hasLock;
        //double signalPower;
        //double snr;
        //string channelName;
        //byte clientCount;
        //string ipAddresses;
        //var infos = _avmProxy.GetTunerInfos(0, out isUsed, out hasLock, out signalPower, out snr, out channelName, out clientCount, out ipAddresses);

        // Find DVB-IP cards for tuning
        IList<Card> dbsCards = Card.ListAll().Where(c => RemoteControl.Instance.Type(c.IdCard) == CardType.DvbIP).ToList();
        if (dbsCards.Count == 0)
        {
          MessageBox.Show(this, "No DVB-IP tuner found. Please make sure you set the number of DVB-IP tuners inside TV card setup at least to one.");
          return;
        }

        _cardNumber = dbsCards.First().IdCard;

        TvBusinessLayer layer = new TvBusinessLayer();
        Card card = layer.GetCardByDevicePath(RemoteControl.Instance.CardDevice(_cardNumber));
        if (card.Enabled == false)
        {
          MessageBox.Show(this, "Tuner is disabled. Please enable the tuner before scanning.");
          return;
        }
        else if (!RemoteControl.Instance.CardPresent(card.IdCard))
        {
          MessageBox.Show(this, "Tuner is not found. Please make sure the tuner is present before scanning.");
          return;
        }
        // Check if the card is locked for scanning.
        IUser user;
        if (RemoteControl.Instance.IsCardInUse(_cardNumber, out user))
        {
          MessageBox.Show(this, "Tuner is locked. Scanning is not possible at the moment. Perhaps you are using another part of a hybrid card?");
          return;
        }
        Thread scanThread = new Thread(DoScan);
        scanThread.Name = "DVB-IP scan thread";
        scanThread.Start();
      }
      else
      {
        _stopScanning = true;
      }
    }

    private void DoScan()
    {
      if (_playlist.Count == 0) return;

      int tvChannelsNew = 0;
      int radioChannelsNew = 0;
      int tvChannelsUpdated = 0;
      int radioChannelsUpdated = 0;

      IUser user = new User();
      user.CardId = _cardNumber;
      try
      {
        // First lock the card, because so that other parts of a hybrid card can't be used at the same time
        _isScanning = true;
        _stopScanning = false;
        mpButtonScanTv.Text = "Cancel...";
        grpTuningOptions.Enabled = false;
        RemoteControl.Instance.EpgGrabberEnabled = false;
        listViewStatus.Items.Clear();


        checkBoxCreateGroups.Enabled = false;
        TvBusinessLayer layer = new TvBusinessLayer();
        Card card = layer.GetCardByDevicePath(RemoteControl.Instance.CardDevice(_cardNumber));

        int index = -1;
        IEnumerator<PlayListItem> enumerator = _playlist.GetEnumerator();

        while (enumerator.MoveNext())
        {
          if (_stopScanning) return;
          index++;
          float percent = ((float)(index)) / _playlist.Count;
          percent *= 100f;
          if (percent > 100f) percent = 100f;
          progressBar1.Value = (int)percent;

          string url = enumerator.Current.FileName.Substring(enumerator.Current.FileName.LastIndexOf('\\') + 1);
          string name = enumerator.Current.Description;

          DVBIPChannel tuneChannel = new DVBIPChannel();
          tuneChannel.Url = url;
          tuneChannel.Name = name;
          string line = String.Format("{0}- {1} - {2}", 1 + index, tuneChannel.Name, tuneChannel.Url);
          ListViewItem item = listViewStatus.Items.Add(new ListViewItem(line));
          item.EnsureVisible();
          RemoteControl.Instance.Tune(ref user, tuneChannel, -1);
          IChannel[] channels;
          channels = RemoteControl.Instance.Scan(_cardNumber, tuneChannel);
          UpdateStatus();
          if (channels == null || channels.Length == 0)
          {
            if (RemoteControl.Instance.TunerLocked(_cardNumber) == false)
            {
              line = String.Format("{0}- {1} - {2}: No Signal", 1 + index, tuneChannel.Url, tuneChannel.Name);
              item.Text = line;
              item.ForeColor = Color.Red;
              continue;
            }
            else
            {
              line = String.Format("{0}- {1} - {2}: Nothing found", 1 + index, tuneChannel.Url, tuneChannel.Name);
              item.Text = line;
              item.ForeColor = Color.Red;
              continue;
            }
          }

          int newChannels = 0;
          int updatedChannels = 0;

          for (int i = 0; i < channels.Length; ++i)
          {
            Channel dbChannel;
            DVBIPChannel channel = (DVBIPChannel)channels[i];
            if (channels.Length > 1)
            {
              if (channel.Name.IndexOf("Unknown") == 0)
              {
                channel.Name = name + (i + 1);
              }
            }
            else
            {
              channel.Name = name;
            }
            bool exists;
            //Check if we already have this tuningdetail. According to DVB-IP specifications there are two ways to identify DVB-IP
            //services: one ONID + SID based, the other domain/URL based. At this time we don't fully and properly implement the DVB-IP
            //specifications, so the safest method for service identification is the URL. The user has the option to enable the use of
            //ONID + SID identification and channel move detection...
            var currentDetail = layer.GetTuningDetail(channel.NetworkId, channel.ServiceId, TvBusinessLayer.GetChannelType(channel));
            if (currentDetail == null)
            {
              //add new channel
              exists = false;
              dbChannel = layer.AddNewChannel(channel.Name, channel.LogicalChannelNumber);
              dbChannel.SortOrder = 10000;
              if (channel.LogicalChannelNumber >= 1)
              {
                dbChannel.SortOrder = channel.LogicalChannelNumber;
              }
              dbChannel.IsTv = channel.IsTv;
              dbChannel.IsRadio = channel.IsRadio;
              dbChannel.Persist();
            }
            else
            {
              exists = true;
              dbChannel = currentDetail.ReferencedChannel();
            }

            layer.AddChannelToGroup(dbChannel, TvConstants.TvGroupNames.AllChannels);

            if (checkBoxCreateGroups.Checked)
            {
              layer.AddChannelToGroup(dbChannel, channel.Provider);
            }

            // Replace the url by the correct channel url (tuning used full transponder with all channel pids)
            var pmtPid = channel.PmtPid;
            var freq = GetFrequency(url);
            string streamUrl;
            string urlKey = string.Format("{0}.{1}", freq, pmtPid);
            if (_tuningUrls.TryGetValue(urlKey, out streamUrl))
            {
              channel.Url = streamUrl;
            }
            else
            {
              item.Text = "Skipping channel that was not part of the playlist.";
              continue;
            }

            if (currentDetail == null)
            {
              layer.AddTuningDetails(dbChannel, channel);
            }
            else
            {
              //update tuning details...
              TuningDetail td = layer.UpdateTuningDetails(dbChannel, channel, currentDetail);
              td.Persist();
            }

            if (channel.IsTv)
            {
              if (exists)
              {
                tvChannelsUpdated++;
                updatedChannels++;
              }
              else
              {
                tvChannelsNew++;
                newChannels++;
              }
            }
            if (channel.IsRadio)
            {
              if (exists)
              {
                radioChannelsUpdated++;
                updatedChannels++;
              }
              else
              {
                radioChannelsNew++;
                newChannels++;
              }
            }
            layer.MapChannelToCard(card, dbChannel, false);
            line = String.Format("{0}- {1}: New:{2} Updated:{3}", 1 + index, tuneChannel.Name, newChannels, updatedChannels);
            item.Text = line;
          }
        }
        //DatabaseManager.Instance.SaveChanges();
      }
      catch (Exception ex)
      {
        Log.Write(ex);
      }
      finally
      {
        RemoteControl.Instance.StopCard(user);
        RemoteControl.Instance.EpgGrabberEnabled = true;
        progressBar1.Value = 100;
        _isScanning = false;
        mpButtonScanTv.Text = "Scan for channels";
        grpTuningOptions.Enabled = true;
      }
      ListViewItem lastItem = listViewStatus.Items.Add(new ListViewItem("Scan done..."));
      lastItem = listViewStatus.Items.Add(new ListViewItem(String.Format("Total radio channels new:{0} updated:{1}", radioChannelsNew, radioChannelsUpdated)));
      lastItem = listViewStatus.Items.Add(new ListViewItem(String.Format("Total tv channels new:{0} updated:{1}", tvChannelsNew, tvChannelsUpdated)));
      lastItem.EnsureVisible();
    }

    private void UpdateStatus()
    {

    }

    private void btnDetect_Click(object sender, EventArgs e)
    {
      if (_networkTracker == null || _networkTracker.SharedControlPointData == null ||
          _networkTracker.SharedControlPointData.SSDPController == null)
        return;

      _networkTracker.SharedControlPointData.SSDPController.SearchAll(null);
    }
  }
}
