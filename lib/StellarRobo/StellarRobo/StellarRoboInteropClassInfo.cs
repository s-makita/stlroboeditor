using StellarRobo.Type;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StellarRobo
{
    /// <summary>
    /// .NET連携クラスの規定を提供します。
    /// </summary>
    public sealed class StellarRoboInteropClassInfo : StellarRoboClassInfo
    {
        internal List<StellarRoboInteropClassInfo> inners = new List<StellarRoboInteropClassInfo>();
        internal List<StellarRoboInteropMethodInfo> methods = new List<StellarRoboInteropMethodInfo>();
        internal List<StellarRoboInteropMethodInfo> classMethods = new List<StellarRoboInteropMethodInfo>();
        private List<string> locals = new List<string>();
        internal IList<StellarRoboInteropClassLocalInfo> LocalInfos { get; } = new List<StellarRoboInteropClassLocalInfo>();
        internal IList<StellarRoboInteropClassLocalInfo> ConstInfos { get; } = new List<StellarRoboInteropClassLocalInfo>();
        /// <summary>
        /// 新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="name">クラス名</param>
        public StellarRoboInteropClassInfo(string name)
        {
            Name = name;
            InnerClasses = inners;
            InstanceMethods = methods;
            ClassMethods = ClassMethods;
            BaseClass = "";
        }

        /// <summary>
        /// インナークラスを追加します。
        /// </summary>
        /// <param name="klass">追加するクラス</param>
        public void AddInnerClass(StellarRoboInteropClassInfo klass)
        {
            if (inners.Any(p => p.Name == klass.Name)) throw new ArgumentException("同じ名前のインナークラスがすでに存在します。");
            inners.Add(klass);
        }

        /// <summary>
        /// メソッドを追加します。
        /// </summary>
        /// <param name="method">追加するメソッド</param>
        public void AddInstanceMethod(StellarRoboInteropMethodInfo method)
        {
            methods.Add(method);
        }

        /// <summary>
        /// クラスメソッドを追加します。
        /// </summary>
        /// <param name="method">追加するメソッド</param>
        public void AddClassMethod(StellarRoboInteropMethodInfo method)
        {
            classMethods.Add(method);
        }

        /// <summary>
        /// フィールドを追加します。
        /// </summary>
        /// <param name="local">追加するメソッド</param>
        public void AddLocal(string local)
        {
            locals.Add(local);
            LocalInfos.Add(new StellarRoboInteropClassLocalInfo { Name = local, Value = StellarRoboNil.Instance });
        }

        /// <summary>
        /// フィールドを追加します。
        /// </summary>
        /// <param name="local">追加するメソッド</param>
        /// <param name="obj">設定する初期値</param>
        public void AddLocal(string local, StellarRoboObject obj)
        {
            locals.Add(local);
            LocalInfos.Add(new StellarRoboInteropClassLocalInfo { Name = local, Value = obj });
        }

        /// <summary>
        /// クラス定数を追加します。
        /// </summary>
        /// <param name="local">名前</param>
        /// <param name="obj">値</param>
        public void AddConstant(string local, StellarRoboObject obj)
        {
            ConstInfos.Add(new StellarRoboInteropClassLocalInfo { Name = local, Value = obj });
        }

        internal sealed class StellarRoboInteropClassLocalInfo
        {
            public string Name { get; set; }
            public StellarRoboObject Value { get; set; }
        }

        #region ヘルパー
        /// <summary>
        /// 指定した列挙体から同等の<see cref="StellarRoboInteropClassInfo"/>を作成します。
        /// </summary>
        /// <param name="enumType">作成する列挙体の<see cref="System.Type"/>オブジェクト</param>
        /// <returns></returns>
        public static StellarRoboInteropClassInfo CreateFromEnum(System.Type enumType)
        {
            var type = enumType;
            var result = new StellarRoboInteropClassInfo(type.Name);
            var names = Enum.GetNames(type);
            foreach (var i in names)
            {
                var val = (int)Enum.Parse(type, i);
                result.AddConstant(i, val.AsStellarRoboInteger());
            }
            return result;
        }
        #endregion
    }
}
