namespace NesCSharp;

public abstract class Mapper
{
    protected byte nPRGBanks; // Equivalent to uint8_t
    protected byte nCHRBanks; // Equivalent to uint8_t

    // Constructor
    protected Mapper(byte prgBanks, byte chrBanks)
    {
        nPRGBanks = prgBanks;
        nCHRBanks = chrBanks;
    }

    // Abstract methods to replace pure virtual functions
    public abstract bool CpuMapRead(ushort addr, out uint mappedAddr); // Equivalent to uint16_t and uint32_t
    public abstract bool CpuMapWrite(ushort addr, out uint mappedAddr);
    public abstract bool PpuMapRead(ushort addr, out uint mappedAddr);
    public abstract bool PpuMapWrite(ushort addr, out uint mappedAddr);
}