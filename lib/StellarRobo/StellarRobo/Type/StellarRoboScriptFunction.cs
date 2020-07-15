using System.Collections.Generic;
using System.Linq;

namespace StellarRobo.Type
{
    /// <summary>
    /// StellarRoboで定義されたメソッドのオブジェクトを定義します。
    /// </summary>
    public sealed class StellarRoboScriptFunction : StellarRoboObject
    {
        /// <summary>
        /// 基となるメソッドを取得します。
        /// </summary>
        public StellarRoboScriptMethodInfo BaseMethod { get; }

        /// <summary>
        /// クラスメソッド・インスタンスメソッドの場合、その<see cref="StellarRoboScriptClassInfo"/>を取得します。
        /// </summary>
        public StellarRoboScriptClassInfo BelongingClass { get; }

        /// <summary>
        /// インスタンスを取得します。
        /// </summary>
        public StellarRoboObject Instance { get; }

        /// <summary>
        /// このメソッドの現在のフレームを取得します。
        /// </summary>
        public StellarRoboStackFrame CurrentFrame { get; private set; }

        /// <summary>
        /// 新しいインスタンスを生成します。
        /// </summary>
        /// <param name="inst">インスタンス</param>
        /// <param name="method">メソッド</param>
        public StellarRoboScriptFunction(StellarRoboObject inst, StellarRoboScriptMethodInfo method)
        {
            ExtraType = "ScriptFunction";
            Instance = inst ?? StellarRoboNil.Instance;
            BaseMethod = method;
        }

        /// <summary>
        /// 呼び出します。
        /// </summary>
        /// <param name="context"></param>
        /// <param name="args">引数</param>
        /// <returns>返り値</returns>
        protected internal override StellarRoboFunctionResult Call(StellarRoboContext context, StellarRoboObject[] args)
        {
            if (args != null)
            {
                CurrentFrame = new StellarRoboStackFrame(context, BaseMethod.Codes);
                CurrentFrame.Locals["self"] = StellarRoboReference.Right(Instance);
                CurrentFrame.Arguments = args;
                if (args.Length > BaseMethod.ArgumentLength)
                {
                    CurrentFrame.VariableArguments = args.Skip(BaseMethod.ArgumentLength).ToList();
                }
            }
            var r = CurrentFrame.Resume();
            return new StellarRoboFunctionResult(CurrentFrame.ReturningObject, r);
        }
    }
}
