using System;

namespace NesCSharp;

using CorePixelEngine;
using System;
using System.Net;

public class Olc2C02
{
    private const ushort PALETTE_OFFSET = 0x3f00;

    #region Register Structs
    private struct StatusRegister
    {
        private byte reg; // Backing field for the 8-bit register

        // Individual bit fields
        public byte Unused
        {
            get => (byte)(reg & 0b00011111); // Lower 5 bits
            set => reg = (byte)((reg & 0b11100000) | (value & 0b00011111));
        }

        public bool SpriteOverflow
        {
            get => (reg & 0b00100000) != 0; // Bit 5
            set => reg = (byte)(value ? (reg | 0b00100000) : (reg & ~0b00100000));
        }

        public bool SpriteZeroHit
        {
            get => (reg & 0b01000000) != 0; // Bit 6
            set => reg = (byte)(value ? (reg | 0b01000000) : (reg & ~0b01000000));
        }

        public bool VerticalBlank
        {
            get => (reg & 0b10000000) != 0; // Bit 7
            set => reg = (byte)(value ? (reg | 0b10000000) : (reg & ~0b10000000));
        }

        // Access the entire register
        public byte Reg
        {
            get => reg;
            set => reg = value;
        }
    }

    private struct MaskRegister
    {
        private byte reg; // Backing field for the 8-bit register

        // Individual bit fields
        public bool Grayscale
        {
            get => (reg & 0b00000001) != 0; // Bit 0
            set => reg = (byte)(value ? (reg | 0b00000001) : (reg & ~0b00000001));
        }

        public bool RenderBackgroundLeft
        {
            get => (reg & 0b00000010) != 0; // Bit 1
            set => reg = (byte)(value ? (reg | 0b00000010) : (reg & ~0b00000010));
        }

        public bool RenderSpritesLeft
        {
            get => (reg & 0b00000100) != 0; // Bit 2
            set => reg = (byte)(value ? (reg | 0b00000100) : (reg & ~0b00000100));
        }

        public bool RenderBackground
        {
            get => (reg & 0b00001000) != 0; // Bit 3
            set => reg = (byte)(value ? (reg | 0b00001000) : (reg & ~0b00001000));
        }

        public bool RenderSprites
        {
            get => (reg & 0b00010000) != 0; // Bit 4
            set => reg = (byte)(value ? (reg | 0b00010000) : (reg & ~0b00010000));
        }

        public bool EnhanceRed
        {
            get => (reg & 0b00100000) != 0; // Bit 5
            set => reg = (byte)(value ? (reg | 0b00100000) : (reg & ~0b00100000));
        }

        public bool EnhanceGreen
        {
            get => (reg & 0b01000000) != 0; // Bit 6
            set => reg = (byte)(value ? (reg | 0b01000000) : (reg & ~0b01000000));
        }

        public bool EnhanceBlue
        {
            get => (reg & 0b10000000) != 0; // Bit 7
            set => reg = (byte)(value ? (reg | 0b10000000) : (reg & ~0b10000000));
        }

        // Access the entire register
        public byte Reg
        {
            get => reg;
            set => reg = value;
        }
    }
    private struct PPUCTRL
    {
        private byte reg; // Backing field for the 8-bit register

        public bool NametableX
        {
            get => (reg & 0b00000001) != 0; // Bit 0
            set => reg = (byte)(value ? (reg | 0b00000001) : (reg & ~0b00000001));
        }

        public bool NametableY
        {
            get => (reg & 0b00000010) != 0; // Bit 1
            set => reg = (byte)(value ? (reg | 0b00000010) : (reg & ~0b00000010));
        }

        public bool IncrementMode
        {
            get => (reg & 0b00000100) != 0; // Bit 2
            set => reg = (byte)(value ? (reg | 0b00000100) : (reg & ~0b00000100));
        }

        public bool PatternSprite
        {
            get => (reg & 0b00001000) != 0; // Bit 3
            set => reg = (byte)(value ? (reg | 0b00001000) : (reg & ~0b00001000));
        }

        public bool PatternBackground
        {
            get => (reg & 0b00010000) != 0; // Bit 4
            set => reg = (byte)(value ? (reg | 0b00010000) : (reg & ~0b00010000));
        }

        public bool SpriteSize
        {
            get => (reg & 0b00100000) != 0; // Bit 5
            set => reg = (byte)(value ? (reg | 0b00100000) : (reg & ~0b00100000));
        }

        public bool SlaveMode
        {
            get => (reg & 0b01000000) != 0; // Bit 6 (Unused)
            set => reg = (byte)(value ? (reg | 0b01000000) : (reg & ~0b01000000));
        }

        public bool EnableNMI
        {
            get => (reg & 0b10000000) != 0; // Bit 7
            set => reg = (byte)(value ? (reg | 0b10000000) : (reg & ~0b10000000));
        }

        public byte Reg
        {
            get => reg;
            set => reg = value;
        }
    }

    private struct LoopyRegister
    {
        private ushort reg; // Backing field for the 16-bit register

        public ushort CoarseX
        {
            get => (ushort)(reg & 0b0000000000011111); // Lower 5 bits
            set => reg = (ushort)((reg & ~0b0000000000011111) | (value & 0b0000000000011111));
        }

        public ushort CoarseY
        {
            get => (ushort)((reg >> 5) & 0b0000000000011111); // Bits 5-9
            set => reg = (ushort)((reg & ~0b0000001111100000) | ((value & 0b0000000000011111) << 5));
        }

        public bool NametableX
        {
            get => (reg & 0b0000010000000000) != 0; // Bit 10
            set => reg = (ushort)(value ? (reg | 0b0000010000000000) : (reg & ~0b0000010000000000));
        }

        public bool NametableY
        {
            get => (reg & 0b0000100000000000) != 0; // Bit 11
            set => reg = (ushort)(value ? (reg | 0b0000100000000000) : (reg & ~0b0000100000000000));
        }

        public ushort FineY
        {
            get => (ushort)((reg >> 12) & 0b0000000000000111); // Bits 12-14
            set => reg = (ushort)((reg & ~0b0001110000000000) | ((value & 0b0000000000000111) << 12));
        }

        public bool Unused
        {
            get => (reg & 0b1000000000000000) != 0; // Bit 15
            set => reg = (ushort)(value ? (reg | 0b1000000000000000) : (reg & ~0b1000000000000000));
        }


        public ushort Reg
        {
            get => reg;
            set => reg = value;
        }
    }

    #endregion

    // Name Tables, Pattern Tables, and Palette Table
    private byte[,] tblName = new byte[2, 1024];
    private byte[,] tblPattern = new byte[2, 4096];
    private byte[] tblPalette = new byte[32];

    private Pixel[] palScreen = new Pixel[0x40];

    // Screen and Debug Sprites
    private Sprite sprScreen;
    private Sprite[] sprNameTable = new Sprite[2];
    private Sprite[] sprPatternTable = new Sprite[2];

    // Registers
    private StatusRegister status;
    private MaskRegister mask;
    private PPUCTRL control;
    private LoopyRegister vram_addr;    // Active "pointer" address into nametable to extract background tile information
    private LoopyRegister tram_addr;    // Temporary store of information to be "transferred" into "pointer" at various times

    // Pixel offset horizontally
    private byte fine_x = 0x00;

    private bool addressLatch = false;
    private byte ppuDataBuffer = 0x00;
    //private ushort ppuAddress = 0x0000;

    // Pixel "dot" position information
    private short scanline = 0;
    private short cycle = 0;

    // Backgroudn rendering
    private byte bg_next_tile_id = 0x00;
    private byte bg_next_tile_attrib = 0x00;
    private byte bg_next_tile_lsb = 0x00;
    private byte bg_next_tile_msb = 0x00;
    private ushort bg_shifter_pattern_hi = 0x0000;
    private ushort bg_shifter_pattern_lo = 0x0000;
    private ushort bg_shifter_attrib_lo = 0x0000;
    private ushort bg_shifter_attrib_hi = 0x0000;


    private Random rnd = new Random();

    // Cartridge Reference
    private Cartridge? cart;

    public bool FrameComplete { get; set; } = false;


    // Debugging Utilities
    public Sprite GetScreen()
    {
        return sprScreen;
    }

    public Sprite GetNameTable(byte i)
    {
        return sprNameTable[i];
    }

    public Sprite GetPatternTable(byte i, byte palette)
    {
        for (var nTileY = 0; nTileY < 16; nTileY++)
        {
            for (var nTileX = 0; nTileX < 16; nTileX++)
            {
                var nOffset = nTileY * 256 + nTileX * 16;

                for (var row = 0; row < 8; row++)
                {
                    var tile_lsb = PpuRead((ushort)(i * 0x1000 + nOffset + row + 0));
                    var tile_msb = PpuRead((ushort)(i * 0x1000 + nOffset + row + 8));

                    for (var col = 0; col < 8; col++)
                    {
                        var pixel = (tile_lsb & 0x01) + (tile_msb & 0x01);
                        tile_lsb >>= 1; tile_msb >>= 1;

                        sprPatternTable[i].SetPixel(
                            nTileX * 8 + (7 - col),
                            nTileY * 8 + row,
                            GetColourFromPaletteRam(palette, pixel));
                    }

                }
            }
        }

        return sprPatternTable[i];
    }

    public Pixel GetColourFromPaletteRam(byte palette, int pixel)
    {
        // This is a convenience function that takes a specified palette and pixel
        // index and returns the appropriate screen colour.
        // "0x3F00"       - Offset into PPU addressable range where palettes are stored
        // "palette << 2" - Each palette is 4 bytes in size
        // "pixel"        - Each pixel index is either 0, 1, 2 or 3
        // "& 0x3F"       - Stops us reading beyond the bounds of the palScreen array
        return palScreen[PpuRead((ushort)(PALETTE_OFFSET + (palette << 2) + pixel))];
    }

    // Constructor
    public Olc2C02()
    {

        palScreen[0x00] = new Pixel(84, 84, 84);
        palScreen[0x01] = new Pixel(0, 30, 116);
        palScreen[0x02] = new Pixel(8, 16, 144);
        palScreen[0x03] = new Pixel(48, 0, 136);
        palScreen[0x04] = new Pixel(68, 0, 100);
        palScreen[0x05] = new Pixel(92, 0, 48);
        palScreen[0x06] = new Pixel(84, 4, 0);
        palScreen[0x07] = new Pixel(60, 24, 0);
        palScreen[0x08] = new Pixel(32, 42, 0);
        palScreen[0x09] = new Pixel(8, 58, 0);
        palScreen[0x0A] = new Pixel(0, 64, 0);
        palScreen[0x0B] = new Pixel(0, 60, 0);
        palScreen[0x0C] = new Pixel(0, 50, 60);
        palScreen[0x0D] = new Pixel(0, 0, 0);
        palScreen[0x0E] = new Pixel(0, 0, 0);
        palScreen[0x0F] = new Pixel(0, 0, 0);

        palScreen[0x10] = new Pixel(152, 150, 152);
        palScreen[0x11] = new Pixel(8, 76, 196);
        palScreen[0x12] = new Pixel(48, 50, 236);
        palScreen[0x13] = new Pixel(92, 30, 228);
        palScreen[0x14] = new Pixel(136, 20, 176);
        palScreen[0x15] = new Pixel(160, 20, 100);
        palScreen[0x16] = new Pixel(152, 34, 32);
        palScreen[0x17] = new Pixel(120, 60, 0);
        palScreen[0x18] = new Pixel(84, 90, 0);
        palScreen[0x19] = new Pixel(40, 114, 0);
        palScreen[0x1A] = new Pixel(8, 124, 0);
        palScreen[0x1B] = new Pixel(0, 118, 40);
        palScreen[0x1C] = new Pixel(0, 102, 120);
        palScreen[0x1D] = new Pixel(0, 0, 0);
        palScreen[0x1E] = new Pixel(0, 0, 0);
        palScreen[0x1F] = new Pixel(0, 0, 0);

        palScreen[0x20] = new Pixel(236, 238, 236);
        palScreen[0x21] = new Pixel(76, 154, 236);
        palScreen[0x22] = new Pixel(120, 124, 236);
        palScreen[0x23] = new Pixel(176, 98, 236);
        palScreen[0x24] = new Pixel(228, 84, 236);
        palScreen[0x25] = new Pixel(236, 88, 180);
        palScreen[0x26] = new Pixel(236, 106, 100);
        palScreen[0x27] = new Pixel(212, 136, 32);
        palScreen[0x28] = new Pixel(160, 170, 0);
        palScreen[0x29] = new Pixel(116, 196, 0);
        palScreen[0x2A] = new Pixel(76, 208, 32);
        palScreen[0x2B] = new Pixel(56, 204, 108);
        palScreen[0x2C] = new Pixel(56, 180, 204);
        palScreen[0x2D] = new Pixel(60, 60, 60);
        palScreen[0x2E] = new Pixel(0, 0, 0);
        palScreen[0x2F] = new Pixel(0, 0, 0);

        palScreen[0x30] = new Pixel(236, 238, 236);
        palScreen[0x31] = new Pixel(168, 204, 236);
        palScreen[0x32] = new Pixel(188, 188, 236);
        palScreen[0x33] = new Pixel(212, 178, 236);
        palScreen[0x34] = new Pixel(236, 174, 236);
        palScreen[0x35] = new Pixel(236, 174, 212);
        palScreen[0x36] = new Pixel(236, 180, 176);
        palScreen[0x37] = new Pixel(228, 196, 144);
        palScreen[0x38] = new Pixel(204, 210, 120);
        palScreen[0x39] = new Pixel(180, 222, 120);
        palScreen[0x3A] = new Pixel(168, 226, 144);
        palScreen[0x3B] = new Pixel(152, 226, 180);
        palScreen[0x3C] = new Pixel(160, 214, 228);
        palScreen[0x3D] = new Pixel(160, 162, 160);
        palScreen[0x3E] = new Pixel(0, 0, 0);
        palScreen[0x3F] = new Pixel(0, 0, 0);

        sprScreen = new Sprite(256, 240);
        sprNameTable[0] = new Sprite(256, 240);
        sprNameTable[1] = new Sprite(256, 240);
        sprPatternTable[0] = new Sprite(128, 128);
        sprPatternTable[1] = new Sprite(128, 128);
    }

    // Communications with Main Bus
    public byte CpuRead(ushort address, bool readOnly = false)
    {
        byte data = 0x00;


        if (readOnly)
        {
            // Reading from PPU registers can affect their contents
            // so this read only option is used for examining the
            // state of the PPU without changing its state. This is
            // really only used in debug mode.
            switch (address)
            {
                case 0x0000: // Control
                    data = control.Reg;
                    break;
                case 0x0001: // Mask
                    data = mask.Reg;
                    break;
                case 0x0002: // Status
                    data = status.Reg;
                    break;
                case 0x0003: // OAM Address
                    break;
                case 0x0004: // OAM Data
                    break;
                case 0x0005: // Scroll
                    break;
                case 0x0006: // PPU Address
                    break;
                case 0x0007: // PPU Data
                    break;
            }
        }
        else
        {
            // These are the live PPU registers that repsond
            // to being read from in various ways. Note that not
            // all the registers are capable of being read from
            // so they just return 0x00
            switch (address)
            {
                case 0x0000: // Control - Not readable
                    break;
                case 0x0001: // Mask - Not readable
                    break;

                case 0x0002: // Status
                    // Reading from the status register has the effect of resetting
                    // different parts of the circuit. Only the top three bits
                    // contain status information, however it is possible that
                    // some "noise" gets picked up on the bottom 5 bits which 
                    // represent the last PPU bus transaction. Some games "may"
                    // use this noise as valid data (even though they probably
                    // shouldn't)
                    data = (byte)((status.Reg & 0xe0) | (ppuDataBuffer & 0x1f));

                    status.VerticalBlank = false;

                    // Reset Loopy's Address latch flag
                    addressLatch = false;

                    break;
                case 0x0003: // OAM Address
                    break;
                case 0x0004: // OAM Data
                    break;
                case 0x0005: // Scroll
                    break;
                case 0x0006: // PPU Address
                    break;
                case 0x0007: // PPU Data
                    // Reads from the NameTable ram get delayed one cycle, 
                    // so output buffer which contains the data from the 
                    // previous read request
                    data = ppuDataBuffer;

                    // then update the buffer for next time
                    ppuDataBuffer = PpuRead(vram_addr.Reg);

                    // However, if the address was in the palette range, the
                    // data is not delayed, so it returns immediately
                    if (vram_addr.Reg >= 0x3f00) data = ppuDataBuffer;

                    // All reads from PPU data automatically increment the nametable
                    // address depending upon the mode set in the control register.
                    // If set to vertical mode, the increment is 32, so it skips
                    // one whole nametable row; in horizontal mode it just increments
                    // by 1, moving to the next column
                    vram_addr.Reg += (byte)(control.IncrementMode ? 32 : 1);
                    break;
            }

        }

        return data;
    }

    public void CpuWrite(ushort address, byte data)
    {
        switch (address)
        {
            case 0x0000: // Control
                control.Reg = data;
                tram_addr.NametableX = control.NametableX;
                tram_addr.NametableY = control.NametableY;
                break;
            case 0x0001: // Mask
                mask.Reg = data;
                break;
            case 0x0002: // Status
                break;
            case 0x0003: // OAM Address
                break;
            case 0x0004: // OAM Data
                break;
            case 0x0005: // Scroll
                if (addressLatch)
                {
                    // First write to scroll register contains X offset in pixel space
                    // which we split into coarse and fine x values
                    fine_x = (byte)(data & 0x07);
                    tram_addr.CoarseX = (byte)(data >> 3);
                    addressLatch = true;
                }
                else
                {
                    // First write to scroll register contains Y offset in pixel space
                    // which we split into coarse and fine Y values
                    tram_addr.FineY = (byte)(data & 0x07);
                    tram_addr.CoarseY = (byte)(data >> 3);
                    addressLatch = false;
                }
                break;
            case 0x0006: // PPU Address
                if (!addressLatch)
                {
                    // PPU address bus can be accessed by CPU via the ADDR and DATA
                    // registers. The fisrt write to this register latches the high byte
                    // of the address, the second is the low byte. Note the writes
                    // are stored in the tram register...
                    //ppuAddress = (ushort)((ppuAddress & 0xff00) | (data << 8));
                    tram_addr.Reg = (ushort)((ushort)((data & 0x3f) << 8) | (tram_addr.Reg & 0x00ff));
                    addressLatch = true;
                }
                else
                {
                    //ppuAddress = (ushort)((ppuAddress & 0xff00) | data);
                    tram_addr.Reg = (ushort)((tram_addr.Reg & 0xff00) | data);
                    vram_addr.Reg = tram_addr.Reg;
                    addressLatch = false;
                }
                break;

            case 0x0007: // PPU Data
                PpuWrite(vram_addr.Reg, data);

                // All writes from PPU data automatically increment the nametable
                // address depending upon the mode set in the control register.
                // If set to vertical mode, the increment is 32, so it skips
                // one whole nametable row; in horizontal mode it just increments
                // by 1, moving to the next column
                vram_addr.Reg += (ushort) (control.IncrementMode ? 32 : 1);

                //ppuAddress++;
                break;
        }
    }

    // Communications with PPU Bus
    public byte PpuRead(ushort address, bool readOnly = false)
    {
        byte data = 0x00;
        address &= 0x3fff;

        if (cart?.PpuRead(address, out data) == true)
        {

        }
        else switch (address)
        {
            case <= 0x1fff:

                var patternIndex = (address & 0x1000) >> 12;
                var patternOffset = address & 0x0fff;

                data = tblPattern[patternIndex, patternOffset];
                break;
            case <= 0x3eff:
                address &= 0x0FFF;

                if (cart?.Mirror == Cartridge.MIRROR.VERTICAL)
                {
                    // Vertical
                    if (address <= 0x03FF)
                        data = tblName[0, address & 0x03FF];
                    if (address is >= 0x0400 and <= 0x07FF)
                        data = tblName[1, address & 0x03FF];
                    if (address is >= 0x0800 and <= 0x0BFF)
                        data = tblName[0, address & 0x03FF];
                    if (address is >= 0x0C00 and <= 0x0FFF)
                        data = tblName[1, address & 0x03FF];
                }
                else if (cart?.Mirror == Cartridge.MIRROR.HORIZONTAL)
                {
                    // Horizontal
                    if (address <= 0x03FF)
                        data = tblName[0, address & 0x03FF];
                    if (address is >= 0x0400 and <= 0x07FF)
                        data = tblName[0, address & 0x03FF];
                    if (address is >= 0x0800 and <= 0x0BFF)
                        data = tblName[1, address & 0x03FF];
                    if (address is >= 0x0C00 and <= 0x0FFF)
                        data = tblName[1, address & 0x03FF];
                }
                break;

            case <= 0x3fff:
                address &= 0x001f;

                if (address == 0x0010) address = 0x0000;
                if (address == 0x0014) address = 0x0004;
                if (address == 0x0018) address = 0x0008;
                if (address == 0x001C) address = 0x000C;


                data = tblPalette[address];
                break;
        }

        return data;
    }

    public void PpuWrite(ushort address, byte data)
    {
        // Implement PPU Bus write behavior here
        address &= 0x3fff;
        if (cart?.PpuWrite(address, data) == true)
        {

        }

        else switch (address)
        {
            case <= 0x1fff:
            {
                var patternIndex = (address & 0x1000) >> 12;
                var patternOffset = address & 0x0fff;

                tblPattern[patternIndex, patternOffset] = data;
                break;
            }
            case <= 0x3eff:
                address &= 0x0FFF;
                if (cart?.Mirror == Cartridge.MIRROR.VERTICAL)
                {
                    // Vertical
                    if (address <= 0x03FF)
                        tblName[0, address & 0x03FF] = data;
                    if (address is >= 0x0400 and <= 0x07FF)
                        tblName[1, address & 0x03FF] = data;
                    if (address is >= 0x0800 and <= 0x0BFF)
                        tblName[0, address & 0x03FF] = data;
                    if (address is >= 0x0C00 and <= 0x0FFF)
                        tblName[1, address & 0x03FF] = data;
                }
                else if (cart?.Mirror == Cartridge.MIRROR.HORIZONTAL)
                {
                    // Horizontal
                    if (address <= 0x03FF)
                        tblName[0, address & 0x03FF] = data;
                    if (address is >= 0x0400 and <= 0x07FF)
                        tblName[0, address & 0x03FF] = data;
                    if (address is >= 0x0800 and <= 0x0BFF)
                        tblName[1, address & 0x03FF] = data;
                    if (address is >= 0x0C00 and <= 0x0FFF)
                        tblName[1, address & 0x03FF] = data;
                }
                break;

            case <= 0x3fff:
            {
                address &= 0x001f;

                if (address == 0x0010) address = 0x0000;
                if (address == 0x0014) address = 0x0004;
                if (address == 0x0018) address = 0x0008;
                if (address == 0x001C) address = 0x000C;

                tblPalette[address] = data;
                break;
            }
        }
    }

    // Connect Cartridge
    public void ConnectCartridge(Cartridge cartridge)
    {
        cart = cartridge;
    }

    public void Reset()
    {
        addressLatch = false;
        ppuDataBuffer = 0x00;
        scanline = 0;
        cycle = 0;
        status.Reg = 0x00;
        mask.Reg = 0x00;
        control.Reg = 0x00;
        vram_addr.Reg = 0x0000;
        tram_addr.Reg = 0x0000;
    }

    // Clock function
    public void Clock()
    {
        if (scanline == -1 && cycle == 1)
        {
            status.VerticalBlank = false;
        } 

        // Implement PPU clock behavior here
        if (scanline == 241 && cycle == 1)
        {
            status.VerticalBlank = true;
            if (control.EnableNMI)
            {
                Nmi = true;
            }

        }

        // Fake some noise for now
        sprScreen.SetPixel(new VectorI2d(cycle - 1, scanline), palScreen[((rnd.Next() % 2) != 0) ? 0x3f : 0x30]);

        // Advance renderer - it never stops, it's relentless
        cycle++;
        if (cycle >= 341)
        {
            cycle = 0;
            scanline++;
            if (scanline >= 261)
            {
                scanline = -1;
                FrameComplete = true;
            }
        }
    }

    public bool Nmi { get; set; }
}
