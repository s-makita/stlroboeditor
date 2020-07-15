using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace StellarRobo.Analyze
{
    /// <summary>
    /// StellarRoboの字句解析器を定義します。
    /// </summary>
    public sealed class StellarRoboLexer
    {
        #region static fields
        private static IOrderedEnumerable<Tuple<string, StellarRoboTokenType>> Keywords = new List<Tuple<string, StellarRoboTokenType>>
        {
            //new Tuple<string, StellarRoboTokenType>("class", StellarRoboTokenType.ClassKeyword),
            //new Tuple<string, StellarRoboTokenType>("endclass", StellarRoboTokenType.EndClassKeyword),
            //new Tuple<string, StellarRoboTokenType>("static", StellarRoboTokenType.StaticKeyword),
            new Tuple<string, StellarRoboTokenType>("func", StellarRoboTokenType.FuncKeyword),
            new Tuple<string, StellarRoboTokenType>("endfunc", StellarRoboTokenType.EndFuncKeyword),
            new Tuple<string, StellarRoboTokenType>("if", StellarRoboTokenType.IfKeyword),
            new Tuple<string, StellarRoboTokenType>("elseif", StellarRoboTokenType.ElifKeyword),
            new Tuple<string, StellarRoboTokenType>("then", StellarRoboTokenType.ThenKeyword),
            new Tuple<string, StellarRoboTokenType>("else", StellarRoboTokenType.ElseKeyword),
            new Tuple<string, StellarRoboTokenType>("endif", StellarRoboTokenType.EndIfKeyword),
            new Tuple<string, StellarRoboTokenType>("case", StellarRoboTokenType.CaseKeyword),
            new Tuple<string, StellarRoboTokenType>("when", StellarRoboTokenType.WhenKeyword),
            new Tuple<string, StellarRoboTokenType>("default", StellarRoboTokenType.DefaultKeyword),
            new Tuple<string, StellarRoboTokenType>("endcase", StellarRoboTokenType.EndCaseKeyword),
            new Tuple<string, StellarRoboTokenType>("for", StellarRoboTokenType.ForKeyword),
            new Tuple<string, StellarRoboTokenType>("continue", StellarRoboTokenType.ContinueKeyword),
            new Tuple<string, StellarRoboTokenType>("break", StellarRoboTokenType.BreakKeyword),
            new Tuple<string, StellarRoboTokenType>("next", StellarRoboTokenType.NextKeyword),
            new Tuple<string, StellarRoboTokenType>("while", StellarRoboTokenType.WhileKeyword),
            new Tuple<string, StellarRoboTokenType>("do", StellarRoboTokenType.DoKeyword),
            new Tuple<string, StellarRoboTokenType>("foreach", StellarRoboTokenType.ForeachKeyword),
            new Tuple<string, StellarRoboTokenType>("in", StellarRoboTokenType.InKeyword),
            //new Tuple<string, StellarRoboTokenType>("of", StellarRoboTokenType.OfKeyword),
            //new Tuple<string, StellarRoboTokenType>("local", StellarRoboTokenType.LocalKeyword),
            //new Tuple<string, StellarRoboTokenType>("self", StellarRoboTokenType.SelfKeyword),
            new Tuple<string, StellarRoboTokenType>("true", StellarRoboTokenType.TrueKeyword),
            new Tuple<string, StellarRoboTokenType>("false", StellarRoboTokenType.FalseKeyword),
            new Tuple<string, StellarRoboTokenType>("nil", StellarRoboTokenType.NilKeyword),
            new Tuple<string, StellarRoboTokenType>("VARGS", StellarRoboTokenType.VargsKeyword),
            new Tuple<string, StellarRoboTokenType>("return", StellarRoboTokenType.ReturnKeyword),
            //new Tuple<string, StellarRoboTokenType>("yield", StellarRoboTokenType.YieldKeyword),
            //new Tuple<string, StellarRoboTokenType>("coroutine", StellarRoboTokenType.CoroutineKeyword),
            //new Tuple<string, StellarRoboTokenType>("coresume", StellarRoboTokenType.CoresumeKeyword),
            new Tuple<string, StellarRoboTokenType>("use", StellarRoboTokenType.UseKeyword),
            new Tuple<string, StellarRoboTokenType>("include", StellarRoboTokenType.IncludeKeyWord),
        }.OrderByDescending(p => p.Item1.Length).ThenBy(p => p.Item1);

        private static IOrderedEnumerable<Tuple<string, StellarRoboTokenType>> Operators = new List<Tuple<string, StellarRoboTokenType>>
        {
            new Tuple<string, StellarRoboTokenType>("+", StellarRoboTokenType.Plus),
            new Tuple<string, StellarRoboTokenType>("-", StellarRoboTokenType.Minus),
            new Tuple<string, StellarRoboTokenType>("*", StellarRoboTokenType.Multiply),
            new Tuple<string, StellarRoboTokenType>("/", StellarRoboTokenType.Divide),
            new Tuple<string, StellarRoboTokenType>("&", StellarRoboTokenType.And),
            new Tuple<string, StellarRoboTokenType>("|", StellarRoboTokenType.Or),
            new Tuple<string, StellarRoboTokenType>("!", StellarRoboTokenType.Not),
            new Tuple<string, StellarRoboTokenType>("^", StellarRoboTokenType.Xor),
            new Tuple<string, StellarRoboTokenType>("%", StellarRoboTokenType.Modular),
            new Tuple<string, StellarRoboTokenType>("=", StellarRoboTokenType.Assign),
            new Tuple<string, StellarRoboTokenType>("<<", StellarRoboTokenType.LeftBitShift),
            new Tuple<string, StellarRoboTokenType>(">>", StellarRoboTokenType.RightBitShift),
            new Tuple<string, StellarRoboTokenType>("==", StellarRoboTokenType.Equal),
            new Tuple<string, StellarRoboTokenType>("!=", StellarRoboTokenType.NotEqual),
            new Tuple<string, StellarRoboTokenType>(">", StellarRoboTokenType.Greater),
            new Tuple<string, StellarRoboTokenType>("<", StellarRoboTokenType.Lesser),
            new Tuple<string, StellarRoboTokenType>(">=", StellarRoboTokenType.GreaterEqual),
            new Tuple<string, StellarRoboTokenType>("<=", StellarRoboTokenType.LesserEqual),
            new Tuple<string, StellarRoboTokenType>("~=", StellarRoboTokenType.SpecialEqual),
            new Tuple<string, StellarRoboTokenType>("&&", StellarRoboTokenType.AndAlso),
            new Tuple<string, StellarRoboTokenType>("||", StellarRoboTokenType.OrElse),
            new Tuple<string, StellarRoboTokenType>("+=", StellarRoboTokenType.PlusAssign),
            new Tuple<string, StellarRoboTokenType>("-=", StellarRoboTokenType.MinusAssign),
            new Tuple<string, StellarRoboTokenType>("*=", StellarRoboTokenType.MultiplyAssign),
            new Tuple<string, StellarRoboTokenType>("/=", StellarRoboTokenType.DivideAssign),
            new Tuple<string, StellarRoboTokenType>("&=", StellarRoboTokenType.AndAssign),
            new Tuple<string, StellarRoboTokenType>("|=", StellarRoboTokenType.OrAssign),
            new Tuple<string, StellarRoboTokenType>("^=", StellarRoboTokenType.XorAssign),
            new Tuple<string, StellarRoboTokenType>("%=", StellarRoboTokenType.ModularAssign),
            new Tuple<string, StellarRoboTokenType>("<<=", StellarRoboTokenType.LeftBitShiftAssign),
            new Tuple<string, StellarRoboTokenType>(">>=", StellarRoboTokenType.RightBitShiftAssign),
            new Tuple<string, StellarRoboTokenType>("++", StellarRoboTokenType.Increment),
            new Tuple<string, StellarRoboTokenType>("--", StellarRoboTokenType.Decrement),
            new Tuple<string, StellarRoboTokenType>("?", StellarRoboTokenType.Question),
            new Tuple<string, StellarRoboTokenType>(":", StellarRoboTokenType.Colon),
            new Tuple<string, StellarRoboTokenType>("||=", StellarRoboTokenType.NilAssign),
            new Tuple<string, StellarRoboTokenType>(",", StellarRoboTokenType.Comma),
            new Tuple<string, StellarRoboTokenType>(".", StellarRoboTokenType.Period),
            new Tuple<string, StellarRoboTokenType>("...", StellarRoboTokenType.VariableArguments),
            //new Tuple<string, StellarRoboTokenType>("=>", StellarRoboTokenType.Lambda),

            //new Tuple<string, StellarRoboTokenType>(Environment.NewLine, StellarRoboTokenType.NewLine),
            new Tuple<string, StellarRoboTokenType>(";", StellarRoboTokenType.Semicolon),
            new Tuple<string, StellarRoboTokenType>("(", StellarRoboTokenType.ParenStart),
            new Tuple<string, StellarRoboTokenType>(")", StellarRoboTokenType.ParenEnd),
            new Tuple<string, StellarRoboTokenType>("{", StellarRoboTokenType.BraceStart),
            new Tuple<string, StellarRoboTokenType>("}", StellarRoboTokenType.BraceEnd),
            new Tuple<string, StellarRoboTokenType>("[", StellarRoboTokenType.BracketStart),
            new Tuple<string, StellarRoboTokenType>("]", StellarRoboTokenType.BracketEnd),
        }.OrderByDescending(p => p.Item1.Length).ThenBy(p => p.Item1);

        private static Regex DecimalNumberPattern = new Regex("[0-9_]+(\\.[0-9_]+)?");
        private static Regex BinaryNumberPattern = new Regex("0b[01]+");
        private static Regex OctadecimalNumberPattern = new Regex("0[oO][0-7]+");
        private static Regex HexadecimalNumberPattern = new Regex("0[xX][0-9a-fA-F]+");
        private static Regex HexatridecimalNumberPattern = new Regex("0[tT][0-9a-zA-Z]+");
        private static Regex IdentiferPattern = new Regex("[\\p{Lu}\\p{Ll}\\p{Lt}\\p{Lm}\\p{Lo}\\p{Nl}][\\p{Lu}\\p{Ll}\\p{Lt}\\p{Lm}\\p{Lo}\\p{Nl}\\p{Mn}\\p{Mc}\\p{Pc}\\p{Nd}\\p{Cf}]*");
        private static List<Tuple<string, string>> MultilineCommentQuotations = new List<Tuple<string, string>> {
            new Tuple<string, string>("/*", "*/"),
            new Tuple<string, string>("#{", "#}"),
        };
        private static List<string> LineCommentStart = new List<string> { "//", "#" };
        private static List<string> StringQuotation = new List<string> { "\"", "'" };
        private static Regex WhitespacePattern = new Regex("[\\p{Zs}\\t]+");
        private static Tuple<string, string> StringEscapes = new Tuple<string, string>("\\\\", "");
        private static List<Tuple<string, string>> StringLiteralEscapes = new List<Tuple<string, string>>
        {
            new Tuple<string, string>("\\r", "\r"),
            new Tuple<string, string>("\\n", Environment.NewLine),
            new Tuple<string, string>("\\t", "\t"),
            new Tuple<string, string>("\\\"", "\""),
            new Tuple<string, string>("\\'", "'"),
            new Tuple<string, string>("\\\\", "\\"),
        };
        #endregion

        #region properties

        /// <summary>
        /// <see cref="AnalyzeFromFile(string)"/>で出力される<see cref="StellarRoboLexResult.SourceName"/>にフルパスを設定します。
        /// </summary>
        public bool SetFullFileName { get; set; } = false;

        /// <summary>
        /// <see cref="AnalyzeFromSource(string)"/>で出力される<see cref="StellarRoboLexResult.SourceName"/>に設定される文字列を取得・設定します。
        /// </summary>
        public string DefaultSourceName { get; set; } = "No name";
        #endregion

        /// <summary>
        /// インスタンスを初期化します。
        /// </summary>
        public StellarRoboLexer()
        {
        }

        /// <summary>
        /// ファイルを指定して解析します。
        /// <see cref="Encoding.Default"/>のエンコードのものとされます。
        /// </summary>
        /// <param name="fileName">ファイル名</param>
        /// <returns>解析結果</returns>
        public StellarRoboLexResult AnalyzeFromFile(string fileName) => AnalyzeFromText(Path.GetFileName(fileName), File.ReadAllText(fileName, Encoding.Default));

        /// <summary>
        /// ファイルを指定して解析します。
        /// </summary>
        /// <param name="fileName">ファイル名</param>
        /// <param name="encode">仕様する<see cref="Encoding"/></param>
        /// <returns>解析結果</returns>
        public StellarRoboLexResult AnalyzeFromFile(string fileName, Encoding encode) => AnalyzeFromText(Path.GetFileName(fileName), File.ReadAllText(fileName, encode));

        /// <summary>
        /// ソースコードを直接指定して解析します。
        /// </summary>
        /// <param name="source">ソースコード</param>
        /// <returns>解析結果</returns>
        public StellarRoboLexResult AnalyzeFromSource(string source) => AnalyzeFromText(DefaultSourceName, source);

        /// <summary>
        /// ソースコードを直接指定して解析します。
        /// </summary>
        /// <param name="source">ソースコード</param>
        /// <param name="sourceName">ソース名</param>
        /// <returns>解析結果</returns>
        public StellarRoboLexResult AnalyzeFromSource(string source, string sourceName) => AnalyzeFromText(sourceName, source);

        /// <summary>
        /// ソース名とソースを指定して解析します。
        /// </summary>
        /// <param name="name">ソース名</param>
        /// <param name="source">解析対象のソースコード</param>
        /// <returns></returns>
        private StellarRoboLexResult AnalyzeFromText(string name, string source)
        {
            var result = new StellarRoboLexResult(name);
            var line = 0;
            var col = 0;
            var cq = "";
            Tuple<string, StellarRoboTokenType> kw;
            Match lm;
            StellarRoboError ei;
            while (source != "")
            {
                //空白論理行

                if (source.StartsWith(Environment.NewLine))
                {
                    source = source.Substring(Environment.NewLine.Length);
                    result.AddToken(new StellarRoboToken { Position = new Tuple<int, int>(line, col), TokenString = "<NewLine>", Type = StellarRoboTokenType.NewLine });
                    line++;
                    col = 0;
                    continue;
                }
                /*
                if (source.StartsWith(";"))
                {
                    source = source.Substring(1);
                    result.AddToken(new StellarRoboToken { Position = new Tuple<int, int>(line, col), TokenString = ";", Type = StellarRoboTokenType.Semicolon });
                    col++;
                    continue;
                }
                */
                Tuple<string, string> mcq = null;
                if ((mcq = MultilineCommentQuotations.FirstOrDefault(p => source.StartsWith(p.Item1))) != null)
                {
                    source = source.Substring(mcq.Item1.Length);
                    col += mcq.Item1.Length;
                    var ce = source.IndexOf(mcq.Item2);
                    var ecs = source.IndexOf(mcq.Item1);
                    //不正な複数行コメント
                    if ((ecs >= 0 && ecs < ce) || ce < 0)
                    {
                        ei = new StellarRoboError
                        {
                            Column = col,
                            Line = line,
                            Message = "不正な複数行コメントです。コメントが終了していないか、入れ子になっています。"
                        };
                        result.Error = ei;
                        result.Success = false;
                        return result;
                    }
                    while (true)
                    {
                        ce = source.IndexOf(mcq.Item2);
                        var cl = source.IndexOf(Environment.NewLine);
                        if ((cl > 0 && ce < cl) || cl < 0)
                        {
                            source = source.Substring(ce + mcq.Item2.Length);
                            col += ce + mcq.Item2.Length;
                            break;
                        }
                        else
                        {
                            source = source.Substring(cl + Environment.NewLine.Length);
                            line++;
                            col = 0;
                        }
                    }
                    continue;
                }
                //コメント
                if ((cq = LineCommentStart.FirstOrDefault(p => source.StartsWith(p))) != null)
                {
                    source = source.Substring(cq.Length);
                    col += cq.Length;
                    var cl = source.IndexOf(Environment.NewLine);
                    if (cl >= 0)
                    {
                        source = source.Substring(cl + Environment.NewLine.Length);
                        line++;
                        col = 0;
                    }
                    else
                    {
                        //ラストコメント
                        source = "";
                    }
                    continue;
                }
                //空白
                if ((lm = WhitespacePattern.Match(source)).Success && lm.Index == 0)
                {
                    source = source.Substring(lm.Length);
                    col += lm.Length;
                    continue;
                }
                //演算子
                if ((kw = Operators.FirstOrDefault(p => source.StartsWith(p.Item1))) != null)
                {
                    source = source.Substring(kw.Item1.Length);
                    col += kw.Item1.Length;
                    result.AddToken(kw.CreateToken(col, line));
                    continue;
                }
                //識別子・キーワード
                if ((lm = IdentiferPattern.Match(source)).Success && lm.Index == 0)
                {
                    if ((kw = Keywords.FirstOrDefault(p => lm.Value == p.Item1)) != null)
                    {
                        source = source.Substring(kw.Item1.Length);
                        col += kw.Item1.Length;
                        result.AddToken(kw.CreateToken(col, line));
                    }
                    else
                    {
                        source = source.Substring(lm.Length);
                        col += lm.Length;
                        result.AddToken(lm.Value.CreateTokenAsIdentifer(col, line));
                    }
                    continue;
                }
                //リテラル
                if ((lm = BinaryNumberPattern.Match(source)).Success && lm.Index == 0)
                {
                    source = source.Substring(lm.Length);
                    col += lm.Length;
                    result.AddToken(lm.Value.CreateTokenAsBinaryNumber(col, line));
                    if (!(lm = IdentiferPattern.Match(source)).Success || lm.Index != 0) continue;
                }
                if ((lm = OctadecimalNumberPattern.Match(source)).Success && lm.Index == 0)
                {
                    source = source.Substring(lm.Length);
                    col += lm.Length;
                    result.AddToken(lm.Value.CreateTokenAsOctadecimalNumber(col, line));
                    if (!(lm = IdentiferPattern.Match(source)).Success || lm.Index != 0) continue;
                }
                if ((lm = HexadecimalNumberPattern.Match(source)).Success && lm.Index == 0)
                {
                    source = source.Substring(lm.Length);
                    col += lm.Length;
                    result.AddToken(lm.Value.CreateTokenAsHexadecimalNumber(col, line));
                    if (!(lm = IdentiferPattern.Match(source)).Success || lm.Index != 0) continue;
                }
                if ((lm = HexatridecimalNumberPattern.Match(source)).Success && lm.Index == 0)
                {
                    source = source.Substring(lm.Length);
                    col += lm.Length;
                    result.AddToken(lm.Value.CreateTokenAsHexatridecimalNumber(col, line));
                    if (!(lm = IdentiferPattern.Match(source)).Success || lm.Index != 0) continue;
                }
                if ((lm = DecimalNumberPattern.Match(source)).Success && lm.Index == 0)
                {
                    source = source.Substring(lm.Length);
                    col += lm.Length;
                    result.AddToken(lm.Value.CreateTokenAsDecimalNumber(col, line));
                    if (!(lm = IdentiferPattern.Match(source)).Success || lm.Index != 0) continue;
                }
                if ((cq = StringQuotation.FirstOrDefault(p => source.StartsWith(p))) != null)
                {
                    source = source.Substring(cq.Length);
                    col += cq.Length;
                    int qp = 0, eqp = 0, inp = 0;
                    var ls = "";
                    do
                    {
                        eqp = source.IndexOf("\\" + cq);
                        qp = source.IndexOf(cq);
                        inp = source.IndexOf(Environment.NewLine);
                        if (inp >= 0 && inp < qp)
                        {
                            ei = new StellarRoboError
                            {
                                Column = col,
                                Line = line,
                                Message = "文字列リテラル中に直接改行が含まれています。改行を表現したい場合、\\nを利用してください。"
                            };
                            result.Error = ei;
                            result.Success = false;
                            return result;
                        }
                        if (qp < 0)
                        {
                            ei = new StellarRoboError
                            {
                                Column = col,
                                Line = line,
                                Message = "文字列リテラルが閉じていません。"
                            };
                            result.Error = ei;
                            result.Success = false;
                            return result;
                        }
                        if (eqp >= 0 && qp - eqp == 1)
                        {
                            string tmp = source.Substring(0, qp + cq.Length).Replace("\\\\", "");
                            if (tmp.Length == 1 || tmp.LastIndexOf("\\\"") < 0)
                            {
                                ls += source.Substring(0, qp);
                                source = source.Substring(qp);
                                col += qp - 1;
                            }
                            else
                            {
                                ls += source.Substring(0, qp + cq.Length);
                                source = source.Substring(qp + cq.Length);
                                col += qp + cq.Length - 1;
                            }
                            continue;
                        }
                        else
                        {
                            ls += source.Substring(0, qp);
                            source = source.Substring(qp + cq.Length);
                            foreach (var i in StringLiteralEscapes)
                            {
                                if (StringEscapes.Item1 == i.Item1 || ls.Replace(StringEscapes.Item1, StringEscapes.Item2).Contains(i.Item1))
                                {
                                    ls = ls.Replace(i.Item1, i.Item2);
                                }
                            }

                            result.AddToken(new StellarRoboToken { Position = new Tuple<int, int>(col, line), TokenString = ls, Type = StellarRoboTokenType.StringLiteral });
                            col += qp + cq.Length;
                            break;
                        }
                    } while (true);
                    continue;
                }
                //不明
                ei = new StellarRoboError
                {
                    Column = col,
                    Line = line,
                    Message = "不正なトークンです。"
                };
                result.Error = ei;
                result.Success = false;
                return result;
            }
            //全てを取り込んだ後にInclude文が存在するか調べ、存在したならばそのソースを字句解析にかける
            for(int i = 0; i < result.Tokens.Count; i++)
            {
                //このTokenはIncludeか？
                StellarRoboToken token = result.Tokens[i];
                if(token.Type == StellarRoboTokenType.IncludeKeyWord)
                {
                    //Includeならば、次の文字は取り込みファイルとして取り込む
                    string filePath = result.Tokens[i + 1].TokenString;

                    //余分な文字を消す
                    filePath = Regex.Replace(filePath, "\"", "");

                    //リテラルで指定されている文字列をファイルとみなし、パスが指定されていない場合には実行パスを指定する
                    try
                    {
                        if (Path.GetDirectoryName(filePath) == string.Empty)
                        {
                            filePath = Path.Combine(Directory.GetCurrentDirectory(), Path.GetFileName(filePath));
                        }
                    }
                    catch
                    {
                        filePath = Path.Combine(Directory.GetCurrentDirectory(), Path.GetFileName(filePath));
                    }

                    //再度、字句解析を行う
                    StellarRoboLexResult ImportResult = new StellarRoboLexResult(filePath);
                    string ImportSource = string.Empty;
                    if (File.Exists(filePath))
                    {
                        using (StreamReader streamReader = new StreamReader(filePath))
                        {
                            //ソースファイルを読み込む
                            ImportSource = streamReader.ReadToEnd();
                        }
                        ImportResult = AnalyzeFromText(Path.GetFileName(filePath), ImportSource);
                    } else
                    {
                        //ファイルが存在しない
                        ImportResult.Success = false;
                        ImportResult.SetError(token.Position.Item1, token.Position.Item2, "指定されたファイルが存在しません");
                    }
                    //字句解析に失敗したなら、そのエラーをもって終了とする
                    if (!ImportResult.Success)
                    {
                        return ImportResult;
                    }
                }
            }

            result.Success = true;
            return result;
        }
    }
}
