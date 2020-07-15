using System.Collections.Generic;

namespace StellarRobo.Analyze
{
    /// <summary>
    /// StellarRoboLexerの解析結果を定義します。
    /// </summary>
    public sealed class StellarRoboLexResult
    {
        /// <summary>
        /// 解析元のソースコードの名前を取得します。
        /// ファイルから解析した場合はデフォルトでファイル名になります。
        /// </summary>
        public string SourceName { get; }

        /// <summary>
        /// 字句解析が成功した場合はtrueになります。
        /// </summary>
        public bool Success { get; internal set; } = false;

        /// <summary>
        /// 解析が失敗した場合のエラー情報を取得します。
        /// </summary>
        public StellarRoboError Error { get; internal set; } = null;

        private List<StellarRoboToken> tokens = new List<StellarRoboToken>();
        /// <summary>
        /// 解析したトークンを取得します。
        /// </summary>
        /// <remarks>失敗した場合でも解析できたポイントまでのトークンが格納されます。</remarks>
        public IReadOnlyList<StellarRoboToken> Tokens { get; }

        /// <summary>
        /// ソース名を指定して初期化します。
        /// </summary>
        /// <param name="name"></param>
        public StellarRoboLexResult(string name)
        {
            SourceName = name;
            Tokens = tokens;
        }

        /// <summary>
        /// トークンを追加します。
        /// </summary>
        /// <param name="token">トークン</param>
        internal void AddToken(StellarRoboToken token) => tokens.Add(token);
        public void SetError(int Column,int Line, string Message)
        {
            if (this.Error == null) { this.Error = new StellarRoboError(); }
            this.Error.Column = Column;
            this.Error.Line = Line;
            this.Error.Message = Message;
        }
    }

    /// <summary>
    /// 字句解析のエラーを定義します。
    /// </summary>
    public sealed class StellarRoboError
    {
        /// <summary>
        /// 列位置を取得します。
        /// </summary>
        public int Column { get; internal set; }

        /// <summary>
        /// 行位置を取得します。
        /// </summary>
        public int Line { get; internal set; }

        /// <summary>
        /// エラーメッセージを取得します。
        /// </summary>
        public string Message { get; internal set; }
    }
}
