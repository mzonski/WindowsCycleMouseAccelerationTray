namespace WindowsCycleMouseAccelerationTray;

internal enum MouseAccelerationState
{
    Disabled = 0,
    Enabled = 1
}

public class MouseAccelerationTray : ApplicationContext
{
    private readonly NotifyIcon _trayIcon;
    private readonly MouseAccelerationSetter _configSetter;
    private MouseAccelerationState _accelerationState;

    public MouseAccelerationTray()
    {
        _configSetter = new MouseAccelerationSetter();
        
        _accelerationState = _configSetter.IsMouseAccelerationEnabled() 
            ? MouseAccelerationState.Enabled 
            : MouseAccelerationState.Disabled;

        _trayIcon = new NotifyIcon()
        {
            Icon = CreateIcon(_accelerationState),
            Visible = true,
            Text = "Mouse Acceleration Toggle"
        };

        _trayIcon.Click += TrayIcon_Click;

        var contextMenu = new ContextMenuStrip();
        contextMenu.Items.Add("Exit", null, Exit_Click);
        _trayIcon.ContextMenuStrip = contextMenu;

        UpdateTrayIcon();
    }

    private void TrayIcon_Click(object sender, EventArgs e)
    {
        if (((MouseEventArgs)e).Button == MouseButtons.Left)
        {
            ToggleMouseAcceleration();
        }
    }

    private void ToggleMouseAcceleration()
    {
        _accelerationState = _accelerationState == MouseAccelerationState.Disabled
            ? MouseAccelerationState.Enabled
            : MouseAccelerationState.Disabled;

        var success = _accelerationState == MouseAccelerationState.Disabled
            ? _configSetter.DisableMouseAcceleration()
            : _configSetter.EnableMouseAcceleration();

        if (!success)
        {
            _accelerationState = _accelerationState == MouseAccelerationState.Disabled
                ? MouseAccelerationState.Enabled
                : MouseAccelerationState.Disabled;
            
            _trayIcon.ShowBalloonTip(5000, "Mouse Acceleration",
                "Failed to change mouse acceleration settings", ToolTipIcon.Error);
            return;
        }

        UpdateTrayIcon();
    }

    private void UpdateTrayIcon()
    {
        var status = _accelerationState == MouseAccelerationState.Disabled ? "OFF" : "ON";
        _trayIcon.Text = $"Mouse Acceleration: {status}";
        _trayIcon.Icon = CreateIcon(_accelerationState);
    }

    private Icon CreateIcon(MouseAccelerationState state)
    {
        var bitmap = new Bitmap(32, 32);
        using (var g = Graphics.FromImage(bitmap))
        {
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            
            using (var whitePen = new Pen(Color.White, 2f))
            {
                g.DrawLine(whitePen, 8, 4, 24, 4);
                g.DrawLine(whitePen, 6, 6, 8, 4);
                g.DrawLine(whitePen, 24, 4, 26, 6);
                g.DrawLine(whitePen, 6, 6, 6, 24);
                g.DrawLine(whitePen, 26, 6, 26, 24);
                g.DrawLine(whitePen, 6, 16, 26, 16);
                g.DrawLine(whitePen, 16, 4, 16, 16);
                g.DrawLine(whitePen, 6, 24, 8, 28);
                g.DrawLine(whitePen, 26, 24, 24, 28);
                g.DrawLine(whitePen, 8, 28, 24, 28);
            }
            
            using (var brush = new SolidBrush(state == MouseAccelerationState.Disabled ? Color.Red : Color.ForestGreen))
                g.FillEllipse(brush, 16, 16, 15, 15);
            
            using (var pen = new Pen(Color.Black, 2f))
                g.DrawEllipse(pen, 16, 16, 15, 15);
        }
        
        var hIcon = bitmap.GetHicon();
        var icon = Icon.FromHandle(hIcon);
        
        return icon;
    }

    private void Exit_Click(object sender, EventArgs e)
    {
        _trayIcon.Visible = false;
        Application.Exit();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _trayIcon?.Dispose();
        }

        base.Dispose(disposing);
    }
}