using System.Collections.Generic;
using System.Linq;

namespace StellarRobo.Analyze
{
    /// <summary>
    /// use文のノードを定義します。
    /// </summary>
    public sealed class StellarRoboUseAstNode : StellarRoboAstNode
    {
        /// <summary>
        /// 対象を取得します。
        /// </summary>
        public string Target { get; internal set; }

        /// <summary>
        /// 新しいインスタンスを初期化します。
        /// </summary>
        public StellarRoboUseAstNode()
        {
            Type = StellarRoboAstNodeType.Use;
        }
    }
    /// <summary>
    /// クラスのノードです。
    /// </summary>
    public class StellarRoboClassAstNode : StellarRoboAstNode
    {
        /// <summary>
        /// クラス名を取得します。
        /// </summary>
        public string Name { get; protected internal set; } = "";

        private List<StellarRoboFunctionAstNode> funcs = new List<StellarRoboFunctionAstNode>();
        /// <summary>
        /// メソッドのノードのリストを取得します。
        /// </summary>
        public IReadOnlyList<StellarRoboFunctionAstNode> Functions { get; }

        private List<StellarRoboLocalAstNode> locals = new List<StellarRoboLocalAstNode>();
        /// <summary>
        /// local宣言のノードのリストを取得します。
        /// </summary>
        public IReadOnlyList<StellarRoboLocalAstNode> Locals { get; }

        /// <summary>
        /// インスタンスを初期化します。
        /// </summary>
        public StellarRoboClassAstNode()
        {
            Functions = funcs;
            Locals = locals;
            Type = StellarRoboAstNodeType.Class;
        }

        /// <summary>
        /// メソッドのノードを追加します。
        /// </summary>
        /// <param name="node">メソッドノード</param>
        protected internal void AddFunctionNode(StellarRoboFunctionAstNode node)
        {
            funcs.Add(node);
            AddNode(node);
        }

        /// <summary>
        /// local宣言のノードを追加します。
        /// </summary>
        /// <param name="node">localノード</param>
        protected internal void AddLocalNode(IEnumerable<StellarRoboLocalAstNode> node)
        {
            locals.AddRange(node);
            AddNode(node);
        }

        /// <summary>
        /// 現在のオブジェクトを表す文字列を返します。
        /// </summary>
        /// <returns>文字列</returns>
        public override IReadOnlyList<string> ToDebugStringList()
        {
            var result = new List<string>();
            result.Add($"Class: {Name}");
            result.Add("| [Locals]");
            foreach (var i in Locals) result.AddRange(i.ToDebugStringList().Select(p => "| " + p));
            result.Add("| [Methods]");
            foreach (var i in Functions) result.AddRange(i.ToDebugStringList().Select(p => "| " + p));
            return result;
        }
    }

    /// <summary>
    /// メソッドのノードです。
    /// </summary>
    public class StellarRoboFunctionAstNode : StellarRoboAstNode
    {
        /// <summary>
        /// メソッド名を取得します。
        /// </summary>
        public string Name { get; protected internal set; } = "";

        /// <summary>
        /// 仮引数リストを取得します。
        /// </summary>
        public IList<string> Parameters { get; } = new List<string>();

        /// <summary>
        /// このメソッドが可変長引数であるかどうかを取得します。
        /// </summary>
        public bool AllowsVariableArguments { get; protected internal set; }

        /// <summary>
        /// このメソッドがクラスメソッドであるかどうかを取得します。
        /// </summary>
        public bool StaticMethod { get; protected internal set; }

        /// <summary>
        /// インスタンスを初期化します。
        /// </summary>
        protected internal StellarRoboFunctionAstNode()
        {
            Type = StellarRoboAstNodeType.Function;
        }

        /// <summary>
        /// 現在のオブジェクトを表す文字列を返します。
        /// </summary>
        /// <returns>文字列</returns>
        public override IReadOnlyList<string> ToDebugStringList()
        {
            var result = new List<string>();
            result.Add($"Method: {Name}");
            if (Parameters.Count > 0)
            {
                result.Add("| [Parameters]");
                foreach (var i in Parameters) result.Add($"| {i.ToString()}");
            }
            result.Add("| [Block]");
            foreach (var i in Children) result.AddRange(i.ToDebugStringList().Select(p => "| " + p));
            return result;
        }
    }

    /// <summary>
    /// local宣言のノードです。
    /// </summary>
    public class StellarRoboLocalAstNode : StellarRoboAstNode
    {
        /// <summary>
        /// 変数名を取得します。
        /// </summary>
        public string Name { get; protected internal set; } = "";

        /// <summary>
        /// 初期値定義がある場合はその式のノードが格納されます。
        /// </summary>
        public StellarRoboExpressionAstNode InitialExpression { get; protected internal set; }

        /// <summary>
        /// インスタンスを初期化します。
        /// </summary>
        protected internal StellarRoboLocalAstNode()
        {
            Type = StellarRoboAstNodeType.LocalStatement;
        }

        /// <summary>
        /// 現在のオブジェクトを表す文字列を返します。
        /// </summary>
        /// <returns>文字列</returns>
        public override IReadOnlyList<string> ToDebugStringList()
        {
            var result = new List<string>();
            result.Add($"Local Assign: \"{Name}\"");
            if (InitialExpression != null)
            {
                result.Add("| [Initial Expression]");
                result.AddRange(InitialExpression.ToDebugStringList().Select(p => "| " + p));
            }
            return result;
        }
    }

    /// <summary>
    /// return/yield文のノードです。
    /// </summary>
    public class StellarRoboReturnAstNode : StellarRoboAstNode
    {
        /// <summary>
        /// 返り値がある場合はその式のノードが格納されます。
        /// </summary>
        public StellarRoboExpressionAstNode Value { get; protected internal set; }

        /// <summary>
        /// インスタンスを初期化します。
        /// </summary>
        protected internal StellarRoboReturnAstNode()
        {
            Type = StellarRoboAstNodeType.ReturnStatement;
        }

        /// <summary>
        /// 現在のオブジェクトを表す文字列を返します。
        /// </summary>
        /// <returns>文字列</returns>
        public override IReadOnlyList<string> ToDebugStringList()
        {
            var result = new List<string>();
            result.Add("Return");
            if (Value != null)
            {
                result.Add("| [Expression]");
                result.AddRange(Value.ToDebugStringList().Select(p => "| " + p));
            }
            return result;
        }
    }

    /// <summary>
    /// local宣言のノードです。
    /// </summary>
    public class StellarRoboCoroutineDeclareAstNode : StellarRoboAstNode
    {
        /// <summary>
        /// 変数名を取得します。
        /// </summary>
        public string Name { get; protected internal set; } = "";

        /// <summary>
        /// コルーチンのノードを取得します。
        /// </summary>
        public StellarRoboExpressionAstNode InitialExpression { get; protected internal set; }

        /// <summary>
        /// 引数がある場合はその式のノードが格納されます。
        /// </summary>
        public IList<StellarRoboExpressionAstNode> ParameterExpressions { get; } = new List<StellarRoboExpressionAstNode>();

        /// <summary>
        /// インスタンスを初期化します。
        /// </summary>
        protected internal StellarRoboCoroutineDeclareAstNode()
        {
            Type = StellarRoboAstNodeType.CoroutineStatement;
        }

        /// <summary>
        /// 現在のオブジェクトを表す文字列を返します。
        /// </summary>
        /// <returns>文字列</returns>
        public override IReadOnlyList<string> ToDebugStringList()
        {
            var result = new List<string>();
            result.Add($"Coroutine Declare: \"{Name}\"");
            if (InitialExpression != null)
            {
                result.Add("| [Expression]");
                result.AddRange(InitialExpression.ToDebugStringList().Select(p => "| " + p));
            }
            return result;
        }
    }

    /// <summary>
    /// if文のノードです。
    /// </summary>
    public class StellarRoboIfAstNode : StellarRoboAstNode
    {
        /// <summary>
        /// ifブロックのノードを取得します。
        /// </summary>
        public StellarRoboIfBlockAstNode IfBlock { get; protected internal set; }

        /// <summary>
        /// elifブロックのノードのリストを取得します。
        /// </summary>
        public IList<StellarRoboIfBlockAstNode> ElifBlocks { get; protected internal set; } = new List<StellarRoboIfBlockAstNode>();

        /// <summary>
        /// elseブロックのノードを取得します。
        /// </summary>
        public StellarRoboIfBlockAstNode ElseBlock { get; protected internal set; }

        /// <summary>
        /// 現在のオブジェクトを表す文字列を返します。
        /// </summary>
        /// <returns>文字列</returns>
        public override IReadOnlyList<string> ToDebugStringList()
        {
            var result = new List<string>();
            result.Add("If Statement");
            result.Add("| [If Block]");
            result.AddRange(IfBlock.ToDebugStringList().Select(p => "| " + p));
            foreach (var i in ElifBlocks)
            {
                result.Add("| [Elif Block]");
                result.AddRange(i.ToDebugStringList().Select(p => "| " + p));
            }
            if (ElseBlock != null)
            {
                result.Add("| [Else Block]");
                result.AddRange(IfBlock.ToDebugStringList().Select(p => "| " + p));
            }
            return result;
        }
    }

    /// <summary>
    /// if文内の各ブロックのノード(if/elif/else)です。
    /// 実行内容は<see cref="StellarRoboAstNode.Children"/>に格納されます。
    /// </summary>
    public class StellarRoboIfBlockAstNode : StellarRoboAstNode
    {
        /// <summary>
        /// 条件式のノードを取得します。
        /// <see cref="StellarRoboIfAstNode.ElseBlock"/>に格納される場合は利用されません。
        /// </summary>
        public StellarRoboExpressionAstNode Condition { get; protected internal set; }

        /// <summary>
        /// 現在のオブジェクトを表す文字列を返します。
        /// </summary>
        /// <returns>文字列</returns>
        public override IReadOnlyList<string> ToDebugStringList()
        {
            var result = new List<string>();
            result.Add("If Block");
            result.Add("| [Condition]");
            result.AddRange(Condition.ToDebugStringList().Select(p => "| " + p));
            result.Add("| [Block]");
            foreach (var i in Children)
            {
                result.AddRange(i.ToDebugStringList().Select(p => "| " + p));
            }
            return result;
        }
    }

    /// <summary>
    /// 繰り返し文(前判断)
    /// </summary>
    public class StellarRoboLoopAstNode : StellarRoboAstNode
    {
        /// <summary>
        /// 条件式を取得します。
        /// </summary>
        public StellarRoboExpressionAstNode Condition { get; protected internal set; }

        /// <summary>
        /// このループの名前を取得します。
        /// </summary>
        public string Name { get; protected internal set; }

        /// <summary>
        /// 
        /// </summary>
        public StellarRoboLoopAstNode()
        {
            Type = StellarRoboAstNodeType.WhileStatement;
        }
    }

    /// <summary>
    /// foreach文だよ
    /// </summary>
    public class StellarRoboForeachAstNode : StellarRoboLoopAstNode
    {
        /// <summary>
        /// ソースの式を取得します。
        /// </summary>
        public StellarRoboExpressionAstNode Source { get; protected internal set; }

        /// <summary>
        /// コルーチンforeachのコルーチン引数を取得します。
        /// </summary>
        public IList<StellarRoboExpressionAstNode> CoroutineArguments { get; } = new List<StellarRoboExpressionAstNode>();

        /// <summary>
        /// 要素を格納する変数名を取得します。
        /// </summary>
        public string ElementVariableName { get; protected internal set; } = "";

        /// <summary>
        /// foreach ofのコルーチンforeachならtrueです
        /// </summary>
        public bool IsCoroutineSource { get; protected internal set; }

        /// <summary>
        /// 
        /// </summary>
        public StellarRoboForeachAstNode()
        {
            Type = StellarRoboAstNodeType.ForeachStatement;
        }
    }

    /// <summary>
    /// for文です
    /// </summary>
    public class StellarRoboForAstNode : StellarRoboLoopAstNode
    {
        /// <summary>
        /// 初期化式を取得します。
        /// </summary>
        public IList<StellarRoboExpressionAstNode> InitializeExpressions { get; } = new List<StellarRoboExpressionAstNode>();

        /// <summary>
        /// カウンタ操作の式を取得します。
        /// </summary>
        public IList<StellarRoboExpressionAstNode> CounterExpressions { get; } = new List<StellarRoboExpressionAstNode>();

        /// <summary>
        /// 
        /// </summary>
        public StellarRoboForAstNode()
        {
            Type = StellarRoboAstNodeType.ForStatement;
        }
    }

    /// <summary>
    /// continue文
    /// </summary>
    public class StellarRoboContinueAstNode : StellarRoboAstNode
    {
        /// <summary>
        /// ジャンプ先ラベル名を取得します。
        /// </summary>
        public string Label { get; protected internal set; }

        /// <summary>
        /// 
        /// </summary>
        public StellarRoboContinueAstNode()
        {
            Type = StellarRoboAstNodeType.ContinueStatement;
        }
    }
}