using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Runtime.InteropServices;
using Windows.Media.Control;

namespace media_overlay
{
    public partial class Form1 : Form
    {
        private GlobalSystemMediaTransportControlsSession? currentSession = null;

        public Form1()
        {
            //AllocConsole();
            InitializeComponent();

            this.BackColor = Color.FromArgb(1, 1, 1); //Color.FromArgb(254, 254, 254);
            this.TransparencyKey = this.BackColor;

            this.StartPosition = FormStartPosition.Manual;
            Rectangle res = Screen.AllScreens.Last().Bounds;
            this.Location = new Point(res.X + res.Width - Size.Width);

            label1.Left = 0;
            label1.Top = 0;
            label1.Width = this.ClientSize.Width - 4;
            label1.Height = this.ClientSize.Height;
            label1.OutlineForeColor = Color.Black;
            label1.OutlineWidth = 2;

            // The magic sauce to make the window completely ignore mouse clicks and hovers (including on non-transparent parts).
            // All other online solutions didn't work.
            // https://stackoverflow.com/a/50245502
            int GWL_EXSTYLE = -20;
            long WS_EX_TRANSPARENT = 0x00000020L;
            long WS_EX_LAYERED = 0x00080000L;
            var cur_style = GetWindowLong(this.Handle, GWL_EXSTYLE);
            SetWindowLong(this.Handle, GWL_EXSTYLE, cur_style | WS_EX_TRANSPARENT | WS_EX_LAYERED);
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            e.Graphics.FillRectangle(new SolidBrush(this.BackColor), e.ClipRectangle);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            var sessionManager = GlobalSystemMediaTransportControlsSessionManager.RequestAsync().GetAwaiter().GetResult();
            sessionManager.CurrentSessionChanged += CurrentSessionChanged;
            CurrentSessionChanged(sessionManager, null);
        }

        private void CurrentSessionChanged(GlobalSystemMediaTransportControlsSessionManager sessionManager, CurrentSessionChangedEventArgs? args)
        {
            if (currentSession != null)
            {
                currentSession.MediaPropertiesChanged -= MediaPropertiesChanged;
                currentSession.PlaybackInfoChanged -= PlaybackInfoChanged;
            }

            currentSession = sessionManager.GetCurrentSession();
            if (currentSession != null)
            {
                currentSession.MediaPropertiesChanged += MediaPropertiesChanged;
                currentSession.PlaybackInfoChanged += PlaybackInfoChanged;
            }

            update(currentSession);
        }

        private void MediaPropertiesChanged(GlobalSystemMediaTransportControlsSession? session, MediaPropertiesChangedEventArgs? args)
        {
            update(session);
        }

        private void PlaybackInfoChanged(GlobalSystemMediaTransportControlsSession? session, PlaybackInfoChangedEventArgs? args)
        {
            update(session);
        }

        private void update(GlobalSystemMediaTransportControlsSession? session)
        {
            var playbackInfo = session?.GetPlaybackInfo();
            var mediaProperties = session?.TryGetMediaPropertiesAsync().GetAwaiter().GetResult();
            string newText;
            if (playbackInfo == null || mediaProperties == null)
            {
                newText = "";
            }
            else if (playbackInfo?.PlaybackStatus != GlobalSystemMediaTransportControlsSessionPlaybackStatus.Playing &&
                playbackInfo?.PlaybackStatus != GlobalSystemMediaTransportControlsSessionPlaybackStatus.Paused)
            {
                newText = "";
            }
            else
            {
                newText = $"{mediaProperties.Artist} - {mediaProperties.Title}";
            }

            label1.InvokeAsync(() => label1.Text = newText);
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool AllocConsole();

        [DllImport("user32.dll", SetLastError = true)]
        static extern long GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hWnd, int nIndex, long dwNewLong);

        private void label1_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.FillRectangle(new SolidBrush(BackColor), ClientRectangle);
            using (GraphicsPath gp = new GraphicsPath())
            using (Pen outline = new Pen(Color.Red, 3)
            { LineJoin = LineJoin.Round })
            using (StringFormat sf = new StringFormat())
            using (Brush foreBrush = new SolidBrush(ForeColor))
            {
                gp.AddString(Text, Font.FontFamily, (int)Font.Style,
                    Font.Size, ClientRectangle, sf);
                e.Graphics.ScaleTransform(1.3f, 1.35f);
                e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
                e.Graphics.DrawPath(outline, gp);
                e.Graphics.FillPath(foreBrush, gp);
            }
        }
    }
}
