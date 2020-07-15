using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using StellarRobo.Type;
using System.Text.RegularExpressions;

namespace StellarRobo.Standard
{
#pragma warning disable 1591
    /// <summary>
    /// StellarRoboで正規表現を扱います。
    /// </summary>
    public sealed class StellarRoboRegex : StellarRoboObject
    {
        public static readonly string ClassName = "Regex";
        internal static StellarRoboInteropClassInfo Information { get; } = new StellarRoboInteropClassInfo(ClassName);
        private Regex regex;

        #region overrideメンバー
        static StellarRoboRegex()
        {
            Information.AddClassMethod(new StellarRoboInteropMethodInfo("new", ClassNew));
            Information.AddClassMethod(new StellarRoboInteropMethodInfo("match", ClassMatch));
            Information.AddClassMethod(new StellarRoboInteropMethodInfo("replace", ClassReplace));
            Information.AddClassMethod(new StellarRoboInteropMethodInfo("split", ClassSplit));
        }


        public StellarRoboRegex(string pattern)
        {
            regex = new Regex(pattern);
            ExtraType = ClassName;
            RegisterInstanceMembers();
        }

        protected internal override StellarRoboReference GetMemberReference(string name)
        {
            switch (name)
            {
                case "match": return i_match;
                case "split": return i_split;

                default: return base.GetMemberReference(name);
            }
        }

        #endregion

        #region インスタンスメンバー
        private StellarRoboReference i_match, i_split;

        private void RegisterInstanceMembers()
        {
            i_match = StellarRoboReference.Right(this, InstanceMatch);
            i_split = StellarRoboReference.Right(this, InstanceSplit);

        }

        private StellarRoboFunctionResult InstanceMatch(StellarRoboContext ctx, StellarRoboObject self, StellarRoboObject[] args)
        {
            Match result;
            switch (args.Length)
            {
                case 1:
                    result = regex.Match(args[0].ToString());
                    break;
                case 2:
                    result = regex.Match(args[0].ToString(), args[1].ToInt32());
                    break;
                case 3:
                    result = regex.Match(args[0].ToString(), args[1].ToInt32(), args[2].ToInt32());
                    break;
                default:
                    result = null;
                    break;
            }
            return new StellarRoboMatch(result).NoResume();
        }

        private StellarRoboFunctionResult InstanceSplit(StellarRoboContext ctx, StellarRoboObject self, StellarRoboObject[] args)
        {
            var result = regex.Split(args[0].ToString());
            return result.ToStellarRoboArray().NoResume();
        }

        #endregion

        #region クラスメソッド

        private static StellarRoboFunctionResult ClassNew(StellarRoboContext ctx, StellarRoboObject self, StellarRoboObject[] args) => new StellarRoboRegex(args[0].ToString()).NoResume();

        private static StellarRoboFunctionResult ClassMatch(StellarRoboContext ctx, StellarRoboObject self, StellarRoboObject[] args)
        {
            //TODO: matchメソッドの処理を記述してください
            var result = Regex.Match(args[0].ToString(), args[1].ToString());
            return new StellarRoboMatch(result).NoResume();
        }

        private static StellarRoboFunctionResult ClassReplace(StellarRoboContext ctx, StellarRoboObject self, StellarRoboObject[] args)
        {
            var sa = args.ExpectString(3, false);
            var result = Regex.Replace(sa[0], sa[1], sa[2]);
            return result.AsStellarRoboString().NoResume();
        }

        private static StellarRoboFunctionResult ClassSplit(StellarRoboContext ctx, StellarRoboObject self, StellarRoboObject[] args)
        {
            var sa = args.ExpectString(3, false);
            var result = Regex.Split(sa[0], sa[1]);
            return result.ToStellarRoboArray().NoResume();
        }

        #endregion
    }
#pragma warning restore 1591
}
