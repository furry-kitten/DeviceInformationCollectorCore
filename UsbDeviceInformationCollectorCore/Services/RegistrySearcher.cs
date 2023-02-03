using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Win32;
using UsbDeviceInformationCollectorCore.Converters;

namespace UsbDeviceInformationCollectorCore.Services
{
    internal class RegistrySearcher
    {
        internal string[] GetDisksIds()
        {
            var disksEnum = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\USBSTOR\Enum");
            if (disksEnum == null)
            {

            }
            var disksNames = disksEnum.GetValueNames().Where(name => Regex.IsMatch(name, @"^(?<index>\d+)$")).ToArray();
            List<string> disks = new();
            foreach (var index in disksNames)
            {
                var item = disksEnum.GetValue(index)?.ToString();
                if (string.IsNullOrWhiteSpace(item))
                {
                    continue;
                }

                var id = DeviceConverter.ExtractDeviceId(item);
                disks.Add(id);
            }

            return disks.ToArray();
        }
    }
}