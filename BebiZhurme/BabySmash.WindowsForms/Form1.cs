using BabySmash.WindowsForms.Naudio;
using Gma.System.MouseKeyHook;
using MaterialWinforms;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace BabySmash.WindowsForms
{
    public partial class MainForm : MaterialWinforms.Controls.MaterialForm
    {
        static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);

        static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);

        static readonly IntPtr HWND_TOP = new IntPtr(0);

        static readonly IntPtr HWND_BOTTOM = new IntPtr(1);

        const UInt32 SWP_NOSIZE = 0x0001;

        const UInt32 SWP_NOMOVE = 0x0002;

        const UInt32 TOPMOST_FLAGS = SWP_NOMOVE | SWP_NOSIZE;



        [DllImport("user32.dll")]

        [return: MarshalAs(UnmanagedType.Bool)]

        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);


        [DllImport("user32.dll")]
        public static extern IntPtr LoadCursorFromFile(string fileName);

        private readonly Random _random;

        private IKeyboardMouseEvents _globalHook;



        public MainForm()
        {
            InitializeComponent();

            _random = new Random();
            var materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.AddFormToManage(this);
            materialSkinManager.Theme = MaterialSkinManager.Themes.LIGHT;

            this.ControlBox = false;
            materialSkinManager.ColorScheme = new ColorScheme(Primary.Cyan400, Primary.BlueGrey900, Primary.BlueGrey500, Accent.LightBlue200, TextShade.WHITE);

            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.Opaque | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.UserPaint, true);

            this.TopMost = true;
            // this.KeyPreview = true;

            var map = new Dictionary<Combination, Action>
            {
                { Combination.TriggeredBy(Keys.Alt).With(Keys.F4), () => this.Close()},
                {Combination.FromString("RMenu+F4"), () => Debug.WriteLine(":-P")},
            };
            _globalHook = Hook.GlobalEvents();
            _globalHook.OnCombination(map);
            _globalHook.KeyDown += GlobalHookKeyDown;


        }


        int numberOfCtrls = 0;
        private void GlobalHookKeyDown(object sender, KeyEventArgs e)
        {

            Debug.WriteLine(e.Control + " control");


            Debug.WriteLine(e.Alt + " alt");

            if (e.KeyCode == Keys.LControlKey)
            {
                numberOfCtrls++;

                if (numberOfCtrls == 5) this.Close();
            }
            if (e.Control && e.KeyCode == Keys.F4)
            {
                e.Handled = true;
                this.Close();
            }

            if (e.KeyCode == Keys.LWin || e.KeyCode == Keys.RWin) { e.Handled = true; return; }
            if (e.KeyCode == Keys.LMenu || e.KeyCode == Keys.RMenu) { e.Handled = true; return; }

            if (e.KeyCode == Keys.NumLock) { e.Handled = true; return; }

            var keyChar = CharFromKeys(e.KeyCode, e.Shift, e.Control);

            if (char.IsLetter(keyChar) || char.IsDigit(keyChar))
            {
                int x = _random.Next(50, this.Width - 100);
                int y = _random.Next(50, this.Height - 100);

                var label = new BabySmashLabel
                {
                    Text = keyChar.ToString(),
                    Location = new Point(x, y),
                    ForeColor = Color.FromArgb(_random.Next(256), _random.Next(256), _random.Next(256))
                };


                this.Controls.Add(label);

                var soundPath = Path.Combine(Environment.CurrentDirectory, "Audio", char.IsDigit(keyChar) ? "Numbers" : "Alphabet", $"{keyChar}.wav");
                AudioPlaybackEngine.Instance.PlaySound(soundPath);

            }

            e.Handled = true;

        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            if (!System.Diagnostics.Debugger.IsAttached)
                SetWindowPos(this.Handle, HWND_TOPMOST, 0, 0, 0, 0, TOPMOST_FLAGS);


        }

        private void MainForm_KeyPress(object sender, KeyPressEventArgs e)
        {
            Debug.WriteLine("");
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            //_globalHook.KeyDown -= GlobalHookKeyDown;
            // _globalHook.Dispose();

            AudioPlaybackEngine.Instance.Dispose();
            Application.OpenForms.OfType<Controller>().FirstOrDefault()?.Close();
        }

        private void MainForm_Click(object sender, EventArgs e)
        {
            var g = this.CreateGraphics();
            var mouseEvents = e as MouseEventArgs;

            var circle = new BabySmashCircle(mouseEvents.X, mouseEvents.Y);

            this.Controls.Add(circle);

        }




        [DllImport("user32.dll")]
        public static extern int ToUnicode(uint virtualKeyCode, uint scanCode, byte[] keyboardState,
      [Out, MarshalAs(UnmanagedType.LPWStr, SizeConst = 64)]
        StringBuilder receivingBuffer,
      int bufferSize, uint flags);
        protected override void OnKeyDown(KeyEventArgs e)
        {
            char r = CharFromKeys(e.KeyCode, e.Shift, e.Control);
            //Logic to handel this char
            //
            //
            base.OnKeyDown(e);
        }
        public char CharFromKeys(Keys keys, bool shift, bool altGr)
        {
            char r = '\0';
            var buf = new StringBuilder(256);
            var keyboardState = new byte[256];
            if (shift)
                keyboardState[(int)Keys.ShiftKey] = 0xff;
            if (altGr)
            {
                keyboardState[(int)Keys.ControlKey] = 0xff;
                keyboardState[(int)Keys.Menu] = 0xff;
            }
            if (ToUnicode((uint)keys, 0, keyboardState, buf, 256, 0) > 0)
                r = buf[0];
            return r;
        }
    }

}
