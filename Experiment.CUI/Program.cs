using System;
using System.Collections.Generic;
using System.Linq;

namespace Experiment.CUI
{
    internal sealed class Program
    {
        private static void Main()
        {
            var columnHouseBitmask = new UInt128[81];
            for (var row = 0; row < 9; ++row)
            {
                var bits = UInt128.Zero;
                for (var column = 0; column < 9; ++column)
                    bits |= UInt128.One << (row * 9 + column);
                for (var column = 0; column < 9; ++column)
                    columnHouseBitmask[row * 9 + column] = bits;
            }

            for (var row = 0; row < 9; ++row)
            {
                Console.WriteLine($"{string.Join(", ", Enumerable.Range(0, 9).Select(column => $"new UInt128(0x{columnHouseBitmask[row * 9 + column] >> 64:x8}, 0x{columnHouseBitmask[row * 9 + column] & ulong.MaxValue:x8})"))},");
            }

            Console.WriteLine();

            _ = Console.ReadLine();

        }
    }
}
