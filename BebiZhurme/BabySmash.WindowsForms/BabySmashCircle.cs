using System;
using System.Drawing;
using System.Windows.Forms;

namespace BabySmash.WindowsForms
{
    public partial class BabySmashCircle : Panel
    {

        private const int CIRCLE_RADIUS = 50;

        private readonly Timer _timer;
        private readonly Random _random;
        private readonly int _x;
        private readonly int _y;

        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                cp.ExStyle |= 0x00000020; // WS_EX_TRANSPARENT

                return cp;
            }
        }

        public BabySmashCircle(int x, int y)
        {
            InitializeComponent();
            SetStyle(ControlStyles.Opaque, true);

            _timer = new Timer
            {
                Interval = 2000 // 2 sekonda
            };
            _timer.Start();

            _timer.Tick += timer_Tick;

            _x = x;
            _y = y;
            _random = new Random();

            this.Size = new Size(2 * CIRCLE_RADIUS, 2 * CIRCLE_RADIUS);
            this.Location = new Point(_x - CIRCLE_RADIUS, _y - CIRCLE_RADIUS);
            this.BackColor = Color.Transparent;
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            this.Visible = false;
            _timer.Stop();
            this.Dispose();
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            base.OnPaint(pe);
            var color = Color.FromArgb(_random.Next(256), _random.Next(256), _random.Next(256));

            using (var graphics = pe.Graphics)
            using (var rectangleBrush = new SolidBrush(color))
            using (var backgroundBrush = new SolidBrush(this.BackColor))
            {
                graphics.FillRectangle(backgroundBrush, this.ClientRectangle);
                graphics.FillEllipse(rectangleBrush, 0, 0, CIRCLE_RADIUS * 2, CIRCLE_RADIUS * 2);
            }
        }
    }
}
