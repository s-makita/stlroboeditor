namespace StellarRobo.Type
{
    /// <summary>
    /// StellarRoboでの.NET連携メソッドを定義します。
    /// </summary>
    public class StellarRoboInteropFunction : StellarRoboObject
    {
        /// <summary>
        /// 呼び出し対象のデリゲートを取得・設定します。
        /// </summary>
        public StellarRoboInteropDelegate Function { get; set; }

        /// <summary>
        /// インスタンスを取得・設定します。
        /// </summary>
        public StellarRoboObject Instance { get; }

        /// <summary>
        /// 呼び出します。
        /// </summary>
        /// <param name="context"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        protected internal override StellarRoboFunctionResult Call(StellarRoboContext context, StellarRoboObject[] args) => Function(context, Instance, args);

        /// <summary>
        /// 新しいインスタンスを生成します。
        /// </summary>
        /// <param name="inst">インスタンス</param>
        /// <param name="method">メソッド</param>
        public StellarRoboInteropFunction(StellarRoboObject inst, StellarRoboInteropDelegate method)
        {
            Instance = inst;
            Function = method;
            ExtraType = "InteropFunction";
        }
    }

    /// <summary>
    /// StellarRoboでの.NET連携インスタンスメソッドの形式を定義します。
    /// </summary>
    /// <param name="context">実行される<see cref="StellarRoboContext"/></param>
    /// <param name="self">インスタンス。インスタンスメソッドでない場合はnullです。</param>
    /// <param name="args">引数。コルーチンで継続中の場合はnullです。</param>
    /// <returns>返り値</returns>
    public delegate StellarRoboFunctionResult StellarRoboInteropDelegate(StellarRoboContext context, StellarRoboObject self, StellarRoboObject[] args);
}
