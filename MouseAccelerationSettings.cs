namespace WindowsCycleMouseAccelerationTray;

public readonly record struct MouseAccelerationSettings
{
    public int Threshold1 { get; }
    public int Threshold2 { get; }
    public int EnabledFlag { get; }

    public bool IsEnabled => EnabledFlag == 1;

    public MouseAccelerationSettings(int threshold1, int threshold2, int enabledFlag)
    {
        Threshold1 = threshold1;
        Threshold2 = threshold2;
        EnabledFlag = enabledFlag;
    }

    public static MouseAccelerationSettings CreateDisabled()
        => new(0, 0, 0);

    public int[] ToArray() => new[] { Threshold1, Threshold2, EnabledFlag };

    public override string ToString()
        => $"[{Threshold1}, {Threshold2}, {EnabledFlag}] (Enabled: {IsEnabled})";
}