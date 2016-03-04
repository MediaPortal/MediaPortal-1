#region Copyright (C) 2005-2016 Team MediaPortal

// Copyright (C) 2005-2016 Team MediaPortal
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
using System.Globalization;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

using MediaPortal.GUI.Library;
using MediaPortal.Localisation;
using MediaPortal.Player;

using Microsoft.DirectX.Direct3D;

namespace MediaPortal.Tests.Core.Player
{
  public class DirectShowPlayerTestHelper : IDisposable
  {
    private const int NumberOfRetriesEnum = 20;
    private D3DEnumeration enumerationSettings;

    [DllImport("User32.dll", CharSet = CharSet.Auto)]
    internal static extern bool GetMonitorInfo(IntPtr hWnd, ref D3D.MonitorInformation info);

    public DirectShowPlayerTestHelper(Form form)
    {
      PrepareGraphicsContext(form);
    }

    ~DirectShowPlayerTestHelper()
    {
      Dispose(false);
      GC.SuppressFinalize(this);
    }

    private void PrepareGraphicsContext(Form form)
    {
      GUILocalizeStrings.SetLocalisationProvider(new FakeLocalizationProvider());
      GUIGraphicsContext.form = form;
      GUIGraphicsContext.ActiveForm = form.Handle;
      GUIGraphicsContext.VolumeHandler = new VolumeHandler { Volume = 1 };
      GUIGraphicsContext.DeviceVideoConnected = 1;
      GUIGraphicsContext.DeviceAudioConnected = 1;
      EnumerateDevices();
      GUIGraphicsContext.currentScreen = Screen.PrimaryScreen;
      GUIGraphicsContext.RenderGUI = new FakeRender();
      var adapterInfo = FindAdapterForScreen(GUIGraphicsContext.currentScreen);
      var presentParams = new PresentParameters
                            {
                              SwapEffect = SwapEffect.Discard,
                              DeviceWindow = form,
                              Windowed = true
                            };

      GUIGraphicsContext.DX9Device = new Device(
        adapterInfo.AdapterOrdinal,
        DeviceType.Hardware,
        form,
        CreateFlags.SoftwareVertexProcessing,
        presentParams);
    }

    private static bool ConfirmDevice(
      Caps caps,
      VertexProcessingType vertexProcessingType,
      Format adapterFormat,
      Format backBufferFormat)
    {
      return true;
    }

    private void EnumerateDevices()
    {
      enumerationSettings = new D3DEnumeration();
      var enumIntCount = 0;
      var confirmDeviceCheck = false;

      // get display adapter info
      while (confirmDeviceCheck != true && enumIntCount < NumberOfRetriesEnum)
      {
        try
        {
          enumerationSettings.ConfirmDeviceCallback = ConfirmDevice;
          try
          {
            enumerationSettings.Enumerate();
            confirmDeviceCheck = true;
          }
          catch
          {
            enumerationSettings = new D3DEnumeration();
          }
        }
        catch
        {
          enumerationSettings = new D3DEnumeration();
        }

        enumIntCount++;
      }
    }

    private GraphicsAdapterInfo FindAdapterForScreen(Screen screen)
    {
      foreach (GraphicsAdapterInfo adapterInfo in enumerationSettings.AdapterInfoList)
      {
        var hMon = Manager.GetAdapterMonitor(adapterInfo.AdapterOrdinal);

        var info = new D3D.MonitorInformation();
        info.Size = (uint)Marshal.SizeOf(info);
        GetMonitorInfo(hMon, ref info);
        var rect = Screen.FromRectangle(info.MonitorRectangle).Bounds;
        if (rect.Equals(screen.Bounds))
        {
          GUIGraphicsContext.currentStartScreen = GUIGraphicsContext.currentScreen;
          return adapterInfo;
        }
      }
      return null;
    }

    public void Dispose()
    {
      Dispose(true);
    }

    private void Dispose(bool disposing)
    {
      if (GUIGraphicsContext.DX9Device != null)
      {
        GUIGraphicsContext.DX9Device.Dispose();
        GUIGraphicsContext.DX9Device = null;
      }

      GUIGraphicsContext.VolumeHandler = null;
      GUIGraphicsContext.form = null;
      GUIGraphicsContext.ActiveForm = IntPtr.Zero;
      GUIGraphicsContext.DeviceVideoConnected = 0;
      GUIGraphicsContext.DeviceAudioConnected = 0;
    }

    private class FakeRender : IRender
    {
      public void RenderFrame(float timePassed)
      {
      }
    }

    private class FakeLocalizationProvider : ILocalizationProvider
    {
      public CultureInfo CurrentLanguage
      {
        get { return Thread.CurrentThread.CurrentCulture; }
      }

      public int Characters
      {
        get { return 255; }
      }

      public bool UseRTL
      {
        get { return false; }
      }

      public CultureInfo[] AvailableLanguages()
      {
        return new[] { Thread.CurrentThread.CurrentCulture };
      }

      public bool IsLocalSupported()
      {
        return true;
      }

      public void AddDirection(string directory)
      {
      }

      public void ChangeLanguage(string cultureName)
      {
      }

      public Localisation.LanguageStrings.StringLocalised Get(string section, int id)
      {
        return null;
      }

      public string GetString(string section, int id)
      {
        switch (id)
        {
          case 2600:
            return "English";
        }

        return string.Empty;
      }

      public string GetString(string section, int id, object[] parameters)
      {
        switch (id)
        {
          case 2600:
            return "English";
        }

        return string.Empty;
      }

      public CultureInfo GetBestLanguage()
      {
        return Thread.CurrentThread.CurrentCulture;
      }
    }
  }
}