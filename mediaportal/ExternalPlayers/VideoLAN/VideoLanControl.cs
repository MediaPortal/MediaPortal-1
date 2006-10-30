using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using MediaPortal.GUI.Library;

namespace MediaPortal.VideoLanPlugin
{
    public sealed partial class VideoLanControl : UserControl
    {
        static readonly VideoLanControl instance = new VideoLanControl();

        public VideoLanControl()
        {
            InitializeComponent();
        }

        public AxAXVLC.AxVLCPlugin Player
        {
            get { return axVLCPlugin1; }
        }

        public static VideoLanControl Instance
        {
            get { return instance; }
        }

             
    }
}
