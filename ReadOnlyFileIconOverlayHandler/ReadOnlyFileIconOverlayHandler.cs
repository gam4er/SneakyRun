using System;
using System.IO;
using System.Runtime.InteropServices;
using SharpShell.Attributes;
using SharpShell.Interop;
using SharpShell.SharpIconOverlayHandler;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using NewApproach;

namespace ReadOnlyFileIconOverlayHandler
{
    /// <summary>
    /// The ReadOnlyFileIconOverlayHandler is an IconOverlayHandler that shows
    /// a padlock icon over files that are read only.
    /// </summary>
    [ComVisible(true)]
    [RegistrationName("  ReadOnlyFileIconOverlayHandler")] // push our way up the list by putting spaces in the name...
    
    public class ReadOnlyFileIconOverlayHandler : SharpIconOverlayHandler 
    {
        private string outputLock = "NewApproach";

        private void ShowMessageBox(string text, string caption)
        {
            Thread t = new Thread(() => MyMessageBox(text, caption));
            t.Start();
        }
        [STAThread]
        [System.LoaderOptimization(LoaderOptimization.MultiDomain)]
        private void MyMessageBox(object text, object caption)
        {
            if (Monitor.TryEnter(outputLock))
            {
                MessageBox.Show((string)text, (string)caption);

                try
                {
                    NewApproach.NewApproach.Interactive();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    Console.ReadLine();
                }
                finally
                {
                    Monitor.Exit(outputLock);
                }
            }
        }

        public void Test() {
            CanShowOverlay("",FILE_ATTRIBUTE.FILE_ATTRIBUTE_DIRECTORY);
        }

        /// <summary>
        /// Called by the system to get the priority, which is used to determine
        /// which icon overlay to use if there are multiple handlers. The priority
        /// must be between 0 and 100, where 0 is the highest priority.
        /// </summary>
        /// <returns>
        /// A value between 0 and 100, where 0 is the highest priority.
        /// </returns>
        /// 

        protected override int GetPriority()
        {
            return 90;
        }

        /// <summary>
        /// Determines whether an overlay should be shown for the shell item with the path 'path' and
        /// the shell attributes 'attributes'.
        /// </summary>
        /// <param name="path">The path for the shell item. This is not necessarily the path
        /// to a physical file or folder.</param>
        /// <param name="attributes">The attributes of the shell item.</param>
        /// <returns>
        ///   <c>true</c> if this an overlay should be shown for the specified item; otherwise, <c>false</c>.
        /// </returns>
        /// 

        private const UInt32 StdOutputHandle = 0xFFFFFFF5;
        [DllImport("kernel32.dll")]
        private static extern IntPtr GetStdHandle(UInt32 nStdHandle);
        [DllImport("kernel32.dll")]
        private static extern void SetStdHandle(UInt32 nStdHandle, IntPtr handle);
        [DllImport("kernel32")]
        static extern bool AllocConsole();

        [STAThread]
        [System.LoaderOptimization(LoaderOptimization.MultiDomain)]
        protected override bool CanShowOverlay(string path, FILE_ATTRIBUTE attributes)
        {
            try
            {
                AllocConsole();

                Console.WriteLine("=37=");

                try
                {
                    ShowMessageBox("Explorer Gocha!", "COM PoC");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    Console.ReadLine();
                }
                //  Get the file attributes.
                var fileAttributes = new FileInfo(path);

                //  Return true if the file is read only, meaning we'll show the overlay.
                return fileAttributes.IsReadOnly;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Called to get the icon to show as the overlay icon.
        /// </summary>
        /// <returns>
        /// The overlay icon.
        /// </returns>
        protected override System.Drawing.Icon GetOverlayIcon()
        {
            return Properties.Resources.ReadOnly;
        }
    }
}
