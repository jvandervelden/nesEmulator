using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestPGE.Nes.Bus;

namespace TestPGE.Nes
{
    public class Ppu : I2C02
    {
        private class ScanlineSprite
        {
            private byte _x = 0x00;
            public byte X { get { return _x; }
                set
                {
                    XTile = (byte)(value & 0xF8);
                    XPixel = (byte)(value & 0x07);
                    _x = value;
                }
            }
            private byte _y = 0x00;
            public byte Y
            {
                get { return _y; }
                set
                {
                    YTile = (byte)(value & 0xF8);
                    YPixel = (byte)(value & 0x07);
                    _y = value;
                }
            }
            public byte PatternIndex { get; set; } = 0x00;
            public byte Attributes { get; set; } = 0x00;
            public byte XTile { get; private set; } = 0x00;
            public byte XPixel { get; private set; } = 0x00;
            public byte YTile { get; private set; } = 0x00;
            public byte YPixel { get; private set; } = 0x00;
            public byte Palette { get { return (byte)(Attributes & 0x03); } }
            public byte Priority { get { return (byte)((Attributes & 0x20) >> 5); } }
            public bool FlipHor { get { return (Attributes & 0x40) == 0x40; } }
            public bool FlipVer { get { return (Attributes & 0x80) == 0x80; } }
            public byte HighPatternByte { get; private set; } = 0x00;
            public byte LowPatternByte { get; private set; } = 0x00;
            public bool Loaded { get; private set; } = false;

            public void Reset()
            {
                X = 0x00;
                Y = 0x00;
                PatternIndex = 0x00;
                Attributes = 0x00;
                HighPatternByte = 0x00;
                LowPatternByte = 0x00;
                Loaded = false;
            }

            public KeyValuePair<bool, byte> ColorIndexAtScreenDot(int dot)
            {
                byte xIdx = (byte)(dot - X);

                if (FlipHor)
                    xIdx = (byte)(7 - xIdx);

                byte colorIdx = (byte)((((HighPatternByte >> (7 - xIdx)) & 0x01) << 1) | 
                ((LowPatternByte >> (7 - xIdx)) & 0x01));

                return new KeyValuePair<bool, byte>(true, colorIdx);
            }

            public ScanlineSprite Load(DataBus ppuBus, byte patternTable, bool is8x16mode, int scanline)
            {
                // if (is8x16mode)
                // {

                // }
                // else
                // {
                // 8x16 000j iiii iiis hsss
                // 8x8  000p iiii iiii hsss
                // p - patternTable
                // i - patternIndex
                // j - lowest nibble of pattern index
                // h - high/low byte of pattern
                // s - scanline - y

                byte yIdx = (byte)(scanline - Y);

                if (FlipVer)
                    yIdx = (byte)((is8x16mode ? 15 : 7) - yIdx);

                byte pattTable = patternTable;
                byte pattIndex = PatternIndex;

                if (is8x16mode)
                {
                    // Set pattern table to lowest nibble of index and clear that bit
                    patternTable = (byte)(PatternIndex & 0x01);
                    PatternIndex = (byte)(PatternIndex & 0xFE);
                    // Move the 4th bit to the 5th
                    yIdx = (byte)((yIdx & 0x07) | ((yIdx & 0x08) << 1));
                }
                
                UInt16 patternAddress = (UInt16)((patternTable << 12) | (PatternIndex << 4) | yIdx);
                LowPatternByte = ppuBus.Read(patternAddress);
                HighPatternByte = ppuBus.Read((UInt16)(patternAddress | 0x0008));
               // }

                Loaded = true;
                return this;
            }
        }

        public const int CLOCK_DIVISOR = 4;
        public const int DOTS_PER_SCANLINE = 341;
        public const int NMI_SCANLINE = 241;
        public const int SCANLINES_PER_FRAME = 262;
        public const int DOTS_PER_FRAME = DOTS_PER_SCANLINE * SCANLINES_PER_FRAME;

        public const int PPU_CONTROL_REGISTER = 0;
        public const int PPU_MASK_REGISTER = 1;
        public const int PPU_STATUS_REGISTER = 2;
        public const int PPU_OAM_ADDRESS_REGISTER = 3;
        public const int PPU_OAM_DATA_REGISTER = 4;
        public const int PPU_SCROLL_REGISTER = 5;
        public const int PPU_ADDRESS_REGISTER = 6;
        public const int PPU_DATA_REGISTER = 7;

        public const byte VBLANK_STATUS_REGISTER_BIT = 0x80;
        public const byte NMI_CONTROL_REGISTER_BIT = 0x80;
        public const byte BACKGROUD_RENDER_BIT = 0x08;
        public const byte SPRITE_RENDER_BIT = 0x10;
        public const byte SPRITE_OVERFLOW_BIT = 0x20;
        public const byte SPRITE_0_HIT_REGISTER_BIT = 0x40;

        private byte[] _oam = new byte[256];
        private byte[] _spriteBuffer = new byte[256];

        private ScanlineSprite[] _scanlineSprites = new ScanlineSprite[8] {
            new ScanlineSprite(),
            new ScanlineSprite(),
            new ScanlineSprite(),
            new ScanlineSprite(),
            new ScanlineSprite(),
            new ScanlineSprite(),
            new ScanlineSprite(),
            new ScanlineSprite()
        };
        private byte[] _registers = new byte[8];
        private int _currentDot = 0;
        private int _currentScanline = 1;
        private bool _oddFrame = false;
        private I6502 _cpu;
        private UInt16 _ppuAddress = 0x0000;

        private int _bgDataIndex = 0;

        private bool _writeLatch = false;
        private UInt16 _tempRegister = 0x00;

        private SimpleTexture _backgroundTexture = new SimpleTexture(256, 240);

        public bool BackgroundRenderEnabled => (_registers[PPU_MASK_REGISTER] & BACKGROUD_RENDER_BIT) == BACKGROUD_RENDER_BIT;
        public bool SpriteRenderEnabled => (_registers[PPU_MASK_REGISTER] & SPRITE_RENDER_BIT) == SPRITE_RENDER_BIT;
        public bool ForcedBlanking => (_registers[PPU_MASK_REGISTER] & (SPRITE_RENDER_BIT | BACKGROUD_RENDER_BIT)) == 0x00;
        public bool InVBlank => (_registers[PPU_STATUS_REGISTER] & VBLANK_STATUS_REGISTER_BIT) == VBLANK_STATUS_REGISTER_BIT;

        public double FPS { get; private set; } = 0;

        public DataBus Bus { get; set; }

        public long RemainingDotsInFrame { get; private set; } = DOTS_PER_FRAME;

        public Ppu(DataBus bus, I6502 cpu)
        {
            Bus = bus;
            _cpu = cpu;
        }

        private Color[] _currentScanlineBG = new Color[8];

        private UInt16 _lowBgPatternValueShiftRegister = 0x0000;
        private UInt16 _highBgPatternValueShiftRegister = 0x0000;
        private byte _paletteIndexShiftRegister = 0x00;

        private byte _ppuBuffer = 0x00;
        private byte _attrTableValue = 0x00;
        private byte _lowBgPatternValue = 0x00;
        private byte _highBgPatternValue = 0x00;
        private byte _ySpriteLine = 0x00;
        private byte _scrollXPixel = 0x00;

        public void Clock()
        {
            if (_currentScanline == NMI_SCANLINE && _currentDot == 1)
                StartVBlank();
            else if (_currentScanline == 261 && _currentDot == 1)
            {
                _bgDataIndex = 0;
                _attrTableValue = 0x00;
                _lowBgPatternValue = 0x00;
                _highBgPatternValue = 0x00;

                _lowBgPatternValueShiftRegister = 0x0000;
                _highBgPatternValueShiftRegister = 0x0000;
                _paletteIndexShiftRegister = 0x00;
                                
                // Clear vblank and sprite overflow flags.
                _registers[PPU_STATUS_REGISTER] = (byte)(_registers[PPU_STATUS_REGISTER] & ~(SPRITE_0_HIT_REGISTER_BIT | VBLANK_STATUS_REGISTER_BIT | SPRITE_OVERFLOW_BIT));
                
            }

            if (!InVBlank && !ForcedBlanking)
            {
                if (_currentScanline < 240 && _currentDot == 0)
                {
                    UpdateScanlineSprites();
                }

                if (_currentScanline == 261 && _currentDot == 1)
                {
                    _ppuAddress = (UInt16)((_tempRegister & 0x0FFF) + 0x2000);
                    _ySpriteLine = (byte)((_tempRegister & 0x7000) >> 12);
                }

                // Load first tile of next scanline
                if ((_currentScanline < 240 || _currentScanline == 261) && _currentDot == 321)
                {
                    LoadBackgroundIntoRegisters(true);
                    PPUAddressIncrementX();
                }

                if (_currentScanline < 240 && _currentDot < 256 && _currentDot % 8 == 0)
                {

                    if ((_currentDot & 0x0007) == 0)
                    {
                        LoadBackgroundIntoRegisters(false);

                        if (_currentDot < 248)
                            PPUAddressIncrementX();
                    }

                    if (_currentDot == 248)
                    {
                        // Clear Hor Nametable and X Course
                        _ppuAddress &= 0xFBE0;
                        // Reset to horizantal nametable and x course pos from temp register to reset line.
                        _ppuAddress |= (UInt16)(_tempRegister & 0x041f);

                        if (_ySpriteLine <= 6)
                            _ySpriteLine++;
                        else
                        {
                            PPUAddressIncrementY();
                            _ySpriteLine = 0;
                        }
                    }
                }

                if (_currentScanline < 240 && _currentDot < 256)
                {
                    byte colorToDraw = (byte)(((_highBgPatternValueShiftRegister & 0x8000) >> 14 - _scrollXPixel) | ((_lowBgPatternValueShiftRegister & 0x8000) >> 15 - _scrollXPixel));
                    byte paletteToDraw = (byte)(_paletteIndexShiftRegister & 0x03);

                    if (_spriteBuffer[_currentDot] != 0xFF)
                    {
                        ScanlineSprite scanlineSprite = _scanlineSprites[_spriteBuffer[_currentDot]];
                        KeyValuePair<bool, byte> spritePixel = scanlineSprite.ColorIndexAtScreenDot(_currentDot);

                        if (spritePixel.Key)
                        {
                            // If sprite 0 non transparent pixel overlaps a non transparent bg pixel flag sprite 0 hit.
                            if (_spriteBuffer[_currentDot] == 0 && colorToDraw != 0 && spritePixel.Value != 0)
                                _registers[PPU_STATUS_REGISTER] |= SPRITE_0_HIT_REGISTER_BIT;

                            // Draw sprite pixel if foreground priority and not transparent or the background pixel is transparent.
                            if ((scanlineSprite.Priority == 0x00 && spritePixel.Value != 0x00) ||
                                (colorToDraw == 0x00 && scanlineSprite.Priority == 0x01))
                            {
                                colorToDraw = spritePixel.Value;
                                paletteToDraw = (byte)(scanlineSprite.Palette + 4);
                            }
                        }
                    }

                    UInt16 paletteAddress = colorToDraw == 0 ? (UInt16)0x3F00 : (UInt16)(0x3F00 + paletteToDraw * 4 + colorToDraw);

                    _backgroundTexture.Data[_bgDataIndex] = BaseColors.Palette[Bus.Read(paletteAddress)];
                    _bgDataIndex++;

                    _highBgPatternValueShiftRegister = (UInt16)(_highBgPatternValueShiftRegister << 1);
                    _lowBgPatternValueShiftRegister = (UInt16)(_lowBgPatternValueShiftRegister << 1);

                    if ((_currentDot & 0x07) == 0x07)
                        _paletteIndexShiftRegister = (byte)(_paletteIndexShiftRegister >> 2);
                }
            }

            MoveElectronGun();
        }

        private void UpdateScanlineSprites()
        {
            byte spritePatternTable = (byte)((_registers[PPU_MASK_REGISTER] & 0x04) >> 3);
            bool is8x16Sprites = (_registers[PPU_CONTROL_REGISTER] & 0x20) == 0x20;
            int spriteOam = 0;
            byte spriteCount = 0;
            int scanline = _currentScanline - 1;

            foreach (ScanlineSprite scanlineSprite in _scanlineSprites) scanlineSprite.Reset();
            for (int i = 0; i < _spriteBuffer.Length; i++) _spriteBuffer[i] = 0xFF;

            for (;spriteOam < 64; spriteOam++)
            {
                byte spriteY = _oam[spriteOam * 4 + 0];
                bool inScanline = scanline - spriteY < (is8x16Sprites ? 16 : 8) && scanline - spriteY >= 0;

                if (spriteCount < 8) {
                    if (inScanline)
                    {
                        _scanlineSprites[spriteCount].Y = _oam[spriteOam * 4 + 0];
                        _scanlineSprites[spriteCount].PatternIndex = _oam[spriteOam * 4 + 1];
                        _scanlineSprites[spriteCount].Attributes = _oam[spriteOam * 4 + 2];
                        _scanlineSprites[spriteCount].X = _oam[spriteOam * 4 + 3];
                        _scanlineSprites[spriteCount].Load(Bus, spritePatternTable, is8x16Sprites, scanline);

                        spriteCount++;
                    }
                }
                else if (inScanline)
                {
                    _registers[PPU_STATUS_REGISTER] |= SPRITE_OVERFLOW_BIT;
                    break;
                }
            }
            
            for (int i = spriteCount - 1; i >= 0; i--)
            {
                if (_scanlineSprites[i].Loaded && _scanlineSprites[i].X <= 0xF7)
                {
                    int xPos = _scanlineSprites[i].X;
                    byte spriteIdx = (byte)i;
                    _spriteBuffer[xPos + 0] = spriteIdx;
                    _spriteBuffer[xPos + 1] = spriteIdx;
                    _spriteBuffer[xPos + 2] = spriteIdx;
                    _spriteBuffer[xPos + 3] = spriteIdx;
                    _spriteBuffer[xPos + 4] = spriteIdx;
                    _spriteBuffer[xPos + 5] = spriteIdx;
                    _spriteBuffer[xPos + 6] = spriteIdx;
                    _spriteBuffer[xPos + 7] = spriteIdx;
                }
            }
        }

        private void PPUAddressIncrementY()
        {
            byte y = (byte)((_ppuAddress & 0x03E0) >> 5);

            if (y < 29)
                // Y corse is bit 6 - 10 so +1 at bit 6 (0x20)
                _ppuAddress += 0x0020;
            else
            {
                _ppuAddress &= 0x2C1F;
                _ppuAddress ^= 0x0800;
            }            
        }

        private void PPUAddressIncrementX()
        {
            if ((_ppuAddress & 0x001f) == 31)
            {
                _ppuAddress &= 0x2FE0;
                _ppuAddress ^= 0x0400;
            }
            else
                _ppuAddress++;
        }

        private void LoadBackgroundIntoRegisters(bool top)
        {
            // Read the tile information
            _ppuBuffer = Bus.Read(_ppuAddress);

            // Read the bg area information
            UInt16 attrAreaLocation = (UInt16)((_ppuAddress & 0x2C00) | 0x03C0 | ((_ppuAddress & 0x380) >> 4) | ((_ppuAddress & 0x001C) >> 2));
            byte attributeAreaValue = Bus.Read(attrAreaLocation);

            // Find which 2 bits of the area byte we are on and get the palette number.
            switch (((_ppuAddress & 0x0040) >> 5) | ((_ppuAddress & 0x0002) >> 1))
            {
                case 0:
                    _attrTableValue = (byte)(attributeAreaValue & 0x03);
                    break;
                case 1:
                    _attrTableValue = (byte)((attributeAreaValue & 0x0C) >> 2);
                    break;
                case 2:
                    _attrTableValue = (byte)((attributeAreaValue & 0x30) >> 4);
                    break;
                case 3:
                    _attrTableValue = (byte)((attributeAreaValue & 0xC0) >> 6);
                    break;
            }

            // load the background pattern top and bottom layers
            UInt16 backgroundPatternTableAddress = (UInt16)(((_ppuBuffer << 4) + _ySpriteLine) | ((_registers[PPU_CONTROL_REGISTER] & 0x10) << 8));

            _lowBgPatternValue = Bus.Read(backgroundPatternTableAddress);
            _highBgPatternValue = Bus.Read((UInt16)(backgroundPatternTableAddress + 8));

            // Put the pattern informaiton into the shift registers at the top or bottom of the 16 bit value.
            if (top)
            {
                _lowBgPatternValueShiftRegister = (UInt16)(_lowBgPatternValue << 8);
                _highBgPatternValueShiftRegister = (UInt16)(_highBgPatternValue << 8);
                _paletteIndexShiftRegister = (byte)(_attrTableValue);
            }
            else
            {
                _lowBgPatternValueShiftRegister = (UInt16)((_lowBgPatternValueShiftRegister & 0xFF00) | _lowBgPatternValue);
                _highBgPatternValueShiftRegister = (UInt16)((_highBgPatternValueShiftRegister & 0xFF00) | _highBgPatternValue);
                _paletteIndexShiftRegister = (byte)((_attrTableValue << 2) | (_paletteIndexShiftRegister & 0x03));
            }
        }

        private void StartVBlank()
        {
            if ((_registers[PPU_CONTROL_REGISTER] & NMI_CONTROL_REGISTER_BIT) != 0)
                _cpu.NMI();

            _registers[PPU_STATUS_REGISTER] = (byte)(_registers[PPU_STATUS_REGISTER] | VBLANK_STATUS_REGISTER_BIT);
        }

        private void MoveElectronGun()
        {
            _currentDot++;

            // On odd frames skip the last dot of the final scanline.
            if (_oddFrame && _currentDot == DOTS_PER_SCANLINE - 1 && _currentScanline == SCANLINES_PER_FRAME - 1)
            {
                _currentDot++;
                RemainingDotsInFrame--;
            }

            if (_currentDot == DOTS_PER_SCANLINE)
            {
                _currentDot = 0;
                _currentScanline++;
            }

            if (_currentScanline == SCANLINES_PER_FRAME)
            {
                _currentScanline = 0;
                RemainingDotsInFrame = DOTS_PER_FRAME;
                _oddFrame = !_oddFrame;
            }

            RemainingDotsInFrame--;
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
            switch (registerNumber)
            {
                case PPU_CONTROL_REGISTER:

                    if (_currentScanline > NMI_SCANLINE &&
                        // Only trigger if NMI bit is going from 0 to 1
                        (_registers[PPU_CONTROL_REGISTER] & NMI_CONTROL_REGISTER_BIT) == 0 &&
                        (data & NMI_CONTROL_REGISTER_BIT) != 0)
                        // Trigger an NMI
                        _cpu.NMI();

                    _tempRegister = (UInt16)(_tempRegister & 0xF3FF | ((data & 0x03) << 10));

                    break;
                case PPU_MASK_REGISTER:
                    break;
                case PPU_STATUS_REGISTER:
                    // Don't update the status register
                    return;
                case PPU_OAM_ADDRESS_REGISTER:
                    break;
                case PPU_OAM_DATA_REGISTER:
                    break;
                case PPU_SCROLL_REGISTER:
                    if (!_writeLatch)
                    {
                        _writeLatch = true;
                        _scrollXPixel = (byte)(data & 0x07);
                        _tempRegister = (UInt16)(_tempRegister & 0xFFE0 | ((data & 0xF6) >> 3));
                    }
                    else
                    {
                        _writeLatch = false;
                        _tempRegister = (UInt16)(_tempRegister & 0xF3FF | ((data & 0xC0) << 2));
                        _tempRegister = (UInt16)(_tempRegister & 0xFF1F | ((data & 0x38) << 2));
                        _tempRegister = (UInt16)(_tempRegister & 0x0FFF | ((data & 0x07) << 12));
                    }
                    break;
                case PPU_ADDRESS_REGISTER:
                    if (!_writeLatch)
                    {
                        _writeLatch = true;
                        _tempRegister = (UInt16)(_tempRegister & 0x00FF | ((data & 0x3F) << 8));
                    }
                    else
                    {
                        _writeLatch = false;
                        _tempRegister = (UInt16)(_tempRegister & 0xFF00 | data);
                        _ppuAddress = _tempRegister;
                    }
                    break;
                case PPU_DATA_REGISTER:
                    Bus.Write(_ppuAddress, data);
                    IncrementPPUAddress();
                    break;
                default:
                    throw new ArgumentOutOfRangeException($"Tried to set register {registerNumber} out of 7.");
            }

            _registers[registerNumber] = data;
        }

        public byte GetRegister(byte registerNumber)
        {
            byte registerValue = _registers[registerNumber];

            switch (registerNumber)
            {
                // Non readable registers.
                case PPU_CONTROL_REGISTER:
                case PPU_MASK_REGISTER:
                case PPU_SCROLL_REGISTER:
                case PPU_ADDRESS_REGISTER:
                    return 0x0000;

                case PPU_STATUS_REGISTER:
                    _writeLatch = false;

                    // Clear VBlank when reading status register.
                    _registers[PPU_STATUS_REGISTER] = (byte)(registerValue & ~VBLANK_STATUS_REGISTER_BIT);
                    break;
                case PPU_OAM_ADDRESS_REGISTER:
                    break;
                case PPU_OAM_DATA_REGISTER:
                    break;
                case PPU_DATA_REGISTER:
                    if (_ppuAddress >= 0x3F00)
                    {
                        // Return pallet data right away and set buffer to underlying nametable
                        registerValue = Bus.Read(_ppuAddress);
                        _ppuBuffer = Bus.Read((UInt16)(_ppuAddress - 0x1000));
                    }
                    else
                    {
                        registerValue = (byte)_ppuBuffer;
                        _ppuBuffer = Bus.Read(_ppuAddress);
                    }

                    IncrementPPUAddress();
                    break;
                default:
                    throw new ArgumentOutOfRangeException($"Tried to fetch register {registerNumber} out of 7.");
            }

            return registerValue;
        }

        private void IncrementPPUAddress()
        {
            _ppuAddress += (UInt16)((_registers[PPU_CONTROL_REGISTER] & 0x04) == 0x04 ? 32 : 1);

            if (_ppuAddress > 0x3FFF)
                _ppuAddress -= 0x2000;
        }

        public SimpleTexture RenderBackground()
        {
            return _backgroundTexture;
        }

        private static Color[] _plainPalette = new Color[] { Color.White, Color.LightGray, Color.Gray, Color.Black };

        public SimpleTexture PrintPattern(byte tableNumber, byte sprite, byte? palette = null)
        {
            SimpleTexture patternTexture = new SimpleTexture(8, 8);
            
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
                        byte colorIndex = (byte)(((colorIndexMsb >> pixelX - 1) & 0x02) | ((colorIndexLsb >> pixelX) & 0x01));
                        
                        patternTexture.Data[pixelY * 8 + (7 - pixelX)] =
                            palette.HasValue 
                                ? BaseColors.Palette[Bus.Read((UInt16)(0x3F00 + palette.Value * 4 + colorIndex)) & 0x3F]
                                : _plainPalette[colorIndex];
                    }
                }
            }

            return patternTexture;
        }
        
        public void WriteOamByte(byte oamAddress, byte data)
        {
            this._oam[oamAddress] = data;
        }

        public void WriteOamBlock(byte oamAddress, byte[] data)
        {
            Array.Copy(data, 0, _oam, oamAddress, data.Length);
        }
    }
}
