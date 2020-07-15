using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using StellarRobo.Type;

namespace StellarRobo.Standard
{
#pragma warning disable 1591
    /// <summary>
    /// StellarRoboでTimeSpanを表現します。
    /// </summary>
    public sealed class StellarRoboTimeSpan : StellarRoboObject
    {
        public static readonly string ClassName = "TimeSpan";
        internal static StellarRoboInteropClassInfo Information { get; } = new StellarRoboInteropClassInfo(ClassName);
        internal TimeSpan timespan;

        #region overrideメンバー
        static StellarRoboTimeSpan()
        {
            Information.AddClassMethod(new StellarRoboInteropMethodInfo("from_days", ClassFromDays));
            Information.AddClassMethod(new StellarRoboInteropMethodInfo("from_hours", ClassFromHours));
            Information.AddClassMethod(new StellarRoboInteropMethodInfo("from_milliseconds", ClassFromMilliseconds));
            Information.AddClassMethod(new StellarRoboInteropMethodInfo("from_minutes", ClassFromMinutes));
            Information.AddClassMethod(new StellarRoboInteropMethodInfo("from_seconds", ClassFromSeconds));
            Information.AddClassMethod(new StellarRoboInteropMethodInfo("from_ticks", ClassFromTicks));
            Information.AddClassMethod(new StellarRoboInteropMethodInfo("parse", ClassParse));
        }

        /// <summary>
        /// 新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="span">基にする<see cref="TimeSpan"/></param>
        public StellarRoboTimeSpan(TimeSpan span)
        {
            ExtraType = ClassName;
            timespan = span;
            f_days = StellarRoboReference.Right(span.Days);
            f_hours = StellarRoboReference.Right(span.Hours);
            f_milliseconds = StellarRoboReference.Right(span.Milliseconds);
            f_minutes = StellarRoboReference.Right(span.Minutes);
            f_seconds = StellarRoboReference.Right(span.Seconds);
            f_ticks = StellarRoboReference.Right(span.Ticks);
            f_total_days = StellarRoboReference.Right(span.TotalDays);
            f_total_hours = StellarRoboReference.Right(span.TotalHours);
            f_total_milliseconds = StellarRoboReference.Right(span.TotalMilliseconds);
            f_total_minutes = StellarRoboReference.Right(span.TotalMinutes);
            f_total_seconds = StellarRoboReference.Right(span.TotalSeconds);
            RegisterInstanceMembers();
        }

        protected internal override StellarRoboReference GetMemberReference(string name)
        {
            switch (name)
            {
                case "add": return i_add;
                case "sub": return i_sub;

                case "days": return f_days;
                case "hours": return f_hours;
                case "milliseconds": return f_milliseconds;
                case "minutes": return f_minutes;
                case "seconds": return f_seconds;
                case "ticks": return f_ticks;
                case "total_days": return f_total_days;
                case "total_hours": return f_total_hours;
                case "total_milliseconds": return f_total_milliseconds;
                case "total_minutes": return f_total_minutes;
                case "total_seconds": return f_total_seconds;
                default: return base.GetMemberReference(name);
            }
        }

        protected internal override StellarRoboObject ExpressionOperation(StellarRoboILCodeType op, StellarRoboObject target)
        {
            if (target.ExtraType == "TimeSpan")
            {
                var t = ((StellarRoboTimeSpan)target).timespan;
                switch (op)
                {
                    case StellarRoboILCodeType.Plus:
                        return new StellarRoboTimeSpan(timespan + t);
                    case StellarRoboILCodeType.Minus:
                        return new StellarRoboTimeSpan(timespan - t);
                    default:
                        return StellarRoboNil.Instance;
                }
            }
            else
            {
                switch (op)
                {
                    case StellarRoboILCodeType.Equal:
                        return StellarRoboBoolean.False;
                    case StellarRoboILCodeType.NotEqual:
                        return StellarRoboBoolean.True;
                    default:
                        return StellarRoboNil.Instance;
                }
            }
        }

        public override bool Equals(object obj)
        {
            if (((StellarRoboObject)obj).ExtraType != "TimeSpan") return false;
            return ((StellarRoboTimeSpan)obj).timespan == timespan;
        }

        public override int GetHashCode() => timespan.GetHashCode();

        public override StellarRoboObject AsByValValue() => new StellarRoboTimeSpan(timespan);

        public override string ToString() => timespan.ToString();

        #endregion

        #region インスタンスメンバー
        private StellarRoboReference f_days, f_hours, f_milliseconds, f_minutes, f_seconds, f_ticks, f_total_days, f_total_hours, f_total_milliseconds, f_total_minutes, f_total_seconds;
        private StellarRoboReference i_add, i_sub;

        private void RegisterInstanceMembers()
        {
            i_add = StellarRoboReference.Right(this, InstanceAdd);
            i_sub = StellarRoboReference.Right(this, InstanceSub);
        }

        private StellarRoboFunctionResult InstanceAdd(StellarRoboContext ctx, StellarRoboObject self, StellarRoboObject[] args)
        {
            var result = new StellarRoboTimeSpan(timespan.Add(((StellarRoboTimeSpan)args[0]).timespan));
            return result.NoResume();
        }

        private StellarRoboFunctionResult InstanceSub(StellarRoboContext ctx, StellarRoboObject self, StellarRoboObject[] args)
        {
            var result = new StellarRoboTimeSpan(timespan.Subtract(((StellarRoboTimeSpan)args[0]).timespan));
            return result.NoResume();
        }

        #endregion

        #region クラスメソッド

        private static StellarRoboFunctionResult ClassFromDays(StellarRoboContext ctx, StellarRoboObject self, StellarRoboObject[] args)
        {
            var result = new StellarRoboTimeSpan(TimeSpan.FromDays(args[0].ToInt32()));
            return result.NoResume();
        }

        private static StellarRoboFunctionResult ClassFromHours(StellarRoboContext ctx, StellarRoboObject self, StellarRoboObject[] args)
        {
            var result = new StellarRoboTimeSpan(TimeSpan.FromHours(args[0].ToInt32()));
            return result.NoResume();
        }

        private static StellarRoboFunctionResult ClassFromMilliseconds(StellarRoboContext ctx, StellarRoboObject self, StellarRoboObject[] args)
        {
            var result = new StellarRoboTimeSpan(TimeSpan.FromMilliseconds(args[0].ToInt32()));
            return result.NoResume();
        }

        private static StellarRoboFunctionResult ClassFromMinutes(StellarRoboContext ctx, StellarRoboObject self, StellarRoboObject[] args)
        {
            var result = new StellarRoboTimeSpan(TimeSpan.FromMinutes(args[0].ToInt32()));
            return result.NoResume();
        }

        private static StellarRoboFunctionResult ClassFromSeconds(StellarRoboContext ctx, StellarRoboObject self, StellarRoboObject[] args)
        {
            var result = new StellarRoboTimeSpan(TimeSpan.FromSeconds(args[0].ToInt32()));
            return result.NoResume();
        }

        private static StellarRoboFunctionResult ClassFromTicks(StellarRoboContext ctx, StellarRoboObject self, StellarRoboObject[] args)
        {
            var result = new StellarRoboTimeSpan(TimeSpan.FromTicks(args[0].ToInt32()));
            return result.NoResume();
        }

        private static StellarRoboFunctionResult ClassParse(StellarRoboContext ctx, StellarRoboObject self, StellarRoboObject[] args)
        {
            var result = new StellarRoboTimeSpan(TimeSpan.Parse(args[0].ToString()));
            return result.NoResume();
        }

        #endregion
    }
#pragma warning restore 1591
}
