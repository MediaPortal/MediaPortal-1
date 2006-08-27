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
using System.Globalization;

namespace MediaPortal
{
    public partial class HomeExtension :  Page
    {

        public string _skinMediaPath;
        private Core _core;
        private ListBoxItem _prevItem;
        private ListBoxItem _nextItem;

        public HomeExtension(ResourceDictionary dict)
        {

             _core = (Core)this.Parent;
            InitializeComponent();
            this.Opacity = 0.0f;
            this.Loaded += new RoutedEventHandler(HomeExtension_Loaded);
            this.SizeChanged += new SizeChangedEventHandler(HomeExtension_SizeChanged);
            this.KeyDown += new System.Windows.Input.KeyEventHandler(HomeExtension_KeyDown);

            ApplyLanguage("German");
            lv.SelectionChanged += new SelectionChangedEventHandler(lv_SelectionChanged);
            // create list box
            lv.Background = new ImageBrush((BitmapImage)FindResource("previewbackground.png"));
            
            // the list of plugins for mpng
            GUIPluginList plugList = new GUIPluginList();

            // plugins
            GUIPlugin myPictures = new GUIPlugin();
            myPictures.GUIPluginObject = typeof(MyPictures);
            myPictures.PluginName = "My Pictures";
            myPictures.PluginText = "pictures";
            myPictures.PluginHover = (BitmapImage)FindResource("defaultPictureBig.png");

            GUIPlugin myMusic = new GUIPlugin();
            myMusic.GUIPluginObject = typeof(MyPictures);
            myMusic.PluginName = "My Music";
            myMusic.PluginText = "music";
            myMusic.PluginHover = (BitmapImage)FindResource("defaultAudioBig.png");


            GUIPlugin myTV = new GUIPlugin();
            myTV.GUIPluginObject = typeof(MyPictures);
            myTV.PluginName = "My TV";
            myTV.PluginText = "tv";
            myTV.PluginHover = (BitmapImage)FindResource("defaultVideoBig.png");
            GUIPlugin myVid = new GUIPlugin();
            myVid.GUIPluginObject = typeof(MyPictures);
            myVid.PluginName = "My Videos";
            myVid.PluginText = "videos";
            myVid.PluginHover = (BitmapImage)FindResource("defaultVideoBig.png");
           
            plugList.Add(myPictures);
            plugList.Add(myMusic);
            plugList.Add(myVid);
            plugList.Add(myTV);

            lv.ItemsSource = plugList;
            //lv.DataContext = plugList;
            lv.ApplyTemplate();
            ScrollViewer sv = (ScrollViewer)VisualTreeHelper.GetChild(VisualTreeHelper.GetChild(lv, 0), 0);
            sv.ApplyTemplate();

      
            
            // set previous item object
            _prevItem = (ListBoxItem)sv.Template.FindName("PrevItem", sv);
            _prevItem.ApplyTemplate();
            _prevItem.Focusable = false;

            // set previous item object
            _nextItem = (ListBoxItem)sv.Template.FindName("NextItem", sv);
            _nextItem.ApplyTemplate();
            _nextItem.Focusable = false;
            
            lv.Focusable = false;
            lv.KeyDown += new System.Windows.Input.KeyEventHandler(lv_KeyDown);
            lv.SelectedItem = lv.Items[0];
        }

        void HomeExtension_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ScalePage();
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

        public void HandleKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            HomeExtension_KeyDown(sender, e);
        }

        void HomeExtension_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            // capture key down and pass to the main menu list box
            lv_KeyDown(sender, e);
        }

        void lv_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Right)
            {
                int itemNumber = lv.Items.IndexOf(lv.SelectedItem);
                itemNumber++;
                if (itemNumber > lv.Items.Count - 1)
                    itemNumber = 0;

                lv.SelectedItem = lv.Items[itemNumber];
                lv.ScrollIntoView(lv.SelectedItem);
            }
            if (e.Key == System.Windows.Input.Key.Left)
            {
                int itemNumber = lv.Items.IndexOf(lv.SelectedItem);
                itemNumber--;
                if (itemNumber < 0)
                    itemNumber = lv.Items.Count-1;

                lv.SelectedItem = lv.Items[itemNumber];
                lv.ScrollIntoView(lv.SelectedItem);
            }
            if (e.Key == System.Windows.Input.Key.Return)
            {
                Core core = (Core)this.Parent;
                if (core != null)
                {
                    GUIPlugin plg = (GUIPlugin)lv.SelectedItem;
                    if (plg.GUIPluginObject == typeof(MyPictures))
                    {
                        core.LoadPlugin(typeof(MyPictures));
                    }
                }
            }
            e.Handled = true;
        }

 
        void lv_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // set next and previous items
            if (_prevItem != null && _nextItem!=null)
            {
                int prev = lv.Items.IndexOf(lv.SelectedItem);
                prev -= 1;
                if (prev <0 ) prev = lv.Items.Count - 1;
                else
                    if (prev >= lv.Items.Count - 1)
                        prev = 0;

                int next = lv.Items.IndexOf(lv.SelectedItem);
                next += 1;

                if (next > lv.Items.Count -1)
                    next = 0;

                _prevItem.DataContext = (GUIPlugin)lv.Items.GetItemAt(prev);
                if(_prevItem.DataContext!=null)
                {
                    ;
                }
                
                _nextItem.DataContext = (GUIPlugin)lv.Items.GetItemAt(next);
                if (_nextItem.DataContext != null)
                {
                    ;
                }

            }

            // animations
            DisplayText(((GUIPlugin)lv.SelectedItem).PluginText);
        }

 
        private void ApplyLanguage(string lang)
        {
            System.Xml.XmlDocument langFile = new System.Xml.XmlDocument();
            langFile.Load(System.IO.Directory.GetCurrentDirectory()+@"\language\"+lang+@"\strings.xml");
            System.Xml.XmlNode node = langFile.GetElementsByTagName("strings").Item(0);
            if (node != null)
            {
                for (int n = 0; n < 9999; n++)
                {
                   object o=this.FindName("id" + n.ToString());
                    if (o != null)
                    {
                      
                         if(o.ToString().StartsWith("System.Windows.Controls.TextBlock"))
                         {

                             ((TextBlock)o).Text = Core.GetLocalizedString("id", ((TextBlock)o).Text, "value", node);
                         }

                         if (o.ToString().StartsWith("System.Windows.Controls.Button"))
                         {
                             string tag = ((Button)o).Tag.ToString();
                             string label=Core.SplitElementTag(tag, "labelNum","##metadata");
                             ((Button)o).Content = Core.GetLocalizedString("id", label, "value", node);
                         }
                         if (o.ToString().StartsWith("System.Windows.Controls.CheckBox"))
                         {
                             string tag = ((CheckBox)o).Tag.ToString();
                             string label = Core.SplitElementTag(tag, "labelNum", "##metadata");
                             ((CheckBox)o).Content = Core.GetLocalizedString("id", label, "value", node);
                         }
                         if (o.ToString().StartsWith("System.Windows.Controls.ComboBox"))
                         {
                             string tag = ((ComboBox)o).Tag.ToString();
                             string label = Core.SplitElementTag(tag, "labelNum", "##metadata");
                             ((ComboBox)o).Text = Core.GetLocalizedString("id", label, "value", node);
                         }
                    }
                }
                int count=VisualTreeHelper.GetChildrenCount(this);
            }

        }

        void HomeExtension_Loaded(object sender, RoutedEventArgs e)
        {

            _skinMediaPath = System.IO.Directory.GetCurrentDirectory() + @"\Media\";
            this.Opacity = 1.0f;
            //
            ScalePage();
        }


        public void selectButton(object sender,RoutedEventArgs e)
        {
       }

        public void MPNG(object sender, RoutedEventArgs e)
        {
        }

        public void DisplayText(string textToDisplay)
        {
            FormattedText formattedText = new FormattedText(
                textToDisplay,
                CultureInfo.GetCultureInfo("en-us"),
                FlowDirection.LeftToRight,
                new Typeface("Arial"),
                96,
                Brushes.LightGray);


            Geometry geometry = formattedText.BuildGeometry(new Point(0, 0));
            PathGeometry pathGeometry = geometry.GetFlattenedPathGeometry();
           // path.Data = pathGeometry;
        }
 
    }
}