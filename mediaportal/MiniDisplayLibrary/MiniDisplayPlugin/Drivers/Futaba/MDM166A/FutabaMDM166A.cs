#region Copyright (C) 2005-2012 Team MediaPortal
// Copyright (C) 2005-2012 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

#region usings

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using System.Windows.Forms;
using System.IO;
using System.ComponentModel;
using System.Data;
using System.Xml;
using System.Runtime.InteropServices;
using System.Threading;
using System.Net.NetworkInformation;
using MediaPortal.GUI.Library;
using MediaPortal.Player;
using MediaPortal.ProcessPlugins.MiniDisplayPlugin.MiniDisplayPlugin.VFD_Control;

#endregion usings


namespace MediaPortal.ProcessPlugins.MiniDisplayPlugin.Drivers
{
    /// <summary>
    /// LED display character set class
    /// </summary>
    /// 
    [System.ComponentModel.DesignerCategory("Code")]
    public class LedDisplay : Control, ISupportInitialize
    {
        #region declarations

        // increment scroll offset by pixel or segment
        public enum Step
        {
            Pixel = 0,
            Segment = 1
        }
        // matrix size, must not exceed x*y = 64 bits = ulong
        const float HSEGMENTS = 6.0f;       // number of horizontal segments
        const float VSEGMENTS = 8.0f;       // number vertical segments
        private Dictionary<char, ulong> segDict = new Dictionary<char, ulong> {};     
        private Bitmap displayBitmap;       // keeps a bitmap for the string to display
        private float segmentSpace;         // space between segments
        private float charSpace;            // space between characters
        private bool scroll;                // true or false
        private int scrollDelay;            // timer ticks between scroll movement
        private int scrollOffset;           // horizontal offset when scrolling
        private Step scrollStep;            // step when scrolling
        private string xmlFile;             // character defintion file

        private System.Windows.Forms.Timer scrollTimer = new System.Windows.Forms.Timer();

        [Browsable(false)]
        public float HorizontalSegments // number
        {
            get { return HSEGMENTS; }
            set { }
        }

        [Browsable(false)]
        public float VerticalSegments
        {
            get { return VSEGMENTS; }
            set { }
        }

        [DefaultValue(20), Description("Space in % between segments"), Category("Apperance")]
        public int SegmentSpace
        {
            get
            {
                return (int)(segmentSpace * 100.0f);
            }
            set
            {
                if (segmentSpace == value)
                    return;
                segmentSpace = Math.Abs(value / 100.0f);
                if (!isInitializing)
                {
                    Invalidate();
                }
            }
        }

        // space between characters could be down to zero
        [DefaultValue(20), Description("Space in % between characters"), Category("Apperance")]
        public int CharSpace
        {
            get
            {
                return (int)(charSpace * 100.0f);
            }
            set
            {
                if (charSpace == value)
                    return;
                charSpace = Math.Abs(value / 100.0f);
                if (!isInitializing)
                {
                    Invalidate();
                }
            }
        }

        [DefaultValue(false), Category("Appearance"), Description("Scroll text")]
        public bool Scroll
        {
            get { return scroll; }
            set
            {
                scroll = value;
                if (scroll == true && !DesignMode)  // don't start timer in design mode
                    scrollTimer.Start();
                else
                    scrollTimer.Stop();
            }
        }

        // windows timers accuracy is depending on the computer
        // a medium computer has average minimum of 15-20 milliseconds between ticks
        [DefaultValue(25), Category("Appearance"), Description("Scroll delay in milliseconds.")]
        public int ScrollDelay
        {
            get { return scrollDelay; }
            set
            {
                scrollDelay = value < 5 ? 5 : value;    // minimum scroll delay 
                scrollTimer.Interval = scrollDelay;
            }
        }

        [DefaultValue(typeof(Step), "Pixel"), Category("Appearance"), Description("Scroll step, pixel or segment")]
        public Step ScrollStep
        {
            get
            {
                return scrollStep;
            }
            set
            {
                scrollStep = value;
                if (!isInitializing)
                {
                    Invalidate();
                }
            }
        }

        [Browsable(true), Description("Text shown in control"), Category("Appearance")]
        public override string Text
        {
            get
            {
                return base.Text;
            }
            set
            {
                base.Text = value;
                if (!isInitializing)
                {
                    Invalidate();
                }
            }
        }

        [Browsable(true), Description("BackColor"), Category("Appearance")]
        public override Color BackColor
        {
            get
            {
                return base.BackColor;
            }
            set
            {
                base.BackColor = value;
                if (!isInitializing)
                {
                    Invalidate();
                }
            }
        }

        // Does not support background image, use transparency and paint the background
        [Browsable(false)]
        public override Image BackgroundImage
        {
            get
            {
                return base.BackgroundImage;
            }
            set
            {
                base.BackgroundImage = null;
            }
        }

        // and not background image layout
        [Browsable(false)]
        public override ImageLayout BackgroundImageLayout
        {
            get
            {
                return base.BackgroundImageLayout;
            }
            set
            {
                base.BackgroundImageLayout = ImageLayout.None;
            }
        }

        #endregion declarations

        #region Initialize

        /// <summary>
        /// Constructor
        /// </summary>
        public LedDisplay()
        {
            //Control styles für optimierte Anzeige definieren
            this.SetStyle(
            ControlStyles.AllPaintingInWmPaint |
            ControlStyles.UserPaint |
            ControlStyles.DoubleBuffer |
            ControlStyles.ResizeRedraw |
            ControlStyles.SupportsTransparentBackColor, true);

            InitializeComponent();

            //Initialize scroll timer for texts that are longer than maximum characters in display
            scrollTimer.Interval = scrollDelay;
            scrollTimer.Tick += new System.EventHandler(this.scrollTimer_Tick);
            scrollTimer.Enabled = false;
            
            // this file is needed in design mode as well, thats why it's stored in 
            // the common program files folder
            // application.startuppath is not available in design mode
            // if the file not exists, just create an empty file
            DirectoryInfo di = Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.CommonProgramFiles) + "\\FutabaMDM166AFont\\");
            xmlFile = Environment.GetFolderPath(Environment.SpecialFolder.CommonProgramFiles) + "\\FutabaMDM166AFont\\LedFont.xml";
            if ((!File.Exists(xmlFile)) && di.Exists)
            {
              //Fill character dictionary with most common characters
              FillCharDictFromStandard();
              //Create new character definition file
              WriteCharDef();
            }
            ReadCharDef();
        }

        /// <summary>
        /// Component initialization
        /// </summary>
        private void InitializeComponent()
        {
            displayBitmap = null;
            this.ForeColor = Color.LightGreen;
            this.BackColor = Color.Black;
            segmentSpace = 0.2f;
            charSpace = 0.2f;
            scroll = false;
            scrollOffset = 0;
            scrollDelay = 25;
            scrollStep = Step.Pixel;
        }

        /// <summary>
        /// Fills character dictionary with most common characters
        /// </summary>
        private void FillCharDictFromStandard()
        {
            segDict.Add(Convert.ToChar("A"), Convert.ToUInt64(227873781661662));
            segDict.Add(Convert.ToChar("Ä"), Convert.ToUInt64(228698217246771));
            segDict.Add(Convert.ToChar("B"), Convert.ToUInt64(422199034855391));
            segDict.Add(Convert.ToChar("C"), Convert.ToUInt64(9565685714534398));
            segDict.Add(Convert.ToChar("D"), Convert.ToUInt64(140724398931935));
            segDict.Add(Convert.ToChar("E"), Convert.ToUInt64(281410551955455));
            segDict.Add(Convert.ToChar("F"), Convert.ToUInt64(9020603847426047));
            segDict.Add(Convert.ToChar("G"), Convert.ToUInt64(417801450766302));
            segDict.Add(Convert.ToChar("H"), Convert.ToUInt64(228785541988957427));
            segDict.Add(Convert.ToChar("I"), Convert.ToUInt64(9223144176159343372));
            segDict.Add(Convert.ToChar("J"), Convert.ToUInt64(3513788460580408368));
            segDict.Add(Convert.ToChar("K"), Convert.ToUInt64(227860354645235));
            segDict.Add(Convert.ToChar("L"), Convert.ToUInt64(281409529589955));
            segDict.Add(Convert.ToChar("M"), Convert.ToUInt64(227860698627297));
            segDict.Add(Convert.ToChar("N"), Convert.ToUInt64(227860832812275));
            segDict.Add(Convert.ToChar("O"), Convert.ToUInt64(136326352420830));
            segDict.Add(Convert.ToChar("Ö"), Convert.ToUInt64(136326355476531));
            segDict.Add(Convert.ToChar("P"), Convert.ToUInt64(13404056010719));
            segDict.Add(Convert.ToChar("Q"), Convert.ToUInt64(277072430710750));
            segDict.Add(Convert.ToChar("R"), Convert.ToUInt64(227860363034591));
            segDict.Add(Convert.ToChar("S"), Convert.ToUInt64(140721356816382));
            segDict.Add(Convert.ToChar("ß"), Convert.ToUInt64(15380068121822));
            segDict.Add(Convert.ToChar("T"), Convert.ToUInt64(53614281281535));
            segDict.Add(Convert.ToChar("U"), Convert.ToUInt64(136326352420083));
            segDict.Add(Convert.ToChar("Ü"), Convert.ToUInt64(136326352416819));
            segDict.Add(Convert.ToChar("V"), Convert.ToUInt64(54906657389811));
            segDict.Add(Convert.ToChar("W"), Convert.ToUInt64(148708944461043));
            segDict.Add(Convert.ToChar("X"), Convert.ToUInt64(227860337605875));
            segDict.Add(Convert.ToChar("Y"), Convert.ToUInt64(53614596799731));
            segDict.Add(Convert.ToChar("Z"), Convert.ToUInt64(281409582370815));
            segDict.Add(Convert.ToChar("0"), Convert.ToUInt64(136326352420830));
            segDict.Add(Convert.ToChar("1"), Convert.ToUInt64(132779118478220));
            segDict.Add(Convert.ToChar("2"), Convert.ToUInt64(281409720881119));
            segDict.Add(Convert.ToChar("3"), Convert.ToUInt64(140720819867615));
            segDict.Add(Convert.ToChar("4"), Convert.ToUInt64(214457363938547));
            segDict.Add(Convert.ToChar("5"), Convert.ToUInt64(140721373593599));
            segDict.Add(Convert.ToChar("6"), Convert.ToUInt64(136326548307966));
            segDict.Add(Convert.ToChar("7"), Convert.ToUInt64(107228568948735));
            segDict.Add(Convert.ToChar("8"), Convert.ToUInt64(136325994594270));
            segDict.Add(Convert.ToChar("9"), Convert.ToUInt64(214457363939294));
            segDict.Add(Convert.ToChar(","), Convert.ToUInt64(36021890711552));
            segDict.Add(Convert.ToChar("."), Convert.ToUInt64(53601191854080));
            segDict.Add(Convert.ToChar(":"), Convert.ToUInt64(53601191854860));
            segDict.Add(Convert.ToChar("-"), Convert.ToUInt64(1073479680));
            segDict.Add(Convert.ToChar("&"), Convert.ToUInt64(203662222900446));
            segDict.Add(Convert.ToChar("("), Convert.ToUInt64(132425704026590));
            segDict.Add(Convert.ToChar(")"), Convert.ToUInt64(135842043727390));
        }
        #endregion

        #region Overrides and Events

        protected override void Dispose(bool disposing)
        {   // maybe should dispose some local variables too
            base.Dispose(disposing);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (displayBitmap == null)
            {
                int width = Convert.ToInt32(base.Height / VSEGMENTS * HSEGMENTS);
                int bitmapWidth = (int)((width + width * charSpace) * base.Text.Length);

                // the displayBitmap has at least the same width as the control, else depending of string lenght
                bitmapWidth = Math.Max(bitmapWidth, base.Width);
                displayBitmap = new Bitmap(bitmapWidth, base.Height);
            }

            // paint the bitmap
            using (Graphics g = Graphics.FromImage(displayBitmap))
            {
                DrawBackground(g, new Rectangle(0, 0, displayBitmap.Width, displayBitmap.Height));
                DrawString(base.Text, g, new Rectangle(0, 0, displayBitmap.Width, displayBitmap.Height));
            }

            if (!scroll) // if not scrolling just copy displayBitmap to control
            {
                Rectangle srcRect = new Rectangle(0, 0, this.Width, this.Height);
                e.Graphics.DrawImage(displayBitmap, 0, 0, srcRect, GraphicsUnit.Pixel);
            }
            else // scroll
            {
                // if we not reached the end of displayBitmap, just copy a part of it
                if ((displayBitmap.Width - scrollOffset) >= this.Width)
                {
                    Rectangle srcRect = new Rectangle(scrollOffset, 0, this.Width, this.Height);
                    e.Graphics.DrawImage(displayBitmap, 0, 0, srcRect, GraphicsUnit.Pixel);
                }
                // here we need a part from the end of displayBitmap + a part from the beginning
                else
                {
                    using (Bitmap bmpCopy = new Bitmap(this.Width, this.Height))
                    {
                        using (Graphics g = Graphics.FromImage(bmpCopy))
                        {
                            // get the end of displayBitmap and draw it at the beginning of bmpCopy
                            Rectangle srcRectEnd = new Rectangle(scrollOffset, 0, displayBitmap.Width - scrollOffset, this.Height);
                            g.DrawImage(displayBitmap, 0, 0, srcRectEnd, GraphicsUnit.Pixel);

                            // get the beginning of displayBitmap and draw it at the end of bmpCopy
                            Rectangle srcRectStart = new Rectangle(0, 0, this.Width - srcRectEnd.Width, this.Height);
                            g.DrawImage(displayBitmap, srcRectEnd.Width, 0, srcRectStart, GraphicsUnit.Pixel);

                            // draw the resulting bitmap to control
                            e.Graphics.DrawImage(bmpCopy, 0, 0);
                        }
                    }
                }
            }
        }

        // create a new bitmap on text changed,
        // else you get an error in design mode
        protected override void OnTextChanged(EventArgs e)
        {
            if (displayBitmap != null)
            {
                displayBitmap.Dispose();
                displayBitmap = null;
            }

            base.OnTextChanged(e);
        }

        // create a new bitmap on control resize,
        // else you get an error in design mode
        protected override void OnResize(EventArgs e)
        {
            if (displayBitmap != null)
            {
                displayBitmap.Dispose();
                displayBitmap = null;
            }

            base.OnResize(e);
        }

        // update backcolor in design mode
        protected override void OnBackColorChanged(EventArgs e)
        {
            if (displayBitmap != null)
            {
                displayBitmap.Dispose();
                displayBitmap = null;
            }

            base.OnBackColorChanged(e);
        }

        // timer ticker, increases scrollOffset until the end of displayBitmap is reached
        private void scrollTimer_Tick(object sender, EventArgs e)
        {
            //scrollOffset += this.Height / (int) VSEGMENTS; // one segment per tick

            //scrollOffset++;                 // one pixel per tick

            if (scrollStep == Step.Pixel)
                scrollOffset++;
            else
                scrollOffset += (int)(this.Height / VSEGMENTS);

            if (scrollOffset > displayBitmap.Width)
                scrollOffset = 0;

            this.Invalidate();
        }

        #endregion

        #region Public methods

        // check if char exists in dictionary
        public bool FindChar(char chr)
        {
            if (segDict.ContainsKey(chr))
                return true;
            else
                return false;
        }

        // add a char to dictionary, remove old one if it exists
        public void AddChar(char chr, ulong bits)
        {
            if (segDict.ContainsKey(chr))
                segDict.Remove(chr);
            segDict.Add(chr, bits);
        }

        // remove char if it exists in dictionary
        public void RemoveChar(char chr)
        {
            if (segDict.ContainsKey(chr))
                segDict.Remove(chr);
        }

        // get value for char
        public ulong GetValue(char chr)
        {
            ulong bits;
            segDict.TryGetValue(chr, out bits);
            return bits;
        }

        // get a list of characters in dictionary
        public ArrayList GetKeys()
        {
            ArrayList keyNameList = new ArrayList(segDict.Keys);
            return keyNameList;
        }

        // gets the complete dictionary
        public Dictionary<char, ulong> GetDictionary()
        {
            return segDict;
        }


        #region Draw characters, strings and background

        // draws a single char to the graphics object within specified rectangle
        // can be used with any graphical object
        public void DrawChar(ulong bits, Graphics g, Rectangle rect)
        {
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            float size = rect.Height / VSEGMENTS;       // height and width makes a square
            float space = (size * segmentSpace) / 2;    // space between segments

            using (SolidBrush segmentBrush = new SolidBrush(ForeColor))
            {
                for (int y = 0; y < VSEGMENTS; y++)         // vertical row
                {
                    for (int x = 0; x < HSEGMENTS; x++)     // horizontal row
                    {
                        if ((bits & 1) != 0)                // if bit 1 is set, fill segment
                        {
                            RectangleF segmentRect = new RectangleF(rect.X + x * size, y * size, size, size);
                            if ((space * 2) < size)         // don't inflate to zero
                                segmentRect.Inflate(-space, -space);
                            g.FillEllipse(segmentBrush, segmentRect);
                        }
                        bits = bits >> 1;                   // roll bits to the right
                    }
                }
            }
        }

        // draws a string to the graphics object within specified rectangle
        // can be used with any graphical object
        public void DrawString(string str, Graphics g, Rectangle rect)
        {
            ulong bits;

            if (str != null && str.Length > 0)
            {
                int width = Convert.ToInt32(this.Height / VSEGMENTS * HSEGMENTS);
                int pos = (int)(width + width * charSpace); // position for next character

                for (int i = 0; i < str.Length; i++)
                {
                    segDict.TryGetValue(str[i], out bits);
                    rect = new Rectangle(pos * i, 0, width, rect.Height);
                    DrawChar(bits, g, rect);
                }
            }
        }

        // paints a graphics object within specified rectangle
        // could be gradient, but that doesn't add much
        private void DrawBackground(Graphics g, Rectangle rect)
        {
            using (SolidBrush brush = new SolidBrush(base.BackColor))
                g.FillRectangle(brush, rect);
        }

        #endregion draw
        #endregion public methods

        #region Xml Read/Write

        // read character definitions
        // currently the file must exist i your documents directory
        internal void ReadCharDef()
        {
            XmlTextReader xmlReader = null;

            try
            {
                xmlReader = new XmlTextReader(xmlFile);

                string chrs = "";
                ulong bits = 0;

                while (xmlReader.Read())
                {
                    // read matrix layout, currently not used
                    if (xmlReader.NodeType == XmlNodeType.Element && xmlReader.Name.CompareTo("Layout") == 0)
                    {
                        int vertical = Int32.Parse(xmlReader.GetAttribute("Vertical").ToString());
                        int horizontal = Int32.Parse(xmlReader.GetAttribute("Horizontal").ToString());
                    }
                    // read character definitions
                    if (xmlReader.NodeType == XmlNodeType.Element && xmlReader.Name.CompareTo("character") == 0)
                    {
                        chrs = xmlReader.GetAttribute("char").ToString();
                        bits = UInt64.Parse(xmlReader.GetAttribute("bits").ToString());
                        if (!FindChar(chrs[0]))
                            segDict.Add(chrs[0], bits);
                    }
                }
            }
            finally
            {
                if (xmlReader != null)
                {
                    xmlReader.Close();
                }
            }
        }


        // write character definitions
        // currently the file is written to your documents directory
        public void WriteCharDef()
        {
            XmlTextWriter xmlWriter = null;

            try
            {
                xmlWriter = new XmlTextWriter(xmlFile, Encoding.UTF8);
                xmlWriter.Formatting = Formatting.Indented;

                xmlWriter.WriteStartDocument(false);
                xmlWriter.WriteComment("Led character definintions");

                // write matrix layout
                xmlWriter.WriteStartElement("Layout");
                xmlWriter.WriteAttributeString("Horizontal", HSEGMENTS.ToString());
                xmlWriter.WriteAttributeString("Vertical", VSEGMENTS.ToString());

                xmlWriter.WriteStartElement("CharacterTable");

                foreach (KeyValuePair<char, ulong> tmp in segDict)
                {
                    xmlWriter.WriteStartElement("character");
                    xmlWriter.WriteAttributeString("char", tmp.Key.ToString());
                    xmlWriter.WriteAttributeString("bits", tmp.Value.ToString());
                    xmlWriter.WriteEndElement();
                }

                xmlWriter.WriteEndElement();
                xmlWriter.WriteEndElement();
                xmlWriter.WriteEndDocument();
                xmlWriter.Close();
            }
            catch (IOException)
            {
                throw;
            }
        }
        #endregion

        #region ISupportInitialize

        private bool isInitializing = false;

        void ISupportInitialize.BeginInit()
        {
            isInitializing = true;
        }

        void ISupportInitialize.EndInit()
        {
            isInitializing = false;
            Invalidate();
        }

        #endregion
    }

    /// <summary>
    /// Futaba MDM166A driver for MediaPortal's minidisplay plugin
    /// </summary>
    /// <author>datenschredder</author>
    /// 
    [System.ComponentModel.DesignerCategory("Code")]
    public class FutabaMDM166A : BaseDisplay
    {

        #region Declarations

        //futaba vfd handler
        private FutabaCOM _vfd;
        //count of display lines
        private const int lines = 1;
        //flag to indicate if device is disabled because of connection error
        private readonly bool isDisabled;
        //handler for error messages
        private readonly string errorMessage = "";

        #endregion Declarations

        #region Constructors and destructor

        /// <summary>
        /// Constructor of Futaba MDM166A vfd
        /// </summary>
        public FutabaMDM166A()
        {
            try
            {
                //create new instance of futaba vfd and set current device
                _vfd = new FutabaCOM("vid_19c2", "pid_6a11");
                //connect to device with specified vid and pid
                //_vfd.SetCurrentDevice("vid_19c2", "pid_6a11");
                _vfd.ConnectCurrentDevice();
                //clear complete display
                _vfd.ClearDisplay();
            }
            catch (NotSupportedException ex)
            {
                //disable device becaus of connection handler
                isDisabled = true;
                //assign error message
                errorMessage = ex.Message;
            }
        }

        /// <summary>
        /// Destructor of Futaba MDM166A vfd
        /// </summary>
        ~FutabaMDM166A()
        {
            try
            {
                //Show big clock
                _vfd.ShowBigClock(FutabaCOM.TimeFormatID.hours24);
            }
            catch { }
            this.Dispose();
        }

        #endregion  Constructors and destructor

        #region Methods

        /// <summary>
        /// Cleans up the display
        /// </summary>
        public override void CleanUp()
        {
            try
            {
                //cleans up text and graphics
                _vfd.ClearDisplay();
                //show actal time
                _vfd.SetClock(DateTime.Now);
                _vfd.ShowBigClock(FutabaCOM.TimeFormatID.hours24);
                //show display symbols depending on current state
                SetDisplaySymbols();
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }

        /// <summary>
        /// Shows the advanced configuration screen
        /// </summary>
        public override void Configure()
        {
            LedDisplay led = new LedDisplay();
            new FutabaMDM166A_AdvancedSetupForm(led).ShowDialog();
        }

        /// <summary>
        /// Description of this display driver
        /// </summary>
        public override string Description
        {
            get { return "Futaba MDM166A VFD driver v0.23 by datenschredder"; }
        }

        /// <summary>
        /// Housekeeping and showing of vfd clock
        /// </summary>
        public override void Dispose()
        {
            try
            {
                //cleans up text and graphics
                _vfd.ClearDisplay();
                //show actal time
                _vfd.SetClock(DateTime.Now);
                _vfd.ShowBigClock(FutabaCOM.TimeFormatID.hours24);
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }

        /// <summary>
        /// Shows a bitmap on the vfd
        /// </summary>
        /// <param name="bitmap">Bitmap with vertical 16 bits and horizontal 96 bits</param>
        public override void DrawImage(Bitmap bitmap) { }

        /// <summary>
        /// Gets error message
        /// </summary>
        public override string ErrorMessage
        {
            get { return errorMessage; }
        }

        /// <summary>
        /// Clears the display
        /// </summary>
        public override void Initialize()
        {
            try
            {
                //cleans up text and graphics
                _vfd.ClearDisplay();
                //show actal time
                _vfd.SetClock(DateTime.Now);
                _vfd.ShowBigClock(FutabaCOM.TimeFormatID.hours24);
                //show display symbols depending on current state
                SetDisplaySymbols();
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }

        /// <summary>
        /// Gets disabled state of vfd
        /// </summary>
        public override bool IsDisabled
        {
            get { return isDisabled; }
        }

        /// <summary>
        /// Gets short name of this display driver
        /// </summary>
        public override string Name
        {
            get { return "FutabaMDM166A"; }
        }

        /// <summary>
        /// Sets custom characters
        /// </summary>
        /// <param name="customCharacters">character to be set</param>
        public override void SetCustomCharacters(int[][] customCharacters) { }

        /// <summary>
        /// Displays the message on the indicated line
        /// </summary>
        /// <param name="line">The line to display the message on</param>
        /// <param name="message">The message to display</param>
        public override void SetLine(int line, string message, ContentAlignment aAlignment)
        {
            //if given line exceeds maximum lines of this display
            if (line >= lines)
            {
                Log.Error("FutabaMDM166A.SetLine: error bad line number" + line);
                return;
            }
            //
            try
            {
                //Write message
                _vfd.ShowText(RemoveDiacritics(message));
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
            //
            try
            {
                //Actualize display symbols
                this.SetDisplaySymbols();
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }

        }

        /// <summary>
        /// Initializes the display
        /// </summary>
        /// <param name="_port">The port the display is connected to</param>
        /// <param name="_lines">The number of lines in text mode</param>
        /// <param name="_cols">The number of columns in text mode</param>
        /// <param name="_delay">Communication delay in text mode</param>
        /// <param name="_linesG">The height in pixels in graphic mode</param>
        /// <param name="_colsG">The width in pixels in graphic mode</param>
        /// <param name="_delayG">Communication delay in graphic mode</param>
        /// <param name="_backLight">Backlight on?</param>
        /// <param name="_backLightLevel">Backlight level</param>
        /// <param name="_contrast">Contrast on?</param>
        /// <param name="_contrastLevel">Contrast level</param>
        /// <param name="_blankOnExit">Blank on exit?</param>
        public override void Setup(string _port, int _lines, int _cols, int _delay, int _linesG, int _colsG, int _delayG,
                          bool _backLight, int _backLightLevel, bool _contrast, int _contrastLevel, bool _blankOnExit)
        {
            try
            {
                //cleans up text and graphics
                _vfd.ClearDisplay();
                //show actal time
                _vfd.SetClock(DateTime.Now);
                _vfd.ShowBigClock(FutabaCOM.TimeFormatID.hours24);
                //show display symbols depending on current state
                SetDisplaySymbols();
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }

        /// <summary>
        /// Does this driver support graphic mode?
        /// </summary>
        public override bool SupportsGraphics
        {
            get { return false; }
        }

        /// <summary>
        /// Does this display support text mode?
        /// </summary>
        public override bool SupportsText
        {
            get { return true; }
        }

        /// <summary>
        /// Sets the player symbols
        /// </summary>
        private void SetDisplaySymbols()
        {
            //show network symbols depending on settings
            SetNetworkSymbols();            
            //
            //show volume and mute symbols depending on settings
            SetVolumeSymbols();
            //
            if (VolumeHandler.Instance.IsMuted)
              _vfd.SetSymbol(FutabaCOM.SymbolID.mute, FutabaCOM.SymbolState.on_low);
            else
                _vfd.SetSymbol(FutabaCOM.SymbolID.mute, FutabaCOM.SymbolState.off);
            //
            //show record symbol, depending on tv recording activity
            if (MiniDisplayHelper.MPStatus.Media_IsRecording)
                _vfd.SetSymbol(FutabaCOM.SymbolID.record, FutabaCOM.SymbolState.on_low);
            else
                _vfd.SetSymbol(FutabaCOM.SymbolID.record, FutabaCOM.SymbolState.off);
            //
            // show play symbol depending on playback state
            if (g_Player.Playing & !g_Player.Paused & (g_Player.Speed == 1) & !g_Player.IsDVDMenu)
                _vfd.SetSymbol(FutabaCOM.SymbolID.play, FutabaCOM.SymbolState.on_low);
            else
                _vfd.SetSymbol(FutabaCOM.SymbolID.play, FutabaCOM.SymbolState.off);
            //
            // show pause symbol depending on playback state
            if (g_Player.Paused)
                _vfd.SetSymbol(FutabaCOM.SymbolID.pause, FutabaCOM.SymbolState.on_low);
            else
                _vfd.SetSymbol(FutabaCOM.SymbolID.pause, FutabaCOM.SymbolState.off);
            //
        }

        /// <summary>
        /// Sets the network symbols
        /// </summary>
        private void SetNetworkSymbols()
        {
            if (NetworkIsConnected())
            {
                _vfd.SetWLAN(FutabaCOM.WLANID.strengthbase, FutabaCOM.SymbolState.on_low);
                _vfd.SetWLAN(FutabaCOM.WLANID.strength1, FutabaCOM.SymbolState.on_low);
                _vfd.SetWLAN(FutabaCOM.WLANID.strength2, FutabaCOM.SymbolState.on_low);
                _vfd.SetWLAN(FutabaCOM.WLANID.strength3, FutabaCOM.SymbolState.on_low); 
            }
            else
            {
                _vfd.SetWLAN(FutabaCOM.WLANID.strengthbase, FutabaCOM.SymbolState.off);
                _vfd.SetWLAN(FutabaCOM.WLANID.strength1, FutabaCOM.SymbolState.off);
                _vfd.SetWLAN(FutabaCOM.WLANID.strength2, FutabaCOM.SymbolState.off);
                _vfd.SetWLAN(FutabaCOM.WLANID.strength3, FutabaCOM.SymbolState.off);
            }
        }

        /// <summary>
        /// Sets the volume symbols
        /// </summary>
        private void SetVolumeSymbols()
        {
            Double volume;
            try
            {
                //if volume is not muted
                if (VolumeHandler.Instance.IsMuted == false)
                {
                    //hide mute symbol
                    _vfd.SetSymbol(FutabaCOM.SymbolID.mute, FutabaCOM.SymbolState.off);
                    //calculate and normalize volume
                    volume = Math.Round(Convert.ToDouble(VolumeHandler.Instance.Volume) / VolumeHandler.Instance.Maximum * 14);
                    //show volume base symbol
                    _vfd.SetVolume(FutabaCOM.VolumeID.volume, FutabaCOM.SymbolState.on_low);
                    //show volume minimum level
                    _vfd.SetVolume(FutabaCOM.VolumeID.level01, FutabaCOM.SymbolState.on_low);
                    //show other volume levels depending on volume feedback
                    if (volume > 0)
                        _vfd.SetVolume(FutabaCOM.VolumeID.level01, FutabaCOM.SymbolState.on_low);
                    else
                        _vfd.SetVolume(FutabaCOM.VolumeID.level01, FutabaCOM.SymbolState.off);
                    if (volume > 1)
                        _vfd.SetVolume(FutabaCOM.VolumeID.level02, FutabaCOM.SymbolState.on_low);
                    else
                        _vfd.SetVolume(FutabaCOM.VolumeID.level02, FutabaCOM.SymbolState.off);
                    if (volume > 2)
                        _vfd.SetVolume(FutabaCOM.VolumeID.level03, FutabaCOM.SymbolState.on_low);
                    else
                        _vfd.SetVolume(FutabaCOM.VolumeID.level03, FutabaCOM.SymbolState.off);
                    if (volume > 3)
                        _vfd.SetVolume(FutabaCOM.VolumeID.level04, FutabaCOM.SymbolState.on_low);
                    else
                        _vfd.SetVolume(FutabaCOM.VolumeID.level04, FutabaCOM.SymbolState.off);
                    if (volume > 4)
                        _vfd.SetVolume(FutabaCOM.VolumeID.level05, FutabaCOM.SymbolState.on_low);
                    else
                        _vfd.SetVolume(FutabaCOM.VolumeID.level05, FutabaCOM.SymbolState.off);
                    if (volume > 5)
                        _vfd.SetVolume(FutabaCOM.VolumeID.level06, FutabaCOM.SymbolState.on_low);
                    else
                        _vfd.SetVolume(FutabaCOM.VolumeID.level06, FutabaCOM.SymbolState.off);
                    if (volume > 6)
                        _vfd.SetVolume(FutabaCOM.VolumeID.level07, FutabaCOM.SymbolState.on_low);
                    else
                        _vfd.SetVolume(FutabaCOM.VolumeID.level07, FutabaCOM.SymbolState.off);
                    if (volume > 7)
                        _vfd.SetVolume(FutabaCOM.VolumeID.level08, FutabaCOM.SymbolState.on_low);
                    else
                        _vfd.SetVolume(FutabaCOM.VolumeID.level08, FutabaCOM.SymbolState.off);
                    if (volume > 8)
                        _vfd.SetVolume(FutabaCOM.VolumeID.level09, FutabaCOM.SymbolState.on_low);
                    else
                        _vfd.SetVolume(FutabaCOM.VolumeID.level09, FutabaCOM.SymbolState.off);
                    if (volume > 9)
                        _vfd.SetVolume(FutabaCOM.VolumeID.level10, FutabaCOM.SymbolState.on_low);
                    else
                        _vfd.SetVolume(FutabaCOM.VolumeID.level10, FutabaCOM.SymbolState.off);
                    if (volume > 10)
                        _vfd.SetVolume(FutabaCOM.VolumeID.level11, FutabaCOM.SymbolState.on_low);
                    else
                        _vfd.SetVolume(FutabaCOM.VolumeID.level11, FutabaCOM.SymbolState.off);
                    if (volume > 11)
                        _vfd.SetVolume(FutabaCOM.VolumeID.level12, FutabaCOM.SymbolState.on_low);
                    else
                        _vfd.SetVolume(FutabaCOM.VolumeID.level12, FutabaCOM.SymbolState.off);
                    if (volume > 12)
                        _vfd.SetVolume(FutabaCOM.VolumeID.level13, FutabaCOM.SymbolState.on_low);
                    else
                        _vfd.SetVolume(FutabaCOM.VolumeID.level13, FutabaCOM.SymbolState.off);
                    if (volume > 13)
                        _vfd.SetVolume(FutabaCOM.VolumeID.level14, FutabaCOM.SymbolState.on_low);
                    else
                        _vfd.SetVolume(FutabaCOM.VolumeID.level14, FutabaCOM.SymbolState.off);
                }
                //if volume is muted
                else
                {
                    //hide volume base symbol and levels
                    _vfd.SetVolume(FutabaCOM.VolumeID.volume, FutabaCOM.SymbolState.off);
                    _vfd.SetVolume(FutabaCOM.VolumeID.level14, FutabaCOM.SymbolState.off);
                    _vfd.SetVolume(FutabaCOM.VolumeID.level13, FutabaCOM.SymbolState.off);
                    _vfd.SetVolume(FutabaCOM.VolumeID.level12, FutabaCOM.SymbolState.off);
                    _vfd.SetVolume(FutabaCOM.VolumeID.level11, FutabaCOM.SymbolState.off);
                    _vfd.SetVolume(FutabaCOM.VolumeID.level10, FutabaCOM.SymbolState.off);
                    _vfd.SetVolume(FutabaCOM.VolumeID.level09, FutabaCOM.SymbolState.off);
                    _vfd.SetVolume(FutabaCOM.VolumeID.level08, FutabaCOM.SymbolState.off);
                    _vfd.SetVolume(FutabaCOM.VolumeID.level07, FutabaCOM.SymbolState.off);
                    _vfd.SetVolume(FutabaCOM.VolumeID.level06, FutabaCOM.SymbolState.off);
                    _vfd.SetVolume(FutabaCOM.VolumeID.level05, FutabaCOM.SymbolState.off);
                    _vfd.SetVolume(FutabaCOM.VolumeID.level04, FutabaCOM.SymbolState.off);
                    _vfd.SetVolume(FutabaCOM.VolumeID.level03, FutabaCOM.SymbolState.off);
                    _vfd.SetVolume(FutabaCOM.VolumeID.level02, FutabaCOM.SymbolState.off);
                    _vfd.SetVolume(FutabaCOM.VolumeID.level01, FutabaCOM.SymbolState.off);
                    _vfd.SetVolume(FutabaCOM.VolumeID.volume, FutabaCOM.SymbolState.off);
                    //show mute symbol
                    _vfd.SetSymbol(FutabaCOM.SymbolID.mute, FutabaCOM.SymbolState.on_low);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }

        /// <summary>
        /// Finds out, if a network connection exists
        /// </summary>
        /// <returns>true, if a network connection is active, otherwise false</returns>
        private static bool NetworkIsConnected()  
        {  
            bool isConnected = false;  
            NetworkInterface[] adapters = NetworkInterface.GetAllNetworkInterfaces();  
            foreach (NetworkInterface n in adapters)  
            {  
                if (n.OperationalStatus == OperationalStatus.Up && n.NetworkInterfaceType != NetworkInterfaceType.Loopback)  
                {  
                    //since we have a non loopback interface up, we have a network connection  
                   isConnected = true;  
                   break;                  
                }              
            }              
            return isConnected;  
  
        } 


        /// <summary>
        /// Removes diacritics
        /// </summary>
        /// <param name="s">text string possibly with diaqcritics</param>
        /// <returns></returns>
        private string RemoveDiacritics(String s)
        {
            //normalize string
            s = s.Normalize(NormalizationForm.FormD);
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < s.Length; i++)
            {
                //substitute characters with diacritics by characters without diacritics
                if (CharUnicodeInfo.GetUnicodeCategory(s[i]) != UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(s[i]);
                }
            }
            //return string where diacritics have been removed
            return sb.ToString();
        }

        #endregion

    }

    /// <summary>
    /// communication class for futaba mdm166a display
    /// </summary>
    public class FutabaCOM
    {
        #region declarations

        /// <summary>
        /// time formats
        /// </summary>
        public enum TimeFormatID
        {
            hours12 = 0x00, //12 hours format
            hours24 = 0x01  //24 hours format
        };

        /// <summary>
        /// general symbol IDs
        /// </summary>
        public enum SymbolID
        {
            play = 0x00, //play
            pause = 0x01, //pause
            record = 0x02, //record
            letter = 0x03, //letter symbol without inner @
            email = 0x04, //@ symbol without outer letter
            mute = 0x05  //mute
        };

        /// <summary>
        /// WLAN symbol IDs
        /// </summary>
        public enum WLANID
        {
            strengthbase = 0x06, //tower base
            strength1 = 0x07,    //strength 1
            strength2 = 0x08,    //strength 2
            strength3 = 0x09     //strength 3
        };

        /// <summary>
        /// WLAN symbol IDs
        /// </summary>
        public enum VolumeID
        {
            volume = 0x0a,    //the word "VOL."
            level01 = 0x0b,    //volume level 1
            level02 = 0x0c,    //volume level 2
            level03 = 0x0d,    //volume level 3
            level04 = 0x0e,    //volume level 4
            level05 = 0x0f,    //volume level 5
            level06 = 0x10,    //volume level 6
            level07 = 0x11,    //volume level 7
            level08 = 0x12,    //volume level 8
            level09 = 0x13,    //volume level 9
            level10 = 0x14,    //volume level 10
            level11 = 0x15,    //volume level 11
            level12 = 0x16,    //volume level 12
            level13 = 0x17,    //volume level 13
            level14 = 0x18     //volume level 14
        };

        /// <summary>
        /// intensity of illumination of a symbol
        /// </summary>
        public enum SymbolState
        {
            off = 0x00, //symbol not shown
            on_low = 0x01, //symbol illuminated normally
            on_high = 0x02  //symbol illuminated brightly
        };

        /// <summary>
        /// intensity of illumination of display
        /// </summary>
        public enum BrightnessLevel
        {
            off = 0x00, //display illmination is OFF
            on_low = 0x01, //symbol illuminated normally
            on_high = 0x02  //symbol illuminated brightly
        };

        //vendor ID
        private String _vid = "";
        //product ID
        private String _pid = "";
        //handle for USB devices 
        private USBInterface _usbDevices;
        //handle for crrent USB device 
        private USBInterface _usbCurrentDevice;
        //handle for display functionalities
        private LedDisplay _ledDisplay;
        //handle for keys
        private ArrayList _keyList;
        //dictionary, which keeps together character and ulong representative
        private Dictionary<char, ulong> _dicChars = new Dictionary<char, ulong> { };
        //2-dimensional byte array to cover a text for the futaba device
        private Byte[,] btTextToByteArray;
        //create 1-dimensional byte array for 
        private Byte[] btToFutaba;
        //timer for update of display content
        private System.Timers.Timer tiWriteText = new System.Timers.Timer(200);
        //offset for startposition of text to be shown
        private Int32 iOffsVisibleText = 0;
        //number of bytes for one character including space to next character
        private Int32 iCharByteCount = 14;
        //text timer standard scroll period in ms
        private Int32 iScrollPeriodStandard = 200;
        //text timer long scroll period in ms
        private Int32 iScrollPeriodLong = 600;

        #endregion declarations

        #region constructor/destructor

        /// <summary>
        /// constructor without accessing devices
        /// </summary>
        public FutabaCOM()
        {
            //create a new display instance
            _ledDisplay = new LedDisplay();
            //read all available keys and their values
            _dicChars = _ledDisplay.GetDictionary();
            //create a timer for display updates
            tiWriteText.Elapsed += new ElapsedEventHandler(tiWriteText_ElapsedHandler);
        }

        /// <summary>
        /// constructor to access all futaba devices of
        /// the same vendor by vendor ID
        /// </summary>
        /// <param name="vid">vendor id of futaba device</param>
        public FutabaCOM(String vid)
        {
            //take over arguments
            _vid = vid;
            //connect to all devices of this vendor
            _usbDevices = new USBInterface(_vid);
            //create a new display instance
            _ledDisplay = new LedDisplay();
            //read all available keys and their values
            _dicChars = _ledDisplay.GetDictionary();
            //create a timer for display updates
            tiWriteText.Elapsed += new ElapsedEventHandler(tiWriteText_ElapsedHandler);
        }

        /// <summary>
        /// constructor to access all futaba devices of
        /// the same vendor by vendor ID and set the device
        /// with the appropriate product ID as the current device
        /// </summary>
        /// <param name="vid">vendor ID of futaba device</param>
        /// <param name="pid">product ID of futaba device</param>
        public FutabaCOM(String vid, String pid)
        {
            //take over arguments
            _vid = vid;
            _pid = pid;
            //connect to device
            _usbCurrentDevice = new USBInterface(_vid, _pid);
            //create a new display instance
            _ledDisplay = new LedDisplay();
            //read all available keys
            _keyList = _ledDisplay.GetKeys();
            //create a timer for display updates
            tiWriteText.Elapsed += new ElapsedEventHandler(tiWriteText_ElapsedHandler);
        }

        /// <summary>
        /// destructor
        /// </summary>
        ~FutabaCOM()
        {
            try
            {
                //stop timer
                tiWriteText.Enabled = false;
                //show big clock
                ShowBigClock(TimeFormatID.hours24);
                //delete display instance
                _ledDisplay.Dispose();
                //delete keys/value dictionary
                _dicChars = null;
                //delete timer for display updates
                tiWriteText.Elapsed -= new ElapsedEventHandler(tiWriteText_ElapsedHandler);
            }
            catch { };
        }
        #endregion constructor/destructor

        #region methods

        /// <summary>
        /// sends a command to the display
        /// </summary>
        /// <param name="bytes">display command</param>
        private void SendCommand(Byte[] bytes)
        {
            try
            {
                //write datagram to futaba display
                _usbCurrentDevice.write(bytes);
            }
            catch (Exception ex)
            {
                throw new Exception("Error when writing data to futaba display. ", ex);
            }
        }

        /// <summary>
        /// sets the current device
        /// </summary>
        /// <param name="vid">vendor ID</param>
        /// <param name="pid">product ID</param>
        public void SetCurrentDevice(string vid, string pid)
        {
            _vid = vid;
            _pid = pid;
            try
            {
                if (_usbCurrentDevice != null) _usbCurrentDevice = null;
                _usbCurrentDevice = new USBInterface(_vid, _pid);
            }
            catch (Exception ex)
            {
                throw new Exception("Setting of current device failed. ", ex);
            }
        }

        /// <summary>
        /// connects the device which has been set as the current one
        /// </summary>
        public void ConnectCurrentDevice()
        {
            try
            {
                //connect current device
                _usbCurrentDevice.Connect();
            }
            catch (Exception ex)
            {
                throw new Exception("Connection of current device failed. ", ex);
            }
        }

        /// <summary>
        /// disconnects the current device
        /// </summary>
        public void DisconnectCurrentDevice() //DOES NOT WORK
        {
            try
            {
                //Disconnect current device
                _usbCurrentDevice.Disconnect();
            }
            catch (Exception ex)
            {
                throw new Exception("Disconnection of current device failed. ", ex);
            }
        }

        /// <summary>
        /// sets the display clock
        /// </summary>
        /// <param name="dt">datetime time info</param>
        public void SetClock(DateTime dt)
        {
            //create handle for datagram
            Byte[] btDatagram = new Byte[64];
            //convert datetime info from integer into
            //"look like" hex view //e.g.: 23 => 0x23
            String stMinute = dt.Minute.ToString();
            if (stMinute.Length == 1) stMinute = "0" + stMinute;
            String stHour = dt.Hour.ToString();
            if (stHour.Length == 1) stHour = "0" + stHour;
            Byte btMinuteMajor = Convert.ToByte(stMinute.Substring(0, 1));
            Byte btMinuteMinor = Convert.ToByte(stMinute.Substring(1, 1));
            Byte btHourMajor = Convert.ToByte(stHour.Substring(0, 1));
            Byte btHourMinor = Convert.ToByte(stHour.Substring(1, 1));
            Byte btMinute = Convert.ToByte((btMinuteMajor * 16) + btMinuteMinor);
            Byte btHour = Convert.ToByte((btHourMajor * 16) + btHourMinor);


            try
            {
                //create datagram
                btDatagram[0x00] = 0x04;          //count of successive bytes
                btDatagram[0x01] = 0x1b;          //header (fix value)
                btDatagram[0x02] = 0x00;          //command: set clock
                btDatagram[0x03] = btMinute;      //minute to be set
                btDatagram[0x04] = btHour;        //hour to be set
                //send command
                SendCommand(btDatagram);
            }
            catch (Exception ex)
            {
                throw new Exception("Setting the clock failed. ", ex);
            }
        }

        /// <summary>
        /// shows a little clock
        /// use TimeFormat enum as argment
        /// </summary>
        /// <param name="timeformat">time format 12/24 h</param>
        public void ShowLittleClock(TimeFormatID timeformat)
        {
            //stop showing text
            tiWriteText.Enabled = false;
            //create handle for datagram
            Byte[] btDatagram = new Byte[64];
            try
            {
                //create datagram
                btDatagram[0x00] = 0x03;                        //count of successive bytes
                btDatagram[0x01] = 0x1b;                        //header (fix value)
                btDatagram[0x02] = 0x01;                        //command: show little clock
                btDatagram[0x03] = Convert.ToByte(timeformat);  //12 or 24 hour format
                //send command
                SendCommand(btDatagram);
            }
            catch (Exception ex)
            {
                throw new Exception("Showing little clock failed: ", ex);
            }
        }

        /// <summary>
        /// shows a big clock
        /// use TimeFormat enum as argment
        /// </summary>
        /// <param name="timeformat">time format 12/24 h</param>
        public void ShowBigClock(TimeFormatID timeformat)
        {
            //stop showing text
            tiWriteText.Enabled = false;
            //create handle for datagram
            Byte[] btDatagram = new Byte[64];
            try
            {
                //create datagram
                btDatagram[0x00] = 0x03;                        //count of successive bytes
                btDatagram[0x01] = 0x1b;                        //header (fix value)
                btDatagram[0x02] = 0x02;                        //command: show big clock
                btDatagram[0x03] = Convert.ToByte(timeformat);  //12 or 24 hour format
                //send command
                SendCommand(btDatagram);
            }
            catch (Exception ex)
            {
                throw new Exception("Showing big clock failed: ", ex);
            }

        }

        /// <summary>
        /// sets a general symbol
        /// use SymbolID enum and SymbolState enum as argument
        /// </summary>
        /// <param name="symbolid">symbol ID</param>
        /// <param name="symbolstate">symbol state</param>
        public void SetSymbol(SymbolID symbolid, SymbolState symbolstate)
        {
            //create handle for datagram
            Byte[] btDatagram = new Byte[64];
            try
            {
                //create datagram
                btDatagram[0x00] = 0x04;                        //count of successive bytes
                btDatagram[0x01] = 0x1b;                        //header (fix value)
                btDatagram[0x02] = 0x30;                        //command: set a symbol
                btDatagram[0x03] = Convert.ToByte(symbolid);    //ID of symbol
                btDatagram[0x04] = Convert.ToByte(symbolstate); //required state of symbol
                //send command
                SendCommand(btDatagram);
            }
            catch (Exception ex)
            {
                throw new Exception("Setting a symbol failed: ", ex);
            }
        }

        /// <summary>
        /// sets WLAN symbols
        /// use WLANID enum and SymbolState enum as argument
        /// </summary>
        /// <param name="wlanid">WLAN symbol</param>
        /// <param name="symbolstate">symbol state</param>
        public void SetWLAN(WLANID wlanid, SymbolState symbolstate)
        {
            //create handle for datagram
            Byte[] btDatagram = new Byte[64];
            try
            {
                //create datagram
                btDatagram[0x00] = 0x04;                        //count of successive bytes
                btDatagram[0x01] = 0x1b;                        //header (fix value)
                btDatagram[0x02] = 0x30;                        //command: set a symbol
                btDatagram[0x03] = Convert.ToByte(wlanid);      //ID of WLAN symbol
                btDatagram[0x04] = Convert.ToByte(symbolstate); //required state of symbol
                //send command
                SendCommand(btDatagram);
            }
            catch (Exception ex)
            {
                throw new Exception("Setting WLAN symbol failed: ", ex);
            }
        }

        /// <summary>
        /// sets volume symbols
        /// use VolumeID enum and SymbolState enum as argument
        /// </summary>
        /// <param name="volumeid">id of volume symbol</param>
        /// <param name="symbolstate">symbol state</param>
        public void SetVolume(VolumeID volumeid, SymbolState symbolstate)
        {
            //create handle for datagram
            Byte[] btDatagram = new Byte[64];
            try
            {
                //create datagram
                btDatagram[0x00] = 0x04;                        //count of successive bytes
                btDatagram[0x01] = 0x1b;                        //header (fix value)
                btDatagram[0x02] = 0x30;                        //command: set a symbol
                btDatagram[0x03] = Convert.ToByte(volumeid);    //ID of volume symbol
                btDatagram[0x04] = Convert.ToByte(symbolstate); //required state of symbol
                //send command
                SendCommand(btDatagram);
            }
            catch (Exception ex)
            {
                throw new Exception("Setting volume symbol failed: ", ex);
            }
        }

        /// <summary>
        /// sets the display dimming
        /// use BrightnessLevel enum as argument
        /// </summary>
        /// <param name="brightnesslevel">brightness level</param>
        public void SetDimming(BrightnessLevel brightnesslevel)
        {
            //create handle for datagram
            Byte[] btDatagram = new Byte[64];
            try
            {
                //create datagram
                btDatagram[0x00] = 0x03;              //count of successive bytes
                btDatagram[0x01] = 0x1b;              //header (fix value)
                btDatagram[0x02] = 0x40;              //command: set display dimming state
                btDatagram[0x03] = Convert.ToByte(brightnesslevel);   //brightness level
                //send command
                SendCommand(btDatagram);
            }
            catch (Exception ex)
            {
                throw new Exception("Setting display brightness failed: ", ex);
            }
        }

        /// <summary>
        /// shows a text on the display
        /// </summary>
        /// <param name="text">text to be shown</param>
        public void ShowText(String text)
        {
            //stop showing text
            tiWriteText.Enabled = false;
            //create byte array to cover the complete text
            //1st dimension index is count of characters
            //2nd dimension index is count of bytes for one character (12) + a space column (2)
            btTextToByteArray = null;
            btTextToByteArray = new Byte[text.Length, iCharByteCount];
            //fill byte array from text
            FillByteArrayFromText(text, ref btTextToByteArray);
            //reset offset for startposition of text to be shown
            iOffsVisibleText = 0;
            //start showing text
            tiWriteText_ElapsedHandler(null, null);
            tiWriteText.Interval = iScrollPeriodLong;
            tiWriteText.Enabled = true;
        }

        /// <summary>
        /// clears the text shown on the display
        /// </summary>
        public void ClearText()
        {
            //stop showing text
            tiWriteText.Enabled = false;
            //create handle for datagram
            Byte[] btDatagram = new Byte[64];
            //RAM adress for text
            Byte btRAMadress = 0x00;

            //for each column to be deleted on display
            for (int i = 0; i < 96; i++)
            {

                //create datagram for RAM adress
                btDatagram[0x00] = 0x03;              //count of successive bytes
                btDatagram[0x01] = 0x1b;              //header (fix value)
                btDatagram[0x02] = 0x60;              //command: set RAM adress
                btDatagram[0x03] = btRAMadress;       //RAM adress
                //send command
                SendCommand(btDatagram);
                //
                //create datagram for empty data
                btDatagram[0x00] = 0x11;              //count of successive bytes
                btDatagram[0x01] = 0x1b;              //header (fix value)
                btDatagram[0x02] = 0x70;              //command: write pixel
                btDatagram[0x03] = 0x2;              //count of data bytes
                //
                //send command
                SendCommand(btDatagram);
                //set RAM adress for next empty data
                btRAMadress = Convert.ToByte(btRAMadress + 0x2);
            }
        }

        /// <summary>
        /// resets the display
        /// </summary>
        public void ClearDisplay()
        {
            //stop showing text
            tiWriteText.Enabled = false;
            //create handle for datagram
            Byte[] btDatagram = new Byte[64];
            try
            {
                //create datagram
                btDatagram[0x00] = 0x02;              //count of successive bytes
                btDatagram[0x01] = 0x1b;              //header (fix value)
                btDatagram[0x02] = 0x50;              //command: reset display
                //send command
                SendCommand(btDatagram);
            }
            catch (Exception ex)
            {
                throw new Exception("Clearing display failed: ", ex);
            }
        }

        /// <summary>
        /// shows a vertical pattern
        /// </summary>
        public void ShowVerticalPattern()
        {
            //stop showing text
            tiWriteText.Enabled = false;
            //create handle for datagram
            Byte[] btDatagram = new Byte[64];
            try
            {
                //create datagram
                btDatagram[0x00] = 0x02;              //count of successive bytes
                btDatagram[0x01] = 0x1b;              //header (fix value)
                btDatagram[0x02] = 0xf0;              //command: show vertical test pattern
                //send command
                SendCommand(btDatagram);
            }
            catch (Exception ex)
            {
                throw new Exception("Showing vertical test pattern failed: ", ex);
            }
        }

        /// <summary>
        /// shows a horizontal pattern
        /// </summary>
        public void ShowHorizontalPattern()
        {
            //stop showing text
            tiWriteText.Enabled = false;
            //create handle for datagram
            Byte[] btDatagram = new Byte[64];
            try
            {
                //create datagram
                btDatagram[0x00] = 0x02;              //count of successive bytes
                btDatagram[0x01] = 0x1b;              //header (fix value)
                btDatagram[0x02] = 0xf1;              //command: show horizontal test pattern
                //send command
                SendCommand(btDatagram);
            }
            catch (Exception ex)
            {
                throw new Exception("Showing horizontal test pattern failed: ", ex);
            }
        }

        /// <summary>
        /// fill futaba byte array from text
        /// </summary>
        /// <param name="text">text to be shown</param>
        /// <param name="TextToByteArray">byte array to be filled</param>
        private void FillByteArrayFromText(String text, ref byte[,] TextToByteArray)
        {
            //byte position in datagram for content
            Byte btBytePos = new Byte();
            //convert string into character array
            Char[] chararray = new Char[text.Length];
            //convert text to upper letters
            chararray = text.ToUpper().ToCharArray();
            //a character is built columnwise, each column is a word,
            //direction top to bottom of the display
            //We have to transfer the content bytewise and each column in display is 2 bytes, so we need
            //a low byte for the column top and the high byte for the column bottom
            Byte btLowByte = new Byte();
            Byte btHighByte = new Byte();

            //for each character in text to be displayed
            for (int i = 0; i < text.Length; i++)
            {
                //byte position in datagram for content
                btBytePos = 0x00;
                //for each column in character from character set
                for (int k = 0; k < 6; k++)
                {
                    //reset low byte of actual column
                    btLowByte = 0;
                    //for first 4 rows in character from character set
                    for (int n = 0; n < 4; n++)
                    {
                        //if the masked bit is set to 1
                        if ((_ledDisplay.GetValue(chararray[i]) & Convert.ToUInt64(Math.Pow(2, (n * 6) + k))) != 0)
                        {
                            //fill low byte starting with 5th bit
                            //the first 4 bits must be empty because we have a character with
                            //8 pixel heigth in the middle of a field with 16 pixel heigth
                            btLowByte = Convert.ToByte(btLowByte | Convert.ToByte(Math.Pow(2, (3 - n))));
                        }
                    }
                    //reset high byte of actual column
                    btHighByte = 0;
                    //for last 4 rows in character from character set
                    for (int n = 4; n < 8; n++)
                    {
                        //if the masked bit is set to 1
                        if ((_ledDisplay.GetValue(chararray[i]) & Convert.ToUInt64(Math.Pow(2, (n * 6) + k))) != 0)
                        {
                            //fill high byte starting with first bit up to 4th bit
                            //the last 4 bits must be empty because we have a character with
                            //8 pixel heigth in the middle of a field with 16 pixel heigth
                            btHighByte = Convert.ToByte(btHighByte | Convert.ToByte(Math.Pow(2, (11 - n))));
                        }
                    }
                    //assign low byte and high byte of actal column to byte array
                    TextToByteArray[i, btBytePos] = btLowByte;
                    TextToByteArray[i, btBytePos + 1] = btHighByte;
                    //set byte position for next column
                    btBytePos = Convert.ToByte(btBytePos + 0x02);
                }
                //
                //each character ends with a space column
                TextToByteArray[i, btBytePos] = 0x00;
                TextToByteArray[i, btBytePos + 1] = 0x00;

            }
        }

        #endregion methods

        #region properties

        /// <summary>
        /// gets device list
        /// </summary>
        public string[] DeviceList
        {
            get
            {
                //if this instance has been initialized at minimum with VID
                if (_vid != "")
                {
                    //return list of all found devices
                    return _usbDevices.getDeviceList();
                }
                //if this instance has not been initialized with VID
                else
                {
                    //nothing to return
                    return null;
                }
            }
        }

        #endregion properties

        #region timers and events

        /// <summary>
        /// shows display text periodically
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="e"></param>
        private void tiWriteText_ElapsedHandler(Object obj, ElapsedEventArgs e)
        {
            //stop this timer
            tiWriteText.Enabled = false;
            //assign standard timer period
            tiWriteText.Interval = iScrollPeriodStandard;

            //create handle for datagram
            Byte[] btDatagram = new Byte[64];
            //byte position in datagram for content
            //Byte btBytePos = new Byte();
            //RAM adress for text
            Byte btRAMadress = new Byte();
            //create 1-dimensional byte array for futaba characters to be transferred
            btToFutaba = null;
            btToFutaba = new Byte[btTextToByteArray.GetLength(0) * btTextToByteArray.GetLength(1)];
            //fill the array from text-to-byte array argument
            //(means: convert 2-dim. array to 1-dim array)
            for (int s = 0; s < btTextToByteArray.GetLength(0); s++)
                for (int t = 0; t < btTextToByteArray.GetLength(1); t++)
                    btToFutaba[(s * btTextToByteArray.GetLength(1)) + t] = btTextToByteArray[s, t];
            //
            //reset RAM address to display start position
            btRAMadress = 0x00;
            //clear datagram
            for (int m = 0x04; m < btDatagram.GetLength(0); m++)
                btDatagram[m] = 0x00;
            //
            //as long as display is not completely filled
            while (btRAMadress < 0xC0)
            {
                //create datagram for RAM adress
                btDatagram[0x00] = 0x03;              //count of successive bytes
                btDatagram[0x01] = 0x1b;              //header (fix value)
                btDatagram[0x02] = 0x60;              //command: set RAM adress
                btDatagram[0x03] = btRAMadress;       //RAM adress
                //send command
                SendCommand(btDatagram);
                //
                //create datagram for characters
                btDatagram[0x00] = 0x3F;              //count of successive bytes
                btDatagram[0x01] = 0x1b;              //header (fix value)
                btDatagram[0x02] = 0x70;              //command: write pixel
                btDatagram[0x03] = 0x30;              //count of data bytes
                //fill in text data
                for (int n = 0x00; n < 0x30; n++)
                {
                    //if end of text has not been reached
                    if (((btRAMadress + n + iOffsVisibleText) < btToFutaba.GetLength(0)) && iOffsVisibleText != -iCharByteCount)
                        //assign next text package to datagram
                        btDatagram[n + 0x04] = btToFutaba[btRAMadress + n + iOffsVisibleText];
                    //if end of text has been reached
                    else
                    {
                        //assign long period to timer so that end of text can conviniently be recognized
                        tiWriteText.Interval = iScrollPeriodLong;
                        //reset datagram content
                        btDatagram[n + 0x04] = 0x00;
                        //reset visible text offset
                        iOffsVisibleText = -iCharByteCount;
                    }
                    //if this is the beginning of the text
                    if (iOffsVisibleText == 0x00)
                    {
                        //assign long period to timer so that beginning of text can conviniently be recognized
                        tiWriteText.Interval = iScrollPeriodLong;
                    }
                }
                //
                //send command
                SendCommand(btDatagram);
                //
                //set RAM adress for next characters
                btRAMadress = Convert.ToByte(btRAMadress + 0x30);
            }

            //increase offset    
            iOffsVisibleText += iCharByteCount;
            //start this timer again
            tiWriteText.Enabled = true;
        }

        #endregion timers and events
    }

    /// <summary>
    /// Interface for the HID USB Driver.
    /// </summary>
    public class USBInterface
    {
        #region declarations

        private string usbVID;
        private string usbPID;
        private bool isConnected;
        private HIDUSBDevice usbdevice;
        //USB LIST BUFFER
        /// <summary>
        /// Buffer for incomming data.
        /// </summary>
        public static ListWithEvent usbBuffer = new ListWithEvent();

        #endregion declarations

        #region constructors and destructor

        /// <summary>
        /// Initializes a new instance of the <see cref="USBInterface"/> class.
        /// </summary>
        /// <param name="vid">The vendor id of the USB device (e.g. vid_06ba)</param>
        /// <param name="pid">The product id of the USB device (e.g. pid_ffff)</param>
        public USBInterface(string vid, string pid)
        {
            this.usbVID = vid;
            this.usbPID = pid;
            this.usbdevice = new HIDUSBDevice(this.usbVID, this.usbPID);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="USBInterface"/> class.
        /// </summary>
        /// <param name="vid">The vendor id of the USB device (e.g. vid_06ba).</param>
        public USBInterface(string vid)
        {
            this.usbVID = vid;
            this.usbdevice = new HIDUSBDevice(this.usbVID, "");
        }

        #endregion constructors and destructor

        #region methods

        /// <summary>
        /// Establishes a connection to the USB device. 
        /// You can only establish a connection to a device if you have used the construct with vendor AND product id. 
        /// Otherwise it will connect to a device which has the same vendor id is specified, 
        /// this means if more than one device with these vendor id is plugged in, 
        /// you can't be determine to which one you will connect. 
        /// </summary>
        /// <returns>false if an error occures</returns>
        public bool Connect()
        {
            isConnected = this.usbdevice.connectDevice();
            return isConnected;
        }

        /// <summary>
        /// Disconnects the device
        /// </summary>
        public void Disconnect()
        {
            if (isConnected)
            {
                this.usbdevice.disconnectDevice();
            }
        }

        /// <summary>
        /// Returns a list of devices with the vendor id (or vendor and product id) 
        /// specified in the constructor.
        /// This function is needed if you want to know how many (and which) devices with the specified
        /// vendor id are plugged in.
        /// </summary>
        /// <returns>String list with device paths</returns>
        public String[] getDeviceList()
        {
            return (String[])this.usbdevice.getDevices().ToArray(typeof(string));
        }

        /// <summary>
        /// Writes the specified bytes to the USB device.
        /// If the array length exceeds 64, the array while be divided into several 
        /// arrays with each containing 64 bytes.
        /// The 0-63 byte of the array is sent first, then the 64-127 byte and so on.
        /// </summary>
        /// <param name="bytes">The bytes to send.</param>
        /// <returns>Returns true if all bytes have been written successfully</returns>
        public bool write(Byte[] bytes)
        {
            int byteCount = bytes.Length;
            int bytePos = 0;

            bool success = true;

            //build hid reports with 64 bytes
            while (bytePos <= byteCount - 1)
            {
                if (bytePos > 0)
                {
                    Thread.Sleep(5);
                }
                Byte[] transfByte = new byte[64];
                for (int u = 0; u < 64; u++)
                {
                    if (bytePos < byteCount)
                    {
                        transfByte[u] = bytes[bytePos];
                        bytePos++;
                    }
                    else
                    {
                        transfByte[u] = 0;
                    }
                }
                //send the report
                if (!this.usbdevice.writeData(transfByte))
                {
                    success = false;
                }
                Thread.Sleep(5);
            }
            return success;
        }

        /// <summary>
        /// Starts reading. 
        /// If you execute this command a thread is started which listens to the USB device and waits for data.
        /// </summary>
        public void startRead()
        {
            this.usbdevice.readData();
        }

        /// <summary>
        /// Stops the read thread.
        /// By executing this command the read data thread is stopped and now data will be received.
        /// </summary>
        public void stopRead()
        {
            this.usbdevice.readData();
        }

        /// <summary>
        /// Enables the usb buffer event.
        /// Whenever a dataset is added to the buffer (and so received from the usb device)
        /// the event handler method will be called.
        /// </summary>
        /// <param name="eHandler">The event handler method.</param>
        public void enableUsbBufferEvent(System.EventHandler eHandler)
        {
            usbBuffer.Changed += eHandler;
        }

        #endregion  methods
    }

    /// <summary>
    /// API import class to provide HID API functions
    /// </summary>
    sealed class HidApiDeclarations
    {
        #region declarations

        // API Declarations for communicating with HID-class devices.

        // ******************************************************************************
        // API constants
        // ******************************************************************************

        // from hidpi.h
        // Typedef enum defines a set of integer constants for HidP_Report_Type
        public const short HidP_Input = 0;
        public const short HidP_Output = 1;
        public const short HidP_Feature = 2;

        // ******************************************************************************
        // Structures and classes for API calls, listed alphabetically
        // ******************************************************************************

        [StructLayout(LayoutKind.Sequential)]
        public struct HIDD_ATTRIBUTES
        {
            public int Size;
            public short VendorID;
            public short ProductID;
            public short VersionNumber;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct HIDP_CAPS
        {
            public short Usage;
            public short UsagePage;
            public short InputReportByteLength;
            public short OutputReportByteLength;
            public short FeatureReportByteLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 17)]
            public short[] Reserved;
            public short NumberLinkCollectionNodes;
            public short NumberInputButtonCaps;
            public short NumberInputValueCaps;
            public short NumberInputDataIndices;
            public short NumberOutputButtonCaps;
            public short NumberOutputValueCaps;
            public short NumberOutputDataIndices;
            public short NumberFeatureButtonCaps;
            public short NumberFeatureValueCaps;
            public short NumberFeatureDataIndices;

        }

        // If IsRange is false, UsageMin is the Usage and UsageMax is unused.
        // If IsStringRange is false, StringMin is the string index and StringMax is unused.
        // If IsDesignatorRange is false, DesignatorMin is the designator index and DesignatorMax is unused.

        [StructLayout(LayoutKind.Sequential)]
        public struct HidP_Value_Caps
        {
            public short UsagePage;
            public byte ReportID;
            public int IsAlias;
            public short BitField;
            public short LinkCollection;
            public short LinkUsage;
            public short LinkUsagePage;
            public int IsRange;
            public int IsStringRange;
            public int IsDesignatorRange;
            public int IsAbsolute;
            public int HasNull;
            public byte Reserved;
            public short BitSize;
            public short ReportCount;
            public short Reserved2;
            public short Reserved3;
            public short Reserved4;
            public short Reserved5;
            public short Reserved6;
            public int LogicalMin;
            public int LogicalMax;
            public int PhysicalMin;
            public int PhysicalMax;
            public short UsageMin;
            public short UsageMax;
            public short StringMin;
            public short StringMax;
            public short DesignatorMin;
            public short DesignatorMax;
            public short DataIndexMin;
            public short DataIndexMax;
        }

        #endregion declarations

        #region DLL import

        // ******************************************************************************
        // API functions, listed alphabetically
        // ******************************************************************************

        [DllImport("hid.dll")]
        static public extern bool HidD_FlushQueue(int HidDeviceObject);

        [DllImport("hid.dll")]
        static public extern bool HidD_FreePreparsedData(ref IntPtr PreparsedData);

        [DllImport("hid.dll")]
        static public extern int HidD_GetAttributes(int HidDeviceObject, ref HIDD_ATTRIBUTES Attributes);

        [DllImport("hid.dll")]
        static public extern bool HidD_GetFeature(int HidDeviceObject, ref byte lpReportBuffer, int ReportBufferLength);

        [DllImport("hid.dll")]
        static public extern bool HidD_GetInputReport(int HidDeviceObject, ref byte lpReportBuffer, int ReportBufferLength);

        [DllImport("hid.dll")]
        static public extern void HidD_GetHidGuid(ref System.Guid HidGuid);

        [DllImport("hid.dll")]
        static public extern bool HidD_GetNumInputBuffers(int HidDeviceObject, ref int NumberBuffers);

        [DllImport("hid.dll")]
        static public extern bool HidD_GetPreparsedData(int HidDeviceObject, ref IntPtr PreparsedData);

        [DllImport("hid.dll")]
        static public extern bool HidD_SetFeature(int HidDeviceObject, ref byte lpReportBuffer, int ReportBufferLength);

        [DllImport("hid.dll")]
        static public extern bool HidD_SetNumInputBuffers(int HidDeviceObject, int NumberBuffers);

        [DllImport("hid.dll")]
        static public extern bool HidD_SetOutputReport(int HidDeviceObject, ref byte lpReportBuffer, int ReportBufferLength);

        [DllImport("hid.dll")]
        static public extern int HidP_GetCaps(IntPtr PreparsedData, ref HIDP_CAPS Capabilities);

        [DllImport("hid.dll")]
        static public extern int HidP_GetValueCaps(short ReportType, ref byte ValueCaps, ref short ValueCapsLength, IntPtr PreparsedData);

        #endregion DLL import
    }

    /// <summary>
    ///
    /// </summary>
    public class HIDUSBDevice : IDisposable
    {
        #region declarations

        bool disposed = false;
        //private Thread usbThread;

        /*Variables --------------------------------------------------------------------*/
        private String vendorID;    //Vendor ID of the Device
        private String productID;   //Product ID of the Device
        private String devicePath;  //device path
        private int deviceCount;    //device count

        private bool connectionState;   //Connection Status true: connected, false: disconnected
        public int byteCount = 0;       //Recieved Bytes
        //recieve Buffer (Each report is one Element)
        //this one was replaced by the receive Buffer in the interface
        //public static ArrayList receiveBuffer = new ArrayList();

        //USB Object
        private USBSharp myUSB = new USBSharp();
        //thread for read operations
        protected Thread dataReadingThread;

        #endregion declarations

        #region constructors and destructor

        //---#+************************************************************************
        //---NOTATION:
        //-  HIDUSBDevice(int vID, int pID)
        //-
        //--- DESCRIPTION:
        //--  constructor
        //--  tries to establish a connection to the device
        //                                                             Autor:      F.L.
        //-*************************************************************************+#*
        /// <summary>
        /// Initializes a new instance of the <see cref="HIDUSBDevice"/> class.
        /// And tries to establish a connection to the device.
        /// </summary>
        /// <param name="vID">The vendor ID of the USB device.</param>
        /// <param name="pID">The product ID of the USB device.</param>
        public HIDUSBDevice(String vID, String pID)
        {
            //set vid and pid
            setDeviceData(vID, pID);
            //try to establish connection
            connectDevice();
            //create Read Thread
            dataReadingThread = new Thread(new ThreadStart(readDataThread));
        }

        #endregion constructors and destructor

        #region methods

        //---#+************************************************************************
        //---NOTATION:
        //-  bool connectDevice()
        //-
        //--- DESCRIPTION:
        //--  tries to establish a connection to the device
        //                                                             Autor:      F.L.
        //-*************************************************************************+#*
        /// <summary>
        /// Connects the device.
        /// </summary>
        /// <returns>true if connection is established</returns>
        public bool connectDevice()
        {
            //searchDevice
            searchDevice();
            //return connection state
            return this.getConnectionState();
        }
        //---#+************************************************************************
        //---NOTATION:
        //-  bool searchDevice()
        //-
        //--- DESCRIPTION:
        //--  tries to find the device with specified vendorID and productID 
        //                                                             Autor:      F.L.
        //-*************************************************************************+#*
        /// <summary>
        /// Searches the device with soecified vendor and product id an connect to it.
        /// </summary>
        /// <returns></returns>
        private bool searchDevice()
        {
            //no device found yet
            bool deviceFound = false;
            this.deviceCount = 0;
            this.devicePath = string.Empty;

            myUSB.CT_HidGuid();
            myUSB.CT_SetupDiGetClassDevs();

            int result = -1;
            int resultb = -1;
            int device_count = 0;
            int size = 0;
            int requiredSize = 0;

            //search the device until you have found it or no more devices in list
            while (result != 0)
            {
                //open the device
                result = myUSB.CT_SetupDiEnumDeviceInterfaces(device_count);
                //get size of device path
                resultb = myUSB.CT_SetupDiGetDeviceInterfaceDetail(ref requiredSize, 0);
                size = requiredSize;
                //get device path
                resultb = myUSB.CT_SetupDiGetDeviceInterfaceDetailx(ref requiredSize, size);

                //is this the device i want?
                string deviceID = this.vendorID + "&" + this.productID;
                if (myUSB.DevicePathName.IndexOf(deviceID) > 0)
                {
                    //yes it is

                    //store device information
                    this.deviceCount = device_count;
                    this.devicePath = myUSB.DevicePathName;
                    deviceFound = true;

                    //init device
                    myUSB.CT_SetupDiEnumDeviceInterfaces(this.deviceCount);

                    size = 0;
                    requiredSize = 0;

                    resultb = myUSB.CT_SetupDiGetDeviceInterfaceDetail(ref requiredSize, size);
                    resultb = 0;
                    resultb = myUSB.CT_SetupDiGetDeviceInterfaceDetailx(ref requiredSize, size);
                    resultb = 0;
                    //create HID Device Handel
                    resultb = myUSB.CT_CreateFile(this.devicePath);

                    //we have found our device so stop searching
                    break;
                }
                device_count++;
            }

            //set connection state
            this.setConnectionState(deviceFound);
            //return state
            return this.getConnectionState();
        }
        //---#+************************************************************************
        //---NOTATION:
        //-  bool getDevices()
        //-
        //--- DESCRIPTION:
        //--  returns the number of devices with specified vendorID and productID 
        //                                                             Autor:      F.L.
        //-*************************************************************************+#*
        /// <summary>
        /// returns the number of devices with specified vendorID and productID 
        /// </summary>
        /// <returns>returns the number of devices with specified vendorID and productID</returns>
        public int getDevice()
        {
            //no device found yet
            //bool deviceFound = false;
            this.deviceCount = 0;
            this.devicePath = string.Empty;

            myUSB.CT_HidGuid();
            myUSB.CT_SetupDiGetClassDevs();

            int result = -1;
            int resultb = -1;
            int device_count = 0;
            int size = 0;
            int requiredSize = 0;
            int numberOfDevices = 0;
            //search the device until you have found it or no more devices in list
            while (result != 0)
            {
                //open the device
                result = myUSB.CT_SetupDiEnumDeviceInterfaces(device_count);
                //get size of device path
                resultb = myUSB.CT_SetupDiGetDeviceInterfaceDetail(ref requiredSize, 0);
                size = requiredSize;
                //get device path
                resultb = myUSB.CT_SetupDiGetDeviceInterfaceDetailx(ref requiredSize, size);

                //is this the device i want?
                string deviceID = this.vendorID + "&" + this.productID;
                if (myUSB.DevicePathName.IndexOf(deviceID) > 0)
                {
                    numberOfDevices++;
                }
                device_count++;
            }
            return numberOfDevices;
        }
        //---#+************************************************************************
        //---NOTATION:
        //-  bool writeData(char[] cDataToWrite)
        //-
        //--- DESCRIPTION:
        //--  writes data to the device and returns true if no error occured
        //                                                             Autor:      F.L.
        //-*************************************************************************+#*
        /// <summary>
        /// Writes the data.
        /// </summary>
        /// <param name="bDataToWrite">The b data to write.</param>
        /// <returns></returns>
        public bool writeData(byte[] bDataToWrite)
        {
            bool success = false;
            if (getConnectionState())
            {
                try
                {
                    //get output report length
                    //int myPtrToPreparsedData = -1;
                    // myUSB.CT_HidD_GetPreparsedData(myUSB.HidHandle, ref myPtrToPreparsedData);
                    // int code = myUSB.CT_HidP_GetCaps(myPtrToPreparsedData);

                    int outputReportByteLength = 65;

                    int bytesSend = 0;
                    //if bWriteData is bigger then one report diveide into sevral reports
                    while (bytesSend < bDataToWrite.Length)
                    {

                        // Set the size of the Output report buffer.
                        // byte[] OutputReportBuffer = new byte[myUSB.myHIDP_CAPS.OutputReportByteLength - 1 + 1];
                        byte[] OutputReportBuffer = new byte[outputReportByteLength - 1 + 1];
                        // Store the report ID in the first byte of the buffer:
                        OutputReportBuffer[0] = 0;

                        // Store the report data following the report ID.
                        for (int i = 1; i < OutputReportBuffer.Length; i++)
                        {
                            if (bytesSend < bDataToWrite.Length)
                            {
                                OutputReportBuffer[i] = bDataToWrite[bytesSend];
                                bytesSend++;
                            }
                            else
                            {
                                OutputReportBuffer[i] = 0;
                            }
                        }

                        OutputReport myOutputReport = new OutputReport();
                        success = myOutputReport.Write(OutputReportBuffer, myUSB.HidHandle);
                    }
                }
                //catch (System.AccessViolationException ex)
                catch
                {
                    success = false;
                }
            }
            else
            {
                success = false;
            }
            return success;
        }
        //---#+************************************************************************
        //---NOTATION:
        //-  readDataThread()
        //-
        //--- DESCRIPTION:
        //--  ThreadMethod for reading Data
        //                                                             Autor:      F.L.
        //-*************************************************************************+#*
        /// <summary>
        ///  ThreadMethod for reading Data
        /// </summary>
        public void readDataThread()
        {
            int receivedNull = 0;
            while (true)
            {
                int myPtrToPreparsedData = -1;
                if (myUSB.CT_HidD_GetPreparsedData(myUSB.HidHandle, ref myPtrToPreparsedData) != 0)
                {
                    int code = myUSB.CT_HidP_GetCaps(myPtrToPreparsedData);
                    int reportLength = myUSB.myHIDP_CAPS.InputReportByteLength;

                    while (true)
                    {//read until thread is stopped
                        byte[] myRead = myUSB.CT_ReadFile(myUSB.myHIDP_CAPS.InputReportByteLength);
                        if (myRead != null)
                        {
                            //ByteCount + bytes received
                            byteCount += myRead.Length;
                            //Store received bytes
                            /*  lock (recieveBuffer.SyncRoot)
                              {
                                  recieveBuffer.Add(myRead);
                              }*/
                            lock (USBInterface.usbBuffer.SyncRoot)
                            {
                                USBInterface.usbBuffer.Add(myRead);
                            }
                        }
                        else
                        {
                            //Recieved a lot of null bytes!
                            //mybe device disconnected?
                            if (receivedNull > 100)
                            {
                                receivedNull = 0;
                                Thread.Sleep(1);
                            }
                            receivedNull++;
                        }
                    }
                }
            }
        }
        //---#+************************************************************************
        //---NOTATION:
        //-  readData()
        //-
        //--- DESCRIPTION:
        //--  handling of the read thread
        //                                                             Autor:      F.L.
        //-*************************************************************************+#*
        /// <summary>
        /// controls the read thread
        /// </summary>
        public void readData()
        {

            if (dataReadingThread.ThreadState.ToString() == "Unstarted")
            {   //start the thread
                dataReadingThread.Start();
                Thread.Sleep(0);
            }
            else if (dataReadingThread.ThreadState.ToString() == "Running")
            {
                //Stop the Thread
                dataReadingThread.Abort();
            }
            else
            {
                //create Read Thread
                dataReadingThread = new Thread(new ThreadStart(readDataThread));
                //start the thread
                dataReadingThread.Start();
                Thread.Sleep(0);
            }

        }
        //---#+************************************************************************
        //---NOTATION:
        //-  abortreadData()
        //-
        //--- DESCRIPTION:
        //--  handling of the read thread
        //                                                             Autor:      F.L.
        //-*************************************************************************+#*
        /// <summary>
        /// Aborts the read thread.
        /// </summary>
        public void abortreadData()
        {

            if (dataReadingThread.ThreadState.ToString() == "Running")
            {
                //Stop the Thread
                dataReadingThread.Abort();
            }

        }

        //---#+************************************************************************
        //---NOTATION:
        //-  disconnectDevice()
        //-
        //--- DESCRIPTION:
        //--  disconnects the device and cleans up
        //                                                             Autor:      F.L.
        //-*************************************************************************+#*
        /// <summary>
        /// Disconnects the device.
        /// </summary>
        public void disconnectDevice()
        {
            //usbThread.Abort();
            myUSB.CT_CloseHandle(myUSB.HidHandle);
        }

        /* GET AND SET Methods*/
        //---#+************************************************************************
        //---NOTATION:
        //-  setDeviceData(String vID, String pID)
        //-
        //--- DESCRIPTION:
        //--  set vendor and product ID
        //                                                             Autor:      F.L.
        //-*************************************************************************+#*
        /// <summary>
        /// Sets the device data.
        /// </summary>
        /// <param name="vID">The vendor ID.</param>
        /// <param name="pID">The product ID.</param>
        public void setDeviceData(String vID, String pID)
        {
            this.vendorID = vID;
            this.productID = pID;
        }
        //---#+************************************************************************
        //---NOTATION:
        //-  String getVendorID()
        //-
        //--- DESCRIPTION:
        //--  returns the vendor ID
        //                                                             Autor:      F.L.
        //-*************************************************************************+#*

        /// <summary>
        /// Gets the vendor ID.
        /// </summary>
        /// <returns>the vendor ID</returns>
        public String getVendorID()
        {
            return this.vendorID;
        }
        //---#+************************************************************************
        //---NOTATION:
        //-  String getProductID()
        //-
        //--- DESCRIPTION:
        //--  returns the product ID
        //                                                             Autor:      F.L.
        //-*************************************************************************+#*
        /// <summary>
        /// Gets the product ID.
        /// </summary>
        /// <returns>the product ID</returns>
        public String getProductID()
        {
            return this.productID;
        }
        //---#+************************************************************************
        //---NOTATION:
        //-  setConnectionState(bool state)
        //-
        //--- DESCRIPTION:
        //--  set the connection state
        //                                                             Autor:      F.L.
        //-*************************************************************************+#*
        /// <summary>
        /// Sets the state of the connection.
        /// </summary>
        /// <param name="state">state</param>
        public void setConnectionState(bool state)
        {
            this.connectionState = state;
        }
        //---#+************************************************************************
        //---NOTATION:
        //-  bool getConnectionState()
        //-
        //--- DESCRIPTION:
        //--  returns the connection state
        //                                                             Autor:      F.L.
        //-*************************************************************************+#*
        /// <summary>
        /// Gets the state of the connection.
        /// </summary>
        /// <returns>true = connected; false = diconnected</returns>
        public bool getConnectionState()
        {
            return this.connectionState;
        }
        //---#+************************************************************************
        //---NOTATION:
        //-  int getDeviceCount()
        //-
        //--- DESCRIPTION:
        //--  returns the device count
        //                                                             Autor:      F.L.
        //-*************************************************************************+#*
        /// <summary>
        /// Gets the device count.
        /// </summary>
        /// <returns></returns>
        public ArrayList getDevices()
        {
            ArrayList devices = new ArrayList();

            //no device found yet
            //bool deviceFound = false;
            this.deviceCount = 0;
            this.devicePath = string.Empty;

            myUSB.CT_HidGuid();
            myUSB.CT_SetupDiGetClassDevs();

            int result = -1;
            int resultb = -1;
            int device_count = 0;
            int size = 0;
            int requiredSize = 0;
            int numberOfDevices = 0;
            //search the device until you have found it or no more devices in list

            while (result != 0)
            {
                //open the device
                result = myUSB.CT_SetupDiEnumDeviceInterfaces(device_count);
                //get size of device path
                resultb = myUSB.CT_SetupDiGetDeviceInterfaceDetail(ref requiredSize, 0);
                size = requiredSize;
                //get device path
                resultb = myUSB.CT_SetupDiGetDeviceInterfaceDetailx(ref requiredSize, size);

                //is this the device i want?
                string deviceID = this.vendorID;
                if (myUSB.DevicePathName.IndexOf(deviceID) > 0)
                {
                    devices.Add(myUSB.DevicePathName);
                    numberOfDevices++;
                }
                device_count++;
            }
            return devices;
        }
        //---#+************************************************************************
        //---NOTATION:
        //-  getDevicePath()
        //-
        //--- DESCRIPTION:
        //--  returns the device path
        //                                                             Autor:      F.L.
        //-*************************************************************************+#*
        /// <summary>
        /// Gets the device path.
        /// </summary>
        /// <returns></returns>
        public string getDevicePath()
        {
            return this.devicePath;
        }

        #endregion methods

        #region internal classes

        /// <summary>
        /// For reports the host sends to the device.
        /// Each report class defines a ProtectedWrite method for writing a type of report.
        /// </summary>
        internal abstract class HostReport
        {
            protected abstract bool ProtectedWrite(int deviceHandle, byte[] reportBuffer);

            internal bool Write(byte[] reportBuffer, int deviceHandle)
            {

                bool Success = false;

                // Purpose    : Calls the overridden ProtectedWrite routine.
                //            : This method enables other classes to override ProtectedWrite
                //            : while limiting access as Friend.
                //            : (Directly declaring Write as Friend MustOverride causes the
                //            : compiler(warning) "Other languages may permit Friend
                //            : Overridable members to be overridden.")
                // Accepts    : reportBuffer - contains the report ID and report data.
                //            : deviceHandle - handle to the device.             '
                // Returns    : True on success. False on failure.

                try
                {
                    Success = ProtectedWrite(deviceHandle, reportBuffer);
                }
                //catch (Exception ex)
                catch
                {

                }

                return Success;
            }
        }

        /// <summary>
        /// For Output reports the host sends to the device.
        /// Uses interrupt or control transfers depending on the device and OS.
        /// </summary>
        internal class OutputReport : HostReport
        {
            protected override bool ProtectedWrite(int hidHandle, byte[] outputReportBuffer)
            {

                // Purpose    : writes an Output report to the device.
                // Accepts    : HIDHandle - a handle to the device.
                //              OutputReportBuffer - contains the report ID and report to send.
                // Returns    : True on success. False on failure.

                int NumberOfBytesWritten = 0;
                int Result;
                bool Success = false;

                try
                {
                    // The host will use an interrupt transfer if the the HID has an interrupt OUT
                    // endpoint (requires USB 1.1 or later) AND the OS is NOT Windows 98 Gold (original version).
                    // Otherwise the the host will use a control transfer.
                    // The application doesn't have to know or care which type of transfer is used.

                    // ***
                    // API function: WriteFile
                    // Purpose: writes an Output report to the device.
                    // Accepts:
                    // A handle returned by CreateFile
                    // The output report byte length returned by HidP_GetCaps.
                    // An integer to hold the number of bytes written.
                    // Returns: True on success, False on failure.
                    // ***

                    Result = USBSharp.WriteFile(hidHandle, ref outputReportBuffer[0], outputReportBuffer.Length, ref NumberOfBytesWritten, 0);

                    Success = (Result == 0) ? false : true;

                }
                //catch (Exception ex)
                catch
                {
                }

                return Success;
            }
        }

        #endregion internal classes

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposeManagedResources)
        {
            if (!this.disposed)
            {
                if (disposeManagedResources)
                {
                    //only clear up managed stuff here
                }

                //clear up unmanaged stuff here
                if (myUSB.HidHandle != -1)
                {
                    myUSB.CT_CloseHandle(myUSB.HidHandle);
                }

                if (myUSB.hDevInfo != -1)
                {
                    myUSB.CT_SetupDiDestroyDeviceInfoList();
                }

                this.disposed = true;
            }
        }

        #endregion
    }

    /// <summary>
    /// A class that works just like ArrayList, but sends event
    /// notifications whenever the list changes
    /// </summary>
    public class ListWithEvent : System.Collections.ArrayList
    {
        #region events

        /// <summary>
        /// An event that clients can use to be notified whenever the
        /// elements of the list change
        /// </summary>
        public event System.EventHandler Changed;

        /// <summary>
        /// Invoke the Changed event; called whenever list changes
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected virtual void OnChanged(System.EventArgs e)
        {
            if (Changed != null)
            {
                Changed(this, e);
            }
        }

        #endregion events

        #region methods

        // Override some of the methods that can change the list;
        // invoke event after each:
        /// <summary>
        /// Fügt am Ende von <see cref="T:System.Collections.ArrayList"></see> ein Objekt hinzu.
        /// </summary>
        /// <param name="value">Das <see cref="T:System.Object"></see>, das am Ende der <see cref="T:System.Collections.ArrayList"></see> hinzugefügt werden soll. Der Wert kann null sein.</param>
        /// <returns>
        /// Der <see cref="T:System.Collections.ArrayList"></see>-Index, an dem value hinzugefügt wurde.
        /// </returns>
        /// <exception cref="T:System.NotSupportedException"><see cref="T:System.Collections.ArrayList"></see> ist schreibgeschützt.- oder -<see cref="T:System.Collections.ArrayList"></see> hat eine feste Größe. </exception>
        public override int Add(object value)
        {
            int i = base.Add(value);
            OnChanged(System.EventArgs.Empty);
            return i;
        }

        #endregion methods
    }

    /// <summary>
    /// Summary description
    /// </summary>
    public class USBSharp
    {

        #region Structs and DLL-Imports
        //
        //
        // Required constants, pointers, handles and variables 
        public int HidHandle = -1;				// file handle for a Hid devices
        public int hDevInfo = -1;				// handle for the device infoset
        public string DevicePathName = "";
        public const int DIGCF_PRESENT = 0x00000002;
        public const int DIGCF_DEVICEINTERFACE = 0x00000010;
        public const int DIGCF_INTERFACEDEVICE = 0x00000010;
        public const uint GENERIC_READ = 0x80000000;
        public const uint GENERIC_WRITE = 0x40000000;
        public const uint FILE_SHARE_READ = 0x00000001;
        public const uint FILE_SHARE_WRITE = 0x00000002;
        public const int OPEN_EXISTING = 3;
        public const int EV_RXFLAG = 0x0002;    // received certain character

        // specified in DCB
        public const int INVALID_HANDLE_VALUE = -1;
        public const int ERROR_INVALID_HANDLE = 6;
        public const int FILE_FLAG_OVERLAPED = 0x40000000;

        // GUID structure
        [StructLayout(LayoutKind.Sequential)]
        public unsafe struct GUID
        {
            public int Data1;
            public System.UInt16 Data2;
            public System.UInt16 Data3;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public byte[] data4;
        }

        // Device interface data
        [StructLayout(LayoutKind.Sequential)]
        public unsafe struct SP_DEVICE_INTERFACE_DATA
        {
            public int cbSize;
            public GUID InterfaceClassGuid;
            public int Flags;
            public int Reserved;
        }

        // Device interface detail data
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public unsafe struct PSP_DEVICE_INTERFACE_DETAIL_DATA
        {
            public int cbSize;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string DevicePath;
        }

        // HIDD_ATTRIBUTES
        [StructLayout(LayoutKind.Sequential)]
        public unsafe struct HIDD_ATTRIBUTES
        {
            public int Size; // = sizeof (struct _HIDD_ATTRIBUTES) = 10

            //
            // Vendor ids of this hid device
            //
            public System.UInt16 VendorID;
            public System.UInt16 ProductID;
            public System.UInt16 VersionNumber;

            //
            // Additional fields will be added to the end of this structure.
            //
        }

        // HIDP_CAPS
        [StructLayout(LayoutKind.Sequential)]
        public unsafe struct HIDP_CAPS
        {
            public System.UInt16 Usage;					// USHORT
            public System.UInt16 UsagePage;				// USHORT
            public System.UInt16 InputReportByteLength;
            public System.UInt16 OutputReportByteLength;
            public System.UInt16 FeatureReportByteLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 17)]
            public System.UInt16[] Reserved;				// USHORT  Reserved[17];			
            public System.UInt16 NumberLinkCollectionNodes;
            public System.UInt16 NumberInputButtonCaps;
            public System.UInt16 NumberInputValueCaps;
            public System.UInt16 NumberInputDataIndices;
            public System.UInt16 NumberOutputButtonCaps;
            public System.UInt16 NumberOutputValueCaps;
            public System.UInt16 NumberOutputDataIndices;
            public System.UInt16 NumberFeatureButtonCaps;
            public System.UInt16 NumberFeatureValueCaps;
            public System.UInt16 NumberFeatureDataIndices;
        }

        //HIDP_REPORT_TYPE 
        public enum HIDP_REPORT_TYPE
        {
            HidP_Input,		// 0 input
            HidP_Output,	// 1 output
            HidP_Feature	// 2 feature
        }

        // Structures in the union belonging to HIDP_VALUE_CAPS (see below)

        // Range
        [StructLayout(LayoutKind.Sequential)]
        public unsafe struct Range
        {
            public System.UInt16 UsageMin;			// USAGE	UsageMin; // USAGE  Usage; 
            public System.UInt16 UsageMax; 			// USAGE	UsageMax; // USAGE	Reserved1;
            public System.UInt16 StringMin;			// USHORT  StringMin; // StringIndex; 
            public System.UInt16 StringMax;			// USHORT	StringMax;// Reserved2;
            public System.UInt16 DesignatorMin;		// USHORT  DesignatorMin; // DesignatorIndex; 
            public System.UInt16 DesignatorMax;		// USHORT	DesignatorMax; //Reserved3; 
            public System.UInt16 DataIndexMin;		// USHORT  DataIndexMin;  // DataIndex; 
            public System.UInt16 DataIndexMax;		// USHORT	DataIndexMax; // Reserved4;
        }

        // Range
        [StructLayout(LayoutKind.Sequential)]
        public unsafe struct NotRange
        {
            public System.UInt16 Usage;
            public System.UInt16 Reserved1;
            public System.UInt16 StringIndex;
            public System.UInt16 Reserved2;
            public System.UInt16 DesignatorIndex;
            public System.UInt16 Reserved3;
            public System.UInt16 DataIndex;
            public System.UInt16 Reserved4;
        }

        //HIDP_VALUE_CAPS
        [StructLayout(LayoutKind.Explicit, CharSet = CharSet.Ansi)]
        public unsafe struct HIDP_VALUE_CAPS
        {
            //
            [FieldOffset(0)]
            public System.UInt16 UsagePage;					// USHORT
            [FieldOffset(2)]
            public System.Byte ReportID;						// UCHAR  ReportID;
            [MarshalAs(UnmanagedType.I1)]
            [FieldOffset(3)]
            public System.Boolean IsAlias;						// BOOLEAN  IsAlias;
            [FieldOffset(4)]
            public System.UInt16 BitField;						// USHORT  BitField;
            [FieldOffset(6)]
            public System.UInt16 LinkCollection;				//USHORT  LinkCollection;
            [FieldOffset(8)]
            public System.UInt16 LinkUsage;					// USAGE  LinkUsage;
            [FieldOffset(10)]
            public System.UInt16 LinkUsagePage;				// USAGE  LinkUsagePage;
            [MarshalAs(UnmanagedType.I1)]
            [FieldOffset(12)]
            public System.Boolean IsRange;					// BOOLEAN  IsRange;
            [MarshalAs(UnmanagedType.I1)]
            [FieldOffset(13)]
            public System.Boolean IsStringRange;				// BOOLEAN  IsStringRange;
            [MarshalAs(UnmanagedType.I1)]
            [FieldOffset(14)]
            public System.Boolean IsDesignatorRange;			// BOOLEAN  IsDesignatorRange;
            [MarshalAs(UnmanagedType.I1)]
            [FieldOffset(15)]
            public System.Boolean IsAbsolute;					// BOOLEAN  IsAbsolute;
            [MarshalAs(UnmanagedType.I1)]
            [FieldOffset(16)]
            public System.Boolean HasNull;					// BOOLEAN  HasNull;
            [FieldOffset(17)]
            public System.Char Reserved;						// UCHAR  Reserved;
            [FieldOffset(18)]
            public System.UInt16 BitSize;						// USHORT  BitSize;
            [FieldOffset(20)]
            public System.UInt16 ReportCount;					// USHORT  ReportCount;
            [FieldOffset(22)]
            public System.UInt16 Reserved2a;					// USHORT  Reserved2[5];		
            [FieldOffset(24)]
            public System.UInt16 Reserved2b;					// USHORT  Reserved2[5];
            [FieldOffset(26)]
            public System.UInt16 Reserved2c;					// USHORT  Reserved2[5];
            [FieldOffset(28)]
            public System.UInt16 Reserved2d;					// USHORT  Reserved2[5];
            [FieldOffset(30)]
            public System.UInt16 Reserved2e;					// USHORT  Reserved2[5];
            [FieldOffset(32)]
            public System.UInt16 UnitsExp;					// ULONG  UnitsExp;
            [FieldOffset(34)]
            public System.UInt16 Units;						// ULONG  Units;
            [FieldOffset(36)]
            public System.Int16 LogicalMin;					// LONG  LogicalMin;   ;
            [FieldOffset(38)]
            public System.Int16 LogicalMax;					// LONG  LogicalMax
            [FieldOffset(40)]
            public System.Int16 PhysicalMin;					// LONG  PhysicalMin, 
            [FieldOffset(42)]
            public System.Int16 PhysicalMax;					// LONG  PhysicalMax;
            // The Structs in the Union			
            [FieldOffset(44)]
            public Range Range;
            [FieldOffset(44)]
            public Range NotRange;
        }


        // ----------------------------------------------------------------------------------
        //
        //
        //
        // 
        // Define istances of the structures
        //
        //

        private GUID MYguid = new GUID();
        //
        // SP_DEVICE_INTERFACE_DATA  mySP_DEVICE_INTERFACE_DATA = new SP_DEVICE_INTERFACE_DATA();
        //
        public SP_DEVICE_INTERFACE_DATA mySP_DEVICE_INTERFACE_DATA;
        //
        public PSP_DEVICE_INTERFACE_DETAIL_DATA myPSP_DEVICE_INTERFACE_DETAIL_DATA;
        // 
        public HIDD_ATTRIBUTES myHIDD_ATTRIBUTES;
        //
        public HIDP_CAPS myHIDP_CAPS;
        //
        public HIDP_VALUE_CAPS[] myHIDP_VALUE_CAPS;

        // ******************************************************************************
        // DLL Calls
        // ******************************************************************************

        //Get GUID for the HID Class
        [DllImport("hid.dll", SetLastError = true)]
        static extern unsafe void HidD_GetHidGuid(
            ref GUID lpHidGuid);

        //Get array of structures with the HID info
        [DllImport("setupapi.dll", SetLastError = true)]
        static extern unsafe int SetupDiGetClassDevs(
            ref GUID lpHidGuid,
            int* Enumerator,
            int* hwndParent,
            int Flags);


        //Get context structure for a device interface element
        //
        //  SetupDiEnumDeviceInterfaces returns a context structure for a device 
        //  interface element of a device information set. Each call returns information 
        //  about one device interface; the function can be called repeatedly to get information 
        //  about several interfaces exposed by one or more devices.
        //
        [DllImport("setupapi.dll", SetLastError = true)]
        static extern unsafe int SetupDiEnumDeviceInterfaces(
            int DeviceInfoSet,
            int DeviceInfoData,
            ref  GUID lpHidGuid,
            int MemberIndex,
            ref  SP_DEVICE_INTERFACE_DATA lpDeviceInterfaceData);


        //	Get device Path name
        //  Works for the first pass  --> to get the required size

        [DllImport("setupapi.dll", SetLastError = true)]
        static extern unsafe int SetupDiGetDeviceInterfaceDetail(
            int DeviceInfoSet,
            ref SP_DEVICE_INTERFACE_DATA lpDeviceInterfaceData,
            int* aPtr,
            int detailSize,
            ref int requiredSize,
            int* bPtr);

        //	Get device Path name
        //  Works for second pass (overide), once size value is known

        [DllImport("setupapi.dll", SetLastError = true)]
        static extern unsafe int SetupDiGetDeviceInterfaceDetail(
            int DeviceInfoSet,
            ref SP_DEVICE_INTERFACE_DATA lpDeviceInterfaceData,
            ref PSP_DEVICE_INTERFACE_DETAIL_DATA myPSP_DEVICE_INTERFACE_DETAIL_DATA,
            int detailSize,
            ref int requiredSize,
            int* bPtr);

        // Get Create File
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern int CreateFile(
            string lpFileName,							// file name
            uint dwDesiredAccess,						// access mode
            uint dwShareMode,							// share mode
            uint lpSecurityAttributes,					// SD
            uint dwCreationDisposition,					// how to create
            uint dwFlagsAndAttributes,					// file attributes
            uint hTemplateFile							// handle to template file
            );


        [DllImport("hid.dll", SetLastError = true)]
        private static extern int HidD_GetAttributes(
            int hObject,								// IN HANDLE  HidDeviceObject,
            ref HIDD_ATTRIBUTES Attributes);			// OUT PHIDD_ATTRIBUTES  Attributes


        [DllImport("hid.dll", SetLastError = true)]
        private unsafe static extern int HidD_GetPreparsedData(
            int hObject,								// IN HANDLE  HidDeviceObject,
            ref int pPHIDP_PREPARSED_DATA);				// OUT PHIDP_PREPARSED_DATA  *PreparsedData


        [DllImport("hid.dll", SetLastError = true)]
        private unsafe static extern int HidP_GetCaps(
            int pPHIDP_PREPARSED_DATA,					// IN PHIDP_PREPARSED_DATA  PreparsedData,
            ref HIDP_CAPS myPHIDP_CAPS);				// OUT PHIDP_CAPS  Capabilities

        [DllImport("hid.dll")]
        public unsafe static extern bool HidD_SetOutputReport(int HidDeviceObject, ref byte lpReportBuffer, int ReportBufferLength);


        [DllImport("hid.dll", SetLastError = true)]
        private unsafe static extern int HidP_GetValueCaps(
            HIDP_REPORT_TYPE ReportType,								// IN HIDP_REPORT_TYPE  ReportType,
            [In, Out] HIDP_VALUE_CAPS[] ValueCaps,						// OUT PHIDP_VALUE_CAPS  ValueCaps,
            ref int ValueCapsLength,									// IN OUT PULONG  ValueCapsLength,
            int pPHIDP_PREPARSED_DATA);									// IN PHIDP_PREPARSED_DATA  PreparsedData



        [DllImport("kernel32.dll", SetLastError = true)]
        private unsafe static extern bool ReadFile(
            int hFile,						// handle to file
            byte[] lpBuffer,				// data buffer
            int nNumberOfBytesToRead,		// number of bytes to read
            ref int lpNumberOfBytesRead,	// number of bytes read
            int* ptr
            // 
            // ref OVERLAPPED lpOverlapped		// overlapped buffer
            );

        [DllImport("setupapi.dll", SetLastError = true)]
        static extern unsafe int SetupDiDestroyDeviceInfoList(
            int DeviceInfoSet				// IN HDEVINFO  DeviceInfoSet
            );

        // 13
        [DllImport("hid.dll", SetLastError = true)]
        static extern unsafe int HidD_FreePreparsedData(
            int pPHIDP_PREPARSED_DATA			// IN PHIDP_PREPARSED_DATA  PreparsedData
            );



        // API declarations relating to file I/O.

        // ******************************************************************************
        // API constants
        // ******************************************************************************


        public const uint FILE_FLAG_OVERLAPPED = 0x40000000;
        public const int WAIT_TIMEOUT = 0x102;
        public const short WAIT_OBJECT_0 = 0;

        // ******************************************************************************
        // Structures and classes for API calls, listed alphabetically
        // ******************************************************************************

        [StructLayout(LayoutKind.Sequential)]
        public struct OVERLAPPED
        {
            public int Internal;
            public int InternalHigh;
            public int Offset;
            public int OffsetHigh;
            public int hEvent;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SECURITY_ATTRIBUTES
        {
            public int nLength;
            public int lpSecurityDescriptor;
            public int bInheritHandle;
        }


        [DllImport("kernel32.dll")]
        static public extern int CancelIo(int hFile);

        [DllImport("kernel32.dll")]
        static public extern int CloseHandle(int hObject);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        static public extern int CreateEvent(ref SECURITY_ATTRIBUTES SecurityAttributes, int bManualReset, int bInitialState, string lpName);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        static public extern int
          CreateFile(string lpFileName, uint dwDesiredAccess, uint dwShareMode, ref SECURITY_ATTRIBUTES lpSecurityAttributes, int dwCreationDisposition, uint dwFlagsAndAttributes, int hTemplateFile);

        [DllImport("kernel32.dll")]
        static public extern int ReadFile(int hFile, ref byte lpBuffer, int nNumberOfBytesToRead, ref int lpNumberOfBytesRead, ref OVERLAPPED lpOverlapped);

        [DllImport("kernel32.dll")]
        static public extern int WaitForSingleObject(int hHandle, int dwMilliseconds);

        [DllImport("kernel32.dll")]
        static public extern int WriteFile(int hFile, ref byte lpBuffer, int nNumberOfBytesToWrite, ref int lpNumberOfBytesWritten, int lpOverlapped);



        #endregion

        #region methods

        //	--------------------------------------
        // Managed Code wrappers for the DLL Calls
        // Naming convention ---> same as unmaneged DLL call with prefix CT_xxxx 
        //---#+************************************************************************
        //---NOTATION:
        //-  CT_HidGuid()
        //-
        //--- DESCRIPTION:
        //--  GUID for HID
        //                                                             Autor:      F.L.
        //-*************************************************************************+#*
        public unsafe void CT_HidGuid()
        {
            HidD_GetHidGuid(ref MYguid);	// 
        }

        //---#+************************************************************************
        //---NOTATION:
        //-  CT_SetupDiGetClassDevs()
        //-
        //--- DESCRIPTION:
        //--  
        //                                                             Autor:      F.L.
        //-*************************************************************************+#*
        public unsafe int CT_SetupDiGetClassDevs()
        {
            hDevInfo = SetupDiGetClassDevs(
                ref MYguid,
                null,
                null,
                DIGCF_INTERFACEDEVICE | DIGCF_PRESENT);
            return hDevInfo;
        }

        //---#+************************************************************************
        //---NOTATION:
        //-  int CT_SetupDiEnumDeviceInterfaces(int memberIndex)
        //-
        //--- DESCRIPTION:
        //--  
        //                                                             Autor:      F.L.
        //-*************************************************************************+#*
        public unsafe int CT_SetupDiEnumDeviceInterfaces(int memberIndex)
        {
            mySP_DEVICE_INTERFACE_DATA = new SP_DEVICE_INTERFACE_DATA();
            mySP_DEVICE_INTERFACE_DATA.cbSize = Marshal.SizeOf(mySP_DEVICE_INTERFACE_DATA);
            int result = SetupDiEnumDeviceInterfaces(
                hDevInfo,
                0,
                ref  MYguid,
                memberIndex,
                ref mySP_DEVICE_INTERFACE_DATA);
            return result;
        }

        //---#+************************************************************************
        //---NOTATION:
        //-  int CT_SetupDiGetDeviceInterfaceDetail(ref int RequiredSize, int DeviceInterfaceDetailDataSize)
        //-
        //--- DESCRIPTION:
        //--    results = 0 is OK with the first pass of the routine since we are
        //-     trying to get the RequiredSize parameter so in the next call we can read the entire detail 
        //
        //                                                              Autor:      F.L.
        //-*************************************************************************+#*
        public unsafe int CT_SetupDiGetDeviceInterfaceDetail(ref int RequiredSize, int DeviceInterfaceDetailDataSize)
        {
            int results =
            SetupDiGetDeviceInterfaceDetail(
                hDevInfo,							// IN HDEVINFO  DeviceInfoSet,
                ref mySP_DEVICE_INTERFACE_DATA,		// IN PSP_DEVICE_INTERFACE_DATA  DeviceInterfaceData,
                null,								// OUT PSP_DEVICE_INTERFACE_DETAIL_DATA  DeviceInterfaceDetailData,  OPTIONAL
                DeviceInterfaceDetailDataSize,		// IN DWORD  DeviceInterfaceDetailDataSize,
                ref RequiredSize,					// OUT PDWORD  RequiredSize,  OPTIONAL
                null); // 
            return results;
        }

        //---#+************************************************************************
        //---NOTATION:
        //-  int CT_SetupDiEnumDeviceInterfaces(int memberIndex)
        //-
        //--- DESCRIPTION:
        //--    results = 1 iin the second pass of the routine is success
        //-     DeviceInterfaceDetailDataSize parameter (RequiredSize) came from the first pass
        //
        //                                                              Autor:      F.L.
        //-*************************************************************************+#*
        public unsafe int CT_SetupDiGetDeviceInterfaceDetailx(ref int RequiredSize, int DeviceInterfaceDetailDataSize)
        {
            myPSP_DEVICE_INTERFACE_DETAIL_DATA = new PSP_DEVICE_INTERFACE_DETAIL_DATA();

            //
            // This part needs some work

            // if I use the following line of code 
            // myPSP_DEVICE_INTERFACE_DETAIL_DATA.cbSize = sizeof(PSP_DEVICE_INTERFACE_DETAIL_DATA);
            // I get the following error
            // !! Cannot take the address or size of a variable of a managed type ('USBSharp.USBSharp.PSP_DEVICE_INTERFACE_DETAIL_DATA')
            // 

            // As a result.. this is hard coded for now !!! 
            // for the c struct PSP_DEVICE_INTERFACE_DETAIL_DATA dizeof = DWORD cbSize (size 4) + Char[0] (size 1) = Total size 5 ?
            //
            myPSP_DEVICE_INTERFACE_DETAIL_DATA.cbSize = 5;

            int results =
                SetupDiGetDeviceInterfaceDetail(
                hDevInfo,									// IN HDEVINFO  DeviceInfoSet,
                ref mySP_DEVICE_INTERFACE_DATA,				// IN PSP_DEVICE_INTERFACE_DATA  DeviceInterfaceData,
                ref myPSP_DEVICE_INTERFACE_DETAIL_DATA,		// DeviceInterfaceDetailData,  OPTIONAL
                DeviceInterfaceDetailDataSize,				// IN DWORD  DeviceInterfaceDetailDataSize,
                ref RequiredSize,							// OUT PDWORD  RequiredSize,  OPTIONAL
                null); // 
            DevicePathName = myPSP_DEVICE_INTERFACE_DETAIL_DATA.DevicePath;
            return results;
        }

        //---#+************************************************************************
        //---NOTATION:
        //-  CT_CreateFile(string DeviceName)
        //-
        //--- DESCRIPTION:
        //--    Get a handle (opens file) to the HID device
        //-     returns  0 is no success - Returns 1 if success
        //
        //                                                              Autor:      F.L.
        //-*************************************************************************+#*
        public unsafe int CT_CreateFile(string DeviceName)
        {

            HidHandle = CreateFile(
                DeviceName,
                GENERIC_READ | GENERIC_WRITE,
                FILE_SHARE_READ | FILE_SHARE_WRITE,
                0,
                OPEN_EXISTING,
                0,
                0);
            if (HidHandle == -1)
            {
                return 0;
            }
            else
            {
                return 1;
            }

        }

        //---#+************************************************************************
        //---NOTATION:
        //-  int CT_CloseHandle(int hObject)
        //-
        //--- DESCRIPTION:
        //--    Closed the file and disposes of the handle
        //
        //                                                              Autor:      F.L.
        //-*************************************************************************+#*
        public unsafe int CT_CloseHandle(int hObject)
        {
            HidHandle = -1;
            return CloseHandle(hObject);

        }

        //---#+************************************************************************
        //---NOTATION:
        //-  int CT_HidD_GetAttributes(int hObject))
        //-
        //--- DESCRIPTION:
        //--    Get a handle to the HID device
        //
        //                                                              Autor:      F.L.
        //-*************************************************************************+#*
        public unsafe int CT_HidD_GetAttributes(int hObject)
        {
            // Create an instance of HIDD_ATTRIBUTES
            myHIDD_ATTRIBUTES = new HIDD_ATTRIBUTES();
            // Calculate its size
            myHIDD_ATTRIBUTES.Size = sizeof(HIDD_ATTRIBUTES);

            return HidD_GetAttributes(
                    hObject,
                    ref myHIDD_ATTRIBUTES);
        }

        //---#+************************************************************************
        //---NOTATION:
        //-  int CT_HidD_GetPreparsedData(int hObject, ref int pPHIDP_PREPARSED_DATA)
        //-
        //--- DESCRIPTION:
        //--    Gets a pointer to the preparsed data buffer
        //
        //                                                              Autor:      F.L.
        //-*************************************************************************+#*
        public unsafe int CT_HidD_GetPreparsedData(int hObject, ref int pPHIDP_PREPARSED_DATA)
        {
            return HidD_GetPreparsedData(
            hObject,
            ref pPHIDP_PREPARSED_DATA);
        }

        //---#+************************************************************************
        //---NOTATION:
        //-  int CT_HidD_SetOutputReport(int HidDeviceObject, ref byte lpReportBuffer, int ReportBufferLength)
        //-
        //--- DESCRIPTION:
        //--    
        //
        //                                                              Autor:      F.L.
        //-*************************************************************************+#*
        public unsafe bool CT_HidD_SetOutputReport(int HidDeviceObject, ref byte lpReportBuffer, int ReportBufferLength)
        {
            return HidD_SetOutputReport(HidDeviceObject, ref lpReportBuffer, ReportBufferLength);
        }

        //---#+************************************************************************
        //---NOTATION:
        //-  int CT_HidP_GetCaps(int pPreparsedData)
        //-
        //--- DESCRIPTION:
        //--    Gets the capabilities report
        //
        //                                                              Autor:      F.L.
        //-*************************************************************************+#*
        public unsafe int CT_HidP_GetCaps(int pPreparsedData)
        {
            myHIDP_CAPS = new HIDP_CAPS();
            return HidP_GetCaps(
             pPreparsedData,
             ref myHIDP_CAPS);
        }

        //---#+************************************************************************
        //---NOTATION:
        //-  int CT_HidP_GetValueCaps(ref int CalsCapsLength, int pPHIDP_PREPARSED_DATA)
        //-
        //--- DESCRIPTION:
        //--    Value Capabilities
        //
        //                                                              Autor:      F.L.
        //-*************************************************************************+#*
        public int CT_HidP_GetValueCaps(ref int CalsCapsLength, int pPHIDP_PREPARSED_DATA)
        {

            HIDP_REPORT_TYPE myType = 0;
            myHIDP_VALUE_CAPS = new HIDP_VALUE_CAPS[5];
            return HidP_GetValueCaps(
                myType,
                myHIDP_VALUE_CAPS,
                ref CalsCapsLength,
                pPHIDP_PREPARSED_DATA);

        }

        //---#+************************************************************************
        //---NOTATION:
        //-  byte[] CT_ReadFile(int InputReportByteLength)
        //-
        //--- DESCRIPTION:
        //--    read Port
        //
        //                                                              Autor:      F.L.
        //-*************************************************************************+#*
        public unsafe byte[] CT_ReadFile(int InputReportByteLength)
        {
            int BytesRead = 0;
            byte[] BufBytes = new byte[InputReportByteLength];
            if (ReadFile(HidHandle, BufBytes, InputReportByteLength, ref BytesRead, null))
            {
                byte[] OutBytes = new byte[BytesRead];
                Array.Copy(BufBytes, OutBytes, BytesRead);
                return OutBytes;
            }
            else
            {
                return null;
            }
        }

        //---#+************************************************************************
        //---NOTATION:
        //-  CT_WriteFile(int hFile, ref byte lpBuffer, int nNumberOfBytesToWrite, ref int lpNumberOfBytesWritten, int lpOverlapped)
        //-
        //--- DESCRIPTION:
        //--    write Port
        //
        //                                                              Autor:      F.L.
        //-*************************************************************************+#*
        public unsafe int CT_WriteFile(int hFile, ref byte lpBuffer, int nNumberOfBytesToWrite, ref int lpNumberOfBytesWritten, int lpOverlapped)
        {
            return WriteFile(hFile, ref lpBuffer, nNumberOfBytesToWrite, ref lpNumberOfBytesWritten, lpOverlapped); ;
        }

        //---#+************************************************************************
        //---NOTATION:
        //-  int CT_SetupDiDestroyDeviceInfoList()
        //-
        //--- DESCRIPTION:
        //--    DestroyDeviceInfoList
        //
        //                                                              Autor:      F.L.
        //-*************************************************************************+#*
        public int CT_SetupDiDestroyDeviceInfoList()
        {
            return SetupDiDestroyDeviceInfoList(hDevInfo);

        }

        //---#+************************************************************************
        //---NOTATION:
        //-  int CT_HidD_FreePreparsedData(int pPHIDP_PREPARSED_DATA)
        //-
        //--- DESCRIPTION:
        //--    FreePreparsedData
        //
        //                                                              Autor:      F.L.
        //-*************************************************************************+#*
        public int CT_HidD_FreePreparsedData(int pPHIDP_PREPARSED_DATA)
        {
            return SetupDiDestroyDeviceInfoList(pPHIDP_PREPARSED_DATA);
        }

        #endregion methods

        #region properties

        internal HidApiDeclarations HidApiDeclarations
        {
            get
            {
                throw new System.NotImplementedException();
            }
            set
            {
            }
        }

        #endregion properties

    }


}


