namespace OJS.Workers.Common.ServiceInstaller.Models
{
    internal enum ServiceControl
    {
        Stop = 0x00000001,
        Pause = 0x00000002,
        Continue = 0x00000003,
        Interrogate = 0x00000004,
        Shutdown = 0x00000005,
        ParamChange = 0x00000006,
        NetBindAdd = 0x00000007,
        NetBindRemove = 0x00000008,
        NetBindEnable = 0x00000009,
        NetBindDisable = 0x0000000A
    }
}
