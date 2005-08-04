using System;
using System.IO;
using System.Drawing;
using System.Drawing.Design;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Windows.Forms.Design;

using Mpe.Controls;
using Mpe.Controls.Properties;

namespace Mpe.Controls.Design
{
	#region MpeTextAreaEditorForm
	public class MpeTextAreaEditorForm : System.Windows.Forms.UserControl {
		
		#region Variables
		private System.ComponentModel.Container components = null;
		private System.Windows.Forms.TextBox textbox;
		private System.Windows.Forms.Button okButton;
		private System.Windows.Forms.Button cancelButton;
		private IWindowsFormsEditorService editorService = null;
		private DialogResult result;
		#endregion
			
		#region Constructors
		public MpeTextAreaEditorForm(string textValue, IWindowsFormsEditorService editorService) {
			InitializeComponent();
			EditorService = editorService;
			TextValue = textValue;
		}
		#endregion

		#region Properties
		public String TextValue {
			get {
				return textbox.Text;
			}
			set {
				textbox.Text = value != null ? value : "";
			}
		}
		public IWindowsFormsEditorService EditorService {
			get {
				return editorService;
			}
			set {
				editorService = value;
			}
		}
		public DialogResult Result {
			get {
				return result;
			}
		}
		#endregion

		#region Methods
		public void Close() {
			if (editorService != null)
				editorService.CloseDropDown();
		}
		#endregion
		
		#region Event Handlers
		private void okButton_Click(object sender, System.EventArgs e) {
			result = DialogResult.OK;
			Close();
		}
		private void cancelButton_Click(object sender, System.EventArgs e) {
			result = DialogResult.Cancel;
			Close();
		}
		#endregion

		#region Windows Form Designer Generated Code
		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing ) {
			if( disposing ) {
				if(components != null) {
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.textbox = new System.Windows.Forms.TextBox();
			this.okButton = new System.Windows.Forms.Button();
			this.cancelButton = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// textbox
			// 
			this.textbox.AcceptsReturn = true;
			this.textbox.Location = new System.Drawing.Point(8, 8);
			this.textbox.Multiline = true;
			this.textbox.Name = "textbox";
			this.textbox.Size = new System.Drawing.Size(320, 88);
			this.textbox.TabIndex = 0;
			this.textbox.Text = "";
			// 
			// okButton
			// 
			this.okButton.Location = new System.Drawing.Point(168, 104);
			this.okButton.Name = "okButton";
			this.okButton.TabIndex = 1;
			this.okButton.Text = "OK";
			this.okButton.Click += new System.EventHandler(this.okButton_Click);
			// 
			// cancelButton
			// 
			this.cancelButton.Location = new System.Drawing.Point(248, 104);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.TabIndex = 2;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
			// 
			// MpeTextAreaEditorForm
			// 
			this.BackColor = System.Drawing.SystemColors.Control;
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.okButton);
			this.Controls.Add(this.textbox);
			this.Name = "MpeTextAreaEditorForm";
			this.Size = new System.Drawing.Size(336, 136);
			this.ResumeLayout(false);

		}
		#endregion
	}
	#endregion

	#region MpeTextAreaEditor
	public class MpeTextAreaEditor : UITypeEditor {
		public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context) {
			return UITypeEditorEditStyle.DropDown;
		}
		public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value) {
			if (context.Instance is MpeControl) {
				try {
					MpeControl mpc = (MpeControl)context.Instance;
					IWindowsFormsEditorService editorService = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
					MpeTextAreaEditorForm selector = new MpeTextAreaEditorForm((string)value, editorService);
					editorService.DropDownControl(selector);
					if (selector.Result == DialogResult.Cancel) {
						return value;
					}
					return selector.TextValue;
				} catch (Exception ee) {
					MpeLog.Debug(ee);
					MpeLog.Error(ee);
				}
			}
			return base.EditValue (context, provider, value);
		}
	}
	#endregion	
}
