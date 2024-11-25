using System.DirectoryServices;

namespace NesCSharp;

public class Bus
{
    public Bus()
    {
        Cpu = new Olc6502();
        Ppu = new Olc2C02();

        CpuRam = new byte[2048]; // Equivalent to std::array<uint8_t, 2048>
        Cpu.ConnectBus(this);
    }


    public Olc6502 Cpu { get; }
    public Olc2C02 Ppu { get; }

    private Cartridge? _cart;

    // System clock counter
    private uint _nSystemClockCounter = 0;

    // 2KB of CPU RAM
    private byte[] CpuRam { get; }

    public void Clock()
    {
        Ppu.Clock();
        if (_nSystemClockCounter % 3 == 0)
        {
            // cpu runs 3x slower than system clock
            Cpu.Clock();
        }

        if (Ppu.Nmi)
        {
            Ppu.Nmi = false;
            Cpu.Nmi();
        }

        _nSystemClockCounter++;
    }

    public void Reset()
    {
        _cart?.Reset();
        Cpu.Reset();
        Ppu.Reset();

        _nSystemClockCounter = 0;
    }

    public void InsertCartridge(Cartridge cartridge)
    {
        _cart = cartridge;
        Ppu.ConnectCartridge(cartridge);
    }

    public byte CpuRead(ushort address, bool readOnly = false)
    {
        if (_cart?.CpuRead(address, out var data) == true)
        {
            return data;
        } 
        
        data = address switch
        {
            <= 0x1fff => CpuRam[address & 0x07ff],
            >= 0x2000 and <= 0x3fff => Ppu.CpuRead((ushort)(address & 0x0007), readOnly),
            _ => 0x00
        };

        return data;
    }

    public void CpuWrite(ushort address, byte data)
    {
        if (_cart?.CpuWrite(address, data) == true)
        {
            return;
        }


        switch (address)
        {
            case <= 0x1fff:
                CpuRam[address & 0x07fff] = data;
                break;
            case <= 0x3fff:
                Ppu.CpuWrite((ushort)(address & 0x0007), data);
                break;
        }
    }


}