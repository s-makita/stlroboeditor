using System.Collections.Generic;
using System.Linq;

namespace StellarRobo.Analyze
{
    /// <summary>
    /// 式のノードです。
    /// </summary>
    public class StellarRoboExpressionAstNode : StellarRoboAstNode
    {
        /// <summary>
        /// 演算子のタイプを取得します。
        /// </summary>
        public StellarRoboOperatorType ExpressionType { get; protected internal set; }

        /// <summary>
        /// インスタンスを初期化します。
        /// </summary>
        protected internal StellarRoboExpressionAstNode()
        {
            Type = StellarRoboAstNodeType.Expression;
        }
    }

    /// <summary>
    /// 単項式ノード
    /// </summary>
    public class StellarRoboFactorExpressionAstNode : StellarRoboPrimaryExpressionAstNode
    {
        /// <summary>
        /// 因子の種類を取得します。
        /// </summary>
        public StellarRoboFactorType FactorType { get; protected internal set; }

        /// <summary>
        /// boolリテラルの内容を取得します。
        /// </summary>
        public bool BooleanValue { get; protected internal set; }

        /// <summary>
        /// 識別子名・文字列リテラルの内容を取得します。
        /// </summary>
        public string StringValue { get; protected internal set; }

        /// <summary>
        /// 整数リテラルの内容を取得します。
        /// </summary>
        public long IntegerValue { get; protected internal set; }

        /// <summary>
        /// 単精度リテラルの内容を取得します。
        /// </summary>
        public float SingleValue { get; protected internal set; }

        /// <summary>
        /// 倍精度リテラルの内容を取得します。
        /// </summary>
        public double DoubleValue { get; protected internal set; }

        /// <summary>
        /// カッコ式のノードを取得します。
        /// </summary>
        public StellarRoboExpressionAstNode ExpressionNode { get; protected internal set; }

        /// <summary>
        /// 配列要素のノードを取得します。
        /// </summary>
        public IList<StellarRoboExpressionAstNode> ElementNodes { get; protected internal set; } = new List<StellarRoboExpressionAstNode>();

        /// <summary>
        /// base
        /// </summary>
        public StellarRoboFactorExpressionAstNode() : base() { }

        /// <summary>
        /// 現在のオブジェクトを表す文字列を返します。
        /// </summary>
        /// <returns>文字列</returns>
        public override IReadOnlyList<string> ToDebugStringList()
        {
            var result = new List<string>();
            switch (FactorType)
            {
                case StellarRoboFactorType.Nil:
                    result.Add("Nil: Nil");
                    break;
                case StellarRoboFactorType.BooleanValue:
                    result.Add($"Boolean: {BooleanValue}");
                    break;
                case StellarRoboFactorType.IntegerValue:
                    result.Add($"Integer: {IntegerValue}");
                    break;
                case StellarRoboFactorType.SingleValue:
                    result.Add($"Single: {SingleValue}");
                    break;
                case StellarRoboFactorType.DoubleValue:
                    result.Add($"Double: {DoubleValue}");
                    break;
                case StellarRoboFactorType.StringValue:
                    result.Add($"String: {StringValue}");
                    break;
                case StellarRoboFactorType.Identifer:
                    result.Add($"Identifer: {StringValue}");
                    break;
                case StellarRoboFactorType.ParenExpression:
                    result.AddRange(ExpressionNode.ToDebugStringList());
                    break;
                case StellarRoboFactorType.VariableArguments:
                    result.Add($"VariableArguments");
                    break;
                case StellarRoboFactorType.CoroutineResume:
                    result.Add($"Resume {StringValue}");
                    break;
                case StellarRoboFactorType.Array:
                    result.Add($"Array");
                    break;
                case StellarRoboFactorType.Lambda:
                    result.Add($"Lambda");
                    break;
            }
            return result;
        }
    }

    /// <summary>
    /// 単項式ノード
    /// </summary>
    public class StellarRoboPrimaryExpressionAstNode : StellarRoboUnaryExpressionAstNode
    {
        /// <summary>
        /// 初期化
        /// </summary>
        protected internal StellarRoboPrimaryExpressionAstNode() : base()
        {

        }
    }

    /// <summary>
    /// 単項式(インクリメント、デクリメントを含む)ノード
    /// </summary>
    public class StellarRoboUnaryExpressionAstNode : StellarRoboExpressionAstNode
    {
        /// <summary>
        /// 対象の一次式を取得します。
        /// </summary>
        public StellarRoboPrimaryExpressionAstNode Target { get; protected internal set; }

        /// <summary>
        /// 現在のオブジェクトを表す文字列を返します。
        /// </summary>
        /// <returns>文字列</returns>
        public override IReadOnlyList<string> ToDebugStringList()
        {
            var result = new List<string>();
            result.Add($"Unary: {ExpressionType}");
            result.AddRange(Target.ToDebugStringList().Select(p => "| " + p));
            return result;
        }
    }

    /// <summary>
    /// アクセサノード
    /// </summary>
    public class StellarRoboMemberAccessExpressionAstNode : StellarRoboPrimaryExpressionAstNode
    {
        /// <summary>
        /// 初期化します。
        /// </summary>
        protected internal StellarRoboMemberAccessExpressionAstNode() : base()
        {
            ExpressionType = StellarRoboOperatorType.MemberAccess;
        }

        /// <summary>
        /// 対象のメンバー名を取得します。
        /// </summary>
        public string MemberName { get; protected internal set; } = "";

        /// <summary>
        /// 現在のオブジェクトを表す文字列を返します。
        /// </summary>
        /// <returns>文字列</returns>
        public override IReadOnlyList<string> ToDebugStringList()
        {
            var result = new List<string>();
            if (ExpressionType == StellarRoboOperatorType.IndexerAccess)
            {
                MemberName = "{Indexer}";
            }
            result.Add($"Member Access: {MemberName}");
            result.Add("| [Target]");
            result.AddRange(Target.ToDebugStringList().Select(p => "| " + p));
            return result;
        }
    }

    /// <summary>
    /// メソッド呼び出しノード
    /// </summary>
    public class StellarRoboArgumentCallExpressionAstNode : StellarRoboPrimaryExpressionAstNode
    {

        /// <summary>
        /// 呼び出し時の引数リストを取得します。
        /// </summary>
        public IList<StellarRoboExpressionAstNode> Arguments { get; protected internal set; } = new List<StellarRoboExpressionAstNode>();

        /// <summary>
        /// 初期化します。
        /// </summary>
        protected internal StellarRoboArgumentCallExpressionAstNode() : base()
        {
            ExpressionType = StellarRoboOperatorType.FunctionCall;
        }

        /// <summary>
        /// 現在のオブジェクトを表す文字列を返します。
        /// </summary>
        /// <returns>文字列</returns>
        public override IReadOnlyList<string> ToDebugStringList()
        {
            var result = new List<string>();
            result.Add($"Function Call:");
            result.Add("| [Target]");
            result.AddRange(Target.ToDebugStringList().Select(p => "| " + p));
            if (Arguments.Count > 0)
            {
                result.Add("| [Arguments]");
                foreach (var i in Arguments) result.AddRange(i.ToDebugStringList().Select(p => "| " + p));
            }
            return result;
        }
    }

    /// <summary>
    /// 二項式ノード
    /// </summary>
    public class StellarRoboBinaryExpressionAstNode : StellarRoboExpressionAstNode
    {
        /// <summary>
        /// 1項目の式ノードを取得します。
        /// </summary>
        public StellarRoboExpressionAstNode FirstNode { get; protected internal set; }

        /// <summary>
        /// 1項目の式ノードを取得します。
        /// </summary>
        public StellarRoboExpressionAstNode SecondNode { get; protected internal set; }

        /// <summary>
        /// 現在のオブジェクトを表す文字列を返します。
        /// </summary>
        /// <returns>文字列</returns>
        public override IReadOnlyList<string> ToDebugStringList()
        {
            var result = new List<string>();
            result.Add($"{ExpressionType}:");
            result.AddRange(FirstNode.ToDebugStringList().Select(p => "| " + p));
            result.AddRange(SecondNode.ToDebugStringList().Select(p => "| " + p));
            return result;
        }
    }

    /// <summary>
    /// 因子の種類を定義します。
    /// </summary>
    public enum StellarRoboFactorType
    {
        /// <summary>
        /// 未定義
        /// </summary>
        Undefined,
        /// <summary>
        /// カッコ式
        /// </summary>
        ParenExpression,
        /// <summary>
        /// nil
        /// </summary>
        Nil,
        /// <summary>
        /// boolリテラル
        /// </summary>
        BooleanValue,
        /// <summary>
        /// 単精度リテラル
        /// </summary>
        SingleValue,
        /// <summary>
        /// 倍精度リテラル
        /// </summary>
        DoubleValue,
        /// <summary>
        /// 整数リテラル
        /// </summary>
        IntegerValue,
        /// <summary>
        /// 文字列リテラル
        /// </summary>
        StringValue,
        /// <summary>
        /// 識別子
        /// </summary>
        Identifer,
        /// <summary>
        /// 可変長引数
        /// </summary>
        VariableArguments,
        /// <summary>
        /// coresume文
        /// </summary>
        CoroutineResume,
        /// <summary>
        /// 配列
        /// </summary>
        Array,
        /// <summary>
        /// ラムダ式
        /// </summary>
        Lambda,
    }

    /// <summary>
    /// 演算子の種類を定義します。
    /// </summary>
    public enum StellarRoboOperatorType
    {
        /// <summary>
        /// 未定義
        /// </summary>
        Undefined,
        /// <summary>
        /// +
        /// </summary>
        Plus,
        /// <summary>
        /// -
        /// </summary>
        Minus,
        /// <summary>
        /// *
        /// </summary>
        Multiply,
        /// <summary>
        /// /
        /// </summary>
        Divide,
        /// <summary>
        /// &amp;
        /// </summary>
        And,
        /// <summary>
        /// |
        /// </summary>
        Or,
        /// <summary>
        /// !
        /// </summary>
        Not,
        /// <summary>
        /// ^
        /// </summary>
        Xor,
        /// <summary>
        /// %
        /// </summary>
        Modular,
        /// <summary>
        /// =
        /// </summary>
        Assign,
        /// <summary>
        /// &lt;&lt;
        /// </summary>
        LeftBitShift,
        /// <summary>
        /// &gt;&gt;
        /// </summary>
        RightBitShift,
        /// <summary>
        /// ==
        /// </summary>
        Equal,
        /// <summary>
        /// !=
        /// </summary>
        NotEqual,
        /// <summary>
        /// &gt;
        /// </summary>
        Greater,
        /// <summary>
        /// &lt;
        /// </summary>
        Lesser,
        /// <summary>
        /// &gt;=
        /// </summary>
        GreaterEqual,
        /// <summary>
        /// &lt;=
        /// </summary>
        LesserEqual,
        /// <summary>
        /// ~=
        /// </summary>
        SpecialEqual,
        /// <summary>
        /// &amp;&amp;
        /// </summary>
        AndAlso,
        /// <summary>
        /// ||
        /// </summary>
        OrElse,
        /// <summary>
        /// +=
        /// </summary>
        PlusAssign,
        /// <summary>
        /// -=
        /// </summary>
        MinusAssign,
        /// <summary>
        /// *=
        /// </summary>
        MultiplyAssign,
        /// <summary>
        /// /=
        /// </summary>
        DivideAssign,
        /// <summary>
        /// &amp;=
        /// </summary>
        AndAssign,
        /// <summary>
        /// |=
        /// </summary>
        OrAssign,
        /// <summary>
        /// ^=
        /// </summary>
        XorAssign,
        /// <summary>
        /// %=
        /// </summary>
        ModularAssign,
        /// <summary>
        /// &lt;&lt;=
        /// </summary>
        LeftBitShiftAssign,
        /// <summary>
        /// &gt;&gt;=
        /// </summary>
        RightBitShiftAssign,
        /// <summary>
        /// ++
        /// </summary>
        Increment,
        /// <summary>
        /// --
        /// </summary>
        Decrement,
        /// <summary>
        /// ?
        /// </summary>
        ConditionalQuestion,
        /// <summary>
        /// :
        /// </summary>
        ConditionalElse,
        /// <summary>
        /// ||=
        /// </summary>
        NilAssign,

        /// <summary>
        /// .
        /// </summary>
        MemberAccess,
        /// <summary>
        /// ()
        /// </summary>
        FunctionCall,
        /// <summary>
        /// []
        /// </summary>
        IndexerAccess,
    }
}
