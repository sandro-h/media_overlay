using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Runtime.InteropServices;
using Windows.Media.Control;

namespace media_overlay
{
    public partial class Form1 : Form
    {
        private GlobalSystemMediaTransportControlsSession? currentSession = null;
        private readonly AppConfig config;

        public Form1(AppConfig config)
        {
            this.config = config;

            InitializeComponent();

            this.BackColor = Color.FromArgb(1, 1, 1); //Color.FromArgb(254, 254, 254);
            this.TransparencyKey = this.BackColor;

            // Apply configuration settings
            ApplyConfigurationSettings();

            label1.Text = "";
            label1.Left = 0;
            label1.Top = 0;
            label1.Width = this.ClientSize.Width - 4;
            label1.Height = this.ClientSize.Height;
            label1.OutlineWidth = config.Display.OutlineWidth;
            label1.OutlineForeColor = ConfigLoader.HexToColor(config.Display.OutlineColor);
            label1.ForeColor = ConfigLoader.HexToColor(config.Display.TextColor);
            label1.Font = new Font(config.Display.FontFamily, config.Display.FontSize);

            // The magic sauce to make the window completely ignore mouse clicks and hovers (including on non-transparent parts).
            // All other online solutions didn't work.
            // https://stackoverflow.com/a/50245502
            int GWL_EXSTYLE = -20;
            long WS_EX_TRANSPARENT = 0x00000020L;
            long WS_EX_LAYERED = 0x00080000L;
            var cur_style = GetWindowLong(this.Handle, GWL_EXSTYLE);
            SetWindowLong(this.Handle, GWL_EXSTYLE, cur_style | WS_EX_TRANSPARENT | WS_EX_LAYERED);
        }

        private void ApplyConfigurationSettings()
        {
            // Set window size
            this.ClientSize = new Size(config.Display.Width, config.Display.Height);

            this.StartPosition = FormStartPosition.Manual;

            // Determine target monitor
            int monitorIndex = config.Display.MonitorIndex;
            if (monitorIndex < 0)
                monitorIndex = Screen.AllScreens.Length - 1; // Use last monitor

            if (monitorIndex >= Screen.AllScreens.Length)
                monitorIndex = Screen.AllScreens.Length - 1;

            Rectangle monitorBounds = Screen.AllScreens[monitorIndex].Bounds;

            // Calculate position based on corner
            int x, y;
            ContentAlignment labelAlignment;

            switch (config.Display.Corner)
            {
                case ScreenCorner.TopLeft:
                    x = monitorBounds.X;
                    y = monitorBounds.Y;
                    labelAlignment = ContentAlignment.TopLeft;
                    break;
                case ScreenCorner.TopRight:
                    x = monitorBounds.X + monitorBounds.Width - this.Width;
                    y = monitorBounds.Y;
                    labelAlignment = ContentAlignment.TopRight;
                    break;
                case ScreenCorner.BottomLeft:
                    x = monitorBounds.X;
                    y = monitorBounds.Y + monitorBounds.Height - this.Height;
                    labelAlignment = ContentAlignment.BottomLeft;
                    break;
                case ScreenCorner.BottomRight:
                    x = monitorBounds.X + monitorBounds.Width - this.Width;
                    y = monitorBounds.Y + monitorBounds.Height - this.Height;
                    labelAlignment = ContentAlignment.BottomRight;
                    break;
                default:
                    x = monitorBounds.X + monitorBounds.Width - this.Width;
                    y = monitorBounds.Y;
                    labelAlignment = ContentAlignment.TopRight;
                    break;
            }

            this.Location = new Point(x, y);

            // Apply label alignment to match corner
            label1.TextAlign = labelAlignment;
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
            var newCurrentSession = sessionManager.GetCurrentSession();
            if (!isValidSession(newCurrentSession))
            {
                return;
            }

            if (currentSession != null)
            {
                currentSession.MediaPropertiesChanged -= MediaPropertiesChanged;
                currentSession.PlaybackInfoChanged -= PlaybackInfoChanged;
            }

            currentSession = newCurrentSession;
            if (currentSession != null)
            {
                currentSession.MediaPropertiesChanged += MediaPropertiesChanged;
                currentSession.PlaybackInfoChanged += PlaybackInfoChanged;
            }

            update(currentSession);
        }

        private bool isValidSession(GlobalSystemMediaTransportControlsSession? session)
        {
            // This can be a process name or some hex value and probably other things...
            var name = session.SourceAppUserModelId;
            if (config.MediaFilter.ExcludePrograms.Any(f => name == f))
            {
                return false;
            }
            if (config.MediaFilter.IncludePrograms.Count >0 && !config.MediaFilter.IncludePrograms.Any(f => name == f))
            {
                return false;
            }
            return true;
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
                // Apply format from configuration
                newText = config.Format
                    .Replace("{Artist}", mediaProperties.Artist)
                    .Replace("{Title}", mediaProperties.Title)
                    .Replace("{Album}", mediaProperties.AlbumTitle)
                    .Replace("{Program}", session.SourceAppUserModelId);
            }

            label1.InvokeAsync(() => label1.Text = newText);
        }

        [DllImport("user32.dll", SetLastError = true)]
        static extern long GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hWnd, int nIndex, long dwNewLong);
    }
}
