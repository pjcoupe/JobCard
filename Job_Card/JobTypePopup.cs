namespace Job_Card
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Drawing;
    using System.Globalization;
    using System.Linq;
    using System.Windows.Forms;

    public class JobTypePopup : Form
    {
        private Button clearButton;
        private Button checkBox1;
        private Button checkBox10;
        private Button checkBox11;
        private Button checkBox12;
        private Button checkBox13;
        private Button checkBox14;
        private Button checkBox15;
        private Button checkBox16;
        private Button checkBox17;
        private Button checkBox18;
        private Button checkBox19;
        private Button checkBox2;
        private Button checkBox20;
        private Button checkBox21;
        private Button checkBox22;
        private Button checkBox3;
        private Button checkBox4;
        private Button checkBox5;
        private Button checkBox6;
        private Button checkBox7;
        private Button checkBox8;
        private Button checkBox9;
        private IContainer components = null;
        private GroupBox groupBox1;
        private GroupBox groupBox2;
        private GroupBox groupBox3;
        private GroupBox groupBox4;
        private GroupBox groupBox5;
        public static TextBox jobType;
        private bool startup = true;

        public JobTypePopup()
        {
            this.startup = true;
            this.InitializeComponent();
            List<string> list = jobType.Text.Split(new string[] { ", " }, StringSplitOptions.None).ToList<string>();
            foreach (object obj2 in base.Controls)
            {
                if (obj2 is GroupBox)
                {
                    GroupBox box = (GroupBox) obj2;
                    foreach (object obj3 in box.Controls)
                    {
                        if (obj3 is Button)
                        {
                            Button box2 = (Button) obj3;
                            string item = new CultureInfo("en-NZ", false).TextInfo.ToTitleCase(box2.Text.ToLowerInvariant());
                            /*
                            if (list.Contains(item))
                            {
                                box2.Checked = true;
                                box2.BackColor = Color.Green;
                            }
                            else
                            {
                                box2.Checked = false;
                                box2.BackColor = Color.Gray;
                            }
                            */
                        }
                    }
                }
            }
            this.startup = false;
        }

        private void ClearClicked(object sender, EventArgs e)
        {
            jobType.Text = "";
        }
        private void CheckedChanged(object sender, EventArgs e)
        {
            if (!this.startup)
            {
                Button box = (Button) sender;
                string item = new CultureInfo("en-NZ", false).TextInfo.ToTitleCase(box.Text.ToLowerInvariant());
                Dictionary<string, int> dict = new Dictionary<string, int>();
                List<string> list = jobType.Text.Split(new string[] { ", " }, StringSplitOptions.None).ToList<string>();
                list.Add(item);
                List<string> santisedList = new List<string>();
                foreach (var l in list)
                {
                    string theItem = l;
                    short counter = 1;
                    if (l.Contains("x)"))
                    {
                        int idx = l.IndexOf('(');
                        if (idx >= 0)
                        {
                            int endIdx = l.IndexOf('x',idx);
                            if (endIdx >= idx + 2)
                            {
                                string numberStr = l.Substring(idx + 1, endIdx - idx - 1);
                                if (!Int16.TryParse(numberStr, out counter))
                                {
                                    counter = 1;
                                }
                                else
                                {
                                    theItem = theItem.Substring(endIdx + 2);
                                    
                                }
                            }
                        }
                    }
                    theItem = theItem.Trim();
                    theItem = new CultureInfo("en-NZ", false).TextInfo.ToTitleCase(theItem);
                    int alreadyExistingCount = 0;
                    dict.TryGetValue(theItem, out alreadyExistingCount);
                    dict[theItem] = counter + alreadyExistingCount;
                    if (!santisedList.Contains(theItem))
                    {
                        santisedList.Add(theItem);
                    }
                }
                
                string str2 = "";
                foreach (string str3 in santisedList)
                {
                    int count = 1;
                    if (str2 != "")
                    {
                        str2 = str2 + ", ";
                    }
                    string xTimes = "";
                    dict.TryGetValue(str3, out count);
                    if (count > 1)
                    {
                        xTimes = "(" + count.ToString() + "x)";
                    }
                    str2 = str2 + xTimes+str3;
                }
                jobType.Text = str2;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (this.components != null))
            {
                this.components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.clearButton = new Button();
            this.groupBox1 = new GroupBox();
            this.checkBox4 = new Button();
            this.checkBox3 = new Button();
            this.checkBox2 = new Button();
            this.checkBox1 = new Button();
            this.groupBox2 = new GroupBox();
            this.checkBox13 = new Button();
            this.checkBox12 = new Button();
            this.checkBox11 = new Button();
            this.checkBox10 = new Button();
            this.checkBox9 = new Button();
            this.checkBox8 = new Button();
            this.checkBox7 = new Button();
            this.checkBox6 = new Button();
            this.checkBox5 = new Button();
            this.groupBox3 = new GroupBox();
            this.checkBox19 = new Button();
            this.checkBox18 = new Button();
            this.checkBox17 = new Button();
            this.checkBox16 = new Button();
            this.checkBox15 = new Button();
            this.checkBox14 = new Button();
            this.groupBox4 = new GroupBox();
            this.checkBox21 = new Button();
            this.checkBox20 = new Button();
            this.groupBox5 = new GroupBox();
            this.checkBox22 = new Button();
            
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.groupBox5.SuspendLayout();
            base.SuspendLayout();
            this.groupBox1.Controls.Add(this.checkBox4);
            this.groupBox1.Controls.Add(this.checkBox3);
            this.groupBox1.Controls.Add(this.checkBox2);
            this.groupBox1.Controls.Add(this.checkBox1);
            this.groupBox1.Location = new Point(9, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new Size(220, 0x5e);
            this.groupBox1.TabIndex = 4;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Repair and Finishing";
           
            this.checkBox4.Location = new Point(0x72, 0x35);
            this.checkBox4.Name = "checkBox4";
            this.checkBox4.Size = new Size(90, 0x17);
            this.checkBox4.TabIndex = 7;
            this.checkBox4.Text = "LAQUER";
            this.checkBox4.TextAlign = ContentAlignment.MiddleCenter;
            this.checkBox4.UseVisualStyleBackColor = true;
            this.checkBox4.Click += new EventHandler(this.CheckedChanged);
            
            this.checkBox3.Location = new Point(11, 0x35);
            this.checkBox3.Name = "checkBox3";
            this.checkBox3.Size = new Size(90, 0x17);
            this.checkBox3.TabIndex = 6;
            this.checkBox3.Text = "POLISH";
            this.checkBox3.TextAlign = ContentAlignment.MiddleCenter;
            this.checkBox3.UseVisualStyleBackColor = true;
            this.checkBox3.Click += new EventHandler(this.CheckedChanged);
            
            this.checkBox2.Location = new Point(0x72, 0x13);
            this.checkBox2.Name = "checkBox2";
            this.checkBox2.Size = new Size(90, 0x17);
            this.checkBox2.TabIndex = 5;
            this.checkBox2.Text = "REPAIR";
            this.checkBox2.TextAlign = ContentAlignment.MiddleCenter;
            this.checkBox2.UseVisualStyleBackColor = true;
            this.checkBox2.Click += new EventHandler(this.CheckedChanged);
            
            this.checkBox1.Location = new Point(11, 0x15);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new Size(90, 0x17);
            this.checkBox1.TabIndex = 4;
            this.checkBox1.Text = "STRIP";
            this.checkBox1.TextAlign = ContentAlignment.MiddleCenter;
            this.checkBox1.UseVisualStyleBackColor = true;
            this.checkBox1.Click += new EventHandler(this.CheckedChanged);
            this.groupBox2.Controls.Add(this.checkBox13);
            this.groupBox2.Controls.Add(this.checkBox12);
            this.groupBox2.Controls.Add(this.checkBox11);
            this.groupBox2.Controls.Add(this.checkBox10);
            this.groupBox2.Controls.Add(this.checkBox9);
            this.groupBox2.Controls.Add(this.checkBox8);
            this.groupBox2.Controls.Add(this.checkBox7);
            this.groupBox2.Controls.Add(this.checkBox6);
            this.groupBox2.Controls.Add(this.checkBox5);
            this.groupBox2.Location = new Point(9, 0x70);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new Size(220, 0xb6);
            this.groupBox2.TabIndex = 5;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Plating";
            
            this.checkBox13.Location = new Point(0x3e, 0x93);
            this.checkBox13.Name = "checkBox13";
            this.checkBox13.Size = new Size(90, 0x17);
            this.checkBox13.TabIndex = 0x10;
            this.checkBox13.Text = "GOLD";
            this.checkBox13.TextAlign = ContentAlignment.MiddleCenter;
            this.checkBox13.UseVisualStyleBackColor = true;
            this.checkBox13.Click += new EventHandler(this.CheckedChanged);
            
            this.checkBox12.Location = new Point(0x72, 0x76);
            this.checkBox12.Name = "checkBox12";
            this.checkBox12.Size = new Size(90, 0x17);
            this.checkBox12.TabIndex = 15;
            this.checkBox12.Text = "SILVER";
            this.checkBox12.TextAlign = ContentAlignment.MiddleCenter;
            this.checkBox12.UseVisualStyleBackColor = true;
            this.checkBox12.Click += new EventHandler(this.CheckedChanged);
        
            this.checkBox11.Location = new Point(12, 0x76);
            this.checkBox11.Name = "checkBox11";
            this.checkBox11.Size = new Size(90, 0x17);
            this.checkBox11.TabIndex = 14;
            this.checkBox11.Text = "TIN";
            this.checkBox11.TextAlign = ContentAlignment.MiddleCenter;
            this.checkBox11.UseVisualStyleBackColor = true;
            this.checkBox11.Click += new EventHandler(this.CheckedChanged);
            
            this.checkBox10.Location = new Point(0x72, 0x56);
            this.checkBox10.Name = "checkBox10";
            this.checkBox10.Size = new Size(90, 0x17);
            this.checkBox10.TabIndex = 13;
            this.checkBox10.Text = "SATIN";
            this.checkBox10.TextAlign = ContentAlignment.MiddleCenter;
            this.checkBox10.UseVisualStyleBackColor = true;
            this.checkBox10.Click += new EventHandler(this.CheckedChanged);
           
            this.checkBox9.Location = new Point(12, 0x56);
            this.checkBox9.Name = "checkBox9";
            this.checkBox9.Size = new Size(90, 0x17);
            this.checkBox9.TabIndex = 12;
            this.checkBox9.Text = "BRONZE";
            this.checkBox9.TextAlign = ContentAlignment.MiddleCenter;
            this.checkBox9.UseVisualStyleBackColor = true;
            this.checkBox9.Click += new EventHandler(this.CheckedChanged);
        
            this.checkBox8.Location = new Point(0x72, 0x36);
            this.checkBox8.Name = "checkBox8";
            this.checkBox8.Size = new Size(90, 0x17);
            this.checkBox8.TabIndex = 11;
            this.checkBox8.Text = "BRASS";
            this.checkBox8.TextAlign = ContentAlignment.MiddleCenter;
            this.checkBox8.UseVisualStyleBackColor = true;
            this.checkBox8.Click += new EventHandler(this.CheckedChanged);
          
            this.checkBox7.Location = new Point(12, 0x35);
            this.checkBox7.Name = "checkBox7";
            this.checkBox7.Size = new Size(90, 0x17);
            this.checkBox7.TabIndex = 10;
            this.checkBox7.Text = "CHROME";
            this.checkBox7.TextAlign = ContentAlignment.MiddleCenter;
            this.checkBox7.UseVisualStyleBackColor = true;
            this.checkBox7.Click += new EventHandler(this.CheckedChanged);
        
            this.checkBox6.Location = new Point(0x72, 0x13);
            this.checkBox6.Name = "checkBox6";
            this.checkBox6.Size = new Size(90, 0x17);
            this.checkBox6.TabIndex = 9;
            this.checkBox6.Text = "NICKLE";
            this.checkBox6.TextAlign = ContentAlignment.MiddleCenter;
            this.checkBox6.UseVisualStyleBackColor = true;
            this.checkBox6.Click += new EventHandler(this.CheckedChanged);
          
            this.checkBox5.Location = new Point(11, 0x13);
            this.checkBox5.Name = "checkBox5";
            this.checkBox5.Size = new Size(90, 0x17);
            this.checkBox5.TabIndex = 8;
            this.checkBox5.Text = "COPPER";
            this.checkBox5.TextAlign = ContentAlignment.MiddleCenter;
            this.checkBox5.UseVisualStyleBackColor = true;
            this.checkBox5.Click += new EventHandler(this.CheckedChanged);
            this.groupBox3.Controls.Add(this.checkBox19);
            this.groupBox3.Controls.Add(this.checkBox18);
            this.groupBox3.Controls.Add(this.checkBox17);
            this.groupBox3.Controls.Add(this.checkBox16);
            this.groupBox3.Controls.Add(this.checkBox15);
            this.groupBox3.Controls.Add(this.checkBox14);
            this.groupBox3.Location = new Point(9, 300);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new Size(220, 0x79);
            this.groupBox3.TabIndex = 9;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Wheel";
         
            this.checkBox19.Location = new Point(0x72, 0x53);
            this.checkBox19.Name = "checkBox19";
            this.checkBox19.Size = new Size(90, 0x17);
            this.checkBox19.TabIndex = 0x16;
            this.checkBox19.Text = "MACHINE";
            this.checkBox19.TextAlign = ContentAlignment.MiddleCenter;
            this.checkBox19.UseVisualStyleBackColor = true;
            this.checkBox19.Click += new EventHandler(this.CheckedChanged);
          
            this.checkBox18.Location = new Point(12, 0x53);
            this.checkBox18.Name = "checkBox18";
            this.checkBox18.Size = new Size(90, 0x17);
            this.checkBox18.TabIndex = 0x15;
            this.checkBox18.Text = "LARGE DENT";
            this.checkBox18.TextAlign = ContentAlignment.MiddleCenter;
            this.checkBox18.UseVisualStyleBackColor = true;
            this.checkBox18.Click += new EventHandler(this.CheckedChanged);
        
            this.checkBox17.Location = new Point(0x72, 50);
            this.checkBox17.Name = "checkBox17";
            this.checkBox17.Size = new Size(90, 0x17);
            this.checkBox17.TabIndex = 20;
            this.checkBox17.Text = "SMALL DENT";
            this.checkBox17.TextAlign = ContentAlignment.MiddleCenter;
            this.checkBox17.UseVisualStyleBackColor = true;
            this.checkBox17.Click += new EventHandler(this.CheckedChanged);
     
            this.checkBox16.Location = new Point(12, 50);
            this.checkBox16.Name = "checkBox16";
            this.checkBox16.Size = new Size(90, 0x17);
            this.checkBox16.TabIndex = 0x13;
            this.checkBox16.Text = "LARGE CRACK";
            this.checkBox16.TextAlign = ContentAlignment.MiddleCenter;
            this.checkBox16.UseVisualStyleBackColor = true;
            this.checkBox16.Click += new EventHandler(this.CheckedChanged);
     
            this.checkBox15.Location = new Point(0x72, 0x15);
            this.checkBox15.Name = "checkBox15";
            this.checkBox15.Size = new Size(90, 0x17);
            this.checkBox15.TabIndex = 0x12;
            this.checkBox15.Text = "SMALL CRACK";
            this.checkBox15.TextAlign = ContentAlignment.MiddleCenter;
            this.checkBox15.UseVisualStyleBackColor = true;
            this.checkBox15.Click += new EventHandler(this.CheckedChanged);
       
            this.checkBox14.Location = new Point(12, 0x15);
            this.checkBox14.Name = "checkBox14";
            this.checkBox14.Size = new Size(90, 0x17);
            this.checkBox14.TabIndex = 0x11;
            this.checkBox14.Text = "TYRE";
            this.checkBox14.TextAlign = ContentAlignment.MiddleCenter;
            this.checkBox14.UseVisualStyleBackColor = true;
            this.checkBox14.Click += new EventHandler(this.CheckedChanged);
            this.groupBox4.Controls.Add(this.checkBox21);
            this.groupBox4.Controls.Add(this.checkBox20);
            this.groupBox4.Location = new Point(9, 0x1ab);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new Size(220, 0x3a);
            this.groupBox4.TabIndex = 5;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Galv";
         
            this.checkBox21.Location = new Point(0x72, 0x15);
            this.checkBox21.Name = "checkBox21";
            this.checkBox21.Size = new Size(90, 0x17);
            this.checkBox21.TabIndex = 0x11;
            this.checkBox21.Text = "GOLD GALV";
            this.checkBox21.TextAlign = ContentAlignment.MiddleCenter;
            this.checkBox21.UseVisualStyleBackColor = true;
            this.checkBox21.Click += new EventHandler(this.CheckedChanged);
       
            this.checkBox20.Location = new Point(12, 0x15);
            this.checkBox20.Name = "checkBox20";
            this.checkBox20.Size = new Size(90, 0x17);
            this.checkBox20.TabIndex = 0x10;
            this.checkBox20.Text = "SILVER GALV";
            this.checkBox20.TextAlign = ContentAlignment.MiddleCenter;
            this.checkBox20.UseVisualStyleBackColor = true;
            this.checkBox20.Click += new EventHandler(this.CheckedChanged);
            this.groupBox5.Controls.Add(this.checkBox22);
            this.groupBox5.Location = new Point(9, 0x1eb);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new Size(110, 0x3a);
            this.groupBox5.TabIndex = 6;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "Other";

            this.clearButton.Location = new Point(130, 0x1f0);
            this.clearButton.Size = new Size(100, 53);
            this.clearButton.Text = "CLEAR";
            this.clearButton.Click += new EventHandler(this.ClearClicked);
          
            this.checkBox22.Location = new Point(11, 0x13);
            this.checkBox22.Name = "checkBox22";
            this.checkBox22.Size = new Size(90, 0x17);
            this.checkBox22.TabIndex = 0x12;
            this.checkBox22.Text = "OTHER";
            this.checkBox22.TextAlign = ContentAlignment.MiddleCenter;
            this.checkBox22.UseVisualStyleBackColor = true;
            this.checkBox22.Click += new EventHandler(this.CheckedChanged);
            base.AutoScaleDimensions = new SizeF(6f, 13f);
            base.AutoScaleMode = AutoScaleMode.Font;
            base.ClientSize = new Size(0xf1, 0x22b);
            base.Controls.Add(this.groupBox5);
            base.Controls.Add(this.groupBox4);
            base.Controls.Add(this.groupBox3);
            base.Controls.Add(this.groupBox2);
            base.Controls.Add(this.groupBox1);
            base.Controls.Add(this.clearButton);
            this.DoubleBuffered = true;
            base.Location = new Point(300, 100);
            base.MaximizeBox = false;
            base.MinimizeBox = false;
            base.MinimumSize = new Size(0xf1 + 16, 0x22b + 45);
            base.MaximumSize = new Size(0xf1 + 16, 0x22b + 45);
            base.Name = "JobTypePopup";
            base.ShowInTaskbar = false;
            base.StartPosition = FormStartPosition.CenterParent;
            this.Text = "JobTypePopup";
            base.TopMost = true;
            this.groupBox1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox4.ResumeLayout(false);
            this.groupBox5.ResumeLayout(false);
            base.ResumeLayout(false);
            if (!isWheelApp())
            {
                this.groupBox3.Visible = false;
            }
            else
            {
                this.groupBox4.Visible = false;
                this.groupBox2.Visible = false;
                this.checkBox4.Visible = false;
            }
            base.FormClosed += new FormClosedEventHandler(FormClosedEvent);
        }
       
        public static bool isWheelApp()
        {
            string appName = Application.ExecutablePath.ToUpper();
            return appName.Contains("WHEEL");
        }

        private void FormClosedEvent(object sender, FormClosedEventArgs e)
        {
            JobTypePopup.jobType = null;
        }

    }
}

