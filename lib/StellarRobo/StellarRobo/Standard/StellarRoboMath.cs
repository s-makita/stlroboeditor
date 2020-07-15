using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using StellarRobo.Type;

namespace StellarRobo.Standard
{
#pragma warning disable 1591
    /// <summary>
    /// StellarRoboに数学関数を提供します。
    /// </summary>
    public sealed class StellarRoboMath : StellarRoboObject
    {
        public static readonly string ClassName = "Math";
        internal static StellarRoboInteropClassInfo Information { get; } = new StellarRoboInteropClassInfo(ClassName);

        #region overrideメンバー
        static StellarRoboMath()
        {
            Information.AddClassMethod(new StellarRoboInteropMethodInfo("sin", ClassSin));
            Information.AddClassMethod(new StellarRoboInteropMethodInfo("cos", ClassCos));
            Information.AddClassMethod(new StellarRoboInteropMethodInfo("tan", ClassTan));
            Information.AddClassMethod(new StellarRoboInteropMethodInfo("sinh", ClassSinh));
            Information.AddClassMethod(new StellarRoboInteropMethodInfo("cosh", ClassCosh));
            Information.AddClassMethod(new StellarRoboInteropMethodInfo("tanh", ClassTanh));
            Information.AddClassMethod(new StellarRoboInteropMethodInfo("asin", ClassAsin));
            Information.AddClassMethod(new StellarRoboInteropMethodInfo("acos", ClassAcos));
            Information.AddClassMethod(new StellarRoboInteropMethodInfo("atan", ClassAtan));
            Information.AddClassMethod(new StellarRoboInteropMethodInfo("atan2", ClassAtan2));
            Information.AddClassMethod(new StellarRoboInteropMethodInfo("max", ClassMax));
            Information.AddClassMethod(new StellarRoboInteropMethodInfo("min", ClassMin));
            Information.AddClassMethod(new StellarRoboInteropMethodInfo("limit", ClassLimit));
            Information.AddClassMethod(new StellarRoboInteropMethodInfo("abs", ClassAbs));
            Information.AddClassMethod(new StellarRoboInteropMethodInfo("ceil", ClassCeil));
            Information.AddClassMethod(new StellarRoboInteropMethodInfo("floor", ClassFloor));
            Information.AddClassMethod(new StellarRoboInteropMethodInfo("exp", ClassExp));
            Information.AddClassMethod(new StellarRoboInteropMethodInfo("log", ClassLog));
            Information.AddClassMethod(new StellarRoboInteropMethodInfo("log10", ClassLog10));
            Information.AddClassMethod(new StellarRoboInteropMethodInfo("pow", ClassPow));
            Information.AddClassMethod(new StellarRoboInteropMethodInfo("round", ClassRound));
            Information.AddClassMethod(new StellarRoboInteropMethodInfo("sign", ClassSign));
        }

        public StellarRoboMath()
        {
            ExtraType = ClassName;
            RegisterInstanceMembers();
        }

        protected internal override StellarRoboReference GetMemberReference(string name)
        {
            switch (name)
            {

                default: return base.GetMemberReference(name);
            }
        }

        #endregion

        #region インスタンスメンバー

        private void RegisterInstanceMembers()
        {

        }

        #endregion
        #region クラスメソッド

        private static StellarRoboFunctionResult ClassSin(StellarRoboContext ctx, StellarRoboObject self, StellarRoboObject[] args)
        {
            var result = Math.Sin(args[0].ToDouble());
            return result.AsStellarRoboFloat().NoResume();
        }

        private static StellarRoboFunctionResult ClassCos(StellarRoboContext ctx, StellarRoboObject self, StellarRoboObject[] args)
        {
            var result = Math.Cos(args[0].ToDouble());
            return result.AsStellarRoboFloat().NoResume();
        }

        private static StellarRoboFunctionResult ClassTan(StellarRoboContext ctx, StellarRoboObject self, StellarRoboObject[] args)
        {
            var result = Math.Tan(args[0].ToDouble());
            return result.AsStellarRoboFloat().NoResume();
        }

        private static StellarRoboFunctionResult ClassSinh(StellarRoboContext ctx, StellarRoboObject self, StellarRoboObject[] args)
        {
            var result = Math.Sinh(args[0].ToDouble());
            return result.AsStellarRoboFloat().NoResume();
        }

        private static StellarRoboFunctionResult ClassCosh(StellarRoboContext ctx, StellarRoboObject self, StellarRoboObject[] args)
        {
            var result = Math.Cosh(args[0].ToDouble());
            return result.AsStellarRoboFloat().NoResume();
        }

        private static StellarRoboFunctionResult ClassTanh(StellarRoboContext ctx, StellarRoboObject self, StellarRoboObject[] args)
        {
            var result = Math.Tanh(args[0].ToDouble());
            return result.AsStellarRoboFloat().NoResume();
        }

        private static StellarRoboFunctionResult ClassAsin(StellarRoboContext ctx, StellarRoboObject self, StellarRoboObject[] args)
        {
            var result = Math.Asin(args[0].ToDouble());
            return result.AsStellarRoboFloat().NoResume();
        }

        private static StellarRoboFunctionResult ClassAcos(StellarRoboContext ctx, StellarRoboObject self, StellarRoboObject[] args)
        {
            var result = Math.Acos(args[0].ToDouble());
            return result.AsStellarRoboFloat().NoResume();
        }

        private static StellarRoboFunctionResult ClassAtan(StellarRoboContext ctx, StellarRoboObject self, StellarRoboObject[] args)
        {
            var result = Math.Atan(args[0].ToDouble());
            return result.AsStellarRoboFloat().NoResume();
        }

        private static StellarRoboFunctionResult ClassAtan2(StellarRoboContext ctx, StellarRoboObject self, StellarRoboObject[] args)
        {
            var da = args.ExpectDouble(2, false);
            var result = Math.Atan2(da[0], da[1]);
            return result.AsStellarRoboFloat().NoResume();
        }

        private static StellarRoboFunctionResult ClassMax(StellarRoboContext ctx, StellarRoboObject self, StellarRoboObject[] args)
        {
            var da = args.ExpectDouble(2, false);
            var result = Math.Max(da[0], da[1]);
            return result.AsStellarRoboFloat().NoResume();
        }

        private static StellarRoboFunctionResult ClassMin(StellarRoboContext ctx, StellarRoboObject self, StellarRoboObject[] args)
        {
            var da = args.ExpectDouble(2, false);
            var result = Math.Min(da[0], da[1]);
            return result.AsStellarRoboFloat().NoResume();
        }

        private static StellarRoboFunctionResult ClassLimit(StellarRoboContext ctx, StellarRoboObject self, StellarRoboObject[] args)
        {
            var da = args.ExpectDouble(3, false);
            var result = da[0];
            if (result < da[1]) result = da[1];
            if (result > da[2]) result = da[2];
            return result.AsStellarRoboFloat().NoResume();
        }

        private static StellarRoboFunctionResult ClassAbs(StellarRoboContext ctx, StellarRoboObject self, StellarRoboObject[] args)
        {
            var result = Math.Abs(args[0].ToDouble());
            return result.AsStellarRoboFloat().NoResume();
        }

        private static StellarRoboFunctionResult ClassCeil(StellarRoboContext ctx, StellarRoboObject self, StellarRoboObject[] args)
        {
            var result = Math.Ceiling(args[0].ToDouble());
            return result.AsStellarRoboFloat().NoResume();
        }

        private static StellarRoboFunctionResult ClassFloor(StellarRoboContext ctx, StellarRoboObject self, StellarRoboObject[] args)
        {
            var result = Math.Floor(args[0].ToDouble());
            return result.AsStellarRoboFloat().NoResume();
        }

        private static StellarRoboFunctionResult ClassExp(StellarRoboContext ctx, StellarRoboObject self, StellarRoboObject[] args)
        {
            var result = Math.Exp(args[0].ToDouble());
            return result.AsStellarRoboFloat().NoResume();
        }

        private static StellarRoboFunctionResult ClassLog(StellarRoboContext ctx, StellarRoboObject self, StellarRoboObject[] args)
        {
            var result = Math.Log(args[0].ToDouble());
            return result.AsStellarRoboFloat().NoResume();
        }

        private static StellarRoboFunctionResult ClassLog10(StellarRoboContext ctx, StellarRoboObject self, StellarRoboObject[] args)
        {
            var result = Math.Log10(args[0].ToDouble());
            return result.AsStellarRoboFloat().NoResume();
        }

        private static StellarRoboFunctionResult ClassPow(StellarRoboContext ctx, StellarRoboObject self, StellarRoboObject[] args)
        {
            var da = args.ExpectDouble(2, false);
            var result = Math.Pow(da[0], da[1]);
            return result.AsStellarRoboFloat().NoResume();
        }

        private static StellarRoboFunctionResult ClassRound(StellarRoboContext ctx, StellarRoboObject self, StellarRoboObject[] args)
        {
            var result = Math.Round(args[0].ToDouble());
            return result.AsStellarRoboFloat().NoResume();
        }

        private static StellarRoboFunctionResult ClassSign(StellarRoboContext ctx, StellarRoboObject self, StellarRoboObject[] args)
        {
            var result = Math.Sign(args[0].ToDouble());
            return result.AsStellarRoboInteger().NoResume();
        }

        #endregion
    }
#pragma warning restore 1591
}
