using System.Collections.Generic;
using System.Linq;

namespace StellarRobo.Type
{
    /// <summary>
    /// StellarRoboで定義されたクラスのインスタンスを定義します。
    /// </summary>
    public class StellarRoboInstance : StellarRoboObject
    {
        private Dictionary<string, StellarRoboReference> localReferences = new Dictionary<string, StellarRoboReference>();
        /// <summary>
        /// このインスタンスのフィールドの参照を取得します。
        /// </summary>
        public IReadOnlyDictionary<string, StellarRoboReference> LocalFieldReferences { get; }

        private Dictionary<string, StellarRoboReference> methodReferences = new Dictionary<string, StellarRoboReference>();
        /// <summary>
        /// このインスタンスのフィールドの参照を取得します。
        /// </summary>
        public IReadOnlyDictionary<string, StellarRoboReference> InstanceMethodReferences { get; }

        /// <summary>
        /// このインスタンスのクラスを取得します。
        /// </summary>
        public StellarRoboClassInfo Class { get; protected internal set; }

        /// <summary>
        /// 特定の<see cref="StellarRoboScriptClassInfo"/>を元にして、インスタンスを生成します。
        /// コンストラクタがあった場合、呼び出します。
        /// </summary>
        /// <param name="klass">クラス</param>
        /// <param name="ctx">コンテキスト</param>
        /// <param name="ctorArgs">コンストラクタ引数</param>
        public StellarRoboInstance(StellarRoboScriptClassInfo klass, StellarRoboContext ctx, StellarRoboObject[] ctorArgs)
        {
            Class = klass;
            ExtraType = klass.Name;
            LocalFieldReferences = localReferences;
            InstanceMethodReferences = methodReferences;
            foreach (var i in klass.LocalInfos)
            {
                localReferences[i.Name] = new StellarRoboReference() { IsLeftValue = true };
                if (i.InitializeIL != null)
                {
                    localReferences[i.Name].RawObject = ctx.ExecuteExpressionIL(i.InitializeIL);
                }
            }
            foreach (var i in klass.methods)
                methodReferences[i.Name] = new StellarRoboReference()
                {
                    IsLeftValue = true,
                    RawObject = new StellarRoboScriptFunction(this, i)
                };
            var ctor = klass.classMethods.FirstOrDefault(p => p.Name == "new");
            if (ctor != null)
            {
                new StellarRoboScriptFunction(this, ctor).Call(ctx, ctorArgs);
            }
        }

        /// <summary>
        /// 特定の<see cref="StellarRoboInteropClassInfo"/>を元にして、インスタンスを生成します。
        /// コンストラクタがあった場合、呼び出します。
        /// </summary>
        /// <param name="klass">クラス</param>
        /// <param name="ctx">コンテキスト</param>
        /// <param name="ctorArgs">コンストラクタ引数</param>
        public StellarRoboInstance(StellarRoboInteropClassInfo klass, StellarRoboContext ctx, StellarRoboObject[] ctorArgs)
        {
            Class = klass;
            ExtraType = klass.Name;
            LocalFieldReferences = localReferences;
            InstanceMethodReferences = methodReferences;
            foreach (var i in klass.LocalInfos)
            {
                localReferences[i.Name] = StellarRoboReference.Left(i.Value.AsByValValue());
            }
            foreach (var i in klass.methods)
                methodReferences[i.Name] = new StellarRoboReference()
                {
                    IsLeftValue = true,
                    RawObject = new StellarRoboInteropFunction(this, i.Body)
                };
            var ctor = klass.classMethods.FirstOrDefault(p => p.Name == "new");
            if (ctor != null) ctor.Body(ctx, this, ctorArgs);
        }

        /// <summary>
        /// メンバーの参照を取得します。
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        protected internal override StellarRoboReference GetMemberReference(string name)
        {
            if (LocalFieldReferences.ContainsKey(name)) return LocalFieldReferences[name];
            if (InstanceMethodReferences.ContainsKey(name)) return InstanceMethodReferences[name];
            StellarRoboReference result;
            if ((result = base.GetMemberReference(name)) == StellarRoboNil.Reference)
            {
                result = localReferences[name] = new StellarRoboReference { IsLeftValue = true };
            }
            return result;
        }

    }
}
