namespace Job_Card
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Drawing;
    using System.Globalization;
    using System.Linq;
    using System.Windows.Forms;

    public class JobTypePopup : Form {

        private static string surchargeJobID = "";
        private double surchargeMultiplier = 1.0;
        private Button smiley;
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
        private Button button1;
        private Button button14;
        private Button button12;
        private Button button13;
        private Button button11;
        private BindingSource bindingSource1;
        private Button button17;
        private Button button16;
        private Button button15;
        private Label overrideControlTextLabel;
        private TextBox overrideControlText;
        private Label label1;
        private TextBox overridePrice;
        private Button button26;
        private Button button27;
        private Button button28;
        private Button button29;
        private Button button22;
        private Button button23;
        private Button button24;
        private Button button25;
        private Button button18;
        private Button button19;
        private Button button20;
        private Button setUpOnLathe;
        private GroupBox wheelTyreService;
        private Button fitTyre;
        private Button removeTyre;
        private GroupBox repairAndFinishing;
        private Button wheelBalance;
        private Button polish;
        private Button bentSpoke;
        private Button strip;
        private Button button30;
        private Button button46;
        private Button button21;
        private Button button47;
        private Button button48;
        private Button button49;
        private Button button50;
        private Button button51;
        private Button button52;
        private Button button53;
        private Button button59;
        private Button button60;
        private Button button61;
        private Button button54;
        private Button button55;
        private Button button56;
        private Button button57;
        private Button button58;
        private Button button64;
        private Button button65;
        private Button button63;
        private Button button62;
        private Label currentPriceLabel;
        private Label currentPrice;
        private Button button66;
        private Button button70;
        private Button button68;
        private Button button69;
        private Button button67;
        private Button button72;
        private Button button71;
        private Button button79;
        private Button button78;
        private Button button77;
        private Button button75;
        private Button button76;
        private Button button74;
        private Button button73;
        private bool startup = true;

        public JobTypePopup()
        {
            this.startup = true;
            this.InitializeComponent();

            // Only run database operations if not in design mode
            if (!DesignMode)
            {
                this.startup = false;
            }
        }

        public void getGroupBoxPrices(GroupBox box)
        {
            // Skip if in design mode
            if (DesignMode) return;
            
            foreach (object obj3 in box.Controls)
            {
                if (obj3 is Button)
                {
                    Button box2 = (Button)obj3;
                    //string item = new CultureInfo("en-NZ", false).TextInfo.ToTitleCase(box2.Text.ToLowerInvariant());
                    DataAccess.findOrUpdatePrice(box2, null, null);
                } else if (obj3 is GroupBox)
                {
                    GroupBox inner = (GroupBox)obj3;
                    getGroupBoxPrices(inner);
                }
            }
        }
        public async System.Threading.Tasks.Task getAllPrices()
        {
            // Skip if in design mode
            if (DesignMode) return;
            
            if (JobTypePopup.jobType == null && jobCard != null && jobCard.jobType != null && jobCard.jobType.Length > 0)
            {
                JobTypePopup.jobDetail = jobCard.jobDetail[0];
                JobTypePopup.jobType = jobCard.jobType[0];
                JobTypePopup.jobQty = jobCard.jobQty[0];
                JobTypePopup.jobPrice = jobCard.jobPrice[0];
                JobTypePopup.jobUnitPrice = jobCard.jobUnitPrice[0];
            }
            
            // Add null check for jobType
            if (jobType == null) return;
            
            List<string> list = jobType.Text.Split(new string[] { ", " }, StringSplitOptions.None).ToList<string>();
            foreach (object obj2 in base.Controls)
            {
                if (obj2 is GroupBox)
                {
                    GroupBox box = (GroupBox)obj2;
                    getGroupBoxPrices(box);
                }
                else if (obj2 is Button)
                {
                    Button box2 = (Button)obj2;
                    //string item = new CultureInfo("en-NZ", false).TextInfo.ToTitleCase(box2.Text.ToLowerInvariant());
                    await DataAccess.findOrUpdatePrice(box2, null, null);
                }
            }
        }

        public async void SetupPricingFromDB()
        {
            // Skip if in design mode
            if (DesignMode) return;
            
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
            if (jobType != null)
            {
                jobType.Text = "";
            }
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

        private void SurchargeClicked(object sender, EventArgs e)
        {
            var id = this.jobCard.Controls.Find("jobID", false);
            if (id.Length == 1)
            {
                JobTypePopup.surchargeJobID = id[0].Text;
            }
        }
        private float getSurchargeMultiplier()
        {
            var id = this.jobCard.Controls.Find("jobID", false);
            if (id.Length == 1)
            {
                if (JobTypePopup.surchargeJobID == id[0].Text)
                {
                    return 1.2f;
                }
            }
            return 1f;
        }
        private async void CheckedChanged(object sender, EventArgs e)
        {
            await doCheckChange(sender);
        }

        private Button hoveringButton = null;
        private async void hoverPrice(object sender, EventArgs e)
        {
            // Skip if in design mode
            if (DesignMode) return;
            
            if (sender.GetType() == typeof(Button))
            {
                Button button = (Button)sender;
                if (button.ClientRectangle.Contains(button.PointToClient(Control.MousePosition)))           
                {
                    if (hoveringButton != button)
                    {
                        hoveringButton = button;
                        this.currentPriceLabel.Text = button.Text;
                        string price = await DataAccess.findOrUpdatePrice(button, null, null);
                        if (!price.Contains("."))
                        {
                            price += ".00";
                        }
                        float unitPriceFloat = 0;
                        float.TryParse(price, out unitPriceFloat);
                        surchargeMultiplier = getSurchargeMultiplier();
                        unitPriceFloat = (float)Math.Round(1.15*unitPriceFloat * surchargeMultiplier, 2);
                        price = "" + unitPriceFloat;

                        this.currentPrice.Text = "$"+price;
                    }
                     
                } else
                {
                    this.currentPriceLabel.Text = "Current Price";
                    this.currentPrice.Text = "Hover to view";
                    hoveringButton = null;
                }
            }
        }

        private async System.Threading.Tasks.Task doCheckChange(object sender) { 

            // Skip if in design mode
            if (DesignMode) return;

            if (!this.startup)
            {
                Button box = (Button) sender;
                GroupBox prnt = box.Parent as GroupBox;

                if (ModifierKeys.HasFlag(Keys.Control))
                {
                    if (overridePrice.Text.Trim() != "" || overrideControlText.Text.Trim() != "")
                    {
                        string newPrice = await DataAccess.findOrUpdatePrice(box, overridePrice, overrideControlText);
                        MessageBox.Show("Successfully changed button pricing and/or button text! Price is: "+newPrice+" and button text is "+box.Text);
                    }
                    else
                    {
                        MessageBox.Show("USER ERROR ! You held the Ctrl key and clicked a pricing button... but you haven't set at least one of 'override price' / 'override button text' input fields, so nothing was changed");

                    }
                    return;
                }


               
                string unitPrice = await DataAccess.findOrUpdatePrice(box, overridePrice, overrideControlText);
                string item = box.Text;
                if (JobTypePopup.jobType == null)
                {
                    JobTypePopup.jobType = jobCard.jobType[0];
                    JobTypePopup.jobDetail = jobCard.jobDetail[0];
                    JobTypePopup.jobPrice = jobCard.jobPrice[0];
                    JobTypePopup.jobQty = jobCard.jobQty[0];
                    JobTypePopup.jobUnitPrice = jobCard.jobUnitPrice[0];

                }
                if (!JobTypePopup.isWheelApp())
                {
                    item = new CultureInfo("en-NZ", false).TextInfo.ToTitleCase(box.Text.ToLowerInvariant());
                } else
                {
                    if (prnt != null)
                    {
                        JobTypePopup.jobDetail.Text = prnt.Text;
                    }
                   
                }
                if (!unitPrice.Contains("."))
                {
                    unitPrice += ".00";
                }

                Dictionary<string, int> dict = new Dictionary<string, int>();
                List<string> list = JobTypePopup.jobType.Text.Split(new string[] { ", " }, StringSplitOptions.None).ToList<string>();
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
                    
                    JobTypePopup.jobType.Text = item;
                    float unitPriceFloat = 0;
                    float.TryParse(unitPrice, out unitPriceFloat);
                    surchargeMultiplier = getSurchargeMultiplier();
                    unitPriceFloat = (float)Math.Round(1.15 * unitPriceFloat * surchargeMultiplier,2);

                    JobTypePopup.jobUnitPrice.Text = "" +unitPriceFloat;
                    float price = (float)Math.Round((double)counter * (double)unitPriceFloat, 2);
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
                Close();
            } else
            {

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
            this.components = new System.ComponentModel.Container();
            this.clearButton = new System.Windows.Forms.Button();
            this.smiley = new System.Windows.Forms.Button();
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
            this.repairAndFinishing = new System.Windows.Forms.GroupBox();
            this.button30 = new System.Windows.Forms.Button();
            this.wheelBalance = new System.Windows.Forms.Button();
            this.polish = new System.Windows.Forms.Button();
            this.bentSpoke = new System.Windows.Forms.Button();
            this.strip = new System.Windows.Forms.Button();
            this.wheelTyreService = new System.Windows.Forms.GroupBox();
            this.button46 = new System.Windows.Forms.Button();
            this.fitTyre = new System.Windows.Forms.Button();
            this.removeTyre = new System.Windows.Forms.Button();
            this.setUpOnLathe = new System.Windows.Forms.Button();
            this.button26 = new System.Windows.Forms.Button();
            this.button27 = new System.Windows.Forms.Button();
            this.button28 = new System.Windows.Forms.Button();
            this.button29 = new System.Windows.Forms.Button();
            this.button22 = new System.Windows.Forms.Button();
            this.button23 = new System.Windows.Forms.Button();
            this.button24 = new System.Windows.Forms.Button();
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
            this.button25 = new System.Windows.Forms.Button();
            this.button18 = new System.Windows.Forms.Button();
            this.button19 = new System.Windows.Forms.Button();
            this.button20 = new System.Windows.Forms.Button();
            this.button17 = new System.Windows.Forms.Button();
            this.button16 = new System.Windows.Forms.Button();
            this.button15 = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.button14 = new System.Windows.Forms.Button();
            this.button12 = new System.Windows.Forms.Button();
            this.button13 = new System.Windows.Forms.Button();
            this.button11 = new System.Windows.Forms.Button();
            this.button45 = new System.Windows.Forms.Button();
            this.button44 = new System.Windows.Forms.Button();
            this.button43 = new System.Windows.Forms.Button();
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
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.checkBox21 = new System.Windows.Forms.Button();
            this.checkBox20 = new System.Windows.Forms.Button();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.checkBox22 = new System.Windows.Forms.Button();
            this.bindingSource1 = new System.Windows.Forms.BindingSource(this.components);
            this.overridePrice = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.overrideControlTextLabel = new System.Windows.Forms.Label();
            this.overrideControlText = new System.Windows.Forms.TextBox();
            this.button21 = new System.Windows.Forms.Button();
            this.button47 = new System.Windows.Forms.Button();
            this.button48 = new System.Windows.Forms.Button();
            this.button49 = new System.Windows.Forms.Button();
            this.button50 = new System.Windows.Forms.Button();
            this.button51 = new System.Windows.Forms.Button();
            this.button52 = new System.Windows.Forms.Button();
            this.button53 = new System.Windows.Forms.Button();
            this.button54 = new System.Windows.Forms.Button();
            this.button55 = new System.Windows.Forms.Button();
            this.button56 = new System.Windows.Forms.Button();
            this.button57 = new System.Windows.Forms.Button();
            this.button58 = new System.Windows.Forms.Button();
            this.button59 = new System.Windows.Forms.Button();
            this.button60 = new System.Windows.Forms.Button();
            this.button61 = new System.Windows.Forms.Button();
            this.button62 = new System.Windows.Forms.Button();
            this.button63 = new System.Windows.Forms.Button();
            this.button64 = new System.Windows.Forms.Button();
            this.button65 = new System.Windows.Forms.Button();
            this.currentPriceLabel = new System.Windows.Forms.Label();
            this.currentPrice = new System.Windows.Forms.Label();
            this.button66 = new System.Windows.Forms.Button();
            this.button67 = new System.Windows.Forms.Button();
            this.button68 = new System.Windows.Forms.Button();
            this.button69 = new System.Windows.Forms.Button();
            this.button70 = new System.Windows.Forms.Button();
            this.button71 = new System.Windows.Forms.Button();
            this.button72 = new System.Windows.Forms.Button();
            this.button73 = new System.Windows.Forms.Button();
            this.button74 = new System.Windows.Forms.Button();
            this.button75 = new System.Windows.Forms.Button();
            this.button76 = new System.Windows.Forms.Button();
            this.button77 = new System.Windows.Forms.Button();
            this.button78 = new System.Windows.Forms.Button();
            this.button79 = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.repairAndFinishing.SuspendLayout();
            this.wheelTyreService.SuspendLayout();
            this.groupBoxRearSkirtDamage.SuspendLayout();
            this.groupBox7.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.groupBox5.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.bindingSource1)).BeginInit();
            this.SuspendLayout();
            this.smiley.Location = new System.Drawing.Point(680, 5);
            this.smiley.Name = "smiley";
            this.smiley.Size = new System.Drawing.Size(30, 30);
            this.smiley.TabIndex = 1;
            this.smiley.Font = new System.Drawing.Font("Segoe MDL2 Assets", 12);
            string smileEmoji = "\uD83D\uDE04";
            this.smiley.Text = smileEmoji;
            this.smiley.Click += new System.EventHandler(this.SurchargeClicked);
            // 
            // clearButton
            // 
            this.clearButton.Location = new System.Drawing.Point(9, 579);
            this.clearButton.Name = "clearButton";
            this.clearButton.Size = new System.Drawing.Size(157, 25);
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
            this.groupBox1.Location = new System.Drawing.Point(9, 2);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(321, 64);
            this.groupBox1.TabIndex = 4;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Repair and Finishing";
            // 
            // checkBox4
            // 
            this.checkBox4.Location = new System.Drawing.Point(238, 21);
            this.checkBox4.Name = "checkBox4";
            this.checkBox4.Size = new System.Drawing.Size(71, 23);
            this.checkBox4.TabIndex = 7;
            this.checkBox4.Text = "LAQUER";
            this.checkBox4.UseVisualStyleBackColor = true;
            this.checkBox4.Click += new System.EventHandler(this.CheckedChanged);
            this.checkBox4.MouseEnter += new System.EventHandler(this.hoverPrice);
            this.checkBox4.MouseLeave += new System.EventHandler(this.hoverPrice);
            // 
            // checkBox3
            // 
            this.checkBox3.Location = new System.Drawing.Point(163, 21);
            this.checkBox3.Name = "checkBox3";
            this.checkBox3.Size = new System.Drawing.Size(68, 23);
            this.checkBox3.TabIndex = 6;
            this.checkBox3.Text = "POLISH";
            this.checkBox3.UseVisualStyleBackColor = true;
            this.checkBox3.Click += new System.EventHandler(this.CheckedChanged);
            this.checkBox3.MouseEnter += new System.EventHandler(this.hoverPrice);
            this.checkBox3.MouseLeave += new System.EventHandler(this.hoverPrice);
            // 
            // checkBox2
            // 
            this.checkBox2.Location = new System.Drawing.Point(90, 21);
            this.checkBox2.Name = "checkBox2";
            this.checkBox2.Size = new System.Drawing.Size(67, 23);
            this.checkBox2.TabIndex = 5;
            this.checkBox2.Text = "REPAIR";
            this.checkBox2.UseVisualStyleBackColor = true;
            this.checkBox2.Click += new System.EventHandler(this.CheckedChanged);
            this.checkBox2.MouseEnter += new System.EventHandler(this.hoverPrice);
            this.checkBox2.MouseLeave += new System.EventHandler(this.hoverPrice);
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
            this.checkBox1.MouseEnter += new System.EventHandler(this.hoverPrice);
            this.checkBox1.MouseLeave += new System.EventHandler(this.hoverPrice);
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
            this.groupBox2.Location = new System.Drawing.Point(336, 0);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(335, 66);
            this.groupBox2.TabIndex = 5;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Plating";
            // 
            // checkBox13
            // 
            this.checkBox13.Location = new System.Drawing.Point(270, 38);
            this.checkBox13.Name = "checkBox13";
            this.checkBox13.Size = new System.Drawing.Size(48, 23);
            this.checkBox13.TabIndex = 16;
            this.checkBox13.Text = "GOLD";
            this.checkBox13.UseVisualStyleBackColor = true;
            this.checkBox13.Click += new System.EventHandler(this.CheckedChanged);
            this.checkBox13.MouseEnter += new System.EventHandler(this.hoverPrice);
            this.checkBox13.MouseLeave += new System.EventHandler(this.hoverPrice);
            // 
            // checkBox12
            // 
            this.checkBox12.Location = new System.Drawing.Point(201, 38);
            this.checkBox12.Name = "checkBox12";
            this.checkBox12.Size = new System.Drawing.Size(63, 23);
            this.checkBox12.TabIndex = 15;
            this.checkBox12.Text = "SILVER";
            this.checkBox12.UseVisualStyleBackColor = true;
            this.checkBox12.Click += new System.EventHandler(this.CheckedChanged);
            this.checkBox12.MouseEnter += new System.EventHandler(this.hoverPrice);
            this.checkBox12.MouseLeave += new System.EventHandler(this.hoverPrice);
            // 
            // checkBox11
            // 
            this.checkBox11.Location = new System.Drawing.Point(147, 38);
            this.checkBox11.Name = "checkBox11";
            this.checkBox11.Size = new System.Drawing.Size(48, 23);
            this.checkBox11.TabIndex = 14;
            this.checkBox11.Text = "TIN";
            this.checkBox11.UseVisualStyleBackColor = true;
            this.checkBox11.Click += new System.EventHandler(this.CheckedChanged);
            this.checkBox11.MouseEnter += new System.EventHandler(this.hoverPrice);
            this.checkBox11.MouseLeave += new System.EventHandler(this.hoverPrice);
            // 
            // checkBox10
            // 
            this.checkBox10.Location = new System.Drawing.Point(89, 38);
            this.checkBox10.Name = "checkBox10";
            this.checkBox10.Size = new System.Drawing.Size(52, 23);
            this.checkBox10.TabIndex = 13;
            this.checkBox10.Text = "SATIN";
            this.checkBox10.UseVisualStyleBackColor = true;
            this.checkBox10.Click += new System.EventHandler(this.CheckedChanged);
            this.checkBox10.MouseEnter += new System.EventHandler(this.hoverPrice);
            this.checkBox10.MouseLeave += new System.EventHandler(this.hoverPrice);
            // 
            // checkBox9
            // 
            this.checkBox9.Location = new System.Drawing.Point(11, 38);
            this.checkBox9.Name = "checkBox9";
            this.checkBox9.Size = new System.Drawing.Size(72, 23);
            this.checkBox9.TabIndex = 12;
            this.checkBox9.Text = "BRONZE";
            this.checkBox9.UseVisualStyleBackColor = true;
            this.checkBox9.Click += new System.EventHandler(this.CheckedChanged);
            this.checkBox9.MouseEnter += new System.EventHandler(this.hoverPrice);
            this.checkBox9.MouseLeave += new System.EventHandler(this.hoverPrice);
            // 
            // checkBox8
            // 
            this.checkBox8.Location = new System.Drawing.Point(237, 12);
            this.checkBox8.Name = "checkBox8";
            this.checkBox8.Size = new System.Drawing.Size(71, 23);
            this.checkBox8.TabIndex = 11;
            this.checkBox8.Text = "BRASS";
            this.checkBox8.UseVisualStyleBackColor = true;
            this.checkBox8.Click += new System.EventHandler(this.CheckedChanged);
            this.checkBox8.MouseEnter += new System.EventHandler(this.hoverPrice);
            this.checkBox8.MouseLeave += new System.EventHandler(this.hoverPrice);
            // 
            // checkBox7
            // 
            this.checkBox7.Location = new System.Drawing.Point(163, 12);
            this.checkBox7.Name = "checkBox7";
            this.checkBox7.Size = new System.Drawing.Size(68, 23);
            this.checkBox7.TabIndex = 10;
            this.checkBox7.Text = "CHROME";
            this.checkBox7.UseVisualStyleBackColor = true;
            this.checkBox7.Click += new System.EventHandler(this.CheckedChanged);
            this.checkBox7.MouseEnter += new System.EventHandler(this.hoverPrice);
            this.checkBox7.MouseLeave += new System.EventHandler(this.hoverPrice);
            // 
            // checkBox6
            // 
            this.checkBox6.Location = new System.Drawing.Point(90, 12);
            this.checkBox6.Name = "checkBox6";
            this.checkBox6.Size = new System.Drawing.Size(67, 23);
            this.checkBox6.TabIndex = 9;
            this.checkBox6.Text = "NICKLE";
            this.checkBox6.UseVisualStyleBackColor = true;
            this.checkBox6.Click += new System.EventHandler(this.CheckedChanged);
            this.checkBox6.MouseEnter += new System.EventHandler(this.hoverPrice);
            this.checkBox6.MouseLeave += new System.EventHandler(this.hoverPrice);
            // 
            // checkBox5
            // 
            this.checkBox5.Location = new System.Drawing.Point(11, 12);
            this.checkBox5.Name = "checkBox5";
            this.checkBox5.Size = new System.Drawing.Size(73, 23);
            this.checkBox5.TabIndex = 8;
            this.checkBox5.Text = "COPPER";
            this.checkBox5.UseVisualStyleBackColor = true;
            this.checkBox5.Click += new System.EventHandler(this.CheckedChanged);
            this.checkBox5.MouseEnter += new System.EventHandler(this.hoverPrice);
            this.checkBox5.MouseLeave += new System.EventHandler(this.hoverPrice);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.button59);
            this.groupBox3.Controls.Add(this.button60);
            this.groupBox3.Controls.Add(this.button61);
            this.groupBox3.Controls.Add(this.button54);
            this.groupBox3.Controls.Add(this.button55);
            this.groupBox3.Controls.Add(this.button56);
            this.groupBox3.Controls.Add(this.button57);
            this.groupBox3.Controls.Add(this.button58);
            this.groupBox3.Controls.Add(this.button53);
            this.groupBox3.Controls.Add(this.button21);
            this.groupBox3.Controls.Add(this.button47);
            this.groupBox3.Controls.Add(this.button48);
            this.groupBox3.Controls.Add(this.button49);
            this.groupBox3.Controls.Add(this.repairAndFinishing);
            this.groupBox3.Controls.Add(this.wheelTyreService);
            this.groupBox3.Controls.Add(this.setUpOnLathe);
            this.groupBox3.Controls.Add(this.button26);
            this.groupBox3.Controls.Add(this.button27);
            this.groupBox3.Controls.Add(this.button28);
            this.groupBox3.Controls.Add(this.button29);
            this.groupBox3.Controls.Add(this.button22);
            this.groupBox3.Controls.Add(this.button23);
            this.groupBox3.Controls.Add(this.button24);
            this.groupBox3.Controls.Add(this.groupBoxRearSkirtDamage);
            this.groupBox3.Controls.Add(this.button25);
            this.groupBox3.Controls.Add(this.button18);
            this.groupBox3.Controls.Add(this.button19);
            this.groupBox3.Controls.Add(this.button20);
            this.groupBox3.Controls.Add(this.button17);
            this.groupBox3.Controls.Add(this.button16);
            this.groupBox3.Controls.Add(this.button15);
            this.groupBox3.Controls.Add(this.button1);
            this.groupBox3.Controls.Add(this.button14);
            this.groupBox3.Controls.Add(this.button12);
            this.groupBox3.Controls.Add(this.button13);
            this.groupBox3.Controls.Add(this.button11);
            this.groupBox3.Controls.Add(this.button45);
            this.groupBox3.Controls.Add(this.button44);
            this.groupBox3.Controls.Add(this.button43);
            this.groupBox3.Controls.Add(this.button40);
            this.groupBox3.Controls.Add(this.button42);
            this.groupBox3.Controls.Add(this.button41);
            this.groupBox3.Controls.Add(this.groupBox7);
            this.groupBox3.Location = new System.Drawing.Point(9, 67);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(822, 436);
            this.groupBox3.TabIndex = 9;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Wheel repair";
            // 
            // repairAndFinishing
            // 
            this.repairAndFinishing.Controls.Add(this.button64);
            this.repairAndFinishing.Controls.Add(this.button65);
            this.repairAndFinishing.Controls.Add(this.button63);
            this.repairAndFinishing.Controls.Add(this.button62);
            this.repairAndFinishing.Controls.Add(this.button30);
            this.repairAndFinishing.Controls.Add(this.wheelBalance);
            this.repairAndFinishing.Controls.Add(this.polish);
            this.repairAndFinishing.Controls.Add(this.bentSpoke);
            this.repairAndFinishing.Controls.Add(this.strip);
            this.repairAndFinishing.Location = new System.Drawing.Point(325, 346);
            this.repairAndFinishing.Name = "repairAndFinishing";
            this.repairAndFinishing.Size = new System.Drawing.Size(484, 71);
            this.repairAndFinishing.TabIndex = 8;
            this.repairAndFinishing.TabStop = false;
            this.repairAndFinishing.Text = "Repair and Finishing";
            // 
            // button30
            // 
            this.button30.Location = new System.Drawing.Point(378, 18);
            this.button30.Name = "button30";
            this.button30.Size = new System.Drawing.Size(94, 23);
            this.button30.TabIndex = 8;
            this.button30.Text = "Unused";
            this.button30.UseVisualStyleBackColor = true;
            this.button30.Click += new System.EventHandler(this.CheckedChanged);
            this.button30.MouseEnter += new System.EventHandler(this.hoverPrice);
            this.button30.MouseLeave += new System.EventHandler(this.hoverPrice);
            // 
            // wheelBalance
            // 
            this.wheelBalance.Location = new System.Drawing.Point(278, 18);
            this.wheelBalance.Name = "wheelBalance";
            this.wheelBalance.Size = new System.Drawing.Size(94, 23);
            this.wheelBalance.TabIndex = 7;
            this.wheelBalance.Text = "Wheel balance           $16";
            this.wheelBalance.UseVisualStyleBackColor = true;
            this.wheelBalance.Click += new System.EventHandler(this.CheckedChanged);
            this.wheelBalance.MouseEnter += new System.EventHandler(this.hoverPrice);
            this.wheelBalance.MouseLeave += new System.EventHandler(this.hoverPrice);
            // 
            // polish
            // 
            this.polish.Location = new System.Drawing.Point(181, 18);
            this.polish.Name = "polish";
            this.polish.Size = new System.Drawing.Size(91, 23);
            this.polish.TabIndex = 6;
            this.polish.Text = "Polish";
            this.polish.UseVisualStyleBackColor = true;
            this.polish.Click += new System.EventHandler(this.CheckedChanged);
            this.polish.MouseEnter += new System.EventHandler(this.hoverPrice);
            this.polish.MouseLeave += new System.EventHandler(this.hoverPrice);
            // 
            // bentSpoke
            // 
            this.bentSpoke.Location = new System.Drawing.Point(90, 18);
            this.bentSpoke.Name = "bentSpoke";
            this.bentSpoke.Size = new System.Drawing.Size(84, 23);
            this.bentSpoke.TabIndex = 5;
            this.bentSpoke.Text = "Bent spoke               $26";
            this.bentSpoke.UseVisualStyleBackColor = true;
            this.bentSpoke.Click += new System.EventHandler(this.CheckedChanged);
            this.bentSpoke.MouseEnter += new System.EventHandler(this.hoverPrice);
            this.bentSpoke.MouseLeave += new System.EventHandler(this.hoverPrice);
            // 
            // strip
            // 
            this.strip.Location = new System.Drawing.Point(11, 18);
            this.strip.Name = "strip";
            this.strip.Size = new System.Drawing.Size(73, 23);
            this.strip.TabIndex = 4;
            this.strip.Text = "Strip";
            this.strip.UseVisualStyleBackColor = true;
            this.strip.Click += new System.EventHandler(this.CheckedChanged);
            this.strip.MouseEnter += new System.EventHandler(this.hoverPrice);
            this.strip.MouseLeave += new System.EventHandler(this.hoverPrice);
            // 
            // wheelTyreService
            // 
            this.wheelTyreService.Controls.Add(this.button50);
            this.wheelTyreService.Controls.Add(this.button51);
            this.wheelTyreService.Controls.Add(this.button52);
            this.wheelTyreService.Controls.Add(this.button46);
            this.wheelTyreService.Controls.Add(this.fitTyre);
            this.wheelTyreService.Controls.Add(this.removeTyre);
            this.wheelTyreService.Location = new System.Drawing.Point(6, 346);
            this.wheelTyreService.Name = "wheelTyreService";
            this.wheelTyreService.Size = new System.Drawing.Size(313, 71);
            this.wheelTyreService.TabIndex = 59;
            this.wheelTyreService.TabStop = false;
            this.wheelTyreService.Text = "Wheel Tyre Service";
            // 
            // button46
            // 
            this.button46.Location = new System.Drawing.Point(217, 17);
            this.button46.Name = "button46";
            this.button46.Size = new System.Drawing.Size(92, 23);
            this.button46.TabIndex = 30;
            this.button46.Text = "Unused";
            this.button46.UseVisualStyleBackColor = true;
            this.button46.Click += new System.EventHandler(this.CheckedChanged);
            this.button46.MouseEnter += new System.EventHandler(this.hoverPrice);
            this.button46.MouseLeave += new System.EventHandler(this.hoverPrice);
            // 
            // fitTyre
            // 
            this.fitTyre.Location = new System.Drawing.Point(111, 17);
            this.fitTyre.Name = "fitTyre";
            this.fitTyre.Size = new System.Drawing.Size(97, 23);
            this.fitTyre.TabIndex = 29;
            this.fitTyre.Text = "Fit Tyre           $12";
            this.fitTyre.UseVisualStyleBackColor = true;
            this.fitTyre.Click += new System.EventHandler(this.CheckedChanged);
            this.fitTyre.MouseEnter += new System.EventHandler(this.hoverPrice);
            this.fitTyre.MouseLeave += new System.EventHandler(this.hoverPrice);
            // 
            // removeTyre
            // 
            this.removeTyre.Location = new System.Drawing.Point(8, 17);
            this.removeTyre.Name = "removeTyre";
            this.removeTyre.Size = new System.Drawing.Size(97, 23);
            this.removeTyre.TabIndex = 28;
            this.removeTyre.Text = "Remove Tyre    $12";
            this.removeTyre.UseVisualStyleBackColor = true;
            this.removeTyre.Click += new System.EventHandler(this.CheckedChanged);
            this.removeTyre.MouseEnter += new System.EventHandler(this.hoverPrice);
            this.removeTyre.MouseLeave += new System.EventHandler(this.hoverPrice);
            // 
            // setUpOnLathe
            // 
            this.setUpOnLathe.Location = new System.Drawing.Point(3, 48);
            this.setUpOnLathe.Name = "setUpOnLathe";
            this.setUpOnLathe.Size = new System.Drawing.Size(93, 23);
            this.setUpOnLathe.TabIndex = 58;
            this.setUpOnLathe.Text = "Set up on lathe    $35";
            this.setUpOnLathe.UseVisualStyleBackColor = true;
            this.setUpOnLathe.Click += new System.EventHandler(this.CheckedChanged);
            this.setUpOnLathe.MouseEnter += new System.EventHandler(this.hoverPrice);
            this.setUpOnLathe.MouseLeave += new System.EventHandler(this.hoverPrice);
            // 
            // button26
            // 
            this.button26.Location = new System.Drawing.Point(697, 48);
            this.button26.Name = "button26";
            this.button26.Size = new System.Drawing.Size(93, 23);
            this.button26.TabIndex = 57;
            this.button26.Text = "Unused";
            this.button26.UseVisualStyleBackColor = true;
            this.button26.Click += new System.EventHandler(this.CheckedChanged);
            this.button26.MouseEnter += new System.EventHandler(this.hoverPrice);
            this.button26.MouseLeave += new System.EventHandler(this.hoverPrice);
            // 
            // button27
            // 
            this.button27.Location = new System.Drawing.Point(598, 48);
            this.button27.Name = "button27";
            this.button27.Size = new System.Drawing.Size(93, 23);
            this.button27.TabIndex = 56;
            this.button27.Text = "Unused";
            this.button27.UseVisualStyleBackColor = true;
            this.button27.Click += new System.EventHandler(this.CheckedChanged);
            this.button27.MouseEnter += new System.EventHandler(this.hoverPrice);
            this.button27.MouseLeave += new System.EventHandler(this.hoverPrice);
            // 
            // button28
            // 
            this.button28.Location = new System.Drawing.Point(499, 48);
            this.button28.Name = "button28";
            this.button28.Size = new System.Drawing.Size(93, 23);
            this.button28.TabIndex = 55;
            this.button28.Text = "Unused";
            this.button28.UseVisualStyleBackColor = true;
            this.button28.Click += new System.EventHandler(this.CheckedChanged);
            this.button28.MouseEnter += new System.EventHandler(this.hoverPrice);
            this.button28.MouseLeave += new System.EventHandler(this.hoverPrice);
            // 
            // button29
            // 
            this.button29.Location = new System.Drawing.Point(399, 48);
            this.button29.Name = "button29";
            this.button29.Size = new System.Drawing.Size(93, 23);
            this.button29.TabIndex = 54;
            this.button29.Text = "Unused";
            this.button29.UseVisualStyleBackColor = true;
            this.button29.Click += new System.EventHandler(this.CheckedChanged);
            this.button29.MouseEnter += new System.EventHandler(this.hoverPrice);
            this.button29.MouseLeave += new System.EventHandler(this.hoverPrice);
            // 
            // button22
            // 
            this.button22.Location = new System.Drawing.Point(697, 19);
            this.button22.Name = "button22";
            this.button22.Size = new System.Drawing.Size(93, 23);
            this.button22.TabIndex = 53;
            this.button22.Text = "Unused";
            this.button22.UseVisualStyleBackColor = true;
            this.button22.Click += new System.EventHandler(this.CheckedChanged);
            this.button22.MouseEnter += new System.EventHandler(this.hoverPrice);
            this.button22.MouseLeave += new System.EventHandler(this.hoverPrice);
            // 
            // button23
            // 
            this.button23.Location = new System.Drawing.Point(598, 19);
            this.button23.Name = "button23";
            this.button23.Size = new System.Drawing.Size(93, 23);
            this.button23.TabIndex = 52;
            this.button23.Text = "Unused";
            this.button23.UseVisualStyleBackColor = true;
            this.button23.Click += new System.EventHandler(this.CheckedChanged);
            this.button23.MouseEnter += new System.EventHandler(this.hoverPrice);
            this.button23.MouseLeave += new System.EventHandler(this.hoverPrice);
            // 
            // button24
            // 
            this.button24.Location = new System.Drawing.Point(499, 19);
            this.button24.Name = "button24";
            this.button24.Size = new System.Drawing.Size(93, 23);
            this.button24.TabIndex = 51;
            this.button24.Text = "Unused";
            this.button24.UseVisualStyleBackColor = true;
            this.button24.Click += new System.EventHandler(this.CheckedChanged);
            this.button24.MouseEnter += new System.EventHandler(this.hoverPrice);
            this.button24.MouseLeave += new System.EventHandler(this.hoverPrice);
            // 
            // groupBoxRearSkirtDamage
            // 
            this.groupBoxRearSkirtDamage.Controls.Add(this.button72);
            this.groupBoxRearSkirtDamage.Controls.Add(this.button71);
            this.groupBoxRearSkirtDamage.Controls.Add(this.button70);
            this.groupBoxRearSkirtDamage.Controls.Add(this.button68);
            this.groupBoxRearSkirtDamage.Controls.Add(this.button69);
            this.groupBoxRearSkirtDamage.Controls.Add(this.button67);
            this.groupBoxRearSkirtDamage.Controls.Add(this.button66);
            this.groupBoxRearSkirtDamage.Controls.Add(this.button10);
            this.groupBoxRearSkirtDamage.Controls.Add(this.button6);
            this.groupBoxRearSkirtDamage.Controls.Add(this.button7);
            this.groupBoxRearSkirtDamage.Controls.Add(this.button8);
            this.groupBoxRearSkirtDamage.Controls.Add(this.button9);
            this.groupBoxRearSkirtDamage.Controls.Add(this.button5);
            this.groupBoxRearSkirtDamage.Controls.Add(this.button4);
            this.groupBoxRearSkirtDamage.Controls.Add(this.button3);
            this.groupBoxRearSkirtDamage.Controls.Add(this.button2);
            this.groupBoxRearSkirtDamage.Location = new System.Drawing.Point(5, 166);
            this.groupBoxRearSkirtDamage.Name = "groupBoxRearSkirtDamage";
            this.groupBoxRearSkirtDamage.Size = new System.Drawing.Size(804, 75);
            this.groupBoxRearSkirtDamage.TabIndex = 23;
            this.groupBoxRearSkirtDamage.TabStop = false;
            this.groupBoxRearSkirtDamage.Text = "Rear skirt damage repair";
            // 
            // button10
            // 
            this.button10.Location = new System.Drawing.Point(424, 45);
            this.button10.Name = "button10";
            this.button10.Size = new System.Drawing.Size(89, 23);
            this.button10.TabIndex = 27;
            this.button10.Text = "Crack 31mm >   ";
            this.button10.UseVisualStyleBackColor = true;
            this.button10.Click += new System.EventHandler(this.CheckedChanged);
            this.button10.MouseEnter += new System.EventHandler(this.hoverPrice);
            this.button10.MouseLeave += new System.EventHandler(this.hoverPrice);
            // 
            // button6
            // 
            this.button6.Location = new System.Drawing.Point(321, 45);
            this.button6.Name = "button6";
            this.button6.Size = new System.Drawing.Size(97, 23);
            this.button6.TabIndex = 26;
            this.button6.Text = "Crack 21-30mm  $32";
            this.button6.UseVisualStyleBackColor = true;
            this.button6.Click += new System.EventHandler(this.CheckedChanged);
            this.button6.MouseEnter += new System.EventHandler(this.hoverPrice);
            this.button6.MouseLeave += new System.EventHandler(this.hoverPrice);
            // 
            // button7
            // 
            this.button7.Location = new System.Drawing.Point(218, 45);
            this.button7.Name = "button7";
            this.button7.Size = new System.Drawing.Size(97, 23);
            this.button7.TabIndex = 25;
            this.button7.Text = "Crack 16-20mm   $26";
            this.button7.UseVisualStyleBackColor = true;
            this.button7.Click += new System.EventHandler(this.CheckedChanged);
            this.button7.MouseEnter += new System.EventHandler(this.hoverPrice);
            this.button7.MouseLeave += new System.EventHandler(this.hoverPrice);
            // 
            // button8
            // 
            this.button8.Location = new System.Drawing.Point(115, 45);
            this.button8.Name = "button8";
            this.button8.Size = new System.Drawing.Size(97, 23);
            this.button8.TabIndex = 24;
            this.button8.Text = "Crack 11-15mm   $22";
            this.button8.UseVisualStyleBackColor = true;
            this.button8.Click += new System.EventHandler(this.CheckedChanged);
            this.button8.MouseEnter += new System.EventHandler(this.hoverPrice);
            this.button8.MouseLeave += new System.EventHandler(this.hoverPrice);
            // 
            // button9
            // 
            this.button9.Location = new System.Drawing.Point(6, 45);
            this.button9.Name = "button9";
            this.button9.Size = new System.Drawing.Size(97, 23);
            this.button9.TabIndex = 23;
            this.button9.Text = "Crack 1-10mm   $18";
            this.button9.UseVisualStyleBackColor = true;
            this.button9.Click += new System.EventHandler(this.CheckedChanged);
            this.button9.MouseEnter += new System.EventHandler(this.hoverPrice);
            this.button9.MouseLeave += new System.EventHandler(this.hoverPrice);
            // 
            // button5
            // 
            this.button5.Location = new System.Drawing.Point(321, 19);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(97, 23);
            this.button5.TabIndex = 22;
            this.button5.Text = "Dent 16mm >   ";
            this.button5.UseVisualStyleBackColor = true;
            this.button5.Click += new System.EventHandler(this.CheckedChanged);
            this.button5.MouseEnter += new System.EventHandler(this.hoverPrice);
            this.button5.MouseLeave += new System.EventHandler(this.hoverPrice);
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(218, 19);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(97, 23);
            this.button4.TabIndex = 21;
            this.button4.Text = "Dent 11-15mm   $46";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.CheckedChanged);
            this.button4.MouseEnter += new System.EventHandler(this.hoverPrice);
            this.button4.MouseLeave += new System.EventHandler(this.hoverPrice);
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(115, 19);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(97, 23);
            this.button3.TabIndex = 20;
            this.button3.Text = "Dent 6-10mm   $38";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.CheckedChanged);
            this.button3.MouseEnter += new System.EventHandler(this.hoverPrice);
            this.button3.MouseLeave += new System.EventHandler(this.hoverPrice);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(6, 19);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(103, 23);
            this.button2.TabIndex = 19;
            this.button2.Text = "Dent 1-5mm   $26";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.CheckedChanged);
            this.button2.MouseEnter += new System.EventHandler(this.hoverPrice);
            this.button2.MouseLeave += new System.EventHandler(this.hoverPrice);
            // 
            // button25
            // 
            this.button25.Location = new System.Drawing.Point(399, 19);
            this.button25.Name = "button25";
            this.button25.Size = new System.Drawing.Size(93, 23);
            this.button25.TabIndex = 50;
            this.button25.Text = "Unused";
            this.button25.UseVisualStyleBackColor = true;
            this.button25.Click += new System.EventHandler(this.CheckedChanged);
            this.button25.MouseEnter += new System.EventHandler(this.hoverPrice);
            this.button25.MouseLeave += new System.EventHandler(this.hoverPrice);
            // 
            // button18
            // 
            this.button18.Location = new System.Drawing.Point(300, 48);
            this.button18.Name = "button18";
            this.button18.Size = new System.Drawing.Size(93, 23);
            this.button18.TabIndex = 49;
            this.button18.Text = "Unused";
            this.button18.UseVisualStyleBackColor = true;
            this.button18.Click += new System.EventHandler(this.CheckedChanged);
            this.button18.MouseEnter += new System.EventHandler(this.hoverPrice);
            this.button18.MouseLeave += new System.EventHandler(this.hoverPrice);
            // 
            // button19
            // 
            this.button19.Location = new System.Drawing.Point(201, 48);
            this.button19.Name = "button19";
            this.button19.Size = new System.Drawing.Size(93, 23);
            this.button19.TabIndex = 48;
            this.button19.Text = "Unused";
            this.button19.UseVisualStyleBackColor = true;
            this.button19.Click += new System.EventHandler(this.CheckedChanged);
            this.button19.MouseEnter += new System.EventHandler(this.hoverPrice);
            this.button19.MouseLeave += new System.EventHandler(this.hoverPrice);
            // 
            // button20
            // 
            this.button20.Location = new System.Drawing.Point(102, 48);
            this.button20.Name = "button20";
            this.button20.Size = new System.Drawing.Size(93, 23);
            this.button20.TabIndex = 47;
            this.button20.Text = "Unused";
            this.button20.UseVisualStyleBackColor = true;
            this.button20.Click += new System.EventHandler(this.CheckedChanged);
            this.button20.MouseEnter += new System.EventHandler(this.hoverPrice);
            this.button20.MouseLeave += new System.EventHandler(this.hoverPrice);
            // 
            // button17
            // 
            this.button17.Location = new System.Drawing.Point(300, 19);
            this.button17.Name = "button17";
            this.button17.Size = new System.Drawing.Size(93, 23);
            this.button17.TabIndex = 41;
            this.button17.Text = "Other";
            this.button17.UseVisualStyleBackColor = true;
            this.button17.Click += new System.EventHandler(this.CheckedChanged);
            this.button17.MouseEnter += new System.EventHandler(this.hoverPrice);
            this.button17.MouseLeave += new System.EventHandler(this.hoverPrice);
            // 
            // button16
            // 
            this.button16.Location = new System.Drawing.Point(201, 19);
            this.button16.Name = "button16";
            this.button16.Size = new System.Drawing.Size(93, 23);
            this.button16.TabIndex = 40;
            this.button16.Text = "Centre Bore     $55";
            this.button16.UseVisualStyleBackColor = true;
            this.button16.Click += new System.EventHandler(this.CheckedChanged);
            this.button16.MouseEnter += new System.EventHandler(this.hoverPrice);
            this.button16.MouseLeave += new System.EventHandler(this.hoverPrice);
            // 
            // button15
            // 
            this.button15.Location = new System.Drawing.Point(102, 19);
            this.button15.Name = "button15";
            this.button15.Size = new System.Drawing.Size(93, 23);
            this.button15.TabIndex = 39;
            this.button15.Text = "Sand Blast    $45";
            this.button15.UseVisualStyleBackColor = true;
            this.button15.Click += new System.EventHandler(this.CheckedChanged);
            this.button15.MouseEnter += new System.EventHandler(this.hoverPrice);
            this.button15.MouseLeave += new System.EventHandler(this.hoverPrice);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(2, 19);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(93, 23);
            this.button1.TabIndex = 38;
            this.button1.Text = "Powder Coating    $85";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.CheckedChanged);
            this.button1.MouseEnter += new System.EventHandler(this.hoverPrice);
            this.button1.MouseLeave += new System.EventHandler(this.hoverPrice);
            // 
            // button14
            // 
            this.button14.Location = new System.Drawing.Point(702, 77);
            this.button14.Name = "button14";
            this.button14.Size = new System.Drawing.Size(107, 23);
            this.button14.TabIndex = 37;
            this.button14.Text = "Other weld 81mm >     $100";
            this.button14.UseVisualStyleBackColor = true;
            this.button14.Click += new System.EventHandler(this.CheckedChanged);
            this.button14.MouseEnter += new System.EventHandler(this.hoverPrice);
            this.button14.MouseLeave += new System.EventHandler(this.hoverPrice);
            // 
            // button12
            // 
            this.button12.Location = new System.Drawing.Point(577, 77);
            this.button12.Name = "button12";
            this.button12.Size = new System.Drawing.Size(119, 23);
            this.button12.TabIndex = 36;
            this.button12.Text = "Other weld 61-80mm     $80";
            this.button12.UseVisualStyleBackColor = true;
            this.button12.Click += new System.EventHandler(this.CheckedChanged);
            this.button12.MouseEnter += new System.EventHandler(this.hoverPrice);
            this.button12.MouseLeave += new System.EventHandler(this.hoverPrice);
            // 
            // button13
            // 
            this.button13.Location = new System.Drawing.Point(457, 77);
            this.button13.Name = "button13";
            this.button13.Size = new System.Drawing.Size(115, 23);
            this.button13.TabIndex = 35;
            this.button13.Text = "Other weld 41-60mm    $60";
            this.button13.UseVisualStyleBackColor = true;
            this.button13.Click += new System.EventHandler(this.CheckedChanged);
            this.button13.MouseEnter += new System.EventHandler(this.hoverPrice);
            this.button13.MouseLeave += new System.EventHandler(this.hoverPrice);
            // 
            // button11
            // 
            this.button11.Location = new System.Drawing.Point(334, 77);
            this.button11.Name = "button11";
            this.button11.Size = new System.Drawing.Size(119, 23);
            this.button11.TabIndex = 34;
            this.button11.Text = "Other weld 21-40mm     $40";
            this.button11.UseVisualStyleBackColor = true;
            this.button11.Click += new System.EventHandler(this.CheckedChanged);
            this.button11.MouseEnter += new System.EventHandler(this.hoverPrice);
            this.button11.MouseLeave += new System.EventHandler(this.hoverPrice);
            // 
            // button45
            // 
            this.button45.Location = new System.Drawing.Point(201, 106);
            this.button45.Name = "button45";
            this.button45.Size = new System.Drawing.Size(93, 23);
            this.button45.TabIndex = 33;
            this.button45.Text = "CNC facing #3      $92";
            this.button45.UseVisualStyleBackColor = true;
            this.button45.Click += new System.EventHandler(this.CheckedChanged);
            this.button45.MouseEnter += new System.EventHandler(this.hoverPrice);
            this.button45.MouseLeave += new System.EventHandler(this.hoverPrice);
            // 
            // button44
            // 
            this.button44.Location = new System.Drawing.Point(103, 106);
            this.button44.Name = "button44";
            this.button44.Size = new System.Drawing.Size(92, 23);
            this.button44.TabIndex = 32;
            this.button44.Text = "CNC facing #2        $72";
            this.button44.UseVisualStyleBackColor = true;
            this.button44.Click += new System.EventHandler(this.CheckedChanged);
            this.button44.MouseEnter += new System.EventHandler(this.hoverPrice);
            this.button44.MouseLeave += new System.EventHandler(this.hoverPrice);
            // 
            // button43
            // 
            this.button43.Location = new System.Drawing.Point(3, 106);
            this.button43.Name = "button43";
            this.button43.Size = new System.Drawing.Size(92, 23);
            this.button43.TabIndex = 31;
            this.button43.Text = "CNC facing #1  $55";
            this.button43.UseVisualStyleBackColor = true;
            this.button43.Click += new System.EventHandler(this.CheckedChanged);
            this.button43.MouseEnter += new System.EventHandler(this.hoverPrice);
            this.button43.MouseLeave += new System.EventHandler(this.hoverPrice);
            // 
            // button40
            // 
            this.button40.Location = new System.Drawing.Point(219, 77);
            this.button40.Name = "button40";
            this.button40.Size = new System.Drawing.Size(111, 23);
            this.button40.TabIndex = 30;
            this.button40.Text = "Other weld 1-20mm     $20";
            this.button40.UseVisualStyleBackColor = true;
            this.button40.Click += new System.EventHandler(this.CheckedChanged);
            this.button40.MouseEnter += new System.EventHandler(this.hoverPrice);
            this.button40.MouseLeave += new System.EventHandler(this.hoverPrice);
            // 
            // button42
            // 
            this.button42.Location = new System.Drawing.Point(102, 77);
            this.button42.Name = "button42";
            this.button42.Size = new System.Drawing.Size(111, 23);
            this.button42.TabIndex = 29;
            this.button42.Text = "Machining tyre bead    $12";
            this.button42.UseVisualStyleBackColor = true;
            this.button42.Click += new System.EventHandler(this.CheckedChanged);
            this.button42.MouseEnter += new System.EventHandler(this.hoverPrice);
            this.button42.MouseLeave += new System.EventHandler(this.hoverPrice);
            // 
            // button41
            // 
            this.button41.Location = new System.Drawing.Point(3, 77);
            this.button41.Name = "button41";
            this.button41.Size = new System.Drawing.Size(93, 23);
            this.button41.TabIndex = 28;
            this.button41.Text = "Remove curbing    $35";
            this.button41.UseVisualStyleBackColor = true;
            this.button41.Click += new System.EventHandler(this.CheckedChanged);
            this.button41.MouseEnter += new System.EventHandler(this.hoverPrice);
            this.button41.MouseLeave += new System.EventHandler(this.hoverPrice);
            // 
            // groupBox7
            // 
            this.groupBox7.Controls.Add(this.button79);
            this.groupBox7.Controls.Add(this.button78);
            this.groupBox7.Controls.Add(this.button77);
            this.groupBox7.Controls.Add(this.button75);
            this.groupBox7.Controls.Add(this.button76);
            this.groupBox7.Controls.Add(this.button74);
            this.groupBox7.Controls.Add(this.button73);
            this.groupBox7.Controls.Add(this.button31);
            this.groupBox7.Controls.Add(this.button32);
            this.groupBox7.Controls.Add(this.button33);
            this.groupBox7.Controls.Add(this.button34);
            this.groupBox7.Controls.Add(this.button35);
            this.groupBox7.Controls.Add(this.button36);
            this.groupBox7.Controls.Add(this.button37);
            this.groupBox7.Controls.Add(this.button38);
            this.groupBox7.Controls.Add(this.button39);
            this.groupBox7.Location = new System.Drawing.Point(6, 257);
            this.groupBox7.Name = "groupBox7";
            this.groupBox7.Size = new System.Drawing.Size(803, 74);
            this.groupBox7.TabIndex = 24;
            this.groupBox7.TabStop = false;
            this.groupBox7.Text = "Front skirt damage repair";
            this.groupBox7.Enter += new System.EventHandler(this.groupBox7_Enter);
            // 
            // button31
            // 
            this.button31.Location = new System.Drawing.Point(421, 45);
            this.button31.Name = "button31";
            this.button31.Size = new System.Drawing.Size(89, 23);
            this.button31.TabIndex = 27;
            this.button31.Text = "Crack 31mm >   ";
            this.button31.UseVisualStyleBackColor = true;
            this.button31.Click += new System.EventHandler(this.CheckedChanged);
            this.button31.MouseEnter += new System.EventHandler(this.hoverPrice);
            this.button31.MouseLeave += new System.EventHandler(this.hoverPrice);
            // 
            // button32
            // 
            this.button32.Location = new System.Drawing.Point(318, 45);
            this.button32.Name = "button32";
            this.button32.Size = new System.Drawing.Size(97, 23);
            this.button32.TabIndex = 26;
            this.button32.Text = "Crack 21-30mm  $72";
            this.button32.UseVisualStyleBackColor = true;
            this.button32.Click += new System.EventHandler(this.CheckedChanged);
            this.button32.MouseEnter += new System.EventHandler(this.hoverPrice);
            this.button32.MouseLeave += new System.EventHandler(this.hoverPrice);
            // 
            // button33
            // 
            this.button33.Location = new System.Drawing.Point(215, 45);
            this.button33.Name = "button33";
            this.button33.Size = new System.Drawing.Size(97, 23);
            this.button33.TabIndex = 25;
            this.button33.Text = "Crack 16-20mm   $64";
            this.button33.UseVisualStyleBackColor = true;
            this.button33.Click += new System.EventHandler(this.CheckedChanged);
            this.button33.MouseEnter += new System.EventHandler(this.hoverPrice);
            this.button33.MouseLeave += new System.EventHandler(this.hoverPrice);
            // 
            // button34
            // 
            this.button34.Location = new System.Drawing.Point(112, 45);
            this.button34.Name = "button34";
            this.button34.Size = new System.Drawing.Size(97, 23);
            this.button34.TabIndex = 24;
            this.button34.Text = "Crack 11-15mm   $56";
            this.button34.UseVisualStyleBackColor = true;
            this.button34.Click += new System.EventHandler(this.CheckedChanged);
            this.button34.MouseEnter += new System.EventHandler(this.hoverPrice);
            this.button34.MouseLeave += new System.EventHandler(this.hoverPrice);
            // 
            // button35
            // 
            this.button35.Location = new System.Drawing.Point(9, 45);
            this.button35.Name = "button35";
            this.button35.Size = new System.Drawing.Size(97, 23);
            this.button35.TabIndex = 23;
            this.button35.Text = "Crack 1-10mm   $48";
            this.button35.UseVisualStyleBackColor = true;
            this.button35.Click += new System.EventHandler(this.CheckedChanged);
            this.button35.MouseEnter += new System.EventHandler(this.hoverPrice);
            this.button35.MouseLeave += new System.EventHandler(this.hoverPrice);
            // 
            // button36
            // 
            this.button36.Location = new System.Drawing.Point(318, 19);
            this.button36.Name = "button36";
            this.button36.Size = new System.Drawing.Size(97, 23);
            this.button36.TabIndex = 22;
            this.button36.Text = "Dent 16mm >   ";
            this.button36.UseVisualStyleBackColor = true;
            this.button36.Click += new System.EventHandler(this.CheckedChanged);
            this.button36.MouseEnter += new System.EventHandler(this.hoverPrice);
            this.button36.MouseLeave += new System.EventHandler(this.hoverPrice);
            // 
            // button37
            // 
            this.button37.Location = new System.Drawing.Point(215, 19);
            this.button37.Name = "button37";
            this.button37.Size = new System.Drawing.Size(97, 23);
            this.button37.TabIndex = 21;
            this.button37.Text = "Dent 11-15mm   $65";
            this.button37.UseVisualStyleBackColor = true;
            this.button37.Click += new System.EventHandler(this.CheckedChanged);
            this.button37.MouseEnter += new System.EventHandler(this.hoverPrice);
            this.button37.MouseLeave += new System.EventHandler(this.hoverPrice);
            // 
            // button38
            // 
            this.button38.Location = new System.Drawing.Point(112, 19);
            this.button38.Name = "button38";
            this.button38.Size = new System.Drawing.Size(97, 23);
            this.button38.TabIndex = 20;
            this.button38.Text = "Dent 6-10mm   $50";
            this.button38.UseVisualStyleBackColor = true;
            this.button38.Click += new System.EventHandler(this.CheckedChanged);
            this.button38.MouseEnter += new System.EventHandler(this.hoverPrice);
            this.button38.MouseLeave += new System.EventHandler(this.hoverPrice);
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
            this.button39.MouseEnter += new System.EventHandler(this.hoverPrice);
            this.button39.MouseLeave += new System.EventHandler(this.hoverPrice);
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
            this.checkBox21.MouseEnter += new System.EventHandler(this.hoverPrice);
            this.checkBox21.MouseLeave += new System.EventHandler(this.hoverPrice);
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
            this.checkBox20.MouseEnter += new System.EventHandler(this.hoverPrice);
            this.checkBox20.MouseLeave += new System.EventHandler(this.hoverPrice);
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
            this.checkBox22.MouseEnter += new System.EventHandler(this.hoverPrice);
            this.checkBox22.MouseLeave += new System.EventHandler(this.hoverPrice);
            // 
            // overridePrice
            // 
            this.overridePrice.Location = new System.Drawing.Point(499, 582);
            this.overridePrice.Name = "overridePrice";
            this.overridePrice.Size = new System.Drawing.Size(52, 20);
            this.overridePrice.TabIndex = 42;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(423, 585);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(74, 13);
            this.label1.TabIndex = 43;
            this.label1.Text = "Override Price";
            // 
            // overrideControlTextLabel
            // 
            this.overrideControlTextLabel.AutoSize = true;
            this.overrideControlTextLabel.Location = new System.Drawing.Point(570, 585);
            this.overrideControlTextLabel.Name = "overrideControlTextLabel";
            this.overrideControlTextLabel.Size = new System.Drawing.Size(105, 13);
            this.overrideControlTextLabel.TabIndex = 45;
            this.overrideControlTextLabel.Text = "Override Button Text";
            // 
            // overrideControlText
            // 
            this.overrideControlText.Location = new System.Drawing.Point(679, 582);
            this.overrideControlText.Name = "overrideControlText";
            this.overrideControlText.Size = new System.Drawing.Size(132, 20);
            this.overrideControlText.TabIndex = 44;
            // 
            // button21
            // 
            this.button21.Location = new System.Drawing.Point(598, 106);
            this.button21.Name = "button21";
            this.button21.Size = new System.Drawing.Size(93, 23);
            this.button21.TabIndex = 63;
            this.button21.Text = "Unused";
            this.button21.UseVisualStyleBackColor = true;
            this.button21.Click += new System.EventHandler(this.CheckedChanged);
            this.button21.MouseEnter += new System.EventHandler(this.hoverPrice);
            this.button21.MouseLeave += new System.EventHandler(this.hoverPrice);
            // 
            // button47
            // 
            this.button47.Location = new System.Drawing.Point(499, 106);
            this.button47.Name = "button47";
            this.button47.Size = new System.Drawing.Size(93, 23);
            this.button47.TabIndex = 62;
            this.button47.Text = "Unused";
            this.button47.UseVisualStyleBackColor = true;
            this.button47.Click += new System.EventHandler(this.CheckedChanged);
            this.button47.MouseEnter += new System.EventHandler(this.hoverPrice);
            this.button47.MouseLeave += new System.EventHandler(this.hoverPrice);
            // 
            // button48
            // 
            this.button48.Location = new System.Drawing.Point(400, 106);
            this.button48.Name = "button48";
            this.button48.Size = new System.Drawing.Size(93, 23);
            this.button48.TabIndex = 61;
            this.button48.Text = "Unused";
            this.button48.UseVisualStyleBackColor = true;
            this.button48.Click += new System.EventHandler(this.CheckedChanged);
            this.button48.MouseEnter += new System.EventHandler(this.hoverPrice);
            this.button48.MouseLeave += new System.EventHandler(this.hoverPrice);
            // 
            // button49
            // 
            this.button49.Location = new System.Drawing.Point(300, 106);
            this.button49.Name = "button49";
            this.button49.Size = new System.Drawing.Size(93, 23);
            this.button49.TabIndex = 60;
            this.button49.Text = "Unused";
            this.button49.UseVisualStyleBackColor = true;
            this.button49.Click += new System.EventHandler(this.CheckedChanged);
            this.button49.MouseEnter += new System.EventHandler(this.hoverPrice);
            this.button49.MouseLeave += new System.EventHandler(this.hoverPrice);
            // 
            // button50
            // 
            this.button50.Location = new System.Drawing.Point(216, 44);
            this.button50.Name = "button50";
            this.button50.Size = new System.Drawing.Size(92, 23);
            this.button50.TabIndex = 33;
            this.button50.Text = "Unused";
            this.button50.UseVisualStyleBackColor = true;
            this.button50.Click += new System.EventHandler(this.CheckedChanged);
            this.button50.MouseEnter += new System.EventHandler(this.hoverPrice);
            this.button50.MouseLeave += new System.EventHandler(this.hoverPrice);
            // 
            // button51
            // 
            this.button51.Location = new System.Drawing.Point(110, 44);
            this.button51.Name = "button51";
            this.button51.Size = new System.Drawing.Size(97, 23);
            this.button51.TabIndex = 32;
            this.button51.Text = "Unused";
            this.button51.UseVisualStyleBackColor = true;
            this.button51.Click += new System.EventHandler(this.CheckedChanged);
            this.button51.MouseEnter += new System.EventHandler(this.hoverPrice);
            this.button51.MouseLeave += new System.EventHandler(this.hoverPrice);
            // 
            // button52
            // 
            this.button52.Location = new System.Drawing.Point(7, 44);
            this.button52.Name = "button52";
            this.button52.Size = new System.Drawing.Size(97, 23);
            this.button52.TabIndex = 31;
            this.button52.Text = "Unused";
            this.button52.UseVisualStyleBackColor = true;
            this.button52.Click += new System.EventHandler(this.CheckedChanged);
            this.button52.MouseEnter += new System.EventHandler(this.hoverPrice);
            this.button52.MouseLeave += new System.EventHandler(this.hoverPrice);
            // 
            // button53
            // 
            this.button53.Location = new System.Drawing.Point(695, 106);
            this.button53.Name = "button53";
            this.button53.Size = new System.Drawing.Size(95, 23);
            this.button53.TabIndex = 64;
            this.button53.Text = "Unused";
            this.button53.UseVisualStyleBackColor = true;
            this.button53.Click += new System.EventHandler(this.CheckedChanged);
            this.button53.MouseEnter += new System.EventHandler(this.hoverPrice);
            this.button53.MouseLeave += new System.EventHandler(this.hoverPrice);
            // 
            // button54
            // 
            this.button54.Location = new System.Drawing.Point(399, 135);
            this.button54.Name = "button54";
            this.button54.Size = new System.Drawing.Size(95, 23);
            this.button54.TabIndex = 69;
            this.button54.Text = "Unused";
            this.button54.UseVisualStyleBackColor = true;
            this.button54.Click += new System.EventHandler(this.CheckedChanged);
            this.button54.MouseEnter += new System.EventHandler(this.hoverPrice);
            this.button54.MouseLeave += new System.EventHandler(this.hoverPrice);
            // 
            // button55
            // 
            this.button55.Location = new System.Drawing.Point(302, 135);
            this.button55.Name = "button55";
            this.button55.Size = new System.Drawing.Size(93, 23);
            this.button55.TabIndex = 68;
            this.button55.Text = "Unused";
            this.button55.UseVisualStyleBackColor = true;
            this.button55.Click += new System.EventHandler(this.CheckedChanged);
            this.button55.MouseEnter += new System.EventHandler(this.hoverPrice);
            this.button55.MouseLeave += new System.EventHandler(this.hoverPrice);
            // 
            // button56
            // 
            this.button56.Location = new System.Drawing.Point(203, 135);
            this.button56.Name = "button56";
            this.button56.Size = new System.Drawing.Size(93, 23);
            this.button56.TabIndex = 67;
            this.button56.Text = "Unused";
            this.button56.UseVisualStyleBackColor = true;
            this.button56.Click += new System.EventHandler(this.CheckedChanged);
            this.button56.MouseEnter += new System.EventHandler(this.hoverPrice);
            this.button56.MouseLeave += new System.EventHandler(this.hoverPrice);
            // 
            // button57
            // 
            this.button57.Location = new System.Drawing.Point(104, 135);
            this.button57.Name = "button57";
            this.button57.Size = new System.Drawing.Size(93, 23);
            this.button57.TabIndex = 66;
            this.button57.Text = "Unused";
            this.button57.UseVisualStyleBackColor = true;
            this.button57.Click += new System.EventHandler(this.CheckedChanged);
            this.button57.MouseEnter += new System.EventHandler(this.hoverPrice);
            this.button57.MouseLeave += new System.EventHandler(this.hoverPrice);
            // 
            // button58
            // 
            this.button58.Location = new System.Drawing.Point(4, 135);
            this.button58.Name = "button58";
            this.button58.Size = new System.Drawing.Size(93, 23);
            this.button58.TabIndex = 65;
            this.button58.Text = "Unused";
            this.button58.UseVisualStyleBackColor = true;
            this.button58.Click += new System.EventHandler(this.CheckedChanged);
            this.button58.MouseEnter += new System.EventHandler(this.hoverPrice);
            this.button58.MouseLeave += new System.EventHandler(this.hoverPrice);
            // 
            // button59
            // 
            this.button59.Location = new System.Drawing.Point(697, 135);
            this.button59.Name = "button59";
            this.button59.Size = new System.Drawing.Size(95, 23);
            this.button59.TabIndex = 72;
            this.button59.Text = "Unused";
            this.button59.UseVisualStyleBackColor = true;
            this.button59.Click += new System.EventHandler(this.CheckedChanged);
            this.button59.MouseEnter += new System.EventHandler(this.hoverPrice);
            this.button59.MouseLeave += new System.EventHandler(this.hoverPrice);
            // 
            // button60
            // 
            this.button60.Location = new System.Drawing.Point(600, 135);
            this.button60.Name = "button60";
            this.button60.Size = new System.Drawing.Size(93, 23);
            this.button60.TabIndex = 71;
            this.button60.Text = "Unused";
            this.button60.UseVisualStyleBackColor = true;
            this.button60.Click += new System.EventHandler(this.CheckedChanged);
            this.button60.MouseEnter += new System.EventHandler(this.hoverPrice);
            this.button60.MouseLeave += new System.EventHandler(this.hoverPrice);
            // 
            // button61
            // 
            this.button61.Location = new System.Drawing.Point(501, 135);
            this.button61.Name = "button61";
            this.button61.Size = new System.Drawing.Size(93, 23);
            this.button61.TabIndex = 70;
            this.button61.Text = "Unused";
            this.button61.UseVisualStyleBackColor = true;
            this.button61.Click += new System.EventHandler(this.CheckedChanged);
            this.button61.MouseEnter += new System.EventHandler(this.hoverPrice);
            this.button61.MouseLeave += new System.EventHandler(this.hoverPrice);
            // 
            // button62
            // 
            this.button62.Location = new System.Drawing.Point(11, 44);
            this.button62.Name = "button62";
            this.button62.Size = new System.Drawing.Size(94, 23);
            this.button62.TabIndex = 9;
            this.button62.Text = "Unused";
            this.button62.UseVisualStyleBackColor = true;
            this.button62.Click += new System.EventHandler(this.CheckedChanged);
            this.button62.MouseEnter += new System.EventHandler(this.hoverPrice);
            this.button62.MouseLeave += new System.EventHandler(this.hoverPrice);
            // 
            // button63
            // 
            this.button63.Location = new System.Drawing.Point(111, 44);
            this.button63.Name = "button63";
            this.button63.Size = new System.Drawing.Size(94, 23);
            this.button63.TabIndex = 10;
            this.button63.Text = "Unused";
            this.button63.UseVisualStyleBackColor = true;
            this.button63.Click += new System.EventHandler(this.CheckedChanged);
            this.button63.MouseEnter += new System.EventHandler(this.hoverPrice);
            this.button63.MouseLeave += new System.EventHandler(this.hoverPrice);
            // 
            // button64
            // 
            this.button64.Location = new System.Drawing.Point(311, 44);
            this.button64.Name = "button64";
            this.button64.Size = new System.Drawing.Size(94, 23);
            this.button64.TabIndex = 12;
            this.button64.Text = "Unused";
            this.button64.UseVisualStyleBackColor = true;
            this.button64.Click += new System.EventHandler(this.CheckedChanged);
            this.button64.MouseEnter += new System.EventHandler(this.hoverPrice);
            this.button64.MouseLeave += new System.EventHandler(this.hoverPrice);
            // 
            // button65
            // 
            this.button65.Location = new System.Drawing.Point(211, 44);
            this.button65.Name = "button65";
            this.button65.Size = new System.Drawing.Size(94, 23);
            this.button65.TabIndex = 11;
            this.button65.Text = "Unused";
            this.button65.UseVisualStyleBackColor = true;
            this.button65.Click += new System.EventHandler(this.CheckedChanged);
            this.button65.MouseEnter += new System.EventHandler(this.hoverPrice);
            this.button65.MouseLeave += new System.EventHandler(this.hoverPrice);
            // 
            // currentPriceLabel
            // 
            this.currentPriceLabel.AutoSize = true;
            this.currentPriceLabel.Location = new System.Drawing.Point(172, 585);
            this.currentPriceLabel.MaximumSize = new System.Drawing.Size(120, 13);
            this.currentPriceLabel.MinimumSize = new System.Drawing.Size(60, 13);
            this.currentPriceLabel.Name = "currentPriceLabel";
            this.currentPriceLabel.Size = new System.Drawing.Size(68, 13);
            this.currentPriceLabel.TabIndex = 46;
            this.currentPriceLabel.Text = "Current Price";
            this.currentPriceLabel.Click += new System.EventHandler(this.label2_Click);
            // 
            // currentPrice
            // 
            this.currentPrice.AutoSize = true;
            this.currentPrice.BackColor = System.Drawing.SystemColors.Info;
            this.currentPrice.Font = new System.Drawing.Font("Courier New", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.currentPrice.ForeColor = System.Drawing.Color.Red;
            this.currentPrice.Location = new System.Drawing.Point(310, 583);
            this.currentPrice.MaximumSize = new System.Drawing.Size(120, 20);
            this.currentPrice.MinimumSize = new System.Drawing.Size(100, 20);
            this.currentPrice.Name = "currentPrice";
            this.currentPrice.Size = new System.Drawing.Size(100, 20);
            this.currentPrice.TabIndex = 47;
            this.currentPrice.Text = "Hover";
            this.currentPrice.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // button66
            // 
            this.button66.Location = new System.Drawing.Point(424, 19);
            this.button66.Name = "button66";
            this.button66.Size = new System.Drawing.Size(97, 23);
            this.button66.TabIndex = 28;
            this.button66.Text = "Unused";
            this.button66.UseVisualStyleBackColor = true;
            this.button66.Click += new System.EventHandler(this.CheckedChanged);
            this.button66.MouseEnter += new System.EventHandler(this.hoverPrice);
            this.button66.MouseLeave += new System.EventHandler(this.hoverPrice);
            // 
            // button67
            // 
            this.button67.Location = new System.Drawing.Point(526, 19);
            this.button67.Name = "button67";
            this.button67.Size = new System.Drawing.Size(97, 23);
            this.button67.TabIndex = 29;
            this.button67.Text = "Unused";
            this.button67.UseVisualStyleBackColor = true;
            this.button67.Click += new System.EventHandler(this.CheckedChanged);
            this.button67.MouseEnter += new System.EventHandler(this.hoverPrice);
            this.button67.MouseLeave += new System.EventHandler(this.hoverPrice);
            // 
            // button68
            // 
            this.button68.Location = new System.Drawing.Point(618, 45);
            this.button68.Name = "button68";
            this.button68.Size = new System.Drawing.Size(97, 23);
            this.button68.TabIndex = 31;
            this.button68.Text = "Unused";
            this.button68.UseVisualStyleBackColor = true;
            this.button68.Click += new System.EventHandler(this.CheckedChanged);
            this.button68.MouseEnter += new System.EventHandler(this.hoverPrice);
            this.button68.MouseLeave += new System.EventHandler(this.hoverPrice);
            // 
            // button69
            // 
            this.button69.Location = new System.Drawing.Point(516, 45);
            this.button69.Name = "button69";
            this.button69.Size = new System.Drawing.Size(97, 23);
            this.button69.TabIndex = 30;
            this.button69.Text = "Unused";
            this.button69.UseVisualStyleBackColor = true;
            this.button69.Click += new System.EventHandler(this.CheckedChanged);
            this.button69.MouseEnter += new System.EventHandler(this.hoverPrice);
            this.button69.MouseLeave += new System.EventHandler(this.hoverPrice);
            // 
            // button70
            // 
            this.button70.Location = new System.Drawing.Point(628, 19);
            this.button70.Name = "button70";
            this.button70.Size = new System.Drawing.Size(87, 23);
            this.button70.TabIndex = 32;
            this.button70.Text = "Unused";
            this.button70.UseVisualStyleBackColor = true;
            this.button70.Click += new System.EventHandler(this.CheckedChanged);
            this.button70.MouseEnter += new System.EventHandler(this.hoverPrice);
            this.button70.MouseLeave += new System.EventHandler(this.hoverPrice);
            // 
            // button71
            // 
            this.button71.Location = new System.Drawing.Point(717, 19);
            this.button71.Name = "button71";
            this.button71.Size = new System.Drawing.Size(81, 23);
            this.button71.TabIndex = 33;
            this.button71.Text = "Unused";
            this.button71.UseVisualStyleBackColor = true;
            this.button71.Click += new System.EventHandler(this.CheckedChanged);
            this.button71.MouseEnter += new System.EventHandler(this.hoverPrice);
            this.button71.MouseLeave += new System.EventHandler(this.hoverPrice);
            // 
            // button72
            // 
            this.button72.Location = new System.Drawing.Point(717, 45);
            this.button72.Name = "button72";
            this.button72.Size = new System.Drawing.Size(81, 23);
            this.button72.TabIndex = 34;
            this.button72.Text = "Unused";
            this.button72.UseVisualStyleBackColor = true;
            this.button72.Click += new System.EventHandler(this.CheckedChanged);
            this.button72.MouseEnter += new System.EventHandler(this.hoverPrice);
            this.button72.MouseLeave += new System.EventHandler(this.hoverPrice);
            // 
            // button73
            // 
            this.button73.Location = new System.Drawing.Point(421, 19);
            this.button73.Name = "button73";
            this.button73.Size = new System.Drawing.Size(97, 23);
            this.button73.TabIndex = 28;
            this.button73.Text = "Unused";
            this.button73.UseVisualStyleBackColor = true;
            this.button73.Click += new System.EventHandler(this.CheckedChanged);
            this.button73.MouseEnter += new System.EventHandler(this.hoverPrice);
            this.button73.MouseLeave += new System.EventHandler(this.hoverPrice);
            // 
            // button74
            // 
            this.button74.Location = new System.Drawing.Point(524, 19);
            this.button74.Name = "button74";
            this.button74.Size = new System.Drawing.Size(97, 23);
            this.button74.TabIndex = 29;
            this.button74.Text = "Unused";
            this.button74.UseVisualStyleBackColor = true;
            this.button74.Click += new System.EventHandler(this.CheckedChanged);
            this.button74.MouseEnter += new System.EventHandler(this.hoverPrice);
            this.button74.MouseLeave += new System.EventHandler(this.hoverPrice);
            // 
            // button75
            // 
            this.button75.Location = new System.Drawing.Point(619, 45);
            this.button75.Name = "button75";
            this.button75.Size = new System.Drawing.Size(97, 23);
            this.button75.TabIndex = 31;
            this.button75.Text = "Unused";
            this.button75.UseVisualStyleBackColor = true;
            this.button75.Click += new System.EventHandler(this.CheckedChanged);
            this.button75.MouseEnter += new System.EventHandler(this.hoverPrice);
            this.button75.MouseLeave += new System.EventHandler(this.hoverPrice);
            // 
            // button76
            // 
            this.button76.Location = new System.Drawing.Point(516, 45);
            this.button76.Name = "button76";
            this.button76.Size = new System.Drawing.Size(97, 23);
            this.button76.TabIndex = 30;
            this.button76.Text = "Unused";
            this.button76.UseVisualStyleBackColor = true;
            this.button76.Click += new System.EventHandler(this.CheckedChanged);
            this.button76.MouseEnter += new System.EventHandler(this.hoverPrice);
            this.button76.MouseLeave += new System.EventHandler(this.hoverPrice);
            // 
            // button77
            // 
            this.button77.Location = new System.Drawing.Point(627, 19);
            this.button77.Name = "button77";
            this.button77.Size = new System.Drawing.Size(97, 23);
            this.button77.TabIndex = 32;
            this.button77.Text = "Unused";
            this.button77.UseVisualStyleBackColor = true;
            this.button77.Click += new System.EventHandler(this.CheckedChanged);
            this.button77.MouseEnter += new System.EventHandler(this.hoverPrice);
            this.button77.MouseLeave += new System.EventHandler(this.hoverPrice);
            // 
            // button78
            // 
            this.button78.Location = new System.Drawing.Point(730, 19);
            this.button78.Name = "button78";
            this.button78.Size = new System.Drawing.Size(69, 23);
            this.button78.TabIndex = 33;
            this.button78.Text = "Unused";
            this.button78.UseVisualStyleBackColor = true;
            this.button78.Click += new System.EventHandler(this.CheckedChanged);
            this.button78.MouseEnter += new System.EventHandler(this.hoverPrice);
            this.button78.MouseLeave += new System.EventHandler(this.hoverPrice);
            // 
            // button79
            // 
            this.button79.Location = new System.Drawing.Point(722, 45);
            this.button79.Name = "button79";
            this.button79.Size = new System.Drawing.Size(75, 23);
            this.button79.TabIndex = 34;
            this.button79.Text = "Unused";
            this.button79.UseVisualStyleBackColor = true;
            this.button79.Click += new System.EventHandler(this.CheckedChanged);
            this.button79.MouseEnter += new System.EventHandler(this.hoverPrice);
            this.button79.MouseLeave += new System.EventHandler(this.hoverPrice);
            // 
            // JobTypePopup
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(834, 616);
            this.Controls.Add(this.currentPrice);
            this.Controls.Add(this.currentPriceLabel);
            this.Controls.Add(this.groupBox5);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.clearButton);
            this.Controls.Add(this.smiley);
            this.Controls.Add(this.overrideControlTextLabel);
            this.Controls.Add(this.overridePrice);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.overrideControlText);
            this.DoubleBuffered = true;
            this.Location = new System.Drawing.Point(20, 20);
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(1200, 800);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(850, 655);
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
            this.repairAndFinishing.ResumeLayout(false);
            this.wheelTyreService.ResumeLayout(false);
            this.groupBoxRearSkirtDamage.ResumeLayout(false);
            this.groupBox7.ResumeLayout(false);
            this.groupBox4.ResumeLayout(false);
            this.groupBox5.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.bindingSource1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
       
        private async void Form_Shown(object sender, EventArgs e)
        {
            if (!isWheelApp())
            {
                this.groupBox3.Visible = false;
                this.wheelTyreService.Visible = false;
                this.groupBox1.Visible = true;
                this.repairAndFinishing.Visible = false;
                this.groupBox4.Visible = true;
                this.groupBox5.Visible = true;
            }
            else
            {
                this.groupBox4.Visible = false;
                this.groupBox5.Visible = false;
                this.groupBox1.Visible = false;
                this.wheelTyreService.Visible = true;
                this.repairAndFinishing.Visible = true;
                this.groupBox2.Visible = false;
                //this.checkBox4.Visible = true;
                //this.SetupPricingFromDB();
                if (jobCard != null && jobCard.jobType[0].Text == "")
                {
                    setupOnLatheAutoPress();
                }
            }
            await this.getAllPrices();
        }
        private async System.Threading.Tasks.Task setupOnLatheAutoPress()
        {
            JobTypePopup.jobDetail = jobCard.jobDetail[0];
            JobTypePopup.jobType = jobCard.jobType[0];
            JobTypePopup.jobQty = jobCard.jobQty[0];
            JobTypePopup.jobPrice = jobCard.jobPrice[0];
            JobTypePopup.jobUnitPrice = jobCard.jobUnitPrice[0];
            await doCheckChange(setUpOnLathe);
          
            this.Close();

        }

        public static bool isCanada()
        {
            string countryCode = RegionInfo.CurrentRegion.TwoLetterISORegionName;
            return (countryCode == "CA");
            
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
            /*
            JobTypePopup.jobType = null;
            JobTypePopup.jobQty = null;
            JobTypePopup.jobUnitPrice = null;
            JobTypePopup.jobPrice = null;
            JobTypePopup.jobDetail = null;
            */

        }

        private void groupBox7_Enter(object sender, EventArgs e)
        {

        }

        private void button15_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }
    }
}

