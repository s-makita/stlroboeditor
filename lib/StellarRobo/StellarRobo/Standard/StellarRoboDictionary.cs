using StellarRobo.Type;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StellarRobo.Standard
{
    /// <summary>
    /// StellarRobo上の辞書(ディクショナリ)構造を提供します。
    /// </summary>
    public sealed class StellarRoboDictionary : StellarRoboObject
    {
        /// <summary>
        /// StellarRobo上でのクラス名を取得します。
        /// </summary>
        public static readonly string ClassName = "Dictionary";

        private Dictionary<StellarRoboObject, StellarRoboReference> dict = new Dictionary<StellarRoboObject, StellarRoboReference>();

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
        static StellarRoboDictionary()
        {
            Information.AddClassMethod(new StellarRoboInteropMethodInfo("new", ClassNew));
        }

        /// <summary>
        /// このクラスのインスタンスを初期化します。
        /// 要するにこのインスタンスがスクリプト中で参照されるので、
        /// インスタンスメソッドやプロパティの設定を忘れずにしてください。
        /// あと<see cref="StellarRoboObject.ExtraType"/>に型名をセットしておくと便利です。
        /// </summary>
        public StellarRoboDictionary()
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
        protected internal override StellarRoboReference GetMemberReference(string name)
        {
            switch (name)
            {
                case nameof(each): return each;
                case nameof(remove): return remove;
                case nameof(has_key): return has_key;

                case "length": return StellarRoboReference.Right(dict.Count);
            }
            return base.GetMemberReference(name);
        }

        /// <summary>
        /// インデクサーの参照を得ます。
        /// <see cref="GetMemberReference(string)"/>と<see cref="StellarRoboObject.Call(StellarRoboContext, StellarRoboObject[])"/>の
        /// 中間のような存在です。
        /// </summary>
        /// <param name="indices">インデックス</param>
        /// <returns>返す参照</returns>
        protected internal override StellarRoboReference GetIndexerReference(StellarRoboObject[] indices)
        {
            if (!dict.ContainsKey(indices[0]))
            {
                dict[indices[0]] = StellarRoboReference.Left(StellarRoboNil.Instance);
            }
            return dict[indices[0]];
        }

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
            var t = obj as StellarRoboDictionary;
            return t != null && t.dict.Equals(dict);
        }

        /// <summary>
        /// この<see cref="StellarRoboObject"/>のインスタンスを表す値を返します。
        /// 同じくDictionaryなどで利用されるので同じ値を表す<see cref="StellarRoboObject"/>が
        /// 同じ値を表すように振る舞いましょう。
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode() => dict.GetHashCode();
        #endregion


        #region インスタンスメソッド
        //nameof使おうな
        StellarRoboReference
            each, remove, has_key;

        private void RegisterInstanceFunction()
        {
            each = StellarRoboReference.Right(this, InstanceEach);
            remove = StellarRoboReference.Right(this, InstanceRemove);
            has_key = StellarRoboReference.Right(this, InstanceHasKey);
        }

        private StellarRoboFunctionResult InstanceHasKey(StellarRoboContext ctx, StellarRoboObject self, StellarRoboObject[] args)
        {
            dict.ContainsKey(args[0]);
            return dict.ContainsKey(args[0]).AsStellarRoboBoolean().NoResume();
        }

        private StellarRoboFunctionResult InstanceRemove(StellarRoboContext ctx, StellarRoboObject self, StellarRoboObject[] args)
        {
            dict.Remove(args[0]);
            return StellarRoboNil.Instance.NoResume();
        }

        private StellarRoboFunctionResult InstanceEach(StellarRoboContext ctx, StellarRoboObject self, StellarRoboObject[] args)
        {
            foreach (var i in dict) args[0].Call(ctx, new[] { i.Key, i.Value.RawObject });
            return StellarRoboNil.Instance.NoResume();
        }

        #endregion

        #region クラスメソッド
        /* 
        当たり前ですがクラスメソッド呼び出しではselfはnullになります。
        selfに代入するのではなく生成したのをNoResumeで返却します。
        */

        private static StellarRoboFunctionResult ClassNew(StellarRoboContext ctx, StellarRoboObject self, StellarRoboObject[] args) => new StellarRoboDictionary().NoResume();
        #endregion
    }


}
