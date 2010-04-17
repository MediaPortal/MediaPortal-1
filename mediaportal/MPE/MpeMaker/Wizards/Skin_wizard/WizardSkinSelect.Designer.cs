namespace MpeMaker.Wizards.Skin_wizard
{
    partial class WizardSkinSelect
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.txt_skinpath = new System.Windows.Forms.TextBox();
            this.btn_browse_skin = new System.Windows.Forms.Button();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.label1 = new System.Windows.Forms.Label();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.btn_browse_font1 = new System.Windows.Forms.Button();
            this.btn_browse_font2 = new System.Windows.Forms.Button();
            this.btn_browse_font3 = new System.Windows.Forms.Button();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.txt_author = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.txt_version4 = new System.Windows.Forms.TextBox();
            this.txt_version3 = new System.Windows.Forms.TextBox();
            this.txt_version2 = new System.Windows.Forms.TextBox();
            this.txt_version1 = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.txt_name = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.btn_next1 = new System.Windows.Forms.Button();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.btn_next2 = new System.Windows.Forms.Button();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.txt_font3 = new System.Windows.Forms.TextBox();
            this.txt_font2 = new System.Windows.Forms.TextBox();
            this.btn_next3 = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.txt_font1 = new System.Windows.Forms.TextBox();
            this.tabPage4 = new System.Windows.Forms.TabPage();
            this.btn_next4 = new System.Windows.Forms.Button();
            this.tabPage5 = new System.Windows.Forms.TabPage();
            this.btn_done = new System.Windows.Forms.Button();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.label8 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.btn_brows_dll3 = new System.Windows.Forms.Button();
            this.txt_plugin_exe = new System.Windows.Forms.TextBox();
            this.btn_brows_dll2 = new System.Windows.Forms.Button();
            this.txt_plugin_window = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.btn_brows_dll1 = new System.Windows.Forms.Button();
            this.txt_plugin_procces = new System.Windows.Forms.TextBox();
            this.radioButton1 = new System.Windows.Forms.RadioButton();
            this.radioButton2 = new System.Windows.Forms.RadioButton();
            this.radioButton3 = new System.Windows.Forms.RadioButton();
            this.label11 = new System.Windows.Forms.Label();
            this.btn_browse_ico = new System.Windows.Forms.Button();
            this.txt_ico = new System.Windows.Forms.TextBox();
            this.txt_mpe_folder = new System.Windows.Forms.TextBox();
            this.btn_browse_folder = new System.Windows.Forms.Button();
            this.label12 = new System.Windows.Forms.Label();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.tabPage4.SuspendLayout();
            this.tabPage5.SuspendLayout();
            this.SuspendLayout();
            // 
            // txt_skinpath
            // 
            this.txt_skinpath.AllowDrop = true;
            this.txt_skinpath.Location = new System.Drawing.Point(6, 39);
            this.txt_skinpath.Name = "txt_skinpath";
            this.txt_skinpath.ReadOnly = true;
            this.txt_skinpath.Size = new System.Drawing.Size(436, 24);
            this.txt_skinpath.TabIndex = 5;
            this.toolTip1.SetToolTip(this.txt_skinpath, "Folder of the skin, can use drag&drop+");
            this.txt_skinpath.TextChanged += new System.EventHandler(this.txt_skinpath_TextChanged);
            this.txt_skinpath.DragDrop += new System.Windows.Forms.DragEventHandler(this.txt_skinpath_DragDrop);
            this.txt_skinpath.DragEnter += new System.Windows.Forms.DragEventHandler(this.txt_skinpath_DragEnter);
            // 
            // btn_browse_skin
            // 
            this.btn_browse_skin.Location = new System.Drawing.Point(450, 39);
            this.btn_browse_skin.Name = "btn_browse_skin";
            this.btn_browse_skin.Size = new System.Drawing.Size(75, 24);
            this.btn_browse_skin.TabIndex = 6;
            this.btn_browse_skin.Text = "Browse";
            this.toolTip1.SetToolTip(this.btn_browse_skin, "Folder of the skin, can use drag&drop+");
            this.btn_browse_skin.UseVisualStyleBackColor = true;
            this.btn_browse_skin.Click += new System.EventHandler(this.btn_browse_skin_Click);
            // 
            // folderBrowserDialog1
            // 
            this.folderBrowserDialog1.Description = "Select the skin folder ";
            this.folderBrowserDialog1.ShowNewFolderButton = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 18);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(316, 18);
            this.label1.TabIndex = 7;
            this.label1.Text = "Skin folder (Use Drag&&Drop or Browse button)";
            this.toolTip1.SetToolTip(this.label1, "Folder of the skin, can use drag&drop+");
            // 
            // btn_browse_font1
            // 
            this.btn_browse_font1.Location = new System.Drawing.Point(450, 40);
            this.btn_browse_font1.Name = "btn_browse_font1";
            this.btn_browse_font1.Size = new System.Drawing.Size(75, 26);
            this.btn_browse_font1.TabIndex = 12;
            this.btn_browse_font1.Text = "Browse";
            this.toolTip1.SetToolTip(this.btn_browse_font1, "Folder of the skin, can use drag&drop+");
            this.btn_browse_font1.UseVisualStyleBackColor = true;
            this.btn_browse_font1.Click += new System.EventHandler(this.btn_browse_font1_Click);
            // 
            // btn_browse_font2
            // 
            this.btn_browse_font2.Location = new System.Drawing.Point(450, 102);
            this.btn_browse_font2.Name = "btn_browse_font2";
            this.btn_browse_font2.Size = new System.Drawing.Size(75, 26);
            this.btn_browse_font2.TabIndex = 16;
            this.btn_browse_font2.Text = "Browse";
            this.toolTip1.SetToolTip(this.btn_browse_font2, "Folder of the skin, can use drag&drop+");
            this.btn_browse_font2.UseVisualStyleBackColor = true;
            this.btn_browse_font2.Click += new System.EventHandler(this.btn_browse_font1_Click);
            // 
            // btn_browse_font3
            // 
            this.btn_browse_font3.Location = new System.Drawing.Point(450, 165);
            this.btn_browse_font3.Name = "btn_browse_font3";
            this.btn_browse_font3.Size = new System.Drawing.Size(75, 26);
            this.btn_browse_font3.TabIndex = 18;
            this.btn_browse_font3.Text = "Browse";
            this.toolTip1.SetToolTip(this.btn_browse_font3, "Folder of the skin, can use drag&drop+");
            this.btn_browse_font3.UseVisualStyleBackColor = true;
            this.btn_browse_font3.Click += new System.EventHandler(this.btn_browse_font1_Click);
            // 
            // tabControl1
            // 
            this.tabControl1.AllowDrop = true;
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Controls.Add(this.tabPage4);
            this.tabControl1.Controls.Add(this.tabPage5);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(541, 327);
            this.tabControl1.TabIndex = 11;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.label11);
            this.tabPage1.Controls.Add(this.btn_browse_ico);
            this.tabPage1.Controls.Add(this.txt_ico);
            this.tabPage1.Controls.Add(this.txt_author);
            this.tabPage1.Controls.Add(this.label5);
            this.tabPage1.Controls.Add(this.txt_version4);
            this.tabPage1.Controls.Add(this.txt_version3);
            this.tabPage1.Controls.Add(this.txt_version2);
            this.tabPage1.Controls.Add(this.txt_version1);
            this.tabPage1.Controls.Add(this.label6);
            this.tabPage1.Controls.Add(this.txt_name);
            this.tabPage1.Controls.Add(this.label7);
            this.tabPage1.Controls.Add(this.btn_next1);
            this.tabPage1.Location = new System.Drawing.Point(4, 27);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(533, 296);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "1.Welcome";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // txt_author
            // 
            this.txt_author.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txt_author.Location = new System.Drawing.Point(144, 138);
            this.txt_author.Name = "txt_author";
            this.txt_author.Size = new System.Drawing.Size(258, 24);
            this.txt_author.TabIndex = 17;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(31, 144);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(51, 18);
            this.label5.TabIndex = 18;
            this.label5.Text = "Author";
            // 
            // txt_version4
            // 
            this.txt_version4.Location = new System.Drawing.Point(341, 92);
            this.txt_version4.Name = "txt_version4";
            this.txt_version4.Size = new System.Drawing.Size(61, 24);
            this.txt_version4.TabIndex = 16;
            // 
            // txt_version3
            // 
            this.txt_version3.Location = new System.Drawing.Point(274, 92);
            this.txt_version3.Name = "txt_version3";
            this.txt_version3.Size = new System.Drawing.Size(61, 24);
            this.txt_version3.TabIndex = 15;
            // 
            // txt_version2
            // 
            this.txt_version2.Location = new System.Drawing.Point(210, 92);
            this.txt_version2.Name = "txt_version2";
            this.txt_version2.Size = new System.Drawing.Size(61, 24);
            this.txt_version2.TabIndex = 13;
            // 
            // txt_version1
            // 
            this.txt_version1.Location = new System.Drawing.Point(146, 92);
            this.txt_version1.Name = "txt_version1";
            this.txt_version1.Size = new System.Drawing.Size(61, 24);
            this.txt_version1.TabIndex = 12;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(33, 95);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(58, 18);
            this.label6.TabIndex = 14;
            this.label6.Text = "Version";
            // 
            // txt_name
            // 
            this.txt_name.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txt_name.Location = new System.Drawing.Point(146, 46);
            this.txt_name.Name = "txt_name";
            this.txt_name.Size = new System.Drawing.Size(258, 24);
            this.txt_name.TabIndex = 10;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(26, 46);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(78, 18);
            this.label7.TabIndex = 11;
            this.label7.Text = "Skin name";
            // 
            // btn_next1
            // 
            this.btn_next1.Image = global::MpeMaker.Properties.Resources.go_next;
            this.btn_next1.Location = new System.Drawing.Point(469, 237);
            this.btn_next1.Name = "btn_next1";
            this.btn_next1.Size = new System.Drawing.Size(56, 51);
            this.btn_next1.TabIndex = 0;
            this.btn_next1.UseVisualStyleBackColor = true;
            this.btn_next1.Click += new System.EventHandler(this.btn_next1_Click);
            // 
            // tabPage2
            // 
            this.tabPage2.AllowDrop = true;
            this.tabPage2.Controls.Add(this.btn_next2);
            this.tabPage2.Controls.Add(this.label1);
            this.tabPage2.Controls.Add(this.txt_skinpath);
            this.tabPage2.Controls.Add(this.btn_browse_skin);
            this.tabPage2.Location = new System.Drawing.Point(4, 27);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(533, 296);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "2.Skin";
            this.tabPage2.UseVisualStyleBackColor = true;
            this.tabPage2.DragEnter += new System.Windows.Forms.DragEventHandler(this.txt_skinpath_DragEnter);
            // 
            // btn_next2
            // 
            this.btn_next2.Image = global::MpeMaker.Properties.Resources.go_next;
            this.btn_next2.Location = new System.Drawing.Point(469, 237);
            this.btn_next2.Name = "btn_next2";
            this.btn_next2.Size = new System.Drawing.Size(56, 51);
            this.btn_next2.TabIndex = 8;
            this.btn_next2.UseVisualStyleBackColor = true;
            this.btn_next2.Click += new System.EventHandler(this.btn_next1_Click);
            // 
            // tabPage3
            // 
            this.tabPage3.AllowDrop = true;
            this.tabPage3.Controls.Add(this.label4);
            this.tabPage3.Controls.Add(this.label3);
            this.tabPage3.Controls.Add(this.btn_browse_font3);
            this.tabPage3.Controls.Add(this.txt_font3);
            this.tabPage3.Controls.Add(this.btn_browse_font2);
            this.tabPage3.Controls.Add(this.txt_font2);
            this.tabPage3.Controls.Add(this.btn_next3);
            this.tabPage3.Controls.Add(this.label2);
            this.tabPage3.Controls.Add(this.btn_browse_font1);
            this.tabPage3.Controls.Add(this.txt_font1);
            this.tabPage3.Location = new System.Drawing.Point(4, 27);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Size = new System.Drawing.Size(533, 296);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "3.Font";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(8, 144);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(101, 18);
            this.label4.TabIndex = 20;
            this.label4.Text = "Font font file 3";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(8, 81);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(101, 18);
            this.label3.TabIndex = 19;
            this.label3.Text = "Font font file 2";
            // 
            // txt_font3
            // 
            this.txt_font3.AllowDrop = true;
            this.txt_font3.Location = new System.Drawing.Point(8, 165);
            this.txt_font3.Name = "txt_font3";
            this.txt_font3.ReadOnly = true;
            this.txt_font3.Size = new System.Drawing.Size(436, 24);
            this.txt_font3.TabIndex = 17;
            this.txt_font3.DragDrop += new System.Windows.Forms.DragEventHandler(this.txt_skin_folder_DragDrop);
            this.txt_font3.DragEnter += new System.Windows.Forms.DragEventHandler(this.txt_skin_folder_DragEnter);
            // 
            // txt_font2
            // 
            this.txt_font2.AllowDrop = true;
            this.txt_font2.Location = new System.Drawing.Point(8, 102);
            this.txt_font2.Name = "txt_font2";
            this.txt_font2.ReadOnly = true;
            this.txt_font2.Size = new System.Drawing.Size(436, 24);
            this.txt_font2.TabIndex = 15;
            this.txt_font2.DragDrop += new System.Windows.Forms.DragEventHandler(this.txt_skin_folder_DragDrop);
            this.txt_font2.DragEnter += new System.Windows.Forms.DragEventHandler(this.txt_skin_folder_DragEnter);
            // 
            // btn_next3
            // 
            this.btn_next3.Image = global::MpeMaker.Properties.Resources.go_next;
            this.btn_next3.Location = new System.Drawing.Point(469, 237);
            this.btn_next3.Name = "btn_next3";
            this.btn_next3.Size = new System.Drawing.Size(56, 51);
            this.btn_next3.TabIndex = 14;
            this.btn_next3.UseVisualStyleBackColor = true;
            this.btn_next3.Click += new System.EventHandler(this.btn_next1_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(8, 19);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(101, 18);
            this.label2.TabIndex = 13;
            this.label2.Text = "Font font file 1";
            // 
            // txt_font1
            // 
            this.txt_font1.AllowDrop = true;
            this.txt_font1.Location = new System.Drawing.Point(8, 40);
            this.txt_font1.Name = "txt_font1";
            this.txt_font1.ReadOnly = true;
            this.txt_font1.Size = new System.Drawing.Size(436, 24);
            this.txt_font1.TabIndex = 11;
            this.txt_font1.DragDrop += new System.Windows.Forms.DragEventHandler(this.txt_skin_folder_DragDrop);
            this.txt_font1.DragEnter += new System.Windows.Forms.DragEventHandler(this.txt_skin_folder_DragEnter);
            // 
            // tabPage4
            // 
            this.tabPage4.Controls.Add(this.radioButton3);
            this.tabPage4.Controls.Add(this.radioButton2);
            this.tabPage4.Controls.Add(this.radioButton1);
            this.tabPage4.Controls.Add(this.label8);
            this.tabPage4.Controls.Add(this.label9);
            this.tabPage4.Controls.Add(this.btn_brows_dll3);
            this.tabPage4.Controls.Add(this.txt_plugin_exe);
            this.tabPage4.Controls.Add(this.btn_brows_dll2);
            this.tabPage4.Controls.Add(this.txt_plugin_window);
            this.tabPage4.Controls.Add(this.label10);
            this.tabPage4.Controls.Add(this.btn_brows_dll1);
            this.tabPage4.Controls.Add(this.txt_plugin_procces);
            this.tabPage4.Controls.Add(this.btn_next4);
            this.tabPage4.Location = new System.Drawing.Point(4, 27);
            this.tabPage4.Name = "tabPage4";
            this.tabPage4.Size = new System.Drawing.Size(533, 296);
            this.tabPage4.TabIndex = 3;
            this.tabPage4.Text = "4.Dll\'s";
            this.tabPage4.UseVisualStyleBackColor = true;
            // 
            // btn_next4
            // 
            this.btn_next4.Image = global::MpeMaker.Properties.Resources.go_next;
            this.btn_next4.Location = new System.Drawing.Point(469, 237);
            this.btn_next4.Name = "btn_next4";
            this.btn_next4.Size = new System.Drawing.Size(56, 51);
            this.btn_next4.TabIndex = 1;
            this.btn_next4.UseVisualStyleBackColor = true;
            this.btn_next4.Click += new System.EventHandler(this.btn_next1_Click);
            // 
            // tabPage5
            // 
            this.tabPage5.Controls.Add(this.label12);
            this.tabPage5.Controls.Add(this.btn_browse_folder);
            this.tabPage5.Controls.Add(this.txt_mpe_folder);
            this.tabPage5.Controls.Add(this.btn_done);
            this.tabPage5.Location = new System.Drawing.Point(4, 27);
            this.tabPage5.Name = "tabPage5";
            this.tabPage5.Size = new System.Drawing.Size(533, 296);
            this.tabPage5.TabIndex = 4;
            this.tabPage5.Text = "5.Done";
            this.tabPage5.UseVisualStyleBackColor = true;
            // 
            // btn_done
            // 
            this.btn_done.Location = new System.Drawing.Point(450, 241);
            this.btn_done.Name = "btn_done";
            this.btn_done.Size = new System.Drawing.Size(75, 47);
            this.btn_done.TabIndex = 0;
            this.btn_done.Text = "Done";
            this.btn_done.UseVisualStyleBackColor = true;
            this.btn_done.Click += new System.EventHandler(this.btn_done_Click);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(8, 171);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(80, 18);
            this.label8.TabIndex = 29;
            this.label8.Text = "Executable";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(8, 98);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(104, 18);
            this.label9.TabIndex = 28;
            this.label9.Text = "Window plugin";
            // 
            // btn_brows_dll3
            // 
            this.btn_brows_dll3.Location = new System.Drawing.Point(450, 192);
            this.btn_brows_dll3.Name = "btn_brows_dll3";
            this.btn_brows_dll3.Size = new System.Drawing.Size(75, 26);
            this.btn_brows_dll3.TabIndex = 27;
            this.btn_brows_dll3.Text = "Browse";
            this.toolTip1.SetToolTip(this.btn_brows_dll3, "Folder of the skin, can use drag&drop+");
            this.btn_brows_dll3.UseVisualStyleBackColor = true;
            this.btn_brows_dll3.Click += new System.EventHandler(this.btn_brows_dll1_Click);
            // 
            // txt_plugin_exe
            // 
            this.txt_plugin_exe.AllowDrop = true;
            this.txt_plugin_exe.Location = new System.Drawing.Point(8, 192);
            this.txt_plugin_exe.Name = "txt_plugin_exe";
            this.txt_plugin_exe.ReadOnly = true;
            this.txt_plugin_exe.Size = new System.Drawing.Size(436, 24);
            this.txt_plugin_exe.TabIndex = 26;
            this.txt_plugin_exe.DragDrop += new System.Windows.Forms.DragEventHandler(this.txt_plugin_procces_DragDrop);
            this.txt_plugin_exe.DragEnter += new System.Windows.Forms.DragEventHandler(this.txt_skin_folder_DragEnter);
            // 
            // btn_brows_dll2
            // 
            this.btn_brows_dll2.Location = new System.Drawing.Point(450, 119);
            this.btn_brows_dll2.Name = "btn_brows_dll2";
            this.btn_brows_dll2.Size = new System.Drawing.Size(75, 26);
            this.btn_brows_dll2.TabIndex = 25;
            this.btn_brows_dll2.Text = "Browse";
            this.toolTip1.SetToolTip(this.btn_brows_dll2, "Folder of the skin, can use drag&drop+");
            this.btn_brows_dll2.UseVisualStyleBackColor = true;
            this.btn_brows_dll2.Click += new System.EventHandler(this.btn_brows_dll1_Click);
            // 
            // txt_plugin_window
            // 
            this.txt_plugin_window.AllowDrop = true;
            this.txt_plugin_window.Location = new System.Drawing.Point(8, 119);
            this.txt_plugin_window.Name = "txt_plugin_window";
            this.txt_plugin_window.ReadOnly = true;
            this.txt_plugin_window.Size = new System.Drawing.Size(436, 24);
            this.txt_plugin_window.TabIndex = 24;
            this.txt_plugin_window.DragDrop += new System.Windows.Forms.DragEventHandler(this.txt_plugin_procces_DragDrop);
            this.txt_plugin_window.DragEnter += new System.Windows.Forms.DragEventHandler(this.txt_skin_folder_DragEnter);
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(8, 26);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(110, 18);
            this.label10.TabIndex = 23;
            this.label10.Text = "Process plugin ";
            // 
            // btn_brows_dll1
            // 
            this.btn_brows_dll1.Location = new System.Drawing.Point(450, 47);
            this.btn_brows_dll1.Name = "btn_brows_dll1";
            this.btn_brows_dll1.Size = new System.Drawing.Size(75, 26);
            this.btn_brows_dll1.TabIndex = 22;
            this.btn_brows_dll1.Text = "Browse";
            this.toolTip1.SetToolTip(this.btn_brows_dll1, "Folder of the skin, can use drag&drop+");
            this.btn_brows_dll1.UseVisualStyleBackColor = true;
            this.btn_brows_dll1.Click += new System.EventHandler(this.btn_brows_dll1_Click);
            // 
            // txt_plugin_procces
            // 
            this.txt_plugin_procces.AllowDrop = true;
            this.txt_plugin_procces.Location = new System.Drawing.Point(8, 47);
            this.txt_plugin_procces.Name = "txt_plugin_procces";
            this.txt_plugin_procces.ReadOnly = true;
            this.txt_plugin_procces.Size = new System.Drawing.Size(436, 24);
            this.txt_plugin_procces.TabIndex = 21;
            this.txt_plugin_procces.DragDrop += new System.Windows.Forms.DragEventHandler(this.txt_plugin_procces_DragDrop);
            this.txt_plugin_procces.DragEnter += new System.Windows.Forms.DragEventHandler(this.txt_skin_folder_DragEnter);
            // 
            // radioButton1
            // 
            this.radioButton1.AutoSize = true;
            this.radioButton1.Location = new System.Drawing.Point(385, 19);
            this.radioButton1.Name = "radioButton1";
            this.radioButton1.Size = new System.Drawing.Size(140, 22);
            this.radioButton1.TabIndex = 30;
            this.radioButton1.TabStop = true;
            this.radioButton1.Text = "Use for configure";
            this.radioButton1.UseVisualStyleBackColor = true;
            // 
            // radioButton2
            // 
            this.radioButton2.AutoSize = true;
            this.radioButton2.Location = new System.Drawing.Point(385, 91);
            this.radioButton2.Name = "radioButton2";
            this.radioButton2.Size = new System.Drawing.Size(140, 22);
            this.radioButton2.TabIndex = 31;
            this.radioButton2.TabStop = true;
            this.radioButton2.Text = "Use for configure";
            this.radioButton2.UseVisualStyleBackColor = true;
            // 
            // radioButton3
            // 
            this.radioButton3.AutoSize = true;
            this.radioButton3.Location = new System.Drawing.Point(385, 164);
            this.radioButton3.Name = "radioButton3";
            this.radioButton3.Size = new System.Drawing.Size(140, 22);
            this.radioButton3.TabIndex = 32;
            this.radioButton3.TabStop = true;
            this.radioButton3.Text = "Use for configure";
            this.radioButton3.UseVisualStyleBackColor = true;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(8, 176);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(152, 18);
            this.label11.TabIndex = 21;
            this.label11.Text = "Icon file (bmp,jpg,png)";
            // 
            // btn_browse_ico
            // 
            this.btn_browse_ico.Location = new System.Drawing.Point(450, 197);
            this.btn_browse_ico.Name = "btn_browse_ico";
            this.btn_browse_ico.Size = new System.Drawing.Size(75, 26);
            this.btn_browse_ico.TabIndex = 20;
            this.btn_browse_ico.Text = "Browse";
            this.toolTip1.SetToolTip(this.btn_browse_ico, "Folder of the skin, can use drag&drop+");
            this.btn_browse_ico.UseVisualStyleBackColor = true;
            this.btn_browse_ico.Click += new System.EventHandler(this.btn_browse_ico_Click);
            // 
            // txt_ico
            // 
            this.txt_ico.AllowDrop = true;
            this.txt_ico.Location = new System.Drawing.Point(8, 197);
            this.txt_ico.Name = "txt_ico";
            this.txt_ico.ReadOnly = true;
            this.txt_ico.Size = new System.Drawing.Size(436, 24);
            this.txt_ico.TabIndex = 19;
            this.txt_ico.DragDrop += new System.Windows.Forms.DragEventHandler(this.txt_ico_DragDrop);
            this.txt_ico.DragEnter += new System.Windows.Forms.DragEventHandler(this.txt_skin_folder_DragEnter);
            // 
            // txt_mpe_folder
            // 
            this.txt_mpe_folder.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.AllSystemSources;
            this.txt_mpe_folder.Location = new System.Drawing.Point(52, 61);
            this.txt_mpe_folder.Name = "txt_mpe_folder";
            this.txt_mpe_folder.Size = new System.Drawing.Size(342, 24);
            this.txt_mpe_folder.TabIndex = 1;
            // 
            // btn_browse_folder
            // 
            this.btn_browse_folder.Location = new System.Drawing.Point(413, 61);
            this.btn_browse_folder.Name = "btn_browse_folder";
            this.btn_browse_folder.Size = new System.Drawing.Size(83, 24);
            this.btn_browse_folder.TabIndex = 2;
            this.btn_browse_folder.Text = "Browse";
            this.btn_browse_folder.UseVisualStyleBackColor = true;
            this.btn_browse_folder.Click += new System.EventHandler(this.btn_browse_folder_Click);
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(49, 40);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(281, 18);
            this.label12.TabIndex = 3;
            this.label12.Text = "Folder were the package should be saved";
            // 
            // WizardSkinSelect
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(541, 327);
            this.Controls.Add(this.tabControl1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "WizardSkinSelect";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "New Skin Extension Package";
            this.TopMost = true;
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            this.tabPage3.ResumeLayout(false);
            this.tabPage3.PerformLayout();
            this.tabPage4.ResumeLayout(false);
            this.tabPage4.PerformLayout();
            this.tabPage5.ResumeLayout(false);
            this.tabPage5.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TextBox txt_skinpath;
        private System.Windows.Forms.Button btn_browse_skin;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.Button btn_next1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btn_browse_font1;
        private System.Windows.Forms.TextBox txt_font1;
        private System.Windows.Forms.TabPage tabPage4;
        private System.Windows.Forms.TabPage tabPage5;
        private System.Windows.Forms.Button btn_next2;
        private System.Windows.Forms.Button btn_next3;
        private System.Windows.Forms.Button btn_next4;
        private System.Windows.Forms.Button btn_done;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button btn_browse_font3;
        private System.Windows.Forms.TextBox txt_font3;
        private System.Windows.Forms.Button btn_browse_font2;
        private System.Windows.Forms.TextBox txt_font2;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.TextBox txt_author;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txt_version4;
        private System.Windows.Forms.TextBox txt_version3;
        private System.Windows.Forms.TextBox txt_version2;
        private System.Windows.Forms.TextBox txt_version1;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox txt_name;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Button btn_brows_dll3;
        private System.Windows.Forms.TextBox txt_plugin_exe;
        private System.Windows.Forms.Button btn_brows_dll2;
        private System.Windows.Forms.TextBox txt_plugin_window;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Button btn_brows_dll1;
        private System.Windows.Forms.TextBox txt_plugin_procces;
        private System.Windows.Forms.RadioButton radioButton1;
        private System.Windows.Forms.RadioButton radioButton3;
        private System.Windows.Forms.RadioButton radioButton2;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Button btn_browse_ico;
        private System.Windows.Forms.TextBox txt_ico;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Button btn_browse_folder;
        private System.Windows.Forms.TextBox txt_mpe_folder;
    }
}