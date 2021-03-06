﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Bus
{
    public class DataBus
    {
        private uint _addressSpace;
        private IBusInterface[] _busConnections;

        public DataBus(uint addressSpace)
        {
            _addressSpace = addressSpace;
            _busConnections = new IBusInterface[addressSpace + 1];

            for (uint i = 0; i < addressSpace; i++) _busConnections[i] = null;
        }

        public void ConnectDevice(IBusInterface device, uint startAddress, uint endAddress)
        {
            if (device == null)
                throw new ArgumentNullException("Device cannot be null");

            if (endAddress > _addressSpace || startAddress < 0)
                throw new ArgumentException($"Address range must be between 0x00 and {_addressSpace:X4}");

            for (uint i = startAddress; i <= endAddress; i++)
            {
                if (_busConnections[i] != null)
                    throw new ArgumentException($"Bus already has a device connected at address {i:X4}");

                _busConnections[i] = device;
            }
        }

        public void Write(uint address, byte data)
        {
            _busConnections[address].WriteByte(address, data);
        }

        public byte Read(uint address, bool readOnly = false)
        {
            return _busConnections[address].ReadByte(address);
        }
    }
}
