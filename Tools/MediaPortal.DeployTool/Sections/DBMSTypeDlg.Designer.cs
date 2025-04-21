#region Copyright (C) 2005-2024 Team MediaPortal

// Copyright (C) 2005-2024 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

namespace MediaPortal.DeployTool.Sections
{
  partial class DBMSTypeDlg
  {
    /// <summary> 
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary> 
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing && (components != null))
      {
        components.Dispose();
      }
      base.Dispose(disposing);
    }

    #region Component Designer generated code

    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.labelHeading = new System.Windows.Forms.Label();
      this.rbMSSQL = new System.Windows.Forms.Label();
      this.rbMySQL = new System.Windows.Forms.Label();
      this.rbMariaDB = new System.Windows.Forms.Label();
      this.rbDBAlreadyInstalled = new System.Windows.Forms.Label();
      this.bMS = new System.Windows.Forms.Button();
      this.bMySQL = new System.Windows.Forms.Button();
      this.bExists = new System.Windows.Forms.Button();
      this.pictureBox1 = new System.Windows.Forms.PictureBox();
      this.pictureBox2 = new System.Windows.Forms.PictureBox();
      this.label1 = new System.Windows.Forms.Label();
      this.pictureBox3 = new System.Windows.Forms.PictureBox();
      this.pictureBox4 = new System.Windows.Forms.PictureBox();
      this.pictureBox5 = new System.Windows.Forms.PictureBox();
      this.lbMSSQL = new System.Windows.Forms.Label();
      this.bMariaDB = new System.Windows.Forms.Button();
      this.pictureBox6 = new System.Windows.Forms.PictureBox();
      this.pictureBox7 = new System.Windows.Forms.PictureBox();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox4)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox5)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox6)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox7)).BeginInit();
      this.SuspendLayout();
      // 
      // labelHeading
      // 
      this.labelHeading.AutoSize = true;
      this.labelHeading.Font = new System.Drawing.Font("Verdana", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.labelHeading.ForeColor = System.Drawing.Color.White;
      this.labelHeading.Location = new System.Drawing.Point(330, 101);
      this.labelHeading.Name = "labelHeading";
      this.labelHeading.Size = new System.Drawing.Size(367, 17);
      this.labelHeading.TabIndex = 1;
      this.labelHeading.Text = "Please select the SQL-Server you want to use:";
      // 
      // rbMSSQL
      // 
      this.rbMSSQL.AutoSize = true;
      this.rbMSSQL.Cursor = System.Windows.Forms.Cursors.Hand;
      this.rbMSSQL.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.rbMSSQL.ForeColor = System.Drawing.Color.White;
      this.rbMSSQL.Location = new System.Drawing.Point(383, 318);
      this.rbMSSQL.Name = "rbMSSQL";
      this.rbMSSQL.Size = new System.Drawing.Size(275, 13);
      this.rbMSSQL.TabIndex = 23;
      this.rbMSSQL.Text = "Microsoft SQL-Server Express is disabled";
      this.rbMSSQL.Click += new System.EventHandler(this.bMS_Click);
      // 
      // rbMySQL
      // 
      this.rbMySQL.AutoSize = true;
      this.rbMySQL.Cursor = System.Windows.Forms.Cursors.Hand;
      this.rbMySQL.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.rbMySQL.ForeColor = System.Drawing.Color.White;
      this.rbMySQL.Location = new System.Drawing.Point(383, 264);
      this.rbMySQL.Name = "rbMySQL";
      this.rbMySQL.Size = new System.Drawing.Size(97, 13);
      this.rbMySQL.TabIndex = 24;
      this.rbMySQL.Text = "MySQL Server";
      this.rbMySQL.Click += new System.EventHandler(this.bMySQL_Click);
      // 
      // rbMariaDB
      // 
      this.rbMariaDB.AutoSize = true;
      this.rbMariaDB.Cursor = System.Windows.Forms.Cursors.Hand;
      this.rbMariaDB.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.rbMariaDB.ForeColor = System.Drawing.Color.White;
      this.rbMariaDB.Location = new System.Drawing.Point(383, 208);
      this.rbMariaDB.Name = "rbMariaDB";
      this.rbMariaDB.Size = new System.Drawing.Size(108, 13);
      this.rbMariaDB.TabIndex = 24;
      this.rbMariaDB.Text = "MariaDB Server";
      this.rbMariaDB.Click += new System.EventHandler(this.bMariaDB_Click);
      // 
      // rbDBAlreadyInstalled
      // 
      this.rbDBAlreadyInstalled.AutoSize = true;
      this.rbDBAlreadyInstalled.Cursor = System.Windows.Forms.Cursors.Hand;
      this.rbDBAlreadyInstalled.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.rbDBAlreadyInstalled.ForeColor = System.Drawing.Color.White;
      this.rbDBAlreadyInstalled.Location = new System.Drawing.Point(383, 146);
      this.rbDBAlreadyInstalled.Name = "rbDBAlreadyInstalled";
      this.rbDBAlreadyInstalled.Size = new System.Drawing.Size(403, 13);
      this.rbDBAlreadyInstalled.TabIndex = 25;
      this.rbDBAlreadyInstalled.Text = "SQL server is already present and will be used for TV-Server";
      this.rbDBAlreadyInstalled.Click += new System.EventHandler(this.bExists_Click);
      // 
      // bMS
      // 
      this.bMS.Cursor = System.Windows.Forms.Cursors.Hand;
      this.bMS.FlatAppearance.BorderSize = 0;
      this.bMS.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
      this.bMS.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
      this.bMS.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.bMS.Image = global::MediaPortal.DeployTool.Images.Choose_button_off;
      this.bMS.Location = new System.Drawing.Point(336, 315);
      this.bMS.Name = "bMS";
      this.bMS.Size = new System.Drawing.Size(32, 23);
      this.bMS.TabIndex = 26;
      this.bMS.UseVisualStyleBackColor = true;
      this.bMS.Click += new System.EventHandler(this.bMS_Click);
      // 
      // bMySQL
      // 
      this.bMySQL.Cursor = System.Windows.Forms.Cursors.Hand;
      this.bMySQL.FlatAppearance.BorderSize = 0;
      this.bMySQL.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
      this.bMySQL.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
      this.bMySQL.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.bMySQL.Image = global::MediaPortal.DeployTool.Images.Choose_button_off;
      this.bMySQL.Location = new System.Drawing.Point(336, 259);
      this.bMySQL.Name = "bMySQL";
      this.bMySQL.Size = new System.Drawing.Size(32, 23);
      this.bMySQL.TabIndex = 27;
      this.bMySQL.UseVisualStyleBackColor = true;
      this.bMySQL.Click += new System.EventHandler(this.bMySQL_Click);
      // 
      // bExists
      // 
      this.bExists.Cursor = System.Windows.Forms.Cursors.Hand;
      this.bExists.FlatAppearance.BorderSize = 0;
      this.bExists.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
      this.bExists.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
      this.bExists.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.bExists.Image = global::MediaPortal.DeployTool.Images.Choose_button_off;
      this.bExists.Location = new System.Drawing.Point(336, 147);
      this.bExists.Name = "bExists";
      this.bExists.Size = new System.Drawing.Size(32, 23);
      this.bExists.TabIndex = 28;
      this.bExists.UseVisualStyleBackColor = true;
      this.bExists.Click += new System.EventHandler(this.bExists_Click);
      // 
      // pictureBox1
      // 
      this.pictureBox1.Image = global::MediaPortal.DeployTool.Images.MSSQL;
      this.pictureBox1.Location = new System.Drawing.Point(258, 305);
      this.pictureBox1.Name = "pictureBox1";
      this.pictureBox1.Size = new System.Drawing.Size(50, 50);
      this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
      this.pictureBox1.TabIndex = 30;
      this.pictureBox1.TabStop = false;
      // 
      // pictureBox2
      // 
      this.pictureBox2.Image = global::MediaPortal.DeployTool.Images.MySQL;
      this.pictureBox2.Location = new System.Drawing.Point(258, 249);
      this.pictureBox2.Name = "pictureBox2";
      this.pictureBox2.Size = new System.Drawing.Size(50, 50);
      this.pictureBox2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
      this.pictureBox2.TabIndex = 31;
      this.pictureBox2.TabStop = false;
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
      this.label1.ForeColor = System.Drawing.Color.White;
      this.label1.Location = new System.Drawing.Point(382, 168);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(244, 13);
      this.label1.TabIndex = 32;
      this.label1.Text = "Support MariaDB, MySQL, MS SQL server";
      // 
      // pictureBox3
      // 
      this.pictureBox3.Image = global::MediaPortal.DeployTool.Images.MariaDB;
      this.pictureBox3.Location = new System.Drawing.Point(258, 137);
      this.pictureBox3.Name = "pictureBox3";
      this.pictureBox3.Size = new System.Drawing.Size(50, 50);
      this.pictureBox3.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
      this.pictureBox3.TabIndex = 33;
      this.pictureBox3.TabStop = false;
      // 
      // pictureBox4
      // 
      this.pictureBox4.Image = global::MediaPortal.DeployTool.Images.MySQL;
      this.pictureBox4.Location = new System.Drawing.Point(202, 137);
      this.pictureBox4.Name = "pictureBox4";
      this.pictureBox4.Size = new System.Drawing.Size(50, 50);
      this.pictureBox4.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
      this.pictureBox4.TabIndex = 34;
      this.pictureBox4.TabStop = false;
      // 
      // pictureBox5
      // 
      this.pictureBox5.Image = global::MediaPortal.DeployTool.Images.MSSQL;
      this.pictureBox5.Location = new System.Drawing.Point(146, 137);
      this.pictureBox5.Name = "pictureBox5";
      this.pictureBox5.Size = new System.Drawing.Size(50, 50);
      this.pictureBox5.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
      this.pictureBox5.TabIndex = 35;
      this.pictureBox5.TabStop = false;
      // 
      // lbMSSQL
      // 
      this.lbMSSQL.AutoSize = true;
      this.lbMSSQL.Cursor = System.Windows.Forms.Cursors.Hand;
      this.lbMSSQL.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
      this.lbMSSQL.ForeColor = System.Drawing.Color.White;
      this.lbMSSQL.Location = new System.Drawing.Point(383, 337);
      this.lbMSSQL.Name = "lbMSSQL";
      this.lbMSSQL.Size = new System.Drawing.Size(108, 13);
      this.lbMSSQL.TabIndex = 36;
      this.lbMSSQL.Text = "Learn why here...";
      this.lbMSSQL.Click += new System.EventHandler(this.lbMSSQL_Click);
      // 
      // bMariaDB
      // 
      this.bMariaDB.Cursor = System.Windows.Forms.Cursors.Hand;
      this.bMariaDB.FlatAppearance.BorderSize = 0;
      this.bMariaDB.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
      this.bMariaDB.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
      this.bMariaDB.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.bMariaDB.Image = global::MediaPortal.DeployTool.Images.Choose_button_off;
      this.bMariaDB.Location = new System.Drawing.Point(336, 203);
      this.bMariaDB.Name = "bMariaDB";
      this.bMariaDB.Size = new System.Drawing.Size(32, 23);
      this.bMariaDB.TabIndex = 37;
      this.bMariaDB.UseVisualStyleBackColor = true;
      this.bMariaDB.Click += new System.EventHandler(this.bMariaDB_Click);
      // 
      // pictureBox6
      // 
      this.pictureBox6.Image = global::MediaPortal.DeployTool.Images.MariaDB;
      this.pictureBox6.Location = new System.Drawing.Point(258, 193);
      this.pictureBox6.Name = "pictureBox6";
      this.pictureBox6.Size = new System.Drawing.Size(50, 50);
      this.pictureBox6.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
      this.pictureBox6.TabIndex = 38;
      this.pictureBox6.TabStop = false;
      // 
      // pictureBox7
      // 
      this.pictureBox7.Image = global::MediaPortal.DeployTool.Images.Database;
      this.pictureBox7.Location = new System.Drawing.Point(146, 193);
      this.pictureBox7.Name = "pictureBox7";
      this.pictureBox7.Size = new System.Drawing.Size(106, 162);
      this.pictureBox7.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
      this.pictureBox7.TabIndex = 39;
      this.pictureBox7.TabStop = false;
      // 
      // DBMSTypeDlg
      // 
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
      this.BackgroundImage = global::MediaPortal.DeployTool.Images.Background_middle_empty;
      this.Controls.Add(this.pictureBox6);
      this.Controls.Add(this.bMariaDB);
      this.Controls.Add(this.lbMSSQL);
      this.Controls.Add(this.pictureBox5);
      this.Controls.Add(this.pictureBox4);
      this.Controls.Add(this.pictureBox3);
      this.Controls.Add(this.label1);
      this.Controls.Add(this.pictureBox2);
      this.Controls.Add(this.pictureBox1);
      this.Controls.Add(this.bExists);
      this.Controls.Add(this.bMySQL);
      this.Controls.Add(this.bMS);
      this.Controls.Add(this.rbDBAlreadyInstalled);
      this.Controls.Add(this.rbMariaDB);
      this.Controls.Add(this.rbMySQL);
      this.Controls.Add(this.rbMSSQL);
      this.Controls.Add(this.labelHeading);
      this.Controls.Add(this.pictureBox7);
      this.Name = "DBMSTypeDlg";
      this.Controls.SetChildIndex(this.pictureBox7, 0);
      this.Controls.SetChildIndex(this.labelHeading, 0);
      this.Controls.SetChildIndex(this.labelSectionHeader, 0);
      this.Controls.SetChildIndex(this.rbMSSQL, 0);
      this.Controls.SetChildIndex(this.rbMySQL, 0);
      this.Controls.SetChildIndex(this.rbMariaDB, 0);
      this.Controls.SetChildIndex(this.rbDBAlreadyInstalled, 0);
      this.Controls.SetChildIndex(this.bMS, 0);
      this.Controls.SetChildIndex(this.bMySQL, 0);
      this.Controls.SetChildIndex(this.bExists, 0);
      this.Controls.SetChildIndex(this.pictureBox1, 0);
      this.Controls.SetChildIndex(this.pictureBox2, 0);
      this.Controls.SetChildIndex(this.label1, 0);
      this.Controls.SetChildIndex(this.pictureBox3, 0);
      this.Controls.SetChildIndex(this.pictureBox4, 0);
      this.Controls.SetChildIndex(this.pictureBox5, 0);
      this.Controls.SetChildIndex(this.lbMSSQL, 0);
      this.Controls.SetChildIndex(this.bMariaDB, 0);
      this.Controls.SetChildIndex(this.pictureBox6, 0);
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox4)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox5)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox6)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox7)).EndInit();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Label labelHeading;
    private System.Windows.Forms.Label rbMSSQL;
    private System.Windows.Forms.Label rbMySQL;
    private System.Windows.Forms.Label rbMariaDB;
    private System.Windows.Forms.Label rbDBAlreadyInstalled;
    private System.Windows.Forms.Button bMS;
    private System.Windows.Forms.Button bMySQL;
    private System.Windows.Forms.Button bExists;
    private System.Windows.Forms.PictureBox pictureBox1;
    private System.Windows.Forms.PictureBox pictureBox2;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.PictureBox pictureBox3;
    private System.Windows.Forms.PictureBox pictureBox4;
    private System.Windows.Forms.PictureBox pictureBox5;
    private System.Windows.Forms.Label lbMSSQL;
    private System.Windows.Forms.Button bMariaDB;
    private System.Windows.Forms.PictureBox pictureBox6;
    private System.Windows.Forms.PictureBox pictureBox7;
  }
}