using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ATray.Dialogs
{
    public partial class DiskUsageForm : Form
    {
        public DiskUsageForm()
        {
            InitializeComponent();
            //this.DiskItems.Columns.
            var nodes = new List<Node>
            {
                new Node("C:\\", 7123654126)
                ,new Node("D:\\", 765476254)
            };
            foreach (var node in nodes)
                this.DiskItems.Items.Add(node.ToListItem());
        }

        public class Node
        {
            public Node(string name, ulong size)
            {
                Name = name;
                ByteSize = size;
                SomeText = "Hej " + name;
            }

            public string Name { get; }
            protected ulong ByteSize { get; }
            public string SizeInclusive => BytesToString(ByteSize);
            public string SomeText { get; set; }

            public ListViewItem ToListItem()
            {
                var item = new ListViewItem(new[] {Name, SizeInclusive, SomeText});
                
                return item;
            }
        }


        public static String BytesToString(ulong byteCount)
        {
            string[] suf = { "B", "KB", "MB", "GB", "TB", "PB", "EB" }; //Longs run out around EB
            if (byteCount == 0)
                return "0" + suf[0];
            var bytes = Math.Abs((long)byteCount);
            var place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            var num = Math.Round(bytes / Math.Pow(1024, place), 1);
            return Math.Sign((long)byteCount) * num + suf[place];
        }

        private void button1_Click(object sender, EventArgs e)
        {

        }
    }
}
