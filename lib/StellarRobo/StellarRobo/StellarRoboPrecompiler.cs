using System;
using System.Collections.Generic;
using System.Linq;
using StellarRobo.Analyze;
using StellarRobo.Type;
using System.IO;

namespace StellarRobo
{
    /// <summary>
    /// StellarRoboAstからStellarRoboILにプリコンパイルする機能を提供します。
    /// </summary>
    public sealed class StellarRoboPrecompiler
    {
        /// <summary>
        /// コンパイル時に式の定数畳み込みを行うがどうかを取得・設定します。
        /// </summary>
        public bool AllowConstantFolding { get; set; }

        /// <summary>
        /// 新しいインスタンスを初期化します。
        /// </summary>
        public StellarRoboPrecompiler()
        {

        }


        private StellarRoboSource current;
        /// <summary>
        /// 1つのソースコード全体からなる<see cref="StellarRoboAst"/>をプリコンパイルします。
        /// </summary>
        /// <param name="ast">対象の<see cref="StellarRoboAst"/></param>
        /// <returns>プリコンパイル結果</returns>
        public StellarRoboSource PrecompileAll(StellarRoboAst ast)
        {
            var result = new StellarRoboSource();
            current = result;
            foreach (var i in ast.RootNode.Children)
            {
                if (i is StellarRoboClassAstNode)
                {
                    result.classes.Add(PrecompileClass(i as StellarRoboClassAstNode));
                }
                else if (i is StellarRoboFunctionAstNode)
                {
                    result.methods.Add(PrecompileFunction(i as StellarRoboFunctionAstNode));
                }
                else if (i is StellarRoboUseAstNode)
                {
                    result.uses.Add((i as StellarRoboUseAstNode).Target);
                }
                else
                {
                    throw new InvalidOperationException("トップレベルにはクラスとメソッドとuse文以外おけません");
                }
            }
            current = null;
            return result;
        }

        private Stack<StellarRoboScriptClassInfo> cuc = new Stack<StellarRoboScriptClassInfo>();
        private StellarRoboScriptClassInfo PrecompileClass(StellarRoboClassAstNode ast)
        {
            //TODO: local初期値式対応
            var result = new StellarRoboScriptClassInfo(ast.Name);
            cuc.Push(result);
            foreach (var i in ast.Functions)
            {
                if (i.StaticMethod)
                {
                    result.AddClassMethod(PrecompileFunction(i));
                }
                else
                {
                    result.AddInstanceMethod(PrecompileFunction(i));
                }
            }
            foreach (var i in ast.Locals)
            {
                if (i.InitialExpression != null)
                {
                    var il = new StellarRoboIL();
                    il.PushCodes(PrecompileExpression(i.InitialExpression));
                    result.AddLocal(i.Name, il);
                }
                else
                {
                    result.AddLocal(i.Name, null);
                }
            }
            cuc.Pop();
            return result;
        }

        private StellarRoboScriptMethodInfo PrecompileFunction(StellarRoboFunctionAstNode ast)
        {
            var al = ast.Parameters;
            var result = new StellarRoboScriptMethodInfo(ast.Name, ast.Parameters.Count, ast.AllowsVariableArguments);
            result.Codes = new StellarRoboIL();
            var b = PrecompileBlock(ast.Children, "").ToList();
            foreach (var i in b.Where(p => p.Type == StellarRoboILCodeType.Jump || p.Type == StellarRoboILCodeType.FalseJump || p.Type == StellarRoboILCodeType.TrueJump))
            {
                i.IntegerValue = b.FindIndex(p => p.Type == StellarRoboILCodeType.Label && p.StringValue == i.StringValue);
            }
            foreach (var i in b.Where(p => p.Type == StellarRoboILCodeType.Label)) i.Type = StellarRoboILCodeType.Nop;
            if (b.Any(p => (p.Type == StellarRoboILCodeType.Jump || p.Type == StellarRoboILCodeType.FalseJump || p.Type == StellarRoboILCodeType.TrueJump) && p.IntegerValue == -1))
            {
                throw new InvalidOperationException("対応していないラベルがあります");
            }
            result.Codes.PushCodes(b);
            foreach (var i in result.Codes.Codes)
            {
                if (i.Type == StellarRoboILCodeType.LoadObject && al.Contains(i.StringValue))
                {
                    i.Type = StellarRoboILCodeType.PushArgument;
                    i.IntegerValue = al.IndexOf(i.StringValue);
                }
            }
            return result;
        }

        /// <summary>
        /// 式からなる<see cref="StellarRoboAst"/>をプリコンパイルします。
        /// </summary>
        /// <param name="ast">対象の<see cref="StellarRoboAst"/></param>
        /// <returns>プリコンパイル結果</returns>
        public StellarRoboIL PrecompileExpression(StellarRoboAst ast)
        {
            var result = new StellarRoboIL();
            result.PushCodes(PrecompileExpression(ast.RootNode));
            return result;
        }

        internal IReadOnlyList<StellarRoboILCode> PrecompileBlock(IReadOnlyList<StellarRoboAstNode> ast, string loopId)
        {
            var result = new StellarRoboIL();
            List<string> locals = new List<string>();
            foreach (var i in ast)
            {
                if (i is StellarRoboExpressionAstNode)
                {
                    if (i is StellarRoboFactorExpressionAstNode)
                    {
                        var exp = i as StellarRoboFactorExpressionAstNode;
                        if (exp.FactorType != StellarRoboFactorType.CoroutineResume) throw new InvalidOperationException("ステートメントにできる式はcoresume・代入・呼び出し・インクリメント・デクリメントのみです");
                    }
                    else if (i is StellarRoboArgumentCallExpressionAstNode)
                    {
                        var exp = i as StellarRoboArgumentCallExpressionAstNode;
                        if (exp.ExpressionType != StellarRoboOperatorType.FunctionCall) throw new InvalidOperationException("ステートメントにできる式はcoresume・代入・呼び出し・インクリメント・デクリメントのみです");
                        result.PushCodes(PrecompileFunctionCall(exp));
                        result.PushCode(StellarRoboILCodeType.Pop);
                    }
                    else if (i is StellarRoboBinaryExpressionAstNode)
                    {
                        var exp = i as StellarRoboBinaryExpressionAstNode;
                        switch (exp.ExpressionType)
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
                                result.PushCodes(PrecompileBinaryExpression(exp));
                                result.PushCode(StellarRoboILCodeType.Pop);
                                break;
                            default:
                                throw new InvalidOperationException("ステートメントにできる式はcoresume・代入・呼び出し・インクリメント・デクリメントのみです");
                        }

                    }
                    else if (i is StellarRoboPrimaryExpressionAstNode)
                    {
                        var exp = i as StellarRoboPrimaryExpressionAstNode;
                        switch (exp.ExpressionType)
                        {
                            case StellarRoboOperatorType.Increment:
                                result.PushCodes(PrecompileSuffixIncrement(exp));
                                result.PushCode(StellarRoboILCodeType.Pop);
                                break;
                            case StellarRoboOperatorType.Decrement:
                                result.PushCodes(PrecompileSuffixDecrement(exp));
                                result.PushCode(StellarRoboILCodeType.Pop);
                                break;
                            default:
                                throw new InvalidOperationException("ステートメントにできる式はcoresume・代入・呼び出し・インクリメント・デクリメントのみです");
                        }
                    }
                    else if (i is StellarRoboUnaryExpressionAstNode)
                    {
                        var exp = i as StellarRoboUnaryExpressionAstNode;
                        switch (exp.ExpressionType)
                        {
                            case StellarRoboOperatorType.Increment:
                                result.PushCodes(PrecompilePrefixIncrement(exp));
                                result.PushCode(StellarRoboILCodeType.Pop);
                                break;
                            case StellarRoboOperatorType.Decrement:
                                result.PushCodes(PrecompilePrefixDecrement(exp));
                                result.PushCode(StellarRoboILCodeType.Pop);
                                break;
                            default:
                                throw new InvalidOperationException("ステートメントにできる式はcoresume・代入・呼び出し・インクリメント・デクリメントのみです");
                        }
                    }
                    else
                    {
                        throw new InvalidOperationException("ステートメントにできる式はcoresume・代入・呼び出し・インクリメント・デクリメントのみです");
                    }
                }
                else if (i is StellarRoboLocalAstNode)
                {
                    var lc = i as StellarRoboLocalAstNode;
                    locals.Add(lc.Name);
                    if (lc.InitialExpression != null)
                    {
                        result.PushCode(StellarRoboILCodeType.LoadObject, lc.Name);
                        result.PushCodes(PrecompileExpression(lc.InitialExpression));
                        result.PushCode(StellarRoboILCodeType.Assign);
                    }
                }
                else if (i is StellarRoboReturnAstNode)
                {
                    var rc = i as StellarRoboReturnAstNode;
                    if (rc.Value != null)
                    {
                        result.PushCodes(PrecompileExpression(rc.Value));
                    }
                    else
                    {
                        result.PushCode(StellarRoboILCodeType.PushNil);
                    }
                    if(rc.Type == StellarRoboAstNodeType.ReturnStatement)
                    {
                        result.PushCode(StellarRoboILCodeType.Return, rc.Line, rc.StartPos, rc.EndPos);
                    }
                    else
                    {
                        result.PushCode(StellarRoboILCodeType.Yield);
                    }
                    //result.PushCode(rc.Type == StellarRoboAstNodeType.ReturnStatement ? StellarRoboILCodeType.Return : StellarRoboILCodeType.Yield);
                }
                else if (i is StellarRoboCoroutineDeclareAstNode)
                {
                    var cd = i as StellarRoboCoroutineDeclareAstNode;
                    result.PushCodes(PrecompileExpression(cd.InitialExpression));
                    foreach (var pe in cd.ParameterExpressions) result.PushCodes(PrecompileExpression(pe));
                    result.PushCode(new StellarRoboILCode { Type = StellarRoboILCodeType.StartCoroutine, StringValue = cd.Name, IntegerValue = cd.ParameterExpressions.Count });
                }
                else if (i is StellarRoboContinueAstNode)
                {
                    var ca = i as StellarRoboContinueAstNode;
                    var ln = ca.Label != "" ? ca.Label : loopId;
                    result.PushCode(StellarRoboILCodeType.Jump, $"{ln}-" + (i.Type == StellarRoboAstNodeType.ContinueStatement ? "Continue" : "End"));
                }
                else if (i is StellarRoboIfAstNode)
                {
                    result.PushCodes(PrecompileIf(i as StellarRoboIfAstNode, loopId));
                }
                else if (i is StellarRoboForAstNode)
                {
                    result.PushCodes(PrecompileFor(i as StellarRoboForAstNode));
                }
                else if (i is StellarRoboForeachAstNode)
                {
                    result.PushCodes(PrecompileForeach(i as StellarRoboForeachAstNode));
                }
                else if (i is StellarRoboLoopAstNode)
                {
                    result.PushCodes(PrecompileWhile(i as StellarRoboLoopAstNode));
                }
            }
            return result.Codes;
        }

        private IList<StellarRoboILCode> PrecompileIf(StellarRoboIfAstNode ifn, string loopId)
        {
            //まあまずかぶらないでしょう
            var id = Guid.NewGuid().ToString().Substring(0, 8);
            var result = new List<StellarRoboILCode>();
            result.AddRange(PrecompileExpression(ifn.IfBlock.Condition));
            result.Add(new StellarRoboILCode { Type = StellarRoboILCodeType.FalseJump, StringValue = $"{id}-IfEnd" });
            result.AddRange(PrecompileBlock(ifn.IfBlock.Children, loopId));
            result.Add(new StellarRoboILCode { Type = StellarRoboILCodeType.Jump, StringValue = $"{id}-End" });
            result.Add(new StellarRoboILCode { Type = StellarRoboILCodeType.Label, StringValue = $"{id}-IfEnd" });
            var c = 0;
            foreach (var i in ifn.ElifBlocks)
            {
                result.AddRange(PrecompileExpression(i.Condition));
                result.Add(new StellarRoboILCode { Type = StellarRoboILCodeType.FalseJump, StringValue = $"{id}-Elif{c}End" });
                result.AddRange(PrecompileBlock(i.Children, loopId));
                result.Add(new StellarRoboILCode { Type = StellarRoboILCodeType.Jump, StringValue = $"{id}-End" });
                result.Add(new StellarRoboILCode { Type = StellarRoboILCodeType.Label, StringValue = $"{id}-Elif{c}End" });
                c++;
            }
            if (ifn.ElseBlock != null)
            {
                result.AddRange(PrecompileBlock(ifn.ElseBlock.Children, loopId));
            }
            result.Add(new StellarRoboILCode { Type = StellarRoboILCodeType.Label, StringValue = $"{id}-End" });
            return result;
        }

        private IList<StellarRoboILCode> PrecompileFor(StellarRoboForAstNode fn)
        {
            var id = fn.Name;
            var result = new List<StellarRoboILCode>();
            foreach (var i in fn.InitializeExpressions)
            {
                result.AddRange(PrecompileExpression(i));
                result.Add(new StellarRoboILCode { Type = StellarRoboILCodeType.Pop });
            }
            result.Add(new StellarRoboILCode { Type = StellarRoboILCodeType.Label, StringValue = $"{id}-Next" });
            result.AddRange(PrecompileExpression(fn.Condition));
            result.Add(new StellarRoboILCode { Type = StellarRoboILCodeType.FalseJump, StringValue = $"{id}-End" });
            result.AddRange(PrecompileBlock(fn.Children, id));

            result.Add(new StellarRoboILCode { Type = StellarRoboILCodeType.Label, StringValue = $"{id}-Continue" });
            foreach (var i in fn.CounterExpressions)
            {
                result.AddRange(PrecompileExpression(i));
                result.Add(new StellarRoboILCode { Type = StellarRoboILCodeType.Pop });
            }
            result.Add(new StellarRoboILCode { Type = StellarRoboILCodeType.Jump, StringValue = $"{id}-Next" });
            result.Add(new StellarRoboILCode { Type = StellarRoboILCodeType.Label, StringValue = $"{id}-End" });
            return result;
        }

        private IList<StellarRoboILCode> PrecompileWhile(StellarRoboLoopAstNode fn)
        {
            var id = Guid.NewGuid().ToString().Substring(0, 8);
            var result = new List<StellarRoboILCode>();
            result.Add(new StellarRoboILCode { Type = StellarRoboILCodeType.Label, StringValue = $"{id}" });
            result.AddRange(PrecompileExpression(fn.Condition));
            result.Add(new StellarRoboILCode { Type = StellarRoboILCodeType.FalseJump, StringValue = $"{id}-End" });
            result.AddRange(PrecompileBlock(fn.Children, id));
            result.Add(new StellarRoboILCode { Type = StellarRoboILCodeType.Jump, StringValue = $"{id}" });
            result.Add(new StellarRoboILCode { Type = StellarRoboILCodeType.Label, StringValue = $"{id}-End" });
            return result;
        }

        private IList<StellarRoboILCode> PrecompileForeach(StellarRoboForeachAstNode fn)
        {
            if (fn.IsCoroutineSource)
            {
                return PrecompileCoroutineForeach(fn);
            }
            else
            {
                return PrecompileNormalForeach(fn);
            }
        }

        private IList<StellarRoboILCode> PrecompileNormalForeach(StellarRoboForeachAstNode fn)
        {
            var id = fn.Name;
            var result = new List<StellarRoboILCode>();
            var cntn = $"{id}-Counter";
            result.Add(new StellarRoboILCode { Type = StellarRoboILCodeType.LoadObject, StringValue = cntn });
            result.Add(new StellarRoboILCode { Type = StellarRoboILCodeType.PushInteger, IntegerValue = 0 });
            result.Add(new StellarRoboILCode { Type = StellarRoboILCodeType.Assign });
            result.Add(new StellarRoboILCode { Type = StellarRoboILCodeType.Pop });

            result.Add(new StellarRoboILCode { Type = StellarRoboILCodeType.Label, StringValue = $"{id}-Next" });
            result.Add(new StellarRoboILCode { Type = StellarRoboILCodeType.LoadObject, StringValue = cntn });
            result.AddRange(PrecompileExpression(fn.Source));
            result.Add(new StellarRoboILCode { Type = StellarRoboILCodeType.LoadMember, StringValue = "length" });
            result.Add(new StellarRoboILCode { Type = StellarRoboILCodeType.Lesser });
            result.Add(new StellarRoboILCode { Type = StellarRoboILCodeType.FalseJump, StringValue = $"{id}-End" });
            result.Add(new StellarRoboILCode { Type = StellarRoboILCodeType.LoadObject, StringValue = fn.ElementVariableName });
            result.AddRange(PrecompileExpression(fn.Source));
            result.Add(new StellarRoboILCode { Type = StellarRoboILCodeType.LoadObject, StringValue = cntn });
            result.Add(new StellarRoboILCode { Type = StellarRoboILCodeType.IndexerCall, IntegerValue = 1 });
            result.Add(new StellarRoboILCode { Type = StellarRoboILCodeType.Assign });
            result.Add(new StellarRoboILCode { Type = StellarRoboILCodeType.Pop });
            result.AddRange(PrecompileBlock(fn.Children, id));
            result.Add(new StellarRoboILCode { Type = StellarRoboILCodeType.Label, StringValue = $"{id}-Continue" });
            result.Add(new StellarRoboILCode { Type = StellarRoboILCodeType.LoadObject, StringValue = cntn });
            result.Add(new StellarRoboILCode { Type = StellarRoboILCodeType.PushInteger, IntegerValue = 1 });
            result.Add(new StellarRoboILCode { Type = StellarRoboILCodeType.PlusAssign });
            result.Add(new StellarRoboILCode { Type = StellarRoboILCodeType.Pop });
            result.Add(new StellarRoboILCode { Type = StellarRoboILCodeType.Jump, StringValue = $"{id}-Next" });
            result.Add(new StellarRoboILCode { Type = StellarRoboILCodeType.Label, StringValue = $"{id}-End" });
            return result;
        }

        private IList<StellarRoboILCode> PrecompileCoroutineForeach(StellarRoboForeachAstNode fn)
        {
            var id = fn.Name;
            var result = new List<StellarRoboILCode>();
            result.AddRange(PrecompileExpression(fn.Source));
            foreach (var pe in fn.CoroutineArguments) result.AddRange(PrecompileExpression(pe));
            result.Add(new StellarRoboILCode { Type = StellarRoboILCodeType.StartCoroutine, StringValue = $"{id}-Coroutine", IntegerValue = fn.CoroutineArguments.Count });
            result.Add(new StellarRoboILCode { Type = StellarRoboILCodeType.Label, StringValue = $"{id}" });
            result.Add(new StellarRoboILCode { Type = StellarRoboILCodeType.LoadObject, StringValue = fn.ElementVariableName });
            result.Add(new StellarRoboILCode { Type = StellarRoboILCodeType.ResumeCoroutine, StringValue = $"{id}-Coroutine", BooleanValue = true });
            result.Add(new StellarRoboILCode { Type = StellarRoboILCodeType.FalseJump, StringValue = $"{id}-End" });
            result.AddRange(PrecompileBlock(fn.Children, id));
            result.Add(new StellarRoboILCode { Type = StellarRoboILCodeType.Jump, StringValue = $"{id}" });
            result.Add(new StellarRoboILCode { Type = StellarRoboILCodeType.Label, StringValue = $"{id}-End" });
            return result;
        }

        private IList<StellarRoboILCode> PrecompileExpression(StellarRoboAstNode node)
        {
            var result = new List<StellarRoboILCode>();
            if (node.Type != StellarRoboAstNodeType.Expression) throw new ArgumentException("ASTが式でない件について");
            var en = node as StellarRoboExpressionAstNode;
            if (en is StellarRoboBinaryExpressionAstNode)
            {
                var exp = en as StellarRoboBinaryExpressionAstNode;
                result.AddRange(PrecompileBinaryExpression(exp));
            }
            else if (en is StellarRoboFactorExpressionAstNode)
            {
                var exp = en as StellarRoboFactorExpressionAstNode;
                switch (exp.FactorType)
                {
                    case StellarRoboFactorType.IntegerValue:
                        result.Add(new StellarRoboILCode { Type = StellarRoboILCodeType.PushInteger, IntegerValue = exp.IntegerValue });
                        break;
                    case StellarRoboFactorType.SingleValue:
                        result.Add(new StellarRoboILCode { Type = StellarRoboILCodeType.PushDouble, FloatValue = exp.SingleValue });
                        break;
                    case StellarRoboFactorType.DoubleValue:
                        result.Add(new StellarRoboILCode { Type = StellarRoboILCodeType.PushDouble, FloatValue = exp.DoubleValue });
                        break;
                    case StellarRoboFactorType.StringValue:
                        result.Add(new StellarRoboILCode { Type = StellarRoboILCodeType.PushString, StringValue = exp.StringValue });
                        break;
                    case StellarRoboFactorType.BooleanValue:
                        result.Add(new StellarRoboILCode { Type = StellarRoboILCodeType.PushBoolean, BooleanValue = exp.BooleanValue });
                        break;
                    case StellarRoboFactorType.Nil:
                        result.Add(new StellarRoboILCode { Type = StellarRoboILCodeType.PushNil });
                        break;
                    case StellarRoboFactorType.Identifer:
                        result.Add(new StellarRoboILCode { Type = StellarRoboILCodeType.LoadObject, StringValue = exp.StringValue, Line = exp.Line, StartPos = exp.StartPos, EndPos = exp.EndPos });
                        break;
                    case StellarRoboFactorType.ParenExpression:
                        result.AddRange(PrecompileExpression(exp.ExpressionNode));
                        break;
                    case StellarRoboFactorType.CoroutineResume:
                        if (exp.BooleanValue)
                        {
                            // state = coresume(cor, val)
                            result.AddRange(PrecompileExpression(exp.ExpressionNode));
                        }
                        result.Add(new StellarRoboILCode { Type = StellarRoboILCodeType.ResumeCoroutine, StringValue = exp.StringValue, BooleanValue = exp.BooleanValue });
                        break;
                    case StellarRoboFactorType.Array:
                        foreach (var i in exp.ElementNodes) result.AddRange(PrecompileExpression(i));
                        result.Add(new StellarRoboILCode { Type = StellarRoboILCodeType.MakeArray, IntegerValue = exp.ElementNodes.Count });
                        break;
                    case StellarRoboFactorType.Lambda:
                        result.AddRange(PrecompileLambda(exp));
                        break;
                }
            }
            else if (en is StellarRoboArgumentCallExpressionAstNode)
            {
                var exp = en as StellarRoboArgumentCallExpressionAstNode;
                if (exp.Target is StellarRoboFactorExpressionAstNode && (exp.Target as StellarRoboFactorExpressionAstNode).FactorType == StellarRoboFactorType.VariableArguments)
                {
                    //vargs
                    foreach (var arg in exp.Arguments) result.AddRange(PrecompileExpression(arg));
                    result.Add(new StellarRoboILCode { Type = StellarRoboILCodeType.LoadVarg, IntegerValue = exp.Arguments.Count });
                }
                else
                {
                    if (exp.ExpressionType == StellarRoboOperatorType.IndexerAccess)
                    {
                        result.AddRange(PrecompileIndexerCall(exp));
                    }
                    else
                    {
                        result.AddRange(PrecompileFunctionCall(exp));
                    }
                }
            }
            else if (en is StellarRoboMemberAccessExpressionAstNode)
            {
                var exp = en as StellarRoboMemberAccessExpressionAstNode;
                result.AddRange(PrecompileExpression(exp.Target));
                result.Add(new StellarRoboILCode { Type = StellarRoboILCodeType.LoadMember, StringValue = exp.MemberName });
            }
            else if (en is StellarRoboPrimaryExpressionAstNode)
            {
                var exp = en as StellarRoboPrimaryExpressionAstNode;
                //後置

                switch (exp.ExpressionType)
                {
                    case StellarRoboOperatorType.Increment:
                        result.AddRange(PrecompileSuffixIncrement(exp));
                        break;
                    case StellarRoboOperatorType.Decrement:
                        result.AddRange(PrecompileSuffixDecrement(exp));
                        break;
                    default:
                        throw new NotImplementedException("多分実装してない1次式なんだと思う");
                }
                result.Add(new StellarRoboILCode { Type = StellarRoboILCodeType.Pop });

            }
            else if (en is StellarRoboUnaryExpressionAstNode)
            {
                var exp = en as StellarRoboUnaryExpressionAstNode;
                switch (exp.ExpressionType)
                {
                    case StellarRoboOperatorType.Minus:
                        result.AddRange(PrecompileExpression(exp.Target));
                        result.Add(new StellarRoboILCode { Type = StellarRoboILCodeType.Negative });
                        break;
                    case StellarRoboOperatorType.Not:
                        result.AddRange(PrecompileExpression(exp.Target));
                        result.Add(new StellarRoboILCode { Type = StellarRoboILCodeType.Not });
                        break;
                    case StellarRoboOperatorType.Increment:
                        result.AddRange(PrecompilePrefixIncrement(exp));
                        break;
                    case StellarRoboOperatorType.Decrement:
                        result.AddRange(PrecompilePrefixDecrement(exp));

                        break;
                }
            }
            else
            {
                throw new InvalidOperationException("ごめん何言ってるかさっぱりわかんない");
            }
            return result;
        }

        private IList<StellarRoboILCode> PrecompileLambda(StellarRoboFactorExpressionAstNode exp)
        {
            var il = PrecompileExpression(exp.ExpressionNode);
            il.Add(new StellarRoboILCode { Type = StellarRoboILCodeType.Return });
            var lma = exp.ElementNodes.Select(p => ((StellarRoboFactorExpressionAstNode)p).StringValue).ToList();
            switch (CheckLocalReference(exp, lma))
            {
                case 0:
                    return PrecompileClassLambda(il, lma);
                case 1:
                case 2:
                    return PrecompileLexicalLambda(il, lma);
                default:
                    throw new ArgumentException("ラムダ式の形式が不正です。");
            }
        }

        private IList<StellarRoboILCode> PrecompileClassLambda(IList<StellarRoboILCode> il, List<string> lma)
        {
            var caps = new List<string>();
            for (int i = 0; i < il.Count; i++)
            {
                var c = il[i];
                if (c.Type == StellarRoboILCodeType.LoadObject)
                {
                    var name = c.StringValue;
                    if (lma.Contains(name))
                    {
                        c.Type = StellarRoboILCodeType.PushArgument;
                        c.IntegerValue = lma.IndexOf(name);
                    }
                }
            }
            var ln = $"Lambda-{Guid.NewGuid().ToString().Substring(0, 17)}"; ;
            var mi = new StellarRoboScriptMethodInfo(ln, lma.Count, false);
            var lc = new StellarRoboIL();
            lc.PushCodes(il);
            mi.Codes = lc;
            var result = new List<StellarRoboILCode>();
            current.methods.Add(mi);
            result.Add(new StellarRoboILCode { Type = StellarRoboILCodeType.LoadObject, StringValue = ln });
            return result;
        }

        private IList<StellarRoboILCode> PrecompileLexicalLambda(IList<StellarRoboILCode> il, List<string> lma)
        {
            var caps = new List<string>();
            for (int i = 0; i < il.Count; i++)
            {
                var c = il[i];
                if (c.Type == StellarRoboILCodeType.LoadObject)
                {
                    var name = c.StringValue;
                    if (lma.Contains(name))
                    {
                        c.Type = StellarRoboILCodeType.PushArgument;
                        c.IntegerValue = lma.IndexOf(name);
                    }
                    else
                    {
                        //キャプチャ対象
                        c.Type = StellarRoboILCodeType.LoadMember;
                        if (caps.Contains(name))
                        {
                            c.StringValue = $"cap_{caps.IndexOf(name)}";
                        }
                        else
                        {
                            c.StringValue = $"cap_{caps.Count}";
                            caps.Add(name);
                        }
                        il.Insert(i, new StellarRoboILCode { Type = StellarRoboILCodeType.LoadObject, StringValue = "self" });
                    }
                }
            }
            var ln = $"Lambda-{Guid.NewGuid().ToString().Substring(0, 17)}";
            var cl = new StellarRoboScriptClassInfo(ln);
            var ctor = new StellarRoboIL();
            for (int i = 0; i < caps.Count; i++)
            {
                cl.AddLocal($"cap_{i}", null);
                ctor.PushCode(StellarRoboILCodeType.LoadObject, "self");
                ctor.PushCode(StellarRoboILCodeType.LoadMember, $"cap_{i}");
                ctor.PushCode(StellarRoboILCodeType.PushArgument, i);
                ctor.PushCode(StellarRoboILCodeType.Assign);
            }
            var ci = new StellarRoboScriptMethodInfo("new", caps.Count, false);
            ci.Codes = ctor;
            cl.AddClassMethod(ci);
            var fi = new StellarRoboScriptMethodInfo("body", lma.Count, false);
            fi.Codes = new StellarRoboIL();
            fi.Codes.PushCodes(il);
            cl.AddInstanceMethod(fi);
            current.classes.Add(cl);
            var result = new List<StellarRoboILCode>();
            result.Add(new StellarRoboILCode { Type = StellarRoboILCodeType.LoadObject, StringValue = ln });
            result.Add(new StellarRoboILCode { Type = StellarRoboILCodeType.LoadMember, StringValue = "new" });
            foreach (var i in caps) result.Add(new StellarRoboILCode { Type = StellarRoboILCodeType.LoadObject, StringValue = i });
            result.Add(new StellarRoboILCode { Type = StellarRoboILCodeType.Call, IntegerValue = caps.Count });
            result.Add(new StellarRoboILCode { Type = StellarRoboILCodeType.LoadMember, StringValue = "body" });
            return result;
        }

        private int CheckLocalReference(StellarRoboExpressionAstNode exp, IList<string> args)
        {
            if (exp is StellarRoboFactorExpressionAstNode)
            {
                var fc = (StellarRoboFactorExpressionAstNode)exp;
                switch (fc.FactorType)
                {
                    case StellarRoboFactorType.BooleanValue:
                    case StellarRoboFactorType.DoubleValue:
                    case StellarRoboFactorType.IntegerValue:
                    case StellarRoboFactorType.Nil:
                    case StellarRoboFactorType.StringValue:
                    case StellarRoboFactorType.SingleValue:
                        return 0;
                    case StellarRoboFactorType.CoroutineResume:
                    case StellarRoboFactorType.VariableArguments:
                        throw new ArgumentException("ラムダ式はcoresume・VARGSを内包できません。");
                    case StellarRoboFactorType.Array:
                        return fc.ElementNodes.Max(p => CheckLocalReference(p, args));
                    case StellarRoboFactorType.Identifer:
                        if (args.Contains(fc.StringValue)) return 0;
                        if (fc.StringValue == "self") return 1;
                        return 2;
                    case StellarRoboFactorType.Lambda:
                    case StellarRoboFactorType.ParenExpression:
                        return CheckLocalReference(fc.ExpressionNode, args);
                    default:
                        return 0;
                }
            }
            else if (exp is StellarRoboPrimaryExpressionAstNode)
            {
                return CheckLocalReference((exp as StellarRoboPrimaryExpressionAstNode).Target, args);
            }
            else if (exp is StellarRoboUnaryExpressionAstNode)
            {
                return CheckLocalReference((exp as StellarRoboUnaryExpressionAstNode).Target, args);
            }
            else if (exp is StellarRoboBinaryExpressionAstNode)
            {
                var be = (StellarRoboBinaryExpressionAstNode)exp;
                return Math.Max(CheckLocalReference(be.FirstNode, args), CheckLocalReference(be.SecondNode, args));
            }
            else
            {
                return 0;
            }
        }

        internal IList<StellarRoboILCode> PrecompileBinaryExpression(StellarRoboBinaryExpressionAstNode node)
        {
            var result = new List<StellarRoboILCode>();
            //ショートサーキット
            switch (node.ExpressionType)
            {
                /*
                case StellarRoboOperatorType.AndAlso:
                    var aid = Guid.NewGuid().ToString().Substring(0, 8);
                    result.AddRange(PrecompileExpression(node.FirstNode));
                    result.Add(new StellarRoboILCode { Type = StellarRoboILCodeType.AsValue });
                    result.Add(new StellarRoboILCode { Type = StellarRoboILCodeType.FalseJump, StringValue = $"AndAlso-{aid}" });
                    result.AddRange(PrecompileExpression(node.SecondNode));
                    result.Add(new StellarRoboILCode { Type = StellarRoboILCodeType.Label, StringValue = $"AndAlso-{aid}" });
                    break;
                case StellarRoboOperatorType.OrElse:
                    var eid = Guid.NewGuid().ToString().Substring(0, 8);
                    result.AddRange(PrecompileExpression(node.FirstNode));
                    result.Add(new StellarRoboILCode { Type = StellarRoboILCodeType.AsValue });
                    result.Add(new StellarRoboILCode { Type = StellarRoboILCodeType.TrueJump, StringValue = $"OrElse-{eid}" });
                    result.AddRange(PrecompileExpression(node.SecondNode));
                    result.Add(new StellarRoboILCode { Type = StellarRoboILCodeType.Label, StringValue = $"OrElse-{eid}" });
                    break;
                */
                default:
                    result.AddRange(PrecompileExpression(node.FirstNode));
                    result.AddRange(PrecompileExpression(node.SecondNode));
                    result.Add(new StellarRoboILCode { Type = (StellarRoboILCodeType)Enum.Parse(typeof(StellarRoboILCodeType), node.ExpressionType.ToString(), true) });
                    break;
            }
            return result;
        }

        internal IList<StellarRoboILCode> PrecompileFunctionCall(StellarRoboArgumentCallExpressionAstNode node)
        {
            var result = new List<StellarRoboILCode>();
            result.AddRange(PrecompileExpression(node.Target));
            foreach (var arg in node.Arguments) result.AddRange(PrecompileExpression(arg));
            result.Add(new StellarRoboILCode { Type = StellarRoboILCodeType.Call, IntegerValue = node.Arguments.Count });
            return result;
        }

        internal IList<StellarRoboILCode> PrecompileIndexerCall(StellarRoboArgumentCallExpressionAstNode node)
        {
            var result = new List<StellarRoboILCode>();
            result.AddRange(PrecompileExpression(node.Target));
            foreach (var arg in node.Arguments) result.AddRange(PrecompileExpression(arg));
            result.Add(new StellarRoboILCode { Type = StellarRoboILCodeType.IndexerCall, IntegerValue = node.Arguments.Count });
            return result;
        }

        internal IList<StellarRoboILCode> PrecompileSuffixIncrement(StellarRoboPrimaryExpressionAstNode node)
        {
            var result = new List<StellarRoboILCode>();
            result.AddRange(PrecompileExpression(node.Target));
            result.Add(new StellarRoboILCode { Type = StellarRoboILCodeType.AsValue });
            result.AddRange(PrecompileExpression(node.Target));
            result.Add(new StellarRoboILCode { Type = StellarRoboILCodeType.PushInteger, IntegerValue = 1 });
            result.Add(new StellarRoboILCode { Type = StellarRoboILCodeType.PlusAssign });
            return result;
        }

        internal IList<StellarRoboILCode> PrecompileSuffixDecrement(StellarRoboPrimaryExpressionAstNode node)
        {
            var result = new List<StellarRoboILCode>();
            result.AddRange(PrecompileExpression(node.Target));
            result.Add(new StellarRoboILCode { Type = StellarRoboILCodeType.AsValue });
            result.AddRange(PrecompileExpression(node.Target));
            result.Add(new StellarRoboILCode { Type = StellarRoboILCodeType.PushInteger, IntegerValue = 1 });
            result.Add(new StellarRoboILCode { Type = StellarRoboILCodeType.MinusAssign });
            return result;
        }

        internal IList<StellarRoboILCode> PrecompilePrefixIncrement(StellarRoboUnaryExpressionAstNode node)
        {
            var result = new List<StellarRoboILCode>();
            result.AddRange(PrecompileExpression(node.Target));
            result.Add(new StellarRoboILCode { Type = StellarRoboILCodeType.PushInteger, IntegerValue = 1 });
            result.Add(new StellarRoboILCode { Type = StellarRoboILCodeType.PlusAssign });
            return result;
        }

        internal IList<StellarRoboILCode> PrecompilePrefixDecrement(StellarRoboUnaryExpressionAstNode node)
        {
            var result = new List<StellarRoboILCode>();
            result.AddRange(PrecompileExpression(node.Target));
            result.Add(new StellarRoboILCode { Type = StellarRoboILCodeType.PushInteger, IntegerValue = 1 });
            result.Add(new StellarRoboILCode { Type = StellarRoboILCodeType.MinusAssign });
            return result;
        }
    }
}
