using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace BMFM
{
    public partial class Form1 : Form
    {
        public string ROMFile;

        public Form1()
        {
            InitializeComponent();
        }

        private void btnLoadROM_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "SNES ROMs (*.sfc; *.smc)|*.sfc; *.smc|All Files (*.*)|*.*";
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                ROMFile = ofd.FileName;
            }
        }

        private void btnLoadLevel_Click(object sender, EventArgs e)
        {
            if (ROMFile == null) return;

            FileStream fs = new FileStream(ROMFile, FileMode.Open, FileAccess.Read, FileShare.Read);
            // Level data starts at PC 0xAB0E and has 0x10 bytes per level
            fs.Seek(0xAB0E + (int)nudLevel.Value * 0x10, SeekOrigin.Begin);
            Level l = new Level(fs);
            levelViewer1.LoadLevel(l);

            fs.Close();
        }

        private void exportImage_Click(object sender, EventArgs e)
        {
            if (levelViewer1.level == null) return;

            Bitmap img = new Bitmap(Level.width, Level.height);
            using (Graphics g = Graphics.FromImage(img))
            {
                levelViewer1.level.BG2.Render(g, levelViewer1.level.tileset);
                levelViewer1.level.BG1.Render(g, levelViewer1.level.tileset);
            }
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "PNG Images (*.png)|*.PNG";
            if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                img.Save(sfd.FileName);
            }
        }
    }
}
