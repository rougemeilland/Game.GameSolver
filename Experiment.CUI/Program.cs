using System;
using System.Linq;

namespace Experiment.CUI
{
    internal sealed class Program
    {
        private static void Main()
        {
            var bitSet = UInt128.Zero;
            for (var row = 0; row < 9; ++row)
                bitSet |= UInt128.One << (row * 9);

            for (var column = 0; column < 9; ++column)
            {
                var value = bitSet << column;
                Console.WriteLine($"private static readonly UInt128 _column{column}BitMask = new (0x{(value >> 64) & ulong.MaxValue:x16}, 0x{value & ulong.MaxValue:x16});");
            }

            Console.WriteLine();

            bitSet = UInt128.Zero;
            for (var column = 0; column < 9; ++column)
                bitSet |= UInt128.One << column;

            for (var row = 0; row < 9; ++row)
            {
                var value = bitSet << (row * 9);
                Console.WriteLine($"private static readonly UInt128 _row{row}BitMask = new (0x{(value >> 64) & ulong.MaxValue:x16}, 0x{value & ulong.MaxValue:x16});");
            }

            Console.WriteLine();

            _ = Console.ReadLine();

        }
    }
}
