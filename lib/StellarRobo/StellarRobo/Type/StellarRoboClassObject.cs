using System.Collections.Generic;

namespace StellarRobo.Type
{
    /// <summary>
    /// クラスメソッドを提供するオブジェクトを定義します。
    /// </summary>
    public sealed class StellarRoboScriptClassObject : StellarRoboObject
    {
        /// <summary>
        /// 元になるクラスを取得します。
        /// </summary>
        public StellarRoboScriptClassInfo Class { get; }

        /// <summary>
        /// コンストラクターの参照を取得します。
        /// </summary>
        public StellarRoboReference Constructor { get; }

        private Dictionary<string, StellarRoboReference> inners = new Dictionary<string, StellarRoboReference>();
        private Dictionary<string, StellarRoboReference> methods = new Dictionary<string, StellarRoboReference>();
        /// <summary>
        /// 新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="info"></param>
        public StellarRoboScriptClassObject(StellarRoboScriptClassInfo info)
        {
            Class = info;
            ExtraType = "ScriptClass";
            Constructor = StellarRoboReference.Right(this, CreateInstance);
            foreach (var i in Class.classMethods)
            {
                methods[i.Name] = (StellarRoboReference.Right(new StellarRoboScriptFunction(StellarRoboNil.Instance, i)));
            }
            foreach (var i in Class.inners)
            {
                inners[i.Name] = (StellarRoboReference.Right(new StellarRoboScriptClassObject(i)));
            }
        }

        /// <summary>
        /// メンバーの参照を取得します。
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        protected internal override StellarRoboReference GetMemberReference(string name)
        {
            switch (name)
            {
                case "new":
                    return Constructor;
                default:
                    if (methods.ContainsKey(name)) return methods[name];
                    if (inners.ContainsKey(name)) return inners[name];
                    return StellarRoboNil.Reference;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        protected internal override StellarRoboFunctionResult Call(StellarRoboContext context, StellarRoboObject[] args) => new StellarRoboInstance(Class, context, args).NoResume();

        private StellarRoboFunctionResult CreateInstance(StellarRoboContext context, StellarRoboObject self, StellarRoboObject[] args) => new StellarRoboInstance(Class, context, args).NoResume();
    }

    /// <summary>
    /// クラスメソッドを提供するオブジェクトを定義します。
    /// </summary>
    public sealed class StellarRoboInteropClassObject : StellarRoboObject
    {
        /// <summary>
        /// 元になるクラスを取得します。
        /// </summary>
        public StellarRoboInteropClassInfo Class { get; }

        private Dictionary<string, StellarRoboReference> methods = new Dictionary<string, StellarRoboReference>();
        private Dictionary<string, StellarRoboReference> consts = new Dictionary<string, StellarRoboReference>();

        /// <summary>
        /// 新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="info"></param>
        public StellarRoboInteropClassObject(StellarRoboInteropClassInfo info)
        {
            Class = info;
            foreach (var i in Class.classMethods) methods[i.Name] = StellarRoboReference.Right(StellarRoboNil.Instance, i.Body);
            foreach (var i in Class.ConstInfos) consts[i.Name] = StellarRoboReference.Right(i.Value);
        }

        /// <summary>
        /// メンバーの参照を取得します。
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        protected internal override StellarRoboReference GetMemberReference(string name)
        {
            switch (name)
            {
                /*
                case "new":
                    return Constructor;
                */
                default:
                    if (methods.ContainsKey(name)) return methods[name];
                    if (consts.ContainsKey(name)) return methods[name];
                    return StellarRoboNil.Reference;
            }
        }

        //private StellarRoboFunctionResult CreateInstance(StellarRoboContext context, StellarRoboObject self, StellarRoboObject[] args) => new StellarRoboInstance(Class, context, args).NoResume();
    }
}
