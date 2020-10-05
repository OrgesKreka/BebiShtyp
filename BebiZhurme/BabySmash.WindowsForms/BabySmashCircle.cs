using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace BabySmash.WindowsForms
{
    public partial class BabySmashCircle : Panel
    {

        private const int CIRCLE_RADIUS = 70;

        private readonly Timer _timer;
        private readonly int _x;
        private readonly int _y;
        private readonly Color _color;
        private bool _firstTime;


        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                cp.ExStyle |= 0x00000020; // WS_EX_TRANSPARENT

                return cp;
            }
        }

        public BabySmashCircle(int x, int y, Color color)
        {


            InitializeComponent();

            SetStyle(ControlStyles.Opaque, true);


            _timer = new Timer
            {
                Interval = 9000 // 2 sekonda
            };
            _timer.Start();

            _timer.Tick += TimerTick;

            _x = x;
            _y = y;
            _color = color;
            _firstTime = true;


            this.Size = new Size(2 * CIRCLE_RADIUS, 2 * CIRCLE_RADIUS);
            this.Location = new Point(_x - CIRCLE_RADIUS, _y - CIRCLE_RADIUS);
            this.BackColor = Color.Transparent;

            this.BringToFront();

        }

        private void BabySmashCircle_MouseHover(object sender, EventArgs e)
        {
            Debug.WriteLine("U be gje ??");
        }

        private void BabySmashCircle_Paint(object sender, PaintEventArgs e)
        {

            Debug.WriteLine("On paint1");

            using (var graphics = this.CreateGraphics())
            using (var rectangleBrush = new SolidBrush(_color))
            using (var backgroundBrush = new SolidBrush(this.BackColor))
            {
                graphics.FillRectangle(backgroundBrush, this.ClientRectangle);
                graphics.FillEllipse(rectangleBrush, 0, 0, CIRCLE_RADIUS * 2, CIRCLE_RADIUS * 2);
            }


        }

        protected override void OnPaint(System.Windows.Forms.PaintEventArgs e)
        {
            // if (!_firstTime) return;
            Debug.WriteLine("On paint2");

            using (var graphics = this.CreateGraphics())
            using (var rectangleBrush = new SolidBrush(_color))
            using (var backgroundBrush = new SolidBrush(this.BackColor))
            {
                graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                graphics.FillRectangle(backgroundBrush, this.ClientRectangle);
                graphics.FillEllipse(rectangleBrush, 0, 0, CIRCLE_RADIUS * 2, CIRCLE_RADIUS * 2);
            }

            // _firstTime = false;
        }

        private void TimerTick(object sender, EventArgs e)
        {
            this.Visible = false;
            _timer.Stop();
            this.Dispose();
        }
    }
}
