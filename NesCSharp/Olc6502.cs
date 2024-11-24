namespace NesCSharp;

public class Olc6502
{
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
        bus = n;
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
        Stkp = 0xFD;
        Status = 0x00 | (byte)Flags6502.U;

        AddrAbs = 0xFFFC;
        ushort lo = Read((ushort)(AddrAbs + 0));
        ushort hi = Read((ushort)(AddrAbs + 1));

        Pc = (ushort)((hi << 8) | lo);

        AddrRel = 0x0000;
        AddrAbs = 0x0000;
        Fetched = 0x00;

        _cycles = 8;
    }

    // IRQ function stub
    public void Irq()
    {
        // Implementation stub
    }

    // NMI function stub
    public void Nmi()
    {
        // Implementation stub
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
            byte opcode = bus?.CpuRead((ushort)addr++, true) ?? 0x00;
            sInst += lookup[opcode].Name + " ";

            // Decode addressing mode and add operands
            if (lookup[opcode].AddrMode == IMP)
            {
                sInst += " {IMP}";
            }
            else if (lookup[opcode].AddrMode == IMM)
            {
                value = bus?.CpuRead((ushort)addr++, true) ?? 0x00;
                sInst += $"#$${Hex(value, 2)} {{IMM}}";
            }
            else if (lookup[opcode].AddrMode == ZP0)
            {
                lo = bus?.CpuRead((ushort)addr++, true) ?? 0x00;
                sInst += $"${Hex(lo, 2)} {{ZP0}}";
            }
            else if (lookup[opcode].AddrMode == ZPX)
            {
                lo = bus?.CpuRead((ushort)addr++, true) ?? 0x00;
                sInst += $"${Hex(lo, 2)}, X {{ZPX}}";
            }
            else if (lookup[opcode].AddrMode == ZPY)
            {
                lo = bus?.CpuRead((ushort)addr++, true) ?? 0x00;
                sInst += $"${Hex(lo, 2)}, Y {{ZPY}}";
            }
            else if (lookup[opcode].AddrMode == IZX)
            {
                lo = bus?.CpuRead((ushort)addr++, true) ?? 0x00;
                sInst += $"(${Hex(lo, 2)}, X) {{IZX}}";
            }
            else if (lookup[opcode].AddrMode == IZY)
            {
                lo = bus?.CpuRead((ushort)addr++, true) ?? 0x00;
                sInst += $"(${Hex(lo, 2)}), Y {{IZY}}";
            }
            else if (lookup[opcode].AddrMode == ABS)
            {
                lo = bus?.CpuRead((ushort)addr++, true) ?? 0x00;
                hi = bus?.CpuRead((ushort)addr++, true) ?? 0x00;
                sInst += $"${Hex((ushort)((hi << 8) | lo), 4)} {{ABS}}";
            }
            else if (lookup[opcode].AddrMode == ABX)
            {
                lo = bus?.CpuRead((ushort)addr++, true) ?? 0x00;
                hi = bus?.CpuRead((ushort)addr++, true) ?? 0x00;
                sInst += $"${Hex((ushort)((hi << 8) | lo), 4)}, X {{ABX}}";
            }
            else if (lookup[opcode].AddrMode == ABY)
            {
                lo = bus?.CpuRead((ushort)addr++, true) ?? 0x00;
                hi = bus?.CpuRead((ushort)addr++, true) ?? 0x00;
                sInst += $"${Hex((ushort)((hi << 8) | lo), 4)}, Y {{ABY}}";
            }
            else if (lookup[opcode].AddrMode == IND)
            {
                lo = bus?.CpuRead((ushort)addr++, true) ?? 0x00;
                hi = bus?.CpuRead((ushort)addr++, true) ?? 0x00;
                sInst += $"(${Hex((ushort)((hi << 8) | lo), 4)}) {{IND}}";
            }
            else if (lookup[opcode].AddrMode == REL)
            {
                value = bus?.CpuRead((ushort)addr++, true) ?? 0x00;
                sInst += $"${Hex(value, 2)} [${Hex((uint)(addr + (short)value), 4)}] {{REL}}";
            }

            // Add the disassembled instruction to the dictionary
            mapLines[lineAddr] = sInst;
        }

        return mapLines;
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
            Fetched = Read(AddrAbs);
        }

        return Fetched;
    }

    // Fetch data for instructions
    public byte Fetched { get; private set; } = 0x00;

    // Addressing data
    public ushort AddrAbs { get; private set; } = 0x0000;
    public ushort AddrRel { get; private set; } = 0x00;

    // Opcode and cycle information
    public byte Opcode { get; private set; } = 0x00;

    //public byte Cycles => _cycles;

    private byte _cycles =0x00;

    // Registers and flags
    public byte A { get; private set; } = 0x00;

    public byte X { get; private set; } = 0x00;

    public byte Y { get; private set; } = 0x00;

    public byte Stkp { get; private set; } = 0x00;

    public ushort Pc { get; private set; } = 0x0000;

    public byte Status { get; private set; } = 0x00; // Status Register

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

    // Private Bus reference
    private Bus? bus;

    // Read from bus
    private byte Read(ushort addr)
    {
        return bus?.CpuRead(addr, false) ?? 0;
    }

    // Write to bus
    private void Write(ushort addr, byte data)
    {
        bus?.CpuWrite(addr, data);
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

    // Instruction struct
    private class Instruction
    {
        public string Name { get; set; }
        public Func<byte> Operate { get; set; }
        public Func<byte> AddrMode { get; set; }
        public byte Cycles { get; set; }
    }

    private List<Instruction> lookup;

    // Address Mode: Implied
    // There is no additional data required for this instruction. The instruction
    // does something very simple like like sets a status bit. However, we will
    // target the accumulator, for instructions like PHA
    private byte IMP()
    {
        Fetched = A;
        return 0;
    }

    // Address Mode: Immediate
    // The instruction expects the next byte to be used as a value, so we'll prep
    // the read address to point to the next byte
    private byte IMM()
    {
        AddrAbs = Pc++;
        return 0;
    }

    // Address Mode: Zero Page
    // To save program bytes, zero page addressing allows you to absolutely address
    // a location in first 0xFF bytes of address range. Clearly this only requires
    // one byte instead of the usual two.
    private byte ZP0()
    {
        AddrAbs = Read(Pc);
        Pc++;
        AddrAbs &= 0x00ff;
        return 0;
    }

    private byte ZPX() => 0;
    private byte ZPY() => 0;
    private byte REL() => 0;
    private byte ABS() => 0;
    private byte ABX() => 0;
    private byte ABY() => 0;
    private byte IND() => 0;
    private byte IZX() => 0;
    private byte IZY() => 0;

    // Opcodes (stubs)
    private byte ADC() => 0;
    private byte AND() => 0;
    private byte ASL() => 0;
    private byte BCC() => 0;
    private byte BCS() => 0;
    private byte BEQ() => 0;
    private byte BIT() => 0;
    private byte BMI() => 0;
    private byte BNE() => 0;
    private byte BPL() => 0;
    private byte BRK() => 0;
    private byte BVC() => 0;
    private byte BVS() => 0;
    private byte CLC() => 0;
    private byte CLD() => 0;
    private byte CLI() => 0;
    private byte CLV() => 0;
    private byte CMP() => 0;
    private byte CPX() => 0;
    private byte CPY() => 0;
    private byte DEC() => 0;
    private byte DEX() => 0;
    private byte DEY() => 0;
    private byte EOR() => 0;
    private byte INC() => 0;
    private byte INX() => 0;

    private byte INY()
    {
        Y++;
        SetFlag(Flags6502.Z, Y == 0x00);
        SetFlag(Flags6502.N, (Y & 0x80) != 0);
        return 0;
    }

    private byte JMP() => 0;
    private byte JSR() => 0;
    private byte LDA() => 0;
    private byte LDX() => 0;

    private byte LDY()
    {
        Fetch();
        Y = Fetched;
        SetFlag(Flags6502.Z, Y == 0X00);
        SetFlag(Flags6502.N, (Y & 0x80) != 0);
        return 1;
    }

    private byte LSR() => 0;
    private byte NOP() => 0;
    private byte ORA() => 0;
    private byte PHA() => 0;
    private byte PHP() => 0;
    private byte PLA() => 0;
    private byte PLP() => 0;
    private byte ROL() => 0;
    private byte ROR() => 0;
    private byte RTI() => 0;
    private byte RTS() => 0;
    private byte SBC() => 0;
    private byte SEC() => 0;
    private byte SED() => 0;
    private byte SEI() => 0;
    private byte STA() => 0;
    private byte STX() => 0;
    private byte STY() => 0;
    private byte TAX() => 0;
    private byte TAY() => 0;
    private byte TSX() => 0;
    private byte TXA() => 0;
    private byte TXS() => 0;
    private byte TYA() => 0;

    private byte XXX() => 0; // Capture all "unofficial" opcodes
}