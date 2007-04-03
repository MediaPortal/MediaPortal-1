using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Dialogs
{
  /// <summary>
  /// Interaction logic for UserControl1.xaml
  /// </summary>

  public partial class MpMenu : System.Windows.Window
  {

    DialogMenuItemCollection _menuItems;
    int _selectedIndex = 0;
    /// <summary>
    /// Initializes a new instance of the <see cref="MpImageMenu"/> class.
    /// </summary>
    public MpMenu()
    {
      _menuItems = new DialogMenuItemCollection();
      this.WindowStyle = WindowStyle.None;
      this.ShowInTaskbar = false;
      this.ResizeMode = ResizeMode.NoResize;
      this.AllowsTransparency = true;
      InitializeComponent();
    }
    public MpMenu(DialogMenuItemCollection items)
    {
      _menuItems = items;
      this.WindowStyle = WindowStyle.None;
      this.ShowInTaskbar = false;
      this.ResizeMode = ResizeMode.NoResize;
      this.AllowsTransparency = true;
      InitializeComponent();
    }
    public string SubTitle
    {
      get
      {
        return labelDate.Content.ToString();
      }
      set
      {
        labelDate.Content = value;
      }
    }

    public string Header
    {
      get
      {
        return labelHeader.Content.ToString();
      }
      set
      {
        labelHeader.Content = value;
      }
    }
    /// <summary>
    /// Shows this instance.
    /// </summary>

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
      Keyboard.AddPreviewKeyDownHandler(this, new KeyEventHandler(onKeyDown));
      gridContent.AddHandler(ListBoxItem.MouseDownEvent, new RoutedEventHandler(Button_Click), true);
      gridContent.KeyDown += new KeyEventHandler(gridContent_KeyDown);
      Mouse.AddMouseMoveHandler(this, new MouseEventHandler(handleMouse));
      this.Visibility = Visibility.Visible;
      gridContent.ItemsSource = _menuItems;
      gridContent.SelectionChanged += new SelectionChangedEventHandler(gridContent_SelectionChanged);
      gridContent.SelectionMode = SelectionMode.Single;
      if (_selectedIndex >= 0)
        gridContent.SelectedIndex = _selectedIndex;
      else
        gridContent.SelectedIndex = 0;

      Keyboard.Focus(gridContent);
      gridContent.ItemContainerGenerator.StatusChanged += new EventHandler(ItemContainerGenerator_StatusChanged);
    }

    void gridContent_KeyDown(object sender, KeyEventArgs e)
    {
      if (e.Key == Key.Enter)
      {
        this.Close();
      }
    }

    void ItemContainerGenerator_StatusChanged(object sender, EventArgs e)
    {
      if (gridContent.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
      {
        DependencyObject dp=gridContent.ItemContainerGenerator.ContainerFromItem(gridContent.SelectedItem);
        Keyboard.Focus((ListBoxItem)dp);
      }
    }

    void handleMouse(object sender, MouseEventArgs e)
    {
      FrameworkElement element = Mouse.DirectlyOver as FrameworkElement;
      while (element != null)
      {
        if (element as Button != null)
        {
          Keyboard.Focus((Button)element);
          return;
        }
        if (element as ListBoxItem != null)
        {
          gridContent.SelectedItem = element.DataContext;
          Keyboard.Focus((ListBoxItem)element);
          return;
        }
        element = element.TemplatedParent as FrameworkElement;
      }
    }

    void Button_Click(object sender, RoutedEventArgs e)
    {
      if (e.Source == gridContent)
      {
        this.Close();
      }
    }
    void gridContent_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      _selectedIndex = gridContent.SelectedIndex;
    }

    /// <summary>
    /// Called when key pressed
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="System.Windows.Input.KeyEventArgs"/> instance containing the event data.</param>
    void onKeyDown(object sender, KeyEventArgs e)
    {
      if (e.Key == Key.Right)
      {
        Keyboard.Focus(buttonClose);
        e.Handled = true;
      }
      if (e.Key == Key.Left)
      {
        Keyboard.Focus(gridContent);
        e.Handled = true;
      }
      if (e.Key == System.Windows.Input.Key.Escape)
      {
        //return to previous screen
        SelectedIndex = -1;
        this.Close();
        return;
      }
    }


    /// <summary>
    /// Gets or sets the index of the selected.
    /// </summary>
    /// <value>The index of the selected.</value>
    public int SelectedIndex
    {
      get
      {
        return _selectedIndex;
      }
      set
      {
        _selectedIndex = value;
      }
    }

    /// <summary>
    /// Gets or sets the selected item.
    /// </summary>
    /// <value>The selected item.</value>
    public DialogMenuItem SelectedItem
    {
      get
      {
        return _menuItems[_selectedIndex];
      }
      set
      {
        _menuItems[_selectedIndex] = value;
      }
    }

    /// <summary>
    /// Gets or sets the items.
    /// </summary>
    /// <value>The items.</value>
    public DialogMenuItemCollection Items
    {
      get
      {
        return _menuItems;
      }
      set
      {
        _menuItems = value;
      }
    }


    /// <summary>
    /// Called when [close clicked].
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="args">The <see cref="System.EventArgs"/> instance containing the event data.</param>
    void OnCloseClicked(object sender, EventArgs args)
    {
      SelectedIndex = -1;
      this.Visibility = Visibility.Hidden;
    }

  }
}