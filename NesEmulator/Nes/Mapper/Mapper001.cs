﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestPGE.Nes.Memory;

namespace TestPGE.Nes.Mapper
{
    public class Mapper001 : IMapper, IDisposable
    {
        public const byte SHIFT_REGISTER_RESET_STATE = 0x10;
        public const byte CONTROL_REGISTER_RESET_STATE = 0x0C;

        private INesHeader _nesHeader;
        private byte ShiftRegister = SHIFT_REGISTER_RESET_STATE;
        private byte ControlRegister = CONTROL_REGISTER_RESET_STATE;
        private byte PrgRomBankRegister = 0x00;
        private byte ChrRomBankRegister1 = 0x00;
        private byte ChrRomBankRegister2 = 0x00;

        private byte[] _programRomData;
        private byte[] _characterRomData;
        private BatteryRam _programRamData;

        public uint NameTableSize { get; private set; } = 0;

        public Mapper001(INesHeader nesHeader, byte[] programRomData, byte[] characterRomData, string romFilePath)
        {
            _nesHeader = nesHeader;
            _programRomData = programRomData;
            _characterRomData = characterRomData;

            // If volitile memory then 8Kb present
            if (nesHeader.HasNonVolMemory)
                _programRamData = new BatteryRam(0x2000, romFilePath);

            // 128kb potential ram if no rom is present
            if (characterRomData.Length == 0)
                _characterRomData = new byte[0x20000];
        }

        public byte ChrRomRead(UInt16 ppuAddress)
        {
            long address = 0;

            switch ((ControlRegister & 0x10) >> 4)
            {
                case 0x00:
                    address = (ChrRomBankRegister1 & 0x1E << 12) | ppuAddress;
                    break;
                case 0x01:
                    address = ((ppuAddress >= 0x1000 ? ChrRomBankRegister2 : ChrRomBankRegister1) << 12) | ppuAddress;
                    break;
            }

            return _characterRomData[address];
        }

        public void ChrRomWrite(UInt16 ppuAddress, byte ppuData)
        {
            long address = 0;

            switch ((ControlRegister & 0x10) >> 4)
            {
                case 0x00:
                    address = (ChrRomBankRegister1 & 0x1E << 12) | ppuAddress;
                    break;
                case 0x01:
                    address = ((ppuAddress >= 0x1000 ? ChrRomBankRegister2 : ChrRomBankRegister1) << 12) | ppuAddress;
                    break;
            }

            _characterRomData[address] = ppuData;
        }

        public byte PrgRomRead(UInt16 cpuAddress)
        {
            if (cpuAddress >= 0x6000 && cpuAddress <= 0x7FFF)
                return _programRamData.ReadByte(cpuAddress);

            long address = 0;

            switch((ControlRegister & 0x0C) >> 2)
            {
                case 0x00:
                case 0x01:
                    address = GetPrgAddressFor32KMode(cpuAddress);
                    break;
                case 0x02:
                    address = GetPrgAddressForBankMode2(cpuAddress);
                    break;
                case 0x03:
                    address = GetPrgAddressForBankMode3(cpuAddress);
                    break;
            }

            return _programRomData[address];
        }

        /// <summary>
        /// 32k mode manages the top 3 bits of the 18 bit address.
        /// xx xyyy yyyy yyyy yyyy
        /// 
        /// x - Middle 3 bits of program bank Register .xxx.
        /// y - Bottom 15 bits of cpu address .yyy yyyy yyyy yyyy
        /// </summary>
        /// <param name="cpuAddress">Cpu Address being accessed</param>
        /// <returns>Full rom address</returns>
        // 
        private uint GetPrgAddressFor32KMode(UInt16 cpuAddress)
        {
            return (uint)((cpuAddress & 0x7FFF) | ((PrgRomBankRegister & 0x0E) << 14));
        }

        /// <summary>
        /// Bank mode 2 locks first 16k of rom to $8000 and switches $C000 with program bank register.
        /// 
        /// When address > $BFFF
        ///   xx xxyy yyyy yyyy yyyy
        ///   
        ///   x - Bottom 4 bits of program bank register .xxxx
        ///   y - Bottom 14 bits of cpu address ..yy yyyy yyyy yyyy
        /// 
        /// When address <= $BFFF
        ///   00 00yy yyyy yyyy yyyy
        ///   
        ///   y - Bottom 14 bits of cpu address ..yy yyyy yyyy yyyy
        /// 
        /// </summary>
        /// <param name="cpuAddress">Cpu Address being accessed</param>
        /// <returns>Full program rom address</returns>
        private uint GetPrgAddressForBankMode2(UInt16 cpuAddress)
        {
            if (cpuAddress > 0xBFFF)
                return (uint)((cpuAddress & 0x3FFF) | ((PrgRomBankRegister & 0x0F) << 14));

            return (uint)(cpuAddress & 0x3FFF);
        }

        /// <summary>
        /// Bank mode 3 locks last 16k to $C000 and switches $8000 with program bank register.
        /// 
        /// When address > $BFFF
        ///   xx xxyy yyyy yyyy yyyy
        ///   
        ///   x - Bottom 4 bits of program bank register .xxxx
        ///   y - Bottom 14 bits of cpu address ..yy yyyy yyyy yyyy
        /// 
        /// When address <= $BFFF
        ///   00 00yy yyyy yyyy yyyy
        ///   
        ///   y - Bottom 14 bits of cpu address ..yy yyyy yyyy yyyy
        /// 
        /// </summary>
        /// <param name="cpuAddress">Cpu Address being accessed</param>
        /// <returns>Full program rom address</returns>
        private uint GetPrgAddressForBankMode3(UInt16 cpuAddress)
        {
            if (cpuAddress < 0xC000)
                return (uint)((cpuAddress & 0x3FFF) | ((PrgRomBankRegister & 0x0F) << 14));
            
            return (uint)((cpuAddress & 0x3FFF) | ((_nesHeader.PrgRomSize - 1) << 14));
        }

        /// <summary>
        /// Shifts the cpu data into the shift register. Moves data into bank register if at the end of the shift register.
        /// </summary>
        /// <param name="cpuAddress">Cpu address being accessed</param>
        /// <param name="cpuData">Data being written</param>
        /// <returns>The address into Non Volitile Memory, null if outside Ram bounds.</returns>
        public void PrgRomWrite(UInt16 cpuAddress, byte cpuData)
        {
            bool writingToRam = (cpuAddress >= 0x6000 && cpuAddress <= 0x7FFF);
            bool resetShiftRegister = (cpuData & 0x80) == 0x80;

            // Only shift if not writing to ram
            if (!writingToRam)
            {
                // If the data doesn't flag a shift register reset, add the data to the shift register.
                if (!resetShiftRegister)
                {
                    resetShiftRegister = AddDataIntoShiftRegister(cpuData);

                    // If adding data flagged the shift register to be full, move it to the appropriate bank register.
                    if (resetShiftRegister)
                        MoveShiftRegisterToBankRegister(cpuAddress);
                }
                else
                {
                    ControlRegister = (byte)(ControlRegister | 0x0C);
                }

                if (resetShiftRegister)
                    ShiftRegister = SHIFT_REGISTER_RESET_STATE;
            }
            else
            {
                // Ram address is $6000 - $7FFF if enabled. Strip off the top 3 bits.
                _programRamData?.WriteByte(cpuAddress, cpuData);
            }
        }

        private bool AddDataIntoShiftRegister(byte cpuData)
        {
            bool ShiftRegisterFull = (ShiftRegister & 0x01) == 0x01;

            byte data = (byte)((cpuData & 0x01) << 4);

            ShiftRegister = (byte)(ShiftRegister >> 1);
            ShiftRegister |= data;

            return ShiftRegisterFull;
        }

        private void MoveShiftRegisterToBankRegister(ushort cpuAddress)
        {
            byte regAddress = (byte)((cpuAddress >> 13) & 0x03);

            switch (regAddress)
            { 
                // Reg 0 - Control Bank
                case 0x00:
                    ControlRegister = ShiftRegister; break;
                // Reg 1 - Character Rom Bank 1
                case 0x01:
                    ChrRomBankRegister1 = ShiftRegister; break;
                // Reg 2 - Character Rom Bank 2
                case 0x02:
                    ChrRomBankRegister2 = ShiftRegister; break;
                // Reg 3 - Program Rom Bank
                case 0x03:
                    PrgRomBankRegister = ShiftRegister; break;
            }
        }

        public void Dispose()
        {
            _programRamData?.Dispose();
        }
    }
}
