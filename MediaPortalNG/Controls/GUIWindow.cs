using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;


namespace MediaPortal
{
  public class GUIWindow : Canvas, IGUIWindow
  {

    public GUIWindow()
    {

    }

    public void HandleKeyDown(Key key)
    {
      Visual element = Core.FindVisualByKeyboardFocused(this);
      IGUIControl ctrl;
      if (element == null)
      {
        element = Core.FindVisualByID(this, _defaultControl);
        if (element != null)
          ((FrameworkElement)element).Focus();
        return;
      }
      if (element == null) return;
      ctrl = element as IGUIControl;
      if (ctrl != null)
      {
        int nextID = -1;
        if (key == Key.Up)
        {
          nextID = ctrl.OnUp;
        }
        if (key == Key.Down)
        {
          nextID = ctrl.OnDown;
        }
        if (key == Key.Left)
        {
          nextID = ctrl.OnLeft;
        }
        if (key == Key.Right)
        {
          nextID = ctrl.OnRight;
        }
        if (nextID >= 1)
        {
          Visual ctrlToFocus = Core.FindVisualByID(this, nextID);
          if (ctrlToFocus != null)
            ((FrameworkElement)ctrlToFocus).Focus();
        }

      }
    }
    //
    // property WindowID
    // 
    private int _windowId;

    public int WindowID
    {
      get
      {
        return (int)GetValue(WindowIDProperty);
      }
      set
      {
        SetValue(WindowIDProperty, value);
      }
    }

    public static readonly DependencyProperty WindowIDProperty =
    DependencyProperty.Register("WindowID", typeof(int), typeof(GUIWindow),
        new FrameworkPropertyMetadata(new PropertyChangedCallback(OnWindowIDChanged)));

    private static void OnWindowIDChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
    {
      GUIWindow control = (GUIWindow)obj;
      control.OnWindowIDChanged(args);
    }

    protected virtual void OnWindowIDChanged(DependencyPropertyChangedEventArgs args)
    {
      _windowId = (int)args.NewValue;

    }

    //
    // property DefaultControl
    // 
    private int _defaultControl;

    public int DefaultControl
    {
      get
      {
        return (int)GetValue(DefaultControlProperty);
      }
      set
      {
        SetValue(DefaultControlProperty, value);
      }
    }

    public static readonly DependencyProperty DefaultControlProperty =
    DependencyProperty.Register("DefaultControl", typeof(int), typeof(GUIWindow),
        new FrameworkPropertyMetadata(new PropertyChangedCallback(OnDefaultControlChanged)));

    private static void OnDefaultControlChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
    {
      GUIWindow control = (GUIWindow)obj;
      control.OnDefaultControlChanged(args);
    }

    protected virtual void OnDefaultControlChanged(DependencyPropertyChangedEventArgs args)
    {
      _defaultControl = (int)args.NewValue;

    }

    //
    // property WindowName
    // 
    private string _windowName;

    public string WindowName
    {
      get
      {
        return (string)GetValue(WindowNameProperty);
      }
      set
      {
        SetValue(WindowNameProperty, value);
      }
    }

    public static readonly DependencyProperty WindowNameProperty =
    DependencyProperty.Register("WindowName", typeof(string), typeof(GUIWindow),
        new FrameworkPropertyMetadata(new PropertyChangedCallback(OnWindowNameChanged)));

    private static void OnWindowNameChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
    {
      GUIWindow control = (GUIWindow)obj;
      control.OnWindowNameChanged(args);
    }

    protected virtual void OnWindowNameChanged(DependencyPropertyChangedEventArgs args)
    {
      _windowName = (string)args.NewValue;

    }

    //
    // property AllowOverlay
    // 
    private bool _allowOverlay;

    public bool AllowOverlay
    {
      get
      {
        return (bool)GetValue(AllowOverlayProperty);
      }
      set
      {
        SetValue(AllowOverlayProperty, value);
      }
    }

    public static readonly DependencyProperty AllowOverlayProperty =
    DependencyProperty.Register("AllowOverlay", typeof(bool), typeof(GUIWindow),
        new FrameworkPropertyMetadata(new PropertyChangedCallback(OnAllowOverlayChanged)));

    private static void OnAllowOverlayChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
    {
      GUIWindow control = (GUIWindow)obj;
      control.OnAllowOverlayChanged(args);
    }

    protected virtual void OnAllowOverlayChanged(DependencyPropertyChangedEventArgs args)
    {
      _allowOverlay = (bool)args.NewValue;

    }

    #region IGUIWindow Members

    int IGUIWindow.ID
    {
      get { return _windowId; }
    }

    int IGUIWindow.DefaultControl
    {
      get { return _defaultControl; }
    }

    #endregion
  }
}

