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
    /// <see cref="StellarRoboRegex"/>の結果を定義します。
    /// </summary>
    public sealed class StellarRoboMatch : StellarRoboObject
    {
        public static readonly string ClassName = "Match";
        internal static StellarRoboInteropClassInfo Information { get; } = new StellarRoboInteropClassInfo(ClassName);
        Match refm;

        #region overrideメンバー
        public StellarRoboMatch(Match match)
        {
            ExtraType = ClassName;
            refm = match;

            f_success = StellarRoboReference.Right(refm.Success);
            f_length = StellarRoboReference.Right(refm.Length);
            f_index = StellarRoboReference.Right(refm.Index);
            f_value = StellarRoboReference.Right(refm.Value);
            f_captures = StellarRoboReference.Right(new StellarRoboRegexCaptures(refm.Captures));
            f_groups = StellarRoboReference.Right(new StellarRoboRegexGroups(refm.Groups));
            RegisterInstanceMembers();
        }

        protected internal override StellarRoboReference GetMemberReference(string name)
        {
            switch (name)
            {

                case "success": return f_success;
                case "length": return f_length;
                case "index": return f_index;
                case "value": return f_value;
                case "captures": return f_captures;
                case "groups": return f_groups;
                default: return base.GetMemberReference(name);
            }
        }

        #endregion

        #region インスタンスメンバー
        private StellarRoboReference f_success, f_length, f_index, f_value, f_captures, f_groups;

        private void RegisterInstanceMembers()
        {

        }

        #endregion
    }

    /// <summary>
    /// <see cref="StellarRoboRegex"/>のキャプチャリストを定義します。
    /// </summary>
    public sealed class StellarRoboRegexCaptures : StellarRoboObject
    {
        public static readonly string ClassName = "RegexCaptures";
        CaptureCollection cc;
        List<StellarRoboReference> captures;

        public StellarRoboRegexCaptures(CaptureCollection col)
        {
            cc = col;
            captures = new List<StellarRoboReference>();
            for (int i = 0; i < cc.Count; i++) captures.Add(StellarRoboReference.Right(new StellarRoboRegexCapture(cc[i])));
        }

        protected internal override StellarRoboReference GetIndexerReference(StellarRoboObject[] indices)
        {
            var i = indices[0].ToInt32();
            return captures[i];
        }
    }

    /// <summary>
    /// <see cref="StellarRoboRegex"/>のキャプチャを定義します。
    /// </summary>
    public sealed class StellarRoboRegexCapture : StellarRoboObject
    {
        public static readonly string ClassName = "RegexCapture";
        Capture cap;
        StellarRoboReference length, index, value;

        internal StellarRoboRegexCapture(Capture c)
        {
            cap = c;
            length = StellarRoboReference.Right(cap.Length);
            index = StellarRoboReference.Right(cap.Index);
            value = StellarRoboReference.Right(cap.Value);
        }

        protected internal override StellarRoboReference GetMemberReference(string name)
        {
            switch (name)
            {
                case nameof(length): return length;
                case nameof(value): return value;
                case nameof(index): return index;
            }
            return base.GetMemberReference(name);
        }
    }

    /// <summary>
    /// <see cref="StellarRoboRegex"/>のグループリストを定義します。
    /// </summary>
    public sealed class StellarRoboRegexGroups : StellarRoboObject
    {
        public static readonly string ClassName = "RegexGroups";
        GroupCollection cc;
        List<StellarRoboReference> groups;

        public StellarRoboRegexGroups(GroupCollection col)
        {
            cc = col;
            groups = new List<StellarRoboReference>();
            for (int i = 0; i < cc.Count; i++) groups.Add(StellarRoboReference.Right(new StellarRoboRegexGroup(cc[i])));
        }

        protected internal override StellarRoboReference GetIndexerReference(StellarRoboObject[] indices)
        {
            var i = indices[0].ToInt32();
            return groups[i];
        }
    }

    /// <summary>
    /// <see cref="StellarRoboRegex"/>のグループを定義します。
    /// </summary>
    public sealed class StellarRoboRegexGroup : StellarRoboObject
    {
        public static readonly string ClassName = "RegexGroup";
        Group gr;
        StellarRoboReference length, index, value;

        internal StellarRoboRegexGroup(Group g)
        {
            gr = g;
            length = StellarRoboReference.Right(gr.Length);
            index = StellarRoboReference.Right(gr.Index);
            value = StellarRoboReference.Right(gr.Value);
        }

        protected internal override StellarRoboReference GetMemberReference(string name)
        {
            switch (name)
            {
                case nameof(length): return length;
                case nameof(value): return value;
                case nameof(index): return index;
            }
            return base.GetMemberReference(name);
        }
    }
#pragma warning restore 1591
}
