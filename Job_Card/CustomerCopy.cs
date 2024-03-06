namespace Job_Card
{
    using RichTextBoxPrintCtrlNS;
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Drawing.Printing;
    using System.Runtime.CompilerServices;
    using System.Windows.Forms;

    public class CustomerCopy : Form
    {
        public static bool autoPrint = false;
        private Button btnPageSetup;
        private Button btnPrint;
        private Button btnPrintPreview;
        private int checkPrint;
        private IContainer components = null;
        public PrintHandler OnPrintPressed;
        private PageSetupDialog pageSetupDialog1;
        private PrintDialog printDialog1;
        private PrintDocument printDocument1;
        private PrintPreviewDialog printPreviewDialog1;
        internal RichTextBoxPrintCtrlNS.RichTextBoxPrintCtrl richTextBox1;

        private void AutoPrintEvent(object sender, EventArgs e)
        {
            Timer timer = (Timer)sender;
            timer.Stop();
            try
            {
                this.printDocument1.Print();
                
            } catch (Exception err)
            {
                MessageBox.Show("An error occured printing - is the printer setup and on? " + err.Message);
            } finally
            {
                
            }
        }
        public CustomerCopy(bool allowAutoPrint = true)
        {
            this.InitializeComponent();
            if (allowAutoPrint && CustomerCopy.autoPrint)
            {
                Timer autoPrintTimer = new Timer();
                
                autoPrintTimer.Interval = 500;
                autoPrintTimer.Tick += new EventHandler(AutoPrintEvent);
                
                autoPrintTimer.Start();
                
            }
        }

        private void btnPageSetup_Click(object sender, EventArgs e)
        {
            this.pageSetupDialog1.ShowDialog();
        }

        private void btnPrint_Click(object sender, EventArgs e)
        {
            if (this.printDialog1.ShowDialog() == DialogResult.OK)
            {
                this.printDocument1.Print();
            }
        }

        private void btnPrintPreview_Click(object sender, EventArgs e)
        {
            this.printPreviewDialog1.ShowDialog();
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
            ComponentResourceManager manager = new ComponentResourceManager(typeof(CustomerCopy));
            this.btnPageSetup = new Button();
            this.btnPrintPreview = new Button();
            this.btnPrint = new Button();
            this.printDialog1 = new PrintDialog();
            this.printDocument1 = new PrintDocument();
            this.pageSetupDialog1 = new PageSetupDialog();
            this.printPreviewDialog1 = new PrintPreviewDialog();
            this.richTextBox1 = new RichTextBoxPrintCtrlNS.RichTextBoxPrintCtrl();
            base.SuspendLayout();
            this.btnPageSetup.Font = new Font("Arial", 12f, FontStyle.Bold);
            this.btnPageSetup.Location = new Point(0, 3);
            this.btnPageSetup.Name = "btnPageSetup";
            this.btnPageSetup.Size = new Size(0xa2, 30);
            this.btnPageSetup.TabIndex = 1;
            this.btnPageSetup.Text = "Page Setup";
            this.btnPageSetup.UseVisualStyleBackColor = true;
            this.btnPageSetup.Click += new EventHandler(this.btnPageSetup_Click);
            this.btnPrintPreview.Font = new Font("Arial", 12f, FontStyle.Bold);
            this.btnPrintPreview.Location = new Point(0xc5, 3);
            this.btnPrintPreview.Name = "btnPrintPreview";
            this.btnPrintPreview.Size = new Size(0xa2, 30);
            this.btnPrintPreview.TabIndex = 2;
            this.btnPrintPreview.Text = "Print Preview";
            this.btnPrintPreview.UseVisualStyleBackColor = true;
            this.btnPrintPreview.Click += new EventHandler(this.btnPrintPreview_Click);
            this.btnPrint.Font = new Font("Arial", 12f, FontStyle.Bold);
            this.btnPrint.Location = new Point(0x18b, 3);
            this.btnPrint.Name = "btnPrint";
            this.btnPrint.Size = new Size(0xa2, 30);
            this.btnPrint.TabIndex = 3;
            this.btnPrint.Text = "Print";
            this.btnPrint.UseVisualStyleBackColor = true;
            this.btnPrint.Click += new EventHandler(this.btnPrint_Click);
            this.printDialog1.Document = this.printDocument1;
            this.printDialog1.UseEXDialog = true;
            this.printDocument1.BeginPrint += new PrintEventHandler(this.printDocument1_BeginPrint);
            this.printDocument1.PrintPage += new PrintPageEventHandler(this.printDocument1_PrintPage);
            this.pageSetupDialog1.Document = this.printDocument1;
            this.printPreviewDialog1.AutoScrollMargin = new Size(0, 0);
            this.printPreviewDialog1.AutoScrollMinSize = new Size(0, 0);
            this.printPreviewDialog1.ClientSize = new Size(400, 300);
            this.printPreviewDialog1.Document = this.printDocument1;
            this.printPreviewDialog1.Enabled = true;
            this.printPreviewDialog1.Icon = (Icon) manager.GetObject("printPreviewDialog1.Icon");
            this.printPreviewDialog1.Name = "printPreviewDialog1";
            this.printPreviewDialog1.Visible = false;
            this.richTextBox1.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Top;
            this.richTextBox1.Location = new Point(0, 30);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.Size = new Size(610, 0x44c);
            this.richTextBox1.TabIndex = 0;
            this.richTextBox1.Text = "";
            base.AutoScaleDimensions = new SizeF(6f, 13f);
            base.AutoScaleMode = AutoScaleMode.Font;
            base.ClientSize = new Size(0x25c, 0x3ec);
            base.Controls.Add(this.btnPrint);
            base.Controls.Add(this.btnPrintPreview);
            base.Controls.Add(this.btnPageSetup);
            base.Controls.Add(this.richTextBox1);
            this.DoubleBuffered = true;
            base.FormBorderStyle = FormBorderStyle.FixedSingle;
            base.Name = "CustomerCopy";
            base.ShowIcon = false;
            base.SizeGripStyle = SizeGripStyle.Hide;
            base.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "CustomerCopy";
            base.ResumeLayout(false);
        }

        private void printDocument1_BeginPrint(object sender, PrintEventArgs e)
        {
            this.checkPrint = 0;
        }

        private void printDocument1_PrintPage(object sender, PrintPageEventArgs e)
        {
            this.checkPrint = this.richTextBox1.Print(this.checkPrint, this.richTextBox1.TextLength, e);
            if (this.checkPrint < this.richTextBox1.TextLength)
            {
                e.HasMorePages = true;
            }
            else
            {
                e.HasMorePages = false;
            }
            if (this.OnPrintPressed != null)
            {
                this.OnPrintPressed();
            }
        }

        public void PrintNow()
        {
            this.printDocument1.Print();
        }

        public delegate void PrintHandler();
    }
}

