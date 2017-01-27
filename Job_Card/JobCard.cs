namespace Job_Card
{
    using Job_Card.Properties;
    using Microsoft.Office.Interop.Word;
    using PresentationControls;
    using System = System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data;
    using Guid = System.Guid;
    using Activator = System.Activator;
    using MailMessage = System.Net.Mail.MailMessage;
    using MidpointRounding = System.MidpointRounding;
    using TimeSpan = System.TimeSpan;
    using DBNull = System.DBNull;
    using Math = System.Math;
    using Exception = System.Exception;
    using EventHandler = System.EventHandler;
    using Environment= System.Environment;
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

    public class JobCard : Form
    {
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
        private Button btnPrintAll;
        private Button btnPrintBusiness;
        private Button btnPrintCustomerCopy;
        private Button btnPrintForWork;
        private Button btnRakesh;
        private Button btnReport;
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
        private ComboBox cboReportEndMonth;
        private ComboBox cboReportProduct;
        private ComboBox cboReportStartMonth;
        private ComboBox cboReportYear;
        private CheckBox[] checkBox;
        private IContainer components = null;
        private bool compress = true;
        private List<Control> controls = new List<Control>();
        public static List<string> currentPhotoPaths;
        public static int currentPictureIndex;
        private DataGridView datagrid;
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
        private const int designHeight = 0x36f;
        private const int designWidth = 0x568;
        private const int detailCount = 0x21;
        private static string Disclaimer;
        private Dictionary<string, Control> fieldNameToControlMapping;
        private int freightIndex = 0x1d;
        private GroupBox grpBoxPlating;
        private GroupBox grpBoxPolish;
        private int gstIndex = 0x1f;
        private static readonly List<string> ImageExtensions;
        private string insertFieldsSql;
        private string insertValuesSql;
        private bool isLocked = true;
        private TextBox jobAddress;
        private CheckBox jobCompleted;
        private CheckBox fastPrint;
        private TextBox jobCustomer;
        private TextBox jobBusinessName;
        private TextBox jobDate;
        private TextBox jobDateCompleted;
        private TextBox jobDatePaid;
        private TextBox jobDateRequired;
        private DateTime jobDateValForPhoto;
        private TextBox jobDelivery;
        private TextBox[] jobDetail;
        private TextBox jobEmail;
        private Label jobID;
        private TextBox jobNotes;
        private TextBox jobOrderNumber;
        private ComboBox jobPaymentBy;
        private TextBox jobPhone;
        private List<string> jobPhotos = null;
        private TextBox[] jobPrice;
        private TextBox[] jobQty;
        private ComboBox jobReceivedFrom;
        private TextBox[] jobType;
        private TextBox[] jobUnitPrice;
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
        private int lastID = 0xea60;
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
        private Dictionary<string, System.Type> photoTypes;
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
        private ComboBox SuperSearchField;
        private static bool temporarilyDisableNewLineAtEnd;
        private int totalIndex = 0x20;
        private TextBox txtSearchField;
        private Dictionary<string, System.Type> types;
        private List<Control> undoList = new List<Control>();
        private string updateSql;
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
            PicturePath = @"K:";//@"\\tcsp4\Kodak Pictures\";
            currentPictureIndex = 0;
            lastFontName = null;
            lastFontSize = -1;
            lastFontStyle = FontStyle.Regular;
            temporarilyDisableNewLineAtEnd = false;
            Disclaimer = "All work not collected within 3 months of completion will be sold to defer costs.  At Advanced Chrome Platers Ltd we have a combined electroplating and polishing" + Environment.NewLine + "history of over 60 years. Advanced Chrome Platers Ltd treat all jobs with the utmost care and attention, however we take no responsibility for any adverse" + Environment.NewLine + "changes in the condition of items during stripping, polishing and/or plating processes.  Please also note that items held at our premises are not covered by our" + Environment.NewLine + "insurance for theft, fire etc, and you may wish to contact your insurance agent regarding cover for any valuable items during the time they are held on our" + Environment.NewLine + "premises.";
            searchRows = 0;
            b = null;
        }

        public JobCard()
        {
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
            this.types = DataAccess.GetFieldDataTypes("jobs");
            this.photoTypes = DataAccess.GetFieldDataTypes("jobPictures");
            SizeF factor = new SizeF(((float) base.Width) / 1384f, ((float) base.Height) / 879f);
            foreach (object obj2 in base.Controls)
            {
                Control item = (Control) obj2;
                item.Scale(factor);
                Font font = new Font(item.Font.FontFamily.Name, item.Font.Size * (((float) base.Width) / 1384f), item.Font.Style);
                item.Font = font;
                string name = item.Name;
                if (this.types.ContainsKey(name))
                {
                    System.Type type = this.types[name];
                    if (item is TextBox)
                    {
                        item.TextChanged += new EventHandler(this.control_TextChanged);
                        ((TextBox) item).DoubleClick += new EventHandler(this.SingleSearch);
                    }
                    else if (item is CheckBox)
                    {
                        ((CheckBox) item).CheckedChanged += new EventHandler(this.control_TextChanged);
                        ((CheckBox) item).DoubleClick += new EventHandler(this.SingleSearch);
                    }
                    else if (item is Label)
                    {
                        ((Label) item).DoubleClick += new EventHandler(this.SingleSearch);
                    }
                    else if (item is ComboBox)
                    {
                        ((ComboBox) item).FlatStyle = FlatStyle.Flat;
                        ((ComboBox) item).TextChanged += new EventHandler(this.control_TextChanged);
                        ((ComboBox) item).DoubleClick += new EventHandler(this.SingleSearch);
                    }
                    this.controls.Add(item);
                    this.fieldNameToControlMapping[name] = item;
                    this.originalValues[name] = null;
                }
            }
            if (!System.IO.File.Exists(DBPath))
            {
            }
            this.GetLatestJob();
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
                        box = (TextBox) sender;
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
                        box2 = (TextBox) this.fieldNameToControlMapping[box.Name.Replace(oldValue, "UnitPrice")];
                        box3 = (TextBox) this.fieldNameToControlMapping[box.Name.Replace(oldValue, "Price")];
                        box4 = (TextBox) this.fieldNameToControlMapping[box.Name.Replace(oldValue, "Qty")];
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
                        box2.Text = (num5 / ((double) result)).ToString("F2");
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
                    now = now.AddDays((Control.ModifierKeys == Keys.Shift) ? ((double) (-7)) : ((double) 7));
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

        private void btnCustomers_Click(object sender, EventArgs e)
        {
            if (!this.NeedSave(true, false))
            {
                JobQueryForm form = new JobQueryForm();
                form.Search("SELECT jobID, jobCustomer, jobBusinessName, jobPhone, jobDateCompleted," + this.AllDetails + " FROM jobs WHERE NOT ISNULL(jobDateCompleted) ORDER BY jobDateCompleted desc");
                form.ShowDialog();
                if (JobQueryForm.selectedJobId > -1)
                {
                    string sql = "SELECT * FROM jobs WHERE jobID = " + JobQueryForm.selectedJobId.ToString();
                    DataAccess.ReadRecords(this.datagrid, sql);
                    this.Load(0);
                }
            }
        }

        private void btnDuplicate_Click(object sender, EventArgs e)
        {
            if (!this.NeedSave(true, false))
            {
                this.lastID = Math.Max(0xea60, this.lastID + 1);
                if (DataAccess.Update(string.Concat(new object[] { 
                    "INSERT INTO jobs(jobID, jobDate, jobOrderNumber, jobCustomer, jobBusinessName, jobPhone, jobAddress, jobEmail, jobDelivery, jobReceivedFrom) Values (", this.lastID.ToString(), ",DATE(),'", this.jobOrderNumber.Text, "', '", this.jobCustomer.Text, "', '", this.jobBusinessName.Text, "', '", this.jobPhone.Text, "', '", this.jobAddress.Text, "', '", this.jobEmail.Text, "', '", this.jobDelivery.Text, "', '", this.jobReceivedFrom,
                    "')"
                })))
                {
                    this.GetLatestJob();
                }
                this.jobDateRequired.Focus();
            }
        }

        private bool incurCreditCardSurcharge()
        {
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
            if (this.jobPaymentBy.Text.Length > 1 && "VISAMasterCard".Contains(this.jobPaymentBy.Text))
            {
                if (new DateTime(_year,_month,_day,0,0,0,0) >= new DateTime(2016,12,31,0,0,0,0))
                {
                    return true;
                }
            }
            return false;
        }

        private void btnEmail_Click(object sender, EventArgs e)
        {
            string emailaddress = this.jobEmail.Text.Trim();
            if (!this.IsValid(emailaddress))
            {
                MessageBox.Show("can't email as email address " + emailaddress + " is empty or not correct. CHECK AGAIN PLEASE");
            }
            else
            {
                string str2 = "Your JobID# " + this.jobID.Text;
                string str3 = string.IsNullOrWhiteSpace(this.jobOrderNumber.Text) ? "" : (" (Your ref Order#" + this.jobOrderNumber.Text.Trim() + ")");
                string csSubject = str2 + str3;
                string csBody = "Dear " + this.jobCustomer.Text + "," + Environment.NewLine + Environment.NewLine;
                string printToPDF = Path.Combine(Path.Combine(Environment.ExpandEnvironmentVariables("%userprofile%"), "Documents"), Environment.UserName + "TempJobToPdf");
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
                            subTotalText += " (+2% Card surch)";
                        }
                        str10 = str10 + Environment.NewLine + subTotalText.PadRight(50) + " $" + this.jobPrice[0x19].Text.PadLeft(8);
                        str10 = (str10 + Environment.NewLine + "GST".PadRight(50) + " $" + this.jobPrice[0x1a].Text.PadLeft(8)) + Environment.NewLine + "_".PadRight(70, '_');
                        csBody = str10 + Environment.NewLine + "Total Due".PadRight(50) + " $" + this.jobPrice[0x1b].Text.PadLeft(8);
                    }
                    else
                    {
                        csBody = csBody + "It has been paid and ready for pickup (or delivery if you specified this).";
                    }
                }
                this.SendMail(emailaddress, csSubject, csBody, flag ? printToPDF : null);
            }
        }

        private void btnExistingJobs_Click(object sender, EventArgs e)
        {
            if (!this.NeedSave(true, false))
            {
                JobQueryForm form = new JobQueryForm();
                form.Search("SELECT jobID, jobCustomer, jobBusinessName, jobPhone, jobDate," + this.AllDetails + " FROM jobs WHERE ISNULL(jobDateCompleted) ORDER BY jobDate");
                form.ShowDialog();
                if (JobQueryForm.selectedJobId > -1)
                {
                    string sql = "SELECT * FROM jobs WHERE jobID = " + JobQueryForm.selectedJobId.ToString();
                    DataAccess.ReadRecords(this.datagrid, sql);
                    this.Load(0);
                }
            }
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            base.Close();
        }

        private void btnLatestJob_Click(object sender, EventArgs e)
        {
            if (!this.NeedSave(true, false))
            {
                string sql = "Select Top 1 * from jobs ORDER BY jobID desc";
                DataAccess.ReadRecords(this.datagrid, sql);
                this.Load(0);
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

        private void btnNavigateBack_Click(object sender, EventArgs e)
        {
            this.GetPreviousJob();
        }

        private void btnNavigateForward_Click(object sender, EventArgs e)
        {
            this.GetNextJob();
        }

        private void btnNewJob_Click(object sender, EventArgs e)
        {
            if (!this.NeedSave(true, false))
            {
                this.lastID = Math.Max(0xea60, this.lastID + 1);
                if (DataAccess.Update("INSERT INTO jobs(jobID, jobDate) Values (" + this.lastID.ToString() + ",DATE())"))
                {
                    this.DisclaimerNote();
                    this.GetLatestJob();
                }
                this.jobCustomer.Focus();
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

        private void btnReport_Click(object sender, EventArgs e)
        {
            int month = DateTime.Now.AddMonths(-1).Month;
            int num2 = month;
            int year = ((DateTime.Now.Month == 1) ? -1 : 0) + DateTime.Now.Year;
            int result = 0;
            int num5 = 0;
            int num6 = 0;
            if (int.TryParse(this.cboReportStartMonth.Text, out result))
            {
                month = result;
            }
            if (int.TryParse(this.cboReportEndMonth.Text, out num5))
            {
                num2 = num5;
            }
            num2 = Math.Max(month, num2);
            if (int.TryParse(this.cboReportYear.Text, out num6))
            {
                year = num6;
            }
            DateTime time = new DateTime(year, month, 1);
            DateTime time2 = new DateTime(year, num2, 1).AddMonths(1);
            int num7 = time2.Year;
            bool flag = true;
            if (this.cboReportProduct.Text.Contains("Select"))
            {
                flag = false;
            }
            int num8 = 0;
            double num9 = 0.0;
            int num10 = 0;
            double num11 = 0.0;
            int num12 = 0;
            double num13 = 0.0;
            int num14 = 0;
            double num15 = 0.0;
            int num16 = 0;
            double num17 = 0.0;
            int num18 = 0;
            double num19 = 0.0;
            int num20 = 0;
            double num21 = 0.0;
            int num22 = 0;
            double num23 = 0.0;
            int num24 = 0;
            double num25 = 0.0;
            int num26 = 0;
            double num27 = 0.0;
            int num28 = 0;
            double num29 = 0.0;
            int num30 = 0;
            double num31 = 0.0;
            string str = string.Concat(new object[] { "#", month, "/1/", year, "#" });
            string str2 = string.Concat(new object[] { "#", time2.Month, "/1/", num7, "#" });
            string str3 = "";
            int num32 = 12;
            int num33 = 1;
            Progress progress = new Progress {
                progressBar1 = { Maximum = num32 },
                label1 = { Text = "For " + (flag ? ("Work item CONTAINING " + this.cboReportProduct.Text) : " all items") + " Between " + time.ToShortDateString() + " and " + time2.ToShortDateString() },
                chart1 = { Visible = false }
            };
            progress.Show();
            for (int i = 1; i <= 12; i++)
            {
                string str6;
                System.Windows.Forms.Application.DoEvents();
                if (progress.IsDisposed)
                {
                    return;
                }
                progress.progressBar1.Value = num33;
                progress.Refresh();
                num33++;
                int num35 = 0;
                double num36 = 0.0;
                string str4 = "";
                if (flag)
                {
                    str3 = str3 + "(";
                }
                int num37 = 0;
                while (num37 < 0x12)
                {
                    if (flag)
                    {
                        str6 = str3;
                        str3 = str6 + "jobType" + num37.ToString("D2") + " Like \"%" + this.cboReportProduct.Text + "%\"" + ((num37 < 0x11) ? " or " : ") and ");
                    }
                    str4 = str4 + ",SUM(jobPrice" + num37.ToString("D2") + ")";
                    num37++;
                }
                string sql = "SELECT COUNT(*) as jobsCount" + str4 + " from jobs WHERE " + str3;
                switch (i)
                {
                    case 1:
                        str6 = sql;
                        sql = str6 + "jobDate >=" + str + " and jobDate < " + str2 + " and ISNULL(jobDateCompleted) and ISNULL(jobDatePaid)";
                        break;

                    case 2:
                        str6 = sql;
                        sql = str6 + "jobDate >=" + str + " and jobDate < " + str2 + " and jobDateCompleted >=" + str + " and jobDateCompleted < " + str2 + " and ISNULL(jobDatePaid)";
                        break;

                    case 3:
                        str6 = sql;
                        sql = str6 + "jobDate >=" + str + " and jobDate < " + str2 + " and NOT (jobDateCompleted >= " + str + " and jobDateCompleted < " + str2 + ") and NOT (jobDatePaid >= " + str + " and jobDatePaid < " + str2 + ")";
                        break;

                    case 4:
                        str6 = sql;
                        sql = str6 + "jobDate >=" + str + " and jobDate < " + str2 + " and jobDateCompleted >= " + str + " and jobDateCompleted < " + str2 + " and NOT (jobDatePaid >= " + str + " and jobDatePaid < " + str2 + ")";
                        break;

                    case 5:
                        str6 = sql;
                        sql = str6 + "jobDate >=" + str + " and jobDate < " + str2 + " and jobDateCompleted >= " + str + " and jobDateCompleted < " + str2 + " and jobDatePaid >= " + str + " and jobDatePaid < " + str2;
                        break;

                    case 6:
                        str6 = sql;
                        sql = str6 + "jobDate < " + str + " and jobDateCompleted >= " + str + " and jobDateCompleted < " + str2 + " and NOT (jobDatePaid >= " + str + " and jobDatePaid < " + str2 + ")";
                        break;

                    case 7:
                        str6 = sql;
                        sql = str6 + "jobDate < " + str + " and jobDateCompleted >= " + str + " and jobDateCompleted < " + str2 + " and jobDatePaid >= " + str + " and jobDatePaid < " + str2;
                        break;

                    case 8:
                        str6 = sql;
                        sql = str6 + "jobDate < " + str + "and jobDateCompleted < " + str + " and jobDatePaid >= " + str + " and jobDatePaid < " + str2;
                        break;

                    case 9:
                        str6 = sql;
                        sql = str6 + "jobDate < " + str + " and jobDateCompleted >= " + str + " and jobDateCompleted < " + str2 + " and ISNULL(jobDatePaid)";
                        break;

                    case 10:
                        str6 = sql;
                        sql = str6 + "jobDate >=" + str + " and jobDate < " + str2;
                        break;

                    case 11:
                        str6 = sql;
                        sql = str6 + "jobDateCompleted >=" + str + " and jobDateCompleted < " + str2;
                        break;

                    case 12:
                        str6 = sql;
                        sql = str6 + "jobDatePaid >=" + str + " and jobDatePaid < " + str2;
                        break;
                }
                DataRowCollection rows = DataAccess.ReadRecords(sql);
                if ((rows != null) && (rows.Count > 0))
                {
                    object obj2 = rows[0][0];
                    if ((obj2 != null) && (obj2.GetType() != typeof(DBNull)))
                    {
                        num35 = (int) obj2;
                    }
                    for (num37 = 1; num37 <= 0x12; num37++)
                    {
                        object obj3 = rows[0][num37];
                        System.Type type = obj3.GetType();
                        if ((obj3 != null) && (type != typeof(DBNull)))
                        {
                            num36 += ((double) ((int) (((double) obj3) * 100.0))) / 100.0;
                        }
                    }
                }
                switch (i)
                {
                    case 1:
                        num8 += num35;
                        num9 += num36;
                        break;

                    case 2:
                        num10 += num35;
                        num11 += num36;
                        break;

                    case 3:
                        num12 += num35;
                        num13 += num36;
                        break;

                    case 4:
                        num14 += num35;
                        num15 += num36;
                        break;

                    case 5:
                        num16 += num35;
                        num17 += num36;
                        break;

                    case 6:
                        num18 += num35;
                        num19 += num36;
                        break;

                    case 7:
                        num20 += num35;
                        num21 += num36;
                        break;

                    case 8:
                        num22 += num35;
                        num23 += num36;
                        break;

                    case 9:
                        num24 += num35;
                        num25 += num36;
                        break;

                    case 10:
                        num26 += num35;
                        num27 += num36;
                        break;

                    case 11:
                        num28 += num35;
                        num29 += num36;
                        break;

                    case 12:
                        num30 += num35;
                        num31 += num36;
                        break;
                }
            }
            progress.richTextBox1.Text = string.Concat(new object[] { 
                "Created here but NOT Completed and NOT paid #", num8, " Total ", num9.ToString("C2"), Environment.NewLine, "Created and completed here but NOT paid #", num10, " Total ", num11.ToString("C2"), Environment.NewLine, "Created here but Completed and paid elsewhere #", num12, " Total ", num13.ToString("C2"), Environment.NewLine, "Created and completed here but paid elsewhere #",
                num14, " Total ", num15.ToString("C2"), Environment.NewLine, "Created, completed and paid here #", num16, " Total ", num17.ToString("C2"), Environment.NewLine, "Created and paid elsewhere but completed here #", num18, " Total ", num19.ToString("C2"), Environment.NewLine, "Created elsewhere but completed and paid here #", num20,
                " Total ", num21.ToString("C2"), Environment.NewLine, "Created and completed elsewhere but paid here #", num22, " Total ", num23.ToString("C2"), Environment.NewLine, "Created elsewhere but completed here and NOT paid #", num24, " Total ", num25.ToString("C2"), Environment.NewLine, "CREATED HERE #", num26, " TOTAL ",
                num27.ToString("C2"), Environment.NewLine, "COMPLETED HERE #", num28, " TOTAL ", num29.ToString("C2"), Environment.NewLine, "PAID HERE #", num30, " TOTAL", num31.ToString("C2")
            });
        }

        private void btnCam1_Click(object sender, EventArgs e)
        {

            Form1.useMediaPlayer = false;
            Form1.VIDEODEVICE = 1;
            if (JobCard.popup != null && !(JobCard.popup.IsDisposed))
            {
                JobCard.popup.Close();
            }
            Form1 form = new Form1();
            
            try
            {
                form.ShowDialog();
                form.TopMost = true;
                SaveWebCamPhoto();
            }
            catch (Exception err)
            { }
        }
        private void btnCam2_Click(object sender, EventArgs e)
        {
            Form1.useMediaPlayer = false;
            Form1.VIDEODEVICE = 2;
            Form1 form = new Form1();
            if (JobCard.popup != null && !(JobCard.popup.IsDisposed))
            {
                JobCard.popup.Close();
            }
            try
            {
                form.ShowDialog();
                form.TopMost = true;
                SaveWebCamPhoto();
            }
            catch (Exception err)
            { }
        }
        private void btnSave_Click(object sender, EventArgs e)
        {
            if (this.NeedSave(false, true))
            {
                if (!DataAccess.Update(this.updateSql))
                {
                    MessageBox.Show("Save failed", "SAVE FAIL", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                }
                else
                {
                    string sql = "SELECT * FROM jobs WHERE jobID = " + this.jobID.Text;
                    DataAccess.ReadRecords(this.datagrid, sql);
                    this.Load(0);
                }
            }
            else
            {
                MessageBox.Show("No changes to save");
            }
        }

        private void btnSearchField_Click(object sender, EventArgs e)
        {
            this.Search();
        }

        private void btnSearchLists_Click(object sender, EventArgs e)
        {
            if (!this.NeedSave(true, false))
            {
                new JobQueryForm().ShowDialog();
                if (JobQueryForm.selectedJobId > -1)
                {
                    string sql = "SELECT * FROM jobs WHERE jobID = " + JobQueryForm.selectedJobId.ToString();
                    DataAccess.ReadRecords(this.datagrid, sql);
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

        private void CheckBeforeQuit(object sender, FormClosingEventArgs e)
        {
            if (this.NeedSave(true, false))
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
                                str = str7 + str5.PadRight(0x2a) + " x" + this.jobQty[i].Text.PadLeft(3) + " @ $" + this.jobUnitPrice[i].Text.PadLeft(8) + "=" + (string.IsNullOrWhiteSpace(this.jobPrice[i].Text) ? "" : (" $" + this.jobPrice[i].Text.PadLeft(8))) + Environment.NewLine;
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
                Control control = (Control) sender;
                string stringValue = "";
                bool flag = this.ControlValueChangedFromLoaded(control, false, out stringValue);
                if (sender is ComboBox)
                {
                    ((ComboBox) control).BackColor = flag ? Color.LightYellow : Color.WhiteSmoke;
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
                stringValue = ((TextBox) control).Text;
                if (isUndo)
                {
                    ((TextBox) control).Text = str2;
                }
            }
            else if (flag3)
            {
                stringValue = ((Label) control).Text;
                if (isUndo)
                {
                    ((Label) control).Text = str2;
                }
            }
            else if (flag4)
            {
                if (str2 == "")
                {
                    str2 = "False";
                }
                stringValue = ((CheckBox) control).Checked.ToString();
                if (isUndo)
                {
                    ((CheckBox) control).Checked = str2.ToUpperInvariant() == "TRUE";
                }
            }
            else if (flag5)
            {
                stringValue = ((ComboBox) control).Text;
                if (isUndo)
                {
                    ((ComboBox) control).Text = str2;
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
                box = (TextBox) sender;
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
         
                    string[] data = (string[]) e.Data.GetData(DataFormats.FileDrop);
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
                                    DialogResult result = MessageBox.Show(string.Concat(new object[] { "Warning image file: ", str, " is ", (int) span.TotalDays, " days ", (int) span.TotalHours, " hours old. Are you sure this is the correct image file?" }), "Check Image File", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Hand);
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
                int num = 0;
                string str = null;
                while (num < 0x3e8)
                {
                    string str2 = "deleteMe" + num + ".jpg";
                    try
                    {
                        if (System.IO.File.Exists(str2))
                        {
                            System.IO.File.Delete(str2);
                        }
                        if (!System.IO.File.Exists(str2))
                        {
                            str = str2;
                            System.IO.File.Copy(path, str2);
                            if (System.IO.File.Exists(str2))
                            {
                                if (path.ToUpper().EndsWith("MOV") || path.ToUpper().EndsWith("MP4"))
                                {
                                    return MovieImage;
                                }
                                else
                                    return Image.FromFile(str2);
                            }
                        }
                    }
                    catch (Exception err)
                    {
                        System.Console.Out.WriteLine(err.Message);
                    }
                    num++;
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

        private void GetLatestJob()
        {
            string sql = "SELECT MAX(jobID) FROM jobs";
            object obj2 = DataAccess.ReadSingleValue(sql);
            if (obj2 != null)
            {
                try
                {
                    this.lastID = (int)obj2;
                    sql = "SELECT * FROM jobs WHERE jobID=" + ((int)obj2).ToString();                    
                    DataAccess.ReadRecords(this.datagrid, sql);
                    this.Load(0);
                }
                catch (Exception err)
                {
                    sql = "INSERT INTO jobs(jobID, jobDate) Values(1000, DATE())";
                    DataAccess.Update(sql);
                }
            }
            DataAccess.ReadRecords(this.datagrid, sql);
            this.Load(0);
        }

        private void GetNextJob()
        {
            if (!this.NeedSave(true, false))
            {
                if (int.Parse(this.jobID.Text) < this.lastID)
                {
                    string sql = "SELECT TOP 1 * FROM jobs WHERE jobID > " + this.jobID.Text + " ORDER BY jobID";
                    DataAccess.ReadRecords(this.datagrid, sql);
                    this.Load(0);
                }
                else
                {
                    SystemSounds.Beep.Play();
                }
            }
        }

        private void GetPreviousJob()
        {
            if (!this.NeedSave(true, false))
            {
                if (int.Parse(this.jobID.Text) > 0)
                {
                    string sql = "SELECT TOP 1 * FROM jobs WHERE jobID < " + this.jobID.Text + " ORDER BY jobID desc";
                    DataAccess.ReadRecords(this.datagrid, sql);
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
                this.jobType[i].ReadOnly = true;
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

        private void DisclaimerNote()
        {

            if (JobTypePopup.isWheelApp())
            {
                if (this.jobNotes.Text == null || this.jobNotes.Text.Length < 300)
                {
                    this.jobNotes.Text += "DISCLAIMER NOTICE:\n" +
        "When Aluminium wheels have cracks or are damaged in any way the stresses caused by the impact cannot be truly identified without getting the wheel tested." +
        "We at Advanced Chrome Platers weld the cracks and push out dents with a specific wheel repair machine designed and built in Europe." +
        "This does not in any way certify the wheel for further use on a Vehicle." +
        "We do not test wheels at Advanced Chrome Platers, and take no responsibility if the wheel is used on a vehicle without the wheel being certified." +
        "It is up to the owner or customer to get the wheel certified and tested for air leaks at their own cost if they feel it is necessary." +
        "We do not paint wheels.";
                    if (this.NeedSave(false, true))
                    {
                        DataAccess.Update(this.updateSql);
                    }
                }
            }
        }

        private void InitializeComponent()
        {
            ComponentResourceManager manager = new ComponentResourceManager(typeof(JobCard));
            this.btnNewJob = new Button();
            this.btnIncompleteJobs = new Button();
            this.btnSearchLists = new Button();
            this.btnUnpaidCustomers = new Button();
            this.label1 = new Label();
            this.jobID = new Label();
            this.btnNavigateBack = new Button();
            this.btnNavigateForward = new Button();
            this.label2 = new Label();
            this.jobDate = new TextBox();
            this.jobCustomer = new TextBox();
            this.label3 = new Label();
            this.jobBusinessName = new TextBox();
            this.labelJobBusinessName = new Label();
            this.jobAddress = new TextBox();
            this.label4 = new Label();
            this.jobPhone = new TextBox();
            this.label5 = new Label();
            this.jobEmail = new TextBox();
            this.label6 = new Label();
            this.jobOrderNumber = new TextBox();
            this.label7 = new Label();
            this.jobDelivery = new TextBox();
            this.label8 = new Label();
            this.btnCollect = new Button();
            this.btnCourier = new Button();
            this.label9 = new Label();
            this.jobReceivedFrom = new ComboBox();
            this.jobDateRequired = new TextBox();
            this.label10 = new Label();
            this.jobDateCompleted = new TextBox();
            this.label11 = new Label();
            this.jobPaymentBy = new ComboBox();
            this.label12 = new Label();
            this.jobNotes = new TextBox();
            this.label13 = new Label();
            this.jobDatePaid = new TextBox();
            this.label14 = new Label();
            this.btnToday = new Button();
            this.btnCopper = new Button();
            this.btnNickle = new Button();
            this.btnChrome = new Button();
            this.btnBrass = new Button();
            this.btnBronze = new Button();
            this.btnTin = new Button();
            this.btnGold = new Button();
            this.btnSilver = new Button();
            this.btnSatin = new Button();
            this.btnGeorge = new Button();
            this.btnHenry = new Button();
            this.btnRakesh = new Button();
            this.btnBritt = new Button();
            this.datagrid = new DataGridView();
            this.btnExit = new Button();
            this.btnSave = new Button();
            this.btnEmail = new Button();
            this.btnCam1 = new Button();
            this.btnCam2 = new Button();

            this.btnPrintCustomerCopy = new Button();
            this.btnPrintBusiness = new Button();
            this.jobCompleted = new CheckBox();
            this.fastPrint = new CheckBox();
            this.panelSearchField = new Panel();
            this.lblResults = new Label();
            this.slider = new TrackBar();
            this.btnCancelSearch = new Button();
            this.btnSearchField = new Button();
            this.txtSearchField = new TextBox();
            this.lblSearchOnField = new Label();
            this.btnLatestJob = new Button();
            this.btnNextPhoto = new Button();
            this.btnPrintForWork = new Button();
            this.btnLockUnlock = new Button();
            this.btnUndo = new Button();
            this.picPaid = new PictureBox();
            this.pictureBox1 = new PictureBox();
            this.btnPrintAll = new Button();
            this.btnTodayForDateCompleted = new Button();
            this.btnAddWeek = new Button();
            this.btnDuplicate = new Button();
            this.grpBoxPlating = new GroupBox();
            this.grpBoxPolish = new GroupBox();
            this.btnCollapseToggle = new Button();
            this.cboReportStartMonth = new ComboBox();
            this.cboReportEndMonth = new ComboBox();
            this.cboReportYear = new ComboBox();
            this.cboReportProduct = new ComboBox();
            this.btnReport = new Button();
            this.SuperSearchField = new ComboBox();
            ((ISupportInitialize) this.datagrid).BeginInit();
            this.panelSearchField.SuspendLayout();
            this.slider.BeginInit();
            ((ISupportInitialize) this.picPaid).BeginInit();
            ((ISupportInitialize) this.pictureBox1).BeginInit();
            this.grpBoxPlating.SuspendLayout();
            this.grpBoxPolish.SuspendLayout();
            base.SuspendLayout();
            this.btnNewJob.Font = new Font("Arial", 13f, FontStyle.Bold);
            this.btnNewJob.Location = new Point(12, 0x35);
            this.btnNewJob.Name = "btnNewJob";
            this.btnNewJob.Size = new Size(0xa2, 0x2a);
            this.btnNewJob.TabIndex = 0;
            this.btnNewJob.Text = "New Job";
            this.btnNewJob.UseVisualStyleBackColor = true;
            this.btnNewJob.Click += new EventHandler(this.btnNewJob_Click);
            this.btnIncompleteJobs.Font = new Font("Arial", 12f, FontStyle.Bold);
            this.btnIncompleteJobs.Location = new Point(12, 0x69);
            this.btnIncompleteJobs.Name = "btnIncompleteJobs";
            this.btnIncompleteJobs.Size = new Size(0xa2, 0x2a);
            this.btnIncompleteJobs.TabIndex = 1;
            this.btnIncompleteJobs.Text = "Incomplete Jobs";
            this.btnIncompleteJobs.UseVisualStyleBackColor = true;
            this.btnIncompleteJobs.Click += new EventHandler(this.btnExistingJobs_Click);
            this.btnSearchLists.Font = new Font("Arial", 13f, FontStyle.Bold);
            this.btnSearchLists.Location = new Point(13, 0x99);
            this.btnSearchLists.Name = "btnSearchLists";
            this.btnSearchLists.Size = new Size(0xa2, 0x2a);
            this.btnSearchLists.TabIndex = 2;
            this.btnSearchLists.Text = "Search Lists";
            this.btnSearchLists.UseVisualStyleBackColor = true;
            this.btnSearchLists.Click += new EventHandler(this.btnSearchLists_Click);
            this.btnUnpaidCustomers.Font = new Font("Arial", 12f, FontStyle.Bold);
            this.btnUnpaidCustomers.Location = new Point(13, 0xca);
            this.btnUnpaidCustomers.Name = "btnUnpaidCustomers";
            this.btnUnpaidCustomers.Size = new Size(0xa2, 0x2a);
            this.btnUnpaidCustomers.TabIndex = 3;
            this.btnUnpaidCustomers.Text = "Unpaid Customers";
            this.btnUnpaidCustomers.UseVisualStyleBackColor = true;
            this.btnUnpaidCustomers.Click += new EventHandler(this.btnCustomers_Click);
            this.label1.AutoSize = true;
            this.label1.Font = new Font("Arial", 11f);
            this.label1.Location = new Point(0xba, 0x10);
            this.label1.Name = "label1";
            this.label1.Size = new Size(0x57, 0x11);
            this.label1.TabIndex = 4;
            this.label1.Text = "Job Number";
            this.label1.TextAlign = ContentAlignment.MiddleRight;
            this.label1.Click += new EventHandler(DeleteJobClicked);
            this.jobID.AutoSize = true;
            this.jobID.Font = new Font("Arial", 14f, FontStyle.Bold);
            this.jobID.ForeColor = Color.Red;
            this.jobID.Location = new Point(0x117, 12);
            this.jobID.Name = "jobID";
            this.jobID.Size = new Size(0x4c, 0x16);
            this.jobID.TabIndex = 5;
            this.jobID.Text = "000000";
            this.jobID.TextAlign = ContentAlignment.MiddleLeft;
            this.jobID.TextChanged += new EventHandler(this.JobIDChanged);
            this.btnNavigateBack.Location = new Point(360, 12);
            this.btnNavigateBack.Name = "btnNavigateBack";
            this.btnNavigateBack.Size = new Size(0x1d, 0x15 + 5);
            this.btnNavigateBack.TabIndex = 6;
            this.btnNavigateBack.Text = "<<";
            this.btnNavigateBack.UseVisualStyleBackColor = true;
            this.btnNavigateBack.Click += new EventHandler(this.btnNavigateBack_Click);
            this.btnNavigateForward.Location = new Point(0x18b, 12);
            this.btnNavigateForward.Name = "btnNavigateForward";
            this.btnNavigateForward.Size = new Size(0x1d, 0x15 + 5);
            this.btnNavigateForward.TabIndex = 7;
            this.btnNavigateForward.Text = ">>";
            this.btnNavigateForward.UseVisualStyleBackColor = true;
            this.btnNavigateForward.Click += new EventHandler(this.btnNavigateForward_Click);
            this.label2.AutoSize = true;
            this.label2.Font = new Font("Arial", 11f);
            this.label2.Location = new Point(0x1b4, 0x10);
            this.label2.Name = "label2";
            this.label2.RightToLeft = RightToLeft.No;
            this.label2.Size = new Size(0x42, 0x11);
            this.label2.TabIndex = 9;
            this.label2.Text = "Job Date";
            this.label2.TextAlign = ContentAlignment.MiddleRight;
            this.jobDate.Font = new Font("Arial", 11f);
            this.jobDate.Location = new Point(0x1f7, 12);
            this.jobDate.Name = "jobDate";
            this.jobDate.Size = new Size(0x5c, 0x18);
            this.jobDate.TabIndex = 10;
            this.jobDate.Validating += new CancelEventHandler(this.DateValidating);
            this.jobCustomer.Font = new Font("Arial", 11f);
            this.jobCustomer.Location = new Point(0x132, 0x2a);//42px h
            this.jobCustomer.Name = "jobCustomer";
            this.jobCustomer.Size = new Size(0x121, 0x18);
            this.jobCustomer.TabIndex = 12;
            this.jobBusinessName.Font = new Font("Arial", 11f);
            this.jobBusinessName.Location = new Point(0x132, 72);//42px h
            this.jobBusinessName.Name = "jobBusinessName";
            this.jobBusinessName.Size = new Size(0x121, 0x18);
            this.jobBusinessName.TabIndex = 13;
            this.labelJobBusinessName.AutoSize = true;
            this.labelJobBusinessName.Font = new Font("Arial", 11f);
            this.labelJobBusinessName.Location = new Point(0xb8, 75);//45px h
            this.labelJobBusinessName.Name = "label3";
            this.labelJobBusinessName.RightToLeft = RightToLeft.No;
            this.labelJobBusinessName.Size = new Size(0x74, 0x11);
            this.labelJobBusinessName.TabIndex = 11;
            this.labelJobBusinessName.Text = "Business Name";
            this.labelJobBusinessName.TextAlign = ContentAlignment.MiddleRight;


            this.label3.AutoSize = true;
            this.label3.Font = new Font("Arial", 11f);
            this.label3.Location = new Point(0xb8, 0x2d);
            this.label3.Name = "label3";
            this.label3.RightToLeft = RightToLeft.No;
            this.label3.Size = new Size(0x74, 0x11);
            this.label3.TabIndex = 11;
            this.label3.Text = "Customer Name";
            this.label3.TextAlign = ContentAlignment.MiddleRight;
            this.jobAddress.Font = new Font("Arial", 11f);
            this.jobAddress.Location = new Point(0x100, 104);//72px h
            this.jobAddress.Multiline = true;
            this.jobAddress.Name = "jobAddress";
            this.jobAddress.Size = new Size(0x152, 0x3e);//62h
            this.jobAddress.TabIndex = 14;
            this.label4.AutoSize = true;
            this.label4.Font = new Font("Arial", 11f);
            this.label4.Location = new Point(0xbd, 105); //75px h
            this.label4.Name = "label4";
            this.label4.RightToLeft = RightToLeft.No;
            this.label4.Size = new Size(0x3e, 0x11);
            this.label4.TabIndex = 13;
            this.label4.Text = "Address";
            this.label4.TextAlign = ContentAlignment.MiddleRight;
            this.jobPhone.Font = new Font("Arial", 11f);
            this.jobPhone.Location = new Point(660, 0x2a);
            this.jobPhone.Name = "jobPhone";
            this.jobPhone.Size = new Size(0x139, 0x18);
            this.jobPhone.TabIndex = 0x10;
            this.label5.AutoSize = true;
            this.label5.Font = new Font("Arial", 11f);
            this.label5.Location = new Point(0x25c, 0x2d);
            this.label5.Name = "label5";
            this.label5.RightToLeft = RightToLeft.No;
            this.label5.Size = new Size(50, 0x11);
            this.label5.TabIndex = 15;
            this.label5.Text = "Phone";
            this.label5.TextAlign = ContentAlignment.MiddleRight;
            this.jobEmail.Font = new Font("Arial", 11f);
            this.jobEmail.Location = new Point(660, 0x48);
            this.jobEmail.Name = "jobEmail";
            this.jobEmail.Size = new Size(0x139, 0x18);
            this.jobEmail.TabIndex = 0x12;
            this.label6.AutoSize = true;
            this.label6.Font = new Font("Arial", 11f);
            this.label6.Location = new Point(0x261, 0x4b);
            this.label6.Name = "label6";
            this.label6.RightToLeft = RightToLeft.No;
            this.label6.Size = new Size(0, 0x11);
            this.label6.TabIndex = 0x11;
            this.label6.TextAlign = ContentAlignment.MiddleRight;
            this.jobOrderNumber.Font = new Font("Arial", 11f);
            this.jobOrderNumber.Location = new Point(0x2d4, 10);
            this.jobOrderNumber.Name = "jobOrderNumber";
            this.jobOrderNumber.Size = new Size(0xf9, 0x18);
            this.jobOrderNumber.TabIndex = 20;
            this.label7.AutoSize = true;
            this.label7.Font = new Font("Arial", 11f);
            this.label7.Location = new Point(0x259, 13);
            this.label7.Name = "label7";
            this.label7.RightToLeft = RightToLeft.No;
            this.label7.Size = new Size(0x66, 0x11);
            this.label7.TabIndex = 0x13;
            this.label7.Text = "Order Number";
            this.label7.TextAlign = ContentAlignment.MiddleRight;
            this.jobDelivery.Font = new Font("Arial", 11f);
            this.jobDelivery.Location = new Point(0x149, 169); // 140px h
            this.jobDelivery.Multiline = true;
            this.jobDelivery.Name = "jobDelivery";
            this.jobDelivery.Size = new Size(0x109, 0x2b);
            this.jobDelivery.TabIndex = 0x16;
            this.label8.AutoSize = true;
            this.label8.Font = new Font("Arial", 11f);
            this.label8.Location = new Point(0xba, 170); // 140px h
            this.label8.Name = "label8";
            this.label8.RightToLeft = RightToLeft.No;
            this.label8.Size = new Size(0x8b, 0x11);
            this.label8.TabIndex = 0x15;
            this.label8.Text = "Delivery Instructions";
            this.label8.TextAlign = ContentAlignment.MiddleRight;
            this.btnCollect.Location = new Point(0x101, 190); // 160px h
            this.btnCollect.Name = "btnCollect";
            this.btnCollect.Size = new Size(0x3b, 0x16 + 5);
            this.btnCollect.TabIndex = 0x17;
            this.btnCollect.Text = "Collect";
            this.btnCollect.UseVisualStyleBackColor = true;
            this.btnCollect.Click += new EventHandler(this.btnCollect_Click);
            this.btnCourier.Location = new Point(0xc0, 190); // 160px h
            this.btnCourier.Name = "btnCourier";
            this.btnCourier.Size = new Size(0x3b, 0x16 + 5);
            this.btnCourier.TabIndex = 0x18;
            this.btnCourier.Text = "Courier";
            this.btnCourier.UseVisualStyleBackColor = true;
            this.btnCourier.Click += new EventHandler(this.btnCourier_Click);
            this.label9.AutoSize = true;
            this.label9.Font = new Font("Arial", 11f);
            this.label9.Location = new Point(0x328, 0xa2);
            this.label9.Name = "label9";
            this.label9.RightToLeft = RightToLeft.No;
            this.label9.Size = new Size(0x6c, 0x11);
            this.label9.TabIndex = 0x19;
            this.label9.Text = "Received From";
            this.label9.TextAlign = ContentAlignment.MiddleRight;
            this.jobReceivedFrom.DropDownStyle = ComboBoxStyle.DropDownList;
            this.jobReceivedFrom.Font = new Font("Arial", 11f);
            this.jobReceivedFrom.FormattingEnabled = true;
            this.jobReceivedFrom.Items.AddRange(new object[] { "","Customer", "Courier" });
            this.jobReceivedFrom.Location = new Point(0x39a, 0x9f);
            this.jobReceivedFrom.Name = "jobReceivedFrom";
            this.jobReceivedFrom.Size = new Size(0x7a, 0x19);
            this.jobReceivedFrom.TabIndex = 0x1a;
            this.jobDateRequired.Font = new Font("Arial", 11f);
            this.jobDateRequired.Location = new Point(0x2d1, 0x65);
            this.jobDateRequired.Name = "jobDateRequired";
            this.jobDateRequired.Size = new Size(0x48, 0x18);
            this.jobDateRequired.TabIndex = 0x1c;
            this.jobDateRequired.Validating += new CancelEventHandler(this.DateValidating);
            this.label10.AutoSize = true;
            this.label10.Font = new Font("Arial", 11f);
            this.label10.Location = new Point(0x259, 0x68);
            this.label10.Name = "label10";
            this.label10.RightToLeft = RightToLeft.No;
            this.label10.Size = new Size(0x66, 0x11);
            this.label10.TabIndex = 0x1b;
            this.label10.Text = "Date Required";
            this.label10.TextAlign = ContentAlignment.MiddleRight;
            this.jobDateCompleted.Font = new Font("Arial", 11f);
            this.jobDateCompleted.Location = new Point(0x2d1, 0x81);
            this.jobDateCompleted.Name = "jobDateCompleted";
            this.jobDateCompleted.Size = new Size(0x48, 0x18);
            this.jobDateCompleted.TabIndex = 30;
            this.jobDateCompleted.Validating += new CancelEventHandler(this.DateValidating);
            this.label11.AutoSize = true;
            this.label11.Font = new Font("Arial", 11f);
            this.label11.Location = new Point(0x259, 0x84);
            this.label11.Name = "label11";
            this.label11.RightToLeft = RightToLeft.No;
            this.label11.Size = new Size(0x72, 0x11);
            this.label11.TabIndex = 0x1d;
            this.label11.Text = "Date Completed";
            this.label11.TextAlign = ContentAlignment.MiddleRight;
            this.jobPaymentBy.DropDownStyle = ComboBoxStyle.DropDownList;
            this.jobPaymentBy.Font = new Font("Arial", 11f);
            this.jobPaymentBy.FormattingEnabled = true;
            this.jobPaymentBy.Items.AddRange(new object[] { "","Cash", "Cheque","Eftpos", "VISA", "MasterCard" });
            this.jobPaymentBy.Location = new Point(0x2b6, 0x9f);
            this.jobPaymentBy.Name = "jobPaymentBy";
            this.jobPaymentBy.Size = new Size(0x6c, 0x19);
            this.jobPaymentBy.TabIndex = 0x20;
            this.jobPaymentBy.TextChanged += new EventHandler(this.CheckForCreditCardSurcharge);
            this.label12.AutoSize = true;
            this.label12.Font = new Font("Arial", 11f);
            this.label12.Location = new Point(0x259, 160);
            this.label12.Name = "label12";
            this.label12.RightToLeft = RightToLeft.No;
            this.label12.Size = new Size(0x57, 0x11);
            this.label12.TabIndex = 0x1f;
            this.label12.Text = "Payment By";
            this.label12.TextAlign = ContentAlignment.MiddleRight;
            this.jobNotes.Font = new Font("Arial", 11f);
            this.jobNotes.Location = new Point(0xf3, 220);
            this.jobNotes.Multiline = true;
            this.jobNotes.Name = "jobNotes";
            this.jobNotes.Size = new Size(0x160, 0x4e);
            this.jobNotes.TabIndex = 0x22;
           
            
            this.label13.AutoSize = true;
            this.label13.Font = new Font("Arial", 11f);
            this.label13.Location = new Point(0xbf, 0xe0);
            this.label13.Name = "label13";
            this.label13.RightToLeft = RightToLeft.No;
            this.label13.Size = new Size(0x2e, 0x11);
            this.label13.TabIndex = 0x21;
            this.label13.Text = "Notes";
            this.label13.TextAlign = ContentAlignment.MiddleRight;
            this.jobDatePaid.Font = new Font("Arial", 11f);
            this.jobDatePaid.Location = new Point(0x2a7, 0xc2);
            this.jobDatePaid.Name = "jobDatePaid";
            this.jobDatePaid.Size = new Size(0x48, 0x18);
            this.jobDatePaid.TabIndex = 0x24;
            this.jobDatePaid.TextChanged += new EventHandler(this.TogglePaidStamp);
            this.label14.AutoSize = true;
            this.label14.Font = new Font("Arial", 11f);
            this.label14.Location = new Point(0x259, 0xc2);
            this.label14.Name = "label14";
            this.label14.RightToLeft = RightToLeft.No;
            this.label14.Size = new Size(0x48, 0x11);
            this.label14.TabIndex = 0x23;
            this.label14.Text = "Date Paid";
            this.label14.TextAlign = ContentAlignment.MiddleRight;
            this.btnToday.Location = new Point(0x2f4, 0xc2);
            this.btnToday.Name = "btnToday";
            this.btnToday.Size = new Size(0x49, 0x16 + 5);
            this.btnToday.TabIndex = 0x25;
            this.btnToday.Text = "Today";
            this.btnToday.UseVisualStyleBackColor = true;
            this.btnToday.Click += new EventHandler(this.btnToday_Click);
            this.btnCopper.Location = new Point(20, 0x13);
            this.btnCopper.Name = "btnCopper";
            this.btnCopper.Size = new Size(0x33, 0x17);
            this.btnCopper.TabIndex = 0x27;
            this.btnCopper.Text = "Copper";
            this.btnCopper.UseVisualStyleBackColor = true;
            this.btnCopper.Click += new EventHandler(this.MetalToPolish);
            this.btnNickle.Location = new Point(0x51, 0x13);
            this.btnNickle.Name = "btnNickle";
            this.btnNickle.Size = new Size(0x33, 0x17);
            this.btnNickle.TabIndex = 40;
            this.btnNickle.Text = "Nickle";
            this.btnNickle.UseVisualStyleBackColor = true;
            this.btnNickle.Click += new EventHandler(this.MetalToPolish);
            this.btnChrome.Location = new Point(0x8d, 0x13);
            this.btnChrome.Name = "btnChrome";
            this.btnChrome.Size = new Size(0x33, 0x17);
            this.btnChrome.TabIndex = 0x29;
            this.btnChrome.Text = "Chrome";
            this.btnChrome.UseVisualStyleBackColor = true;
            this.btnChrome.Click += new EventHandler(this.MetalToPolish);
            this.btnBrass.Location = new Point(0x8d, 0x30);
            this.btnBrass.Name = "btnBrass";
            this.btnBrass.Size = new Size(0x33, 0x17);
            this.btnBrass.TabIndex = 0x2a;
            this.btnBrass.Text = "Brass";
            this.btnBrass.UseVisualStyleBackColor = true;
            this.btnBrass.Click += new EventHandler(this.MetalToPolish);
            this.btnBronze.Location = new Point(0x8d, 0x4c);
            this.btnBronze.Name = "btnBronze";
            this.btnBronze.Size = new Size(0x33, 0x17);
            this.btnBronze.TabIndex = 0x2b;
            this.btnBronze.Text = "Bronze";
            this.btnBronze.UseVisualStyleBackColor = true;
            this.btnBronze.Click += new EventHandler(this.MetalToPolish);
            this.btnTin.Location = new Point(20, 0x30);
            this.btnTin.Name = "btnTin";
            this.btnTin.Size = new Size(0x33, 0x17);
            this.btnTin.TabIndex = 0x2c;
            this.btnTin.Text = "Tin";
            this.btnTin.UseVisualStyleBackColor = true;
            this.btnTin.Click += new EventHandler(this.MetalToPolish);
            this.btnGold.Location = new Point(0x51, 0x4c);
            this.btnGold.Name = "btnGold";
            this.btnGold.Size = new Size(0x33, 0x17);
            this.btnGold.TabIndex = 0x2f;
            this.btnGold.Text = "Gold";
            this.btnGold.UseVisualStyleBackColor = true;
            this.btnGold.Click += new EventHandler(this.MetalToPolish);
            this.btnSilver.Location = new Point(20, 0x4c);
            this.btnSilver.Name = "btnSilver";
            this.btnSilver.Size = new Size(0x33, 0x17);
            this.btnSilver.TabIndex = 0x2e;
            this.btnSilver.Text = "Silver";
            this.btnSilver.UseVisualStyleBackColor = true;
            this.btnSilver.Click += new EventHandler(this.MetalToPolish);
            this.btnSatin.Location = new Point(0x51, 0x30);
            this.btnSatin.Name = "btnSatin";
            this.btnSatin.Size = new Size(0x33, 0x17);
            this.btnSatin.TabIndex = 0x2d;
            this.btnSatin.Text = "Satin";
            this.btnSatin.UseVisualStyleBackColor = true;
            this.btnSatin.Click += new EventHandler(this.MetalToPolish);

            /*
            this.btnCopper.Visible = false;
            this.btnNickle.Visible = false;
            this.btnChrome.Visible = false;
            this.btnTin.Visible = false;
            this.btnSatin.Visible = false;
            this.btnBrass.Visible = false;
            this.btnSilver.Visible = false;
            this.btnGold.Visible = false;
            this.btnBronze.Visible = false;
            */
            this.btnGeorge.Location = new Point(0x2a, 0x13);
            this.btnGeorge.Name = "btnGeorge";
            this.btnGeorge.Size = new Size(0x1f, 0x1c);
            this.btnGeorge.TabIndex = 0x31;
            this.btnGeorge.Text = "G";
            this.btnGeorge.UseVisualStyleBackColor = true;
            this.btnGeorge.Click += new EventHandler(this.PolisherSelect);
            this.btnHenry.Location = new Point(0x4f, 0x13);
            this.btnHenry.Name = "btnHenry";
            this.btnHenry.Size = new Size(0x1f, 0x1c);
            this.btnHenry.TabIndex = 50;
            this.btnHenry.Text = "H";
            this.btnHenry.UseVisualStyleBackColor = true;
            this.btnHenry.Click += new EventHandler(this.PolisherSelect);
            this.btnRakesh.Location = new Point(0x74, 0x13);
            this.btnRakesh.Name = "btnRakesh";
            this.btnRakesh.Size = new Size(0x1f, 0x1c);
            this.btnRakesh.TabIndex = 0x33;
            this.btnRakesh.Text = "R";
            this.btnRakesh.UseVisualStyleBackColor = true;
            this.btnRakesh.Click += new EventHandler(this.PolisherSelect);
            this.btnBritt.Location = new Point(0x99, 0x13);
            this.btnBritt.Name = "btnBritt";
            this.btnBritt.Size = new Size(0x1f, 0x1c);
            this.btnBritt.TabIndex = 0x34;
            this.btnBritt.Text = "B";
            this.btnBritt.UseVisualStyleBackColor = true;
            this.btnBritt.Click += new EventHandler(this.PolisherSelect);
            this.datagrid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.datagrid.Location = new Point(13, -23);
            this.datagrid.Name = "datagrid";
            this.datagrid.Size = new Size(0x9f, 0x25);
            this.datagrid.TabIndex = 0x36;
            this.datagrid.Visible = false;
            this.btnExit.Font = new Font("Arial", 13f, FontStyle.Bold);
            this.btnExit.Location = new Point(12, 0x188);
            this.btnExit.Name = "btnExit";
            this.btnExit.Size = new Size(0xa2, 0x2a);
            this.btnExit.TabIndex = 0x37;
            this.btnExit.Text = "Exit";
            this.btnExit.UseVisualStyleBackColor = true;
            this.btnExit.Click += new EventHandler(this.btnExit_Click);

            this.btnCam1.Font = new Font("Arial", 13f, FontStyle.Regular);
            this.btnCam1.Location = new Point(0x343+ 80, 0xc2);
            this.btnCam1.Name = "btnCam1";
            this.btnCam1.Size = new Size(0xb1 - 40, 50);
            this.btnCam1.TabIndex = 0x38;
            this.btnCam1.Text = "Snap Cam1";
            this.btnCam1.UseVisualStyleBackColor = true;
            this.btnCam1.Click += new EventHandler(this.btnCam1_Click);

            this.btnCam2.Font = new Font("Arial", 13f, FontStyle.Regular);
            this.btnCam2.Location = new Point(0x343+ 80, 0xc2 + 50);
            this.btnCam2.Name = "btnCam2";
            this.btnCam2.Size = new Size(0xb1 - 40, 50);
            this.btnCam2.TabIndex = 0x38;
            this.btnCam2.Text = "Snap Cam2";
            this.btnCam2.UseVisualStyleBackColor = true;
            this.btnCam2.Click += new EventHandler(this.btnCam2_Click);

            this.btnSave.Font = new Font("Arial", 13f, FontStyle.Bold);
            this.btnSave.Location = new Point(0x259, 0xc2 + 30);//new Point(0x343, 0xc2);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new Size(0xb1, 75); //new Size(0xb1, 100);
            this.btnSave.TabIndex = 0x38;
            this.btnSave.Text = "Save Job";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new EventHandler(this.btnSave_Click);
            this.btnEmail.Font = new Font("Arial", 9f);
            this.btnEmail.Location = new Point(0x25f, 0x48);
            this.btnEmail.Name = "btnEmail";
            this.btnEmail.Size = new Size(0x33, 0x17 + 5);
            this.btnEmail.TabIndex = 0x39;
            this.btnEmail.Text = "email";
            this.btnEmail.UseVisualStyleBackColor = true;
            this.btnEmail.Click += new EventHandler(this.btnEmail_Click);
            this.btnPrintCustomerCopy.Font = new Font("Arial", 13f, FontStyle.Bold);
            this.btnPrintCustomerCopy.Location = new Point(12, 0x23d);
            this.btnPrintCustomerCopy.Name = "btnPrintCustomerCopy";
            this.btnPrintCustomerCopy.Size = new Size(0xa1, 0x2a);
            this.btnPrintCustomerCopy.TabIndex = 0x3a;
            this.btnPrintCustomerCopy.Text = "Print Customer";
            this.btnPrintCustomerCopy.UseVisualStyleBackColor = true;
            this.btnPrintCustomerCopy.Click += new EventHandler(this.btnPrintCustomerCopy_Click);
            this.btnPrintBusiness.Font = new Font("Arial", 13f, FontStyle.Bold);
            this.btnPrintBusiness.Location = new Point(12, 0x26d);
            this.btnPrintBusiness.Name = "btnPrintBusiness";
            this.btnPrintBusiness.Size = new Size(0xa1, 0x2a);
            this.btnPrintBusiness.TabIndex = 0x3b;
            this.btnPrintBusiness.Text = "Print ACP Copy";
            this.btnPrintBusiness.UseVisualStyleBackColor = true;
            this.btnPrintBusiness.Click += new EventHandler(this.btnPrintBusiness_Click);
            this.jobCompleted.AutoSize = true;
            this.jobCompleted.Font = new Font("Arial", 12f);
            this.jobCompleted.Location = new Point(12, 0x21f);
            this.jobCompleted.Name = "jobCompleted";
            this.jobCompleted.RightToLeft = RightToLeft.Yes;
            this.jobCompleted.Size = new Size(0x97, 0x16);
            this.jobCompleted.TabIndex = 60;
            this.jobCompleted.Text = "Duplicate Receipt";
            this.jobCompleted.UseVisualStyleBackColor = true;
            this.jobCompleted.CheckedChanged += new EventHandler(this.jobCompleted_CheckedChanged);

            this.fastPrint.AutoSize = true;
            this.fastPrint.Font = new Font("Arial", 12f);
            this.fastPrint.Location = new Point(12, 0x2cd + 0x2a + 10);
            this.fastPrint.Name = "fastPrint";
            this.fastPrint.RightToLeft = RightToLeft.Yes;
            this.fastPrint.Size = new Size(0x97, 0x16);
            this.fastPrint.TabIndex = 60;
            this.fastPrint.Text = "Fast Print";
            this.fastPrint.UseVisualStyleBackColor = true;
            this.fastPrint.Checked = true;


            this.panelSearchField.BackColor = Color.FromArgb(0xc0, 0xc0, 0xff);
            this.panelSearchField.Controls.Add(this.lblResults);
            this.panelSearchField.Controls.Add(this.slider);
            this.panelSearchField.Controls.Add(this.btnCancelSearch);
            this.panelSearchField.Controls.Add(this.btnSearchField);
            this.panelSearchField.Controls.Add(this.txtSearchField);
            this.panelSearchField.Controls.Add(this.lblSearchOnField);
            this.panelSearchField.Location = new Point(660, 0x1c4);
            this.panelSearchField.Name = "panelSearchField";
            this.panelSearchField.Size = new Size(0x1f0, 0xc3);
            this.panelSearchField.TabIndex = 0x3d;
            this.panelSearchField.Visible = false;
            this.panelSearchField.Paint += new PaintEventHandler(this.panelSearchField_Paint);
            this.panelSearchField.MouseDown += new MouseEventHandler(this.PanelMouseDown);
            this.panelSearchField.MouseMove += new MouseEventHandler(this.panelSearchField_MouseMove);
            this.panelSearchField.MouseUp += new MouseEventHandler(this.PanelMouseUp);
            this.lblResults.AutoSize = true;
            this.lblResults.Font = new Font("Arial", 14f);
            this.lblResults.Location = new Point(0x88, 0xa9);
            this.lblResults.Name = "lblResults";
            this.lblResults.Size = new Size(0, 0x16);
            this.lblResults.TabIndex = 6;
            this.slider.Location = new Point(0x22, 0x79);
            this.slider.Name = "slider";
            this.slider.Size = new Size(0x1ac, 0x2d);
            this.slider.TabIndex = 5;
            this.slider.Visible = false;
            this.slider.Scroll += new EventHandler(this.slider_Scroll);
            this.btnCancelSearch.Font = new Font("Arial", 14f);
            this.btnCancelSearch.Location = new Point(340, 0x52);
            this.btnCancelSearch.Name = "btnCancelSearch";
            this.btnCancelSearch.Size = new Size(0x73, 0x21);
            this.btnCancelSearch.TabIndex = 4;
            this.btnCancelSearch.Text = "Cancel";
            this.btnCancelSearch.UseVisualStyleBackColor = true;
            this.btnCancelSearch.Click += new EventHandler(this.btnCancelSearch_Click);
            this.btnSearchField.Font = new Font("Arial", 14f);
            this.btnSearchField.Location = new Point(0x2a, 0x52);
            this.btnSearchField.Name = "btnSearchField";
            this.btnSearchField.Size = new Size(0x73, 0x21);
            this.btnSearchField.TabIndex = 3;
            this.btnSearchField.Text = "Search";
            this.btnSearchField.UseVisualStyleBackColor = true;
            this.btnSearchField.Click += new EventHandler(this.btnSearchField_Click);
            this.txtSearchField.Font = new Font("Arial", 14f);
            this.txtSearchField.Location = new Point(0x2a, 0x29);
            this.txtSearchField.Name = "txtSearchField";
            this.txtSearchField.Size = new Size(0x19d, 0x1d);
            this.txtSearchField.TabIndex = 2;
            this.txtSearchField.TextChanged += new EventHandler(this.txtSearchField_TextChanged);
            this.lblSearchOnField.AutoSize = true;
            this.lblSearchOnField.Font = new Font("Arial", 15f);
            this.lblSearchOnField.Location = new Point(0x9f, 15);
            this.lblSearchOnField.Name = "lblSearchOnField";
            this.lblSearchOnField.Size = new Size(0x8d, 0x17);
            this.lblSearchOnField.TabIndex = 0;
            this.lblSearchOnField.Text = "Search on field";
            this.btnLatestJob.Font = new Font("Arial", 12f, FontStyle.Bold);
            this.btnLatestJob.Location = new Point(12, 250);
            this.btnLatestJob.Name = "btnLatestJob";
            this.btnLatestJob.Size = new Size(0xa2, 0x2a);
            this.btnLatestJob.TabIndex = 0x3e;
            this.btnLatestJob.Text = "Latest Job";
            this.btnLatestJob.UseVisualStyleBackColor = true;
            this.btnLatestJob.Click += new EventHandler(this.btnLatestJob_Click);
            this.btnNextPhoto.Font = new Font("Arial", 10f);
            this.btnNextPhoto.Location = new Point(990, 3);
            this.btnNextPhoto.Name = "btnNextPhoto";
            this.btnNextPhoto.Size = new Size(0x36, 0x92);
            this.btnNextPhoto.TabIndex = 0x3f;
            this.btnNextPhoto.Text = "Next Photo";
            this.btnNextPhoto.UseVisualStyleBackColor = true;
            this.btnNextPhoto.Click += new EventHandler(this.btnNextPhoto_Click);
            this.btnPrintForWork.Font = new Font("Arial", 13f, FontStyle.Bold);
            this.btnPrintForWork.Location = new Point(12, 0x29d);
            this.btnPrintForWork.Name = "btnPrintForWork";
            this.btnPrintForWork.Size = new Size(0xa1, 0x2a);
            this.btnPrintForWork.TabIndex = 0x40;
            this.btnPrintForWork.Text = "Print for Work";
            this.btnPrintForWork.UseVisualStyleBackColor = true;
            this.btnPrintForWork.Click += new EventHandler(this.btnPrintForWork_Click);
            this.btnLockUnlock.Font = new Font("Arial", 13f, FontStyle.Bold);
            this.btnLockUnlock.Location = new Point(12, 0x1e8);
            this.btnLockUnlock.Name = "btnLockUnlock";
            this.btnLockUnlock.Size = new Size(0xa2, 0x2a);
            this.btnLockUnlock.TabIndex = 0x41;
            this.btnLockUnlock.Text = "Lock";
            this.btnLockUnlock.UseVisualStyleBackColor = true;
            this.btnLockUnlock.Click += new EventHandler(this.btnLockUnlock_Click);
            this.btnUndo.Font = new Font("Arial", 13f, FontStyle.Bold);
            this.btnUndo.Location = new Point(12, 440);
            this.btnUndo.Name = "btnUndo";
            this.btnUndo.Size = new Size(0xa2, 0x2a);
            this.btnUndo.TabIndex = 0x42;
            this.btnUndo.Text = "Undo";
            this.btnUndo.UseVisualStyleBackColor = true;
            this.btnUndo.Click += new EventHandler(this.btnUndo_Click);
            this.picPaid.Image = Resources.paid_stamp;
            this.picPaid.Location = new Point(0x377, 0x66);
            this.picPaid.Name = "picPaid";
            this.picPaid.Size = new Size(0x61, 0x33);
            this.picPaid.SizeMode = PictureBoxSizeMode.Zoom;
            this.picPaid.TabIndex = 0x26;
            this.picPaid.TabStop = false;
            this.picPaid.Visible = false;
            this.pictureBox1.BackColor = SystemColors.ActiveBorder;
            this.pictureBox1.Location = new Point(0x425, 3);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new Size(0x127, 290);
            this.pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 8;
            this.pictureBox1.TabStop = false;
            this.pictureBox1.Click += new EventHandler(this.pictureBox1_Click);
            this.btnPrintAll.Font = new Font("Arial", 13f, FontStyle.Bold);
            this.btnPrintAll.Location = new Point(13, 0x2cd);
            this.btnPrintAll.Name = "btnPrintAll";
            this.btnPrintAll.Size = new Size(0xa1, 0x2a);
            this.btnPrintAll.TabIndex = 0x43;
            this.btnPrintAll.Text = "Print 3 Copies";
            this.btnPrintAll.UseVisualStyleBackColor = true;
            this.btnPrintAll.Click += new EventHandler(this.btnPrintAll_Click);
            this.btnTodayForDateCompleted.Location = new Point(0x31e, 0x83);
            this.btnTodayForDateCompleted.Name = "btnTodayForDateCompleted";
            this.btnTodayForDateCompleted.Size = new Size(0x49, 0x16 + 5);
            this.btnTodayForDateCompleted.TabIndex = 0x44;
            this.btnTodayForDateCompleted.Text = "Today";
            this.btnTodayForDateCompleted.UseVisualStyleBackColor = true;
            this.btnTodayForDateCompleted.Click += new EventHandler(this.btnTodayForDateCompleted_Click);
            this.btnAddWeek.Font = new Font("Arial", 8f);
            this.btnAddWeek.Location = new Point(0x31f, 0x65);
            this.btnAddWeek.Name = "btnAddWeek";
            this.btnAddWeek.Size = new Size(0x48, 0x16 + 5);
            this.btnAddWeek.TabIndex = 0x45;
            this.btnAddWeek.Text = "+1 week";
            this.btnAddWeek.UseVisualStyleBackColor = true;
            this.btnAddWeek.Click += new EventHandler(this.btnAddWeek_Click);
            this.btnDuplicate.Font = new Font("Arial", 13f, FontStyle.Bold);
            this.btnDuplicate.Location = new Point(13, 5);
            this.btnDuplicate.Name = "btnDuplicate";
            this.btnDuplicate.Size = new Size(0xa2, 0x2a);
            this.btnDuplicate.TabIndex = 70;
            this.btnDuplicate.Text = "Duplicate Job";
            this.btnDuplicate.UseVisualStyleBackColor = true;
            this.btnDuplicate.Click += new EventHandler(this.btnDuplicate_Click);
            this.grpBoxPlating.Controls.Add(this.btnChrome);
            this.grpBoxPlating.Controls.Add(this.btnCopper);
            this.grpBoxPlating.Controls.Add(this.btnNickle);
            this.grpBoxPlating.Controls.Add(this.btnBrass);
            this.grpBoxPlating.Controls.Add(this.btnBronze);
            this.grpBoxPlating.Controls.Add(this.btnSilver);
            this.grpBoxPlating.Controls.Add(this.btnTin);
            this.grpBoxPlating.Controls.Add(this.btnSatin);
            this.grpBoxPlating.Controls.Add(this.btnGold);
            this.grpBoxPlating.Location = new Point(0x343, 0xc2);
            this.grpBoxPlating.Name = "grpBoxPlating";
            this.grpBoxPlating.Size = new Size(0xd1, 0x69);
            this.grpBoxPlating.TabIndex = 0x47;
            this.grpBoxPlating.TabStop = false;
            this.grpBoxPlating.Text = "Plating";
            this.grpBoxPlating.Visible = false;
            this.grpBoxPolish.Controls.Add(this.btnRakesh);
            this.grpBoxPolish.Controls.Add(this.btnGeorge);
            this.grpBoxPolish.Controls.Add(this.btnHenry);
            this.grpBoxPolish.Controls.Add(this.btnBritt);
            this.grpBoxPolish.Location = new Point(0x25c, 0xe5);
            this.grpBoxPolish.Name = "grpBoxPolish";
            this.grpBoxPolish.Size = new Size(0xd6, 0x45);
            this.grpBoxPolish.TabIndex = 0x48;
            this.grpBoxPolish.TabStop = false;
            this.grpBoxPolish.Text = "Polish";
            this.grpBoxPolish.Visible = false;
            this.btnCollapseToggle.Font = new Font("Microsoft Sans Serif", 10f, FontStyle.Bold, GraphicsUnit.Point, 0);
            this.btnCollapseToggle.Location = new Point(12, 0x134);
            this.btnCollapseToggle.Name = "btnCollapseToggle";
            this.btnCollapseToggle.Size = new Size(160, 0x27);
            this.btnCollapseToggle.TabIndex = 0x49;
            this.btnCollapseToggle.Text = "Collapse/Expand";
            this.btnCollapseToggle.UseVisualStyleBackColor = true;
            this.btnCollapseToggle.Click += new EventHandler(this.btnCollapseToggle_Click);
            this.cboReportStartMonth.DropDownHeight = 250;
            this.cboReportStartMonth.FormattingEnabled = true;
            this.cboReportStartMonth.IntegralHeight = false;
            this.cboReportStartMonth.Items.AddRange(new object[] { "01", "02", "03", "04", "05", "06", "07", "08", "09", "10", "11", "12" });
            this.cboReportStartMonth.Location = new Point(0xbd, 0x2f2);
            this.cboReportStartMonth.Name = "cboReportStartMonth";
            this.cboReportStartMonth.Size = new Size(0xa3, 0x15);
            this.cboReportStartMonth.TabIndex = 0x4a;
            this.cboReportStartMonth.Text = "<Select Report Start Month>";
            this.cboReportEndMonth.DropDownHeight = 250;
            this.cboReportEndMonth.FormattingEnabled = true;
            this.cboReportEndMonth.IntegralHeight = false;
            this.cboReportEndMonth.Items.AddRange(new object[] { "01", "02", "03", "04", "05", "06", "07", "08", "09", "10", "11", "12" });
            this.cboReportEndMonth.Location = new Point(0xbd, 0x30d);
            this.cboReportEndMonth.Name = "cboReportEndMonth";
            this.cboReportEndMonth.Size = new Size(0xa3, 0x15);
            this.cboReportEndMonth.TabIndex = 0x4b;
            this.cboReportEndMonth.Text = "<Select Report End Month>";
            this.cboReportYear.DropDownHeight = 250;
            this.cboReportYear.FormattingEnabled = true;
            this.cboReportYear.IntegralHeight = false;
            this.cboReportYear.Items.AddRange(new object[] { "2015", "2016", "2017", "2018", "2019", "2020" });
            this.cboReportYear.Location = new Point(0xbd, 0x328);
            this.cboReportYear.Name = "cboReportYear";
            this.cboReportYear.Size = new Size(0xa3, 0x15);
            this.cboReportYear.TabIndex = 0x4c;
            this.cboReportYear.Text = "<Select Report Year>";
            this.cboReportProduct.DropDownHeight = 250;
            this.cboReportProduct.FormattingEnabled = true;
            this.cboReportProduct.IntegralHeight = false;
            this.cboReportProduct.Items.AddRange(new object[] { 
                "Strip", "Repair", "Polish", "Laquer", "Copper", "Nickle", "Chrome", "Brass", "Bronze", "Tin", "Satin", "Silver", "Gold", "Tyre", "Small Crack", "Large Crack",
                "Small Dent", "Large Dent", "Machine", "Silver Galv", "Gold Galv", "Other"
            });
            this.cboReportProduct.Location = new Point(360, 0x2f2);
            this.cboReportProduct.Name = "cboReportProduct";
            this.cboReportProduct.Size = new Size(0xa3, 0x15);
            this.cboReportProduct.TabIndex = 0x4d;
            this.cboReportProduct.Text = "<Select Report Product>";
            this.cboReportProduct.SelectedIndexChanged += new EventHandler(this.cboReportProduct_SelectedIndexChanged);
            this.btnReport.Font = new Font("Arial", 13f, FontStyle.Bold);
            this.btnReport.Location = new Point(360, 0x30d);
            this.btnReport.Name = "btnReport";
            this.btnReport.Size = new Size(0xa3, 0x30);
            this.btnReport.TabIndex = 0x4e;
            this.btnReport.Text = "Report";
            this.btnReport.UseVisualStyleBackColor = true;
            this.btnReport.Click += new EventHandler(this.btnReport_Click);
            this.SuperSearchField.DropDownHeight = 250;
            this.SuperSearchField.DropDownStyle = ComboBoxStyle.Simple;
            this.SuperSearchField.FormattingEnabled = true;
            this.SuperSearchField.IntegralHeight = false;
            this.SuperSearchField.Location = new Point(0xbd, 0x2d7);
            this.SuperSearchField.Name = "SuperSearchField";
            this.SuperSearchField.Size = new Size(0x310, 0x15);
            this.SuperSearchField.TabIndex = 80;
            this.SuperSearchField.Text = "Enter super SQL search (advanced users only!)";
            this.SuperSearchField.KeyDown += new KeyEventHandler(this.OnSuperSearchEnterKey);
            this.AllowDrop = true;
            base.AutoScaleDimensions = new SizeF(96f, 96f);
            base.AutoScaleMode = AutoScaleMode.Dpi;
            base.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            base.ClientSize = new Size(0x558, 0x349);
            base.Controls.Add(this.SuperSearchField);
            base.Controls.Add(this.btnReport);
            base.Controls.Add(this.cboReportProduct);
            base.Controls.Add(this.cboReportYear);
            base.Controls.Add(this.cboReportEndMonth);
            base.Controls.Add(this.cboReportStartMonth);
            base.Controls.Add(this.btnCollapseToggle);
            base.Controls.Add(this.grpBoxPolish);
            base.Controls.Add(this.grpBoxPlating);
            base.Controls.Add(this.panelSearchField);
            base.Controls.Add(this.btnDuplicate);
            base.Controls.Add(this.btnAddWeek);
            base.Controls.Add(this.btnTodayForDateCompleted);
            base.Controls.Add(this.btnPrintAll);
            base.Controls.Add(this.btnUndo);
            base.Controls.Add(this.btnLockUnlock);
            base.Controls.Add(this.btnPrintForWork);
            base.Controls.Add(this.btnNextPhoto);
            base.Controls.Add(this.btnLatestJob);
            base.Controls.Add(this.jobCompleted);
            base.Controls.Add(this.btnPrintBusiness);
            base.Controls.Add(this.btnPrintCustomerCopy);
            base.Controls.Add(this.btnEmail);
            base.Controls.Add(this.btnSave);
            base.Controls.Add(this.btnCam1);
            base.Controls.Add(this.btnCam2);
            base.Controls.Add(this.btnExit);
            base.Controls.Add(this.datagrid);
            base.Controls.Add(this.picPaid);
            base.Controls.Add(this.fastPrint);
            base.Controls.Add(this.btnToday);
            base.Controls.Add(this.jobDatePaid);
            base.Controls.Add(this.label14);
            base.Controls.Add(this.jobNotes);
            base.Controls.Add(this.label13);
            base.Controls.Add(this.jobPaymentBy);
            base.Controls.Add(this.label12);
            base.Controls.Add(this.jobDateCompleted);
            base.Controls.Add(this.label11);
            base.Controls.Add(this.jobDateRequired);
            base.Controls.Add(this.label10);
            base.Controls.Add(this.jobReceivedFrom);
            base.Controls.Add(this.label9);
            base.Controls.Add(this.btnCourier);
            base.Controls.Add(this.btnCollect);
            base.Controls.Add(this.jobDelivery);
            base.Controls.Add(this.label8);
            base.Controls.Add(this.jobOrderNumber);
            base.Controls.Add(this.label7);
            base.Controls.Add(this.jobEmail);
            base.Controls.Add(this.label6);
            base.Controls.Add(this.jobPhone);
            base.Controls.Add(this.label5);
            base.Controls.Add(this.jobAddress);
            base.Controls.Add(this.label4);
            base.Controls.Add(this.jobCustomer);
            base.Controls.Add(this.label3);
            base.Controls.Add(this.jobBusinessName);
            base.Controls.Add(this.labelJobBusinessName);
            base.Controls.Add(this.jobDate);
            base.Controls.Add(this.label2);
            base.Controls.Add(this.pictureBox1);
            base.Controls.Add(this.btnNavigateForward);
            base.Controls.Add(this.btnNavigateBack);
            base.Controls.Add(this.jobID);
            base.Controls.Add(this.label1);
            base.Controls.Add(this.btnUnpaidCustomers);
            base.Controls.Add(this.btnSearchLists);
            base.Controls.Add(this.btnIncompleteJobs);
            base.Controls.Add(this.btnNewJob);
            base.Icon = (Icon) manager.GetObject("$this.Icon");
            base.Name = "JobCard";
            this.Text = "JobCard";
            base.FormClosing += new FormClosingEventHandler(this.CheckBeforeQuit);
            base.ControlAdded += new ControlEventHandler(this.ControlAdd);
            base.DragDrop += new DragEventHandler(this.DoDragDrop);
            base.DragEnter += new DragEventHandler(this.DoDragEnter);
            base.Resize += new EventHandler(this.Form1_ResizeEnd);
            ((ISupportInitialize) this.datagrid).EndInit();
            this.panelSearchField.ResumeLayout(false);
            this.panelSearchField.PerformLayout();
            this.slider.EndInit();
            ((ISupportInitialize) this.picPaid).EndInit();
            ((ISupportInitialize) this.pictureBox1).EndInit();
            this.grpBoxPlating.ResumeLayout(false);
            this.grpBoxPolish.ResumeLayout(false);
            base.ResumeLayout(false);
            base.PerformLayout();
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

        private void DeleteJobClicked(object sender, EventArgs e)
        {
            MouseEventArgs me = (MouseEventArgs)e;
            if (me.Button == MouseButtons.Right)
            {
                if (MessageBox.Show("Are you sure you wish to delete this JOB?" + Environment.NewLine + "This cannot be undone", "Confirm Deletion", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation) == DialogResult.OK)
                {
                    if (MessageBox.Show("Are you REALLY REALLY REALLY sure you wish to delete this JOB?" + Environment.NewLine + "This cannot be undone", "Confirm Deletion", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation) == DialogResult.OK)
                    {
                        if (DataAccess.Update("DELETE FROM jobs WHERE jobID=" + this.jobID.Text))
                        {
                            GetPreviousJob();
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
                TextBox box = (TextBox) sender;
                JobTypePopup.jobType = box;

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
                    
                    JobCard.popup.Show();
                    
                }
            }
        }

        private void Load(int selectedRow = 0)
        {
            this.Loading = true;
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
                        toSet = ((float) obj2).ToString("F2");
                    }
                    if ((obj2 is int) || (obj2 is long))
                    {
                        toSet = ((int) obj2).ToString();
                    }
                    else if (obj2 is DateTime)
                    {
                        if (name == "jobDate")
                        {
                            this.jobDateValForPhoto = (DateTime) obj2;
                            this.UpdatePhotos();
                        }
                        toSet = ((DateTime) obj2).ToString("d/M/yy");
                    }
                    else if (obj2 is bool)
                    {
                        flag3 = (bool) obj2;
                        toSet = flag3.ToString();
                    }
                    else if (obj2 is string)
                    {
                        toSet = (string) obj2;
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
                            CheckBoxComboBox c = (CheckBoxComboBox) control;
                            c.BackColor = whiteSmoke;
                            this.JobCard_DropDown(c, toSet);
                        }
                        else if (flag4)
                        {
                            TextBox box2 = (TextBox) control;
                            box2.BackColor = whiteSmoke;
                            box2.Text = toSet;
                        }
                        else if (flag5)
                        {
                            ((Label) control).Text = toSet;
                        }
                        else if (flag6)
                        {
                            CheckBox item = (CheckBox) control;
                            item.BackColor = whiteSmoke;
                            item.Checked = flag3;
                            if (name != "jobCompleted")
                            {
                                flag2 |= flag3;
                                item.Enabled = false;
                                list.Add(item);
                            }
                        }
                        else if (flag7)
                        {
                            ComboBox box4 = (ComboBox) control;
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
                DisclaimerNote();
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
                    button = (Button) sender;
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

        private void UpdateAllTotals()
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
                num2 = Math.Round((double)(num2 * 1.02), 2, MidpointRounding.AwayFromZero);
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
        private bool NeedSave(bool promptIfChanged = true, bool fromSaveButton = false)
        {
            bool flag = true;
            int num = 0;
            if (this.NeedCompulsory(fromSaveButton))
            {
                //return true;
            }
            this.updateSql = "UPDATE jobs SET ";
            this.UpdateAllTotals();
            foreach (Control control in this.fieldNameToControlMapping.Values)
            {
                string name = control.Name;
                string stringValue = "";
                control.DoubleClick += new EventHandler(this.SingleSearch);
                if (this.ControlValueChangedFromLoaded(control, false, out stringValue))
                {
                    flag = false;
                    System.Type type = this.types[name];
                    bool flag2 = type == typeof(DateTime);
                    string str3 = (type == typeof(string)) ? "'" : "";
                    if (string.IsNullOrWhiteSpace(stringValue))
                    {
                        stringValue = "null";
                        str3 = "";
                    }
                    else
                    {
                        DateTime time;
                        if (flag2 && JobQueryForm.ParsedDateOK(stringValue, out time))
                        {
                            stringValue = "#" + time.ToString("MM/dd/yyyy") + "#";
                        }
                    }
                    string updateSql = this.updateSql;
                    this.updateSql = updateSql + ((num > 0) ? "," : "") + name + "=" + str3 + DoubleQuote(stringValue) + str3;
                    num++;
                }
            }
            this.updateSql = this.updateSql + " WHERE jobID=" + this.jobID.Text;
            if (!flag && promptIfChanged)
            {
                flag = DataAccess.Update(this.updateSql);
                /*
                switch (MessageBox.Show("This operation will cause you to LOSE UNSAVED DATA!" + Environment.NewLine + "Do you wish to Save first?" + Environment.NewLine + "Yes - to save" + Environment.NewLine + "No - to lose data and continue" + Environment.NewLine + "Cancel - to cancel operation", "Save First?", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Hand))
                {
                    case DialogResult.No:
                        flag = true;
                        break;

                    case DialogResult.Yes:
                        flag = DataAccess.Update(this.updateSql);
                        break;
                }
                */
            }
            return !flag;
        }

        private void OnSuperSearchEnterKey(object sender, KeyEventArgs e)
        {
            if ((e.KeyCode == Keys.Enter) && this.SuperSearchField.Text.StartsWith("PJC"))
            {
                string cmd = this.SuperSearchField.Text.Substring(3);
                DataAccess.Update(cmd);
            }
            if ((!this.panelSearchField.Visible && (e.KeyCode == Keys.Enter)) && !this.NeedSave(true, false))
            {
                if (this.panelSetLocation)
                {
                    this.panelSearchField.Location = this.panelFinalLocation;
                }
                else
                {
                    this.panelSearchField.Location = new Point((int) (((float) base.Width) / 3f), (int) (((float) base.Height) / 2.5f));
                }
                this.txtSearchField.Text = this.SuperSearchField.Text;
                this.searchFieldName = "";
                this.slider.Visible = false;
                this.slider.Value = 0;
                this.slider.Maximum = 0;
                this.lblResults.Text = "";
                this.lblSearchOnField.Text = "SUPER SEARCH ON SQL";
                this.txtSearchField.Focus();
                this.btnSearchField.Visible = false;
                this.panelSearchField.Visible = true;
                this.txtSearchField.Enabled = false;
                this.SearchSQL(this.SuperSearchField.Text);
            }
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
                            currentPictureIndex = JobCard.currentPhotoPaths.Count-1;
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
                    button = (Button) sender;
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
            CustomerCopy.autoPrint = fastPrint.Checked;
            CustomerCopy copy = new CustomerCopy {
                OnPrintPressed = new CustomerCopy.PrintHandler(this.PrintPressed)
            };
            copy.Height = (int) (copy.Width * Math.Sqrt(2.0));
            RichTextBox r = copy.richTextBox1;
            this.AddLine(r, "");
            if (this.pictureBox1.Image != null)
            {
                Clipboard.SetImage(resizeImage(this.pictureBox1.Image, new Size((int) (copy.Width * 0.8f), (int) (copy.Height * 0.25f))));
                r.Paste();
            }
            this.AddLine(r, this.jobID.Text, "Arial", 100, FontStyle.Bold, 0);
            this.AddLine(r, "Job Date: " + this.jobDate.Text.PadLeft(10) + "Order Number: ".PadLeft(40) + this.jobOrderNumber.Text, "Courier New", 0x10, FontStyle.Regular, 0);
            this.AddLine(r, "Business/Customer:", FontStyle.Bold);
            this.AddLine(r, "Business/Customer:" + this.jobBusinessName+"/"+this.jobCustomer.Text.PadRight(0x23) + " Ph:" + this.jobPhone.Text, FontStyle.Regular);
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

        private void PrintPressed()
        {
            DataAccess.Update("UPDATE jobs SET jobCompleted=true WHERE jobID=" + this.jobID.Text);
        }

        private void PromptDatabasePath()
        {
            DialogResult result = MessageBox.Show("Initial Setup requires you to point to the jobCard database (jobCard.mdb)" + Environment.NewLine + " Would you like to auto search for it (will take time to search your entire computer), or would you rather search manually?)" + Environment.NewLine + "Yes - to auto search" + Environment.NewLine + "No - to manual search (via dialog)", "Find JobCard.mdb", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            string path = "";
            if (result == DialogResult.Yes)
            {
                path = this.SearchFile(@"C:\Users", "jobCard.mdb");
            }
            if (!System.IO.File.Exists(path))
            {
                OpenFileDialog dialog = new OpenFileDialog {
                    InitialDirectory = @"c:\",
                    Filter = "MS Access database files (*.mdb)|*.mdb",
                    FilterIndex = 1,
                    RestoreDirectory = true,
                    CheckFileExists = true
                };
                while (!System.IO.File.Exists(path))
                {
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        path = dialog.FileName;
                    }
                    if (!System.IO.File.Exists(path))
                    {
                        MessageBox.Show("File: '" + path + "' is invalid!" + Environment.NewLine + "YOU MUST COMPLETE THIS FIRST STEP OF POINTING TO jobCard.mdb");
                    }
                }
            }
            Settings.Default.JobCardDatabasePath = path;
            Settings.Default.Save();
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
                
                int num4 = this.btnPrintAll.Bottom - this.btnCollapseToggle.Top;
                int num5 = list[0];
                int num6 = 3;
                int height = ((int) (((float) num4) / ((float) list.Count))) - num6;
                float emSize = Math.Min((float) 11f, (float) (0.6111111f * height));
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
                    this.jobDetail[index].Size = new Size((int) (num3 * 0.4), height);
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
                    this.jobPrice[index].Size = new Size((int) (num3 * 0.07), this.jobDetail[index].Height);
                    this.jobPrice[index].Location = new Point(this.pictureBox1.Right - this.jobPrice[index].Width, this.jobDetail[index].Location.Y);
                    this.jobPrice[index].TabIndex = 0x2a + (num * 3);
                    this.jobUnitPrice[index].Font = new Font("Arial", emSize);
                    this.jobUnitPrice[index].Size = new Size((int) (num3 * 0.06), this.jobDetail[index].Height);
                    this.jobUnitPrice[index].Location = new Point((this.jobPrice[index].Left - num2) - this.jobUnitPrice[index].Width, this.jobDetail[index].Location.Y);
                    this.jobQty[index].Font = new Font("Arial", emSize);
                    this.jobQty[index].Size = new Size((int) (num3 * 0.04), this.jobDetail[index].Height);
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
            num4 = ((float) size.Width) / ((float) width);
            num5 = ((float) size.Height) / ((float) height);
            if (num5 < num4)
            {
                num3 = num5;
            }
            else
            {
                num3 = num4;
            }
            int num6 = (int) (width * num3);
            int num7 = (int) (height * num3);
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

        private void SaveWebCamPhoto()
        {
            List<System.Drawing.Image> images = Job_Card.Form1.selectedImages;
            if (images.Count > 0)
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
                    businessName = this.jobBusinessName.Text+"-";
                }
                string str7 = (this.jobID.Text + " " + businessName+this.jobCustomer.Text + " " + (string.IsNullOrWhiteSpace(this.jobPhone.Text) ? "" : (this.jobPhone.Text + " ")) + str6 + str5 + str2).Replace('<', '-').Replace('>', '-').Replace(':', '-').Replace('"', '-').Replace('/', '-').Replace('\\', '-').Replace('|', '-').Replace('?', '-').Replace('*', '-');
                string destFileName = path + @"\" + str7;
                System.IO.File.Copy(sourcePath, destFileName);
            }
        }

        private void Search()
        {
            if (this.types.ContainsKey(this.searchFieldName))
            {
                string str2;
                string sql = "";
                System.Type type = this.types[this.searchFieldName];
                if (type == typeof(DateTime))
                {
                    DateTime time;
                    if (this.CheckDate(this.txtSearchField.Text, out time))
                    {
                        str2 = sql;
                        sql = str2 + this.searchFieldName + "=#" + time.ToString("MM/dd/yyyy") + "#";
                    }
                }
                else if (type == typeof(float))
                {
                    float result = 0f;
                    if (float.TryParse(this.txtSearchField.Text, out result))
                    {
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
                    sql = str2 + this.searchFieldName + " LIKE '%" + this.txtSearchField.Text + "%'";
                }
                else if (type == typeof(int))
                {
                    int num2 = 0;
                    if (int.TryParse(this.txtSearchField.Text, out num2))
                    {
                        sql = sql + this.searchFieldName + "=" + this.txtSearchField.Text;
                    }
                    else
                    {
                        MessageBox.Show("You must only have digits in this field");
                    }
                }
                if (sql != "")
                {
                    sql = "Select * from jobs WHERE " + sql + " order by jobDate desc";
                    this.SearchSQL(sql);
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

        private bool SendMail(string mailTo, string csSubject, string csBody, string attachment)
        {
            MailAddress from = new MailAddress("team@plating.co.nz", "Advanced Chrome Platers");
            MailAddress to = new MailAddress(mailTo);
            MailMessage message = new MailMessage(from, to) {
                Subject = csSubject,
                Body = csBody,
                IsBodyHtml = false,
                DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure,
                ReplyTo = from
            };
            if (!string.IsNullOrWhiteSpace(attachment))
            {
                message.Attachments.Add(new Attachment(attachment));
            }

            SmtpClient client = new SmtpClient("mail.1stdomains.co.nz", 587) {
                Credentials = new NetworkCredential("team@plating.co.nz", "Sterling-03")
                //Credentials = CredentialCache.DefaultNetworkCredentials
            };
            client.EnableSsl = true;
            try
            {
                client.Send(message);
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

        private bool ShowPrintForm(bool customerCopy = true, bool isPrintAll = false, string printToPDF = null)
        {
            lastFontName = null;
            lastFontSize = -1;
            lastFontStyle = FontStyle.Regular;
            CustomerCopy.autoPrint = fastPrint.Checked;
            CustomerCopy copy = new CustomerCopy {
                OnPrintPressed = new CustomerCopy.PrintHandler(this.PrintPressed)
            };
            copy.Height = (int) (copy.Width * Math.Sqrt(2.0));
            RichTextBox r = copy.richTextBox1;
            this.AddLine(r, "");
            Resources.logo.MakeTransparent();
            if (customerCopy)
            {
                Clipboard.SetImage(Resources.logoHalfSize);
            }
            else if (this.pictureBox1.Image != null)
            {
                Clipboard.SetImage(resizeImage(this.pictureBox1.Image, new Size((int) (copy.Width * 0.8f), (int) (copy.Height * 0.25f))));
            }
            r.Paste();
            this.AddLine(r, this.jobID.Text, "Arial", 0x24, FontStyle.Bold, 0);
            this.AddLine(r, this.IsCompleted() ? "Tax Invoice  GST 83-712-147" : "Quotation/Job Card", null, 0x12, FontStyle.Regular, 0);
            this.AddLine(r, "");
            this.AddLine(r, "Job Date: " + this.jobDate.Text.PadLeft(10) + "Order Number: ".PadLeft(40) + this.jobOrderNumber.Text, "Courier New", 0x10, FontStyle.Regular, 0);
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
                    this.AddLine(r, "******************************", "Courier New", 15, FontStyle.Regular, 0);
                    this.AddLine(r, "*     DUPLICATE RECEIPT      *");
                    this.AddLine(r, "******************************");
                }
            }
            this.AddLine(r, "".PadRight(0x4a, '-'));
            this.AddLine(r, "Notes");
            this.AddLine(r, this.jobNotes.Text);
            this.AddLine(r, "".PadRight(0x4a, '-'));
            this.AddLine(r, "DISCLAIMER", "Arial", 15, FontStyle.Bold, 0);
            this.AddLine(r, Disclaimer, null, 10, FontStyle.Regular, 0);
            this.AddLine(r, "".PadRight(0x4a, '-'), "Courier New", 0x10, FontStyle.Regular, 0);
            if (customerCopy)
            {
                this.AddLine(r, "CUSTOMER COPY - " + (this.IsCompleted() ? " ** TAX INVOICE **" : "PRICING ABOVE AN ESTIMATE ONLY"), FontStyle.Bold);
            }
            else
            {
                this.AddLine(r, "Advanced Chrome Platers copy", FontStyle.Bold);
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

        private void SingleSearch(object sender, EventArgs e)
        {
            if (!this.NeedSave(true, false))
            {
                if (this.panelSetLocation)
                {
                    this.panelSearchField.Location = this.panelFinalLocation;
                }
                else
                {
                    this.panelSearchField.Location = new Point((int) (((float) base.Width) / 3f), (int) (((float) base.Height) / 2.5f));
                }
                this.txtSearchField.Text = "";
                this.searchFieldName = ((Control) sender).Name;
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

        private void tb_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                this.Search();
            }
        }

        private void TogglePaidStamp(object sender, EventArgs e)
        {
            if (sender is TextBox)
            {
                this.picPaid.Visible = ((TextBox) sender).Text.Length >= 8;
            }
        }

        private void updateCreditCardSurcharge()
        {
            if (this.incurCreditCardSurcharge())
            {
                this.label[this.subTotalIndex].Text = "Sub Total (+2% card surch)";
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
                application = (Microsoft.Office.Interop.Word.Application) Activator.CreateInstance(System.Type.GetTypeFromCLSID(new Guid("000209FF-0000-0000-C000-000000000046")));
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
    }
}

