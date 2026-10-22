using System.Globalization;
using System.Linq;
using System;

namespace Game.SudokuSolver.GUI.Models
{
    internal partial class MainWindowModel
    {
        private const string sourceArray_Beginner =
            "019062053" +
            "547038629" +
            "030975418" +
            "085390201" +
            "021084070" +
            "704206380" +
            "402859037" +
            "178043592" +
            "950021864";

        private const string sourceArray_Fails =
            "000000100" +
            "000000200" +
            "000000300" +
            "123456089" +
            "000000500" +
            "000000600" +
            "000000700" +
            "000000800" +
            "000000900";

        private const string sourceArray_Fullhouse =
            "170906500" +
            "059170060" +
            "046050179" +
            "630090715" +
            "010307986" +
            "798561000" +
            "080709640" +
            "907630851" +
            "060005097";

        private const string sourceArray_NakedSingle1 =
            "080043170" +
            "160007053" +
            "070851090" +
            "936005741" +
            "400070628" +
            "027400935" +
            "000734010" +
            "300100067" +
            "001560080";

        private const string sourceArray_NakedSingle2 =
            "987010200" +
            "102800900" +
            "504090803" +
            "678020531" +
            "351060492" +
            "429050687" +
            "700080309" +
            "005004708" +
            "000070125";

        private const string sourceArray_NakedSingle3 =
            "300050200" +
            "009200810" +
            "008030500" +
            "000000000" +
            "200060071" +
            "074080000" +
            "000007900" +
            "900000145" +
            "005000008";

        private static string[] CreateInitialBoard()
            => [..
                sourceArray_NakedSingle3
                .Select(c => c is < '0' or > '9' ? throw new ApplicationException() : c == '0' ? string.Empty : c.ToString())
            ];
    }
}
