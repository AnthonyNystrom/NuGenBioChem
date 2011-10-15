using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using NuGenBioChem.Data;


namespace NuGenBioChem
{
    /// <summary>
    /// Entry point in the application
    /// </summary>
    public partial class Application : System.Windows.Application
    {


        static Application()
        {
            string[] commandLineArgs = Environment.GetCommandLineArgs();
            // Handles command to clear all data in the storage (styles, settings, etc.)
            if (commandLineArgs.Length > 1 && commandLineArgs[1] == "-clearstorage")
            {
                Storage.Clear();
                Process.GetCurrentProcess().Kill();
            }

            // Initialize single instance
            SingleInstance.Initialize();            
            
            // Show splash screen
            SplashScreen splashScreen = new SplashScreen("Images\\Splash.png");
            splashScreen.Show(true, true);
        }

        // Handles application startup event
        void OnStartup(object sender, StartupEventArgs e)
        {
        }

        // Handles application exit
        private void OnApplicationExit(object sender, ExitEventArgs e)
        {
            // Deinitialize single instance
            SingleInstance.Deinitialize(); 
        }               
    }
}
