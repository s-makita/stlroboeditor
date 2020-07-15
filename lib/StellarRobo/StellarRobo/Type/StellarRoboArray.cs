using System;
using System.Collections.Generic;
using System.Linq;

namespace StellarRobo.Type
{
    /// <summary>
    /// 配列を提供します。
    /// </summary>
    public sealed class StellarRoboArray : StellarRoboObject
    {
        internal List<StellarRoboReference> array;

        private StellarRoboReference length;
        private StellarRoboReference find, each, filter, map, reduce, copy;//, first, last;

        internal StellarRoboArray(int[] dim)
        {
            Type = TypeCode.Object;
            ExtraType = "Array";
            array = new List<StellarRoboReference>();
            length = StellarRoboReference.Right(dim[0]);
            InitializeMembers();
            for (int i = 0; i < dim[0]; i++) array.Add(StellarRoboReference.Left(StellarRoboNil.Instance));
            if (dim.Length == 1) return;
            for (int i = 0; i < array.Count; i++) array[i] = StellarRoboReference.Left(new StellarRoboArray(dim.Skip(1).ToArray()));
        }

        internal StellarRoboArray(List<StellarRoboReference> arr)
        {
            Type = TypeCode.Object;
            array = arr;
            length = StellarRoboReference.Right(arr.Count);
            InitializeMembers();
        }

        /// <summary>
        /// 配列の新しいインスタンスを生成します。
        /// </summary>
        /// <param name="arr">生成する対象の<see cref="StellarRoboObject"/>のリスト</param>
        public StellarRoboArray(IEnumerable<StellarRoboObject> arr)
        {
            Type = TypeCode.Object;
            array = new List<StellarRoboReference>();
            foreach (var i in arr) array.Add(StellarRoboReference.Left(i));
            length = StellarRoboReference.Right(arr.Count());
            InitializeMembers();
        }

        private void InitializeMembers()
        {
            each = StellarRoboReference.Right(this, InstanceEach);
            find = StellarRoboReference.Right(this, InstanceFind);
            filter = StellarRoboReference.Right(this, InstanceFilter);
            map = StellarRoboReference.Right(this, InstanceMap);
            reduce = StellarRoboReference.Right(this, InstanceReduce);
            copy = StellarRoboReference.Right(this, InstanceCopy);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        protected internal override StellarRoboReference GetMemberReference(string name)
        {
            switch (name)
            {
                case nameof(length):
                    return length;
                case nameof(each):
                    return each;
                case nameof(find):
                    return find;
                case nameof(filter):
                    return filter;
                case nameof(map):
                    return map;
                case nameof(reduce):
                    return reduce;
                case nameof(copy):
                    return copy;
                /*
            case nameof(first):
                return first;
            case nameof(last):
                return last;
                */
                default:
                    return base.GetMemberReference(name);
            }

        }

        /// <summary>
        /// 配列要素の参照を取得します。
        /// </summary>
        /// <param name="indices"></param>
        /// <returns></returns>
        protected internal override StellarRoboReference GetIndexerReference(StellarRoboObject[] indices)
        {
            if (indices.Length != 1) throw new ArgumentException("配列のインデックスの数は必ず1です。");
            return array[(int)indices[0].ToInt64()];
        }

        private StellarRoboFunctionResult InstanceCopy(StellarRoboContext ctx, StellarRoboObject self, StellarRoboObject[] args)
        {
            var result = new List<StellarRoboObject>();
            foreach (var i in array) result.Add(i.RawObject.AsByValValue());
            return new StellarRoboArray(result).NoResume();
        }

        private StellarRoboFunctionResult InstanceEach(StellarRoboContext ctx, StellarRoboObject self, StellarRoboObject[] args)
        {
            foreach (var i in array) args[0].Call(ctx, new[] { i.RawObject });
            return StellarRoboNil.Instance.NoResume();
        }

        private StellarRoboFunctionResult InstanceFind(StellarRoboContext ctx, StellarRoboObject self, StellarRoboObject[] args)
        {
            var result = array.FindIndex(p => p.RawObject.ExpressionOperation(StellarRoboILCodeType.Equal, args[0]).ToBoolean());
            return result.AsStellarRoboInteger().NoResume();
        }

        private StellarRoboFunctionResult InstanceFilter(StellarRoboContext ctx, StellarRoboObject self, StellarRoboObject[] args)
        {
            var result = array.Where(p => args[0].Call(ctx, new[] { p.RawObject }).ReturningObject.ToBoolean());
            return new StellarRoboArray(result.ToList()).NoResume();
        }

        private StellarRoboFunctionResult InstanceMap(StellarRoboContext ctx, StellarRoboObject self, StellarRoboObject[] args)
        {
            var result = array.Select(p => args[0].Call(ctx, new[] { p.RawObject }).ReturningObject);
            return new StellarRoboArray(result.ToList()).NoResume();
        }

        private StellarRoboFunctionResult InstanceReduce(StellarRoboContext ctx, StellarRoboObject self, StellarRoboObject[] args)
        {
            var result = array.Aggregate((p, q) => StellarRoboReference.Right(args[0].Call(ctx, new[] { p.RawObject, q.RawObject }).ReturningObject));
            return result.RawObject.NoResume();
        }

#pragma warning disable 1591
        public override string ToString() => $"Array: {array.Count} elements";
        public override bool Equals(object obj)
        {
            var ar = obj as StellarRoboArray;
            if (ar == null) return false;
            return ar.array == array;
        }
        public override int GetHashCode() => array.GetHashCode();
#pragma warning restore 1591
    }
}
