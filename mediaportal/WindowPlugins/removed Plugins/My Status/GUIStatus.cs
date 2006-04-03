/* 
 *	Copyright (C) 2005 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */

#region Usings
using System;
using System.Drawing;
using System.Windows.Forms;
using System.Management;
using System.Collections;
using MediaPortal.Util;
using MediaPortal.GUI.Library;
using MediaPortal.Dialogs;
using MediaPortal.Player;
using MediaPortal.Playlists;
using mbm5.MBMInfo;
#endregion

namespace MediaPortal.GUI.GUIStatus
{
  /// <summary>
  /// Summary description for GUIStatus.
  /// </summary>
  public class GUIStatus : GUIWindow 
  {
    public static int WINDOW_STATUS = 755;

    #region Private Enumerations
    enum Controls 
    {
      CONTROL_PREFERENCES	= 2,	  // preference button
      CONTROL_DETAIL		= 3,	  // status details
      CONTROL_NEXTPAGE		= 4,	  // next page
      CONTROL_STATUS		= 9,	  // status text
      CONTROL_1				= 10,
      CONTROL_STATUSBAR		= 9875	  // status bar
    };

    enum SensorTypes 
    {
      Volt			= 1,
      Fan			= 2,
      Percentage	= 3,
      Temperature	= 4,
      HD			= 5,
      Mhz			= 6,
      Ram			= 7
    };

    #endregion

    #region Private Variables 

    private struct Sensor 
    {
      public string name;
      public string sname;
      public int sensorNum;
      public int sensorType;
      public string drive;
      public string first;
      public string last;
      public bool alarm;
      public bool shutdown;
      public long max;
      public long min;
    }

    private string soundFolder="";
    private string sound="";
    private int playTime=0;
    private int maxSensors = 24;
    private int pageSensors = 12;
    private int numSensors = 0;
    private static Sensor[] sensors = new Sensor[42];
    private bool starttimer=false;
    private bool onWindow=false;
    private bool onDetails=false;
    private bool onStatus=false;
    private bool showStatusBar=false;
    private bool isMbm=false;
    private bool page1=true;
    private string statusBarSensor="";
    private bool showTopBottom=false;  // true=show status bar on top false=bottom
    private static bool alarm=false;
    private static bool shutdown=false;
    private static int selectedSensor=0;
    private static string name="";
    private static int delayPlay=6;
    private mbmSharedData MBMInfo=new mbmSharedData();
    private System.Windows.Forms.Timer statusTimer = new System.Windows.Forms.Timer();
    static bool	m_bForceShutdown = true;		// Force shutdown

    #endregion

    #region Constructor
    public GUIStatus() 
    {
      //
      // TODO: Add constructor logic here
      //
    }

    #endregion

    #region Overrides
		
    public override int GetID 
    {
      get { return WINDOW_STATUS; }
      set { base.GetID = value; }
    }

    public override bool Init() 
    {
      onWindow=false;
      onDetails=false;
      onStatus=false;
      //Log.Write("Start My Status");
      LoadSettings();
      InitializeStatusTimer();
      return Load (GUIGraphicsContext.Skin+@"\mystatus.xml");
    }

    public override void OnAction(Action action) 
    {
      if (action.wID == Action.ActionType.ACTION_PREVIOUS_MENU) 
      {
        onWindow=false;
        onDetails=false;
        onStatus=false;
				GUIWindowManager.ShowPreviousWindow();
        return;
      }
      base.OnAction(action);
    }
    public override bool OnMessage(GUIMessage message) 
    {
      switch ( message.Message ) 
      {  
        case GUIMessage.MessageType.GUI_MSG_WINDOW_INIT:
          base.OnMessage(message);
          onStatus=false;
          onWindow=true;
          onDetails=false;
          page1=true;
          ShowButtons();
          UpdateFields();
          LoadSound();
          GUIPropertyManager.SetProperty("#currentmodule", GUILocalizeStrings.Get(1950));
          return true;
        case GUIMessage.MessageType.GUI_MSG_CLICKED:
          //get sender control
          base.OnMessage(message);
          int iControl=message.SenderControlId;
          if (iControl>=(int)Controls.CONTROL_1 && iControl<=((int)Controls.CONTROL_1 + maxSensors)) 
          { 
            selectedSensor=iControl;
            alarm=sensors[selectedSensor-10].alarm;
            name=sensors[selectedSensor-10].name;
            onWindow=false;
            onDetails=true;
            onStatus=true;
            GUIWindowManager.ActivateWindow(GUIStatusDetails.WINDOW_STATUS_DETAILS);
            return true;
          }
          if (iControl==(int)Controls.CONTROL_PREFERENCES) // select preference page
          {
            onStatus=false;
            onWindow=false;
            onDetails=false;
            GUIWindowManager.ActivateWindow(GUIStatusPrefs.WINDOW_STATUS_PREFS);
            return true;
          }
          if (iControl==(int)Controls.CONTROL_NEXTPAGE) // select preference page
          {
            if(numSensors>12) 
            {
              if (page1==true) 
              {
                page1=false;
                HideButtons();
                ShowButtons();
              }
              else
              {
                page1=true;
                HideButtons();
                ShowButtons();
              }
            }
            return true;
          }
          if (iControl==(int)Controls.CONTROL_DETAIL) // select status details
          {
            if (onStatus==false)  // show textfield
            {
              onStatus=true;
              onWindow=false;
              onDetails=false;
              HideButtons();
              GUIControl.HideControl( GetID,(int)Controls.CONTROL_NEXTPAGE); 
              GUIControl.DisableControl( GetID,(int)Controls.CONTROL_NEXTPAGE);
              GUIControl.ShowControl( GetID,(int)Controls.CONTROL_STATUS); 
              GUIControl.EnableControl( GetID,(int)Controls.CONTROL_STATUS);
              GUIControl.SetControlLabel(GetID,(int)Controls.CONTROL_DETAIL,GUILocalizeStrings.Get(712));
              return true;
            } 
            else				  // hide textfield
            {
              onStatus=false;
              onWindow=true;
              onDetails=false;
              GUIControl.HideControl( GetID,(int)Controls.CONTROL_STATUS); 
              GUIControl.DisableControl( GetID,(int)Controls.CONTROL_STATUS);
              GUIControl.SetControlLabel(GetID,(int)Controls.CONTROL_DETAIL,GUILocalizeStrings.Get(1972));
              ShowButtons();
              return true;
            }
          }
          return true;
        case GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT:
          onWindow=false;
          break;
      }
      return base.OnMessage (message);
    }

    #endregion

    #region Private Methods

    private void HideButtons() // Switch status buttons off
    {
      for (int i=(int)Controls.CONTROL_1;i<((int)Controls.CONTROL_1+pageSensors);i++) 
      {
        GUIControl.HideControl( GetID,i );
        GUIControl.DisableControl( GetID,i );
      }
    }

    private void ShowButtons() // Switch status buttons on
    {
      int st;
      if (onStatus==false) 
      {
        GUIControl.HideControl( GetID,(int)Controls.CONTROL_STATUS); 
        GUIControl.DisableControl( GetID,(int)Controls.CONTROL_STATUS);
      }
      if(numSensors>12) 
      {
        GUIControl.ShowControl( GetID,(int)Controls.CONTROL_NEXTPAGE); 
        GUIControl.EnableControl( GetID,(int)Controls.CONTROL_NEXTPAGE);
      } 
      else 
      {
        GUIControl.HideControl( GetID,(int)Controls.CONTROL_NEXTPAGE); 
        GUIControl.DisableControl( GetID,(int)Controls.CONTROL_NEXTPAGE);
      }
      if (page1==true) 
      {
        GUIControl.SetControlLabel(GetID,(int)Controls.CONTROL_NEXTPAGE,GUILocalizeStrings.Get(1975));
        if(numSensors>12) 
        {
          st=(12+(int)Controls.CONTROL_1);
        }
        else
        {
          st=(numSensors+(int)Controls.CONTROL_1);
        }
      } 
      else 
      {
        GUIControl.SetControlLabel(GetID,(int)Controls.CONTROL_NEXTPAGE,GUILocalizeStrings.Get(712));
        st=((numSensors-12)+(int)Controls.CONTROL_1);
      }
      for (int i=(int)Controls.CONTROL_1; i<((int)Controls.CONTROL_1+pageSensors); i++) 
      {
        if (i<st) 
        {
          GUIControl.ShowControl( GetID,i); 
          GUIControl.EnableControl( GetID,i);
        } 
        else 
        {
          GUIControl.HideControl( GetID,i );
          GUIControl.DisableControl( GetID,i );
        }
      }
    }

    /// <summary>
    /// init 1 sec timer. 
    /// </summary>
    private void InitializeStatusTimer() 
    {
      statusTimer.Tick += new EventHandler(OnTimer);
      statusTimer.Interval = 1000;	  //1 sec Intervall
      statusTimer.Enabled=true;
      statusTimer.Start();
      starttimer=true;
    }

    private void OnTimer(Object sender, EventArgs e) 
    {
      if(sender == statusTimer) 
      {
        if (starttimer) UpdateFields();
      }
    }

    /// <summary>
    /// calculate KB,MB and GB View
    /// </summary>
    private string CalcExt(long m)
    {
      string lw="";
      if (m >= 10737418240) 
      {
        m = (m / (1024 * 1024 * 1024));
        lw=m.ToString()+" GB";
      } 
      else if (m >= 1048576 ) 
      {
        m = (m / (1024 * 1024));
        lw=m.ToString()+" MB";
      } 
      else if (m >= 1024 ) 
      {
        m = (m / 1024);
        lw=m.ToString()+" KB";
      }
      return lw;
    }
  
    /// <summary>
    /// get infos of a disk
    /// </summary>
    private string GetDiskInfo(string lw) 
    {
      ManagementObjectSearcher query;
      ManagementObjectCollection queryCollection;
      System.Management.ObjectQuery oq;
      string stringMachineName = "localhost";
      long s=0;
      long m=0;
      string f="";
      string str="";
      //Connect to the remote computer
      ConnectionOptions co = new ConnectionOptions();

      //Point to machine
      System.Management.ManagementScope ms = new System.Management.ManagementScope("\\\\" + stringMachineName + "\\root\\cimv2", co);
      oq = new System.Management.ObjectQuery("SELECT * FROM Win32_LogicalDisk WHERE DeviceID = '"+lw+"'");
      query = new ManagementObjectSearcher(ms,oq);
      queryCollection = query.Get();
      foreach ( ManagementObject mo in queryCollection) 
      {
        m=Convert.ToInt64(mo["FreeSpace"]);
        s=Convert.ToInt64(mo["Size"]);
        f=Convert.ToString(mo["FileSystem"]);
      }
      str=lw+" "+CalcExt(m)+" "+GUILocalizeStrings.Get(1973)+" "+CalcExt(s)+" "+GUILocalizeStrings.Get(1953)+"    "+GUILocalizeStrings.Get(1977)+": "+" "+f;
      return str;
    }

    /// <summary>
    /// get the size of a disk
    /// </summary>
    private long GetDiskSize(string lw) 
    {
      ManagementObjectSearcher query;
      ManagementObjectCollection queryCollection;
      System.Management.ObjectQuery oq;
      string stringMachineName = "localhost";
      long s=0;
      //Connect to the remote computer
      ConnectionOptions co = new ConnectionOptions();

      //Point to machine
      System.Management.ManagementScope ms = new System.Management.ManagementScope("\\\\" + stringMachineName + "\\root\\cimv2", co);
      oq = new System.Management.ObjectQuery("SELECT * FROM Win32_LogicalDisk WHERE DeviceID = '"+lw+"'");
      query = new ManagementObjectSearcher(ms,oq);
      queryCollection = query.Get();
      foreach ( ManagementObject mo in queryCollection) 
      {
        s=Convert.ToInt64(mo["Size"]);
      }
      return s;
    }

    /// <summary>
    /// get memory infos
    /// </summary>
    private string GetMemInfo() 
    {
      string str="";
      try
      {
        ManagementClass memoryClass = new ManagementClass("Win32_OperatingSystem");
        ManagementObjectCollection memory = memoryClass.GetInstances();
        ManagementObjectCollection.ManagementObjectEnumerator memoryEnumerator = memory.GetEnumerator();
        memoryEnumerator.MoveNext();
        str=str+GUILocalizeStrings.Get(1974)+" "+Convert.ToString(memoryEnumerator.Current.Properties["FreePhysicalMemory"].Value) + " KB ";
        str=str+GUILocalizeStrings.Get(1973)+" "+Convert.ToString(memoryEnumerator.Current.Properties["TotalVisibleMemorySize"].Value)+" KB "+GUILocalizeStrings.Get(1953)+"\n";
        str=str+GUILocalizeStrings.Get(1978)+": "+Convert.ToString(memoryEnumerator.Current.Properties["FreeVirtualMemory"].Value)+" KB "+GUILocalizeStrings.Get(1973);
        str=str+" "+Convert.ToString(memoryEnumerator.Current.Properties["TotalVirtualMemorySize"].Value)+" KB "+GUILocalizeStrings.Get(1953)+"\n";
      } 
      catch {}
      return str;
    }

    private long GetFreeDiskLong(string lw) 
    {
      ManagementObjectSearcher query;
      ManagementObjectCollection queryCollection;
      System.Management.ObjectQuery oq;
      string stringMachineName = "localhost";
      long m=0;
      //Connect to the remote computer
      ConnectionOptions co = new ConnectionOptions();

      //Point to machine
      try 
      {
        System.Management.ManagementScope ms = new System.Management.ManagementScope("\\\\" + stringMachineName + "\\root\\cimv2", co);
        oq = new System.Management.ObjectQuery("SELECT * FROM Win32_LogicalDisk WHERE DeviceID = '"+lw+"'");
        query = new ManagementObjectSearcher(ms,oq);
        queryCollection = query.Get();
        foreach ( ManagementObject mo in queryCollection) 
        {
          m=Convert.ToInt64(mo["FreeSpace"]);
        }
      } 
      catch
      {
      }
      return m;
    }
	  
    private string GetFreeDisk(string lw) 
    {
      return CalcExt(GetFreeDiskLong(lw));
    }

	
    private void UpdateFields() 
    {
      int start=(int)Controls.CONTROL_1;
      string s="";
      string text="";
      double act=0;
      double max=0;
      int p=0;
      if (isMbm==true) 
      {	 
        MBMInfo.Refresh();
      }
      if (onStatus==true) // show status detail page 
      {
        text=text+GetMemInfo()+"\n";
        for (int i=0; i<numSensors && i<=(int)Controls.CONTROL_1+maxSensors; i++) 
        {
          if (sensors[i].sensorType==(int)SensorTypes.HD) 
          { 
            text=text+GetDiskInfo(sensors[i].drive)+"\n";
          } 
          else 
          {
            text=text+String.Format("{0}: {1:0.##} {2}",sensors[i].sname,MBMInfo.Sensor(sensors[i].sensorNum).ssCurrent,sensors[i].last);
            if (sensors[i].sensorType!=(int)SensorTypes.Mhz && sensors[i].sensorType!=(int)SensorTypes.Percentage) 
            {
              text=text+"    < "+String.Format("{0:0.##} {1}",MBMInfo.Sensor(sensors[i].sensorNum).ssLow,sensors[i].last);
              text=text+"  > "+String.Format("{0:0.##} {1}",MBMInfo.Sensor(sensors[i].sensorNum).ssHigh, sensors[i].last)+"\n";
            }
            else 
            {
              text=text+"\n";
            }
          }
        }
        GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_STATUS, text );
      }

      // test threshold for each sensor

      for (int i=0; i<numSensors && i<=(int)Controls.CONTROL_1+maxSensors; i++) 
      {
        if (showStatusBar==true) // show statusbar
        {
          if (statusBarSensor==sensors[i].sname) 
          { 
            GUIPropertyManager.SetProperty("#statusbar_name",sensors[i].name);
            if(sensors[i].name=="lw") 
            { 
              act=Convert.ToDouble((GetFreeDiskLong(sensors[i].drive)));
              max=GetDiskSize(sensors[i].drive);
              p=100-Convert.ToInt16(act/(max/100));
              GUIPropertyManager.SetProperty("#statusbar_perc",p.ToString());
              GUIPropertyManager.SetProperty("#statusbar_act",act.ToString());
            } 
            else 
            {  
              if (sensors[i].name=="perc1") 
              {
                GUIPropertyManager.SetProperty("#statusbar_perc",Convert.ToString((int)MBMInfo.Sensor(sensors[i].sensorNum).ssCurrent));
                GUIPropertyManager.SetProperty("#statusbar_act",Convert.ToString((int)MBMInfo.Sensor(sensors[i].sensorNum).ssCurrent));
              } 
              else 
              {
                act=Convert.ToDouble(MBMInfo.Sensor(sensors[i].sensorNum).ssCurrent);
                max=(double)sensors[i].max;
                try
                {
                  p=Convert.ToInt16(act/(max/100d));
                }
                catch(Exception){}
                GUIPropertyManager.SetProperty("#statusbar_perc",p.ToString());
                GUIPropertyManager.SetProperty("#statusbar_act",act.ToString());
              }
            }
          }
        }
        if (sensors[i].alarm==true) // is test alarm set?
        {	
          switch (sensors[i].name)  // which sensor
          {	
            case "lw" :				// hd
              if ((GetFreeDiskLong(sensors[i].drive)/ (1024 * 1024))<sensors[i].min) Play();
              break;
            case "fan1" :			  // Fan Sensor 1
              if (Convert.ToInt16(MBMInfo.Sensor(sensors[i].sensorNum).ssCurrent) <= sensors[i].min) Play();
              break;
            case "fan2" :			  // Fan Sensor 2
              if (Convert.ToInt16(MBMInfo.Sensor(sensors[i].sensorNum).ssCurrent) <= sensors[i].min) Play();
              break;
            case "fan3" :			  // Fan Sensor 3
              if (Convert.ToInt16(MBMInfo.Sensor(sensors[i].sensorNum).ssCurrent) <= sensors[i].min) Play();
              break;
            case "temp1" :		  // Temp Sensor 1
              if (Convert.ToInt16(MBMInfo.Sensor(sensors[i].sensorNum).ssCurrent) >= sensors[i].max) 
              {
                Play();
                if (shutdown==true) 
                {
                  Log.Write("Shutdown Temp Sensor 1 Alert");
                  WindowsController.ExitWindows(RestartOptions.ShutDown, m_bForceShutdown);
                }
              }
              break;
            case "temp2" :		  // Temp Sensor 2
              if (Convert.ToInt16(MBMInfo.Sensor(sensors[i].sensorNum).ssCurrent) >= sensors[i].max) 
              {
                Play();
                if (shutdown==true) 
                {
                  WindowsController.ExitWindows(RestartOptions.ShutDown, m_bForceShutdown);
                  Log.Write("Shutdown Temp Sensor 1 Alert");
                }
              }
              break;
            case "temp3" :		  // Temp Sensor 3
              if (Convert.ToInt16(MBMInfo.Sensor(sensors[i].sensorNum).ssCurrent) >= sensors[i].max) 
              {
                Play();
                if (shutdown==true) 
                {
                  WindowsController.ExitWindows(RestartOptions.ShutDown, m_bForceShutdown);
                  Log.Write("Shutdown Temp Sensor 1 Alert");
                }
              }
              break;
          }
        }
      }
      if (onWindow==true || onDetails==true) 
      {
        int st=0;
        int offset=0;
        int i=0;
        if(page1==true)
        {
          st=numSensors;
        } 
        else
        {
          st=numSensors-pageSensors;
          offset=pageSensors;
        }
        i=offset;
        for (int l=0; l<st && l<=(int)Controls.CONTROL_1+pageSensors; i++,l++) 
        {
          if (sensors[i].sensorType==(int)SensorTypes.HD) 
          { 
            s=String.Format("{0} {1} {2}",sensors[i].first,GetFreeDisk(sensors[i].drive),sensors[i].last);
          } 
          else 
          {
            s=String.Format("{0}: {1:0.##} {2}",sensors[i].sname,MBMInfo.Sensor(sensors[i].sensorNum).ssCurrent,sensors[i].last);
          }
		
          if (onWindow==true) GUIControl.SetControlLabel(GetID,start,s);
          if (onDetails==true) 
          {
            if (selectedSensor==start) 
            {
              GUIPropertyManager.SetProperty("#sensor",s);
              if (sensors[i].sensorType!=(int)SensorTypes.HD && sensors[i].sensorType!=(int)SensorTypes.Mhz) 
              { 
                GUIPropertyManager.SetProperty("#high",String.Format("{0}: {1:0.##}", GUILocalizeStrings.Get(1965),MBMInfo.Sensor(sensors[i].sensorNum).ssHigh));
                GUIPropertyManager.SetProperty("#low",String.Format("{0}: {1:0.##}",GUILocalizeStrings.Get(1966),MBMInfo.Sensor(sensors[i].sensorNum).ssLow));
                GUIPropertyManager.SetProperty("#alhigh",String.Format("{0}: {1:0.##}",GUILocalizeStrings.Get(1967),sensors[i].max));
                GUIPropertyManager.SetProperty("#allow",String.Format("{0}: {1:0.##}",GUILocalizeStrings.Get(1968),sensors[i].min));
              } 
              else 
              {
                if (sensors[i].sensorType==(int)SensorTypes.Mhz) 
                {
                  GUIPropertyManager.SetProperty("#high"," ");
                } 
                else 
                {
                  GUIPropertyManager.SetProperty("#high",GUILocalizeStrings.Get(1968)+" "+sensors[i].min.ToString()+" MB");
                }
                GUIPropertyManager.SetProperty("#low"," ");
                GUIPropertyManager.SetProperty("#alhigh"," ");
                GUIPropertyManager.SetProperty("#allow"," ");
              }
            }
          }
          start++;
        }
      }
    }

    public static bool IsShutdown() 
    {
      return shutdown;
    }

    public static bool IsAlarm() 
    {
      return alarm;
    }

    public static void SetShutdown(bool sh) 
    {
      shutdown=sh;
    }

    public static void SetAlarm(bool al) 
    {
      sensors[selectedSensor-10].alarm=al;
    }

    public static void SetInterval(int dl) 
    {
      delayPlay=dl;
    }

    public static int GetInterval() 
    {
      return delayPlay;
    }

    public static string GetName() 
    {
      return name;
    }

    private void Play() 
    {
      try 
      {
        if (playTime==0) 
        {
          g_Player.Play(soundFolder + "\\" + sound);
          g_Player.Volume=99;
        }
        playTime++;
        if (playTime>delayPlay) playTime=0;
      }
      catch 
      {
      }
    }

    private void LoadSound() 
    {
      using(MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings("MediaPortal.xml")) 
      {	
        soundFolder=xmlreader.GetValueAsString("status","status_sound_folder","");
        sound=xmlreader.GetValueAsString("status","status_sound","");
      }
    }

    private void LoadSettings() 
    {
      int num=0;
      using(MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings("MediaPortal.xml")) 
      {
        isMbm=xmlreader.GetValueAsBool("status","status_is_mbm",false);
        showStatusBar=xmlreader.GetValueAsBool("status","status_bar_show",false);
        if (showStatusBar==false) GUIPropertyManager.SetProperty("#statusbar_perc","-1");
        bool top=xmlreader.GetValueAsBool("status","status_bar_top",false);
        bool bot=xmlreader.GetValueAsBool("status","status_bar_bottom",false);
        if (top==true) showTopBottom=true;
        if (bot==true) showTopBottom=false;
        if (showTopBottom==true) 
        {
          GUIPropertyManager.SetProperty("#statusbarTB","1");
        } 
        else 
        {
          GUIPropertyManager.SetProperty("#statusbarTB","2");
        }
        statusBarSensor=xmlreader.GetValueAsString("status","status_bar_sensor","");
        soundFolder=xmlreader.GetValueAsString("status","status_sound_folder","c:\\windows\\media");
        sound=xmlreader.GetValueAsString("status","status_sound","ding.wav");
        delayPlay=xmlreader.GetValueAsInt("status","status_sound_delay",6);
        shutdown=xmlreader.GetValueAsBool("status","status_shutdown",false);
        int hdt=xmlreader.GetValueAsInt("status","status_hd_threshold",200);
        char drive='C';
        char ldrive='c';
        for (int i=0;i<24;i++) // read all drives
        {
          if (xmlreader.GetValueAsBool("status","status_lw"+ldrive,false)) 
          {
            sensors[num].name="lw";
            sensors[num].sname=drive+":";
            sensors[num].drive=drive+":";
            sensors[num].min=hdt;
            sensors[num].alarm=xmlreader.GetValueAsBool("status","status_lw"+ldrive+"al",false);
            sensors[num].first=GUILocalizeStrings.Get(1951)+" "+drive+":";
            sensors[num].last=GUILocalizeStrings.Get(1953);
            sensors[num++].sensorType=(int)SensorTypes.HD;
          }
          drive++;
          ldrive++;
        }
        if (isMbm==true) 
        {
          MBMInfo.Refresh();
          if (xmlreader.GetValueAsBool("status","status_temp1",false)) 
          {
            sensors[num].name="temp1";
            sensors[num].sensorNum=xmlreader.GetValueAsInt("status","status_temp1i",0);
            sensors[num].sname=Convert.ToString(MBMInfo.Sensor(sensors[num].sensorNum).ssName);
            sensors[num].alarm=xmlreader.GetValueAsBool("status","status_temp1al",false);
            sensors[num].shutdown=xmlreader.GetValueAsBool("status","status_temp1sh",false);
            sensors[num].min=Convert.ToInt16(MBMInfo.Sensor(sensors[num].sensorNum).ssAlarm2);
            sensors[num].max=Convert.ToInt16(MBMInfo.Sensor(sensors[num].sensorNum).ssAlarm1);
            sensors[num].first=GUILocalizeStrings.Get(1954)+" 1:";
            sensors[num].last=GUILocalizeStrings.Get(1955);
            sensors[num++].sensorType=(int)SensorTypes.Temperature;
          }
          if (xmlreader.GetValueAsBool("status","status_temp2",false)) 
          {
            sensors[num].name="temp2";
            sensors[num].sensorNum=xmlreader.GetValueAsInt("status","status_temp2i",0);
            sensors[num].sname=Convert.ToString(MBMInfo.Sensor(sensors[num].sensorNum).ssName);
            sensors[num].alarm=xmlreader.GetValueAsBool("status","status_temp2al",false);
            sensors[num].shutdown=xmlreader.GetValueAsBool("status","status_temp1sh",false);
            sensors[num].min=Convert.ToInt16(MBMInfo.Sensor(sensors[num].sensorNum).ssAlarm2);
            sensors[num].max=Convert.ToInt16(MBMInfo.Sensor(sensors[num].sensorNum).ssAlarm1);
            sensors[num].first=GUILocalizeStrings.Get(1954)+" 2:";
            sensors[num].last=GUILocalizeStrings.Get(1955);
            sensors[num++].sensorType=(int)SensorTypes.Temperature;
          }
          if (xmlreader.GetValueAsBool("status","status_temp3",false)) 
          {
            sensors[num].name="temp3";
            sensors[num].sensorNum=xmlreader.GetValueAsInt("status","status_temp3i",0);
            sensors[num].sname=Convert.ToString(MBMInfo.Sensor(sensors[num].sensorNum).ssName);
            sensors[num].alarm=xmlreader.GetValueAsBool("status","status_temp3al",false);
            sensors[num].shutdown=xmlreader.GetValueAsBool("status","status_temp1sh",false);
            sensors[num].min=Convert.ToInt16(MBMInfo.Sensor(sensors[num].sensorNum).ssAlarm2);
            sensors[num].max=Convert.ToInt16(MBMInfo.Sensor(sensors[num].sensorNum).ssAlarm1);
            sensors[num].first=GUILocalizeStrings.Get(1954)+" 3:";
            sensors[num].last=GUILocalizeStrings.Get(1955);
            sensors[num++].sensorType=(int)SensorTypes.Temperature;
          }
          if (xmlreader.GetValueAsBool("status","status_fan1",false)) 
          {
            sensors[num].name="fan1";
            sensors[num].sensorNum=xmlreader.GetValueAsInt("status","status_fan1i",0);
            sensors[num].sname=Convert.ToString(MBMInfo.Sensor(sensors[num].sensorNum).ssName);
            sensors[num].alarm=xmlreader.GetValueAsBool("status","status_fan1al",false);
            sensors[num].min=Convert.ToInt16(MBMInfo.Sensor(sensors[num].sensorNum).ssAlarm2);
            sensors[num].max=Convert.ToInt16(MBMInfo.Sensor(sensors[num].sensorNum).ssAlarm1);
            sensors[num].first=GUILocalizeStrings.Get(1952)+" 1:";
            sensors[num].last=GUILocalizeStrings.Get(1976);
            sensors[num++].sensorType=(int)SensorTypes.Fan;
          }
          if (xmlreader.GetValueAsBool("status","status_fan2",false)) 
          {
            sensors[num].name="fan2";
            sensors[num].sensorNum=xmlreader.GetValueAsInt("status","status_fan2i",0);
            sensors[num].sname=Convert.ToString(MBMInfo.Sensor(sensors[num].sensorNum).ssName);
            sensors[num].alarm=xmlreader.GetValueAsBool("status","status_fan2al",false);
            sensors[num].min=Convert.ToInt16(MBMInfo.Sensor(sensors[num].sensorNum).ssAlarm2);
            sensors[num].max=Convert.ToInt16(MBMInfo.Sensor(sensors[num].sensorNum).ssAlarm1);
            sensors[num].first=GUILocalizeStrings.Get(1952)+" 2:";
            sensors[num].last=GUILocalizeStrings.Get(1976);
            sensors[num++].sensorType=(int)SensorTypes.Fan;
          }
          if (xmlreader.GetValueAsBool("status","status_fan3",false)) 
          {
            sensors[num].name="fan3";
            sensors[num].sensorNum=xmlreader.GetValueAsInt("status","status_fan3i",0);
            sensors[num].sname=Convert.ToString(MBMInfo.Sensor(sensors[num].sensorNum).ssName);
            sensors[num].alarm=xmlreader.GetValueAsBool("status","status_fan3al",false);
            sensors[num].min=Convert.ToInt16(MBMInfo.Sensor(sensors[num].sensorNum).ssAlarm2);
            sensors[num].max=Convert.ToInt16(MBMInfo.Sensor(sensors[num].sensorNum).ssAlarm1);
            sensors[num].first=GUILocalizeStrings.Get(1952)+" 3:";
            sensors[num].last=GUILocalizeStrings.Get(1976);
            sensors[num++].sensorType=(int)SensorTypes.Fan;
          }
          if (xmlreader.GetValueAsBool("status","status_volt1",false)) 
          {
            sensors[num].name="volt1";
            sensors[num].sensorNum=xmlreader.GetValueAsInt("status","status_volt1i",0);
            sensors[num].sname=Convert.ToString(MBMInfo.Sensor(sensors[num].sensorNum).ssName);
            sensors[num].alarm=xmlreader.GetValueAsBool("status","status_volt1al",false);
            sensors[num].min=Convert.ToInt16(MBMInfo.Sensor(sensors[num].sensorNum).ssAlarm2);
            sensors[num].max=Convert.ToInt16(MBMInfo.Sensor(sensors[num].sensorNum).ssAlarm1);
            sensors[num].first=GUILocalizeStrings.Get(1959)+" 1:";
            sensors[num].last=GUILocalizeStrings.Get(1960);
            sensors[num++].sensorType=(int)SensorTypes.Volt;
          }
          if (xmlreader.GetValueAsBool("status","status_volt2",false)) 
          {
            sensors[num].name="volt2";
            sensors[num].sensorNum=xmlreader.GetValueAsInt("status","status_volt2i",0);
            sensors[num].sname=Convert.ToString(MBMInfo.Sensor(sensors[num].sensorNum).ssName);
            sensors[num].alarm=xmlreader.GetValueAsBool("status","status_volt2al",false);
            sensors[num].min=Convert.ToInt16(MBMInfo.Sensor(sensors[num].sensorNum).ssAlarm2);
            sensors[num].max=Convert.ToInt16(MBMInfo.Sensor(sensors[num].sensorNum).ssAlarm1);
            sensors[num].first=GUILocalizeStrings.Get(1959)+" 2:";
            sensors[num].last=GUILocalizeStrings.Get(1960);
            sensors[num++].sensorType=(int)SensorTypes.Volt;
          }
          if (xmlreader.GetValueAsBool("status","status_volt3",false)) 
          {
            sensors[num].name="volt3";
            sensors[num].sensorNum=xmlreader.GetValueAsInt("status","status_volt3i",0);
            sensors[num].sname=Convert.ToString(MBMInfo.Sensor(sensors[num].sensorNum).ssName);
            sensors[num].alarm=xmlreader.GetValueAsBool("status","status_volt3al",false);
            sensors[num].min=Convert.ToInt16(MBMInfo.Sensor(sensors[num].sensorNum).ssAlarm2);
            sensors[num].max=Convert.ToInt16(MBMInfo.Sensor(sensors[num].sensorNum).ssAlarm1);
            sensors[num].first=GUILocalizeStrings.Get(1959)+" 3:";
            sensors[num].last=GUILocalizeStrings.Get(1960);
            sensors[num++].sensorType=(int)SensorTypes.Volt;
          }	
          if (xmlreader.GetValueAsBool("status","status_mhz1",false)) 
          {
            sensors[num].name="mhz1";
            sensors[num].sensorNum=xmlreader.GetValueAsInt("status","status_mhz1i",0);
            sensors[num].sname=Convert.ToString(MBMInfo.Sensor(sensors[num].sensorNum).ssName);
            sensors[num].alarm=xmlreader.GetValueAsBool("status","status_mhz1al",false);
            sensors[num].min=Convert.ToInt16(MBMInfo.Sensor(sensors[num].sensorNum).ssAlarm2);
            sensors[num].max=Convert.ToInt16(MBMInfo.Sensor(sensors[num].sensorNum).ssAlarm1);
            sensors[num].first=GUILocalizeStrings.Get(1961);
            sensors[num].last="Mhz";
            sensors[num++].sensorType=(int)SensorTypes.Mhz;
          }
          if (xmlreader.GetValueAsBool("status","status_perc1",false)) 
          {
            sensors[num].name="perc1";
            sensors[num].sensorNum=xmlreader.GetValueAsInt("status","status_perc1i",0);
            sensors[num].sname=Convert.ToString(MBMInfo.Sensor(sensors[num].sensorNum).ssName);
            sensors[num].alarm=xmlreader.GetValueAsBool("status","status_perc1al",false);
            sensors[num].min=Convert.ToInt16(MBMInfo.Sensor(sensors[num].sensorNum).ssAlarm2);
            sensors[num].max=Convert.ToInt16(MBMInfo.Sensor(sensors[num].sensorNum).ssAlarm1);
            sensors[num].first=GUILocalizeStrings.Get(1958);
            sensors[num].last="%";
            sensors[num++].sensorType=(int)SensorTypes.Percentage;
          }
        }
        numSensors=num;
      }
    }
    #endregion
  }
}
