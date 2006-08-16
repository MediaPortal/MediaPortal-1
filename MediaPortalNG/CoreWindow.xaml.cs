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
        public XmlNode defNodes;
        public System.Collections.ArrayList idArray;
        public System.Collections.ArrayList typeArray;
        public string currentFile;
        private HomeExtension _home;
        /// <summary>
        /// The MediaPortal core. The Core always loads the HomeExtension as start point.
        /// </summary>
        public Core()
        {
            this.Loaded += new RoutedEventHandler(Core_Loaded);
            this.ShowsNavigationUI = true;
            // params
            this.Width = 720;
            this.Height = 670;
            this.Background = Brushes.Black;
            this.Title = "MediaPortalNG";
            this.Show();
            this.Navigating += new NavigatingCancelEventHandler(Core_Navigating);
            this.KeyDown += new System.Windows.Input.KeyEventHandler(Core_KeyDown);
            _home = new HomeExtension(this.Resources);
            _home.InitializeComponent();
            this.Navigate(_home);
        }

        void Core_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (_home!=null)
            {
                _home.HandleKeyDown(sender, e);
                e.Handled = true;
            }
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
            
            LoadSkin();

        }

        
         public void LoadSkin()
        {
            
            //FileInfo fi;

            //StreamWriter sw=File.CreateText("E:\\skinImages1.xaml");
            //DirectoryInfo di = new DirectoryInfo("E:\\MPNG\\bin\\Debug\\Media\");
            //if (di.Exists)
            //{
            //    FileInfo[] fis=di.GetFiles("*.*");
            //    foreach (FileInfo f in fis)
            //    {
            //        try
            //        {
            //            BitmapImage bmp = new BitmapImage(new Uri(f.FullName));
            //            int width = (int)bmp.Width;
            //            int height = (int)bmp.Height;
            //            if (bmp.DpiX < 96)
            //            {
            //                width = ((int)(width * bmp.DpiX)) / 96;

            //            }
            //            if (bmp.DpiY < 96)
            //            {
            //                height = ((int)(height * bmp.DpiY)) / 96;

            //            }
            //            if (f.Name == "hover_my pictures.png}")
            //            {
            //                int a = 0;
            //            }
            //            sw.WriteLine("<BitmapImage x:Key=" + ((char)34) + f.Name + ((char)34) + " UriSource=" + ((char)34) + "Media\" + f.Name + ((char)34) + " DecodePixelWidth="+chr34+width.ToString()+chr34+" DecodePixelHeight="+chr34+height.ToString()+chr34+"/>");
            //        }
            //        catch { }
            //    }
            //}
            // sw.Close();
        
        }
         

        private void ParseSkinXML(string path,string file)
        {
            string header1 = "<Page xmlns=" + chr34 + "http://schemas.microsoft.com/winfx/2006/xaml/presentation" + chr34;
            string header2="xmlns:x=" + chr34 + "http://schemas.microsoft.com/winfx/2006/xaml" + chr34 ;
            string header3="Style=" + chr34 + "{StaticResource PageBackground}" + chr34 ;
            string header4="x:Class=" + chr34 + "MediaPortal.HomeExtension" + chr34 + ">";
            // the skin
            XmlDocument doc = new System.Xml.XmlDocument();
            XmlDocument defaults = new XmlDocument();
            XmlDocument commonFacade = new XmlDocument();
            idArray = new System.Collections.ArrayList();


            // the common.window.xml/ common.settings.xml
            XmlDocument commonWindow = new System.Xml.XmlDocument();

            doc.Load(path + @"\"+file);
            defaults.Load(path + @"\" + "references.xml");

            //string test = doc.NameTable.Get("controls");
            // for skin file
            XmlNode node =doc.GetElementsByTagName("controls").Item(0);
            XmlNode listCtrls = null;// commonFacade.GetElementsByTagName("controls").Item(0);

            defNodes = defaults.GetElementsByTagName("controls").Item(0);

            if(node!=null)
            foreach (XmlNode property in node.ChildNodes)
            {
                if (property.Name == "import")
                {
                    if (property.InnerText == "common.window.xml" ||
                        property.InnerText == "common.settings.xml")
                        commonWindow.Load(path + @"\" + property.InnerText);
                    else
                    {
                        commonFacade.Load(path + @"\common.facade.xml");
                    }
                }
            }

            listCtrls = commonFacade.GetElementsByTagName("controls").Item(0);
            
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

            //string test = GetDefaultValue("button", "posX", defNodes);
            //if (di.Exists)
            //{
            //    FileInfo[] fis = di.GetFiles("*.*");
            //    foreach (FileInfo f in fis)
            //    {
            //        sw.WriteLine("<BitmapImage x:Key=" + ((char)34) + f.Name + ((char)34) + " UriSource=" + ((char)34) + "Media\\" + f.Name + ((char)34) + "/>");
            //    }
            //}

            XmlNode group = null;
          
            // process the defines
            if(node!=null)
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
                    if (control.ChildNodes.Count > 0)
                    {
                        if (group==null)
                        {
                            foreach (XmlNode property in control.ChildNodes)
                            {
                                if (property.InnerText == "group")
                                {
                                    group = property;
                                    break;
                                }

                                XAMLWriter(sw, property, control);
                            }//foreach (XmlNode property in control.ChildNodes)
                        }
                        if(group!=null)
                        {
                            control = group;
                            group = null;
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
                                    XAMLWriter(sw, property, control, xVal, yVal);
                                }
                                if (yVal > 0)
                                    yVal += Convert.ToInt32(GetDefaultValue("button","height",defNodes));
                                control = control.NextSibling;
                            } while (control != null);
                        }
                    };//if (control.ChildNodes.Count > 0)
                };//foreach(XmlNode control in node.ChildNodes)
            }
            //
            //
            group = null;
            if (listCtrls != null)
            {
                foreach (XmlNode control1 in listCtrls.ChildNodes)
                {
                    XmlNode control = control1;
                    if (control.ChildNodes.Count > 0)
                    {
                        if (group == null)
                        {
                            foreach (XmlNode property in control.ChildNodes)
                            {
                                if (property.InnerText == "group")
                                {
                                    group = property;
                                    break;
                                }

                                XAMLWriter(sw, property, control);
                            }//foreach (XmlNode property in control.ChildNodes)
                        }
                        if (group != null)
                        {
                            control = group;
                            group = null;
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
                                foreach (XmlNode child in control.ChildNodes)
                                {
                                   foreach(XmlNode property in child.ChildNodes)
                                    XAMLWriter(sw, property, control, xVal, yVal);
                                }
                                if (yVal > 0)
                                    yVal += Convert.ToInt32(GetDefaultValue("button", "height", defNodes));
                                control = control.NextSibling;
                            } while (control != null);
                        }
                    };//if (control.ChildNodes.Count > 0)
                };//foreach(XmlNode control in node.ChildNodes)
            }

            //
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
                try
                {
                    int x = (FindNodeByName("posX", parent) != "" ? Convert.ToInt32(FindNodeByName("posX", parent)) : 0);
                    int y = (FindNodeByName("posX", parent) != "" ? Convert.ToInt32(FindNodeByName("posY", parent)) : 0);
                    posx = x.ToString();
                    posy = y.ToString();
                }
                catch { }
            }
            else
            {
                try
                {
                    int x = (FindNodeByName("posX", parent) != "" ? Convert.ToInt32(FindNodeByName("posX", parent)) : 0);
                    int y = (FindNodeByName("posX", parent) != "" ? Convert.ToInt32(FindNodeByName("posY", parent)) : 0);
                    x += baseX;
                    y += baseY;
                    posx = x.ToString();
                    posy = y.ToString();
                }
                catch { }
            }
            // process list control
            if (property.Name == "type")
            {
                if (typeArray.Contains(property.InnerText + " " + currentFile) == false)
                    typeArray.Add(property.InnerText+" "+currentFile);
            }
            if (property.InnerText == "listcontrol" && property.Name == "type")
            {
                sw.WriteLine("  <!-- " + FindNodeByName("description", parent) + " -->");
                sw.WriteLine("<ListView");
                string id="id" + FindNodeByName("id", parent);

                if (idArray.IndexOf(id) >= 0)
                {
                    int n = 0;
                    bool loop=true;
                    do
                    {
                        id += "_" + n.ToString();
                        if (idArray.IndexOf(id) >= 0)
                            n++;
                        else
                            loop = false;
                    } while (loop == true);

                }
                idArray.Add(id);
                sw.WriteLine("  Name=" + chr34 + id+ chr34);
                sw.WriteLine("  Canvas.Top=" + chr34 + GetDefaultValue("listcontrol", "posY", defNodes) + chr34);
                sw.WriteLine("  Canvas.Left=" + chr34 + GetDefaultValue("listcontrol", "posX", defNodes) + chr34);
                sw.WriteLine("  Width=" + chr34 + GetDefaultValue("listcontrol", "width", defNodes) + chr34);
                sw.WriteLine("  Height=" + chr34 + GetDefaultValue("listcontrol", "height", defNodes) + chr34);
                sw.WriteLine("  Style="+chr34+"{DynamicResource GUIListControl}" + chr34 );
                sw.WriteLine("/>");
                sw.WriteLine(sw.NewLine);
                return;
                
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

                if (FindNodeByName("texture", parent) == "background.png}")
                    return;//skin background image

                sw.WriteLine("  <!-- " + FindNodeByName("description", parent) + " -->");
                sw.WriteLine("<Image");
                string id = "id" + FindNodeByName("id", parent);

                if (idArray.IndexOf(id) >= 0)
                {
                    int n = 0;
                    bool loop = true;
                    do
                    {
                        id += "_" + n.ToString();
                        if (idArray.IndexOf(id) >= 0)
                            n++;
                        else
                            loop = false;
                    } while (loop == true);

                }
                idArray.Add(id);
                sw.WriteLine("  Name=" + chr34 + id + chr34); 
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
                string id = "id" + FindNodeByName("id", parent);

                if (idArray.IndexOf(id) >= 0)
                {
                    int n = 0;
                    bool loop = true;
                    do
                    {
                        id += "_" + n.ToString();
                        if (idArray.IndexOf(id) >= 0)
                            n++;
                        else
                            loop = false;
                    } while (loop == true);

                }
                idArray.Add(id);
                sw.WriteLine("  Name=" + chr34 + id + chr34); 
                string foreColor = FindNodeByName("textcolor", parent);
                if (foreColor == "")
                    foreColor = GetDefaultValue("label", "textcolor", defNodes);
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

                try
                {
                    if (Convert.ToInt32(labelText) > 0)
                        metaData = "labelNum:" + labelText;
                }
                catch { }
                string id = "id" + FindNodeByName("id", parent);

                if (idArray.IndexOf(id) >= 0)
                {
                    int n = 0;
                    bool loop = true;
                    do
                    {
                        id += "_" + n.ToString();
                        if (idArray.IndexOf(id) >= 0)
                            n++;
                        else
                            loop = false;
                    } while (loop == true);

                }
                idArray.Add(id);
                sw.WriteLine("  Name=" + chr34 + id + chr34); 
                sw.WriteLine("  Canvas.Top=" + chr34 + posy + chr34);
                sw.WriteLine("  Canvas.Left=" + chr34 + posx + chr34);
                sw.WriteLine("  Visibility=" + chr34 + (FindNodeByName("visible", parent) == "no" ? "Hidden" : "Visible") + chr34);

                if (FindNodeByName("width", parent) != "")
                    sw.WriteLine("  Width=" + chr34 + FindNodeByName("width", parent) + chr34);
                else
                    sw.WriteLine("  Width=" + chr34 + GetDefaultValue("button","width",defNodes) + chr34);

                if (FindNodeByName("height", parent) != "")
                    sw.WriteLine("  Height=" + chr34 + FindNodeByName("height", parent) + chr34);
                else
                    sw.WriteLine("  Height=" + chr34 + GetDefaultValue("button", "height", defNodes) + chr34);

                sw.WriteLine("  Style=" + chr34 + "{DynamicResource GUIButton}" + chr34);
                sw.WriteLine("  Tag=" + chr34 + "##id::" + FindNodeByName("id", parent) + "//##keycontrol::" + onLeft + onRight + onUp + onDown + "//##metadata::" + metaData + chr34);
                sw.WriteLine(">"+labelText+"</Button>");
                sw.WriteLine(sw.NewLine);
                return;
            }
            if (property.InnerText == "sortbutton" && property.Name == "type")
            {
                sw.WriteLine("  <!-- " + FindNodeByName("description", parent) + " -->");
                sw.WriteLine("<Button");
                string labelText = FindNodeByName("label", parent);
                if (labelText == "")
                    labelText = "textField";

                try
                {
                    if (Convert.ToInt32(labelText) > 0)
                        metaData = "labelNum:" + labelText;
                }
                catch { }
                string id = "id" + FindNodeByName("id", parent);

                if (idArray.IndexOf(id) >= 0)
                {
                    int n = 0;
                    bool loop = true;
                    do
                    {
                        id += "_" + n.ToString();
                        if (idArray.IndexOf(id) >= 0)
                            n++;
                        else
                            loop = false;
                    } while (loop == true);

                }
                idArray.Add(id);
                sw.WriteLine("  Name=" + chr34 + id + chr34);
                sw.WriteLine("  Canvas.Top=" + chr34 + posy + chr34);
                sw.WriteLine("  Canvas.Left=" + chr34 + posx + chr34);
                sw.WriteLine("  Visibility=" + chr34 + (FindNodeByName("visible", parent) == "no" ? "Hidden" : "Visible") + chr34);

                if (FindNodeByName("width", parent) != "")
                    sw.WriteLine("  Width=" + chr34 + FindNodeByName("width", parent) + chr34);
                else
                    sw.WriteLine("  Width=" + chr34 + GetDefaultValue("button", "width", defNodes) + chr34);

                if (FindNodeByName("height", parent) != "")
                    sw.WriteLine("  Height=" + chr34 + FindNodeByName("height", parent) + chr34);
                else
                    sw.WriteLine("  Height=" + chr34 + GetDefaultValue("button", "height", defNodes) + chr34);

                sw.WriteLine("  Style=" + chr34 + "{DynamicResource GUISortButton}" + chr34);
                sw.WriteLine("  Tag=" + chr34 + "##id::" + FindNodeByName("id", parent) + "//##keycontrol::" + onLeft + onRight + onUp + onDown + "//##metadata::" + metaData + chr34);
                sw.WriteLine(">" + labelText + "</Button>");
                sw.WriteLine(sw.NewLine);
                return;
            }
            // process button
            if (property.InnerText == "togglebutton" && property.Name == "type")
            {

                sw.WriteLine("  <!-- " + FindNodeByName("description", parent) + " -->");
                sw.WriteLine("<CheckBox");
                string labelText = FindNodeByName("label", parent);
                if (labelText == "")
                    labelText = "textField";

                try
                {
                    if (Convert.ToInt32(labelText) > 0)
                        metaData = "labelNum:" + labelText;
                }
                catch { }
                string id = "id" + FindNodeByName("id", parent);

                if (idArray.IndexOf(id) >= 0)
                {
                    int n = 0;
                    bool loop = true;
                    do
                    {
                        id += "_" + n.ToString();
                        if (idArray.IndexOf(id) >= 0)
                            n++;
                        else
                            loop = false;
                    } while (loop == true);

                }
                idArray.Add(id);
                sw.WriteLine("  Name=" + chr34 + id + chr34); 
                sw.WriteLine("  Canvas.Top=" + chr34 + posy + chr34);
                sw.WriteLine("  Canvas.Left=" + chr34 + posx + chr34);
                sw.WriteLine("  Visibility=" + chr34 + (FindNodeByName("visible", parent) == "no" ? "Hidden" : "Visible") + chr34);

                if (FindNodeByName("width", parent) != "")
                    sw.WriteLine("  Width=" + chr34 + FindNodeByName("width", parent) + chr34);
                else
                    sw.WriteLine("  Width=" + chr34 + GetDefaultValue("togglebutton", "width", defNodes) + chr34);

                if (FindNodeByName("height", parent) != "")
                    sw.WriteLine("  Height=" + chr34 + FindNodeByName("height", parent) + chr34);
                else
                    sw.WriteLine("  Height=" + chr34 + GetDefaultValue("togglebutton", "height", defNodes) + chr34);

                sw.WriteLine("  Style=" + chr34 + "{DynamicResource GUIToggleButton}" + chr34);
                sw.WriteLine("  Tag=" + chr34 + "##id::" + FindNodeByName("id", parent) + "//##keycontrol::" + onLeft + onRight + onUp + onDown + "//##metadata::" + metaData + chr34);
                sw.WriteLine(">" + labelText + "</CheckBox>");
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

        private string GetDefaultValue(string type, string property, XmlNode defaults)
        {
            XmlNode found=null;

            foreach (XmlNode prop in defaults.ChildNodes)
            {
                foreach (XmlNode loop in prop.ChildNodes)
                {
                    if (loop.Name == "type" && loop.InnerText == type)
                    {
                        found = prop;
                    }
                }
           }
           if (found == null)
               return "";
           
           string result="";
           foreach (XmlNode loop in found.ChildNodes)
           {
               if (loop.Name == property)
                   result = loop.InnerText;
           }
           return result;
        }

        public static string GetLocalizedString(string nodeName,string type, string property, XmlNode defaults)
        {
            XmlNode found = null;

            foreach (XmlNode prop in defaults.ChildNodes)
            {
                foreach (XmlNode loop in prop.ChildNodes)
                {
                    if (loop.Name == nodeName && loop.InnerText == type)
                    {
                        found = prop;
                    }
                }
            }
            if (found == null)
                return "";
            
            string prefix="";
            foreach(XmlAttribute attrib in found.Attributes)
            {
                if (attrib.Name == "Prefix")
                    prefix = attrib.Value;

            }
            
            string result = "";
            foreach (XmlNode loop in found.ChildNodes)
            {
                if (loop.Name == property)
                    result = loop.InnerText;
            }
            return prefix+result;
        }

        public static string SplitElementTag(string tag,string returnValue,string data)
        {
            string[] splitt = tag.Split(new string[] { "//" },StringSplitOptions.RemoveEmptyEntries);
            if (splitt.Length > 0)
            {
                foreach (string splitString in splitt)
                {
                    string[] props = splitString.Split(new string[] { "::" }, StringSplitOptions.RemoveEmptyEntries);
                    
                    if (props.Length > 1)
                    {
                        if (props[0]==data && props[1].StartsWith(returnValue))
                        {
                            string[] values = props[1].Split(new char[] { ':' });
                            if (values.Length == 2)
                                return values[1];
                        }
                    }
                }
            }
            return "";
        }

        internal void LoadPlugin(Type pluginType)
        {
            if (pluginType == typeof(MyPictures))
            {
                MyPictures myPic = new MyPictures(this.Resources);
                myPic.InitializeComponent();
                this.Navigate(myPic);
            }
        }
    }
}