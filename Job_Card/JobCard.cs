﻿namespace Job_Card
{
    using Job_Card.Properties;
    using System.Threading.Tasks;
    using Microsoft.Office.Interop.Word;
    using PresentationControls;
    using System = System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data;
    using MongoDB.Bson;
    using MongoDB.Bson.Serialization.Attributes;
    using MongoDB.Driver;
    using Guid = System.Guid;
    using Activator = System.Activator;
    using MailMessage = System.Net.Mail.MailMessage;
    using MidpointRounding = System.MidpointRounding;
    using TimeSpan = System.TimeSpan;
    using DBNull = System.DBNull;
    using Math = System.Math;
    using Exception = System.Exception;
    using EventHandler = System.EventHandler;
    using Environment = System.Environment;
    using EventArgs = System.EventArgs;
    using DateTime = System.DateTime;
    using System.Drawing;
    using Font = System.Drawing.Font;
    using Point = System.Drawing.Point;
    using Rectangle = System.Drawing.Rectangle;
    using System.Drawing.Drawing2D;
    using System.Drawing.Imaging;
    using System.Globalization;
    using System.IO;
    using System.Media;
    using System.Net;
    using System.Net.Mail;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Security.Principal;
    using System.Windows.Forms;
    using CheckBox = System.Windows.Forms.CheckBox;
    using System;
    using System.Timers;
    using System.Diagnostics;
    using AForge.Video;
    using AForge.Video.DirectShow;
    using MailKit.Net.Smtp;
    using MailKit;
    using MimeKit;

    public class JobCard : Form
    {
        FilterInfoCollection filterInfoCollection;
        VideoCaptureDevice videoCaptureDevice;
        private MongoClient client;
        private IMongoDatabase db;
        
        private bool amountValidating = false;
        private static Bitmap b;
        private Button btnAddWeek;
        private Button btnBrass;
        private Button btnBritt;
        private Button btnBronze;
        private Button btnCancelSearch;
        private Button btnChrome;
        private Button btnCollapseToggle;
        private Button btnCollect;
        private Button btnCopper;
        private Button btnCourier;
        private Button btnDuplicate;
        private Button btnEmail;
        private Button btnExit;
        private Button btnGeorge;
        private Button btnGold;
        private Button btnHenry;
        private Button btnIncompleteJobs;
        private Button btnLatestJob;
        private Button btnLockUnlock;
        private Button btnNavigateBack;
        private Button btnNavigateForward;
        private Button btnNewJob;
        private Button btnNextPhoto;
        private Button btnNickle;
        private Button btnPrintBusiness;
        private Button btnPrintCustomerCopy;
        private Button btnPrintForWork;
        private Button btnRakesh;
        private Button btnSatin;
        private Button btnSave;
        private Button btnSearchField;
        private Button btnSearchLists;
        private Button btnSilver;
        private Button btnTin;
        private Button btnToday;
        private Button btnTodayForDateCompleted;
        private Button btnUndo;
        private Button btnUnpaidCustomers;
        private Button btnCam1;
        private Button btnCam2;
        private CheckBox[] checkBox;
        private IContainer components = null;
        private bool compress = true;
        private List<Control> controls = new List<Control>();
        public static List<string> currentPhotoPaths;
        public static int currentPictureIndex;
        private DataGridView datagrid;

        public static string DBTable
        {
            get
            {
                if (JobTypePopup.isWheelApp())
                {
                    return @"jobsWheel";
                }
                else
                {
                    return @"jobs";
                }
            }
        }
        public static string DBPath
        {
            get
            {
                if (JobTypePopup.isWheelApp())
                {
                    return @"J:\jobWheelCard.mdb";
                }
                else
                {
                    return @"J:\jobCard.mdb";
                }
            }
        }
        private const int designHeight = 879;
        private const int designWidth = 1384;
        private const int detailCount = 33;
        private static string Disclaimer;
        private Dictionary<string, Control> fieldNameToControlMapping;
        private int freightIndex = 29;
        private GroupBox grpBoxPlating;
        private GroupBox grpBoxPolish;
        private int gstIndex = 31;
        private static readonly List<string> ImageExtensions;
        private string insertFieldsSql;
        private string insertValuesSql;
        private bool isLocked = true;
        private TextBox jobAddress;
        private CheckBox jobCompleted;
        private TextBox jobCustomer;
        private TextBox jobBusinessName;
        private TextBox jobDate;
        private TextBox jobDateCompleted;
        private TextBox jobDatePaid;
        private TextBox jobDateRequired;
        private DateTime jobDateValForPhoto;
        private TextBox jobDelivery;

        private TextBox jobEmail;
        private Label jobID;
        private TextBox jobNotes;
        private TextBox jobOrderNumber;
        private ComboBox jobPaymentBy;
        private TextBox jobPhone;
        private List<string> jobPhotos = null;

        private ComboBox jobReceivedFrom;
        public TextBox[] jobPrice;
        public TextBox[] jobQty;
        public TextBox[] jobDetail;
        public TextBox[] jobType;
        public TextBox[] jobUnitPrice;
        private Label[] label;
        private Label label1;
        private Label label10;
        private Label label11;
        private Label label12;
        private Label label13;
        private Label label14;
        private Label label2;
        private Label label3;
        private Label label4;
        private Label label5;
        private Label label6;
        private Label label7;
        private Label label8;
        private Label label9;
        private Label labelJobBusinessName;
        private static string lastFontName;
        private static int lastFontSize;
        private static FontStyle lastFontStyle;
        private int lastID = 0;
        private Label lblResults;
        private Label lblSearchOnField;
        private bool Loading = false;
        private string[] months = new string[] { "", "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" };
        private Dictionary<string, string> originalValues;
        private bool panelDragging = false;
        private Point panelDragStartPoint;
        private Point panelFinalLocation;
        private bool panelMoved = false;
        private Panel panelSearchField;
        private bool panelSetLocation = false;
        private PictureBox picPaid;
        internal PictureBox pictureBox1;
        public static string PicturePath;
        private int platingIndex = 0;
        private int polishIndex = 0;
        private Dictionary<Control, float> restoreFontSize = new Dictionary<Control, float>();
        private const float scaleFactor = 0.63f;
        private string searchFieldName;
        private static int searchRows;
        private TrackBar slider;
        private Color stripe = Color.FromArgb(0xff, 0xeb, 0xeb, 0xeb);
        private int subTotalIndex = 30;
        private static bool temporarilyDisableNewLineAtEnd;
        private int totalIndex = 0x20;
        private TextBox txtSearchField;
        private Dictionary<string, System.Type> types;
        private List<Control> undoList = new List<Control>();
        private string updateSql;
        private List<KeyValuePair<string, dynamic>> updateSqlSetList;
        private Button btnFussy;
        private Button btnRDAddressSurcharge;
        private System.Windows.Forms.Timer getLatestTimer;
        private TextBox jobFussyNotes;
        private CheckBox jobGoodReserved;
        private ComboBox cboCamera;
        internal PictureBox pictureBox2;
        private CheckBox jobQuotation;
        private Rectangle workingArea;

        static JobCard()
        {
            List<string> list = new List<string> {
                ".JPG",
                ".JPE",
                ".BMP",
                ".GIF",
                ".PNG",
                ".MOV",
                ".MP4"
            };
            ImageExtensions = list;
            if (System.Environment.MachineName == "TCSP4")
            {
                if (Directory.Exists("K:"))
                {
                    PicturePath = @"K:";//@"\\tcsp4\Kodak Pictures\";
                } else
                {
                    PicturePath = @"D:\Kodak Pictures\";
                }
                
            }
            else {
                PicturePath = @"K:";//@"\\tcsp4\Kodak Pictures\";
            }
            currentPictureIndex = 0;
            lastFontName = null;
            lastFontSize = -1;
            lastFontStyle = FontStyle.Regular;
            temporarilyDisableNewLineAtEnd = false;
            Disclaimer = "All work not collected within 3 months of completion will be sold to defer costs.  At "+JobCard.getBusinessName()+" we have a combined electroplating and polishing" + Environment.NewLine + "history of over 60 years. Advanced Chrome Platers Ltd treat all jobs with the utmost care and attention, however we take no responsibility for any adverse" + Environment.NewLine + "changes in the condition of items during stripping, polishing and/or plating processes.  Please also note that items held at our premises are not covered by our" + Environment.NewLine + "insurance for theft, fire etc, and you may wish to contact your insurance agent regarding cover for any valuable items during the time they are held on our" + Environment.NewLine + "premises.";
            searchRows = 0;
            b = null;
        }

        public static string getBusinessName()
        {
            if (JobTypePopup.isCanada())
            {
                return "WheelTec Alloy Wheel Repair";
            } else {
                return "Advanced Chrome Platers Ltd";
            }
        }

        public static bool isWheel = false; 
        public JobCard(string[] args)
        {
            if (args.Length > 0 && args[0].ToUpper().Contains("WHEEL"))
            {
                isWheel = true;
            }
            //DataAccess.connectMongoDb();
            //DataAccess.migrateJobCardAsync();
            //DataAccess.migrateFussyCustomerAsync();
            this.fieldNameToControlMapping = new Dictionary<string, Control>();
            this.originalValues = new Dictionary<string, string>();
            this.InitializeComponent();
            this.workingArea = Screen.PrimaryScreen.WorkingArea;
            base.Width = this.workingArea.Width;
            base.Left = this.workingArea.Left;
            base.Top = this.workingArea.Top;
            base.Height = this.workingArea.Height;
            this.InitializeArrayComponent();
            this.txtSearchField.KeyDown += new KeyEventHandler(this.tb_KeyDown);
            this.datagrid.AllowUserToAddRows = false;
            this.jobReceivedFrom.SelectedIndex = 0;
            this.jobPaymentBy.SelectedIndex = 0;
            this.types = DataAccess.GetFieldDataTypes();
            SizeF factor = new SizeF(((float)base.Width) / 1384f, ((float)base.Height) / 879f);
            foreach (object obj2 in base.Controls)
            {
                Control item = (Control)obj2;
                item.Scale(factor);
                Font font = new Font(item.Font.FontFamily.Name, item.Font.Size * (((float)base.Width) / 1384f), item.Font.Style);
                item.Font = font;
                string name = item.Name;
                if (this.types.ContainsKey(name))
                {
                    System.Type type = this.types[name];
                    if (item is TextBox)
                    {
                        item.TextChanged += new EventHandler(this.control_TextChanged);
                        ((TextBox)item).DoubleClick += new EventHandler(this.SingleSearch);
                    }
                    else if (item is CheckBox)
                    {
                        ((CheckBox)item).CheckedChanged += new EventHandler(this.control_TextChanged);
                        ((CheckBox)item).DoubleClick += new EventHandler(this.SingleSearch);
                    }
                    else if (item is Label)
                    {
                        ((Label)item).DoubleClick += new EventHandler(this.SingleSearch);
                    }
                    else if (item is ComboBox)
                    {
                        ((ComboBox)item).FlatStyle = FlatStyle.Flat;
                        ((ComboBox)item).TextChanged += new EventHandler(this.control_TextChanged);
                        ((ComboBox)item).DoubleClick += new EventHandler(this.SingleSearch);
                    }
                    this.controls.Add(item);
                    this.fieldNameToControlMapping[name] = item;
                    this.originalValues[name] = null;
                }
            }
            this.filterInfoCollection = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            if (cboCamera != null)
            {
                foreach (FilterInfo filterInfo in filterInfoCollection)
                {
                    this.cboCamera.Items.Add(filterInfo.Name);
                }
                if (this.filterInfoCollection.Count > 0)
                {
                    this.cboCamera.SelectedIndex = 0;
                }
            }
           
        }


        private void AddLine(RichTextBox r, string line)
        {
            this.AddLine(r, line, null, -1, FontStyle.Regular, 0);
        }

        private void AddLine(RichTextBox r, string line, FontStyle style)
        {
            this.AddLine(r, line, null, -1, style, 0);
        }

        private void AddLine(RichTextBox r, string line, int fontSize)
        {
            this.AddLine(r, line, null, fontSize, FontStyle.Regular, 0);
        }

        private void AddLine(RichTextBox r, string line, string fontName = null, int fontSize = -1, FontStyle style = FontStyle.Regular, int indent = 0)
        {
            int textLength = r.TextLength;
            if (fontName == null)
            {
                fontName = lastFontName;
                if (fontName == null)
                {
                    fontName = "Courier";
                }
            }
            else
            {
                lastFontName = fontName;
            }
            if (fontSize == -1)
            {
                fontSize = lastFontSize;
                if (fontSize == -1)
                {
                    fontSize = 11;
                }
            }
            else
            {
                lastFontSize = fontSize;
            }
            if (style == FontStyle.Regular)
            {
                style = lastFontStyle;
            }
            else
            {
                lastFontStyle = style;
            }
            Font font = new Font(fontName, fontSize * 0.63f, style);
            if (temporarilyDisableNewLineAtEnd)
            {
                temporarilyDisableNewLineAtEnd = false;
            }
            else
            {
                line = line + Environment.NewLine;
            }
            int num2 = r.TextLength;
            r.AppendText(line);
            r.SelectionStart = num2;
            int num3 = r.TextLength - num2;
            r.SelectionLength = num3;
            r.SelectionFont = font;
            if (indent > 0)
            {
                r.SelectionIndent += indent;
            }
        }

        private void AmountValidate(object sender, CancelEventArgs e)
        {
            if (!this.amountValidating)
            {
                try
                {
                    this.amountValidating = true;
                    TextBox box = null;
                    if (sender is TextBox)
                    {
                        box = (TextBox)sender;
                    }
                    else
                    {
                        return;
                    }
                    bool flag = box.Name.Contains("Qty");
                    bool flag2 = !flag && box.Name.Contains("UnitPrice");
                    bool flag3 = !flag2 && box.Name.Contains("Price");
                    if ((box != null) && !string.IsNullOrEmpty(box.Text))
                    {
                        if (flag)
                        {
                            int num = 0;
                            if (!int.TryParse(box.Text, out num))
                            {
                                e.Cancel = true;
                                box.Select(0, box.Text.Length);
                                MessageBox.Show("Invalid amount format " + box.Text + " must be without $, only numeric e.g. 10", "Amount error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                                return;
                            }
                        }
                        else
                        {
                            double num2 = 0.0;
                            if (!double.TryParse(box.Text, out num2))
                            {
                                e.Cancel = true;
                                box.Select(0, box.Text.Length);
                                MessageBox.Show("Invalid amount format " + box.Text + " must be without $, only numeric e.g. 100.99", "Amount error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                                return;
                            }
                        }
                    }
                    TextBox box2 = null;
                    TextBox box3 = null;
                    TextBox box4 = null;
                    string oldValue = flag ? "Qty" : (flag2 ? "UnitPrice" : "Price");
                    try
                    {
                        box2 = (TextBox)this.fieldNameToControlMapping[box.Name.Replace(oldValue, "UnitPrice")];
                        box3 = (TextBox)this.fieldNameToControlMapping[box.Name.Replace(oldValue, "Price")];
                        box4 = (TextBox)this.fieldNameToControlMapping[box.Name.Replace(oldValue, "Qty")];
                    }
                    catch (Exception)
                    {
                    }
                    int result = 0;
                    double num4 = 0.0;
                    double num5 = 0.0;
                    if (box2 != null)
                    {
                        double.TryParse(box2.Text, out num4);
                    }
                    if (box3 != null)
                    {
                        double.TryParse(box3.Text, out num5);
                    }
                    if (box4 != null)
                    {
                        int.TryParse(box4.Text, out result);
                    }
                    if (flag || flag2)
                    {
                        if (((result != 0) && (num4 != 0.0)) && (box3 != null))
                        {
                            double num6 = result * num4;
                            box3.Text = num6.ToString("F2");
                        }
                    }
                    else if (flag3 && (((num5 != 0.0) && (result != 0)) && (box2 != null)))
                    {
                        box2.Text = (num5 / ((double)result)).ToString("F2");
                    }
                    if ((((flag && (result == 0)) || (flag2 && (num4 == 0.0))) && (box2 != null)) && (box4 != null))
                    {
                        box4.Text = "";
                        box2.Text = "";
                    }
                }
                catch (Exception)
                {
                }
                finally
                {
                    this.amountValidating = false;
                }
            }
        }

        private void btnAddWeek_Click(object sender, EventArgs e)
        {
            if (!this.isLocked)
            {
                DateTime now = DateTime.Now;
                bool flag = false;
                if (!string.IsNullOrEmpty(this.jobDateRequired.Text))
                {
                    flag = this.CheckDate(this.jobDateRequired.Text, out now);
                }
                else
                {
                    flag = true;
                }
                if (flag)
                {
                    now = now.AddDays((Control.ModifierKeys == Keys.Shift) ? ((double)(-7)) : ((double)7));
                    if (now < DateTime.Now)
                    {
                        now = DateTime.Now;
                    }
                    this.jobDateRequired.Text = now.ToString("d/M/yy");
                }
            }
        }

        private void btnCancelSearch_Click(object sender, EventArgs e)
        {
            this.panelSearchField.Visible = false;
        }

        private void btnCollapseToggle_Click(object sender, EventArgs e)
        {
            this.compress = !this.compress;
            this.RedrawArrayComponent();
        }

        private void btnCollect_Click(object sender, EventArgs e)
        {
            if (!this.isLocked)
            {
                bool flag = false;
                if (!string.IsNullOrEmpty(this.jobDelivery.Text))
                {
                    if (MessageBox.Show("There is already text here - overwrite?", "Overwrite data?", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
                    {
                        flag = true;
                    }
                }
                else
                {
                    flag = true;
                }
                if (flag)
                {
                    this.jobDelivery.Text = "Customer to collect";
                }
            }
        }

        private void btnCourier_Click(object sender, EventArgs e)
        {
            if (!this.isLocked && !this.CheckAlreadyText(this.jobDelivery.Text))
            {
                this.jobDelivery.Text = ("Courier to:" + this.jobDelivery.Text) ?? "";
            }
        }

        private async void btnCustomers_Click(object sender, EventArgs e)
        {

            if (!(await this.NeedSaveAsync(true, false)))
            {
                JobQueryForm form = new JobQueryForm();
                DataGridView datagrid = form.getSearchDataGridView();
                var filter = Builders<JobCardDoc>.Filter.Ne("jobDateCompleted", BsonNull.Value);

                var result = await DataAccess.findJobByFilterAsync(datagrid, filter, "jobDateCompleted", true, 0, 50);
                /* PJC OLD
                form.Search("SELECT jobID, jobCustomer, jobBusinessName, jobPhone, jobDateCompleted," + this.AllDetails + " FROM " + JobCard.DBTable + " WHERE NOT ISNULL(jobDateCompleted) ORDER BY jobDateCompleted desc");
                */
                form.ShowDialog();
                if (JobQueryForm.selectedJobId > -1)
                {
                    /* PJC OLD
                    string sql = "SELECT * FROM " + JobCard.DBTable + " WHERE jobID = " + JobQueryForm.selectedJobId.ToString();
                    DataAccess.ReadRecords(this.datagrid, sql);
                    */
                    var list = await DataAccess.FindJobByFieldAsync(this.datagrid, "jobID", JobQueryForm.selectedJobId);
                    this.Load(0);
                }
            }
        }

        private async void btnDuplicate_Click(object sender, EventArgs e)
        {
            if (!(await this.NeedSaveAsync(true, false)))
            {
                this.stopVideoCapture();
                this.lastID = await DataAccess.GetLastJobIDAsync();
                this.lastID++;
                JobCardDoc newDoc = new JobCardDoc();
                newDoc.jobID = this.lastID;
                newDoc.jobDate = DateTime.Now;
                newDoc.jobOrderNumber = this.jobOrderNumber.Text;
                newDoc.jobCustomer = this.jobCustomer.Text;
                newDoc.jobBusinessName = this.jobBusinessName.Text;
                newDoc.jobPhone = this.jobPhone.Text;
                newDoc.jobAddress = this.jobAddress.Text;
                newDoc.jobEmail = this.jobEmail.Text;
                newDoc.jobDelivery = this.jobDelivery.Text;
                newDoc.jobReceivedFrom = this.jobReceivedFrom.Text;
                await DataAccess.CreateJobAsync(newDoc);
                /* PJC OLD
                if (DataAccess.Update(string.Concat(new object[] {
                    "INSERT INTO "+JobCard.DBTable+"(jobID, jobDate, jobOrderNumber, jobCustomer, jobBusinessName, jobPhone, jobAddress, jobEmail, jobDelivery, jobReceivedFrom) Values (", this.lastID.ToString(), ",DATE(),'", this.jobOrderNumber.Text, "', '", this.jobCustomer.Text,
                    "', '", this.jobBusinessName.Text, "', '", this.jobPhone.Text, "', '", this.jobAddress.Text, "', '", this.jobEmail.Text, "', '", this.jobDelivery.Text, "', '", this.jobReceivedFrom,
                    "')"
                })))
                */
                
                await this.GetLatestJobAsync();
                
                this.jobDateRequired.Focus();
            }
        }

        private bool incurCreditCardSurcharge()
        {
            try {
                int jobYear = 0;
                int lastSlash = this.jobDate.Text.LastIndexOf('/');
                string year = this.jobDate.Text.Substring(lastSlash + 1);
                int firstSlash = this.jobDate.Text.IndexOf('/');
                string month = this.jobDate.Text.Substring(firstSlash + 1, lastSlash - firstSlash - 1);
                string day = this.jobDate.Text.Substring(0, firstSlash);
                int _year = 2017;
                int _month = 1;
                int _day = 1;
                int.TryParse(year, out _year);
                if (_year < 2000)
                    _year += 2000;
                int.TryParse(month, out _month);
                int.TryParse(day, out _day);
                bool is2017AndBeyond = new DateTime(_year, _month, _day, 0, 0, 0, 0) >= new DateTime(2016, 12, 31, 0, 0, 0, 0);
                if (is2017AndBeyond && this.jobPaymentBy.Text.Length <= 1)
                {
                    return false;
                }
                if (this.jobPaymentBy.Text.Length > 1 && "VISAMasterCard".Contains(this.jobPaymentBy.Text))
                {
                    if (is2017AndBeyond)
                    {
                        return true;
                    }
                }
            } catch (Exception err)
            {

            }
            return false;
        }

        protected virtual bool IsFileLocked(FileInfo file)
        {
            FileStream stream = null;

            try
            {
                stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None);
            }
            catch (IOException err)
            {
                if (err.Message.StartsWith("Could not find file"))
                {
                    return false;
                }
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                try
                {
                    foreach (Process clsProcess in Process.GetProcessesByName("WINWORD"))
                    {
                        if (clsProcess.ProcessName.StartsWith("WINWORD"))
                        {
                            // clsProcess.Kill();
                        }
                    }
                }
                catch (Exception exc)
                {
                    System.Console.Out.WriteLine("Failed " + exc.ToString());
                }
                return true;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }

            //file is not locked
            return false;
        }

        private async void btnEmail_Click(object sender, EventArgs e)
        {
            string emailaddress = this.jobEmail.Text.Trim();
            if (!this.IsValid(emailaddress))
            {
                MessageBox.Show("can't email as email address " + emailaddress + " is empty or not correct. CHECK AGAIN PLEASE");
            }
            else
            {
                SettingsSettingsDoc settings = await DataAccess.findSettings();
                string str2 = "Your JobID# " + this.jobID.Text;
                string str3 = string.IsNullOrWhiteSpace(this.jobOrderNumber.Text) ? "" : (" (Your ref Order#" + this.jobOrderNumber.Text.Trim() + ")");
                string csSubject = str2 + str3;
                string csBody = "Dear " + this.jobCustomer.Text + "," + Environment.NewLine + Environment.NewLine;
                string printToPDF = Path.Combine(Path.Combine(Environment.ExpandEnvironmentVariables("%userprofile%"), "Documents"), Environment.UserName + "TempJobToPdf");
                int failCount = 0;
                //MessageBox.Show("PJC0");
                while (IsFileLocked(new FileInfo(printToPDF + ".rtf")))
                {
                    failCount++;
                    if (failCount > 1)
                    {
                        return;
                    }
                }
                if (System.IO.File.Exists(printToPDF + ".rtf"))
                {
                    System.IO.File.Delete(printToPDF + ".rtf");
                }
                if (System.IO.File.Exists(printToPDF + ".pdf"))
                {
                    System.IO.File.Delete(printToPDF + ".pdf");
                }
                if (System.IO.File.Exists(printToPDF + ".doc"))
                {
                    System.IO.File.Delete(printToPDF + ".doc");
                }
                if (System.IO.File.Exists(printToPDF + ".docx"))
                {
                    System.IO.File.Delete(printToPDF + ".docx");
                }
                this.ShowPrintForm(true, false, printToPDF);
                bool flag = true;
                if (System.IO.File.Exists(printToPDF + ".pdf"))
                {
                    printToPDF = printToPDF + ".pdf";
                }
                else if (System.IO.File.Exists(printToPDF + ".docx"))
                {
                    printToPDF = printToPDF + ".docx";
                }
                else if (System.IO.File.Exists(printToPDF + ".doc"))
                {
                    printToPDF = printToPDF + ".doc";
                }
                else if (System.IO.File.Exists(printToPDF + ".rtf"))
                {
                    printToPDF = printToPDF + ".rtf";
                }
                else
                {
                    printToPDF = null;
                    flag = false;
                }
                if (!string.IsNullOrWhiteSpace(this.jobDateCompleted.Text))
                {
                    string str10 = csBody;
                    csBody = str10 + "Your job was completed " + this.jobDateCompleted.Text + "." + Environment.NewLine + Environment.NewLine;
                    if (flag)
                    {
                        csBody = csBody + "Please find attached your invoice. A simple text summary is also included below." + Environment.NewLine;
                    }
                    if (string.IsNullOrWhiteSpace(this.jobDatePaid.Text))
                    {
                        for (int i = 0; i <= 24; i++)
                        {
                            string text = this.jobPrice[i].Text;
                            string str9 = this.jobDetail[i].Text;
                            if (!string.IsNullOrWhiteSpace(text))
                            {
                                if (i == 24)
                                {
                                    str9 = "Freight";
                                }
                                str10 = csBody;
                                csBody = str10 + Environment.NewLine + str9.PadRight(50) + " $" + text.PadLeft(8);
                            }
                        }
                        str10 = csBody;
                        string subTotalText = "Sub Total";
                        if (incurCreditCardSurcharge())
                        {
                            subTotalText += " (+3% Card surch)";
                        }
                        str10 = str10 + Environment.NewLine + subTotalText.PadRight(50) + " $" + this.jobPrice[0x19].Text.PadLeft(8);
                        str10 = (str10 + Environment.NewLine + JobCard.GSTText().PadRight(50) + " $" + this.jobPrice[0x1a].Text.PadLeft(8)) + Environment.NewLine + "_".PadRight(70, '_');
                        csBody = str10 + Environment.NewLine + "Total Due".PadRight(50) + " $" + this.jobPrice[0x1b].Text.PadLeft(8);
                    }
                    else
                    {
                        csBody = csBody + "It has been paid and ready for pickup (or delivery if you specified this).";
                    }
                }
                this.SendMail(settings, emailaddress, csSubject, csBody, flag ? printToPDF : null);
            }
        }

        public static string GSTText()
        {
            if (JobTypePopup.isCanada())
            {
                return "HST";
            } else
            {
                return "GST";
            }
        }

        private async void btnExistingJobs_Click(object sender, EventArgs e)
        {
            if (!(await this.NeedSaveAsync(true, false)))
            {
                JobQueryForm form = new JobQueryForm();
                DataGridView datagrid = form.getSearchDataGridView();
                var filter = Builders<JobCardDoc>.Filter.Eq("jobDateCompleted", BsonNull.Value);

                var result = await DataAccess.findJobByFilterAsync(datagrid, filter, "jobDate", false, 0, 50);
                /* PJC OLD
                form.Search("SELECT jobID, jobCustomer, jobBusinessName, jobPaymentBy, jobPhone, jobDate," + this.AllDetails + " FROM " + JobCard.DBTable + " WHERE ISNULL(jobDateCompleted) ORDER BY jobDate");
                */
                form.ShowDialog();
                if (JobQueryForm.selectedJobId > -1)
                {
                    /* PJC OLD
                    string sql = "SELECT * FROM " + JobCard.DBTable + " WHERE jobID = " + JobQueryForm.selectedJobId.ToString();
                    DataAccess.ReadRecords(this.datagrid, sql);
                    */
                    var list = await DataAccess.FindJobByFieldAsync(this.datagrid, "jobID", JobQueryForm.selectedJobId);
                    this.Load(0);
                }
            }
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            base.Close();
        }

        private async void btnLatestJob_Click(object sender, EventArgs e)
        {
            if (!(await this.NeedSaveAsync(true, false)))
            {
                /* PJC OLD
                string sql = "Select Top 1 * from " + JobCard.DBTable + " ORDER BY jobID desc";
                DataAccess.ReadRecords(this.datagrid, sql);
                this.Load(0);
                */
                await this.GetLatestJobAsync();
                
            }
        }

        private void btnLockUnlock_Click(object sender, EventArgs e)
        {
            if (this.isLocked)
            {
                if (!(string.IsNullOrWhiteSpace(this.jobDateCompleted.Text) || string.IsNullOrWhiteSpace(this.jobDatePaid.Text)))
                {
                    if (MessageBox.Show("This job is completed and paid." + Environment.NewLine + "Are you sure you wish to make changes?" + Environment.NewLine + "OK - to unlock and make changes", "Unlock?", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation) == DialogResult.OK)
                    {
                        this.LockAll(false);
                    }
                }
                else
                {
                    this.LockAll(false);
                }
            }
            else
            {
                this.LockAll(true);
            }
            this.LockAll(this.isLocked);
        }

        private async void btnNavigateBack_Click(object sender, EventArgs e)
        {
            this.stopVideoCapture();
            await this.GetPreviousJobAsync();
        }

        private async void btnNavigateForward_Click(object sender, EventArgs e)
        {
            this.stopVideoCapture();
            await this.GetNextJobAsync();
        }

        private void stopVideoCapture()
        {
            if (this.videoCaptureDevice != null && this.videoCaptureDevice.IsRunning)
            {
                this.videoCaptureDevice.SignalToStop();
                this.pictureBox1.Visible = true;
                this.pictureBox2.Visible = false;
                this.btnCam1.Text = "Toggle ON Cam";
                this.pictureBox2.BackgroundImage = null;
            }
        }

        private bool gotAllPrices = false;
        private async void btnNewJob_Click(object sender, EventArgs e)
        {
            if (!(await this.NeedSaveAsync(true, false)))
            {
                
                this.lastID = await DataAccess.GetLastJobIDAsync();
                this.lastID++;
                JobCardDoc newDoc = new JobCardDoc();
                newDoc.jobID = this.lastID;
                newDoc.jobDate = DateTime.Now;               
                await DataAccess.CreateJobAsync(newDoc);
                /* PJC OLD
                if (DataAccess.Update("INSERT INTO " + JobCard.DBTable + "(jobID, jobDate) Values (" + this.lastID.ToString() + ",DATE())"))
                */
                
                    await this.DisclaimerNoteAsync();
                    await this.GetLatestJobAsync();
                
                this.jobCustomer.Focus();
                if (JobTypePopup.isWheelApp())
                {

                    if (JobCard.popup == null || JobCard.popup.IsDisposed)
                    {
                        JobCard.popup = new JobTypePopup();
                    }
                    if (JobTypePopup.jobType == null)
                    {
                        JobTypePopup.jobType = this.jobType[0];
                        JobTypePopup.jobQty = this.jobQty[0];
                        JobTypePopup.jobUnitPrice = this.jobUnitPrice[0];
                        JobTypePopup.jobPrice = this.jobPrice[0];
                        JobTypePopup.jobDetail = this.jobDetail[0];
                    }
                    JobCard.popup.jobCard = this;

                    JobCard.popup.Show();
                }
            }
        }

        private void btnNextPhoto_Click(object sender, EventArgs e)
        {
            if (currentPhotoPaths.Count > 0)
            {
                currentPictureIndex = (currentPictureIndex + 1) % currentPhotoPaths.Count;
                Image image = FromFile(currentPhotoPaths[currentPictureIndex]);
                UpdatePictureBox(this.pictureBox1, image);
            }
        }

        private void btnPrintAll_Click(object sender, EventArgs e)
        {
            this.ShowPrintForm(true, true, null);
            this.ShowPrintForm(false, true, null);
            this.PrintForWork(true);
        }

        private void btnPrintBusiness_Click(object sender, EventArgs e)
        {
            this.ShowPrintForm(false, false, null);
        }

        private void btnPrintCustomerCopy_Click(object sender, EventArgs e)
        {
            this.ShowPrintForm(true, false, null);
        }

        private void btnPrintForWork_Click(object sender, EventArgs e)
        {
            this.PrintForWork(false);
        }
        
        private void btnCam1_Click(object sender, EventArgs e)
        {
            // on
            if (this.videoCaptureDevice == null && this.filterInfoCollection.Count > 0 && this.cboCamera.SelectedIndex >= 0)
            {
                this.videoCaptureDevice = new VideoCaptureDevice(this.filterInfoCollection[this.cboCamera.SelectedIndex].MonikerString);
            }
            if (this.videoCaptureDevice.IsRunning) {
                this.stopVideoCapture();
                return;
            }
            try
            {
                if (this.videoCaptureDevice.VideoCapabilities.Length > 0)
                {
                    string highestSolution = "0;0";
                    for (int i=0; i < this.videoCaptureDevice.VideoCapabilities.Length; i++)
                    {
                        if (this.videoCaptureDevice.VideoCapabilities[i].FrameSize.Width > Convert.ToInt32(highestSolution.Split(';')[0]))
                        {
                            highestSolution = this.videoCaptureDevice.VideoCapabilities[i].FrameSize.Width.ToString() + ";" + i.ToString();
                        }
                    }
                    System.Console.Out.WriteLine("Chose resolution " + highestSolution);
                    this.videoCaptureDevice.VideoResolution = this.videoCaptureDevice.VideoCapabilities[Convert.ToInt32(highestSolution.Split(';')[1])];
                }
            }catch (Exception Err)
            {
                System.Console.Out.WriteLine("Error getting capabilities ", Err);
            }
            this.videoCaptureDevice.NewFrame += this.VideoCaptureDevice_NewFrame;
            this.videoCaptureDevice.Start();
            this.pictureBox1.Visible = false;
            this.pictureBox2.Visible = true;
            this.btnCam1.Text = "Toggle OFF Cam";

        }
        private void VideoCaptureDevice_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            this.pictureBox2.BackgroundImageLayout = ImageLayout.Zoom;
            this.pictureBox2.BackgroundImage = (Bitmap)eventArgs.Frame.Clone();
        }


     
        private async void btnSave_Click(object sender, EventArgs e)
        {
            if (await this.NeedSaveAsync(false, true))
            {
                var ok = await DataAccess.UpdateMongoAsync(this.updateSqlSetList);
                if (!ok
                    /* PJC OLD
                    DataAccess.Update(this.updateSql)
                    */
                    )
                {
                    MessageBox.Show("Save failed", "SAVE FAIL", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                }
                else
                {
                    int jobId = int.Parse(this.jobID.Text);
                    var list = await DataAccess.FindJobByFieldAsync(this.datagrid, "jobID", jobId);
                    /* PJC OLD
                    string sql = "SELECT * FROM " + JobCard.DBTable + " WHERE jobID = " + this.jobID.Text;
                    DataAccess.ReadRecords(this.datagrid, sql);
                    */
                    this.Load(0);
                }
            }
            else
            {
                MessageBox.Show("No changes to save");
            }
        }

        private async void btnSearchField_Click(object sender, EventArgs e)
        {
            await this.SearchAsync();
        }

        private async void btnSearchLists_Click(object sender, EventArgs e)
        {
            if (!(await this.NeedSaveAsync(true, false)))
            {
                new JobQueryForm().ShowDialog();
                if (JobQueryForm.selectedJobId > -1)
                {
                    /* PJC OLD
                    string sql = "SELECT * FROM " + JobCard.DBTable + " WHERE jobID = " + JobQueryForm.selectedJobId.ToString();
                    DataAccess.ReadRecords(this.datagrid, sql);
                    */
                    var list = await DataAccess.FindJobByFieldAsync(this.datagrid, "jobID", JobQueryForm.selectedJobId);
                    this.Load(0);
                }
            }
        }

        private void btnToday_Click(object sender, EventArgs e)
        {
            if (!this.isLocked)
            {
                bool flag = false;
                if (!string.IsNullOrEmpty(this.jobDatePaid.Text))
                {
                    if (MessageBox.Show("There is already text here - overwrite?", "Overwrite data?", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
                    {
                        flag = true;
                    }
                }
                else
                {
                    flag = true;
                }
                if (flag)
                {
                    this.jobDatePaid.Text = DateTime.Now.ToString("d/M/yy");
                }
            }
        }

        private void btnTodayForDateCompleted_Click(object sender, EventArgs e)
        {
            if (!this.isLocked)
            {
                bool flag = false;
                if (!string.IsNullOrEmpty(this.jobDateCompleted.Text))
                {
                    if (MessageBox.Show("There is already text here - overwrite?", "Overwrite data?", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
                    {
                        flag = true;
                    }
                }
                else
                {
                    flag = true;
                }
                if (flag)
                {
                    this.jobDateCompleted.Text = DateTime.Now.ToString("d/M/yy");
                }
            }
        }

        private void btnUndo_Click(object sender, EventArgs e)
        {
            if (this.undoList.Count > 0)
            {
                string str;
                this.ControlValueChangedFromLoaded(this.undoList[this.undoList.Count - 1], true, out str);
            }
        }

        private void button9_Click(object sender, EventArgs e)
        {
        }

        private void cboReportProduct_SelectedIndexChanged(object sender, EventArgs e)
        {
        }

        private bool CheckAlreadyText(string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                if (MessageBox.Show("There is already text here - overwrite?", "Overwrite data?", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
                {
                    return false;
                }
                return true;
            }
            return false;
        }

        private async void CheckBeforeQuit(object sender, FormClosingEventArgs e)
        {
            if (this.videoCaptureDevice != null && this.videoCaptureDevice.IsRunning)
            {
                this.videoCaptureDevice.Stop();
            }
            if (await this.NeedSaveAsync(true, false))
            {
                e.Cancel = true;
            }
        }

        private bool CheckDate(string dateText, out DateTime thedate)
        {
            thedate = DateTime.MinValue;
            bool flag = false;
            if (!this.Loading && !string.IsNullOrEmpty(dateText))
            {
                if (!JobQueryForm.ParsedDateOK(dateText, out thedate))
                {
                    flag = true;
                }
                if (flag)
                {
                    MessageBox.Show("Invalid date format! " + dateText + " must be d/m/yy", "Date error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                }
            }
            return !flag;
        }

        private string CombinedDetailText(bool forPrint = false)
        {
            string str = "";
            for (int i = 0; i < 0x21; i++)
            {
                string str2 = this.jobDetail[i].Text.Trim();
                string str3 = this.checkBox[i].Checked ? (this.checkBox[i].Text + ": ") : "";
                string text = this.label[i].Text;
                if (!string.IsNullOrEmpty(str2) || (forPrint && ((str3 != "") || !string.IsNullOrWhiteSpace(text))))
                {
                    if (forPrint)
                    {
                        if (str3 != "")
                        {
                            str3 = str3.PadLeft(12);
                        }
                        if (!string.IsNullOrWhiteSpace(text))
                        {
                            str3 = "";
                            str2 = text.PadLeft(0x3e);
                        }
                        string str5 = this.jobType[i].Text.Trim();
                        if (i >= this.freightIndex)
                        {
                            str = str + (str3 + str2).PadRight(0x3e) + (string.IsNullOrWhiteSpace(this.jobPrice[i].Text) ? "" : (" $" + this.jobPrice[i].Text.PadLeft(8))) + Environment.NewLine;
                        }
                        else
                        {
                            string str7;
                            if (string.IsNullOrWhiteSpace(this.jobQty[i].Text.Trim()))
                            {
                                str7 = str + (str3 + str2).PadRight(0x49) + Environment.NewLine;
                                str = str7 + str5.PadRight(0x2a) + "  " + this.jobQty[i].Text.PadLeft(3) + "    " + "".PadLeft(8) + " " + (string.IsNullOrWhiteSpace(this.jobPrice[i].Text) ? "" : (" $" + this.jobPrice[i].Text.PadLeft(8))) + Environment.NewLine;
                            }
                            else
                            {
                                str7 = str + (str3 + str2).PadRight(0x49) + Environment.NewLine;
                                str = str7 + str5.PadRight(44) + " x" + this.jobQty[i].Text.PadLeft(3) + " @ $" + this.jobUnitPrice[i].Text.PadLeft(8) + "=" + (string.IsNullOrWhiteSpace(this.jobPrice[i].Text) ? "" : (" $" + this.jobPrice[i].Text.PadLeft(8))) + Environment.NewLine;
                            }
                        }
                    }
                    else
                    {
                        str = str + str2 + " ";
                    }
                }
            }
            if (!forPrint)
            {
                str = str.Trim();
            }
            return str;
        }

        private void control_TextChanged(object sender, EventArgs e)
        {
            if (!this.Loading && (sender is Control))
            {
                Control control = (Control)sender;
                string stringValue = "";
                bool flag = this.ControlValueChangedFromLoaded(control, false, out stringValue);
                if (sender is ComboBox)
                {
                    ((ComboBox)control).BackColor = flag ? Color.LightYellow : Color.WhiteSmoke;
                }
                else
                {
                    if (sender is TextBox)
                    {
                        TextBox key = sender as TextBox;

                        if (!string.IsNullOrEmpty(key.Text))
                        {
                            Graphics graphics = Graphics.FromHwnd(base.Handle);
                            if (!this.restoreFontSize.ContainsKey(key))
                            {
                                this.restoreFontSize[key] = key.Font.Size;
                            }
                            float num = this.restoreFontSize[key];
                            float size = key.Font.Size;
                            if ((num > -1f) && (num != size))
                            {
                                size = num;
                            }

                            Font font = new Font(key.Font.FontFamily, size, key.Font.Style);
                            for (SizeF ef = graphics.MeasureString(key.Text, font); ef.Width > key.Size.Width; ef = graphics.MeasureString(key.Text, font))
                            {
                                size -= 0.5f;
                                font = new Font(font.FontFamily, size, key.Font.Style);
                                if (size < 8)
                                    break;
                            }

                            if (!(size == key.Font.Size))
                            {
                                key.Font = font;
                            }
                        }
                    }
                    control.BackColor = flag ? Color.LightYellow : Color.WhiteSmoke;
                }
            }
        }

        private void ControlAdd(object sender, ControlEventArgs e)
        {
        }

        private bool ControlValueChangedFromLoaded(Control control, bool isUndo, out string stringValue)
        {
            bool flag = false;
            stringValue = "";
            string name = control.Name;
            if (!this.originalValues.ContainsKey(name))
            {
                return flag;
            }
            string str2 = this.originalValues[name];
            if (str2 == null)
            {
                str2 = "";
            }
            bool flag2 = control is TextBox;
            bool flag3 = control is Label;
            bool flag4 = control is CheckBox;
            bool flag5 = control is ComboBox;
            if (flag2)
            {
                stringValue = ((TextBox)control).Text;
                if (isUndo)
                {
                    ((TextBox)control).Text = str2;
                }
            }
            else if (flag3)
            {
                stringValue = ((Label)control).Text;
                if (isUndo)
                {
                    ((Label)control).Text = str2;
                }
            }
            else if (flag4)
            {
                if (str2 == "")
                {
                    str2 = "False";
                }
                stringValue = ((CheckBox)control).Checked.ToString();
                if (isUndo)
                {
                    ((CheckBox)control).Checked = str2.ToUpperInvariant() == "TRUE";
                }
            }
            else if (flag5)
            {
                stringValue = ((ComboBox)control).Text;
                if (isUndo)
                {
                    ((ComboBox)control).Text = str2;
                }
            }
            flag = stringValue != str2;
            this.undoList.Remove(control);
            if (!isUndo)
            {
                if (flag)
                {
                    this.undoList.Add(control);
                }
                return flag;
            }
            return false;
        }

        private void DateValidating(object sender, CancelEventArgs e)
        {
            TextBox box = null;
            if (sender is TextBox)
            {
                box = (TextBox)sender;
            }
            if ((box != null) && !string.IsNullOrWhiteSpace(box.Text))
            {
                DateTime time;
                if (this.CheckDate(box.Text, out time))
                {
                    box.Text = time.ToString("d/M/yy");
                }
                else
                {
                    e.Cancel = true;
                    box.Select(0, box.Text.Length);
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

        private void DoDragDrop(object sender, DragEventArgs e)
        {
            int x = base.PointToClient(new Point(e.X, e.Y)).X;
            int y = base.PointToClient(new Point(e.X, e.Y)).Y;
            if ((((x >= this.pictureBox1.Location.X) && (x <= (this.pictureBox1.Location.X + this.pictureBox1.Width))) && (y >= this.pictureBox1.Location.Y)) && (y <= (this.pictureBox1.Location.Y + this.pictureBox1.Height)))
            {

                string[] data = (string[])e.Data.GetData(DataFormats.FileDrop);
                foreach (string str in data)
                {
                    try
                    {
                        try
                        {
                            Image image = FromFile(str);
                            FileInfo info = new FileInfo(str);
                            DateTime creationTime = info.CreationTime;
                            DateTime now = DateTime.Now;
                            if (!JobQueryForm.ParsedDateOK(this.jobDate.Text, out now))
                            {
                                now = DateTime.Now;
                            }
                            TimeSpan span = now.Subtract(creationTime);
                            bool flag = true;
                            if (span.TotalSeconds > 3600.0)
                            {
                                DialogResult result = MessageBox.Show(string.Concat(new object[] { "Warning image file: ", str, " is ", (int)span.TotalDays, " days ", (int)span.TotalHours, " hours old. Are you sure this is the correct image file?" }), "Check Image File", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Hand);
                                if (result == DialogResult.Cancel)
                                {
                                    break;
                                }
                                flag = result == DialogResult.Yes;
                            }
                            if (flag)
                            {
                                string outPath = "";
                                this.jobPhotos = this.GetJobPictureFiles(now.Year, now.Month, int.Parse(this.jobID.Text), out outPath, false);
                                this.SaveUniquePhoto(outPath, image, this.jobPhotos, str);
                                currentPictureIndex = 0;
                                currentPhotoPaths = this.jobPhotos;
                            }
                            this.UpdatePhotos();
                            UpdatePictureBox(this.pictureBox1, image);
                        }
                        catch (Exception exception)
                        {
                            MessageBox.Show(str + " is NOT an image file - error:" + exception.Message + "\nStack:" + exception.StackTrace);
                        }
                    }
                    catch (Exception)
                    {
                    }
                }

            }
        }

        private void DoDragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move;
        }

        public static string DoubleQuote(string inStr) =>
            inStr.Replace("'", "''");

        private void Form1_ResizeEnd(object sender, EventArgs e)
        {
            this.RedrawArrayComponent();
        }

        private static Image movieImage = null;
        public static Image MovieImage
        {
            get
            {
                if (movieImage == null)
                    movieImage = DataAccess.Base64ToImage("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");
                return movieImage;
            }
        }
        public static Image FromFile(string path)
        {
            if (System.IO.File.Exists(path))
            {
                try
                {
                    if (path.ToUpper().EndsWith("MOV") || path.ToUpper().EndsWith("MP4"))
                    {
                        return MovieImage;
                    }
                    Image myImage = (Image)Image.FromFile(path).Clone();
                    return myImage;
                }
                catch (Exception e)
                {

                }
            }
            return null;
        }

        private List<string> GetJobPictureFiles(int year, int month, int jobId, out string outPath, bool complainIfError = false)
        {
            outPath = "";
            List<string> list = new List<string>();
            string picturePath = PicturePath;
            try
            {
                DirectoryInfo info = new DirectoryInfo(picturePath);
                if (!info.Exists)
                {
                    if (complainIfError)
                    {
                        this.ShowError("Picture root path " + picturePath + " does not exist", "Pictures Not Found", false);
                    }
                    return list;
                }
                picturePath = picturePath + year.ToString();
                info = new DirectoryInfo(picturePath);
                if (!info.Exists)
                {
                    info.Create();
                    info = new DirectoryInfo(picturePath);
                }
                if (info.Exists)
                {
                    string str2 = picturePath;
                    picturePath = str2 + @"\" + year.ToString() + " " + this.months[month];
                    info = new DirectoryInfo(picturePath);
                    if (!info.Exists)
                    {
                        info.Create();
                        info = new DirectoryInfo(picturePath);
                    }
                    if (info.Exists)
                    {
                        outPath = picturePath;
                        FileInfo[] files = info.GetFiles();
                        foreach (FileInfo info2 in files)
                        {
                            if (ImageExtensions.Contains(info2.Extension.ToUpperInvariant()))
                            {
                                string[] strArray = info2.Name.Split(new char[] { ' ' });
                                int result = -1;
                                int.TryParse(strArray[0], out result);
                                if ((jobId == 0) || (jobId == result))
                                {
                                    list.Add(info2.FullName);
                                }
                            }
                        }
                        return list;
                    }
                    if (complainIfError)
                    {
                        this.ShowError("Picture error for path " + picturePath, "Pictures Not Found", false);
                    }
                    return list;
                }
                if (complainIfError)
                {
                    this.ShowError("Picture error for path " + picturePath, "Pictures Not Found", false);
                }
            }
            catch (Exception exception)
            {
                if (complainIfError)
                {
                    this.ShowError("Picture error " + exception.Message + " for path " + picturePath, "Pictures Not Found", false);
                }
            }
            return list;
        }

        private async System.Threading.Tasks.Task GetLatestJobAsync()
        {
            /* PJC OLD
            string sql = "SELECT MAX(jobID) FROM " + JobCard.DBTable;
            object obj2 = DataAccess.ReadSingleValue(sql);
            if (obj2 != null)
            {
                try
                {
                    this.lastID = (int)obj2;
                    sql = "SELECT * FROM " + JobCard.DBTable + " WHERE jobID=" + ((int)obj2).ToString();
                    DataAccess.ReadRecords(this.datagrid, sql);
                    this.Load(0);
                }
                catch (Exception err)
                {
                    
                }
            }
            */
            this.lastID = await DataAccess.GetLastJobIDAsync();
            var list = await DataAccess.FindJobByFieldAsync(this.datagrid, "jobID", this.lastID);
            /* PJC OLD
            DataAccess.ReadRecords(this.datagrid, sql);
            */
            this.Load(0);
            
        }

        private async System.Threading.Tasks.Task GetNextJobAsync()
        {
            if (!(await this.NeedSaveAsync(true, false)))
            {
                this.lastID = await DataAccess.GetLastJobIDAsync();
                int jobId = int.Parse(this.jobID.Text);
                if (jobId < this.lastID)
                {
                    var filter = Builders<JobCardDoc>.Filter.Gt("jobID", jobId);
                    var list = await DataAccess.findJobByFilterAsync(this.datagrid, filter, "jobID", false);
                    /* PJC OLD
                    string sql = "SELECT TOP 1 * FROM " + JobCard.DBTable + " WHERE jobID > " + this.jobID.Text + " ORDER BY jobID";
                    DataAccess.ReadRecords(this.datagrid, sql);
                    */
                    this.Load(0);
                }
                else
                {
                    SystemSounds.Beep.Play();
                }
            }
        }

        private async System.Threading.Tasks.Task GetPreviousJobAsync()
        {
            if (!(await this.NeedSaveAsync(true, false)))
            {
                int jobId = int.Parse(this.jobID.Text);
                if (jobId > 0)
                {
                    var filter = Builders<JobCardDoc>.Filter.Lt("jobID", jobId);
                    var list = await DataAccess.findJobByFilterAsync(this.datagrid, filter, "jobID", true);
                    /* PJC OLD
                    string sql = "SELECT TOP 1 * FROM " + JobCard.DBTable + " WHERE jobID < " + this.jobID.Text + " ORDER BY jobID desc";
                    DataAccess.ReadRecords(this.datagrid, sql);
                    */
                    this.Load(0);
                }
                else
                {
                    SystemSounds.Beep.Play();
                }
            }
        }

        private void InitializeArrayComponent()
        {
            this.jobDetail = new TextBox[0x21];
            this.jobType = new TextBox[0x21];
            this.jobQty = new TextBox[0x21];
            this.jobUnitPrice = new TextBox[0x21];
            this.jobPrice = new TextBox[0x21];
            this.checkBox = new CheckBox[0x21];
            this.label = new Label[0x21];
            for (int i = 0; i < 0x21; i++)
            {
                this.jobDetail[i] = new TextBox();
                this.jobType[i] = new TextBox();
               // this.jobType[i].ReadOnly = true;
                this.jobType[i].Click += new EventHandler(this.JobTypeClick);
                this.jobQty[i] = new TextBox();
                this.jobUnitPrice[i] = new TextBox();
                this.jobPrice[i] = new TextBox();
                this.checkBox[i] = new CheckBox();
                this.label[i] = new Label();
                string str = "jobDetail" + i.ToString("D2");
                string str2 = "jobType" + i.ToString("D2");
                string str3 = "jobQty" + i.ToString("D2");
                string str4 = "jobUnitPrice" + i.ToString("D2");
                string str5 = "";
                string str6 = "";
                string str7 = "";
                string str8 = "jobPrice" + i.ToString("D2");
                switch (i)
                {
                    case 0x12:
                        str = "jobRepairText";
                        str2 = "jobRepairType";
                        str3 = "jobRepairQty";
                        str4 = "jobRepairUnitPrice";
                        str5 = "jobRepair";
                        str8 = "jobRepairPrice";
                        break;

                    case 0x13:
                        str = "jobStripText";
                        str5 = "jobStrip";
                        str8 = "jobStripPrice";
                        str2 = "jobStripType";
                        str3 = "jobStripQty";
                        str4 = "jobStripUnitPrice";
                        break;

                    case 20:
                        str = "jobPolishText";
                        str5 = "jobPolish";
                        str8 = "jobPolishPrice";
                        str2 = "jobPolishType";
                        str3 = "jobPolishQty";
                        str4 = "jobPolishUnitPrice";
                        this.polishIndex = i;
                        break;

                    case 0x15:
                        str = "jobPlatingText";
                        str5 = "jobPlating";
                        str8 = "jobPlatingPrice";
                        str2 = "jobPlatingType";
                        str3 = "jobPlatingQty";
                        str4 = "jobPlatingUnitPrice";
                        this.platingIndex = i;
                        break;

                    case 0x16:
                        str = "jobLaquerText";
                        str5 = "jobLaquer";
                        str8 = "jobLaquerPrice";
                        str2 = "jobLaquerType";
                        str3 = "jobLaquerQty";
                        str4 = "jobLaquerUnitPrice";
                        break;

                    case 0x17:
                        str = "jobSilvGalvText";
                        str5 = "jobSilvGalv";
                        str8 = "jobSilvGalvPrice";
                        str2 = "jobSilvGalvType";
                        str3 = "jobSilvGalvQty";
                        str4 = "jobSilvGalvUnitPrice";
                        break;

                    case 0x18:
                        str = "jobGoldGalvText";
                        str5 = "jobGoldGalv";
                        str8 = "jobGoldGalvPrice";
                        str2 = "jobGoldGalvType";
                        str3 = "jobGoldGalvQty";
                        str4 = "jobGoldGalvUnitPrice";
                        break;

                    case 0x19:
                        str = "jobWheelCrackText";
                        str5 = "jobWheelCrack";
                        str8 = "jobWheelCrackPrice";
                        str2 = "jobWheelCrackType";
                        str3 = "jobWheelCrackQty";
                        str4 = "jobWheelCrackUnitPrice";
                        break;

                    case 0x1a:
                        str = "jobWheelDentText";
                        str5 = "jobWheelDent";
                        str8 = "jobWheelDentPrice";
                        str2 = "jobWheelDentType";
                        str3 = "jobWheelDentQty";
                        str4 = "jobWheelDentUnitPrice";
                        break;

                    case 0x1b:
                        str = "jobWheelMachineText";
                        str5 = "jobWheelMachine";
                        str8 = "jobWheelMachinePrice";
                        str2 = "jobWheelMachineType";
                        str3 = "jobWheelMachineQty";
                        str4 = "jobWheelMachineUnitPrice";
                        break;

                    case 0x1c:
                        str = "jobTyreText";
                        str5 = "jobTyre";
                        str8 = "jobTyrePrice";
                        str2 = "jobTyreType";
                        str3 = "jobTyreQty";
                        str4 = "jobTyreUnitPrice";
                        break;

                    case 0x1d:
                        this.freightIndex = i;
                        str = "txtFreightText";
                        str8 = "jobFreight";
                        str6 = "Freight";
                        break;

                    case 30:
                        this.subTotalIndex = i;
                        str = "txtSubTotal";
                        str8 = "jobSubTotal";
                        str6 = "Sub Total";
                        break;

                    case 0x1f:
                        this.gstIndex = i;
                        str = "txtGST";
                        str8 = "jobGST";
                        break;

                    case 0x20:
                        this.totalIndex = i;
                        str = "txtTOTAL";
                        str8 = "jobTOTAL";
                        str6 = "TOTAL";
                        break;
                }
                this.jobDetail[i].Name = str;
                this.jobType[i].Name = str2;
                this.jobQty[i].Name = str3;
                this.jobUnitPrice[i].Name = str4;
                if (i >= 0x12)
                {
                    this.jobType[i].Visible = false;
                    this.jobType[i].Enabled = false;
                }
                if (i >= this.freightIndex)
                {
                    this.jobDetail[i].Visible = false;
                    this.jobDetail[i].Enabled = false;
                    this.jobQty[i].Visible = false;
                    this.jobQty[i].Enabled = false;
                    this.jobUnitPrice[i].Visible = false;
                    this.jobUnitPrice[i].Enabled = false;
                }
                this.jobPrice[i].Name = str8;
                this.jobPrice[i].WordWrap = false;
                this.jobPrice[i].TextAlign = HorizontalAlignment.Right;
                this.jobPrice[i].Validating += new CancelEventHandler(this.AmountValidate);
                this.jobQty[i].Validating += new CancelEventHandler(this.AmountValidate);
                this.jobUnitPrice[i].Validating += new CancelEventHandler(this.AmountValidate);
                this.checkBox[i].CheckAlign = ContentAlignment.MiddleRight;
                this.checkBox[i].Name = str5;
                this.checkBox[i].TextAlign = ContentAlignment.BottomRight;
                if ((i >= 0x12) && (i < this.freightIndex))
                {
                    str7 = str5.Substring(3);
                }
                this.checkBox[i].Text = str7;
                this.checkBox[i].UseVisualStyleBackColor = true;
                this.checkBox[i].Enabled = false;
                this.checkBox[i].Visible = str7 != "";
                if ((i >= this.freightIndex) && (str6 == ""))
                {
                    str6 = str.Substring(3);
                    if (str6 == "GST")
                    {
                        str6 = JobCard.GSTText();
                    }
                }
                this.label[i].Name = "label" + i.ToString();
                this.label[i].RightToLeft = RightToLeft.No;
                this.label[i].Text = str6;
                this.label[i].TextAlign = ContentAlignment.BottomRight;
                this.label[i].Enabled = str6 != "";
                this.label[i].Visible = str6 != "";
                if ((i % 2) == 0)
                {
                    this.jobDetail[i].BackColor = this.stripe;
                    this.jobType[i].BackColor = this.stripe;
                    this.jobQty[i].BackColor = this.stripe;
                    this.jobUnitPrice[i].BackColor = this.stripe;
                    this.jobPrice[i].BackColor = this.stripe;
                    this.jobDetail[i].BackColor = this.stripe;
                    this.checkBox[i].BackColor = this.stripe;
                    this.label[i].BackColor = this.stripe;
                }
                base.Controls.Add(this.jobPrice[i]);
                base.Controls.Add(this.jobType[i]);
                base.Controls.Add(this.jobQty[i]);
                base.Controls.Add(this.jobUnitPrice[i]);
                base.Controls.Add(this.jobDetail[i]);
                base.Controls.Add(this.checkBox[i]);
                base.Controls.Add(this.label[i]);
            }
            this.RedrawArrayComponent();
        }

        private async System.Threading.Tasks.Task DisclaimerNoteAsync()
        {

            if (JobTypePopup.isWheelApp())
            {
                if (this.jobNotes.Text == null || this.jobNotes.Text.Length < 300)
                {
                    
                    this.jobNotes.Text += "DISCLAIMER NOTICE:\n" +
        "When Aluminium wheels have cracks or are damaged in any way the stresses caused by the impact cannot be truly identified without getting the wheel tested." +
        "We at "+JobCard.getBusinessName()+" weld the cracks and push out dents with a specific wheel repair machine designed and built in Europe." +
        "This does not in any way certify the wheel for further use on a Vehicle." +
        "We do not test wheels at "+JobCard.getBusinessName()+", and take no responsibility if the wheel is used on a vehicle without the wheel being certified." +
        "It is up to the owner or customer to get the wheel certified and tested for air leaks at their own cost if they feel it is necessary." +
        "We do not paint wheels.\nCUSTOMER SIGNATURE:   x\n";
                    if (await this.NeedSaveAsync(false, true))
                    {
                        var ok = await DataAccess.UpdateMongoAsync(this.updateSqlSetList);
                    /* PJC OLD
                    DataAccess.Update(this.updateSql)
                    */
                    
                    }
                }
            }
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.btnNewJob = new System.Windows.Forms.Button();
            this.btnIncompleteJobs = new System.Windows.Forms.Button();
            this.btnSearchLists = new System.Windows.Forms.Button();
            this.btnUnpaidCustomers = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.jobID = new System.Windows.Forms.Label();
            this.btnNavigateBack = new System.Windows.Forms.Button();
            this.btnNavigateForward = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.jobDate = new System.Windows.Forms.TextBox();
            this.jobCustomer = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.jobBusinessName = new System.Windows.Forms.TextBox();
            this.labelJobBusinessName = new System.Windows.Forms.Label();
            this.jobAddress = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.jobPhone = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.jobEmail = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.jobOrderNumber = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.jobDelivery = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.btnCollect = new System.Windows.Forms.Button();
            this.btnCourier = new System.Windows.Forms.Button();
            this.label9 = new System.Windows.Forms.Label();
            this.jobReceivedFrom = new System.Windows.Forms.ComboBox();
            this.jobDateRequired = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.jobDateCompleted = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.jobPaymentBy = new System.Windows.Forms.ComboBox();
            this.label12 = new System.Windows.Forms.Label();
            this.jobNotes = new System.Windows.Forms.TextBox();
            this.label13 = new System.Windows.Forms.Label();
            this.jobDatePaid = new System.Windows.Forms.TextBox();
            this.label14 = new System.Windows.Forms.Label();
            this.btnToday = new System.Windows.Forms.Button();
            this.btnCopper = new System.Windows.Forms.Button();
            this.btnNickle = new System.Windows.Forms.Button();
            this.btnChrome = new System.Windows.Forms.Button();
            this.btnBrass = new System.Windows.Forms.Button();
            this.btnBronze = new System.Windows.Forms.Button();
            this.btnTin = new System.Windows.Forms.Button();
            this.btnGold = new System.Windows.Forms.Button();
            this.btnSilver = new System.Windows.Forms.Button();
            this.btnSatin = new System.Windows.Forms.Button();
            this.btnGeorge = new System.Windows.Forms.Button();
            this.btnHenry = new System.Windows.Forms.Button();
            this.btnRakesh = new System.Windows.Forms.Button();
            this.btnBritt = new System.Windows.Forms.Button();
            this.datagrid = new System.Windows.Forms.DataGridView();
            this.btnExit = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnEmail = new System.Windows.Forms.Button();
            this.btnCam1 = new System.Windows.Forms.Button();
            this.btnCam2 = new System.Windows.Forms.Button();
            this.btnPrintCustomerCopy = new System.Windows.Forms.Button();
            this.btnPrintBusiness = new System.Windows.Forms.Button();
            this.jobCompleted = new System.Windows.Forms.CheckBox();
            this.panelSearchField = new System.Windows.Forms.Panel();
            this.lblResults = new System.Windows.Forms.Label();
            this.slider = new System.Windows.Forms.TrackBar();
            this.btnCancelSearch = new System.Windows.Forms.Button();
            this.btnSearchField = new System.Windows.Forms.Button();
            this.txtSearchField = new System.Windows.Forms.TextBox();
            this.lblSearchOnField = new System.Windows.Forms.Label();
            this.btnLatestJob = new System.Windows.Forms.Button();
            this.btnNextPhoto = new System.Windows.Forms.Button();
            this.btnPrintForWork = new System.Windows.Forms.Button();
            this.btnLockUnlock = new System.Windows.Forms.Button();
            this.btnUndo = new System.Windows.Forms.Button();
            this.picPaid = new System.Windows.Forms.PictureBox();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.btnTodayForDateCompleted = new System.Windows.Forms.Button();
            this.btnAddWeek = new System.Windows.Forms.Button();
            this.btnDuplicate = new System.Windows.Forms.Button();
            this.grpBoxPlating = new System.Windows.Forms.GroupBox();
            this.grpBoxPolish = new System.Windows.Forms.GroupBox();
            this.btnCollapseToggle = new System.Windows.Forms.Button();
            this.btnFussy = new System.Windows.Forms.Button();
            this.btnRDAddressSurcharge = new System.Windows.Forms.Button();
            this.getLatestTimer = new System.Windows.Forms.Timer(this.components);
            this.jobFussyNotes = new System.Windows.Forms.TextBox();
            this.jobGoodReserved = new System.Windows.Forms.CheckBox();
            this.cboCamera = new System.Windows.Forms.ComboBox();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.jobQuotation = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.datagrid)).BeginInit();
            this.panelSearchField.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.slider)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picPaid)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.grpBoxPlating.SuspendLayout();
            this.grpBoxPolish.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            this.SuspendLayout();
            // 
            // btnNewJob
            // 
            this.btnNewJob.Font = new System.Drawing.Font("Arial", 13F, System.Drawing.FontStyle.Bold);
            this.btnNewJob.Location = new System.Drawing.Point(12, 53);
            this.btnNewJob.Name = "btnNewJob";
            this.btnNewJob.Size = new System.Drawing.Size(162, 73);
            this.btnNewJob.TabIndex = 0;
            this.btnNewJob.Text = "New Job";
            this.btnNewJob.UseVisualStyleBackColor = true;
            this.btnNewJob.Click += new System.EventHandler(this.btnNewJob_Click);
            // 
            // btnIncompleteJobs
            // 
            this.btnIncompleteJobs.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnIncompleteJobs.Location = new System.Drawing.Point(12, 132);
            this.btnIncompleteJobs.Name = "btnIncompleteJobs";
            this.btnIncompleteJobs.Size = new System.Drawing.Size(160, 32);
            this.btnIncompleteJobs.TabIndex = 1;
            this.btnIncompleteJobs.Text = "Incomplete Jobs";
            this.btnIncompleteJobs.UseVisualStyleBackColor = true;
            this.btnIncompleteJobs.Click += new System.EventHandler(this.btnExistingJobs_Click);
            // 
            // btnSearchLists
            // 
            this.btnSearchLists.Font = new System.Drawing.Font("Arial", 13F, System.Drawing.FontStyle.Bold);
            this.btnSearchLists.Location = new System.Drawing.Point(13, 213);
            this.btnSearchLists.Name = "btnSearchLists";
            this.btnSearchLists.Size = new System.Drawing.Size(162, 31);
            this.btnSearchLists.TabIndex = 2;
            this.btnSearchLists.Text = "Search Lists";
            this.btnSearchLists.UseVisualStyleBackColor = true;
            this.btnSearchLists.Click += new System.EventHandler(this.btnSearchLists_Click);
            // 
            // btnUnpaidCustomers
            // 
            this.btnUnpaidCustomers.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold);
            this.btnUnpaidCustomers.Location = new System.Drawing.Point(13, 246);
            this.btnUnpaidCustomers.Name = "btnUnpaidCustomers";
            this.btnUnpaidCustomers.Size = new System.Drawing.Size(162, 31);
            this.btnUnpaidCustomers.TabIndex = 3;
            this.btnUnpaidCustomers.Text = "Unpaid Customers";
            this.btnUnpaidCustomers.UseVisualStyleBackColor = true;
            this.btnUnpaidCustomers.Click += new System.EventHandler(this.btnCustomers_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Arial", 11F);
            this.label1.Location = new System.Drawing.Point(186, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(87, 17);
            this.label1.TabIndex = 4;
            this.label1.Text = "Job Number";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.label1.Click += new System.EventHandler(this.DeleteJobClicked);
            // 
            // jobID
            // 
            this.jobID.AutoSize = true;
            this.jobID.Font = new System.Drawing.Font("Arial", 14F, System.Drawing.FontStyle.Bold);
            this.jobID.ForeColor = System.Drawing.Color.Red;
            this.jobID.Location = new System.Drawing.Point(279, 12);
            this.jobID.Name = "jobID";
            this.jobID.Size = new System.Drawing.Size(76, 22);
            this.jobID.TabIndex = 5;
            this.jobID.Text = "000000";
            this.jobID.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.jobID.TextChanged += new System.EventHandler(this.JobIDChanged);
            // 
            // btnNavigateBack
            // 
            this.btnNavigateBack.Location = new System.Drawing.Point(360, 12);
            this.btnNavigateBack.Name = "btnNavigateBack";
            this.btnNavigateBack.Size = new System.Drawing.Size(29, 21);
            this.btnNavigateBack.TabIndex = 6;
            this.btnNavigateBack.Text = "<<";
            this.btnNavigateBack.UseVisualStyleBackColor = true;
            this.btnNavigateBack.Click += new System.EventHandler(this.btnNavigateBack_Click);
            // 
            // btnNavigateForward
            // 
            this.btnNavigateForward.Location = new System.Drawing.Point(395, 12);
            this.btnNavigateForward.Name = "btnNavigateForward";
            this.btnNavigateForward.Size = new System.Drawing.Size(29, 21);
            this.btnNavigateForward.TabIndex = 7;
            this.btnNavigateForward.Text = ">>";
            this.btnNavigateForward.UseVisualStyleBackColor = true;
            this.btnNavigateForward.Click += new System.EventHandler(this.btnNavigateForward_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Arial", 11F);
            this.label2.Location = new System.Drawing.Point(436, 16);
            this.label2.Name = "label2";
            this.label2.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.label2.Size = new System.Drawing.Size(66, 17);
            this.label2.TabIndex = 9;
            this.label2.Text = "Job Date";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // jobDate
            // 
            this.jobDate.Font = new System.Drawing.Font("Arial", 11F);
            this.jobDate.Location = new System.Drawing.Point(503, 12);
            this.jobDate.Name = "jobDate";
            this.jobDate.Size = new System.Drawing.Size(92, 24);
            this.jobDate.TabIndex = 10;
            this.jobDate.Validating += new System.ComponentModel.CancelEventHandler(this.DateValidating);
            // 
            // jobCustomer
            // 
            this.jobCustomer.Font = new System.Drawing.Font("Arial", 11F);
            this.jobCustomer.Location = new System.Drawing.Point(306, 42);
            this.jobCustomer.Name = "jobCustomer";
            this.jobCustomer.Size = new System.Drawing.Size(289, 24);
            this.jobCustomer.TabIndex = 12;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Arial", 11F);
            this.label3.Location = new System.Drawing.Point(184, 45);
            this.label3.Name = "label3";
            this.label3.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.label3.Size = new System.Drawing.Size(116, 17);
            this.label3.TabIndex = 11;
            this.label3.Text = "Customer Name";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // jobBusinessName
            // 
            this.jobBusinessName.Font = new System.Drawing.Font("Arial", 11F);
            this.jobBusinessName.Location = new System.Drawing.Point(306, 72);
            this.jobBusinessName.Name = "jobBusinessName";
            this.jobBusinessName.Size = new System.Drawing.Size(289, 24);
            this.jobBusinessName.TabIndex = 13;
            // 
            // labelJobBusinessName
            // 
            this.labelJobBusinessName.AutoSize = true;
            this.labelJobBusinessName.Font = new System.Drawing.Font("Arial", 11F);
            this.labelJobBusinessName.Location = new System.Drawing.Point(184, 75);
            this.labelJobBusinessName.Name = "labelJobBusinessName";
            this.labelJobBusinessName.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.labelJobBusinessName.Size = new System.Drawing.Size(112, 17);
            this.labelJobBusinessName.TabIndex = 11;
            this.labelJobBusinessName.Text = "Business Name";
            this.labelJobBusinessName.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // jobAddress
            // 
            this.jobAddress.Font = new System.Drawing.Font("Arial", 11F);
            this.jobAddress.Location = new System.Drawing.Point(256, 103);
            this.jobAddress.Multiline = true;
            this.jobAddress.Name = "jobAddress";
            this.jobAddress.Size = new System.Drawing.Size(338, 62);
            this.jobAddress.TabIndex = 14;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Arial", 11F);
            this.label4.Location = new System.Drawing.Point(189, 105);
            this.label4.Name = "label4";
            this.label4.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.label4.Size = new System.Drawing.Size(62, 17);
            this.label4.TabIndex = 13;
            this.label4.Text = "Address";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // jobPhone
            // 
            this.jobPhone.Font = new System.Drawing.Font("Arial", 11F);
            this.jobPhone.Location = new System.Drawing.Point(724, 42);
            this.jobPhone.Name = "jobPhone";
            this.jobPhone.Size = new System.Drawing.Size(249, 24);
            this.jobPhone.TabIndex = 16;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Arial", 11F);
            this.label5.Location = new System.Drawing.Point(674, 45);
            this.label5.Name = "label5";
            this.label5.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.label5.Size = new System.Drawing.Size(50, 17);
            this.label5.TabIndex = 15;
            this.label5.Text = "Phone";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // jobEmail
            // 
            this.jobEmail.Font = new System.Drawing.Font("Arial", 11F);
            this.jobEmail.Location = new System.Drawing.Point(660, 72);
            this.jobEmail.Name = "jobEmail";
            this.jobEmail.Size = new System.Drawing.Size(313, 24);
            this.jobEmail.TabIndex = 18;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Arial", 11F);
            this.label6.Location = new System.Drawing.Point(609, 75);
            this.label6.Name = "label6";
            this.label6.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.label6.Size = new System.Drawing.Size(0, 17);
            this.label6.TabIndex = 17;
            this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // jobOrderNumber
            // 
            this.jobOrderNumber.Font = new System.Drawing.Font("Arial", 11F);
            this.jobOrderNumber.Location = new System.Drawing.Point(724, 12);
            this.jobOrderNumber.Name = "jobOrderNumber";
            this.jobOrderNumber.Size = new System.Drawing.Size(249, 24);
            this.jobOrderNumber.TabIndex = 20;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Arial", 11F);
            this.label7.Location = new System.Drawing.Point(624, 16);
            this.label7.Name = "label7";
            this.label7.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.label7.Size = new System.Drawing.Size(102, 17);
            this.label7.TabIndex = 19;
            this.label7.Text = "Order Number";
            this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // jobDelivery
            // 
            this.jobDelivery.Font = new System.Drawing.Font("Arial", 11F);
            this.jobDelivery.Location = new System.Drawing.Point(329, 169);
            this.jobDelivery.Multiline = true;
            this.jobDelivery.Name = "jobDelivery";
            this.jobDelivery.Size = new System.Drawing.Size(265, 43);
            this.jobDelivery.TabIndex = 22;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Arial", 11F);
            this.label8.Location = new System.Drawing.Point(186, 170);
            this.label8.Name = "label8";
            this.label8.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.label8.Size = new System.Drawing.Size(139, 17);
            this.label8.TabIndex = 21;
            this.label8.Text = "Delivery Instructions";
            this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // btnCollect
            // 
            this.btnCollect.Location = new System.Drawing.Point(257, 190);
            this.btnCollect.Name = "btnCollect";
            this.btnCollect.Size = new System.Drawing.Size(59, 22);
            this.btnCollect.TabIndex = 23;
            this.btnCollect.Text = "Collect";
            this.btnCollect.UseVisualStyleBackColor = true;
            this.btnCollect.Click += new System.EventHandler(this.btnCollect_Click);
            // 
            // btnCourier
            // 
            this.btnCourier.Location = new System.Drawing.Point(192, 190);
            this.btnCourier.Name = "btnCourier";
            this.btnCourier.Size = new System.Drawing.Size(59, 22);
            this.btnCourier.TabIndex = 24;
            this.btnCourier.Text = "Courier";
            this.btnCourier.UseVisualStyleBackColor = true;
            this.btnCourier.Click += new System.EventHandler(this.btnCourier_Click);
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("Arial", 11F);
            this.label9.Location = new System.Drawing.Point(808, 162);
            this.label9.Name = "label9";
            this.label9.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.label9.Size = new System.Drawing.Size(108, 17);
            this.label9.TabIndex = 25;
            this.label9.Text = "Received From";
            this.label9.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // jobReceivedFrom
            // 
            this.jobReceivedFrom.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.jobReceivedFrom.Font = new System.Drawing.Font("Arial", 11F);
            this.jobReceivedFrom.FormattingEnabled = true;
            this.jobReceivedFrom.Items.AddRange(new object[] {
            "",
            "Customer",
            "Courier"});
            this.jobReceivedFrom.Location = new System.Drawing.Point(922, 159);
            this.jobReceivedFrom.Name = "jobReceivedFrom";
            this.jobReceivedFrom.Size = new System.Drawing.Size(122, 25);
            this.jobReceivedFrom.TabIndex = 26;
            // 
            // jobDateRequired
            // 
            this.jobDateRequired.Font = new System.Drawing.Font("Arial", 11F);
            this.jobDateRequired.Location = new System.Drawing.Point(721, 103);
            this.jobDateRequired.Name = "jobDateRequired";
            this.jobDateRequired.Size = new System.Drawing.Size(72, 24);
            this.jobDateRequired.TabIndex = 28;
            this.jobDateRequired.Validating += new System.ComponentModel.CancelEventHandler(this.DateValidating);
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Font = new System.Drawing.Font("Arial", 11F);
            this.label10.Location = new System.Drawing.Point(601, 104);
            this.label10.Name = "label10";
            this.label10.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.label10.Size = new System.Drawing.Size(102, 17);
            this.label10.TabIndex = 27;
            this.label10.Text = "Date Required";
            this.label10.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // jobDateCompleted
            // 
            this.jobDateCompleted.Font = new System.Drawing.Font("Arial", 11F);
            this.jobDateCompleted.Location = new System.Drawing.Point(721, 129);
            this.jobDateCompleted.Name = "jobDateCompleted";
            this.jobDateCompleted.Size = new System.Drawing.Size(72, 24);
            this.jobDateCompleted.TabIndex = 30;
            this.jobDateCompleted.Validating += new System.ComponentModel.CancelEventHandler(this.DateValidating);
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Font = new System.Drawing.Font("Arial", 11F);
            this.label11.Location = new System.Drawing.Point(601, 132);
            this.label11.Name = "label11";
            this.label11.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.label11.Size = new System.Drawing.Size(114, 17);
            this.label11.TabIndex = 29;
            this.label11.Text = "Date Completed";
            this.label11.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // jobPaymentBy
            // 
            this.jobPaymentBy.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.jobPaymentBy.Font = new System.Drawing.Font("Arial", 11F);
            this.jobPaymentBy.FormattingEnabled = true;
            this.jobPaymentBy.Items.AddRange(new object[] {
            "",
            "Cash",
            "Eftpos",
            "VISA",
            "MasterCard"});
            this.jobPaymentBy.Location = new System.Drawing.Point(694, 159);
            this.jobPaymentBy.Name = "jobPaymentBy";
            this.jobPaymentBy.Size = new System.Drawing.Size(108, 25);
            this.jobPaymentBy.TabIndex = 32;
            this.jobPaymentBy.TextChanged += new System.EventHandler(this.CheckForCreditCardSurcharge);
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Font = new System.Drawing.Font("Arial", 11F);
            this.label12.Location = new System.Drawing.Point(601, 160);
            this.label12.Name = "label12";
            this.label12.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.label12.Size = new System.Drawing.Size(87, 17);
            this.label12.TabIndex = 31;
            this.label12.Text = "Payment By";
            this.label12.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // jobNotes
            // 
            this.jobNotes.Font = new System.Drawing.Font("Arial", 12F);
            this.jobNotes.Location = new System.Drawing.Point(243, 220);
            this.jobNotes.Multiline = true;
            this.jobNotes.Name = "jobNotes";
            this.jobNotes.Size = new System.Drawing.Size(352, 78);
            this.jobNotes.TabIndex = 34;
            this.jobNotes.Enter += new System.EventHandler(this.OnNotesEnter);
            this.jobNotes.Leave += new System.EventHandler(this.OnNotesLeave);
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Font = new System.Drawing.Font("Arial", 11F);
            this.label13.Location = new System.Drawing.Point(191, 224);
            this.label13.Name = "label13";
            this.label13.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.label13.Size = new System.Drawing.Size(46, 17);
            this.label13.TabIndex = 33;
            this.label13.Text = "Notes";
            this.label13.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // jobDatePaid
            // 
            this.jobDatePaid.Font = new System.Drawing.Font("Arial", 11F);
            this.jobDatePaid.Location = new System.Drawing.Point(679, 194);
            this.jobDatePaid.Name = "jobDatePaid";
            this.jobDatePaid.Size = new System.Drawing.Size(72, 24);
            this.jobDatePaid.TabIndex = 36;
            this.jobDatePaid.TextChanged += new System.EventHandler(this.TogglePaidStamp);
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Font = new System.Drawing.Font("Arial", 11F);
            this.label14.Location = new System.Drawing.Point(601, 194);
            this.label14.Name = "label14";
            this.label14.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.label14.Size = new System.Drawing.Size(72, 17);
            this.label14.TabIndex = 35;
            this.label14.Text = "Date Paid";
            this.label14.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // btnToday
            // 
            this.btnToday.Location = new System.Drawing.Point(756, 190);
            this.btnToday.Name = "btnToday";
            this.btnToday.Size = new System.Drawing.Size(73, 28);
            this.btnToday.TabIndex = 37;
            this.btnToday.Text = "Today";
            this.btnToday.UseVisualStyleBackColor = true;
            this.btnToday.Click += new System.EventHandler(this.btnToday_Click);
            // 
            // btnCopper
            // 
            this.btnCopper.Location = new System.Drawing.Point(20, 19);
            this.btnCopper.Name = "btnCopper";
            this.btnCopper.Size = new System.Drawing.Size(51, 23);
            this.btnCopper.TabIndex = 39;
            this.btnCopper.Text = "Copper";
            this.btnCopper.UseVisualStyleBackColor = true;
            this.btnCopper.Click += new System.EventHandler(this.MetalToPolish);
            // 
            // btnNickle
            // 
            this.btnNickle.Location = new System.Drawing.Point(81, 19);
            this.btnNickle.Name = "btnNickle";
            this.btnNickle.Size = new System.Drawing.Size(51, 23);
            this.btnNickle.TabIndex = 40;
            this.btnNickle.Text = "Nickle";
            this.btnNickle.UseVisualStyleBackColor = true;
            this.btnNickle.Click += new System.EventHandler(this.MetalToPolish);
            // 
            // btnChrome
            // 
            this.btnChrome.Location = new System.Drawing.Point(141, 19);
            this.btnChrome.Name = "btnChrome";
            this.btnChrome.Size = new System.Drawing.Size(51, 23);
            this.btnChrome.TabIndex = 41;
            this.btnChrome.Text = "Chrome";
            this.btnChrome.UseVisualStyleBackColor = true;
            this.btnChrome.Click += new System.EventHandler(this.MetalToPolish);
            // 
            // btnBrass
            // 
            this.btnBrass.Location = new System.Drawing.Point(141, 48);
            this.btnBrass.Name = "btnBrass";
            this.btnBrass.Size = new System.Drawing.Size(51, 23);
            this.btnBrass.TabIndex = 42;
            this.btnBrass.Text = "Brass";
            this.btnBrass.UseVisualStyleBackColor = true;
            this.btnBrass.Click += new System.EventHandler(this.MetalToPolish);
            // 
            // btnBronze
            // 
            this.btnBronze.Location = new System.Drawing.Point(141, 76);
            this.btnBronze.Name = "btnBronze";
            this.btnBronze.Size = new System.Drawing.Size(51, 23);
            this.btnBronze.TabIndex = 43;
            this.btnBronze.Text = "Bronze";
            this.btnBronze.UseVisualStyleBackColor = true;
            this.btnBronze.Click += new System.EventHandler(this.MetalToPolish);
            // 
            // btnTin
            // 
            this.btnTin.Location = new System.Drawing.Point(20, 48);
            this.btnTin.Name = "btnTin";
            this.btnTin.Size = new System.Drawing.Size(51, 23);
            this.btnTin.TabIndex = 44;
            this.btnTin.Text = "Tin";
            this.btnTin.UseVisualStyleBackColor = true;
            this.btnTin.Click += new System.EventHandler(this.MetalToPolish);
            // 
            // btnGold
            // 
            this.btnGold.Location = new System.Drawing.Point(81, 76);
            this.btnGold.Name = "btnGold";
            this.btnGold.Size = new System.Drawing.Size(51, 23);
            this.btnGold.TabIndex = 47;
            this.btnGold.Text = "Gold";
            this.btnGold.UseVisualStyleBackColor = true;
            this.btnGold.Click += new System.EventHandler(this.MetalToPolish);
            // 
            // btnSilver
            // 
            this.btnSilver.Location = new System.Drawing.Point(20, 76);
            this.btnSilver.Name = "btnSilver";
            this.btnSilver.Size = new System.Drawing.Size(51, 23);
            this.btnSilver.TabIndex = 46;
            this.btnSilver.Text = "Silver";
            this.btnSilver.UseVisualStyleBackColor = true;
            this.btnSilver.Click += new System.EventHandler(this.MetalToPolish);
            // 
            // btnSatin
            // 
            this.btnSatin.Location = new System.Drawing.Point(81, 48);
            this.btnSatin.Name = "btnSatin";
            this.btnSatin.Size = new System.Drawing.Size(51, 23);
            this.btnSatin.TabIndex = 45;
            this.btnSatin.Text = "Satin";
            this.btnSatin.UseVisualStyleBackColor = true;
            this.btnSatin.Click += new System.EventHandler(this.MetalToPolish);
            // 
            // btnGeorge
            // 
            this.btnGeorge.Location = new System.Drawing.Point(6, 15);
            this.btnGeorge.Name = "btnGeorge";
            this.btnGeorge.Size = new System.Drawing.Size(31, 28);
            this.btnGeorge.TabIndex = 49;
            this.btnGeorge.Text = "G";
            this.btnGeorge.UseVisualStyleBackColor = true;
            this.btnGeorge.Click += new System.EventHandler(this.PolisherSelect);
            // 
            // btnHenry
            // 
            this.btnHenry.Location = new System.Drawing.Point(42, 15);
            this.btnHenry.Name = "btnHenry";
            this.btnHenry.Size = new System.Drawing.Size(31, 28);
            this.btnHenry.TabIndex = 50;
            this.btnHenry.Text = "H";
            this.btnHenry.UseVisualStyleBackColor = true;
            this.btnHenry.Click += new System.EventHandler(this.PolisherSelect);
            // 
            // btnRakesh
            // 
            this.btnRakesh.Location = new System.Drawing.Point(6, 46);
            this.btnRakesh.Name = "btnRakesh";
            this.btnRakesh.Size = new System.Drawing.Size(31, 28);
            this.btnRakesh.TabIndex = 51;
            this.btnRakesh.Text = "R";
            this.btnRakesh.UseVisualStyleBackColor = true;
            this.btnRakesh.Click += new System.EventHandler(this.PolisherSelect);
            // 
            // btnBritt
            // 
            this.btnBritt.Location = new System.Drawing.Point(42, 46);
            this.btnBritt.Name = "btnBritt";
            this.btnBritt.Size = new System.Drawing.Size(31, 28);
            this.btnBritt.TabIndex = 52;
            this.btnBritt.Text = "B";
            this.btnBritt.UseVisualStyleBackColor = true;
            this.btnBritt.Click += new System.EventHandler(this.PolisherSelect);
            // 
            // datagrid
            // 
            this.datagrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.datagrid.Location = new System.Drawing.Point(13, -23);
            this.datagrid.Name = "datagrid";
            this.datagrid.Size = new System.Drawing.Size(159, 37);
            this.datagrid.TabIndex = 54;
            this.datagrid.Visible = false;
            // 
            // btnExit
            // 
            this.btnExit.Font = new System.Drawing.Font("Arial", 13F, System.Drawing.FontStyle.Bold);
            this.btnExit.Location = new System.Drawing.Point(12, 392);
            this.btnExit.Name = "btnExit";
            this.btnExit.Size = new System.Drawing.Size(162, 42);
            this.btnExit.TabIndex = 55;
            this.btnExit.Text = "Exit";
            this.btnExit.UseVisualStyleBackColor = true;
            this.btnExit.Click += new System.EventHandler(this.btnExit_Click);
            // 
            // btnSave
            // 
            this.btnSave.Font = new System.Drawing.Font("Arial", 13F, System.Drawing.FontStyle.Bold);
            this.btnSave.Location = new System.Drawing.Point(604, 224);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(137, 68);
            this.btnSave.TabIndex = 56;
            this.btnSave.Text = "Save Job";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnEmail
            // 
            this.btnEmail.Font = new System.Drawing.Font("Arial", 9F);
            this.btnEmail.Location = new System.Drawing.Point(607, 72);
            this.btnEmail.Name = "btnEmail";
            this.btnEmail.Size = new System.Drawing.Size(51, 23);
            this.btnEmail.TabIndex = 57;
            this.btnEmail.Text = "email";
            this.btnEmail.UseVisualStyleBackColor = true;
            this.btnEmail.Click += new System.EventHandler(this.btnEmail_Click);
            // 
            // btnCam1
            // 
            this.btnCam1.Font = new System.Drawing.Font("Arial", 13F);
            this.btnCam1.Location = new System.Drawing.Point(875, 194);
            this.btnCam1.Name = "btnCam1";
            this.btnCam1.Size = new System.Drawing.Size(167, 40);
            this.btnCam1.TabIndex = 56;
            this.btnCam1.Text = "Toggle ON Cam";
            this.btnCam1.UseVisualStyleBackColor = true;
            this.btnCam1.Click += new System.EventHandler(this.btnCam1_Click);
            // 
            // btnCam2
            // 
            this.btnCam2.Font = new System.Drawing.Font("Arial", 13F);
            this.btnCam2.Location = new System.Drawing.Point(916, 242);
            this.btnCam2.Name = "btnCam2";
            this.btnCam2.Size = new System.Drawing.Size(137, 50);
            this.btnCam2.TabIndex = 56;
            this.btnCam2.Text = "Snap Cam";
            this.btnCam2.UseVisualStyleBackColor = true;
            this.btnCam2.Click += new System.EventHandler(this.btnCam2_Click);
            // 
            // btnPrintCustomerCopy
            // 
            this.btnPrintCustomerCopy.Font = new System.Drawing.Font("Arial", 13F, System.Drawing.FontStyle.Bold);
            this.btnPrintCustomerCopy.Location = new System.Drawing.Point(12, 573);
            this.btnPrintCustomerCopy.Name = "btnPrintCustomerCopy";
            this.btnPrintCustomerCopy.Size = new System.Drawing.Size(161, 42);
            this.btnPrintCustomerCopy.TabIndex = 58;
            this.btnPrintCustomerCopy.Text = "Print Customer";
            this.btnPrintCustomerCopy.UseVisualStyleBackColor = true;
            this.btnPrintCustomerCopy.Click += new System.EventHandler(this.btnPrintCustomerCopy_Click);
            // 
            // btnPrintBusiness
            // 
            this.btnPrintBusiness.Font = new System.Drawing.Font("Arial", 13F, System.Drawing.FontStyle.Bold);
            this.btnPrintBusiness.Location = new System.Drawing.Point(12, 621);
            this.btnPrintBusiness.Name = "btnPrintBusiness";
            this.btnPrintBusiness.Size = new System.Drawing.Size(161, 42);
            this.btnPrintBusiness.TabIndex = 59;
            this.btnPrintBusiness.Text = "Print ACP Copy";
            this.btnPrintBusiness.UseVisualStyleBackColor = true;
            this.btnPrintBusiness.Click += new System.EventHandler(this.btnPrintBusiness_Click);
            // 
            // jobCompleted
            // 
            this.jobCompleted.AutoSize = true;
            this.jobCompleted.Font = new System.Drawing.Font("Arial", 12F);
            this.jobCompleted.Location = new System.Drawing.Point(12, 543);
            this.jobCompleted.Name = "jobCompleted";
            this.jobCompleted.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.jobCompleted.Size = new System.Drawing.Size(151, 22);
            this.jobCompleted.TabIndex = 60;
            this.jobCompleted.Text = "Duplicate Receipt";
            this.jobCompleted.UseVisualStyleBackColor = true;
            this.jobCompleted.CheckedChanged += new System.EventHandler(this.jobCompleted_CheckedChanged);
            // 
            // panelSearchField
            // 
            this.panelSearchField.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            this.panelSearchField.Controls.Add(this.lblResults);
            this.panelSearchField.Controls.Add(this.slider);
            this.panelSearchField.Controls.Add(this.btnCancelSearch);
            this.panelSearchField.Controls.Add(this.btnSearchField);
            this.panelSearchField.Controls.Add(this.txtSearchField);
            this.panelSearchField.Controls.Add(this.lblSearchOnField);
            this.panelSearchField.Location = new System.Drawing.Point(660, 452);
            this.panelSearchField.Name = "panelSearchField";
            this.panelSearchField.Size = new System.Drawing.Size(496, 195);
            this.panelSearchField.TabIndex = 61;
            this.panelSearchField.Visible = false;
            this.panelSearchField.Paint += new System.Windows.Forms.PaintEventHandler(this.panelSearchField_Paint);
            this.panelSearchField.MouseDown += new System.Windows.Forms.MouseEventHandler(this.PanelMouseDown);
            this.panelSearchField.MouseMove += new System.Windows.Forms.MouseEventHandler(this.panelSearchField_MouseMove);
            this.panelSearchField.MouseUp += new System.Windows.Forms.MouseEventHandler(this.PanelMouseUp);
            // 
            // lblResults
            // 
            this.lblResults.AutoSize = true;
            this.lblResults.Font = new System.Drawing.Font("Arial", 14F);
            this.lblResults.Location = new System.Drawing.Point(136, 169);
            this.lblResults.Name = "lblResults";
            this.lblResults.Size = new System.Drawing.Size(0, 22);
            this.lblResults.TabIndex = 6;
            // 
            // slider
            // 
            this.slider.Location = new System.Drawing.Point(34, 121);
            this.slider.Name = "slider";
            this.slider.Size = new System.Drawing.Size(428, 45);
            this.slider.TabIndex = 5;
            this.slider.Visible = false;
            this.slider.Scroll += new System.EventHandler(this.slider_Scroll);
            // 
            // btnCancelSearch
            // 
            this.btnCancelSearch.Font = new System.Drawing.Font("Arial", 14F);
            this.btnCancelSearch.Location = new System.Drawing.Point(340, 82);
            this.btnCancelSearch.Name = "btnCancelSearch";
            this.btnCancelSearch.Size = new System.Drawing.Size(115, 33);
            this.btnCancelSearch.TabIndex = 4;
            this.btnCancelSearch.Text = "Cancel";
            this.btnCancelSearch.UseVisualStyleBackColor = true;
            this.btnCancelSearch.Click += new System.EventHandler(this.btnCancelSearch_Click);
            // 
            // btnSearchField
            // 
            this.btnSearchField.Font = new System.Drawing.Font("Arial", 14F);
            this.btnSearchField.Location = new System.Drawing.Point(42, 82);
            this.btnSearchField.Name = "btnSearchField";
            this.btnSearchField.Size = new System.Drawing.Size(115, 33);
            this.btnSearchField.TabIndex = 3;
            this.btnSearchField.Text = "Search";
            this.btnSearchField.UseVisualStyleBackColor = true;
            this.btnSearchField.Click += new System.EventHandler(this.btnSearchField_Click);
            // 
            // txtSearchField
            // 
            this.txtSearchField.Font = new System.Drawing.Font("Arial", 14F);
            this.txtSearchField.Location = new System.Drawing.Point(42, 41);
            this.txtSearchField.Name = "txtSearchField";
            this.txtSearchField.Size = new System.Drawing.Size(413, 29);
            this.txtSearchField.TabIndex = 2;
            this.txtSearchField.TextChanged += new System.EventHandler(this.txtSearchField_TextChanged);
            // 
            // lblSearchOnField
            // 
            this.lblSearchOnField.AutoSize = true;
            this.lblSearchOnField.Font = new System.Drawing.Font("Arial", 15F);
            this.lblSearchOnField.Location = new System.Drawing.Point(159, 15);
            this.lblSearchOnField.Name = "lblSearchOnField";
            this.lblSearchOnField.Size = new System.Drawing.Size(141, 23);
            this.lblSearchOnField.TabIndex = 0;
            this.lblSearchOnField.Text = "Search on field";
            // 
            // btnLatestJob
            // 
            this.btnLatestJob.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold);
            this.btnLatestJob.Location = new System.Drawing.Point(12, 279);
            this.btnLatestJob.Name = "btnLatestJob";
            this.btnLatestJob.Size = new System.Drawing.Size(162, 26);
            this.btnLatestJob.TabIndex = 62;
            this.btnLatestJob.Text = "Latest Job";
            this.btnLatestJob.UseVisualStyleBackColor = true;
            this.btnLatestJob.Click += new System.EventHandler(this.btnLatestJob_Click);
            // 
            // btnNextPhoto
            // 
            this.btnNextPhoto.Font = new System.Drawing.Font("Arial", 10F);
            this.btnNextPhoto.Location = new System.Drawing.Point(990, 3);
            this.btnNextPhoto.Name = "btnNextPhoto";
            this.btnNextPhoto.Size = new System.Drawing.Size(54, 146);
            this.btnNextPhoto.TabIndex = 63;
            this.btnNextPhoto.Text = "Next Photo";
            this.btnNextPhoto.UseVisualStyleBackColor = true;
            this.btnNextPhoto.Click += new System.EventHandler(this.btnNextPhoto_Click);
            // 
            // btnPrintForWork
            // 
            this.btnPrintForWork.Font = new System.Drawing.Font("Arial", 13F, System.Drawing.FontStyle.Bold);
            this.btnPrintForWork.Location = new System.Drawing.Point(12, 669);
            this.btnPrintForWork.Name = "btnPrintForWork";
            this.btnPrintForWork.Size = new System.Drawing.Size(161, 42);
            this.btnPrintForWork.TabIndex = 64;
            this.btnPrintForWork.Text = "Print for Work";
            this.btnPrintForWork.UseVisualStyleBackColor = true;
            this.btnPrintForWork.Click += new System.EventHandler(this.btnPrintForWork_Click);
            // 
            // btnLockUnlock
            // 
            this.btnLockUnlock.Font = new System.Drawing.Font("Arial", 13F, System.Drawing.FontStyle.Bold);
            this.btnLockUnlock.Location = new System.Drawing.Point(12, 488);
            this.btnLockUnlock.Name = "btnLockUnlock";
            this.btnLockUnlock.Size = new System.Drawing.Size(162, 42);
            this.btnLockUnlock.TabIndex = 65;
            this.btnLockUnlock.Text = "Lock";
            this.btnLockUnlock.UseVisualStyleBackColor = true;
            this.btnLockUnlock.Click += new System.EventHandler(this.btnLockUnlock_Click);
            // 
            // btnUndo
            // 
            this.btnUndo.Font = new System.Drawing.Font("Arial", 13F, System.Drawing.FontStyle.Bold);
            this.btnUndo.Location = new System.Drawing.Point(12, 440);
            this.btnUndo.Name = "btnUndo";
            this.btnUndo.Size = new System.Drawing.Size(162, 42);
            this.btnUndo.TabIndex = 66;
            this.btnUndo.Text = "Undo";
            this.btnUndo.UseVisualStyleBackColor = true;
            this.btnUndo.Click += new System.EventHandler(this.btnUndo_Click);
            // 
            // picPaid
            // 
            this.picPaid.Location = new System.Drawing.Point(887, 102);
            this.picPaid.Name = "picPaid";
            this.picPaid.Size = new System.Drawing.Size(97, 51);
            this.picPaid.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picPaid.TabIndex = 38;
            this.picPaid.TabStop = false;
            this.picPaid.Visible = false;
            // 
            // pictureBox1
            // 
            this.pictureBox1.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.pictureBox1.Location = new System.Drawing.Point(1061, 28);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(295, 265);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 8;
            this.pictureBox1.TabStop = false;
            this.pictureBox1.Click += new System.EventHandler(this.pictureBox1_Click);
            // 
            // btnTodayForDateCompleted
            // 
            this.btnTodayForDateCompleted.Location = new System.Drawing.Point(798, 131);
            this.btnTodayForDateCompleted.Name = "btnTodayForDateCompleted";
            this.btnTodayForDateCompleted.Size = new System.Drawing.Size(73, 22);
            this.btnTodayForDateCompleted.TabIndex = 68;
            this.btnTodayForDateCompleted.Text = "Today";
            this.btnTodayForDateCompleted.UseVisualStyleBackColor = true;
            this.btnTodayForDateCompleted.Click += new System.EventHandler(this.btnTodayForDateCompleted_Click);
            // 
            // btnAddWeek
            // 
            this.btnAddWeek.Font = new System.Drawing.Font("Arial", 8F);
            this.btnAddWeek.Location = new System.Drawing.Point(799, 101);
            this.btnAddWeek.Name = "btnAddWeek";
            this.btnAddWeek.Size = new System.Drawing.Size(72, 22);
            this.btnAddWeek.TabIndex = 69;
            this.btnAddWeek.Text = "+1 week";
            this.btnAddWeek.UseVisualStyleBackColor = true;
            this.btnAddWeek.Click += new System.EventHandler(this.btnAddWeek_Click);
            // 
            // btnDuplicate
            // 
            this.btnDuplicate.Font = new System.Drawing.Font("Arial", 13F, System.Drawing.FontStyle.Bold);
            this.btnDuplicate.Location = new System.Drawing.Point(13, 5);
            this.btnDuplicate.Name = "btnDuplicate";
            this.btnDuplicate.Size = new System.Drawing.Size(162, 42);
            this.btnDuplicate.TabIndex = 70;
            this.btnDuplicate.Text = "Duplicate Job";
            this.btnDuplicate.UseVisualStyleBackColor = true;
            this.btnDuplicate.Click += new System.EventHandler(this.btnDuplicate_Click);
            // 
            // grpBoxPlating
            // 
            this.grpBoxPlating.Controls.Add(this.btnChrome);
            this.grpBoxPlating.Controls.Add(this.btnCopper);
            this.grpBoxPlating.Controls.Add(this.btnNickle);
            this.grpBoxPlating.Controls.Add(this.btnBrass);
            this.grpBoxPlating.Controls.Add(this.btnBronze);
            this.grpBoxPlating.Controls.Add(this.btnSilver);
            this.grpBoxPlating.Controls.Add(this.btnTin);
            this.grpBoxPlating.Controls.Add(this.btnSatin);
            this.grpBoxPlating.Controls.Add(this.btnGold);
            this.grpBoxPlating.Location = new System.Drawing.Point(835, 194);
            this.grpBoxPlating.Name = "grpBoxPlating";
            this.grpBoxPlating.Size = new System.Drawing.Size(209, 105);
            this.grpBoxPlating.TabIndex = 71;
            this.grpBoxPlating.TabStop = false;
            this.grpBoxPlating.Text = "Plating";
            this.grpBoxPlating.Visible = false;
            // 
            // grpBoxPolish
            // 
            this.grpBoxPolish.Controls.Add(this.btnRakesh);
            this.grpBoxPolish.Controls.Add(this.btnGeorge);
            this.grpBoxPolish.Controls.Add(this.btnHenry);
            this.grpBoxPolish.Controls.Add(this.btnBritt);
            this.grpBoxPolish.Location = new System.Drawing.Point(747, 219);
            this.grpBoxPolish.Name = "grpBoxPolish";
            this.grpBoxPolish.Size = new System.Drawing.Size(82, 75);
            this.grpBoxPolish.TabIndex = 72;
            this.grpBoxPolish.TabStop = false;
            this.grpBoxPolish.Text = "Polish";
            this.grpBoxPolish.Visible = false;
            // 
            // btnCollapseToggle
            // 
            this.btnCollapseToggle.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCollapseToggle.Location = new System.Drawing.Point(12, 308);
            this.btnCollapseToggle.Name = "btnCollapseToggle";
            this.btnCollapseToggle.Size = new System.Drawing.Size(160, 39);
            this.btnCollapseToggle.TabIndex = 73;
            this.btnCollapseToggle.Text = "Collapse/Expand";
            this.btnCollapseToggle.UseVisualStyleBackColor = true;
            this.btnCollapseToggle.Click += new System.EventHandler(this.btnCollapseToggle_Click);
            // 
            // btnFussy
            // 
            this.btnFussy.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(128)))));
            this.btnFussy.Location = new System.Drawing.Point(194, 126);
            this.btnFussy.Name = "btnFussy";
            this.btnFussy.Size = new System.Drawing.Size(56, 39);
            this.btnFussy.TabIndex = 81;
            this.btnFussy.Text = "!";
            this.btnFussy.UseVisualStyleBackColor = false;
            this.btnFussy.Click += new System.EventHandler(this.btnFussy_Click);
            // 
            // btnRDAddressSurcharge
            // 
            this.btnRDAddressSurcharge.Font = new System.Drawing.Font("Arial", 6F);
            this.btnRDAddressSurcharge.Location = new System.Drawing.Point(1000, 500);
            this.btnRDAddressSurcharge.Name = "btnRDAddressSurcharge";
            this.btnRDAddressSurcharge.Size = new System.Drawing.Size(47, 20);
            this.btnRDAddressSurcharge.TabIndex = 82;
            this.btnRDAddressSurcharge.Text = "RD";
            this.btnRDAddressSurcharge.UseVisualStyleBackColor = true;
            this.btnRDAddressSurcharge.Click += new System.EventHandler(this.btnRDAddressSurcharge_Click);
            // 
            // getLatestTimer
            // 
            this.getLatestTimer.Enabled = true;
            this.getLatestTimer.Interval = 1000;
            this.getLatestTimer.Tick += new System.EventHandler(this.getLatestTimer_Tick);
            // 
            // jobFussyNotes
            // 
            this.jobFussyNotes.Font = new System.Drawing.Font("Arial", 12F);
            this.jobFussyNotes.Location = new System.Drawing.Point(187, 244);
            this.jobFussyNotes.Multiline = true;
            this.jobFussyNotes.Name = "jobFussyNotes";
            this.jobFussyNotes.Size = new System.Drawing.Size(50, 55);
            this.jobFussyNotes.TabIndex = 83;
            this.jobFussyNotes.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            this.jobFussyNotes.Enter += new System.EventHandler(this.ShowFussyNotes);
            this.jobFussyNotes.Leave += new System.EventHandler(this.OnFussyNotesLeave);
            // 
            // jobGoodReserved
            // 
            this.jobGoodReserved.BackColor = System.Drawing.SystemColors.Control;
            this.jobGoodReserved.Location = new System.Drawing.Point(13, 170);
            this.jobGoodReserved.Name = "jobGoodReserved";
            this.jobGoodReserved.Size = new System.Drawing.Size(160, 37);
            this.jobGoodReserved.TabIndex = 84;
            this.jobGoodReserved.Text = "Reserved / Good Customer";
            this.jobGoodReserved.UseVisualStyleBackColor = false;
            this.jobGoodReserved.CheckedChanged += new System.EventHandler(this.jobGoodReserved_CheckedChanged);
            // 
            // cboCamera
            // 
            this.cboCamera.FormattingEnabled = true;
            this.cboCamera.Location = new System.Drawing.Point(1061, 3);
            this.cboCamera.Name = "cboCamera";
            this.cboCamera.Size = new System.Drawing.Size(281, 21);
            this.cboCamera.TabIndex = 85;
            this.cboCamera.SelectedIndexChanged += new System.EventHandler(this.cboCamera_SelectedIndexChanged);
            // 
            // pictureBox2
            // 
            this.pictureBox2.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.pictureBox2.Location = new System.Drawing.Point(1059, 27);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(295, 265);
            this.pictureBox2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox2.TabIndex = 86;
            this.pictureBox2.TabStop = false;
            this.pictureBox2.Visible = false;
            // 
            // jobQuotation
            // 
            this.jobQuotation.BackColor = System.Drawing.SystemColors.Control;
            this.jobQuotation.Location = new System.Drawing.Point(604, 36);
            this.jobQuotation.Name = "jobQuotation";
            this.jobQuotation.Size = new System.Drawing.Size(74, 36);
            this.jobQuotation.TabIndex = 87;
            this.jobQuotation.Text = "Quotation";
            this.jobQuotation.UseVisualStyleBackColor = false;
            this.jobQuotation.CheckedChanged += new System.EventHandler(this.jobQuotation_CheckedChanged);
            // 
            // JobCard
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(1361, 722);
            this.Controls.Add(this.jobQuotation);
            this.Controls.Add(this.pictureBox2);
            this.Controls.Add(this.cboCamera);
            this.Controls.Add(this.jobGoodReserved);
            this.Controls.Add(this.jobFussyNotes);
            this.Controls.Add(this.btnFussy);
            this.Controls.Add(this.btnRDAddressSurcharge);
            this.Controls.Add(this.btnCollapseToggle);
            this.Controls.Add(this.grpBoxPolish);
            this.Controls.Add(this.grpBoxPlating);
            this.Controls.Add(this.panelSearchField);
            this.Controls.Add(this.btnDuplicate);
            this.Controls.Add(this.btnAddWeek);
            this.Controls.Add(this.btnTodayForDateCompleted);
            this.Controls.Add(this.btnUndo);
            this.Controls.Add(this.btnLockUnlock);
            this.Controls.Add(this.btnPrintForWork);
            this.Controls.Add(this.btnNextPhoto);
            this.Controls.Add(this.btnLatestJob);
            this.Controls.Add(this.jobCompleted);
            this.Controls.Add(this.btnPrintBusiness);
            this.Controls.Add(this.btnPrintCustomerCopy);
            this.Controls.Add(this.btnEmail);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.btnCam1);
            this.Controls.Add(this.btnCam2);
            this.Controls.Add(this.btnExit);
            this.Controls.Add(this.datagrid);
            this.Controls.Add(this.picPaid);
            this.Controls.Add(this.btnToday);
            this.Controls.Add(this.jobDatePaid);
            this.Controls.Add(this.label14);
            this.Controls.Add(this.jobNotes);
            this.Controls.Add(this.label13);
            this.Controls.Add(this.jobPaymentBy);
            this.Controls.Add(this.label12);
            this.Controls.Add(this.jobDateCompleted);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.jobDateRequired);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.jobReceivedFrom);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.btnCourier);
            this.Controls.Add(this.btnCollect);
            this.Controls.Add(this.jobDelivery);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.jobOrderNumber);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.jobEmail);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.jobPhone);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.jobAddress);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.jobCustomer);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.jobBusinessName);
            this.Controls.Add(this.labelJobBusinessName);
            this.Controls.Add(this.jobDate);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.btnNavigateForward);
            this.Controls.Add(this.btnNavigateBack);
            this.Controls.Add(this.jobID);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnUnpaidCustomers);
            this.Controls.Add(this.btnSearchLists);
            this.Controls.Add(this.btnIncompleteJobs);
            this.Controls.Add(this.btnNewJob);
            this.Name = "JobCard";
            this.Text = "JobCard";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.CheckBeforeQuit);
            this.Click += new System.EventHandler(this.JobCard_Click);
            this.ControlAdded += new System.Windows.Forms.ControlEventHandler(this.ControlAdd);
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.DoDragDrop);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.DoDragEnter);
            this.Resize += new System.EventHandler(this.Form1_ResizeEnd);
            ((System.ComponentModel.ISupportInitialize)(this.datagrid)).EndInit();
            this.panelSearchField.ResumeLayout(false);
            this.panelSearchField.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.slider)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picPaid)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.grpBoxPlating.ResumeLayout(false);
            this.grpBoxPolish.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private bool IsCompleted() =>
            !string.IsNullOrWhiteSpace(this.jobDateCompleted.Text);

        private bool IsPaid() =>
            !string.IsNullOrWhiteSpace(this.jobDatePaid.Text);

        private bool IsValid(string emailaddress)
        {
            try
            {
                MailAddress address = new MailAddress(emailaddress);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private async void DeleteJobClicked(object sender, EventArgs e)
        {
            MouseEventArgs me = (MouseEventArgs)e;
            if (me.Button == MouseButtons.Right)
            {
                if (MessageBox.Show("Are you sure you wish to delete this JOB?" + Environment.NewLine + "This cannot be undone", "Confirm Deletion", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation) == DialogResult.OK)
                {
                    if (MessageBox.Show("Are you REALLY REALLY REALLY sure you wish to delete this JOB?" + Environment.NewLine + "This cannot be undone", "Confirm Deletion", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation) == DialogResult.OK)
                    {
                        if (await DataAccess.DeleteJobAsync(System.Convert.ToInt32(this.jobID.Text))
                            /* PJC OLD
                            DataAccess.Update("DELETE FROM " + JobCard.DBTable + " WHERE jobID=" + this.jobID.Text)
                            */
                            )
                        {
                            await this.GetPreviousJobAsync();
                        }
                        this.jobCustomer.Focus();
                    }
                }
            }
        }

        private void JobCard_DropDown(CheckBoxComboBox c, string toSet)
        {
            string str = toSet;
            c.CheckBoxItems.Clear();
            c.SelectedItem = null;
            c.SelectedText = "";
            if (!string.IsNullOrEmpty(str))
            {
                string[] strArray = str.Split(new char[] { ',' });
                foreach (string str2 in strArray)
                {
                    foreach (CheckBoxComboBoxItem item in c.CheckBoxItems)
                    {
                        if (item.Text == str2.Trim())
                        {
                            item.Checked = true;
                        }
                    }
                }
            }
            c.Text = str;
        }

        private void jobCompleted_CheckedChanged(object sender, EventArgs e)
        {
        }

        private void JobIDChanged(object sender, EventArgs e)
        {
        }

        private static JobTypePopup popup = null;
        private void JobTypeClick(object sender, EventArgs e)
        {
            if (sender is TextBox)
            {
                TextBox box = (TextBox)sender;
                JobTypePopup.jobType = box;
                if (JobTypePopup.isWheelApp())
                {
                    string qtyName = box.Name.Replace("Type", "Qty");
                    JobTypePopup.jobQty = this.Controls.Find(qtyName, true)[0] as TextBox;
                    string unitPriceName = box.Name.Replace("Type", "UnitPrice");
                    JobTypePopup.jobUnitPrice = this.Controls.Find(unitPriceName, true)[0] as TextBox;
                    string priceName = box.Name.Replace("Type", "Price");
                    JobTypePopup.jobPrice = this.Controls.Find(priceName, true)[0] as TextBox;
                    string detailName = box.Name.Replace("Type", "Detail");
                    JobTypePopup.jobDetail = this.Controls.Find(detailName, true)[0] as TextBox;


                }
                if (JobCard.popup == null)
                {
                    JobCard.popup = new JobTypePopup();
                }
                if (box.Enabled)
                {

                    if (JobCard.popup.IsDisposed)
                    {
                        JobCard.popup = new JobTypePopup();

                    }

                    if (JobTypePopup.jobType == null)
                    {
                        JobTypePopup.jobType = this.jobType[0];
                        JobTypePopup.jobQty = this.jobQty[0];
                        JobTypePopup.jobUnitPrice = this.jobUnitPrice[0];
                        JobTypePopup.jobPrice = this.jobPrice[0];
                        JobTypePopup.jobDetail = this.jobDetail[0];
                    }
                    JobCard.popup.jobCard = this;

                    JobCard.popup.Show();

                }
            }
        }

        private void Load(int selectedRow = 0)
        {
            this.Loading = true;
             this.stopVideoCapture();
            
            UpdatePictureBox(this.pictureBox1, null);
            currentPictureIndex = 0;
            if ((this.datagrid.Rows != null) && (this.datagrid.Rows.Count != 0))
            {
                int num;
                for (num = 0; num < this.datagrid.RowCount; num++)
                {
                    bool flag = num == selectedRow;
                    this.datagrid.Rows[num].Selected = flag;
                }
                DataGridViewSelectedCellCollection selectedCells = this.datagrid.SelectedCells;
                List<CheckBox> list = new List<CheckBox>();
                bool flag2 = false;
                for (num = 0; num < selectedCells.Count; num++)
                {
                    DataGridViewCell cell = selectedCells[num];
                    int columnIndex = cell.ColumnIndex;
                    string name = this.datagrid.Columns[columnIndex].Name;
                    object obj2 = cell.Value;
                    string toSet = "";
                    bool flag3 = false;
                    if (((obj2 is float) || (obj2 is double)) || (obj2 is float))
                    {
                        toSet = ((float)obj2).ToString("F2");
                    }
                    if ((obj2 is int) || (obj2 is long))
                    {
                        toSet = ((int)obj2).ToString();
                    }
                    else if (obj2 is DateTime)
                    {
                        if (name == "jobDate")
                        {
                            this.jobDateValForPhoto = (DateTime)obj2;
                            this.UpdatePhotos();
                        }
                        toSet = ((DateTime)obj2).ToString("d/M/yy");
                    }
                    else if (obj2 is bool)
                    {
                        flag3 = (bool)obj2;
                        toSet = flag3.ToString();
                    }
                    else if (obj2 is string)
                    {
                        toSet = (string)obj2;
                    }
                    System.Type valueType = cell.ValueType;
                    if (this.fieldNameToControlMapping.ContainsKey(name))
                    {
                        this.originalValues[name] = toSet;
                        Control control = this.fieldNameToControlMapping[name];
                        bool flag4 = control is TextBox;
                        bool flag5 = control is Label;
                        bool flag6 = control is CheckBox;
                        bool flag7 = control is ComboBox;
                        bool flag8 = control is CheckBoxComboBox;
                        Color whiteSmoke = Color.WhiteSmoke;
                        if (control.BackColor == this.stripe)
                        {
                            whiteSmoke = control.BackColor;
                        }
                        if (flag8)
                        {
                            CheckBoxComboBox c = (CheckBoxComboBox)control;
                            c.BackColor = whiteSmoke;
                            this.JobCard_DropDown(c, toSet);
                        }
                        else if (flag4)
                        {
                            TextBox box2 = (TextBox)control;
                            box2.BackColor = whiteSmoke;
                            box2.Text = toSet;
                        }
                        else if (flag5)
                        {
                            ((Label)control).Text = toSet;
                        }
                        else if (flag6)
                        {
                            CheckBox item = (CheckBox)control;
                            item.BackColor = whiteSmoke;
                            item.Checked = flag3;
                            if (name != "jobCompleted" && name != "jobGoodReserved" && name != "jobQuotation") 
                            {
                                flag2 |= flag3;
                                item.Enabled = false;
                                list.Add(item);
                            }
                        }
                        else if (flag7)
                        {
                            ComboBox box4 = (ComboBox)control;
                            box4.BackColor = whiteSmoke;
                            box4.Text = toSet;
                        }
                    }
                }
                foreach (CheckBox box3 in list)
                {
                    box3.Visible = flag2;
                }
                this.LockAll(this.NeedLock());
                this.undoList.Clear();
                this.Loading = false;
                this.updateCreditCardSurcharge();
                this.RedrawArrayComponent();
                DisclaimerNoteAsync();
            }
            bool isFussy = DataAccess.isFussyCustomers(this.jobPhone.Text, this.jobEmail.Text);
            if (isFussy)
            {
                this.BackColor = Color.LightSalmon;
                this.jobFussyNotes.Visible = true;
            } else
            {
                this.BackColor = DefaultBackColor;
                this.jobFussyNotes.Visible = false;

                if (this.jobGoodReserved.Checked)
                {
                    this.BackColor = Color.LightGreen;
                } 
                if (this.jobQuotation.Checked)
                {
                    this.BackColor = Color.Purple;
                }
            }
        }

        private void LockAll(bool isLock)
        {
            this.isLocked = isLock;
            foreach (Control control in this.fieldNameToControlMapping.Values)
            {
                control.Enabled = !isLock;
            }
            this.btnLockUnlock.Text = this.isLocked ? "Unlock" : "Lock";
        }

        private void MetalToPolish(object sender, EventArgs e)
        {
            if (!this.isLocked)
            {
                Button button = null;
                if (sender is Button)
                {
                    button = (Button)sender;
                }
                if (button != null)
                {
                    this.jobDetail[this.platingIndex].Text = (this.jobDetail[this.platingIndex].Text + " " + button.Text).TrimStart(new char[] { ' ' });
                }
            }
        }

        private bool NeedCompulsory(bool fromSaveButton = false)
        {
            DateTime time;
            bool flag = string.IsNullOrWhiteSpace(this.CombinedDetailText(false));
            bool flag2 = string.IsNullOrWhiteSpace(this.jobCustomer.Text);
            bool flag3 = string.IsNullOrWhiteSpace(this.jobDate.Text);
            bool flag4 = string.IsNullOrWhiteSpace(this.jobPhone.Text);
            if (((flag4 && !flag3) && DateTime.TryParse(this.jobDate.Text, out time)) && (time.Year < 0x7df))
            {
                flag4 = false;
            }
            if ((flag || flag2) || flag3)
            {
                if (!fromSaveButton)
                {
                    //MessageBox.Show("Operation cannot be completed due to missing fields:" + Environment.NewLine + (flag3 ? ("Job Date" + Environment.NewLine) : "") + (flag2 ? ("Customer Name" + Environment.NewLine) : "") + (flag ? ("at least 1 Job Detail field" + Environment.NewLine) : ""), "Compulsory fields missing!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
                if (flag3)
                {
                    this.jobDate.BackColor = Color.FromArgb(0xff, 100, 100);
                }
                if (flag2)
                {
                    this.jobCustomer.BackColor = Color.FromArgb(0xff, 100, 100);
                }
                if (flag4)
                {
                }
                return !fromSaveButton;
            }
            return false;
        }

        private bool NeedLock()
        {
            bool flag = string.IsNullOrWhiteSpace(this.jobDateCompleted.Text) || string.IsNullOrWhiteSpace(this.jobDatePaid.Text);
            return !flag;
        }

        public void UpdateAllTotals()
        {

            double num2 = 0.0;
            double num3 = 0.0;
            double num4 = 0.0;
            for (int i = 0; i <= this.freightIndex; i++)
            {
                double num6 = 0.0;
                if (double.TryParse(this.jobPrice[i].Text, out num6))
                {
                    num2 += num6;
                }
            }
            if (incurCreditCardSurcharge())
            {
                // add credit card surcharge
                num2 = Math.Round((double)(num2 * 1.03), 2, MidpointRounding.AwayFromZero);
            }
            num3 = Math.Round((double)(num2 * 0.15), 2, MidpointRounding.AwayFromZero);
            num4 = Math.Round((double)(num2 + num3), 2, MidpointRounding.AwayFromZero);
            double result = 0.0;
            double num8 = 0.0;
            double num9 = 0.0;
            double.TryParse(this.jobPrice[this.subTotalIndex].Text, out result);
            double.TryParse(this.jobPrice[this.gstIndex].Text, out num8);
            double.TryParse(this.jobPrice[this.totalIndex].Text, out num9);
            if (!(result == num2))
            {
                result = num2;
                num9 = num4;
                num8 = num3;
                this.jobPrice[this.subTotalIndex].Text = num2.ToString("F2");
                this.jobPrice[this.gstIndex].Text = num3.ToString("F2");
                this.jobPrice[this.totalIndex].Text = num4.ToString("F2");
            }
            if (!(num9 == (result + num8)))
            {
                this.jobPrice[this.totalIndex].Text = (result + num8).ToString("F2");
                num9 = result + num8;
            }
        }
        private async System.Threading.Tasks.Task<bool> NeedSaveAsync(bool promptIfChanged = true, bool fromSaveButton = false)
        {
            bool flag = true;
            int num = 0;
            if (this.NeedCompulsory(fromSaveButton))
            {
                //return true;
            }
            this.updateSql = "UPDATE " + JobCard.DBTable + " SET ";
            this.updateSqlSetList = new List<KeyValuePair<string, dynamic>>();
            this.UpdateAllTotals();
            foreach (Control control in this.fieldNameToControlMapping.Values)
            {
                string name = control.Name;
                string stringValue = "";
                control.DoubleClick += new EventHandler(this.SingleSearch);
                if (name == "jobID")
                {
                    this.updateSqlSetList.Add(new KeyValuePair<string, dynamic>(name, System.Convert.ToInt32(control.Text)));
                }
                if (this.ControlValueChangedFromLoaded(control, false, out stringValue))
                {
                    if (name == "jobID" && stringValue == "000000")
                    {
                        continue;
                    }
                    flag = false;
                    System.Type type = this.types[name];
                    DateTime time = DateTime.Now;
                    bool isDateTime = (type == typeof(DateTime)) || (type == typeof(DateTime?));
                    bool isBool = (type == typeof(bool)) || (type == typeof(bool?));
                    bool isString = (type == typeof(string));
                    bool isInt = (type == typeof(int)) || (type == typeof(long)) || (type == typeof(long?)) || (type == typeof(int?));
                    bool isFloat = (type == typeof(float)) || (type == typeof(decimal)) || (type == typeof(Single)) || (type == typeof(Double))  || (type == typeof(Double?)) || (type == typeof(Single?)) || (type == typeof(decimal?)) || (type == typeof(float?));
                    string str3 = (isString) ? "'" : "";
                    
                    bool isNull = string.IsNullOrWhiteSpace(stringValue);
                    if (isNull)
                    {
                        stringValue = "null";
                        str3 = "";
                    }
                    else if (isDateTime)
                    {
                        
                        if (JobQueryForm.ParsedDateOK(stringValue, out time))
                        {
                            stringValue = "#" + time.ToString("MM/dd/yyyy") + "#";
                        } else
                        {
                            MessageBox.Show("Invalid date format MM/dd/yyyy in field " + name);
                            isNull = true;
                        }
                    }
                    string updateSql = this.updateSql;
                    if (isNull)
                    {
                        this.updateSqlSetList.Add(new KeyValuePair<string, dynamic>(name, null));
                    } else if (isDateTime)
                    {
                        this.updateSqlSetList.Add(new KeyValuePair<string, dynamic>(name,time));
                    } else if (isString)
                    {
                        this.updateSqlSetList.Add(new KeyValuePair<string, dynamic>(name, stringValue));
                    } else if (isBool)
                    {
                        this.updateSqlSetList.Add(new KeyValuePair<string, dynamic>(name, System.Convert.ToBoolean(stringValue)));
                    } else if (isInt)
                    {
                        this.updateSqlSetList.Add(new KeyValuePair<string, dynamic>(name, System.Convert.ToInt32(stringValue)));
                    } else if (isFloat)
                    {
                        try {
                            this.updateSqlSetList.Add(new KeyValuePair<string, dynamic>(name, System.Convert.ToDouble(stringValue)));
                        }
                        catch (Exception err)
                        {
                            System.Console.Out.WriteLine(err);
                        }
                    } else
                    {
                        MessageBox.Show("Unknown type for field " + name + " type was: " + type.ToString());
                    }
                    this.updateSql = updateSql + ((num > 0) ? "," : "") + name + "=" + str3 + DoubleQuote(stringValue) + str3;
                    num++;
                }
            }
            this.updateSql = this.updateSql + " WHERE jobID=" + this.jobID.Text;
            if (!flag && promptIfChanged)
            {
                flag = await DataAccess.UpdateMongoAsync(this.updateSqlSetList);
                    /* PJC OLD
                    DataAccess.Update(this.updateSql)
                    */
                    

            }
            return !flag;
        }


        private void PanelMouseDown(object sender, MouseEventArgs e)
        {
            this.panelDragging = true;
            this.panelMoved = false;
            this.panelDragStartPoint = e.Location;
        }

        private void PanelMouseUp(object sender, MouseEventArgs e)
        {
            if (this.panelDragging)
            {
                this.panelFinalLocation = this.panelSearchField.Location;
                this.panelSetLocation = true;
                this.panelMoved = false;
                this.panelDragging = false;
            }
        }

        private void panelSearchField_MouseMove(object sender, MouseEventArgs e)
        {
            if (this.panelDragging)
            {
                this.panelMoved = true;
                int num = e.Location.X - this.panelDragStartPoint.X;
                int num2 = e.Location.Y - this.panelDragStartPoint.Y;
                Point location = this.panelSearchField.Location;
                this.panelSearchField.Location = new Point(location.X + num, location.Y + num2);
            }
        }

        private void panelSearchField_Paint(object sender, PaintEventArgs e)
        {
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

            MouseEventArgs me = (MouseEventArgs)e;
            if (me.Button == MouseButtons.Left)
            {
                if ((currentPhotoPaths != null) && (currentPhotoPaths.Count > 0))
                {
                    PictureViewer viewer = new PictureViewer();
                    viewer.SetPictureList(this.pictureBox1);
                    viewer.ShowDialog();
                }
            }
            else
            {
                if (currentPictureIndex > -1 && currentPictureIndex < JobCard.currentPhotoPaths.Count)
                {
                    if (MessageBox.Show("Are you sure you wish to delete this picture?" + Environment.NewLine + "This cannot be undone", "Confirm Deletion", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation) == DialogResult.OK)
                    {
                        string path = JobCard.currentPhotoPaths[currentPictureIndex];
                        try
                        {
                            File.Delete(path);
                        }
                        catch (Exception err)
                        { }
                        JobCard.currentPhotoPaths.RemoveAt(currentPictureIndex);
                        if (currentPictureIndex >= JobCard.currentPhotoPaths.Count)
                        {
                            currentPictureIndex = JobCard.currentPhotoPaths.Count - 1;
                        }
                        if (currentPictureIndex >= 0)
                        {
                            UpdatePictureBox(this.pictureBox1, FromFile(currentPhotoPaths[currentPictureIndex]));
                        }
                        else
                        {
                            UpdatePictureBox(this.pictureBox1, null);
                        }
                    }
                }
            }
        }

        private void PolisherSelect(object sender, EventArgs e)
        {
            if (!this.isLocked)
            {
                Button button = null;
                if (sender is Button)
                {
                    button = (Button)sender;
                }
                if (button != null)
                {
                    this.jobDetail[this.polishIndex].Text = button.Name.Substring(3) + " to polish";
                }
            }
        }

        private void PrintForWork(bool isPrintAll = false)
        {
            lastFontName = null;
            lastFontSize = -1;
            lastFontStyle = FontStyle.Regular;
            CustomerCopy.autoPrint = false;
            CustomerCopy copy = new CustomerCopy
            {
                OnPrintPressed = new CustomerCopy.PrintHandler(this.PrintPressed)
            };
            copy.Height = (int)(copy.Width * Math.Sqrt(2.0));
            RichTextBox r = copy.richTextBox1;
            this.AddLine(r, "");
            if (this.pictureBox1.Image != null)
            {
                Clipboard.SetImage(resizeImage(this.pictureBox1.Image, new Size((int)(copy.Width * 0.8f), (int)(copy.Height * 0.25f))));
                r.Paste();
            }
            this.AddLine(r, this.jobID.Text, "Arial", 100, FontStyle.Bold, 0);
            this.AddLine(r, "Job Date: " + this.jobDate.Text.PadLeft(10) + "Order Number: ".PadLeft(40) + this.jobOrderNumber.Text, "Courier New", 0x10, FontStyle.Regular, 0);
            this.AddLine(r, "Business/Customer:", FontStyle.Bold);
            this.AddLine(r, "Business/Customer:" + this.jobBusinessName.Text + "/" + this.jobCustomer.Text.PadRight(0x23) + " Ph:" + this.jobPhone.Text, FontStyle.Regular);
            this.AddLine(r, "Date Required: " + this.jobDateRequired.Text.PadLeft(10) + (this.IsPaid() ? ("  Payment By: " + this.jobPaymentBy.Text) : ""));
            this.AddLine(r, "Details:", FontStyle.Bold);
            this.AddLine(r, this.CombinedDetailText(true), FontStyle.Regular);
            this.AddLine(r, "Notes");
            this.AddLine(r, this.jobNotes.Text);
            if (isPrintAll)
            {
                copy.PrintNow();
            }
            else
            {
                copy.ShowDialog();
            }
        }

        private async void PrintPressed()
        {
            this.jobCompleted.Checked = true;
            if (await this.NeedSaveAsync(false, true))
            {
                var ok = await DataAccess.UpdateMongoAsync(this.updateSqlSetList);
                //DataAccess.Update("UPDATE " + JobCard.DBTable + " SET jobCompleted=true WHERE jobID=" + this.jobID.Text);
            }
        }

     

        private void RedrawArrayComponent()
        {
            if ((this.jobPrice != null) && (this.jobPrice.Length >= 0x20))
            {
                int num;
                List<int> list = new List<int>();
                for (num = 0; num < 0x21; num++)
                {
                    bool flag = false;
                    if (!((((num >= 5) && (num < 0x12)) && (this.compress && string.IsNullOrWhiteSpace(this.jobDetail[num].Text))) && string.IsNullOrWhiteSpace(this.jobPrice[num].Text)))
                    {
                        list.Add(num);
                        flag = true;
                    }
                    if (num < 0x12)
                    {
                        this.jobDetail[num].Visible = flag;
                        this.jobType[num].Visible = flag;
                        this.jobQty[num].Visible = flag;
                        this.jobUnitPrice[num].Visible = flag;
                        this.jobPrice[num].Visible = flag;
                    }
                }
                int num2 = 10;
                int num3 = this.pictureBox1.Right - (this.btnCollapseToggle.Right + num2);

                int num4 = this.btnPrintForWork.Bottom - this.btnCollapseToggle.Top;
                int num5 = list[0];
                int num6 = 3;
                int height = ((int)(((float)num4) / ((float)list.Count))) - num6;
                float emSize = Math.Min((float)11f, (float)(0.6111111f * height));
                double num9 = 1.0;
                for (num = 0; num < list.Count; num++)
                {
                    int index = list[num];

                    this.jobDetail[index].Font = new Font("Arial", emSize);
                    this.jobDetail[index].Location = new Point(this.btnCollapseToggle.Right + num2, this.btnCollapseToggle.Top + (num * (height + num6)));

                    if (num >= 0x1d)
                    {
                        num9 = 0.0;
                    }
                    this.jobDetail[index].Size = new Size((int)(num3 * 0.4), height);
                    this.jobDetail[index].TabIndex = 40 + (num * 3);
                    if (num >= 0x12)
                    {
                        this.jobType[index].Visible = false;
                        this.jobType[index].Enabled = false;
                    }
                    if (num9 == 0.0)
                    {
                        this.jobDetail[index].Visible = false;
                        this.jobDetail[index].Enabled = false;
                        this.jobQty[index].Visible = false;
                        this.jobQty[index].Enabled = false;
                        this.jobUnitPrice[index].Visible = false;
                        this.jobUnitPrice[index].Enabled = false;
                    }
                    
                    this.jobPrice[index].Font = new Font("Arial", emSize);
                    this.jobPrice[index].Size = new Size((int)(num3 * 0.07), this.jobDetail[index].Height);
                    this.jobPrice[index].Location = new Point(this.pictureBox1.Right - this.jobPrice[index].Width, this.jobDetail[index].Location.Y);
                    this.jobPrice[index].TabIndex = 0x2a + (num * 3);
                    this.jobUnitPrice[index].Font = new Font("Arial", emSize);
                    this.jobUnitPrice[index].Size = new Size((int)(num3 * 0.06), this.jobDetail[index].Height);
                    this.jobUnitPrice[index].Location = new Point((this.jobPrice[index].Left - num2) - this.jobUnitPrice[index].Width, this.jobDetail[index].Location.Y);
                    this.jobQty[index].Font = new Font("Arial", emSize);
                    this.jobQty[index].Size = new Size((int)(num3 * 0.04), this.jobDetail[index].Height);
                    this.jobQty[index].Location = new Point((this.jobUnitPrice[index].Left - num2) - this.jobQty[index].Width, this.jobDetail[index].Location.Y);
                    this.jobType[index].Font = new Font("Arial", emSize);
                    int width = ((this.jobQty[index].Left - num2) - this.jobDetail[index].Right) - num2;
                    this.jobType[index].Location = new Point(this.jobDetail[index].Right + num2, this.jobDetail[index].Location.Y);
                    this.jobType[index].Size = new Size(width, this.jobDetail[index].Height);
                    this.checkBox[index].Font = new Font("Arial", emSize);
                    this.checkBox[index].Size = new Size(this.jobType[index].Width, this.jobDetail[index].Height);
                    Point point = new Point(this.jobType[index].Location.X, this.jobDetail[index].Location.Y);
                    this.checkBox[index].Location = point;
                    this.checkBox[index].TabIndex = 0x29 + (num * 3);
                    this.label[index].Size = new Size(90, this.jobDetail[index].Height);
                    if (this.label[index].Name == "TOTAL")
                    {
                        this.label[index].Font = new Font("Arial", emSize, FontStyle.Bold, GraphicsUnit.Point, 0);
                    }
                    else
                    {
                        this.label[index].Font = new Font("Arial", emSize);
                    }
                    Point point2 = new Point((this.jobPrice[index].Location.X - 5) - this.label[index].Width, this.jobDetail[index].Location.Y);
                    this.label[index].Location = point2;
                    if (index == this.freightIndex)
                    {
                        this.btnRDAddressSurcharge.Location = new Point(point2.X - 40, point2.Y + 3);
                    }
                    if (index == this.subTotalIndex)
                    {
                        point2.Offset(-140, 0);
                        this.label[index].Location = point2;
                        this.label[index].Size = new Size(this.label[index].Size.Width + 140, this.label[index].Size.Height);

                    }

                    Color whiteSmoke = Color.WhiteSmoke;
                    if ((num < this.freightIndex) && ((num % 2) == 0))
                    {
                        whiteSmoke = this.stripe;
                    }
                    this.jobDetail[index].BackColor = whiteSmoke;
                    this.jobType[index].BackColor = whiteSmoke;
                    this.jobQty[index].BackColor = whiteSmoke;
                    this.jobUnitPrice[index].BackColor = whiteSmoke;
                    this.jobPrice[index].BackColor = whiteSmoke;
                    this.jobDetail[index].BackColor = whiteSmoke;
                    this.checkBox[index].BackColor = whiteSmoke;
                    this.label[index].BackColor = whiteSmoke;
                }
            }
        }

        public static Image resizeImage(Image imgToResize, Size size)
        {
            int width = imgToResize.Width;
            int height = imgToResize.Height;
            float num3 = 0f;
            float num4 = 0f;
            float num5 = 0f;
            num4 = ((float)size.Width) / ((float)width);
            num5 = ((float)size.Height) / ((float)height);
            if (num5 < num4)
            {
                num3 = num5;
            }
            else
            {
                num3 = num4;
            }
            int num6 = (int)(width * num3);
            int num7 = (int)(height * num3);
            if (b != null)
            {
                b.Dispose();
            }
            b = new Bitmap(num6, num7);
            Graphics graphics = Graphics.FromImage(b);
            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            graphics.DrawImage(imgToResize, 0, 0, num6, num7);
            graphics.Dispose();
            return b;
        }

        private void SaveWebCamPhoto(List<System.Drawing.Image> images = null)
        {
            if (images == null)
            {
             images = Job_Card.Form1.selectedImages;
            }
            if (images != null && images.Count > 0)
            {
                string path = "";
                string str2 = ".jpg";
                ImageCodecInfo myImageCodecInfo;
                Encoder myEncoder;
                EncoderParameter myEncoderParameter;
                EncoderParameters myEncoderParameters;
                myImageCodecInfo = GetEncoderInfo("image/jpeg");
                myEncoder = Encoder.Quality;
                myEncoderParameters = new EncoderParameters(1);
                myEncoderParameter = new EncoderParameter(myEncoder, 100);
                myEncoderParameters.Param[0] = myEncoderParameter;


                DateTime now = DateTime.Now;
                if (!JobQueryForm.ParsedDateOK(this.jobDate.Text, out now))
                {
                    now = DateTime.Now;
                }

                this.jobPhotos = this.GetJobPictureFiles(now.Year, now.Month, int.Parse(this.jobID.Text), out path, false);
                for (int i = 0; i < images.Count; i++)
                {
                    int num = this.jobPhotos.Count + i + 1;
                    string str5 = (num == 0) ? "" : (" " + num.ToString("D3"));
                    string str6 = this.CombinedDetailText(false);
                    if (str6.Length > 60)
                    {
                        str6 = str6.Substring(0, 60);
                    }
                    string businessName = "";
                    if (this.jobBusinessName.Text.Length > 0)
                    {
                        businessName = this.jobBusinessName.Text + "-";
                    }
                    string str7 = (this.jobID.Text + " " + businessName + this.jobCustomer.Text + " " + (string.IsNullOrWhiteSpace(this.jobPhone.Text) ? "" : (this.jobPhone.Text + " ")) + str6 + str5 + str2).Replace('<', '-').Replace('>', '-').Replace(':', '-').Replace('"', '-').Replace('/', '-').Replace('\\', '-').Replace('|', '-').Replace('?', '-').Replace('*', '-');
                    string destFileName = path + @"\" + str7;
                    images[i].Save(destFileName, ImageFormat.Jpeg);
                }
                currentPictureIndex = 0;
                this.jobPhotos = this.GetJobPictureFiles(now.Year, now.Month, int.Parse(this.jobID.Text), out path, false);
                currentPhotoPaths = this.jobPhotos;

                this.UpdatePhotos();
                UpdatePictureBox(this.pictureBox1, images[0]);
            }
            images.Clear();
        }

        private static ImageCodecInfo GetEncoderInfo(string mimeType)
        {
            int j;
            ImageCodecInfo[] encoders;
            encoders = ImageCodecInfo.GetImageEncoders();
            for (j = 0; j < encoders.Length; ++j)
            {
                if (encoders[j].MimeType == mimeType)
                    return encoders[j];
            }
            return null;
        }

        private void SaveUniquePhoto(string path, Image image, List<string> jobPhotos, string sourcePath)
        {
            string str = DataAccess.ImageToBase64(image);
            bool flag = false;
            int num = 1;
            string str2 = ".jpg";
            if (image == JobCard.MovieImage)
            {
                str2 = ".mov";
                num += jobPhotos.Count;
            }
            else
            {
                ImageFormat jpeg = ImageFormat.Jpeg;
                foreach (string str3 in jobPhotos)
                {
                    try
                    {
                        Image image2 = FromFile(str3);
                        if (DataAccess.ImageToBase64(image2) == str)
                        {
                            flag = true;
                            str2 = str3.Substring(str3.Length - 4);
                            jpeg = image2.RawFormat;
                        }
                    }
                    catch (Exception)
                    {
                    }
                    num++;
                }
            }
            if (!flag)
            {
                string str5 = (num == 0) ? "" : (" " + num.ToString("D3"));
                string str6 = this.CombinedDetailText(false);
                if (str6.Length > 60)
                {
                    str6 = str6.Substring(0, 60);
                }
                string businessName = "";
                if (this.jobBusinessName.Text.Length > 0)
                {
                    businessName = this.jobBusinessName.Text + "-";
                }
                string str7 = (this.jobID.Text + " " + businessName + this.jobCustomer.Text + " " + (string.IsNullOrWhiteSpace(this.jobPhone.Text) ? "" : (this.jobPhone.Text + " ")) + str6 + str5 + str2).Replace('<', '-').Replace('>', '-').Replace(':', '-').Replace('"', '-').Replace('/', '-').Replace('\\', '-').Replace('|', '-').Replace('?', '-').Replace('*', '-');
                string destFileName = path + @"\" + str7;
                System.IO.File.Copy(sourcePath, destFileName);
            }
        }

        private async System.Threading.Tasks.Task SearchAsync()
        {
            if (this.types.ContainsKey(this.searchFieldName))
            {
                BsonDocument bson = new BsonDocument();
                bool bsonValid = false;
                bool useRegEx = false;
                FilterDefinition<JobCardDoc> filter = null;


                string str2;
                string sql = "";
                System.Type type = this.types[this.searchFieldName];
                if (type == typeof(DateTime))
                {
                    DateTime time;
                    if (this.CheckDate(this.txtSearchField.Text, out time))
                    {
                        bson[searchFieldName] = time;
                        bsonValid = true;
                        str2 = sql;
                        filter = Builders<JobCardDoc>.Filter.Eq(this.searchFieldName, time);
                        sql = str2 + this.searchFieldName + "=#" + time.ToString("MM/dd/yyyy") + "#";
                    }
                }
                else if (type == typeof(float))
                {
                    float result = 0f;
                    if (float.TryParse(this.txtSearchField.Text, out result))
                    {
                        bson[searchFieldName] = result;
                        bsonValid = true;
                        filter = Builders<JobCardDoc>.Filter.Eq(this.searchFieldName, result);
                        sql = sql + this.searchFieldName + "=" + this.txtSearchField.Text;
                    }
                    else
                    {
                        MessageBox.Show("This field should only contain numbers");
                    }
                }
                else if (type == typeof(bool))
                {
                    bool flag = false;
                    if (bool.TryParse(this.txtSearchField.Text, out flag))
                    {
                        bson[searchFieldName] = flag;
                        filter = Builders<JobCardDoc>.Filter.Eq(this.searchFieldName, flag);
                        bsonValid = true;
                        sql = sql + this.searchFieldName + "=" + this.txtSearchField.Text;
                    }
                    else
                    {
                        MessageBox.Show("You must only type true or false for this checkbox");
                    }
                }
                else if (type == typeof(string))
                {
                    str2 = sql;
                    filter = Builders<JobCardDoc>.Filter.Regex(this.searchFieldName, new BsonRegularExpression(this.txtSearchField.Text, "i"));
                    bsonValid = true;
                    useRegEx = true;
                    sql = str2 + this.searchFieldName + " LIKE '%" + this.txtSearchField.Text + "%'";
                }
                else if (type == typeof(int))
                {
                    int num2 = 0;
                    if (int.TryParse(this.txtSearchField.Text, out num2))
                    {
                        bson[searchFieldName] = num2;
                        bsonValid = true;
                        filter = Builders<JobCardDoc>.Filter.Eq(this.searchFieldName, num2);
                        sql = sql + this.searchFieldName + "=" + this.txtSearchField.Text;
                    }
                    else
                    {
                        MessageBox.Show("You must only have digits in this field");
                    }
                } else
                {
                    MessageBox.Show("Unknown type " + type.ToString());
                }
                /* PJC OLD
                if (sql != "")
                {
                    sql = "Select * from " + JobCard.DBTable + " WHERE " + sql + " order by jobDate desc";
                    this.SearchSQL(sql);
                }
                */
                if (bsonValid)
                {
                    var list = await DataAccess.findJobByFilterAsync(this.datagrid, filter, "jobDate", true, 0, 50);
                    if (list != null && list.Count > 0)
                    {
                        this.Load(0);

                        int searchRows = list.Count;
                         if (searchRows == 1)
                        {
                            this.panelSearchField.Visible = false;
                        }
                        else
                        {
                            this.slider.Value = 0;
                            this.slider.Maximum = searchRows - 1;
                            this.slider.Visible = true;
                            this.lblResults.Text = string.Concat(new object[] { "Showing match ", this.slider.Value + 1, " of ", searchRows });
                        }



                    } else
                    {
                        MessageBox.Show("No results found");
                    }
                }
            }
        }

        private string SearchFile(string dirToSearch, string fileToFind)
        {
            foreach (string str in Directory.GetDirectories(dirToSearch))
            {
                if ((!str.Contains("Windows") && !str.Contains("Microsoft")) && !str.Contains("RECYCLE"))
                {
                    try
                    {
                        
                        foreach (string str2 in Directory.GetFiles(str, fileToFind))
                        {
                            if (str2.EndsWith(fileToFind, true, CultureInfo.InvariantCulture))
                            {
                                return str2;
                            }
                        }
                        string str3 = this.SearchFile(str, fileToFind);
                        if (str3 != null)
                        {
                            return str3;
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
            }
            return null;
        }

        private void SearchSQL(string sql)
        {
            if (sql != "")
            {
                DataAccess.ReadRecords(this.datagrid, sql);
                this.Load(0);
                searchRows = this.datagrid.RowCount;
                if (searchRows == 0)
                {
                    MessageBox.Show("Sorry no matches");
                }
                else if (searchRows == 1)
                {
                    this.panelSearchField.Visible = false;
                }
                else
                {
                    this.slider.Value = 0;
                    this.slider.Maximum = searchRows - 1;
                    this.slider.Visible = true;
                    this.lblResults.Text = string.Concat(new object[] { "Showing match ", this.slider.Value + 1, " of ", searchRows });
                }
            }
        }

    
        private bool SendMail(SettingsSettingsDoc settings, string mailTo, string csSubject, string csBody, string attachment)
        {
           
            //MessageBox.Show("PJC1 "+settings.emailAddress);
            MailAddress from = new MailAddress(settings.emailAddress, settings.emailName);

            MailAddressCollection to = new MailAddressCollection();
            to.Add(new MailAddress(mailTo));
            MailAddressCollection replyToList = new MailAddressCollection();
            replyToList.Add(from);
            MailMessage message = new MailMessage(settings.emailAddress, mailTo, csSubject, csBody)
            {
                IsBodyHtml = false,
                DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure
            };

            if (!string.IsNullOrWhiteSpace(attachment))
            {
                try
                {
                    //MessageBox.Show("PJC2");
                    foreach (Process clsProcess in Process.GetProcessesByName("WINWORD"))
                    {
                        if (clsProcess.ProcessName.StartsWith("WINWORD"))
                        {
                            //clsProcess.Kill();
                        }
                    }
                    //MessageBox.Show("PJC3");
                    message.Attachments.Add(new Attachment(attachment));
                    //MessageBox.Show("PJC4");
                    foreach (Process clsProcess in Process.GetProcessesByName("WINWORD"))
                    {
                        if (clsProcess.ProcessName.StartsWith("WINWORD"))
                        {
                           // clsProcess.Kill();
                        }
                    }
                }
                catch (Exception exc)
                {
                    System.Console.Out.WriteLine("Failed " + exc.InnerException.Message);
                }
                
            }
            //MessageBox.Show("PJC5");
            System.Net.Mail.SmtpClient client = new System.Net.Mail.SmtpClient(settings.emailDomain, settings.emailPort)
            {
                Credentials = new NetworkCredential(settings.emailAddress, settings.emailPassword)
                //Credentials = CredentialCache.DefaultNetworkCredentials
            };
            client.EnableSsl = true;
            try
            {
                //MessageBox.Show("PJC6");
                client.Send(message);
                //MessageBox.Show("PJC7");
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "Not able to send mail", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                message.Dispose();
                return false;
            }
            message.Dispose();
            return true;
        }

        private DialogResult ShowError(string errMsg, string title, bool yesNoCancel = false) =>
            MessageBox.Show(errMsg, title, yesNoCancel ? MessageBoxButtons.YesNoCancel : MessageBoxButtons.OK, MessageBoxIcon.Hand);

        public static Image Base64StringToImage(string base64String)
        {
            byte[] imageBytes = Convert.FromBase64String(base64String);
            using (MemoryStream ms = new MemoryStream(imageBytes))
            {
                return Image.FromStream(ms);
            }
        }
        private bool ShowPrintForm(bool customerCopy = true, bool isPrintAll = false, string printToPDF = null)
        {
            lastFontName = null;
            lastFontSize = -1;
            lastFontStyle = FontStyle.Regular;
            CustomerCopy.autoPrint = true;
            CustomerCopy copy = new CustomerCopy
            {
                OnPrintPressed = new CustomerCopy.PrintHandler(this.PrintPressed)
            };
            copy.Height = (int)(copy.Width * Math.Sqrt(2.0));
            RichTextBox r = copy.richTextBox1;
            this.AddLine(r, "");
            Resources.logo.MakeTransparent();
           
            if (customerCopy)
            {
                if (JobTypePopup.isCanada())
                {
                    Image wheelTec = JobCard.Base64StringToImage("iVBORw0KGgoAAAANSUhEUgAAAZAAAABfCAMAAADxqGErAAACClBMVEVHcExtXFVaW11bXF5bW11aW11kXlxmXVhTTUtbXF5ZWVtcW1xcW1xbXF5bW10EBwdbXF1aW11ZWVsBBQZiXlxZWlxgXV1aWlxbXF4CBgdZWlxYWFpbXF5bXF4DBgZbXF5bW11bXF5bW11YWVtYWFpbXF5bXF5bXF4DBgZbXF5ZWlxbXF5bXF4DBgZbW101IRYBBQZYWFpaWlxbXF5ZWVtYWFoEBwdaWlxYWFoEBwgBBgdYWFpaW11ZWlxaWlwAAwUbEw5ZWlxYWFoABAUBBAVcXF5aW11YWFpjYF8ODQtcXF1aW10BBQZ9bGFYWFoCBgZjYF8EBwbmdDVbXF4HCQhYWFp / bGDzcStYWFpDJhZbW11dXF30cSrzcStyZmApGxKmb09YWFoRDgsBBQV4aWBYWFprZGBrY1 + fdVvxcS3wcSzzcSv0cSvzcSu6cEd8a2Hwci3zcSuIcWJwY13xcSzzcSvzcSv0cSv0cSvzcSv0cSsDBweMcWBuZWB3Z10CBQcBBgaQaFGJbVwDBwcCBgekbk + scE + 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");
                    Clipboard.SetImage(wheelTec);
                }
                else
                {
                    Clipboard.SetImage(Resources.logoHalfSize);
                }
            }
            else if (this.pictureBox1.Image != null)
            {
                Clipboard.SetImage(resizeImage(this.pictureBox1.Image, new Size((int)(copy.Width * 0.8f), (int)(copy.Height * 0.25f))));
            }
            r.Paste();
            this.AddLine(r, this.jobID.Text, "Arial", 24, FontStyle.Bold, 0);
            this.AddLine(r, this.IsCompleted() ? "Tax Invoice  GST 83-712-147" : "Quotation/Job Card", null, 12, FontStyle.Regular, 0);
            this.AddLine(r, "");
            this.AddLine(r, "Job Date: " + this.jobDate.Text.PadLeft(10) + "Order Number: ".PadLeft(40) + this.jobOrderNumber.Text, "Courier New", 12, FontStyle.Regular, 0);
            this.AddLine(r, "".PadRight(0x4a, '-'));
            string businessCustomerTitle = "Customer:";
            string businessName = "";
            if (this.jobBusinessName.Text.Length > 0)
            {
                businessCustomerTitle = "Business/" + businessCustomerTitle;
                businessName = this.jobBusinessName.Text + " / ";
            }
            this.AddLine(r, businessCustomerTitle, FontStyle.Bold);

            this.AddLine(r, businessName + this.jobCustomer.Text.PadRight(0x2d) + " Ph:" + this.jobPhone.Text, FontStyle.Regular);
            if (!string.IsNullOrWhiteSpace(this.jobEmail.Text))
            {
                this.AddLine(r, "Email: " + this.jobEmail.Text);
            }
            if (!string.IsNullOrWhiteSpace(this.jobAddress.Text))
            {
                this.AddLine(r, this.jobAddress.Text ?? "");
            }
            this.AddLine(r, "");
            this.AddLine(r, "Delivery Instructions:" + ("Received From: " + this.jobReceivedFrom.Text).PadLeft(0x2f));
            this.AddLine(r, "");
            this.AddLine(r, this.jobDelivery.Text, null, -1, FontStyle.Regular, 90);
            this.AddLine(r, "");
            this.AddLine(r, "Date Required: " + this.jobDateRequired.Text.PadLeft(10) + (this.IsPaid() ? ("  Payment By: " + this.jobPaymentBy.Text) : ""));
            this.AddLine(r, "".PadRight(0x4a, '-'));
            this.AddLine(r, "Details:", FontStyle.Bold);
            this.AddLine(r, this.CombinedDetailText(true), FontStyle.Regular);
            if (this.IsPaid())
            {
                temporarilyDisableNewLineAtEnd = true;
                this.AddLine(r, "Payment received: " + this.jobDatePaid.Text.PadLeft(10) + "".PadRight(0x21));
                r.AppendText(" ");
                Clipboard.SetImage(Resources.paidSmall);
                r.Paste();
                r.AppendText(Environment.NewLine);
                if (customerCopy && (this.jobCompleted.Checked || !string.IsNullOrWhiteSpace(printToPDF)))
                {
                    //this.AddLine(r, "******************************", "Courier New", 15, FontStyle.Regular, 0);
                    //this.AddLine(r, "*     DUPLICATE RECEIPT      *");
                    //this.AddLine(r, "******************************");
                }
            }
            this.AddLine(r, "".PadRight(0x4a, '-'));
            this.AddLine(r, "Notes");
            this.AddLine(r, this.jobNotes.Text, "Arial", 9, FontStyle.Regular);
            this.AddLine(r, "".PadRight(0x8a, '-'));
            this.AddLine(r, "DISCLAIMER", "Arial", 10, FontStyle.Bold, 0);
            this.AddLine(r, Disclaimer, null, 7, FontStyle.Regular, 0);
            this.AddLine(r, "".PadRight(0x4a, '-'), "Courier New", 13, FontStyle.Regular, 0);
            if (customerCopy)
            {
                this.AddLine(r, "CUSTOMER COPY - " + (this.IsCompleted() ? " ** TAX INVOICE **" : "PRICING ABOVE AN ESTIMATE ONLY"), FontStyle.Bold);
                if (JobTypePopup.isCanada())
                {
                    this.AddLine(r, "Our phone is: (709) 330-9832, use job#" + this.jobID.Text + " as reference");
                }
                else
                {
                    this.AddLine(r, "Our bank details are: 031557013897600 , use job#" + this.jobID.Text + " as reference");
                }
            }
            else
            {
                this.AddLine(r, JobCard.getBusinessName()+" copy", FontStyle.Bold);
            }
            if (string.IsNullOrWhiteSpace(printToPDF))
            {
                if (isPrintAll)
                {
                    copy.PrintNow();
                }
                else
                {
                    copy.ShowDialog();
                }
            }
            else
            {
                try
                {
                    r.SaveFile(printToPDF + ".rtf");
                    WordToPDF(printToPDF + ".rtf");
                }
                catch (Exception)
                {
                    return false;
                }
            }
            return true;
        }

        private async void SingleSearch(object sender, EventArgs e)
        {
            if (!(await this.NeedSaveAsync(true, false)))
            {
                if (this.panelSetLocation)
                {
                    this.panelSearchField.Location = this.panelFinalLocation;
                }
                else
                {
                    this.panelSearchField.Location = new Point((int)(((float)base.Width) / 3f), (int)(((float)base.Height) / 2.5f));
                }
                this.txtSearchField.Text = "";
                this.searchFieldName = ((Control)sender).Name;
                this.slider.Visible = false;
                this.slider.Value = 0;
                this.slider.Maximum = 0;
                this.lblResults.Text = "";
                this.lblSearchOnField.Text = "Search on " + this.searchFieldName;

                this.btnSearchField.Visible = true;
                this.txtSearchField.Enabled = true;
                this.panelSearchField.Visible = true;
                this.txtSearchField.Focus();
            }
        }

        private void slider_Scroll(object sender, EventArgs e)
        {
            this.Load(this.slider.Value);
            this.lblResults.Text = string.Concat(new object[] { "Showing match ", this.slider.Value + 1, " of ", searchRows });
        }

        private async void tb_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                await this.SearchAsync();
            }
        }

        private void TogglePaidStamp(object sender, EventArgs e)
        {
            if (sender is TextBox)
            {
                this.picPaid.Visible = ((TextBox)sender).Text.Length >= 8;
            }
        }

        private void updateCreditCardSurcharge()
        {
            if (this.incurCreditCardSurcharge())
            {
                this.label[this.subTotalIndex].Text = "Sub Total (+3% card surch)";
            }
            else
            {
                this.label[this.subTotalIndex].Text = "Sub Total";
            }
            this.UpdateAllTotals();
        }
        private void CheckForCreditCardSurcharge(object sender, EventArgs e)
        {
            updateCreditCardSurcharge();
        }

        private void txtSearchField_TextChanged(object sender, EventArgs e)
        {
        }

        private void UpdatePhotos()
        {
            int year = this.jobDateValForPhoto.Year;
            string outPath = "";
            currentPhotoPaths = this.GetJobPictureFiles(this.jobDateValForPhoto.Year, this.jobDateValForPhoto.Month, int.Parse(this.jobID.Text), out outPath, false);
            currentPictureIndex = 0;
            if (currentPhotoPaths.Count > 0)
            {
                UpdatePictureBox(this.pictureBox1, FromFile(currentPhotoPaths[currentPictureIndex]));
            }
        }

        public static void UpdatePictureBox(PictureBox pbox, Image image)
        {
            if ((pbox != null) && (pbox.Image != null))
            {
                if (pbox.Image != JobCard.MovieImage)
                {
                    pbox.Image.Dispose();
                }
                pbox.Image = null;
            }
            if ((pbox != null) && (image != pbox.Image))
            {
                pbox.Image = image;
            }
        }

        public bool ValidEmailAddress(string emailAddress, out string errorMessage)
        {
            if (emailAddress.Length == 0)
            {
                errorMessage = "e-mail address is required.";
                return false;
            }
            if ((emailAddress.IndexOf("@") > -1) && (emailAddress.IndexOf(".", emailAddress.IndexOf("@")) > emailAddress.IndexOf("@")))
            {
                errorMessage = "";
                return true;
            }
            errorMessage = "e-mail address must be valid e-mail address format.\nFor example 'someone@example.com' ";
            return false;
        }

        public static void WordToPDF(string docFileName)
        {
            Microsoft.Office.Interop.Word.Application application = null;
            Document document = null;
            object confirmConversions = Missing.Value;
            try
            {
                application = (Microsoft.Office.Interop.Word.Application)Activator.CreateInstance(System.Type.GetTypeFromCLSID(new Guid("000209FF-0000-0000-C000-000000000046")));
                FileInfo info = new FileInfo(docFileName);
                application.Visible = false;
                application.ScreenUpdating = false;
                object fullName = info.FullName;
                try
                {
                    document = application.Documents.Open(ref fullName, ref confirmConversions, ref confirmConversions, ref confirmConversions, ref confirmConversions, ref confirmConversions, ref confirmConversions, ref confirmConversions, ref confirmConversions, ref confirmConversions, ref confirmConversions, ref confirmConversions, ref confirmConversions, ref confirmConversions, ref confirmConversions, ref confirmConversions);
                    document.Activate();
                }
                catch (Exception)
                {
                }
                object fileName = info.FullName.Replace(".rtf", ".pdf");
                object wdFormatPDF = WdSaveFormat.wdFormatPDF;
                try
                {
                    document.SaveAs(ref fileName, ref wdFormatPDF, ref confirmConversions, ref confirmConversions, ref confirmConversions, ref confirmConversions, ref confirmConversions, ref confirmConversions, ref confirmConversions, ref confirmConversions, ref confirmConversions, ref confirmConversions, ref confirmConversions, ref confirmConversions, ref confirmConversions, ref confirmConversions);
                }
                catch (Exception)
                {
                    fileName = info.FullName.Replace(".rtf", ".doc");
                    wdFormatPDF = WdSaveFormat.wdFormatDocument;
                    try
                    {
                        document.SaveAs(ref fileName, ref wdFormatPDF, ref confirmConversions, ref confirmConversions, ref confirmConversions, ref confirmConversions, ref confirmConversions, ref confirmConversions, ref confirmConversions, ref confirmConversions, ref confirmConversions, ref confirmConversions, ref confirmConversions, ref confirmConversions, ref confirmConversions, ref confirmConversions);
                    }
                    catch (Exception)
                    {
                    }
                }
            }
            catch (Exception)
            {
            }
            finally
            {
                if (document != null)
                {
                    object wdDoNotSaveChanges = WdSaveOptions.wdDoNotSaveChanges;
                    document.Close(ref wdDoNotSaveChanges, ref confirmConversions, ref confirmConversions);
                    document = null;
                }
                if (application != null)
                {
                    application.Quit(ref confirmConversions, ref confirmConversions, ref confirmConversions);
                    application = null;
                }
            }
        }

        private string AllDetails
        {
            get
            {
                string str = "";
                for (int i = 0; i < this.freightIndex; i++)
                {
                    if (this.types.ContainsKey(this.jobDetail[i].Name))
                    {
                        str = str + ((str != "") ? " & " : "") + this.jobDetail[i].Name;
                    }
                }
                return str;
            }
        }


        private bool interceptTabKey = true;
        protected override bool ProcessTabKey(bool forward)
        {
            // We can intercept/process the [Keys.Tab] via this method.
            if (interceptTabKey)
            {
                var activeControl = FindFocusedControl(this);

                if (forward)            // [Keys.Shift] was not used
                {
                    this.GetTextBoxes(activeControl, 1);
                }
                else                    // [Keys.Shift] was used
                {
                    this.GetTextBoxes(activeControl, -1);
                }

                // [return true;]  -- To indicate that a control is selected.
                // [return false;] -- Also, it happens that [return false;] causes the TabKey 
                //                    to be processed by the [OnKeyDown()] and related methods.
                return true;
                //return false;
            }

            return base.ProcessTabKey(forward); // One would normally do this, but we may
                                                // have wanted to intercept [Keys.Tab] above
        }

        public void GetTextBoxes(Control activeControl, int dir)
        {
            string t = "";
            SortedList<int, TextBox> s = new SortedList<int, TextBox>();
            foreach (var control in Controls)
            {
                if (control is TextBox)
                {
                    //Box the control into a textbox. Not really needed, but do it anyway
                    var textbox = (TextBox)control;
                    var key = textbox.Top * 2000 + textbox.Left;
                    if (key > 0 && textbox.Visible && textbox.Enabled)
                    {
                        s.Add(key, textbox);
                        //textbox.Text = textbox.Name;
                    }
                }
            }
            List<TextBox> l = new List<TextBox>();
            t = "";
            int index = 0;
            int foundIndex = -1;
            foreach (var textbox in s)
            {
                if (activeControl != null && activeControl is TextBox)
                {
                    if (textbox.Value == (TextBox)activeControl)
                    {
                        foundIndex = index;
                        
                    }
                }
                l.Add(textbox.Value);
                index++;
                t += ",  " + textbox.Value.Name;
            }
           
            if (foundIndex > -1)
            {
                foundIndex += dir;
                if (foundIndex >= 0 && foundIndex < l.Count)
                {
                    l[foundIndex].Focus();
                }
            }
            //MessageBox.Show("Found " + foundIndex + " t-=" + t);          
        }

        public static Control FindFocusedControl(Control control)
        {
            var container = control as IContainerControl;
            while (container != null)
            {
                control = container.ActiveControl;
                container = control as IContainerControl;
            }
            return control;
        }

        
        private void btnFussy_Click(object sender, EventArgs e)
        {
            string phone = this.jobPhone.Text;
            string email = this.jobEmail.Text;
            if (!string.IsNullOrEmpty(phone))
            {
                DialogResult dialogResult = MessageBox.Show("Sure", "Put EXCLAMATION", MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.Yes)
                {
                    DataAccess.InsertFussyCustomer(phone, email);
                    this.BackColor = Color.LightSalmon;
                    this.jobFussyNotes.Visible = true;
                }
                
            } else
            {
                MessageBox.Show("Phone must have at least a 9 digit number");
            }
        }

        private void btnRDAddressSurcharge_Click(object sender, EventArgs e)
        {
            double result = 0.0;
            double.TryParse(this.jobPrice[this.freightIndex].Text, out result);
            this.jobPrice[freightIndex].Text = string.Format("{0:N2}", (7.0 + result));
            this.UpdateAllTotals();
        }

        private async void getLatestTimer_Tick(object sender, EventArgs e)
        {
            if (this.jobID.Text == "" || this.jobID.Text == "000000")
            {
                this.jobID.Text = "Loading";
                await this.GetLatestJobAsync();
            }
            
        }

        private void OnNotesEnter(object sender, EventArgs e)
        {
            this.jobNotes.Size = new Size(1000, 500);
            this.jobNotes.BringToFront();
        }

        private void OnNotesLeave(object sender, EventArgs e)
        {
            this.jobNotes.Size = new Size(352, 73);
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void ShowFussyNotes(object sender, EventArgs e)
        {
            this.jobFussyNotes.Size = new Size(1000, 500);
            this.jobFussyNotes.BringToFront();
        }

        private void OnFussyNotesLeave(object sender, EventArgs e)
        {
            this.jobFussyNotes.Size = new Size(50, 55);
        }

     
        private void JobCard_Click(object sender, EventArgs e)
        {
            this.OnFussyNotesLeave(sender, e);
            this.OnNotesLeave(sender, e);
        }

        private void jobGoodReserved_CheckedChanged(object sender, EventArgs e)
        {
            if (this.jobGoodReserved.Checked)
            {
                this.BackColor = Color.LightGreen;
            }
            else
            {
                this.BackColor = DefaultBackColor;
            }
        }

        private void jobQuotation_CheckedChanged(object sender, EventArgs e)
        {
            if (this.jobQuotation.Checked)
            {
                this.BackColor = Color.Purple;
            }
            else
            {
                this.BackColor = DefaultBackColor;
            }
        }


        private void cboCamera_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void btnCam2_Click(object sender, EventArgs e)
        {
            
            if (this.videoCaptureDevice != null && this.videoCaptureDevice.IsRunning)
            {
                List<Image> images = new List<Image>();

                images.Add(this.pictureBox2.BackgroundImage);
                this.SaveWebCamPhoto(images);
                return;
            } else
            {
                MessageBox.Show("Please Toggle ON Cam first");
            }
        }
    }
}

