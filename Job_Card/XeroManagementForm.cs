namespace Job_Card
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Globalization;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Forms;
    using System.Web.Script.Serialization;

    public class XeroManagementForm : Form
    {
        private static readonly JavaScriptSerializer Json = new JavaScriptSerializer();
        private readonly JobCard jobCard;
        private SettingsSettingsDoc settings;
        private ComboBox cboMode;
        private Button btnConnect;
        private ComboBox cboTenants;
        private Label lblConnection;
        private Label lblBusiness;
        private Label lblCustomer;
        private Button btnCheckCustomer;
        private Button btnSendInvoice;
        private Button btnDeleteInvoice;
        private Button btnSyncAllUnpaid;
        private Label lblHistory;
        private Label lblStatus;
        private ToolTip actionButtonsToolTip;
        private bool syncAllUnpaidRunning;
        private string selectedContactId;
        private string selectedContactName;
        private SentInvoiceDoc currentSentInvoice;
        private CancellationTokenSource connectCancellation;
        private bool suppressTenantComboPersist;

        public XeroManagementForm(JobCard owner)
        {
            this.jobCard = owner;
            this.InitializeComponent();
        }

        private async void XeroManagementForm_Load(object sender, EventArgs e)
        {
            await this.ReloadStateAsync();
        }

        private void InitializeComponent()
        {
            this.Text = "Xero Management";
            this.Size = new Size(760, 460);
            this.StartPosition = FormStartPosition.CenterParent;
            this.actionButtonsToolTip = new ToolTip();

            this.btnSyncAllUnpaid = new Button
            {
                Text = "Sync All Unpaid",
                Size = new Size(150, 28),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Enabled = false
            };
            this.btnSyncAllUnpaid.Click += this.btnSyncAllUnpaid_Click;
            this.btnSyncAllUnpaid.Location = new Point(this.ClientSize.Width - this.btnSyncAllUnpaid.Width - 16, 16);

            var lblMode = new Label { Text = "Invoice Mode", Location = new Point(20, 20), AutoSize = true };
            this.cboMode = new ComboBox { Location = new Point(130, 16), Width = 180, DropDownStyle = ComboBoxStyle.DropDownList };
            this.cboMode.Items.AddRange(new object[] { "Draft", "AuthoriseAndEmail" });
            this.cboMode.SelectedIndexChanged += this.cboMode_SelectedIndexChanged;

            this.btnConnect = new Button { Text = "Connect to Xero", Location = new Point(20, 55), Size = new Size(140, 32) };
            this.btnConnect.Click += this.btnConnect_Click;
            this.lblConnection = new Label { Text = "Disconnected", Location = new Point(170, 62), AutoSize = true };

            var lblTenant = new Label { Text = "Tenant", Location = new Point(20, 100), AutoSize = true };
            this.cboTenants = new ComboBox { Location = new Point(130, 96), Width = 300, DropDownStyle = ComboBoxStyle.DropDownList };
            this.cboTenants.SelectedIndexChanged += this.cboTenants_SelectedIndexChanged;

            this.lblBusiness = new Label { Text = "Business: ", Location = new Point(20, 145), AutoSize = true };
            this.btnCheckCustomer = new Button { Text = "Check Customer", Location = new Point(20, 175), Size = new Size(140, 32) };
            this.btnCheckCustomer.Click += this.btnCheckCustomer_Click;
            this.lblCustomer = new Label { Text = "Customer: not checked", Location = new Point(170, 183), AutoSize = true };

            this.btnSendInvoice = new Button { Text = "Send Invoice", Location = new Point(20, 225), Size = new Size(120, 36), Enabled = false };
            this.btnSendInvoice.Click += this.btnSendInvoice_Click;
            this.btnDeleteInvoice = new Button { Text = "Delete Invoice", Location = new Point(150, 225), Size = new Size(120, 36), Enabled = false };
            this.btnDeleteInvoice.Click += this.btnDeleteInvoice_Click;

            this.lblHistory = new Label { Text = "No sent invoice for this job yet.", Location = new Point(20, 280), AutoSize = false, Size = new Size(700, 80) };
            this.lblStatus = new Label { Text = "", Location = new Point(20, 370), AutoSize = false, Size = new Size(700, 40), ForeColor = Color.DarkBlue };

            this.Controls.Add(lblMode);
            this.Controls.Add(this.cboMode);
            this.Controls.Add(this.btnConnect);
            this.Controls.Add(this.lblConnection);
            this.Controls.Add(lblTenant);
            this.Controls.Add(this.cboTenants);
            this.Controls.Add(this.lblBusiness);
            this.Controls.Add(this.btnCheckCustomer);
            this.Controls.Add(this.lblCustomer);
            this.Controls.Add(this.btnSendInvoice);
            this.Controls.Add(this.btnDeleteInvoice);
            this.Controls.Add(this.lblHistory);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.btnSyncAllUnpaid);
            this.btnSyncAllUnpaid.BringToFront();
            this.Load += this.XeroManagementForm_Load;
            this.FormClosing += this.XeroManagementForm_FormClosing;
            this.MouseMove += this.XeroManagementForm_MouseMove;
        }

        private void XeroManagementForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this.connectCancellation != null)
            {
                this.connectCancellation.Cancel();
            }
        }

        private void XeroManagementForm_MouseMove(object sender, MouseEventArgs e)
        {
            bool connected = string.Equals(this.lblConnection.Text, "Connected", StringComparison.Ordinal);
            if (connected)
            {
                this.actionButtonsToolTip.Hide(this);
                return;
            }
            bool hoveringSend = this.btnSendInvoice.Bounds.Contains(e.Location);
            bool hoveringDelete = this.btnDeleteInvoice.Bounds.Contains(e.Location);
            if (!hoveringSend && !hoveringDelete)
            {
                this.actionButtonsToolTip.Hide(this);
                return;
            }
            this.actionButtonsToolTip.Show("Connect first!", this, e.Location.X + 12, e.Location.Y + 12, 1200);
        }

        private async Task ReloadStateAsync()
        {
            this.settings = await DataAccess.findSettings();
            this.lblBusiness.Text = "Business: " + this.jobCard.GetCurrentBusinessName();
            string mode = XeroService.GetDefaultMode(this.settings != null ? this.settings.xeroInvoiceMode : null);
            this.cboMode.SelectedItem = mode;
            if (this.cboMode.SelectedIndex < 0)
            {
                this.cboMode.SelectedIndex = 0;
            }

            bool connected = this.settings != null && !string.IsNullOrWhiteSpace(this.settings.xeroAccessToken);
            this.lblConnection.Text = connected ? "Connected" : "Disconnected";
            if (connected)
            {
                await this.LoadTenantsAsync();
            }
            else
            {
                this.cboTenants.Items.Clear();
            }
            this.currentSentInvoice = await DataAccess.FindSentInvoiceByJobAsync(this.jobCard.GetCurrentJobId(), this.settings != null ? this.settings.xeroTenantId : null);
            this.ApplyStoredContactFromSentInvoiceIfNeeded();
            await this.RefreshHistoryAsync();
            this.RefreshActionStates("ReloadStateAsync initial");
            await this.RefreshPaidStatusFromXeroAsync();
            this.RefreshActionStates("ReloadStateAsync after Xero status sync");
            this.UpdateSyncAllUnpaidButtonEnabled();
        }

        private void ApplyStoredContactFromSentInvoiceIfNeeded()
        {
            if (this.currentSentInvoice == null)
            {
                return;
            }
            if (!string.IsNullOrWhiteSpace(this.selectedContactId))
            {
                return;
            }
            if (string.IsNullOrWhiteSpace(this.currentSentInvoice.xeroContactId))
            {
                return;
            }
            this.selectedContactId = this.currentSentInvoice.xeroContactId.Trim();
            string name = this.currentSentInvoice.jobBusinessName;
            if (string.IsNullOrWhiteSpace(name))
            {
                name = this.jobCard.GetCurrentBusinessName();
            }
            if (string.IsNullOrWhiteSpace(name))
            {
                name = "Xero contact";
            }
            this.selectedContactName = name;
            this.lblCustomer.Text = "Customer: " + this.selectedContactName + " (from last invoice)";
        }

        private async Task RefreshHistoryAsync()
        {
            if (this.currentSentInvoice == null)
            {
                this.lblHistory.Text = "No sent invoice for this job yet.";
                return;
            }
            this.lblHistory.Text = string.Format(
                "Sent invoice: #{0} | Status: {1} | Amount: {2:F2} {3} | Date sent: {4:d/M/yy}",
                this.currentSentInvoice.invoiceNumber,
                this.currentSentInvoice.status,
                this.currentSentInvoice.amountTotal,
                this.currentSentInvoice.currency,
                this.currentSentInvoice.dateSentUtc.ToLocalTime());
        }

        private void RefreshActionStates(string source)
        {
            bool connected = string.Equals(this.lblConnection.Text, "Connected", StringComparison.Ordinal);
            bool hasBusiness = !string.IsNullOrWhiteSpace(this.jobCard.GetCurrentBusinessName());
            bool hasContact = !string.IsNullOrWhiteSpace(this.selectedContactId);
            bool totalNonZero = this.jobCard.GetCurrentTotal() > 0.0;
            bool hasTenant = this.settings != null && !string.IsNullOrWhiteSpace(this.settings.xeroTenantId);
            bool hasExistingSentInvoice = this.currentSentInvoice != null;
            string existingInvoiceStatus = hasExistingSentInvoice && this.currentSentInvoice.status != null
                ? this.currentSentInvoice.status.Trim()
                : "";
            bool existingInvoiceIsDeleted = hasExistingSentInvoice &&
                string.Equals(existingInvoiceStatus, "DELETED", StringComparison.OrdinalIgnoreCase);
            bool alreadySent = hasExistingSentInvoice && !existingInvoiceIsDeleted;
            bool canSendInvoice = connected && hasBusiness && hasContact && totalNonZero && hasTenant && !alreadySent;
            bool canDeleteInvoice = connected && hasExistingSentInvoice;
            this.btnSendInvoice.Enabled = canSendInvoice;
            this.btnDeleteInvoice.Enabled = canDeleteInvoice;
            string disconnectedTooltip = connected ? string.Empty : "Connect first!";
            this.actionButtonsToolTip.SetToolTip(this.btnSendInvoice, disconnectedTooltip);
            this.actionButtonsToolTip.SetToolTip(this.btnDeleteInvoice, disconnectedTooltip);
            System.Console.WriteLine(
                "XeroManagementForm.RefreshActionStates [{0}] => Connected={1}; SendInvoiceEnabled={2}; DeleteInvoiceEnabled={3}; " +
                "hasBusiness={4}; hasContact={5}; totalNonZero={6}; hasTenant={7}; hasExistingSentInvoice={8}; existingInvoiceStatus='{9}'; existingInvoiceIsDeleted={10}; alreadySent={11}",
                source,
                connected,
                this.btnSendInvoice.Enabled,
                this.btnDeleteInvoice.Enabled,
                hasBusiness,
                hasContact,
                totalNonZero,
                hasTenant,
                hasExistingSentInvoice,
                existingInvoiceStatus,
                existingInvoiceIsDeleted,
                alreadySent);
        }

        private void UpdateSyncAllUnpaidButtonEnabled()
        {
            bool connected = string.Equals(this.lblConnection.Text, "Connected", StringComparison.Ordinal);
            this.btnSyncAllUnpaid.Enabled = connected && !this.syncAllUnpaidRunning;
        }

        private async void cboMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.cboMode.SelectedItem == null)
            {
                return;
            }
            await DataAccess.UpdateSettingsFieldsAsync(new List<KeyValuePair<string, dynamic>>
            {
                new KeyValuePair<string, dynamic>("xeroInvoiceMode", this.cboMode.SelectedItem.ToString())
            });
            this.lblStatus.Text = "Invoice mode saved.";
        }

        private async void btnConnect_Click(object sender, EventArgs e)
        {
            this.settings = await DataAccess.findSettings();
            bool missingClientId = this.settings == null || string.IsNullOrWhiteSpace(this.settings.xeroClientId);
            bool missingClientSecret = this.settings == null || string.IsNullOrWhiteSpace(this.settings.xeroClientSecret);
            bool missingRedirect = this.settings == null || string.IsNullOrWhiteSpace(this.settings.xeroRedirectUri);
            if (this.settings == null || missingClientId || missingClientSecret || missingRedirect)
            {
                string settingsJson = "(settings is null)";
                if (this.settings != null)
                {
                    settingsJson = Json.Serialize(this.settings);
                }
                string msg = "Xero config is incomplete in Mongo settings.settings." + Environment.NewLine +
                             "Database: settings, Collection: settings" + Environment.NewLine +
                             "Missing fields:" + Environment.NewLine +
                             "- xeroClientId: " + (missingClientId ? "MISSING" : "OK") + Environment.NewLine +
                             "- xeroClientSecret: " + (missingClientSecret ? "MISSING" : "OK") + Environment.NewLine +
                             "- xeroRedirectUri: " + (missingRedirect ? "MISSING" : "OK") + Environment.NewLine + Environment.NewLine +
                             "settings doc json:" + Environment.NewLine + settingsJson;
                System.Console.WriteLine("XERO SETTINGS DOC JSON: " + settingsJson);
                MessageBox.Show(msg);
                return;
            }
            if (this.connectCancellation != null)
            {
                this.connectCancellation.Dispose();
                this.connectCancellation = null;
            }
            this.connectCancellation = new CancellationTokenSource();
            CancellationToken token = this.connectCancellation.Token;
            this.btnConnect.Enabled = false;
            try
            {
                string state = Guid.NewGuid().ToString("N");
                string url = XeroService.BuildAuthorizeUrl(this.settings, state);
                string code = null;
                if (XeroService.IsLocalHttpRedirectUri(this.settings.xeroRedirectUri))
                {
                    this.lblStatus.Text = "Waiting for Xero in your browser…";
                    var capture = await XeroService.CaptureAuthorizationCodeFromLocalRedirectAsync(
                        this.settings.xeroRedirectUri,
                        state,
                        () => { XeroService.OpenAuthInBrowser(url); },
                        TimeSpan.FromMinutes(3),
                        token);
                    if (capture.Success && !string.IsNullOrWhiteSpace(capture.Code))
                    {
                        code = capture.Code.Trim();
                    }
                    else
                    {
                        string msg = string.IsNullOrWhiteSpace(capture.ErrorMessage)
                            ? "Automatic capture did not complete."
                            : capture.ErrorMessage;
                        this.lblStatus.Text = msg;
                        code = this.PromptForAuthCode();
                    }
                }
                else
                {
                    XeroService.OpenAuthInBrowser(url);
                    this.lblStatus.Text = "Complete login in the browser, then paste the authorization code.";
                    code = this.PromptForAuthCode();
                }
                if (string.IsNullOrWhiteSpace(code))
                {
                    this.lblStatus.Text = "Authorization cancelled.";
                    return;
                }
                this.lblStatus.Text = "Completing connection…";
                bool ok = await XeroService.ExchangeCodeAsync(this.settings, code.Trim());
                if (!ok)
                {
                    this.lblStatus.Text = "Failed to exchange auth code.";
                    return;
                }
                this.settings = await DataAccess.findSettings();
                await this.LoadTenantsAsync();
                this.lblStatus.Text = "Connected to Xero.";
            }
            catch (Exception ex)
            {
                this.lblStatus.Text = "Connect failed: " + ex.Message;
            }
            finally
            {
                this.btnConnect.Enabled = true;
                if (this.connectCancellation != null)
                {
                    this.connectCancellation.Dispose();
                    this.connectCancellation = null;
                }
                this.settings = await DataAccess.findSettings();
                this.UpdateSyncAllUnpaidButtonEnabled();
            }
        }

        private async Task LoadTenantsAsync()
        {
            this.settings = await DataAccess.findSettings();
            bool tokenOk = await XeroService.EnsureValidTokenAsync(this.settings);
            if (!tokenOk)
            {
                this.cboTenants.Items.Clear();
                this.lblConnection.Text = "Disconnected";
                this.UpdateSyncAllUnpaidButtonEnabled();
                return;
            }
            var tenants = await XeroService.GetTenantsAsync(this.settings);
            this.cboTenants.Items.Clear();
            foreach (var tenant in tenants)
            {
                this.cboTenants.Items.Add(tenant.tenantName + " | " + tenant.tenantId);
            }
            this.suppressTenantComboPersist = true;
            try
            {
                bool picked = this.TrySelectTenantComboById(this.settings != null ? this.settings.xeroTenantId : null);
                if (!picked && this.settings != null)
                {
                    picked = this.TrySelectTenantComboById(this.settings.xeroLastTenant);
                }
                if (!picked && this.cboTenants.Items.Count == 1)
                {
                    this.cboTenants.SelectedIndex = 0;
                }
            }
            finally
            {
                this.suppressTenantComboPersist = false;
            }
            if (this.cboTenants.SelectedIndex >= 0)
            {
                await this.PersistTenantSelectionFromComboAsync(false);
            }
            this.settings = await DataAccess.findSettings();
            this.lblConnection.Text = "Connected";
            this.UpdateSyncAllUnpaidButtonEnabled();
        }

        private bool TrySelectTenantComboById(string tenantId)
        {
            if (string.IsNullOrWhiteSpace(tenantId))
            {
                return false;
            }
            for (int i = 0; i < this.cboTenants.Items.Count; i++)
            {
                if (this.cboTenants.Items[i].ToString().Contains(tenantId))
                {
                    this.cboTenants.SelectedIndex = i;
                    return true;
                }
            }
            return false;
        }

        private async void cboTenants_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.suppressTenantComboPersist)
            {
                return;
            }
            await this.PersistTenantSelectionFromComboAsync(true);
        }

        private async Task PersistTenantSelectionFromComboAsync(bool showStatus)
        {
            if (this.cboTenants.SelectedItem == null)
            {
                return;
            }
            string selected = this.cboTenants.SelectedItem.ToString();
            int sep = selected.LastIndexOf('|');
            if (sep <= 0)
            {
                return;
            }
            string name = selected.Substring(0, sep).Trim();
            string id = selected.Substring(sep + 1).Trim();
            await DataAccess.UpdateSettingsFieldsAsync(new List<KeyValuePair<string, dynamic>>
            {
                new KeyValuePair<string, dynamic>("xeroTenantId", id),
                new KeyValuePair<string, dynamic>("xeroTenantName", name),
                new KeyValuePair<string, dynamic>("xeroLastTenant", id)
            });
            this.settings = await DataAccess.findSettings();
            if (showStatus)
            {
                this.lblStatus.Text = "Tenant selected: " + name;
            }
            this.RefreshActionStates("PersistTenantSelectionFromComboAsync");
        }

        private async void btnCheckCustomer_Click(object sender, EventArgs e)
        {
            this.settings = await DataAccess.findSettings();
            bool tokenOk = await XeroService.EnsureValidTokenAsync(this.settings);
            if (!tokenOk || string.IsNullOrWhiteSpace(this.settings.xeroTenantId))
            {
                this.lblStatus.Text = "Connect to Xero and choose a tenant first.";
                return;
            }
            string businessName = this.jobCard.GetCurrentBusinessName();
            var candidates = await XeroService.FindContactsAsync(this.settings, this.settings.xeroTenantId, businessName);
            XeroContactMatch exact = null;
            foreach (var c in candidates)
            {
                if (string.Equals(c.Name, businessName, StringComparison.OrdinalIgnoreCase))
                {
                    exact = c;
                    break;
                }
            }
            if (exact != null)
            {
                this.selectedContactId = exact.ContactID;
                this.selectedContactName = exact.Name;
                this.lblCustomer.Text = "Customer: " + this.selectedContactName + " (exact)";
                this.RefreshActionStates("btnCheckCustomer_Click exact");
                return;
            }
            XeroContactMatch selected = this.ShowContactPicker(candidates);
            if (selected == null)
            {
                this.lblCustomer.Text = "Customer: not selected";
                this.selectedContactId = null;
                this.selectedContactName = null;
            }
            else
            {
                this.selectedContactId = selected.ContactID;
                this.selectedContactName = selected.Name;
                this.lblCustomer.Text = "Customer: " + this.selectedContactName + " (fallback)";
            }
            this.RefreshActionStates("btnCheckCustomer_Click fallback");
        }

        private XeroContactMatch ShowContactPicker(List<XeroContactMatch> candidates)
        {
            if (candidates == null || candidates.Count == 0)
            {
                MessageBox.Show("No matching Xero customer found.");
                return null;
            }
            using (var picker = new Form())
            {
                picker.Text = "Select Xero Customer";
                picker.Size = new Size(540, 420);
                var list = new ListBox { Left = 10, Top = 10, Width = 500, Height = 320 };
                foreach (var c in candidates)
                {
                    list.Items.Add(c.Name + " | " + c.EmailAddress + " | " + c.ContactID);
                }
                var ok = new Button { Text = "Select", Left = 300, Top = 340, Width = 100, DialogResult = DialogResult.OK };
                var cancel = new Button { Text = "Cancel", Left = 410, Top = 340, Width = 100, DialogResult = DialogResult.Cancel };
                picker.Controls.Add(list);
                picker.Controls.Add(ok);
                picker.Controls.Add(cancel);
                picker.AcceptButton = ok;
                picker.CancelButton = cancel;
                if (picker.ShowDialog(this) != DialogResult.OK || list.SelectedIndex < 0)
                {
                    return null;
                }
                return candidates[list.SelectedIndex];
            }
        }

        private string PromptForAuthCode()
        {
            using (var prompt = new Form())
            {
                prompt.Text = "Xero OAuth Code";
                prompt.Size = new Size(620, 160);
                var label = new Label { Left = 10, Top = 10, Width = 580, Text = "Paste the authorization code from the Xero redirect URL:" };
                var input = new TextBox { Left = 10, Top = 35, Width = 580 };
                var ok = new Button { Left = 430, Top = 70, Width = 75, Text = "OK", DialogResult = DialogResult.OK };
                var cancel = new Button { Left = 515, Top = 70, Width = 75, Text = "Cancel", DialogResult = DialogResult.Cancel };
                prompt.Controls.Add(label);
                prompt.Controls.Add(input);
                prompt.Controls.Add(ok);
                prompt.Controls.Add(cancel);
                prompt.AcceptButton = ok;
                prompt.CancelButton = cancel;
                return prompt.ShowDialog(this) == DialogResult.OK ? input.Text : "";
            }
        }

        private async void btnSendInvoice_Click(object sender, EventArgs e)
        {
            this.settings = await DataAccess.findSettings();
            bool tokenOk = await XeroService.EnsureValidTokenAsync(this.settings);
            if (!tokenOk)
            {
                this.lblStatus.Text = "Xero token is invalid. Reconnect.";
                return;
            }
            string orderNumber = this.jobCard.GetCurrentOrderNumber();
            if (string.IsNullOrWhiteSpace(orderNumber))
            {
                MessageBox.Show(this, "Order Number cannot be blank!", "Xero", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            var lines = this.jobCard.BuildXeroLineItems(
                string.IsNullOrWhiteSpace(this.settings.xeroDefaultSalesAccountCode) ? "200" : this.settings.xeroDefaultSalesAccountCode,
                string.IsNullOrWhiteSpace(this.settings.xeroDefaultTaxType) ? "OUTPUT2" : this.settings.xeroDefaultTaxType);
            string mode = XeroService.GetDefaultMode(this.settings.xeroInvoiceMode);
            var result = await XeroService.CreateInvoiceAsync(this.settings, this.settings.xeroTenantId, this.selectedContactId, mode, orderNumber, lines);
            if (!result.Success)
            {
                this.lblStatus.Text = "Send failed: " + result.ErrorMessage;
                return;
            }
            var sent = new SentInvoiceDoc
            {
                jobId = this.jobCard.GetCurrentJobId(),
                jobBusinessName = this.jobCard.GetCurrentBusinessName(),
                xeroTenantId = this.settings.xeroTenantId,
                xeroContactId = this.selectedContactId,
                xeroInvoiceId = result.InvoiceId,
                invoiceNumber = result.InvoiceNumber,
                invoiceMode = mode,
                amountTotal = this.jobCard.GetCurrentTotal(),
                currency = "NZD",
                dateSentUtc = DateTime.UtcNow,
                status = result.Status,
                lineItemsSnapshot = XeroService.BuildLineItemsSnapshot(lines),
                rawResponseSnippet = result.RawResponse
            };
            await DataAccess.UpsertSentInvoiceAsync(sent);
            this.currentSentInvoice = sent;
            await this.RefreshHistoryAsync();
            this.lblStatus.Text = "Invoice sent successfully.";
            this.RefreshActionStates("btnSendInvoice_Click");
        }

        private async void btnDeleteInvoice_Click(object sender, EventArgs e)
        {
            if (this.currentSentInvoice == null)
            {
                return;
            }
            if (MessageBox.Show("Delete/Void the Xero invoice for this job?", "Confirm", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) != DialogResult.OK)
            {
                return;
            }
            this.settings = await DataAccess.findSettings();
            bool tokenOk = await XeroService.EnsureValidTokenAsync(this.settings);
            if (!tokenOk)
            {
                this.lblStatus.Text = "Cannot validate token.";
                return;
            }
            var invoiceRoot = await XeroService.GetInvoiceAsync(this.settings, this.currentSentInvoice.xeroTenantId, this.currentSentInvoice.xeroInvoiceId);
            string invoiceStatus = this.GetInvoiceStatusFromRoot(invoiceRoot);
            string statusToApply = this.GetInvoiceDeleteAction(invoiceStatus);
            if (statusToApply == "NONE")
            {
                this.lblStatus.Text = "Invoice is already " + invoiceStatus + " in Xero. Removing local sent record.";
                await DataAccess.DeleteSentInvoiceAsync(this.currentSentInvoice.jobId, this.currentSentInvoice.xeroTenantId);
                this.currentSentInvoice = null;
                await this.RefreshHistoryAsync();
                this.RefreshActionStates("btnDeleteInvoice_Click already deleted in Xero");
                return;
            }
            if (statusToApply == null)
            {
                this.lblStatus.Text = "Cannot delete/void invoice while Xero status is " + (string.IsNullOrWhiteSpace(invoiceStatus) ? "(unknown)" : invoiceStatus) + ".";
                return;
            }
            var xeroDelete = await XeroService.UpdateInvoiceStatusAsync(this.settings, this.currentSentInvoice.xeroTenantId, this.currentSentInvoice.xeroInvoiceId, statusToApply);
            if (!xeroDelete.Success)
            {
                System.Console.WriteLine(
                    "Xero status update failed for InvoiceID {0}. CurrentStatus={1}, RequestedStatus={2}. {3}",
                    this.currentSentInvoice.xeroInvoiceId,
                    invoiceStatus ?? "(unknown)",
                    statusToApply,
                    xeroDelete.ErrorMessage ?? "(no error message)");
                if (!string.IsNullOrWhiteSpace(xeroDelete.RawResponse))
                {
                    System.Console.WriteLine("Xero status update raw response: " + xeroDelete.RawResponse);
                }
                this.lblStatus.Text = "Xero rejected status update to " + statusToApply + ": " + (xeroDelete.ErrorMessage ?? "Unknown error.");
                return;
            }
            await DataAccess.DeleteSentInvoiceAsync(this.currentSentInvoice.jobId, this.currentSentInvoice.xeroTenantId);
            this.currentSentInvoice = null;
            await this.RefreshHistoryAsync();
            this.lblStatus.Text = "Invoice marked " + statusToApply + " in Xero and removed locally.";
            this.RefreshActionStates("btnDeleteInvoice_Click");
        }

        private string GetInvoiceStatusFromRoot(Dictionary<string, object> root)
        {
            if (root == null || !root.ContainsKey("Invoices"))
            {
                return null;
            }
            var rows = root["Invoices"] as System.Collections.ArrayList;
            if (rows == null || rows.Count == 0)
            {
                return null;
            }
            var invoice = rows[0] as Dictionary<string, object>;
            if (invoice == null || !invoice.ContainsKey("Status"))
            {
                return null;
            }
            string status = Convert.ToString(invoice["Status"]);
            if (string.IsNullOrWhiteSpace(status))
            {
                return null;
            }
            return status.Trim().ToUpperInvariant();
        }

        private string GetInvoiceDeleteAction(string invoiceStatus)
        {
            if (string.IsNullOrWhiteSpace(invoiceStatus))
            {
                return null;
            }
            if (invoiceStatus == "DRAFT" || invoiceStatus == "SUBMITTED")
            {
                return "DELETED";
            }
            if (invoiceStatus == "AUTHORISED")
            {
                return "VOIDED";
            }
            if (invoiceStatus == "VOIDED" || invoiceStatus == "DELETED")
            {
                return "NONE";
            }
            return null;
        }

        public async Task RefreshPaidStatusFromXeroAsync()
        {
            if (this.currentSentInvoice == null)
            {
                return;
            }
            this.settings = await DataAccess.findSettings();
            bool tokenOk = await XeroService.EnsureValidTokenAsync(this.settings);
            if (!tokenOk)
            {
                return;
            }
            var root = await XeroService.GetInvoiceAsync(this.settings, this.currentSentInvoice.xeroTenantId, this.currentSentInvoice.xeroInvoiceId);
            if (root == null || !root.ContainsKey("Invoices"))
            {
                return;
            }
            var rows = root["Invoices"] as System.Collections.ArrayList;
            if (rows == null || rows.Count == 0)
            {
                return;
            }
            var invoice = rows[0] as Dictionary<string, object>;
            if (invoice == null)
            {
                return;
            }
            await this.ApplyInvoiceFromXeroToSentAsync(this.currentSentInvoice, invoice);
            await this.RefreshHistoryAsync();
            this.RefreshActionStates("RefreshPaidStatusFromXeroAsync");
        }

        private async Task ApplyInvoiceFromXeroToSentAsync(SentInvoiceDoc sent, Dictionary<string, object> invoice)
        {
            string status = invoice.ContainsKey("Status") ? Convert.ToString(invoice["Status"]) : sent.status;
            sent.status = status;
            if (invoice.ContainsKey("FullyPaidOnDate") && invoice["FullyPaidOnDate"] != null)
            {
                DateTime paidDate;
                if (DateTime.TryParse(Convert.ToString(invoice["FullyPaidOnDate"]), CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out paidDate))
                {
                    sent.datePaidUtc = paidDate.ToUniversalTime();
                    await DataAccess.UpdateJobPaidStatusAsync(sent.jobId, paidDate);
                    if (sent.jobId == this.jobCard.GetCurrentJobId())
                    {
                        this.jobCard.SetPaidDateText(paidDate);
                    }
                }
            }
            await DataAccess.UpsertSentInvoiceAsync(sent);
        }

        private async void btnSyncAllUnpaid_Click(object sender, EventArgs e)
        {
            if (!string.Equals(this.lblConnection.Text, "Connected", StringComparison.Ordinal))
            {
                return;
            }
            this.settings = await DataAccess.findSettings();
            bool tokenOk = await XeroService.EnsureValidTokenAsync(this.settings);
            if (!tokenOk)
            {
                this.lblStatus.Text = "Xero token is invalid. Reconnect.";
                return;
            }
            if (string.IsNullOrWhiteSpace(this.settings.xeroTenantId))
            {
                this.lblStatus.Text = "Choose a Xero tenant first.";
                return;
            }
            this.syncAllUnpaidRunning = true;
            this.UpdateSyncAllUnpaidButtonEnabled();
            try
            {
                var unpaid = await DataAccess.FindUnpaidSentInvoicesForTenantAsync(this.settings.xeroTenantId);
                int checkedCount = 0;
                foreach (var sent in unpaid)
                {
                    if (string.IsNullOrWhiteSpace(sent.xeroInvoiceId))
                    {
                        continue;
                    }
                    var root = await XeroService.GetInvoiceAsync(this.settings, sent.xeroTenantId, sent.xeroInvoiceId);
                    if (root == null || !root.ContainsKey("Invoices"))
                    {
                        continue;
                    }
                    var rows = root["Invoices"] as System.Collections.ArrayList;
                    if (rows == null || rows.Count == 0)
                    {
                        continue;
                    }
                    var invoice = rows[0] as Dictionary<string, object>;
                    if (invoice == null)
                    {
                        continue;
                    }
                    await this.ApplyInvoiceFromXeroToSentAsync(sent, invoice);
                    checkedCount++;
                }
                this.currentSentInvoice = await DataAccess.FindSentInvoiceByJobAsync(this.jobCard.GetCurrentJobId(), this.settings.xeroTenantId);
                this.ApplyStoredContactFromSentInvoiceIfNeeded();
                await this.RefreshHistoryAsync();
                this.RefreshActionStates("btnSyncAllUnpaid_Click after reload");
                this.lblStatus.Text = string.Format(CultureInfo.InvariantCulture, "Synced {0} unpaid Xero invoice(s) from Xero.", checkedCount);
            }
            finally
            {
                this.syncAllUnpaidRunning = false;
                this.UpdateSyncAllUnpaidButtonEnabled();
            }
        }
    }
}
