using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrayNotify;

namespace Cleanzorv12.Custom_Controls
{
    public class CustomLabel : Label
    {
        // Fields
        private int borderSize = 0;  // Default to 0 (no border)
        private int borderRadius = 0;  // Default to 0 (no rounding)
        private Color borderColor;  // Default color, but can be changed
        private float letterSpacing = -4;  // Default to no spacing

        // Properties
        [Category("Custom Controls")]
        public int BorderSize
        {
            get { return borderSize; }
            set
            {
                borderSize = value;
                this.Invalidate();
            }
        }

        [Category("Custom Controls")]
        public int BorderRadius
        {
            get { return borderRadius; }
            set
            {
                borderRadius = value;
                this.Invalidate();
            }
        }

        [Category("Custom Controls")]
        public Color BorderColor
        {
            get { return borderColor; }
            set
            {
                borderColor = value;
                this.Invalidate();
            }
        }

        [Category("Custom Controls")]
        public Color BackgroundColor
        {
            get { return this.BackColor; }
            set { this.BackColor = value; }
        }

        [Category("Custom Controls")]
        public Color TextColor
        {
            get { return this.ForeColor; }
            set { this.ForeColor = value; }
        }

        [Category("Custom Controls")]
        public float LetterSpacing
        {
            get { return letterSpacing; }
            set
            {
                letterSpacing = value;
                this.Invalidate();  // Redraw when spacing changes
            }
        }

        // Constructor
        public CustomLabel()
        {
            // No explicit property assignments; use base Label defaults
            this.Resize += new EventHandler(CustomLabel_Resize);
        }

        // Methods
        private GraphicsPath GetFigurePath(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            float curveSize = radius * 2F;

            path.StartFigure();
            path.AddArc(rect.X, rect.Y, curveSize, curveSize, 180, 90);
            path.AddArc(rect.Right - curveSize, rect.Y, curveSize, curveSize, 270, 90);
            path.AddArc(rect.Right - curveSize, rect.Bottom - curveSize, curveSize, curveSize, 0, 90);
            path.AddArc(rect.X, rect.Bottom - curveSize, curveSize, curveSize, 90, 90);
            path.CloseFigure();
            return path;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Rectangle rectSurface = this.ClientRectangle;
            Rectangle rectBorder = Rectangle.Inflate(rectSurface, -borderSize, -borderSize);
            int smoothSize = 2;
            if (borderSize > 0)
                smoothSize = borderSize;

            // Draw rounded border if specified
            if (borderRadius > 2)  // Rounded label
            {
                using (GraphicsPath pathSurface = GetFigurePath(rectSurface, borderRadius))
                using (GraphicsPath pathBorder = GetFigurePath(rectBorder, borderRadius - borderSize))
                using (Pen penSurface = new Pen(this.Parent.BackColor, smoothSize))
                using (Pen penBorder = new Pen(borderColor, borderSize))
                {
                    e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    this.Region = new Region(pathSurface);  // Set the region for rounded shape
                    e.Graphics.DrawPath(penSurface, pathSurface);  // Draw surface
                    if (borderSize >= 1)
                        e.Graphics.DrawPath(penBorder, pathBorder);  // Draw border
                }
            }
            else  // Normal label
            {
                e.Graphics.SmoothingMode = SmoothingMode.None;
                this.Region = new Region(rectSurface);
                if (borderSize >= 1)
                {
                    using (Pen penBorder = new Pen(borderColor, borderSize))
                    {
                        e.Graphics.DrawRectangle(penBorder, 0, 0, this.Width - 1, this.Height - 1);
                    }
                }
            }

            // Now draw the text with letter spacing
            if (!string.IsNullOrEmpty(this.Text))
            {
                using (Brush textBrush = new SolidBrush(this.ForeColor))
                {
                    float totalWidth = 0;
                    foreach (char c in this.Text)
                    {
                        SizeF charSize = e.Graphics.MeasureString(c.ToString(), this.Font);
                        totalWidth += charSize.Width + LetterSpacing;  // Add spacing
                    }
                    totalWidth -= LetterSpacing;  // Subtract extra spacing at the end

                    float x = rectSurface.X;  // Start x position
                    float y = rectSurface.Y + (rectSurface.Height - e.Graphics.MeasureString(this.Text, this.Font).Height) / 2;  // Center vertically

                    // Adjust x based on TextAlign
                    if (this.TextAlign == ContentAlignment.MiddleCenter)
                    {
                        x += (rectSurface.Width - totalWidth) / 2;
                    }
                    else if (this.TextAlign == ContentAlignment.MiddleRight)
                    {
                        x += rectSurface.Width - totalWidth;
                    }
                    // else: MiddleLeft (default), so x remains as is

                    float offset = x;  // Starting offset for drawing
                    foreach (char c in this.Text)
                    {
                        e.Graphics.DrawString(c.ToString(), this.Font, textBrush, offset, y);
                        SizeF charSize = e.Graphics.MeasureString(c.ToString(), this.Font);
                        offset += charSize.Width + LetterSpacing;  // Move to next character with spacing
                    }
                }
            }
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            this.Parent.BackColorChanged += new EventHandler(Container_BackColorChanged);
        }

        private void Container_BackColorChanged(object sender, EventArgs e)
        {
            this.Invalidate();
        }

        private void CustomLabel_Resize(object sender, EventArgs e)
        {
            if (borderRadius > this.Height)
                borderRadius = this.Height;
        }
    }
}