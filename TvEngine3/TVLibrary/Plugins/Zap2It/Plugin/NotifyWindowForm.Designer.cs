namespace ProcessPlugins.EpgGrabber
{
   partial class NotifyWindowForm
   {
      /// <summary>
      /// Required designer variable.
      /// </summary>
      private System.ComponentModel.IContainer components = null;

      ///// <summary>
      ///// Clean up any resources being used.
      ///// </summary>
      ///// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
      //protected override void Dispose(bool disposing)
      //{
      //   if (disposing && (components != null))
      //   {
      //      components.Dispose();
      //   }
      //   base.Dispose(disposing);
      //}

      #region Windows Form Designer generated code

      /// <summary>
      /// Required method for Designer support - do not modify
      /// the contents of this method with the code editor.
      /// </summary>
      private void InitializeComponent()
      {
         this.labelText = new System.Windows.Forms.Label();
         this.buttonOK = new System.Windows.Forms.Button();
         this.SuspendLayout();
         // 
         // labelText
         // 
         this.labelText.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
         this.labelText.Location = new System.Drawing.Point(9, 9);
         this.labelText.Name = "labelText";
         this.labelText.Size = new System.Drawing.Size(276, 144);
         this.labelText.TabIndex = 0;
         // 
         // buttonOK
         // 
         this.buttonOK.Location = new System.Drawing.Point(95, 166);
         this.buttonOK.Name = "buttonOK";
         this.buttonOK.Size = new System.Drawing.Size(100, 30);
         this.buttonOK.TabIndex = 1;
         this.buttonOK.Text = "OK";
         this.buttonOK.UseVisualStyleBackColor = true;
         this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
         // 
         // NotifyWindowForm
         // 
         this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.ClientSize = new System.Drawing.Size(292, 205);
         this.ControlBox = false;
         this.Controls.Add(this.buttonOK);
         this.Controls.Add(this.labelText);
         this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
         this.Name = "NotifyWindowForm";
         this.Text = "Notification";
         this.Load += new System.EventHandler(this.NotifyWindowForm_Load);
         this.ResumeLayout(false);

      }

      #endregion

      private System.Windows.Forms.Label labelText;
      private System.Windows.Forms.Button buttonOK;
   }
}