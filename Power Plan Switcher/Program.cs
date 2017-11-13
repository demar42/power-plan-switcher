using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Taskbar_Test
{
    static class Program
    {
        enum PowerDataAccessor : uint
        {
            ACCESS_SCHEME = 16
        }

        [DllImport("powrprof.dll")]
        private static extern uint PowerEnumerate(IntPtr RootPowerKey,
                    IntPtr SchemeGuid,
                    IntPtr SubGroupOfPowerSettingsGuid,
                    PowerDataAccessor AccessFlags,
                    uint Index,
                    IntPtr Buffer,
                    ref uint BufferSize);

        [DllImport("powrprof.dll")]
        private static extern uint PowerReadFriendlyName(IntPtr RootPowerKey,
                    ref Guid SchemeGuid,
                    IntPtr SubGroupOfPowerSettingGuid,
                    IntPtr PowerSettingGuid,
                    IntPtr Buffer,
                    ref UInt32 BufferSize);

        [DllImport("powrprof.dll")]
        private static extern uint PowerGetActiveScheme(IntPtr UserRootPowerKey,
                    ref IntPtr ActivePolicyGuid);

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            List<Guid> GUIDs = new List<Guid>();
            List<String> names = new List<String>();

            uint returnCode = 0;

            uint index = 0;
            uint buffersize = 16;
            IntPtr readbuffer;

            while (returnCode == 0)
            {
                readbuffer = Marshal.AllocHGlobal((int)buffersize);

                returnCode = PowerEnumerate(IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, PowerDataAccessor.ACCESS_SCHEME, index, readbuffer, ref buffersize);
                if (returnCode == 259) break;

                GUIDs.Add((Guid)Marshal.PtrToStructure(readbuffer, typeof(Guid)));

                uint reqSize = 0;
                Guid planguid = GUIDs[(int)index];
                String name = "Unnamed";

                returnCode = PowerReadFriendlyName(IntPtr.Zero, ref planguid, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, ref reqSize);
                if (returnCode == 0)
                {
                    //Request was successful
                    if (reqSize == 0u) name = "";
                    else
                    {
                        IntPtr ptrName = Marshal.AllocHGlobal((int)reqSize);
                        returnCode = PowerReadFriendlyName(IntPtr.Zero, ref planguid, IntPtr.Zero, IntPtr.Zero, ptrName, ref reqSize);
                        if (returnCode == 0) name = Marshal.PtrToStringUni(ptrName);
                        Marshal.FreeHGlobal(ptrName);
                    }
                    names.Add(name);
                }

                Marshal.FreeHGlobal(readbuffer);

                Console.WriteLine(GUIDs[(int)index]);
                Console.WriteLine(names[(int)index]);

                index++;
            }

            IntPtr activeGuid = IntPtr.Zero;
            PowerGetActiveScheme(IntPtr.Zero, ref activeGuid);

            Guid active = (Guid)Marshal.PtrToStructure(activeGuid, typeof(Guid));

            using (ProcessIcon pi = new ProcessIcon(GUIDs, names, active))
            {
                pi.Display();

                Application.Run();
            }
        }
    }
}
