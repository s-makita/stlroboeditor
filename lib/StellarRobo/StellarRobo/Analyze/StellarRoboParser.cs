using Base36Encoder;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;

namespace StellarRobo.Analyze
{
    /// <summary>
    /// StellarRoboの構文解析器を定義します。
    /// </summary>
    public sealed class StellarRoboParser
    {
        private static Dictionary<StellarRoboTokenType, int> OperatorPriorities = new Dictionary<StellarRoboTokenType, int>
        {
            //C#に準拠
            [StellarRoboTokenType.Plus] = 11,
            [StellarRoboTokenType.Minus] = 11,
            [StellarRoboTokenType.Multiply] = 12,
            [StellarRoboTokenType.Divide] = 12,
            [StellarRoboTokenType.And] = 7,
            [StellarRoboTokenType.Or] = 5,
            //[StellarRoboTokenType.Not] = 1,
            [StellarRoboTokenType.Xor] = 6,
            [StellarRoboTokenType.Modular] = 12,
            [StellarRoboTokenType.Assign] = 1,
            [StellarRoboTokenType.LeftBitShift] = 10,
            [StellarRoboTokenType.RightBitShift] = 10,
            [StellarRoboTokenType.Equal] = 8,
            [StellarRoboTokenType.NotEqual] = 8,
            [StellarRoboTokenType.Greater] = 9,
            [StellarRoboTokenType.Lesser] = 9,
            [StellarRoboTokenType.GreaterEqual] = 9,
            [StellarRoboTokenType.LesserEqual] = 9,
            [StellarRoboTokenType.SpecialEqual] = 8,
            [StellarRoboTokenType.AndAlso] = 4,
            [StellarRoboTokenType.OrElse] = 3,
            [StellarRoboTokenType.PlusAssign] = 1,
            [StellarRoboTokenType.MinusAssign] = 1,
            [StellarRoboTokenType.MultiplyAssign] = 1,
            [StellarRoboTokenType.DivideAssign] = 1,
            [StellarRoboTokenType.AndAssign] = 1,
            [StellarRoboTokenType.OrAssign] = 1,
            [StellarRoboTokenType.XorAssign] = 1,
            [StellarRoboTokenType.ModularAssign] = 1,
            [StellarRoboTokenType.LeftBitShiftAssign] = 1,
            [StellarRoboTokenType.RightBitShiftAssign] = 1,
            //[StellarRoboTokenType.Increment] = 1,
            //[StellarRoboTokenType.Decrement] = 1,
            [StellarRoboTokenType.Question] = 2,
            [StellarRoboTokenType.Colon] = 2,
            [StellarRoboTokenType.NilAssign] = 1,
        };

        private static Dictionary<StellarRoboTokenType, StellarRoboOperatorType> OperatorsTokenTable = new Dictionary<StellarRoboTokenType, StellarRoboOperatorType>
        {
            //C#に準拠
            [StellarRoboTokenType.Plus] = StellarRoboOperatorType.Plus,
            [StellarRoboTokenType.Minus] = StellarRoboOperatorType.Minus,
            [StellarRoboTokenType.Multiply] = StellarRoboOperatorType.Multiply,
            [StellarRoboTokenType.Divide] = StellarRoboOperatorType.Divide,
            [StellarRoboTokenType.And] = StellarRoboOperatorType.And,
            [StellarRoboTokenType.Or] = StellarRoboOperatorType.Or,
            //[StellarRoboTokenType.Not] = StellarRoboOperatorType.Not,
            [StellarRoboTokenType.Xor] = StellarRoboOperatorType.Xor,
            [StellarRoboTokenType.Modular] = StellarRoboOperatorType.Modular,
            [StellarRoboTokenType.Assign] = StellarRoboOperatorType.Assign,
            [StellarRoboTokenType.LeftBitShift] = StellarRoboOperatorType.LeftBitShift,
            [StellarRoboTokenType.RightBitShift] = StellarRoboOperatorType.RightBitShift,
            [StellarRoboTokenType.Equal] = StellarRoboOperatorType.Equal,
            [StellarRoboTokenType.NotEqual] = StellarRoboOperatorType.NotEqual,
            [StellarRoboTokenType.Greater] = StellarRoboOperatorType.Greater,
            [StellarRoboTokenType.Lesser] = StellarRoboOperatorType.Lesser,
            [StellarRoboTokenType.GreaterEqual] = StellarRoboOperatorType.GreaterEqual,
            [StellarRoboTokenType.LesserEqual] = StellarRoboOperatorType.LesserEqual,
            [StellarRoboTokenType.SpecialEqual] = StellarRoboOperatorType.SpecialEqual,
            [StellarRoboTokenType.AndAlso] = StellarRoboOperatorType.AndAlso,
            [StellarRoboTokenType.OrElse] = StellarRoboOperatorType.OrElse,
            [StellarRoboTokenType.PlusAssign] = StellarRoboOperatorType.PlusAssign,
            [StellarRoboTokenType.MinusAssign] = StellarRoboOperatorType.MinusAssign,
            [StellarRoboTokenType.MultiplyAssign] = StellarRoboOperatorType.MultiplyAssign,
            [StellarRoboTokenType.DivideAssign] = StellarRoboOperatorType.DivideAssign,
            [StellarRoboTokenType.AndAssign] = StellarRoboOperatorType.AndAssign,
            [StellarRoboTokenType.OrAssign] = StellarRoboOperatorType.OrAssign,
            [StellarRoboTokenType.XorAssign] = StellarRoboOperatorType.XorAssign,
            [StellarRoboTokenType.ModularAssign] = StellarRoboOperatorType.ModularAssign,
            [StellarRoboTokenType.LeftBitShiftAssign] = StellarRoboOperatorType.LeftBitShiftAssign,
            [StellarRoboTokenType.RightBitShiftAssign] = StellarRoboOperatorType.RightBitShiftAssign,
            //[StellarRoboTokenType.Increment] = StellarRoboOperatorType.Increment,
            //[StellarRoboTokenType.Decrement] = StellarRoboOperatorType.Decrement,
            [StellarRoboTokenType.Question] = StellarRoboOperatorType.ConditionalQuestion,
            [StellarRoboTokenType.Colon] = StellarRoboOperatorType.ConditionalElse,
            [StellarRoboTokenType.NilAssign] = StellarRoboOperatorType.NilAssign,
        };

        private static int OperatorMaxPriority = OperatorPriorities.Max(p => p.Value);

        /// <summary>
        /// インスタンスを初期化します。
        /// </summary>
        public StellarRoboParser()
        {

        }

        /// <summary>
        /// 指定された<see cref="StellarRoboLexResult"/>を元にASTを構築します。
        /// </summary>
        /// <param name="lex">字句解析の結果</param>
        /// <returns>構築されたAST</returns>
        public StellarRoboAst Parse(StellarRoboLexResult lex)
        {
            var result = new StellarRoboAst(lex.SourceName);
            try
            {
                var top = ParseFirstLevel(new Queue<StellarRoboToken>(lex.Tokens));
                result.RootNode = top;
                result.Success = true;
            }
            catch (StellarRoboParseException e)
            {
                result.RootNode = null;
                result.Success = false;
                result.Error = e.Error;
            }
            return result;
        }

        /// <summary>
        /// 指定された<see cref="StellarRoboLexResult"/>を式として解析します。
        /// </summary>
        /// <param name="lex">字句解析の結果</param>
        /// <returns>構築されたAST</returns>
        public StellarRoboAst ParseAsExpression(StellarRoboLexResult lex)
        {
            var result = new StellarRoboAst(lex.SourceName);
            try
            {
                var q = new Queue<StellarRoboToken>(lex.Tokens);
                var top = ParseExpression(q);
                if (q.Count != 0) throw new StellarRoboParseException(q.Dequeue().CreateErrorAt("解析されないトークンが残っています。"));
                result.RootNode = top;
                result.Success = true;
            }
            catch (StellarRoboParseException e)
            {
                result.RootNode = null;
                result.Success = false;
                result.Error = e.Error;
            }
            return result;
        }

        private StellarRoboAstNode ParseFirstLevel(Queue<StellarRoboToken> tokens)
        {
            var result = new StellarRoboAstNode();
            tokens.SkipLogicalLineBreak();
            try
            {
                while (tokens.Count != 0)
                {
                    var t = tokens.Dequeue();
                    switch (t.Type)
                    {
                        case StellarRoboTokenType.UseKeyword:
                            t = tokens.Dequeue();
                            if (t.Type != StellarRoboTokenType.StringLiteral) throw new StellarRoboParseException(t.CreateErrorAt("use文には文字列を指定して下さい。"));
                            result.AddNode(new StellarRoboUseAstNode { Target = t.TokenString });
                            break;
                        case StellarRoboTokenType.IncludeKeyWord:
                            t = tokens.Dequeue();
                            if (t.Type != StellarRoboTokenType.StringLiteral) throw new StellarRoboParseException(t.CreateErrorAt("Include文には文字列を指定して下さい。"));
                            StellarRoboLexer lexer = new StellarRoboLexer();
                            StellarRoboLexResult lexResult = lexer.AnalyzeFromFile(t.TokenString,System.Text.Encoding.UTF8);
                            Queue<StellarRoboToken> LexToken = new Queue<StellarRoboToken>(lexResult.Tokens);
                            LexToken.Dequeue();
                            result.AddNode(ParseFunction(LexToken, true));
                            break;
                        case StellarRoboTokenType.ClassKeyword:
                            result.AddNode(ParseClass(tokens));
                            break;
                        case StellarRoboTokenType.FuncKeyword:
                            result.AddNode(ParseFunction(tokens, true));
                            break;
                        default:
                            throw new StellarRoboParseException(t.CreateErrorAt("トップレベルにはクラスとメソッド、use文以外は定義できません。"));
                    }
                    tokens.SkipLogicalLineBreak();
                }
            }
            catch (StellarRoboParseException)
            {
                throw;
            }
            return result;
        }

        private StellarRoboClassAstNode ParseClass(Queue<StellarRoboToken> tokens)
        {
            var result = new StellarRoboClassAstNode();
            var nt = tokens.Dequeue();
            if (nt.Type != StellarRoboTokenType.Identifer) throw new StellarRoboParseException(nt.CreateErrorAt("クラス名にはキーワードではない識別子を指定して下さい。"));
            result.Name = nt.TokenString;
            if (!tokens.SkipLogicalLineBreak()) throw new StellarRoboParseException(nt.CreateErrorAt("class宣言の後ろに改行が必要です。"));

            while (true)
            {
                tokens.SkipLogicalLineBreak();
                var t = tokens.Dequeue();
                if (t.Type == StellarRoboTokenType.EndClassKeyword) break;
                switch (t.Type)
                {
                    case StellarRoboTokenType.FuncKeyword:
                        result.AddFunctionNode(ParseFunction(tokens, false));
                        break;
                    case StellarRoboTokenType.LocalKeyword:
                        result.AddLocalNode(ParseLocal(tokens));
                        break;
                    case StellarRoboTokenType.EndIfKeyword:
                    case StellarRoboTokenType.EndCaseKeyword:
                    case StellarRoboTokenType.EndFuncKeyword:
                        throw new StellarRoboParseException(nt.CreateErrorAt("endclassが対応していません。"));
                    default:
                        throw new StellarRoboParseException(nt.CreateErrorAt("クラス内にはメソッドかlocal宣言のみ記述出来ます。"));
                }
            }
            ReplaceClassAccess(result);
            return result;
        }

        private void ReplaceClassAccess(StellarRoboClassAstNode node)
        {
            var imn = node.Functions.Where(p => !p.StaticMethod).Select(p => p.Name).ToList();
            var cmn = node.Functions.Where(p => p.StaticMethod).Select(p => p.Name).ToList();
            var ln = node.Locals.Select(p => p.Name).ToList();
            var cn = node.Name;
            foreach (var i in node.Functions) ReplaceBlockClassAccess(i.Children.ToList(), i.StaticMethod, cn, imn, cmn, ln);
        }

        private void ReplaceBlockClassAccess(IList<StellarRoboAstNode> node, bool isStatic, string className, IList<string> instanceMethods, IList<string> staticMethods, IList<string> Locals)
        {
            Action<IList<StellarRoboAstNode>> curryblk = (p) => ReplaceBlockClassAccess(p, isStatic, className, instanceMethods, staticMethods, Locals);
            Action<StellarRoboExpressionAstNode> curryexp = (p) => ReplaceExpressionClassAccess(p, isStatic, className, instanceMethods, staticMethods, Locals);
            foreach (var i in node)
            {
                if (i is StellarRoboLocalAstNode)
                {
                    var e = ((StellarRoboLocalAstNode)i).InitialExpression;
                    if (e != null) curryexp(e);
                }
                else if (i is StellarRoboReturnAstNode)
                {
                    var e = ((StellarRoboReturnAstNode)i).Value;
                    if (e != null) curryexp(e);
                }
                else if (i is StellarRoboCoroutineDeclareAstNode)
                {
                    var ie = ((StellarRoboCoroutineDeclareAstNode)i).InitialExpression;
                    var prm = ((StellarRoboCoroutineDeclareAstNode)i).ParameterExpressions;
                    curryexp(ie);
                    foreach (var j in prm) curryexp(j);
                }
                else if (i is StellarRoboIfAstNode)
                {
                    var ian = (StellarRoboIfAstNode)i;
                    curryexp(ian.IfBlock.Condition);
                    curryblk(ian.IfBlock.Children.ToList());
                    foreach (var j in ian.ElifBlocks)
                    {
                        curryexp(j.Condition);
                        curryblk(j.Children.ToList());
                    }
                    if (ian.ElseBlock != null) curryblk(ian.ElseBlock.Children.ToList());
                }
                else if (i is StellarRoboForAstNode)
                {
                    var fan = (StellarRoboForAstNode)i;
                    foreach (var j in fan.InitializeExpressions) curryexp(j);
                    curryexp(fan.Condition);
                    foreach (var j in fan.CounterExpressions) curryexp(j);
                    curryblk(fan.Children.ToList());
                }
                else if (i is StellarRoboForeachAstNode)
                {
                    var fean = (StellarRoboForeachAstNode)i;
                    curryexp(fean.Source);
                    curryblk(fean.Children.ToList());
                    foreach (var j in fean.CoroutineArguments) curryexp(j);
                }
                else if (i is StellarRoboLoopAstNode)
                {
                    var lan = (StellarRoboLoopAstNode)i;
                    curryexp(lan.Condition);
                    curryblk(lan.Children.ToList());
                }
                else if (i is StellarRoboExpressionAstNode)
                {
                    curryexp(i as StellarRoboExpressionAstNode);
                }
            }
        }

        private void ReplaceExpressionClassAccess(StellarRoboExpressionAstNode node, bool isStatic, string className, IList<string> instanceMethods, IList<string> staticMethods, IList<string> Locals)
        {
            Action<StellarRoboExpressionAstNode> curry = (p) => ReplaceExpressionClassAccess(p, isStatic, className, instanceMethods, staticMethods, Locals);
            if (node is StellarRoboBinaryExpressionAstNode)
            {
                var bn = (StellarRoboBinaryExpressionAstNode)node;
                curry(bn.FirstNode);
                curry(bn.SecondNode);
            }
            else if (node is StellarRoboFactorExpressionAstNode)
            {
                var f = (StellarRoboFactorExpressionAstNode)node;
                if (f.FactorType == StellarRoboFactorType.Identifer)
                {
                    if (!isStatic && (instanceMethods.Contains(f.StringValue) || Locals.Contains(f.StringValue)))
                    {
                        var newfac = new StellarRoboMemberAccessExpressionAstNode();
                        newfac.MemberName = f.StringValue;
                        newfac.Target = new StellarRoboFactorExpressionAstNode { FactorType = StellarRoboFactorType.Identifer, StringValue = "self" };
                        f.FactorType = StellarRoboFactorType.ParenExpression;
                        f.ExpressionNode = newfac;
                    }

                    if (staticMethods.Contains(f.StringValue))
                    {
                        //サブクラスも含む、そのうちね
                        var newfac = new StellarRoboMemberAccessExpressionAstNode();
                        newfac.MemberName = f.StringValue;
                        newfac.Target = new StellarRoboFactorExpressionAstNode { FactorType = StellarRoboFactorType.Identifer, StringValue = className };
                        f.FactorType = StellarRoboFactorType.ParenExpression;
                        f.ExpressionNode = newfac;
                    }
                }
            }
            else if (node is StellarRoboArgumentCallExpressionAstNode)
            {
                var ac = (StellarRoboArgumentCallExpressionAstNode)node;
                curry(ac.Target);
                foreach (var i in ac.Arguments) curry(i);
            }
            else if (node is StellarRoboPrimaryExpressionAstNode)
            {
                curry(((StellarRoboPrimaryExpressionAstNode)node).Target);
            }
            else if (node is StellarRoboUnaryExpressionAstNode)
            {
                curry(((StellarRoboUnaryExpressionAstNode)node).Target);
            }
        }

        private StellarRoboFunctionAstNode ParseFunction(Queue<StellarRoboToken> tokens, bool top)
        {
            var result = new StellarRoboFunctionAstNode();
            if (tokens.CheckSkipToken(StellarRoboTokenType.StaticKeyword))
            {
                if (top)
                {
                    throw new StellarRoboParseException(tokens.Dequeue().CreateErrorAt("トップレベルのメソッドにstaticは指定できません。"));
                }
                else
                {
                    result.StaticMethod = true;
                }
            }
            var nt = tokens.Dequeue();
            if (nt.Type != StellarRoboTokenType.Identifer) throw new StellarRoboParseException(nt.CreateErrorAt("メソッド名にはキーワードではない識別子を指定して下さい。"));
            result.Name = nt.TokenString;
            #region debug情報追加
            result.Line = nt.Position.Item2;
            result.StartPos = nt.Position.Item1 - nt.TokenString.Length;
            result.EndPos = nt.Position.Item1;
            #endregion
            ParseFunctionArgumentsList(tokens, result);

            if (!tokens.SkipLogicalLineBreak()) throw new StellarRoboParseException(nt.CreateErrorAt("func宣言の後ろに改行が必要です。"));
            foreach (var n in ParseBlock(tokens)) result.AddNode(n);
            if (!tokens.CheckSkipToken(StellarRoboTokenType.EndFuncKeyword)) throw new StellarRoboParseException(nt.CreateErrorAt("endfunc対応していません。"));
            return result;
        }

        private static void ParseFunctionArgumentsList(Queue<StellarRoboToken> tokens, StellarRoboFunctionAstNode result)
        {
            var nt = tokens.Peek();
            switch (nt.Type)
            {
                case StellarRoboTokenType.NewLine:
                case StellarRoboTokenType.Semicolon:
                    break;
                case StellarRoboTokenType.ParenStart:
                    tokens.Dequeue();
                    while (true)
                    {
                        nt = tokens.Dequeue();
                        switch (nt.Type)
                        {
                            case StellarRoboTokenType.Identifer:
                                result.Parameters.Add(nt.TokenString);
                                tokens.CheckSkipToken(StellarRoboTokenType.Comma);
                                break;
                            case StellarRoboTokenType.VariableArguments:
                                result.AllowsVariableArguments = true;
                                if (!tokens.CheckToken(StellarRoboTokenType.ParenEnd)) throw new StellarRoboParseException(nt.CreateErrorAt("可変長引数は最後に配置して下さい。"));
                                break;
                            case StellarRoboTokenType.ParenEnd:
                                goto EndArgsList;
                            default:
                                throw new StellarRoboParseException(nt.CreateErrorAt("仮引数リストに識別子・可変長引数以外を指定しないで下さい。"));
                        }

                    }
                EndArgsList:;
                    break;
            }
        }

        private IList<StellarRoboAstNode> ParseBlock(Queue<StellarRoboToken> tokens)
        {
            var result = new List<StellarRoboAstNode>();
            while (true)
            {
                tokens.SkipLogicalLineBreak();
                //式かもしれないのでPeek
                var nt = tokens.Peek();
                switch (nt.Type)
                {
                    case StellarRoboTokenType.IfKeyword:
                        tokens.Dequeue();
                        result.Add(ParseIf(tokens, false));
                        break;
                    case StellarRoboTokenType.CaseKeyword:
                        tokens.Dequeue();
                        result.Add(ParseCase(tokens));
                        break;
                    case StellarRoboTokenType.ForKeyword:
                        tokens.Dequeue();
                        result.Add(ParseFor(tokens, false));
                        break;
                    case StellarRoboTokenType.WhileKeyword:
                        tokens.Dequeue();
                        result.Add(ParseWhile(tokens, false));
                        break;
                    case StellarRoboTokenType.ForeachKeyword:
                        tokens.Dequeue();
                        result.Add(ParseForeach(tokens, false));
                        break;
                    case StellarRoboTokenType.DoKeyword:
                        break;
                    case StellarRoboTokenType.ElifKeyword:
                    case StellarRoboTokenType.ElseKeyword:
                    case StellarRoboTokenType.EndIfKeyword:
                    case StellarRoboTokenType.WhenKeyword:
                    case StellarRoboTokenType.DefaultKeyword:
                    case StellarRoboTokenType.EndCaseKeyword:
                    case StellarRoboTokenType.EndFuncKeyword:
                    case StellarRoboTokenType.NextKeyword:
                        //呼ばれ元で終了判定するから飛ばさないでね
                        goto EndBlock;
                    default:
                        result.AddRange(ParseSingleLineStatement(tokens));
                        break;
                }
            }
        EndBlock: return result;
        }

        private IList<StellarRoboAstNode> ParseSingleLineStatement(Queue<StellarRoboToken> tokens)
        {
            var result = new List<StellarRoboAstNode>();
            var nt = tokens.Peek();
            var l = "";
            switch (nt.Type)
            {
                //loop系
                case StellarRoboTokenType.BreakKeyword:
                    tokens.Dequeue();
                    var bl = tokens.Peek();
                    if (bl.Type == StellarRoboTokenType.Identifer)
                    {
                        tokens.Dequeue();
                        l = bl.TokenString;
                    }
                    result.Add(new StellarRoboContinueAstNode { Type = StellarRoboAstNodeType.BreakStatement, Label = l });
                    break;
                case StellarRoboTokenType.ContinueKeyword:
                    tokens.Dequeue();
                    var cl = tokens.Peek();
                    if (cl.Type == StellarRoboTokenType.Identifer)
                    {
                        tokens.Dequeue();
                        l = cl.TokenString;
                    }
                    result.Add(new StellarRoboContinueAstNode { Label = l });
                    break;
                //return系
                case StellarRoboTokenType.ReturnKeyword:
                case StellarRoboTokenType.YieldKeyword:
                    result.Add(ParseReturn(tokens));
                    break;
                //declare系
                case StellarRoboTokenType.CoroutineKeyword:
                    tokens.Dequeue();
                    result.AddRange(ParseCoroutineDeclare(tokens));
                    break;
                case StellarRoboTokenType.LocalKeyword:
                    tokens.Dequeue();
                    result.AddRange(ParseLocal(tokens));
                    break;
                //単行制御構造系
                //ブロックからは呼ばれないはず
                case StellarRoboTokenType.IfKeyword:
                    tokens.Dequeue();
                    result.Add(ParseIf(tokens, true));
                    break;
                case StellarRoboTokenType.ForKeyword:
                    tokens.Dequeue();
                    result.Add(ParseFor(tokens, true));
                    break;
                case StellarRoboTokenType.WhileKeyword:
                    tokens.Dequeue();
                    result.Add(ParseWhile(tokens, true));
                    break;
                case StellarRoboTokenType.ForeachKeyword:
                    tokens.Dequeue();
                    result.Add(ParseForeach(tokens, true));
                    break;

                case StellarRoboTokenType.Semicolon:
                case StellarRoboTokenType.NewLine:
                    throw new StellarRoboParseException(nt.CreateErrorAt("ステートメントが空です。"));
                default:
                    var exp = ParseExpression(tokens);
                    if (!CheckStatementExpression(exp)) throw new StellarRoboParseException(nt.CreateErrorAt("ステートメントにできない式です。"));
                    result.Add(exp);
                    break;
            }
            return result;
        }

        private bool CheckStatementExpression(StellarRoboExpressionAstNode node)
        {
            if (node is StellarRoboArgumentCallExpressionAstNode &&
                (node as StellarRoboArgumentCallExpressionAstNode).ExpressionType == StellarRoboOperatorType.FunctionCall)
                return true;

            var bn = node as StellarRoboBinaryExpressionAstNode;
            if (bn == null) return false;
            switch (bn.ExpressionType)
            {
                case StellarRoboOperatorType.Assign:
                case StellarRoboOperatorType.PlusAssign:
                case StellarRoboOperatorType.MinusAssign:
                case StellarRoboOperatorType.MultiplyAssign:
                case StellarRoboOperatorType.DivideAssign:
                case StellarRoboOperatorType.AndAssign:
                case StellarRoboOperatorType.OrAssign:
                case StellarRoboOperatorType.XorAssign:
                case StellarRoboOperatorType.LeftBitShiftAssign:
                case StellarRoboOperatorType.RightBitShiftAssign:
                case StellarRoboOperatorType.NilAssign:
                    return true;
            }
            return false;
        }

        private IList<StellarRoboLocalAstNode> ParseLocal(Queue<StellarRoboToken> tokens)
        {
            var result = new List<StellarRoboLocalAstNode>();
            while (true)
            {
                var nt = tokens.Dequeue();
                if (nt.Type != StellarRoboTokenType.Identifer) throw new StellarRoboParseException(nt.CreateErrorAt("識別子を指定して下さい。"));
                var lan = new StellarRoboLocalAstNode();
                lan.Name = nt.TokenString;
                result.Add(lan);
                if (tokens.SkipLogicalLineBreak()) return result;
                nt = tokens.Dequeue();
                switch (nt.Type)
                {
                    case StellarRoboTokenType.Assign:
                        lan.InitialExpression = ParseExpression(tokens);
                        if (tokens.SkipLogicalLineBreak()) return result;
                        if (!tokens.CheckSkipToken(StellarRoboTokenType.Comma)) throw new StellarRoboParseException(nt.CreateErrorAt("無効なlocal宣言です。"));
                        tokens.SkipLogicalLineBreak();
                        break;
                    case StellarRoboTokenType.Comma:
                        tokens.SkipLogicalLineBreak();
                        continue;
                    default:
                        throw new StellarRoboParseException(nt.CreateErrorAt("無効なlocal宣言です。"));
                }
            }
        }

        private IList<StellarRoboCoroutineDeclareAstNode> ParseCoroutineDeclare(Queue<StellarRoboToken> tokens)
        {
            var result = new List<StellarRoboCoroutineDeclareAstNode>();
            while (true)
            {
                var nt = tokens.Dequeue();
                if (nt.Type != StellarRoboTokenType.Identifer) throw new StellarRoboParseException(nt.CreateErrorAt("識別子を指定して下さい。"));
                var lan = new StellarRoboCoroutineDeclareAstNode();
                lan.Name = nt.TokenString;
                nt = tokens.Dequeue();
                if (nt.Type != StellarRoboTokenType.Assign) throw new StellarRoboParseException(nt.CreateErrorAt("coroutine宣言は必ず代入して下さい。"));
                lan.InitialExpression = ParseExpression(tokens);

                if (tokens.CheckSkipToken(StellarRoboTokenType.Colon))
                {
                    //引数
                    if (!tokens.CheckSkipToken(StellarRoboTokenType.ParenStart)) throw new StellarRoboParseException(nt.CreateErrorAt("coroutine宣言の引数リストが不正です。"));
                    tokens.SkipLogicalLineBreak();
                    while (true)
                    {
                        lan.ParameterExpressions.Add(ParseExpression(tokens));
                        if (tokens.CheckSkipToken(StellarRoboTokenType.ParenEnd)) break;
                        if (!tokens.CheckSkipToken(StellarRoboTokenType.Comma)) throw new StellarRoboParseException(nt.CreateErrorAt("coroutine宣言の引数リストが閉じていません。"));
                        tokens.SkipLogicalLineBreak();
                    }
                }
                result.Add(lan);
                if (!tokens.CheckSkipToken(StellarRoboTokenType.Comma)) break;
            }
            return result;
        }

        private StellarRoboReturnAstNode ParseReturn(Queue<StellarRoboToken> tokens)
        {
            var result = new StellarRoboReturnAstNode();
            var nt = tokens.Dequeue();
            if (nt.Type == StellarRoboTokenType.ReturnKeyword)
            {
                result.Type = StellarRoboAstNodeType.ReturnStatement;
                #region debug情報追加
                result.Line = nt.Position.Item2;
                result.StartPos = nt.Position.Item1 - nt.TokenString.Length;
                result.EndPos = nt.Position.Item1;
                #endregion
            }
            else
            {
                result.Type = StellarRoboAstNodeType.YieldStatement;
            }
            //result.Type = nt.Type == StellarRoboTokenType.ReturnKeyword ? StellarRoboAstNodeType.ReturnStatement : StellarRoboAstNodeType.YieldStatement;
            if (!tokens.SkipLogicalLineBreak()) result.Value = ParseExpression(tokens);
            return result;
        }

        private StellarRoboIfAstNode ParseIf(Queue<StellarRoboToken> tokens, bool single)
        {
            var result = new StellarRoboIfAstNode();
            if (!tokens.CheckSkipToken(StellarRoboTokenType.ParenStart)) throw new StellarRoboParseException(tokens.Peek().CreateErrorAt("if文の条件式は括弧で括って下さい。"));
            tokens.SkipLogicalLineBreak();
            var cnd = ParseExpression(tokens);
            tokens.SkipLogicalLineBreak();
            var ifb = new StellarRoboIfBlockAstNode();
            ifb.Condition = cnd;
            if (!tokens.CheckSkipToken(StellarRoboTokenType.ParenEnd)) throw new StellarRoboParseException(tokens.Peek().CreateErrorAt("if文の条件式が閉じていません。"));
            if (!tokens.CheckSkipToken(StellarRoboTokenType.ThenKeyword)) throw new StellarRoboParseException(tokens.Peek().CreateErrorAt("thenキーワードをおいて下さい。"));
            if (!single && tokens.SkipLogicalLineBreak())
            {
                //ブロックif
                var b = ParseBlock(tokens);
                foreach (var i in b) ifb.AddNode(i);
                result.IfBlock = ifb;
                while (true)
                {
                    var nt = tokens.Dequeue();
                    if (nt.Type == StellarRoboTokenType.EndIfKeyword)
                    {
                        break;
                    }
                    else if (nt.Type == StellarRoboTokenType.ElifKeyword)
                    {
                        if (!tokens.CheckSkipToken(StellarRoboTokenType.ParenStart)) throw new StellarRoboParseException(tokens.Peek().CreateErrorAt("elseif文の条件式は括弧で括って下さい。"));
                        tokens.SkipLogicalLineBreak();
                        cnd = ParseExpression(tokens);
                        var elb = new StellarRoboIfBlockAstNode();
                        tokens.SkipLogicalLineBreak();
                        elb.Condition = cnd;
                        if (!tokens.CheckSkipToken(StellarRoboTokenType.ParenEnd)) throw new StellarRoboParseException(tokens.Peek().CreateErrorAt("elseif文の条件式が閉じていません。"));
                        if (!tokens.CheckSkipToken(StellarRoboTokenType.ThenKeyword)) throw new StellarRoboParseException(tokens.Peek().CreateErrorAt("thenキーワードをおいて下さい。"));
                        tokens.SkipLogicalLineBreak();
                        b = ParseBlock(tokens);
                        foreach (var i in b) elb.AddNode(i);
                        result.ElifBlocks.Add(elb);
                    }
                    else if (nt.Type == StellarRoboTokenType.ElseKeyword)
                    {
                        tokens.SkipLogicalLineBreak();
                        var esb = new StellarRoboIfBlockAstNode();
                        b = ParseBlock(tokens);
                        foreach (var i in b) esb.AddNode(i);
                        result.ElseBlock = esb;
                    }
                    else
                    {
                        throw new StellarRoboParseException(nt.CreateErrorAt("不正なif文です。"));
                    }
                }
            }
            else
            {
                //単行if
                ifb.AddNode(ParseSingleLineStatement(tokens));
                result.IfBlock = ifb;
                if (tokens.CheckSkipToken(StellarRoboTokenType.ElseKeyword))
                {
                    var esb = new StellarRoboIfBlockAstNode();
                    esb.AddNode(ParseSingleLineStatement(tokens));
                    result.ElseBlock = esb;
                }
            }
            return result;
        }

        private StellarRoboIfAstNode ParseCase(Queue<StellarRoboToken> tokens)
        {
            var result = new StellarRoboIfAstNode();
            var ifb = new StellarRoboIfBlockAstNode();
            ifb.Condition = new StellarRoboFactorExpressionAstNode { FactorType = StellarRoboFactorType.BooleanValue, BooleanValue = false };
            result.IfBlock = ifb;
            if (!tokens.CheckSkipToken(StellarRoboTokenType.ParenStart)) throw new StellarRoboParseException(tokens.Peek().CreateErrorAt("case文の判定式は括弧で括って下さい。"));
            tokens.SkipLogicalLineBreak();
            var target = ParseExpression(tokens);
            tokens.SkipLogicalLineBreak();
            if (!tokens.CheckSkipToken(StellarRoboTokenType.ParenEnd)) throw new StellarRoboParseException(tokens.Peek().CreateErrorAt("case文の判定式は括弧で括って下さい。"));
            if (!tokens.SkipLogicalLineBreak()) throw new StellarRoboParseException(tokens.Peek().CreateErrorAt("case文の判定式の後は改行して下さい。"));
            var wls = new List<StellarRoboExpressionAstNode>();
            var df = false;
            while (true)
            {
                var nt = tokens.Peek();
                if (nt.Type == StellarRoboTokenType.EndCaseKeyword)
                {
                    tokens.Dequeue();
                    break;
                }
                else if (nt.Type == StellarRoboTokenType.WhenKeyword)
                {
                    tokens.Dequeue();
                    var t2 = ParseExpression(tokens);
                    if (!tokens.CheckSkipToken(StellarRoboTokenType.Colon)) throw new StellarRoboParseException(nt.CreateErrorAt("whenの式の後ろはコロンを付けて下さい。"));
                    wls.Add(t2);
                    tokens.SkipLogicalLineBreak();
                    continue;
                }
                else if (nt.Type == StellarRoboTokenType.DefaultKeyword)
                {
                    tokens.Dequeue();
                    if (!tokens.CheckSkipToken(StellarRoboTokenType.Colon)) throw new StellarRoboParseException(nt.CreateErrorAt("defaultの後ろはコロンを付けて下さい。"));
                    df = true;
                    continue;
                }
                else
                {
                    var bl = ParseBlock(tokens);
                    if (wls.Count == 0)
                    {
                        if (!df) throw new StellarRoboParseException(nt.CreateErrorAt("case文内でブロックが浮いています。"));
                        var en = new StellarRoboIfBlockAstNode();
                        foreach (var j in bl) en.AddNode(j);
                        result.ElseBlock = en;
                        df = false;
                    }
                    else
                    {
                        var tn = new StellarRoboBinaryExpressionAstNode();
                        tn.ExpressionType = StellarRoboOperatorType.OrElse;
                        tn.FirstNode = new StellarRoboBinaryExpressionAstNode { ExpressionType = StellarRoboOperatorType.Equal, FirstNode = target, SecondNode = wls[0] };
                        var eln = new StellarRoboIfBlockAstNode();
                        foreach (var i in wls.Skip(1))
                        {
                            var nc = new StellarRoboBinaryExpressionAstNode { ExpressionType = StellarRoboOperatorType.Equal, FirstNode = target, SecondNode = i };
                            tn.SecondNode = nc;
                            var ntn = new StellarRoboBinaryExpressionAstNode();
                            ntn.FirstNode = tn;
                            ntn.ExpressionType = StellarRoboOperatorType.OrElse;
                            tn = ntn;

                        }
                        eln.Condition = tn.FirstNode;
                        foreach (var j in bl) eln.AddNode(j);
                        result.ElifBlocks.Add(eln);
                        if (df)
                        {
                            result.ElseBlock = eln;
                            df = false;
                        }
                        wls.Clear();
                    }
                }
            }
            return result;
        }


        private StellarRoboForAstNode ParseFor(Queue<StellarRoboToken> tokens, bool single)
        {
            var result = new StellarRoboForAstNode();
            var t = tokens.Peek();
            if (t.Type == StellarRoboTokenType.Identifer)
            {
                tokens.Dequeue();
                result.Name = t.TokenString;
            }
            else
            {
                result.Name = Guid.NewGuid().ToString().Substring(0, 8);
            }
            if (!tokens.CheckSkipToken(StellarRoboTokenType.ParenStart)) throw new StellarRoboParseException(tokens.Dequeue().CreateErrorAt("forの各式は全体を括弧で括って下さい。"));
            tokens.SkipLogicalLineBreak();
            while (true)
            {
                var exp = ParseExpression(tokens);
                result.InitializeExpressions.Add(exp);
                if (tokens.CheckSkipToken(StellarRoboTokenType.Semicolon)) break;
                if (!tokens.CheckSkipToken(StellarRoboTokenType.Comma)) throw new StellarRoboParseException(tokens.Dequeue().CreateErrorAt("初期化式はコンマで区切って下さい。"));
                tokens.SkipLogicalLineBreak();
            }
            tokens.SkipLogicalLineBreak();
            result.Condition = ParseExpression(tokens);
            if (!tokens.CheckSkipToken(StellarRoboTokenType.Semicolon)) throw new StellarRoboParseException(tokens.Dequeue().CreateErrorAt("セミコロンで区切って下さい。"));
            tokens.SkipLogicalLineBreak();
            while (true)
            {
                var exp = ParseExpression(tokens);
                result.CounterExpressions.Add(exp);
                tokens.SkipLogicalLineBreak();
                if (tokens.CheckToken(StellarRoboTokenType.ParenEnd)) break;
                if (!tokens.CheckSkipToken(StellarRoboTokenType.Comma)) throw new StellarRoboParseException(tokens.Dequeue().CreateErrorAt("初期化式はコンマで区切って下さい。"));
                tokens.SkipLogicalLineBreak();
            }
            if (!tokens.CheckSkipToken(StellarRoboTokenType.ParenEnd)) throw new StellarRoboParseException(tokens.Dequeue().CreateErrorAt("forの各式は全体を括弧で括って下さい。"));
            if (single || !tokens.SkipLogicalLineBreak())
            {
                if (tokens.SkipLogicalLineBreak()) throw new StellarRoboParseException(tokens.Dequeue().CreateErrorAt("単行for文のみ記述できます。"));
                foreach (var i in ParseSingleLineStatement(tokens)) result.AddNode(i);
            }
            else
            {
                foreach (var i in ParseBlock(tokens)) result.AddNode(i);
                if (!tokens.CheckSkipToken(StellarRoboTokenType.NextKeyword)) throw new StellarRoboParseException(tokens.Dequeue().CreateErrorAt("nextで終わっていません。"));
            }
            return result;
        }

        private StellarRoboLoopAstNode ParseWhile(Queue<StellarRoboToken> tokens, bool single)
        {
            var result = new StellarRoboLoopAstNode();
            var t = tokens.Peek();
            if (t.Type == StellarRoboTokenType.Identifer)
            {
                tokens.Dequeue();
                result.Name = t.TokenString;
            }
            else
            {
                result.Name = Guid.NewGuid().ToString().Substring(0, 8);
            }
            if (!tokens.CheckSkipToken(StellarRoboTokenType.ParenStart)) throw new StellarRoboParseException(tokens.Dequeue().CreateErrorAt("whileの条件式は括弧で括って下さい。"));
            tokens.SkipLogicalLineBreak();
            result.Condition = ParseExpression(tokens);
            tokens.SkipLogicalLineBreak();
            if (!tokens.CheckSkipToken(StellarRoboTokenType.ParenEnd)) throw new StellarRoboParseException(tokens.Dequeue().CreateErrorAt("whileの条件式は括弧で括って下さい。"));
            if (single || !tokens.SkipLogicalLineBreak())
            {
                if (tokens.SkipLogicalLineBreak()) throw new StellarRoboParseException(tokens.Dequeue().CreateErrorAt("単行while文のみ記述できます。"));
                foreach (var i in ParseSingleLineStatement(tokens)) result.AddNode(i);
            }
            else
            {
                foreach (var i in ParseBlock(tokens)) result.AddNode(i);
                if (!tokens.CheckSkipToken(StellarRoboTokenType.NextKeyword)) throw new StellarRoboParseException(tokens.Dequeue().CreateErrorAt("nextで終わっていません。"));
            }
            return result;
        }
        private StellarRoboLoopAstNode ParseForeach(Queue<StellarRoboToken> tokens, bool single)
        {
            var result = new StellarRoboForeachAstNode();
            var t = tokens.Peek();
            if (t.Type == StellarRoboTokenType.Identifer)
            {
                tokens.Dequeue();
                result.Name = t.TokenString;
            }
            else
            {
                result.Name = Guid.NewGuid().ToString().Substring(0, 8);
            }
            if (!tokens.CheckSkipToken(StellarRoboTokenType.ParenStart)) throw new StellarRoboParseException(tokens.Dequeue().CreateErrorAt("whileの条件式は括弧で括って下さい。"));
            tokens.SkipLogicalLineBreak();
            var nt = tokens.Dequeue();
            if (nt.Type != StellarRoboTokenType.Identifer) throw new StellarRoboParseException(nt.CreateErrorAt("foreachのループ変数には識別子を指定して下さい。"));
            result.ElementVariableName = nt.TokenString;
            tokens.SkipLogicalLineBreak();
            nt = tokens.Dequeue();
            tokens.SkipLogicalLineBreak();
            if (nt.Type == StellarRoboTokenType.InKeyword)
            {
                result.Source = ParseExpression(tokens);
            }
            else if (nt.Type == StellarRoboTokenType.OfKeyword)
            {
                result.IsCoroutineSource = true;
                result.Source = ParseExpression(tokens);
                if (tokens.CheckSkipToken(StellarRoboTokenType.Colon))
                {
                    if (!tokens.CheckSkipToken(StellarRoboTokenType.ParenStart)) throw new StellarRoboParseException(nt.CreateErrorAt("コルーチンの引数リストが不正です。"));
                    tokens.SkipLogicalLineBreak();
                    while (true)
                    {
                        result.CoroutineArguments.Add(ParseExpression(tokens));
                        tokens.SkipLogicalLineBreak();
                        if (tokens.CheckSkipToken(StellarRoboTokenType.ParenEnd)) break;
                        if (!tokens.CheckSkipToken(StellarRoboTokenType.Comma)) throw new StellarRoboParseException(nt.CreateErrorAt("コルーチンの引数リストが閉じていません。"));
                        tokens.SkipLogicalLineBreak();
                    }
                }
            }
            else
            {
                throw new StellarRoboParseException(nt.CreateErrorAt("foreachにはinかofを指定して下さい。"));
            }
            tokens.SkipLogicalLineBreak();
            if (!tokens.CheckSkipToken(StellarRoboTokenType.ParenEnd)) throw new StellarRoboParseException(tokens.Dequeue().CreateErrorAt("foreachの条件式は括弧で括って下さい。"));
            if (single || !tokens.SkipLogicalLineBreak())
            {
                if (tokens.SkipLogicalLineBreak()) throw new StellarRoboParseException(tokens.Dequeue().CreateErrorAt("単行foreach文のみ記述できます。"));
                foreach (var i in ParseSingleLineStatement(tokens)) result.AddNode(i);
            }
            else
            {
                foreach (var i in ParseBlock(tokens)) result.AddNode(i);
                if (!tokens.CheckSkipToken(StellarRoboTokenType.NextKeyword)) throw new StellarRoboParseException(tokens.Dequeue().CreateErrorAt("nextで終わっていません。"));
            }
            return result;
        }

        private StellarRoboExpressionAstNode ParseExpression(Queue<StellarRoboToken> tokens)
            => ParseBinaryExpression(tokens, 1);

        private StellarRoboExpressionAstNode ParseBinaryExpression(Queue<StellarRoboToken> tokens, int priority)
        {
            if (priority > OperatorMaxPriority) return ParseUnaryExpression(tokens);
            var left = ParseBinaryExpression(tokens, priority + 1);
            var result = new StellarRoboBinaryExpressionAstNode();
            result.FirstNode = left;
            while (true)
            {
                if (tokens.Count == 0) break;
                if (tokens.CheckToken(
                    StellarRoboTokenType.ParenEnd, StellarRoboTokenType.Comma, StellarRoboTokenType.BracketEnd,
                    StellarRoboTokenType.ThenKeyword, StellarRoboTokenType.ElseKeyword, StellarRoboTokenType.Semicolon,
                    StellarRoboTokenType.NewLine, StellarRoboTokenType.Colon))
                {
                    //tokens.Dequeue();
                    break;
                }
                var nt = tokens.Peek();
                if (!OperatorPriorities.ContainsKey(nt.Type)) throw new StellarRoboParseException(nt.CreateErrorAt($"演算子ではなく{nt.Type}が検出されました。"));
                if (OperatorPriorities[nt.Type] != priority) break;
                tokens.Dequeue();
                tokens.SkipLogicalLineBreak();
                var right = ParseBinaryExpression(tokens, priority + 1);

                result.SecondNode = right;
                result.ExpressionType = OperatorsTokenTable[nt.Type];
                var newres = new StellarRoboBinaryExpressionAstNode();
                newres.FirstNode = result;
                result = newres;
            }
            if (priority == 1)
            {
                var pn = result.FirstNode as StellarRoboBinaryExpressionAstNode;
                if (pn == null) return result.FirstNode;
                while (pn.FirstNode is StellarRoboBinaryExpressionAstNode)
                {
                    switch (pn.ExpressionType)
                    {
                        case StellarRoboOperatorType.Assign:
                        case StellarRoboOperatorType.PlusAssign:
                        case StellarRoboOperatorType.MinusAssign:
                        case StellarRoboOperatorType.MultiplyAssign:
                        case StellarRoboOperatorType.DivideAssign:
                        case StellarRoboOperatorType.AndAssign:
                        case StellarRoboOperatorType.OrAssign:
                        case StellarRoboOperatorType.XorAssign:
                        case StellarRoboOperatorType.ModularAssign:
                        case StellarRoboOperatorType.LeftBitShiftAssign:
                        case StellarRoboOperatorType.RightBitShiftAssign:
                        case StellarRoboOperatorType.NilAssign:
                            break;
                        default:
                            return pn;
                    }
                    var kb = pn.FirstNode as StellarRoboBinaryExpressionAstNode;
                    var nn = new StellarRoboBinaryExpressionAstNode();
                    nn.ExpressionType = pn.ExpressionType;
                    nn.SecondNode = pn.SecondNode;
                    nn.FirstNode = kb.SecondNode;
                    pn.FirstNode = kb.FirstNode;
                    pn.SecondNode = nn;
                }
                return pn;
            }
            return result.FirstNode;
        }

        private StellarRoboUnaryExpressionAstNode ParseUnaryExpression(Queue<StellarRoboToken> tokens)
        {
            var ut = tokens.Peek();
            switch (ut.Type)
            {
                case StellarRoboTokenType.Minus:
                    tokens.Dequeue();
                    var mue = new StellarRoboUnaryExpressionAstNode();
                    mue.Target = ParsePrimaryExpression(tokens);
                    mue.ExpressionType = StellarRoboOperatorType.Minus;
                    return mue;
                case StellarRoboTokenType.Not:
                    tokens.Dequeue();
                    var nue = new StellarRoboUnaryExpressionAstNode();
                    nue.Target = ParsePrimaryExpression(tokens);
                    nue.ExpressionType = StellarRoboOperatorType.Not;
                    return nue;
                case StellarRoboTokenType.Increment:
                    tokens.Dequeue();
                    var iue = new StellarRoboUnaryExpressionAstNode();
                    iue.Target = ParsePrimaryExpression(tokens);
                    iue.ExpressionType = StellarRoboOperatorType.Increment;
                    return iue;
                case StellarRoboTokenType.Decrement:
                    tokens.Dequeue();
                    var due = new StellarRoboUnaryExpressionAstNode();
                    due.Target = ParsePrimaryExpression(tokens);
                    due.ExpressionType = StellarRoboOperatorType.Decrement;
                    return due;
                case StellarRoboTokenType.Plus:
                    tokens.Dequeue();
                    goto default;
                default:
                    var pe = ParsePrimaryExpression(tokens);
                    return pe;
            }
        }

        /// <summary>
        /// 一次式の処理
        /// </summary>
        /// <param name="tokens"></param>
        /// <returns></returns>
        private StellarRoboPrimaryExpressionAstNode ParsePrimaryExpression(Queue<StellarRoboToken> tokens)
        {
            var factor = ParseFactorExpression(tokens);
            //tokens.SkipLogicalLineBreak();
            var re = ParsePrimaryRecursiveExpression(tokens, factor);
            if (re != factor) return re;
            if (!tokens.CheckToken(StellarRoboTokenType.Increment, StellarRoboTokenType.Decrement)) return factor;
            re = new StellarRoboPrimaryExpressionAstNode();
            re.Target = factor;
            if (tokens.CheckSkipToken(StellarRoboTokenType.Increment)) re.ExpressionType = StellarRoboOperatorType.Increment;
            if (tokens.CheckSkipToken(StellarRoboTokenType.Decrement)) re.ExpressionType = StellarRoboOperatorType.Decrement;
            return re;
        }

        /// <summary>
        /// 再帰的に連続させられる一次式の処理
        /// </summary>
        /// <param name="tokens"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        private StellarRoboPrimaryExpressionAstNode ParsePrimaryRecursiveExpression(Queue<StellarRoboToken> tokens, StellarRoboPrimaryExpressionAstNode parent)
        {
            var result = parent;
            if (!tokens.CheckToken(StellarRoboTokenType.Period, StellarRoboTokenType.ParenStart, StellarRoboTokenType.BracketStart)) return result;
            while (true)
            {
                if (tokens.CheckSkipToken(StellarRoboTokenType.Period))
                {
                    result = ParsePrimaryMemberAccessExpression(tokens, result);
                }
                else if (tokens.CheckSkipToken(StellarRoboTokenType.ParenStart))
                {
                    result = ParsePrimaryFunctionCallExpression(tokens, result);
                }
                else if (tokens.CheckSkipToken(StellarRoboTokenType.BracketStart))
                {
                    result = ParsePrimaryIndexerAccessExpression(tokens, result);
                }
                else
                {
                    break;
                }
            }
            return result;
        }

        /// <summary>
        /// メンバーアクセス処理
        /// </summary>
        /// <param name="tokens"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        private StellarRoboMemberAccessExpressionAstNode ParsePrimaryMemberAccessExpression(Queue<StellarRoboToken> tokens, StellarRoboPrimaryExpressionAstNode parent)
        {
            var mnt = tokens.Dequeue();
            if (mnt.Type != StellarRoboTokenType.Identifer) throw new StellarRoboParseException(mnt.CreateErrorAt("有効なメンバー名を指定して下さい。"));
            var r = new StellarRoboMemberAccessExpressionAstNode();
            r.Target = parent;
            r.ExpressionType = StellarRoboOperatorType.MemberAccess;
            r.MemberName = mnt.TokenString;
            return r;
        }

        /// <summary>
        /// メソッド呼び出しの引数リスト処理
        /// </summary>
        /// <param name="tokens"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        private StellarRoboArgumentCallExpressionAstNode ParsePrimaryFunctionCallExpression(Queue<StellarRoboToken> tokens, StellarRoboPrimaryExpressionAstNode parent)
        {
            var r = new StellarRoboArgumentCallExpressionAstNode();
            r.Target = parent;
            r.ExpressionType = StellarRoboOperatorType.FunctionCall;
            tokens.SkipLogicalLineBreak();
            if (tokens.CheckSkipToken(StellarRoboTokenType.ParenEnd)) return r;
            while (true)
            {
                r.Arguments.Add(ParseExpression(tokens));
                if (tokens.CheckSkipToken(StellarRoboTokenType.Comma)) continue;
                tokens.SkipLogicalLineBreak();
                if (tokens.CheckSkipToken(StellarRoboTokenType.ParenEnd)) break;
                throw new StellarRoboParseException(tokens.Peek().CreateErrorAt("メソッド呼び出しの引数リストが無効です。"));
            }
            return r;
        }

        /// <summary>
        /// インデクサの引数リスト処理
        /// </summary>
        /// <param name="tokens"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        private StellarRoboArgumentCallExpressionAstNode ParsePrimaryIndexerAccessExpression(Queue<StellarRoboToken> tokens, StellarRoboPrimaryExpressionAstNode parent)
        {
            var r = new StellarRoboArgumentCallExpressionAstNode();
            r.Target = parent;
            r.ExpressionType = StellarRoboOperatorType.IndexerAccess;
            tokens.SkipLogicalLineBreak();
            if (tokens.CheckSkipToken(StellarRoboTokenType.BracketEnd)) return r;
            while (true)
            {
                r.Arguments.Add(ParseExpression(tokens));
                if (tokens.CheckSkipToken(StellarRoboTokenType.Comma)) continue;
                tokens.SkipLogicalLineBreak();
                if (tokens.CheckSkipToken(StellarRoboTokenType.BracketEnd)) break;
                throw new StellarRoboParseException(tokens.Peek().CreateErrorAt("メソッド呼び出しの引数リストが無効です。"));
            }
            return r;
        }

        private StellarRoboFactorExpressionAstNode ParseFactorExpression(Queue<StellarRoboToken> tokens)
        {
            var t = tokens.Dequeue();
            string lv = "";
            switch (t.Type)
            {
                case StellarRoboTokenType.CoresumeKeyword:
                    if (!tokens.CheckSkipToken(StellarRoboTokenType.ParenStart)) throw new StellarRoboParseException(t.CreateErrorAt("coresumeが不正です。"));
                    t = tokens.Dequeue();
                    if (t.Type != StellarRoboTokenType.Identifer) throw new StellarRoboParseException(t.CreateErrorAt("coresumeが不正です。"));
                    var result = new StellarRoboFactorExpressionAstNode { FactorType = StellarRoboFactorType.CoroutineResume, StringValue = t.TokenString };
                    if (tokens.CheckSkipToken(StellarRoboTokenType.Comma))
                    {
                        //代入とステート返却
                        result.ExpressionNode = ParseExpression(tokens);
                        result.BooleanValue = true;
                    }
                    if (!tokens.CheckSkipToken(StellarRoboTokenType.ParenEnd)) throw new StellarRoboParseException(t.CreateErrorAt("coresumeが不正です。"));
                    return result;
                case StellarRoboTokenType.And:
                    var lambda = new StellarRoboFactorExpressionAstNode();
                    lambda.FactorType = StellarRoboFactorType.Lambda;
                    if (!tokens.CheckSkipToken(StellarRoboTokenType.ParenStart)) throw new StellarRoboParseException(t.CreateErrorAt("ラムダ式の&には引数リストを続けて下さい。"));
                    if (!tokens.CheckSkipToken(StellarRoboTokenType.ParenEnd))
                        while (true)
                        {
                            lambda.ElementNodes.Add(new StellarRoboFactorExpressionAstNode { FactorType = StellarRoboFactorType.Identifer, StringValue = tokens.Dequeue().TokenString });
                            if (tokens.CheckSkipToken(StellarRoboTokenType.ParenEnd)) break;
                            if (!tokens.CheckSkipToken(StellarRoboTokenType.Comma)) throw new StellarRoboParseException(tokens.Peek().CreateErrorAt("ラムダ引数が括弧で閉じていません。"));
                        }
                    if (!tokens.CheckSkipToken(StellarRoboTokenType.Lambda)) throw new StellarRoboParseException(t.CreateErrorAt("ラムダ式の引数リストに=>で式を続けて下さい。"));
                    lambda.ExpressionNode = ParseExpression(tokens);
                    return lambda;
                case StellarRoboTokenType.ParenStart:
                    tokens.SkipLogicalLineBreak();
                    var exp = ParseExpression(tokens);
                    tokens.SkipLogicalLineBreak();
                    if (!tokens.CheckSkipToken(StellarRoboTokenType.ParenEnd)) throw new StellarRoboParseException(tokens.Peek().CreateErrorAt("括弧は閉じて下さい。"));
                    return new StellarRoboFactorExpressionAstNode { FactorType = StellarRoboFactorType.ParenExpression, ExpressionNode = exp };
                case StellarRoboTokenType.BracketStart:
                    var are = new StellarRoboFactorExpressionAstNode { FactorType = StellarRoboFactorType.Array };
                    tokens.SkipLogicalLineBreak();
                    while (true)
                    {
                        are.ElementNodes.Add(ParseExpression(tokens));
                        tokens.SkipLogicalLineBreak();
                        if (tokens.CheckSkipToken(StellarRoboTokenType.BracketEnd)) break;
                        if (!tokens.CheckSkipToken(StellarRoboTokenType.Comma)) throw new StellarRoboParseException(tokens.Peek().CreateErrorAt("配列が括弧で閉じていません。"));
                        tokens.SkipLogicalLineBreak();
                    }
                    return are;
                case StellarRoboTokenType.TrueKeyword:
                case StellarRoboTokenType.FalseKeyword:
                    return new StellarRoboFactorExpressionAstNode { FactorType = StellarRoboFactorType.BooleanValue, BooleanValue = Convert.ToBoolean(t.TokenString) };
                case StellarRoboTokenType.NilKeyword:
                    return new StellarRoboFactorExpressionAstNode { FactorType = StellarRoboFactorType.Nil };
                case StellarRoboTokenType.VargsKeyword:
                    return new StellarRoboFactorExpressionAstNode { FactorType = StellarRoboFactorType.VariableArguments };
                case StellarRoboTokenType.Identifer:
                    return new StellarRoboFactorExpressionAstNode { FactorType = StellarRoboFactorType.Identifer, StringValue = t.TokenString, Line = t.Position.Item2, StartPos = t.Position.Item1 - t.TokenString.Length, EndPos = t.Position.Item1 };
                case StellarRoboTokenType.StringLiteral:
                    return new StellarRoboFactorExpressionAstNode { FactorType = StellarRoboFactorType.StringValue, StringValue = t.TokenString };
                case StellarRoboTokenType.BinaryNumberLiteral:
                    lv = t.TokenString.Substring(2);
                    if (lv.Length > 64) lv = lv.Substring(lv.Length - 64);
                    return new StellarRoboFactorExpressionAstNode { FactorType = StellarRoboFactorType.IntegerValue, IntegerValue = unchecked(Convert.ToInt64(lv, 2)) };
                case StellarRoboTokenType.OctadecimalNumberLiteral:
                    lv = t.TokenString.Substring(2);
                    if (lv.Length > 64) lv = lv.Substring(lv.Length - 21);
                    return new StellarRoboFactorExpressionAstNode { FactorType = StellarRoboFactorType.IntegerValue, IntegerValue = unchecked(Convert.ToInt64(lv, 8)) };
                case StellarRoboTokenType.HexadecimalNumberLiteral:
                    lv = t.TokenString.Substring(2);
                    if (lv.Length > 64) lv = lv.Substring(lv.Length - 16);
                    return new StellarRoboFactorExpressionAstNode { FactorType = StellarRoboFactorType.IntegerValue, IntegerValue = unchecked(Convert.ToInt64(lv, 16)) };
                case StellarRoboTokenType.HexatridecimalNumberLiteral:
                    return new StellarRoboFactorExpressionAstNode { FactorType = StellarRoboFactorType.IntegerValue, IntegerValue = unchecked(Base36.Decode(t.TokenString.Substring(2))) };
                case StellarRoboTokenType.DecimalNumberLiteral:
                    if (t.TokenString.IndexOf('.') >= 0)
                    {

                        var v = 0.0;
                        var r = double.TryParse(t.TokenString, out v);
                        return new StellarRoboFactorExpressionAstNode { FactorType = StellarRoboFactorType.DoubleValue, DoubleValue = r ? v : double.MaxValue };
                    }
                    else
                    {
                        var v = 0L;
                        var r = long.TryParse(t.TokenString, out v);
                        return new StellarRoboFactorExpressionAstNode { FactorType = StellarRoboFactorType.IntegerValue, IntegerValue = r ? v : long.MaxValue };
                    }
                default:
                    throw new StellarRoboParseException(t.CreateErrorAt($"Factorが検出されるべきですが{t.Type}が検出されました。"));

            }
        }
    }

    /// <summary>
    /// StellarRoboのAST構築時の例外を定義します。
    /// </summary>
    [Serializable]
    internal class StellarRoboParseException : Exception
    {
        /// <summary>
        /// 発生時の<see cref="StellarRoboError"/>を取得します。
        /// </summary>
        public StellarRoboError Error { get; internal set; }

        /// <summary>
        /// 規定のコンストラクター
        /// </summary>
        public StellarRoboParseException()
        {
            Error = new StellarRoboError { Column = 0, Line = 0, Message = "" };
        }

        /// <summary>
        /// 指定した<see cref="StellarRoboError"/>を格納して初期化します。
        /// </summary>
        /// <param name="error">エラー情報</param>
        public StellarRoboParseException(StellarRoboError error) : base(error.Message)
        {
            Error = error;
        }

        /// <summary>
        /// 指定したメッセージ以下略
        /// </summary>
        /// <param name="message">メッセージ</param>
        public StellarRoboParseException(string message) : base(message)
        {
            Error = new StellarRoboError { Column = 0, Line = 0, Message = message };
        }

        /// <summary>
        /// 規定
        /// </summary>
        /// <param name="message">メッセージ</param>
        /// <param name="inner">内側</param>
        public StellarRoboParseException(string message, Exception inner) : base(message, inner)
        {
            Error = new StellarRoboError { Column = 0, Line = 0, Message = message };
        }
    }
}
