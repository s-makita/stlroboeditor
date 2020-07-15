using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using StellarRobo.Type;
using System.IO;

namespace StellarRobo.Standard
{
#pragma warning disable 1591
    /// <summary>
    /// リフレクションでどうにかします。
    /// </summary>
    public sealed class StellarRoboDynamicLibrary : StellarRoboObject
    {
        public static readonly string ClassName = "DynamicLibrary";
        internal static StellarRoboInteropClassInfo Information { get; } = new StellarRoboInteropClassInfo(ClassName);
        private Assembly assembly;

        static StellarRoboDynamicLibrary()
        {
            Information.AddClassMethod(new StellarRoboInteropMethodInfo("load_file", ClassLoadFile));
        }

        private StellarRoboDynamicLibrary(Assembly asm)
        {
            ExtraType = ClassName;
            assembly = asm;
        }

        protected internal override StellarRoboReference GetIndexerReference(StellarRoboObject[] indices)
        {
            var tn = indices[0].ToString();
            var type = assembly.GetType(tn);
            return StellarRoboReference.Right(new StellarRoboDynamicType(type));
        }

        private static StellarRoboFunctionResult ClassLoadFile(StellarRoboContext ctx, StellarRoboObject self, StellarRoboObject[] args)
        {
            var fn = args[0].ToString();
            return new StellarRoboDynamicLibrary(Assembly.LoadFile(Path.GetFullPath(fn))).NoResume();
        }
    }

    /// <summary>
    /// リフレクションでどうにかしたやつのあれです。
    /// </summary>
    public sealed class StellarRoboDynamicType : StellarRoboObject
    {
        public static readonly string ClassName = "DynamicLibraryType";
        internal static StellarRoboInteropClassInfo Information { get; } = new StellarRoboInteropClassInfo(ClassName);
        private System.Type type;
        private StellarRoboReference name, create_instance;

        internal StellarRoboDynamicType(System.Type t)
        {
            ExtraType = ClassName;
            type = t;
            name = StellarRoboReference.Right(type.Name);
            create_instance = StellarRoboReference.Right(new StellarRoboInteropFunction(this, InstanceCreareInstance));
        }

        protected internal override StellarRoboReference GetMemberReference(string name)
        {
            if (name == "create") return create_instance;
            return base.GetMemberReference(name);
        }

        private StellarRoboFunctionResult InstanceCreareInstance(StellarRoboContext ctx, StellarRoboObject self, StellarRoboObject[] args)
        {
            List<object> ia = new List<object>();
            foreach (var i in args)
            {
                switch (i.Type)
                {
                    case TypeCode.Boolean:
                        ia.Add(i.ToBoolean());
                        break;
                    case TypeCode.Double:
                        ia.Add(i.ToDouble());
                        break;
                    case TypeCode.Int64:
                        ia.Add(i.ToInt32());
                        break;
                    case TypeCode.String:
                        ia.Add(i.ToString());
                        break;
                    case TypeCode.Empty:
                        ia.Add(null);
                        break;
                    default:
                        var t = i as StellarRoboDynamicLibraryObject;
                        ia.Add(t != null ? t.rawobj : null);
                        break;
                }
            }
            var obj = Activator.CreateInstance(type, ia.ToArray());
            return new StellarRoboDynamicLibraryObject(obj).NoResume();
        }
    }

    /// <summary>
    /// リフレクションでどうにかしたやつのあれです。
    /// </summary>
    public sealed class StellarRoboDynamicLibraryObject : StellarRoboObject
    {
        public static readonly string ClassName = "DynamicLibraryObject";
        internal static StellarRoboInteropClassInfo Information { get; } = new StellarRoboInteropClassInfo(ClassName);
        private System.Type type;
        internal object rawobj;
        private StellarRoboReference name;
        private Dictionary<string, StellarRoboReference> methodCache = new Dictionary<string, StellarRoboReference>();
        private Dictionary<string, StellarRoboReference> fieldCache = new Dictionary<string, StellarRoboReference>();

        internal StellarRoboDynamicLibraryObject(object self)
        {
            ExtraType = ClassName;
            type = self?.GetType();
            name = StellarRoboReference.Right(type?.Name ?? "");
            rawobj = self;
        }

        protected internal override StellarRoboReference GetMemberReference(string name)
        {
            if (!methodCache.ContainsKey(name))
            {
                var mi = type.GetMethod(name);
                methodCache[name] = StellarRoboReference.Right(new StellarRoboDynamicLibraryFunction(rawobj, mi));
            }
            return methodCache[name];
        }

        protected internal override StellarRoboReference GetIndexerReference(StellarRoboObject[] indices)
        {
            var name = indices[0].ToString();
            if (!fieldCache.ContainsKey(name))
            {
                var mi = type.GetField(name);
                fieldCache[name] = StellarRoboReference.Right(new StellarRoboDynamicLibraryField(rawobj, mi));
            }
            return fieldCache[name];
        }
    }

    /// <summary>
    /// リフレクションでどうにかしたやつのあれのこれです。
    /// </summary>
    public sealed class StellarRoboDynamicLibraryFunction : StellarRoboObject
    {
        public static readonly string ClassName = "DynamicLibraryObjectFunction";
        private MethodInfo info;
        private object instance;
        private Dictionary<string, StellarRoboReference> methodCache = new Dictionary<string, StellarRoboReference>();

        internal StellarRoboDynamicLibraryFunction(object self, MethodInfo mi)
        {
            ExtraType = ClassName;
            instance = self;
            info = mi;
        }

        protected internal override StellarRoboFunctionResult Call(StellarRoboContext context, StellarRoboObject[] args)
        {
            List<object> ia = new List<object>();
            foreach (var i in args)
            {
                switch (i.Type)
                {
                    case TypeCode.Boolean:
                        ia.Add(i.ToBoolean());
                        break;
                    case TypeCode.Double:
                        ia.Add(i.ToDouble());
                        break;
                    case TypeCode.Int64:
                        ia.Add(i.ToInt32());
                        break;
                    case TypeCode.String:
                        ia.Add(i.ToString());
                        break;
                    case TypeCode.Empty:
                        ia.Add(null);
                        break;
                    default:
                        var t = i as StellarRoboDynamicLibraryObject;
                        ia.Add(t != null ? t.rawobj : null);
                        break;
                }
            }
            var result = info.Invoke(instance, ia.ToArray());
            return new StellarRoboDynamicLibraryObject(result).NoResume();
        }
    }

    /// <summary>
    /// </summary>
    public sealed class StellarRoboDynamicLibraryField : StellarRoboObject
    {
        public static readonly string ClassName = "DynamicLibraryObjectField";
        private FieldInfo info;
        internal object instance;
        private StellarRoboReference getter, setter;

        internal StellarRoboDynamicLibraryField(object self, FieldInfo fi)
        {
            ExtraType = ClassName;
            instance = self;
            info = fi;
            getter = StellarRoboReference.Right(new StellarRoboInteropFunction(this, InstanceGet));
            setter = StellarRoboReference.Right(new StellarRoboInteropFunction(this, InstanceSet));
        }

        protected internal override StellarRoboReference GetMemberReference(string name)
        {
            switch (name)
            {
                case "get": return getter;
                case "set": return setter;
            }
            return base.GetMemberReference(name);
        }

        private StellarRoboFunctionResult InstanceGet(StellarRoboContext ctx, StellarRoboObject self, StellarRoboObject[] args) => new StellarRoboDynamicLibraryObject(info.GetValue(instance)).NoResume();

        private StellarRoboFunctionResult InstanceSet(StellarRoboContext ctx, StellarRoboObject self, StellarRoboObject[] args)
        {
            object sv = null;
            switch (args[0].Type)
            {
                case TypeCode.Boolean:
                    sv = args[0].ToBoolean();
                    break;
                case TypeCode.Double:
                    sv = args[0].ToBoolean();
                    break;
                case TypeCode.Int64:
                    sv = args[0].ToInt64();
                    break;
                case TypeCode.String:
                    sv = args[0].ToString();
                    break;
                case TypeCode.Empty:
                    sv = null;
                    break;
                default:
                    break;
            }
            info.SetValue(instance, sv);
            return StellarRoboNil.Instance.NoResume();
        }
    }
#pragma warning restore 1591
}
