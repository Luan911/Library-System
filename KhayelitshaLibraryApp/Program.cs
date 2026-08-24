namespace KhayelitshaLibraryApp
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            // Global handlers to prevent the app from terminating unexpectedly and to
            // provide user-friendly error messages.
            Application.ThreadException += (sender, e) =>
            {
                try
                {
                    MessageBox.Show($"An unexpected error occurred:\n\n{e.Exception.Message}",
                        "Application Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch { }
            };

            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                try
                {
                    if (e.ExceptionObject is Exception ex)
                    {
                        MessageBox.Show($"A critical error occurred:\n\n{ex.Message}",
                            "Critical Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                    {
                        MessageBox.Show("A critical non-CLS exception occurred.", "Critical Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch { }
            };

            if (!DatabaseHelper.TestConnection(out string error))
            {
                MessageBox.Show(
                    "Could not connect to the library database.\n\n" +
                    error + "\n\n" +
                    "Please ensure SQL Server LocalDB is running and that you have executed " +
                    "Khayelitsha_Community_Library_DB.sql (or run the SetupDatabase project).",
                    "Database Connection Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }

            Application.Run(new MainForm());
        }
    }
}
