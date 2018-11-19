namespace Job_Card
{
    using System;
    using System.Windows.Forms;

    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            try {
                Application.Run(new JobCard());
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

