using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using StellarRobo.Type;

namespace StellarRobo.Standard
{
#pragma warning disable 1591
    /// <summary>
    /// StellarRoboで日付を定義します。
    /// </summary>
    public sealed class StellarRoboDateTime : StellarRoboObject
    {
        public static readonly string ClassName = "DateTime";
        internal static StellarRoboInteropClassInfo Information { get; } = new StellarRoboInteropClassInfo(ClassName);
        private DateTime datetime;
        #region overrideメンバー
        static StellarRoboDateTime()
        {
            Information.AddClassMethod(new StellarRoboInteropMethodInfo("now", ClassNow));
            Information.AddClassMethod(new StellarRoboInteropMethodInfo("days_in_month", ClassDaysInMonth));
            Information.AddClassMethod(new StellarRoboInteropMethodInfo("is_leap", ClassIsLeap));
            Information.AddClassMethod(new StellarRoboInteropMethodInfo("parse", ClassParse));
        }

        /// <summary>
        /// 新しいインスタンスを生成します。
        /// </summary>
        /// <param name="dt"></param>
        public StellarRoboDateTime(DateTime dt)
        {
            datetime = dt;
            ExtraType = ClassName;
            f_date = StellarRoboReference.Right(new StellarRoboObject());
            f_day = StellarRoboReference.Right(dt.Day);
            f_day_of_week = StellarRoboReference.Right((int)dt.DayOfWeek);
            f_day_of_year = StellarRoboReference.Right(dt.DayOfYear);
            f_hour = StellarRoboReference.Right(dt.Hour);
            f_kind = StellarRoboReference.Right(dt.Kind.ToString());
            f_millisecond = StellarRoboReference.Right(dt.Millisecond);
            f_minute = StellarRoboReference.Right(dt.Minute);
            f_month = StellarRoboReference.Right(dt.Month);
            f_second = StellarRoboReference.Right(dt.Second);
            f_ticks = StellarRoboReference.Right(dt.Ticks);
            f_time = StellarRoboReference.Right(new StellarRoboObject());
            f_today = StellarRoboReference.Right(new StellarRoboObject());
            f_year = StellarRoboReference.Right(dt.Year);
            RegisterInstanceMembers();
        }

        protected internal override StellarRoboReference GetMemberReference(string name)
        {
            switch (name)
            {
                case "add": return i_add;
                case "add_days": return i_add_days;
                case "add_hours": return i_add_hours;
                case "add_minutes": return i_add_minutes;
                case "add_months": return i_add_months;
                case "add_seconds": return i_add_seconds;
                case "add_ticks": return i_add_ticks;
                case "add_years": return i_add_years;
                case "sub": return i_sub;
                case "to_local": return i_to_local;

                case "date": return f_date;
                case "day": return f_day;
                case "day_of_week": return f_day_of_week;
                case "day_of_year": return f_day_of_year;
                case "hour": return f_hour;
                case "kind": return f_kind;
                case "millisecond": return f_millisecond;
                case "minute": return f_minute;
                case "month": return f_month;
                case "second": return f_second;
                case "ticks": return f_ticks;
                case "time": return f_time;
                case "today": return f_today;
                case "year": return f_year;
                default: return base.GetMemberReference(name);
            }
        }

        public override StellarRoboObject AsByValValue() => new StellarRoboDateTime(datetime);

        public override bool Equals(object obj)
        {
            if (((StellarRoboObject)obj).ExtraType != "DateTime") return false;
            return ((StellarRoboDateTime)obj).datetime == datetime;
        }

        public override int GetHashCode() => datetime.GetHashCode();

        public override string ToString() => datetime.ToString();

        protected internal override StellarRoboObject ExpressionOperation(StellarRoboILCodeType op, StellarRoboObject target)
        {
            if (target.ExtraType == "DateTime")
            {
                var t = ((StellarRoboDateTime)target).datetime;
                switch (op)
                {
                    case StellarRoboILCodeType.Minus:
                        return new StellarRoboTimeSpan(datetime - t);
                    default:
                        return StellarRoboNil.Instance;
                }
            }
            else if (target.ExtraType == "TimeSpan")
            {
                var t = ((StellarRoboTimeSpan)target).timespan;
                switch (op)
                {
                    case StellarRoboILCodeType.Plus:
                        return new StellarRoboDateTime(datetime + t);
                    case StellarRoboILCodeType.Minus:
                        return new StellarRoboDateTime(datetime - t);
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
        #endregion

        #region インスタンスメンバー
        private StellarRoboReference f_date, f_day, f_day_of_week, f_day_of_year, f_hour, f_kind, f_millisecond, f_minute, f_month, f_second, f_ticks, f_time, f_today, f_year;
        private StellarRoboReference i_add, i_add_days, i_add_hours, i_add_minutes, i_add_months, i_add_seconds, i_add_ticks, i_add_years, i_sub, i_to_local;

        private void RegisterInstanceMembers()
        {
            i_add = StellarRoboReference.Right(this, InstanceAdd);
            i_add_days = StellarRoboReference.Right(this, InstanceAddDays);
            i_add_hours = StellarRoboReference.Right(this, InstanceAddHours);
            i_add_minutes = StellarRoboReference.Right(this, InstanceAddMinutes);
            i_add_months = StellarRoboReference.Right(this, InstanceAddMonths);
            i_add_seconds = StellarRoboReference.Right(this, InstanceAddSeconds);
            i_add_ticks = StellarRoboReference.Right(this, InstanceAddTicks);
            i_add_years = StellarRoboReference.Right(this, InstanceAddYears);
            i_sub = StellarRoboReference.Right(this, InstanceSub);
            i_to_local = StellarRoboReference.Right(this, InstanceToLocal);
        }

        private StellarRoboFunctionResult InstanceAdd(StellarRoboContext ctx, StellarRoboObject self, StellarRoboObject[] args)
        {
            if (args[0].ExtraType != "DateTime") return StellarRoboNil.Instance.NoResume();
            var result = new StellarRoboDateTime(datetime.Add(((StellarRoboTimeSpan)args[0]).timespan));
            return result.NoResume();
        }

        private StellarRoboFunctionResult InstanceAddDays(StellarRoboContext ctx, StellarRoboObject self, StellarRoboObject[] args)
        {
            var result = new StellarRoboDateTime(datetime.AddDays(args[0].ToInt32()));
            return result.NoResume();
        }

        private StellarRoboFunctionResult InstanceAddHours(StellarRoboContext ctx, StellarRoboObject self, StellarRoboObject[] args)
        {
            var result = new StellarRoboDateTime(datetime.AddHours(args[0].ToInt32()));
            return result.NoResume();
        }

        private StellarRoboFunctionResult InstanceAddMinutes(StellarRoboContext ctx, StellarRoboObject self, StellarRoboObject[] args)
        {
            var result = new StellarRoboDateTime(datetime.AddMinutes(args[0].ToInt32()));
            return result.NoResume();
        }

        private StellarRoboFunctionResult InstanceAddMonths(StellarRoboContext ctx, StellarRoboObject self, StellarRoboObject[] args)
        {
            var result = new StellarRoboDateTime(datetime.AddMonths(args[0].ToInt32()));
            return result.NoResume();
        }

        private StellarRoboFunctionResult InstanceAddSeconds(StellarRoboContext ctx, StellarRoboObject self, StellarRoboObject[] args)
        {
            var result = new StellarRoboDateTime(datetime.AddSeconds(args[0].ToInt32()));
            return result.NoResume();
        }

        private StellarRoboFunctionResult InstanceAddTicks(StellarRoboContext ctx, StellarRoboObject self, StellarRoboObject[] args)
        {
            var result = new StellarRoboDateTime(datetime.AddTicks(args[0].ToInt32()));
            return result.NoResume();
        }

        private StellarRoboFunctionResult InstanceAddYears(StellarRoboContext ctx, StellarRoboObject self, StellarRoboObject[] args)
        {
            var result = new StellarRoboDateTime(datetime.AddYears(args[0].ToInt32()));
            return result.NoResume();
        }

        private StellarRoboFunctionResult InstanceSub(StellarRoboContext ctx, StellarRoboObject self, StellarRoboObject[] args)
        {
            var result = new StellarRoboDateTime(datetime - ((StellarRoboTimeSpan)args[0]).timespan);
            return result.NoResume();
        }

        private StellarRoboFunctionResult InstanceToLocal(StellarRoboContext ctx, StellarRoboObject self, StellarRoboObject[] args)
        {
            var result = new StellarRoboDateTime(datetime.ToLocalTime());
            return result.NoResume();
        }

        #endregion

        #region クラスメソッド

        private static StellarRoboFunctionResult ClassNow(StellarRoboContext ctx, StellarRoboObject self, StellarRoboObject[] args)
        {
            var result = new StellarRoboDateTime(DateTime.Now);
            return result.NoResume();
        }

        private static StellarRoboFunctionResult ClassDaysInMonth(StellarRoboContext ctx, StellarRoboObject self, StellarRoboObject[] args)
        {
            var result = DateTime.DaysInMonth(args[0].ToInt32(), args[1].ToInt32());
            return result.AsStellarRoboInteger().NoResume();
        }

        private static StellarRoboFunctionResult ClassIsLeap(StellarRoboContext ctx, StellarRoboObject self, StellarRoboObject[] args)
        {
            var result = DateTime.IsLeapYear(args[0].ToInt32());
            return result.AsStellarRoboBoolean().NoResume();
        }

        private static StellarRoboFunctionResult ClassParse(StellarRoboContext ctx, StellarRoboObject self, StellarRoboObject[] args)
        {
            var result = new StellarRoboDateTime(DateTime.Parse(args[0].ToString()));
            return result.NoResume();
        }

        #endregion
    }
#pragma warning restore 1591
}
