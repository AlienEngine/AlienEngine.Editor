using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace AlienEngine.Editor.UI.Core
{
    public class HighGraphicsEnablementExporter
    {
        /// <summary>
        /// Enables high performace for NVIDIA
        /// graphics cards.
        /// </summary>
        static uint NvOptimusEnablement = 1;

        /// <summary>
        /// Enables high performance for AMD
        /// graphic cards.
        /// </summary>
        static uint AmdPowerXpressRequestHighPerformance = 1;

        const uint PAGE_READWRITE = 0x04;
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool VirtualProtect(IntPtr lpAddress, uint dwSize, uint flNewProtect, out uint lpflOldProtect);

        [DllImport("kernel32.dll", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
        static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

        const uint GET_MODULE_HANDLE_EX_FLAG_UNCHANGED_REFCOUNT = 0x2;
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern bool GetModuleHandleEx(uint dwFlags, string lpModuleName, out IntPtr phModule);

        [DllImport("kernel32.dll")]
        static extern uint GetLastError();

        static HighGraphicsEnablementExporter()
        {
            Assembly thisAssembly = Assembly.GetExecutingAssembly();
            IntPtr myNativeModuleHandle = IntPtr.Zero;

            if (GetModuleHandleEx(GET_MODULE_HANDLE_EX_FLAG_UNCHANGED_REFCOUNT, thisAssembly.ManifestModule.Name, out myNativeModuleHandle))
            {
                IntPtr nvExportAddress = GetProcAddress(myNativeModuleHandle, "NvOptimusEnablement");
                if (nvExportAddress != IntPtr.Zero)
                {
                    uint oldProtect = 0;
                    //make it writable 
                    if (VirtualProtect(nvExportAddress, 4, PAGE_READWRITE, out oldProtect))
                    {
                        unsafe
                        {
                            uint* dwordValuePtr = (uint*)nvExportAddress.ToPointer();
                            //overwrite code that will never be called with the dword the driver is looking for
                            *dwordValuePtr = NvOptimusEnablement;
                        }
                        VirtualProtect(nvExportAddress, 4, oldProtect, out oldProtect);
                       }
                }
                else
                {
                    Console.Error.WriteLine("You didn't hack the MSIL output!");
                    Console.WriteLine(GetLastError());
                }

                IntPtr amdExportAddress = GetProcAddress(myNativeModuleHandle, "AmdPowerXpressRequestHighPerformance");
                if (amdExportAddress != IntPtr.Zero)
                {
                    uint oldProtect = 0;
                    //make it writable 
                    if (VirtualProtect(amdExportAddress, 4, PAGE_READWRITE, out oldProtect))
                    {
                        unsafe
                        {
                            uint* dwordValuePtr = (uint*)amdExportAddress.ToPointer();
                            //overwrite code that will never be called with the dword the driver is looking for
                            *dwordValuePtr = AmdPowerXpressRequestHighPerformance;
                        }
                        VirtualProtect(amdExportAddress, 4, oldProtect, out oldProtect);
                    }
                }
                else
                {
                    Console.Error.WriteLine("You didn't hack the MSIL output!");
                    Console.WriteLine(GetLastError());
                }
            }
            else
            {
                Console.WriteLine(GetLastError());
            }

        }

        private static void NvOptimusEnablementExporter_DontCallThis()
        { }

        private static void AmdPowerXpressRequestHighPerformanceExporter_DontCallThis()
        { }
    }
}
