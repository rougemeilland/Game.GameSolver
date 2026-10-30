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

        #region Full House

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

        #region Hidden Pair

        private const string sourceArray_HiddenPair1 =
            "000009021" +
            "000000038" +
            "030004056" +
            "942513687" +
            "785926314" +
            "163748295" +
            "316897542" +
            "000462173" +
            "427000869";

        private const string sourceArray_HiddenPair2 =
            "070051089" +
            "089000013" +
            "001000207" +
            "800600901" +
            "095130800" +
            "100000300" +
            "010980736" +
            "768010090" +
            "900006108";

        private const string sourceArray_HiddenPair3 =
            "730600045" +
            "006047230" +
            "080000671" +
            "067000000" +
            "000080006" +
            "300206090" +
            "600010007" +
            "003002060" +
            "000065080";

        #endregion

        #region Hidden Triple

        // 別の定石により 8行1列 と 8行3列 の候補から 2, 3 を排除しておかないと隠れ三国同盟は動作しない
        private const string sourceArray_HiddenTriple1 =
            "460591820" +
            "005000410" +
            "810047065" +
            "070435198" +
            "100700530" +
            "358010074" +
            "901802700" +
            "080100000" +
            "500070081";

        private const string sourceArray_HiddenTriple2 =
            "350000041" +
            "460000800" +
            "128940700" +
            "283400597" +
            "705300610" +
            "601700080" +
            "970100008" +
            "010070020" +
            "032600170";

        private const string sourceArray_HiddenTriple3 =
            "040050029" +
            "000009604" +
            "100640800" +
            "020080001" +
            "700430080" +
            "004006000" +
            "457190000" +
            "003060070" +
            "000070000";

        #endregion

        #region Hidden Quad

        private const string sourceArray_HiddenQuad1 =
            "875136294" +
            "002805010" +
            "000020538" +
            "900501400" +
            "721000000" +
            "540203100" +
            "000000000" +
            "680019000" +
            "000600801";

        private const string sourceArray_HiddenQuad2 =
            "460017800" +
            "010006040" +
            "879452300" +
            "000769500" +
            "200548000" +
            "700231400" +
            "600104200" +
            "020603180" +
            "000025630";

        private const string sourceArray_HiddenQuad3 =
            "064208000" +
            "039417000" +
            "081506000" +
            "156842379" +
            "372159846" +
            "498763000" +
            "843625917" +
            "905370000" +
            "607980530";

        #endregion

        #region X-Wing

        private const string sourceArray_XWing1 =
            "261897534" +
            "007300060" +
            "300600700" +
            "126479385" +
            "400158627" +
            "785236149" +
            "000704006" +
            "670901453" +
            "900563270";

        private const string sourceArray_XWing2 =
            "360701090" +
            "754692813" +
            "019003070" +
            "437206109" +
            "590074030" +
            "080039407" +
            "123968745" +
            "970415320" +
            "045327901";

        private const string sourceArray_Extra1 =
            "153798426" +
            "600030000" +
            "840256103" +
            "700643985" +
            "538917264" +
            "496000731" +
            "005000610" +
            "060070000" +
            "304060009";

        private const string sourceArray_Extra2 =
            "300457861" +
            "060930405" +
            "405680003" +
            "000096207" +
            "670014539" +
            "000073608" +
            "006729104" +
            "001368002" +
            "900145306";

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

        private const string sourceArray_SueDeCoq6 =
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
        private const string sourceArray_SueDeCoq7 =
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
                sourceArray_Extra2
                .Select(c => c is < '0' or > '9' ? throw new ApplicationException() : c == '0' ? string.Empty : c.ToString())
            ];
    }
}
