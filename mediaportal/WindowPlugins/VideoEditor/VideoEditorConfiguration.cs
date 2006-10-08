using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using DShowNET;
using DirectShowLib;

namespace WindowPlugins.VideoEditor
{
	public partial class VideoEditorConfiguration : Form
	{
		System.Collections.ArrayList codecList;
		public VideoEditorConfiguration()
		{
			InitializeComponent();
			codecList = DShowNET.Helper.FilterHelper.GetFilters(MediaType.Video, MediaSubType.YV12);
			for (int i = 0; i < codecList.Count; i++)
			{
				listBox1.Items.Add(codecList[i]);
			}
		}
	}
}