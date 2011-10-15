using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Threading;
using NuGenBioChem.Data;

namespace NuGenBioChem
{
    /// <summary>
    /// Contains methods for single instance application
    /// </summary>
    static class SingleInstance
    {
        #region Fields

        // Unique identifier
        static readonly string uniqueIdentifier;

        // Mutex for single-instance run
        static Mutex singleInstance;

        #endregion

        #region Constructors

        /// <summary>
        /// Static constructor
        /// </summary>
        static SingleInstance()
        {
            // Create unique name
            Assembly entry = Assembly.GetEntryAssembly();
            string ver = "0.0.0.0";
            string name = "";
            if (entry != null)
            {
                System.Reflection.AssemblyName asmname = entry.GetName();
                if (asmname != null)
                {
                    ver = asmname.Version.ToString();
                    name = asmname.Name;
                }
            }
            uniqueIdentifier = string.Format("Singleton-{0}-{1}", name, ver);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Initialize single instance
        /// </summary>
        public static void Initialize()
        {
            // Single run
            singleInstance = new Mutex(false, uniqueIdentifier);
            if (!singleInstance.WaitOne(1, true))
            {
                IntPtr hWnd = GetHWndOfPrevInstance(Process.GetCurrentProcess().ProcessName);
                string[] commandLineArgs = Environment.GetCommandLineArgs();
                string args = string.Empty;
                for (int i = 1; i < commandLineArgs.Length;i++ )
                {
                    if (i > 1) args += "|";
                    args += commandLineArgs[i];
                }
                if (hWnd != IntPtr.Zero) SendArgs(hWnd, args);
                Process.GetCurrentProcess().Kill();
                return;
            }
        }

        /// <summary>
        /// Deinitialize single instance
        /// </summary>
        public static void Deinitialize()
        {
            singleInstance.Close();
        }

        /// <summary>
        /// Gets args from WM_COPYDATA
        /// </summary>
        /// <param name="window">Window</param>
        /// <param name="lparam">LParam</param>
        public static void GetArgs(Window window, IntPtr lparam)
        {
            CopyDataStruct st = (CopyDataStruct)Marshal.PtrToStructure(lparam, typeof(CopyDataStruct));
            string strData = Marshal.PtrToStringUni(st.lpData);
            if (!string.IsNullOrEmpty(strData))
            {
                window.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                                              (ThreadStart) (() =>
                                                                 {
                                                                     string[] args = strData.Split('|');
                                                                     for (int i = 0; i < args.Length; i++)
                                                                     {
                                                                         window.OpenFile(args[i]);
                                                                     }
                                                                 }));
            }
            else
            {
                window.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)(() =>
                {
                    Window wnd = new Window(true);
                    wnd.Show();
                }));
            }
        }

        #endregion   
     
        #region Private Methods

        /// <summary>
        /// Searches for a previous instance of this app.
        /// </summary>
        /// <returns>
        /// hWnd of the main window of the previous instance
        /// or IntPtr.Zero if not found.
        /// </returns>
        private static IntPtr GetHWndOfPrevInstance(string processName)
        {
            // Get the current process
            Process currentProcess = Process.GetCurrentProcess();
            // Get a collection of the currently active processes with the same name
            Process[] processes = Process.GetProcessesByName(processName);
            // If only one exists then there is no previous instance
            if (processes.Length > 1)
            {
                foreach (Process process in processes)
                {
                    if (process.Id != currentProcess.Id) // Ignore this process
                    {
                        // Weed out apps that have the same exe name but are started from a different filename.
                        if (process.ProcessName == processName)
                        {
                            IntPtr hWnd = IntPtr.Zero;
                            try
                            {
                                // If process does not have a MainWindowHandle then an exception 
                                // will be thrown so catch and ignore the error.
                                hWnd = process.MainWindowHandle;
                            }
                            catch { }
                            // Return if hWnd found.
                            if (hWnd.ToInt32() != 0) return hWnd;
                        }
                    }
                }
            }
            return IntPtr.Zero;
        }

        /// <summary>
        /// Sends string to window by WM_COPYDATA
        /// </summary>
        /// <param name="targetHWnd"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        private static bool SendArgs(IntPtr targetHWnd, string args)
        {
            CopyDataStruct cds = new CopyDataStruct();
            try
            {
                cds.cbData = (args.Length + 1) * 2;
                cds.lpData = LocalAlloc(0x40, cds.cbData);
                Marshal.Copy(args.ToCharArray(), 0, cds.lpData, args.Length);
                cds.dwData = (IntPtr)1;
                SendMessage(targetHWnd, WM_COPYDATA, IntPtr.Zero, ref cds);
            }
            finally
            {
                cds.Dispose();
            }

            return true;
        }

        #endregion

        #region Interop

        /// <summary>
        /// Copy data window message
        /// </summary>
        public const int WM_COPYDATA = 0x004A;

        private struct CopyDataStruct : IDisposable
        {
            public IntPtr dwData;
            public int cbData;
            public IntPtr lpData;

            /// <summary>
            /// Disposes unmanaged resources
            /// </summary>
            public void Dispose()
            {
                if (this.lpData != IntPtr.Zero)
                {
                    LocalFree(this.lpData);
                    this.lpData = IntPtr.Zero;
                }
            }
        }

        /// <summary>
        /// The SendMessage API
        /// </summary>
        /// <param name="hWnd">handle to the required window</param>
        /// <param name="Msg">the system/Custom message to send</param>
        /// <param name="wParam">first message parameter</param>
        /// <param name="lParam">second message parameter</param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        private static extern int SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, ref CopyDataStruct lParam);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr LocalFree(IntPtr p);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr LocalAlloc(int flag, int size);

        #endregion
    }
}
