namespace Xpera;

static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        // To customize application configuration such as set high DPI settings or default font,
        // see https://aka.ms/applicationconfiguration.
        ApplicationConfiguration.Initialize();
        // Every control starts in Inter, even ones made in code, so nothing falls back to the Windows font.
        Application.SetDefaultFont(Typography.Body);
        Application.Run(new LoginForm());
    }    
}