using StellarRobo.Type;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StellarRobo.Standard
{
#pragma warning disable 1591
    /// <summary>
    /// StellarRoboと.NET連携の型の基底を提供します。
    /// 実際に作成する際はこのクラスをコピーするといいかもしれません。
    /// というかこの他にもオーバーロードできるメソッドはあるので適当にどうにかしてください。
    /// </summary>
    public sealed class StellarRoboInteropClassBase : StellarRoboObject
    {
        /// <summary>
        /// StellarRobo上でのクラス名を取得します。
        /// </summary>
        public static readonly string ClassName = "InteropBase";

        #region 改変不要
        /// <summary>
        /// このクラスのクラスメソッドが定義される<see cref="StellarRoboInteropClassInfo"/>を取得します。
        /// こいつを適当なタイミングで<see cref="StellarRoboModule.RegisterClass(StellarRoboInteropClassInfo)"/>に
        /// 渡してください。
        /// </summary>
        internal static StellarRoboInteropClassInfo Information { get; } = new StellarRoboInteropClassInfo(ClassName);
        #endregion

        #region overrideメンバー
        /// <summary>
        /// 主にInformationを初期化します。
        /// コンストラクタを含む全てのクラスメソッドはここから追加してください。
        /// 逆に登録しなければコンストラクタを隠蔽できるということでもありますが。
        /// </summary>
        static StellarRoboInteropClassBase()
        {
            Information.AddClassMethod(new StellarRoboInteropMethodInfo("new", ClassNew));
        }

        /// <summary>
        /// このクラスのインスタンスを初期化します。
        /// 要するにこのインスタンスがスクリプト中で参照されるので、
        /// インスタンスメソッドやプロパティの設定を忘れずにしてください。
        /// あと<see cref="StellarRoboObject.ExtraType"/>に型名をセットしておくと便利です。
        /// </summary>
        public StellarRoboInteropClassBase()
        {
            ExtraType = ClassName;
            RegisterInstanceFunction();
        }

        /// <summary>
        /// 指定された名前のメンバーへの参照を取得します。
        /// 「参照」というのは右辺値・左辺値どちらにもなりうる<see cref="StellarRoboReference"/>を差し、
        /// インスタンスごとに1つずつ(呼ばれる毎にnewしない)である必要があります。
        /// ここで返されるべき参照は
        /// ・インスタンスメソッド
        /// ・プロパティ
        /// などです。どちらもフィールドに<see cref="StellarRoboReference"/>のインスタンスを確保して
        /// switch分岐でそれらを返すことが推奨されます。
        /// </summary>
        /// <param name="name">メンバー名</param>
        /// <returns></returns>
        protected internal override StellarRoboReference GetMemberReference(string name) => base.GetMemberReference(name);

        /// <summary>
        /// この<see cref="StellarRoboObject"/>を「呼び出し」ます。
        /// このインスタンスそのものがメソッドのオブジェクトであるかのように振る舞います。
        /// あまり乱用するべきではありません。
        /// </summary>
        /// <param name="context">呼び出される時の<see cref="StellarRoboContext"/></param>
        /// <param name="args">引数</param>
        /// <returns>
        /// 返り値。基本的にはresumeできないと思うので適当な<see cref="StellarRoboObject"/>に<see cref="TypeExtensions.NoResume(StellarRoboObject)"/>してください。
        /// 参考にしてください。
        /// </returns>
        protected internal override StellarRoboFunctionResult Call(StellarRoboContext context, StellarRoboObject[] args) => base.Call(context, args);

        /// <summary>
        /// インデクサーの参照を得ます。
        /// <see cref="GetMemberReference(string)"/>と<see cref="StellarRoboObject.Call(StellarRoboContext, StellarRoboObject[])"/>の
        /// 中間のような存在です。
        /// </summary>
        /// <param name="indices">インデックス</param>
        /// <returns>返す参照</returns>
        protected internal override StellarRoboReference GetIndexerReference(StellarRoboObject[] indices) => base.GetIndexerReference(indices);

        /// <summary>
        /// 二項演算をします。
        /// 実際には単項演算子でも適用されるため、-(<see cref="StellarRoboILCodeType.Negative"/>)と
        /// !(<see cref="StellarRoboILCodeType.Not"/>)にも対応出来ます。
        /// <see cref="StellarRoboILCodeType"/>内にはその他の演算も含まれていますが、
        /// 複合演算子はILレベルで処理されるので対応する意味はありません。
        /// ちなみに<see cref="StellarRoboObject"/>との演算方法はどのように実装しても構いません。
        /// StellarRobo内部では<see cref="StellarRoboObject.Type"/>の比較と内部のValueプロパティによる比較となっています。
        /// thisで比較すると99%falseになってしまうので注意してください。
        /// </summary>
        /// <param name="op">演算子</param>
        /// <param name="target">対象のインスタンス</param>
        /// <returns></returns>
        protected internal override StellarRoboObject ExpressionOperation(StellarRoboILCodeType op, StellarRoboObject target) => base.ExpressionOperation(op, target);

        /// <summary>
        /// 値渡しの際に渡すオブジェクトを生成します。
        /// 値型の場合は必ずこれをオーバーライドしてください。それ以外の場合は原則的に挙動が参照型になります。
        /// </summary>
        /// <returns>クローン</returns>
        public override StellarRoboObject AsByValValue() => base.AsByValValue();

        /// <summary>
        /// 値的に等価であるか比較します。
        /// Dictionaryなどで利用されるのでできるだけオーバーライドしましょう。
        /// ==演算子の方はその機能をExpressionOperationで担うのでオーバーライドする必要はありません。
        /// </summary>
        /// <param name="obj">
        /// 比較対象。
        /// デフォルト実装のようにasキャストしてnull比較と内部値比較を同時に行うのが早いかと思われます。
        /// 重ね重ね言いますが==はオーバーライドする必要はありません。むしろしないでください。
        /// </param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            var t = obj as StellarRoboInteropClassBase;
            return t != null && t.Value == Value;
        }

        /// <summary>
        /// この<see cref="StellarRoboObject"/>のインスタンスを表す値を返します。
        /// 同じくDictionaryなどで利用されるので同じ値を表す<see cref="StellarRoboObject"/>が
        /// 同じ値を表すように振る舞いましょう。
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode() => Value.GetHashCode();
        #endregion

        #region インスタンスメソッド
        //nameof使おうな
        //StellarRoboReference instance_method;

        private void RegisterInstanceFunction()
        {

        }
        #endregion

        #region クラスメソッド
        /* 
        当たり前ですがクラスメソッド呼び出しではselfはnullになります。
        selfに代入するのではなく生成したのをNoResumeで返却します。
        */

        private static StellarRoboFunctionResult ClassNew(StellarRoboContext ctx, StellarRoboObject self, StellarRoboObject[] args) => new StellarRoboInteropClassBase().NoResume();
        #endregion
    }
#pragma warning restore 1591
}
