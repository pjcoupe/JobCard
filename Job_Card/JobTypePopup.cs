﻿namespace Job_Card
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
        public static TextBox jobQty;
        public static TextBox jobUnitPrice;
        public static TextBox jobPrice;
        public static TextBox jobDetail;

        public JobCard jobCard;
        private GroupBox groupBoxRearSkirtDamage;
        private GroupBox groupBox6;
        private Button button11;
        private Button button12;
        private Button button13;
        private Button button14;
        private Button button15;
        private Button button16;
        private Button button17;
        private Button button18;
        private Button button19;
        private Button button20;
        private Button button10;
        private Button button6;
        private Button button7;
        private Button button8;
        private Button button9;
        private Button button5;
        private Button button4;
        private Button button3;
        private Button button2;
        private Button button1;
        private GroupBox groupBox7;
        private GroupBox groupBox8;
        private Button button21;
        private Button button22;
        private Button button23;
        private Button button24;
        private Button button25;
        private Button button26;
        private Button button27;
        private Button button28;
        private Button button29;
        private Button button30;
        private Button button31;
        private Button button32;
        private Button button33;
        private Button button34;
        private Button button35;
        private Button button36;
        private Button button37;
        private Button button38;
        private Button button39;
        private Button button40;
        private Button button42;
        private Button button41;
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
            if (jobQty != null)
            {
                jobQty.Text = "";
            }
            if (jobUnitPrice != null)
            {
                jobUnitPrice.Text = "";
            }
            if (jobPrice != null)
            {
                jobPrice.Text = "";
            }
            if (jobDetail != null)
            {
                jobDetail.Text = "";
            }
        }
        private void CheckedChanged(object sender, EventArgs e)
        {
            if (!this.startup)
            {
                Button box = (Button) sender;
                GroupBox prnt = box.Parent as GroupBox;

                string item = box.Text;
                string unitPrice = "";
                if (!JobTypePopup.isWheelApp())
                {
                    item = new CultureInfo("en-NZ", false).TextInfo.ToTitleCase(box.Text.ToLowerInvariant());
                } else
                {
                    if (prnt.Text.Contains("damage"))
                    {
                        jobDetail.Text = prnt.Text;
                    }
                    int dollarPos = item.IndexOf("$");
                    if (dollarPos > 0)
                    {
                        unitPrice = item.Substring(dollarPos + 1);
                        item = item.Substring(0, dollarPos).Trim();
                        if (!unitPrice.Contains("."))
                        {
                            unitPrice += ".00";
                        }
                    } else
                    {
                        item = item.Trim();
                    }
                }
                Dictionary<string, int> dict = new Dictionary<string, int>();
                List<string> list = jobType.Text.Split(new string[] { ", " }, StringSplitOptions.None).ToList<string>();
                list.Add(item);
                List<string> santisedList = new List<string>();
                if (JobTypePopup.isWheelApp())
                {
                    santisedList.Add(item);
                    short counter = 1;
                    if (Int16.TryParse(JobTypePopup.jobQty.Text, out counter))
                    {
                        counter++;
                    } else
                    {
                        counter = 1;
                        
                    }
                    JobTypePopup.jobQty.Text = counter.ToString();
                    dict[item] = counter;
                    JobTypePopup.jobUnitPrice.Text = unitPrice;
                    JobTypePopup.jobType.Text = item;
                    float unitPriceFloat = 0;
                    float.TryParse(unitPrice, out unitPriceFloat);
                    float price = counter * unitPriceFloat;
                    string priceText = price.ToString();
                    if (!priceText.Contains("."))
                    {
                        priceText += ".00";
                    }
                    JobTypePopup.jobPrice.Text = price.ToString();
                    if (this.jobCard != null && !this.jobCard.IsDisposed)
                    {
                        this.jobCard.UpdateAllTotals();
                    }
                }
                else {
                    foreach (var l in list)
                    {
                        string theItem = l;
                        short counter = 1;
                        if (l.Contains("x)"))
                        {
                            int idx = l.IndexOf('(');
                            if (idx >= 0)
                            {
                                int endIdx = l.IndexOf('x', idx);
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
                        if (!JobTypePopup.isWheelApp())
                        {
                            theItem = new CultureInfo("en-NZ", false).TextInfo.ToTitleCase(theItem);
                        }
                        int alreadyExistingCount = 0;
                        dict.TryGetValue(theItem, out alreadyExistingCount);
                        dict[theItem] = counter + alreadyExistingCount;
                        if (theItem != "" && !santisedList.Contains(theItem))
                        {
                            santisedList.Add(theItem);
                        }
                    }
                }
                
                string str2 = "";
                if (JobTypePopup.isWheelApp() && santisedList.Count > 1)
                {
                    MessageBox.Show("Put each different item on another line please");
                    return;
                }
                if (!JobTypePopup.isWheelApp())
                {
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
                        str2 = str2 + xTimes + str3;
                    }
                    jobType.Text = str2;
                }
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
            this.clearButton = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.checkBox4 = new System.Windows.Forms.Button();
            this.checkBox3 = new System.Windows.Forms.Button();
            this.checkBox2 = new System.Windows.Forms.Button();
            this.checkBox1 = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.checkBox13 = new System.Windows.Forms.Button();
            this.checkBox12 = new System.Windows.Forms.Button();
            this.checkBox11 = new System.Windows.Forms.Button();
            this.checkBox10 = new System.Windows.Forms.Button();
            this.checkBox9 = new System.Windows.Forms.Button();
            this.checkBox8 = new System.Windows.Forms.Button();
            this.checkBox7 = new System.Windows.Forms.Button();
            this.checkBox6 = new System.Windows.Forms.Button();
            this.checkBox5 = new System.Windows.Forms.Button();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.button42 = new System.Windows.Forms.Button();
            this.button41 = new System.Windows.Forms.Button();
            this.groupBox7 = new System.Windows.Forms.GroupBox();
            this.groupBox8 = new System.Windows.Forms.GroupBox();
            this.button21 = new System.Windows.Forms.Button();
            this.button22 = new System.Windows.Forms.Button();
            this.button23 = new System.Windows.Forms.Button();
            this.button24 = new System.Windows.Forms.Button();
            this.button25 = new System.Windows.Forms.Button();
            this.button26 = new System.Windows.Forms.Button();
            this.button27 = new System.Windows.Forms.Button();
            this.button28 = new System.Windows.Forms.Button();
            this.button29 = new System.Windows.Forms.Button();
            this.button30 = new System.Windows.Forms.Button();
            this.button31 = new System.Windows.Forms.Button();
            this.button32 = new System.Windows.Forms.Button();
            this.button33 = new System.Windows.Forms.Button();
            this.button34 = new System.Windows.Forms.Button();
            this.button35 = new System.Windows.Forms.Button();
            this.button36 = new System.Windows.Forms.Button();
            this.button37 = new System.Windows.Forms.Button();
            this.button38 = new System.Windows.Forms.Button();
            this.button39 = new System.Windows.Forms.Button();
            this.button40 = new System.Windows.Forms.Button();
            this.groupBoxRearSkirtDamage = new System.Windows.Forms.GroupBox();
            this.groupBox6 = new System.Windows.Forms.GroupBox();
            this.button11 = new System.Windows.Forms.Button();
            this.button12 = new System.Windows.Forms.Button();
            this.button13 = new System.Windows.Forms.Button();
            this.button14 = new System.Windows.Forms.Button();
            this.button15 = new System.Windows.Forms.Button();
            this.button16 = new System.Windows.Forms.Button();
            this.button17 = new System.Windows.Forms.Button();
            this.button18 = new System.Windows.Forms.Button();
            this.button19 = new System.Windows.Forms.Button();
            this.button20 = new System.Windows.Forms.Button();
            this.button10 = new System.Windows.Forms.Button();
            this.button6 = new System.Windows.Forms.Button();
            this.button7 = new System.Windows.Forms.Button();
            this.button8 = new System.Windows.Forms.Button();
            this.button9 = new System.Windows.Forms.Button();
            this.button5 = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.checkBox21 = new System.Windows.Forms.Button();
            this.checkBox20 = new System.Windows.Forms.Button();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.checkBox22 = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox7.SuspendLayout();
            this.groupBox8.SuspendLayout();
            this.groupBoxRearSkirtDamage.SuspendLayout();
            this.groupBox6.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.groupBox5.SuspendLayout();
            this.SuspendLayout();
            // 
            // clearButton
            // 
            this.clearButton.Location = new System.Drawing.Point(9, 579);
            this.clearButton.Name = "clearButton";
            this.clearButton.Size = new System.Drawing.Size(363, 25);
            this.clearButton.TabIndex = 10;
            this.clearButton.Text = "CLEAR";
            this.clearButton.Click += new System.EventHandler(this.ClearClicked);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.checkBox4);
            this.groupBox1.Controls.Add(this.checkBox3);
            this.groupBox1.Controls.Add(this.checkBox2);
            this.groupBox1.Controls.Add(this.checkBox1);
            this.groupBox1.Location = new System.Drawing.Point(9, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(363, 55);
            this.groupBox1.TabIndex = 4;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Repair and Finishing";
            // 
            // checkBox4
            // 
            this.checkBox4.Location = new System.Drawing.Point(281, 21);
            this.checkBox4.Name = "checkBox4";
            this.checkBox4.Size = new System.Drawing.Size(71, 23);
            this.checkBox4.TabIndex = 7;
            this.checkBox4.Text = "LAQUER";
            this.checkBox4.UseVisualStyleBackColor = true;
            this.checkBox4.Click += new System.EventHandler(this.CheckedChanged);
            // 
            // checkBox3
            // 
            this.checkBox3.Location = new System.Drawing.Point(191, 21);
            this.checkBox3.Name = "checkBox3";
            this.checkBox3.Size = new System.Drawing.Size(68, 23);
            this.checkBox3.TabIndex = 6;
            this.checkBox3.Text = "POLISH";
            this.checkBox3.UseVisualStyleBackColor = true;
            this.checkBox3.Click += new System.EventHandler(this.CheckedChanged);
            // 
            // checkBox2
            // 
            this.checkBox2.Location = new System.Drawing.Point(104, 21);
            this.checkBox2.Name = "checkBox2";
            this.checkBox2.Size = new System.Drawing.Size(67, 23);
            this.checkBox2.TabIndex = 5;
            this.checkBox2.Text = "REPAIR";
            this.checkBox2.UseVisualStyleBackColor = true;
            this.checkBox2.Click += new System.EventHandler(this.CheckedChanged);
            // 
            // checkBox1
            // 
            this.checkBox1.Location = new System.Drawing.Point(11, 21);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(73, 23);
            this.checkBox1.TabIndex = 4;
            this.checkBox1.Text = "STRIP";
            this.checkBox1.UseVisualStyleBackColor = true;
            this.checkBox1.Click += new System.EventHandler(this.CheckedChanged);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.checkBox13);
            this.groupBox2.Controls.Add(this.checkBox12);
            this.groupBox2.Controls.Add(this.checkBox11);
            this.groupBox2.Controls.Add(this.checkBox10);
            this.groupBox2.Controls.Add(this.checkBox9);
            this.groupBox2.Controls.Add(this.checkBox8);
            this.groupBox2.Controls.Add(this.checkBox7);
            this.groupBox2.Controls.Add(this.checkBox6);
            this.groupBox2.Controls.Add(this.checkBox5);
            this.groupBox2.Location = new System.Drawing.Point(9, 73);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(363, 85);
            this.groupBox2.TabIndex = 5;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Plating";
            // 
            // checkBox13
            // 
            this.checkBox13.Location = new System.Drawing.Point(305, 48);
            this.checkBox13.Name = "checkBox13";
            this.checkBox13.Size = new System.Drawing.Size(48, 23);
            this.checkBox13.TabIndex = 16;
            this.checkBox13.Text = "GOLD";
            this.checkBox13.UseVisualStyleBackColor = true;
            this.checkBox13.Click += new System.EventHandler(this.CheckedChanged);
            // 
            // checkBox12
            // 
            this.checkBox12.Location = new System.Drawing.Point(224, 48);
            this.checkBox12.Name = "checkBox12";
            this.checkBox12.Size = new System.Drawing.Size(63, 23);
            this.checkBox12.TabIndex = 15;
            this.checkBox12.Text = "SILVER";
            this.checkBox12.UseVisualStyleBackColor = true;
            this.checkBox12.Click += new System.EventHandler(this.CheckedChanged);
            // 
            // checkBox11
            // 
            this.checkBox11.Location = new System.Drawing.Point(159, 48);
            this.checkBox11.Name = "checkBox11";
            this.checkBox11.Size = new System.Drawing.Size(48, 23);
            this.checkBox11.TabIndex = 14;
            this.checkBox11.Text = "TIN";
            this.checkBox11.UseVisualStyleBackColor = true;
            this.checkBox11.Click += new System.EventHandler(this.CheckedChanged);
            // 
            // checkBox10
            // 
            this.checkBox10.Location = new System.Drawing.Point(95, 48);
            this.checkBox10.Name = "checkBox10";
            this.checkBox10.Size = new System.Drawing.Size(52, 23);
            this.checkBox10.TabIndex = 13;
            this.checkBox10.Text = "SATIN";
            this.checkBox10.UseVisualStyleBackColor = true;
            this.checkBox10.Click += new System.EventHandler(this.CheckedChanged);
            // 
            // checkBox9
            // 
            this.checkBox9.Location = new System.Drawing.Point(11, 48);
            this.checkBox9.Name = "checkBox9";
            this.checkBox9.Size = new System.Drawing.Size(72, 23);
            this.checkBox9.TabIndex = 12;
            this.checkBox9.Text = "BRONZE";
            this.checkBox9.UseVisualStyleBackColor = true;
            this.checkBox9.Click += new System.EventHandler(this.CheckedChanged);
            // 
            // checkBox8
            // 
            this.checkBox8.Location = new System.Drawing.Point(282, 19);
            this.checkBox8.Name = "checkBox8";
            this.checkBox8.Size = new System.Drawing.Size(71, 23);
            this.checkBox8.TabIndex = 11;
            this.checkBox8.Text = "BRASS";
            this.checkBox8.UseVisualStyleBackColor = true;
            this.checkBox8.Click += new System.EventHandler(this.CheckedChanged);
            // 
            // checkBox7
            // 
            this.checkBox7.Location = new System.Drawing.Point(193, 19);
            this.checkBox7.Name = "checkBox7";
            this.checkBox7.Size = new System.Drawing.Size(68, 23);
            this.checkBox7.TabIndex = 10;
            this.checkBox7.Text = "CHROME";
            this.checkBox7.UseVisualStyleBackColor = true;
            this.checkBox7.Click += new System.EventHandler(this.CheckedChanged);
            // 
            // checkBox6
            // 
            this.checkBox6.Location = new System.Drawing.Point(104, 19);
            this.checkBox6.Name = "checkBox6";
            this.checkBox6.Size = new System.Drawing.Size(67, 23);
            this.checkBox6.TabIndex = 9;
            this.checkBox6.Text = "NICKLE";
            this.checkBox6.UseVisualStyleBackColor = true;
            this.checkBox6.Click += new System.EventHandler(this.CheckedChanged);
            // 
            // checkBox5
            // 
            this.checkBox5.Location = new System.Drawing.Point(11, 19);
            this.checkBox5.Name = "checkBox5";
            this.checkBox5.Size = new System.Drawing.Size(73, 23);
            this.checkBox5.TabIndex = 8;
            this.checkBox5.Text = "COPPER";
            this.checkBox5.UseVisualStyleBackColor = true;
            this.checkBox5.Click += new System.EventHandler(this.CheckedChanged);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.button42);
            this.groupBox3.Controls.Add(this.button41);
            this.groupBox3.Controls.Add(this.groupBox7);
            this.groupBox3.Controls.Add(this.groupBoxRearSkirtDamage);
            this.groupBox3.Location = new System.Drawing.Point(9, 159);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(363, 344);
            this.groupBox3.TabIndex = 9;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Wheel";
            // 
            // button42
            // 
            this.button42.Location = new System.Drawing.Point(171, 316);
            this.button42.Name = "button42";
            this.button42.Size = new System.Drawing.Size(116, 23);
            this.button42.TabIndex = 29;
            this.button42.Text = "Machining tyre bead    $12";
            this.button42.UseVisualStyleBackColor = true;
            this.button42.Click += new System.EventHandler(this.CheckedChanged);
            // 
            // button41
            // 
            this.button41.Location = new System.Drawing.Point(69, 316);
            this.button41.Name = "button41";
            this.button41.Size = new System.Drawing.Size(96, 23);
            this.button41.TabIndex = 28;
            this.button41.Text = "Remove curbing    $35";
            this.button41.UseVisualStyleBackColor = true;
            this.button41.Click += new System.EventHandler(this.CheckedChanged);
            // 
            // groupBox7
            // 
            this.groupBox7.Controls.Add(this.groupBox8);
            this.groupBox7.Controls.Add(this.button31);
            this.groupBox7.Controls.Add(this.button32);
            this.groupBox7.Controls.Add(this.button33);
            this.groupBox7.Controls.Add(this.button34);
            this.groupBox7.Controls.Add(this.button35);
            this.groupBox7.Controls.Add(this.button36);
            this.groupBox7.Controls.Add(this.button37);
            this.groupBox7.Controls.Add(this.button38);
            this.groupBox7.Controls.Add(this.button39);
            this.groupBox7.Controls.Add(this.button40);
            this.groupBox7.Location = new System.Drawing.Point(4, 163);
            this.groupBox7.Name = "groupBox7";
            this.groupBox7.Size = new System.Drawing.Size(354, 151);
            this.groupBox7.TabIndex = 24;
            this.groupBox7.TabStop = false;
            this.groupBox7.Text = "Wheel front skirt damage repair";
            this.groupBox7.Enter += new System.EventHandler(this.groupBox7_Enter);
            // 
            // groupBox8
            // 
            this.groupBox8.Controls.Add(this.button21);
            this.groupBox8.Controls.Add(this.button22);
            this.groupBox8.Controls.Add(this.button23);
            this.groupBox8.Controls.Add(this.button24);
            this.groupBox8.Controls.Add(this.button25);
            this.groupBox8.Controls.Add(this.button26);
            this.groupBox8.Controls.Add(this.button27);
            this.groupBox8.Controls.Add(this.button28);
            this.groupBox8.Controls.Add(this.button29);
            this.groupBox8.Controls.Add(this.button30);
            this.groupBox8.Location = new System.Drawing.Point(3, 164);
            this.groupBox8.Name = "groupBox8";
            this.groupBox8.Size = new System.Drawing.Size(354, 165);
            this.groupBox8.TabIndex = 24;
            this.groupBox8.TabStop = false;
            this.groupBox8.Text = "REAR SKIRT DAMAGE";
            // 
            // button21
            // 
            this.button21.Location = new System.Drawing.Point(262, 136);
            this.button21.Name = "button21";
            this.button21.Size = new System.Drawing.Size(86, 23);
            this.button21.TabIndex = 27;
            this.button21.Text = "Crack 31mm >   ";
            this.button21.UseVisualStyleBackColor = true;
            // 
            // button22
            // 
            this.button22.Location = new System.Drawing.Point(262, 106);
            this.button22.Name = "button22";
            this.button22.Size = new System.Drawing.Size(86, 23);
            this.button22.TabIndex = 26;
            this.button22.Text = "Crack 21-30mm  $32";
            this.button22.UseVisualStyleBackColor = true;
            // 
            // button23
            // 
            this.button23.Location = new System.Drawing.Point(262, 77);
            this.button23.Name = "button23";
            this.button23.Size = new System.Drawing.Size(87, 23);
            this.button23.TabIndex = 25;
            this.button23.Text = "Crack 16-20mm   $26";
            this.button23.UseVisualStyleBackColor = true;
            // 
            // button24
            // 
            this.button24.Location = new System.Drawing.Point(262, 19);
            this.button24.Name = "button24";
            this.button24.Size = new System.Drawing.Size(86, 23);
            this.button24.TabIndex = 24;
            this.button24.Text = "Crack 11-15mm   $22";
            this.button24.UseVisualStyleBackColor = true;
            // 
            // button25
            // 
            this.button25.Location = new System.Drawing.Point(262, 48);
            this.button25.Name = "button25";
            this.button25.Size = new System.Drawing.Size(86, 23);
            this.button25.TabIndex = 23;
            this.button25.Text = "Crack 1-10mm   $18";
            this.button25.UseVisualStyleBackColor = true;
            // 
            // button26
            // 
            this.button26.Location = new System.Drawing.Point(4, 135);
            this.button26.Name = "button26";
            this.button26.Size = new System.Drawing.Size(97, 23);
            this.button26.TabIndex = 22;
            this.button26.Text = "Dent 16mm >   ";
            this.button26.UseVisualStyleBackColor = true;
            // 
            // button27
            // 
            this.button27.Location = new System.Drawing.Point(4, 106);
            this.button27.Name = "button27";
            this.button27.Size = new System.Drawing.Size(97, 23);
            this.button27.TabIndex = 21;
            this.button27.Text = "Dent 11-15mm   $46";
            this.button27.UseVisualStyleBackColor = true;
            // 
            // button28
            // 
            this.button28.Location = new System.Drawing.Point(4, 77);
            this.button28.Name = "button28";
            this.button28.Size = new System.Drawing.Size(97, 23);
            this.button28.TabIndex = 20;
            this.button28.Text = "Dent 6-10mm   $38";
            this.button28.UseVisualStyleBackColor = true;
            // 
            // button29
            // 
            this.button29.Location = new System.Drawing.Point(4, 48);
            this.button29.Name = "button29";
            this.button29.Size = new System.Drawing.Size(97, 23);
            this.button29.TabIndex = 19;
            this.button29.Text = "Dent 1-5mm   $26";
            this.button29.UseVisualStyleBackColor = true;
            // 
            // button30
            // 
            this.button30.Location = new System.Drawing.Point(6, 19);
            this.button30.Name = "button30";
            this.button30.Size = new System.Drawing.Size(201, 23);
            this.button30.TabIndex = 18;
            this.button30.Text = "Set up on lathe   $35";
            this.button30.UseVisualStyleBackColor = true;
            // 
            // button31
            // 
            this.button31.Location = new System.Drawing.Point(244, 123);
            this.button31.Name = "button31";
            this.button31.Size = new System.Drawing.Size(104, 23);
            this.button31.TabIndex = 27;
            this.button31.Text = "Crack 31mm >   ";
            this.button31.UseVisualStyleBackColor = true;
            this.button31.Click += new System.EventHandler(this.CheckedChanged);
            // 
            // button32
            // 
            this.button32.Location = new System.Drawing.Point(244, 97);
            this.button32.Name = "button32";
            this.button32.Size = new System.Drawing.Size(104, 23);
            this.button32.TabIndex = 26;
            this.button32.Text = "Crack 21-30mm  $46";
            this.button32.UseVisualStyleBackColor = true;
            this.button32.Click += new System.EventHandler(this.CheckedChanged);
            // 
            // button33
            // 
            this.button33.Location = new System.Drawing.Point(244, 71);
            this.button33.Name = "button33";
            this.button33.Size = new System.Drawing.Size(105, 23);
            this.button33.TabIndex = 25;
            this.button33.Text = "Crack 16-20mm   $34";
            this.button33.UseVisualStyleBackColor = true;
            this.button33.Click += new System.EventHandler(this.CheckedChanged);
            // 
            // button34
            // 
            this.button34.Location = new System.Drawing.Point(244, 45);
            this.button34.Name = "button34";
            this.button34.Size = new System.Drawing.Size(104, 23);
            this.button34.TabIndex = 24;
            this.button34.Text = "Crack 11-15mm   $28";
            this.button34.UseVisualStyleBackColor = true;
            this.button34.Click += new System.EventHandler(this.CheckedChanged);
            // 
            // button35
            // 
            this.button35.Location = new System.Drawing.Point(244, 20);
            this.button35.Name = "button35";
            this.button35.Size = new System.Drawing.Size(104, 23);
            this.button35.TabIndex = 23;
            this.button35.Text = "Crack 1-10mm   $22";
            this.button35.UseVisualStyleBackColor = true;
            this.button35.Click += new System.EventHandler(this.CheckedChanged);
            // 
            // button36
            // 
            this.button36.Location = new System.Drawing.Point(6, 123);
            this.button36.Name = "button36";
            this.button36.Size = new System.Drawing.Size(97, 23);
            this.button36.TabIndex = 22;
            this.button36.Text = "Dent 16mm >   ";
            this.button36.UseVisualStyleBackColor = true;
            this.button36.Click += new System.EventHandler(this.CheckedChanged);
            // 
            // button37
            // 
            this.button37.Location = new System.Drawing.Point(6, 97);
            this.button37.Name = "button37";
            this.button37.Size = new System.Drawing.Size(97, 23);
            this.button37.TabIndex = 21;
            this.button37.Text = "Dent 11-15mm   $58";
            this.button37.UseVisualStyleBackColor = true;
            this.button37.Click += new System.EventHandler(this.CheckedChanged);
            // 
            // button38
            // 
            this.button38.Location = new System.Drawing.Point(6, 71);
            this.button38.Name = "button38";
            this.button38.Size = new System.Drawing.Size(97, 23);
            this.button38.TabIndex = 20;
            this.button38.Text = "Dent 6-10mm   $46";
            this.button38.UseVisualStyleBackColor = true;
            this.button38.Click += new System.EventHandler(this.CheckedChanged);
            // 
            // button39
            // 
            this.button39.Location = new System.Drawing.Point(6, 45);
            this.button39.Name = "button39";
            this.button39.Size = new System.Drawing.Size(97, 23);
            this.button39.TabIndex = 19;
            this.button39.Text = "Dent 1-5mm   $35";
            this.button39.UseVisualStyleBackColor = true;
            this.button39.Click += new System.EventHandler(this.CheckedChanged);
            // 
            // button40
            // 
            this.button40.Location = new System.Drawing.Point(6, 19);
            this.button40.Name = "button40";
            this.button40.Size = new System.Drawing.Size(96, 23);
            this.button40.TabIndex = 18;
            this.button40.Text = "Set up on lathe   $35";
            this.button40.UseVisualStyleBackColor = true;
            // 
            // groupBoxRearSkirtDamage
            // 
            this.groupBoxRearSkirtDamage.Controls.Add(this.groupBox6);
            this.groupBoxRearSkirtDamage.Controls.Add(this.button10);
            this.groupBoxRearSkirtDamage.Controls.Add(this.button6);
            this.groupBoxRearSkirtDamage.Controls.Add(this.button7);
            this.groupBoxRearSkirtDamage.Controls.Add(this.button8);
            this.groupBoxRearSkirtDamage.Controls.Add(this.button9);
            this.groupBoxRearSkirtDamage.Controls.Add(this.button5);
            this.groupBoxRearSkirtDamage.Controls.Add(this.button4);
            this.groupBoxRearSkirtDamage.Controls.Add(this.button3);
            this.groupBoxRearSkirtDamage.Controls.Add(this.button2);
            this.groupBoxRearSkirtDamage.Controls.Add(this.button1);
            this.groupBoxRearSkirtDamage.Location = new System.Drawing.Point(3, 10);
            this.groupBoxRearSkirtDamage.Name = "groupBoxRearSkirtDamage";
            this.groupBoxRearSkirtDamage.Size = new System.Drawing.Size(354, 151);
            this.groupBoxRearSkirtDamage.TabIndex = 23;
            this.groupBoxRearSkirtDamage.TabStop = false;
            this.groupBoxRearSkirtDamage.Text = "Wheel rear skirt damage repair";
            // 
            // groupBox6
            // 
            this.groupBox6.Controls.Add(this.button11);
            this.groupBox6.Controls.Add(this.button12);
            this.groupBox6.Controls.Add(this.button13);
            this.groupBox6.Controls.Add(this.button14);
            this.groupBox6.Controls.Add(this.button15);
            this.groupBox6.Controls.Add(this.button16);
            this.groupBox6.Controls.Add(this.button17);
            this.groupBox6.Controls.Add(this.button18);
            this.groupBox6.Controls.Add(this.button19);
            this.groupBox6.Controls.Add(this.button20);
            this.groupBox6.Location = new System.Drawing.Point(3, 164);
            this.groupBox6.Name = "groupBox6";
            this.groupBox6.Size = new System.Drawing.Size(354, 165);
            this.groupBox6.TabIndex = 24;
            this.groupBox6.TabStop = false;
            this.groupBox6.Text = "REAR SKIRT DAMAGE";
            // 
            // button11
            // 
            this.button11.Location = new System.Drawing.Point(262, 136);
            this.button11.Name = "button11";
            this.button11.Size = new System.Drawing.Size(86, 23);
            this.button11.TabIndex = 27;
            this.button11.Text = "Crack 31mm >   ";
            this.button11.UseVisualStyleBackColor = true;
            // 
            // button12
            // 
            this.button12.Location = new System.Drawing.Point(262, 106);
            this.button12.Name = "button12";
            this.button12.Size = new System.Drawing.Size(86, 23);
            this.button12.TabIndex = 26;
            this.button12.Text = "Crack 21-30mm  $32";
            this.button12.UseVisualStyleBackColor = true;
            // 
            // button13
            // 
            this.button13.Location = new System.Drawing.Point(262, 77);
            this.button13.Name = "button13";
            this.button13.Size = new System.Drawing.Size(87, 23);
            this.button13.TabIndex = 25;
            this.button13.Text = "Crack 16-20mm   $26";
            this.button13.UseVisualStyleBackColor = true;
            // 
            // button14
            // 
            this.button14.Location = new System.Drawing.Point(262, 19);
            this.button14.Name = "button14";
            this.button14.Size = new System.Drawing.Size(86, 23);
            this.button14.TabIndex = 24;
            this.button14.Text = "Crack 11-15mm   $22";
            this.button14.UseVisualStyleBackColor = true;
            // 
            // button15
            // 
            this.button15.Location = new System.Drawing.Point(262, 48);
            this.button15.Name = "button15";
            this.button15.Size = new System.Drawing.Size(86, 23);
            this.button15.TabIndex = 23;
            this.button15.Text = "Crack 1-10mm   $18";
            this.button15.UseVisualStyleBackColor = true;
            // 
            // button16
            // 
            this.button16.Location = new System.Drawing.Point(4, 135);
            this.button16.Name = "button16";
            this.button16.Size = new System.Drawing.Size(97, 23);
            this.button16.TabIndex = 22;
            this.button16.Text = "Dent 16mm >   ";
            this.button16.UseVisualStyleBackColor = true;
            // 
            // button17
            // 
            this.button17.Location = new System.Drawing.Point(4, 106);
            this.button17.Name = "button17";
            this.button17.Size = new System.Drawing.Size(97, 23);
            this.button17.TabIndex = 21;
            this.button17.Text = "Dent 11-15mm   $46";
            this.button17.UseVisualStyleBackColor = true;
            // 
            // button18
            // 
            this.button18.Location = new System.Drawing.Point(4, 77);
            this.button18.Name = "button18";
            this.button18.Size = new System.Drawing.Size(97, 23);
            this.button18.TabIndex = 20;
            this.button18.Text = "Dent 6-10mm   $38";
            this.button18.UseVisualStyleBackColor = true;
            // 
            // button19
            // 
            this.button19.Location = new System.Drawing.Point(4, 48);
            this.button19.Name = "button19";
            this.button19.Size = new System.Drawing.Size(97, 23);
            this.button19.TabIndex = 19;
            this.button19.Text = "Dent 1-5mm   $26";
            this.button19.UseVisualStyleBackColor = true;
            // 
            // button20
            // 
            this.button20.Location = new System.Drawing.Point(6, 19);
            this.button20.Name = "button20";
            this.button20.Size = new System.Drawing.Size(201, 23);
            this.button20.TabIndex = 18;
            this.button20.Text = "Set up on lathe   $35";
            this.button20.UseVisualStyleBackColor = true;
            // 
            // button10
            // 
            this.button10.Location = new System.Drawing.Point(245, 123);
            this.button10.Name = "button10";
            this.button10.Size = new System.Drawing.Size(103, 23);
            this.button10.TabIndex = 27;
            this.button10.Text = "Crack 31mm >   ";
            this.button10.UseVisualStyleBackColor = true;
            this.button10.Click += new System.EventHandler(this.CheckedChanged);
            // 
            // button6
            // 
            this.button6.Location = new System.Drawing.Point(245, 97);
            this.button6.Name = "button6";
            this.button6.Size = new System.Drawing.Size(103, 23);
            this.button6.TabIndex = 26;
            this.button6.Text = "Crack 21-30mm  $32";
            this.button6.UseVisualStyleBackColor = true;
            this.button6.Click += new System.EventHandler(this.CheckedChanged);
            // 
            // button7
            // 
            this.button7.Location = new System.Drawing.Point(245, 71);
            this.button7.Name = "button7";
            this.button7.Size = new System.Drawing.Size(104, 23);
            this.button7.TabIndex = 25;
            this.button7.Text = "Crack 16-20mm   $26";
            this.button7.UseVisualStyleBackColor = true;
            this.button7.Click += new System.EventHandler(this.CheckedChanged);
            // 
            // button8
            // 
            this.button8.Location = new System.Drawing.Point(245, 45);
            this.button8.Name = "button8";
            this.button8.Size = new System.Drawing.Size(104, 23);
            this.button8.TabIndex = 24;
            this.button8.Text = "Crack 11-15mm   $22";
            this.button8.UseVisualStyleBackColor = true;
            this.button8.Click += new System.EventHandler(this.CheckedChanged);
            // 
            // button9
            // 
            this.button9.Location = new System.Drawing.Point(245, 19);
            this.button9.Name = "button9";
            this.button9.Size = new System.Drawing.Size(104, 23);
            this.button9.TabIndex = 23;
            this.button9.Text = "Crack 1-10mm   $18";
            this.button9.UseVisualStyleBackColor = true;
            this.button9.Click += new System.EventHandler(this.CheckedChanged);
            // 
            // button5
            // 
            this.button5.Location = new System.Drawing.Point(6, 123);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(97, 23);
            this.button5.TabIndex = 22;
            this.button5.Text = "Dent 16mm >   ";
            this.button5.UseVisualStyleBackColor = true;
            this.button5.Click += new System.EventHandler(this.CheckedChanged);
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(6, 97);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(97, 23);
            this.button4.TabIndex = 21;
            this.button4.Text = "Dent 11-15mm   $46";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.CheckedChanged);
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(6, 71);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(97, 23);
            this.button3.TabIndex = 20;
            this.button3.Text = "Dent 6-10mm   $38";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.CheckedChanged);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(6, 45);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(97, 23);
            this.button2.TabIndex = 19;
            this.button2.Text = "Dent 1-5mm   $26";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.CheckedChanged);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(6, 19);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(97, 23);
            this.button1.TabIndex = 18;
            this.button1.Text = "Set up on lathe   $35";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.CheckedChanged);
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.checkBox21);
            this.groupBox4.Controls.Add(this.checkBox20);
            this.groupBox4.Location = new System.Drawing.Point(12, 503);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(207, 70);
            this.groupBox4.TabIndex = 5;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Galv";
            // 
            // checkBox21
            // 
            this.checkBox21.Location = new System.Drawing.Point(114, 21);
            this.checkBox21.Name = "checkBox21";
            this.checkBox21.Size = new System.Drawing.Size(82, 23);
            this.checkBox21.TabIndex = 17;
            this.checkBox21.Text = "GOLD GALV";
            this.checkBox21.UseVisualStyleBackColor = true;
            this.checkBox21.Click += new System.EventHandler(this.CheckedChanged);
            // 
            // checkBox20
            // 
            this.checkBox20.Location = new System.Drawing.Point(12, 21);
            this.checkBox20.Name = "checkBox20";
            this.checkBox20.Size = new System.Drawing.Size(89, 23);
            this.checkBox20.TabIndex = 16;
            this.checkBox20.Text = "SILVER GALV";
            this.checkBox20.UseVisualStyleBackColor = true;
            this.checkBox20.Click += new System.EventHandler(this.CheckedChanged);
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.checkBox22);
            this.groupBox5.Location = new System.Drawing.Point(233, 503);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(139, 70);
            this.groupBox5.TabIndex = 6;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "Other";
            // 
            // checkBox22
            // 
            this.checkBox22.Location = new System.Drawing.Point(24, 19);
            this.checkBox22.Name = "checkBox22";
            this.checkBox22.Size = new System.Drawing.Size(90, 23);
            this.checkBox22.TabIndex = 18;
            this.checkBox22.Text = "OTHER";
            this.checkBox22.UseVisualStyleBackColor = true;
            this.checkBox22.Click += new System.EventHandler(this.CheckedChanged);
            // 
            // JobTypePopup
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(384, 616);
            this.Controls.Add(this.groupBox5);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.clearButton);
            this.DoubleBuffered = true;
            this.Location = new System.Drawing.Point(20, 20);
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(400, 655);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(400, 655);
            this.Name = "JobTypePopup";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "JobTypePopup";
            this.TopMost = true;
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FormClosedEvent);
            this.Shown += new System.EventHandler(this.Form_Shown);
            this.groupBox1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox7.ResumeLayout(false);
            this.groupBox8.ResumeLayout(false);
            this.groupBoxRearSkirtDamage.ResumeLayout(false);
            this.groupBox6.ResumeLayout(false);
            this.groupBox4.ResumeLayout(false);
            this.groupBox5.ResumeLayout(false);
            this.ResumeLayout(false);

        }
       
        private void Form_Shown(object sender, EventArgs e)
        {
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
        }
        public static bool isWheelApp()
        {
            string appName = Application.ExecutablePath.ToUpper();
            return appName.Contains("WHEEL");
        }

        private void FormClosedEvent(object sender, FormClosedEventArgs e)
        {
            JobTypePopup.jobType = null;
            JobTypePopup.jobQty = null;
            JobTypePopup.jobUnitPrice = null;
            JobTypePopup.jobPrice = null;
            JobTypePopup.jobDetail = null;


        }

        private void groupBox7_Enter(object sender, EventArgs e)
        {

        }
    }
}

