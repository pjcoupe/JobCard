namespace Job_Card
{
    using System;
    using System.Windows.Forms;

    internal static class Program
    {
        [STAThread]
        private static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            try {
                 DataAccess.connectMongoDb(args);
                
            } catch (Exception err)
            {
                var message = err.Message;
                if (err.InnerException != null)
                {
                    message += " INNER: " + err.InnerException.Message + " LINE >>>" + err.InnerException.StackTrace;
                }
                MessageBox.Show("The Application will exit message:" + message);
            }
        }
    }
}

