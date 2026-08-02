namespace Job_Card
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Drawing;
    using System.Drawing.Printing;
    using System.Globalization;
    using System.Threading.Tasks;
    using System.Windows.Forms;

    public class JobQueryForm : Form
    {
        private Button btnSearch;
        private Button btnPrint;
        private ComboBox cboSearchType;
        private ComboBox cboDateRange;
        private Label lblSearchType;
        private Label lblDateRange;
        private Label lblHint;
        private IContainer components = null;
        private DataGridView dataGridView;
        private PrintDocument printDocument;
        private int printNextRowIndex;
        public static int selectedJobId = -1;
        private static int lastSearchTypeIndex;
        private static int lastDateRangeIndex = 1;
        private string _sortMongoField;
        private bool _sortDesc;
        private bool _listQueryMode;
        private int _listSkip;
        private Panel panelTop;
        private Panel panelFooter;
        private Button btnPrevPage;
        private Button btnNextPage;
        private Label lblResultCount;
        private const int PageSize = 50;
        private const int ListQueryColumnMinWidth = 50;
        private static HashSet<string> _jobDocSortFields;

        public JobQueryForm()
        {
            selectedJobId = -1;
            this._listQueryMode = true;
            this._listSkip = 0;
            this._sortMongoField = null;
            this._sortDesc = true;
            this.InitializeComponent();
            this.printDocument = new PrintDocument();
            this.printDocument.BeginPrint += this.PrintDocument_BeginPrint;
            this.printDocument.PrintPage += this.PrintDocument_PrintPage;
            this.dataGridView.AllowUserToAddRows = false;
            this.cboSearchType.Items.Clear();
            this.cboSearchType.Items.Add("Show unpaid (ALL)");
            this.cboSearchType.Items.Add("Show Collected but unpaid");
            this.cboSearchType.Items.Add("Show Unpaid Xero Customers");
            this.cboSearchType.Items.Add("Show overdue Xero Customers");
            this.cboSearchType.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cboDateRange.Items.Clear();
            this.cboDateRange.Items.AddRange(new object[]
            {
                "Last 7 days",
                "Last 30 days",
                "Last 90 days",
                "Last 6 months",
                "Last Year",
                "All Time"
            });
            this.cboDateRange.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cboSearchType.SelectedIndex = lastSearchTypeIndex >= 0 && lastSearchTypeIndex < this.cboSearchType.Items.Count
                ? lastSearchTypeIndex
                : 0;
            this.cboDateRange.SelectedIndex = lastDateRangeIndex >= 0 && lastDateRangeIndex < this.cboDateRange.Items.Count
                ? lastDateRangeIndex
                : 1;
            this.cboSearchType.SelectionChangeCommitted += this.ListFilter_ComboChanged;
            this.cboDateRange.SelectionChangeCommitted += this.ListFilter_ComboChanged;
            this.Load += this.JobQueryForm_Load;
        }

        private void ListFilter_ComboChanged(object sender, EventArgs e)
        {
            if (this._listQueryMode)
            {
                this._listSkip = 0;
            }
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            this.ApplyProgrammaticSortModeAndGlyphs();
            this.btnPrint.Enabled = this.dataGridView.RowCount > 0;
            if (!this._listQueryMode)
            {
                this.btnPrevPage.Enabled = false;
                this.btnNextPage.Enabled = false;
                this.lblResultCount.Text = string.Empty;
            }
        }

        private void JobQueryForm_Load(object sender, EventArgs e)
        {
            this.Load -= this.JobQueryForm_Load;
            if (this.dataGridView.DataSource == null)
            {
                this._listQueryMode = true;
                this.btnPrevPage.Enabled = false;
                this.btnNextPage.Enabled = false;
                this.btnPrint.Enabled = false;
                this.lblResultCount.Text = "Click Search to load results.";
                this.PositionResultCountLabel();
            }
            else
            {
                this._listQueryMode = false;
            }
        }

        private async Task RunSearchAsync()
        {
            if (!this._listQueryMode)
            {
                return;
            }
            long totalMatching = 0L;
            int rowsOnPage = 0;
            try
            {
                this.btnSearch.Enabled = false;
                this.btnPrevPage.Enabled = false;
                this.btnNextPage.Enabled = false;
                this.btnPrint.Enabled = false;
                selectedJobId = -1;
                int searchIndex = this.cboSearchType.SelectedIndex;
                int dateIndex = this.cboDateRange.SelectedIndex;
                if (searchIndex < 0)
                {
                    searchIndex = 0;
                }
                if (dateIndex < 0)
                {
                    dateIndex = 1;
                }
                lastSearchTypeIndex = searchIndex;
                lastDateRangeIndex = dateIndex;
                totalMatching = await DataAccess.CountJobsForListQueryAsync(searchIndex, dateIndex);
                if (totalMatching > 0L && this._listSkip >= totalMatching)
                {
                    this._listSkip = (int)Math.Max(0L, ((totalMatching - 1L) / PageSize) * PageSize);
                }
                if (totalMatching == 0L)
                {
                    this._listSkip = 0;
                }
                var page = await DataAccess.FindJobsForListQueryAsync(
                    this.dataGridView,
                    searchIndex,
                    dateIndex,
                    this._listSkip,
                    PageSize,
                    this._sortMongoField,
                    this._sortMongoField == null ? null : (bool?)this._sortDesc);
                rowsOnPage = page == null ? 0 : page.Count;
                this.ApplyProgrammaticSortModeAndGlyphs();
                this.UpdatePagingFooter(totalMatching, rowsOnPage);
                this.ApplyListQueryColumnFillLayout();
            }
            catch (Exception exception)
            {
                MessageBox.Show("Search failed: " + exception.Message);
            }
            finally
            {
                this.btnSearch.Enabled = true;
                this.btnPrevPage.Enabled = this._listQueryMode && this._listSkip > 0;
                this.btnNextPage.Enabled = this._listQueryMode && totalMatching > 0L && (this._listSkip + rowsOnPage < totalMatching);
                this.btnPrint.Enabled = this.dataGridView.RowCount > 0;
            }
        }

        private void UpdatePagingFooter(long totalMatching, int rowsOnPage)
        {
            if (!this._listQueryMode)
            {
                return;
            }
            if (totalMatching == 0L)
            {
                this.lblResultCount.Text = "0 jobs matching filter.";
            }
            else
            {
                int first = this._listSkip + 1;
                int last = this._listSkip + rowsOnPage;
                this.lblResultCount.Text = string.Format(
                    CultureInfo.CurrentCulture,
                    "{0:N0} job(s) matching filter. Showing {1:N0}–{2:N0}.",
                    totalMatching,
                    first,
                    last);
            }
            this.PositionResultCountLabel();
        }

        private void PositionResultCountLabel()
        {
            this.lblResultCount.Top = Math.Max(0, (this.panelFooter.ClientSize.Height - this.lblResultCount.Height) / 2);
            this.lblResultCount.Left = this.panelFooter.ClientSize.Width - this.lblResultCount.Width - 12;
        }

        private void PanelFooter_SizeChanged(object sender, EventArgs e)
        {
            this.PositionResultCountLabel();
        }

        private void ApplyListQueryColumnFillLayout()
        {
            if (!this._listQueryMode || this.dataGridView.Columns.Count == 0)
            {
                return;
            }
            this.dataGridView.SuspendLayout();
            try
            {
                float sumWeights = 0f;
                foreach (DataGridViewColumn col in this.dataGridView.Columns)
                {
                    col.MinimumWidth = ListQueryColumnMinWidth;
                    float w = Math.Max((float)ListQueryColumnMinWidth, (float)col.Width);
                    col.FillWeight = w;
                    sumWeights += w;
                }
                if (sumWeights <= 0f)
                {
                    float even = 100f;
                    foreach (DataGridViewColumn col in this.dataGridView.Columns)
                    {
                        col.FillWeight = even;
                    }
                }
                this.dataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            }
            finally
            {
                this.dataGridView.ResumeLayout(true);
            }
        }

        private async void btnSearch_Click(object sender, EventArgs e)
        {
            if (!this._listQueryMode)
            {
                return;
            }
            this._listSkip = 0;
            await this.RunSearchAsync();
        }

        private async void BtnPrevPage_Click(object sender, EventArgs e)
        {
            if (!this._listQueryMode)
            {
                return;
            }
            this._listSkip = Math.Max(0, this._listSkip - PageSize);
            await this.RunSearchAsync();
        }

        private async void BtnNextPage_Click(object sender, EventArgs e)
        {
            if (!this._listQueryMode)
            {
                return;
            }
            this._listSkip += PageSize;
            await this.RunSearchAsync();
        }

        private void btnPrint_Click(object sender, EventArgs e)
        {
            if (this.dataGridView.RowCount == 0)
            {
                MessageBox.Show(this, "Nothing to print.", "Print", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            this.printDocument.DefaultPageSettings.Landscape = true;
            this.printDocument.DefaultPageSettings.Margins = new Margins(0, 0, 0, 0);
            using (var preview = new PrintPreviewDialog())
            {
                preview.Document = this.printDocument;
                preview.WindowState = FormWindowState.Maximized;
                preview.ShowDialog(this);
            }
        }

        private void PrintDocument_BeginPrint(object sender, PrintEventArgs e)
        {
            this.printNextRowIndex = 0;
        }

        private void PrintDocument_PrintPage(object sender, PrintPageEventArgs e)
        {
            var g = e.Graphics;
            var margin = e.MarginBounds;
            if (this.dataGridView.RowCount == 0)
            {
                g.DrawString("No rows.", this.Font, Brushes.Black, margin.Left, margin.Top);
                e.HasMorePages = false;
                return;
            }
            var visibleCols = new List<DataGridViewColumn>();
            foreach (DataGridViewColumn c in this.dataGridView.Columns)
            {
                if (c.Visible)
                {
                    visibleCols.Add(c);
                }
            }
            if (visibleCols.Count == 0)
            {
                e.HasMorePages = false;
                return;
            }
            using (Font headerFont = new Font("Arial", 8f, FontStyle.Bold))
            using (Font cellFont = new Font("Arial", 7.5f))
            using (var headerBrush = new SolidBrush(Color.Black))
            using (var cellBrush = new SolidBrush(Color.Black))
            using (var gridPen = new Pen(Color.Gray, 0.5f))
            {
                float y = margin.Top;
                float lineHeight = headerFont.GetHeight(g) + 4f;
                float x = margin.Left;
                float totalWidth = margin.Width;
                var colWidths = new float[visibleCols.Count];
                float sum = 0f;
                for (int i = 0; i < visibleCols.Count; i++)
                {
                    string h = visibleCols[i].HeaderText;
                    if (string.IsNullOrEmpty(h))
                    {
                        h = visibleCols[i].Name;
                    }
                    float w = g.MeasureString(h, headerFont).Width + 12f;
                    if (w < 36f)
                    {
                        w = 36f;
                    }
                    if (w > 140f)
                    {
                        w = 140f;
                    }
                    colWidths[i] = w;
                    sum += w;
                }
                if (sum > totalWidth && sum > 0f)
                {
                    float scale = totalWidth / sum;
                    for (int i = 0; i < colWidths.Length; i++)
                    {
                        colWidths[i] *= scale;
                    }
                }
                x = margin.Left;
                for (int i = 0; i < visibleCols.Count; i++)
                {
                    var rc = new RectangleF(x, y, colWidths[i], lineHeight);
                    g.FillRectangle(Brushes.LightGray, rc);
                    g.DrawRectangle(gridPen, rc.X, rc.Y, rc.Width, rc.Height);
                    g.DrawString(visibleCols[i].HeaderText, headerFont, headerBrush, rc, new StringFormat { Trimming = StringTrimming.EllipsisCharacter, FormatFlags = StringFormatFlags.NoWrap });
                    x += colWidths[i];
                }
                y += lineHeight;
                float rowH = cellFont.GetHeight(g) + 3f;
                var fmt = new StringFormat { Trimming = StringTrimming.EllipsisCharacter, FormatFlags = StringFormatFlags.NoWrap };
                while (this.printNextRowIndex < this.dataGridView.Rows.Count)
                {
                    DataGridViewRow row = this.dataGridView.Rows[this.printNextRowIndex];
                    if (row.IsNewRow)
                    {
                        this.printNextRowIndex++;
                        continue;
                    }
                    if (y + rowH > margin.Bottom)
                    {
                        e.HasMorePages = true;
                        return;
                    }
                    x = margin.Left;
                    for (int i = 0; i < visibleCols.Count; i++)
                    {
                        object val = row.Cells[visibleCols[i].Index].Value;
                        string s = val == null || val == DBNull.Value ? "" : Convert.ToString(val, CultureInfo.CurrentCulture);
                        var cellRc = new RectangleF(x, y, colWidths[i], rowH);
                        g.DrawRectangle(gridPen, cellRc.X, cellRc.Y, cellRc.Width, cellRc.Height);
                        g.DrawString(s, cellFont, cellBrush, cellRc, fmt);
                        x += colWidths[i];
                    }
                    y += rowH;
                    this.printNextRowIndex++;
                }
            }
            e.HasMorePages = false;
        }

        private static HashSet<string> JobDocSortFields
        {
            get
            {
                if (_jobDocSortFields == null)
                {
                    _jobDocSortFields = DataAccess.GetJobCardDocSortableFieldNames();
                }
                return _jobDocSortFields;
            }
        }

        private static string ColumnNameToMongoField(string columnName)
        {
            if (string.Equals(columnName, "Id", StringComparison.Ordinal))
            {
                return "_id";
            }
            return columnName;
        }

        private static bool IsSortableMongoField(string mongoField)
        {
            return !string.IsNullOrEmpty(mongoField) && JobDocSortFields.Contains(mongoField);
        }

        private void ApplyProgrammaticSortModeAndGlyphs()
        {
            if (this.dataGridView.Columns.Count == 0)
            {
                return;
            }
            foreach (DataGridViewColumn col in this.dataGridView.Columns)
            {
                col.SortMode = DataGridViewColumnSortMode.Programmatic;
                string mongo = ColumnNameToMongoField(col.Name);
                if (string.IsNullOrEmpty(this._sortMongoField))
                {
                    col.HeaderCell.SortGlyphDirection = SortOrder.None;
                }
                else if (string.Equals(mongo, this._sortMongoField, StringComparison.Ordinal))
                {
                    col.HeaderCell.SortGlyphDirection = this._sortDesc ? SortOrder.Descending : SortOrder.Ascending;
                }
                else
                {
                    col.HeaderCell.SortGlyphDirection = SortOrder.None;
                }
            }
        }

        private async void DataGridView_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (!this._listQueryMode)
            {
                return;
            }
            if (e.Button != MouseButtons.Left || e.ColumnIndex < 0)
            {
                return;
            }
            DataGridViewColumn col = this.dataGridView.Columns[e.ColumnIndex];
            string mongoField = ColumnNameToMongoField(col.Name);
            if (!IsSortableMongoField(mongoField))
            {
                return;
            }
            if (string.Equals(this._sortMongoField, mongoField, StringComparison.Ordinal))
            {
                if (this._sortDesc)
                {
                    this._sortDesc = false;
                }
                else
                {
                    this._sortMongoField = null;
                }
            }
            else
            {
                this._sortMongoField = mongoField;
                this._sortDesc = true;
            }
            this._listSkip = 0;
            await this.RunSearchAsync();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.printDocument != null)
                {
                    this.printDocument.BeginPrint -= this.PrintDocument_BeginPrint;
                    this.printDocument.PrintPage -= this.PrintDocument_PrintPage;
                    this.printDocument.Dispose();
                    this.printDocument = null;
                }
                if (this.components != null)
                {
                    this.components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.panelTop = new Panel();
            this.panelFooter = new Panel();
            this.lblSearchType = new Label();
            this.cboSearchType = new ComboBox();
            this.lblDateRange = new Label();
            this.cboDateRange = new ComboBox();
            this.btnSearch = new Button();
            this.btnPrint = new Button();
            this.lblHint = new Label();
            this.btnPrevPage = new Button();
            this.btnNextPage = new Button();
            this.lblResultCount = new Label();
            this.dataGridView = new DataGridView();
            ((ISupportInitialize)this.dataGridView).BeginInit();
            base.SuspendLayout();
            this.panelTop.SuspendLayout();
            this.panelFooter.SuspendLayout();
            this.lblSearchType.AutoSize = true;
            this.lblSearchType.Font = new Font("Arial", 10f);
            this.lblSearchType.Location = new Point(12, 12);
            this.lblSearchType.Name = "lblSearchType";
            this.lblSearchType.Text = "Search";
            this.cboSearchType.Font = new Font("Arial", 10f);
            this.cboSearchType.FormattingEnabled = true;
            this.cboSearchType.Location = new Point(12, 32);
            this.cboSearchType.Name = "cboSearchType";
            this.cboSearchType.Size = new Size(260, 24);
            this.cboSearchType.TabIndex = 0;
            this.lblDateRange.AutoSize = true;
            this.lblDateRange.Font = new Font("Arial", 10f);
            this.lblDateRange.Location = new Point(288, 12);
            this.lblDateRange.Name = "lblDateRange";
            this.lblDateRange.Text = "Date range";
            this.cboDateRange.Font = new Font("Arial", 10f);
            this.cboDateRange.FormattingEnabled = true;
            this.cboDateRange.Location = new Point(288, 32);
            this.cboDateRange.Name = "cboDateRange";
            this.cboDateRange.Size = new Size(160, 24);
            this.cboDateRange.TabIndex = 1;
            this.btnSearch.Font = new Font("Arial", 10f, FontStyle.Bold);
            this.btnSearch.Location = new Point(462, 30);
            this.btnSearch.Name = "btnSearch";
            this.btnSearch.Size = new Size(88, 28);
            this.btnSearch.TabIndex = 2;
            this.btnSearch.Text = "Search";
            this.btnSearch.UseVisualStyleBackColor = true;
            this.btnSearch.Click += new EventHandler(this.btnSearch_Click);
            this.btnPrint.Font = new Font("Arial", 10f, FontStyle.Bold);
            this.btnPrint.Location = new Point(556, 30);
            this.btnPrint.Name = "btnPrint";
            this.btnPrint.Size = new Size(88, 28);
            this.btnPrint.TabIndex = 4;
            this.btnPrint.Text = "Print";
            this.btnPrint.UseVisualStyleBackColor = true;
            this.btnPrint.Enabled = false;
            this.btnPrint.Click += new EventHandler(this.btnPrint_Click);
            this.lblHint.Font = new Font("Arial", 9f, FontStyle.Italic);
            this.lblHint.ForeColor = SystemColors.GrayText;
            this.lblHint.Location = new Point(652, 8);
            this.lblHint.Name = "lblHint";
            this.lblHint.Size = new Size(268, 48);
            this.lblHint.Text = "Overdue Xero: unpaid, due date before today (from sent invoice / Xero sync). Header sort: desc, asc, off. Double-click row to open job.";
            this.panelTop.Controls.Add(this.lblHint);
            this.panelTop.Controls.Add(this.btnPrint);
            this.panelTop.Controls.Add(this.btnSearch);
            this.panelTop.Controls.Add(this.cboDateRange);
            this.panelTop.Controls.Add(this.lblDateRange);
            this.panelTop.Controls.Add(this.cboSearchType);
            this.panelTop.Controls.Add(this.lblSearchType);
            this.panelTop.Name = "panelTop";
            this.panelTop.Size = new Size(932, 72);
            this.panelTop.TabIndex = 0;
            this.btnPrevPage.Font = new Font("Arial", 9f);
            this.btnPrevPage.Location = new Point(12, 10);
            this.btnPrevPage.Name = "btnPrevPage";
            this.btnPrevPage.Size = new Size(100, 28);
            this.btnPrevPage.TabIndex = 0;
            this.btnPrevPage.Text = "Prev page";
            this.btnPrevPage.UseVisualStyleBackColor = true;
            this.btnPrevPage.Enabled = false;
            this.btnPrevPage.Click += new EventHandler(this.BtnPrevPage_Click);
            this.btnNextPage.Font = new Font("Arial", 9f);
            this.btnNextPage.Location = new Point(118, 10);
            this.btnNextPage.Name = "btnNextPage";
            this.btnNextPage.Size = new Size(100, 28);
            this.btnNextPage.TabIndex = 1;
            this.btnNextPage.Text = "Next page";
            this.btnNextPage.UseVisualStyleBackColor = true;
            this.btnNextPage.Enabled = false;
            this.btnNextPage.Click += new EventHandler(this.BtnNextPage_Click);
            this.lblResultCount.AutoSize = true;
            this.lblResultCount.Font = new Font("Arial", 9f);
            this.lblResultCount.Name = "lblResultCount";
            this.lblResultCount.TabIndex = 2;
            this.lblResultCount.Text = string.Empty;
            this.lblResultCount.TextAlign = ContentAlignment.MiddleRight;
            this.panelFooter.Controls.Add(this.lblResultCount);
            this.panelFooter.Controls.Add(this.btnNextPage);
            this.panelFooter.Controls.Add(this.btnPrevPage);
            this.panelFooter.Name = "panelFooter";
            this.panelFooter.Size = new Size(932, 46);
            this.panelFooter.TabIndex = 2;
            this.panelFooter.SizeChanged += new EventHandler(this.PanelFooter_SizeChanged);
            this.dataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            this.dataGridView.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
            this.dataGridView.RowTemplate.Height = 22;
            this.dataGridView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView.Dock = DockStyle.Fill;
            this.dataGridView.MultiSelect = false;
            this.dataGridView.Name = "dataGridView";
            this.dataGridView.ReadOnly = true;
            this.dataGridView.TabIndex = 1;
            this.dataGridView.RowEnter += new DataGridViewCellEventHandler(this.DataGridView_RowEnter);
            this.dataGridView.CellDoubleClick += new DataGridViewCellEventHandler(this.DataGridView_CellDoubleClick);
            this.dataGridView.ColumnHeaderMouseClick += new DataGridViewCellMouseEventHandler(this.DataGridView_ColumnHeaderMouseClick);
            base.AutoScaleDimensions = new SizeF(6f, 13f);
            base.AutoScaleMode = AutoScaleMode.Font;
            base.ClientSize = new Size(932, 560);
            TableLayoutPanel layoutRoot = new TableLayoutPanel();
            layoutRoot.Dock = DockStyle.Fill;
            layoutRoot.Margin = new Padding(0);
            layoutRoot.Padding = new Padding(0);
            layoutRoot.ColumnCount = 1;
            layoutRoot.RowCount = 3;
            layoutRoot.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            layoutRoot.RowStyles.Add(new RowStyle(SizeType.Absolute, 72f));
            layoutRoot.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            layoutRoot.RowStyles.Add(new RowStyle(SizeType.Absolute, 46f));
            this.panelTop.Dock = DockStyle.Fill;
            this.panelFooter.Dock = DockStyle.Fill;
            this.dataGridView.Dock = DockStyle.Fill;
            layoutRoot.Controls.Add(this.panelTop, 0, 0);
            layoutRoot.Controls.Add(this.dataGridView, 0, 1);
            layoutRoot.Controls.Add(this.panelFooter, 0, 2);
            base.Controls.Add(layoutRoot);
            base.Name = "JobQueryForm";
            this.Text = "Search job lists";
            this.panelTop.ResumeLayout(false);
            this.panelTop.PerformLayout();
            this.panelFooter.ResumeLayout(false);
            this.panelFooter.PerformLayout();
            ((ISupportInitialize)this.dataGridView).EndInit();
            base.ResumeLayout(false);
            this.PositionResultCountLabel();
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

        private void DataGridView_RowEnter(object sender, DataGridViewCellEventArgs e)
        {
            this.TrySelectJobIdFromRow(e.RowIndex);
        }

        private void DataGridView_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
            {
                return;
            }
            this.TrySelectJobIdFromRow(e.RowIndex);
            if (selectedJobId > -1)
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        private void TrySelectJobIdFromRow(int rowIndex)
        {
            if (rowIndex < 0 || rowIndex >= this.dataGridView.Rows.Count)
            {
                return;
            }
            int jobIdCol = -1;
            for (int c = 0; c < this.dataGridView.Columns.Count; c++)
            {
                if (this.dataGridView.Columns[c].Name == "jobID")
                {
                    jobIdCol = c;
                    break;
                }
            }
            if (jobIdCol < 0)
            {
                return;
            }
            object val = this.dataGridView[jobIdCol, rowIndex].Value;
            if (val == null || val == DBNull.Value)
            {
                return;
            }
            selectedJobId = Convert.ToInt32(val, CultureInfo.InvariantCulture);
        }

        public DataGridView getSearchDataGridView()
        {
            return this.dataGridView;
        }
    }
}
