﻿namespace Job_Card
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Windows.Forms;

    public class PictureViewer : Form
    {
        private Button btnDeletePicture;
        private Button btnPrint;
        private IContainer components = null;
        private int deleteIndex = -1;
        private PictureBox mainPic;
        private const int maxPics = 9;
        private int originalImageHeight;
        private int originalImageWidth;
        private PictureBox pictureBox1;
        private PictureBox pictureBox10;
        private PictureBox pictureBox2;
        private PictureBox pictureBox3;
        private PictureBox pictureBox4;
        private PictureBox pictureBox5;
        private PictureBox pictureBox6;
        private PictureBox pictureBox7;
        private PictureBox pictureBox8;
        private PictureBox pictureBox9;
        private PictureBox pictureBoxZoom;
        private Button btnNext;
        private Bitmap zoomBitMap;
        private int nextOffset = 0;

        public PictureViewer()
        {
            this.InitializeComponent();
            this.Init();
        }

        private void btnDeletePicture_Click(object sender, EventArgs e)
        {
            if ((this.deleteIndex >= 0) && (MessageBox.Show("Are you sure you wish to delete this picture?" + Environment.NewLine + "This cannot be undone", "Confirm Deletion", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation) == DialogResult.OK))
            {
                try
                {
                    for (int i = 1; i <= 10; i++)
                    {
                        Control[] controlArray = base.Controls.Find("pictureBox" + i, true);
                        if (controlArray.Length > 0)
                        {
                            JobCard.UpdatePictureBox((PictureBox) controlArray[0], null);
                        }
                    }
                    JobCard.UpdatePictureBox(this.mainPic, null);
                    this.mainPic.Image = null;
                    string path = this.allPictures[this.deleteIndex];
                    this.allPictures.RemoveAt(this.deleteIndex);
                    this.deleteIndex = -1;
                    if (this.zoomBitMap != null)
                    {
                        this.zoomBitMap.Dispose();
                    }
                    this.zoomBitMap = null;
                    JobCard.UpdatePictureBox(this.pictureBoxZoom, null);
                    this.pictureBox10.Image = null;
                    this.pictureBoxZoom.Image = null;
                    this.pictureBox10.Visible = false;
                    this.pictureBoxZoom.Visible = false;
                    this.btnDeletePicture.Visible = false;
                    File.Delete(path);
                    JobCard.currentPictureIndex = (this.allPictures.Count == 0) ? -1 : 0;
                    JobCard.UpdatePictureBox(this.mainPic, (JobCard.currentPictureIndex == -1) ? null : JobCard.FromFile(this.allPictures[JobCard.currentPictureIndex]));
                    if (this.allPictures.Count > 0)
                    {
                        this.SetPictureList(this.mainPic);
                    }
                    else
                    {
                        base.Close();
                    }
                }
                catch (Exception exception)
                {
                    MessageBox.Show("Deletion failed err:" + exception.Message);
                }
            }
        }

        private void btnPrint_Click(object sender, EventArgs e)
        {
            CustomerCopy.autoPrint = true;
            CustomerCopy copy = new CustomerCopy(true);
            copy.Height = (int) (copy.Width * Math.Sqrt(2.0));
            RichTextBox box = copy.richTextBox1;
            int count = this.allPictures.Count;
            float num2 = 0.98f;
            float num3 = 0.98f;
            int num5 = count - 1;
            if (this.pictureBox10.Visible)
            {
                Clipboard.SetImage(JobCard.resizeImage(this.pictureBox10.Image, new Size((int) (copy.Width * num2), (int) (copy.Height * num3))));
                box.Paste();
            }
            else
            {
                switch (count)
                {
                    case 2:
                    case 3:
                        num3 = 0.98f / ((float) count);
                        break;

                    case 4:
                        num2 = 0.49f;
                        num3 = 0.49f;
                        break;

                    case 5:
                    case 6:
                        num2 = 0.49f;
                        num3 = 0.3266667f;
                        break;

                    case 7:
                    case 8:
                    case 9:
                        num2 = 0.3266667f;
                        num3 = 0.3266667f;
                        break;
                }
                float num6 = 0f;
                for (int i = 0; i < this.allPictures.Count; i++)
                {
                    Clipboard.SetImage(JobCard.resizeImage(JobCard.FromFile(this.allPictures[i]), new Size((int) (copy.Width * num2), (int) (copy.Height * num3))));
                    num6 += num2;
                    if (num6 > 1f)
                    {
                        box.AppendText(Environment.NewLine);
                        num6 = num2;
                    }
                    box.Paste();
                }
            }
            copy.PrintNow();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (this.components != null))
            {
                this.components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void Init()
        {
            for (int i = 1; i <= 10; i++)
            {
                Control[] controlArray = base.Controls.Find("pictureBox" + i, true);
                if (controlArray.Length > 0)
                {
                    ((PictureBox) controlArray[0]).SizeMode = PictureBoxSizeMode.Zoom;
                    ((PictureBox) controlArray[0]).Visible = false;
                    ((PictureBox) controlArray[0]).Click += new EventHandler(this.pictureBox_Click);
                }
            }
            this.pictureBoxZoom.Visible = false;
            this.btnDeletePicture.Visible = false;
        }

        private void InitializeComponent()
        {
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.pictureBox3 = new System.Windows.Forms.PictureBox();
            this.pictureBox4 = new System.Windows.Forms.PictureBox();
            this.pictureBox5 = new System.Windows.Forms.PictureBox();
            this.pictureBox6 = new System.Windows.Forms.PictureBox();
            this.pictureBox7 = new System.Windows.Forms.PictureBox();
            this.pictureBox8 = new System.Windows.Forms.PictureBox();
            this.pictureBox9 = new System.Windows.Forms.PictureBox();
            this.pictureBox10 = new System.Windows.Forms.PictureBox();
            this.btnPrint = new System.Windows.Forms.Button();
            this.pictureBoxZoom = new System.Windows.Forms.PictureBox();
            this.btnDeletePicture = new System.Windows.Forms.Button();
            this.btnNext = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox4)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox5)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox6)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox7)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox8)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox9)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox10)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxZoom)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureBox1
            // 
            this.pictureBox1.Location = new System.Drawing.Point(7, 7);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(347, 268);
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            // 
            // pictureBox2
            // 
            this.pictureBox2.Location = new System.Drawing.Point(360, 7);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(347, 268);
            this.pictureBox2.TabIndex = 1;
            this.pictureBox2.TabStop = false;
            // 
            // pictureBox3
            // 
            this.pictureBox3.Location = new System.Drawing.Point(713, 7);
            this.pictureBox3.Name = "pictureBox3";
            this.pictureBox3.Size = new System.Drawing.Size(347, 268);
            this.pictureBox3.TabIndex = 2;
            this.pictureBox3.TabStop = false;
            // 
            // pictureBox4
            // 
            this.pictureBox4.Location = new System.Drawing.Point(713, 281);
            this.pictureBox4.Name = "pictureBox4";
            this.pictureBox4.Size = new System.Drawing.Size(347, 268);
            this.pictureBox4.TabIndex = 5;
            this.pictureBox4.TabStop = false;
            // 
            // pictureBox5
            // 
            this.pictureBox5.Location = new System.Drawing.Point(360, 281);
            this.pictureBox5.Name = "pictureBox5";
            this.pictureBox5.Size = new System.Drawing.Size(347, 268);
            this.pictureBox5.TabIndex = 4;
            this.pictureBox5.TabStop = false;
            // 
            // pictureBox6
            // 
            this.pictureBox6.Location = new System.Drawing.Point(7, 281);
            this.pictureBox6.Name = "pictureBox6";
            this.pictureBox6.Size = new System.Drawing.Size(347, 268);
            this.pictureBox6.TabIndex = 3;
            this.pictureBox6.TabStop = false;
            // 
            // pictureBox7
            // 
            this.pictureBox7.Location = new System.Drawing.Point(713, 554);
            this.pictureBox7.Name = "pictureBox7";
            this.pictureBox7.Size = new System.Drawing.Size(347, 268);
            this.pictureBox7.TabIndex = 8;
            this.pictureBox7.TabStop = false;
            // 
            // pictureBox8
            // 
            this.pictureBox8.Location = new System.Drawing.Point(360, 554);
            this.pictureBox8.Name = "pictureBox8";
            this.pictureBox8.Size = new System.Drawing.Size(347, 268);
            this.pictureBox8.TabIndex = 7;
            this.pictureBox8.TabStop = false;
            // 
            // pictureBox9
            // 
            this.pictureBox9.Location = new System.Drawing.Point(7, 554);
            this.pictureBox9.Name = "pictureBox9";
            this.pictureBox9.Size = new System.Drawing.Size(347, 268);
            this.pictureBox9.TabIndex = 6;
            this.pictureBox9.TabStop = false;
            // 
            // pictureBox10
            // 
            this.pictureBox10.Location = new System.Drawing.Point(6, 6);
            this.pictureBox10.Name = "pictureBox10";
            this.pictureBox10.Size = new System.Drawing.Size(1052, 816);
            this.pictureBox10.TabIndex = 9;
            this.pictureBox10.TabStop = false;
            this.pictureBox10.Visible = false;
            this.pictureBox10.MouseMove += new System.Windows.Forms.MouseEventHandler(this.MouseOver);
            // 
            // btnPrint
            // 
            this.btnPrint.Font = new System.Drawing.Font("Arial", 9F);
            this.btnPrint.Location = new System.Drawing.Point(7, 7);
            this.btnPrint.Name = "btnPrint";
            this.btnPrint.Size = new System.Drawing.Size(42, 29);
            this.btnPrint.TabIndex = 10;
            this.btnPrint.Text = "Print";
            this.btnPrint.UseVisualStyleBackColor = true;
            this.btnPrint.Click += new System.EventHandler(this.btnPrint_Click);
            // 
            // pictureBoxZoom
            // 
            this.pictureBoxZoom.Location = new System.Drawing.Point(373, 6);
            this.pictureBoxZoom.Name = "pictureBoxZoom";
            this.pictureBoxZoom.Size = new System.Drawing.Size(100, 50);
            this.pictureBoxZoom.TabIndex = 11;
            this.pictureBoxZoom.TabStop = false;
            this.pictureBoxZoom.Visible = false;
            // 
            // btnDeletePicture
            // 
            this.btnDeletePicture.Font = new System.Drawing.Font("Arial", 9F);
            this.btnDeletePicture.Location = new System.Drawing.Point(82, 7);
            this.btnDeletePicture.Name = "btnDeletePicture";
            this.btnDeletePicture.Size = new System.Drawing.Size(55, 29);
            this.btnDeletePicture.TabIndex = 12;
            this.btnDeletePicture.Text = "Delete";
            this.btnDeletePicture.UseVisualStyleBackColor = true;
            this.btnDeletePicture.Click += new System.EventHandler(this.btnDeletePicture_Click);
            // 
            // btnNext
            // 
            this.btnNext.Font = new System.Drawing.Font("Arial", 9F);
            this.btnNext.Location = new System.Drawing.Point(1016, 6);
            this.btnNext.Name = "btnNext";
            this.btnNext.Size = new System.Drawing.Size(42, 29);
            this.btnNext.TabIndex = 13;
            this.btnNext.Text = "Next";
            this.btnNext.UseVisualStyleBackColor = true;
            this.btnNext.Click += new System.EventHandler(this.btnNext_Click);
            // 
            // PictureViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1070, 741);
            this.Controls.Add(this.btnNext);
            this.Controls.Add(this.btnDeletePicture);
            this.Controls.Add(this.pictureBoxZoom);
            this.Controls.Add(this.btnPrint);
            this.Controls.Add(this.pictureBox10);
            this.Controls.Add(this.pictureBox7);
            this.Controls.Add(this.pictureBox8);
            this.Controls.Add(this.pictureBox9);
            this.Controls.Add(this.pictureBox4);
            this.Controls.Add(this.pictureBox5);
            this.Controls.Add(this.pictureBox6);
            this.Controls.Add(this.pictureBox3);
            this.Controls.Add(this.pictureBox2);
            this.Controls.Add(this.pictureBox1);
            this.Name = "PictureViewer";
            this.Text = "PictureViewer";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.PictureViewer_FormClosing);
            this.Load += new System.EventHandler(this.PictureViewer_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox4)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox5)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox6)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox7)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox8)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox9)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox10)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxZoom)).EndInit();
            this.ResumeLayout(false);

        }

        private void MouseOver(object sender, MouseEventArgs e)
        {
            if (sender is PictureBox)
            {
                PictureBox box = (PictureBox) sender;
                if (box.Visible && (box.Image != null))
                {
                    int width = box.Width;
                    int height = box.Height;
                    float num3 = ((float) this.originalImageWidth) / ((float) this.originalImageHeight);
                    if (num3 >= 1f)
                    {
                        height = (int) (((float) width) / num3);
                    }
                    else
                    {
                        width = (int) (height * num3);
                    }
                    int num4 = (box.Width - width) / 2;
                    int num5 = num4 + width;
                    int num6 = (box.Height - height) / 2;
                    int num7 = num6 + height;
                    int x = e.X;
                    int y = e.Y;
                    if ((((x >= num4) && (x <= num5)) && (y >= num6)) && (y <= num7))
                    {
                        int num10 = Math.Max(num4, x - 50) - num4;
                        int num11 = Math.Min(num5, x + 50) - num4;
                        int num12 = Math.Max(num6, y - 50) - num6;
                        int num13 = Math.Min(num7, y + 50) - num6;
                        int num14 = (int) ((((float) num10) / ((float) width)) * this.originalImageWidth);
                        int num15 = (int) ((((float) num11) / ((float) width)) * this.originalImageWidth);
                        int num16 = (int) ((((float) num12) / ((float) height)) * this.originalImageHeight);
                        int num17 = (int) ((((float) num13) / ((float) height)) * this.originalImageHeight);
                        if (y < ((box.Height / 3) + 10))
                        {
                            if (x > (box.Width / 2))
                            {
                                this.pictureBoxZoom.Left = 0;
                            }
                            else
                            {
                                this.pictureBoxZoom.Left = (int) (box.Width * 0.6666);
                            }
                        }
                        Rectangle rect = new Rectangle(num14, num16, num15 - num14, num17 - num16);
                        PixelFormat pixelFormat = this.zoomBitMap.PixelFormat;
                        JobCard.UpdatePictureBox(this.pictureBoxZoom, this.zoomBitMap.Clone(rect, pixelFormat));
                    }
                }
            }
        }

        private void pictureBox_Click(object sender, EventArgs e)
        {
            if (sender is PictureBox)
            {
                PictureBox box = (PictureBox) sender;
                int num = int.Parse(box.Name.Substring(10)) - 1;
                string filename = "";
                if (num <= this.allPictures.Count)
                {
                    filename = this.allPictures[num];           
                }
                if (box.Image == JobCard.MovieImage)
                {
                    Form1.useMediaPlayer = true;
                    Form1.url = filename;
                    Form1 form1 = new Form1();
                    form1.ShowDialog();
                }
                else
                {
                    this.originalImageWidth = box.Image.Width;
                    this.originalImageHeight = box.Image.Height;
                    if (num <= this.allPictures.Count)
                    {
                        this.zoomBitMap = new Bitmap(filename);
                    }
                    if (box == this.pictureBox10)
                    {
                        box.Image = null;
                    }
                    else
                    {
                        this.deleteIndex = num;
                        this.pictureBox10.Image = box.Image;
                    }
                    this.pictureBox10.Visible = box != this.pictureBox10;
                    this.pictureBoxZoom.Visible = box != this.pictureBox10;
                    this.pictureBoxZoom.Width = this.pictureBox10.Width / 3;
                    this.pictureBoxZoom.Height = this.pictureBox10.Height / 3;
                    this.pictureBoxZoom.Left = 0;
                    this.pictureBoxZoom.Top = 0;
                    this.pictureBoxZoom.SizeMode = PictureBoxSizeMode.Zoom;
                    this.btnDeletePicture.Visible = this.pictureBox10.Visible;
                }
            }
        }

        private void PictureViewer_FormClosing(object sender, FormClosingEventArgs e)
        {
            for (int i = 1; i <= 10; i++)
            {
                Control[] controlArray = base.Controls.Find("pictureBox" + i, true);
                if (controlArray.Length > 0)
                {
                    PictureBox box = (PictureBox) controlArray[0];
                    if ((box != null) && (box.Image != null))
                    {
                        if (box.Image != JobCard.MovieImage)
                        {
                            box.Image.Dispose();
                        }
                    }
                }
            }
        }

        private void PictureViewer_Load(object sender, EventArgs e)
        {
        }

        public void SetPictureList(PictureBox mainPictureBox, int theNextOffset = 0)
        {
            int num;
            Control[] controlArray;
            this.mainPic = mainPictureBox;
            for (num = 0; num < 9; num++)
            {
                controlArray = base.Controls.Find("pictureBox" + (num + 1), true);
                if (controlArray.Length > 0)
                {
                    ((PictureBox) controlArray[0]).Visible = false;
                }
            }
            nextOffset += theNextOffset;
            if (nextOffset >= this.allPictures.Count)
            {
                nextOffset = 0;
            }
            int maxCount = Math.Min(9, this.allPictures.Count - nextOffset);
            for (num = 0; num < maxCount; num++)
            {
                controlArray = base.Controls.Find("pictureBox" + (num + 1), true);
                if (controlArray.Length > 0)
                {
                    ((PictureBox) controlArray[0]).Visible = true;
                    JobCard.UpdatePictureBox((PictureBox) controlArray[0], JobCard.FromFile(this.allPictures[nextOffset + num]));
                }
            }
        }

        private List<string> allPictures =>
            JobCard.currentPhotoPaths;

        private void btnNext_Click(object sender, EventArgs e)
        {
            this.SetPictureList(this.mainPic, 9);
        }
    }
}

