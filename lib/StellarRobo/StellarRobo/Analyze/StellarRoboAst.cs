using System.Collections.Generic;

namespace StellarRobo.Analyze
{
    /// <summary>
    /// StellarRoboのAST(抽象構文木)を提供します。
    /// </summary>
    public sealed class StellarRoboAst
    {
        /// <summary>
        /// <see cref="StellarRoboLexResult.SourceName"/>から継承されたソース名を取得します。
        /// </summary>
        public string SourceName { get; }

        /// <summary>
        /// 構文解析が成功した場合はtrueになります。
        /// </summary>
        public bool Success { get; internal set; } = false;

        /// <summary>
        /// 解析が失敗した場合のエラー情報を取得します。
        /// </summary>
        public StellarRoboError Error { get; internal set; } = null;

        /// <summary>
        /// ASTのルートを取得します。
        /// </summary>
        public StellarRoboAstNode RootNode { get; internal set; }

        /// <summary>
        /// インスタンスを初期化します。
        /// </summary>
        /// <param name="name">ソース名</param>
        public StellarRoboAst(string name)
        {
            SourceName = name;
        }
    }

    /// <summary>
    /// ASTのノードを定義します。
    /// </summary>
    public class StellarRoboAstNode
    {
        /// <summary>
        /// このノードの種類を取得します。
        /// </summary>
        public StellarRoboAstNodeType Type { get; protected internal set; } = StellarRoboAstNodeType.Undefined;

        private List<StellarRoboAstNode> children = new List<StellarRoboAstNode>();

        /// <summary>
        /// 行番号
        /// </summary>
        public int Line { get; protected internal set; }
        /// <summary>
        /// 開始位置
        /// </summary>
        public int StartPos { get; protected internal set; }
        /// <summary>
        /// 終了位置
        /// </summary>
        public int EndPos { get; protected internal set; }

        /// <summary>
        /// 子ノードを取得します。
        /// </summary>
        public IReadOnlyList<StellarRoboAstNode> Children { get; protected internal set; }

        /// <summary>
        /// インスタンスを初期化します。
        /// </summary>
        public StellarRoboAstNode()
        {
            Children = children;
        }

        /// <summary>
        /// ノードを追加します。
        /// </summary>
        /// <param name="node">追加するノード</param>
        protected internal void AddNode(StellarRoboAstNode node) => children.Add(node);

        /// <summary>
        /// ノードを追加します。
        /// </summary>
        /// <param name="nodes">追加するノード</param>
        protected internal void AddNode(IEnumerable<StellarRoboAstNode> nodes) => children.AddRange(nodes);

        /// <summary>
        /// 現在のオブジェクトを表す文字列を返します。
        /// </summary>
        /// <returns>文字列</returns>
        public virtual IReadOnlyList<string> ToDebugStringList()
        {
            var result = new List<string>();
            foreach (var i in Children) result.AddRange(i.ToDebugStringList());
            return result;
        }
    }

    /// <summary>
    /// ASTのノードの種類を定義します。
    /// </summary>
    public enum StellarRoboAstNodeType
    {
        /// <summary>
        /// 未定義
        /// </summary>
        Undefined,
        /// <summary>
        /// use文
        /// </summary>
        Use,
        /// <summary>
        /// クラス
        /// </summary>
        Class,
        /// <summary>
        /// メソッド
        /// </summary>
        Function,
        /// <summary>
        /// ブロック
        /// </summary>
        StatementBlock,
        /// <summary>
        /// メソッド呼び出し
        /// </summary>
        CallStatement,
        /// <summary>
        /// ifブロック
        /// </summary>
        IfStatement,
        /// <summary>
        /// caseブロック
        /// </summary>
        CaseStatement,
        /// <summary>
        /// forブロック
        /// </summary>
        ForStatement,
        /// <summary>
        /// doブロック
        /// </summary>
        DoStatement,
        /// <summary>
        /// whileブロック
        /// </summary>
        WhileStatement,
        /// <summary>
        /// local文
        /// </summary>
        LocalStatement,
        /// <summary>
        /// coroutine文
        /// </summary>
        CoroutineStatement,
        /// <summary>
        /// return文
        /// </summary>
        ReturnStatement,
        /// <summary>
        /// yield文
        /// </summary>
        YieldStatement,
        /// <summary>
        /// continue文
        /// </summary>
        ContinueStatement,
        /// <summary>
        /// 式
        /// </summary>
        Expression,

        /// <summary>
        /// break文
        /// </summary>
        BreakStatement,

        /// <summary>
        /// foreach文
        /// </summary>
        ForeachStatement,
    }

}
