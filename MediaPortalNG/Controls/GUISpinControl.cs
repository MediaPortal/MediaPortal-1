using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace MediaPortal
{

    public class GUISpinControl : UserControl, IGUIControl
    {
        private TextAlignment _Align;
        private string _Label;
        private string _Texture;
        private string _Hyperlink;
        private int _onUP;
        private int _onDOWN;
        private int _onLEFT;
        private int _onRIGHT;
        private static RoutedCommand _increaseCommand;
        private static RoutedCommand _decreaseCommand;
        private static int MinValue = -100, MaxValue = 100;
        private int _controlID;

        public void SetMinValue(int val)
        {
            MinValue = val;
            if(Value<val)
                Value = val;
        }

        public void SetMaxValue(int val)
        {
            MaxValue = val;
            if (Value > val)
                Value = val;
        }

     
        public GUISpinControl()
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
            SetMaxValue(10);
            SetMinValue(0);
            InitializeCommands(); 
            // set layout to left because it is the default
            this.Loaded += new RoutedEventHandler(GUISpinControl_Loaded);
            ButtonContentValue = 0;
            ButtonUpValue = 1;
            ButtonDownValue = 2;
            //
         

        }

        void GUISpinControl_Loaded(object sender, RoutedEventArgs e)
        {
            Align = TextAlignment.Right;
            Align = TextAlignment.Left;
        }

       

        public int Value
        {
            get { return (int)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }


         public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(
                "Value", typeof(int), typeof(GUISpinControl),
                new FrameworkPropertyMetadata(MinValue, new PropertyChangedCallback(OnValueChanged)));
       
        
        private static void OnValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            GUISpinControl control = (GUISpinControl)obj;

            RoutedPropertyChangedEventArgs<int> e = new RoutedPropertyChangedEventArgs<int>(
                (int)args.OldValue, (int)args.NewValue, ValueChangedEvent);
            control.OnValueChanged(e);
        }

        public static readonly RoutedEvent ValueChangedEvent = EventManager.RegisterRoutedEvent(
            "ValueChanged", RoutingStrategy.Bubble,
            typeof(RoutedPropertyChangedEventHandler<int>), typeof(GUISpinControl));

        public event RoutedPropertyChangedEventHandler<int> ValueChanged
        {
            add { AddHandler(ValueChangedEvent, value); }
            remove { RemoveHandler(ValueChangedEvent, value); }
        }
 
        
        
        protected virtual void OnValueChanged(RoutedPropertyChangedEventArgs<int> args)
        {
            RaiseEvent(args);
            Core.OnClick(this);
        }

 
        public static RoutedCommand IncreaseCommand
        {
            get
            {
                return _increaseCommand;
            }
        }
        public static RoutedCommand DecreaseCommand
        {
            get
            {
                return _decreaseCommand;
            }
        }

        private static void InitializeCommands()
        {
            _increaseCommand = new RoutedCommand("IncreaseCommand", typeof(GUISpinControl));
            CommandManager.RegisterClassCommandBinding(typeof(GUISpinControl), new CommandBinding(_increaseCommand, OnIncreaseCommand));
            CommandManager.RegisterClassInputBinding(typeof(GUISpinControl), new InputBinding(_increaseCommand, new KeyGesture(Key.Up)));

            _decreaseCommand = new RoutedCommand("DecreaseCommand", typeof(GUISpinControl));
            CommandManager.RegisterClassCommandBinding(typeof(GUISpinControl), new CommandBinding(_decreaseCommand, OnDecreaseCommand));
            CommandManager.RegisterClassInputBinding(typeof(GUISpinControl), new InputBinding(_decreaseCommand, new KeyGesture(Key.Down)));
        }

        private static void OnIncreaseCommand(object sender, ExecutedRoutedEventArgs e)
        {
            GUISpinControl control = sender as GUISpinControl;
            if (control != null)
            {
                control.OnIncrease();
            }
        }
        private static void OnDecreaseCommand(object sender, ExecutedRoutedEventArgs e)
        {
            GUISpinControl control = sender as GUISpinControl;
            if (control != null)
            {
                control.OnDecrease();
            }
        }

        protected virtual void OnIncrease()
        {
            if (Value < MaxValue)
            {
                Value += 1;
            }
        }
        protected virtual void OnDecrease()
        {
            if (Value > MinValue)
            {
                Value -= 1;
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
        DependencyProperty.Register("ID", typeof(int), typeof(GUISpinControl),
                new FrameworkPropertyMetadata(new PropertyChangedCallback(OnIDChanged)));

        private static void OnIDChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            GUISpinControl control = (GUISpinControl)obj;

            RoutedPropertyChangedEventArgs<int> e = new RoutedPropertyChangedEventArgs<int>(
                (int)args.OldValue, (int)args.NewValue, IDChangedEvent);
            control.OnIDChanged(e);
        }

        public static readonly RoutedEvent IDChangedEvent = EventManager.RegisterRoutedEvent(
    "IDChanged", RoutingStrategy.Bubble,
    typeof(RoutedPropertyChangedEventHandler<int>), typeof(GUISpinControl));

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
        DependencyProperty.Register("OnUP", typeof(int), typeof(GUISpinControl),
                new FrameworkPropertyMetadata(new PropertyChangedCallback(OnOnUPChanged)));

        private static void OnOnUPChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            GUISpinControl control = (GUISpinControl)obj;
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
        DependencyProperty.Register("OnDOWN", typeof(int), typeof(GUISpinControl),
                new FrameworkPropertyMetadata(new PropertyChangedCallback(OnOnDOWNChanged)));

        private static void OnOnDOWNChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            GUISpinControl control = (GUISpinControl)obj;
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
        DependencyProperty.Register("OnLEFT", typeof(int), typeof(GUISpinControl),
                new FrameworkPropertyMetadata(new PropertyChangedCallback(OnOnLEFTChanged)));

        private static void OnOnLEFTChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            GUISpinControl control = (GUISpinControl)obj;
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
        DependencyProperty.Register("OnRIGHT", typeof(int), typeof(GUISpinControl),
                new FrameworkPropertyMetadata(new PropertyChangedCallback(OnOnRIGHTChanged)));

        private static void OnOnRIGHTChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            GUISpinControl control = (GUISpinControl)obj;
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
        DependencyProperty.Register("Label", typeof(string), typeof(GUISpinControl),
                new FrameworkPropertyMetadata(new PropertyChangedCallback(OnLabelChanged)));

        private static void OnLabelChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            GUISpinControl control = (GUISpinControl)obj;
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
        DependencyProperty.Register("Hyperlink", typeof(string), typeof(GUISpinControl),
                new FrameworkPropertyMetadata(new PropertyChangedCallback(OnHyperlinkChanged)));

        private static void OnHyperlinkChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            GUISpinControl control = (GUISpinControl)obj;
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
        DependencyProperty.Register("Texture", typeof(string), typeof(GUISpinControl),
                new FrameworkPropertyMetadata(new PropertyChangedCallback(OnTextureChanged)));

        private static void OnTextureChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            GUISpinControl control = (GUISpinControl)obj;
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
                return (TextAlignment)GetValue(AlignProperty);
            }
            set
            {
                SetValue(AlignProperty,value);
            }
        }

        public static readonly DependencyProperty AlignProperty =
        DependencyProperty.Register("Align", typeof(TextAlignment), typeof(GUISpinControl),
                new FrameworkPropertyMetadata(new PropertyChangedCallback(OnAlignChanged)));

        private static void OnAlignChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            GUISpinControl control = (GUISpinControl)obj;
            control.OnAlignChanged(args);
        }

        protected virtual void OnAlignChanged(DependencyPropertyChangedEventArgs args)
        {
            _Align = (TextAlignment)args.NewValue;

            // try to get the grid object
            Visual obj = Core.FindVisualByName(this, "ContentGrid");
           Grid grid=null;
            if (obj != null)
                grid = (Grid)obj;
            if (_Align == TextAlignment.Center)
            {
                ButtonContentValue = 1;
                ButtonUpValue = 0;
                ButtonDownValue = 2;
                if (grid != null)
                {
                    grid.ColumnDefinitions[0].Width = new GridLength(16);
                    grid.ColumnDefinitions[2].Width = new GridLength(16);
                }

            }
            if (_Align == TextAlignment.Left)
            {
                ButtonContentValue = 0;
                ButtonUpValue = 1;
                ButtonDownValue = 2;
                if (grid != null)
                {
                    grid.ColumnDefinitions[1].Width = new GridLength(16);
                    grid.ColumnDefinitions[2].Width = new GridLength(16);
                }
            }
            if (_Align == TextAlignment.Right)
            {
                ButtonContentValue = 2;
                ButtonUpValue = 0;
                ButtonDownValue = 1;
                if (grid != null)
                {
                    grid.ColumnDefinitions[0].Width = new GridLength(16);
                    grid.ColumnDefinitions[1].Width = new GridLength(16);
                }
            }
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
        DependencyProperty.Register("DisabledColor", typeof(Brush), typeof(GUISpinControl),
                new FrameworkPropertyMetadata(new PropertyChangedCallback(OnDisabledColorChanged)));

        private static void OnDisabledColorChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            GUISpinControl control = (GUISpinControl)obj;
            control.OnDisabledColorChanged(args);
        }

        protected virtual void OnDisabledColorChanged(DependencyPropertyChangedEventArgs args)
        {
            _DisabledColor = (Brush)args.NewValue;
        }


        
       protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            //if (_Align == TextAlignment.Right)
            //    Canvas.SetLeft(this, _PosX - sizeInfo.NewSize.Width);
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
        DependencyProperty.Register("PosY", typeof(double), typeof(GUISpinControl),
            new FrameworkPropertyMetadata(new PropertyChangedCallback(OnPosYChanged)));

        private static void OnPosYChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            GUISpinControl control = (GUISpinControl)obj;
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
        DependencyProperty.Register("PosX", typeof(double), typeof(GUISpinControl),
            new FrameworkPropertyMetadata(new PropertyChangedCallback(OnPosXChanged)));

        private static void OnPosXChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            GUISpinControl control = (GUISpinControl)obj;
            control.OnPosXChanged(args);
        }

        protected virtual void OnPosXChanged(DependencyPropertyChangedEventArgs args)
        {
            _PosX=(double)args.NewValue;
            Canvas.SetLeft(this, _PosX);
        }        
        //
        // property Orientation
        // 
        private Orientation _Orientation;

        public Orientation Orientation
        {
            get
            {
                return (Orientation)GetValue(OrientationProperty);
            }
            set
            {
                SetValue(OrientationProperty,value);
            }
        }

        public static readonly DependencyProperty OrientationProperty =
        DependencyProperty.Register("Orientation", typeof(Orientation), typeof(GUISpinControl),
            new FrameworkPropertyMetadata(new PropertyChangedCallback(OnOrientationChanged)));

        private static void OnOrientationChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            GUISpinControl control = (GUISpinControl)obj;
            control.OnOrientationChanged(args);
        }

        protected virtual void OnOrientationChanged(DependencyPropertyChangedEventArgs args)
        {
            _Orientation=(Orientation)args.NewValue;
        }        
        //
        // property ShowRange
        // 
        private bool _ShowRange;

        public bool ShowRange
        {
            get
            {
                return (bool)GetValue(ShowRangeProperty);
            }
            set
            {
                SetValue(ShowRangeProperty,value);
            }
        }

        public static readonly DependencyProperty ShowRangeProperty =
        DependencyProperty.Register("ShowRange", typeof(bool), typeof(GUISpinControl),
            new FrameworkPropertyMetadata(new PropertyChangedCallback(OnShowRangeChanged)));

        private static void OnShowRangeChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            GUISpinControl control = (GUISpinControl)obj;
            control.OnShowRangeChanged(args);
        }

        protected virtual void OnShowRangeChanged(DependencyPropertyChangedEventArgs args)
        {
            _ShowRange=(bool)args.NewValue;
            
        }        
        //
        // property Reverse
        // 
        private bool _Reverse;

        public bool Reverse
        {
            get
            {
                return (bool)GetValue(ReverseProperty);
            }
            set
            {
                SetValue(ReverseProperty,value);
            }
        }

        public static readonly DependencyProperty ReverseProperty =
        DependencyProperty.Register("Reverse", typeof(bool), typeof(GUISpinControl),
            new FrameworkPropertyMetadata(new PropertyChangedCallback(OnReverseChanged)));

        private static void OnReverseChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            GUISpinControl control = (GUISpinControl)obj;
            control.OnReverseChanged(args);
        }

        protected virtual void OnReverseChanged(DependencyPropertyChangedEventArgs args)
        {
            _Reverse=(bool)args.NewValue;
            
        }        
        //
        // property ButtonUpValue
        // 
        private int _ButtonUpValue;

        public int ButtonUpValue
        {
            get
            {
                return (int)GetValue(ButtonUpValueProperty);
            }
            set
            {
                SetValue(ButtonUpValueProperty,value);
            }
        }

        public static readonly DependencyProperty ButtonUpValueProperty =
        DependencyProperty.Register("ButtonUpValue", typeof(int), typeof(GUISpinControl),
            new FrameworkPropertyMetadata(new PropertyChangedCallback(OnButtonUpValueChanged)));

        private static void OnButtonUpValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            GUISpinControl control = (GUISpinControl)obj;
            control.OnButtonUpValueChanged(args);
        }

        protected virtual void OnButtonUpValueChanged(DependencyPropertyChangedEventArgs args)
        {
            _ButtonUpValue=(int)args.NewValue;
            
        }        
        //
        // property ButtonDownValue
        // 
        private int _ButtonDownValue;

        public int ButtonDownValue
        {
            get
            {
                return (int)GetValue(ButtonDownValueProperty);
            }
            set
            {
                SetValue(ButtonDownValueProperty,value);
            }
        }

        public static readonly DependencyProperty ButtonDownValueProperty =
        DependencyProperty.Register("ButtonDownValue", typeof(int), typeof(GUISpinControl),
            new FrameworkPropertyMetadata(new PropertyChangedCallback(OnButtonDownValueChanged)));

        private static void OnButtonDownValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            GUISpinControl control = (GUISpinControl)obj;
            control.OnButtonDownValueChanged(args);
        }

        protected virtual void OnButtonDownValueChanged(DependencyPropertyChangedEventArgs args)
        {
            _ButtonDownValue=(int)args.NewValue;
            
        }        
        //
        // property ButtonContentValue
        // 
        private int _ButtonContentValue;

        public int ButtonContentValue
        {
            get
            {
                return (int)GetValue(ButtonContentValueProperty);
            }
            set
            {
                SetValue(ButtonContentValueProperty,value);
            }
        }

        public static readonly DependencyProperty ButtonContentValueProperty =
        DependencyProperty.Register("ButtonContentValue", typeof(int), typeof(GUISpinControl),
            new FrameworkPropertyMetadata(new PropertyChangedCallback(OnButtonContentValueChanged)));

        private static void OnButtonContentValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            GUISpinControl control = (GUISpinControl)obj;
            control.OnButtonContentValueChanged(args);
        }

        protected virtual void OnButtonContentValueChanged(DependencyPropertyChangedEventArgs args)
        {
            _ButtonContentValue=(int)args.NewValue;
            
        }        
        //
        // property Column0
        // 
        private int _Column0;

        public int Column0
        {
            get
            {
                return (int)GetValue(Column0Property);
            }
            set
            {
                SetValue(Column0Property,value);
            }
        }

        public static readonly DependencyProperty Column0Property =
        DependencyProperty.Register("Column0", typeof(int), typeof(GUISpinControl),
            new FrameworkPropertyMetadata(new PropertyChangedCallback(OnColumn0Changed)));

        private static void OnColumn0Changed(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            GUISpinControl control = (GUISpinControl)obj;
            control.OnColumn0Changed(args);
        }

        protected virtual void OnColumn0Changed(DependencyPropertyChangedEventArgs args)
        {
            _Column0=(int)args.NewValue;
            
        }        
        //
        // property Column1
        // 
        private int _Column1;

        public int Column1
        {
            get
            {
                return (int)GetValue(Column1Property);
            }
            set
            {
                SetValue(Column1Property,value);
            }
        }

        public static readonly DependencyProperty Column1Property =
        DependencyProperty.Register("Column1", typeof(int), typeof(GUISpinControl),
            new FrameworkPropertyMetadata(new PropertyChangedCallback(OnColumn1Changed)));

        private static void OnColumn1Changed(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            GUISpinControl control = (GUISpinControl)obj;
            control.OnColumn1Changed(args);
        }

        protected virtual void OnColumn1Changed(DependencyPropertyChangedEventArgs args)
        {
            _Column1=(int)args.NewValue;
            
        }        
        //
        // property Column2
        // 
        private int _Column2;

        public int Column2
        {
            get
            {
                return (int)GetValue(Column2Property);
            }
            set
            {
                SetValue(Column2Property,value);
            }
        }

        public static readonly DependencyProperty Column2Property =
        DependencyProperty.Register("Column2", typeof(int), typeof(GUISpinControl),
            new FrameworkPropertyMetadata(new PropertyChangedCallback(OnColumn2Changed)));

        private static void OnColumn2Changed(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            GUISpinControl control = (GUISpinControl)obj;
            control.OnColumn2Changed(args);
        }

        protected virtual void OnColumn2Changed(DependencyPropertyChangedEventArgs args)
        {
            _Column2=(int)args.NewValue;
            
        }
}
}

