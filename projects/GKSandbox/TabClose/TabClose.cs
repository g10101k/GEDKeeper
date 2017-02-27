using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using GKSandbox;

namespace tabClose
{
    public partial class Form1 : Form
    {
        const int LEADING_SPACE = 8;
        const int CLOSE_SPACE = 8;
        const int CLOSE_AREA = 14;
        const int CLOSE_TOP_SPAN = 7;
        private Rectangle tabArea;
        private RectangleF tabTextArea;
        
        private int Xwid = 8;
        private const int tab_margin = 3;
        
        int I = 0;
        public Form1()
        {
            InitializeComponent();
            //this.SetStyle(ControlStyles.AllPaintingInWmPaint
            TopMost = true;
            //TransparencyKey = SystemColors.Control;
            tabArea = tabControl1.GetTabRect(0);
            tabTextArea = (RectangleF)tabControl1.GetTabRect(0);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            int y = richTextBox1.Top;
            int x = richTextBox1.Left;
            int w = richTextBox1.Width;
            int h = richTextBox1.Height;
            Pen pen = new Pen(richTextBox1.BackColor, 2);
            DrwRectangleRound(pen, x, y, w, h, 20, 0);
            DrwRectangleRound(pen, x, y, w, h, 20, -1);
            DrwRectangleRound(pen, x, y, w, h, 20, -2);
            DrwRectangleRound(pen, x, y, w, h, 20, -3);
            DrwRectangleRound(pen, x, y, w, h, 20, -4);
            //DrwRectangleRound(pen, x, y, w, h, 20, -5);
            pen = new Pen(Color.Black, 2);
            DrwRectangleRound(pen, x, y, w, h, 20, -5);
            //e.Graphics.DrawEllipse(Pens.Red, 100, 100, 300, 300);
        }

        private void tabControl1_DrawItem(object sender, DrawItemEventArgs e)
        {
            Brush txt_brush, box_brush;
            Pen box_pen;

            // We draw in the TabRect rather than on e.Bounds
            // so we can use TabRect later in MouseDown.
            Rectangle tab_rect = tabControl1.GetTabRect(e.Index);

            // Draw the background.
            // Pick appropriate pens and brushes.
            if (e.State == DrawItemState.Selected)
            {
                e.Graphics.FillRectangle(Brushes.DarkOrange, tab_rect);
                e.DrawFocusRectangle();

                txt_brush = Brushes.Yellow;
                box_brush = Brushes.Silver;
                box_pen = Pens.DarkBlue;
            }
            else
            {
                e.Graphics.FillRectangle(Brushes.PaleGreen, tab_rect);

                txt_brush = Brushes.DarkBlue;
                box_brush = Brushes.LightGray;
                box_pen = Pens.DarkBlue;
            }

            // Allow room for margins.
            RectangleF layout_rect = new RectangleF(
                tab_rect.Left + tab_margin,
                tab_rect.Y + tab_margin,
                tab_rect.Width - 2 * tab_margin,
                tab_rect.Height - 2 * tab_margin);
            using (StringFormat string_format = new StringFormat())
            {
                // Draw the tab # in the upper left corner.
                using (Font small_font = new Font(this.Font.FontFamily,
                                                  6, FontStyle.Bold))
                {
                    string_format.Alignment = StringAlignment.Near;
                    string_format.LineAlignment = StringAlignment.Near;
                    e.Graphics.DrawString(
                        e.Index.ToString(),
                        small_font,
                        txt_brush,
                        layout_rect,
                        string_format);
                }

                // Draw the tab's text centered.
                using (Font big_font = new Font(this.Font, FontStyle.Bold))
                {
                    string_format.Alignment = StringAlignment.Center;
                    string_format.LineAlignment = StringAlignment.Center;
                    e.Graphics.DrawString(
                        tabControl1.TabPages[e.Index].Text,
                        big_font,
                        txt_brush,
                        layout_rect,
                        string_format);
                }

                // Draw an X in the upper right corner.
                Rectangle rect = tabControl1.GetTabRect(e.Index);
                e.Graphics.FillRectangle(box_brush,
                                         layout_rect.Right - Xwid,
                                         layout_rect.Top,
                                         Xwid,
                                         Xwid);
                e.Graphics.DrawRectangle(box_pen,
                                         layout_rect.Right - Xwid,
                                         layout_rect.Top,
                                         Xwid,
                                         Xwid);
                e.Graphics.DrawLine(box_pen,
                                    layout_rect.Right - Xwid,
                                    layout_rect.Top,
                                    layout_rect.Right,
                                    layout_rect.Top + Xwid);
                e.Graphics.DrawLine(box_pen,
                                    layout_rect.Right - Xwid,
                                    layout_rect.Top + Xwid,
                                    layout_rect.Right,
                                    layout_rect.Top);
            }
            return;
            
            TabPage page = tabControl1.TabPages[e.Index];
            e.Graphics.FillRectangle(new SolidBrush(page.BackColor), e.Bounds);

            Rectangle paddedBounds = e.Bounds;
            int yOffset = (e.State == DrawItemState.Selected) ? -2 : 1;
            paddedBounds.Offset(1, yOffset);
            TextRenderer.DrawText(e.Graphics, page.Text, Font, paddedBounds, page.ForeColor);
            //return;
            Image i = Resources.cross;
            e.Graphics.DrawImage(i, e.Bounds.Right - CLOSE_AREA + yOffset, e.Bounds.Top + CLOSE_TOP_SPAN, CLOSE_SPACE, CLOSE_SPACE);
            //e.Graphics.DrawString(this.tabControl1.TabPages[e.Index].Text, e.Font, Brushes.Black, e.Bounds.Left, e.Bounds.Top + 4);
            //e.Bounds.Width = 100;
            //e.DrawFocusRectangle();
        }
        
        /* 
        
        private void tabControl1_DrawItem(object sender, System.Windows.Forms.DrawItemEventArgs e)
        {
            TabPage CurrentTab = tabControl1.TabPages[e.Index];
            Rectangle ItemRect = tabControl1.GetTabRect(e.Index);
            SolidBrush FillBrush = new SolidBrush(Color.Red);
            SolidBrush TextBrush = new SolidBrush(Color.White);
            StringFormat sf = new StringFormat();
            sf.Alignment = StringAlignment.Center;
            sf.LineAlignment = StringAlignment.Center;

            //If we are currently painting the Selected TabItem we'll
            //change the brush colors and inflate the rectangle.
            if (System.Convert.ToBoolean(e.State & DrawItemState.Selected))
            {
                FillBrush.Color = Color.White;
                TextBrush.Color = Color.Red;
                ItemRect.Inflate(2, 2);
            }

            //Set up rotation for left and right aligned tabs
            if (tabControl1.Alignment == TabAlignment.Left || tabControl1.Alignment == TabAlignment.Right)
            {
                float RotateAngle = 90;
                if (tabControl1.Alignment == TabAlignment.Left)
                    RotateAngle = 270;
                PointF cp = new PointF(ItemRect.Left + (ItemRect.Width / 2), ItemRect.Top + (ItemRect.Height / 2));
                e.Graphics.TranslateTransform(cp.X, cp.Y);
                e.Graphics.RotateTransform(RotateAngle);
                ItemRect = new Rectangle(-(ItemRect.Height / 2), -(ItemRect.Width / 2), ItemRect.Height, ItemRect.Width);
            }

            //Next we'll paint the TabItem with our Fill Brush
            e.Graphics.FillRectangle(FillBrush, ItemRect);

            //Now draw the text.
            e.Graphics.DrawString(CurrentTab.Text, e.Font, TextBrush, (RectangleF)ItemRect, sf);

            //Reset any Graphics rotation
            e.Graphics.ResetTransform();

            //Finally, we should Dispose of our brushes.
            FillBrush.Dispose();
            TextBrush.Dispose();
        }
        
         */
        private void Form1_Load_1(object sender, EventArgs e)
        {
            //Looping through the controls.
            //Paint += new PaintEventHandler(Form1_Paint);//перерисовываем
            //timer1.Start();

        }

        void Form1_Paint(object sender, PaintEventArgs e)
        {

        }
        public void DrwRectangleRound(Pen p, int x, int y, int w, int h, int r, int ofset) {
            DrwRectangleRound(p, x + ofset, y + ofset, w - ofset*2, h - ofset*2, r);
        }

        public void DrwRectangleRound(Pen p, int x, int y, int w, int h, int r)
        {
            Graphics gr = CreateGraphics();

            Point p1 = new Point(x, y + r);
            Point c1 = new Point(x, y);
            Point p2 = new Point(x + r, y);
            Point p3 = new Point(x + w - r, y);
            Point c5 = new Point(x + w, y);
            Point p4 = new Point(x + w, y + r);
            Point p5 = new Point(x + w, y + h - r);
            Point c9 = new Point(x + w, y + h);
            Point p6 = new Point(x + w - r, y + h);
            Point p7 = new Point(x + r, y + h);
            Point c13 = new Point(x, y + h);
            Point p8 = new Point(x, y + h - r);
            Point p9 = new Point(x, y + r);
            
            
            gr.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            gr.DrawBeziers(p, new Point[] {
                               p1, // point
                               c1, c1, // round
                               p2, // point
                               p2, p3, // straight
                               p3, // point
                               c5, c5, // round
                               p4, // point
                               p4, p5, // straight
                               p5, // point
                               c9, c9, // round
                               p6, // point
                               p6, p7, // straight
                               p7, // point
                               c13, c13,  // round
                               p8, // point
                               p8, p1,  // straight
                               p1
                           });
        }
        
        void drwSpot(Brush p, int x, int y, int width)
        {
            Graphics gr = CreateGraphics();
            gr.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

            int HalfWidt = width / 2;
            width = HalfWidt * 2;
            gr.FillEllipse(p, x - HalfWidt, y - HalfWidt, width, width);

        }

        private void DrawBeziersPoint(PaintEventArgs e)
        {
            
        }
        private void tabControl1_MouseDown(object sender, MouseEventArgs e)
        {
            Rectangle r = tabControl1.GetTabRect(this.tabControl1.SelectedIndex);
            Rectangle closeButton = new Rectangle(r.Right - CLOSE_AREA, r.Top + CLOSE_TOP_SPAN, CLOSE_SPACE, CLOSE_SPACE);
            if (closeButton.Contains(e.Location))
            {
                this.tabControl1.TabPages.Remove(this.tabControl1.SelectedTab);
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            int y = richTextBox1.Top - 4;
            int x = richTextBox1.Left - 4;
            int w = richTextBox1.Width;
            int h = richTextBox1.Height;
            Pen blackPen = new Pen(Color.Black, 2);
            DrwRectangleRound(blackPen, x, y, w, h, 20, I);
            I++;
        }
    }
}
