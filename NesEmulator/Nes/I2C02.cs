using Common.Bus;
using CorePixelEngine;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestPGE.Nes.Bus;

namespace TestPGE.Nes
{
    public interface I2C02 : IBusInterface
    {
        void SetRegister(byte registerNumber, byte data);
        byte GetRegister(byte registerNumber);

        void WriteOamByte(byte oamAddress, byte data);

        void WriteOamBlock(byte oamAddress, byte[] data);
        void Clock();

        Sprite RenderBackground();

        Sprite PrintPattern(byte tableNumber, byte sprite, byte? palette = null);
        Sprite PrintTile(byte nameTable, byte xTile, byte yTile);


        bool BackgroundRenderEnabled { get; }

        long RemainingDotsInFrame { get; }

        byte[] Registers { get; }
        UInt16 PpuAddress { get; }
        UInt16 TempRegister { get; }
        UInt16 YSpriteLine { get; }
        byte ScrollYPixel { get; }
        byte ScrollXPixel { get; }

        bool FrameReady { get; }
    }
}
