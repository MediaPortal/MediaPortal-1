#region Copyright (C) 2005-2009 Team MediaPortal

/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
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

#endregion

using System;
using System.Threading;
using System.Windows.Forms;
using MediaPortal.TV.Recording;
using MediaPortal.TV.Scanning;
using MediaPortal.UserInterface.Controls;

namespace MediaPortal.Configuration.Sections
{
  public class Wizard_ScanBase : SectionSettings, AutoTuneCallback
  {
    protected TVCaptureDevice _card = null;
    private bool _stopScan = false;
    private bool _scanning = false;
    private bool _radio = false;

    public delegate void ScanFinishedHandler(object sender, EventArgs args);

    public event ScanFinishedHandler OnScanFinished;

    public delegate void ScanStartedHandler(object sender, EventArgs args);

    public event ScanStartedHandler OnScanStarted;

    protected MPLabel lblStatus;
    protected MPLabel lblStatus2;
    protected ProgressBar progressBarQuality;
    protected ProgressBar progressBarStrength;
    protected ProgressBar progressBarProgress;
    protected MPButton buttonScan;


    public bool Scanning
    {
      get { return _scanning; }
    }

    public bool Radio
    {
      get { return _radio; }
      set { _radio = value; }
    }

    public Wizard_ScanBase()
      : this("ScanBase")
    {
      _card = null;
    }

    public Wizard_ScanBase(string name)
      : base(name)
    {
      _card = null;

      // TODO: Add any initialization after the InitializeComponent call
    }

    public TVCaptureDevice Card
    {
      set { _card = value; }
    }

    protected void buttonScan_Click(object sender, EventArgs e)
    {
      if (_scanning)
      {
        _stopScan = true;
        buttonScan.Enabled = false;
        Cursor = Cursors.WaitCursor;
      }
      else
      {
        buttonScan.Enabled = false;
        Cursor = Cursors.WaitCursor;
        Thread thread = new Thread(new ThreadStart(DoScan));
        thread.IsBackground = true;
        thread.Start();
      }
    }

    protected void DoScan()
    {
      if (_card == null)
      {
        if (OnScanFinished != null)
        {
          OnScanFinished(this, null);
        }
        return;
      }
      _stopScan = false;
      _scanning = true;
      buttonScan.Text = "Stop";
      buttonScan.Enabled = true;
      Cursor = Cursors.Default;
      if (OnScanStarted != null)
      {
        OnScanStarted(this, null);
      }
      OnStatus("Scan started");
      OnStatus2("");

      String[] parameters = GetScanParameters();
      _card.CreateGraph();
      ITuning _tuning;
      if (_radio)
      {
        _tuning = new AnalogRadioTuning();
        _tuning.AutoTuneRadio(_card, this);
      }
      else
      {
        _tuning = GraphFactory.CreateTuning(_card);
        _tuning.AutoTuneTV(_card, this, parameters);
      }
      _tuning.Start();
      while (!_tuning.IsFinished() && (!_stopScan))
      {
        _tuning.Next();
      }
      _card.DeleteGraph();
      OnProgress(100);
      OnSignal(0, 0);
      OnStatus("Scan finished");
      if (OnScanFinished != null)
      {
        OnScanFinished(this, null);
      }
      _scanning = false;
      buttonScan.Text = "Scan";
      buttonScan.Enabled = true;
      Cursor = Cursors.Default;
    }

    protected virtual String[] GetScanParameters()
    {
      return null;
    }

    #region AutoTuneCallback

    public virtual void OnNewChannel()
    {
    }

    public void OnSignal(int quality, int strength)
    {
      if (quality < 0)
      {
        quality = 0;
      }
      if (quality > 100)
      {
        quality = 100;
      }
      if (strength < 0)
      {
        strength = 0;
      }
      if (strength > 100)
      {
        strength = 100;
      }
      progressBarQuality.Value = quality;
      progressBarStrength.Value = strength;
    }

    public void OnStatus(string description)
    {
      lblStatus.Text = description;
      lblStatus.Update();
    }

    public void OnStatus2(string description)
    {
      lblStatus2.Text = description;
      lblStatus2.Update();
    }

    public void OnProgress(int percentDone)
    {
      progressBarProgress.Value = percentDone;
    }

    public void OnEnded()
    {
    }

    public virtual void UpdateList()
    {
    }

    #endregion
  }
}