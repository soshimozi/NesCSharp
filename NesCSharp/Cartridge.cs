using NesCSharp;
using System.IO;
using static System.Runtime.InteropServices.JavaScript.JSType;

public class Cartridge
{
    private bool bImageValid = false;

    private byte nMapperID = 0;
    private byte nPRGBanks = 0;
    private byte nCHRBanks = 0;

    private List<byte> vPRGMemory = new();
    private List<byte> vCHRMemory = new();

    private Mapper? pMapper;

    public enum MIRROR
    {
        HORIZONTAL,
        VERTICAL,
        ONESCREEN_LO,
        ONESCREEN_HI,
    };

    public MIRROR Mirror = MIRROR.HORIZONTAL;

    // Define the iNES Header as a private nested struct
    private struct sHeader
    {
        public char[] name; // 4 characters
        public byte prg_rom_chunks;
        public byte chr_rom_chunks;
        public byte mapper1;
        public byte mapper2;
        public byte prg_ram_size;
        public byte tv_system1;
        public byte tv_system2;
        public byte[] unused; // 5 bytes
    }

    // Constructor
    public Cartridge(string sFilename)
    {
        var header = new sHeader
        {
            name = new char[4],
            unused = new byte[5]
        };

        //FileStream? fs = null;
        //BinaryReader? reader = null;

        if (!File.Exists(sFilename))
        {
            throw new FileNotFoundException($"File {sFilename} not found.");
        }

        using var fs = new FileStream(sFilename, FileMode.Open, FileAccess.Read);
        using var reader = new BinaryReader(fs);

        // Read the header
        header.name = reader.ReadChars(4);
        header.prg_rom_chunks = reader.ReadByte();
        header.chr_rom_chunks = reader.ReadByte();
        header.mapper1 = reader.ReadByte();
        header.mapper2 = reader.ReadByte();
        header.prg_ram_size = reader.ReadByte();
        header.tv_system1 = reader.ReadByte();
        header.tv_system2 = reader.ReadByte();
        header.unused = reader.ReadBytes(5);

        // Check for trainer
        if ((header.mapper1 & 0x04) != 0)
        {
            fs.Seek(512, SeekOrigin.Current);
        }

        // Determine Mapper ID
        nMapperID = (byte)(((header.mapper2 >> 4) << 4) | (header.mapper1 >> 4));

        Mirror = (header.mapper1 & 0x01) != 0 ? MIRROR.VERTICAL : MIRROR.HORIZONTAL;

        // "Discover" File Format
        byte nFileType = 1;

        if (nFileType == 0)
        {

        }

        if (nFileType == 1)
        {

            // Determine File Type (simplified to nFileType == 1)
            nPRGBanks = header.prg_rom_chunks;
            vPRGMemory = [.. reader.ReadBytes(nPRGBanks * 16384)];

            nCHRBanks = header.chr_rom_chunks;

            if (nCHRBanks == 0)
            {
                vCHRMemory = [.. reader.ReadBytes(8192)];
            }
            else
            {
                vCHRMemory = [.. reader.ReadBytes(nCHRBanks * 8192)];
            }
        }

        if (nFileType == 2)
        {

        }


        // Load the appropriate Mapper
        switch (nMapperID)
        {
            case 0:
                pMapper = new Mapper000(nPRGBanks, nCHRBanks);
                break;
            default:
                throw new NotSupportedException($"Mapper ID {nMapperID} is not supported.");
        }

        bImageValid = true;

    }

    public void Reset()
    {

    }
    // CPU Write
    public bool CpuWrite(ushort address, byte data)
    {
        if (pMapper?.CpuMapWrite(address, out var mappedAddress) == true)
        {
            vPRGMemory[(int)mappedAddress] = data;
            return true;
        }

        return false;
    }

// CPU Read
    public bool CpuRead(ushort address, out byte data)
    {
        data = 0;
        if (pMapper?.CpuMapRead(address, out var mappedAddress) == true)
        {
            data = vPRGMemory[(int)mappedAddress];
            return true;
        }

        return false;
    }

    // PPU Write
    public bool PpuWrite(ushort address, byte data)
    {
        if (pMapper?.PpuMapWrite(address, out var mappedAddress) == true)
        {
            vCHRMemory[(int)mappedAddress] = data;
            return true;
        }

        return false;
    }

    // PPU Read
    public bool PpuRead(ushort address, out byte data)
    {
        data = 0;
        if (pMapper?.PpuMapRead(address, out var mappedAddress) == true)
        {
            data = vCHRMemory[(int)mappedAddress];
            return true;
        }

        return false;
    }
}
