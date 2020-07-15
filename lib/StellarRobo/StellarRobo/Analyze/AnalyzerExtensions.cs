using System;
using System.Collections.Generic;
using System.Linq;

namespace StellarRobo.Analyze
{
    internal static class AnalyzerExtensions
    {
        private static List<StellarRoboTokenType> logicallines = new List<StellarRoboTokenType> {
            StellarRoboTokenType.NewLine,
            StellarRoboTokenType.Semicolon,
        };

        public static StellarRoboToken CreateToken(this Tuple<string, StellarRoboTokenType> tt, int col, int line)
            => new StellarRoboToken { Position = new Tuple<int, int>(col, line), TokenString = tt.Item1, Type = tt.Item2 };

        public static StellarRoboToken CreateTokenAsIdentifer(this string ls, int col, int line)
            => new StellarRoboToken { Position = new Tuple<int, int>(col, line), TokenString = ls, Type = StellarRoboTokenType.Identifer };

        public static StellarRoboToken CreateTokenAsDecimalNumber(this string ls, int col, int line)
            => new StellarRoboToken { Position = new Tuple<int, int>(col, line), TokenString = ls, Type = StellarRoboTokenType.DecimalNumberLiteral };

        public static StellarRoboToken CreateTokenAsBinaryNumber(this string ls, int col, int line)
            => new StellarRoboToken { Position = new Tuple<int, int>(col, line), TokenString = ls, Type = StellarRoboTokenType.BinaryNumberLiteral };

        public static StellarRoboToken CreateTokenAsOctadecimalNumber(this string ls, int col, int line)
            => new StellarRoboToken { Position = new Tuple<int, int>(col, line), TokenString = ls, Type = StellarRoboTokenType.OctadecimalNumberLiteral };

        public static StellarRoboToken CreateTokenAsHexadecimalNumber(this string ls, int col, int line)
            => new StellarRoboToken { Position = new Tuple<int, int>(col, line), TokenString = ls, Type = StellarRoboTokenType.HexadecimalNumberLiteral };

        public static StellarRoboToken CreateTokenAsHexatridecimalNumber(this string ls, int col, int line)
            => new StellarRoboToken { Position = new Tuple<int, int>(col, line), TokenString = ls, Type = StellarRoboTokenType.HexatridecimalNumberLiteral };

        public static StellarRoboError CreateErrorAt(this StellarRoboToken token, string message)
            => new StellarRoboError { Column = token.Position.Item1, Line = token.Position.Item2, Message = message };


        /// <summary>
        /// 飛ばせるなら飛ばせるだけ飛ばす
        /// </summary>
        /// <param name="tokens">リスト</param>
        /// <returns>1つ以上飛ばせたらtrue</returns>
        public static bool SkipLogicalLineBreak(this Queue<StellarRoboToken> tokens)
        {
            if (tokens.Count == 0) return false;
            if (!logicallines.Any(p => p == tokens.Peek().Type)) return false;
            do
            {
                tokens.Dequeue();
            } while (tokens.Count > 0 && logicallines.Any(p => p == tokens.Peek().Type));
            return true;
        }

        /// <summary>
        /// チェックして飛ばせたら飛ばす
        /// </summary>
        /// <param name="tokens">きゅう</param>
        /// <param name="tt">チェック対象</param>
        /// <returns>結果</returns>
        public static bool CheckSkipToken(this Queue<StellarRoboToken> tokens, params StellarRoboTokenType[] tt)
        {
            if (tokens.Count == 0) return false;
            if (!tt.Any(p => p == tokens.Peek().Type)) return false;
            tokens.Dequeue();
            return true;
        }

        /// <summary>
        /// チェックする
        /// </summary>
        /// <param name="tokens">きゅう</param>
        /// <param name="tt">チェック対象</param>
        /// <returns>結果</returns>
        public static bool CheckToken(this Queue<StellarRoboToken> tokens, params StellarRoboTokenType[] tt) => tokens.Count != 0 ? tt.Any(p => p == tokens.Peek().Type) : false;
    }
}
