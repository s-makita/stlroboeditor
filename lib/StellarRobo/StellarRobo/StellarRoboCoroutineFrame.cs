using StellarRobo.Type;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StellarRobo
{
    /// <summary>
    /// コルーチン実行の際にデータを保持します。
    /// </summary>
    public class StellarRoboCoroutineFrame
    {
        /// <summary>
        /// 再開します。
        /// </summary>
        /// <returns></returns>
        public virtual StellarRoboFunctionResult Resume() => StellarRoboNil.Instance.NoResume();
    }

    internal sealed class StellarRoboInteropCoroutineFrame : StellarRoboCoroutineFrame
    {
        StellarRoboInteropFunction Function { get; }
        StellarRoboContext Context { get; }
        StellarRoboObject[] Args { get; }

        public StellarRoboInteropCoroutineFrame(StellarRoboContext ctx, StellarRoboInteropFunction func, StellarRoboObject[] args)
        {
            Function = func;
            Context = ctx;
        }

        public override StellarRoboFunctionResult Resume() => Function.Function(Context, Function.Instance, Args);
    }

    internal sealed class StellarRoboScriptCoroutineFrame : StellarRoboCoroutineFrame
    {
        StellarRoboStackFrame StackFrame { get; }
        StellarRoboObject[] Args { get; }

        public StellarRoboScriptCoroutineFrame(StellarRoboContext ctx, StellarRoboScriptFunction func, StellarRoboObject[] args)
        {
            StackFrame = new StellarRoboStackFrame(ctx, func.BaseMethod.Codes);
            Args = args;
            StackFrame.Arguments = Args;
        }

        public override StellarRoboFunctionResult Resume()
        {
            var s = StackFrame.Resume();
            return new StellarRoboFunctionResult(StackFrame.ReturningObject, s);
        }
    }
}
