using System;

namespace NesCSharp;

public class Olc6502
{
    const ushort STACK_OFFSET = 0x0100;

    // Instruction struct
    private class Instruction
    {
        public string Name { get; set; }
        public Func<byte> Operate { get; set; }
        public Func<byte> AddrMode { get; set; }
        public byte Cycles { get; set; }
    }

    private List<Instruction> lookup;

    // Private Bus reference
    private Bus? _bus;
    private byte _cycles = 0x00;

    // Fetch data for instructions
    private byte _fetched;

    // Addressing data
    public ushort AddressAbs { get; private set; } = 0x0000;
    public ushort AddressRel { get; private set; } = 0x00;

    // Opcode and cycle information
    public byte Opcode { get; private set; } = 0x00;


    // Registers and flags
    public byte A { get; private set; } = 0x00;

    public byte X { get; private set; } = 0x00;

    public byte Y { get; private set; } = 0x00;

    public byte Sp { get; private set; } = 0x00;

    public ushort Pc { get; private set; } = 0x0000;

    public byte Status { get; private set; } = 0x00; // Status Register

    // Constructor
    public Olc6502()
    {
        // Initialize the lookup table with instruction details
        lookup =
        [
            new Instruction { Name = "BRK", Operate = BRK, AddrMode = IMM, Cycles = 7 },
            new Instruction { Name = "ORA", Operate = ORA, AddrMode = IZX, Cycles = 6 },
            new Instruction { Name = "???", Operate = XXX, AddrMode = IMP, Cycles = 2 },
            new Instruction { Name = "???", Operate = XXX, AddrMode = IMP, Cycles = 8 },
            new Instruction { Name = "???", Operate = NOP, AddrMode = IMP, Cycles = 3 },
            new Instruction { Name = "ORA", Operate = ORA, AddrMode = ZP0, Cycles = 3 },
            new Instruction { Name = "ASL", Operate = ASL, AddrMode = ZP0, Cycles = 5 },
            new Instruction { Name = "???", Operate = XXX, AddrMode = IMP, Cycles = 5 },
            new Instruction { Name = "PHP", Operate = PHP, AddrMode = IMP, Cycles = 3 },
            new Instruction { Name = "ORA", Operate = ORA, AddrMode = IMM, Cycles = 2 },
            new Instruction { Name = "ASL", Operate = ASL, AddrMode = IMP, Cycles = 2 },
            new Instruction { Name = "???", Operate = XXX, AddrMode = IMP, Cycles = 2 },
            new Instruction { Name = "???", Operate = NOP, AddrMode = IMP, Cycles = 4 },
            new Instruction { Name = "ORA", Operate = ORA, AddrMode = ABS, Cycles = 4 },
            new Instruction { Name = "ASL", Operate = ASL, AddrMode = ABS, Cycles = 6 },
            new Instruction { Name = "???", Operate = XXX, AddrMode = IMP, Cycles = 6 },

            new Instruction { Name = "BPL", Operate = BPL, AddrMode = REL, Cycles = 2 },
            new Instruction { Name = "ORA", Operate = ORA, AddrMode = IZY, Cycles = 5 },
            new Instruction { Name = "???", Operate = XXX, AddrMode = IMP, Cycles = 2 },
            new Instruction { Name = "???", Operate = XXX, AddrMode = IMP, Cycles = 8 },
            new Instruction { Name = "???", Operate = NOP, AddrMode = IMP, Cycles = 4 },
            new Instruction { Name = "ORA", Operate = ORA, AddrMode = ZPX, Cycles = 4 },
            new Instruction { Name = "ASL", Operate = ASL, AddrMode = ZPX, Cycles = 6 },
            new Instruction { Name = "???", Operate = XXX, AddrMode = IMP, Cycles = 6 },
            new Instruction { Name = "CLC", Operate = CLC, AddrMode = IMP, Cycles = 2 },
            new Instruction { Name = "ORA", Operate = ORA, AddrMode = ABY, Cycles = 4 },
            new Instruction { Name = "???", Operate = NOP, AddrMode = IMP, Cycles = 2 },
            new Instruction { Name = "???", Operate = XXX, AddrMode = IMP, Cycles = 7 },
            new Instruction { Name = "???", Operate = NOP, AddrMode = IMP, Cycles = 4 },
            new Instruction { Name = "ORA", Operate = ORA, AddrMode = ABX, Cycles = 4 },
            new Instruction { Name = "ASL", Operate = ASL, AddrMode = ABX, Cycles = 7 },
            new Instruction { Name = "???", Operate = XXX, AddrMode = IMP, Cycles = 7 },
            
            new Instruction { Name = "JSR", Operate = JSR, AddrMode = ABS, Cycles = 6 },
            new Instruction { Name = "AND", Operate = AND, AddrMode = IZX, Cycles = 6 },
            new Instruction { Name = "???", Operate = XXX, AddrMode = IMP, Cycles = 2 },
            new Instruction { Name = "???", Operate = XXX, AddrMode = IMP, Cycles = 8 },
            new Instruction { Name = "BIT", Operate = BIT, AddrMode = ZP0, Cycles = 3 },
            new Instruction { Name = "AND", Operate = AND, AddrMode = ZP0, Cycles = 3 },
            new Instruction { Name = "ROL", Operate = ROL, AddrMode = ZP0, Cycles = 5 },
            new Instruction { Name = "???", Operate = XXX, AddrMode = IMP, Cycles = 5 },
            new Instruction { Name = "PLP", Operate = PLP, AddrMode = IMP, Cycles = 4 },
            new Instruction { Name = "AND", Operate = AND, AddrMode = IMM, Cycles = 2 },
            new Instruction { Name = "ROL", Operate = ROL, AddrMode = IMP, Cycles = 2 },
            new Instruction { Name = "???", Operate = XXX, AddrMode = IMP, Cycles = 2 },
            new Instruction { Name = "BIT", Operate = BIT, AddrMode = ABS, Cycles = 4 },
            new Instruction { Name = "AND", Operate = AND, AddrMode = ABS, Cycles = 4 },
            new Instruction { Name = "ROL", Operate = ROL, AddrMode = ABS, Cycles = 6 },
            new Instruction { Name = "???", Operate = XXX, AddrMode = IMP, Cycles = 6 },

            new Instruction { Name = "BMI", Operate = BMI, AddrMode = REL, Cycles = 2 },
            new Instruction { Name = "AND", Operate = AND, AddrMode = IZY, Cycles = 5 },
            new Instruction { Name = "???", Operate = XXX, AddrMode = IMP, Cycles = 2 },
            new Instruction { Name = "???", Operate = XXX, AddrMode = IMP, Cycles = 8 },
            new Instruction { Name = "???", Operate = NOP, AddrMode = IMP, Cycles = 4 },
            new Instruction { Name = "AND", Operate = AND, AddrMode = ZPX, Cycles = 4 },
            new Instruction { Name = "ROL", Operate = ROL, AddrMode = ZPX, Cycles = 6 },
            new Instruction { Name = "???", Operate = XXX, AddrMode = IMP, Cycles = 6 },
            new Instruction { Name = "SEC", Operate = SEC, AddrMode = IMP, Cycles = 2 },
            new Instruction { Name = "AND", Operate = AND, AddrMode = ABY, Cycles = 4 },
            new Instruction { Name = "???", Operate = NOP, AddrMode = IMP, Cycles = 2 },
            new Instruction { Name = "???", Operate = XXX, AddrMode = IMP, Cycles = 7 },
            new Instruction { Name = "???", Operate = NOP, AddrMode = IMP, Cycles = 4 },
            new Instruction { Name = "AND", Operate = AND, AddrMode = ABX, Cycles = 4 },
            new Instruction { Name = "ROL", Operate = ROL, AddrMode = ABX, Cycles = 7 },
            new Instruction { Name = "???", Operate = XXX, AddrMode = IMP, Cycles = 7 },

            new Instruction { Name = "RTI", Operate = RTI, AddrMode = IMP, Cycles = 6 },
            new Instruction { Name = "EOR", Operate = EOR, AddrMode = IZX, Cycles = 6 },
            new Instruction { Name = "???", Operate = XXX, AddrMode = IMP, Cycles = 2 },
            new Instruction { Name = "???", Operate = XXX, AddrMode = IMP, Cycles = 8 },
            new Instruction { Name = "???", Operate = NOP, AddrMode = IMP, Cycles = 3 },
            new Instruction { Name = "EOR", Operate = EOR, AddrMode = ZP0, Cycles = 3 },
            new Instruction { Name = "LSR", Operate = LSR, AddrMode = ZP0, Cycles = 5 },
            new Instruction { Name = "???", Operate = XXX, AddrMode = IMP, Cycles = 5 },
            new Instruction { Name = "PHA", Operate = PHA, AddrMode = IMP, Cycles = 3 },
            new Instruction { Name = "EOR", Operate = EOR, AddrMode = IMM, Cycles = 2 },
            new Instruction { Name = "LSR", Operate = LSR, AddrMode = IMP, Cycles = 2 },
            new Instruction { Name = "???", Operate = XXX, AddrMode = IMP, Cycles = 2 },
            new Instruction { Name = "JMP", Operate = JMP, AddrMode = ABS, Cycles = 3 },
            new Instruction { Name = "EOR", Operate = EOR, AddrMode = ABS, Cycles = 4 },
            new Instruction { Name = "LSR", Operate = LSR, AddrMode = ABS, Cycles = 6 },
            new Instruction { Name = "???", Operate = XXX, AddrMode = IMP, Cycles = 6 },

            new Instruction { Name = "BVC", Operate = BVC, AddrMode = REL, Cycles = 2 },
            new Instruction { Name = "EOR", Operate = EOR, AddrMode = IZY, Cycles = 5 },
            new Instruction { Name = "???", Operate = XXX, AddrMode = IMP, Cycles = 2 },
            new Instruction { Name = "???", Operate = XXX, AddrMode = IMP, Cycles = 8 },
            new Instruction { Name = "???", Operate = NOP, AddrMode = IMP, Cycles = 4 },
            new Instruction { Name = "EOR", Operate = EOR, AddrMode = ZPX, Cycles = 4 },
            new Instruction { Name = "LSR", Operate = LSR, AddrMode = ZPX, Cycles = 6 },
            new Instruction { Name = "???", Operate = XXX, AddrMode = IMP, Cycles = 6 },
            new Instruction { Name = "CLI", Operate = CLI, AddrMode = IMP, Cycles = 2 },
            new Instruction { Name = "EOR", Operate = EOR, AddrMode = ABY, Cycles = 4 },
            new Instruction { Name = "???", Operate = NOP, AddrMode = IMP, Cycles = 2 },
            new Instruction { Name = "???", Operate = XXX, AddrMode = IMP, Cycles = 7 },
            new Instruction { Name = "???", Operate = NOP, AddrMode = IMP, Cycles = 4 },
            new Instruction { Name = "EOR", Operate = EOR, AddrMode = ABX, Cycles = 4 },
            new Instruction { Name = "LSR", Operate = LSR, AddrMode = ABX, Cycles = 7 },
            new Instruction { Name = "???", Operate = XXX, AddrMode = IMP, Cycles = 7 },

            new Instruction { Name = "RTS", Operate = RTS, AddrMode = IMP, Cycles = 6 },
            new Instruction { Name = "ADC", Operate = ADC, AddrMode = IZX, Cycles = 6 },
            new Instruction { Name = "???", Operate = XXX, AddrMode = IMP, Cycles = 2 },
            new Instruction { Name = "???", Operate = XXX, AddrMode = IMP, Cycles = 8 },
            new Instruction { Name = "???", Operate = NOP, AddrMode = IMP, Cycles = 3 },
            new Instruction { Name = "ADC", Operate = ADC, AddrMode = ZP0, Cycles = 3 },
            new Instruction { Name = "ROR", Operate = ROR, AddrMode = ZP0, Cycles = 5 },
            new Instruction { Name = "???", Operate = XXX, AddrMode = IMP, Cycles = 5 },
            new Instruction { Name = "PLA", Operate = PLA, AddrMode = IMP, Cycles = 4 },
            new Instruction { Name = "ADC", Operate = ADC, AddrMode = IMM, Cycles = 2 },
            new Instruction { Name = "ROR", Operate = ROR, AddrMode = IMP, Cycles = 2 },
            new Instruction { Name = "???", Operate = XXX, AddrMode = IMP, Cycles = 2 },
            new Instruction { Name = "JMP", Operate = JMP, AddrMode = IND, Cycles = 5 },
            new Instruction { Name = "ADC", Operate = ADC, AddrMode = ABS, Cycles = 4 },
            new Instruction { Name = "ROR", Operate = ROR, AddrMode = ABS, Cycles = 6 },
            new Instruction { Name = "???", Operate = XXX, AddrMode = IMP, Cycles = 6 },

            new Instruction { Name = "BVS", Operate = BVS, AddrMode = REL, Cycles = 2 },
            new Instruction { Name = "ADC", Operate = ADC, AddrMode = IZY, Cycles = 5 },
            new Instruction { Name = "???", Operate = XXX, AddrMode = IMP, Cycles = 2 },
            new Instruction { Name = "???", Operate = XXX, AddrMode = IMP, Cycles = 8 },
            new Instruction { Name = "???", Operate = NOP, AddrMode = IMP, Cycles = 4 },
            new Instruction { Name = "ADC", Operate = ADC, AddrMode = ZPX, Cycles = 4 },
            new Instruction { Name = "ROR", Operate = ROR, AddrMode = ZPX, Cycles = 6 },
            new Instruction { Name = "???", Operate = XXX, AddrMode = IMP, Cycles = 6 },
            new Instruction { Name = "SEI", Operate = SEI, AddrMode = IMP, Cycles = 2 },
            new Instruction { Name = "ADC", Operate = ADC, AddrMode = ABY, Cycles = 4 },
            new Instruction { Name = "???", Operate = NOP, AddrMode = IMP, Cycles = 2 },
            new Instruction { Name = "???", Operate = XXX, AddrMode = IMP, Cycles = 7 },
            new Instruction { Name = "???", Operate = NOP, AddrMode = IMP, Cycles = 4 },
            new Instruction { Name = "ADC", Operate = ADC, AddrMode = ABX, Cycles = 4 },
            new Instruction { Name = "ROR", Operate = ROR, AddrMode = ABX, Cycles = 7 },
            new Instruction { Name = "???", Operate = XXX, AddrMode = IMP, Cycles = 7 },

            new Instruction { Name = "???", Operate = NOP, AddrMode = IMP, Cycles = 2 },
            new Instruction { Name = "STA", Operate = STA, AddrMode = IZX, Cycles = 6 },
            new Instruction { Name = "???", Operate = NOP, AddrMode = IMP, Cycles = 2 },
            new Instruction { Name = "???", Operate = XXX, AddrMode = IMP, Cycles = 6 },
            new Instruction { Name = "STY", Operate = STY, AddrMode = ZP0, Cycles = 3 },
            new Instruction { Name = "STA", Operate = STA, AddrMode = ZP0, Cycles = 3 },
            new Instruction { Name = "STX", Operate = STX, AddrMode = ZP0, Cycles = 3 },
            new Instruction { Name = "???", Operate = XXX, AddrMode = IMP, Cycles = 3 },
            new Instruction { Name = "DEY", Operate = DEY, AddrMode = IMP, Cycles = 2 },
            new Instruction { Name = "???", Operate = NOP, AddrMode = IMP, Cycles = 2 },
            new Instruction { Name = "TXA", Operate = TXA, AddrMode = IMP, Cycles = 2 },
            new Instruction { Name = "???", Operate = XXX, AddrMode = IMP, Cycles = 2 },
            new Instruction { Name = "STY", Operate = STY, AddrMode = ABS, Cycles = 4 },
            new Instruction { Name = "STA", Operate = STA, AddrMode = ABS, Cycles = 4 },
            new Instruction { Name = "STX", Operate = STX, AddrMode = ABS, Cycles = 4 },
            new Instruction { Name = "???", Operate = XXX, AddrMode = IMP, Cycles = 4 },

            new Instruction { Name = "BCC", Operate = BCC, AddrMode = REL, Cycles = 2 },
            new Instruction { Name = "STA", Operate = STA, AddrMode = IZY, Cycles = 6 },
            new Instruction { Name = "???", Operate = XXX, AddrMode = IMP, Cycles = 2 },
            new Instruction { Name = "???", Operate = XXX, AddrMode = IMP, Cycles = 6 },
            new Instruction { Name = "STY", Operate = STY, AddrMode = ZPX, Cycles = 4 },
            new Instruction { Name = "STA", Operate = STA, AddrMode = ZPX, Cycles = 4 },
            new Instruction { Name = "STX", Operate = STX, AddrMode = ZPY, Cycles = 4 },
            new Instruction { Name = "???", Operate = XXX, AddrMode = IMP, Cycles = 4 },
            new Instruction { Name = "TYA", Operate = TYA, AddrMode = IMP, Cycles = 2 },
            new Instruction { Name = "STA", Operate = STA, AddrMode = ABY, Cycles = 5 },
            new Instruction { Name = "TXS", Operate = TXS, AddrMode = IMP, Cycles = 2 },
            new Instruction { Name = "???", Operate = XXX, AddrMode = IMP, Cycles = 5 },
            new Instruction { Name = "???", Operate = NOP, AddrMode = IMP, Cycles = 5 },
            new Instruction { Name = "STA", Operate = STA, AddrMode = ABX, Cycles = 5 },
            new Instruction { Name = "???", Operate = XXX, AddrMode = IMP, Cycles = 5 },
            new Instruction { Name = "???", Operate = XXX, AddrMode = IMP, Cycles = 5 },

            new Instruction { Name = "LDY", Operate = LDY, AddrMode = IMM, Cycles = 2 },
            new Instruction { Name = "LDA", Operate = LDA, AddrMode = IZX, Cycles = 6 },
            new Instruction { Name = "LDX", Operate = LDX, AddrMode = IMM, Cycles = 2 },
            new Instruction { Name = "???", Operate = XXX, AddrMode = IMP, Cycles = 6 },
            new Instruction { Name = "LDY", Operate = LDY, AddrMode = ZP0, Cycles = 3 },
            new Instruction { Name = "LDA", Operate = LDA, AddrMode = ZP0, Cycles = 3 },
            new Instruction { Name = "LDX", Operate = LDX, AddrMode = ZP0, Cycles = 3 },
            new Instruction { Name = "???", Operate = XXX, AddrMode = IMP, Cycles = 3 },
            new Instruction { Name = "TAY", Operate = TAY, AddrMode = IMP, Cycles = 2 },
            new Instruction { Name = "LDA", Operate = LDA, AddrMode = IMM, Cycles = 2 },
            new Instruction { Name = "TAX", Operate = TAX, AddrMode = IMP, Cycles = 2 },
            new Instruction { Name = "???", Operate = XXX, AddrMode = IMP, Cycles = 2 },
            new Instruction { Name = "LDY", Operate = LDY, AddrMode = ABS, Cycles = 4 },
            new Instruction { Name = "LDA", Operate = LDA, AddrMode = ABS, Cycles = 4 },
            new Instruction { Name = "LDX", Operate = LDX, AddrMode = ABS, Cycles = 4 },
            new Instruction { Name = "???", Operate = XXX, AddrMode = IMP, Cycles = 4 },

            new Instruction { Name = "BCS", Operate = BCS, AddrMode = REL, Cycles = 2 },
            new Instruction { Name = "LDA", Operate = LDA, AddrMode = IZY, Cycles = 5 },
            new Instruction { Name = "???", Operate = XXX, AddrMode = IMP, Cycles = 2 },
            new Instruction { Name = "???", Operate = XXX, AddrMode = IMP, Cycles = 5 },
            new Instruction { Name = "LDY", Operate = LDY, AddrMode = ZPX, Cycles = 4 },
            new Instruction { Name = "LDA", Operate = LDA, AddrMode = ZPX, Cycles = 4 },
            new Instruction { Name = "LDX", Operate = LDX, AddrMode = ZPY, Cycles = 4 },
            new Instruction { Name = "???", Operate = XXX, AddrMode = IMP, Cycles = 4 },
            new Instruction { Name = "CLV", Operate = CLV, AddrMode = IMP, Cycles = 2 },
            new Instruction { Name = "LDA", Operate = LDA, AddrMode = ABY, Cycles = 4 },
            new Instruction { Name = "TSX", Operate = TSX, AddrMode = IMP, Cycles = 2 },
            new Instruction { Name = "???", Operate = XXX, AddrMode = IMP, Cycles = 4 },
            new Instruction { Name = "LDY", Operate = LDY, AddrMode = ABX, Cycles = 4 },
            new Instruction { Name = "LDA", Operate = LDA, AddrMode = ABX, Cycles = 4 },
            new Instruction { Name = "LDX", Operate = LDX, AddrMode = ABY, Cycles = 4 },
            new Instruction { Name = "???", Operate = XXX, AddrMode = IMP, Cycles = 4 },

            new Instruction { Name = "CPY", Operate = CPY, AddrMode = IMM, Cycles = 2 },
            new Instruction { Name = "CMP", Operate = CMP, AddrMode = IZX, Cycles = 6 },
            new Instruction { Name = "???", Operate = NOP, AddrMode = IMP, Cycles = 2 },
            new Instruction { Name = "???", Operate = XXX, AddrMode = IMP, Cycles = 8 },
            new Instruction { Name = "CPY", Operate = CPY, AddrMode = ZP0, Cycles = 3 },
            new Instruction { Name = "CMP", Operate = CMP, AddrMode = ZP0, Cycles = 3 },
            new Instruction { Name = "DEC", Operate = DEC, AddrMode = ZP0, Cycles = 5 },
            new Instruction { Name = "???", Operate = XXX, AddrMode = IMP, Cycles = 5 },
            new Instruction { Name = "INY", Operate = INY, AddrMode = IMP, Cycles = 2 },
            new Instruction { Name = "CMP", Operate = CMP, AddrMode = IMM, Cycles = 2 },
            new Instruction { Name = "DEX", Operate = DEX, AddrMode = IMP, Cycles = 2 },
            new Instruction { Name = "???", Operate = XXX, AddrMode = IMP, Cycles = 2 },
            new Instruction { Name = "CPY", Operate = CPY, AddrMode = ABS, Cycles = 4 },
            new Instruction { Name = "CMP", Operate = CMP, AddrMode = ABS, Cycles = 4 },
            new Instruction { Name = "DEC", Operate = DEC, AddrMode = ABS, Cycles = 6 },
            new Instruction { Name = "???", Operate = XXX, AddrMode = IMP, Cycles = 6 },

            new Instruction { Name = "BNE", Operate = BNE, AddrMode = REL, Cycles = 2 },
            new Instruction { Name = "CMP", Operate = CMP, AddrMode = IZY, Cycles = 5 },
            new Instruction { Name = "???", Operate = XXX, AddrMode = IMP, Cycles = 2 },
            new Instruction { Name = "???", Operate = XXX, AddrMode = IMP, Cycles = 8 },
            new Instruction { Name = "???", Operate = NOP, AddrMode = IMP, Cycles = 4 },
            new Instruction { Name = "CMP", Operate = CMP, AddrMode = ZPX, Cycles = 4 },
            new Instruction { Name = "DEC", Operate = DEC, AddrMode = ZPX, Cycles = 6 },
            new Instruction { Name = "???", Operate = XXX, AddrMode = IMP, Cycles = 6 },
            new Instruction { Name = "CLD", Operate = CLD, AddrMode = IMP, Cycles = 2 },
            new Instruction { Name = "CMP", Operate = CMP, AddrMode = ABY, Cycles = 4 },
            new Instruction { Name = "NOP", Operate = NOP, AddrMode = IMP, Cycles = 2 },
            new Instruction { Name = "???", Operate = XXX, AddrMode = IMP, Cycles = 7 },
            new Instruction { Name = "???", Operate = NOP, AddrMode = IMP, Cycles = 4 },
            new Instruction { Name = "CMP", Operate = CMP, AddrMode = ABX, Cycles = 4 },
            new Instruction { Name = "DEC", Operate = DEC, AddrMode = ABX, Cycles = 7 },
            new Instruction { Name = "???", Operate = XXX, AddrMode = IMP, Cycles = 7 },

            new Instruction { Name = "CPX", Operate = CPX, AddrMode = IMM, Cycles = 2 },
            new Instruction { Name = "SBC", Operate = SBC, AddrMode = IZX, Cycles = 6 },
            new Instruction { Name = "???", Operate = NOP, AddrMode = IMP, Cycles = 2 },
            new Instruction { Name = "???", Operate = XXX, AddrMode = IMP, Cycles = 8 },
            new Instruction { Name = "CPX", Operate = CPX, AddrMode = ZP0, Cycles = 3 },
            new Instruction { Name = "SBC", Operate = SBC, AddrMode = ZP0, Cycles = 3 },
            new Instruction { Name = "INC", Operate = INC, AddrMode = ZP0, Cycles = 5 },
            new Instruction { Name = "???", Operate = XXX, AddrMode = IMP, Cycles = 5 },
            new Instruction { Name = "INX", Operate = INX, AddrMode = IMP, Cycles = 2 },
            new Instruction { Name = "SBC", Operate = SBC, AddrMode = IMM, Cycles = 2 },
            new Instruction { Name = "NOP", Operate = NOP, AddrMode = IMP, Cycles = 2 },
            new Instruction { Name = "???", Operate = SBC, AddrMode = IMP, Cycles = 2 },
            new Instruction { Name = "CPX", Operate = CPX, AddrMode = ABS, Cycles = 4 },
            new Instruction { Name = "SBC", Operate = SBC, AddrMode = ABS, Cycles = 4 },
            new Instruction { Name = "INC", Operate = INC, AddrMode = ABS, Cycles = 6 },
            new Instruction { Name = "???", Operate = XXX, AddrMode = IMP, Cycles = 6 },

            new Instruction { Name = "BEQ", Operate = BEQ, AddrMode = REL, Cycles = 2 },
            new Instruction { Name = "SBC", Operate = SBC, AddrMode = IZY, Cycles = 5 },
            new Instruction { Name = "???", Operate = XXX, AddrMode = IMP, Cycles = 2 },
            new Instruction { Name = "???", Operate = XXX, AddrMode = IMP, Cycles = 8 },
            new Instruction { Name = "???", Operate = NOP, AddrMode = IMP, Cycles = 4 },
            new Instruction { Name = "SBC", Operate = SBC, AddrMode = ZPX, Cycles = 4 },
            new Instruction { Name = "INC", Operate = INC, AddrMode = ZPX, Cycles = 6 },
            new Instruction { Name = "???", Operate = XXX, AddrMode = IMP, Cycles = 6 },
            new Instruction { Name = "SED", Operate = SED, AddrMode = IMP, Cycles = 2 },
            new Instruction { Name = "SBC", Operate = SBC, AddrMode = ABY, Cycles = 4 },
            new Instruction { Name = "NOP", Operate = NOP, AddrMode = IMP, Cycles = 2 },
            new Instruction { Name = "???", Operate = XXX, AddrMode = IMP, Cycles = 7 },
            new Instruction { Name = "???", Operate = NOP, AddrMode = IMP, Cycles = 4 },
            new Instruction { Name = "SBC", Operate = SBC, AddrMode = ABX, Cycles = 4 },
            new Instruction { Name = "INC", Operate = INC, AddrMode = ABX, Cycles = 7 },
            new Instruction { Name = "???", Operate = XXX, AddrMode = IMP, Cycles = 7 },
        ];
    }

    // Connect to the bus
    public void ConnectBus(Bus n)
    {
        _bus = n;
    }

    // Clock function stub
    public void Clock()
    {
        // Each instruction requires a variable number of clock cycles to execute.
        // In my emulation, I only care about the final result and so I perform
        // the entire computation in one hit. In hardware, each clock cycle would
        // perform "microcode" style transformations of the CPUs state.
        //
        // To remain compliant with connected devices, it's important that the 
        // emulation also takes "time" in order to execute instructions, so I
        // implement that delay by simply counting down the cycles required by 
        // the instruction. When it reaches 0, the instruction is complete, and
        // the next one is ready to be executed
        if (_cycles == 0)
        {
            Opcode = Read(Pc);

            // Always set the unused status flag bit to 1
            SetFlag(Flags6502.U, true);

            // Increment program counter, we read the opcode byte
            Pc++;

            // Get starting number of cycles
            _cycles = lookup[Opcode].Cycles;

            var additionalCycle1 = lookup[Opcode].AddrMode();
            var additionalCycle2 = lookup[Opcode].Operate();

            _cycles += (byte)(additionalCycle1 & additionalCycle2);

            // Always set unused status flag bit to 1
            SetFlag(Flags6502.U, true);

        }

        _cycles--;
    }

    // Reset function stub
    public void Reset()
    {
        // Implementation stub
        A = 0;
        X = 0;
        Y = 0;
        Sp = 0xFD;
        Status = 0x00 | (byte)Flags6502.U;

        AddressAbs = 0xFFFC;

        //ushort lo = Read((ushort)(AddressAbs + 0));
        //ushort hi = Read((ushort)(AddressAbs + 1));

        //Pc = (ushort)((hi << 8) | lo);

        Pc = ReadWord(AddressAbs);

        AddressRel = 0x0000;
        AddressAbs = 0x0000;
        _fetched = 0x00;

        _cycles = 8;
    }

    // IRQ function stub
    public void Irq()
    {
        if (GetFlag(Flags6502.I) != 0) return;

        Push((byte)((Pc >> 8) & 0x00ff));
        Push((byte)(Pc & 0x00ff));

        //Write((ushort)(0x0100 + Sp), (byte)((Pc >> 8) & 0x00ff));
        //Sp++;
        //Write((ushort)(0x0100 + Sp), (byte)(Pc & 0x00ff));
        //Sp++;

        // Push status register to the stack
        SetFlag(Flags6502.B, false);
        SetFlag(Flags6502.U, true);
        SetFlag(Flags6502.I, true);

        Push(Status);
        //Write((ushort)(0x0100 + Sp), Status);
        //Sp++;

        // Read new program counter location from fixed address
        AddressAbs = 0xfffe;
        //ushort lo = Read((ushort)(AddressAbs + 0));
        //ushort hi = Read((ushort)(AddressAbs + 1));

        //Pc = (ushort)((hi << 8) | lo);
        Pc = ReadWord(AddressAbs);

        _cycles = 7;
    }

    // NMI function stub
    public void Nmi()
    {
        //Write((ushort)(0x0100 + Sp), (byte)((Pc >> 8) & 0x00ff));
        //Sp++;
        //Write((ushort)(0x0100 + Sp), (byte)(Pc & 0x00ff));
        //Sp++;

        Push((byte)((Pc >> 8) & 0x00ff));
        Push((byte)(Pc & 0x00ff));

        // Push status register to the stack
        SetFlag(Flags6502.B, false);
        SetFlag(Flags6502.U, true);
        SetFlag(Flags6502.I, true);

        Push(Status);
        //Write((ushort)(0x0100 + Sp), Status);
        //Sp++;

        // Read new program counter location from fixed address
        AddressAbs = 0xfffa;
        //ushort lo = Read((ushort)(AddressAbs + 0));
        //ushort hi = Read((ushort)(AddressAbs + 1));

        //Pc = (ushort)((hi << 8) | lo);
        Pc = ReadWord(AddressAbs);

        _cycles = 8;
    }


    // Check if the CPU is complete
    public bool Complete()
    {
        return _cycles == 0;
    }

    // This function sources the data used by the instruction into 
    // a convenient numeric variable. Some instructions dont have to 
    // fetch data as the source is implied by the instruction. For example
    // "INX" increments the X register. There is no additional data
    // required. For all other addressing modes, the data resides at 
    // the location held within addr_abs, so it is read from there. 
    // Immediate adress mode exploits this slightly, as that has
    // set addr_abs = pc + 1, so it fetches the data from the
    // next byte for example "LDA $FF" just loads the accumulator with
    // 256, i.e. no far reaching memory fetch is required. "fetched"
    // is a variable global to the CPU, and is set by calling this 
    // function. It also returns it for convenience.
    public byte Fetch()
    {
        if (lookup[Opcode].AddrMode != IMP)
        {
            _fetched = Read(AddressAbs);
        }

        return _fetched;
    }


    // Enum for flags
    [Flags]
    public enum Flags6502
    {
        C = 1 << 0, // Carry Bit
        Z = 1 << 1, // Zero
        I = 1 << 2, // Disable Interrupts
        D = 1 << 3, // Decimal Mode (unused)
        B = 1 << 4, // Break
        U = 1 << 5, // Unused
        V = 1 << 6, // Overflow
        N = 1 << 7  // Negative
    }

    // Read from bus
    private byte Read(ushort addr)
    {
        return _bus?.CpuRead(addr, false) ?? 0;
    }

    // Write to bus
    private void Write(ushort addr, byte data)
    {
        _bus?.CpuWrite(addr, data);
    }

    // Get specific flag
    private byte GetFlag(Flags6502 flag)
    {
        return (byte)((Status & (byte)flag) > 0 ? 1 : 0);
    }

    // Set specific flag
    private void SetFlag(Flags6502 flag, bool value)
    {
        if (value)
            Status |= (byte)flag;
        else
            Status &= (byte)~flag;
    }

    private void SetFlag(Flags6502 flag, int value)
    {
        if (value != 0)
            Status |= (byte)flag;
        else
            Status &= (byte)~flag;
    }

    // Address Mode: Implied
    // There is no additional data required for this instruction. The instruction
    // does something very simple like like sets a status bit. However, we will
    // target the accumulator, for instructions like PHA
    private byte IMP()
    {
        _fetched = A;
        return 0;
    }

    // Address Mode: Immediate
    // The instruction expects the next byte to be used as a value, so we'll prep
    // the read address to point to the next byte
    private byte IMM()
    {
        AddressAbs = Pc++;
        return 0;
    }

    // Address Mode: Zero Page
    // To save program bytes, zero page addressing allows you to absolutely address
    // a location in first 0xFF bytes of address range. Clearly this only requires
    // one byte instead of the usual two.
    private byte ZP0()
    {
        AddressAbs = Read(Pc);
        Pc++;
        AddressAbs &= 0x00ff;
        return 0;
    }

    // Address Mode: Zero Page with X Offset
    // Fundamentally the same as Zero Page addressing, but the contents of the X Register
    // is added to the supplied single byte address. This is useful for iterating through
    // ranges within the first page.
    private byte ZPX()
    {
        AddressAbs = Read((ushort)(Pc + X));
        Pc++;
        AddressAbs &= 0x00ff;
        return 0;
    }

    // Address Mode: Zero Page with Y Offset
    // Same as above but uses Y Register for offset
    private byte ZPY()
    {
        AddressAbs = Read((ushort)(Pc + Y));
        Pc++;
        AddressAbs &= 0x00ff;
        return 0;
    }

    // Address Mode: Relative
    // This address mode is exclusive to branch instructions. The address
    // must reside within -128 to +127 of the branch instruction, i.e.
    // you cant directly branch to any address in the addressable range.
    private byte REL()
    {
        AddressRel = Read(Pc);
        Pc++;
        if ((AddressRel & 0x80) != 0)
        {
            AddressRel |= 0xff00;
        }

        return 0;
    }

    // Address Mode: Absolute 
    // A full 16-bit address is loaded and used
    private byte ABS()
    {
        ushort lo = Read(Pc);
        Pc++;
        ushort hi = Read(Pc);
        Pc++;

        AddressAbs = (ushort)((hi << 8) | lo);

        return 0;
    }

    // Address Mode: Absolute with X Offset
    // Fundamentally the same as absolute addressing, but the contents of the X Register
    // is added to the supplied two byte address. If the resulting address changes
    // the page, an additional clock cycle is required
    private byte ABX()
    {
        ushort lo = Read(Pc);
        Pc++;
        ushort hi = Read(Pc);
        Pc++;

        AddressAbs = (ushort)((hi << 8) | lo);
        AddressAbs += X;

        // check for page boundary
        return (AddressAbs & 0xff00) != (hi << 8) ? (byte)1 : (byte)0;
    }

    // Address Mode: Absolute with Y Offset
    // Fundamentally the same as absolute addressing, but the contents of the Y Register
    // is added to the supplied two byte address. If the resulting address changes
    // the page, an additional clock cycle is required
    private byte ABY()
    {
        ushort lo = Read(Pc);
        Pc++;
        ushort hi = Read(Pc);
        Pc++;

        AddressAbs = (ushort)((hi << 8) | lo);
        AddressAbs += Y;

        // check for page boundary
        return (AddressAbs & 0xff00) != (hi << 8) ? (byte)1 : (byte)0;
    }

    // Note: The next 3 address modes use indirection (aka Pointers!)

    // Address Mode: Indirect
    // The supplied 16-bit address is read to get the actual 16-bit address. This is
    // instruction is unusual in that it has a bug in the hardware! To emulate its
    // function accurately, we also need to emulate this bug. If the low byte of the
    // supplied address is 0xFF, then to read the high byte of the actual address
    // we need to cross a page boundary. This doesnt actually work on the chip as 
    // designed, instead it wraps back around in the same page, yielding an 
    // invalid actual address
    private byte IND()
    {
        ushort ptrLo = Read(Pc);
        Pc++;
        ushort ptrHi = Read(Pc);
        Pc++;

        var ptr = (ushort)((ptrHi << 8) | ptrLo);

        if (ptrLo == 0x00ff) // Simulate page boundary bug
        {
            AddressAbs = (ushort)((Read((ushort)(ptr & 0xff00)) << 8) | Read(ptr));
        }
        else // Behave normally
        {
            AddressAbs = (ushort)((Read((ushort)(ptr + 1)) << 8) | Read(ptr));
        }

        return 0;
    }

    // Address Mode: Indirect X
    // The supplied 8-bit address is offset by X Register to index
    // a location in page 0x00. The actual 16-bit address is read 
    // from this location
    private byte IZX()
    {
        ushort t = Read(Pc);
        Pc++;

        ushort lo = Read((ushort)((ushort)(t + (ushort)X) & 0x00ff));
        ushort hi = Read((ushort)((ushort)(t + (ushort)X + 1) & 0x00ff));

        AddressAbs = (ushort)((hi << 8) | lo);
        return 0;
    }

    // Address Mode: Indirect Y
    // The supplied 8-bit address indexes a location in page 0x00. From 
    // here the actual 16-bit address is read, and the contents of
    // Y Register is added to it to offset it. If the offset causes a
    // change in page then an additional clock cycle is required.
    private byte IZY()
    {
        ushort t = Read(Pc);
        Pc++;

        ushort lo = Read((ushort)(t & 0x00ff));
        ushort hi = Read((ushort)((t + 1) & 0x00ff));

        AddressAbs = (ushort)((hi << 8) | lo);
        AddressAbs += Y;

        return (AddressAbs & 0xff00) != (hi << 8) ? (byte)1 : (byte)0;
    }

    // Instruction: Add with Carry In
    // Function:    A = A + M + C
    // Flags Out:   C, V, N, Z
    //
    // Explanation:
    // The purpose of this function is to add a value to the accumulator and a carry bit. If
    // the result is > 255 there is an overflow setting the carry bit. Ths allows you to
    // chain together ADC instructions to add numbers larger than 8-bits. This in itself is
    // simple, however the 6502 supports the concepts of Negativity/Positivity and Signed Overflow.
    //
    // 10000100 = 128 + 4 = 132 in normal circumstances, we know this as unsigned and it allows
    // us to represent numbers between 0 and 255 (given 8 bits). The 6502 can also interpret 
    // this word as something else if we assume those 8 bits represent the range -128 to +127,
    // i.e. it has become signed.
    //
    // Since 132 > 127, it effectively wraps around, through -128, to -124. This wraparound is
    // called overflow, and this is a useful to know as it indicates that the calculation has
    // gone outside the permissable range, and therefore no longer makes numeric sense.
    //
    // Note the implementation of ADD is the same in binary, this is just about how the numbers
    // are represented, so the word 10000100 can be both -124 and 132 depending upon the 
    // context the programming is using it in. We can prove this!
    //
    //  10000100 =  132  or  -124
    // +00010001 = + 17      + 17
    //  ========    ===       ===     See, both are valid additions, but our interpretation of
    //  10010101 =  149  or  -107     the context changes the value, not the hardware!
    //
    // In principle under the -128 to 127 range:
    // 10000000 = -128, 11111111 = -1, 00000000 = 0, 00000000 = +1, 01111111 = +127
    // therefore negative numbers have the most significant set, positive numbers do not
    //
    // To assist us, the 6502 can set the overflow flag, if the result of the addition has
    // wrapped around. V <- ~(A^M) & A^(A+M+C) :D lol, let's work out why!
    //
    // Let's suppose we have A = 30, M = 10 and C = 0
    //          A = 30 = 00011110
    //          M = 10 = 00001010+
    //     RESULT = 40 = 00101000
    //
    // Here we have not gone out of range. The resulting significant bit has not changed.
    // So let's make a truth table to understand when overflow has occurred. Here I take
    // the MSB of each component, where R is RESULT.
    //
    // A  M  R | V | A^R | A^M |~(A^M) | 
    // 0  0  0 | 0 |  0  |  0  |   1   |
    // 0  0  1 | 1 |  1  |  0  |   1   |
    // 0  1  0 | 0 |  0  |  1  |   0   |
    // 0  1  1 | 0 |  1  |  1  |   0   |  so V = ~(A^M) & (A^R)
    // 1  0  0 | 0 |  1  |  1  |   0   |
    // 1  0  1 | 0 |  0  |  1  |   0   |
    // 1  1  0 | 1 |  1  |  0  |   1   |
    // 1  1  1 | 0 |  0  |  0  |   1   |
    //
    // We can see how the above equation calculates V, based on A, M and R. V was chosen
    // based on the following hypothesis:
    //       Positive Number + Positive Number = Negative Result -> Overflow
    //       Negative Number + Negative Number = Positive Result -> Overflow
    //       Positive Number + Negative Number = Either Result -> Cannot Overflow
    //       Positive Number + Positive Number = Positive Result -> OK! No Overflow
    //       Negative Number + Negative Number = Negative Result -> OK! NO Overflow
    private byte ADC()
    {
        Fetch();

        var temp = (ushort)((ushort)A + (ushort)_fetched + (ushort)GetFlag(Flags6502.C));

        SetFlag(Flags6502.C, temp > 255);
        SetFlag(Flags6502.Z, (temp & 0x00ff) == 0x00);

        SetFlag(Flags6502.V, ((~((ushort)A ^ (ushort)_fetched) & ((ushort)A ^ (ushort)temp)) & 0x0080) != 0);
        //SetFlag(Flags6502.N, (temp & 0x80) != 0);
        SetFlag(Flags6502.N, IsNegative((byte)temp));

        A = (byte)(temp & 0x00ff);

        return 1;
    }

    // Instruction: Bitwise Logic AND
    // Function:    A = A & M
    // Flags Out:   N, Z
    private byte AND()
    {
        Fetch();
        A = (byte)(A & _fetched);
        SetFlag(Flags6502.Z, A == 0x00);
        //SetFlag(Flags6502.N, (A & 0x80) != 0);
        SetFlag(Flags6502.N, IsNegative(A));
        return 1;
    }

    // Instruction: Arithmetic Shift Left
    // Function:    A = C <- (A << 1) <- 0
    // Flags Out:   N, Z, C
    private byte ASL()
    {
        Fetch();

        var temp = (ushort)_fetched << 1;
        SetFlag(Flags6502.C, (temp & 0xff00) > 0);
        SetFlag(Flags6502.Z, (temp & 0x00ff) == 0x00);
        SetFlag(Flags6502.N, (temp & 0x80) != 0);

        if (lookup[Opcode].AddrMode == IMP)
        {
            A = (byte)(temp & 0x00ff);
        }
        else
        {
            Write(AddressAbs, (byte)(temp & 0x00ff));
        }

        return 0;
    }


    // Instruction: Branch if Carry Clear
    // Function:    if(C == 0) pc = address 
    private byte BCC()
    {
        //if (GetFlag(Flags6502.C) != 0) return 0;

        Branch(() => GetFlag(Flags6502.C) == 0);
        return 0;
    }

    // Instruction: Branch if Carry Set
    // Function:    if(C == 1) pc = address
    private byte BCS()
    {
        //if (GetFlag(Flags6502.C) != 1) return 0;

        Branch(() => GetFlag(Flags6502.C) == 1);
        return 0;
    }

    // Instruction: Branch if Equal
    // Function:    if(Z == 1) pc = address
    private byte BEQ()
    {
        //if (GetFlag(Flags6502.Z) != 1) return 0;

        Branch(() => GetFlag(Flags6502.Z) == 1);
        return 0;
    }


    private byte BIT()
    {
        Fetch();

        var tmp = A & _fetched;
        SetFlag(Flags6502.Z, (tmp & 0x00ff) == 0x00);
        SetFlag(Flags6502.N, (_fetched & (1 << 7)) != 0);
        SetFlag(Flags6502.V, (_fetched & (1 << 6)) != 0);
        return 0;
    }

    // Instruction: Branch if Negative
    // Function:    if(N == 1) pc = address
    private byte BMI()
    {
        Branch(() => GetFlag(Flags6502.N) == 1);
        return 0;
    }

    // Instruction: Branch if Not Equal
    // Function:    if(Z == 0) pc = address
    private byte BNE()
    {
        Branch(() => GetFlag(Flags6502.Z) == 0);
        return 0;
    }

    // Instruction: Branch if Positive
    // Function:    if(N == 0) pc = address
    private byte BPL()
    {
        Branch(() => GetFlag(Flags6502.N) == 0);
        return 0;
    }

    // Instruction: Break
    // Function:    Program Sourced Interrupt
    private byte BRK()
    {
        Pc++;

        SetFlag(Flags6502.I, true);


        Push((byte)((Pc >> 8) & 0x00ff));
        Push((byte)(Pc & 0x00ff));

        //Write((ushort)(0x0100 + Sp), (byte)((Pc >> 8) & 0x00ff));
        //Sp++;
        //Write((ushort)(0x0100 + Sp), (byte)(Pc & 0x00ff));
        //Sp++;

        SetFlag(Flags6502.B, true);

        Push(Status);
        //Write((ushort)(0x0100 + Sp), Status);
        //Sp++;
        SetFlag(Flags6502.B, false);

        //Pc = (ushort)(Read(0xfffe) | (Read(0xffff) << 8));
        Pc = ReadWord(0xfffe);

        return 0;
    }

    // Instruction: Branch if Overflow Clear
    // Function:    if(V == 0) pc = address
    private byte BVC()
    {
        Branch(() => GetFlag(Flags6502.V) == 0);
        return 0;
    }

    // Instruction: Branch if Overflow Set
    // Function:    if(V == 1) pc = address
    private byte BVS()
    {
        Branch(() => GetFlag(Flags6502.V) == 1);
        return 0;
    }

    // Instruction: Clear Carry Flag
    // Function:    C = 0
    private byte CLC()
    {
        SetFlag(Flags6502.C, false);
        return 0;
    }

    // Instruction: Clear Decimal Flag
    // Function:    D = 0
    private byte CLD()
    {
        SetFlag(Flags6502.D, false);
        return 0;
    }

    // Instruction: Clear Interrupt Flag / Disable Interrupts
    // Function:    I = 0
    private byte CLI()
    {
        SetFlag(Flags6502.I, false);
        return 0;
    }

    // Instruction: Clear Overflow Flag
    // Function:    V = 0
    private byte CLV()
    {
        SetFlag(Flags6502.V, false);
        return 0;
    }

    // Instruction: Compare Accumulator
    // Function:    C <- A >= M      Z <- (A - M) == 0
    // Flags Out:   N, C, Z
    private byte CMP()
    {
        Fetch();
        var temp = (ushort)((ushort)A - (ushort)_fetched);
        SetFlag(Flags6502.C, A >= _fetched);
        SetFlag(Flags6502.Z, (temp & 0x00ff) == 0x0000);
        //SetFlag(Flags6502.N, (temp & 0x0080) != 0);
        SetFlag(Flags6502.N, IsNegative((byte)temp));

        return 1;
    }

    // Instruction: Compare X Register
    // Function:    C <- X >= M      Z <- (X - M) == 0
    // Flags Out:   N, C, Z
    private byte CPX()
    {
        Fetch();
        var temp = (ushort)((ushort)X - (ushort)_fetched);

        SetFlag(Flags6502.C, X >= _fetched);
        SetFlag(Flags6502.Z, (temp & 0x00ff) == 0x0000);


        //SetFlag(Flags6502.N, (temp & 0x0080) != 0);
        SetFlag(Flags6502.N, IsNegative((byte)temp));

        return 0;
    }

    // Instruction: Compare Y Register
    // Function:    C <- Y >= M      Z <- (Y - M) == 0
    // Flags Out:   N, C, Z
    private byte CPY()
    {
        Fetch();
        var temp = (ushort)((ushort)Y - (ushort)_fetched);

        SetFlag(Flags6502.C, Y >= _fetched);
        SetFlag(Flags6502.Z, (temp & 0x00ff) == 0x0000);
        SetFlag(Flags6502.N, (temp & 0x0080) != 0);

        return 0;
    }

    // Instruction: Decrement Value at Memory Location
    // Function:    M = M - 1
    // Flags Out:   N, Z
    private byte DEC()
    {
        Fetch();
        var temp = (ushort)(_fetched - 1);

        Write(AddressAbs, (byte)(temp & 0x00ff));
        SetFlag(Flags6502.Z, (temp & 0x00ff) == 0x0000);
        SetFlag(Flags6502.N, (temp & 0x0080) != 0);

        return 0;
    }

    // Instruction: Decrement X Register
    // Function:    X = X - 1
    // Flags Out:   N, Z
    private byte DEX()
    {
        X--;
        SetFlag(Flags6502.Z, X == 0x00);
        //SetFlag(Flags6502.N, (X & 0x80) != 0);
        SetFlag(Flags6502.N, IsNegative(X));

        return 0;
    }

    // Instruction: Decrement Y Register
    // Function:    Y = Y - 1
    // Flags Out:   N, Z
    private byte DEY()
    {
        Y--;
        SetFlag(Flags6502.Z, Y == 0x00);
        //SetFlag(Flags6502.N, (Y & 0x80) != 0);
        SetFlag(Flags6502.N, IsNegative(Y));

        return 0;
    }

    // Instruction: Bitwise Logic XOR
    // Function:    A = A xor M
    // Flags Out:   N, Z
    private byte EOR()
    {
        Fetch();
        A = (byte)(A ^ _fetched);
        SetFlag(Flags6502.Z, A == 0x00);
        //SetFlag(Flags6502.N, (A & 0x80) != 0);
        SetFlag(Flags6502.N, IsNegative(A));
        return 1;
    }

    // Instruction: Increment Value at Memory Location
    // Function:    M = M + 1
    // Flags Out:   N, Z
    private byte INC()
    {
        Fetch();
        var temp = (ushort)(_fetched + 1);
        Write(AddressAbs, (byte)(temp & 0x00ff));

        SetFlag(Flags6502.Z, (temp & 0x00ff) == 0x0000);
        SetFlag(Flags6502.N, (temp & 0x0080) != 0);

        return 0;
    }

    // Instruction: Increment X Register
    // Function:    X = X + 1
    // Flags Out:   N, Z
    private byte INX()
    {
        X++;
        SetFlag(Flags6502.Z, X == 0x00);
        SetFlag(Flags6502.N, (X & 0x80) != 0);
        return 0;
    }

    // Instruction: Increment Y Register
    // Function:    X = X + 1
    // Flags Out:   N, Z
    private byte INY()
    {
        Y++;
        SetFlag(Flags6502.Z, Y == 0x00);
        SetFlag(Flags6502.N, (Y & 0x80) != 0);
        return 0;
    }

    // Instruction: Jump To Location
    // Function:    pc = address
    private byte JMP()
    {
        Pc = AddressAbs;
        return 0;
    }

    // Instruction: Jump To Sub-Routine
    // Function:    Push current pc to stack, pc = address
    private byte JSR()
    {
        Pc++;

        Push((byte)((Pc >> 8) & 0x00ff));
        //Write((ushort)(0x0100 + Sp), (byte)((Pc >> 8) & 0x00ff));
        //Sp++;
        //Write((ushort)(0x0100 + Sp), (byte)(Pc & 0x00ff));
        //Sp++;
        Push((byte)(Pc & 0x00ff));

        Pc = AddressAbs;

        return 0;
    }

    // Instruction: Load The Accumulator
    // Function:    A = M
    // Flags Out:   N, Z
    private byte LDA()
    {
        Fetch();
        A = _fetched;
        SetFlag(Flags6502.Z, A == 0x00);
        SetFlag(Flags6502.N, (A & 0x80) != 0);
        return 1;
    }

    // Instruction: Load The X Register
    // Function:    X = M
    // Flags Out:   N, Z
    private byte LDX()
    {
        Fetch();
        X = _fetched;
        SetFlag(Flags6502.Z, X == 0x00);
        SetFlag(Flags6502.N, (X & 0x80) != 0);
        return 1;

    }

    // Instruction: Load The Y Register
    // Function:    Y = M
    // Flags Out:   N, Z
    private byte LDY()
    {
        Fetch();
        Y = _fetched;
        SetFlag(Flags6502.Z, Y == 0x00);
        SetFlag(Flags6502.N, (Y & 0x80) != 0);
        return 1;
    }

    private byte LSR()
    {
        Fetch();
        SetFlag(Flags6502.C, (_fetched & 0x0001) != 0);
        var temp = (ushort)(_fetched >> 1);

        SetFlag(Flags6502.Z, (temp & 0x00ff) == 0x0000);
        SetFlag(Flags6502.N, (temp & 0x0080) != 0);
        if (lookup[Opcode].AddrMode == IMP)
        {
            A = (byte)(temp & 0x00ff);
        }
        else
        {
            Write(AddressAbs, (byte)(temp & 0x00ff));
        }

        return 0;
    }

    private byte NOP()
    {
        return Opcode switch
        {
            0x1c or 0x3c or 0x5c or 0x7c or 0xdc or 0xfc => 1,
            _ => 0,
        };
    }

    // Instruction: Bitwise Logic OR
    // Function:    A = A | M
    // Flags Out:   N, Z
    private byte ORA()
    {
        Fetch();
        A = (byte)(A | _fetched);
        SetFlag(Flags6502.Z, A == 0x00);
        SetFlag(Flags6502.N, (A & 0x80) != 0);
        return 1;
    }

    // Instruction: Push Accumulator to Stack
    // Function:    A -> stack
    private byte PHA()
    {
        Push(A);
        return 0;
    }

    // Instruction: Push Status Register to Stack
    // Function:    status -> stack
    // Note:        Break flag is set to 1 before push
    private byte PHP()
    {
        Push((byte)(Status | (byte)Flags6502.B | (byte)Flags6502.U));
        SetFlag(Flags6502.B, false);
        SetFlag(Flags6502.U, false);

        return 0;
    }

    // Instruction: Pop Accumulator off Stack
    // Function:    A <- stack
    // Flags Out:   N, Z
    private byte PLA()
    {
        A = Pop();
        SetFlag(Flags6502.Z, A == 0x00);
        SetFlag(Flags6502.N, IsNegative(A));
        return 0;
    }

    // Instruction: Pop Status Register off Stack
    // Function:    Status <- stack
    private byte PLP()
    {
        Status = Pop();
        SetFlag(Flags6502.U, true);
        return 0;
    }

    private byte ROL()
    {
        Fetch();
        var temp = (ushort)((ushort)(_fetched << 1) | GetFlag(Flags6502.C));
        SetFlag(Flags6502.C, temp & 0xff00);
        SetFlag(Flags6502.Z, (temp & 0x00ff) == 0x0000);
        SetFlag(Flags6502.N, temp & 0x0080);
        if (lookup[Opcode].AddrMode == IMP)
        {
            A = (byte)(temp & 0x00ff);
        }
        else
        {
            Write(AddressAbs, (byte)(temp & 0x00ff));
        }

        return 0;
    }

    private byte ROR()
    {
        Fetch();
        var temp = (ushort)((GetFlag(Flags6502.C) << 7) | ( _fetched >> 1));
        SetFlag(Flags6502.C, temp & 0xff00);
        SetFlag(Flags6502.Z, (temp & 0x00ff) == 0x0000);
        SetFlag(Flags6502.N, temp & 0x0080);
        if (lookup[Opcode].AddrMode == IMP)
        {
            A = (byte)(temp & 0x00ff);
        }
        else
        {
            Write(AddressAbs, (byte)(temp & 0x00ff));
        }

        return 0;
    }

    private byte RTI()
    {
        Status = Pop();
        SetFlag(Flags6502.B, false);
        SetFlag(Flags6502.U, false);

        Pc = (ushort)Pop();
        Pc |= (ushort)Pop();

        return 0;
    }

    private byte RTS()
    {
        Pc = (ushort)Pop();
        Pc |= (ushort)Pop();

        Pc++;
        return 0;
    }

    private byte SBC()
    {
        Fetch();

        // Operating in 16-bit domain to capture carry out

        // We can invert the bottom 8 bits with bitwise xor
        ushort value = (ushort)(((ushort)_fetched) ^ 0x00FF);

        // Notice this is exactly the same as addition from here!
        ushort temp = (ushort)((ushort)A + value + (ushort)GetFlag(Flags6502.C));

        SetFlag(Flags6502.C, temp & 0xFF00);
        SetFlag(Flags6502.Z, ((temp & 0x00FF) == 0));
        SetFlag(Flags6502.V, (temp ^ (ushort)A) & (temp ^ value) & 0x0080);
        SetFlag(Flags6502.N, temp & 0x0080);
        A = (byte)(temp & 0x00FF);
        return 1;
    }

    // Instruction: Set Carry Flag
    // Function:    C = 1
    private byte SEC()
    {
        SetFlag(Flags6502.C, true);
        return 0;
    }

    // Instruction: Set Decimal Flag
    // Function:    D = 1
    private byte SED()
    {
        SetFlag(Flags6502.D, true);
        return 0;
    }

    // Instruction: Set Interrupt Flag / Enable Interrupts
    // Function:    I = 1
    private byte SEI()
    {
        SetFlag(Flags6502.I, true);
        return 0;
    }

    // Instruction: Store Accumulator at Address
    // Function:    M = A
    private byte STA()
    {
        Write(AddressAbs, A);
        return 0;
    }

    // Instruction: Store X at Address
    // Function:    M = X
    private byte STX()
    {
        Write(AddressAbs, X);
        return 0;
    }

    // Instruction: Store Y at Address
    // Function:    M = Y
    private byte STY()
    {
        Write(AddressAbs, Y);
        return 0;
    }

    // Instruction: Transfer Accumulator to X Register
    // Function:    X = A
    // Flags Out:   N, Z
    private byte TAX()
    {
        X = A;
        SetFlag(Flags6502.Z, X == 0x00);
        SetFlag(Flags6502.N, X & 0x80);
        return 0;
    }

    // Instruction: Transfer Accumulator to Y Register
    // Function:    Y = A
    // Flags Out:   N, Z
    private byte TAY()
    {
        Y = A;
        SetFlag(Flags6502.Z, Y == 0x00);
        SetFlag(Flags6502.N, Y & 0x80);
        return 0;
    }

    // Instruction: Transfer Stack Pointer to X Register
    // Function:    X = stack pointer
    // Flags Out:   N, Z
    private byte TSX()
    {
        X = Sp;
        SetFlag(Flags6502.Z, X == 0x00);
        SetFlag(Flags6502.N, X & 0x80);
        return 0;
    }

    // Instruction: Transfer X Register to Accumulator 
    // Function:    A = X
    // Flags Out:   N, Z
    private byte TXA()
    {
        A = X;
        SetFlag(Flags6502.Z, A == 0x00);
        SetFlag(Flags6502.N, A & 0x80);
        return 0;
    }

    // Instruction: Transfer X Register to Stack Pointer
    // Function:    stack pointer = X
    private byte TXS()
    {
        Sp = A;
        return 0;
    }

    // Instruction: Transfer Y Register to Accumulator
    // Function:    A = Y
    // Flags Out:   N, Z
    private byte TYA()
    {
        A = Y;
        SetFlag(Flags6502.Z, A == 0x00);
        SetFlag(Flags6502.N, A & 0x80);
        return 0;
    }

    private byte XXX() => 0; // Capture all "unofficial" opcodes

    // helper functions
    private byte Pop()
    {
        Sp++;
        return Read((byte)(STACK_OFFSET + Sp));
    }
    private void Push(byte value)
    {
        Write((ushort)(STACK_OFFSET + Sp), value);
        Sp--;
    }
    private bool IsNegative(byte value)
    {
        return (value & 0x80) != 0;
    }

    // helper function for branches
    private void Branch(Func<bool> predicate)
    {
        if (!predicate()) return;

        _cycles++;
        AddressAbs = (ushort)(Pc + AddressRel);

        if ((AddressAbs & 0xff00) != (Pc & 0xff00))
            _cycles++;

        Pc = AddressAbs;
    }

    private ushort ReadWord(ushort address)
    {
        var lo = Read((ushort)(address + 0));
        var hi = (ushort)(Read((ushort)(address + 1)) << 8);

        return (ushort)(hi | lo);

    }

    // Disassemble function stub
    public Dictionary<ushort, string> Disassemble(ushort nStart, ushort nStop)
    {
        uint addr = nStart;
        byte value = 0x00, lo = 0x00, hi = 0x00;
        var mapLines = new Dictionary<ushort, string>();

        // Utility to convert variables into hex strings
        string Hex(uint n, int d)
        {
            return n.ToString($"X{d}");
        }

        // Iterate through memory from start to stop
        while (addr <= nStop)
        {
            ushort lineAddr = (ushort)addr;
            string sInst = $"${Hex(addr, 4)}: ";

            // Read the opcode and increment address
            byte opcode = _bus?.CpuRead((ushort)addr++, true) ?? 0x00;
            sInst += lookup[opcode].Name + " ";

            // Decode addressing mode and add operands
            if (lookup[opcode].AddrMode == IMP)
            {
                sInst += " {IMP}";
            }
            else if (lookup[opcode].AddrMode == IMM)
            {
                value = _bus?.CpuRead((ushort)addr++, true) ?? 0x00;
                sInst += $"#$${Hex(value, 2)} {{IMM}}";
            }
            else if (lookup[opcode].AddrMode == ZP0)
            {
                lo = _bus?.CpuRead((ushort)addr++, true) ?? 0x00;
                sInst += $"${Hex(lo, 2)} {{ZP0}}";
            }
            else if (lookup[opcode].AddrMode == ZPX)
            {
                lo = _bus?.CpuRead((ushort)addr++, true) ?? 0x00;
                sInst += $"${Hex(lo, 2)}, X {{ZPX}}";
            }
            else if (lookup[opcode].AddrMode == ZPY)
            {
                lo = _bus?.CpuRead((ushort)addr++, true) ?? 0x00;
                sInst += $"${Hex(lo, 2)}, Y {{ZPY}}";
            }
            else if (lookup[opcode].AddrMode == IZX)
            {
                lo = _bus?.CpuRead((ushort)addr++, true) ?? 0x00;
                sInst += $"(${Hex(lo, 2)}, X) {{IZX}}";
            }
            else if (lookup[opcode].AddrMode == IZY)
            {
                lo = _bus?.CpuRead((ushort)addr++, true) ?? 0x00;
                sInst += $"(${Hex(lo, 2)}), Y {{IZY}}";
            }
            else if (lookup[opcode].AddrMode == ABS)
            {
                lo = _bus?.CpuRead((ushort)addr++, true) ?? 0x00;
                hi = _bus?.CpuRead((ushort)addr++, true) ?? 0x00;
                sInst += $"${Hex((ushort)((hi << 8) | lo), 4)} {{ABS}}";
            }
            else if (lookup[opcode].AddrMode == ABX)
            {
                lo = _bus?.CpuRead((ushort)addr++, true) ?? 0x00;
                hi = _bus?.CpuRead((ushort)addr++, true) ?? 0x00;
                sInst += $"${Hex((ushort)((hi << 8) | lo), 4)}, X {{ABX}}";
            }
            else if (lookup[opcode].AddrMode == ABY)
            {
                lo = _bus?.CpuRead((ushort)addr++, true) ?? 0x00;
                hi = _bus?.CpuRead((ushort)addr++, true) ?? 0x00;
                sInst += $"${Hex((ushort)((hi << 8) | lo), 4)}, Y {{ABY}}";
            }
            else if (lookup[opcode].AddrMode == IND)
            {
                lo = _bus?.CpuRead((ushort)addr++, true) ?? 0x00;
                hi = _bus?.CpuRead((ushort)addr++, true) ?? 0x00;
                sInst += $"(${Hex((ushort)((hi << 8) | lo), 4)}) {{IND}}";
            }
            else if (lookup[opcode].AddrMode == REL)
            {
                value = _bus?.CpuRead((ushort)addr++, true) ?? 0x00;
                sInst += $"${Hex(value, 2)} [${Hex((uint)(addr + (short)value), 4)}] {{REL}}";
            }

            // Add the disassembled instruction to the dictionary
            mapLines[lineAddr] = sInst;
        }

        return mapLines;
    }

}