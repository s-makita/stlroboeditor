using StellarRobo.Type;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using StellarRobo.External;
using System.IO;

namespace StellarRobo.Standard
{
#pragma warning disable 1591
    /// <summary>
    /// StellarRobo向けのアセンブリを読み込み、クラスをやりとりするための
    /// 受け渡しを担います。
    /// </summary>
    public sealed class StellarRoboExtensionLibrary : StellarRoboObject, IPartImportsSatisfiedNotification
    {

        public static readonly string ClassName = "ExtensionLibrary";

        #region 改変不要
        /// <summary>
        /// このクラスのクラスメソッドが定義される<see cref="StellarRoboInteropClassInfo"/>を取得します。
        /// こいつを適当なタイミングで<see cref="StellarRoboModule.RegisterClass(StellarRoboInteropClassInfo)"/>に
        /// 渡してください。
        /// </summary>
        internal static StellarRoboInteropClassInfo Information { get; } = new StellarRoboInteropClassInfo(ClassName);
        #endregion
        [ImportMany]
        private List<ExternalInfoFetcher> exclasses = new List<ExternalInfoFetcher>();
        private List<StellarRoboExternalClassInfo> infos = new List<StellarRoboExternalClassInfo>();
        private Dictionary<string, StellarRoboReference> classReferences = new Dictionary<string, StellarRoboReference>();


        static StellarRoboExtensionLibrary()
        {
            Information.AddClassMethod(new StellarRoboInteropMethodInfo("load_file", ClassLoadFile));
        }

        public StellarRoboExtensionLibrary()
        {
            ExtraType = ClassName;
        }

        protected internal override StellarRoboReference GetIndexerReference(StellarRoboObject[] indices)
        {
            var name = indices[0].ToString();
            if (!classReferences.ContainsKey(name)) return StellarRoboNil.Reference;
            return classReferences[name];
        }

        private static StellarRoboFunctionResult ClassLoadFile(StellarRoboContext ctx, StellarRoboObject self, StellarRoboObject[] args)
        {
            var name = Path.GetFullPath(args[0].ToString());
            var catalog = new DirectoryCatalog(Path.GetDirectoryName(name), Path.GetFileName(name));
            var result = new StellarRoboExtensionLibrary();
            var container = new CompositionContainer(catalog);
            container.ComposeParts(result);
            return result.NoResume();
        }

        public void OnImportsSatisfied()
        {
            infos.AddRange(exclasses.Select(p => p()));
            foreach (var i in infos) classReferences[i.ClassName] = StellarRoboReference.Right(new StellarRoboExtensionClass(i));
        }
    }

    /// <summary>
    /// StellarRobo外部ライブラリのクラスを定義します。
    /// </summary>
    public sealed class StellarRoboExtensionClass : StellarRoboObject
    {

        public static readonly string ClassName = "ExtensionClass";
        private StellarRoboExternalClassInfo exclass;
        private StellarRoboReference i_create = StellarRoboNil.Reference;
        private StellarRoboInteropClassObject cobj;
        private StellarRoboReference cobjRef;
        private StellarRoboInteropMethodInfo ctor;
        private bool isStatic;

        internal StellarRoboExtensionClass(StellarRoboExternalClassInfo kec)
        {
            ExtraType = ClassName;
            exclass = kec;
            cobj = new StellarRoboInteropClassObject(exclass.Information);
            cobjRef = StellarRoboReference.Right(cobj);

            isStatic = exclass.IsStaticClass;
            if (!isStatic)
            {
                i_create = StellarRoboReference.Right(this, InstanceCreate);
                ctor = cobj.Class.classMethods.FirstOrDefault(p => p.Name == "new");
                if (ctor == null) throw new CompositionException("staticクラスでないにもかかわらずコンストラクタがありません。");
            }
        }

        protected internal override StellarRoboReference GetMemberReference(string name)
        {
            switch (name)
            {
                case "create": return i_create;
                case "class": return cobjRef;
            }
            return base.GetMemberReference(name);
        }

        private StellarRoboFunctionResult InstanceCreate(StellarRoboContext ctx, StellarRoboObject self, StellarRoboObject[] args)
        {
            if (isStatic) throw new InvalidOperationException("静的クラスでコンストラクタが呼ばれています。");
            var result = ctor.Body(ctx, self, args);
            return result;
        }
    }
#pragma warning restore 1591
}
