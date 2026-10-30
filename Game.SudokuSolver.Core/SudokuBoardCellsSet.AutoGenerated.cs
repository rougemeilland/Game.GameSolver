using System.Runtime.CompilerServices;

namespace Game.SudokuSolver
{
    internal sealed partial class SudokuBoardCellsSet
    {
        private static readonly byte[][] _visibleCellsIndexes =
        [
            [ 01, 02, 03, 04, 05, 06, 07, 08, 09, 10, 11, 18, 19, 20, 27, 36, 45, 54, 63, 72 ],
            [ 00, 02, 03, 04, 05, 06, 07, 08, 09, 10, 11, 18, 19, 20, 28, 37, 46, 55, 64, 73 ],
            [ 00, 01, 03, 04, 05, 06, 07, 08, 09, 10, 11, 18, 19, 20, 29, 38, 47, 56, 65, 74 ],
            [ 00, 01, 02, 04, 05, 06, 07, 08, 12, 13, 14, 21, 22, 23, 30, 39, 48, 57, 66, 75 ],
            [ 00, 01, 02, 03, 05, 06, 07, 08, 12, 13, 14, 21, 22, 23, 31, 40, 49, 58, 67, 76 ],
            [ 00, 01, 02, 03, 04, 06, 07, 08, 12, 13, 14, 21, 22, 23, 32, 41, 50, 59, 68, 77 ],
            [ 00, 01, 02, 03, 04, 05, 07, 08, 15, 16, 17, 24, 25, 26, 33, 42, 51, 60, 69, 78 ],
            [ 00, 01, 02, 03, 04, 05, 06, 08, 15, 16, 17, 24, 25, 26, 34, 43, 52, 61, 70, 79 ],
            [ 00, 01, 02, 03, 04, 05, 06, 07, 15, 16, 17, 24, 25, 26, 35, 44, 53, 62, 71, 80 ],
            [ 00, 01, 02, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 27, 36, 45, 54, 63, 72 ],
            [ 00, 01, 02, 09, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 28, 37, 46, 55, 64, 73 ],
            [ 00, 01, 02, 09, 10, 12, 13, 14, 15, 16, 17, 18, 19, 20, 29, 38, 47, 56, 65, 74 ],
            [ 03, 04, 05, 09, 10, 11, 13, 14, 15, 16, 17, 21, 22, 23, 30, 39, 48, 57, 66, 75 ],
            [ 03, 04, 05, 09, 10, 11, 12, 14, 15, 16, 17, 21, 22, 23, 31, 40, 49, 58, 67, 76 ],
            [ 03, 04, 05, 09, 10, 11, 12, 13, 15, 16, 17, 21, 22, 23, 32, 41, 50, 59, 68, 77 ],
            [ 06, 07, 08, 09, 10, 11, 12, 13, 14, 16, 17, 24, 25, 26, 33, 42, 51, 60, 69, 78 ],
            [ 06, 07, 08, 09, 10, 11, 12, 13, 14, 15, 17, 24, 25, 26, 34, 43, 52, 61, 70, 79 ],
            [ 06, 07, 08, 09, 10, 11, 12, 13, 14, 15, 16, 24, 25, 26, 35, 44, 53, 62, 71, 80 ],
            [ 00, 01, 02, 09, 10, 11, 19, 20, 21, 22, 23, 24, 25, 26, 27, 36, 45, 54, 63, 72 ],
            [ 00, 01, 02, 09, 10, 11, 18, 20, 21, 22, 23, 24, 25, 26, 28, 37, 46, 55, 64, 73 ],
            [ 00, 01, 02, 09, 10, 11, 18, 19, 21, 22, 23, 24, 25, 26, 29, 38, 47, 56, 65, 74 ],
            [ 03, 04, 05, 12, 13, 14, 18, 19, 20, 22, 23, 24, 25, 26, 30, 39, 48, 57, 66, 75 ],
            [ 03, 04, 05, 12, 13, 14, 18, 19, 20, 21, 23, 24, 25, 26, 31, 40, 49, 58, 67, 76 ],
            [ 03, 04, 05, 12, 13, 14, 18, 19, 20, 21, 22, 24, 25, 26, 32, 41, 50, 59, 68, 77 ],
            [ 06, 07, 08, 15, 16, 17, 18, 19, 20, 21, 22, 23, 25, 26, 33, 42, 51, 60, 69, 78 ],
            [ 06, 07, 08, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 26, 34, 43, 52, 61, 70, 79 ],
            [ 06, 07, 08, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 35, 44, 53, 62, 71, 80 ],
            [ 00, 09, 18, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 45, 46, 47, 54, 63, 72 ],
            [ 01, 10, 19, 27, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 45, 46, 47, 55, 64, 73 ],
            [ 02, 11, 20, 27, 28, 30, 31, 32, 33, 34, 35, 36, 37, 38, 45, 46, 47, 56, 65, 74 ],
            [ 03, 12, 21, 27, 28, 29, 31, 32, 33, 34, 35, 39, 40, 41, 48, 49, 50, 57, 66, 75 ],
            [ 04, 13, 22, 27, 28, 29, 30, 32, 33, 34, 35, 39, 40, 41, 48, 49, 50, 58, 67, 76 ],
            [ 05, 14, 23, 27, 28, 29, 30, 31, 33, 34, 35, 39, 40, 41, 48, 49, 50, 59, 68, 77 ],
            [ 06, 15, 24, 27, 28, 29, 30, 31, 32, 34, 35, 42, 43, 44, 51, 52, 53, 60, 69, 78 ],
            [ 07, 16, 25, 27, 28, 29, 30, 31, 32, 33, 35, 42, 43, 44, 51, 52, 53, 61, 70, 79 ],
            [ 08, 17, 26, 27, 28, 29, 30, 31, 32, 33, 34, 42, 43, 44, 51, 52, 53, 62, 71, 80 ],
            [ 00, 09, 18, 27, 28, 29, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 54, 63, 72 ],
            [ 01, 10, 19, 27, 28, 29, 36, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 55, 64, 73 ],
            [ 02, 11, 20, 27, 28, 29, 36, 37, 39, 40, 41, 42, 43, 44, 45, 46, 47, 56, 65, 74 ],
            [ 03, 12, 21, 30, 31, 32, 36, 37, 38, 40, 41, 42, 43, 44, 48, 49, 50, 57, 66, 75 ],
            [ 04, 13, 22, 30, 31, 32, 36, 37, 38, 39, 41, 42, 43, 44, 48, 49, 50, 58, 67, 76 ],
            [ 05, 14, 23, 30, 31, 32, 36, 37, 38, 39, 40, 42, 43, 44, 48, 49, 50, 59, 68, 77 ],
            [ 06, 15, 24, 33, 34, 35, 36, 37, 38, 39, 40, 41, 43, 44, 51, 52, 53, 60, 69, 78 ],
            [ 07, 16, 25, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 44, 51, 52, 53, 61, 70, 79 ],
            [ 08, 17, 26, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 51, 52, 53, 62, 71, 80 ],
            [ 00, 09, 18, 27, 28, 29, 36, 37, 38, 46, 47, 48, 49, 50, 51, 52, 53, 54, 63, 72 ],
            [ 01, 10, 19, 27, 28, 29, 36, 37, 38, 45, 47, 48, 49, 50, 51, 52, 53, 55, 64, 73 ],
            [ 02, 11, 20, 27, 28, 29, 36, 37, 38, 45, 46, 48, 49, 50, 51, 52, 53, 56, 65, 74 ],
            [ 03, 12, 21, 30, 31, 32, 39, 40, 41, 45, 46, 47, 49, 50, 51, 52, 53, 57, 66, 75 ],
            [ 04, 13, 22, 30, 31, 32, 39, 40, 41, 45, 46, 47, 48, 50, 51, 52, 53, 58, 67, 76 ],
            [ 05, 14, 23, 30, 31, 32, 39, 40, 41, 45, 46, 47, 48, 49, 51, 52, 53, 59, 68, 77 ],
            [ 06, 15, 24, 33, 34, 35, 42, 43, 44, 45, 46, 47, 48, 49, 50, 52, 53, 60, 69, 78 ],
            [ 07, 16, 25, 33, 34, 35, 42, 43, 44, 45, 46, 47, 48, 49, 50, 51, 53, 61, 70, 79 ],
            [ 08, 17, 26, 33, 34, 35, 42, 43, 44, 45, 46, 47, 48, 49, 50, 51, 52, 62, 71, 80 ],
            [ 00, 09, 18, 27, 36, 45, 55, 56, 57, 58, 59, 60, 61, 62, 63, 64, 65, 72, 73, 74 ],
            [ 01, 10, 19, 28, 37, 46, 54, 56, 57, 58, 59, 60, 61, 62, 63, 64, 65, 72, 73, 74 ],
            [ 02, 11, 20, 29, 38, 47, 54, 55, 57, 58, 59, 60, 61, 62, 63, 64, 65, 72, 73, 74 ],
            [ 03, 12, 21, 30, 39, 48, 54, 55, 56, 58, 59, 60, 61, 62, 66, 67, 68, 75, 76, 77 ],
            [ 04, 13, 22, 31, 40, 49, 54, 55, 56, 57, 59, 60, 61, 62, 66, 67, 68, 75, 76, 77 ],
            [ 05, 14, 23, 32, 41, 50, 54, 55, 56, 57, 58, 60, 61, 62, 66, 67, 68, 75, 76, 77 ],
            [ 06, 15, 24, 33, 42, 51, 54, 55, 56, 57, 58, 59, 61, 62, 69, 70, 71, 78, 79, 80 ],
            [ 07, 16, 25, 34, 43, 52, 54, 55, 56, 57, 58, 59, 60, 62, 69, 70, 71, 78, 79, 80 ],
            [ 08, 17, 26, 35, 44, 53, 54, 55, 56, 57, 58, 59, 60, 61, 69, 70, 71, 78, 79, 80 ],
            [ 00, 09, 18, 27, 36, 45, 54, 55, 56, 64, 65, 66, 67, 68, 69, 70, 71, 72, 73, 74 ],
            [ 01, 10, 19, 28, 37, 46, 54, 55, 56, 63, 65, 66, 67, 68, 69, 70, 71, 72, 73, 74 ],
            [ 02, 11, 20, 29, 38, 47, 54, 55, 56, 63, 64, 66, 67, 68, 69, 70, 71, 72, 73, 74 ],
            [ 03, 12, 21, 30, 39, 48, 57, 58, 59, 63, 64, 65, 67, 68, 69, 70, 71, 75, 76, 77 ],
            [ 04, 13, 22, 31, 40, 49, 57, 58, 59, 63, 64, 65, 66, 68, 69, 70, 71, 75, 76, 77 ],
            [ 05, 14, 23, 32, 41, 50, 57, 58, 59, 63, 64, 65, 66, 67, 69, 70, 71, 75, 76, 77 ],
            [ 06, 15, 24, 33, 42, 51, 60, 61, 62, 63, 64, 65, 66, 67, 68, 70, 71, 78, 79, 80 ],
            [ 07, 16, 25, 34, 43, 52, 60, 61, 62, 63, 64, 65, 66, 67, 68, 69, 71, 78, 79, 80 ],
            [ 08, 17, 26, 35, 44, 53, 60, 61, 62, 63, 64, 65, 66, 67, 68, 69, 70, 78, 79, 80 ],
            [ 00, 09, 18, 27, 36, 45, 54, 55, 56, 63, 64, 65, 73, 74, 75, 76, 77, 78, 79, 80 ],
            [ 01, 10, 19, 28, 37, 46, 54, 55, 56, 63, 64, 65, 72, 74, 75, 76, 77, 78, 79, 80 ],
            [ 02, 11, 20, 29, 38, 47, 54, 55, 56, 63, 64, 65, 72, 73, 75, 76, 77, 78, 79, 80 ],
            [ 03, 12, 21, 30, 39, 48, 57, 58, 59, 66, 67, 68, 72, 73, 74, 76, 77, 78, 79, 80 ],
            [ 04, 13, 22, 31, 40, 49, 57, 58, 59, 66, 67, 68, 72, 73, 74, 75, 77, 78, 79, 80 ],
            [ 05, 14, 23, 32, 41, 50, 57, 58, 59, 66, 67, 68, 72, 73, 74, 75, 76, 78, 79, 80 ],
            [ 06, 15, 24, 33, 42, 51, 60, 61, 62, 69, 70, 71, 72, 73, 74, 75, 76, 77, 79, 80 ],
            [ 07, 16, 25, 34, 43, 52, 60, 61, 62, 69, 70, 71, 72, 73, 74, 75, 76, 77, 78, 80 ],
            [ 08, 17, 26, 35, 44, 53, 60, 61, 62, 69, 70, 71, 72, 73, 74, 75, 76, 77, 78, 79 ],
        ];

        /// <summary>
        /// 全ての未確定セルの数字のメモの数の合計値を求めます。
        /// </summary>
        /// <returns>
        /// 全ての未確定セルの数字のメモの数の合計値を示す <see cref="int"/> 値です。
        /// </returns>
        /// <remarks>
        /// 未確定セルの数字のメモの数の合計値は定石が適用される度に減少して盤面が完了すれば 0 になるので、進捗確認などの目的で使用することが出来ます。
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public int GetTotalCellNoteCount()
        {
            var cellValue = _cells[0];
            var totalCellNoteCount = ushort.PopCount((ushort)(cellValue & (((cellValue & _IS_DETERMINED) >> 9) - 1)));
            cellValue = _cells[1];
            totalCellNoteCount += ushort.PopCount((ushort)(cellValue & (((cellValue & _IS_DETERMINED) >> 9) - 1)));
            cellValue = _cells[2];
            totalCellNoteCount += ushort.PopCount((ushort)(cellValue & (((cellValue & _IS_DETERMINED) >> 9) - 1)));
            cellValue = _cells[3];
            totalCellNoteCount += ushort.PopCount((ushort)(cellValue & (((cellValue & _IS_DETERMINED) >> 9) - 1)));
            cellValue = _cells[4];
            totalCellNoteCount += ushort.PopCount((ushort)(cellValue & (((cellValue & _IS_DETERMINED) >> 9) - 1)));
            cellValue = _cells[5];
            totalCellNoteCount += ushort.PopCount((ushort)(cellValue & (((cellValue & _IS_DETERMINED) >> 9) - 1)));
            cellValue = _cells[6];
            totalCellNoteCount += ushort.PopCount((ushort)(cellValue & (((cellValue & _IS_DETERMINED) >> 9) - 1)));
            cellValue = _cells[7];
            totalCellNoteCount += ushort.PopCount((ushort)(cellValue & (((cellValue & _IS_DETERMINED) >> 9) - 1)));
            cellValue = _cells[8];
            totalCellNoteCount += ushort.PopCount((ushort)(cellValue & (((cellValue & _IS_DETERMINED) >> 9) - 1)));
            cellValue = _cells[9];
            totalCellNoteCount += ushort.PopCount((ushort)(cellValue & (((cellValue & _IS_DETERMINED) >> 9) - 1)));
            cellValue = _cells[10];
            totalCellNoteCount += ushort.PopCount((ushort)(cellValue & (((cellValue & _IS_DETERMINED) >> 9) - 1)));
            cellValue = _cells[11];
            totalCellNoteCount += ushort.PopCount((ushort)(cellValue & (((cellValue & _IS_DETERMINED) >> 9) - 1)));
            cellValue = _cells[12];
            totalCellNoteCount += ushort.PopCount((ushort)(cellValue & (((cellValue & _IS_DETERMINED) >> 9) - 1)));
            cellValue = _cells[13];
            totalCellNoteCount += ushort.PopCount((ushort)(cellValue & (((cellValue & _IS_DETERMINED) >> 9) - 1)));
            cellValue = _cells[14];
            totalCellNoteCount += ushort.PopCount((ushort)(cellValue & (((cellValue & _IS_DETERMINED) >> 9) - 1)));
            cellValue = _cells[15];
            totalCellNoteCount += ushort.PopCount((ushort)(cellValue & (((cellValue & _IS_DETERMINED) >> 9) - 1)));
            cellValue = _cells[16];
            totalCellNoteCount += ushort.PopCount((ushort)(cellValue & (((cellValue & _IS_DETERMINED) >> 9) - 1)));
            cellValue = _cells[17];
            totalCellNoteCount += ushort.PopCount((ushort)(cellValue & (((cellValue & _IS_DETERMINED) >> 9) - 1)));
            cellValue = _cells[18];
            totalCellNoteCount += ushort.PopCount((ushort)(cellValue & (((cellValue & _IS_DETERMINED) >> 9) - 1)));
            cellValue = _cells[19];
            totalCellNoteCount += ushort.PopCount((ushort)(cellValue & (((cellValue & _IS_DETERMINED) >> 9) - 1)));
            cellValue = _cells[20];
            totalCellNoteCount += ushort.PopCount((ushort)(cellValue & (((cellValue & _IS_DETERMINED) >> 9) - 1)));
            cellValue = _cells[21];
            totalCellNoteCount += ushort.PopCount((ushort)(cellValue & (((cellValue & _IS_DETERMINED) >> 9) - 1)));
            cellValue = _cells[22];
            totalCellNoteCount += ushort.PopCount((ushort)(cellValue & (((cellValue & _IS_DETERMINED) >> 9) - 1)));
            cellValue = _cells[23];
            totalCellNoteCount += ushort.PopCount((ushort)(cellValue & (((cellValue & _IS_DETERMINED) >> 9) - 1)));
            cellValue = _cells[24];
            totalCellNoteCount += ushort.PopCount((ushort)(cellValue & (((cellValue & _IS_DETERMINED) >> 9) - 1)));
            cellValue = _cells[25];
            totalCellNoteCount += ushort.PopCount((ushort)(cellValue & (((cellValue & _IS_DETERMINED) >> 9) - 1)));
            cellValue = _cells[26];
            totalCellNoteCount += ushort.PopCount((ushort)(cellValue & (((cellValue & _IS_DETERMINED) >> 9) - 1)));
            cellValue = _cells[27];
            totalCellNoteCount += ushort.PopCount((ushort)(cellValue & (((cellValue & _IS_DETERMINED) >> 9) - 1)));
            cellValue = _cells[28];
            totalCellNoteCount += ushort.PopCount((ushort)(cellValue & (((cellValue & _IS_DETERMINED) >> 9) - 1)));
            cellValue = _cells[29];
            totalCellNoteCount += ushort.PopCount((ushort)(cellValue & (((cellValue & _IS_DETERMINED) >> 9) - 1)));
            cellValue = _cells[30];
            totalCellNoteCount += ushort.PopCount((ushort)(cellValue & (((cellValue & _IS_DETERMINED) >> 9) - 1)));
            cellValue = _cells[31];
            totalCellNoteCount += ushort.PopCount((ushort)(cellValue & (((cellValue & _IS_DETERMINED) >> 9) - 1)));
            cellValue = _cells[32];
            totalCellNoteCount += ushort.PopCount((ushort)(cellValue & (((cellValue & _IS_DETERMINED) >> 9) - 1)));
            cellValue = _cells[33];
            totalCellNoteCount += ushort.PopCount((ushort)(cellValue & (((cellValue & _IS_DETERMINED) >> 9) - 1)));
            cellValue = _cells[34];
            totalCellNoteCount += ushort.PopCount((ushort)(cellValue & (((cellValue & _IS_DETERMINED) >> 9) - 1)));
            cellValue = _cells[35];
            totalCellNoteCount += ushort.PopCount((ushort)(cellValue & (((cellValue & _IS_DETERMINED) >> 9) - 1)));
            cellValue = _cells[36];
            totalCellNoteCount += ushort.PopCount((ushort)(cellValue & (((cellValue & _IS_DETERMINED) >> 9) - 1)));
            cellValue = _cells[37];
            totalCellNoteCount += ushort.PopCount((ushort)(cellValue & (((cellValue & _IS_DETERMINED) >> 9) - 1)));
            cellValue = _cells[38];
            totalCellNoteCount += ushort.PopCount((ushort)(cellValue & (((cellValue & _IS_DETERMINED) >> 9) - 1)));
            cellValue = _cells[39];
            totalCellNoteCount += ushort.PopCount((ushort)(cellValue & (((cellValue & _IS_DETERMINED) >> 9) - 1)));
            cellValue = _cells[40];
            totalCellNoteCount += ushort.PopCount((ushort)(cellValue & (((cellValue & _IS_DETERMINED) >> 9) - 1)));
            cellValue = _cells[41];
            totalCellNoteCount += ushort.PopCount((ushort)(cellValue & (((cellValue & _IS_DETERMINED) >> 9) - 1)));
            cellValue = _cells[42];
            totalCellNoteCount += ushort.PopCount((ushort)(cellValue & (((cellValue & _IS_DETERMINED) >> 9) - 1)));
            cellValue = _cells[43];
            totalCellNoteCount += ushort.PopCount((ushort)(cellValue & (((cellValue & _IS_DETERMINED) >> 9) - 1)));
            cellValue = _cells[44];
            totalCellNoteCount += ushort.PopCount((ushort)(cellValue & (((cellValue & _IS_DETERMINED) >> 9) - 1)));
            cellValue = _cells[45];
            totalCellNoteCount += ushort.PopCount((ushort)(cellValue & (((cellValue & _IS_DETERMINED) >> 9) - 1)));
            cellValue = _cells[46];
            totalCellNoteCount += ushort.PopCount((ushort)(cellValue & (((cellValue & _IS_DETERMINED) >> 9) - 1)));
            cellValue = _cells[47];
            totalCellNoteCount += ushort.PopCount((ushort)(cellValue & (((cellValue & _IS_DETERMINED) >> 9) - 1)));
            cellValue = _cells[48];
            totalCellNoteCount += ushort.PopCount((ushort)(cellValue & (((cellValue & _IS_DETERMINED) >> 9) - 1)));
            cellValue = _cells[49];
            totalCellNoteCount += ushort.PopCount((ushort)(cellValue & (((cellValue & _IS_DETERMINED) >> 9) - 1)));
            cellValue = _cells[50];
            totalCellNoteCount += ushort.PopCount((ushort)(cellValue & (((cellValue & _IS_DETERMINED) >> 9) - 1)));
            cellValue = _cells[51];
            totalCellNoteCount += ushort.PopCount((ushort)(cellValue & (((cellValue & _IS_DETERMINED) >> 9) - 1)));
            cellValue = _cells[52];
            totalCellNoteCount += ushort.PopCount((ushort)(cellValue & (((cellValue & _IS_DETERMINED) >> 9) - 1)));
            cellValue = _cells[53];
            totalCellNoteCount += ushort.PopCount((ushort)(cellValue & (((cellValue & _IS_DETERMINED) >> 9) - 1)));
            cellValue = _cells[54];
            totalCellNoteCount += ushort.PopCount((ushort)(cellValue & (((cellValue & _IS_DETERMINED) >> 9) - 1)));
            cellValue = _cells[55];
            totalCellNoteCount += ushort.PopCount((ushort)(cellValue & (((cellValue & _IS_DETERMINED) >> 9) - 1)));
            cellValue = _cells[56];
            totalCellNoteCount += ushort.PopCount((ushort)(cellValue & (((cellValue & _IS_DETERMINED) >> 9) - 1)));
            cellValue = _cells[57];
            totalCellNoteCount += ushort.PopCount((ushort)(cellValue & (((cellValue & _IS_DETERMINED) >> 9) - 1)));
            cellValue = _cells[58];
            totalCellNoteCount += ushort.PopCount((ushort)(cellValue & (((cellValue & _IS_DETERMINED) >> 9) - 1)));
            cellValue = _cells[59];
            totalCellNoteCount += ushort.PopCount((ushort)(cellValue & (((cellValue & _IS_DETERMINED) >> 9) - 1)));
            cellValue = _cells[60];
            totalCellNoteCount += ushort.PopCount((ushort)(cellValue & (((cellValue & _IS_DETERMINED) >> 9) - 1)));
            cellValue = _cells[61];
            totalCellNoteCount += ushort.PopCount((ushort)(cellValue & (((cellValue & _IS_DETERMINED) >> 9) - 1)));
            cellValue = _cells[62];
            totalCellNoteCount += ushort.PopCount((ushort)(cellValue & (((cellValue & _IS_DETERMINED) >> 9) - 1)));
            cellValue = _cells[63];
            totalCellNoteCount += ushort.PopCount((ushort)(cellValue & (((cellValue & _IS_DETERMINED) >> 9) - 1)));
            cellValue = _cells[64];
            totalCellNoteCount += ushort.PopCount((ushort)(cellValue & (((cellValue & _IS_DETERMINED) >> 9) - 1)));
            cellValue = _cells[65];
            totalCellNoteCount += ushort.PopCount((ushort)(cellValue & (((cellValue & _IS_DETERMINED) >> 9) - 1)));
            cellValue = _cells[66];
            totalCellNoteCount += ushort.PopCount((ushort)(cellValue & (((cellValue & _IS_DETERMINED) >> 9) - 1)));
            cellValue = _cells[67];
            totalCellNoteCount += ushort.PopCount((ushort)(cellValue & (((cellValue & _IS_DETERMINED) >> 9) - 1)));
            cellValue = _cells[68];
            totalCellNoteCount += ushort.PopCount((ushort)(cellValue & (((cellValue & _IS_DETERMINED) >> 9) - 1)));
            cellValue = _cells[69];
            totalCellNoteCount += ushort.PopCount((ushort)(cellValue & (((cellValue & _IS_DETERMINED) >> 9) - 1)));
            cellValue = _cells[70];
            totalCellNoteCount += ushort.PopCount((ushort)(cellValue & (((cellValue & _IS_DETERMINED) >> 9) - 1)));
            cellValue = _cells[71];
            totalCellNoteCount += ushort.PopCount((ushort)(cellValue & (((cellValue & _IS_DETERMINED) >> 9) - 1)));
            cellValue = _cells[72];
            totalCellNoteCount += ushort.PopCount((ushort)(cellValue & (((cellValue & _IS_DETERMINED) >> 9) - 1)));
            cellValue = _cells[73];
            totalCellNoteCount += ushort.PopCount((ushort)(cellValue & (((cellValue & _IS_DETERMINED) >> 9) - 1)));
            cellValue = _cells[74];
            totalCellNoteCount += ushort.PopCount((ushort)(cellValue & (((cellValue & _IS_DETERMINED) >> 9) - 1)));
            cellValue = _cells[75];
            totalCellNoteCount += ushort.PopCount((ushort)(cellValue & (((cellValue & _IS_DETERMINED) >> 9) - 1)));
            cellValue = _cells[76];
            totalCellNoteCount += ushort.PopCount((ushort)(cellValue & (((cellValue & _IS_DETERMINED) >> 9) - 1)));
            cellValue = _cells[77];
            totalCellNoteCount += ushort.PopCount((ushort)(cellValue & (((cellValue & _IS_DETERMINED) >> 9) - 1)));
            cellValue = _cells[78];
            totalCellNoteCount += ushort.PopCount((ushort)(cellValue & (((cellValue & _IS_DETERMINED) >> 9) - 1)));
            cellValue = _cells[79];
            totalCellNoteCount += ushort.PopCount((ushort)(cellValue & (((cellValue & _IS_DETERMINED) >> 9) - 1)));
            cellValue = _cells[80];
            totalCellNoteCount += ushort.PopCount((ushort)(cellValue & (((cellValue & _IS_DETERMINED) >> 9) - 1)));
            return totalCellNoteCount;
        }
    }
}
