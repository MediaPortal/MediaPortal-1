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
using System.Windows.Shapes;

namespace Dialogs
{
  /// <summary>
  /// Interaction logic for MpDialogYesNo.xaml
  /// </summary>

  public partial class MpDialogYesNo : System.Windows.Window
  {
    string _title;
    string _header;
    string _content;
    DialogResult _result = DialogResult.No;

    public MpDialogYesNo()
    {
      this.WindowStyle = WindowStyle.None;
      this.ShowInTaskbar = false;
      this.ResizeMode = ResizeMode.NoResize;
      this.AllowsTransparency = true;
      InitializeComponent();
    }
    public DialogResult DialogResult
    {
      get
      {
        return _result;
      }
    }
    /// <summary>
    /// Shows this instance.
    /// </summary>

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
      _result = DialogResult.No;
      labelHeader.Content = Header;
      labelTitle.Content = Title;
      textBox.Text = Content;
      textBox.TextWrapping = TextWrapping.WrapWithOverflow;
      Keyboard.Focus(buttonYes);
    }
    void subItemMouseEnter(object sender, MouseEventArgs e)
    {
      IInputElement b = sender as IInputElement;
      if (b != null)
      {
        Keyboard.Focus(b);
      }
    }
    void OnYesClicked(object sender, EventArgs args)
    {
      _result = DialogResult.Yes;
      this.Close();
    }
    void OnNoClicked(object sender, EventArgs args)
    {
      this.Close();
    }
    void OnCloseClicked(object sender, EventArgs args)
    {
      this.Close();
    }

    /// <summary>
    /// Gets or sets the title.
    /// </summary>
    /// <value>The title.</value>
    public string Title
    {
      get
      {
        return _title;
      }
      set
      {
        _title = value;
      }
    }
    /// <summary>
    /// Gets or sets the header.
    /// </summary>
    /// <value>The header.</value>
    public string Header
    {
      get
      {
        return _header;
      }
      set
      {
        _header = value;
      }
    }
    /// <summary>
    /// Gets or sets the content.
    /// </summary>
    /// <value>The content.</value>
    public string Content
    {
      get
      {
        return _content;
      }
      set
      {
        _content = value;
      }
    }
  }
}