namespace Job_Card
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Windows.Forms;
    using System.Windows.Forms.DataVisualization.Charting;

    public class Progress : Form
    {
        private Button button1;
        public Chart chart1;
        private IContainer components = null;
        public Label label1;
        public ProgressBar progressBar1;
        public RichTextBox richTextBox1;

        public Progress()
        {
            this.InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            base.Close();
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
            ChartArea item = new ChartArea();
            ChartArea area2 = new ChartArea();
            ChartArea area3 = new ChartArea();
            Legend legend = new Legend();
            Legend legend2 = new Legend();
            Legend legend3 = new Legend();
            Series series = new Series();
            DataPoint point = new DataPoint(0.0, 10.0);
            DataPoint point2 = new DataPoint(0.0, 3.0);
            Series series2 = new Series();
            DataPoint point3 = new DataPoint(0.0, 1.0);
            DataPoint point4 = new DataPoint(0.0, 1.0);
            DataPoint point5 = new DataPoint(0.0, 2.0);
            Series series3 = new Series();
            DataPoint point6 = new DataPoint(0.0, 1.0);
            DataPoint point7 = new DataPoint(0.0, 1.0);
            DataPoint point8 = new DataPoint(0.0, 2.0);
            this.progressBar1 = new ProgressBar();
            this.chart1 = new Chart();
            this.label1 = new Label();
            this.button1 = new Button();
            this.richTextBox1 = new RichTextBox();
            this.chart1.BeginInit();
            base.SuspendLayout();
            this.progressBar1.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Bottom;
            this.progressBar1.Location = new Point(12, 0x213);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new Size(0x2b7, 0x1b);
            this.progressBar1.TabIndex = 0;
            this.chart1.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Top;
            item.BackColor = Color.FromArgb(0xc0, 0xc0, 0xff);
            item.Name = "Creation";
            item.ShadowOffset = 3;
            area2.BackColor = Color.FromArgb(0xff, 0xff, 0xc0);
            area2.Name = "Completion";
            area2.ShadowOffset = 3;
            area3.BackColor = Color.FromArgb(0xc0, 0xff, 0xc0);
            area3.Name = "Paid";
            area3.ShadowOffset = 3;
            this.chart1.ChartAreas.Add(item);
            this.chart1.ChartAreas.Add(area2);
            this.chart1.ChartAreas.Add(area3);
            legend.DockedToChartArea = "Creation";
            legend.Docking = Docking.Left;
            legend.Name = "Creation Legend";
            legend.Title = "Creation";
            legend2.DockedToChartArea = "Completion";
            legend2.Docking = Docking.Left;
            legend2.Name = "Completion Legend";
            legend2.Title = "Completion";
            legend3.DockedToChartArea = "Paid";
            legend3.Docking = Docking.Left;
            legend3.Name = "Paid Legend";
            legend3.Title = "Paid";
            this.chart1.Legends.Add(legend);
            this.chart1.Legends.Add(legend2);
            this.chart1.Legends.Add(legend3);
            this.chart1.Location = new Point(12, 0x27);
            this.chart1.Name = "chart1";
            series.ChartArea = "Creation";
            series.ChartType = SeriesChartType.Pie;
            series.IsValueShownAsLabel = true;
            series.Legend = "Creation Legend";
            series.Name = "Creation";
            point.Label = "here";
            point.LabelAngle = 0x2d;
            point2.Label = "elsewhere";
            point2.LabelAngle = -45;
            series.Points.Add(point);
            series.Points.Add(point2);
            series2.ChartArea = "Completion";
            series2.ChartType = SeriesChartType.Pie;
            series2.IsValueShownAsLabel = true;
            series2.Legend = "Completion Legend";
            series2.Name = "Completion";
            point3.Label = "here";
            point3.LabelAngle = 0x2d;
            point4.Label = "Elsewhere";
            point4.LabelAngle = -45;
            point5.Label = "Not Completed";
            point5.LabelAngle = 0;
            series2.Points.Add(point3);
            series2.Points.Add(point4);
            series2.Points.Add(point5);
            series3.ChartArea = "Paid";
            series3.ChartType = SeriesChartType.Pie;
            series3.Legend = "Paid Legend";
            series3.Name = "Paid";
            point6.IsValueShownAsLabel = true;
            point6.Label = "here";
            point6.LabelAngle = 0x2d;
            point7.IsValueShownAsLabel = true;
            point7.Label = "elsewhere";
            point7.LabelAngle = -45;
            point8.IsValueShownAsLabel = true;
            point8.Label = "Not Paid";
            series3.Points.Add(point6);
            series3.Points.Add(point7);
            series3.Points.Add(point8);
            this.chart1.Series.Add(series);
            this.chart1.Series.Add(series2);
            this.chart1.Series.Add(series3);
            this.chart1.Size = new Size(0x1b4, 0x1d8);
            this.chart1.TabIndex = 1;
            this.chart1.Text = "chart1";
            this.label1.AutoSize = true;
            this.label1.Font = new Font("Microsoft Sans Serif", 10f, FontStyle.Regular, GraphicsUnit.Point, 0);
            this.label1.Location = new Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new Size(0x2e, 0x11);
            this.label1.TabIndex = 2;
            this.label1.Text = "label1";
            this.button1.Font = new Font("Microsoft Sans Serif", 12f, FontStyle.Regular, GraphicsUnit.Point, 0);
            this.button1.Location = new Point(0x2d7, 530);
            this.button1.Name = "button1";
            this.button1.Size = new Size(0x7d, 0x1b);
            this.button1.TabIndex = 3;
            this.button1.Text = "OK";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new EventHandler(this.button1_Click);
            this.richTextBox1.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Top;
            this.richTextBox1.Font = new Font("Microsoft Sans Serif", 11f, FontStyle.Regular, GraphicsUnit.Point, 0);
            this.richTextBox1.Location = new Point(12, 0x29);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.Size = new Size(840, 0x1d5);
            this.richTextBox1.TabIndex = 4;
            this.richTextBox1.Text = "";
            base.AutoScaleDimensions = new SizeF(6f, 13f);
            base.AutoScaleMode = AutoScaleMode.Font;
            this.BackColor = SystemColors.Highlight;
            base.ClientSize = new Size(0x361, 570);
            base.ControlBox = false;
            base.Controls.Add(this.richTextBox1);
            base.Controls.Add(this.button1);
            base.Controls.Add(this.label1);
            base.Controls.Add(this.chart1);
            base.Controls.Add(this.progressBar1);
            this.DoubleBuffered = true;
            base.FormBorderStyle = FormBorderStyle.None;
            base.Name = "Progress";
            this.Text = "Progress";
            this.chart1.EndInit();
            base.ResumeLayout(false);
            base.PerformLayout();
        }
    }
}

