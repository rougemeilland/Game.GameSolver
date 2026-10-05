using System;
using System.Collections.Generic;
using System.Linq;

namespace Experiment.CUI
{
    internal sealed class Program
    {
        private static void Main()
        {
            for (var cellIndex = 0; cellIndex < 81; ++cellIndex)
            {
                var column = cellIndex % 9;
                var row = cellIndex / 9;
                var blockColumn = column / 3;
                var blockRow = row / 3;
                var indexList = new List<int>();

                // 横方向のハウスのインデックスの追加
                for (var count = 0; count < 9; ++count)
                {
                    var index = row * 9 + count;
                    if (index != cellIndex)
                    {
                        if (index < 0 || index >= 81)
                            throw new ApplicationException();
                        indexList.Add(index);
                    }
                }

                // 縦方向のハウスのインデックスの追加
                for (var count = 0; count < 9; ++count)
                {
                    var index = column + count * 9;
                    if (index != cellIndex)
                    {
                        if (index < 0 || index >= 81)
                            throw new ApplicationException();
                        indexList.Add(index);
                    }
                }

                // ブロックのインデックスの追加
                for (var rowCount = 0; rowCount < 3; ++rowCount)
                {
                    for (var columnCount = 0; columnCount < 3; ++columnCount)
                    {
                        var index = blockRow * 27 + blockColumn * 3 + rowCount * 9 + columnCount;
                        if (index != cellIndex)
                        {
                            if (index < 0 || index >= 81)
                                throw new ApplicationException();
                            indexList.Add(index);
                        }
                    }
                }

                Console.WriteLine($"[ {string.Join(", ", indexList.Distinct().OrderBy(n => n).Select(n => $"{n:D2}"))} ],");
            }

            Console.WriteLine();

            _ = Console.ReadLine();

        }
    }
}
