/* 
 *	Copyright (C) 2005-2006 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */

using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System;

namespace ProcessPlugins.ExternalDisplay
{
  /// <summary>
  /// This class simulates an external display, by showing a Form to the user that shows the
  /// messages that otherwise would be displayed on the display
  /// </summary>
  /// <author>JoeDalton</author>
  public class DebugForm : Form, IDisplay
  {
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private Container components = null;
    private TextBox[] textLines = null;
    private delegate void SetLineDelegate(int _line, string _message);
    private bool isDisabled = false;
    private string errorMessage = "";

    public DebugForm()
    {
      try
      {//
        // Required for Windows Form Designer support
        //
        InitializeComponent();
        SuspendLayout();
        //Dynamically create textboxes for the number of configured display lines
        textLines = new TextBox[Settings.Instance.TextHeight];
        for (int i = 0; i < Settings.Instance.TextHeight; i++)
        {
          TextBox line = new TextBox();
          line.Anchor = (AnchorStyles.Top | AnchorStyles.Left) | AnchorStyles.Right;
          line.Enabled = false;
          line.Location = new Point(8, 8 + (24 * i));
          line.Name = "txtLine" + i;
          line.Size = new Size(320, 20);
          line.TabIndex = 0;
          line.Text = "";
          textLines[i] = line;
          Controls.Add(line);
        }
        Height = Settings.Instance.TextHeight * 24 + 50;
        ResumeLayout(false);
      }
      catch (Exception ex)
      {
        isDisabled = true;
        errorMessage = ex.Message;
      }
    }

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    protected override void Dispose( bool disposing )
    {
      if( disposing )
      {
        if(components != null)
        {
          components.Dispose();
          Hide();
        }
      }
      base.Dispose( disposing );
    }

    #region Windows Form Designer generated code
    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.SuspendLayout();
      // 
      // DebugForm
      // 
      this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
      this.ClientSize = new System.Drawing.Size(336, 70);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
      this.Name = "DebugForm";
      this.Text = "MediaPortal Status";
      this.TopMost = true;
      this.ResumeLayout(false);
    }

    #endregion

    #region IDisplay Members

    public bool IsDisabled
    {
      get { return isDisabled; }
    }

    public string ErrorMessage
    {
      get { return errorMessage; }
    }

    /// <summary>
    /// Start the display
    /// </summary>
    public void Start()
    {
      if (InvokeRequired)
      {
        Invoke(new MethodInvoker(Start));
        return;
      }
      Show();
    }

    /// <summary>
    /// Stop the display
    /// </summary>
    public void Stop()
    {
      if (InvokeRequired)
      {
        Invoke(new MethodInvoker(Stop));
        return;
      }
      Close();
    }

    /// <summary>
    /// Displays the text on the indicated line
    /// </summary>
    /// <param name="_line">The line to display the text on</param>
    /// <param name="_message">The text to display</param>
    public void SetLine(int _line, string _message)
    {
      if (InvokeRequired)
      {
        Invoke(new SetLineDelegate(SetLine), _line, _message);
        return;
      }
      textLines[_line].Text = _message;
      Update(); //Give this form the time to repaint itself...
    }

    #endregion

    /// <summary>
    /// Show the advanced configuration screen
    /// </summary>
    public void Configure()
    {
      //No advanced configuration screen
    }

    /// <summary>
    /// Initialize the display
    /// </summary>
    /// <param name="_port">ignored</param>
    /// <param name="_lines">ignored</param>
    /// <param name="_cols">ignored</param>
    /// <param name="_time">ignored</param>
    /// <param name="_linesG">ignored</param>
    /// <param name="_colsG">ignored</param>
    /// <param name="_timeG">ignored</param>
    /// <param name="_backLight">ignored</param>
    /// <param name="_contrast">ignored</param>
    public void Initialize(string _port, int _lines, int _cols, int _time, int _linesG, int _colsG, int _timeG, bool _backLight, int _contrast)
    {
      if (InvokeRequired)
        Invoke(new MethodInvoker(Show));
      else
        Show();
      Clear();
    }

    public void Clear()
    {
      if (InvokeRequired)
      {
        Invoke(new MethodInvoker(Clear));
        return;
      }
      for(int i=0; i<Settings.Instance.TextHeight; i++)
        textLines[i].Text=new string(' ',Settings.Instance.TextWidth);
    }

    /// <summary>
    /// Does this display support text mode?
    /// </summary>
    public bool SupportsText
    {
      get { return true; }
    }

    /// <summary>
    /// Does this display support graphic mode?
    /// </summary>
    public bool SupportsGraphics
    {
      get { return false; }
    }

    /// <summary>
    /// Short name for display
    /// </summary>
    /// <remarks>
    /// Explicit declaration because of the Form.Name property
    /// </remarks>
    string IDisplay.Name
    {
      get { return "DebugForm"; }
    }

    /// <summary>
    /// Description for display
    /// </summary>
    public string Description
    {
      get { return "Debug Form V1.0"; }
    }
  }
}
