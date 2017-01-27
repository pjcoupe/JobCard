namespace Job_Card
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Drawing;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;

    public class JobQueryForm : Form
    {
        private Button btnSearch;
        private ComboBox cboWhereClause;
        private static List<string> clauses;
        private string[] clausesStr = new string[] { 
            "ISNULL(jobDateCompleted) ORDER BY jobDate desc", "NOT ISNULL(jobDateCompleted) ORDER BY jobDate desc", "jobID=[1]", "jobDatePaid={1}", "jobDate={1}", "jobDate BETWEEN {1} AND {2} ORDER BY jobDate desc", "jobOrderNumber LIKE '%[1]%' ORDER BY jobDate desc", "jobEmail LIKE '%[1]%' ORDER BY jobDate desc", "jobPhone LIKE '%[1]%' ORDER BY jobDate desc", "jobCustomer LIKE '%[1]%' ORDER BY jobDate desc", "jobDetail00 LIKE '%[1]%' ORDER BY jobDate desc", "jobRepairText LIKE '%[1]%' ORDER BY jobDate desc", "jobStripText LIKE '%[1]%' ORDER BY jobDate desc", "jobPolishText LIKE '%[1]%' ORDER BY jobDate desc", "jobPlatingText LIKE '%[1]%' ORDER BY jobDate desc", "jobLaquerText LIKE '%[1]%' ORDER BY jobDate desc",
            "jobGalvaniseText LIKE '%[1]%' ORDER BY jobDate desc", "jobNotes LIKE '%[1]%' ORDER BY jobDate desc", "jobDelivery LIKE '%[1]%' ORDER BY jobDate desc", "jobTotal BETWEEN [1] AND [2] ORDER BY jobDate desc", "jobDateRequired BETWEEN {1} AND {1} ORDER BY jobDate desc", "jobDateCompleted BETWEEN {1} AND {2} ORDER BY jobDate desc", "jobDatePaid BETWEEN {1} AND {2} ORDER BY jobDate desc"
        };
        private IContainer components = null;
        private DataGridView dataGridView;
        private static string lastComboText = "";
        private static string lastData1Text = "";
        private static string lastData2Text = "";
        private static string lastWhereClause = "";
        private string[] listStr = new string[] { 
            "List all Incomplete jobs", "List all completed jobs", "Get job number in box1", "List all job Date Paid in box1", "List all jobDate in box1", "List jobDate from box1 to box2", "List all jobs like Order# in box1", "List all jobs like email in box1", "List all jobs like Phone in box1", "List all jobs like Customer in box1", "List all jobs like Detail00 in box1", "List all jobs like Repair in box1", "List all jobs like Strip in box1", "List all jobs like Polish in box1", "List all jobs like Plating in box1", "List all jobs like Laquer in box1",
            "List all jobs like Galvanise in box1", "List all jobs like Notes in box1", "List all jobs like Delivery in box1", "List total price from box1 to box2", "List date required from box1 to box2", "List date completed from box1 to box2", "List date paid from box1 to box2"
        };
        public static int selectedJobId = -1;
        private TextBox txtData1;
        private TextBox txtData2;

        public JobQueryForm()
        {
            selectedJobId = -1;
            this.InitializeComponent();
            this.dataGridView.AllowUserToAddRows = false;
            if (clauses == null)
            {
                clauses = new List<string>();
                for (int i = 0; i < this.clausesStr.Length; i++)
                {
                    clauses.Add(this.clausesStr[i]);
                }
            }
            this.cboWhereClause.Items.AddRange(this.listStr);
            this.cboWhereClause.Text = lastComboText;
            this.txtData1.Text = lastData1Text;
            this.txtData2.Text = lastData2Text;
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            if ((this.cboWhereClause.SelectedIndex == -1) && (this.cboWhereClause.Text != ""))
            {
                lastWhereClause = this.cboWhereClause.Text;
            }
            string sql = this.PassedChecks();
            if (sql != null)
            {
                try
                {
                    lastData1Text = this.txtData1.Text;
                    lastData2Text = this.txtData2.Text;
                    lastComboText = this.cboWhereClause.Text;
                    this.Search(sql);
                }
                catch (Exception exception)
                {
                    MessageBox.Show("Search fail " + exception.Message);
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
            this.cboWhereClause = new ComboBox();
            this.btnSearch = new Button();
            this.txtData1 = new TextBox();
            this.txtData2 = new TextBox();
            this.dataGridView = new DataGridView();
            ((ISupportInitialize) this.dataGridView).BeginInit();
            base.SuspendLayout();
            this.cboWhereClause.Font = new Font("Arial", 11f);
            this.cboWhereClause.FormattingEnabled = true;
            this.cboWhereClause.Location = new Point(5, 7);
            this.cboWhereClause.Name = "cboWhereClause";
            this.cboWhereClause.Size = new Size(0x173, 0x19);
            this.cboWhereClause.TabIndex = 0;
            this.cboWhereClause.SelectionChangeCommitted += new EventHandler(this.SelectedClause);
            this.btnSearch.Font = new Font("Arial", 11f, FontStyle.Bold);
            this.btnSearch.Location = new Point(0x340, 7);
            this.btnSearch.Name = "btnSearch";
            this.btnSearch.Size = new Size(0x43, 0x18);
            this.btnSearch.TabIndex = 1;
            this.btnSearch.Text = "Search";
            this.btnSearch.UseVisualStyleBackColor = true;
            this.btnSearch.Click += new EventHandler(this.btnSearch_Click);
            this.txtData1.Font = new Font("Arial", 11f);
            this.txtData1.Location = new Point(0x17e, 7);
            this.txtData1.Name = "txtData1";
            this.txtData1.Size = new Size(0xdb, 0x18);
            this.txtData1.TabIndex = 2;
            this.txtData2.Font = new Font("Arial", 11f);
            this.txtData2.Location = new Point(0x25f, 7);
            this.txtData2.Name = "txtData2";
            this.txtData2.Size = new Size(0xdb, 0x18);
            this.txtData2.TabIndex = 3;
            this.dataGridView.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Top;
            this.dataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;
            this.dataGridView.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.DisplayedCells;
            this.dataGridView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView.Location = new Point(6, 0x2c);
            this.dataGridView.MultiSelect = false;
            this.dataGridView.Name = "dataGridView";
            this.dataGridView.ReadOnly = true;
            this.dataGridView.Size = new Size(0x379, 0x210);
            this.dataGridView.TabIndex = 4;
            this.dataGridView.RowEnter += new DataGridViewCellEventHandler(this.RowSelected);
            base.AutoScaleDimensions = new SizeF(6f, 13f);
            base.AutoScaleMode = AutoScaleMode.Font;
            base.ClientSize = new Size(0x38b, 0x248);
            base.Controls.Add(this.dataGridView);
            base.Controls.Add(this.txtData2);
            base.Controls.Add(this.txtData1);
            base.Controls.Add(this.btnSearch);
            base.Controls.Add(this.cboWhereClause);
            base.Name = "JobQueryForm";
            this.Text = "JobQueryForm";
            ((ISupportInitialize) this.dataGridView).EndInit();
            base.ResumeLayout(false);
            base.PerformLayout();
        }

        public static bool ParsedDateOK(string dateText, out DateTime parsedDate)
        {
            string format = "d/M/yy";
            bool flag = DateTime.TryParseExact(dateText, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDate);
            if (!flag)
            {
                format = "d/M/yyyy";
                flag = DateTime.TryParseExact(dateText, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDate);
            }
            return flag;
        }

        private string PassedChecks()
        {
            string lastWhereClause = null;
            try
            {
                DateTime time;
                lastWhereClause = JobQueryForm.lastWhereClause;
                if (lastWhereClause.Contains("{1}"))
                {
                    if (!ParsedDateOK(this.txtData1.Text, out time))
                    {
                        MessageBox.Show("You must enter dd/mm/yy in box 1");
                        return null;
                    }
                    lastWhereClause = lastWhereClause.Replace("{1}", "#" + time.ToString("MM/dd/yyyy") + "#");
                }
                if (lastWhereClause.Contains("{2}"))
                {
                    if (!ParsedDateOK(this.txtData2.Text, out time))
                    {
                        MessageBox.Show("You must enter dd/mm/yy in box 2");
                        return null;
                    }
                    lastWhereClause = lastWhereClause.Replace("{2}", "#" + time.ToString("MM/dd/yyyy") + "#");
                }
                if (lastWhereClause.Contains("[1]"))
                {
                    if (string.IsNullOrWhiteSpace(this.txtData1.Text))
                    {
                        MessageBox.Show("You must enter search data in box 1");
                        return null;
                    }
                    lastWhereClause = lastWhereClause.Replace("[1]", JobCard.DoubleQuote(this.txtData1.Text));
                }
                if (lastWhereClause.Contains("[2]"))
                {
                    if (string.IsNullOrWhiteSpace(this.txtData2.Text))
                    {
                        MessageBox.Show("You must ALSO enter search data in box 2");
                        return null;
                    }
                    lastWhereClause = lastWhereClause.Replace("[2]", JobCard.DoubleQuote(this.txtData2.Text));
                }
                if (!lastWhereClause.ToUpper().StartsWith("SELECT"))
                {
                    lastWhereClause = "SELECT * FROM " + JobCard.DBTable + " WHERE " + lastWhereClause;
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show("Error " + exception.Message);
                return null;
            }
            return lastWhereClause;
        }

        private void RowSelected(object sender, DataGridViewCellEventArgs e)
        {
            int rowIndex = e.RowIndex;
            if ((rowIndex >= 0) && (this.dataGridView[0, rowIndex].OwningColumn.Name == "jobID"))
            {
                object obj2 = this.dataGridView[0, rowIndex].Value;
                if (obj2 != null)
                {
                    selectedJobId = (int) obj2;
                }
            }
        }

        public void Search(string sql)
        {
            try
            {
                DataAccess.ReadRecords(this.dataGridView, sql);
            }
            catch (Exception exception)
            {
                MessageBox.Show("Search fail " + exception.Message);
            }
        }

        private void SelectedClause(object sender, EventArgs e)
        {
            try
            {
                selectedJobId = -1;
                int selectedIndex = this.cboWhereClause.SelectedIndex;
                lastComboText = this.cboWhereClause.Text;
                if (selectedIndex >= 0)
                {
                    lastWhereClause = clauses[selectedIndex];
                    this.txtData1.Visible = lastWhereClause.Contains("[1]") || lastWhereClause.Contains("{1}");
                    this.txtData2.Visible = lastWhereClause.Contains("[2]") || lastWhereClause.Contains("{2}");
                }
                else
                {
                    this.txtData1.Visible = true;
                    this.txtData2.Visible = true;
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show("Error " + exception.Message);
            }
        }
    }
}

