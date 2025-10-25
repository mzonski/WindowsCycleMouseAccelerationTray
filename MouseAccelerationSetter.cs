using System.Runtime.InteropServices;

namespace WindowsCycleMouseAccelerationTray;

public partial class MouseAccelerationSetter : IDisposable
{
    [LibraryImport("user32.dll", EntryPoint = "SystemParametersInfoA", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool SystemParametersInfo(uint uiAction, uint uiParam, IntPtr pvParam, uint fWinIni);
    
    // ReSharper disable InconsistentNaming
    private const uint SPI_GETMOUSE = 0x0003;
    private const uint SPI_SETMOUSE = 0x0004;
    private const uint SPIF_UPDATEINIFILE = 0x01;
    private const uint SPIF_SENDCHANGE = 0x02;
    // ReSharper restore InconsistentNaming

    private const int MouseParamsSize = 3;
    private const int MouseParamsBytes = MouseParamsSize * sizeof(int);

    private readonly Lock _lock = new();
    private MouseAccelerationSettings _savedSettings;
    private bool _disposed;

    public MouseAccelerationSetter()
    {
        var defaultSettings = new MouseAccelerationSettings(6, 10, 1);

        if (TryGetCurrentSettings(out var currentSettings))
        {
            _savedSettings = currentSettings.IsEnabled
                ? currentSettings
                : defaultSettings;
        }
        else
        {
            _savedSettings = defaultSettings;
        }
    }

    public bool IsMouseAccelerationEnabled()
    {
        ThrowIfDisposed();

        return TryGetCurrentSettings(out var settings) && settings.IsEnabled;
    }

    public bool DisableMouseAcceleration()
    {
        ThrowIfDisposed();

        lock (_lock)
        {
            if (TryGetCurrentSettings(out var currentSettings) && currentSettings.IsEnabled)
            {
                _savedSettings = currentSettings;
            }

            var disabledSettings = MouseAccelerationSettings.CreateDisabled();
            return TrySetSettings(disabledSettings);
        }
    }

    public bool EnableMouseAcceleration()
    {
        ThrowIfDisposed();

        lock (_lock)
        {
            var success = TrySetSettings(_savedSettings);
            return success;
        }
    }


    public MouseAccelerationSettings GetSavedSettings()
    {
        ThrowIfDisposed();
        lock (_lock)
        {
            return _savedSettings;
        }
    }

    private static bool TryGetCurrentSettings(out MouseAccelerationSettings settings)
    {
        settings = MouseAccelerationSettings.CreateDisabled();
        var ptr = IntPtr.Zero;

        try
        {
            ptr = Marshal.AllocHGlobal(MouseParamsBytes);
            var result = SystemParametersInfo(SPI_GETMOUSE, 0, ptr, 0);

            if (!result)
            {
                return false;
            }

            var mouseParams = new int[MouseParamsSize];
            Marshal.Copy(ptr, mouseParams, 0, MouseParamsSize);

            settings = new MouseAccelerationSettings(
                mouseParams[0],
                mouseParams[1],
                mouseParams[2]);

            return true;
        }
        catch
        {
            return false;
        }
        finally
        {
            if (ptr != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(ptr);
            }
        }
    }

    private static bool TrySetSettings(MouseAccelerationSettings settings)
    {
        var ptr = IntPtr.Zero;

        try
        {
            ptr = Marshal.AllocHGlobal(MouseParamsBytes);
            var mouseParams = settings.ToArray();
            Marshal.Copy(mouseParams, 0, ptr, MouseParamsSize);

            var result = SystemParametersInfo(
                SPI_SETMOUSE,
                0,
                ptr,
                SPIF_UPDATEINIFILE | SPIF_SENDCHANGE);

            return result;
        }
        catch
        {
            return false;
        }
        finally
        {
            if (ptr != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(ptr);
            }
        }
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }


    public void Dispose()
    {
        if (!_disposed)
        {
            _disposed = true;
        }
    }
}