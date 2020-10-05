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

            _timer = new Timer
            {
                Interval = 9000 // 2 sekonda
            };

            _timer.Start();

            _timer.Tick += TimerTick;

        }


        private void TimerTick(object sender, System.EventArgs e)
        {

            this.Visible = false;
            _timer.Stop();
            this.Dispose();
        }

        protected override void OnCreateControl()
        {
            base.OnCreateControl();
            base.Font = new Font("Calibri", 60F, FontStyle.Bold);
            this.BackColor = Color.Transparent;
            this.AutoSize = true;
            this.BringToFront();
        }

        //protected override void OnPaint(PaintEventArgs pe)
        //{
        //    Debug.WriteLine("U therrit ??");
        //}
    }
}
