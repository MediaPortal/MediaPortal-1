// #define USE_VISUALBRUS

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
using System.Xml;

namespace MediaPortal
{
    public class Core : NavigationWindow
    {
        public char chr34=((char)34);
        
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
            this.Height = 608;
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
            ParseSkinXML(Directory.GetCurrentDirectory() + @"\BlueTwo\");
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

        private void ParseSkinXML(string path)
        {
            string header1 = "<Page xmlns=" + chr34 + "http://schemas.microsoft.com/winfx/2006/xaml/presentation" + chr34;
            string header2="xmlns:x=" + chr34 + "http://schemas.microsoft.com/winfx/2006/xaml" + chr34 ;
            string header3="Style=" + chr34 + "{StaticResource PageBackground}" + chr34 ;
            string header4="x:Class=" + chr34 + "MediaPortal.HomeExtension" + chr34 + ">";
            string file = "mytvhome";
            // the skin
            System.Xml.XmlDocument doc = new System.Xml.XmlDocument();

            // the common.window.xml
            System.Xml.XmlDocument commonWindow = new System.Xml.XmlDocument();

            doc.Load(path + @"\"+file+".xml");
            commonWindow.Load(path + @"\common.window.xml");


            string test = doc.NameTable.Get("controls");
            // for skin file
            System.Xml.XmlNodeList nodes =doc.GetElementsByTagName("controls");
            XmlNode node = nodes.Item(0);
            
            // for common.xxxxxx.xml file
            System.Xml.XmlNodeList nodesCW =commonWindow.GetElementsByTagName("controls");
            XmlNode cwNode = nodesCW.Item(0);

            StreamWriter sw = File.CreateText(path + @"\" + file + ".xaml");
            sw.WriteLine(header1);
            sw.WriteLine(header2);
            sw.WriteLine(header3);
            sw.WriteLine(header4);
            sw.WriteLine(sw.NewLine);
            sw.WriteLine("<Canvas>");

            //if (di.Exists)
            //{
            //    FileInfo[] fis = di.GetFiles("*.*");
            //    foreach (FileInfo f in fis)
            //    {
            //        sw.WriteLine("<BitmapImage x:Key=" + ((char)34) + f.Name + ((char)34) + " UriSource=" + ((char)34) + "Media\\" + f.Name + ((char)34) + "/>");
            //    }
            //}
            XmlNode group = null;
            bool loopGroup = false;
             
            // process the defines
            foreach (XmlNode child in node.ParentNode)
            {
                if (child.Name == "define")
                {
                    string[] define = child.InnerText.Split(new char[] { ':' });
                    XmlNode nodeToAdd = FindNodeByInnerText(define[0], cwNode);
                    if (nodeToAdd != null)
                    {
                        XmlNode clone = nodeToAdd.CloneNode(true);
                        foreach (XmlNode property in clone.ChildNodes)
                        {
                            if (property.InnerText == define[0])
                                property.InnerText = define[1];
                        }
                        foreach (XmlNode property in clone.ChildNodes)
                        {
                            XAMLWriter(sw, property, clone);
                        }
                    }
                    
                }

            }

            // loop the controls
            if (node != null)
            {
                foreach(XmlNode control1 in node.ChildNodes)
                {
                    XmlNode control = control1;
                    if (group != null)
                    {
                        control = group;
                        loopGroup = true;
                        group = null;
                    }
                    if (control.ChildNodes.Count > 0)
                    {
                       if(loopGroup==false)
                        foreach (XmlNode property in control.ChildNodes)
                        {
                            if (property.InnerText == "group")
                            {
                                group = property;
                                break;
                            }

                            XAMLWriter(sw, property, control);
                        }//foreach (XmlNode property in control.ChildNodes)
                        else
                        {
                            int xVal = -1;
                            int yVal = -1;
                            bool loop = true;
                            do
                            {
                                if (control.Name == "posX")
                                {
                                    xVal = Convert.ToInt32(control.InnerText);
                                }

                                if (control.Name == "posY")
                                {
                                    yVal = Convert.ToInt32(control.InnerText);
                                }

                                if (control.Name == "control")
                                {
                                    loop = false;
                                }
                                else
                                    control = control.NextSibling;
                            } while (loop == true);
                            do
                            {
                                foreach (XmlNode property in control.ChildNodes)
                                {
                                    XAMLWriter(sw, property, control,xVal,yVal);
                                }
                                if (yVal > 0)
                                    yVal += 32;
                                control = control.NextSibling;
                            } while (control != null);
                            loopGroup = false;
                        }
                    };//if (control.ChildNodes.Count > 0)
                };//foreach(XmlNode control in node.ChildNodes)
            }
            sw.WriteLine("</Canvas>");
            sw.WriteLine("</Page>");
            sw.Close();

        }
        private void XAMLWriter(StreamWriter sw, XmlNode property, XmlNode control)
        {
            XAMLWriter(sw, property, control, -1, -1);
        }
        private void XAMLWriter(StreamWriter sw, XmlNode property,XmlNode control,int baseX,int baseY)
        {
            XmlNode parent = control;
            string onUp = "", onDown = "", onLeft = "", onRight = "";
            onLeft = "onleft:" + FindNodeByName("onleft", parent) + ";";
            onRight = "onright:" + FindNodeByName("onright", parent) + ";";
            onUp = "onup:" + FindNodeByName("onup", parent) + ";";
            onDown = "ondown:" + FindNodeByName("ondown", parent);
            string metaData = "";
            string posx="";
            string posy="";
            if (baseX == -1 && baseY == -1)
            {
                int x = (FindNodeByName("posX", parent) != "" ? Convert.ToInt32(FindNodeByName("posX", parent)) : 0);
                int y = (FindNodeByName("posX", parent) != "" ? Convert.ToInt32(FindNodeByName("posY", parent)) : 0);
                posx = x.ToString();
                posy = y.ToString();
            }
            else
            {
                int x = (FindNodeByName("posX", parent) != "" ? Convert.ToInt32(FindNodeByName("posX", parent)) : 0);
                int y = (FindNodeByName("posX", parent) != "" ? Convert.ToInt32(FindNodeByName("posY", parent)) : 0);
                x += baseX;
                y += baseY;
                posx = x.ToString();
                posy = y.ToString();
            }
            // process image
            if (property.InnerText == "image" && property.Name == "type")
            {
                string texture = "";
                if (FindNodeByName("texture", parent).StartsWith("#"))
                {
                    metaData = FindNodeByName("texture", parent);
                    texture = "emptyImage";
                }
                else
                    texture = FindNodeByName("texture", parent);

                if (FindNodeByName("texture", parent) == "background.png")
                    return;//skin background image
                sw.WriteLine("  <!-- " + FindNodeByName("description", parent) + " -->");
                sw.WriteLine("<Image");
                sw.WriteLine("  Canvas.Top=" + chr34 + posy + chr34);
                sw.WriteLine("  Canvas.Left=" + chr34 + posx + chr34);
                if (FindNodeByName("width", parent) != "")
                    sw.WriteLine("  Width=" + chr34 + FindNodeByName("width", parent) + chr34);

                if (FindNodeByName("height", parent) != "")
                    sw.WriteLine("  Height=" + chr34 + FindNodeByName("height", parent) + chr34);
                sw.WriteLine("  Source=" + chr34 + "{DynamicResource " + FindNodeByName("texture", parent) + "}" + chr34);
                sw.WriteLine("  Visibility=" + chr34 + (FindNodeByName("visible", parent) == "no" ? "Hidden" : "Visible") + chr34);
                sw.WriteLine("  Tag=" + chr34 + "##id::" + FindNodeByName("id", parent) + "//##keycontrol::" + onLeft + onRight + onUp + onDown + "//##metadata::" + metaData + chr34);

                sw.WriteLine("/>");
                sw.WriteLine(sw.NewLine);
                return;
            }
            // process label
            if (property.InnerText == "label" && property.Name == "type")
            {
                sw.WriteLine("  <!-- " + FindNodeByName("description", parent) + " -->");
                sw.WriteLine("<TextBlock");
                string foreColor = FindNodeByName("textcolor", parent);
                try
                {

                    if (Convert.ToUInt64("0x" + foreColor, 16) >= 0)
                    {
                        foreColor = "#" + foreColor;
                    }
                }
                catch
                {
                }
                string labelText = FindNodeByName("label", parent);
                if (labelText == "")
                    labelText = "textField";
                if (labelText.StartsWith("#"))
                {
                    metaData = labelText;
                }
                sw.WriteLine("  Text=" + chr34 + labelText + chr34);
                sw.WriteLine("  Canvas.Top=" + chr34 + posy + chr34);
                sw.WriteLine("  Foreground=" + chr34 + foreColor + chr34);
                sw.WriteLine("  Visibility=" + chr34 + (FindNodeByName("visible", parent) == "no" ? "Hidden" : "Visible") + chr34);
                if (FindNodeByName("align", parent) == "right")
                {
                    sw.WriteLine("  Canvas.Left=" + chr34 + "0" + chr34);
                    sw.WriteLine("  Width=" + chr34 + posx + chr34);
                    sw.WriteLine("  TextAlignment=" + chr34 + "Right" + chr34);
                }
                if (FindNodeByName("align", parent) == "left")
                {
                    sw.WriteLine("  Canvas.Left=" + chr34 + posx + chr34);
                    sw.WriteLine("  TextAlignment=" + chr34 + "Left" + chr34);
                }
                if (FindNodeByName("align", parent) != "left" && FindNodeByName("align", parent) != "right")
                {
                    sw.WriteLine("  Canvas.Left=" + chr34 + posx + chr34);
                }
                sw.WriteLine("  FontSize=" + chr34 + FindNodeByName("font", parent) + chr34);
                sw.WriteLine("  Tag=" + chr34 + "##id::" + FindNodeByName("id", parent) + "//##keycontrol::" + onLeft + onRight + onUp + onDown + "//##metadata::" + metaData + chr34);
                sw.WriteLine("/>");
                sw.WriteLine(sw.NewLine);
                return;
            }

            // process button
            if (property.InnerText == "button" && property.Name == "type")
            {
                
                sw.WriteLine("  <!-- " + FindNodeByName("description", parent) + " -->");
                sw.WriteLine("<Button");
                string labelText = FindNodeByName("label", parent);
                if (labelText == "")
                    labelText = "textField";

                if (Convert.ToInt32(labelText) > 0)
                    metaData = "labelNum:" + labelText;

                sw.WriteLine("  Canvas.Top=" + chr34 + posy + chr34);
                sw.WriteLine("  Canvas.Left=" + chr34 + posx + chr34);
                sw.WriteLine("  Visibility=" + chr34 + (FindNodeByName("visible", parent) == "no" ? "Hidden" : "Visible") + chr34);

                if (FindNodeByName("width", parent) != "")
                    sw.WriteLine("  Width=" + chr34 + FindNodeByName("width", parent) + chr34);
                else
                    sw.WriteLine("  Width=" + chr34 + "190" + chr34);

                if (FindNodeByName("height", parent) != "")
                    sw.WriteLine("  Height=" + chr34 + FindNodeByName("height", parent) + chr34);
                else
                    sw.WriteLine("  Height=" + chr34 + "32" + chr34);

                sw.WriteLine("  Style=" + chr34 + "{DynamicResource GUIButton}" + chr34);
                sw.WriteLine("  Tag=" + chr34 + "##id::" + FindNodeByName("id", parent) + "//##keycontrol::" + onLeft + onRight + onUp + onDown + "//##metadata::" + metaData + chr34);
                sw.WriteLine(">"+labelText+"</Button>");
                sw.WriteLine(sw.NewLine);
                return;
            }
        }

        private XmlNode FindNodeByInnerText(string text,XmlNode tree)
        {
            if (tree == null)
                return null;
            
            foreach (XmlNode control in tree.ChildNodes)
            {

                foreach (XmlNode property in control.ChildNodes)
                {
                    if (property.InnerText == text && property.ParentNode != null)
                    {
                        return property.ParentNode;
                    }
                }
            };//foreach (XmlNode property in control.ChildNodes)
            return null;
        }

        private string FindNodeByName(string text, XmlNode tree)
        {
            if (tree == null)
                return "";

            foreach (XmlNode property in tree.ChildNodes)
            {
                if (property.Name == text && property.ParentNode != null)
                {
                    if (property.InnerText.StartsWith("#"))
                        return property.InnerText;
                    
                    if (property.InnerText.StartsWith("font"))
                        return property.InnerText.Substring(4);

                    if (property.InnerText == "-") 
                        return "emptyImage";
                    else
                        return property.InnerText;
                }
            };//foreach (XmlNode property in control.ChildNodes)
            return "";
        }

 
    }
}