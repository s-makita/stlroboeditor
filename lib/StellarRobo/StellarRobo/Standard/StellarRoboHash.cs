using StellarRobo.Type;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StellarRobo.Standard
{
#pragma warning disable 1591
    /// <summary>
    /// StellarRoboでデータ集合を定義するハッシュを定義します。
    /// </summary>
    public sealed class StellarRoboHash : StellarRoboObject
    {
        public static readonly string ClassName = "Hash";

        #region 改変不要
        internal static StellarRoboInteropClassInfo Information { get; } = new StellarRoboInteropClassInfo(ClassName);
        #endregion

        Dictionary<string, StellarRoboReference> members = new Dictionary<string, StellarRoboReference>();

        #region overrideメンバー

        static StellarRoboHash()
        {
            Information.AddClassMethod(new StellarRoboInteropMethodInfo("new", ClassNew));
        }

        public StellarRoboHash()
        {
            ExtraType = ClassName;
        }

        protected internal override StellarRoboReference GetMemberReference(string name)
        {
            if (!members.ContainsKey(name))
            {
                members[name] = new StellarRoboReference { IsLeftValue = true, RawObject = StellarRoboNil.Instance };
            }
            return members[name];
        }

        protected internal override StellarRoboReference GetIndexerReference(StellarRoboObject[] indices)
        {
            var name = indices[0].ToString();
            return GetMemberReference(name);
        }

        public override StellarRoboObject AsByValValue() => base.AsByValValue();
        
        
        public override int GetHashCode() => members.GetHashCode();
        #endregion


        #region クラスメソッド
        

        private static StellarRoboFunctionResult ClassNew(StellarRoboContext ctx, StellarRoboObject self, StellarRoboObject[] args) => new StellarRoboInteropClassBase().NoResume();
        #endregion
    }
#pragma warning restore 1591
}
