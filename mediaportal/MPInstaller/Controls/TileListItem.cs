using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;

namespace MediaPortal.MPInstaller.Controls
{
	/// <summary>
	/// Summary description for TileListItem.
	/// </summary>
	public class TileListItem : System.Windows.Forms.UserControl
	{
		private System.Windows.Forms.PictureBox picBox;
		private System.Windows.Forms.Label lblTitle;
		private System.Windows.Forms.Label lblDescription;
		private System.Windows.Forms.Label lblCreateDate;
		public new event MouseEventHandler MouseDown=null;
		public new event MouseEventHandler MouseUp=null;
		public new event EventHandler DoubleClick=null;
		public event EventHandler ItemSelected=null;
		private ToolTip itemTip;
    private Label lblAuthor;
    private LinkLabel lblLInk;
    private Label label1;
    private Label label2;
    private Label lblVersion;
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public TileListItem()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();

			// TODO: Add any initialization after the InitializeComponent call
			InitializeMyComponent();
			this.id=Guid.NewGuid().ToString();
			this.CreateDate=DateTime.Now;
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
				}
			}
			base.Dispose( disposing );
		}

		#region Component Designer generated code
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
      this.picBox = new System.Windows.Forms.PictureBox();
      this.lblTitle = new System.Windows.Forms.Label();
      this.lblDescription = new System.Windows.Forms.Label();
      this.lblCreateDate = new System.Windows.Forms.Label();
      this.lblAuthor = new System.Windows.Forms.Label();
      this.lblLInk = new System.Windows.Forms.LinkLabel();
      this.label1 = new System.Windows.Forms.Label();
      this.label2 = new System.Windows.Forms.Label();
      this.lblVersion = new System.Windows.Forms.Label();
      ((System.ComponentModel.ISupportInitialize)(this.picBox)).BeginInit();
      this.SuspendLayout();
      // 
      // picBox
      // 
      this.picBox.Anchor = System.Windows.Forms.AnchorStyles.Left;
      this.picBox.BackColor = System.Drawing.Color.Transparent;
      this.picBox.Location = new System.Drawing.Point(3, 3);
      this.picBox.Name = "picBox";
      this.picBox.Size = new System.Drawing.Size(160, 90);
      this.picBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
      this.picBox.TabIndex = 0;
      this.picBox.TabStop = false;
      this.picBox.DoubleClick += new System.EventHandler(this.TileListItem_DoubleClick);
      this.picBox.MouseMove += new System.Windows.Forms.MouseEventHandler(this.TileListItem_MouseMove);
      this.picBox.Click += new System.EventHandler(this.TileListItem_Click);
      this.picBox.MouseDown += new System.Windows.Forms.MouseEventHandler(this.TileListItem_MouseDown);
      this.picBox.MouseUp += new System.Windows.Forms.MouseEventHandler(this.TileListItem_MouseUp);
      this.picBox.MouseEnter += new System.EventHandler(this.TileListItem_MouseEnter);
      // 
      // lblTitle
      // 
      this.lblTitle.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.lblTitle.BackColor = System.Drawing.Color.Transparent;
      this.lblTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.lblTitle.Location = new System.Drawing.Point(169, 3);
      this.lblTitle.Name = "lblTitle";
      this.lblTitle.Size = new System.Drawing.Size(157, 16);
      this.lblTitle.TabIndex = 1;
      this.lblTitle.Text = "Title";
      this.lblTitle.DoubleClick += new System.EventHandler(this.TileListItem_DoubleClick);
      this.lblTitle.MouseMove += new System.Windows.Forms.MouseEventHandler(this.TileListItem_MouseMove);
      this.lblTitle.Click += new System.EventHandler(this.TileListItem_Click);
      this.lblTitle.MouseDown += new System.Windows.Forms.MouseEventHandler(this.TileListItem_MouseDown);
      this.lblTitle.MouseUp += new System.Windows.Forms.MouseEventHandler(this.TileListItem_MouseUp);
      this.lblTitle.MouseEnter += new System.EventHandler(this.TileListItem_MouseEnter);
      // 
      // lblDescription
      // 
      this.lblDescription.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.lblDescription.BackColor = System.Drawing.Color.Transparent;
      this.lblDescription.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      this.lblDescription.ForeColor = System.Drawing.Color.DimGray;
      this.lblDescription.Location = new System.Drawing.Point(397, 0);
      this.lblDescription.Name = "lblDescription";
      this.lblDescription.Size = new System.Drawing.Size(240, 96);
      this.lblDescription.TabIndex = 2;
      this.lblDescription.Text = "Description";
      this.lblDescription.DoubleClick += new System.EventHandler(this.TileListItem_DoubleClick);
      this.lblDescription.MouseMove += new System.Windows.Forms.MouseEventHandler(this.TileListItem_MouseMove);
      this.lblDescription.Click += new System.EventHandler(this.TileListItem_Click);
      this.lblDescription.MouseDown += new System.Windows.Forms.MouseEventHandler(this.TileListItem_MouseDown);
      this.lblDescription.MouseUp += new System.Windows.Forms.MouseEventHandler(this.TileListItem_MouseUp);
      this.lblDescription.MouseEnter += new System.EventHandler(this.TileListItem_MouseEnter);
      // 
      // lblCreateDate
      // 
      this.lblCreateDate.BackColor = System.Drawing.Color.Transparent;
      this.lblCreateDate.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.lblCreateDate.ForeColor = System.Drawing.Color.Gray;
      this.lblCreateDate.Location = new System.Drawing.Point(231, 65);
      this.lblCreateDate.Name = "lblCreateDate";
      this.lblCreateDate.Size = new System.Drawing.Size(107, 19);
      this.lblCreateDate.TabIndex = 3;
      this.lblCreateDate.Text = "Create Date";
      this.lblCreateDate.DoubleClick += new System.EventHandler(this.TileListItem_DoubleClick);
      this.lblCreateDate.MouseMove += new System.Windows.Forms.MouseEventHandler(this.TileListItem_MouseMove);
      this.lblCreateDate.Click += new System.EventHandler(this.TileListItem_Click);
      this.lblCreateDate.MouseDown += new System.Windows.Forms.MouseEventHandler(this.TileListItem_MouseDown);
      this.lblCreateDate.MouseUp += new System.Windows.Forms.MouseEventHandler(this.TileListItem_MouseUp);
      this.lblCreateDate.MouseEnter += new System.EventHandler(this.TileListItem_MouseEnter);
      // 
      // lblAuthor
      // 
      this.lblAuthor.AutoSize = true;
      this.lblAuthor.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.lblAuthor.Location = new System.Drawing.Point(231, 48);
      this.lblAuthor.Name = "lblAuthor";
      this.lblAuthor.Size = new System.Drawing.Size(44, 13);
      this.lblAuthor.TabIndex = 4;
      this.lblAuthor.Text = "Author";
      this.lblAuthor.MouseLeave += new System.EventHandler(this.TileListItem_MouseLeave);
      this.lblAuthor.DoubleClick += new System.EventHandler(this.TileListItem_DoubleClick);
      this.lblAuthor.Click += new System.EventHandler(this.TileListItem_Click);
      this.lblAuthor.MouseHover += new System.EventHandler(this.TileListItem_MouseHover);
      this.lblAuthor.MouseEnter += new System.EventHandler(this.TileListItem_MouseEnter);
      // 
      // lblLInk
      // 
      this.lblLInk.AutoSize = true;
      this.lblLInk.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.lblLInk.Location = new System.Drawing.Point(169, 80);
      this.lblLInk.Name = "lblLInk";
      this.lblLInk.Size = new System.Drawing.Size(74, 16);
      this.lblLInk.TabIndex = 5;
      this.lblLInk.TabStop = true;
      this.lblLInk.Text = "More Infos";
      this.lblLInk.MouseLeave += new System.EventHandler(this.TileListItem_MouseLeave);
      this.lblLInk.DoubleClick += new System.EventHandler(this.TileListItem_DoubleClick);
      this.lblLInk.Click += new System.EventHandler(this.TileListItem_Click);
      this.lblLInk.MouseHover += new System.EventHandler(this.TileListItem_MouseHover);
      this.lblLInk.MouseEnter += new System.EventHandler(this.TileListItem_MouseEnter);
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label1.Location = new System.Drawing.Point(169, 48);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(48, 13);
      this.label1.TabIndex = 6;
      this.label1.Text = "Author:";
      this.label1.MouseLeave += new System.EventHandler(this.TileListItem_MouseLeave);
      this.label1.DoubleClick += new System.EventHandler(this.TileListItem_DoubleClick);
      this.label1.Click += new System.EventHandler(this.TileListItem_Click);
      this.label1.MouseHover += new System.EventHandler(this.TileListItem_MouseHover);
      this.label1.MouseEnter += new System.EventHandler(this.TileListItem_MouseEnter);
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label2.Location = new System.Drawing.Point(169, 30);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(57, 13);
      this.label2.TabIndex = 7;
      this.label2.Text = "Version :";
      this.label2.MouseLeave += new System.EventHandler(this.TileListItem_MouseLeave);
      this.label2.DoubleClick += new System.EventHandler(this.TileListItem_DoubleClick);
      this.label2.Click += new System.EventHandler(this.TileListItem_Click);
      this.label2.MouseHover += new System.EventHandler(this.TileListItem_MouseHover);
      this.label2.MouseEnter += new System.EventHandler(this.TileListItem_MouseEnter);
      // 
      // lblVersion
      // 
      this.lblVersion.AutoSize = true;
      this.lblVersion.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.lblVersion.Location = new System.Drawing.Point(231, 30);
      this.lblVersion.Name = "lblVersion";
      this.lblVersion.Size = new System.Drawing.Size(49, 13);
      this.lblVersion.TabIndex = 8;
      this.lblVersion.Text = "Version";
      this.lblVersion.MouseLeave += new System.EventHandler(this.TileListItem_MouseLeave);
      this.lblVersion.DoubleClick += new System.EventHandler(this.TileListItem_DoubleClick);
      this.lblVersion.Click += new System.EventHandler(this.TileListItem_Click);
      this.lblVersion.MouseHover += new System.EventHandler(this.TileListItem_MouseHover);
      this.lblVersion.MouseEnter += new System.EventHandler(this.TileListItem_MouseEnter);
      // 
      // TileListItem
      // 
      this.Controls.Add(this.lblVersion);
      this.Controls.Add(this.label2);
      this.Controls.Add(this.label1);
      this.Controls.Add(this.lblLInk);
      this.Controls.Add(this.lblAuthor);
      this.Controls.Add(this.lblCreateDate);
      this.Controls.Add(this.lblDescription);
      this.Controls.Add(this.lblTitle);
      this.Controls.Add(this.picBox);
      this.Name = "TileListItem";
      this.Size = new System.Drawing.Size(640, 100);
      this.MouseLeave += new System.EventHandler(this.TileListItem_MouseLeave);
      this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.TileListItem_MouseMove);
      this.MouseEnter += new System.EventHandler(this.TileListItem_MouseEnter);
      ((System.ComponentModel.ISupportInitialize)(this.picBox)).EndInit();
      this.ResumeLayout(false);
      this.PerformLayout();

		}
		#endregion
		private void InitializeMyComponent()
		{
			base.MouseUp += new System.Windows.Forms.MouseEventHandler(this.TileListItem_MouseUp);
			base.DoubleClick += new System.EventHandler(this.TileListItem_DoubleClick);
			base.MouseDown += new System.Windows.Forms.MouseEventHandler(this.TileListItem_MouseDown);
			itemTip=new ToolTip();
			itemTip.Active=false;
		}
		private void TileListItem_MouseEnter(object sender, System.EventArgs e)
		{
			if (selected==false)
			{
				if (horverImage!=null)
				{
					this.BackgroundImage=horverImage;
				}
				else
					this.BackColor=horverColor;
			}
		}

		private void TileListItem_MouseLeave(object sender, System.EventArgs e)
		{
			if (selected==false)
			{
				if (normalImage!=null)
				{
					this.BackgroundImage=normalImage;
				}
				else
					this.BackColor=normalColor;
			}
		}
		private string id="";
		public string ID
		{
			get
			{
				return id;
			}
			set
			{
				id=value;
			}
		}
		private string type;
		public string Type
		{
			get
			{
				return type;
			}
			set
			{
				type=value;
				SetToolTipText();
			}
		}
		private bool selected=false;
		public bool Selected
		{
			get
			{
				return selected;
			}
			set
			{
				selected=value;
				if (this.selected==true)
				{
					if (selectionImage!=null)
					{
						this.BackgroundImage=selectionImage;
					}
					else
					{
						this.BackColor=selectionColor;
					}
					
					if (ItemSelected!=null)
					{
						ItemSelected(this,new EventArgs());
					}
				}
				else
				{
					if (normalImage!=null)
					{
						this.BackgroundImage=normalImage;
					}
					else
						this.BackColor=normalColor;
				}
			}
		}
		public System.Drawing.Image Image
		{
			get
			{
				return this.picBox.Image;
			}
			set
			{
				this.picBox.Image=value;
			}
		}
		private int imageIndex=0;
		public int ImageIndex
		{
			get
			{
				return imageIndex;
			}
			set
			{
				imageIndex=value;
				if(this.Parent!=null && this.Parent is ControlListView)
				{
					ControlListView listView=(ControlListView)this.Parent;
					this.Image = listView.ItemIconImageList.Images[imageIndex];
				}
			}
		}
		private DataRow attachmentDataRow=null;
		public DataRow AttachmentDataRow
		{
			get
			{
				return attachmentDataRow;
			}
			set
			{
				attachmentDataRow=value;
			}
		}

    private string infoLink;
    public string InfoLink
    {
      get
      {
        return infoLink;
      }
      set
      {
        infoLink = value;
      }
    }

		public string Title
		{
			get
			{
				return lblTitle.Text;
			}
			set
			{
				lblTitle.Text=value;
				if(this.AttachmentDataRow!=null)
				{
					AttachmentDataRow["Title"]=value;
				}
				SetToolTipText();
			}
		}
    
    public string Author
    {
      get
      {
        return lblAuthor.Text;
      }
      set
      {
        lblAuthor.Text = value;
        if (this.AttachmentDataRow != null)
        {
          AttachmentDataRow["Author"] = value;
        }
        SetToolTipText();
      }
    }

    public string Version
    {
      get
      {
        return lblVersion.Text;
      }
      set
      {
        lblVersion.Text = value;
        if (this.AttachmentDataRow != null)
        {
          AttachmentDataRow["Version"] = value;
        }
        SetToolTipText();
      }
    }

    public string Description
		{
			get
			{
				return lblDescription.Text;
			}
			set
			{
				lblDescription.Text=value;
				if(this.AttachmentDataRow!=null)
				{
					this.AttachmentDataRow["Description"]=value;
				}
				SetToolTipText();
			}
		}

		private DateTime createDate=DateTime.Now;
		public DateTime CreateDate
		{
			get
			{
				return createDate;
			}
			set
			{
				createDate=value;
				this.lblCreateDate.Text=createDate.ToShortDateString();
				SetToolTipText();
			}
		}
    private void SetToolTipText()
    {
      string tooltipText = "";
      tooltipText += "Title: " + this.Title + "\r\n";
      tooltipText += "Version: " + this.Version + "\r\n";
      tooltipText += "Type: " + this.Type + "\r\n";
      tooltipText += "Author: " + this.Author + "\r\n";
      tooltipText += "Create Date: " + this.CreateDate.ToString() + "\r\n";
      itemTip.SetToolTip(this, tooltipText);
      itemTip.SetToolTip(this.lblCreateDate, tooltipText);
      itemTip.SetToolTip(this.lblDescription, tooltipText);
      itemTip.SetToolTip(this.lblTitle, tooltipText);
      itemTip.SetToolTip(this.lblAuthor, tooltipText);
      itemTip.SetToolTip(this.picBox, tooltipText);
    }

		private System.Drawing.Image selectionImage=null;
		public System.Drawing.Image SelectionImage
		{
			get
			{
				return selectionImage;
			}
			set
			{
				selectionImage=value;
			}
		}
		private System.Drawing.Image horverImage=null;
		public System.Drawing.Image HorverImage
		{
			get
			{
				return horverImage;
			}
			set
			{
				horverImage=value;
			}
		}
		private System.Drawing.Image normalImage=null;
		public System.Drawing.Image NormalImage
		{
			get
			{
				return normalImage;
			}
			set
			{
				normalImage=value;
			}
		}
		private System.Drawing.Color selectionColor;
		public System.Drawing.Color SelectionColor
		{
			get
			{
				return selectionColor;
			}
			set
			{
				selectionColor=value;
			}
		}
		private System.Drawing.Color horverColor;
		public System.Drawing.Color HorverColor
		{
			get
			{
				return horverColor;
			}
			set
			{
				horverColor=value;
			}
		}
		private System.Drawing.Color normalColor;
		public System.Drawing.Color NormalColor
		{
			get
			{
				return normalColor;
			}
			set
			{
				normalColor=value;
			}
		}
		public System.Windows.Forms.PictureBox PictureBox
		{
			get
			{
				return picBox;
			}
		}
		public System.Windows.Forms.Label TitleLabel
		{
			get
			{
				return lblTitle;
			}
		}
		public System.Windows.Forms.Label DescriptionLabel
		{
			get
			{
				return lblDescription;
			}
		}

		private bool showToolTips=false;
		public bool ShowToolTips
		{
			get
			{
				return showToolTips;
			}
			set
			{
				showToolTips=value;
				if (showToolTips==true)
				{
					itemTip.Active=true;
				}
				else
				{
					itemTip.Active=false;
				}
			}
		}

		private void TileListItem_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			if (selected==false)
			{
				if (horverImage!=null)
				{
					this.BackgroundImage=horverImage;
				}
				else
					this.BackColor=horverColor;
			}
		}

		private void TileListItem_MouseHover(object sender, System.EventArgs e)
		{
			if (selected==false)
			{
				if (horverImage!=null)
				{
					this.BackgroundImage=horverImage;
				}
				else
					this.BackColor=horverColor;
			}
		}

		private void TileListItem_Click(object sender, EventArgs e)
		{
			
		}

		private void TileListItem_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			if (Selected==false)
			{
				Selected=true;
			}
			if (MouseDown!=null)
			{
				MouseDown(sender,e);
			}
		}

		private void TileListItem_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			if (MouseUp!=null)
			{
				MouseUp(sender,e);
			}
		}

		private void TileListItem_DoubleClick(object sender, EventArgs e)
		{
			if (DoubleClick!=null)
			{
				DoubleClick(this,e);
			}
		}
	}
}
