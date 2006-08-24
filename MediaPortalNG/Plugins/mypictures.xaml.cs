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
    public partial class MyPictures :  Page
    {

        private ScrollViewer sv;
        public string _skinMediaPath;
        private int viewThumbNails = 0;
        private Core _core;

        public MyPictures()
        {
            
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(MyPictures_Loaded);
            _core = (Core)this.Parent;
            lv1.SelectionChanged += new SelectionChangedEventHandler(lv1_SelectionChanged);       

            ApplyLanguage("German");

            upDown1.SetMinValue(0);
            upDown1.SetMaxValue(25);

            select1.AddItem("Test 1");
            select1.AddItem("Test 2");
            select1.AddItem("ABC");
            select1.AddItem("123");
            select1.AddItem("HUH?");

           


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

        void MyPictures_Loaded(object sender, RoutedEventArgs e)
        {

            _skinMediaPath = System.IO.Directory.GetCurrentDirectory() + @"\Media\";
            this.Opacity = 1.0f;
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


        public void showDialog(object sender,RoutedEventArgs e)
        {
            GUIDialog dial = new GUIDialog("Test-Context",(Core)this.Parent);
            dial.AddMenuItem("Entry 1");
            dial.AddMenuItem("Entry 2");
            dial.AddMenuItem("Entry 3");
            dial.AddMenuItem("Entry 4");
            dial.AddMenuItem("Entry 5");
            dial.AddMenuItem("Entry 6");
            dial.AddMenuItem("Entry 7");
            dial.AddMenuItem("Entry 8");
            dial.AddMenuItem("Entry 9");
            dial.AddMenuItem("Entry 10");
            dial.AddMenuItem("Entry 11");
            dial.AddMenuItem("Entry 12");

            pb1.Maximum = 25;
            pb1.Minimum = 0;
           
            // res holds the selected item
            int res=dial.ShowDialog();
            if (res >= 0)
            {
                pb1.Value = res + 1;
                upDown1.Value = res + 1;
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