using System.Diagnostics;
using System.Windows.Forms;

namespace BabySmash.WindowsForms
{
    public partial class Controller : Form
    {
        public Controller()
        {
            InitializeComponent();
        }

        private void Controller_Load(object sender, System.EventArgs e)
        {
            var number = 1;
            foreach (var s in Screen.AllScreens)
            {
                MainForm m = new MainForm()
                {
                    Left = s.WorkingArea.Left,
                    Top = s.WorkingArea.Top,
                    Width = s.WorkingArea.Width,
                    Height = s.WorkingArea.Height,
                    Name = "Window" + number++.ToString(),
                    Location = s.WorkingArea.Location,
                    StartPosition = FormStartPosition.Manual
                };

                if (number > 1)
                    m.ShowInTaskbar = false;

                this.Visible = false;
                this.Hide();
                m.Show();

                if (Debugger.IsAttached)
                    break;
            }
        }
    }
}
