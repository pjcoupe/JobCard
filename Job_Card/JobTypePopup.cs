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
       
        private Button button31;
        private Button button32;
        private Button button33;
        private Button button34;
        private Button button35;
        private Button button36;
        private Button button37;
        private Button button38;
        private Button button39;
        private Button button42;
        private Button button41;
        private Button button40;
        private Button button45;
        private Button button44;
        private Button button43;
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

                        }
                    }
                }
            }
            this.startup = false;
        }

        public async void SetupPricingFromDB()
        {
            System.Console.WriteLine("Setting up pricing");
            SettingsSettingsDoc settings = await DataAccess.findSettings();
            if (settings.pricing.IsBsonDocument)
            {
                foreach (object obj2 in base.Controls)
                {
                    string rootName = (obj2 as Control).Name;
                    System.Console.WriteLine("Control: " + rootName);
                    if (obj2 is GroupBox)
                    {
                        GroupBox box = (GroupBox)obj2;
                        foreach (object obj3 in box.Controls)
                        {
                            if (obj3 is GroupBox)
                            {
                                GroupBox box2 = (GroupBox)obj3;
                                foreach (object obj4 in box2.Controls)
                                {
                                    string name = (obj4 as Control).Name;
                                    string value = (string)settings.pricing.GetValue(name, null);

                                    if (value != null)
                                    {
                                        (obj4 as Control).Text = value;
                                        System.Console.WriteLine("Debug setting " + name + " value " + value);
                                    }
                                }
                            } else {
                                string name = (obj3 as Control).Name;
                                string value = (string)settings.pricing.GetValue(name, null);

                                if (value != null)
                                {
                                    (obj3 as Control).Text = value;
                                    System.Console.WriteLine("Debug setting " + name + " value " + value);
                                }
                            }
                        }
                    }
                }
            }
            System.Console.WriteLine("Setting up pricing COMPLETE");
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
                    if (prnt.Text.Contains("damage") || prnt.Text.Contains("Wheel"))
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
            this.button40 = new System.Windows.Forms.Button();
            this.button42 = new System.Windows.Forms.Button();
            this.button41 = new System.Windows.Forms.Button();
            this.groupBox7 = new System.Windows.Forms.GroupBox();

            this.button31 = new System.Windows.Forms.Button();
            this.button32 = new System.Windows.Forms.Button();
            this.button33 = new System.Windows.Forms.Button();
            this.button34 = new System.Windows.Forms.Button();
            this.button35 = new System.Windows.Forms.Button();
            this.button36 = new System.Windows.Forms.Button();
            this.button37 = new System.Windows.Forms.Button();
            this.button38 = new System.Windows.Forms.Button();
            this.button39 = new System.Windows.Forms.Button();
            this.groupBoxRearSkirtDamage = new System.Windows.Forms.GroupBox();

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
            this.button43 = new System.Windows.Forms.Button();
            this.button44 = new System.Windows.Forms.Button();
            this.button45 = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox7.SuspendLayout();
            //this.groupBox8.SuspendLayout();
            this.groupBoxRearSkirtDamage.SuspendLayout();
            //this.groupBox6.SuspendLayout();
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
            this.groupBox3.Controls.Add(this.button45);
            this.groupBox3.Controls.Add(this.button44);
            this.groupBox3.Controls.Add(this.button43);
            this.groupBox3.Controls.Add(this.button40);
            this.groupBox3.Controls.Add(this.button42);
            this.groupBox3.Controls.Add(this.button41);
            this.groupBox3.Controls.Add(this.groupBox7);
            this.groupBox3.Controls.Add(this.groupBoxRearSkirtDamage);
            this.groupBox3.Controls.Add(this.button1);
            this.groupBox3.Location = new System.Drawing.Point(9, 159);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(363, 344);
            this.groupBox3.TabIndex = 9;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Wheel repair";
            // 
            // button40
            // 
            this.button40.Location = new System.Drawing.Point(310, 16);
            this.button40.Name = "button40";
            this.button40.Size = new System.Drawing.Size(50, 35);
            this.button40.TabIndex = 30;
            this.button40.Text = "Other weld";
            this.button40.UseVisualStyleBackColor = true;
            this.button40.Click += new System.EventHandler(this.CheckedChanged);
            // 
            // button42
            // 
            this.button42.Location = new System.Drawing.Point(197, 16);
            this.button42.Name = "button42";
            this.button42.Size = new System.Drawing.Size(111, 23);
            this.button42.TabIndex = 29;
            this.button42.Text = "Machining tyre bead    $12";
            this.button42.UseVisualStyleBackColor = true;
            this.button42.Click += new System.EventHandler(this.CheckedChanged);
            // 
            // button41
            // 
            this.button41.Location = new System.Drawing.Point(100, 16);
            this.button41.Name = "button41";
            this.button41.Size = new System.Drawing.Size(93, 23);
            this.button41.TabIndex = 28;
            this.button41.Text = "Remove curbing    $35";
            this.button41.UseVisualStyleBackColor = true;
            this.button41.Click += new System.EventHandler(this.CheckedChanged);
            // 
            // groupBox7
            // 
           // this.groupBox7.Controls.Add(this.groupBox8);
            this.groupBox7.Controls.Add(this.button31);
            this.groupBox7.Controls.Add(this.button32);
            this.groupBox7.Controls.Add(this.button33);
            this.groupBox7.Controls.Add(this.button34);
            this.groupBox7.Controls.Add(this.button35);
            this.groupBox7.Controls.Add(this.button36);
            this.groupBox7.Controls.Add(this.button37);
            this.groupBox7.Controls.Add(this.button38);
            this.groupBox7.Controls.Add(this.button39);
            this.groupBox7.Location = new System.Drawing.Point(9, 203);
            this.groupBox7.Name = "groupBox7";
            this.groupBox7.Size = new System.Drawing.Size(354, 125);
            this.groupBox7.TabIndex = 24;
            this.groupBox7.TabStop = false;
            this.groupBox7.Text = "Front skirt damage repair";
            this.groupBox7.Enter += new System.EventHandler(this.groupBox7_Enter);
           
            // 
            // button31
            // 
            this.button31.Location = new System.Drawing.Point(243, 97);
            this.button31.Name = "button31";
            this.button31.Size = new System.Drawing.Size(104, 23);
            this.button31.TabIndex = 27;
            this.button31.Text = "Crack 31mm >   ";
            this.button31.UseVisualStyleBackColor = true;
            this.button31.Click += new System.EventHandler(this.CheckedChanged);
            // 
            // button32
            // 
            this.button32.Location = new System.Drawing.Point(243, 71);
            this.button32.Name = "button32";
            this.button32.Size = new System.Drawing.Size(104, 23);
            this.button32.TabIndex = 26;
            this.button32.Text = "Crack 21-30mm  $72";
            this.button32.UseVisualStyleBackColor = true;
            this.button32.Click += new System.EventHandler(this.CheckedChanged);
            // 
            // button33
            // 
            this.button33.Location = new System.Drawing.Point(243, 45);
            this.button33.Name = "button33";
            this.button33.Size = new System.Drawing.Size(105, 23);
            this.button33.TabIndex = 25;
            this.button33.Text = "Crack 16-20mm   $64";
            this.button33.UseVisualStyleBackColor = true;
            this.button33.Click += new System.EventHandler(this.CheckedChanged);
            // 
            // button34
            // 
            this.button34.Location = new System.Drawing.Point(243, 19);
            this.button34.Name = "button34";
            this.button34.Size = new System.Drawing.Size(104, 23);
            this.button34.TabIndex = 24;
            this.button34.Text = "Crack 11-15mm   $56";
            this.button34.UseVisualStyleBackColor = true;
            this.button34.Click += new System.EventHandler(this.CheckedChanged);
            // 
            // button35
            // 
            this.button35.Location = new System.Drawing.Point(128, 19);
            this.button35.Name = "button35";
            this.button35.Size = new System.Drawing.Size(104, 23);
            this.button35.TabIndex = 23;
            this.button35.Text = "Crack 1-10mm   $48";
            this.button35.UseVisualStyleBackColor = true;
            this.button35.Click += new System.EventHandler(this.CheckedChanged);
            // 
            // button36
            // 
            this.button36.Location = new System.Drawing.Point(9, 97);
            this.button36.Name = "button36";
            this.button36.Size = new System.Drawing.Size(97, 23);
            this.button36.TabIndex = 22;
            this.button36.Text = "Dent 16mm >   ";
            this.button36.UseVisualStyleBackColor = true;
            this.button36.Click += new System.EventHandler(this.CheckedChanged);
            // 
            // button37
            // 
            this.button37.Location = new System.Drawing.Point(9, 71);
            this.button37.Name = "button37";
            this.button37.Size = new System.Drawing.Size(97, 23);
            this.button37.TabIndex = 21;
            this.button37.Text = "Dent 11-15mm   $65";
            this.button37.UseVisualStyleBackColor = true;
            this.button37.Click += new System.EventHandler(this.CheckedChanged);
            // 
            // button38
            // 
            this.button38.Location = new System.Drawing.Point(9, 45);
            this.button38.Name = "button38";
            this.button38.Size = new System.Drawing.Size(97, 23);
            this.button38.TabIndex = 20;
            this.button38.Text = "Dent 6-10mm   $50";
            this.button38.UseVisualStyleBackColor = true;
            this.button38.Click += new System.EventHandler(this.CheckedChanged);
            // 
            // button39
            // 
            this.button39.Location = new System.Drawing.Point(9, 19);
            this.button39.Name = "button39";
            this.button39.Size = new System.Drawing.Size(97, 23);
            this.button39.TabIndex = 19;
            this.button39.Text = "Dent 1-5mm   $42";
            this.button39.UseVisualStyleBackColor = true;
            this.button39.Click += new System.EventHandler(this.CheckedChanged);
            // 
            // groupBoxRearSkirtDamage
            // 
            //this.groupBoxRearSkirtDamage.Controls.Add(this.groupBox6);
            this.groupBoxRearSkirtDamage.Controls.Add(this.button10);
            this.groupBoxRearSkirtDamage.Controls.Add(this.button6);
            this.groupBoxRearSkirtDamage.Controls.Add(this.button7);
            this.groupBoxRearSkirtDamage.Controls.Add(this.button8);
            this.groupBoxRearSkirtDamage.Controls.Add(this.button9);
            this.groupBoxRearSkirtDamage.Controls.Add(this.button5);
            this.groupBoxRearSkirtDamage.Controls.Add(this.button4);
            this.groupBoxRearSkirtDamage.Controls.Add(this.button3);
            this.groupBoxRearSkirtDamage.Controls.Add(this.button2);
            this.groupBoxRearSkirtDamage.Location = new System.Drawing.Point(6, 68);
            this.groupBoxRearSkirtDamage.Name = "groupBoxRearSkirtDamage";
            this.groupBoxRearSkirtDamage.Size = new System.Drawing.Size(354, 129);
            this.groupBoxRearSkirtDamage.TabIndex = 23;
            this.groupBoxRearSkirtDamage.TabStop = false;
            this.groupBoxRearSkirtDamage.Text = "Rear skirt damage repair";

           
            // 
            // button10
            // 
            this.button10.Location = new System.Drawing.Point(246, 97);
            this.button10.Name = "button10";
            this.button10.Size = new System.Drawing.Size(103, 23);
            this.button10.TabIndex = 27;
            this.button10.Text = "Crack 31mm >   ";
            this.button10.UseVisualStyleBackColor = true;
            this.button10.Click += new System.EventHandler(this.CheckedChanged);
            // 
            // button6
            // 
            this.button6.Location = new System.Drawing.Point(246, 71);
            this.button6.Name = "button6";
            this.button6.Size = new System.Drawing.Size(103, 23);
            this.button6.TabIndex = 26;
            this.button6.Text = "Crack 21-30mm  $32";
            this.button6.UseVisualStyleBackColor = true;
            this.button6.Click += new System.EventHandler(this.CheckedChanged);
            // 
            // button7
            // 
            this.button7.Location = new System.Drawing.Point(246, 45);
            this.button7.Name = "button7";
            this.button7.Size = new System.Drawing.Size(104, 23);
            this.button7.TabIndex = 25;
            this.button7.Text = "Crack 16-20mm   $26";
            this.button7.UseVisualStyleBackColor = true;
            this.button7.Click += new System.EventHandler(this.CheckedChanged);
            // 
            // button8
            // 
            this.button8.Location = new System.Drawing.Point(246, 19);
            this.button8.Name = "button8";
            this.button8.Size = new System.Drawing.Size(104, 23);
            this.button8.TabIndex = 24;
            this.button8.Text = "Crack 11-15mm   $22";
            this.button8.UseVisualStyleBackColor = true;
            this.button8.Click += new System.EventHandler(this.CheckedChanged);
            // 
            // button9
            // 
            this.button9.Location = new System.Drawing.Point(129, 19);
            this.button9.Name = "button9";
            this.button9.Size = new System.Drawing.Size(104, 23);
            this.button9.TabIndex = 23;
            this.button9.Text = "Crack 1-10mm   $18";
            this.button9.UseVisualStyleBackColor = true;
            this.button9.Click += new System.EventHandler(this.CheckedChanged);
            // 
            // button5
            // 
            this.button5.Location = new System.Drawing.Point(6, 97);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(97, 23);
            this.button5.TabIndex = 22;
            this.button5.Text = "Dent 16mm >   ";
            this.button5.UseVisualStyleBackColor = true;
            this.button5.Click += new System.EventHandler(this.CheckedChanged);
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(6, 71);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(97, 23);
            this.button4.TabIndex = 21;
            this.button4.Text = "Dent 11-15mm   $46";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.CheckedChanged);
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(6, 45);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(97, 23);
            this.button3.TabIndex = 20;
            this.button3.Text = "Dent 6-10mm   $38";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.CheckedChanged);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(6, 19);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(97, 23);
            this.button2.TabIndex = 19;
            this.button2.Text = "Dent 1-5mm   $26";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.CheckedChanged);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(6, 16);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(92, 23);
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
            this.checkBox22.Location = new System.Drawing.Point(14, 19);
            this.checkBox22.Name = "checkBox22";
            this.checkBox22.Size = new System.Drawing.Size(120, 23);
            this.checkBox22.TabIndex = 18;
            this.checkBox22.Text = "OTHER";
            this.checkBox22.UseVisualStyleBackColor = true;
            this.checkBox22.Click += new System.EventHandler(this.CheckedChanged);
            // 
            // button43
            // 
            this.button43.Location = new System.Drawing.Point(6, 45);
            this.button43.Name = "button43";
            this.button43.Size = new System.Drawing.Size(92, 23);
            this.button43.TabIndex = 31;
            this.button43.Text = "CNC facing #1  $55";
            this.button43.UseVisualStyleBackColor = true;
            this.button43.Click += new System.EventHandler(this.CheckedChanged);
            // 
            // button44
            // 
            this.button44.Location = new System.Drawing.Point(100, 45);
            this.button44.Name = "button44";
            this.button44.Size = new System.Drawing.Size(92, 23);
            this.button44.TabIndex = 32;
            this.button44.Text = "CNC facing #2  $72";
            this.button44.UseVisualStyleBackColor = true;
            this.button44.Click += new System.EventHandler(this.CheckedChanged);
            // 
            // button45
            // 
            this.button45.Location = new System.Drawing.Point(198, 45);
            this.button45.Name = "button45";
            this.button45.Size = new System.Drawing.Size(92, 23);
            this.button45.TabIndex = 33;
            this.button45.Text = "CNC facing #3  $92";
            this.button45.UseVisualStyleBackColor = true;
            this.button45.Click += new System.EventHandler(this.CheckedChanged);
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
            //this.groupBox8.ResumeLayout(false);
            this.groupBoxRearSkirtDamage.ResumeLayout(false);
            //this.groupBox6.ResumeLayout(false);
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
                this.groupBox4.Text =  "Wheel Tyre Service";
                this.checkBox20.Text = "Remove Tyre              $12";
                this.checkBox21.Text = "Fit Tyre                $12";

                this.groupBox2.Visible = false;
                this.checkBox4.Visible = false;
                this.SetupPricingFromDB();
            }
            
        }
        public static bool isWheelApp()
        {
            string appName = Application.ExecutablePath.ToUpper();
            if (JobCard.isWheel)
            {
                return true;
            }
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

