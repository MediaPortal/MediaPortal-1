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
  public class DialogMenuItem
  {
    List<UIElement> _subItems = new List<UIElement>();

    public DialogMenuItem(string logo, string label1, string label2, string label3)
    {
      Button button = new Button();
      button.Template = (ControlTemplate)Application.Current.Resources["MpButton"];
      Grid grid = new Grid();
      grid.ColumnDefinitions.Add(new ColumnDefinition());
      grid.ColumnDefinitions.Add(new ColumnDefinition());
      grid.ColumnDefinitions.Add(new ColumnDefinition());
      grid.ColumnDefinitions.Add(new ColumnDefinition());
      grid.ColumnDefinitions.Add(new ColumnDefinition());
      grid.ColumnDefinitions.Add(new ColumnDefinition());
      grid.ColumnDefinitions.Add(new ColumnDefinition());
      grid.ColumnDefinitions.Add(new ColumnDefinition());
      grid.ColumnDefinitions.Add(new ColumnDefinition());
      grid.RowDefinitions.Add(new RowDefinition());
      grid.RowDefinitions.Add(new RowDefinition());
      if (logo.Length > 0)
      {
        Image image = new Image();
        PngBitmapDecoder decoder = new PngBitmapDecoder(new Uri(logo, UriKind.Relative), BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);

        image.Source = decoder.Frames[0];
        Grid.SetColumn(image, 0);
        Grid.SetRow(image, 0);
        Grid.SetRowSpan(image, 2);
        grid.Children.Add(image);
      }
      Label label = new Label();
      label.Content = label1;
      label.Style = (Style)Application.Current.Resources["LabelNormalStyleWhite"];
      Grid.SetColumn(label, 1);
      Grid.SetRow(label, 0);
      Grid.SetColumnSpan(label, 8);
      grid.Children.Add(label);

      label = new Label();
      label.Content = label2;
      label.Style = (Style)Application.Current.Resources["LabelSmallStyleWhite"];
      Grid.SetColumn(label, 1);
      Grid.SetColumnSpan(label, 6);
      Grid.SetRow(label, 1);
      grid.Children.Add(label);

      label = new Label();
      label.Content = label3;
      label.Style = (Style)Application.Current.Resources["LabelSmallStyleWhite"];
      label.HorizontalAlignment = HorizontalAlignment.Right;
      label.Margin = new Thickness(0, 0, 20, 0);
      Grid.SetColumn(label, 7);
      Grid.SetColumnSpan(label, 2);
      Grid.SetRow(label, 1);
      grid.Children.Add(label);
      grid.Loaded += new RoutedEventHandler(grid_Loaded);
      button.Content = grid;

      _subItems.Add(button);
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="DialogMenuItem"/> class.
    /// </summary>
    /// <param name="label1">The label1.</param>
    /// <param name="label2">The label2.</param>
    /// <param name="label3">The label3.</param>
    public DialogMenuItem(string label1, string label2, string label3)
    {
      Button button = new Button();
      button.Template = (ControlTemplate)Application.Current.Resources["MpButton"];
      Grid grid = new Grid();
      grid.ColumnDefinitions.Add(new ColumnDefinition());
      grid.ColumnDefinitions.Add(new ColumnDefinition());
      grid.RowDefinitions.Add(new RowDefinition());
      grid.RowDefinitions.Add(new RowDefinition());

      Label label = new Label();
      label.Content = label1;
      label.Style = (Style)Application.Current.Resources["LabelNormalStyleWhite"];
      Grid.SetColumn(label, 0);
      Grid.SetRow(label, 0);
      Grid.SetColumnSpan(label, 2);
      grid.Children.Add(label);

      label = new Label();
      label.Content = label2;
      label.Style = (Style)Application.Current.Resources["LabelSmallStyleWhite"];
      Grid.SetColumn(label, 0);
      Grid.SetRow(label, 1);
      grid.Children.Add(label);

      label = new Label();
      label.Content = label3;
      label.Style = (Style)Application.Current.Resources["LabelSmallStyleWhite"];
      label.HorizontalAlignment = HorizontalAlignment.Right;
      label.Margin = new Thickness(0, 0, 20, 0);
      Grid.SetColumn(label, 1);
      Grid.SetRow(label, 1);
      grid.Children.Add(label);
      grid.Loaded += new RoutedEventHandler(grid_Loaded);
      button.Content = grid;
      
      _subItems.Add(button);
    }

    void grid_Loaded(object sender, RoutedEventArgs e)
    {
      Grid g = sender as Grid;
      if (g == null) return;
      g.Width = ((Button)(g.Parent)).ActualWidth;
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="DialogMenuItem"/> class.
    /// </summary>
    /// <param name="buttonName">Name of the button.</param>
    public DialogMenuItem(string buttonName)
    {
      Button b = new Button();
      b.Content = buttonName;
      b.Template = (ControlTemplate)Application.Current.Resources["MpButton"];
      b.Height = 32;
      _subItems.Add(b);
    }

    /// <summary>
    /// Gets or sets the sub items.
    /// </summary>
    /// <value>The sub items.</value>
    public List<UIElement> SubItems
    {
      get
      {
        return _subItems;
      }
      set
      {
        _subItems = value;
      }
    }
  }
}
