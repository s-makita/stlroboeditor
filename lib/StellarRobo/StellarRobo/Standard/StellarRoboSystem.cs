using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StellarRobo.Type;

namespace StellarRobo.Standard
{
#pragma warning disable 1591
    /// <summary>
    /// OSとのやりとりを提供します。
    /// </summary>
    public sealed class StellarRoboSystem
    {
        public static readonly string ClassName = "System";
        internal static StellarRoboInteropClassInfo Information { get; } = new StellarRoboInteropClassInfo(ClassName);

        static StellarRoboSystem()
        {

        }

        public StellarRoboSystem()
        {
        }

        private static StellarRoboFunctionResult ClassFoo(StellarRoboContext ctx, StellarRoboObject self, StellarRoboObject[] args) => StellarRoboNil.Instance.NoResume();
    }
#pragma warning restore 1591
}
