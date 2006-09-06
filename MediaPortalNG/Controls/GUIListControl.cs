using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Media;

namespace MediaPortal
{

    public class GUIListControl : ListView, IGUIControl
    {
        private TextAlignment _Align;
        private string _Label;
        private string _Texture;
        private string _Hyperlink;
        private int _onUP;
        private int _onDOWN;
        private int _onLEFT;
        private int _onRIGHT;
        private const string _thumbStyleName = "GUIThumbnailControl";
        private const string _listStyleName = "GUIListControl";
        private const string _filmstripStyleName = "GUIFilmstripControl";

        private int _controlID;
        private MPListStyle _currentListStyle;

        public string StatusExt = "0/0";
        public string _statusText = "0 items";

        public enum MPListStyle
        {
            Listview=1,
            Thumbnail,
            Filmstrip
        }
  
        public GUIListControl()
        {
            string styleName = this.GetType().ToString() + "Style";
            styleName = styleName.Replace("MediaPortal.", "");
            object resource = null;
            try
            {
                resource = this.FindResource(styleName);
                if (resource != null)
                {
                    this.Style = resource as Style;
                }
            }
            catch { }
            this.Loaded+=new RoutedEventHandler(GUIListControl_Loaded);
        }


        void GUIListControl_Loaded(object sender, RoutedEventArgs e)
        {
            StatusText = "Test";
        }

        private void ChangeListStyle(MPListStyle style)
        {
            
            object theStyle=null;
            if (style == MPListStyle.Thumbnail)
            {
                try
                {
                    theStyle = FindResource(_thumbStyleName);
                }
                catch { }
            }
            if (style == MPListStyle.Filmstrip)
            {
                try
                {
                    theStyle = FindResource(_filmstripStyleName);
                }
                catch { }
            }
            if (style == MPListStyle.Listview)
            {
                try
                {
                    theStyle = FindResource(_listStyleName);
                }
                catch { }
            }

            if (theStyle != null)
            {
                this.Style = null;
                this.Style = theStyle as Style;
                this.ApplyTemplate();
                _currentListStyle = style;
            }

        }

        // control id
        public int ID
        {
            get
            {
                return _controlID;
            }
            set
            {
                _controlID = value;
            }
        }

        public static readonly DependencyProperty IDProperty =
        DependencyProperty.Register("ID", typeof(int), typeof(GUIListControl),
                new FrameworkPropertyMetadata(new PropertyChangedCallback(OnIDChanged)));

        private static void OnIDChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            GUIListControl control = (GUIListControl)obj;

            RoutedPropertyChangedEventArgs<int> e = new RoutedPropertyChangedEventArgs<int>(
                (int)args.OldValue, (int)args.NewValue, IDChangedEvent);
            control.OnIDChanged(e);
        }

        public static readonly RoutedEvent IDChangedEvent = EventManager.RegisterRoutedEvent(
    "IDChanged", RoutingStrategy.Bubble,
    typeof(RoutedPropertyChangedEventHandler<int>), typeof(GUIListControl));

        public event RoutedPropertyChangedEventHandler<int> IDChanged
        {
            add { AddHandler(IDChangedEvent, value); }
            remove { RemoveHandler(IDChangedEvent, value); }
        }

        protected virtual void OnIDChanged(RoutedPropertyChangedEventArgs<int> args)
        {
            _controlID = args.NewValue;
            RaiseEvent(args);
        }


        // style
        public MPListStyle ListStyle
        {
            get
            {
                return _currentListStyle;
            }
            set
            {
                _currentListStyle = value;
            }
        }

        public static readonly DependencyProperty ListStyleProperty =
        DependencyProperty.Register("ListStyle", typeof(MPListStyle), typeof(GUIListControl),
                new FrameworkPropertyMetadata(new PropertyChangedCallback(OnListStyleChanged)));

        private static void OnListStyleChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            GUIListControl control = (GUIListControl)obj;

            RoutedPropertyChangedEventArgs<MPListStyle> e = new RoutedPropertyChangedEventArgs<MPListStyle>(
                (MPListStyle)args.OldValue, (MPListStyle)args.NewValue, ListStyleChangedEvent);
            control.OnListStyleChanged(e);
        }

        public static readonly RoutedEvent ListStyleChangedEvent = EventManager.RegisterRoutedEvent(
    "ListStyleChanged", RoutingStrategy.Bubble,
    typeof(RoutedPropertyChangedEventHandler<MPListStyle>), typeof(GUIListControl));

        public event RoutedPropertyChangedEventHandler<MPListStyle> ListStyleChanged
        {
            add { AddHandler(ListStyleChangedEvent, value); }
            remove { RemoveHandler(ListStyleChangedEvent, value); }
        }

        protected virtual void OnListStyleChanged(RoutedPropertyChangedEventArgs<MPListStyle> args)
        {
            ChangeListStyle(args.NewValue);
            RaiseEvent(args);
        }

        // status text
        public string StatusText
        {
            get
            {
                return _statusText;
            }
            set
            {
                _statusText = value;
            }
        }

        public static readonly DependencyProperty StatusTextProperty =
        DependencyProperty.Register("StatusText", typeof(string), typeof(GUIListControl), new FrameworkPropertyMetadata("", new PropertyChangedCallback(OnStatusTextChanged)));

        protected event RoutedPropertyChangedEventHandler<string> StatusTextChanged
        {
            add
            {
                AddHandler(StatusTextChangedEvent, value);
            }
            remove
            {
                RemoveHandler(StatusTextChangedEvent, value);
            }
        }

        public static readonly RoutedEvent StatusTextChangedEvent = EventManager.RegisterRoutedEvent(
        "StatusTextChanged", RoutingStrategy.Bubble,
        typeof(RoutedPropertyChangedEventHandler<string>), typeof(GUIListControl));

        private static void OnStatusTextChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            GUIListControl control = (GUIListControl)obj;

            RoutedPropertyChangedEventArgs<string> e = new RoutedPropertyChangedEventArgs<string>(
                (string)args.OldValue, (string)args.NewValue, StatusTextChangedEvent);
            control.OnStatusTextChanged(e);
        }

        protected virtual void OnStatusTextChanged(RoutedPropertyChangedEventArgs<string> args)
        {
            _statusText = args.NewValue;
            RaiseEvent(args);
        }

        public int OnUP
        {
            get
            {
                return _onUP;
            }
            set
            {
                _onUP = value;
            }
        }

        public static readonly DependencyProperty OnUPProperty =
        DependencyProperty.Register("OnUP", typeof(int), typeof(GUIListControl),
                new FrameworkPropertyMetadata(new PropertyChangedCallback(OnOnUPChanged)));

        private static void OnOnUPChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            GUIListControl control = (GUIListControl)obj;
            control.OnOnUPChanged(args);
        }


        protected virtual void OnOnUPChanged(DependencyPropertyChangedEventArgs args)
        {
            _onUP = (int)args.NewValue;
        }


        // ondown

        public int OnDOWN
        {
            get
            {
                return _onDOWN;
            }
            set
            {
                _onDOWN = value;
            }
        }

        public static readonly DependencyProperty OnDOWNProperty =
        DependencyProperty.Register("OnDOWN", typeof(int), typeof(GUIListControl),
                new FrameworkPropertyMetadata(new PropertyChangedCallback(OnOnDOWNChanged)));

        private static void OnOnDOWNChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            GUIListControl control = (GUIListControl)obj;
            control.OnOnDOWNChanged(args);
        }


        protected virtual void OnOnDOWNChanged(DependencyPropertyChangedEventArgs args)
        {
            _onDOWN = (int)args.NewValue;
        }

        // onleft

        public int OnLEFT
        {
            get
            {
                return _onLEFT;
            }
            set
            {
                _onLEFT = value;
            }
        }

        public static readonly DependencyProperty OnLEFTProperty =
        DependencyProperty.Register("OnLEFT", typeof(int), typeof(GUIListControl),
                new FrameworkPropertyMetadata(new PropertyChangedCallback(OnOnLEFTChanged)));

        private static void OnOnLEFTChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            GUIListControl control = (GUIListControl)obj;
            control.OnOnLEFTChanged(args);
        }


        protected virtual void OnOnLEFTChanged(DependencyPropertyChangedEventArgs args)
        {
            _onLEFT = (int)args.NewValue;
        }

        // onright

        public int OnRIGHT
        {
            get
            {
                return _onRIGHT;
            }
            set
            {
                _onRIGHT = value;
            }
        }

        public static readonly DependencyProperty OnRIGHTProperty =
        DependencyProperty.Register("OnRIGHT", typeof(int), typeof(GUIListControl),
                new FrameworkPropertyMetadata(new PropertyChangedCallback(OnOnRIGHTChanged)));

        private static void OnOnRIGHTChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            GUIListControl control = (GUIListControl)obj;
            control.OnOnRIGHTChanged(args);
        }


        protected virtual void OnOnRIGHTChanged(DependencyPropertyChangedEventArgs args)
        {
            _onRIGHT = (int)args.NewValue;
        }

        public string Label
        {
            get
            {
                return _Label;
            }
            set
            {
                _Label = value;
            }
        }

        public static readonly DependencyProperty LabelProperty =
        DependencyProperty.Register("Label", typeof(string), typeof(GUIListControl),
                new FrameworkPropertyMetadata(new PropertyChangedCallback(OnLabelChanged)));

        private static void OnLabelChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            GUIListControl control = (GUIListControl)obj;
            control.OnLabelChanged(args);
        }

        protected virtual void OnLabelChanged(DependencyPropertyChangedEventArgs args)
        {
            _Label = (string)args.NewValue;
        }

        // hyperlink
        public string Hyperlink
        {
            get
            {
                return _Hyperlink;
            }
            set
            {
                _Hyperlink = value;
            }
        }

        public static readonly DependencyProperty HyperlinkProperty =
        DependencyProperty.Register("Hyperlink", typeof(string), typeof(GUIListControl),
                new FrameworkPropertyMetadata(new PropertyChangedCallback(OnHyperlinkChanged)));

        private static void OnHyperlinkChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            GUIListControl control = (GUIListControl)obj;
            control.OnHyperlinkChanged(args);
        }

        protected virtual void OnHyperlinkChanged(DependencyPropertyChangedEventArgs args)
        {
            _Hyperlink = (string)args.NewValue;
        }

        // Texture
        public string Texture
        {
            get
            {
                return _Texture;
            }
            set
            {
                _Texture = value;
            }
        }

        public static readonly DependencyProperty TextureProperty =
        DependencyProperty.Register("Texture", typeof(string), typeof(GUIListControl),
                new FrameworkPropertyMetadata(new PropertyChangedCallback(OnTextureChanged)));

        private static void OnTextureChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            GUIListControl control = (GUIListControl)obj;
            control.OnTextureChanged(args);
        }

        protected virtual void OnTextureChanged(DependencyPropertyChangedEventArgs args)
        {
            _Texture = (string)args.NewValue;
        }
        public TextAlignment Align
        {
            get
            {
                return _Align;
            }
            set
            {
                _Align = value;
            }
        }

        public static readonly DependencyProperty AlignProperty =
        DependencyProperty.Register("Align", typeof(TextAlignment), typeof(GUIListControl),
                new FrameworkPropertyMetadata(new PropertyChangedCallback(OnAlignChanged)));

        private static void OnAlignChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            GUIListControl control = (GUIListControl)obj;
            control.OnAlignChanged(args);
        }

        protected virtual void OnAlignChanged(DependencyPropertyChangedEventArgs args)
        {
            _Align = (TextAlignment)args.NewValue;
        }
        // disabled color
        private Brush _DisabledColor;

        public Brush DisabledColor
        {
            get
            {
                return _DisabledColor;
            }
            set
            {
                _DisabledColor = value;
            }
        }

        public static readonly DependencyProperty DisabledColorProperty =
        DependencyProperty.Register("DisabledColor", typeof(Brush), typeof(GUIListControl),
                new FrameworkPropertyMetadata(new PropertyChangedCallback(OnDisabledColorChanged)));

        private static void OnDisabledColorChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            GUIListControl control = (GUIListControl)obj;
            control.OnDisabledColorChanged(args);
        }

        protected virtual void OnDisabledColorChanged(DependencyPropertyChangedEventArgs args)
        {
            _DisabledColor = (Brush)args.NewValue;
        }



        
       protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            if (_Align == TextAlignment.Right)
                Canvas.SetLeft(this, _PosX - sizeInfo.NewSize.Width);
        }
string IGUIControl.Label
        {
            get
            {
                return (string)GetValue(LabelProperty);
            }
            set
            {
                SetValue(LabelProperty, value);
            }
        }
        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            base.OnSelectionChanged(e);
            Core.OnClick(this);
        }

        int IGUIControl.ID
        {
            get { return _controlID; }
        }

        int IGUIControl.OnUP
        {
            get { return _onUP; }
        }

        int IGUIControl.OnDOWN
        {
            get { return _onDOWN; }
        }

        int IGUIControl.OnLEFT
        {
            get { return _onLEFT; }
        }

        int IGUIControl.OnRIGHT
        {
            get { return _onRIGHT; }
        }

           
        //
        // property PosY
        // 
        private double _PosY;

        public double PosY
        {
            get
            {
                return (double)GetValue(PosYProperty);
            }
            set
            {
                SetValue(PosYProperty,value);
            }
        }

        public static readonly DependencyProperty PosYProperty =
        DependencyProperty.Register("PosY", typeof(double), typeof(GUIListControl),
            new FrameworkPropertyMetadata(new PropertyChangedCallback(OnPosYChanged)));

        private static void OnPosYChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            GUIListControl control = (GUIListControl)obj;
            control.OnPosYChanged(args);
        }

        protected virtual void OnPosYChanged(DependencyPropertyChangedEventArgs args)
        {
            _PosY=(double)args.NewValue;
            Canvas.SetTop(this, _PosY);
        }        
        //
        // property PosX
        // 
        private double _PosX;

        public double PosX
        {
            get
            {
                return (double)GetValue(PosXProperty);
            }
            set
            {
                SetValue(PosXProperty,value);
            }
        }

        public static readonly DependencyProperty PosXProperty =
        DependencyProperty.Register("PosX", typeof(double), typeof(GUIListControl),
            new FrameworkPropertyMetadata(new PropertyChangedCallback(OnPosXChanged)));

        private static void OnPosXChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            GUIListControl control = (GUIListControl)obj;
            control.OnPosXChanged(args);
        }

        protected virtual void OnPosXChanged(DependencyPropertyChangedEventArgs args)
        {
            _PosX=(double)args.NewValue;
            Canvas.SetLeft(this, _PosX);
        }
}
}

