using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using MpeCore;

namespace MpeMaker.Sections
{
  public partial class WelcomSection : UserControl, ISectionControl
  {
    public WelcomSection()
    {
      InitializeComponent();
    }

    private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      try
      {
        System.Diagnostics.Process.Start(
          "http://forum.team-mediaportal.com/skins-plugins-installer-mpei-212/extension-installer-v2-71542/");
      }
      catch (Exception) {}
    }

    public void Set(PackageClass pak) {}

    public PackageClass Get()
    {
      throw new NotImplementedException();
    }

    private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      try
      {
        System.Diagnostics.Process.Start("http://wiki.team-mediaportal.com/MpeMaker");
      }
      catch (Exception) {}
    }
  }
}