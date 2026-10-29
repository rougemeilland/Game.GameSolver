using System;
using System.Linq;

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

        #region Fullhouse

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

        #endregion

        #region Naked Single

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

        #endregion

        #region Hidden Single

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

        #endregion

        #region Pointing

        private const string sourceArray_Pointing1 =
            "005078069" +
            "843569712" +
            "769200005" +
            "501607200" +
            "000900507" +
            "000052601" +
            "452896173" +
            "008724956" +
            "000005020";

        private const string sourceArray_Pointing2 =
            "005078069" +
            "843569712" +
            "769200005" +
            "501607200" +
            "000900507" +
            "000052601" +
            "452896173" +
            "008724956" +
            "000135020";

        #endregion

        #region Box Line Reduction

        private const string sourceArray_BoxLineReduction1 =
            "305100709" +
            "980057100" +
            "176000400" +
            "003700501" +
            "500016070" +
            "701000040" +
            "610070000" +
            "057901600" +
            "239000817";

        private const string sourceArray_BoxLineReduction2 =
            "000007009" +
            "980100047" +
            "467589132" +
            "000760090" +
            "800290073" +
            "079804001" +
            "103002700" +
            "008370910" +
            "700018320";

        #endregion

        #region Naked Pair

        private const string sourceArray_NakedPair1 =
            "000008510" +
            "180052306" +
            "000091208" +
            "000003080" +
            "348120605" +
            "090805030" +
            "800014763" +
            "064030821" +
            "000086954";
        private const string sourceArray_NakedPair2_3 =
            "189004673" +
            "752361004" +
            "436009125" +
            "397015400" +
            "210093750" +
            "560000300" +
            "641002530" +
            "803006000" +
            "905030000";

        private const string sourceArray_NakedPair4 =
            "030000200" +
            "842513697" +
            "150602300" +
            "061287930" +
            "080139026" +
            "293456700" +
            "025301879" +
            "070025103" +
            "310700002";

        private const string sourceArray_NakedPair5 =
            "940710006" +
            "050000009" +
            "000905002" +
            "368109007" +
            "721068090" +
            "594070618" +
            "480020970" +
            "279800060" +
            "005097004";

        #endregion

        #region Naked Triple

        private const string sourceArray_NakedTriple1 =
            "060000325" +
            "300001796" +
            "590030814" +
            "604093102" +
            "010002403" +
            "230000908" +
            "076300049" +
            "020004637" +
            "003000081";

        private const string sourceArray_NakedTriple2 =
            "504920067" +
            "000040052" +
            "602750894" +
            "200300475" +
            "000072610" +
            "000014230" +
            "006235981" +
            "329187546" +
            "005400723";

        private const string sourceArray_NakedTriple3 =
            "200000680" +
            "389260700" +
            "065080209" +
            "602090408" +
            "008604302" +
            "700802960" +
            "801040526" +
            "506008190" +
            "000006800";

        private const string sourceArray_NakedTriple4 =
            "340258100" +
            "018349500" +
            "000716000" +
            "080027490" +
            "000405001" +
            "400003600" +
            "134902000" +
            "960001300" +
            "800034010";

        private const string sourceArray_NakedTriple5 =
            "000809400" +
            "480716250" +
            "000405000" +
            "126950007" +
            "000273060" +
            "007601092" +
            "092307000" +
            "000500720" +
            "005102000";

        #endregion

        #region Naked Quad

        private const string sourceArray_NakedQuad1 =
            "200608579" +
            "809507040" +
            "057094000" +
            "005000000" +
            "000081005" +
            "908450700" +
            "782345691" +
            "403809257" +
            "590070080";

        private const string sourceArray_NakedQuad2 =
            "050000908" +
            "930500072" +
            "068000051" +
            "000050003" +
            "000076040" +
            "516000007" +
            "000410006" +
            "005600704" +
            "640730080";

        private const string sourceArray_NakedQuad3 =
            "590007800" +
            "072050090" +
            "030690000" +
            "000760045" +
            "709005682" +
            "065080030" +
            "080900000" +
            "900076000" +
            "000021060";

        #endregion

        #region Sue de Coq

        private const string sourceArray_SueDeCoq1 =
            "352748169" +
            "146529387" +
            "789316524" +
            "815463972" +
            "427895631" +
            "693172000" +
            "500080203" +
            "230051700" +
            "900230000";

        private const string sourceArray_SueDeCoq2 =
            "008001703" +
            "741935682" +
            "230867000" +
            "010504230" +
            "007306004" +
            "403000000" +
            "302600500" +
            "100700028" +
            "070000069";

        private const string sourceArray_SueDeCoq3 =
            "203075009" +
            "065030047" +
            "700006053" +
            "600053900" +
            "394018070" +
            "500700000" +
            "800002090" +
            "006000700" +
            "030007006";

        private const string sourceArray_SueDeCoq4 =
            "136000289" +
            "872369514" +
            "459812736" +
            "945623178" +
            "761985423" +
            "328741965" +
            "280000600" +
            "500206800" +
            "607008302";

        private const string sourceArray_SueDeCoq5 =
            "002630749" +
            "070240831" +
            "300710652" +
            "090357128" +
            "728164395" +
            "000982476" +
            "261573984" +
            "007891263" +
            "000426517";

        private const string sourceArray_SueDeCoq7 =
            "238641759" +
            "154729863" +
            "679000412" +
            "461000238" +
            "825463971" +
            "793182546" +
            "506000107" +
            "907006304" +
            "302000605";

        // 別の定石で 1行9列 の数字の候補 4 をあらかじめ消しておかないと Su de Coq が適用されない。
        private const string sourceArray_SueDeCoq8 =
            "001000000" +
            "000140580" +
            "050000070" +
            "038700025" +
            "006000897" +
            "090000403" +
            "004005000" +
            "000920050" +
            "060003000";

        #endregion

        private static string[] CreateInitialBoard()
            => [..
                sourceArray_Hard
                .Select(c => c is < '0' or > '9' ? throw new ApplicationException() : c == '0' ? string.Empty : c.ToString())
            ];
    }
}
