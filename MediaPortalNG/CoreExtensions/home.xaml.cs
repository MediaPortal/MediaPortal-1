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
        private bool viewThumbNails = true;
        public HomeExtension(ResourceDictionary dict)
        {
          
            this.ShowsNavigationUI = true;
            this.Opacity = 0.0f;
            this.Resources = dict;
            this.Loaded += new RoutedEventHandler(HomeExtension_Loaded);
            this.Height = 608;
            this.Width = 720;
        }

        void HomeExtension_Loaded(object sender, RoutedEventArgs e)
        {
            _skinMediaPath = System.IO.Directory.GetCurrentDirectory() + @"\BlueTwo\Media\";
            // set image
            //id0.Source = new BitmapImage(new Uri(_skinMediaPath + @"hover_musicvideo.png"));
            //id0.Width = 225;
            //id0.Height = 230;
            DoubleAnimation anim = new DoubleAnimation(1.0f, new Duration(new TimeSpan(0, 0, 0,0,500)));
           this.BeginAnimation(Page.OpacityProperty, anim);
           lv1.Items.Add("frodo");
           lv1.Items.Add("dman");
           lv1.Items.Add("mpod");
           lv1.Items.Add("agree");
           lv1.Items.Add("mediaportal");
           lv1.Items.Add("Annie Lenox");
           lv1.Items.Add("What the heck");
           lv1.Items.Add("some numbers:");
           lv1.Items.Add("1");
           lv1.Items.Add("2");
           lv1.Items.Add("3");
           lv1.Items.Add("4");
           lv1.Items.Add("5");
           lv1.Items.Add("6");
           lv1.Items.Add("7");
           lv1.Items.Add("8");
           lv1.Items.Add("9");
           lv1.Items.Add("10");
           lv1.Items.Add("11");
           lv1.Items.Add("12");
           lv1.Items.Add("13");
           lv1.Items.Add("14");
           lv1.Items.Add("15");
           lv1.Items.Add("16");
           
        
        }

        public void Launch_Wizard(object sender,RoutedEventArgs e)
        {
        }
        public void MPNG(object sender, RoutedEventArgs e)
        {
            if (lv1 == null)
                return;

            if (viewThumbNails == true)
            {
                lv1.Style = (Style)lv1.FindResource("GUIListControl");
                viewThumbNails = false;
            }
            else
            {
                lv1.Style = (Style)lv1.FindResource("GUIThumbnailControl");
                viewThumbNails = true;
            }
        }
 
    }
}