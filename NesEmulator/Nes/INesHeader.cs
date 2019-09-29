namespace TestPGE.Nes
{
    public struct INesHeader
    {
        public byte[] NesHeader;
        public byte PrgRomSize;
        public byte ChrRomSize;
        public bool INes2;
        public bool HasTrainer;
        public ushort Mapper;
        public byte SubMapper;
        public bool HasNonVolMemory;
        public byte Flags6;
        public byte Flags7;
        public byte Flags8;
        public byte Flags9;
        public byte Flags10;
        public byte Flags11;
        public byte Flags12;
        public byte Flags13;
        public byte Flags14;
        public byte Flags15;
    }
}
