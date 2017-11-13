using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Taskbar_Test.Properties;

namespace Taskbar_Test
{
    class ProcessIcon : IDisposable
    {
        [DllImport("powrprof.dll")]
        private static extern uint PowerSetActiveScheme(IntPtr UserRootPowerKey,
                    ref Guid SchemeGuid);

        NotifyIcon ni;

        private ContextMenuStrip menu;
        private ToolStripMenuItem item;
        private ToolStripLabel label;
        private ToolStripSeparator sep;

        private List<ToolStripMenuItem> itemList = new List<ToolStripMenuItem>();

        private List<Guid> GUIDs;
        private List<String> names;
        private Guid active;

        public ProcessIcon(List<Guid> guidsIn, List<String> namesIn, Guid activeIn)
        {
            GUIDs = guidsIn;
            names = namesIn;
            active = activeIn;

            ni = new NotifyIcon();
        }

        public void Display()
        {
            ni.Icon = Resources.icon;
            ni.Text = "Power Plan Switcher v.0.1";

            menu = new ContextMenuStrip();
            label = new ToolStripLabel();
            sep = new ToolStripSeparator();

            menu.Name = "Menu";
            menu.Size = new Size(153, 70);

            label.Text = "Power Plan Switcher v.0.1";
            label.Font = new Font(label.Font, FontStyle.Bold);
            menu.Items.Add(label);

            menu.Items.Add(sep);

            for (int i = 0; i < names.Count; i++)
            {
                item = new ToolStripMenuItem();
                item.Text = names[i];
                Guid guid = GUIDs[i];
                if (guid == active) item.Checked = true;
                item.Click += (sender, e) => Change_Plan(sender, e, guid);
                itemList.Add(item);
                menu.Items.Add(item);
            }

            sep = new ToolStripSeparator();
            menu.Items.Add(sep);

            item = new ToolStripMenuItem();
            item.Text = "Close";
            item.Click += Exit_Click;
            menu.Items.Add(item);

            ni.ContextMenuStrip = menu;
            ni.MouseClick += Icon_Click;
            ni.Visible = true;
        }

        private void Icon_Click(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                MethodInfo mi = typeof(NotifyIcon).GetMethod("ShowContextMenu",
                         BindingFlags.Instance | BindingFlags.NonPublic);
                mi.Invoke(ni, null);
            }
        }

        private void Exit_Click(object sender, EventArgs e)
        {
            Dispose();
            Application.Exit();
        }

        private void Change_Plan(object sender, EventArgs e, Guid target)
        {
            uint resp = PowerSetActiveScheme(IntPtr.Zero, ref target);

            if (resp != 0) throw new Exception("Error in changing the power plan.");

            itemList.ForEach(delegate (ToolStripMenuItem check)
            {
                if (check == sender) check.Checked = true;
                else if (check != null) check.Checked = false;
            });
        }

        public void Dispose()
        {
            ni.Dispose();
        }
    }
}
