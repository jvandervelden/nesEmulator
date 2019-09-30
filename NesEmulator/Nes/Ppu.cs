using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestPGE.Nes.Bus;

namespace TestPGE.Nes
{
    public class Ppu : IPPU
    {
        byte[] OAM = new byte[255];
        byte[] registers = new byte[8];

        public DataBus Bus { get; set; }

        public Ppu(DataBus bus)
        {
            Bus = bus;
        }

        public void Clock()
        {
            registers[0x02] = 0x80;
        }

        public byte ReadByte(uint address)
        {
            return Bus.Read(address);
        }

        public void WriteByte(uint address, byte data)
        {
            Bus.Write(address, data);
        }

        public void SetRegister(byte registerNumber, byte data)
        {
            if (registerNumber > 7)
            {
                throw new ArgumentOutOfRangeException($"Tried to set register {registerNumber} out of 7.");
            }

            registers[registerNumber] = data;
        }

        public byte GetRegister(byte registerNumber)
        {
            if (registerNumber > 7)
            {
                throw new ArgumentOutOfRangeException($"Tried to fetch register {registerNumber} out of 7.");
            }

            return registers[registerNumber];
        }

        public Image PrintPattern(byte tableNumber, byte sprite)
        {
            Color[] greyPallet = new Color[4];

            greyPallet[0] = Color.White;
            greyPallet[1] = Color.Gray;
            greyPallet[2] = Color.DarkGray;
            greyPallet[3] = Color.Black;

            Bitmap pattern = new Bitmap(8, 8);
            
            if (tableNumber <= 0x01)
            {
                for (byte pixelY = 0; pixelY <= 0x07; pixelY++)
                {
                    UInt16 msbAddress = (UInt16)((tableNumber << 12) | (sprite << 4) | pixelY);
                    UInt16 lsbAddress = (UInt16)((tableNumber << 12) | (sprite << 4) | (pixelY + 8));

                    byte colorIndexMsb = ReadByte(msbAddress);
                    byte colorIndexLsb = ReadByte(lsbAddress);

                    for (byte pixelX = 0; pixelX <= 0x07; pixelX++)
                    {
                        // Shift index by which pixel we are interested in, then mask it by 1 to get bit 0. MSB get shifted 1 and or'd with LSB.
                        byte colorIndex = (byte)((((colorIndexMsb >> pixelX) & 0x01) << 1) | ((colorIndexLsb >> pixelX) & 0x01));

                        pattern.SetPixel(pixelX, pixelY, greyPallet[colorIndex]);
                    }
                }
            }

            return pattern;
        }
    }
}
