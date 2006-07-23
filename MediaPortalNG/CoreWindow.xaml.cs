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

        }

        void Core_Loaded(object sender, RoutedEventArgs e)
        {
            LoadWelcomeScreen();
        }

         public void LoadSkin()
        {
            string path = Directory.GetCurrentDirectory();
            string pathMedia = path + @"\BlueTwo\Media\";
            string pathSkin = path + @"\BlueTwo\skin.xaml";
             // load the skin defs
            ResourceDictionary dict = new ResourceDictionary();
            dict.Source = new System.Uri(pathSkin);
            this.Resources.MergedDictionaries.Add(dict);

           
        }

        public void LoadWelcomeScreen()
        {
            Welcome welcome=new Welcome(this.Resources);
            welcome.InitializeComponent();
            this.Navigate(welcome);
        }

        public void LoadHome()
        {
            HomeExtension home = new HomeExtension(this.Resources);
            home.InitializeComponent();
            this.Navigate(home);
        }

 



    }
}