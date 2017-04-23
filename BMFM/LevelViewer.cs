using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace BMFM
{
    public partial class LevelViewer : UserControl
    {
        public Level level;

        public LevelViewer()
        {
            InitializeComponent();
        }

        public void LoadLevel(Level level)
        {
            this.level = level;
            updateScrollBars();
        }

        private void updateScrollBars()
        {
            hScroll.Maximum = Level.width;
            vScroll.Maximum = Level.height;
            hScroll.LargeChange = canvas.Width;
            vScroll.LargeChange = canvas.Height;
            hScroll.Value = Math.Min(hScroll.Value, Math.Max(0, hScroll.Maximum - hScroll.LargeChange));
            vScroll.Value = Math.Min(vScroll.Value, Math.Max(0, vScroll.Maximum - vScroll.LargeChange));
            canvas.Invalidate();
        }

        private void canvas_Paint(object sender, PaintEventArgs e)
        {
            if (level == null) return;

            e.Graphics.TranslateTransform(-hScroll.Value, -vScroll.Value);
            Rectangle r = new Rectangle(hScroll.Value, vScroll.Value, canvas.Width, canvas.Height);
            level.BG2.Render(e.Graphics, level.tileset, r);
            level.BG1.Render(e.Graphics, level.tileset, r);
        }

        private void canvas_SizeChanged(object sender, EventArgs e)
        {
            updateScrollBars();
        }

        private void hScroll_Scroll(object sender, ScrollEventArgs e)
        {
            canvas.Invalidate();
        }
    }
}
