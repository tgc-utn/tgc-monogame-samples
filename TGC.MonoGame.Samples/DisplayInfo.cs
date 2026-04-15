using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace TGC.MonoGame.Samples
{
    public static class DisplayInfo
    {
        public static bool TryGetRefreshRate(out int refreshRate)
        {
            refreshRate = 0;
            
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return TryWindows(out refreshRate);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return TryLinux(out refreshRate);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return TryOSX(out refreshRate);
            }

            // platform not supported
            return false;
        }

        private static bool TryWindows(out int refreshRate)
        {
            refreshRate = 0;
            var devMode = new DEVMODE();
            devMode.dmSize = (short)Marshal.SizeOf(devMode);
            var res = EnumDisplaySettings(null, ENUM_CURRENT_SETTINGS, ref devMode);
            if (res)
                refreshRate = devMode.dmDisplayFrequency;

            return res;
        }

        private static bool TryLinux(out int refreshRate)
        {
            refreshRate = 0;
            var process = new Process
            {
                StartInfo = new ProcessStartInfo("xrandr", "--query")
                {
                    RedirectStandardOutput = true,
                    UseShellExecute = false
                }
            };
            try
            {
                process.Start();
                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
                // Parse active mode line, e.g. "1920x1080     60.00*+"
                foreach (string line in output.Split('\n'))
                {
                    if (line.Contains(" connected ") && line.Contains("*"))
                    {
                        var parts = line.Trim().Split(new char[0], StringSplitOptions.RemoveEmptyEntries);
                        for (int i = 0; i < parts.Length; i++)
                        {
                            if (parts[i] == "*" && i > 0)
                            {
                                refreshRate = (int)Math.Round(double.Parse(parts[i - 1]));

                                return true;
                            }
                        }
                    }
                }
            }
            catch
            {
                // xrandr failed / not installed
            }
            return false;
        }
        private static bool TryOSX(out int refreshRate)
        {
            refreshRate = 0;

            var process = new Process
            {
                StartInfo = new ProcessStartInfo("displayplacer", "list")
                {
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            try
            {
                process.Start();
                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
                // Parse "Hertz: 60"
                foreach (string line in output.Split('\n'))
                {
                    if (line.Contains("Hertz:"))
                    {
                        refreshRate = int.Parse(line.Split(':')[1].Trim());

                        return true;
                    }
                }
            }
            catch
            {
                // displayplacer failed / not installed
            }
            return false;
        }

        [DllImport("user32.dll")]
        private static extern bool EnumDisplaySettings(string lpszDeviceName, int iModeNum, ref DEVMODE lpDevMode);

        private const int ENUM_CURRENT_SETTINGS = -1;

        [StructLayout(LayoutKind.Sequential)]
        private struct DEVMODE
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string dmDeviceName;
            public short dmSpecVersion;
            public short dmDriverVersion;
            public short dmSize;
            public short dmDriverExtra;
            public int dmFields;
            public int dmPositionX;
            public int dmPositionY;
            public int dmDisplayOrientation;
            public int dmDisplayFixedOutput;
            public short dmColor;
            public short dmDuplex;
            public short dmYResolution;
            public short dmTTOption;
            public short dmCollate;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string dmFormName;
            public short dmLogPixels;
            public int dmBitsPerPel;
            public int dmPelsWidth;
            public int dmPelsHeight;
            public int dmDisplayFlags;
            public int dmDisplayFrequency;
            public int dmICMMethod;
            public int dmICMIntent;
            public int dmMediaType;
            public int dmDitherType;
            public int dmReserved1;
            public int dmReserved2;
            public int dmPanningWidth;
            public int dmPanningHeight;
        }
    }
}
