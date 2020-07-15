using System.IO;
using System.Text;
using System.Linq;

namespace StellarRobo
{
    /// <summary>
    /// StellarRoboのバイトコード関連の機能を提供します。
    /// </summary>
    public static class StellarRoboBytecode
    {
        private static readonly Encoding stringEncoding = new UTF8Encoding(false, true);
        private static readonly byte[] magicNumber = { (byte)'K', (byte)'C' };
        private const ushort BytecodeVersion = 2;

        private enum TopLevelBlockType : byte
        {
            Class = 1,
            TopLevelMethod = 2,
            Use = 3,
        }

        private enum ClassBlockType : byte
        {
            InnerClass = 10,
            InstanceMethod = 11,
            ClassMethod = 12,
            Local = 13
        }

        private enum ClassElementType : byte
        {
            StartBlocks = 0,
            Name = 1
        }

        private enum MethodElementType : byte
        {
            StartCode = 0,
            Name = 1,
            ArgumentLength = 2,
            VariableArgument = 3
        }

        /// <summary>
        /// <see cref="StellarRoboSource"/>から指定の<see cref="Stream"/>にバイトコードを出力します。
        /// </summary>
        /// <param name="source">対象の<see cref="StellarRoboSource"/></param>
        /// <param name="output">出力先</param>
        public static void Save(StellarRoboSource source, Stream output)
        {
            using (var writer = new BinaryWriter(output, stringEncoding, true))
            {
                writer.Write(magicNumber);
                writer.Write(BytecodeVersion);

                writer.Write(source.Uses.Count + source.Classes.Count + source.TopLevelMethods.Count);
                foreach (var x in source.Uses)
                {
                    writer.Write((byte)TopLevelBlockType.Use);
                    writer.Write(x);
                }

                foreach (var x in source.Classes)
                {
                    writer.Write((byte)TopLevelBlockType.Class);
                    WriteClass(x, writer);
                }

                foreach (var x in source.TopLevelMethods)
                {
                    writer.Write((byte)TopLevelBlockType.TopLevelMethod);
                    WriteMethod(x, writer);
                }
            }
        }
        private static void WriteClass(StellarRoboScriptClassInfo klass, BinaryWriter writer)
        {
            writer.Write((byte)ClassElementType.Name);
            writer.Write(klass.Name);

            writer.Write((byte)ClassElementType.StartBlocks);
            writer.Write(klass.inners.Count + klass.methods.Count + klass.classMethods.Count + klass.Locals.Count);

            foreach (var x in klass.inners)
            {
                writer.Write((byte)ClassBlockType.InnerClass);
                WriteClass(x, writer);
            }

            foreach (var x in klass.methods)
            {
                writer.Write((byte)ClassBlockType.InstanceMethod);
                WriteMethod(x, writer);
            }

            foreach (var x in klass.classMethods)
            {
                writer.Write((byte)ClassBlockType.ClassMethod);
                WriteMethod(x, writer);
            }

            foreach (var x in klass.Locals)
            {
                writer.Write((byte)ClassBlockType.Local);
                writer.Write(x);
            }
        }
        private static void WriteMethod(StellarRoboScriptMethodInfo method, BinaryWriter writer)
        {
            writer.Write((byte)MethodElementType.Name);
            writer.Write(method.Name);

            writer.Write((byte)MethodElementType.ArgumentLength);
            writer.Write(method.ArgumentLength);

            if (method.VariableArgument)
                writer.Write((byte)MethodElementType.VariableArgument);

            writer.Write((byte)MethodElementType.StartCode);
            var codes = method.Codes.Codes;
            writer.Write(codes.Count);

            foreach (var x in codes)
            {
                switch (x.Type)
                {
                    case StellarRoboILCodeType.Nop:
                        writer.Write((byte)0);
                        break;
                    case StellarRoboILCodeType.Label:
                        goto case StellarRoboILCodeType.Nop;
                    case StellarRoboILCodeType.PushInteger:
                        writer.Write((byte)1);
                        writer.Write(x.IntegerValue);
                        break;
                    case StellarRoboILCodeType.PushString:
                        writer.Write((byte)2);
                        writer.Write(x.StringValue);
                        break;
                    case StellarRoboILCodeType.PushSingle:
                        writer.Write((byte)3);
                        writer.Write((float)x.FloatValue);
                        break;
                    case StellarRoboILCodeType.PushDouble:
                        writer.Write((byte)4);
                        writer.Write(x.FloatValue);
                        break;
                    case StellarRoboILCodeType.PushBoolean:
                        writer.Write(x.BooleanValue ? (byte)6 : (byte)5);
                        break;
                    case StellarRoboILCodeType.PushNil:
                        writer.Write((byte)7);
                        break;
                    case StellarRoboILCodeType.Pop:
                        writer.Write((byte)8);
                        break;
                    case StellarRoboILCodeType.Plus:
                        writer.Write((byte)9);
                        break;
                    case StellarRoboILCodeType.Minus:
                        writer.Write((byte)10);
                        break;
                    case StellarRoboILCodeType.Multiply:
                        writer.Write((byte)11);
                        break;
                    case StellarRoboILCodeType.Divide:
                        writer.Write((byte)12);
                        break;
                    case StellarRoboILCodeType.Modular:
                        writer.Write((byte)13);
                        break;
                    case StellarRoboILCodeType.And:
                        writer.Write((byte)14);
                        break;
                    case StellarRoboILCodeType.Or:
                        writer.Write((byte)15);
                        break;
                    case StellarRoboILCodeType.Xor:
                        writer.Write((byte)16);
                        break;
                    case StellarRoboILCodeType.Not:
                        writer.Write((byte)17);
                        break;
                    case StellarRoboILCodeType.Negative:
                        writer.Write((byte)18);
                        break;
                    case StellarRoboILCodeType.AndAlso:
                        writer.Write((byte)19);
                        break;
                    case StellarRoboILCodeType.OrElse:
                        writer.Write((byte)20);
                        break;
                    case StellarRoboILCodeType.LeftBitShift:
                        writer.Write((byte)21);
                        break;
                    case StellarRoboILCodeType.RightBitShift:
                        writer.Write((byte)22);
                        break;
                    case StellarRoboILCodeType.Equal:
                        writer.Write((byte)23);
                        break;
                    case StellarRoboILCodeType.NotEqual:
                        writer.Write((byte)24);
                        break;
                    case StellarRoboILCodeType.Greater:
                        writer.Write((byte)25);
                        break;
                    case StellarRoboILCodeType.Lesser:
                        writer.Write((byte)26);
                        break;
                    case StellarRoboILCodeType.GreaterEqual:
                        writer.Write((byte)27);
                        break;
                    case StellarRoboILCodeType.LesserEqual:
                        writer.Write((byte)28);
                        break;
                    case StellarRoboILCodeType.Assign:
                        writer.Write((byte)29);
                        break;
                    case StellarRoboILCodeType.Jump:
                        writer.Write((byte)30);
                        writer.Write((int)x.IntegerValue);
                        break;
                    case StellarRoboILCodeType.TrueJump:
                        writer.Write((byte)31);
                        writer.Write((int)x.IntegerValue);
                        break;
                    case StellarRoboILCodeType.FalseJump:
                        writer.Write((byte)32);
                        writer.Write((int)x.IntegerValue);
                        break;
                    case StellarRoboILCodeType.Return:
                        writer.Write((byte)33);
                        break;
                    case StellarRoboILCodeType.Yield:
                        writer.Write((byte)34);
                        break;
                    case StellarRoboILCodeType.Call:
                        writer.Write((byte)35);
                        writer.Write((int)x.IntegerValue);
                        break;
                    case StellarRoboILCodeType.IndexerCall:
                        writer.Write((byte)36);
                        writer.Write((int)x.IntegerValue);
                        break;
                    case StellarRoboILCodeType.PushArgument:
                        writer.Write((byte)37);
                        writer.Write((int)x.IntegerValue);
                        break;
                    case StellarRoboILCodeType.LoadObject:
                        writer.Write((byte)38);
                        writer.Write(x.StringValue);
                        break;
                    case StellarRoboILCodeType.LoadMember:
                        writer.Write((byte)39);
                        writer.Write(x.StringValue);
                        break;
                    case StellarRoboILCodeType.AsValue:
                        writer.Write((byte)40);
                        break;
                    case StellarRoboILCodeType.LoadVarg:
                        writer.Write((byte)41);
                        writer.Write((int)x.IntegerValue);
                        break;
                    case StellarRoboILCodeType.StartCoroutine:
                        writer.Write((byte)42);
                        writer.Write(x.StringValue);
                        writer.Write((int)x.IntegerValue);
                        break;
                    case StellarRoboILCodeType.ResumeCoroutine:
                        writer.Write(x.BooleanValue ? (byte)44 : (byte)43);
                        writer.Write(x.StringValue);
                        break;
                    case StellarRoboILCodeType.MakeArray:
                        writer.Write((byte)45);
                        writer.Write((int)x.IntegerValue);
                        break;
                    case StellarRoboILCodeType.PlusAssign:
                        writer.Write((byte)46);
                        break;
                    case StellarRoboILCodeType.MinusAssign:
                        writer.Write((byte)47);
                        break;
                    case StellarRoboILCodeType.MultiplyAssign:
                        writer.Write((byte)48);
                        break;
                    case StellarRoboILCodeType.DivideAssign:
                        writer.Write((byte)49);
                        break;
                    case StellarRoboILCodeType.AndAssign:
                        writer.Write((byte)50);
                        break;
                    case StellarRoboILCodeType.OrAssign:
                        writer.Write((byte)51);
                        break;
                    case StellarRoboILCodeType.XorAssign:
                        writer.Write((byte)52);
                        break;
                    case StellarRoboILCodeType.ModularAssign:
                        writer.Write((byte)53);
                        break;
                    case StellarRoboILCodeType.LeftBitShiftAssign:
                        writer.Write((byte)54);
                        break;
                    case StellarRoboILCodeType.RightBitShiftAssign:
                        writer.Write((byte)55);
                        break;
                    case StellarRoboILCodeType.NilAssign:
                        writer.Write((byte)56);
                        break;
                    default:
                        throw new InvalidDataException("In StellarRobo, please");
                }
            }
        }

        /// <summary>
        /// バイトコードを読み込み、<see cref="StellarRoboSource"/>に変換します。
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static StellarRoboSource Load(Stream input)
        {
            using (var reader = new BinaryReader(input, stringEncoding, true))
            {
                if (!reader.ReadBytes(magicNumber.Length).SequenceEqual(magicNumber))
                    throw new InvalidDataException("合言葉は「KC」");

                var version = reader.ReadUInt16();
                if (version > BytecodeVersion)
                    throw new InvalidDataException($"StellarRobo Bytecode v{version} には対応していません。");

                var result = new StellarRoboSource();
                var count = reader.ReadInt32();

                for (var i = 0; i < count; i++)
                {
                    switch ((TopLevelBlockType)reader.ReadByte())
                    {
                        case TopLevelBlockType.Use:
                            result.uses.Add(reader.ReadString());
                            break;
                        case TopLevelBlockType.Class:
                            result.classes.Add(ReadClass(reader));
                            break;
                        case TopLevelBlockType.TopLevelMethod:
                            result.methods.Add(ReadMethod(reader));
                            break;
                        default:
                            throw new InvalidDataException("変なデータが出ーた！ｗｗｗｗ");
                    }
                }

                return result;
            }
        }

        private static StellarRoboScriptClassInfo ReadClass(BinaryReader reader)
        {
            string name = null;
            while (true)
            {
                switch ((ClassElementType)reader.ReadByte())
                {
                    case ClassElementType.Name:
                        name = reader.ReadString();
                        break;
                    case ClassElementType.StartBlocks:
                        var klass = new StellarRoboScriptClassInfo(name);
                        var count = reader.ReadInt32();
                        for (var i = 0; i < count; i++)
                        {
                            switch ((ClassBlockType)reader.ReadByte())
                            {
                                case ClassBlockType.InnerClass:
                                    klass.AddInnerClass(ReadClass(reader));
                                    break;
                                case ClassBlockType.InstanceMethod:
                                    klass.AddInstanceMethod(ReadMethod(reader));
                                    break;
                                case ClassBlockType.ClassMethod:
                                    klass.AddInstanceMethod(ReadMethod(reader));
                                    break;
                                case ClassBlockType.Local:
                                    klass.AddLocal(reader.ReadString(), null);
                                    break;
                                default:
                                    throw new InvalidDataException("やめて");
                            }
                        }
                        return klass;
                    default:
                        throw new InvalidDataException("無効なクラス");
                }
            }
        }

        private static StellarRoboScriptMethodInfo ReadMethod(BinaryReader reader)
        {
            string name = null;
            var length = 0;
            var vargs = false;

            while (true)
            {
                switch ((MethodElementType)reader.ReadByte())
                {
                    case MethodElementType.Name:
                        name = reader.ReadString();
                        break;
                    case MethodElementType.ArgumentLength:
                        length = reader.ReadInt32();
                        break;
                    case MethodElementType.VariableArgument:
                        vargs = true;
                        break;
                    case MethodElementType.StartCode:
                        var method = new StellarRoboScriptMethodInfo(name, length, vargs);
                        var il = new StellarRoboIL();
                        method.Codes = il;
                        var count = reader.ReadInt32();
                        for (var i = 0; i < count; i++)
                        {
                            switch (reader.ReadByte())
                            {
                                case 0:
                                    il.PushCode(StellarRoboILCodeType.Nop);
                                    break;
                                case 1:
                                    il.PushCode(StellarRoboILCodeType.PushInteger, reader.ReadInt64());
                                    break;
                                case 2:
                                    il.PushCode(StellarRoboILCodeType.PushString, reader.ReadString());
                                    break;
                                case 3:
                                    il.PushCode(StellarRoboILCodeType.PushSingle, reader.ReadSingle());
                                    break;
                                case 4:
                                    il.PushCode(StellarRoboILCodeType.PushDouble, reader.ReadDouble());
                                    break;
                                case 5:
                                    il.PushCode(StellarRoboILCodeType.PushBoolean, false);
                                    break;
                                case 6:
                                    il.PushCode(StellarRoboILCodeType.PushBoolean, true);
                                    break;
                                case 7:
                                    il.PushCode(StellarRoboILCodeType.PushNil);
                                    break;
                                case 8:
                                    il.PushCode(StellarRoboILCodeType.Pop);
                                    break;
                                case 9:
                                    il.PushCode(StellarRoboILCodeType.Plus);
                                    break;
                                case 10:
                                    il.PushCode(StellarRoboILCodeType.Minus);
                                    break;
                                case 11:
                                    il.PushCode(StellarRoboILCodeType.Multiply);
                                    break;
                                case 12:
                                    il.PushCode(StellarRoboILCodeType.Divide);
                                    break;
                                case 13:
                                    il.PushCode(StellarRoboILCodeType.Modular);
                                    break;
                                case 14:
                                    il.PushCode(StellarRoboILCodeType.And);
                                    break;
                                case 15:
                                    il.PushCode(StellarRoboILCodeType.Or);
                                    break;
                                case 16:
                                    il.PushCode(StellarRoboILCodeType.Xor);
                                    break;
                                case 17:
                                    il.PushCode(StellarRoboILCodeType.Not);
                                    break;
                                case 18:
                                    il.PushCode(StellarRoboILCodeType.Negative);
                                    break;
                                case 19:
                                    il.PushCode(StellarRoboILCodeType.AndAlso);
                                    break;
                                case 20:
                                    il.PushCode(StellarRoboILCodeType.OrElse);
                                    break;
                                case 21:
                                    il.PushCode(StellarRoboILCodeType.LeftBitShift);
                                    break;
                                case 22:
                                    il.PushCode(StellarRoboILCodeType.RightBitShift);
                                    break;
                                case 23:
                                    il.PushCode(StellarRoboILCodeType.Equal);
                                    break;
                                case 24:
                                    il.PushCode(StellarRoboILCodeType.NotEqual);
                                    break;
                                case 25:
                                    il.PushCode(StellarRoboILCodeType.Greater);
                                    break;
                                case 26:
                                    il.PushCode(StellarRoboILCodeType.Lesser);
                                    break;
                                case 27:
                                    il.PushCode(StellarRoboILCodeType.GreaterEqual);
                                    break;
                                case 28:
                                    il.PushCode(StellarRoboILCodeType.LesserEqual);
                                    break;
                                case 29:
                                    il.PushCode(StellarRoboILCodeType.Assign);
                                    break;
                                case 30:
                                    il.PushCode(StellarRoboILCodeType.Jump, reader.ReadInt32());
                                    break;
                                case 31:
                                    il.PushCode(StellarRoboILCodeType.TrueJump, reader.ReadInt32());
                                    break;
                                case 32:
                                    il.PushCode(StellarRoboILCodeType.FalseJump, reader.ReadInt32());
                                    break;
                                case 33:
                                    il.PushCode(StellarRoboILCodeType.Return);
                                    break;
                                case 34:
                                    il.PushCode(StellarRoboILCodeType.Yield);
                                    break;
                                case 35:
                                    il.PushCode(StellarRoboILCodeType.Call, reader.ReadInt32());
                                    break;
                                case 36:
                                    il.PushCode(StellarRoboILCodeType.IndexerCall, reader.ReadInt32());
                                    break;
                                case 37:
                                    il.PushCode(StellarRoboILCodeType.PushArgument, reader.ReadInt32());
                                    break;
                                case 38:
                                    il.PushCode(StellarRoboILCodeType.LoadObject, reader.ReadString());
                                    break;
                                case 39:
                                    il.PushCode(StellarRoboILCodeType.LoadMember, reader.ReadString());
                                    break;
                                case 40:
                                    il.PushCode(StellarRoboILCodeType.AsValue);
                                    break;
                                case 41:
                                    il.PushCode(StellarRoboILCodeType.LoadVarg, reader.ReadInt32());
                                    break;
                                case 42:
                                    var code = new StellarRoboILCode() { Type = StellarRoboILCodeType.StartCoroutine };
                                    code.StringValue = reader.ReadString();
                                    code.IntegerValue = reader.ReadInt32();
                                    il.PushCode(code);
                                    break;
                                case 43:
                                    il.PushCode(new StellarRoboILCode()
                                    {
                                        Type = StellarRoboILCodeType.ResumeCoroutine,
                                        StringValue = reader.ReadString(),
                                        BooleanValue = false
                                    });
                                    break;
                                case 44:
                                    il.PushCode(new StellarRoboILCode()
                                    {
                                        Type = StellarRoboILCodeType.ResumeCoroutine,
                                        StringValue = reader.ReadString(),
                                        BooleanValue = true
                                    });
                                    break;
                                case 45:
                                    il.PushCode(StellarRoboILCodeType.MakeArray, reader.ReadInt32());
                                    break;
                                case 46:
                                    il.PushCode(StellarRoboILCodeType.PlusAssign);
                                    break;
                                case 47:
                                    il.PushCode(StellarRoboILCodeType.MinusAssign);
                                    break;
                                case 48:
                                    il.PushCode(StellarRoboILCodeType.MultiplyAssign);
                                    break;
                                case 49:
                                    il.PushCode(StellarRoboILCodeType.DivideAssign);
                                    break;
                                case 50:
                                    il.PushCode(StellarRoboILCodeType.AndAssign);
                                    break;
                                case 51:
                                    il.PushCode(StellarRoboILCodeType.OrAssign);
                                    break;
                                case 52:
                                    il.PushCode(StellarRoboILCodeType.XorAssign);
                                    break;
                                case 53:
                                    il.PushCode(StellarRoboILCodeType.ModularAssign);
                                    break;
                                case 54:
                                    il.PushCode(StellarRoboILCodeType.LeftBitShiftAssign);
                                    break;
                                case 55:
                                    il.PushCode(StellarRoboILCodeType.RightBitShiftAssign);
                                    break;
                                case 56:
                                    il.PushCode(StellarRoboILCodeType.NilAssign);
                                    break;
                                default:
                                    throw new InvalidDataException("危険オペコードにはダマされない！！近づかない！！");
                            }
                        }

                        return method;
                    default:
                        throw new InvalidDataException("無効なメソッド");
                }
            }
        }
    }
}
