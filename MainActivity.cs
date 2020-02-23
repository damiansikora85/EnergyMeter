using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.App;
using Android.Bluetooth;
using Android.Bluetooth.LE;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;

namespace EnergyMonitor
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
    public class MainActivity : AppCompatActivity, BluetoothAdapter.ILeScanCallback
    {
        private Plugin.BLE.Abstractions.Contracts.IAdapter _adapter;
        private IBluetoothLE _bluetoothLE;
        private RecyclerView _listView;
        private BleDevicesAdapter _listViewAdapter;
        private InodeDevice _inodeDevice;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            Android.Support.V7.Widget.Toolbar toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);

            // Use this check to determine whether BLE is supported on the device. Then
            // you can selectively disable BLE-related features.
            if (!PackageManager.HasSystemFeature(Android.Content.PM.PackageManager.FeatureBluetoothLe))
            {
                Toast.MakeText(this, "Not supported", ToastLength.Short).Show();
                Finish();
            }

            _bluetoothLE = CrossBluetoothLE.Current;
            _adapter = CrossBluetoothLE.Current.Adapter;
            _adapter.DeviceDiscovered += DeviceDiscovered;

            var scanBtn = FindViewById<Button>(Resource.Id.buttonScan);
            scanBtn.Click += ScanBtn_Click;

            var connectBtn = FindViewById<Button>(Resource.Id.buttonConnect);
            connectBtn.Click += ConnectBtn_Click;

            var getDataBtn = FindViewById<Button>(Resource.Id.buttonGetData);
            getDataBtn.Click += GetDataBtn_Click;

            Plugin.CurrentActivity.CrossCurrentActivity.Current.Init(this, savedInstanceState);

            _listView = FindViewById<RecyclerView>(Resource.Id.listView);
            _listView.HasFixedSize = true;

            var layoutManager = new LinearLayoutManager(this);
            _listView.SetLayoutManager(layoutManager);

            _listViewAdapter = new BleDevicesAdapter();
            _listView.SetAdapter(_listViewAdapter);
        }

        private async void GetDataBtn_Click(object sender, EventArgs e)
        {
            if(_inodeDevice != null)
            {
                await _inodeDevice.GetData();
            }
        }

        private async void ConnectBtn_Click(object sender, EventArgs e)
        {
            var device = _listViewAdapter.GetSelectedDevice();
            if (device != null)
            {
                await _adapter.ConnectToDeviceAsync(device);
                if (device.State == Plugin.BLE.Abstractions.DeviceState.Connected)
                {
                    _inodeDevice = new InodeDevice(device);
                    await _inodeDevice.InitAsync();
                }
            }
        }

        private void DeviceDiscovered(object sender, Plugin.BLE.Abstractions.EventArgs.DeviceEventArgs e)
        {
            _listViewAdapter.Add(e.Device);
            _listViewAdapter.NotifyDataSetChanged();
        }

        private async void ScanBtn_Click(object sender, EventArgs e)
        {
            _adapter.ScanMode = Plugin.BLE.Abstractions.Contracts.ScanMode.Balanced;
            await _adapter.StartScanningForDevicesAsync();
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menu_main, menu);
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            int id = item.ItemId;
            if (id == Resource.Id.action_settings)
            {
                return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        public void OnLeScan(BluetoothDevice device, int rssi, byte[] scanRecord)
        {
            throw new NotImplementedException();
        }

        protected override void OnStart()
        {
            base.OnStart();
            Task.Factory.StartNew(async () =>
            {
                var status = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Location);
                if (status != PermissionStatus.Granted)
                {
                    var result = await CrossPermissions.Current.RequestPermissionsAsync(new Permission[] { Permission.Location });
                    status = result.ContainsKey(Permission.Location) ? result[Permission.Location] : PermissionStatus.Disabled;
                }

                if (status == PermissionStatus.Granted)
                {
                    //Query permission
                }
                else if (status != PermissionStatus.Unknown)
                {
                    //location denied
                }
            });
        }
    }
}

