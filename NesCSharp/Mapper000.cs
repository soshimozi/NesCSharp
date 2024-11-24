using System.Net;

namespace NesCSharp;

public class Mapper000(byte prgBanks, byte chrBanks) : Mapper(prgBanks, chrBanks)
{
    public override bool CpuMapRead(ushort address, out uint mappedAddress)
    {
        mappedAddress = 0;

        if (address is >= 0x8000 and <= 0xFFFF)
        {
            mappedAddress = (uint)(address & (nPRGBanks > 1 ? 0x7FFF : 0x3FFF));
            return true;
        }

        return false;
    }

    public override bool CpuMapWrite(ushort address, out uint mappedAddress)
    {
        mappedAddress = 0;

        if (address is >= 0x8000 and <= 0xFFFF)
        {
            mappedAddress = (uint)(address & (nPRGBanks > 1 ? 0x7FFF : 0x3FFF));
            return true;
        }

        return false;
    }

    public override bool PpuMapRead(ushort address, out uint mappedAddress)
    {
        mappedAddress = 0;

        if (address <= 0x1FFF)
        {
            mappedAddress = address;
            return true;
        }

        return false;
    }

    public override bool PpuMapWrite(ushort address, out uint mappedAddress)
    {
        mappedAddress = 0;

        if (address <= 0x1FFF)
        {
            if (nCHRBanks == 0)
            {
                mappedAddress = address;
                return true;
            }
        }

        return false;
    }
}