using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Threading;

namespace OriBot
{
    public partial class frmMain : Form
    {
        [DllImport("user32.dll")]
        static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData,
          int dwExtraInfo);

        private Point cursorOrigin;
        private BackgroundWorker bwWizard;

        public enum MouseEventFlags : uint
        {
            LEFTDOWN = 0x00000002,
            LEFTUP = 0x00000004,
            MIDDLEDOWN = 0x00000020,
            MIDDLEUP = 0x00000040,
            MOVE = 0x00000001,
            ABSOLUTE = 0x00008000,
            RIGHTDOWN = 0x00000008,
            RIGHTUP = 0x00000010,
            WHEEL = 0x00000800,
            XDOWN = 0x00000080,
            XUP = 0x00000100
        }

        public frmMain()
        {
            InitializeComponent();
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            WriteToMsgBox($"------{DateTime.Now.ToString("MM/dd/yyyy")}---------");
            WriteToMsgBox("Starting up...");

            cursorOrigin = Cursor.Position;
            bwWizard = new BackgroundWorker();
            bwWizard.DoWork += new DoWorkEventHandler(bw_findButton_and_click);
            bwWizard.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bw_WizardComplete);
            bwWizard.WorkerReportsProgress = true;
            if (!bwWizard.IsBusy)
            {
                WriteToMsgBox("Finding Solo Battle Button...");
                bwWizard.RunWorkerAsync(Properties.Resources.bmpLogin);
                btnLogin.Enabled = false;
            }
            waitForWizard();
            WriteToMsgBox("Finding Single Battle Button...");
            bwWizard.RunWorkerAsync(Properties.Resources.bmpSingleBattle);

            //all done, enable to button
            btnLogin.Enabled = true;
        }

        /// <summary>
        /// Simulates a mouse click
        /// </summary>
        private void MouseClick()
        {
            Thread.Sleep((new Random()).Next(35, 150));
            mouse_event((uint)MouseEventFlags.LEFTDOWN, 0, 0, 0, 0);
            Thread.Sleep((new Random()).Next(20, 30));
            mouse_event((uint)MouseEventFlags.LEFTUP, 0, 0, 0, 0);
        }

        private void moveMouseToSafeLocation()
        {
            Thread.Sleep((new Random()).Next(1000, 2000));
            Cursor.Position = cursorOrigin;
        }

        /// <summary>
        /// Takes a snapshot of the screen
        /// </summary>
        /// <returns>A snapshot of the screen</returns>
        private Bitmap Screenshot()
        {
            WriteToMsgBox("Taking screenshot...");
            // this is where we will store a snapshot of the screen
            Bitmap bmpScreenshot = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);


            // creates a graphics object so we can draw the screen in the bitmap (bmpScreenshot)
            Graphics g = Graphics.FromImage(bmpScreenshot);

            // copy from screen into the bitmap we created
            g.CopyFromScreen(0, 0, 0, 0, Screen.PrimaryScreen.Bounds.Size);

            // return the screenshot
            return bmpScreenshot;
        }

        /// <summary>
        /// Find the location of a bitmap within another bitmap and return if it was successfully found
        /// </summary>
        /// <param name="bmpNeedle">The image we want to find</param>
        /// <param name="bmpHaystack">Where we want to search for the image</param>
        /// <param name="location">Where we found the image</param>
        /// <returns>If the bmpNeedle was found successfully</returns>
        private bool FindBitmap(Bitmap bmpNeedle, Bitmap bmpHaystack, out Point location)
        {
            for (int outerX = 0; outerX < bmpHaystack.Width - bmpNeedle.Width; outerX++)
            {
                for (int outerY = 0; outerY < bmpHaystack.Height - bmpNeedle.Height; outerY++)
                {
                    for (int innerX = 0; innerX < bmpNeedle.Width; innerX++)
                    {
                        for (int innerY = 0; innerY < bmpNeedle.Height; innerY++)
                        {
                            Color cNeedle = bmpNeedle.GetPixel(innerX, innerY);
                            Color cHaystack = bmpHaystack.GetPixel(innerX + outerX, innerY + outerY);

                            if (cNeedle.R != cHaystack.R || cNeedle.G != cHaystack.G || cNeedle.B != cHaystack.B)
                            {
                                goto notFound;
                            }
                        }
                    }
                    WriteToMsgBox($"Button found at location: {outerX} , {outerY}");
                    location = new Point(outerX, outerY);
                    return true;
                notFound:
                    continue;
                }
            }
            location = Point.Empty;
            return false;
        }


        private void bw_findButton_and_click(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = (BackgroundWorker)sender;
            Bitmap bmpScreenshot = Screenshot();
            Bitmap buttonToFind = (Bitmap)e.Argument;
            Point location;
            bool success = FindBitmap(buttonToFind, bmpScreenshot, out location);
            if (success == false)
            {
                WriteToMsgBox("Could't find the button?!");
                return;
            }

            // move the mouse to login button
            Cursor.Position = location;

            // click
            MouseClick();
            moveMouseToSafeLocation();
            //wait for screen to change
            Thread.Sleep((new Random()).Next(1000, 2000));
            e.Result = true;
        }

        public void WriteToMsgBox(string value)
        {
            //TODO: Move this to Utils class
            if (InvokeRequired)
            {
                this.Invoke(new Action<string>(WriteToMsgBox), new object[] { value });
                return;
            }
            if (msgBox.Text == string.Empty)
            {
                msgBox.Text += value;
            }
            else
            {
                msgBox.Text += "\n" + DateTime.Now.ToString("hh:mm:ss") + ": " + value;
            }
        }
        private void waitForWizard()
        {
            //we will just sit here until the wizard is ready...
            while (bwWizard.IsBusy)
            {
                Application.DoEvents();
            }
        }
        private void frmMain_Load(object sender, EventArgs e)
        {

        }

        private void msgBox_TextChanged(object sender, EventArgs e)
        {

        }

        private void bw_WizardComplete(object sender, RunWorkerCompletedEventArgs e)
        {

        }
    }
}
