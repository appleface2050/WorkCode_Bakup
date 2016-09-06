using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.Drawing;

namespace BlueStacks.hyperDroid.GameManager
{
    public class TriangularPictureBox : PictureBox
    {
        protected override void OnPaint(PaintEventArgs pe)
        {
            using (var p = new GraphicsPath())
            {
                p.AddPolygon(new Point[] {
                new Point(this.Width, 0), 
                new Point(0, Height), 
                new Point(Width, Height) });

                this.Region = new Region(p);
                base.OnPaint(pe);
            }

            this.Cursor = System.Windows.Forms.Cursors.SizeNWSE;

        }
    }
}
