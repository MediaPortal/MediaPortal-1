using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Text;
using MediaPortal.UserInterface.Controls;

namespace MediaPortal.Configuration.Sections
{
	public class Volume : MediaPortal.Configuration.SectionSettings
	{
		#region Constructors

		public Volume() : base("Volume")
		{
			InitializeComponent();
		}

		#endregion Constructors

		#region Methods

		protected override void Dispose(bool disposing)
		{
			if(disposing && components != null)
				components.Dispose();

			base.Dispose(disposing);
		}

		public override void LoadSettings()
		{
			// default default
			_useClassicHandler.Checked = true;

			using(MediaPortal.Profile.Xml reader = new MediaPortal.Profile.Xml("MediaPortal.xml"))
			{
				int volumeStyle = reader.GetValueAsInt("volume", "handler", 0);

				_useClassicHandler.Checked = volumeStyle == 0;
				_useWindowsHandler.Checked = volumeStyle == 1;
				_useLogarithmicHandler.Checked = volumeStyle == 2;
				_useCustomHandler.Checked = volumeStyle == 3;
				_customText = reader.GetValueAsString("volume", "table", string.Empty);
			}

			if(_customText == string.Empty)
				_customText = "0, 1039, 1234, 1467, 1744, 2072, 2463,  2927,  3479,  4135,  4914,  5841, 6942,  8250,  9806, 11654, 13851, 16462, 19565, 23253, 27636, 32845, 39037, 46395, 55141, 65535";

			_customTextbox.Enabled = _useCustomHandler.Checked;
			_customTextbox.Text = _customTextbox.Enabled ? _customText : string.Empty;
		}

		public override void SaveSettings()
		{
			using(MediaPortal.Profile.Xml writer = new MediaPortal.Profile.Xml("MediaPortal.xml"))
			{
				if(_useClassicHandler.Checked)
					writer.SetValue("volume", "handler", 0);
				else if(_useWindowsHandler.Checked)
					writer.SetValue("volume", "handler", 1);
				else if(_useLogarithmicHandler.Checked)
					writer.SetValue("volume", "handler", 2);
				else if(_useCustomHandler.Checked)
					writer.SetValue("volume", "handler", 3);

				writer.SetValue("volume", "table", _customText);
			}
		}

		private void OnCheckChanged(object sender, System.EventArgs e)
		{
			_customTextbox.Enabled = sender == _useCustomHandler;
			_customTextbox.Text = _customTextbox.Enabled ? _customText : string.Empty;
		}

		void OnValidateCustomTable(object sender, System.ComponentModel.CancelEventArgs e)
		{
			try
			{
				StringBuilder builder = new StringBuilder();

				ArrayList valueArray = new ArrayList();				

				// for now we're happy so long as the token can be converted to integer
				foreach(string token in ((TextBox)sender).Text.Split(new char[] { ',', ';', ' ' }))
				{
					if(token == string.Empty)
						continue;

					valueArray.Add(Math.Max(0, Math.Min(65535, Convert.ToInt32(token))));
				}

				valueArray.Sort();

				// rebuild a fully formatted string to represent the volume table
				foreach(int volume in valueArray)
				{
					if(builder.Length != 0)
						builder.Append(", ");

					builder.Append(volume.ToString());
				}

				_customTextbox.Text = builder.ToString();
				_customText = _customTextbox.Text;
			}
			catch(Exception ex)
			{
				if((e.Cancel = ex is FormatException || ex is OverflowException) == false)
					throw;
			}
		}

		#endregion Methods

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.groupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox();
			this._customTextbox = new System.Windows.Forms.TextBox();
			this._useCustomHandler = new System.Windows.Forms.RadioButton();
			this._useLogarithmicHandler = new System.Windows.Forms.RadioButton();
			this._useWindowsHandler = new System.Windows.Forms.RadioButton();
			this.label1 = new System.Windows.Forms.Label();
			this._useClassicHandler = new System.Windows.Forms.RadioButton();
			this.groupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// groupBox1
			// 
			this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox1.Controls.Add(this._customTextbox);
			this.groupBox1.Controls.Add(this._useCustomHandler);
			this.groupBox1.Controls.Add(this._useLogarithmicHandler);
			this.groupBox1.Controls.Add(this._useWindowsHandler);
			this.groupBox1.Controls.Add(this.label1);
			this.groupBox1.Controls.Add(this._useClassicHandler);
			this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.groupBox1.Location = new System.Drawing.Point(8, 8);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(440, 240);
			this.groupBox1.TabIndex = 0;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Volume";
			// 
			// _customTextbox
			// 
			this._customTextbox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this._customTextbox.Enabled = false;
			this._customTextbox.Location = new System.Drawing.Point(40, 168);
			this._customTextbox.Name = "_customTextbox";
			this._customTextbox.Size = new System.Drawing.Size(384, 20);
			this._customTextbox.TabIndex = 5;
			this._customTextbox.Text = "";
			this._customTextbox.Validating += new System.ComponentModel.CancelEventHandler(this.OnValidateCustomTable);
			// 
			// _useCustomHandler
			// 
			this._useCustomHandler.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this._useCustomHandler.Location = new System.Drawing.Point(24, 144);
			this._useCustomHandler.Name = "_useCustomHandler";
			this._useCustomHandler.Size = new System.Drawing.Size(408, 24);
			this._useCustomHandler.TabIndex = 4;
			this._useCustomHandler.Text = "C&ustom:";
			this._useCustomHandler.CheckedChanged += new System.EventHandler(this.OnCheckChanged);
			// 
			// _useLogarithmicHandler
			// 
			this._useLogarithmicHandler.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this._useLogarithmicHandler.Location = new System.Drawing.Point(24, 120);
			this._useLogarithmicHandler.Name = "_useLogarithmicHandler";
			this._useLogarithmicHandler.Size = new System.Drawing.Size(408, 24);
			this._useLogarithmicHandler.TabIndex = 3;
			this._useLogarithmicHandler.Text = "Logarithmic";
			this._useLogarithmicHandler.CheckedChanged += new System.EventHandler(this.OnCheckChanged);
			// 
			// _useWindowsHandler
			// 
			this._useWindowsHandler.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this._useWindowsHandler.Location = new System.Drawing.Point(24, 72);
			this._useWindowsHandler.Name = "_useWindowsHandler";
			this._useWindowsHandler.Size = new System.Drawing.Size(408, 24);
			this._useWindowsHandler.TabIndex = 2;
			this._useWindowsHandler.Text = "Windows default";
			this._useWindowsHandler.CheckedChanged += new System.EventHandler(this.OnCheckChanged);
			// 
			// label1
			// 
			this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.label1.Location = new System.Drawing.Point(16, 32);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(416, 32);
			this.label1.TabIndex = 1;
			this.label1.Text = "Select how you would like the volume increments and decrements to be handled from" +
				" the following options:";
			// 
			// _useClassicHandler
			// 
			this._useClassicHandler.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this._useClassicHandler.Location = new System.Drawing.Point(24, 96);
			this._useClassicHandler.Name = "_useClassicHandler";
			this._useClassicHandler.Size = new System.Drawing.Size(408, 24);
			this._useClassicHandler.TabIndex = 0;
			this._useClassicHandler.Text = "&Classic";
			this._useClassicHandler.CheckedChanged += new System.EventHandler(this.OnCheckChanged);
			// 
			// Volume
			// 
			this.Controls.Add(this.groupBox1);
			this.Name = "Volume";
			this.Size = new System.Drawing.Size(456, 448);
			this.groupBox1.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		#region Fields

		Container					components = null;
		MPGroupBox					groupBox1;
		RadioButton					_useClassicHandler;
		Label						label1;
		RadioButton					_useWindowsHandler;
		RadioButton					_useLogarithmicHandler;
		string						_customText = string.Empty;
		TextBox						_customTextbox;
		RadioButton					_useCustomHandler;

		#endregion Fields
	}
}
