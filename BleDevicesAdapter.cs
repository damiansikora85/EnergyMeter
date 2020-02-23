using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Plugin.BLE.Abstractions.Contracts;

namespace EnergyMonitor
{
    public class BleDevicesAdapter : RecyclerView.Adapter
    {
        public class MyViewHolder : RecyclerView.ViewHolder
        {
            // each data item is just a string in this case
            public TextView textView;
            public int Index;
            public bool IsSelected;
            public MyViewHolder(TextView v) : base(v)
            {
                textView = v;
            }
        }

        private List<IDevice> _devices;
        private int _selectedIndex;

        public BleDevicesAdapter()
        {
            _devices = new List<IDevice>();
        }

        public override int ItemCount => _devices.Count;

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            if (holder is MyViewHolder myViewHolder)
            {
                myViewHolder.textView.Text = _devices[position].Name;
                myViewHolder.Index = position;
                myViewHolder.IsSelected = false;
                myViewHolder.textView.Click += (s,e)  =>
                {
                    myViewHolder.IsSelected = !myViewHolder.IsSelected;
                    _selectedIndex = myViewHolder.IsSelected ? myViewHolder.Index : -1;
                    myViewHolder.textView.SetBackgroundColor(myViewHolder.IsSelected ? Color.LightPink : Color.Transparent);
                };
            }
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            TextView v = (TextView)LayoutInflater.From(parent.Context)
                .Inflate(Resource.Layout.device_cell, parent, false);

            MyViewHolder vh = new MyViewHolder(v);
            return vh;
        }

        public void Add(IDevice device)
        {
            _devices.Add(device);
        }

        internal IDevice GetSelectedDevice() => _devices[_selectedIndex];
    }
}