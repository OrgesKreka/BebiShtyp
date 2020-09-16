using System.Drawing;
using System.Windows.Forms;

namespace BabySmash.WindowsForms
{
    public partial class BabySmashLabel : Label//MaterialLabel
    {
        private readonly Timer _timer;



        public BabySmashLabel()
        {
            InitializeComponent();

            _timer = new Timer();

            _timer.Interval = 2000; // 2 sekonda
            _timer.Start();

            _timer.Tick += timer_Tick;
        }

        private void timer_Tick(object sender, System.EventArgs e)
        {

            this.Visible = false;
            _timer.Stop();
            this.Dispose();
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            base.OnPaint(pe);
        }

        protected override void OnCreateControl()
        {
            base.OnCreateControl();
            base.Font = new Font("Calibri", 60F, FontStyle.Bold);
            this.BackColor = Color.Transparent;
            this.AutoSize = true;
            this.BringToFront();
            //this.SetStyle(ControlStyles.Opaque, true);
        }
    }
}
