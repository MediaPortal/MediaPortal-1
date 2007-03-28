using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
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

    List<DialogMenuItem> _menuItems = new List<DialogMenuItem>();
    int _selectedIndex = 0;
    /// <summary>
    /// Initializes a new instance of the <see cref="MpMenu"/> class.
    /// </summary>
    public MpMenu()
    {
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
      gridContent.Children.Clear();
      this.Visibility = Visibility.Visible;
      int maxColumns = 0;
      Grid grid = new Grid();
      grid.VerticalAlignment = VerticalAlignment.Top;
      for (int row = 0; row < _menuItems.Count; ++row)
      {
        DialogMenuItem item = _menuItems[row];
        grid.RowDefinitions.Add(new RowDefinition());
        for (int i = 0; i < item.SubItems.Count; ++i)
        {
          if (i >= maxColumns)
          {
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            maxColumns = i + 1;
          }
          Grid.SetColumn(item.SubItems[i], i);
          Grid.SetRow(item.SubItems[i], row);
          grid.Children.Add(item.SubItems[i]);
          if ((item.SubItems[i] as Button) != null)
          {
            Button element = (Button)item.SubItems[i];
            element.Click += new RoutedEventHandler(OnItemClicked);
          }
          item.SubItems[i].MouseEnter += new MouseEventHandler(subItemMouseEnter);
        }
      }
      gridContent.Children.Add(grid);
      if (_selectedIndex >= 0 && _selectedIndex < _menuItems.Count)
      {
        Keyboard.Focus(_menuItems[SelectedIndex].SubItems[0]);
      }
    }

    /// <summary>
    /// Updates the selected index.
    /// </summary>
    void UpdateSelectedIndex()
    {
      int selectedIndex = -1;
      foreach (DialogMenuItem item in _menuItems)
      {
        selectedIndex++;
        foreach (UIElement element in item.SubItems)
        {
          if (element.IsFocused)
          {
            SelectedIndex = selectedIndex;
            return;
          }
        }
      }
      SelectedIndex = -1;
    }

    /// <summary>
    /// Called when an item is clicked
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
    void OnItemClicked(object sender, RoutedEventArgs e)
    {
      UpdateSelectedIndex();
      this.Visibility = Visibility.Hidden;
      return;
    }

    /// <summary>
    /// Called when key pressed
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="System.Windows.Input.KeyEventArgs"/> instance containing the event data.</param>
    void OnScrollKeyDown(object sender, KeyEventArgs e)
    {
      if (e.Key == Key.Right)
      {
        Keyboard.Focus(buttonClose);
        e.Handled = true;
      }
      if (e.Key == Key.Left)
      {
        Keyboard.Focus(_menuItems[SelectedIndex].SubItems[0]);
        e.Handled = true;
      }
    }

    /// <summary>
    /// called when mouse enters a button
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="System.Windows.Input.MouseEventArgs"/> instance containing the event data.</param>
    void subItemMouseEnter(object sender, MouseEventArgs e)
    {
      IInputElement b = sender as IInputElement;
      if (b != null)
      {
        Keyboard.Focus(b);
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
    public List<DialogMenuItem> Items
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