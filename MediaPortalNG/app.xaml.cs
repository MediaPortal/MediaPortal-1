using System;
using System.Windows;
using System.IO;
using System.Windows.Media;
using System.Data;
using System.Xml;
using System.Configuration;

namespace MediaPortal
{

    public partial class app : Application
    {

        void AppStartup(object sender, StartupEventArgs args)
        {
            string path = Directory.GetCurrentDirectory();
            ResourceDictionary dict = new ResourceDictionary();
            dict.Source = new System.Uri(path + "\\skinElements.xaml");
            this.Resources.MergedDictionaries.Add(dict);
            // load image resources
            dict = new ResourceDictionary();
            dict.Source = new System.Uri(path + "\\skinImages.xaml");
            this.Resources.MergedDictionaries.Add(dict);
            Core mpCore = new Core();
       }
 
    }
}