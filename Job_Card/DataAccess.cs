namespace Job_Card
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.OleDb;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;
    using MongoDB.Bson;
    using MongoDB.Bson.Serialization.Attributes;
    using MongoDB.Driver;
    using System.Reflection;
    using System.Threading.Tasks;
    using System.ComponentModel;
    using System.Runtime.Remoting;

    public static class MongoIPAddressInputDialog
    {
        const string ConfigFilePath = "./mongoIP.txt"; // Path to the config file
        public static bool userCancelled = false;
        public static string ReadLastIpValue()
        {
            try
            {
                if (File.Exists(ConfigFilePath))
                {
                    return File.ReadAllText(ConfigFilePath);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading config file: {ex.Message}");
            }

            return "localhost"; // Default value if config file doesn't exist
        }

        public static void SaveLastIpValue(string ipAddress)
        {
            try
            {
                File.WriteAllText(ConfigFilePath, ipAddress);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving config file: {ex.Message}");
            }
        }
    
        public static void InterceptFormClose(object sender, FormClosingEventArgs e) 
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                MongoIPAddressInputDialog.userCancelled = true;
            }
        }

    public static string ShowInputDialog(string prompt, string title)
        {
            Form inputForm = new Form
            {
                Width = 600,
                Height = 150,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = title,
                StartPosition = FormStartPosition.CenterScreen
            };
             // Read the last IP value from the config file
            string defaultValue = MongoIPAddressInputDialog.ReadLastIpValue();
            if (defaultValue == "")
            {
                defaultValue = "localhost";
            }
            Label label = new Label { Left = 20, Top = 20, Width = 450, Text = prompt };
            
            TextBox textBox = new TextBox { Left = 20, Top = 50, Width = 450, Text = defaultValue };
            Button okButton = new Button { Text = "OK", Left = 200, Width = 60, Top = 80, DialogResult = DialogResult.OK };

            okButton.Click += (sender, e) =>
            {
                inputForm.Close();
                MongoIPAddressInputDialog.userCancelled = false;
            };
            inputForm.FormClosing += MongoIPAddressInputDialog.InterceptFormClose;
            inputForm.Controls.Add(label);
            inputForm.Controls.Add(textBox);
            inputForm.Controls.Add(okButton);

            DialogResult result = inputForm.ShowDialog();
            if (MongoIPAddressInputDialog.userCancelled)
            {
                Application.Exit();
                Application.ExitThread();

                Environment.Exit(0);
            }
            inputForm.FormClosing -= MongoIPAddressInputDialog.InterceptFormClose;
            if (result == DialogResult.OK && textBox.Text.Length > 8 && textBox.Text != defaultValue)
            {

               MongoIPAddressInputDialog.SaveLastIpValue(textBox.Text);
            }
            return result == DialogResult.OK ? textBox.Text : defaultValue;
        }
    }

    public class FussyCustomerDoc
    {
        [BsonId]
        public ObjectId Id { get; set; }
        [BsonElement("phoneOrEmail")]
        public string phoneOrEmail { get; set; }

    }
    public class PricingDoc
    {
        [BsonId]
        public ObjectId Id { get; set; }
        [BsonElement("isWheel")]
        public bool isWheel { get; set; }
        [BsonElement("controlName")]
        public string controlName { get; set; }
        [BsonElement("controlText")]
        public string controlText { get; set; }
        [BsonElement("stringPrice")]
        public string stringPrice { get; set; }


    }
    public class SettingsSettingsDoc
    {
        [BsonId]
        public ObjectId Id { get; set; }
        [BsonElement("emailAddress")]
        public string emailAddress { get; set; }
        [BsonElement("emailPassword")]
        public string emailPassword { get; set; }
        [BsonElement("emailName")]
        public string emailName { get; set; }
        [BsonElement("emailPort")]
        public int emailPort { get; set; }
        [BsonElement("emailDomain")]
        public string emailDomain { get; set; }
      
        public BsonDocument pricing { get; set; }


    }


    public class JobCardDoc
    {
        [BsonId]
        public ObjectId Id { get; set; }

        [BsonElement("jobID")]
        public int jobID { get; set; }


        [BsonIgnore]
        private BsonDateTime _jobDate;
        [BsonElement("jobDate")]
        public DateTime? jobDate
        {
            get
            {
                if (_jobDate == null)
                {
                    return null;
                }
                return _jobDate.ToNullableLocalTime();
            }
            set
            {
                _jobDate = value;
            }
        }

        [BsonElement("jobCustomer")]
        public string jobCustomer { get; set; }

        [BsonElement("jobAddress")]
        public string jobAddress { get; set; }

        [BsonElement("jobPhone")]
        public string jobPhone { get; set; }

        [BsonElement("jobEmail")]
        public string jobEmail { get; set; }

        [BsonElement("jobOrderNumber")]
        public string jobOrderNumber { get; set; }

        [BsonElement("jobFussyNotes")]
        public string jobFussyNotes { get; set; }

        [BsonElement("jobDelivery")]
        public string jobDelivery { get; set; }

        [BsonElement("jobReceivedFrom")]
        public string jobReceivedFrom { get; set; }


        [BsonIgnore]
        private BsonDateTime _jobDateRequired;
        [BsonElement("jobDateRequired")]
        public DateTime? jobDateRequired
        {
            get
            {
                if (_jobDateRequired == null)
                {
                    return null;
                }
                return _jobDateRequired.ToNullableLocalTime();
            }
            set
            {
                _jobDateRequired = value;
            }
        }

        [BsonIgnore]
        private BsonDateTime _jobDateCompleted;
        [BsonElement("jobDateCompleted")]
        public DateTime? jobDateCompleted
        {
            get
            {
                if (_jobDateCompleted == null)
                {
                    return null;
                }
                return _jobDateCompleted.ToNullableLocalTime();
            }
            set
            {
                _jobDateCompleted = value;
            }
        }

        [BsonElement("jobPaymentBy")]
        public string jobPaymentBy { get; set; }

        [BsonElement("jobNotes")]
        public string jobNotes { get; set; }


        [BsonIgnore]
        private BsonDateTime _jobDatePaid;
        [BsonElement("jobDatePaid")]
        public DateTime? jobDatePaid
        {
            get
            {
                if (_jobDatePaid == null)
                {
                    return null;
                }
                return _jobDatePaid.ToNullableLocalTime();
            }
            set
            {
                _jobDatePaid = value;
            }
        }

        [BsonElement("jobDetail00")]
        public string jobDetail00 { get; set; }

        [BsonElement("jobType00")]
        public string jobType00 { get; set; }

        [BsonElement("jobQty00")]
        public int? jobQty00 { get; set; }

        [BsonElement("jobUnitPrice00")]
        [BsonRepresentation(BsonType.Double, AllowTruncation = true)]
        public float? jobUnitPrice00 { get; set; }

        [BsonElement("jobPrice00")]
        [BsonRepresentation(BsonType.Double, AllowTruncation = true)]
        public float? jobPrice00 { get; set; }

        [BsonElement("jobDetail01")]
        public string jobDetail01 { get; set; }

        [BsonElement("jobType01")]
        public string jobType01 { get; set; }

        [BsonElement("jobQty01")]
        public int? jobQty01 { get; set; }

        [BsonElement("jobUnitPrice01")]
        [BsonRepresentation(BsonType.Double, AllowTruncation = true)]
        public float? jobUnitPrice01 { get; set; }

        [BsonElement("jobPrice01")]
        [BsonRepresentation(BsonType.Double, AllowTruncation = true)]
        public float? jobPrice01 { get; set; }

        [BsonElement("jobDetail02")]
        public string jobDetail02 { get; set; }

        [BsonElement("jobType02")]
        public string jobType02 { get; set; }

        [BsonElement("jobQty02")]
        public int? jobQty02 { get; set; }

        [BsonElement("jobUnitPrice02")]
        [BsonRepresentation(BsonType.Double, AllowTruncation = true)]
        public float? jobUnitPrice02 { get; set; }

        [BsonElement("jobPrice02")]
        [BsonRepresentation(BsonType.Double, AllowTruncation = true)]
        public float? jobPrice02 { get; set; }

        [BsonElement("jobDetail03")]
        public string jobDetail03 { get; set; }

        [BsonElement("jobType03")]
        public string jobType03 { get; set; }

        [BsonElement("jobQty03")]
        public int? jobQty03 { get; set; }

        [BsonElement("jobUnitPrice03")]
        [BsonRepresentation(BsonType.Double, AllowTruncation = true)]
        public float? jobUnitPrice03 { get; set; }

        [BsonElement("jobPrice03")]
        [BsonRepresentation(BsonType.Double, AllowTruncation = true)]
        public float? jobPrice03 { get; set; }

        [BsonElement("jobDetail04")]
        public string jobDetail04 { get; set; }

        [BsonElement("jobType04")]
        public string jobType04 { get; set; }

        [BsonElement("jobQty04")]
        public int? jobQty04 { get; set; }

        [BsonElement("jobUnitPrice04")]
        [BsonRepresentation(BsonType.Double, AllowTruncation = true)]
        public float? jobUnitPrice04 { get; set; }

        [BsonElement("jobPrice04")]
        [BsonRepresentation(BsonType.Double, AllowTruncation = true)]
        public float? jobPrice04 { get; set; }

        [BsonElement("jobDetail05")]
        public string jobDetail05 { get; set; }

        [BsonElement("jobType05")]
        public string jobType05 { get; set; }

        [BsonElement("jobQty05")]
        public int? jobQty05 { get; set; }

        [BsonElement("jobUnitPrice05")]
        [BsonRepresentation(BsonType.Double, AllowTruncation = true)]
        public float? jobUnitPrice05 { get; set; }

        [BsonElement("jobPrice05")]
        [BsonRepresentation(BsonType.Double, AllowTruncation = true)]
        public float? jobPrice05 { get; set; }

        [BsonElement("jobDetail06")]
        public string jobDetail06 { get; set; }

        [BsonElement("jobType06")]
        public string jobType06 { get; set; }

        [BsonElement("jobQty06")]
        public int? jobQty06 { get; set; }

        [BsonElement("jobUnitPrice06")]
        [BsonRepresentation(BsonType.Double, AllowTruncation = true)]
        public float? jobUnitPrice06 { get; set; }

        [BsonElement("jobPrice06")]
        [BsonRepresentation(BsonType.Double, AllowTruncation = true)]
        public float? jobPrice06 { get; set; }

        [BsonElement("jobDetail07")]
        public string jobDetail07 { get; set; }

        [BsonElement("jobType07")]
        public string jobType07 { get; set; }

        [BsonElement("jobQty07")]
        public int? jobQty07 { get; set; }

        [BsonElement("jobUnitPrice07")]
        [BsonRepresentation(BsonType.Double, AllowTruncation = true)]
        public float? jobUnitPrice07 { get; set; }

        [BsonElement("jobPrice07")]
        [BsonRepresentation(BsonType.Double, AllowTruncation = true)]
        public float? jobPrice07 { get; set; }

        [BsonElement("jobDetail08")]
        public string jobDetail08 { get; set; }

        [BsonElement("jobType08")]
        public string jobType08 { get; set; }

        [BsonElement("jobQty08")]
        public int? jobQty08 { get; set; }

        [BsonElement("jobUnitPrice08")]
        [BsonRepresentation(BsonType.Double, AllowTruncation = true)]
        public float? jobUnitPrice08 { get; set; }

        [BsonElement("jobPrice08")]
        [BsonRepresentation(BsonType.Double, AllowTruncation = true)]
        public float? jobPrice08 { get; set; }

        [BsonElement("jobDetail09")]
        public string jobDetail09 { get; set; }

        [BsonElement("jobType09")]
        public string jobType09 { get; set; }

        [BsonElement("jobQty09")]
        public int? jobQty09 { get; set; }

        [BsonElement("jobUnitPrice09")]
        [BsonRepresentation(BsonType.Double, AllowTruncation = true)]
        public float? jobUnitPrice09 { get; set; }

        [BsonElement("jobPrice09")]
        [BsonRepresentation(BsonType.Double, AllowTruncation = true)]
        public float? jobPrice09 { get; set; }

        [BsonElement("jobDetail10")]
        public string jobDetail10 { get; set; }

        [BsonElement("jobType10")]
        public string jobType10 { get; set; }

        [BsonElement("jobQty10")]
        public int? jobQty10 { get; set; }

        [BsonElement("jobUnitPrice10")]
        [BsonRepresentation(BsonType.Double, AllowTruncation = true)]
        public float? jobUnitPrice10 { get; set; }

        [BsonElement("jobPrice10")]
        [BsonRepresentation(BsonType.Double, AllowTruncation = true)]
        public float? jobPrice10 { get; set; }

        [BsonElement("jobDetail11")]
        public string jobDetail11 { get; set; }

        [BsonElement("jobType11")]
        public string jobType11 { get; set; }

        [BsonElement("jobQty11")]
        public int? jobQty11 { get; set; }

        [BsonElement("jobUnitPrice11")]
        [BsonRepresentation(BsonType.Double, AllowTruncation = true)]
        public float? jobUnitPrice11 { get; set; }

        [BsonElement("jobPrice11")]
        [BsonRepresentation(BsonType.Double, AllowTruncation = true)]
        public float? jobPrice11 { get; set; }

        [BsonElement("jobDetail12")]
        public string jobDetail12 { get; set; }

        [BsonElement("jobType12")]
        public string jobType12 { get; set; }

        [BsonElement("jobQty12")]
        public int? jobQty12 { get; set; }

        [BsonElement("jobUnitPrice12")]
        [BsonRepresentation(BsonType.Double, AllowTruncation = true)]
        public float? jobUnitPrice12 { get; set; }

        [BsonElement("jobPrice12")]
        [BsonRepresentation(BsonType.Double, AllowTruncation = true)]
        public float? jobPrice12 { get; set; }

        [BsonElement("jobDetail13")]
        public string jobDetail13 { get; set; }

        [BsonElement("jobType13")]
        public string jobType13 { get; set; }

        [BsonElement("jobQty13")]
        public int? jobQty13 { get; set; }

        [BsonElement("jobUnitPrice13")]
        [BsonRepresentation(BsonType.Double, AllowTruncation = true)]
        public float? jobUnitPrice13 { get; set; }

        [BsonElement("jobPrice13")]
        [BsonRepresentation(BsonType.Double, AllowTruncation = true)]
        public float? jobPrice13 { get; set; }

        [BsonElement("jobDetail14")]
        public string jobDetail14 { get; set; }

        [BsonElement("jobType14")]
        public string jobType14 { get; set; }

        [BsonElement("jobQty14")]
        public int? jobQty14 { get; set; }

        [BsonElement("jobUnitPrice14")]
        [BsonRepresentation(BsonType.Double, AllowTruncation = true)]
        public float? jobUnitPrice14 { get; set; }

        [BsonElement("jobPrice14")]
        [BsonRepresentation(BsonType.Double, AllowTruncation = true)]
        public float? jobPrice14 { get; set; }

        [BsonElement("jobDetail15")]
        public string jobDetail15 { get; set; }

        [BsonElement("jobType15")]
        public string jobType15 { get; set; }

        [BsonElement("jobQty15")]
        public int? jobQty15 { get; set; }

        [BsonElement("jobUnitPrice15")]
        [BsonRepresentation(BsonType.Double, AllowTruncation = true)]
        public float? jobUnitPrice15 { get; set; }

        [BsonElement("jobPrice15")]
        [BsonRepresentation(BsonType.Double, AllowTruncation = true)]
        public float? jobPrice15 { get; set; }

        [BsonElement("jobDetail16")]
        public string jobDetail16 { get; set; }

        [BsonElement("jobType16")]
        public string jobType16 { get; set; }

        [BsonElement("jobQty16")]
        public int? jobQty16 { get; set; }

        [BsonElement("jobUnitPrice16")]
        [BsonRepresentation(BsonType.Double, AllowTruncation = true)]
        public float? jobUnitPrice16 { get; set; }

        [BsonElement("jobPrice16")]
        [BsonRepresentation(BsonType.Double, AllowTruncation = true)]
        public float? jobPrice16 { get; set; }

        [BsonElement("jobDetail17")]
        public string jobDetail17 { get; set; }

        [BsonElement("jobType17")]
        public string jobType17 { get; set; }

        [BsonElement("jobQty17")]
        public int? jobQty17 { get; set; }

        [BsonElement("jobUnitPrice17")]
        [BsonRepresentation(BsonType.Double, AllowTruncation = true)]
        public float? jobUnitPrice17 { get; set; }

        [BsonElement("jobPrice17")]
        [BsonRepresentation(BsonType.Double, AllowTruncation = true)]
        public float? jobPrice17 { get; set; }

        [BsonElement("jobRepair")]
        public bool? jobRepair { get; set; }

        [BsonElement("jobRepairText")]
        public string jobRepairText { get; set; }

        [BsonElement("jobRepairType")]
        public string jobRepairType { get; set; }

        [BsonElement("jobRepairQty")]
        public int? jobRepairQty { get; set; }

        [BsonElement("jobRepairUnitPrice")]
        [BsonRepresentation(BsonType.Double, AllowTruncation = true)]
        public float? jobRepairUnitPrice { get; set; }

        [BsonElement("jobRepairPrice")]
        [BsonRepresentation(BsonType.Double, AllowTruncation = true)]
        public float? jobRepairPrice { get; set; }

        [BsonElement("jobStrip")]
        public bool? jobStrip { get; set; }

        [BsonElement("jobStripText")]
        public string jobStripText { get; set; }

        [BsonElement("jobStripType")]
        public string jobStripType { get; set; }

        [BsonElement("jobStripQty")]
        public int? jobStripQty { get; set; }

        [BsonElement("jobStripUnitPrice")]
        [BsonRepresentation(BsonType.Double, AllowTruncation = true)]
        public float? jobStripUnitPrice { get; set; }

        [BsonElement("jobStripPrice")]
        [BsonRepresentation(BsonType.Double, AllowTruncation = true)]
        public float? jobStripPrice { get; set; }

        [BsonElement("jobPolish")]
        public bool? jobPolish { get; set; }

        [BsonElement("jobPolishText")]
        public string jobPolishText { get; set; }

        [BsonElement("jobPolishType")]
        public string jobPolishType { get; set; }

        [BsonElement("jobPolishQty")]
        public int? jobPolishQty { get; set; }

        [BsonElement("jobPolishUnitPrice")]
        [BsonRepresentation(BsonType.Double, AllowTruncation = true)]
        public float? jobPolishUnitPrice { get; set; }

        [BsonElement("jobPolishPrice")]
        [BsonRepresentation(BsonType.Double, AllowTruncation = true)]
        public float? jobPolishPrice { get; set; }

        [BsonElement("jobPlating")]
        public bool? jobPlating { get; set; }

        [BsonElement("jobPlatingText")]
        public string jobPlatingText { get; set; }

        [BsonElement("jobPlatingType")]
        public string jobPlatingType { get; set; }

        [BsonElement("jobPlatingQty")]
        public int? jobPlatingQty { get; set; }

        [BsonElement("jobPlatingUnitPrice")]
        [BsonRepresentation(BsonType.Double, AllowTruncation = true)]
        public float? jobPlatingUnitPrice { get; set; }

        [BsonElement("jobPlatingPrice")]
        [BsonRepresentation(BsonType.Double, AllowTruncation = true)]
        public float? jobPlatingPrice { get; set; }

        [BsonElement("jobLaquer")]
        public bool? jobLaquer { get; set; }

        [BsonElement("jobLaquerText")]
        public string jobLaquerText { get; set; }

        [BsonElement("jobLaquerType")]
        public string jobLaquerType { get; set; }

        [BsonElement("jobLaquerQty")]
        public int? jobLaquerQty { get; set; }

        [BsonElement("jobLaquerUnitPrice")]
        [BsonRepresentation(BsonType.Double, AllowTruncation = true)]
        public float? jobLaquerUnitPrice { get; set; }

        [BsonElement("jobLaquerPrice")]
        [BsonRepresentation(BsonType.Double, AllowTruncation = true)]
        public float? jobLaquerPrice { get; set; }

        [BsonElement("jobSilvGalv")]
        public bool? jobSilvGalv { get; set; }

        [BsonElement("jobSilvGalvText")]
        public string jobSilvGalvText { get; set; }

        [BsonElement("jobSilvGalvType")]
        public string jobSilvGalvType { get; set; }

        [BsonElement("jobSilvGalvQty")]
        public int? jobSilvGalvQty { get; set; }

        [BsonElement("jobSilvGalvUnitPrice")]
        [BsonRepresentation(BsonType.Double, AllowTruncation = true)]
        public float? jobSilvGalvUnitPrice { get; set; }

        [BsonElement("jobSilvGalvPrice")]
        [BsonRepresentation(BsonType.Double, AllowTruncation = true)]
        public float? jobSilvGalvPrice { get; set; }

        [BsonElement("jobGoldGalv")]
        public bool? jobGoldGalv { get; set; }

        [BsonElement("jobGoldGalvText")]
        public string jobGoldGalvText { get; set; }

        [BsonElement("jobGoldGalvType")]
        public string jobGoldGalvType { get; set; }

        [BsonElement("jobGoldGalvQty")]
        public int? jobGoldGalvQty { get; set; }

        [BsonElement("jobGoldGalvUnitPrice")]
        [BsonRepresentation(BsonType.Double, AllowTruncation = true)]
        public float? jobGoldGalvUnitPrice { get; set; }

        [BsonElement("jobGoldGalvPrice")]
        [BsonRepresentation(BsonType.Double, AllowTruncation = true)]
        public float? jobGoldGalvPrice { get; set; }

        [BsonElement("jobWheelCrack")]
        public bool? jobWheelCrack { get; set; }

        [BsonElement("jobWheelCrackText")]
        public string jobWheelCrackText { get; set; }

        [BsonElement("jobWheelCrackType")]
        public string jobWheelCrackType { get; set; }

        [BsonElement("jobWheelCrackQty")]
        public int? jobWheelCrackQty { get; set; }

        [BsonElement("jobWheelCrackUnitPrice")]
        [BsonRepresentation(BsonType.Double, AllowTruncation = true)]
        public float? jobWheelCrackUnitPrice { get; set; }

        [BsonElement("jobWheelCrackPrice")]
        [BsonRepresentation(BsonType.Double, AllowTruncation = true)]
        public float? jobWheelCrackPrice { get; set; }

        [BsonElement("jobWheelDent")]
        public bool? jobWheelDent { get; set; }

        [BsonElement("jobWheelDentText")]
        public string jobWheelDentText { get; set; }

        [BsonElement("jobWheelDentType")]
        public string jobWheelDentType { get; set; }

        [BsonElement("jobWheelDentQty")]
        public int? jobWheelDentQty { get; set; }

        [BsonElement("jobWheelDentUnitPrice")]
        [BsonRepresentation(BsonType.Double, AllowTruncation = true)]
        public float? jobWheelDentUnitPrice { get; set; }

        [BsonElement("jobWheelDentPrice")]
        [BsonRepresentation(BsonType.Double, AllowTruncation = true)]
        public float? jobWheelDentPrice { get; set; }

        [BsonElement("jobWheelMachine")]
        public bool? jobWheelMachine { get; set; }

        [BsonElement("jobWheelMachineText")]
        public string jobWheelMachineText { get; set; }

        [BsonElement("jobWheelMachineType")]
        public string jobWheelMachineType { get; set; }

        [BsonElement("jobWheelMachineQty")]
        public int? jobWheelMachineQty { get; set; }

        [BsonElement("jobWheelMachineUnitPrice")]
        [BsonRepresentation(BsonType.Double, AllowTruncation = true)]
        public float? jobWheelMachineUnitPrice { get; set; }

        [BsonElement("jobWheelMachinePrice")]
        [BsonRepresentation(BsonType.Double, AllowTruncation = true)]
        public float? jobWheelMachinePrice { get; set; }

        [BsonElement("jobTyre")]
        public bool? jobTyre { get; set; }

        [BsonElement("jobTyreText")]
        public string jobTyreText { get; set; }

        [BsonElement("jobTyreType")]
        public string jobTyreType { get; set; }

        [BsonElement("jobTyreQty")]
        public int? jobTyreQty { get; set; }

        [BsonElement("jobTyreUnitPrice")]
        [BsonRepresentation(BsonType.Double, AllowTruncation = true)]
        public float? jobTyreUnitPrice { get; set; }

        [BsonElement("jobTyrePrice")]
        [BsonRepresentation(BsonType.Double, AllowTruncation = true)]
        public float? jobTyrePrice { get; set; }

        [BsonElement("jobFreight")]
        [BsonRepresentation(BsonType.Double, AllowTruncation = true)]
        public float? jobFreight { get; set; }

        [BsonElement("jobSubTotal")]
        [BsonRepresentation(BsonType.Double, AllowTruncation = true)]
        public float? jobSubTotal { get; set; }

        [BsonElement("jobGST")]
        [BsonRepresentation(BsonType.Double, AllowTruncation = true)]
        public float? jobGST { get; set; }

        [BsonElement("jobTOTAL")]
        [BsonRepresentation(BsonType.Double, AllowTruncation = true)]
        public float? jobTOTAL { get; set; }

        [BsonElement("jobCompleted")]
        public bool? jobCompleted { get; set; }

        [BsonElement("jobCollected")]
        public bool? jobCollected { get; set; }

        [BsonElement("jobBusinessName")]
        public string jobBusinessName { get; set; }

        [BsonElement("jobGoodReserved")]
        public bool? jobGoodReserved { get; set; }

        [BsonElement("jobQuotation")]
        public bool? jobQuotation { get; set; }
    }
    public class DataAccess
    {
        private static IMongoClient _client = null;
        private static IMongoDatabase _database = null;
        private static IMongoDatabase _settingsdatabase = null;
        private static IMongoCollection<SettingsSettingsDoc> _settings = null;
        private static IMongoCollection<PricingDoc> _pricing = null;
        private static IMongoCollection<JobCardDoc> _jobCard = null;
        private static IMongoCollection<FussyCustomerDoc> _fussyCustomer = null;
        public static void connectMongoDb(string[] args)
        {
           
            if (_client == null)
            {

                try {
                    string prompt = "Please enter dB IP address (your country is: " + (JobTypePopup.isCanada() ? "Canada" : "New Zealand") + ")";
                    string ip = MongoIPAddressInputDialog.ShowInputDialog(prompt, "Database Connection");

                    MongoClientSettings settings = MongoClientSettings.FromConnectionString("mongodb://" + ip + ":27017");
                    settings.ConnectTimeout = TimeSpan.FromSeconds(5);
                    settings.ServerSelectionTimeout = TimeSpan.FromSeconds(5);
                    _client = new MongoClient(settings);

                    var databaseNames = _client.ListDatabaseNames().ToList();

                    if (databaseNames.Count == 0)
                    {
                        throw new Exception("Invalid ip address or server not running!");
                    }
                    string databaseName = "plating";
                    if (JobTypePopup.isWheelApp())
                    {
                        databaseName = "wheel";
                    }
                    //var lists = await _client.ListDatabaseNamesAsync();
                    _database = _client.GetDatabase(databaseName);
                    _settingsdatabase = _client.GetDatabase("settings");
                    _jobCard = _database.GetCollection<JobCardDoc>("jobCard");
                    _pricing = _database.GetCollection<PricingDoc>("pricing");
                    _fussyCustomer = _database.GetCollection<FussyCustomerDoc>("fussyCustomer");
                    _settings = _settingsdatabase.GetCollection<SettingsSettingsDoc>("settings");
                    Application.Run(new JobCard(args));
                } catch (Exception err)
                {
                    ShowError("Cannot connect to dB. IP address wrong or firewalled");
                    Application.Exit();
                    Application.ExitThread();

                    Environment.Exit(0);
                }
                finally
                {
                    if (_client == null || _database == null || _jobCard == null || _fussyCustomer == null)
                    {
                        ShowError("Mongo vital database is null ");
                        Application.Exit();
                        Application.ExitThread();

                        Environment.Exit(0);
                    }
                }  
            }            
        }

        public static async Task CreateJobAsync(JobCardDoc newDoc)
        {
            await DataAccess._jobCard.InsertOneAsync(newDoc);
        }

        public static async Task<SettingsSettingsDoc> findSettings()
        {
            try
            {
                var result = await DataAccess._settings.Find(new BsonDocument() { }).ToListAsync();
                if (result.Count > 0)
                {
                    return result[0];
                }
            } catch (Exception err)
            {
                ShowError("Failed to find settings.settings");
            }
                return null;
        }
        public static int increment = 1;
        public static async Task<string> findOrUpdatePrice(Control control, TextBox overridePrice, TextBox overrideControlText)
        {
            string controlName = control.Name;
            string controlText = control.Text.Trim();
            int dollarIndex = controlText.LastIndexOf('$');
            bool needUpdateOrInsert = (overridePrice != null && overridePrice.Text.Trim() != "") || (overrideControlText != null && overrideControlText.Text.Trim() != "");
            string amount = "$0";
            if (dollarIndex > 0)
            {
                amount = controlText.Substring(dollarIndex).Trim();
                controlText = controlText.Substring(0, dollarIndex).Trim();
            }
            PricingDoc found = null;
            var filters = new List<FilterDefinition<PricingDoc>>();
            filters.Add(Builders<PricingDoc>.Filter.Eq("controlName", controlName));
            filters.Add(Builders<PricingDoc>.Filter.Eq("isWheel", JobTypePopup.isWheelApp()));
            var builder = Builders<PricingDoc>.Filter;
            var finalFilter = builder.And(filters);
            var result = await DataAccess._pricing.Find(finalFilter).ToListAsync();
            if (result.Count == 1)
            {
                found = result[0];
                amount = found.stringPrice;
            } 

            if (found == null)
            {
                needUpdateOrInsert = true;
                found = new PricingDoc();
                found.isWheel = JobTypePopup.isWheelApp();
                found.Id = new ObjectId(DateTime.Now, 12345, 0, increment++);
                if (increment > 998)
                {
                    increment = 0;
                }
                found.stringPrice = (overridePrice == null || overridePrice.Text.Trim() == "") ? amount : overridePrice.Text.Trim();
                found.controlName = controlName;
                found.controlText = controlText; 
            }
            if (needUpdateOrInsert)
            {
                found.stringPrice = (overridePrice != null && overridePrice.Text.Trim() != "") ? overridePrice.Text.Trim() : amount;
                found.controlText = (overrideControlText != null && overrideControlText.Text.Trim() != "") ? overrideControlText.Text.Trim() : controlText;
                UpdateOptions updateOptions = new UpdateOptions();
                updateOptions.IsUpsert = true;
                var updateResult = await DataAccess._pricing.ReplaceOneAsync(finalFilter, found, updateOptions);
                if ((updateResult.IsModifiedCountAvailable && updateResult.ModifiedCount > 0) || (updateResult.IsAcknowledged && updateResult.UpsertedId != null))
                {
                    //System.Console.Out.WriteLine("updated text "+found.controlText);
                }
            }
            if (overrideControlText != null)
            {

                overrideControlText.Text = "";
            }
            if (overridePrice != null)
            {
                overridePrice.Text = "";
            }
            control.Text = found.controlText;
            //System.Console.Out.WriteLine(found.controlName + " = " + found.controlText);
            if (found.stringPrice.StartsWith("$"))
            {
                found.stringPrice = found.stringPrice.Substring(1);
            }
            return found.stringPrice;
        }

        public static async Task<List<JobCardDoc>> findJobByFilterAsync(DataGridView datagrid, FilterDefinition<JobCardDoc> filter, string sortByField = "jobID", bool sortDescending = true, int skip = 0, int limit = 1)
        {

            var result = await DataAccess._jobCard.Find(filter).Sort(new BsonDocument(sortByField, sortDescending ? -1 : 1))
                                            .Skip(skip).Limit(limit)
                                            .ToListAsync();
            BindingList<JobCardDoc> doclist = new BindingList<JobCardDoc>();
            foreach (var doc in result)
            {
                doclist.Add(doc);
            }
            datagrid.DataSource = doclist;
            return result;
        }
        public static async Task<int> GetLastJobIDAsync()
        {
            var filter = new BsonDocument(); //Builders<JobCardDoc>.Filter.Ne("jobID", BsonNull.Value);
            
            var result = await DataAccess._jobCard.Find(filter).Sort(new BsonDocument("jobID" , -1))                                     
                                            .Limit(1)
                                            .ToListAsync();
            if (result != null && result.Count == 1)
            {
                return result[0].jobID;
            }
            ShowError("Failed to find last jobID - app will quit");
            Application.Exit();
            Application.ExitThread();
            Environment.Exit(0);
            return 0;
        }
        public static async Task migrateJobCardAsync()
        {
            long recs = 0;
            try {
                recs = DataAccess._jobCard.EstimatedDocumentCount();
            } catch (Exception exc)
            {

            }
            if (recs > 0)
            {
                // PJC REMOVE
                try {
                    var deleteMe = DataAccess._jobCard.Find(new BsonDocument() { }).ToList();
                } catch (Exception err)
                {
                    System.Console.WriteLine("Already migrated jobs");
                }
                System.Console.WriteLine("Already migrated jobs");
                return;
            }
            string sql = "SELECT * FROM " + JobCard.DBTable;
            var rows = DataAccess.ReadRecordsJobCard(sql);

            
            var existing = await DataAccess._jobCard.Find(new BsonDocument()).ToListAsync();
            Dictionary<int, bool> existMap = new Dictionary<int, bool>(existing.Count);
            existing.ForEach(x => existMap[x.jobID] = true);
            System.Console.WriteLine("FOUND EXISTING COUNT " + existing.Count);
            Type type = typeof(JobCardDoc);
            int count = rows.Count;
            if (count != 0)
            {
                for (int num = 0; num < count; num++)
                {
                    DataRow row = rows[num];
                    var percentage = 100 * num / count;
                    System.Console.WriteLine("Migrating row " + (num + 1) + " - " + percentage + "% complete");
                    JobCardDoc newDoc = new JobCardDoc();

                    int jobID = -1;
                    foreach (DataColumn c in row.Table.Columns)
                    {
                        int columnIndex = c.Ordinal;
                        string name = c.ColumnName;
                        object obj2 = row.ItemArray[columnIndex];
                        try
                        {

                            if (obj2 != null && obj2.GetType() == typeof(System.DBNull))
                            {
                                obj2 = null;
                            }
                            var p = type.GetProperty(name);
                            if (name == "jobID")
                            {
                                try
                                {
                                    jobID = (int)obj2;
                                }
                                catch (Exception exc)
                                {
                                    System.Console.WriteLine("Error!" + exc.ToString());
                                }
                            }
                            if (p != null)
                            {
                                p.SetValue(newDoc, obj2);
                            }
                            else {
                                System.Console.WriteLine("Error!");
                            }
                        }
                        catch (Exception err)
                        {
                            System.Console.WriteLine("err" + err.ToString());
                        }
                    }
                    bool foundExisitng = false;
                    if (existMap.TryGetValue(jobID, out foundExisitng))
                    {

                        System.Console.WriteLine("Already migrated jobID# " + jobID);
                    }
                    else {
                        if (jobID == -1)
                        {
                            System.Console.WriteLine("Unknown doc", newDoc.ToJson());
                        }
                        else
                        {
                            await DataAccess.CreateJobAsync(newDoc);
                        }

                    }
                }
                var options = new CreateIndexOptions() { Unique = true };
                var jobCardIndex = new IndexKeysDefinitionBuilder<JobCardDoc>().Ascending(c => c.jobID);
                var jobCardIndexModel = new CreateIndexModel<JobCardDoc>(jobCardIndex, options);
                await DataAccess._jobCard.Indexes.CreateOneAsync(jobCardIndexModel);//Exception happens at this line

                options = new CreateIndexOptions() { Unique = false };
                jobCardIndex = new IndexKeysDefinitionBuilder<JobCardDoc>().Ascending(c => c.jobBusinessName);
                jobCardIndexModel = new CreateIndexModel<JobCardDoc>(jobCardIndex, options);
                await DataAccess._jobCard.Indexes.CreateOneAsync(jobCardIndexModel);//Exception happens at this line

                jobCardIndex = new IndexKeysDefinitionBuilder<JobCardDoc>().Ascending(c => c.jobCustomer);
                jobCardIndexModel = new CreateIndexModel<JobCardDoc>(jobCardIndex, options);
                await DataAccess._jobCard.Indexes.CreateOneAsync(jobCardIndexModel);//Exception happens at this line

                jobCardIndex = new IndexKeysDefinitionBuilder<JobCardDoc>().Ascending(c => c.jobPhone);
                jobCardIndexModel = new CreateIndexModel<JobCardDoc>(jobCardIndex, options);
                await DataAccess._jobCard.Indexes.CreateOneAsync(jobCardIndexModel);//Exception happens at this line

                jobCardIndex = new IndexKeysDefinitionBuilder<JobCardDoc>().Ascending(c => c.jobDetail00);
                jobCardIndexModel = new CreateIndexModel<JobCardDoc>(jobCardIndex, options);
                await DataAccess._jobCard.Indexes.CreateOneAsync(jobCardIndexModel);//Exception happens at this line

                jobCardIndex = new IndexKeysDefinitionBuilder<JobCardDoc>().Ascending(c => c.jobEmail);
                jobCardIndexModel = new CreateIndexModel<JobCardDoc>(jobCardIndex, options);
                await DataAccess._jobCard.Indexes.CreateOneAsync(jobCardIndexModel);//Exception happens at this line

                jobCardIndex = new IndexKeysDefinitionBuilder<JobCardDoc>().Ascending(c => c.jobDate);
                jobCardIndexModel = new CreateIndexModel<JobCardDoc>(jobCardIndex, options);
                await DataAccess._jobCard.Indexes.CreateOneAsync(jobCardIndexModel);//Exception happens at this line

                System.Console.WriteLine("Migration complete");
                MessageBox.Show("Migration of jobs complete", "Success", MessageBoxButtons.OK, MessageBoxIcon.None);
            }
        }

        public static async Task migrateFussyCustomerAsync()
        {
            long recs = 0;
            try
            {
                recs = DataAccess._fussyCustomer.EstimatedDocumentCount();
            }
            catch (Exception exc)
            {

            }
            if (recs > 0)
            {
                System.Console.WriteLine("Already migrated fussyCustomer");
                return;
            }
            string sql = "SELECT * FROM fussyCustomer";
            DataRowCollection rows = DataAccess.ReadRecordsFussyCustomer(sql);


            var existing = await DataAccess._fussyCustomer.Find(new BsonDocument()).ToListAsync();
            Dictionary<string, bool> existMap = new Dictionary<string, bool>(existing.Count);
            existing.ForEach(x => existMap[x.phoneOrEmail] = true);
            System.Console.WriteLine("FOUND EXISTING COUNT " + existing.Count);
            Type type = typeof(FussyCustomerDoc);
            int count = rows.Count;
            if (count != 0)
            {
                for (int num = 0; num < count; num++)
                {
                    var percentage = 100 * num / count;
                    System.Console.WriteLine("Migrating row " + (num + 1) + " - " + percentage + "% complete");
                    FussyCustomerDoc newDoc = new FussyCustomerDoc();
                    var cells = rows[num].ItemArray;
                    string phoneOrEmail = "";
                    for (int col = 0; col < 1; col++)
                    {
                        string name = "phoneOrEmail";
                        object obj2 = cells[0];
                        
                        try
                        {
                            
                            if (obj2 != null && obj2.GetType() == typeof(System.DBNull))
                            {
                                obj2 = null;
                            }
                            var p = type.GetProperty(name);
                            if (name == "phoneOrEmail")
                            {
                                try
                                {
                                    phoneOrEmail = (string)obj2;
                                }
                                catch (Exception exc)
                                {
                                    System.Console.WriteLine("Error!" + exc.ToString());
                                }
                            }
                            if (p != null)
                            {
                                p.SetValue(newDoc, obj2);
                            }
                            else {
                                System.Console.WriteLine("Error!");
                            }
                        }
                        catch (Exception err)
                        {
                            System.Console.WriteLine("err" + err.ToString());
                        }
                    }
                    bool foundExisitng = false;
                    if (existMap.TryGetValue(phoneOrEmail, out foundExisitng))
                    {

                        System.Console.WriteLine("Already migrated phoneOrEmail# " + phoneOrEmail);
                    }
                    else {
                        if (phoneOrEmail == "")
                        {
                            System.Console.WriteLine("Unknown doc", newDoc.ToJson());
                        }
                        else
                        {
                            var filter = Builders<FussyCustomerDoc>.Filter.Eq("phoneOrEmail", phoneOrEmail);
                            var result = DataAccess._fussyCustomer.Find(filter).ToList();
                            if (result == null || result.Count == 0)
                            {
                                DataAccess._fussyCustomer.InsertOneAsync(newDoc);
                            }
                        }

                    }
                }
            }
            var options = new CreateIndexOptions() { Unique = true };
            var fussyIndex = new IndexKeysDefinitionBuilder<FussyCustomerDoc>().Ascending(c => c.phoneOrEmail);
            var fussyIndexModel = new CreateIndexModel<FussyCustomerDoc>(fussyIndex, options);
            await DataAccess._fussyCustomer.Indexes.CreateOneAsync(fussyIndexModel);//Exception happens at this line
            
            System.Console.WriteLine("Migration complete");
        }


        private static BindingSource binder = new BindingSource();

        public static Image Base64ToImage(string base64)
        {
            try
            {
                using (MemoryStream stream = new MemoryStream(Convert.FromBase64String(base64)))
                {
                    return Image.FromStream(stream);
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show("Failed to convert base64 to image - " + exception.Message);
            }
            return null;
        }

        public static Dictionary<string, System.Type> GetFieldDataTypes()
        {
            Dictionary<string, System.Type> dictionary = new Dictionary<string, System.Type>();
            try
            {
                /* PJC OLD
                OleDbConnection selectConnection = new OleDbConnection(ConnectionString);
                OleDbDataAdapter adapter = new OleDbDataAdapter("Select top 1 * from " + tableName, selectConnection);
                DataSet dataSet = new DataSet();
                selectConnection.Open();
                adapter.Fill(dataSet, tableName + "_table");
                selectConnection9();
                DataColumnCollection columns = dataSet.Tables[0].Columns;
                */
                Type type = typeof(JobCardDoc);
                var props = type.GetProperties();
                foreach (var prop in props)
                {
                    dictionary.Add(prop.Name, prop.PropertyType);
                    Console.WriteLine(prop.Name + " " + prop.PropertyType.ToString());
                }
                
            }
            catch (Exception)
            {
            }
            return dictionary;
        }

        public static string ImageFileToBase64(string path)
        {
            try
            {
                Image image = JobCard.FromFile(path);
                using (MemoryStream stream = new MemoryStream())
                {
                    image.Save(stream, ImageFormat.Jpeg);
                    return Convert.ToBase64String(stream.ToArray());
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show("Failed to get image into string " + path + " - " + exception.Message);
            }
            return null;
        }

        public static string ImageToBase64(Image image)
        {
            try
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    image.Save(stream, ImageFormat.Jpeg);
                    return Convert.ToBase64String(stream.ToArray());
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show("Failed to get image" + exception.Message);
            }
            return null;
        }

        public static string StripPhoneAndEmailToSqlSuitable(string phone, string email)
        {            
            List<string> all = new List<string>();
            phone = phone.Trim();
            email = email.Trim();
            int len = 0;
            int nonLen = 0;
        
            string x = "";
            for (int i=0; i < phone.Length; i++)
            {
                var c = phone[i];
                switch (c)
                {
                    case '0':
                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                    case '8':
                    case '9':
                        len++;
                        nonLen = 0;
                        x = x + c;
                        break;
                    case ' ':
                        nonLen++;
                        if (len >= 9)
                        {
                            all.Add(x);
                            x = "";
                            len = 0;
                        }
                        break;
                    default:
                        nonLen++;
                        break;
                }
                if (nonLen >= 2)
                {
                    len = 0;
                    x = "";
                }                
            }
            if (len >= 9)
            {
                all.Add(x);
            }
            string retVal = "";
            for (int i=0; i < all.Count; i++)
            {
                string s = all[i];
                retVal += ((i > 0) ? ",'" : "'") + s + "'"; 
            }
            return retVal;
        }

        public static bool isFussyCustomers(string phone, string email)
        {
            int count = 0;
            try
            {
                FilterDefinitionBuilder<FussyCustomerDoc> filter = Builders<FussyCustomerDoc>.Filter;
                var filterIn = filter.In(c => c.phoneOrEmail, new[] { StripPhoneAndEmailToSqlSuitable(phone, email) });

                // Now you can use this filter in your MongoDB queries
                var matchingCustomers = DataAccess._fussyCustomer.Count(filterIn);
                if (matchingCustomers > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            } catch (Exception err)
            {
                return false;
            }
        }

        public static DataRowCollection ReadRecords(string sql)
        {
            OleDbConnection selectConnection = null;
            try
            {
                try
                {
                    selectConnection = new OleDbConnection(ConnectionString);
                    OleDbDataAdapter adapter = new OleDbDataAdapter(sql, selectConnection);
                    DataSet dataSet = new DataSet();
                    selectConnection.Open();
                    adapter.Fill(dataSet, "jobs_table");
                    selectConnection.Close();
                    return dataSet.Tables[0].Rows;
                }
                catch (Exception exception)
                {
                    if (selectConnection != null)
                    {
                        selectConnection.Close();
                    }
                    ShowError(exception.Message);
                }
            }
            finally
            {
            }
            return null;
        }

        public static DataRowCollection ReadRecordsJobCard(string sql)
        {
            OleDbConnection selectConnection = null;
            DataRowCollection returnResult = null;
            try
            {
                try
                {
                    selectConnection = new OleDbConnection(ConnectionString);
                    OleDbDataAdapter adapter = new OleDbDataAdapter(sql, selectConnection);
                    DataSet dataSet = new DataSet();
                    selectConnection.Open();
                    adapter.Fill(dataSet, "jobs_table");
                    selectConnection.Close();
                    returnResult = dataSet.Tables[0].Rows;
                }
                catch (Exception exception)
                {
                    if (selectConnection != null)
                    {
                        selectConnection.Close();
                    }
                    ShowError(exception.Message);
                }
            }
            finally
            {
            }
            return returnResult;
        }

        public static DataRowCollection ReadRecordsFussyCustomer(string sql)
        {
            OleDbConnection selectConnection = null;
            DataRowCollection returnResult = null;
            try
            {
                try
                {
                    selectConnection = new OleDbConnection(ConnectionString);
                    OleDbDataAdapter adapter = new OleDbDataAdapter(sql, selectConnection);
                    DataSet dataSet = new DataSet();
                    selectConnection.Open();
                    adapter.Fill(dataSet, "fussyCustomer_table");
                    selectConnection.Close();
                    returnResult = dataSet.Tables[0].Rows;
                }
                catch (Exception exception)
                {
                    if (selectConnection != null)
                    {
                        selectConnection.Close();
                    }
                    ShowError(exception.Message);
                }
            }
            finally
            {
            }
            return returnResult;
        }

        public static void ReadRecords(DataGridView datagrid, string sql)
        {
            OleDbConnection selectConnection = null;
            try
            {
                try
                {
                    selectConnection = new OleDbConnection(ConnectionString);
                    OleDbDataAdapter adapter = new OleDbDataAdapter(sql, selectConnection);
                    DataSet dataSet = new DataSet();
                    selectConnection.Open();
                    adapter.Fill(dataSet, "jobs_table");
                    int count = 0;
                    if (dataSet.Tables.Count == 1)
                    {
                        count = dataSet.Tables[0].Rows.Count;
                    }
                    selectConnection.Close();
                    datagrid.DataSource = dataSet;
                    datagrid.DataMember = "jobs_table";
                }
                catch (Exception exception)
                {
                    if (selectConnection != null)
                    {
                        selectConnection.Close();
                    }
                    MessageBox.Show("Query failed " + exception.Message);
                }
            }
            finally
            {
            }
        }

        public static object ReadSingleValue(string sql)
        {
            OleDbConnection selectConnection = null;
            try
            {
                try
                {
                    selectConnection = new OleDbConnection(ConnectionString);
                    OleDbDataAdapter adapter = new OleDbDataAdapter(sql, selectConnection);
                    DataSet dataSet = new DataSet();
                    selectConnection.Open();
                    adapter.Fill(dataSet, "jobs_table");
                    selectConnection.Close();
                    int num = 0;
                    while (num < dataSet.Tables[0].Rows.Count)
                    {
                        return dataSet.Tables[0].Rows[num][0];
                    }
                }
                catch (Exception exception)
                {
                    if (selectConnection != null)
                    {
                        selectConnection.Close();
                    }
                    ShowError(exception.Message);
                }
            }
            finally
            {
            }
            return null;
        }

        private static void ShowError(string msg)
        {
            MessageBox.Show(msg, "Database connection error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
        }

        public static async Task<List<JobCardDoc>> FindJobByFieldAsync(DataGridView datagrid, string fieldName, dynamic fieldValue, bool sortDescending = true, int limit = 1, int skip = 0)
        {
            var fields = typeof(JobCardDoc).GetProperties();
            FilterDefinitionBuilder<JobCardDoc> filter = Builders<JobCardDoc>.Filter;
            FilterDefinition<JobCardDoc> filterDef = new BsonDocument();
            bool found = false;
            for (int col = 0; col < fields.Length; col++)
            {
                if (fields[col].Name == fieldName)
                {
                    Type type = fields[col].PropertyType;
                    filterDef = filter.Eq(fieldName, fieldValue);
                    found = true;
                    break;
                }
            }
            if (found)
            {
                var result = await DataAccess._jobCard.Find<JobCardDoc>(filterDef).
                    Skip(skip).
                    Limit(limit).
                    Sort(new BsonDocument(fieldName, sortDescending ? -1 : 1)).
                    ToListAsync();
                BindingList<JobCardDoc> doclist = new BindingList<JobCardDoc>();
                foreach (var doc in result)
                {
                    doclist.Add(doc);
                }
                datagrid.DataSource = doclist;
                return result;
            } else
            {
                datagrid.DataSource = null;
                return null;
            }

        }

        public static async Task<bool> UpdateMongoAsync(List<KeyValuePair<string, dynamic>> fields)
        {
            if (fields != null && fields.Count > 0)
            {
                int jobID = 0;
                var updateList = new List<UpdateDefinition<JobCardDoc>>();
             
                
                fields.ForEach(x =>
                    {
                        UpdateDefinition<JobCardDoc> update = null;
                        if (x.Key == "jobID")
                        {
                            jobID = x.Value;
                        } else
                        {
                            try {
                                if (x.Value == null)
                                {
                                   update = Builders<JobCardDoc>.Update.Set<System.DBNull>(x.Key, null);
                                }
                                else {
                                   update = Builders<JobCardDoc>.Update.Set(x.Key, x.Value);
                                }
                            } catch (Exception err)
                            {
                                MessageBox.Show("Invalid field " + x.Key + " value ");
                            }
                        }
                        if (update != null)
                        {
                            updateList.Add(update);
                        }
                    }
                );
                if (jobID == 0)
                {
                    ShowError("invalid jobID");
                    return false;
                }
                var filter = Builders<JobCardDoc>.Filter.Eq("jobID", jobID);
                UpdateOptions options = new UpdateOptions();
                //options.BypassDocumentValidation = true;
                var finalUpdate = Builders<JobCardDoc>.Update.Combine(updateList);
                var result = await DataAccess._jobCard.UpdateOneAsync(filter, finalUpdate);
                System.Console.WriteLine("Update result", result);
                return result.IsAcknowledged;
            } else
            {
                return false;
            }
        }

        public static bool Update(string sql)
        {
            int num = 0;
            OleDbConnection connection = null;
            try
            {
                connection = new OleDbConnection(ConnectionString);
                
                connection.Open();
                using (OleDbCommand command = connection.CreateCommand())
                {
                    command.CommandType = CommandType.Text;
                    command.CommandText = sql;
                    num = command.ExecuteNonQuery();
                }
                connection.Close();
                if (num == 0)
                {
                    //MessageBox.Show("Error No records updated");
                    num = 1;
                    //throw new Exception("Failed to update " + sql);
                }
                
            }
            catch (Exception exception)
            {
                if (connection != null)
                {
                    if (exception.Message.Contains("Null"))
                    {
                        using (OleDbCommand command = connection.CreateCommand())
                        {
                            sql = sql.Replace("null", "\"\"");
                            command.CommandType = CommandType.Text;
                            command.CommandText = sql;
                            try
                            {
                                num = command.ExecuteNonQuery();
                            }
                            catch (Exception err)
                            {

                            }
                        }                        
                    }
                    connection.Close();
                }
                if (num == 0)
                {
                    MessageBox.Show("Failed to update error " + exception.Message);
                    //ShowError(exception.Message);
                    num = 1;
                }                
            }
            return (num > 0);
        }

        public static async Task<bool> DeleteJobAsync(int jobID)
        {
            var filter = Builders<JobCardDoc>.Filter.Eq("jobID", jobID);
            var result = await DataAccess._jobCard.DeleteOneAsync(filter);
            return result.DeletedCount != 0;
        }

        public static void InsertFussyCustomer(string phone, string email = "")
        {
            int num = 0;
            OleDbConnection connection = null;
            try
            {
                connection = new OleDbConnection(ConnectionString);

                connection.Open();
                using (OleDbCommand command = connection.CreateCommand())
                {
                    command.CommandType = CommandType.Text;
                    string phones = StripPhoneAndEmailToSqlSuitable(phone, email);
                    if (phones != "")
                    {
                        string[] split = phones.Split(',');
                        for (int i = 0; i < split.Length; i++)
                        {
                            try
                            {
                                command.CommandText = "INSERT INTO fussyCustomer VALUES (" + split[i] + ")";
                                num = command.ExecuteNonQuery();
                            }
                            catch (Exception e)
                            {

                            }
                        }
                    }
                }
                connection.Close();
                if (num == 0)
                {
                    //MessageBox.Show("Error No records updated");
                    num = 1;
                    //throw new Exception("Failed to update " + sql);
                }

            }
            catch (Exception exception)
            {
            }
        }

        private static string ConnectionString =>
            ("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + JobCard.DBPath + ";User Id=admin;Password=;");
    }
}

