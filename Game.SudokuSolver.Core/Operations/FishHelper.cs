using System;
using System.Linq;

namespace Game.SudokuSolver.Operations
{
    /// <summary>
    /// フィッシュのヘルパークラスです。
    /// </summary>
    internal static class FishHelper
    {
        /// <summary>
        /// フィン付きフィッシュの説明テキストを構築します。
        /// </summary>
        /// <param name="digitText">
        /// 対象の数字を示す <see cref="string"/> オブジェクトです。
        /// </param>
        /// <param name="baseLineHouseTypeName">
        /// ベースラインを示す <see cref="string"/> オブジェクトです。<paramref name="baseLineHouseTypeName"/> は通常 "行" または "列" の値をとります。
        /// </param>
        /// <param name="coverLineHouseTypeName">
        /// カバーラインを示す <see cref="string"/> オブジェクトです。<paramref name="coverLineHouseTypeName"/> は通常 "行" または "列" の値をとります。
        /// </param>
        /// <param name="baseLineHouseNamesText">
        /// 全てのベースラインのハウス名を ',' (カンマ) で区切った文字列を示す <see cref="string"/> オブジェクトです。
        /// </param>
        /// <param name="coverLineHouseNamesText">
        /// 全てのカバーラインのハウス名を ',' (カンマ) で区切った文字列を示す <see cref="string"/> オブジェクトです。
        /// </param>
        /// <param name="finCellIndexBitSet">
        /// フィンのセルの位置のビット集合を示す <see cref="UInt128"/> 値です。
        /// </param>
        /// <param name="finnedBaseLineHouseName">
        /// フィンが存在するベースラインハウスの名前を示す <see cref="string"/> オブジェクトです。
        /// </param>
        /// <param name="finCellPositionsText">
        /// フィンのセルの位置を ',' (カンマ) で区切った文字列を示す <see cref="string"/> オブジェクトです。
        /// </param>
        /// <param name="finBlockHouseName">
        /// フィンが存在するブロックを示す <see cref="SudokuBoardCellBlock"/> 値です。
        /// </param>
        /// <param name="fishName">
        /// フィッシュの名前を示す <see cref="string"/> オブジェクトです。
        /// <list type="bullet">
        /// <item>
        /// <term>例</term>
        /// <description>ジェリーフィッシュ</description>
        /// </item>
        /// </list>
        /// </param>
        /// <param name="finnedFishName">
        /// フィン付きフィッシュの名前を示す <see cref="string"/> オブジェクトです。
        /// <list type="bullet">
        /// <item>
        /// <term>例</term>
        /// <description>フィン付きジェリーフィッシュ</description>
        /// </item>
        /// </list>
        /// </param>
        /// <param name="sashimiFishName">
        /// サシミフィッシュの名前を示す <see cref="string"/> オブジェクトです。
        /// <list type="bullet">
        /// <item>
        /// <term>例</term>
        /// <description>サシミジェリーフィッシュ</description>
        /// </item>
        /// </list>
        /// </param>
        /// <param name="isDegeneratable">
        /// フィッシュが退化可能かどうかを示す <see cref="bool"/> 値です。
        /// フィッシュが退化可能である場合は <see langword="true"/>、そうではない場合は <see langword="false"/> です。
        /// </param>
        /// <param name="sashimi">
        /// フィッシュの退化状態を示す <see cref="SashimiFishDegeneratable"/> オブジェクトです。
        /// </param>
        /// <param name="isolatedCellHouseName">
        /// 単独のマスを持つベースラインまたはカバーラインのハウス名を示す <see cref="string"/> オブジェクトです。
        /// <paramref name="isDegeneratable"/> が <see langword="false"/> である場合は無視されます。
        /// </param>
        /// <param name="effectecdByIsolatedCellHouseNamesText">
        /// ベースラインまたはカバーライン上の単独のマスによって影響を受けるカバーラインまたはベースラインのハウス名を示す <see cref="string"/> オブジェクトです。
        /// <paramref name="isDegeneratable"/> が <see langword="false"/> である場合は無視されます。
        /// </param>
        /// <returns>
        /// 構築された説明テキストを示す <see cref="string"/> オブジェクトです。
        /// </returns>
        /// <exception cref="ApplicationException"></exception>
        public static string FinnedFishDescriptionBuilder(
            string digitText,
            string baseLineHouseTypeName,
            string coverLineHouseTypeName,
            string baseLineHouseNamesText,
            string coverLineHouseNamesText,
            UInt128 finCellIndexBitSet,
            string finnedBaseLineHouseName,
            string finCellPositionsText,
            string finBlockHouseName,
            string fishName,
            string finnedFishName,
            string sashimiFishName,
            bool isDegeneratable,
            SashimiFishDegeneratable sashimi,
            string isolatedCellHouseName,
            string effectecdByIsolatedCellHouseNamesText)
        {
            if (isDegeneratable)
            {
                // フィッシュが退化可能である場合 (サシミフィッシュ)

                var (degenerationMessages, undegeneratedBaseLineHouseNames, undegeneratedCoverLineHouseNames, determinedCellIndexBitSet) =
                    sashimi.Degenerate(
                        (isFirstDegeneration, baseLineHouseNames, coverLineHouseNames, baseHouseName, effectedCoverHouseName, cellPositionText) =>
                        {
#if DEBUG
                            System.Diagnostics.Debug.Assert(baseLineHouseNames.Length > 0);
                            System.Diagnostics.Debug.Assert(coverLineHouseNames.Length > 0);
                            System.Diagnostics.Debug.Assert(baseLineHouseNames.Length == coverLineHouseNames.Length);
#endif
                            if (isFirstDegeneration)
                            {
                                return
                                    $"{baseHouseName}で {digitText} が入り得るマスは{cellPositionText}だけなので{cellPositionText}に {digitText} が入ります。" +
                                    $"したがって、{cellPositionText}以外の{effectedCoverHouseName}のマスに {digitText} は入り得ません。";
                            }
                            else if (baseLineHouseNames.Length > 1)
                            {
                                var baseLineHouseNamesText = string.Join(", ", baseLineHouseNames.AsEnumerable());
                                var coverLineHouseNamesText = string.Join(", ", coverLineHouseNames.AsEnumerable());
                                return
                                    $"残った {baseLineHouseNamesText} と {coverLineHouseNamesText} のうち、" +
                                    $"{baseHouseName}で {digitText} が入り得るマスは{cellPositionText}だけなので{cellPositionText}に {digitText} が入ります。" +
                                    $"したがって、{cellPositionText}以外の{effectedCoverHouseName}のマスに {digitText} は入り得ません。";
                            }
                            else
                            {
                                return
                                    $"残った{cellPositionText}のマスには {digitText} が入ります。" +
                                    $"したがって、{cellPositionText}以外の{effectedCoverHouseName}のマスに {digitText} は入り得ません。";
                            }
                        },
                        (isFirstDegeneration, baseLineHouseNames, coverLineHouseNames, coverHouseName, effectedBaseHouseName, cellPositionText) =>
                        {
#if DEBUG
                            System.Diagnostics.Debug.Assert(baseLineHouseNames.Length > 0);
                            System.Diagnostics.Debug.Assert(coverLineHouseNames.Length > 0);
                            System.Diagnostics.Debug.Assert(baseLineHouseNames.Length == coverLineHouseNames.Length);
#endif
                            if (isFirstDegeneration)
                            {
                                return
                                    $"{cellPositionText}に {digitText} が入らないと仮定すると{coverHouseName}以外の{coverLineHouseTypeName}で {digitText} が入り得るマスが足りなくなるので{cellPositionText}には {digitText} が入ります。" +
                                    $"したがって、{cellPositionText}以外の{coverHouseName}と{effectedBaseHouseName}のマスに {digitText} は入り得ません。";
                            }
                            else if (coverLineHouseNames.Length > 1)
                            {
                                var baseLineHouseNamesText = string.Join(", ", baseLineHouseNames.AsEnumerable());
                                var coverLineHouseNamesText = string.Join(", ", coverLineHouseNames.AsEnumerable());
                                return
                                    $"残った {baseLineHouseNamesText} と {coverLineHouseNamesText} のうち、" +
                                    $"{cellPositionText}に {digitText} が入らないと仮定すると{coverHouseName}以外の{coverLineHouseTypeName}で {digitText} が入り得るマスが足りなくなるので{cellPositionText}には {digitText} が入ります。" +
                                    $"したがって、{cellPositionText}以外の{coverHouseName}と{effectedBaseHouseName}のマスに {digitText} は入り得ません。";
                            }
                            else
                            {
                                // このコードには事実上到達し得ない。
                                // 理由は、ベースラインが先に評価されるため、単独のセルしか残っていない場合はカバーラインでは評価されないから。
                                return
                                    $"残った{cellPositionText}のマスには {digitText} が入ります。" +
                                    $"したがって、{cellPositionText}以外の{coverHouseName}のマスに {digitText} は入り得ません。";
                            }
                        });
#if DEBUG
                System.Diagnostics.Debug.Assert(undegeneratedBaseLineHouseNames.Length == undegeneratedCoverLineHouseNames.Length);
#pragma warning disable IDE0047 // 不要なかっこの削除 (ソースコードの可読性を重視するために抑制)
                System.Diagnostics.Debug.Assert((determinedCellIndexBitSet != UInt128.Zero) == (degenerationMessages.Length > 0));
#pragma warning restore IDE0047 // 不要なかっこの削除 (ソースコードの可読性を重視するために抑制)

                // サシミフィッシュは退化可能であることが前提なので、以下も成立する。
                System.Diagnostics.Debug.Assert(determinedCellIndexBitSet != UInt128.Zero);
                System.Diagnostics.Debug.Assert(degenerationMessages.Length > 0);
#endif
                var undegeneratedBaseLineHouseNamesText = string.Join(", ", undegeneratedBaseLineHouseNames.AsEnumerable());
                var undegeneratedCoverLineHouseNamesText = string.Join(", ", undegeneratedCoverLineHouseNames.AsEnumerable());
                var degenerationTextLines = degenerationMessages.AsEnumerable();
                if (undegeneratedBaseLineHouseNames.Length > 0)
                {
                    var lastFishName =
                        undegeneratedBaseLineHouseNames.Length switch
                        {
                            2 => "X-ウィング",
                            3 => "ソードフィッシュ",
                            _ => throw new ApplicationException(),

                        };
                    degenerationTextLines =
                        degenerationTextLines
                        .Append(
                            $"残った {undegeneratedBaseLineHouseNamesText} と {undegeneratedCoverLineHouseNamesText} で{lastFishName}が成立するので、{undegeneratedCoverLineHouseNamesText} の各{coverLineHouseTypeName}の {undegeneratedBaseLineHouseNamesText} を除くマスには {digitText} は入り得ません。");
                }

                var degenerationText =
                    string.Join(
                        "\n",
                        degenerationTextLines
                        .Zip(Enumerable.Range(1, int.MaxValue - 1), (message, n) => $"  {n}) {message}"));
                if (!finCellIndexBitSet.IsSingleBit())
                {
                    // フィンのセルが複数存在する場合
                    return
                        $"数字 {digitText} に着目します。\n" +
                        $"以下の条件が全て成立するため、{sashimiFishName}が成立しています。\n" +
                        $"1) {finnedBaseLineHouseName}にあるフィンと呼ばれる一部のマス ({finCellPositionsText}) を除いて {baseLineHouseNamesText} の数字の候補 {digitText} を含むマスは {coverLineHouseNamesText} の何れかとの交差点にのみに存在する。\n" +
                        $"2) {isolatedCellHouseName}の {effectecdByIsolatedCellHouseNamesText} との交差点にある数字の候補 {digitText} を持つマスは 1 個だけである。\n" +
                        $"3) 全てのフィンが{finBlockHouseName}に収まっている。\n" +
                        $"\n" +
                        $"ここで、何れかのフィンのマスの数字が {digitText} かどうかの仮定をしてみます。\n" +
                        $"\n" +
                        $"a) もし、何れかのフィンのマスの数字が {digitText} である場合、" +
                        $"全てのフィンのマスとハウスを共有するフィン以外のマスには {digitText} は入り得ません。\n" +
                        $"\n" +
                        $"b) もし (a) ではない場合、つまり何れのフィンのマスの数字も {digitText} ではない場合、以下のことが言えます。\n" +
                        $"{degenerationText}\n" +
                        $"結局 {coverLineHouseNamesText} の各{coverLineHouseTypeName}で {baseLineHouseNamesText} 以外のマスには {digitText} は入り得ません。\n" +
                        $"\n" +
                        $"(a) と (b) のどちらが成立したとしても、少なくとも以下の条件を満たすマスには {digitText} は入り得ないので、それらのマスの数字の候補から {digitText} を排除します。\n" +
                        $"1) {coverLineHouseNamesText} の各{coverLineHouseTypeName}の {baseLineHouseNamesText} 以外のマスであり、かつ\n" +
                        $"2) 全てのフィンのマス ({finCellPositionsText}) と同じブロック ({finBlockHouseName}) に存在するマス。";
                }
                else
                {
                    // フィンのセルが一つだけ存在する場合
                    return
                        $"数字 {digitText} に着目します。\n" + $"以下の条件が全て成立するため、{sashimiFishName}が成立しています。\n" +
                        $"1) {finnedBaseLineHouseName}にあるフィンと呼ばれる一部のマス ({finCellPositionsText}) を除いて {baseLineHouseNamesText} の数字の候補 {digitText} を含むマスは {coverLineHouseNamesText} の何れかとの交差点にのみに存在する。\n" +
                        $"2) {isolatedCellHouseName}の {effectecdByIsolatedCellHouseNamesText} との交差点にある数字の候補 {digitText} を持つマスは 1 個だけである。\n" +
                        $"\n" +
                        $"ここで、フィンのマスの数字が {digitText} かどうかの仮定をしてみます。\n" +
                        $"\n" +
                        $"a) もし、フィンのマスの数字が {digitText} である場合、" +
                        $"フィンのマスとハウスを共有するフィン以外のマスには {digitText} は入り得ません。\n" +
                        $"\n" +
                        $"b) もし (a) ではない場合、つまりフィンのマスの数字が {digitText} ではない場合、以下のことが言えます。\n" +
                        $"{degenerationText}\n" +
                        $"結局 {coverLineHouseNamesText} の各{coverLineHouseTypeName}で {baseLineHouseNamesText} 以外のマスには {digitText} は入り得ません。\n" +
                        $"\n" +
                        $"(a) と (b) のどちらが成立したとしても、少なくとも以下の条件を満たすマスには {digitText} は入り得ないので、それらのマスの数字の候補から {digitText} を排除します。\n" +
                        $"1) {coverLineHouseNamesText} の各{coverLineHouseTypeName}の {baseLineHouseNamesText} 以外のマスであり、かつ\n" +
                        $"2) フィンのマス ({finCellPositionsText}) と同じブロック ({finBlockHouseName}) に存在するマス。";
                }
            }
            else
            {
                // フィッシュが退化可能ではない場合 (フィン付きフィッシュ)

                if (!finCellIndexBitSet.IsSingleBit())
                {
                    // フィンのセルが複数存在する場合
                    return
                        $"数字 {digitText} に着目します。\n" +
                        $"以下の条件が全て成立するため、{finnedFishName}が成立しています。\n" +
                        $"1) {finnedBaseLineHouseName}にあるフィンと呼ばれる一部のマス ({finCellPositionsText}) を除いて {baseLineHouseNamesText} の数字の候補 {digitText} を含むマスは {coverLineHouseNamesText} の何れかとの交差点にのみに存在する。\n" +
                        $"2) 全てのフィンが{finBlockHouseName}に収まっている。\n" +
                        $"\n" +
                        $"a) もし、何れかのフィンのマスの数字が {digitText} である場合、" +
                        $"全てのフィンのマスとハウスを共有するフィン以外のマスには {digitText} は入り得ません。\n" +
                        $"\n" +
                        $"b) もし (a) ではない場合、つまり何れのフィンのマスの数字も {digitText} ではない場合、" +
                        $"{fishName}が成立するので {baseLineHouseNamesText} を除く {coverLineHouseNamesText} のマスには {digitText} は入り得ません。\n" +
                        $"\n" +
                        $"(a) と (b) のどちらが成立したとしても、少なくとも以下の条件を満たすマスには {digitText} は入り得ないので、それらのマスの数字の候補から {digitText} を排除します。\n" +
                        $"1) {baseLineHouseNamesText} を除く {coverLineHouseNamesText} の何れかに存在し、かつ\n" +
                        $"2) 全てのフィンのマス ({finCellPositionsText}) と同じブロック ({finBlockHouseName}) に存在するマス。";
                }
                else
                {
                    // フィンのセルが一つだけ存在する場合
                    return
                        $"数字 {digitText} に着目します。\n" +
                        $"{finnedBaseLineHouseName}にあるフィンと呼ばれる一部のマス ({finCellPositionsText}) を除いて {baseLineHouseNamesText} の数字の候補 {digitText} を含むマスは {coverLineHouseNamesText} の何れかとの交差点にのみに存在するため、" +
                        $"{finnedFishName}が成立しています。\n" +
                        $"\n" +
                        $"ここで、フィンのマスの数字が {digitText} かどうかの仮定をしてみます。\n" +
                        $"\n" +
                        $"a) もし、フィンのマスの数字が {digitText} である場合、" +
                        $"フィンのマスとハウスを共有するフィン以外のマスには {digitText} は入り得ません。\n" +
                        $"\n" +
                        $"b) もし (a) ではない場合、つまりフィンのマスの数字が {digitText} ではない場合、" +
                        $"{fishName}が成立するので、" +
                        $"{baseLineHouseNamesText} を除く {coverLineHouseNamesText} のマスには {digitText} は入り得ません。\n" +
                        $"\n" +
                        $"(a) と (b) のどちらが成立したとしても、少なくとも以下の条件を満たすマスには {digitText} は入り得ないので、それらのマスの数字の候補から {digitText} を排除します。\n" +
                        $"1) {baseLineHouseNamesText} を除く {coverLineHouseNamesText} の何れかに存在し、かつ\n" +
                        $"2) フィンのマス ({finCellPositionsText}) と同じブロック ({finBlockHouseName}) に存在するマス。";
                }
            }
        }
    }
}
