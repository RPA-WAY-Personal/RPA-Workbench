using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Forms;

namespace RPA_Workbench.Utilities.TreeNodeClasses
{
   public class ExtractIcon
    {
        public System.Windows.Forms.ListView listView1;
        public ImageList imageList1;
        public void ExtractAssociatedIconEx(string Directory)
        {
            
            // Initialize the ListView, ImageList and Form.
            listView1 = new System.Windows.Forms.ListView();
            imageList1 = new ImageList();
            listView1.Location = new Point(37, 12);
            listView1.Size = new Size(151, 262);
            listView1.SmallImageList = imageList1;
            listView1.View = View.SmallIcon;

            // Get the c:\ directory.
            System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo(@Directory);

            System.Windows.Forms.ListViewItem item;
            listView1.BeginUpdate();

            // For each file in the c:\ directory, create a ListViewItem
            // and set the icon to the icon extracted from the file.
            foreach (System.IO.FileInfo file in dir.GetFiles())
            {
                // Set a default icon for the file.
                Icon iconForFile = SystemIcons.WinLogo;

                item = new System.Windows.Forms.ListViewItem(file.Name, 1);

                // Check to see if the image collection contains an image
                // for this extension, using the extension as a key.
                if (!imageList1.Images.ContainsKey(file.Extension))
                {
                    // If not, add the image to the image list.
                    iconForFile = System.Drawing.Icon.ExtractAssociatedIcon(file.FullName);
                    imageList1.Images.Add(file.Extension, iconForFile);
                }
                item.ImageKey = file.Extension;
                listView1.Items.Add(item);
            }
            listView1.EndUpdate();
        }
    }

  
}
