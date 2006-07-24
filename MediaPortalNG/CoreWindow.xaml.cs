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
using System.Windows.Navigation;
using System.IO;

namespace MediaPortal
{
    public class Core : NavigationWindow
    {
        /// <summary>
        /// The MediaPortal core. The Core always loads the HomeExtension as start point.
        /// </summary>
        public Core()
        {
            LoadSkin();
            this.Loaded += new RoutedEventHandler(Core_Loaded);
            this.ShowsNavigationUI = true;
            // params
            this.Width = 720;
            this.Height = 576;
            this.Background = Brushes.Black;
            this.Title = "MediaPortalNG";
            this.Show();
            this.Navigating += new NavigatingCancelEventHandler(Core_Navigating);
        }

        void Core_Navigating(object sender, NavigatingCancelEventArgs e)
        {
            object content = e.Content;
            if (content.GetType() == typeof(MediaPortal.HomeExtension))
            {
                // we get here on navigating
            }
        }

        void Core_Loaded(object sender, RoutedEventArgs e)
        {
            LoadHome();
        }

         public void LoadSkin()
        {
            string path = Directory.GetCurrentDirectory();
            string pathSkinElements = path + @"\BlueTwo\skinElements.xaml";
            string pathSkinImages = path + @"\BlueTwo\skinImages.xaml";
            // load the skin defs
            ResourceDictionary dict = new ResourceDictionary();
            // load elements resources
             dict.Source = new System.Uri(pathSkinElements);
            this.Resources.MergedDictionaries.Add(dict);
             // load image resources
            dict = new ResourceDictionary(); 
            dict.Source = new System.Uri(pathSkinImages);
            this.Resources.MergedDictionaries.Add(dict);
            // FileInfo fi;
           // StreamWriter sw=File.CreateText("E:\\skinMedia.xaml");
           // DirectoryInfo di = new DirectoryInfo(pathMedia);
           // if (di.Exists)
           // {
           //     FileInfo[] fis=di.GetFiles("*.*");
           //     foreach (FileInfo f in fis)
           //     {
           //         sw.WriteLine("<BitmapImage x:Key=" + ((char)34) + f.Name + ((char)34) + " UriSource=" + ((char)34) + "Media\\" +  f.Name + ((char)34) + "/>");
           //     }
           // }
           //  sw.Close();
           
        }

        public void LoadHome()
        {
            HomeExtension home = new HomeExtension(this.Resources);
            home.InitializeComponent();
            this.Navigate(home);
        }

 



    }
}