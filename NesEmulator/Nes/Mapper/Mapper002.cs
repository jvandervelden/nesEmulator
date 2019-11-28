using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestPGE.Nes.Mapper
{
    public class Mapper002 : IMapper
    {
        private INesHeader _nesHeader;
        private uint _prgRomBankRegister = 0x00000000;

        private byte[] _programRomData;
        private byte[] _characterRomData;

        public uint NameTableSize { get; private set; } = 0;

        public Mapper002(INesHeader nesHeader, byte[] programRomData, byte[] characterRomData)
        {
            _nesHeader = nesHeader;
            _programRomData = programRomData;
            _characterRomData = characterRomData;

            // 8kb potential ram if no rom is present
            if (characterRomData.Length == 0)
                _characterRomData = new byte[0x2000];
        }

        public byte ChrRomRead(UInt16 ppuAddress)
        {
            return _characterRomData[ppuAddress];
        }

        public void ChrRomWrite(UInt16 ppuAddress, byte ppuData)
        {
            _characterRomData[ppuAddress] = ppuData;
        }

        public byte PrgRomRead(UInt16 cpuAddress)
        {
            return _programRomData[GetProgramAddress(cpuAddress)];
        }

        /// <summary>
        /// Lock last 16k to $C000 and switchs $8000 with program bank register.
        /// 
        /// Address: x xxyy yyyy yyyy yyyy
        /// 
        /// When address bit 4 is 0
        ///   
        ///   x - program bank register         ...x xx.. .... .... ....
        ///   y - Bottom 14 bits of cpu address .... ..yy yyyy yyyy yyyy
        /// 
        /// When address bit 4 is 1
        /// 
        ///   x - All 1s                        ...1 11.. .... .... ....
        ///   y - Bottom 14 bits of cpu address .... ..yy yyyy yyyy yyyy
        ///   
        /// </summary>
        /// <param name="cpuAddress">Cpu Address being accessed</param>
        /// <returns>Full program rom address</returns>
        private uint GetProgramAddress(UInt16 cpuAddress)
        {
            if ((cpuAddress & 0x4000) == 0x00)
                return cpuAddress & 0x3FFFu | _prgRomBankRegister;
            
            return cpuAddress & 0x3FFFu | 0x1C000;
        }

        /// <summary>
        /// Shifts the cpu data into the shift register. Moves data into bank register if at the end of the shift register.
        /// </summary>
        /// <param name="cpuAddress">Cpu address being accessed</param>
        /// <param name="cpuData">Data being written</param>
        /// <returns>The address into Non Volitile Memory, null if outside Ram bounds.</returns>
        public void PrgRomWrite(UInt16 cpuAddress, byte cpuData)
        {
            _prgRomBankRegister = (cpuData & 0x0Fu) << 14;
        }
    }
}
