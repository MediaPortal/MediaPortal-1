// #define USE_VISUALBRUSH

using MediaPortal;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Media.Media3D;
using System.Collections.Generic;
using System.Windows.Media.Imaging;
using System.Windows.Media.Animation;
using System.Windows.Markup;

namespace MediaPortal
{
  public partial class MyPictures : Page
  {

    public string _skinMediaPath;
    private Core _core;

    public MyPictures()
    {

      _core = (Core)this.Parent;
      //InitializeComponent();
      this.Loaded += new RoutedEventHandler(MyPictures_Loaded);
      this.SizeChanged += new SizeChangedEventHandler(MyPictures_SizeChanged);



    }

    public void ScalePage()
    {
      double x1 = this.ActualWidth / 720.0f;
      double y1 = this.ActualHeight / 576.0f;
      double xc = 0;
      double yc = 0;
      try
      {
        if (x1 > 1.0f)
          xc = this.ActualWidth / 2.0f;
        if (y1 > 1.0f)
          yc = this.ActualHeight / 2.0f;

        ScaleTransform st = new ScaleTransform(x1, y1, xc, yc);
        this.RenderTransform = st;
      }
      catch { }
    }

    void MyPictures_SizeChanged(object sender, SizeChangedEventArgs e)
    {
      ScalePage();
    }


    void MyPictures_Loaded(object sender, RoutedEventArgs e)
    {
      // media
      ScalePage();
    }


    public void showDialog(object sender, RoutedEventArgs e)
    {
      //GUIDialog dial = new GUIDialog("Test-Context",(Core)this.Parent);
      //dial.AddMenuItem("Entry 1");
      //dial.AddMenuItem("Entry 2");
      //dial.AddMenuItem("Entry 3");
      //dial.AddMenuItem("Entry 4");
      //dial.AddMenuItem("Entry 5");
      //dial.AddMenuItem("Entry 6");
      //dial.AddMenuItem("Entry 7");
      //dial.AddMenuItem("Entry 8");
      //dial.AddMenuItem("Entry 9");
      //dial.AddMenuItem("Entry 10");
      //dial.AddMenuItem("Entry 11");
      //dial.AddMenuItem("Entry 12");


      // res holds the selected item

    }

    public void MPNG(object sender, RoutedEventArgs e)
    {

      int a = 0;

    }

  }
}