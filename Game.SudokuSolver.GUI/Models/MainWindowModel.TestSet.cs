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

        private const string sourceArray_Hard =
            "600002004" +
            "001647800" +
            "024050076" +
            "010506040" +
            "402000701" +
            "030401090" +
            "240080010" +
            "103205407" +
            "500004009";

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

        private const string sourceArray_NakedSingle4 =
            "200001008" +
            "001900400" +
            "060030005" +
            "430700000" +
            "000000500" +
            "007000104" +
            "000000700" +
            "900064000" +
            "000000962";

        // '6' と '7' を交換済み
        private const string sourceArray_HiddenSingle1 =
            "207010006" +
            "586002001" +
            "103500807" +
            "358974002" +
            "629381754" +
            "714256389" +
            "401003905" +
            "802600043" +
            "935040208";

        // '1' と '8' を交換済み
        private const string sourceArray_HiddenSingle2 =
            "007023564" +
            "001000792" +
            "065000318" +
            "603004907" +
            "798536241" +
            "104900600" +
            "576000420" +
            "809000100" +
            "012750800";

        // '3' と '9' を交換済み
        private const string sourceArray_HiddenSingle3 =
            "206010000" +
            "580002001" +
            "109500806" +
            "058364000" +
            "723981654" +
            "000257980" +
            "401009305" +
            "800700049" +
            "000040208";

        private const string sourceArray_HiddenSingle4_5 =
            "100046098" +
            "870000004" +
            "000830007" +
            "300051000" +
            "400600000" +
            "000400050" +
            "900020076" +
            "702000010" +
            "560070040";

        private const string sourceArray_HiddenSingle6 =
            "123746598" +
            "870012634" +
            "600830127" +
            "387251469" +
            "400690080" +
            "206480050" +
            "900020076" +
            "702060015" +
            "560070040";

        private static string[] CreateInitialBoard()
            => [..
                sourceArray_HiddenSingle6
                .Select(c => c is < '0' or > '9' ? throw new ApplicationException() : c == '0' ? string.Empty : c.ToString())
            ];
    }
}
