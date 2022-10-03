using System;
using System.Reactive.Linq;
using UsbDeviceInformationCollectorCore.Services;

namespace UsbDeviceInformationCollectorCore.Utils
{
    internal class DelayEraser
    {
        internal DelayEraser(DeviceManager manager)
        {
            Observable.FromEvent(handler => DevicePoolErase += handler,
                    handler => DevicePoolErase -= handler)
                .Throttle(TimeSpan.FromSeconds(0.2))
                .Subscribe(_ => { manager.EraseDevicePool(); });
        }

        internal virtual void OnDevicePoolErase()
        {
            DevicePoolErase?.Invoke();
        }

        private event Action DevicePoolErase;
    }
}