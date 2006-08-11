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

        private ScrollViewer sv;
        public string _skinMediaPath;
        private int viewThumbNails = 0;
        private Core _core;
        private int selectButtonIndex = 0;
        private System.Collections.ArrayList selectButtonList;

        public HomeExtension(ResourceDictionary dict)
        {
            InitializeComponent();
            this.ShowsNavigationUI = true;
            this.Opacity = 0.0f;
            this.Loaded += new RoutedEventHandler(HomeExtension_Loaded);
            this.Height = 608;
            this.Width = 720;
            _core = (Core)this.Parent;
            lv1.SelectionChanged += new SelectionChangedEventHandler(lv1_SelectionChanged);
            
            ApplyLanguage("German");
            selectButtonList = new System.Collections.ArrayList();

            selectButtonList.Add("A");
            selectButtonList.Add("B");
            selectButtonList.Add("C");
            selectButtonList.Add("D");
            selectButtonList.Add("E");
            selectButtonList.Add("F");

            upDown1.SetMinValue(10);
            upDown1.SetMaxValue(25);
           

        }

 
        void lv1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            int c = VisualTreeHelper.GetChildrenCount(lv1);
            Border b = (Border)VisualTreeHelper.GetChild(lv1, 0);
            sv = (ScrollViewer)VisualTreeHelper.GetChild(b, 0);
            if (c > 0)
            {
                Image tb = (Image)sv.Template.FindName("PreviewImage", sv);
                if (tb != null && lv1.SelectedItem.GetType()==typeof(Image))
                {
                    tb.Source = ((Image)lv1.SelectedItem).Source;
                    tb.Width = 325;
                    tb.Height = 263;
                }
            } 

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

            _skinMediaPath = System.IO.Directory.GetCurrentDirectory() + @"\BlueTwo\BlueTwo\Media\";
            DoubleAnimation anim = new DoubleAnimation(1.0f, new Duration(new TimeSpan(0, 0, 0, 0, 500)));
            this.BeginAnimation(Page.OpacityProperty, anim);

            //
            string folderPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyPictures);
            string[] files=System.IO.Directory.GetFiles(folderPath);
            foreach (string fi in files)
            {
                System.IO.FileInfo fInfo = new System.IO.FileInfo(fi);
                Image img = new Image();
                try
                {
                    img.Source = new BitmapImage(new Uri(fInfo.FullName));
                    img.Tag = fInfo.Name;
                    lv1.Items.Add(img);
                }
                catch { }
            }
            // media
 
        }


        public void selectButton(object sender,RoutedEventArgs e)
        {
            // example for an gui-selectbutton
            CheckBox cb = (CheckBox)sender;
            ControlTemplate t = ((CheckBox)sender).Template;
            ControlTemplate rtp=(ControlTemplate)(((CheckBox)sender).FindResource("SelectButtonPressed"));
            ControlTemplate rtr=(ControlTemplate)(((CheckBox)sender).FindResource("SelectButtonReleased"));
            TextBlock tb = null;

            if(rtr==null || rtp==null || t==null) 
                return;



            if (cb.IsChecked == true)
            {
                if (t.Equals(rtp) == false)
                {
                    cb.Template = rtp;
                    cb.ApplyTemplate();
                    tb = (TextBlock)cb.Template.FindName("SelectContent", cb);
                    if (tb != null)
                        tb.Text = (string)selectButtonList[selectButtonIndex];

                    return;
                }              
            }
            else
            {
                if (t.Equals(rtp) )
                {
                    cb.Template = rtr;
                    cb.ApplyTemplate();
                    return;
                }

            }

            Button left=(Button)t.FindName("LeftButton", cb);
            Button right = (Button)t.FindName("RightButton", cb);
            Button source=(Button)e.OriginalSource;
            tb = (TextBlock)cb.Template.FindName("SelectContent", cb);

            if (tb==null || left == null || right == null || source==null)
                return;
            

            if (source.Equals(left))
            {
                selectButtonIndex -= 1;
                if (selectButtonIndex < 0)
                    selectButtonIndex = selectButtonList.Count - 1;
                tb.Text = (string)selectButtonList[selectButtonIndex];
            }

            if (source.Equals(right))
            {
                selectButtonIndex += 1;
                if (selectButtonIndex > selectButtonList.Count - 1)
                    selectButtonIndex = 0;
                tb.Text = (string)selectButtonList[selectButtonIndex];
            }

        }

        public void MPNG(object sender, RoutedEventArgs e)
        {
            if (lv1 == null)
                return;
            lv1.Style = null;
            
            if (viewThumbNails == 0)
            {
                lv1.Style = (Style)lv1.FindResource("GUIListControl");
                lv1.ApplyTemplate();
                viewThumbNails = 1;
            }
            else 

            if(viewThumbNails==1)
            {
                lv1.Style = (Style)lv1.FindResource("GUIThumbnailControl");
                lv1.ApplyTemplate();
                viewThumbNails = 2;
            }
            else
                if (viewThumbNails == 2)
                {
                    lv1.Style = (Style)lv1.FindResource("GUIFilmstripControl");
                    lv1.ApplyTemplate();
                    viewThumbNails = 0;

                }
            try
            {
                // example to get the elements from an style and apply the template
                // to access its elements by name
                int c = VisualTreeHelper.GetChildrenCount(lv1);
                Border b = (Border)VisualTreeHelper.GetChild(lv1, 0);
                sv = (ScrollViewer)VisualTreeHelper.GetChild(b, 0);
                if (c > 0)
                {
                    sv.ApplyTemplate();
                    TextBlock tb = (TextBlock)sv.Template.FindName("objCount", sv);
                    if(tb!=null)
                        tb.Text=lv1.Items.Count.ToString()+" objects";
                }
            }
            catch
            {
            }

        }
 
    }
}