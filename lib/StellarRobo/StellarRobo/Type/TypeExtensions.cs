using System;
using System.Collections.Generic;
using System.Linq;

namespace StellarRobo.Type
{
    /// <summary>
    /// StellarRoboの型システムに関するヘルパーメソッドを提供します。
    /// </summary>
    public static class TypeExtensions
    {
        #region StellarRoboFunctionResult拡張
        /// <summary>
        /// 再開可能です。
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static StellarRoboFunctionResult CanResume(this StellarRoboObject obj) => new StellarRoboFunctionResult(obj, true);

        /// <summary>
        /// 再開不可能です。
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static StellarRoboFunctionResult NoResume(this StellarRoboObject obj) => new StellarRoboFunctionResult(obj, false);
        #endregion

        #region .NETオブジェクト拡張
        /// <summary>
        /// ラムダ式などで<see cref="Predicate{T}"/>相当のオブジェクトが渡されたものとしてCallします。
        /// </summary>
        /// <param name="obj">対象</param>
        /// <param name="ctx">現在の<see cref="StellarRoboContext"/></param>
        /// <param name="tr">渡すオブジェクト</param>
        /// <returns></returns>
        public static bool CallAsPredicate(this StellarRoboObject obj, StellarRoboContext ctx, StellarRoboObject tr) => obj.Call(ctx, new[] { tr }).ReturningObject.ToBoolean();
        
        /// <summary>
        /// <see cref="StellarRoboInteger"/>を生成します。
        /// </summary>
        /// <param name="num">対象の64bit整数</param>
        /// <returns>結果</returns>
        public static StellarRoboInteger AsStellarRoboInteger(this long num) => new StellarRoboInteger { Value = num };

        /// <summary>
        /// <see cref="StellarRoboInteger"/>を生成します。
        /// </summary>
        /// <param name="num">対象の32bit整数</param>
        /// <returns>結果</returns>
        public static StellarRoboInteger AsStellarRoboInteger(this int num) => new StellarRoboInteger { Value = num };

        /// <summary>
        /// <see cref="StellarRoboFloat"/>を生成します。
        /// </summary>
        /// <param name="num">対象の倍精度浮動小数点数</param>
        /// <returns>結果</returns>
        public static StellarRoboFloat AsStellarRoboFloat(this double num) => new StellarRoboFloat { Value = num };

        /// <summary>
        /// <see cref="StellarRoboString"/>を生成します。
        /// </summary>
        /// <param name="val">対象の文字列</param>
        /// <returns>結果</returns>
        public static StellarRoboString AsStellarRoboString(this string val) => new StellarRoboString { Value = val };

        /// <summary>
        /// <see cref="StellarRoboBoolean"/>を生成します。
        /// </summary>
        /// <param name="val">対象の値</param>
        /// <returns>結果</returns>
        public static StellarRoboBoolean AsStellarRoboBoolean(this bool val) => new StellarRoboBoolean { Value = val };

        /// <summary>
        /// <see cref="StellarRoboInteropFunction"/>を生成します。
        /// </summary>
        /// <param name="val">対象の<see cref="StellarRoboInteropDelegate"/></param>
        /// <returns>結果</returns>
        public static StellarRoboInteropFunction AsStellarRoboInteropFunction(this StellarRoboInteropDelegate val) => new StellarRoboInteropFunction(null, val);

        /// <summary>
        /// nil化します。
        /// </summary>
        /// <param name="obj">値</param>
        /// <returns>nil</returns>
        public static StellarRoboNil AsNil(this StellarRoboObject obj) => StellarRoboNil.Instance;
        #endregion

        #region 引数配列関係
        /// <summary>
        /// この<see cref="StellarRoboObject"/>のリストが指定した個数の<see cref="Int32"/>に変換可能であるとみなし、
        /// そのリストを返します。
        /// </summary>
        /// <param name="arr">対象</param>
        /// <param name="length">長さ</param>
        /// <param name="allowMore">
        /// 指定した長さを超えるリストであるときに全て変換する場合はtrueを指定します。
        /// 超過分を切り捨てる場合はfalseを指定します。
        /// </param>
        /// <returns>変換結果</returns>
        public static IList<int> ExpectInt32(this IList<StellarRoboObject> arr, int length, bool allowMore)
        {

            if (arr.Count < length) throw new ArgumentException("要素の数が足りません");
            return (allowMore ? arr : arr.Take(length)).Select(p => p.ToInt32()).ToList();
        }

        /// <summary>
        /// この<see cref="StellarRoboObject"/>のリストが指定した個数の<see cref="Int64"/>に変換可能であるとみなし、
        /// そのリストを返します。
        /// </summary>
        /// <param name="arr">対象</param>
        /// <param name="length">長さ</param>
        /// <param name="allowMore">
        /// 指定した長さを超えるリストであるときに全て変換する場合はtrueを指定します。
        /// 超過分を切り捨てる場合はfalseを指定します。
        /// </param>
        /// <returns>変換結果</returns>
        public static IList<long> ExpectInt64(this IList<StellarRoboObject> arr, int length, bool allowMore)
        {

            if (arr.Count < length) throw new ArgumentException("要素の数が足りません");
            return (allowMore ? arr : arr.Take(length)).Select(p => p.ToInt64()).ToList();
        }

        /// <summary>
        /// この<see cref="StellarRoboObject"/>のリストが指定した個数の<see cref="Single"/>に変換可能であるとみなし、
        /// そのリストを返します。
        /// </summary>
        /// <param name="arr">対象</param>
        /// <param name="length">長さ</param>
        /// <param name="allowMore">
        /// 指定した長さを超えるリストであるときに全て変換する場合はtrueを指定します。
        /// 超過分を切り捨てる場合はfalseを指定します。
        /// </param>
        /// <returns>変換結果</returns>
        public static IList<float> ExpectSingle(this IList<StellarRoboObject> arr, int length, bool allowMore)
        {

            if (arr.Count < length) throw new ArgumentException("要素の数が足りません");
            return (allowMore ? arr : arr.Take(length)).Select(p => (float)p.ToDouble()).ToList();
        }

        /// <summary>
        /// この<see cref="StellarRoboObject"/>のリストが指定した個数の<see cref="Double"/>に変換可能であるとみなし、
        /// そのリストを返します。
        /// </summary>
        /// <param name="arr">対象</param>
        /// <param name="length">長さ</param>
        /// <param name="allowMore">
        /// 指定した長さを超えるリストであるときに全て変換する場合はtrueを指定します。
        /// 超過分を切り捨てる場合はfalseを指定します。
        /// </param>
        /// <returns>変換結果</returns>
        public static IList<double> ExpectDouble(this IList<StellarRoboObject> arr, int length, bool allowMore)
        {

            if (arr.Count < length) throw new ArgumentException("要素の数が足りません");
            return (allowMore ? arr : arr.Take(length)).Select(p => p.ToDouble()).ToList();
        }

        /// <summary>
        /// この<see cref="StellarRoboObject"/>のリストが指定した個数の<see cref="String"/>に変換可能であるとみなし、
        /// そのリストを返します。
        /// </summary>
        /// <param name="arr">対象</param>
        /// <param name="length">長さ</param>
        /// <param name="allowMore">
        /// 指定した長さを超えるリストであるときに全て変換する場合はtrueを指定します。
        /// 超過分を切り捨てる場合はfalseを指定します。
        /// </param>
        /// <returns>変換結果</returns>
        public static IList<string> ExpectString(this IList<StellarRoboObject> arr, int length, bool allowMore)
        {

            if (arr.Count < length) throw new ArgumentException("要素の数が足りません");
            return (allowMore ? arr : arr.Take(length)).Select(p => p.ToString()).ToList();
        }

        /// <summary>
        /// この<see cref="StellarRoboObject"/>のリストが指定した<see cref="TypeCode"/>の順に従うとみなし、
        /// そのリストを返します。
        /// </summary>
        /// <param name="arr">対象</param>
        /// <param name="codes">
        /// 変換先の<see cref="TypeCode"/>のリスト。
        /// <para>
        ///     利用できるのは
        ///     <see cref="TypeCode.Int32"/>、
        ///     <see cref="TypeCode.Int64"/>、
        ///     <see cref="TypeCode.Single"/>、
        ///     <see cref="TypeCode.Double"/>、
        ///     <see cref="TypeCode.Boolean"/>、
        ///     <see cref="TypeCode.String"/>、
        ///     <see cref="TypeCode.Object"/>です。
        ///     <see cref="TypeCode.Object"/>を指定した場合、該当する<see cref="StellarRoboObject"/>は
        ///     変換されずそのまま格納されます。
        /// </para>
        /// <para>
        ///     また、
        ///     <see cref="TypeCode.Int32"/>、
        ///     <see cref="TypeCode.Single"/>を指定した場合
        ///     精度が失われる可能性があります。
        /// </para>
        /// </param>
        /// <param name="allowMore">
        /// 指定した長さを超えるリストであるときに全て変換する場合はtrueを指定します。
        /// 超過分を切り捨てる場合はfalseを指定します。
        /// 超過分は<see cref="TypeCode.Object"/>と同じ挙動になります。
        /// </param>
        /// <returns>変換結果</returns>
        public static IList<object> ExpectTypes(this IList<StellarRoboObject> arr, IList<TypeCode> codes, bool allowMore)
        {
            if (arr.Count < codes.Count) throw new ArgumentException("要素の数が足りません");
            var result = new List<object>();
            var q = new Queue<StellarRoboObject>(arr);
            foreach (var i in codes)
            {
                var obj = q.Dequeue();
                switch (i)
                {
                    case TypeCode.Int32:
                        result.Add(obj.ToInt32());
                        break;
                    case TypeCode.Int64:
                        result.Add(obj.ToInt64());
                        break;
                    case TypeCode.Single:
                        result.Add((float)obj.ToDouble());
                        break;
                    case TypeCode.Double:
                        result.Add(obj.ToDouble());
                        break;
                    case TypeCode.Boolean:
                        result.Add(obj.ToBoolean());
                        break;
                    case TypeCode.String:
                        result.Add(obj.ToString());
                        break;
                    case TypeCode.Object:
                        result.Add(obj);
                        break;
                    default:
                        throw new ArgumentException("無効なTypeCode値です");
                }
            }
            if (allowMore) result.AddRange(q);
            return result;
        }

        /// <summary>
        /// この<see cref="StellarRoboObject"/>のリストが指定した<see cref="TypeCode"/>の順に従うとみなし、
        /// そのリストを返します。超過分は切り捨てられます。
        /// </summary>
        /// <param name="arr">対象</param>
        /// <param name="codes">
        /// 変換先の<see cref="TypeCode"/>のリスト。
        /// <para>
        ///     利用できるのは
        ///     <see cref="TypeCode.Int32"/>、
        ///     <see cref="TypeCode.Int64"/>、
        ///     <see cref="TypeCode.Single"/>、
        ///     <see cref="TypeCode.Double"/>、
        ///     <see cref="TypeCode.Boolean"/>、
        ///     <see cref="TypeCode.String"/>、
        ///     <see cref="TypeCode.Object"/>です。
        ///     <see cref="TypeCode.Object"/>を指定した場合、該当する<see cref="StellarRoboObject"/>は
        ///     変換されずそのまま格納されます。
        /// </para>
        /// <para>
        ///     また、
        ///     <see cref="TypeCode.Int32"/>、
        ///     <see cref="TypeCode.Single"/>を指定した場合
        ///     精度が失われる可能性があります。
        /// </para>
        /// </param>
        /// <returns>変換結果</returns>
        public static IList<object> ExpectTypes(this IList<StellarRoboObject> arr, params TypeCode[] codes) => ExpectTypes(arr, codes, false);
        #endregion

        #region StellarRoboObject配列化関係
        /// <summary>
        /// この<see cref="StellarRoboObject"/>が配列・リストなどの列挙オブジェクトであるとみなし、
        /// lengthと[]を用いて<see cref="string"/>のリストに変換します。
        /// </summary>
        /// <param name="obj">対象</param>
        /// <returns>結果</returns>
        public static IList<string> ToStringArray(this StellarRoboObject obj)
        {
            var len = obj["length"].ToInt64();
            var result = new List<string>();
            for (int i = 0; i < len; i++)
            {
                result.Add(obj.GetIndexerReference(new[] { i.AsStellarRoboInteger() }).RawObject.ToString());
            }
            return result;
        }

        /// <summary>
        /// この<see cref="StellarRoboObject"/>が配列・リストなどの列挙オブジェクトであるとみなし、
        /// lengthと[]を用いて<see cref="int"/>のリストに変換します。
        /// </summary>
        /// <param name="obj">対象</param>
        /// <returns>結果</returns>
        public static IList<int> ToInt32Array(this StellarRoboObject obj)
        {
            var len = obj["length"].ToInt64();
            var result = new List<int>();
            for (int i = 0; i < len; i++)
            {
                result.Add(obj.GetIndexerReference(new[] { i.AsStellarRoboInteger() }).RawObject.ToInt32());
            }
            return result;
        }

        /// <summary>
        /// この<see cref="StellarRoboObject"/>が配列・リストなどの列挙オブジェクトであるとみなし、
        /// lengthと[]を用いて文<see cref="long"/>のリストに変換します。
        /// </summary>
        /// <param name="obj">対象</param>
        /// <returns>結果</returns>
        public static IList<long> ToInt64Array(this StellarRoboObject obj)
        {
            var len = obj["length"].ToInt64();
            var result = new List<long>();
            for (int i = 0; i < len; i++)
            {
                result.Add(obj.GetIndexerReference(new[] { i.AsStellarRoboInteger() }).RawObject.ToInt64());
            }
            return result;
        }

        /// <summary>
        /// この<see cref="StellarRoboObject"/>が配列・リストなどの列挙オブジェクトであるとみなし、
        /// lengthと[]を用いて文<see cref="double"/>のリストに変換します。
        /// </summary>
        /// <param name="obj">対象</param>
        /// <returns>結果</returns>
        public static IList<double> ToDoubleArray(this StellarRoboObject obj)
        {
            var len = obj["length"].ToInt64();
            var result = new List<double>();
            for (int i = 0; i < len; i++)
            {
                result.Add(obj.GetIndexerReference(new[] { i.AsStellarRoboInteger() }).RawObject.ToDouble());
            }
            return result;
        }

        /// <summary>
        /// この<see cref="StellarRoboObject"/>が配列・リストなどの列挙オブジェクトであるとみなし、
        /// lengthと[]を用いて文<see cref="bool"/>のリストに変換します。
        /// </summary>
        /// <param name="obj">対象</param>
        /// <returns>結果</returns>
        public static IList<bool> ToBooleanArray(this StellarRoboObject obj)
        {
            var len = obj["length"].ToInt64();
            var result = new List<bool>();
            for (int i = 0; i < len; i++)
            {
                result.Add(obj.GetIndexerReference(new[] { i.AsStellarRoboInteger() }).RawObject.ToBoolean());
            }
            return result;
        }

        /// <summary>
        /// この<see cref="StellarRoboObject"/>が配列・リストなどの列挙オブジェクトであるとみなし、
        /// lengthと[]を用いて文<see cref="StellarRoboObject"/>のリストに変換します。
        /// </summary>
        /// <param name="obj">対象</param>
        /// <returns>結果</returns>
        public static IList<StellarRoboObject> AsArray(this StellarRoboObject obj)
        {
            var len = obj["length"].ToInt64();
            var result = new List<StellarRoboObject>();
            for (int i = 0; i < len; i++)
            {
                result.Add(obj.GetIndexerReference(new[] { i.AsStellarRoboInteger() }).RawObject);
            }
            return result;
        }

        #endregion

        #region StellarRoboArray拡張
        /// <summary>
        /// このリストを<see cref="StellarRoboArray"/>に変換します。
        /// </summary>
        /// <param name="ol">対象</param>
        /// <returns></returns>
        public static StellarRoboArray ToStellarRoboArray(this IList<StellarRoboObject> ol) => new StellarRoboArray(ol);

        /// <summary>
        /// このリストを<see cref="StellarRoboArray"/>に変換します。
        /// </summary>
        /// <param name="ol">対象</param>
        /// <returns></returns>
        public static StellarRoboArray ToStellarRoboArray(this IList<int> ol) => new StellarRoboArray(ol.Select(p => p.AsStellarRoboInteger()));

        /// <summary>
        /// このリストを<see cref="StellarRoboArray"/>に変換します。
        /// </summary>
        /// <param name="ol">対象</param>
        /// <returns></returns>
        public static StellarRoboArray ToStellarRoboArray(this IList<long> ol) => new StellarRoboArray(ol.Select(p => p.AsStellarRoboInteger()));

        /// <summary>
        /// このリストを<see cref="StellarRoboArray"/>に変換します。
        /// </summary>
        /// <param name="ol">対象</param>
        /// <returns></returns>
        public static StellarRoboArray ToStellarRoboArray(this IList<double> ol) => new StellarRoboArray(ol.Select(p => p.AsStellarRoboFloat()));

        /// <summary>
        /// このリストを<see cref="StellarRoboArray"/>に変換します。
        /// </summary>
        /// <param name="ol">対象</param>
        /// <returns></returns>
        public static StellarRoboArray ToStellarRoboArray(this IList<bool> ol) => new StellarRoboArray(ol.Select(p => p.AsStellarRoboBoolean()));

        /// <summary>
        /// このリストを<see cref="StellarRoboArray"/>に変換します。
        /// </summary>
        /// <param name="ol">対象</param>
        /// <returns></returns>
        public static StellarRoboArray ToStellarRoboArray(this IList<string> ol) => new StellarRoboArray(ol.Select(p => p.AsStellarRoboString()));
        /*
        /// <summary>
        /// このリストを<see cref="StellarRoboList"/>に変換します。
        /// </summary>
        /// <param name="ol">対象</param>
        /// <returns></returns>
        public static StellarRoboList ToStellarRoboList(this IList<StellarRoboObject> ol) => new StellarRoboList(ol);

        /// <summary>
        /// このリストを<see cref="StellarRoboList"/>に変換します。
        /// </summary>
        /// <param name="ol">対象</param>
        /// <returns></returns>
        public static StellarRoboList ToStellarRoboList(this IList<int> ol) => new StellarRoboList(ol.Select(p => p.AsStellarRoboInteger()));

        /// <summary>
        /// このリストを<see cref="StellarRoboList"/>に変換します。
        /// </summary>
        /// <param name="ol">対象</param>
        /// <returns></returns>
        public static StellarRoboList ToStellarRoboList(this IList<long> ol) => new StellarRoboList(ol.Select(p => p.AsStellarRoboInteger()));

        /// <summary>
        /// このリストを<see cref="StellarRoboList"/>に変換します。
        /// </summary>
        /// <param name="ol">対象</param>
        /// <returns></returns>
        public static StellarRoboList ToStellarRoboList(this IList<double> ol) => new StellarRoboList(ol.Select(p => p.AsStellarRoboFloat()));

        /// <summary>
        /// このリストを<see cref="StellarRoboList"/>に変換します。
        /// </summary>
        /// <param name="ol">対象</param>
        /// <returns></returns>
        public static StellarRoboList ToStellarRoboList(this IList<bool> ol) => new StellarRoboList(ol.Select(p => p.AsStellarRoboBoolean()));

        /// <summary>
        /// このリストを<see cref="StellarRoboList"/>に変換します。
        /// </summary>
        /// <param name="ol">対象</param>
        /// <returns></returns>
        public static StellarRoboList ToStellarRoboList(this IList<string> ol) => new StellarRoboList(ol.Select(p => p.AsStellarRoboString()));
        */
        #endregion

        #region IEnumerable<T>拡張系
        /// <summary>
        /// 列挙可能なリストに対してmapメソッド(Select相当)を生成します。
        /// </summary>
        /// <param name="list">対象のリスト</param>
        /// <returns>生成されたDelegate。</returns>
        public static StellarRoboInteropDelegate GenerateMapFunction(this IEnumerable<StellarRoboObject> list)
        {
            StellarRoboInteropDelegate retfunc = (ctx, self, args) =>
            {
                var lambda = args[0];
                var res = list.Select(p => lambda.Call(ctx, new[] { p }).ReturningObject);
                return new StellarRoboArray(res).NoResume();
            };
            return retfunc;
        }

        /// <summary>
        /// 列挙可能なリストに対してfilterメソッド(Where相当)を生成します。
        /// </summary>
        /// <param name="list">対象のリスト</param>
        /// <returns>生成されたDelegate。</returns>
        public static StellarRoboInteropDelegate GenerateFilterFunction(this IEnumerable<StellarRoboObject> list)
        {
            StellarRoboInteropDelegate retfunc = (ctx, self, args) =>
            {
                var lambda = args[0];
                var res = list.Where(p => lambda.Call(ctx, new[] { p }).ReturningObject.ToBoolean());
                return new StellarRoboArray(res).NoResume();
            };
            return retfunc;
        }

        /// <summary>
        /// 列挙可能なリストに対してeachメソッドを生成します。
        /// </summary>
        /// <param name="list">対象のリスト</param>
        /// <returns>生成されたDelegate。</returns>
        public static StellarRoboInteropDelegate GenerateEachFunction(this IEnumerable<StellarRoboObject> list)
        {
            StellarRoboInteropDelegate retfunc = (ctx, self, args) =>
            {
                var lambda = args[0];
                foreach (var i in list)
                    lambda.Call(ctx, new[] { i });
                return StellarRoboNil.Instance.NoResume();
            };
            return retfunc;
        }

        /// <summary>
        /// 列挙可能なリストに対してreduceメソッド(Aggregate相当)を生成します。
        /// </summary>
        /// <param name="list">対象のリスト</param>
        /// <returns>生成されたDelegate。</returns>
        public static StellarRoboInteropDelegate GenerateReduceFunction(this IEnumerable<StellarRoboObject> list)
        {
            StellarRoboInteropDelegate retfunc = (ctx, self, args) =>
            {
                var lambda = args[0];
                var res = list.Aggregate((p, q) => lambda.Call(ctx, new[] { p, q }).ReturningObject);
                return res.NoResume();
            };
            return retfunc;
        }

        /// <summary>
        /// 列挙可能なリストに対してskipメソッド(Skip相当)を生成します。
        /// </summary>
        /// <param name="list">対象のリスト</param>
        /// <returns>生成されたDelegate。</returns>
        public static StellarRoboInteropDelegate GenerateSkipFunction(this IEnumerable<StellarRoboObject> list)
        {
            StellarRoboInteropDelegate retfunc = (ctx, self, args) =>
            {
                var res = list.Skip(args[0].ToInt32());
                return new StellarRoboArray(res).NoResume();
            };
            return retfunc;
        }

        /// <summary>
        /// 列挙可能なリストに対してtakeメソッド(Skip相当)を生成します。
        /// </summary>
        /// <param name="list">対象のリスト</param>
        /// <returns>生成されたDelegate。</returns>
        public static StellarRoboInteropDelegate GenerateTakeFunction(this IEnumerable<StellarRoboObject> list)
        {
            StellarRoboInteropDelegate retfunc = (ctx, self, args) =>
            {
                var res = list.Take(args[0].ToInt32());
                return new StellarRoboArray(res).NoResume();
            };
            return retfunc;
        }

        /// <summary>
        /// 列挙可能なリストに対してreverseメソッド(Reverse相当)を生成します。
        /// </summary>
        /// <param name="list">対象のリスト</param>
        /// <returns>生成されたDelegate。</returns>
        public static StellarRoboInteropDelegate GenerateReverseFunction(this IEnumerable<StellarRoboObject> list)
        {
            StellarRoboInteropDelegate retfunc = (ctx, self, args) =>
            {
                var res = list.Reverse();
                return new StellarRoboArray(res).NoResume();
            };
            return retfunc;
        }

        /// <summary>
        /// 列挙可能なリストに対してfirstメソッド(First相当)を生成します。
        /// </summary>
        /// <param name="list">対象のリスト</param>
        /// <returns>生成されたDelegate。</returns>
        public static StellarRoboInteropDelegate GenerateFirstFunction(this IEnumerable<StellarRoboObject> list)
        {
            StellarRoboInteropDelegate retfunc = (ctx, self, args) =>
            {
                if (args.Length == 0) return list.First().NoResume();
                var lambda = args[0];
                var res = list.First(p => args[0].Call(ctx, new[] { p }).ReturningObject.ToBoolean());
                return res.NoResume();
            };
            return retfunc;
        }

        /// <summary>
        /// 列挙可能なリストに対してlastメソッド(Last相当)を生成します。
        /// </summary>
        /// <param name="list">対象のリスト</param>
        /// <returns>生成されたDelegate。</returns>
        public static StellarRoboInteropDelegate GenerateLastFunction(this IEnumerable<StellarRoboObject> list)
        {
            StellarRoboInteropDelegate retfunc = (ctx, self, args) =>
            {
                if (args.Length == 0) return list.Last().NoResume();
                var lambda = args[0];
                var res = list.Last(p => args[0].Call(ctx, new[] { p }).ReturningObject.ToBoolean());
                return res.NoResume();
            };
            return retfunc;
        }

        /// <summary>
        /// 列挙可能なリストに対してanyメソッド(any相当)を生成します。
        /// </summary>
        /// <param name="list">対象のリスト</param>
        /// <returns>生成されたDelegate。</returns>
        public static StellarRoboInteropDelegate GenerateAnyFunction(this IEnumerable<StellarRoboObject> list)
        {
            StellarRoboInteropDelegate retfunc = (ctx, self, args) =>
            {
                var lambda = args[0];
                var res = list.Any(p => args[0].Call(ctx, new[] { p }).ReturningObject.ToBoolean());
                return res.AsStellarRoboBoolean().NoResume();
            };
            return retfunc;
        }

        /// <summary>
        /// 列挙可能なリストに対してallメソッド(all相当)を生成します。
        /// </summary>
        /// <param name="list">対象のリスト</param>
        /// <returns>生成されたDelegate。</returns>
        public static StellarRoboInteropDelegate GenerateAllFunction(this IEnumerable<StellarRoboObject> list)
        {
            StellarRoboInteropDelegate retfunc = (ctx, self, args) =>
            {
                var lambda = args[0];
                var res = list.All(p => args[0].Call(ctx, new[] { p }).ReturningObject.ToBoolean());
                return res.AsStellarRoboBoolean().NoResume();
            };
            return retfunc;
        }
        #endregion
    }
}
