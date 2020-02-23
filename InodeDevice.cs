using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Plugin.BLE.Abstractions.Contracts;

namespace EnergyMonitor
{
    public class InodeDevice
    {
        private IDevice _device;
        private ICharacteristic _eepromControlCharacteristic;
        private IDescriptor _clientConfigDescriptor;
        private const string INODE_UUID = "0000CB4A-5EFD-45BE-B5BE-158DF376D8AD";
        private const string EEPROM_PAGE_UUID = "0000CB4D-5EFD-45BE-B5BE-158DF376D8AD";
        private const string EEPROM_CONTROL_UUID = "0000CB4C-5EFD-45BE-B5BE-158DF376D8AD";

        public InodeDevice(IDevice device)
        {
            _device = device;
        }

        public async Task InitAsync()
        {
            var service = await _device.GetServiceAsync(Guid.Parse(INODE_UUID));
            var characteristics = await service.GetCharacteristicAsync(Guid.Parse(EEPROM_PAGE_UUID));
            _clientConfigDescriptor = await characteristics.GetDescriptorAsync(Guid.Parse("00002902-0000-1000-8000-00805f9b34fb"));

            _eepromControlCharacteristic = await service.GetCharacteristicAsync(Guid.Parse(EEPROM_CONTROL_UUID));
        }

        public async Task GetData()
        {
            try
            {
                await SendTimeToDevice();
                await SetArchiving(false);
                await SetReadMode();
                await ReadLastRecordAddress();
                await ReadRecordsNum();
            }
            catch (Exception exc)
            {
                var msg = exc.Message;
            }
            finally
            {
                await Finish();
            }
        }

        private async Task Finish()
        {
            //UUID_EEPROM_CONTROL < -0x09, 0x01(ustawienie trybu bufora cyklicznego)
            //UUID_EEPROM_CONTROL < -0x01, 0x01
            var data = new byte[2];
            data[0] = 0x09;
            data[1] = 0x01;
            await _eepromControlCharacteristic.WriteAsync(data);

            data[0] = 0x01;
            data[1] = 0x01;
            await _eepromControlCharacteristic.WriteAsync(data);
        }

        private async Task SendTimeToDevice()
        {
            //send actual time
            //UUID_EEPROM_CONTROL <- 0x04, 0x01,
            //time_stamp_0,
            //time_stamp_1,
            //time_stamp_2,
            //time_stamp_3
            var unixTimestamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            var data = new byte[6];
            data[0] = 0x04;
            data[1] = 0x01;
            var bytes = BitConverter.GetBytes(unixTimestamp);
            data[2] = bytes[3];
            data[3] = bytes[2];
            data[4] = bytes[1];
            data[5] = bytes[0];
            await _eepromControlCharacteristic.WriteAsync(data);
        }

        private async Task SetArchiving(bool enable)
        {
            //turn off archiving
            //UUID_EEPROM_CONTROL <- 0x02, 0x01
            var data = new byte[2];
            data[0] = 0x02;
            data[1] = 0x01;
            await _eepromControlCharacteristic.WriteAsync(data);
        }

        private async Task SetReadMode()
        {
            //choose data read mode
            //UUID_EEPROM_CONTROL <- 0x0B, 0x01(odczyt rewersyjny)
            var data = new byte[2];
            data[0] = 0x0B;
            data[1] = 0x01;
            await _eepromControlCharacteristic.WriteAsync(data);
        }

        private async Task ReadLastRecordAddress()
        {
            //read address of last record
            //UUID_EEPROM_CONTROL <- 0x07, 0x01, 0x10, 0x00
            //UUID_EEPROM_CONTROL->last_addr_lsb, last_addr_msb
            var data = new byte[4];
            data[0] = 0x07;
            data[1] = 0x01;
            data[2] = 0x10;
            data[3] = 0x00;
            await _eepromControlCharacteristic.WriteAsync(data);
            var response = await _eepromControlCharacteristic.ReadAsync();
            var lastRecordAddress = BitConverter.ToInt16(response, 0);
        }

        private async Task ReadRecordsNum()
        {
            //read number of records
            //UUID_EEPROM_CONTROL <- 0x07, 0x01, 0x12, 0x00
            //UUID_EEPROM_CONTROL->len_records_lsb, len_records_msb
            var data = new byte[4];
            data[0] = 0x07;
            data[1] = 0x01;
            data[2] = 0x12;
            data[3] = 0x00;
            await _eepromControlCharacteristic.WriteAsync(data);
            var response = await _eepromControlCharacteristic.ReadAsync();
            var recordsNum = BitConverter.ToInt16(response);
        }
    }
}