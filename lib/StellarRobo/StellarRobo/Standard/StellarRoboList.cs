using StellarRobo.Type;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StellarRobo.Standard
{
    /// <summary>
    /// StellarRoboでのリストを定義します。
    /// </summary>
    public sealed class StellarRoboList : StellarRoboObject
    {
        /// <summary>
        /// クラス名を取得します。
        /// </summary>
        public static readonly string ClassName = "List";
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
        static StellarRoboList()
        {
            Information.AddClassMethod(new StellarRoboInteropMethodInfo("new", ClassNew));
        }

        private List<StellarRoboReference> list = new List<StellarRoboReference>();
        /// <summary>
        /// このクラスのインスタンスを初期化します。
        /// 要するにこのインスタンスがスクリプト中で参照されるので、
        /// インスタンスメソッドやプロパティの設定を忘れずにしてください。
        /// あと<see cref="StellarRoboObject.ExtraType"/>に型名をセットしておくと便利です。
        /// </summary>
        public StellarRoboList()
        {
            ExtraType = ClassName;
            RegisterInstanceFunction();
        }

        /// <summary>
        /// 要素を指定して新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="refs">追加する要素</param>
        public StellarRoboList(IEnumerable<StellarRoboObject> refs) : base()
        {
            foreach (var i in refs) list.Add(StellarRoboReference.Left(i));
        }

        private StellarRoboList(IEnumerable<StellarRoboReference> refs) : base()
        {
            list.AddRange(refs);
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
                case nameof(add): return add;
                case nameof(add_range): return add_range;
                case nameof(insert): return insert;
                case nameof(clear): return clear;
                case nameof(each): return each;
                case nameof(remove_at): return remove_at;
                case nameof(remove_by): return remove_by;
                case nameof(map): return map;
                case nameof(reduce): return reduce;
                case nameof(filter): return filter;

                case "length": return StellarRoboReference.Right(list.Count);
            }
            return base.GetMemberReference(name);
        }

        /// <summary>
        /// インデクサーの参照を得ます。
        /// <see cref="StellarRoboObject.GetMemberReference(string)"/>と<see cref="StellarRoboObject.Call(StellarRoboContext, StellarRoboObject[])"/>の
        /// 中間のような存在です。
        /// </summary>
        /// <param name="indices">インデックス</param>
        /// <returns>返す参照</returns>
        protected internal override StellarRoboReference GetIndexerReference(StellarRoboObject[] indices) => list[(int)indices[0].ToInt64()];
        #endregion

        #region インスタンスメソッド
        //Dictionary解決でもいいかも
        StellarRoboReference
            add, add_range, insert, each, remove_at, remove_by,
            filter, map, reduce, clear, any, all;

        private void RegisterInstanceFunction()
        {
            add = StellarRoboReference.Right(this, InstanceAdd);
            add_range = StellarRoboReference.Right(this, InstanceAddRange);
            clear = StellarRoboReference.Right(this, InstanceClear);
            insert = StellarRoboReference.Right(this, InstanceInsert);
            each = StellarRoboReference.Right(this, InstanceEach);
            remove_at = StellarRoboReference.Right(this, InstanceRemoveAt);
            remove_by = StellarRoboReference.Right(this, InstanceRemoveBy);
            filter = StellarRoboReference.Right(this, InstanceFilter);
            map = StellarRoboReference.Right(this, InstanceMap);
            reduce = StellarRoboReference.Right(this, InstanceReduce);
            any = StellarRoboReference.Right(this, list.Select(p => p.RawObject).GenerateAnyFunction());
            all = StellarRoboReference.Right(this, list.Select(p => p.RawObject).GenerateAllFunction());
        }

        private StellarRoboFunctionResult InstanceAdd(StellarRoboContext ctx, StellarRoboObject self, StellarRoboObject[] args)
        {
            foreach (var i in args)
                list.Add(StellarRoboReference.Left(i));
            return StellarRoboNil.Instance.NoResume();
        }

        private StellarRoboFunctionResult InstanceAddRange(StellarRoboContext ctx, StellarRoboObject self, StellarRoboObject[] args)
        {
            var al = args[0].AsArray();
            list.AddRange(al.Select(p => StellarRoboReference.Left(p)));
            return StellarRoboNil.Instance.NoResume();
        }

        private StellarRoboFunctionResult InstanceClear(StellarRoboContext ctx, StellarRoboObject self, StellarRoboObject[] args)
        {
            list.Clear();
            return StellarRoboNil.Instance.NoResume();
        }

        private StellarRoboFunctionResult InstanceInsert(StellarRoboContext ctx, StellarRoboObject self, StellarRoboObject[] args)
        {
            list.Insert((int)args[0].ToInt64(), new StellarRoboReference { IsLeftValue = true, RawObject = args[1] });
            return StellarRoboNil.Instance.NoResume();
        }

        private StellarRoboFunctionResult InstanceRemoveAt(StellarRoboContext ctx, StellarRoboObject self, StellarRoboObject[] args)
        {
            list.RemoveAt((int)args[0].ToInt64());
            return StellarRoboNil.Instance.NoResume();
        }

        private StellarRoboFunctionResult InstanceRemoveBy(StellarRoboContext ctx, StellarRoboObject self, StellarRoboObject[] args)
        {
            list.RemoveAll(p => args[0].CallAsPredicate(ctx, p.RawObject));
            return StellarRoboNil.Instance.NoResume();
        }

        private StellarRoboFunctionResult InstanceEach(StellarRoboContext ctx, StellarRoboObject self, StellarRoboObject[] args)
        {
            foreach (var i in list) args[0].Call(ctx, new[] { i.RawObject });
            return StellarRoboNil.Instance.NoResume();
        }

        private StellarRoboFunctionResult InstanceFilter(StellarRoboContext ctx, StellarRoboObject self, StellarRoboObject[] args)
        {
            var r = list.Where(p => args[0].CallAsPredicate(ctx, p.RawObject));
            return new StellarRoboList(r).NoResume();
        }

        private StellarRoboFunctionResult InstanceMap(StellarRoboContext ctx, StellarRoboObject self, StellarRoboObject[] args)
        {
            var r = list.Select(p => args[0].Call(ctx, new[] { p.RawObject }).ReturningObject);
            return new StellarRoboList(r).NoResume();
        }

        private StellarRoboFunctionResult InstanceReduce(StellarRoboContext ctx, StellarRoboObject self, StellarRoboObject[] args)
        {
            var r = list.Select(p => p.RawObject).Aggregate((p, q) => args[0].Call(ctx, new[] { p, q }).ReturningObject);
            return r.NoResume();
        }

#pragma warning disable 1591
        public override bool Equals(object obj)
        {
            var t = obj as StellarRoboList;
            return t != null && list.Equals(t.list);
        }

        public override int GetHashCode() => list.GetHashCode();
#pragma warning restore 1591

        #endregion

        #region クラスメソッド
        /* 
        当たり前ですがクラスメソッド呼び出しではselfはnullになります。
        selfに代入するのではなく生成したのをNoResumeで返却します。
        */

        private static StellarRoboFunctionResult ClassNew(StellarRoboContext ctx, StellarRoboObject self, StellarRoboObject[] args) => new StellarRoboList().NoResume();
        #endregion
    }
}
