using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using MediaPortal.GUI.Library;

namespace MediaPortal.ProcessPlugins.MiniDisplayPlugin.Drivers
{
    /// <summary>
    /// iMON VFD Driver for iMON Manager >= 8.01.0419
    /// </summary>
    public class ImonVfd : ImonBase
    {
        public ImonVfd()
        {
            UnsupportedDeviceErrorMessage = "Only VFDs are supported";
            Description = "iMON VFD for iMON Manager >= 8.01.0419";
            Name = "iMONVFD";
            DisplayType = DSPN_DSP_VFD;
        }

        public override void SetLine(int line, string message)
        {
            if (line == 0)
            {
                Line1 = message;
            }
            else if (line == 1)
            {
                Line2 = message;
                IDW_SetVfdText(Line1, Line2);
            }
        }

        [DllImport("iMONDisplayWrapper.dll")]
        private static extern int IDW_SetVfdText(
          [MarshalAs(UnmanagedType.LPWStr)] string line1,
          [MarshalAs(UnmanagedType.LPWStr)] string line2);

    }
}
