using BabySmash.WindowsForms.Keyboard;
using BabySmash.WindowsForms.Naudio;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using System.Windows.Input;

namespace BabySmash.WindowsForms
{
    public partial class Controller : Form
    {
        private static readonly InterceptKeys.LowLevelKeyboardProc _proc = HookCallback;
        private static IntPtr _hookID = IntPtr.Zero;
        private readonly List<MainForm> _windows = new List<MainForm>();
        private readonly Random _random;

        public Controller()
        {
            _random = new Random();

            try
            {
                _hookID = InterceptKeys.SetHook(_proc);
            }
            catch
            {
                DetachKeyboardHook();
            }

            InitializeComponent();

        }

        /// <summary>
        /// Detach the keyboard hook; call during shutdown to prevent calls as we unload
        /// </summary>
        private static void DetachKeyboardHook()
        {
            if (_hookID != IntPtr.Zero)
                InterceptKeys.UnhookWindowsHookEx(_hookID);
        }

        public static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                bool alt = (Control.ModifierKeys & Keys.Alt) != 0;
                bool control = (Control.ModifierKeys & Keys.Control) != 0;

                int vkCode = Marshal.ReadInt32(lParam);
                Keys key = (Keys)vkCode;

                if (alt && key == Keys.F4)
                {
                    Application.Exit();
                    return (IntPtr)1; // Handled.
                }

                if (!AllowKeyboardInput(alt, control, key))
                {
                    return (IntPtr)1; // Handled.
                }
            }

            return InterceptKeys.CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        /// <summary>Determines whether the specified keyboard input should be allowed to be processed by the system.</summary>
        /// <remarks>Helps block unwanted keys and key combinations that could exit the app, make system changes, etc.</remarks>
        public static bool AllowKeyboardInput(bool alt, bool control, Keys key)
        {
            // Disallow various special keys.
            if (key <= Keys.Back || key == Keys.None ||
                key == Keys.Menu || key == Keys.Pause ||
                key == Keys.Help)
            {
                return false;
            }

            // Disallow ranges of special keys.
            // Currently leaves volume controls enabled; consider if this makes sense.
            // Disables non-existing Keys up to 65534, to err on the side of caution for future keyboard expansion.
            if ((key >= Keys.LWin && key <= Keys.Sleep) ||
                (key >= Keys.KanaMode && key <= Keys.HanjaMode) ||
                (key >= Keys.IMEConvert && key <= Keys.IMEModeChange) ||
                (key >= Keys.BrowserBack && key <= Keys.BrowserHome) ||
                (key >= Keys.MediaNextTrack && key <= Keys.LaunchApplication2) ||
                (key >= Keys.ProcessKey && key <= (Keys)65534))
            {
                return false;
            }

            // Disallow specific key combinations. (These component keys would be OK on their own.)
            if ((alt && key == Keys.Tab) ||
                (alt && key == Keys.Space) ||
                (control && key == Keys.Escape))
            {
                return false;
            }

            // Allow anything else (like letters, numbers, spacebar, braces, and so on).
            return true;
        }

        private void Controller_Load(object sender, EventArgs e)
        {
            var number = 0;
            foreach (var s in Screen.AllScreens)
            {
                MainForm m = new MainForm(s.Bounds)
                {

                    Left = s.WorkingArea.Left,
                    Top = s.WorkingArea.Top,
                    Name = "Window" + number++.ToString(),
                    StartPosition = FormStartPosition.Manual,
                    TopMost = true,

                };

                if (number > 1)
                {
                    m.ShowInTaskbar = false;

                    this.Visible = false;
                    this.Hide();
                    m.HideInfoLabel();
                }

                m.Show(this);

                m.KeyDown += ProcessKey;
                m.Click += ProcessClick;

                _windows.Add(m);

                //if (Debugger.IsAttached)
                //    break;
            }
        }

        private void ProcessClick(object sender, EventArgs e)
        {
            var mouseEvents = e as MouseEventArgs;

            if (mouseEvents.Button == MouseButtons.Left)
            {

                foreach (var form in _windows)
                {
                    Point p = form.PointToClient(Cursor.Position); // merr koordinatat e kursorit ne forme
                    var color = Color.FromArgb(_random.Next(256), _random.Next(256), _random.Next(256));
                    var circle = new BabySmashCircle(p.X, p.Y, color);


                    form.Controls.Add(circle);
                }
            }
        }

        private void ProcessKey(object sender, KeyEventArgs e)
        {
            var key = KeyInterop.KeyFromVirtualKey((int)e.KeyCode);
            var displayChar = GetDisplayChar(key);

            if (char.IsLetterOrDigit(displayChar))
            {

                foreach (var form in _windows)
                {
                    var formX = form.Location.X;
                    var formY = form.Location.Y;

                    int x = _random.Next(formX + 100, formX + form.Width - 100);
                    int y = _random.Next(formY + 100, formY + form.Height - 100);

                    Point p = form.PointToClient(new Point(x, y)); // merr koordinatat e kursorit ne forme

                    var label = new BabySmashLabel
                    {
                        Text = displayChar.ToString(),
                        Location = p,
                        ForeColor = Color.FromArgb(_random.Next(256), _random.Next(256), _random.Next(256))
                    };

                    form.Controls.Add(label);

                }



                var soundPath = Path.Combine(Environment.CurrentDirectory, "Audio", char.IsDigit(displayChar) ? "Numbers" : "Alphabet", $"{displayChar}.wav");
                AudioPlaybackEngine.Instance.PlaySound(soundPath);
            }

        }

        private char GetDisplayChar(Key key)
        {
            // If a number on the normal number track is pressed, display the number.
            if (key >= Key.D0 && key <= Key.D9)
            {
                return (char)('0' + key - Key.D0);
            }

            // If a number on the numpad is pressed, display the number.
            if (key >= Key.NumPad0 && key <= Key.NumPad9)
            {
                return (char)('0' + key - Key.NumPad0);
            }

            try
            {
                return char.ToUpperInvariant(TryGetLetter(key));
            }
            catch (Exception ex)
            {
                Debug.Assert(false, ex.ToString());
                return '*';
            }
        }

        public enum MapType : uint
        {
            MAPVK_VK_TO_VSC = 0x0,
            MAPVK_VSC_TO_VK = 0x1,
            MAPVK_VK_TO_CHAR = 0x2,
            MAPVK_VSC_TO_VK_EX = 0x3,
        }

        [DllImport("user32.dll")]
        public static extern int ToUnicode(
                uint wVirtKey,
                uint wScanCode,
                byte[] lpKeyState,
                [Out, MarshalAs(UnmanagedType.LPWStr, SizeParamIndex = 4)]
                        StringBuilder pwszBuff,
                int cchBuff,
                uint wFlags);

        [DllImport("user32.dll")]
        public static extern bool GetKeyboardState(byte[] lpKeyState);

        [DllImport("user32.dll")]
        public static extern uint MapVirtualKey(uint uCode, MapType uMapType);

        private static char TryGetLetter(Key key)
        {
            char ch = ' ';

            int virtualKey = KeyInterop.VirtualKeyFromKey(key);
            byte[] keyboardState = new byte[256];
            GetKeyboardState(keyboardState);

            uint scanCode = MapVirtualKey((uint)virtualKey, MapType.MAPVK_VK_TO_VSC);
            StringBuilder stringBuilder = new StringBuilder(2);

            int result = ToUnicode((uint)virtualKey, scanCode, keyboardState, stringBuilder, stringBuilder.Capacity, 0);
            switch (result)
            {
                case -1:
                    break;
                case 0:
                    break;
                case 1:
                    {
                        ch = stringBuilder[0];
                        break;
                    }
                default:
                    {
                        ch = stringBuilder[0];
                        break;
                    }
            }
            return ch;
        }
    }
}
