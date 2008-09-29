using System;
using System.Windows.Forms;
using System.Drawing;
using System.Reflection;
using System.IO;
using Windows;

namespace FullscreenMsg
{
	/// <summary>
	/// Main form for the SingleInstanceApp demo application.
	/// </summary>
  public class MainForm : System.Windows.Forms.Form
  {
    private System.ComponentModel.IContainer components;
    bool _bFullScreenMode;
    frmFullScreen FullScreenForm;
    private RichTextBox txtArgs;

    public string[] Args;

    #region Creation and Disposal

    public MainForm()
    {
      //
      // Required for Windows Form Designer support
      //
      InitializeComponent();

      // Add any constructor code after InitializeComponent call            
    }

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        if (components != null)
        {
          components.Dispose();
        }
      }
      base.Dispose(disposing);
    }

    #endregion

    #region Windows Form Designer generated code
    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
      this.txtArgs = new System.Windows.Forms.RichTextBox();
      this.SuspendLayout();
      // 
      // txtArgs
      // 
      this.txtArgs.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.txtArgs.Location = new System.Drawing.Point(12, 12);
      this.txtArgs.Name = "txtArgs";
      this.txtArgs.ReadOnly = true;
      this.txtArgs.Size = new System.Drawing.Size(847, 554);
      this.txtArgs.TabIndex = 0;
      this.txtArgs.Text = "";
      this.txtArgs.WordWrap = false;
      // 
      // MainForm
      // 
      this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
      this.ClientSize = new System.Drawing.Size(871, 578);
      this.Controls.Add(this.txtArgs);
      this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
      this.Name = "MainForm";
      this.ShowInTaskbar = false;
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
      this.Text = "FullScreenMainForm";
      this.WindowState = System.Windows.Forms.FormWindowState.Minimized;
      this.Load += new System.EventHandler(this.MainForm_Load);
      this.ResumeLayout(false);

    }
    #endregion

    private void MainForm_Load(object sender, System.EventArgs e)
    {
      // The single-instance code is going to save the command line 
      // arguments in this member variable before opening the first instance
      // of the app.
      if (this.Args != null)
      {
          ProcessParameters(null, this.Args);
          this.Args = null;
      }

    }

    public delegate void ProcessParametersDelegate(object sender, string[] args);
    public void ProcessParameters(object sender, string[] args)
    {
      // The form has loaded, and initialization will have been be done.

      // Add the command-line arguments to our textbox, just to confirm that
      // it reached here.

      lock (args)
      {
        if (args != null && args.Length != 0)
        {
          txtArgs.Text += DateTime.Now.ToString("mm:ss.ff") + " ";

          for (int i = 0; i < args.Length; i++)
          {
            txtArgs.Text += args[i] + " ";
          }
          txtArgs.Text += "\r\n";

          if (FullScreenForm == null) // create the fullscreen form instance if she does not exist
          {
            FullScreenForm = new frmFullScreen();
            this.Owner = FullScreenForm;
            this.FormBorderStyle = FormBorderStyle.None;
            this.Left = (Screen.PrimaryScreen.Bounds.Width / 2 - this.Width / 2) + 1;
            this.Top = (Screen.PrimaryScreen.Bounds.Height / 2 - this.Height / 2) + 1;
            FullScreenForm.lblMainLable.Text = "";
            FullScreenForm.lblMainLable.Parent = FullScreenForm.pbBackground;
            FullScreenForm.MainFormObj = this;

            Cursor.Position = new System.Drawing.Point(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);

            _bFullScreenMode = true;
          }

          lock (FullScreenForm)
          {
            cmdArgs tmpArgs = new cmdArgs(args);

            if (tmpArgs.ArgExists("Text")) FullScreenForm.lblMainLable.Text = tmpArgs.Values[tmpArgs.FindArgPos("Text")];
            if (tmpArgs.ArgExists("TextColor")) FullScreenForm.lblMainLable.ForeColor = System.Drawing.ColorTranslator.FromHtml(tmpArgs.Values[tmpArgs.FindArgPos("TextColor")]);
            if (tmpArgs.ArgExists("TextSize")) FullScreenForm.lblMainLable.Font = new Font(FullScreenForm.lblMainLable.Font.FontFamily, float.Parse(tmpArgs.Values[tmpArgs.FindArgPos("TextSize")]), FullScreenForm.lblMainLable.Font.Style);
            if (tmpArgs.ArgExists("BgImage"))
            {
              FullScreenForm.pbBackground.Image = new Bitmap(tmpArgs.Values[tmpArgs.FindArgPos("BgImage")]);
              if (!FullScreenForm.Visible || FullScreenForm.Opacity==0)
              {
                FullScreenForm.Show();
                FullScreenForm.Update();
                FullScreenForm.Opacity = 100;
                this.Visible = false;
              }
            }
            if (tmpArgs.ArgExists("ReadMpBackground"))
            {
              if (FullScreenForm.RetrieveSplashBackground())
              {
                FullScreenForm.Show();
                FullScreenForm.Update();
                FullScreenForm.Opacity = 100;
                this.Visible = false;
              }
            }
            if (tmpArgs.ArgExists("ObservateMpStartup")) bool.TryParse(tmpArgs.Values[tmpArgs.FindArgPos("ObservateMpStartup")], out FullScreenForm.OberservateMPStartup);
            if (tmpArgs.ArgExists("ForceForeground")) bool.TryParse(tmpArgs.Values[tmpArgs.FindArgPos("ForceForeground")], out FullScreenForm.ForceForeground);
            if (tmpArgs.ArgExists("CloseOnWindowName")) FullScreenForm.CloseOnWindowName = tmpArgs.Values[tmpArgs.FindArgPos("CloseOnWindowName")];
            if (tmpArgs.ArgExists("CloseOnForegroundWindowName")) FullScreenForm.CloseOnForegroundWindowName = tmpArgs.Values[tmpArgs.FindArgPos("CloseOnForegroundWindowName")];
            if (tmpArgs.ArgExists("CloseTimeOut")) int.TryParse(tmpArgs.Values[tmpArgs.FindArgPos("CloseTimeOut")], out FullScreenForm.CloseTimeOut);

            if (tmpArgs.ArgExists("Close")) FullScreenForm.Close();
          }
        }
        else
        {
          Stream stream = this.GetType().Assembly.GetManifestResourceStream("FullScreenMsg.help.rtf");
          txtArgs.LoadFile(stream, RichTextBoxStreamType.RichText);

          this.ShowInTaskbar = true;
          this.WindowState = FormWindowState.Normal;
          this.Show();
        }
      }
    }
  }
}
