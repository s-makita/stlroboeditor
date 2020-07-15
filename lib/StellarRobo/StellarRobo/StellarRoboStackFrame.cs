using StellarRobo.Type;
using System;
using System.Text;
using System.Collections.Generic;
using System.Windows.Forms;
using UserException;

namespace StellarRobo
{
    /// <summary>
    /// ローカル変数を含むスタックフレームを提供します。
    /// </summary>
    public sealed class StellarRoboStackFrame
    {
        private Dictionary<string, StellarRoboReference> locals = new Dictionary<string, StellarRoboReference>();
        /// <summary>
        /// ローカル変数の参照を取得します。
        /// </summary>
        /// <remarks>再開可能な状態で操作すると不具合が発生する可能性があります。</remarks>
        public IDictionary<string, StellarRoboReference> Locals => locals;

        private Dictionary<string, StellarRoboCoroutineFrame> cors = new Dictionary<string, StellarRoboCoroutineFrame>();
        /// <summary>
        /// 起動中のコルーチンの参照を取得します。
        /// </summary>
        /// <remarks>再開可能な状態で操作すると不具合が発生する可能性があります。</remarks>
        public IDictionary<string, StellarRoboCoroutineFrame> Coroutines => cors;

        /// <summary>
        /// 実行中の<see cref="StellarRoboILCode"/>のリストを取得します。
        /// </summary>
        public IReadOnlyList<StellarRoboILCode> Codes { get; internal set; }

        /// <summary>
        /// コードにおける参照のスタックを取得します。
        /// </summary>
        /// <remarks>再開可能な状態で操作すると不具合が発生する可能性があります。</remarks>
        public Stack<StellarRoboReference> ReferenceStack { get; internal set; } = new Stack<StellarRoboReference>();

        /// <summary>
        /// 引数を取得します。
        /// </summary>
        public IList<StellarRoboObject> Arguments { get; internal set; } = new List<StellarRoboObject>();

        /// <summary>
        /// 可変長引数を取得します。
        /// </summary>
        public IList<StellarRoboObject> VariableArguments { get; internal set; } = new List<StellarRoboObject>();

        /// <summary>
        /// 現在このスタックフレームが属している<see cref="StellarRoboContext"/>を取得します。
        /// </summary>
        public StellarRoboContext RunningContext { get; internal set; }

        /// <summary>
        /// 現在の<see cref="StellarRoboIL"/>の位置を取得します。
        /// </summary>
        public int ProgramCounter { get; internal set; }

        /// <summary>
        /// 現在の状態でreturn/yieldされた<see cref="StellarRoboObject"/>を取得します。
        /// </summary>
        public StellarRoboObject ReturningObject { get; internal set; } = StellarRoboNil.Instance;

        /// <summary>
        /// 新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="ctx">実行している<see cref="StellarRoboContext"/></param>
        /// <param name="il">実行する<see cref="StellarRoboIL"/></param>
        public StellarRoboStackFrame(StellarRoboContext ctx, StellarRoboIL il)
        {
            Codes = il.Codes;
            ProgramCounter = 0;
            RunningContext = ctx;
        }
#if EDITOR
        private void SetDebugLineColor(int line)
        {
            //デバック中か？若しくはBreakPointか？
            if (RunningContext.IsDebug || (RunningContext.BreakPoint.FindIndex(x => x == line) > -1))
            {
                //フラグ更新
                RunningContext.IsDebug = true;

                //現在の行背景色変更
                RunningContext.SetDebugLine(line);

                //処理を中断
                RunningContext.countdown.Wait();
            }
        }
#endif
        private StellarRoboReference GetReference(string name)
        {
            StellarRoboReference result;
            if (Locals.ContainsKey(name)) return Locals[name];
            if ((result = RunningContext.Module.GetReference(name)) != StellarRoboNil.Reference) return result;
            result = new StellarRoboReference { IsLeftValue = true };
            locals[name] = result;
            return result;
        }

        /// <summary>
        /// 現在のコードを最初から実行します。
        /// </summary>
        /// <returns>継続可能な場合はtrueが、それ以外の場合はfalseが帰ります。</returns>
        public bool Execute()
        {
            Reset();
            return Resume();
        }

        /// <summary>
        /// 現在の状態で、現在のコードの実行を再開します。
        /// </summary>
        /// <returns></returns>
        public bool Resume()
        {
            StellarRoboObject v1, v2;
            StellarRoboReference rfr;
            Stack<StellarRoboObject> args;
            while (ProgramCounter < Codes.Count)
            {
                //強制停止するか？
                if(RunningContext.ForcedStop)
                {
                    //フラグを戻す
                    RunningContext.ForcedStop = false;

                    //例外を発生させ一気に抜ける
                    throw new ManualStopException();
                }

                var c = Codes[ProgramCounter];
                switch (c.Type)
                {
                    //基本--------------------------------------------------------------------
                    case StellarRoboILCodeType.Nop:
                        break;
                    case StellarRoboILCodeType.Label:
                        break;
                    case StellarRoboILCodeType.PushInteger:
                        ReferenceStack.Push(StellarRoboReference.Right(c.IntegerValue));
                        break;
                    case StellarRoboILCodeType.PushString:
                        ReferenceStack.Push(StellarRoboReference.Right(c.StringValue));
                        break;
                    case StellarRoboILCodeType.PushDouble:
                        ReferenceStack.Push(StellarRoboReference.Right(c.FloatValue));
                        break;
                    case StellarRoboILCodeType.PushBoolean:
                        ReferenceStack.Push(StellarRoboReference.Right(c.BooleanValue));
                        break;
                    case StellarRoboILCodeType.PushNil:
                        ReferenceStack.Push(StellarRoboNil.Reference);
                        break;
                    case StellarRoboILCodeType.Pop:
                        ReferenceStack.Pop();
                        break;

                    //二項演算子--------------------------------------------------------------
                    case StellarRoboILCodeType.Plus:
                    case StellarRoboILCodeType.Minus:
                    case StellarRoboILCodeType.Multiply:
                    case StellarRoboILCodeType.Divide:
                    case StellarRoboILCodeType.Modular:
                    case StellarRoboILCodeType.And:
                    case StellarRoboILCodeType.Or:
                    case StellarRoboILCodeType.Xor:
                    case StellarRoboILCodeType.AndAlso:
                    case StellarRoboILCodeType.OrElse:
                    case StellarRoboILCodeType.LeftBitShift:
                    case StellarRoboILCodeType.RightBitShift:
                    case StellarRoboILCodeType.Equal:
                    case StellarRoboILCodeType.NotEqual:
                    case StellarRoboILCodeType.Greater:
                    case StellarRoboILCodeType.Lesser:
                    case StellarRoboILCodeType.GreaterEqual:
                    case StellarRoboILCodeType.LesserEqual:
                        v2 = ReferenceStack.Pop().RawObject;
                        v1 = ReferenceStack.Pop().RawObject;
                        ReferenceStack.Push(StellarRoboReference.Right(v1.ExpressionOperation(c.Type, v2)));
                        break;
                    case StellarRoboILCodeType.Not:
                    case StellarRoboILCodeType.Negative:
                        v1 = ReferenceStack.Pop().RawObject;
                        ReferenceStack.Push(StellarRoboReference.Right(v1.ExpressionOperation(c.Type, null)));
                        break;

                    //代入など--------------------------------------------------------------
                    case StellarRoboILCodeType.Assign:
                        v1 = ReferenceStack.Pop().RawObject;
                        rfr = ReferenceStack.Pop();
                        rfr.RawObject = v1.AsByValValue();
                        ReferenceStack.Push(StellarRoboReference.Right(v1));
                        break;
                    case StellarRoboILCodeType.PlusAssign:
                        v1 = ReferenceStack.Pop().RawObject;
                        rfr = ReferenceStack.Pop();
                        v2 = rfr.RawObject.ExpressionOperation(StellarRoboILCodeType.Plus, v1);
                        rfr.RawObject = v2;
                        ReferenceStack.Push(StellarRoboReference.Right(v2));
                        break;
                    case StellarRoboILCodeType.MinusAssign:
                        v1 = ReferenceStack.Pop().RawObject;
                        rfr = ReferenceStack.Pop();
                        v2 = rfr.RawObject.ExpressionOperation(StellarRoboILCodeType.Minus, v1);
                        rfr.RawObject = v2;
                        ReferenceStack.Push(StellarRoboReference.Right(v2));
                        break;
                    case StellarRoboILCodeType.MultiplyAssign:
                        v1 = ReferenceStack.Pop().RawObject;
                        rfr = ReferenceStack.Pop();
                        v2 = rfr.RawObject.ExpressionOperation(StellarRoboILCodeType.Multiply, v1);
                        rfr.RawObject = v2;
                        ReferenceStack.Push(StellarRoboReference.Right(v2));
                        break;
                    case StellarRoboILCodeType.DivideAssign:
                        v1 = ReferenceStack.Pop().RawObject;
                        rfr = ReferenceStack.Pop();
                        v2 = rfr.RawObject.ExpressionOperation(StellarRoboILCodeType.Divide, v1);
                        rfr.RawObject = v2;
                        ReferenceStack.Push(StellarRoboReference.Right(v2));
                        break;
                    case StellarRoboILCodeType.AndAssign:
                        v1 = ReferenceStack.Pop().RawObject;
                        rfr = ReferenceStack.Pop();
                        v2 = rfr.RawObject.ExpressionOperation(StellarRoboILCodeType.And, v1);
                        rfr.RawObject = v2;
                        ReferenceStack.Push(StellarRoboReference.Right(v2));
                        break;
                    case StellarRoboILCodeType.OrAssign:
                        v1 = ReferenceStack.Pop().RawObject;
                        rfr = ReferenceStack.Pop();
                        v2 = rfr.RawObject.ExpressionOperation(StellarRoboILCodeType.Or, v1);
                        rfr.RawObject = v2;
                        ReferenceStack.Push(StellarRoboReference.Right(v2));
                        break;
                    case StellarRoboILCodeType.XorAssign:
                        v1 = ReferenceStack.Pop().RawObject;
                        rfr = ReferenceStack.Pop();
                        v2 = rfr.RawObject.ExpressionOperation(StellarRoboILCodeType.Xor, v1);
                        rfr.RawObject = v2;
                        ReferenceStack.Push(StellarRoboReference.Right(v2));
                        break;
                    case StellarRoboILCodeType.ModularAssign:
                        v1 = ReferenceStack.Pop().RawObject;
                        rfr = ReferenceStack.Pop();
                        v2 = rfr.RawObject.ExpressionOperation(StellarRoboILCodeType.Modular, v1);
                        rfr.RawObject = v2;
                        ReferenceStack.Push(StellarRoboReference.Right(v2));
                        break;
                    case StellarRoboILCodeType.LeftBitShiftAssign:
                        v1 = ReferenceStack.Pop().RawObject;
                        rfr = ReferenceStack.Pop();
                        v2 = rfr.RawObject.ExpressionOperation(StellarRoboILCodeType.LeftBitShift, v1);
                        rfr.RawObject = v2;
                        ReferenceStack.Push(StellarRoboReference.Right(v2));
                        break;
                    case StellarRoboILCodeType.RightBitShiftAssign:
                        v1 = ReferenceStack.Pop().RawObject;
                        rfr = ReferenceStack.Pop();
                        v2 = rfr.RawObject.ExpressionOperation(StellarRoboILCodeType.RightBitShift, v1);
                        rfr.RawObject = v2;
                        ReferenceStack.Push(StellarRoboReference.Right(v2));
                        break;
                    case StellarRoboILCodeType.NilAssign:
                        v1 = ReferenceStack.Pop().RawObject;
                        rfr = ReferenceStack.Pop();
                        v2 = rfr.RawObject.ExpressionOperation(StellarRoboILCodeType.NilAssign, v1);
                        rfr.RawObject = v2;
                        ReferenceStack.Push(StellarRoboReference.Right(v2));
                        break;

                    //特殊----------------------------------------------------------------------------
                    case StellarRoboILCodeType.StartCoroutine:
                        args = new Stack<StellarRoboObject>();
                        for (int i = 0; i < c.IntegerValue; i++) args.Push(ReferenceStack.Pop().RawObject.AsByValValue());
                        var ct = ReferenceStack.Peek().RawObject as StellarRoboScriptFunction;
                        var ict = ReferenceStack.Peek().RawObject as StellarRoboInteropFunction;
                        ReferenceStack.Pop();
                        if (ct == null && ict == null) throw new InvalidOperationException("スクリプト上のメソッド以外はコルーチン化出来ません");
                        if (!ct.Equals(null))
                        {
                            cors[c.StringValue] = new StellarRoboScriptCoroutineFrame(RunningContext, ct, args.ToArray());
                        }
                        else
                        {
                            cors[c.StringValue] = new StellarRoboInteropCoroutineFrame(RunningContext, ict, args.ToArray());
                        }
                        break;
                    case StellarRoboILCodeType.ResumeCoroutine:
                        if (!cors.ContainsKey(c.StringValue)) throw new KeyNotFoundException($"{c.StringValue}という名前のコルーチンは生成されていません。");
                        var cobj = cors[c.StringValue];
                        if (cobj == null)
                        {
                            if (c.BooleanValue) ReferenceStack.Pop();
                            ReferenceStack.Push(StellarRoboNil.Reference);
                            break;
                        }
                        var cr = cobj.Resume();
                        if (c.BooleanValue)
                        {
                            //2引数
                            var vas = ReferenceStack.Pop();
                            vas.RawObject = cr.ReturningObject;
                            ReferenceStack.Push(StellarRoboReference.Right(cr.CanResume.AsStellarRoboBoolean()));
                        }
                        else
                        {
                            //1引数
                            ReferenceStack.Push(StellarRoboReference.Right(cr.ReturningObject));
                        }
                        if (!cr.CanResume)
                        {
                            cors[c.StringValue] = null;
                        }
                        break;
                    case StellarRoboILCodeType.MakeArray:
                        var ars = new Stack<StellarRoboObject>();
                        for (int i = 0; i < c.IntegerValue; i++) ars.Push(ReferenceStack.Pop().RawObject);
                        var arr = new StellarRoboArray(new[] { (int)c.IntegerValue });
                        for (int i = 0; i < c.IntegerValue; i++) arr.array[i] = new StellarRoboReference { IsLeftValue = true, RawObject = ars.Pop() };
                        ReferenceStack.Push(StellarRoboReference.Right(arr));
                        break;
                    case StellarRoboILCodeType.Jump:
                        ProgramCounter = (int)c.IntegerValue;
                        continue;
                    case StellarRoboILCodeType.TrueJump:
                        v1 = ReferenceStack.Pop().RawObject;
                        if (v1.ToBoolean())
                        {
                            ProgramCounter = (int)c.IntegerValue;
                            continue;
                        }
                        break;
                    case StellarRoboILCodeType.FalseJump:
                        v1 = ReferenceStack.Pop().RawObject;
                        if (!v1.ToBoolean())
                        {
                            ProgramCounter = (int)c.IntegerValue;
                            continue;
                        }
                        break;
                    case StellarRoboILCodeType.Return:
#if EDITOR
                        //現在の背景色変更
                        SetDebugLineColor(c.Line + 1);
#endif

                        ReturningObject = ReferenceStack.Pop().RawObject;
                        return false;
                    case StellarRoboILCodeType.Yield:
                        ReturningObject = ReferenceStack.Pop().RawObject;
                        ProgramCounter++;
                        return true;
                    case StellarRoboILCodeType.Call:
                        args = new Stack<StellarRoboObject>();
                        for (int i = 0; i < c.IntegerValue; i++) args.Push(ReferenceStack.Pop().RawObject);
                        v1 = ReferenceStack.Pop().RawObject;
                        if (RunningContext.IsDump)
                        {
                            //ダンプ内容作成
                            DateTime now = DateTime.Now;
                            StringBuilder stringBuilder = new StringBuilder();
                            stringBuilder.Append("\n/////////////////////////////\n");
                            stringBuilder.Append(string.Format("// Date : {0}\n// Time : {1}\n", now.ToString("yyyy/MM/dd"), now.ToString("HH:mm:ss")));
                            stringBuilder.Append("/////////////////////////////\n");
                            switch (v1.ExtraType)
                            {
                                //命令
                                case "InteropFunction":
                                    stringBuilder.Append(string.Format("Function: {0}\n", ((StellarRoboInteropFunction)v1).Function.Method.Name));
                                    foreach (var arg in args.ToArray())
                                    {
                                        stringBuilder.Append(string.Format("Argument: {0}({1})\n", arg.ToString(), arg.ExtraType.ToString()));
                                    }
                                    break;
                                //関数
                                case "ScriptFunction":
                                    stringBuilder.Append(string.Format("ScriptFunction: {0}\n", ((StellarRoboScriptFunction)v1).BaseMethod.Name));
                                    break;
                                default:
                                    break;
                            }

                            //ダンプ出力
                            RunningContext.writeDump(stringBuilder.ToString());
                        }
                        if (v1 == StellarRoboNil.Instance) throw new InvalidOperationException("nilに対してメソッド呼び出し出来ません。名前を間違っていませんか？");
                        ReferenceStack.Push(StellarRoboReference.Right(v1.Call(RunningContext, args.ToArray()).ReturningObject));
                        if (RunningContext.IsDump)
                        {
                            //ダンプ内容作成
                            StringBuilder stringBuilder = new StringBuilder();

                            var result = ReferenceStack.Peek();
                            if(result.RawObject.ExtraType== "Array")
                            {
                                foreach (var obj in result.RawObject.AsArray())
                                {
                                    //ダンプ内容作成
                                    stringBuilder.Append(string.Format("Result: {0}({1})\n", obj.ToString(), obj.ExtraType.ToString()));
                                }
                            }
                            else
                            {
                                //ダンプ内容作成
                                stringBuilder.Append(string.Format("Result: {0}({1})\n", result.RawObject.ToString(), result.RawObject.ExtraType.ToString()));
                            }

                            //ダンプ出力
                            RunningContext.writeDump(stringBuilder.ToString());
                        }
                        break;
                    case StellarRoboILCodeType.IndexerCall:
                        args = new Stack<StellarRoboObject>();
                        for (int i = 0; i < c.IntegerValue; i++) args.Push(ReferenceStack.Pop().RawObject);
                        v1 = ReferenceStack.Pop().RawObject;
                        if (v1 == StellarRoboNil.Instance) throw new InvalidOperationException("nilに対してインデクサ呼び出し出来ません。名前を間違っていませんか？");
                        ReferenceStack.Push(v1.GetIndexerReference(args.ToArray()));
                        break;
                    case StellarRoboILCodeType.PushArgument:
                        ReferenceStack.Push(StellarRoboReference.Right(Arguments[(int)c.IntegerValue]));
                        break;
                    case StellarRoboILCodeType.LoadObject:
#if EDITOR
                        //現在の背景色変更
                        SetDebugLineColor(c.Line + 1);
#endif
                        string refname = c.StringValue;
                        ReferenceStack.Push(GetReference(refname));
                        break;
                    case StellarRoboILCodeType.LoadMember:
                        var or = ReferenceStack.Pop();
                        if (or.RawObject == StellarRoboNil.Instance) throw new InvalidOperationException("nilに対してメンバーアクセス出来ません。名前を間違っていませんか？");
                        ReferenceStack.Push(or.GetMemberReference(c.StringValue));
                        break;
                    case StellarRoboILCodeType.LoadVarg:
                        args = new Stack<StellarRoboObject>();
                        for (int i = 0; i < c.IntegerValue; i++) args.Push(ReferenceStack.Pop().RawObject);
                        ReferenceStack.Push(StellarRoboReference.Right(VariableArguments[(int)args.Pop().ToInt64()]));
                        break;
                    case StellarRoboILCodeType.AsValue:
                        ReferenceStack.Push(StellarRoboReference.Right(ReferenceStack.Pop().RawObject.AsByValValue()));
                        break;
                }
                ProgramCounter++;
            }
            if (ReferenceStack.Count == 0) ReferenceStack.Push(StellarRoboNil.Reference);
            ReturningObject = ReferenceStack.Pop().RawObject;
#if EDITOR
            //現在の背景色変更
            SetDebugLineColor(0);
#endif
            return false;
        }

        /// <summary>
        /// 実行位置をリセットします。
        /// </summary>
        public void Reset()
        {
            ProgramCounter = 0;
        }
    }
}
