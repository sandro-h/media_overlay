using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Text;
using System.ComponentModel;

namespace media_overlay
{
    public class OutlinedLabel: Label
    {
        public OutlinedLabel()
        {
            OutlineForeColor = Color.Green;
            OutlineWidth = 2;
        }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color OutlineForeColor { get; set; }
        
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public float OutlineWidth { get; set; }

        protected override void OnPaint(PaintEventArgs e)
        {
            // BG
            e.Graphics.FillRectangle(new SolidBrush(BackColor), ClientRectangle);

            // Text with outline
            // https://stackoverflow.com/a/19851271/32727753
            using GraphicsPath gp = new GraphicsPath();
            using Pen outline = new Pen(OutlineForeColor, OutlineWidth) { LineJoin = LineJoin.Round };
            using StringFormat sf = new StringFormat(StringFormatFlags.NoWrap);
            using Brush foreBrush = new SolidBrush(ForeColor);

            // Convert TextAlign to StringFormat alignment
            (sf.Alignment, sf.LineAlignment) = TextAlign switch
            {
                ContentAlignment.TopLeft => (StringAlignment.Near, StringAlignment.Near),
                ContentAlignment.TopRight => (StringAlignment.Far, StringAlignment.Near),
                ContentAlignment.BottomLeft => (StringAlignment.Near, StringAlignment.Far),
                ContentAlignment.BottomRight => (StringAlignment.Far, StringAlignment.Far),
                _ => (StringAlignment.Near, StringAlignment.Near)
            };

            gp.AddString(Text, Font.FontFamily, (int)Font.Style,
                Font.Size, ClientRectangle, sf);
            e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
            e.Graphics.DrawPath(outline, gp);
            e.Graphics.FillPath(foreBrush, gp);
        }
    }
}
