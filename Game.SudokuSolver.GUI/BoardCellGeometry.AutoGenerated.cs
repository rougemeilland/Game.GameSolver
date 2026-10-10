using System;

namespace Game.SudokuSolver.GUI
{
    internal static partial class BoardCellGeometry
    {
        /// <summary>
        /// 添え字のセルと列を同じくするセルの集合。
        /// </summary>
        private static readonly UInt128[] _columnHouseCellsBitMasksByCellIndex =
        [
            new UInt128(0x0000000000000100ul, 0x8040201008040201ul), new UInt128(0x0000000000000201ul, 0x0080402010080402ul), new UInt128(0x0000000000000402ul, 0x0100804020100804ul), new UInt128(0x0000000000000804ul, 0x0201008040201008ul), new UInt128(0x0000000000001008ul, 0x0402010080402010ul), new UInt128(0x0000000000002010ul, 0x0804020100804020ul), new UInt128(0x0000000000004020ul, 0x1008040201008040ul), new UInt128(0x0000000000008040ul, 0x2010080402010080ul), new UInt128(0x0000000000010080ul, 0x4020100804020100ul),
            new UInt128(0x0000000000000100ul, 0x8040201008040201ul), new UInt128(0x0000000000000201ul, 0x0080402010080402ul), new UInt128(0x0000000000000402ul, 0x0100804020100804ul), new UInt128(0x0000000000000804ul, 0x0201008040201008ul), new UInt128(0x0000000000001008ul, 0x0402010080402010ul), new UInt128(0x0000000000002010ul, 0x0804020100804020ul), new UInt128(0x0000000000004020ul, 0x1008040201008040ul), new UInt128(0x0000000000008040ul, 0x2010080402010080ul), new UInt128(0x0000000000010080ul, 0x4020100804020100ul),
            new UInt128(0x0000000000000100ul, 0x8040201008040201ul), new UInt128(0x0000000000000201ul, 0x0080402010080402ul), new UInt128(0x0000000000000402ul, 0x0100804020100804ul), new UInt128(0x0000000000000804ul, 0x0201008040201008ul), new UInt128(0x0000000000001008ul, 0x0402010080402010ul), new UInt128(0x0000000000002010ul, 0x0804020100804020ul), new UInt128(0x0000000000004020ul, 0x1008040201008040ul), new UInt128(0x0000000000008040ul, 0x2010080402010080ul), new UInt128(0x0000000000010080ul, 0x4020100804020100ul),
            new UInt128(0x0000000000000100ul, 0x8040201008040201ul), new UInt128(0x0000000000000201ul, 0x0080402010080402ul), new UInt128(0x0000000000000402ul, 0x0100804020100804ul), new UInt128(0x0000000000000804ul, 0x0201008040201008ul), new UInt128(0x0000000000001008ul, 0x0402010080402010ul), new UInt128(0x0000000000002010ul, 0x0804020100804020ul), new UInt128(0x0000000000004020ul, 0x1008040201008040ul), new UInt128(0x0000000000008040ul, 0x2010080402010080ul), new UInt128(0x0000000000010080ul, 0x4020100804020100ul),
            new UInt128(0x0000000000000100ul, 0x8040201008040201ul), new UInt128(0x0000000000000201ul, 0x0080402010080402ul), new UInt128(0x0000000000000402ul, 0x0100804020100804ul), new UInt128(0x0000000000000804ul, 0x0201008040201008ul), new UInt128(0x0000000000001008ul, 0x0402010080402010ul), new UInt128(0x0000000000002010ul, 0x0804020100804020ul), new UInt128(0x0000000000004020ul, 0x1008040201008040ul), new UInt128(0x0000000000008040ul, 0x2010080402010080ul), new UInt128(0x0000000000010080ul, 0x4020100804020100ul),
            new UInt128(0x0000000000000100ul, 0x8040201008040201ul), new UInt128(0x0000000000000201ul, 0x0080402010080402ul), new UInt128(0x0000000000000402ul, 0x0100804020100804ul), new UInt128(0x0000000000000804ul, 0x0201008040201008ul), new UInt128(0x0000000000001008ul, 0x0402010080402010ul), new UInt128(0x0000000000002010ul, 0x0804020100804020ul), new UInt128(0x0000000000004020ul, 0x1008040201008040ul), new UInt128(0x0000000000008040ul, 0x2010080402010080ul), new UInt128(0x0000000000010080ul, 0x4020100804020100ul),
            new UInt128(0x0000000000000100ul, 0x8040201008040201ul), new UInt128(0x0000000000000201ul, 0x0080402010080402ul), new UInt128(0x0000000000000402ul, 0x0100804020100804ul), new UInt128(0x0000000000000804ul, 0x0201008040201008ul), new UInt128(0x0000000000001008ul, 0x0402010080402010ul), new UInt128(0x0000000000002010ul, 0x0804020100804020ul), new UInt128(0x0000000000004020ul, 0x1008040201008040ul), new UInt128(0x0000000000008040ul, 0x2010080402010080ul), new UInt128(0x0000000000010080ul, 0x4020100804020100ul),
            new UInt128(0x0000000000000100ul, 0x8040201008040201ul), new UInt128(0x0000000000000201ul, 0x0080402010080402ul), new UInt128(0x0000000000000402ul, 0x0100804020100804ul), new UInt128(0x0000000000000804ul, 0x0201008040201008ul), new UInt128(0x0000000000001008ul, 0x0402010080402010ul), new UInt128(0x0000000000002010ul, 0x0804020100804020ul), new UInt128(0x0000000000004020ul, 0x1008040201008040ul), new UInt128(0x0000000000008040ul, 0x2010080402010080ul), new UInt128(0x0000000000010080ul, 0x4020100804020100ul),
            new UInt128(0x0000000000000100ul, 0x8040201008040201ul), new UInt128(0x0000000000000201ul, 0x0080402010080402ul), new UInt128(0x0000000000000402ul, 0x0100804020100804ul), new UInt128(0x0000000000000804ul, 0x0201008040201008ul), new UInt128(0x0000000000001008ul, 0x0402010080402010ul), new UInt128(0x0000000000002010ul, 0x0804020100804020ul), new UInt128(0x0000000000004020ul, 0x1008040201008040ul), new UInt128(0x0000000000008040ul, 0x2010080402010080ul), new UInt128(0x0000000000010080ul, 0x4020100804020100ul),
        ];

        /// <summary>
        /// 添え字の列と列を同じくするセルの集合。
        /// </summary>
        private static readonly UInt128[] _columnHouseCellsBitMasksByColumn =
        [
            new UInt128(0x0000000000000100ul, 0x8040201008040201ul),
            new UInt128(0x0000000000000201ul, 0x0080402010080402ul),
            new UInt128(0x0000000000000402ul, 0x0100804020100804ul),
            new UInt128(0x0000000000000804ul, 0x0201008040201008ul),
            new UInt128(0x0000000000001008ul, 0x0402010080402010ul),
            new UInt128(0x0000000000002010ul, 0x0804020100804020ul),
            new UInt128(0x0000000000004020ul, 0x1008040201008040ul),
            new UInt128(0x0000000000008040ul, 0x2010080402010080ul),
            new UInt128(0x0000000000010080ul, 0x4020100804020100ul),
        ];

        /// <summary>
        /// 添え字のセルと行を同じくするセルの集合。
        /// </summary>
        private static readonly UInt128[] _rowHouseCellsBitMasksByCellIndex =
        [
            new UInt128(0x0000000000000000ul, 0x00000000000001fful), new UInt128(0x0000000000000000ul, 0x00000000000001fful), new UInt128(0x0000000000000000ul, 0x00000000000001fful), new UInt128(0x0000000000000000ul, 0x00000000000001fful), new UInt128(0x0000000000000000ul, 0x00000000000001fful), new UInt128(0x0000000000000000ul, 0x00000000000001fful), new UInt128(0x0000000000000000ul, 0x00000000000001fful), new UInt128(0x0000000000000000ul, 0x00000000000001fful), new UInt128(0x0000000000000000ul, 0x00000000000001fful),
            new UInt128(0x0000000000000000ul, 0x000000000003fe00ul), new UInt128(0x0000000000000000ul, 0x000000000003fe00ul), new UInt128(0x0000000000000000ul, 0x000000000003fe00ul), new UInt128(0x0000000000000000ul, 0x000000000003fe00ul), new UInt128(0x0000000000000000ul, 0x000000000003fe00ul), new UInt128(0x0000000000000000ul, 0x000000000003fe00ul), new UInt128(0x0000000000000000ul, 0x000000000003fe00ul), new UInt128(0x0000000000000000ul, 0x000000000003fe00ul), new UInt128(0x0000000000000000ul, 0x000000000003fe00ul),
            new UInt128(0x0000000000000000ul, 0x0000000007fc0000ul), new UInt128(0x0000000000000000ul, 0x0000000007fc0000ul), new UInt128(0x0000000000000000ul, 0x0000000007fc0000ul), new UInt128(0x0000000000000000ul, 0x0000000007fc0000ul), new UInt128(0x0000000000000000ul, 0x0000000007fc0000ul), new UInt128(0x0000000000000000ul, 0x0000000007fc0000ul), new UInt128(0x0000000000000000ul, 0x0000000007fc0000ul), new UInt128(0x0000000000000000ul, 0x0000000007fc0000ul), new UInt128(0x0000000000000000ul, 0x0000000007fc0000ul),
            new UInt128(0x0000000000000000ul, 0x0000000ff8000000ul), new UInt128(0x0000000000000000ul, 0x0000000ff8000000ul), new UInt128(0x0000000000000000ul, 0x0000000ff8000000ul), new UInt128(0x0000000000000000ul, 0x0000000ff8000000ul), new UInt128(0x0000000000000000ul, 0x0000000ff8000000ul), new UInt128(0x0000000000000000ul, 0x0000000ff8000000ul), new UInt128(0x0000000000000000ul, 0x0000000ff8000000ul), new UInt128(0x0000000000000000ul, 0x0000000ff8000000ul), new UInt128(0x0000000000000000ul, 0x0000000ff8000000ul),
            new UInt128(0x0000000000000000ul, 0x00001ff000000000ul), new UInt128(0x0000000000000000ul, 0x00001ff000000000ul), new UInt128(0x0000000000000000ul, 0x00001ff000000000ul), new UInt128(0x0000000000000000ul, 0x00001ff000000000ul), new UInt128(0x0000000000000000ul, 0x00001ff000000000ul), new UInt128(0x0000000000000000ul, 0x00001ff000000000ul), new UInt128(0x0000000000000000ul, 0x00001ff000000000ul), new UInt128(0x0000000000000000ul, 0x00001ff000000000ul), new UInt128(0x0000000000000000ul, 0x00001ff000000000ul),
            new UInt128(0x0000000000000000ul, 0x003fe00000000000ul), new UInt128(0x0000000000000000ul, 0x003fe00000000000ul), new UInt128(0x0000000000000000ul, 0x003fe00000000000ul), new UInt128(0x0000000000000000ul, 0x003fe00000000000ul), new UInt128(0x0000000000000000ul, 0x003fe00000000000ul), new UInt128(0x0000000000000000ul, 0x003fe00000000000ul), new UInt128(0x0000000000000000ul, 0x003fe00000000000ul), new UInt128(0x0000000000000000ul, 0x003fe00000000000ul), new UInt128(0x0000000000000000ul, 0x003fe00000000000ul),
            new UInt128(0x0000000000000000ul, 0x7fc0000000000000ul), new UInt128(0x0000000000000000ul, 0x7fc0000000000000ul), new UInt128(0x0000000000000000ul, 0x7fc0000000000000ul), new UInt128(0x0000000000000000ul, 0x7fc0000000000000ul), new UInt128(0x0000000000000000ul, 0x7fc0000000000000ul), new UInt128(0x0000000000000000ul, 0x7fc0000000000000ul), new UInt128(0x0000000000000000ul, 0x7fc0000000000000ul), new UInt128(0x0000000000000000ul, 0x7fc0000000000000ul), new UInt128(0x0000000000000000ul, 0x7fc0000000000000ul),
            new UInt128(0x00000000000000fful, 0x8000000000000000ul), new UInt128(0x00000000000000fful, 0x8000000000000000ul), new UInt128(0x00000000000000fful, 0x8000000000000000ul), new UInt128(0x00000000000000fful, 0x8000000000000000ul), new UInt128(0x00000000000000fful, 0x8000000000000000ul), new UInt128(0x00000000000000fful, 0x8000000000000000ul), new UInt128(0x00000000000000fful, 0x8000000000000000ul), new UInt128(0x00000000000000fful, 0x8000000000000000ul), new UInt128(0x00000000000000fful, 0x8000000000000000ul),
            new UInt128(0x000000000001ff00ul, 0x0000000000000000ul), new UInt128(0x000000000001ff00ul, 0x0000000000000000ul), new UInt128(0x000000000001ff00ul, 0x0000000000000000ul), new UInt128(0x000000000001ff00ul, 0x0000000000000000ul), new UInt128(0x000000000001ff00ul, 0x0000000000000000ul), new UInt128(0x000000000001ff00ul, 0x0000000000000000ul), new UInt128(0x000000000001ff00ul, 0x0000000000000000ul), new UInt128(0x000000000001ff00ul, 0x0000000000000000ul), new UInt128(0x000000000001ff00ul, 0x0000000000000000ul),
        ];

        /// <summary>
        /// 添え字の行と行を同じくするセルの集合。
        /// </summary>
        private static readonly UInt128[] _rowHouseCellsBitMasksByRow =
        [
            new UInt128(0x0000000000000000ul, 0x00000000000001fful),
            new UInt128(0x0000000000000000ul, 0x000000000003fe00ul),
            new UInt128(0x0000000000000000ul, 0x0000000007fc0000ul),
            new UInt128(0x0000000000000000ul, 0x0000000ff8000000ul),
            new UInt128(0x0000000000000000ul, 0x00001ff000000000ul),
            new UInt128(0x0000000000000000ul, 0x003fe00000000000ul),
            new UInt128(0x0000000000000000ul, 0x7fc0000000000000ul),
            new UInt128(0x00000000000000fful, 0x8000000000000000ul),
            new UInt128(0x000000000001ff00ul, 0x0000000000000000ul),
        ];

        /// <summary>
        /// 添え字のセルとブロックを同じくするセルの集合。
        /// </summary>
        private static readonly UInt128[] _blockHouseCellsBitMasksByCellIndex =
        [
            new UInt128(0x0000000000000000ul, 0x00000000001c0e07ul), new UInt128(0x0000000000000000ul, 0x00000000001c0e07ul), new UInt128(0x0000000000000000ul, 0x00000000001c0e07ul), new UInt128(0x0000000000000000ul, 0x0000000000e07038ul), new UInt128(0x0000000000000000ul, 0x0000000000e07038ul), new UInt128(0x0000000000000000ul, 0x0000000000e07038ul), new UInt128(0x0000000000000000ul, 0x00000000070381c0ul), new UInt128(0x0000000000000000ul, 0x00000000070381c0ul), new UInt128(0x0000000000000000ul, 0x00000000070381c0ul),
            new UInt128(0x0000000000000000ul, 0x00000000001c0e07ul), new UInt128(0x0000000000000000ul, 0x00000000001c0e07ul), new UInt128(0x0000000000000000ul, 0x00000000001c0e07ul), new UInt128(0x0000000000000000ul, 0x0000000000e07038ul), new UInt128(0x0000000000000000ul, 0x0000000000e07038ul), new UInt128(0x0000000000000000ul, 0x0000000000e07038ul), new UInt128(0x0000000000000000ul, 0x00000000070381c0ul), new UInt128(0x0000000000000000ul, 0x00000000070381c0ul), new UInt128(0x0000000000000000ul, 0x00000000070381c0ul),
            new UInt128(0x0000000000000000ul, 0x00000000001c0e07ul), new UInt128(0x0000000000000000ul, 0x00000000001c0e07ul), new UInt128(0x0000000000000000ul, 0x00000000001c0e07ul), new UInt128(0x0000000000000000ul, 0x0000000000e07038ul), new UInt128(0x0000000000000000ul, 0x0000000000e07038ul), new UInt128(0x0000000000000000ul, 0x0000000000e07038ul), new UInt128(0x0000000000000000ul, 0x00000000070381c0ul), new UInt128(0x0000000000000000ul, 0x00000000070381c0ul), new UInt128(0x0000000000000000ul, 0x00000000070381c0ul),
            new UInt128(0x0000000000000000ul, 0x0000e07038000000ul), new UInt128(0x0000000000000000ul, 0x0000e07038000000ul), new UInt128(0x0000000000000000ul, 0x0000e07038000000ul), new UInt128(0x0000000000000000ul, 0x00070381c0000000ul), new UInt128(0x0000000000000000ul, 0x00070381c0000000ul), new UInt128(0x0000000000000000ul, 0x00070381c0000000ul), new UInt128(0x0000000000000000ul, 0x00381c0e00000000ul), new UInt128(0x0000000000000000ul, 0x00381c0e00000000ul), new UInt128(0x0000000000000000ul, 0x00381c0e00000000ul),
            new UInt128(0x0000000000000000ul, 0x0000e07038000000ul), new UInt128(0x0000000000000000ul, 0x0000e07038000000ul), new UInt128(0x0000000000000000ul, 0x0000e07038000000ul), new UInt128(0x0000000000000000ul, 0x00070381c0000000ul), new UInt128(0x0000000000000000ul, 0x00070381c0000000ul), new UInt128(0x0000000000000000ul, 0x00070381c0000000ul), new UInt128(0x0000000000000000ul, 0x00381c0e00000000ul), new UInt128(0x0000000000000000ul, 0x00381c0e00000000ul), new UInt128(0x0000000000000000ul, 0x00381c0e00000000ul),
            new UInt128(0x0000000000000000ul, 0x0000e07038000000ul), new UInt128(0x0000000000000000ul, 0x0000e07038000000ul), new UInt128(0x0000000000000000ul, 0x0000e07038000000ul), new UInt128(0x0000000000000000ul, 0x00070381c0000000ul), new UInt128(0x0000000000000000ul, 0x00070381c0000000ul), new UInt128(0x0000000000000000ul, 0x00070381c0000000ul), new UInt128(0x0000000000000000ul, 0x00381c0e00000000ul), new UInt128(0x0000000000000000ul, 0x00381c0e00000000ul), new UInt128(0x0000000000000000ul, 0x00381c0e00000000ul),
            new UInt128(0x0000000000000703ul, 0x81c0000000000000ul), new UInt128(0x0000000000000703ul, 0x81c0000000000000ul), new UInt128(0x0000000000000703ul, 0x81c0000000000000ul), new UInt128(0x000000000000381cul, 0x0e00000000000000ul), new UInt128(0x000000000000381cul, 0x0e00000000000000ul), new UInt128(0x000000000000381cul, 0x0e00000000000000ul), new UInt128(0x000000000001c0e0ul, 0x7000000000000000ul), new UInt128(0x000000000001c0e0ul, 0x7000000000000000ul), new UInt128(0x000000000001c0e0ul, 0x7000000000000000ul),
            new UInt128(0x0000000000000703ul, 0x81c0000000000000ul), new UInt128(0x0000000000000703ul, 0x81c0000000000000ul), new UInt128(0x0000000000000703ul, 0x81c0000000000000ul), new UInt128(0x000000000000381cul, 0x0e00000000000000ul), new UInt128(0x000000000000381cul, 0x0e00000000000000ul), new UInt128(0x000000000000381cul, 0x0e00000000000000ul), new UInt128(0x000000000001c0e0ul, 0x7000000000000000ul), new UInt128(0x000000000001c0e0ul, 0x7000000000000000ul), new UInt128(0x000000000001c0e0ul, 0x7000000000000000ul),
            new UInt128(0x0000000000000703ul, 0x81c0000000000000ul), new UInt128(0x0000000000000703ul, 0x81c0000000000000ul), new UInt128(0x0000000000000703ul, 0x81c0000000000000ul), new UInt128(0x000000000000381cul, 0x0e00000000000000ul), new UInt128(0x000000000000381cul, 0x0e00000000000000ul), new UInt128(0x000000000000381cul, 0x0e00000000000000ul), new UInt128(0x000000000001c0e0ul, 0x7000000000000000ul), new UInt128(0x000000000001c0e0ul, 0x7000000000000000ul), new UInt128(0x000000000001c0e0ul, 0x7000000000000000ul),
        ];

        /// <summary>
        /// 添え字のブロックとブロックを同じくするセルの集合。
        /// </summary>
        private static readonly UInt128[] _blockHouseCellsBitMasksByBlock =
        [
            new UInt128(0x0000000000000000ul, 0x00000000001c0e07ul),
            new UInt128(0x0000000000000000ul, 0x0000000000e07038ul),
            new UInt128(0x0000000000000000ul, 0x00000000070381c0ul),
            new UInt128(0x0000000000000000ul, 0x0000e07038000000ul),
            new UInt128(0x0000000000000000ul, 0x00070381c0000000ul),
            new UInt128(0x0000000000000000ul, 0x00381c0e00000000ul),
            new UInt128(0x0000000000000703ul, 0x81c0000000000000ul),
            new UInt128(0x000000000000381cul, 0x0e00000000000000ul),
            new UInt128(0x000000000001c0e0ul, 0x7000000000000000ul),
        ];

        /// <summary>
        /// 添え字のセルと行、列、ブロックの何れかを同じくするセルの集合。
        /// </summary>
        private static readonly UInt128[] _visibleCellsBitMasksByCellIndex =
        [
            new UInt128(0x0000000000000100ul, 0x80402010081c0ffful), new UInt128(0x0000000000000201ul, 0x00804020101c0ffful), new UInt128(0x0000000000000402ul, 0x01008040201c0ffful), new UInt128(0x0000000000000804ul, 0x0201008040e071fful), new UInt128(0x0000000000001008ul, 0x0402010080e071fful), new UInt128(0x0000000000002010ul, 0x0804020100e071fful), new UInt128(0x0000000000004020ul, 0x10080402070381fful), new UInt128(0x0000000000008040ul, 0x20100804070381fful), new UInt128(0x0000000000010080ul, 0x40201008070381fful),
            new UInt128(0x0000000000000100ul, 0x80402010081ffe07ul), new UInt128(0x0000000000000201ul, 0x00804020101ffe07ul), new UInt128(0x0000000000000402ul, 0x01008040201ffe07ul), new UInt128(0x0000000000000804ul, 0x0201008040e3fe38ul), new UInt128(0x0000000000001008ul, 0x0402010080e3fe38ul), new UInt128(0x0000000000002010ul, 0x0804020100e3fe38ul), new UInt128(0x0000000000004020ul, 0x100804020703ffc0ul), new UInt128(0x0000000000008040ul, 0x201008040703ffc0ul), new UInt128(0x0000000000010080ul, 0x402010080703ffc0ul),
            new UInt128(0x0000000000000100ul, 0x804020100ffc0e07ul), new UInt128(0x0000000000000201ul, 0x0080402017fc0e07ul), new UInt128(0x0000000000000402ul, 0x0100804027fc0e07ul), new UInt128(0x0000000000000804ul, 0x0201008047fc7038ul), new UInt128(0x0000000000001008ul, 0x0402010087fc7038ul), new UInt128(0x0000000000002010ul, 0x0804020107fc7038ul), new UInt128(0x0000000000004020ul, 0x1008040207ff81c0ul), new UInt128(0x0000000000008040ul, 0x2010080407ff81c0ul), new UInt128(0x0000000000010080ul, 0x4020100807ff81c0ul),
            new UInt128(0x0000000000000100ul, 0x8040e07ff8040201ul), new UInt128(0x0000000000000201ul, 0x0080e07ff8080402ul), new UInt128(0x0000000000000402ul, 0x0100e07ff8100804ul), new UInt128(0x0000000000000804ul, 0x0207038ff8201008ul), new UInt128(0x0000000000001008ul, 0x0407038ff8402010ul), new UInt128(0x0000000000002010ul, 0x0807038ff8804020ul), new UInt128(0x0000000000004020ul, 0x10381c0ff9008040ul), new UInt128(0x0000000000008040ul, 0x20381c0ffa010080ul), new UInt128(0x0000000000010080ul, 0x40381c0ffc020100ul),
            new UInt128(0x0000000000000100ul, 0x8040fff038040201ul), new UInt128(0x0000000000000201ul, 0x0080fff038080402ul), new UInt128(0x0000000000000402ul, 0x0100fff038100804ul), new UInt128(0x0000000000000804ul, 0x02071ff1c0201008ul), new UInt128(0x0000000000001008ul, 0x04071ff1c0402010ul), new UInt128(0x0000000000002010ul, 0x08071ff1c0804020ul), new UInt128(0x0000000000004020ul, 0x10381ffe01008040ul), new UInt128(0x0000000000008040ul, 0x20381ffe02010080ul), new UInt128(0x0000000000010080ul, 0x40381ffe04020100ul),
            new UInt128(0x0000000000000100ul, 0x807fe07038040201ul), new UInt128(0x0000000000000201ul, 0x00bfe07038080402ul), new UInt128(0x0000000000000402ul, 0x013fe07038100804ul), new UInt128(0x0000000000000804ul, 0x023fe381c0201008ul), new UInt128(0x0000000000001008ul, 0x043fe381c0402010ul), new UInt128(0x0000000000002010ul, 0x083fe381c0804020ul), new UInt128(0x0000000000004020ul, 0x103ffc0e01008040ul), new UInt128(0x0000000000008040ul, 0x203ffc0e02010080ul), new UInt128(0x0000000000010080ul, 0x403ffc0e04020100ul),
            new UInt128(0x0000000000000703ul, 0xffc0201008040201ul), new UInt128(0x0000000000000703ul, 0xffc0402010080402ul), new UInt128(0x0000000000000703ul, 0xffc0804020100804ul), new UInt128(0x000000000000381cul, 0x7fc1008040201008ul), new UInt128(0x000000000000381cul, 0x7fc2010080402010ul), new UInt128(0x000000000000381cul, 0x7fc4020100804020ul), new UInt128(0x000000000001c0e0ul, 0x7fc8040201008040ul), new UInt128(0x000000000001c0e0ul, 0x7fd0080402010080ul), new UInt128(0x000000000001c0e0ul, 0x7fe0100804020100ul),
            new UInt128(0x00000000000007fful, 0x81c0201008040201ul), new UInt128(0x00000000000007fful, 0x81c0402010080402ul), new UInt128(0x00000000000007fful, 0x81c0804020100804ul), new UInt128(0x00000000000038fful, 0x8e01008040201008ul), new UInt128(0x00000000000038fful, 0x8e02010080402010ul), new UInt128(0x00000000000038fful, 0x8e04020100804020ul), new UInt128(0x000000000001c0fful, 0xf008040201008040ul), new UInt128(0x000000000001c0fful, 0xf010080402010080ul), new UInt128(0x000000000001c0fful, 0xf020100804020100ul),
            new UInt128(0x000000000001ff03ul, 0x81c0201008040201ul), new UInt128(0x000000000001ff03ul, 0x81c0402010080402ul), new UInt128(0x000000000001ff03ul, 0x81c0804020100804ul), new UInt128(0x000000000001ff1cul, 0x0e01008040201008ul), new UInt128(0x000000000001ff1cul, 0x0e02010080402010ul), new UInt128(0x000000000001ff1cul, 0x0e04020100804020ul), new UInt128(0x000000000001ffe0ul, 0x7008040201008040ul), new UInt128(0x000000000001ffe0ul, 0x7010080402010080ul), new UInt128(0x000000000001ffe0ul, 0x7020100804020100ul),
        ];

        /// <summary>
        /// ブロックハウスと重なる列の集合。第一添え字はブロック、第二添え字はブロック内の列のオフセット値。
        /// </summary>
        private static readonly BoardCellColumn[][] _columnsByBlock =
        [
            [BoardCellColumn.One + 0, BoardCellColumn.One + 1, BoardCellColumn.One + 2],
            [BoardCellColumn.One + 3, BoardCellColumn.One + 4, BoardCellColumn.One + 5],
            [BoardCellColumn.One + 6, BoardCellColumn.One + 7, BoardCellColumn.One + 8],
            [BoardCellColumn.One + 0, BoardCellColumn.One + 1, BoardCellColumn.One + 2],
            [BoardCellColumn.One + 3, BoardCellColumn.One + 4, BoardCellColumn.One + 5],
            [BoardCellColumn.One + 6, BoardCellColumn.One + 7, BoardCellColumn.One + 8],
            [BoardCellColumn.One + 0, BoardCellColumn.One + 1, BoardCellColumn.One + 2],
            [BoardCellColumn.One + 3, BoardCellColumn.One + 4, BoardCellColumn.One + 5],
            [BoardCellColumn.One + 6, BoardCellColumn.One + 7, BoardCellColumn.One + 8],
        ];

        /// <summary>
        /// ブロックハウスと重なる行の集合。第一添え字はブロック、第二添え字はブロック内の行のオフセット値。
        /// </summary>
        private static readonly BoardCellRow[][] _rowsByBlock =
        [
            [BoardCellRow.One + 0, BoardCellRow.One + 1, BoardCellRow.One + 2],
            [BoardCellRow.One + 0, BoardCellRow.One + 1, BoardCellRow.One + 2],
            [BoardCellRow.One + 0, BoardCellRow.One + 1, BoardCellRow.One + 2],
            [BoardCellRow.One + 3, BoardCellRow.One + 4, BoardCellRow.One + 5],
            [BoardCellRow.One + 3, BoardCellRow.One + 4, BoardCellRow.One + 5],
            [BoardCellRow.One + 3, BoardCellRow.One + 4, BoardCellRow.One + 5],
            [BoardCellRow.One + 6, BoardCellRow.One + 7, BoardCellRow.One + 8],
            [BoardCellRow.One + 6, BoardCellRow.One + 7, BoardCellRow.One + 8],
            [BoardCellRow.One + 6, BoardCellRow.One + 7, BoardCellRow.One + 8],
        ];

        /// <summary>
        /// 列ハウスと重なるブロックの集合。第一添え字は列、第二添え字は 0 から 2 の整数。
        /// </summary>
        private static readonly BoardCellBlock[][] _blocksByColumn =
        [
            [BoardCellBlock.FirstValue + 0, BoardCellBlock.FirstValue + 3, BoardCellBlock.FirstValue + 6],
            [BoardCellBlock.FirstValue + 0, BoardCellBlock.FirstValue + 3, BoardCellBlock.FirstValue + 6],
            [BoardCellBlock.FirstValue + 0, BoardCellBlock.FirstValue + 3, BoardCellBlock.FirstValue + 6],
            [BoardCellBlock.FirstValue + 1, BoardCellBlock.FirstValue + 4, BoardCellBlock.FirstValue + 7],
            [BoardCellBlock.FirstValue + 1, BoardCellBlock.FirstValue + 4, BoardCellBlock.FirstValue + 7],
            [BoardCellBlock.FirstValue + 1, BoardCellBlock.FirstValue + 4, BoardCellBlock.FirstValue + 7],
            [BoardCellBlock.FirstValue + 2, BoardCellBlock.FirstValue + 5, BoardCellBlock.FirstValue + 8],
            [BoardCellBlock.FirstValue + 2, BoardCellBlock.FirstValue + 5, BoardCellBlock.FirstValue + 8],
            [BoardCellBlock.FirstValue + 2, BoardCellBlock.FirstValue + 5, BoardCellBlock.FirstValue + 8],
        ];

        /// <summary>
        /// 行ハウスと重なるブロックの集合。第一添え字は行、第二添え字は 0 から 2 の整数。
        /// </summary>
        private static readonly BoardCellBlock[][] _blocksByRow =
        [
            [BoardCellBlock.FirstValue + 0, BoardCellBlock.FirstValue + 1, BoardCellBlock.FirstValue + 2],
            [BoardCellBlock.FirstValue + 0, BoardCellBlock.FirstValue + 1, BoardCellBlock.FirstValue + 2],
            [BoardCellBlock.FirstValue + 0, BoardCellBlock.FirstValue + 1, BoardCellBlock.FirstValue + 2],
            [BoardCellBlock.FirstValue + 3, BoardCellBlock.FirstValue + 4, BoardCellBlock.FirstValue + 5],
            [BoardCellBlock.FirstValue + 3, BoardCellBlock.FirstValue + 4, BoardCellBlock.FirstValue + 5],
            [BoardCellBlock.FirstValue + 3, BoardCellBlock.FirstValue + 4, BoardCellBlock.FirstValue + 5],
            [BoardCellBlock.FirstValue + 6, BoardCellBlock.FirstValue + 7, BoardCellBlock.FirstValue + 8],
            [BoardCellBlock.FirstValue + 6, BoardCellBlock.FirstValue + 7, BoardCellBlock.FirstValue + 8],
            [BoardCellBlock.FirstValue + 6, BoardCellBlock.FirstValue + 7, BoardCellBlock.FirstValue + 8],
        ];

        /// <summary>
        /// 行・列からセル番号への変換テーブル。
        /// </summary>
        private static readonly BoardCellIndex[] _cellIndexByRowColumn =
        [
            BoardCellIndex.FirstValue + 0, BoardCellIndex.FirstValue + 1, BoardCellIndex.FirstValue + 2, BoardCellIndex.FirstValue + 3, BoardCellIndex.FirstValue + 4, BoardCellIndex.FirstValue + 5, BoardCellIndex.FirstValue + 6, BoardCellIndex.FirstValue + 7, BoardCellIndex.FirstValue + 8, BoardCellIndex.Undefined, BoardCellIndex.Undefined, BoardCellIndex.Undefined, BoardCellIndex.Undefined, BoardCellIndex.Undefined, BoardCellIndex.Undefined, BoardCellIndex.Undefined,
            BoardCellIndex.FirstValue + 9, BoardCellIndex.FirstValue + 10, BoardCellIndex.FirstValue + 11, BoardCellIndex.FirstValue + 12, BoardCellIndex.FirstValue + 13, BoardCellIndex.FirstValue + 14, BoardCellIndex.FirstValue + 15, BoardCellIndex.FirstValue + 16, BoardCellIndex.FirstValue + 17, BoardCellIndex.Undefined, BoardCellIndex.Undefined, BoardCellIndex.Undefined, BoardCellIndex.Undefined, BoardCellIndex.Undefined, BoardCellIndex.Undefined, BoardCellIndex.Undefined,
            BoardCellIndex.FirstValue + 18, BoardCellIndex.FirstValue + 19, BoardCellIndex.FirstValue + 20, BoardCellIndex.FirstValue + 21, BoardCellIndex.FirstValue + 22, BoardCellIndex.FirstValue + 23, BoardCellIndex.FirstValue + 24, BoardCellIndex.FirstValue + 25, BoardCellIndex.FirstValue + 26, BoardCellIndex.Undefined, BoardCellIndex.Undefined, BoardCellIndex.Undefined, BoardCellIndex.Undefined, BoardCellIndex.Undefined, BoardCellIndex.Undefined, BoardCellIndex.Undefined,
            BoardCellIndex.FirstValue + 27, BoardCellIndex.FirstValue + 28, BoardCellIndex.FirstValue + 29, BoardCellIndex.FirstValue + 30, BoardCellIndex.FirstValue + 31, BoardCellIndex.FirstValue + 32, BoardCellIndex.FirstValue + 33, BoardCellIndex.FirstValue + 34, BoardCellIndex.FirstValue + 35, BoardCellIndex.Undefined, BoardCellIndex.Undefined, BoardCellIndex.Undefined, BoardCellIndex.Undefined, BoardCellIndex.Undefined, BoardCellIndex.Undefined, BoardCellIndex.Undefined,
            BoardCellIndex.FirstValue + 36, BoardCellIndex.FirstValue + 37, BoardCellIndex.FirstValue + 38, BoardCellIndex.FirstValue + 39, BoardCellIndex.FirstValue + 40, BoardCellIndex.FirstValue + 41, BoardCellIndex.FirstValue + 42, BoardCellIndex.FirstValue + 43, BoardCellIndex.FirstValue + 44, BoardCellIndex.Undefined, BoardCellIndex.Undefined, BoardCellIndex.Undefined, BoardCellIndex.Undefined, BoardCellIndex.Undefined, BoardCellIndex.Undefined, BoardCellIndex.Undefined,
            BoardCellIndex.FirstValue + 45, BoardCellIndex.FirstValue + 46, BoardCellIndex.FirstValue + 47, BoardCellIndex.FirstValue + 48, BoardCellIndex.FirstValue + 49, BoardCellIndex.FirstValue + 50, BoardCellIndex.FirstValue + 51, BoardCellIndex.FirstValue + 52, BoardCellIndex.FirstValue + 53, BoardCellIndex.Undefined, BoardCellIndex.Undefined, BoardCellIndex.Undefined, BoardCellIndex.Undefined, BoardCellIndex.Undefined, BoardCellIndex.Undefined, BoardCellIndex.Undefined,
            BoardCellIndex.FirstValue + 54, BoardCellIndex.FirstValue + 55, BoardCellIndex.FirstValue + 56, BoardCellIndex.FirstValue + 57, BoardCellIndex.FirstValue + 58, BoardCellIndex.FirstValue + 59, BoardCellIndex.FirstValue + 60, BoardCellIndex.FirstValue + 61, BoardCellIndex.FirstValue + 62, BoardCellIndex.Undefined, BoardCellIndex.Undefined, BoardCellIndex.Undefined, BoardCellIndex.Undefined, BoardCellIndex.Undefined, BoardCellIndex.Undefined, BoardCellIndex.Undefined,
            BoardCellIndex.FirstValue + 63, BoardCellIndex.FirstValue + 64, BoardCellIndex.FirstValue + 65, BoardCellIndex.FirstValue + 66, BoardCellIndex.FirstValue + 67, BoardCellIndex.FirstValue + 68, BoardCellIndex.FirstValue + 69, BoardCellIndex.FirstValue + 70, BoardCellIndex.FirstValue + 71, BoardCellIndex.Undefined, BoardCellIndex.Undefined, BoardCellIndex.Undefined, BoardCellIndex.Undefined, BoardCellIndex.Undefined, BoardCellIndex.Undefined, BoardCellIndex.Undefined,
            BoardCellIndex.FirstValue + 72, BoardCellIndex.FirstValue + 73, BoardCellIndex.FirstValue + 74, BoardCellIndex.FirstValue + 75, BoardCellIndex.FirstValue + 76, BoardCellIndex.FirstValue + 77, BoardCellIndex.FirstValue + 78, BoardCellIndex.FirstValue + 79, BoardCellIndex.FirstValue + 80, BoardCellIndex.Undefined, BoardCellIndex.Undefined, BoardCellIndex.Undefined, BoardCellIndex.Undefined, BoardCellIndex.Undefined, BoardCellIndex.Undefined, BoardCellIndex.Undefined,
            BoardCellIndex.Undefined, BoardCellIndex.Undefined, BoardCellIndex.Undefined, BoardCellIndex.Undefined, BoardCellIndex.Undefined, BoardCellIndex.Undefined, BoardCellIndex.Undefined, BoardCellIndex.Undefined, BoardCellIndex.Undefined, BoardCellIndex.Undefined, BoardCellIndex.Undefined, BoardCellIndex.Undefined, BoardCellIndex.Undefined, BoardCellIndex.Undefined, BoardCellIndex.Undefined, BoardCellIndex.Undefined,
            BoardCellIndex.Undefined, BoardCellIndex.Undefined, BoardCellIndex.Undefined, BoardCellIndex.Undefined, BoardCellIndex.Undefined, BoardCellIndex.Undefined, BoardCellIndex.Undefined, BoardCellIndex.Undefined, BoardCellIndex.Undefined, BoardCellIndex.Undefined, BoardCellIndex.Undefined, BoardCellIndex.Undefined, BoardCellIndex.Undefined, BoardCellIndex.Undefined, BoardCellIndex.Undefined, BoardCellIndex.Undefined,
            BoardCellIndex.Undefined, BoardCellIndex.Undefined, BoardCellIndex.Undefined, BoardCellIndex.Undefined, BoardCellIndex.Undefined, BoardCellIndex.Undefined, BoardCellIndex.Undefined, BoardCellIndex.Undefined, BoardCellIndex.Undefined, BoardCellIndex.Undefined, BoardCellIndex.Undefined, BoardCellIndex.Undefined, BoardCellIndex.Undefined, BoardCellIndex.Undefined, BoardCellIndex.Undefined, BoardCellIndex.Undefined,
            BoardCellIndex.Undefined, BoardCellIndex.Undefined, BoardCellIndex.Undefined, BoardCellIndex.Undefined, BoardCellIndex.Undefined, BoardCellIndex.Undefined, BoardCellIndex.Undefined, BoardCellIndex.Undefined, BoardCellIndex.Undefined, BoardCellIndex.Undefined, BoardCellIndex.Undefined, BoardCellIndex.Undefined, BoardCellIndex.Undefined, BoardCellIndex.Undefined, BoardCellIndex.Undefined, BoardCellIndex.Undefined,
            BoardCellIndex.Undefined, BoardCellIndex.Undefined, BoardCellIndex.Undefined, BoardCellIndex.Undefined, BoardCellIndex.Undefined, BoardCellIndex.Undefined, BoardCellIndex.Undefined, BoardCellIndex.Undefined, BoardCellIndex.Undefined, BoardCellIndex.Undefined, BoardCellIndex.Undefined, BoardCellIndex.Undefined, BoardCellIndex.Undefined, BoardCellIndex.Undefined, BoardCellIndex.Undefined, BoardCellIndex.Undefined,
            BoardCellIndex.Undefined, BoardCellIndex.Undefined, BoardCellIndex.Undefined, BoardCellIndex.Undefined, BoardCellIndex.Undefined, BoardCellIndex.Undefined, BoardCellIndex.Undefined, BoardCellIndex.Undefined, BoardCellIndex.Undefined, BoardCellIndex.Undefined, BoardCellIndex.Undefined, BoardCellIndex.Undefined, BoardCellIndex.Undefined, BoardCellIndex.Undefined, BoardCellIndex.Undefined, BoardCellIndex.Undefined,
            BoardCellIndex.Undefined, BoardCellIndex.Undefined, BoardCellIndex.Undefined, BoardCellIndex.Undefined, BoardCellIndex.Undefined, BoardCellIndex.Undefined, BoardCellIndex.Undefined, BoardCellIndex.Undefined, BoardCellIndex.Undefined, BoardCellIndex.Undefined, BoardCellIndex.Undefined, BoardCellIndex.Undefined, BoardCellIndex.Undefined, BoardCellIndex.Undefined, BoardCellIndex.Undefined, BoardCellIndex.Undefined,
        ];

        /// <summary>
        /// セル番号から列・行・ブロックへの変換テーブル。
        /// </summary>
        private static readonly (BoardCellColumn column, BoardCellRow row, BoardCellBlock block)[] _columnRowBlockByCellIndex =
        [
            (BoardCellColumn.One + 0, BoardCellRow.One + 0, BoardCellBlock.FirstValue + 0), (BoardCellColumn.One + 1, BoardCellRow.One + 0, BoardCellBlock.FirstValue + 0), (BoardCellColumn.One + 2, BoardCellRow.One + 0, BoardCellBlock.FirstValue + 0), (BoardCellColumn.One + 3, BoardCellRow.One + 0, BoardCellBlock.FirstValue + 1), (BoardCellColumn.One + 4, BoardCellRow.One + 0, BoardCellBlock.FirstValue + 1), (BoardCellColumn.One + 5, BoardCellRow.One + 0, BoardCellBlock.FirstValue + 1), (BoardCellColumn.One + 6, BoardCellRow.One + 0, BoardCellBlock.FirstValue + 2), (BoardCellColumn.One + 7, BoardCellRow.One + 0, BoardCellBlock.FirstValue + 2), (BoardCellColumn.One + 8, BoardCellRow.One + 0, BoardCellBlock.FirstValue + 2),
            (BoardCellColumn.One + 0, BoardCellRow.One + 1, BoardCellBlock.FirstValue + 0), (BoardCellColumn.One + 1, BoardCellRow.One + 1, BoardCellBlock.FirstValue + 0), (BoardCellColumn.One + 2, BoardCellRow.One + 1, BoardCellBlock.FirstValue + 0), (BoardCellColumn.One + 3, BoardCellRow.One + 1, BoardCellBlock.FirstValue + 1), (BoardCellColumn.One + 4, BoardCellRow.One + 1, BoardCellBlock.FirstValue + 1), (BoardCellColumn.One + 5, BoardCellRow.One + 1, BoardCellBlock.FirstValue + 1), (BoardCellColumn.One + 6, BoardCellRow.One + 1, BoardCellBlock.FirstValue + 2), (BoardCellColumn.One + 7, BoardCellRow.One + 1, BoardCellBlock.FirstValue + 2), (BoardCellColumn.One + 8, BoardCellRow.One + 1, BoardCellBlock.FirstValue + 2),
            (BoardCellColumn.One + 0, BoardCellRow.One + 2, BoardCellBlock.FirstValue + 0), (BoardCellColumn.One + 1, BoardCellRow.One + 2, BoardCellBlock.FirstValue + 0), (BoardCellColumn.One + 2, BoardCellRow.One + 2, BoardCellBlock.FirstValue + 0), (BoardCellColumn.One + 3, BoardCellRow.One + 2, BoardCellBlock.FirstValue + 1), (BoardCellColumn.One + 4, BoardCellRow.One + 2, BoardCellBlock.FirstValue + 1), (BoardCellColumn.One + 5, BoardCellRow.One + 2, BoardCellBlock.FirstValue + 1), (BoardCellColumn.One + 6, BoardCellRow.One + 2, BoardCellBlock.FirstValue + 2), (BoardCellColumn.One + 7, BoardCellRow.One + 2, BoardCellBlock.FirstValue + 2), (BoardCellColumn.One + 8, BoardCellRow.One + 2, BoardCellBlock.FirstValue + 2),
            (BoardCellColumn.One + 0, BoardCellRow.One + 3, BoardCellBlock.FirstValue + 3), (BoardCellColumn.One + 1, BoardCellRow.One + 3, BoardCellBlock.FirstValue + 3), (BoardCellColumn.One + 2, BoardCellRow.One + 3, BoardCellBlock.FirstValue + 3), (BoardCellColumn.One + 3, BoardCellRow.One + 3, BoardCellBlock.FirstValue + 4), (BoardCellColumn.One + 4, BoardCellRow.One + 3, BoardCellBlock.FirstValue + 4), (BoardCellColumn.One + 5, BoardCellRow.One + 3, BoardCellBlock.FirstValue + 4), (BoardCellColumn.One + 6, BoardCellRow.One + 3, BoardCellBlock.FirstValue + 5), (BoardCellColumn.One + 7, BoardCellRow.One + 3, BoardCellBlock.FirstValue + 5), (BoardCellColumn.One + 8, BoardCellRow.One + 3, BoardCellBlock.FirstValue + 5),
            (BoardCellColumn.One + 0, BoardCellRow.One + 4, BoardCellBlock.FirstValue + 3), (BoardCellColumn.One + 1, BoardCellRow.One + 4, BoardCellBlock.FirstValue + 3), (BoardCellColumn.One + 2, BoardCellRow.One + 4, BoardCellBlock.FirstValue + 3), (BoardCellColumn.One + 3, BoardCellRow.One + 4, BoardCellBlock.FirstValue + 4), (BoardCellColumn.One + 4, BoardCellRow.One + 4, BoardCellBlock.FirstValue + 4), (BoardCellColumn.One + 5, BoardCellRow.One + 4, BoardCellBlock.FirstValue + 4), (BoardCellColumn.One + 6, BoardCellRow.One + 4, BoardCellBlock.FirstValue + 5), (BoardCellColumn.One + 7, BoardCellRow.One + 4, BoardCellBlock.FirstValue + 5), (BoardCellColumn.One + 8, BoardCellRow.One + 4, BoardCellBlock.FirstValue + 5),
            (BoardCellColumn.One + 0, BoardCellRow.One + 5, BoardCellBlock.FirstValue + 3), (BoardCellColumn.One + 1, BoardCellRow.One + 5, BoardCellBlock.FirstValue + 3), (BoardCellColumn.One + 2, BoardCellRow.One + 5, BoardCellBlock.FirstValue + 3), (BoardCellColumn.One + 3, BoardCellRow.One + 5, BoardCellBlock.FirstValue + 4), (BoardCellColumn.One + 4, BoardCellRow.One + 5, BoardCellBlock.FirstValue + 4), (BoardCellColumn.One + 5, BoardCellRow.One + 5, BoardCellBlock.FirstValue + 4), (BoardCellColumn.One + 6, BoardCellRow.One + 5, BoardCellBlock.FirstValue + 5), (BoardCellColumn.One + 7, BoardCellRow.One + 5, BoardCellBlock.FirstValue + 5), (BoardCellColumn.One + 8, BoardCellRow.One + 5, BoardCellBlock.FirstValue + 5),
            (BoardCellColumn.One + 0, BoardCellRow.One + 6, BoardCellBlock.FirstValue + 6), (BoardCellColumn.One + 1, BoardCellRow.One + 6, BoardCellBlock.FirstValue + 6), (BoardCellColumn.One + 2, BoardCellRow.One + 6, BoardCellBlock.FirstValue + 6), (BoardCellColumn.One + 3, BoardCellRow.One + 6, BoardCellBlock.FirstValue + 7), (BoardCellColumn.One + 4, BoardCellRow.One + 6, BoardCellBlock.FirstValue + 7), (BoardCellColumn.One + 5, BoardCellRow.One + 6, BoardCellBlock.FirstValue + 7), (BoardCellColumn.One + 6, BoardCellRow.One + 6, BoardCellBlock.FirstValue + 8), (BoardCellColumn.One + 7, BoardCellRow.One + 6, BoardCellBlock.FirstValue + 8), (BoardCellColumn.One + 8, BoardCellRow.One + 6, BoardCellBlock.FirstValue + 8),
            (BoardCellColumn.One + 0, BoardCellRow.One + 7, BoardCellBlock.FirstValue + 6), (BoardCellColumn.One + 1, BoardCellRow.One + 7, BoardCellBlock.FirstValue + 6), (BoardCellColumn.One + 2, BoardCellRow.One + 7, BoardCellBlock.FirstValue + 6), (BoardCellColumn.One + 3, BoardCellRow.One + 7, BoardCellBlock.FirstValue + 7), (BoardCellColumn.One + 4, BoardCellRow.One + 7, BoardCellBlock.FirstValue + 7), (BoardCellColumn.One + 5, BoardCellRow.One + 7, BoardCellBlock.FirstValue + 7), (BoardCellColumn.One + 6, BoardCellRow.One + 7, BoardCellBlock.FirstValue + 8), (BoardCellColumn.One + 7, BoardCellRow.One + 7, BoardCellBlock.FirstValue + 8), (BoardCellColumn.One + 8, BoardCellRow.One + 7, BoardCellBlock.FirstValue + 8),
            (BoardCellColumn.One + 0, BoardCellRow.One + 8, BoardCellBlock.FirstValue + 6), (BoardCellColumn.One + 1, BoardCellRow.One + 8, BoardCellBlock.FirstValue + 6), (BoardCellColumn.One + 2, BoardCellRow.One + 8, BoardCellBlock.FirstValue + 6), (BoardCellColumn.One + 3, BoardCellRow.One + 8, BoardCellBlock.FirstValue + 7), (BoardCellColumn.One + 4, BoardCellRow.One + 8, BoardCellBlock.FirstValue + 7), (BoardCellColumn.One + 5, BoardCellRow.One + 8, BoardCellBlock.FirstValue + 7), (BoardCellColumn.One + 6, BoardCellRow.One + 8, BoardCellBlock.FirstValue + 8), (BoardCellColumn.One + 7, BoardCellRow.One + 8, BoardCellBlock.FirstValue + 8), (BoardCellColumn.One + 8, BoardCellRow.One + 8, BoardCellBlock.FirstValue + 8),
        ];
    }
}
