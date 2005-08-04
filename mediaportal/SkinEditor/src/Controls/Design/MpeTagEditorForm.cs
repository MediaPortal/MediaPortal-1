using System;
using System.IO;
using System.Drawing;
using System.Drawing.Design;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace Mpe.Controls.Design
{
	#region MpeTagEditorForm
	/// <summary>
	///
	/// </summary>
	public class MpeTagEditorForm : System.Windows.Forms.UserControl {
		
		#region Variables
		private System.ComponentModel.Container components = null;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TextBox tbName;
		private System.Windows.Forms.TextBox tbValue;
		private System.Windows.Forms.Button okButton;
		private System.Windows.Forms.Button cancelButton;
		private System.Windows.Forms.Button deleteButton;
		private IWindowsFormsEditorService editorService;
		private DialogResult result;
		#endregion
			
		#region Constructors
		public MpeTagEditorForm(MpeTag tag, IWindowsFormsEditorService service) {
			InitializeComponent();
			editorService = service;
			tbName.Text = tag.Name;
			tbValue.Text = tag.Value;
			result = DialogResult.Cancel;
		}
		#endregion

		#region Properties
		public string TagName {
			get {
				return tbName.Text;
			}
		}
		public string TagValue {
			get {
				return tbValue.Text;
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
		private void OnOkClick(object sender, System.EventArgs e) {
			result = DialogResult.OK;
			Close();
		}
		private void OnCancelClick(object sender, System.EventArgs e) {
			result = DialogResult.Cancel;
			Close();
		}
		private void OnDeleteClick(object sender, System.EventArgs e) {
			result = DialogResult.Abort;
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
			this.okButton = new System.Windows.Forms.Button();
			this.label1 = new System.Windows.Forms.Label();
			this.tbName = new System.Windows.Forms.TextBox();
			this.tbValue = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.cancelButton = new System.Windows.Forms.Button();
			this.deleteButton = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// okButton
			// 
			this.okButton.Location = new System.Drawing.Point(48, 56);
			this.okButton.Name = "okButton";
			this.okButton.TabIndex = 4;
			this.okButton.Text = "Ok";
			this.okButton.Click += new System.EventHandler(this.OnOkClick);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(8, 8);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(34, 16);
			this.label1.TabIndex = 0;
			this.label1.Text = "Name";
			// 
			// tbName
			// 
			this.tbName.Location = new System.Drawing.Point(48, 5);
			this.tbName.Name = "tbName";
			this.tbName.Size = new System.Drawing.Size(240, 20);
			this.tbName.TabIndex = 1;
			this.tbName.Text = "Name";
			// 
			// tbValue
			// 
			this.tbValue.Location = new System.Drawing.Point(48, 29);
			this.tbValue.Name = "tbValue";
			this.tbValue.Size = new System.Drawing.Size(240, 20);
			this.tbValue.TabIndex = 3;
			this.tbValue.Text = "Value";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(8, 32);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(33, 16);
			this.label2.TabIndex = 2;
			this.label2.Text = "Value";
			// 
			// cancelButton
			// 
			this.cancelButton.Location = new System.Drawing.Point(128, 56);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.TabIndex = 5;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.Click += new System.EventHandler(this.OnCancelClick);
			// 
			// deleteButton
			// 
			this.deleteButton.Location = new System.Drawing.Point(208, 56);
			this.deleteButton.Name = "deleteButton";
			this.deleteButton.TabIndex = 6;
			this.deleteButton.Text = "Delete";
			this.deleteButton.Click += new System.EventHandler(this.OnDeleteClick);
			// 
			// MpeTagEditorForm
			// 
			this.Controls.Add(this.deleteButton);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.tbValue);
			this.Controls.Add(this.tbName);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.okButton);
			this.Name = "MpeTagEditorForm";
			this.Size = new System.Drawing.Size(296, 88);
			this.ResumeLayout(false);
			this.BackColor = System.Drawing.SystemColors.Control;
		}
		#endregion

	}
	#endregion

	#region MpeTagEditor
	public class MpeTagEditor : UITypeEditor {
		public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context) {
			return UITypeEditorEditStyle.DropDown;
		}
		public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value) {
			if (context.Instance is MpeTagCollection) {
				MpeTagCollection tags = (MpeTagCollection)context.Instance;
				MpeTag tag = tags[context.PropertyDescriptor.DisplayName];
				if (tag != null) {
					IWindowsFormsEditorService editorService = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
					MpeTagEditorForm form = new MpeTagEditorForm(tag, editorService);
					editorService.DropDownControl(form);
					switch (form.Result) {
						case DialogResult.OK:
							if (tag.Name == form.TagName && tag.Name != "-") {
								tag.Value = form.TagValue;
								MpeLog.Info("Tag Updated! Name = " + tag.Name + " Value = " + tag.Value);
							} else if (tag.Name == form.TagName) {
								MpeLog.Warn("Invalid tag name specified");
							} else {
								tags.Remove(tag.Name);
								tag.Name = form.TagName;
								tag.Value = form.TagValue;
								tags.Add(tag);
								MpeLog.Info("Tag Updated! Name = [" + tag.Name + "] Value = [" + tag.Value + "]");
							}
							break;
						case DialogResult.Abort:
							tags.Remove(tag.Name);
							MpeLog.Info("Tag Removed! Name = [" + tag.Name + "[ Value = [" + tag.Value + "]");
							break;
					}
					return tag;
				}
			}
			return base.EditValue (context, provider, value);
		}
	}
	#endregion
	
}
