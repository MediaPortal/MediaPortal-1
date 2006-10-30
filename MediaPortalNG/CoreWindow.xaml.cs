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
using System.Xml.XPath;
using System.Collections;

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
        private static XmlDocument _langDoc;
        private bool _localizationDone;
        private string _language;
        private DirectoryInfo[] _languages;
        private GUIDialog _langDialog;
        private ArrayList _currentControls;
        private Type _currentPlugin;
        private static GUIWindow _guiWindow;

        /// <summary>
        /// The MediaPortal core. The Core always loads the HomeExtension as start point.
        /// </summary>
        public Core()
        {
            this.Loaded += new RoutedEventHandler(Core_Loaded);
            this.Width = 720;
            this.Height = 642;
            this.ShowsNavigationUI = true;
            // params
            this.Background = Brushes.Black;
            this.Title = "MediaPortalNG";
            this.Show();
            this.Navigating += new NavigatingCancelEventHandler(Core_Navigating);
            this.KeyDown += new System.Windows.Input.KeyEventHandler(Core_KeyDown);
            _home = new HomeExtension(this.Resources);
            _home.InitializeComponent();
            this.Navigate(_home);

            //WriteNewProperty("Column0", "int", "");
            //WriteNewProperty("Column1", "int", "");
            //WriteNewProperty("Column2", "int", "");
            //WriteNewProperty("ButtOnDownWidth", "int", "");
            //WriteNewProperty("ButtonContentValue", "int", "");
            //WriteNewProperty("ShowRange", "bool","");
            //WriteNewProperty("Reverse", "bool","");
            //WriteNewProperty("WindowID","int","");
            //WriteNewProperty("DefaultControl", "int", "");
            //WriteNewProperty("WindowName", "string", "");
            //WriteNewProperty("AllowOverlay", "bool", "");



            string writeAfter ="string IGUIControl.Label";
            string data = @"       protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            if (_align == TextAlignment.Right)
                Canvas.SetLeft(this, _positionX - sizeInfo.NewSize.Width);
        }";
            //WriteContentCode(writeAfter, data);
            
        }

        public string SelectedLanguage
        {
            get { return _language; }
            set { SetGUILanguage(value); }
        }
 
        public static void OnClick(object sender)
        {
            IGUIControl ctrl = sender as IGUIControl;
            if (ctrl != null)
            {
                int id = ctrl.ID;
                int a = 0;
            }
        }

        private void WriteNewProperty(string name,string type,string data)
        {
            string files = @"E:\mpng\Controls\";
            //string[] nameSources = new string[] { "GUIHVScrollBar","GUITextbox","GUIListControl", "GUIAnimation", "GUIButton", "GUICheckMark", "GUIDateTime", "GUIFadelabel", "GUIImage", "GUILabel", "GUIProgress", "GUISelectButton", "GUISpinControl", "GUITextboxScrollUp", "GUIToggleButton" };
            string[] nameSources = new string[] { "GUIWindow" };
            string compare = "private ***TYPE*** _***PROPERTY***;";
            compare = compare.Replace("***TYPE***", type);
            compare = compare.Replace("***PROPERTY***", name);
            string property = @"        
        //
        // property ***PROPERTY***
        // 
        private ***TYPE*** _***PROPERTY***;

        public ***TYPE*** ***PROPERTY***
        {
            get
            {
                return (***TYPE***)GetValue(***PROPERTY***Property);
            }
            set
            {
                SetValue(***PROPERTY***Property,value);
            }
        }

        public static readonly DependencyProperty ***PROPERTY***Property =
        DependencyProperty.Register(" + chr34 + "***PROPERTY***" + chr34 + @", typeof(***TYPE***), typeof(***CLASS***),
            new FrameworkPropertyMetadata(new PropertyChangedCallback(On***PROPERTY***Changed)));

        private static void On***PROPERTY***Changed(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            ***CLASS*** control = (***CLASS***)obj;
            control.On***PROPERTY***Changed(args);
        }

        protected virtual void On***PROPERTY***Changed(DependencyPropertyChangedEventArgs args)
        {
            _***PROPERTY***=(***TYPE***)args.NewValue;
            
        }
";
            StreamReader sr;

            foreach (string f in nameSources)
            {
                string finalCode = property.Replace("***TYPE***", type);
                finalCode = finalCode.Replace("***PROPERTY***", name);
                finalCode = finalCode.Replace("***CLASS***", f);
                if(data!="")
                    finalCode = finalCode.Replace("***DATA***", data);
                // open source file
                string fileName = files + f + ".cs";
                sr = new StreamReader(fileName);
                string oldContent=sr.ReadToEnd();
                sr.Close();
                if (oldContent.IndexOf(compare) >= 0)
                    continue;
                // remove class and namespace closer
                oldContent = oldContent.Substring(0, oldContent.LastIndexOf("}") - 1);
                oldContent = oldContent.Substring(0, oldContent.LastIndexOf("}") - 1);
                // add new property
                oldContent += finalCode;
                // write file
                if (File.Exists(fileName + ".old") == true) 
                    File.Delete(fileName + ".old");
                
                File.Move(fileName,fileName+".old");
                File.Delete(fileName);
                StreamWriter sw = new StreamWriter(fileName);
                sw.Write(oldContent);
                sw.WriteLine("}");
                sw.WriteLine("}");
                sw.WriteLine("");
                sw.Close();
            }
        }

        private void WriteContentCode(string writeAfter, string data)
        {
            string files = @"E:\mpng\Controls\";
            string[] nameSources = new string[] { "GUIListControl","GUIAnimation", "GUIButton", "GUICheckMark", "GUIDateTime", "GUIFadelabel", "GUIImage", "GUILabel", "GUIProgress", "GUISelectButton", "GUISpinControl", "GUITextboxScrollUp", "GUIToggleButton" };

            StreamReader sr;

            foreach (string f in nameSources)
            {
                // open source file
                string fileName = files + f + ".cs";
                sr = new StreamReader(fileName);
                string oldContent = sr.ReadToEnd();
                sr.Close();
                int pos=oldContent.IndexOf(writeAfter);
                if (pos < 0 || oldContent.IndexOf(data)>=0)
                    continue;

                // remove class and namespace closer
                // add new property
                oldContent=oldContent.Insert(pos,"\r\n"+data+"\r\n");
                // write file
                if (File.Exists(fileName + ".old") == true)
                    File.Delete(fileName + ".old");

                File.Move(fileName, fileName + ".old");
                File.Delete(fileName);
                StreamWriter sw = new StreamWriter(fileName);
                sw.Write(oldContent);
                sw.Close();
            }
        }

        void Core_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            GUIWindow window = ((FrameworkElement)this.Content).FindName("GUIWindow") as GUIWindow;
            if (e.Key == System.Windows.Input.Key.F8)
            {
                if (_languages != null)
                {
                    _langDialog = new GUIDialog("Select Language", this);
                    foreach (DirectoryInfo di in _languages)
                    {
                        
                        _langDialog.AddMenuItem(di.Name);
                    }

                    int result = _langDialog.ShowDialog();
                    if (result >= 0)
                    {
                        if (SetGUILanguage(_languages[result].Name) == true)
                        {
                            FrameworkElement vis = (FrameworkElement)this.Content;
                            GUIWindow canvas = vis.FindName("GUIWindow") as GUIWindow;

                            if(canvas!=null)
                                ApplyLanguage(canvas);
                        }
                    }
                }
                return;
            }
            if (window != null)
            {
                window.HandleKeyDown(e.Key);
            }
            if (_home!=null)
            {
                _home.HandleKeyDown(sender, e);
                e.Handled = true;
            }
        }

         void Core_Navigating(object sender, NavigatingCancelEventArgs e)
        {
            object content = e.Content;
            if (content == null) return;

            if (content.GetType() == typeof(MediaPortal.HomeExtension))
            {
                // we get here on navigating
            }
            
       }

        void Core_Loaded(object sender, RoutedEventArgs e)
        {
            //ParseSkinXML(@"E:\mpng\bin\Debug\BlueTwo", "mymusicsongs.xml");
            GetGUILanguages();
            SelectedLanguage = "German";
            LoadSkin();

        }

        
         public void LoadSkin()
        {


        
        }
        void ApplyLanguage(Canvas elementsContainer)
        {
            ApplyGUIControlsLanguage(elementsContainer);
        }

        /// <summary>
        /// This methode will check all controls for the label-property and force re-set to apply the new language
        /// </summary>
        /// <param name="container"></param>
        /// <returns>object</returns>
        void ApplyGUIControlsLanguage(Visual container)
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(container); i++)
            {
                Visual child = (Visual)VisualTreeHelper.GetChild(container, i);
                IGUIControl ctrl = child as IGUIControl;
                if (ctrl != null)
                {
                    string label = ctrl.Label;
                    if (label != null)
                    {
                        ctrl.Label = "";// delete old to force
                        ctrl.Label = label;// update the language
                    }
                }
                // check children 
                ApplyGUIControlsLanguage(child);
            }
        }

        public static Visual FindVisualByName(Visual parent,string name)
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                
                Visual child = (Visual)VisualTreeHelper.GetChild(parent, i);
                if ((string)child.GetValue(NameProperty) == name)
                    return child;
                // check children 
                Visual found=FindVisualByName(child,name);
                if (found!=null) 
                    return found;
            }
            return null;
        }

        public static Visual FindVisualByID(Visual parent, int id)
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {

                Visual child = (Visual)VisualTreeHelper.GetChild(parent, i);
                IGUIControl ctrl = child as IGUIControl;
                if(ctrl!=null)
                if (ctrl.ID == id)
                    return child;
                // check children 
                Visual found = FindVisualByID(child, id);
                if (found != null)
                    return found;
            }
            return null;
        }

        public static Visual FindVisualByKeyboardFocused(Visual parent)
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {

                Visual child = (Visual)VisualTreeHelper.GetChild(parent, i);
                if(((FrameworkElement)child).IsKeyboardFocused==true)
                    return child;
                // check children 
                Visual found = FindVisualByKeyboardFocused(child);
                if (found != null)
                    return found;
            }
            return null;
        }

        private void ParseSkinXML(string path,string file)
        {
            string header1 = "<Page xmlns=" + chr34 + "http://schemas.microsoft.com/winfx/2006/xaml/presentation" + chr34;
            string header2="xmlns:x=" + chr34 + "http://schemas.microsoft.com/winfx/2006/xaml" + chr34 ;
            string header3 = "xmlns:ctrl=" + chr34 + "clr-namespace:MediaPortal" + chr34 ;
            string header4 = "x:Class=" + chr34 + "MediaPortal.HomeExtension" + chr34 + ">";
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
            XmlNode node =doc.SelectSingleNode("/window/controls");
            XmlNode nodeList = doc.SelectSingleNode("/window/controls");

            defNodes=defaults.SelectSingleNode("/controls");
            XmlNodeList nList = nodeList.SelectNodes("import");

            foreach (XmlNode property in nList)
            {
                if (property.Name == "import")
                {
                    if (property.InnerText == "common.window.xml" ||
                        property.InnerText == "common.settings.xml" ||
                        property.InnerText == "common.time.xml")
                    {
                        commonWindow.Load(path + @"\" + property.InnerText);
                        XmlNode newnode = doc.ImportNode(commonWindow.SelectSingleNode("/window/controls"), true);
                        node.AppendChild(newnode);
                    }
                    else
                    {
                        commonFacade.Load(path + @"\common.facade.xml");
                        XmlNode newnode=doc.ImportNode(commonFacade.SelectSingleNode("/window/controls"),true);
                        node.AppendChild(newnode);
                    }
                }
            }

            
            // for common.xxxxxx.xml file

            StreamWriter sw = File.CreateText(@"E:\testing.xaml");//path + @"\" + file + "
            sw.WriteLine(header1);
            sw.WriteLine(header2);
            sw.WriteLine(header3);
            sw.WriteLine(header4);
            sw.WriteLine(sw.NewLine);

            int a = 0;
            XmlNode winNode=doc.SelectSingleNode("window");

            // write the gui window properties

            if ( winNode!= null)
            {
                sw.WriteLine("<ctrl:GUIWindow");
                sw.WriteLine("  Style=" + chr34 + "{DynamicResource PageBackground}" + chr34);
                if (winNode.SelectSingleNode("allowoverlay") != null)
                {
                    sw.WriteLine(" AllowOverlay=" + chr34 + GetBoolValue(winNode.SelectSingleNode("allowoverlay").InnerText) + chr34);
                }
                if (winNode.SelectSingleNode("defaultcontrol") != null)
                {
                    sw.WriteLine(" DefaultControl=" + chr34 + winNode.SelectSingleNode("defaultcontrol").InnerText + chr34);
                }
                if (winNode.SelectSingleNode("id") != null)
                {
                    sw.WriteLine(" WindowID=" + chr34 + winNode.SelectSingleNode("id").InnerText + chr34);
                }
                sw.WriteLine(" x:Name=" + chr34 + "GUIWindow" + chr34);
                // close window tag
                sw.WriteLine(">");
                sw.WriteLine("");
            }


            XmlNodeList defList = doc.SelectNodes("/window/define");
            XmlNode defaultControl = doc.SelectSingleNode("/window/defaultcontrol");

            // loop imported nodes from common.xyz.xml files
            XmlNodeList commNodes = node.SelectNodes("controls");
            XmlNode foundNode =null;
            // process the defines
            if (defList != null)
            {
                foreach (XmlNode child in defList)
                {
                    string[] define = child.InnerText.Split(new char[] { ':' });

                    // search doc
                    foundNode = FindNodeByInnerText(define[0], node);
                    if (foundNode != null)
                    {
                        foreach (XmlNode property in foundNode.ChildNodes)
                        {
                            if (property.InnerText == define[0])
                                property.InnerText = define[1];
                        }
                    }


                    // search new nodes
                    foreach (XmlNode impNode in commNodes)
                    {
                        foundNode = FindNodeByInnerText(define[0], impNode);
                        if (foundNode != null)
                        {
                            foreach (XmlNode property in foundNode.ChildNodes)
                            {
                                if (property.InnerText == define[0])
                                    property.InnerText = define[1];
                            }
                        }
                    }

                }

            }
            XmlWriter xx = XmlWriter.Create("E:\\tmp.xml");
            doc.WriteTo(xx);
            xx.Close();
            // loop the controls
            if (node != null)
            {
                // loop for the imported nodes
                foreach (XmlNode impControl in commNodes)
                {
                    foreach (XmlNode ctrl in impControl.ChildNodes)
                    {
                        if (ctrl.Name != "control")
                            continue;

                        WriteXAML(sw, ctrl, false);
                    }
                }

                foreach(XmlNode control in node.ChildNodes)
                {
                    if(control.Name!="control")
                        continue;

                    XmlNode typeNode = control.SelectSingleNode("type");
                    if(typeNode==null)
                        continue;

                    string ctrlType = typeNode.InnerText;

                    if (ctrlType == "group")
                    {
                        // we need an stack-panel in the xaml page
                        string width=chr34+GetDefaultValue("button", "width", defNodes)+chr34;
                        string newline = "<StackPanel Width="+width+" Canvas.Top=" + FindNodeByName("posY", control) + " Canvas.Left=" + FindNodeByName("posX", control) + ">";
                        sw.WriteLine(newline);
                        XmlNodeList nextNodes = control.SelectNodes("control");
                        foreach (XmlNode nextNode in nextNodes)
                        {
                            WriteXAML(sw, nextNode,true);
                        }
                        sw.WriteLine("</StackPanel>");
                    }
                    else
                        WriteXAML(sw, control,false);


                };//foreach(XmlNode control in node.ChildNodes)


            }
            //
            //

            //
            sw.WriteLine("</ctrl:GUIWindow>");
            sw.WriteLine("</Page>");
            sw.Close();
            int x = 0;

        }

        private void WriteXAML(StreamWriter sw, XmlNode control, bool groupCall)
        {
            XmlNode typeNode = control.SelectSingleNode("type");
            if (typeNode == null)
                return;

            string ctrlType = typeNode.InnerText;
            string ctrlTag = "";
            switch (ctrlType)
            {
                case "vscrollbar":
                case "hscrollbar":
                    ctrlTag = "<ctrl:GUIHVScrollBar";
                    break;
                case "textbox":
                    ctrlTag = "<ctrl:GUITextBox";
                    break;
                case "animation":
                    ctrlTag = "<ctrl:GUIAnimation";
                    break;
                case "button":
                    ctrlTag = "<ctrl:GUIButton";
                    break;
                case "progress":
                    ctrlTag = "<ctrl:GUIProgress";
                    break;
                case "textboxscrollup":
                    ctrlTag = "<ctrl:GUITextboxScrollUp";
                    break;
                case "listcontrol":
                    ctrlTag = "<ctrl:GUIListControl";
                    break;
                case "checkmark":
                    ctrlTag = "<ctrl:GUICheckMark";
                    break;
                case "fadelabel":
                    ctrlTag = "<ctrl:GUIFadelabel";
                    break;
                case "image":
                    ctrlTag = "<ctrl:GUIImage";
                    break;
                case "selectbutton":
                    ctrlTag = "<ctrl:GUISelectButton";
                    break;
                case "sortbutton":
                    ctrlTag = "<ctrl:GUIButton";
                    break;
                case "label":
                    ctrlTag = "<ctrl:GUILabel";
                    break;
                case "spincontrol":
                    ctrlTag = "<ctrl:GUISpinControl";
                    break;
                case "togglebutton":
                    ctrlTag = "<ctrl:GUIToggleButton";
                    break;
            }
            if (ctrlTag == "") return;

            if (control.SelectSingleNode("label") != null && ctrlType == "label")
            {
                XmlNode node = control.SelectSingleNode("label");
                if (node != null)
                {
                    if (node.InnerText == "#date" || node.InnerText=="#time")
                    {
                        ctrlTag = "<ctrl:GUIDateTime Format=" + chr34 + node.InnerText + chr34;
                    }
                }
            }

            sw.WriteLine("");
            sw.WriteLine(ctrlTag);

            // common properties
            string x="", y="", w, h;

            XmlNode width = control.SelectSingleNode("width");
            XmlNode height = control.SelectSingleNode("height");
            XmlNode posx = control.SelectSingleNode("posX");
            XmlNode posy = control.SelectSingleNode("posY");

            if (groupCall == false)
            {
                if (posx == null)
                    x = GetDefaultValue(ctrlType, "posX", defNodes);
                else
                    x = posx.InnerText;

                if (posy == null)
                    y = GetDefaultValue(ctrlType, "posY", defNodes);
                else
                    y = posy.InnerText;
            }
            if (width == null)
                w = GetDefaultValue(ctrlType, "width", defNodes);
            else
                w = width.InnerText;

            if (height == null)
                h = GetDefaultValue(ctrlType, "height", defNodes);
            else
                h = height.InnerText;

            if(y!="")
                sw.WriteLine(" PosY=" + chr34 + y + chr34);
            if(x!="")
                sw.WriteLine(" PosX=" + chr34 + x + chr34);
            if(w!="")
            sw.WriteLine(" Width=" + chr34 + w + chr34);
            if(h!="")
            sw.WriteLine(" Height=" + chr34 + h + chr34);

            //
            try
            {
                sw.WriteLine(" ID=" + chr34+control.SelectSingleNode("id").InnerText+chr34);
            }
            catch { }
            try
            {
                sw.WriteLine(" OnUp=" + chr34 + control.SelectSingleNode("OnUp").InnerText+chr34);
            }
            catch { }
            try
            {
                sw.WriteLine(" OnDown=" + chr34 + control.SelectSingleNode("OnDown").InnerText + chr34);
            }
            catch { }
            try
            {
                sw.WriteLine(" OnRight=" + chr34 + control.SelectSingleNode("OnRight").InnerText + chr34);
            }
            catch { }
            try
            {
                sw.WriteLine(" OnLeft=" + chr34 + control.SelectSingleNode("OnLeft").InnerText + chr34);
            }
            catch { }

            if (control.SelectSingleNode("label") != null)
            {
                sw.WriteLine(" Label=" + chr34+control.SelectSingleNode("label").InnerText + chr34);
            }

            if (control.SelectSingleNode("hyperlink") != null)
            {
                sw.WriteLine(" Hyperlink=" + chr34+control.SelectSingleNode("hyperlink").InnerText + chr34);
            }
            if (control.SelectSingleNode("texture") != null)
            {
                sw.WriteLine(" Texture=" + chr34 + control.SelectSingleNode("texture").InnerText + chr34);
            }
            if (control.SelectSingleNode("orientation") != null)
            {
                sw.WriteLine(" Orientation=" + chr34 + control.SelectSingleNode("orientation").InnerText + chr34);
            }
            if (control.SelectSingleNode("reverse") != null)
            {
                sw.WriteLine(" Reverse=" + chr34 + GetBoolValue(control.SelectSingleNode("reverse").InnerText) + chr34);
            }
            if (control.SelectSingleNode("showrange") != null)
            {
                sw.WriteLine(" ShowRange=" + chr34 + GetBoolValue(control.SelectSingleNode("showrange").InnerText) + chr34);
            }

            if (ctrlType=="vscrollbar")
            {
                sw.WriteLine(" Orientation=" + chr34 + "Vertical" + chr34);
            }
            if (ctrlType == "hscrollbar")
            {
                sw.WriteLine(" Orientation=" + chr34 + "Horizontal" + chr34);
            }
            if (control.SelectSingleNode("visible") != null)
            {
                if(control.SelectSingleNode("visible").InnerText.ToLower()=="no")
                    sw.WriteLine(" Visibility=" + chr34 + "Hidden" + chr34);
            }

            if (control.SelectSingleNode("align") != null)
            {
                sw.WriteLine(" Align=" + chr34 + control.SelectSingleNode("align").InnerText + chr34);
            }

            //
            if (HasProperty(ctrlType, "textcolor", defNodes))
            {
                XmlNode node = control.SelectSingleNode("textcolor");
                string textcolor="";
                if (node != null)
                {
                    textcolor = node.InnerText;
                }
                else
                    textcolor = GetDefaultValue(ctrlType, "textcolor", defNodes);

                textcolor = ColorConvert(textcolor);

                if (textcolor != "")
                    sw.WriteLine(" Foreground=" + chr34 + textcolor + chr34);

            }
            if (HasProperty(ctrlType, "disabledcolor", defNodes))
            {
                XmlNode node = control.SelectSingleNode("disabledcolor");
                string textcolor = "";
                if (node != null)
                {
                    textcolor = node.InnerText;
                }
                else
                    textcolor = GetDefaultValue(ctrlType, "disabledcolor", defNodes);

                textcolor = ColorConvert(textcolor);

                if (textcolor != "")
                    sw.WriteLine(" DisabledColor=" + chr34 + textcolor + chr34);

            }
            if (HasProperty(ctrlType, "font", defNodes))
            {
                XmlNode node = control.SelectSingleNode("font");
                string font = "";
                if (node != null)
                {
                    font = node.InnerText;
                }
                else
                    font = GetDefaultValue(ctrlType, "font", defNodes);

                font = ((int)(Convert.ToInt32(font.Substring(4))+3)).ToString();
                if (font != "")
                    sw.WriteLine(" FontSize=" + chr34 + font + chr34);

            }

            sw.WriteLine("/>");
            sw.WriteLine("");
        }

        string GetBoolValue(string org)
        {
            string compare = org.ToLower();
            if (compare == "no" || compare == "n" || compare == "0")
                return "false";
            if (compare == "yes" || compare == "y" || compare == "1")
                return "true";
            return "false";
        }

        string ColorConvert(string skinColor)
        {
            string color = "";
            try
            {
                if (Convert.ToUInt32(skinColor, 16) >= 0)
                    color = "#" + skinColor;
            }
            catch
            {
                color = skinColor;
            }
            if (skinColor.StartsWith("#") == true)
                return "#00000000"; // return transparent
            return color;
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
                    return chr34 + property.InnerText + chr34;
                }
            };//foreach (XmlNode property in control.ChildNodes)
            return "";
        }

        private bool HasProperty(string type, string property, XmlNode defaults)
        {
            if (defaults == null) return false;
            foreach (XmlNode child in defaults.ChildNodes)
            {
                XmlNode ctrlType = child.SelectSingleNode("type");
                if (ctrlType != null)
                {
                    if (ctrlType.InnerText == type)
                    {
                        if (child.SelectSingleNode(property) != null)
                            return true;
                        else
                            return false;
                    }

                }
            }
            return false;

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

        private bool GetGUILanguages()
        {
            string dir = Directory.GetCurrentDirectory() + "\\language";
            if (Directory.Exists(dir))
            {
                DirectoryInfo di= new DirectoryInfo(dir);
                ArrayList tmp = new ArrayList();
                foreach (DirectoryInfo d in di.GetDirectories())
                {
                    if ((d.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden)
                    {
                        foreach (FileInfo f in d.GetFiles())
                        {
                            if (f.Name == "strings.xml")
                            {
                                tmp.Add(d);
                                break;
                            }
                        }
                    }
                }
                _languages = new DirectoryInfo[tmp.Count];
                tmp.CopyTo(_languages);
                return true;
            }
            return false;
        }

        protected bool SetGUILanguage(string lang)
        {
            foreach (DirectoryInfo di in _languages)
            {
                if (di.ToString() == lang)
                {
                    _langDoc = new XmlDocument();
                    try
                    {
                        _langDoc.Load(di.FullName + "\\strings.xml");
                        _language = lang;
                    }
                    catch { _language = ""; return false; }
                    return true;
                }
            }
            return false;
         }

        public static string GetLocalizedString(string search)
        {
            XmlNode found = null;
            if (_langDoc == null) return search;

            XmlNode defaults = _langDoc.SelectSingleNode("strings");

            foreach (XmlNode prop in defaults.ChildNodes)
            {
                foreach (XmlNode loop in prop.ChildNodes)
                {
                    if (loop.Name == "id" && loop.InnerText == search)
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
                if (loop.Name == "value")
                    result = loop.InnerText;
            }
            return prefix+result;
        }

  
        internal void LoadPlugin(Type pluginType)
        {
            if (pluginType == typeof(MyPictures))
            {
                MyPictures myPic = new MyPictures();
                _currentPlugin = pluginType;
                myPic.InitializeComponent();
                this.Navigate(myPic);
            }
        }

  
    }
}