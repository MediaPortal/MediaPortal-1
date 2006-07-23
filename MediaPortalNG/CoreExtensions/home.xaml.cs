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

namespace MediaPortal
{
    public partial class HomeExtension :  Page
    {
        
        public string _skinMediaPath;
        public HomeExtension(ResourceDictionary dict)
        {
          
            this.ShowsNavigationUI = true;
            this.Opacity = 0.0f;
            this.Resources = dict;
            this.Loaded += new RoutedEventHandler(HomeExtension_Loaded);
           
        }

        void HomeExtension_Loaded(object sender, RoutedEventArgs e)
        {
            _skinMediaPath = System.IO.Directory.GetCurrentDirectory() + @"\BlueTwo\Media\";
            // set image
            id0.Source = new BitmapImage(new Uri(_skinMediaPath + @"hover_musicvideo.png"));
            id0.Width = 225;
            id0.Height = 230;
            DoubleAnimation anim = new DoubleAnimation(1.0f, new Duration(new TimeSpan(0, 0, 0,0,500)));
           this.BeginAnimation(Page.OpacityProperty, anim);
        }

        public void Launch_Wizard(object sender,RoutedEventArgs e)
        {
            DoubleAnimation anim = new DoubleAnimation(225, 0, new Duration(new TimeSpan(0, 0, 2)));
            anim.AutoReverse = true;
            id0.BeginAnimation(Image.WidthProperty,anim);

        }
        public void MPNG(object sender, RoutedEventArgs e)
        {
            id0.Source = new BitmapImage(new Uri(_skinMediaPath + @"preview.png"));
        }
 
    }
}