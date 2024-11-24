using CorePixelEngine;
using Pixel = CorePixelEngine.Pixel;

namespace NesCSharp
{
    public class SoshimoziEntertainmentSystem : PixelGameEngine
    {
        protected override string sAppName => "olc6502 System";
        private readonly Bus _nes = new();
        private Cartridge? _cartridge;

        private bool emulationRun = false;
        private float residualTime = 0.0f;

        private Dictionary<ushort, string>? _mapAsm;

        private static string Hex(uint n, int d)
        {
            return n.ToString($"X{d}");
        }

        private void DrawCpu(int x, int y)
        {
            DrawString(new VectorI2d(x, y), "STATUS:", Pixel.WHITE, 1);
            DrawString(new VectorI2d(x + 64, y), "N", (_nes.Cpu.Status & (byte)Olc6502.Flags6502.N) != 0 ? Pixel.GREEN: Pixel.RED, 1);
            DrawString(new VectorI2d(x + 80, y), "V", (_nes.Cpu.Status & (byte)Olc6502.Flags6502.V) != 0 ? Pixel.GREEN : Pixel.RED, 1);
            DrawString(new VectorI2d(x + 96, y), "-", (_nes.Cpu.Status & (byte)Olc6502.Flags6502.U) != 0 ? Pixel.GREEN : Pixel.RED, 1);
            DrawString(new VectorI2d(x + 112, y), "B", (_nes.Cpu.Status & (byte)Olc6502.Flags6502.B) != 0 ? Pixel.GREEN : Pixel.RED, 1);
            DrawString(new VectorI2d(x + 128, y), "D", (_nes.Cpu.Status & (byte)Olc6502.Flags6502.D) != 0 ? Pixel.GREEN : Pixel.RED, 1);
            DrawString(new VectorI2d(x + 144, y), "I", (_nes.Cpu.Status & (byte)Olc6502.Flags6502.I) != 0 ? Pixel.GREEN : Pixel.RED, 1);
            DrawString(new VectorI2d(x + 160, y), "Z", (_nes.Cpu.Status & (byte)Olc6502.Flags6502.Z) != 0 ? Pixel.GREEN : Pixel.RED, 1);
            DrawString(new VectorI2d(x + 178, y), "C", (_nes.Cpu.Status & (byte)Olc6502.Flags6502.C) != 0 ? Pixel.GREEN : Pixel.RED, 1);
            DrawString(new VectorI2d(x, y + 10), "PC: $" + Hex(_nes.Cpu.Pc, 4), Pixel.WHITE, 1);
            DrawString(new VectorI2d(x, y + 20), "A: $" + Hex(_nes.Cpu.A, 2) + "  [" + _nes.Cpu.A + "]", Pixel.WHITE, 1);
            DrawString(new VectorI2d(x, y + 30), "X: $" + Hex(_nes.Cpu.X, 2) + "  [" + _nes.Cpu.X + "]", Pixel.WHITE, 1);
            DrawString(new VectorI2d(x, y + 40), "Y: $" + Hex(_nes.Cpu.Y, 2) + "  [" + _nes.Cpu.Y + "]", Pixel.WHITE, 1);
            DrawString(new VectorI2d(x, y + 50), "Stack P: $" + Hex(_nes.Cpu.Stkp, 4), Pixel.WHITE, 1);
        }

        //private void DrawRam(int x, int y, ushort address, int nRows, int nColumns)
        //{
        //    var nRamY = y;
        //    for (var row = 0; row < nRows; row++)
        //    {
        //        var sOffset = new StringBuilder("$" + Hex(address, 4) + ":");
        //        for (var col = 0; col < nColumns; col++)
        //        {
        //            sOffset.Append($" {Hex(_nes.CpuRead(address, true), 1)}");
        //            address += 1;
        //        }
        //        DrawString(x, nRamY, sOffset.ToString(), Pixel.WHITE, 1);
        //        nRamY += 10;
        //    }
        //}

        private void DrawCode(int x, int y, int nLines)
        {
            // Find the current instruction in the disassembly map
            if (_mapAsm == null || !_mapAsm.TryGetValue(_nes.Cpu.Pc, out var currentInstruction))
                return;

            // Calculate the middle line's Y-coordinate
            var nLineY = ((nLines / 2) * 10) + y;

            // Draw the current instruction in cyan
            DrawString(new VectorI2d(x, nLineY), currentInstruction, Pixel.CYAN, 1);

            var maxY = (nLines * 10) + y;

            var offset = _nes.Cpu.Pc;
            while (nLineY < maxY)
            {
                offset++;
                if (!_mapAsm.TryGetValue(offset, out currentInstruction))
                    continue;

                nLineY += 10;
                DrawString(new VectorI2d(x, nLineY), currentInstruction, Pixel.WHITE, 1);
            }

            // Reset iterator to the current PC instruction
            offset = _nes.Cpu.Pc;
            nLineY = (nLines >> 1) * 10 + y;

            // Draw lines above the current instruction
            while (nLineY > y)
            {
                offset--;
                if (!_mapAsm.TryGetValue(offset, out currentInstruction))
                    continue;

                nLineY -= 10;
                DrawString(new VectorI2d(x, nLineY), currentInstruction, Pixel.WHITE, 1);
            }
        }

        public override bool OnUserCreate()
        {
            _cartridge = new Cartridge("nestest.nes");
            _nes.InsertCartridge(_cartridge);

            _mapAsm = _nes.Cpu.Disassemble(0x0000, 0xffff);
            _nes.Reset();
            
            return true;
        }

        public override bool OnUserUpdate(float fElapsedTime)
        {
            Clear(Pixel.DARK_BLUE);

            if (emulationRun)
            {
                if (residualTime > 0.0f)
                {
                    fElapsedTime -= fElapsedTime;
                }
                else
                {
                    residualTime += (1.0f / 60.0f) - fElapsedTime;
                    do
                    {
                        _nes.Clock();
                    } while (!_nes.Ppu.FrameComplete);
                    _nes.Ppu.FrameComplete = false;
                }
            }
            else
            {
                if (Input.GetKey(Key.C).bPressed)
                {
                    // Clock enough times to execute a whole CPU instruction
                    do { _nes.Clock(); } while (!_nes.Cpu.Complete());

                    //  CPU clock runs slower than system clock, so it may be
                    // complete for additional clock cycles.  Drain
                    // those out
                    do { _nes.Clock(); } while (_nes.Cpu.Complete());
                }

                if (Input.GetKey(Key.F).bPressed)
                {
                    // Clock enough times to execute a whole CPU instruction
                    do { _nes.Clock(); } while (!_nes.Ppu.FrameComplete);

                    //  CPU clock runs slower than system clock, so it may be
                    // complete for additional clock cycles.  Drain
                    // those out
                    do { _nes.Clock(); } while (_nes.Cpu.Complete());

                    _nes.Ppu.FrameComplete = false;
                }
            }

            if (Input.GetKey(Key.SPACE).bPressed) emulationRun = !emulationRun;
            if (Input.GetKey(Key.R).bPressed)
            {
                _nes.Reset();
            }

            DrawCpu(516, 2);
            DrawCode(516, 72, 26);

            DrawSprite(new VectorI2d(0, 0), _nes.Ppu.GetScreen(), 2, 0);
            return true;
        }
    }

    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            var game = new SoshimoziEntertainmentSystem();

            if (game.Construct(780, 480, 2, 2, false, false) == RCode.OK)
            {
                game.Start();
            }
        }
    }
}