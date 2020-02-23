using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Plugin.BLE.Abstractions.Contracts;

namespace EnergyMonitor
{
    public class BleDevice
    {
        private IDevice _device;

        public BleDevice(IDevice device)
        {
            _device = device;
        }

        public override string ToString()
        {
            return _device.Name;
        }
    }
}