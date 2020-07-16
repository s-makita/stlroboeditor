/**
 * エクスプローラに表示するアイテム。階層構造で入れ子になる。
 */
module.exports = class ExplorerItem {
    constructor(name,path,stats,status,isFile,ext,items) {
        // 名前。コンストラクタで設定。
        this.name = name;
        // フルパス。コンストラクタで設定。
        this.path = path;
        // ファイルシステムから取得した情報。コンストラクタで設定。
        this.stats = stats;
        // ファイルか
        this.isFile = isFile;
        // 拡張子
        this.ext = ext;
        // 状態
        this.status = status;
        // 子階層
        this.items = items;
        // 開閉したか
        this.isOpen = false;
    }
    /*
    * 名前を取得
    */
    getName() {
        // 名前を取得後のイベント
        // Event.ExplorerItem.getName
    }
    /**
     * フルパスを取得
     */
    getPath() {
        // フルパスを取得後のイベント
        // Event.ExplorerItem.getPath
    }
    /**
     * ファイルシステムから取得した情報を取得
     */
    getStats(){
        // ファイルシステムから取得した情報を取得後のイベント
        // Event.ExplorerItem.getStats
    }
    /**
     * 状態を取得する
     */
    getStatus(){
        // 状態を取得後のイベント
        // Event.ExplorerItem.getStatus
    }
    /**
     * 状態を設定する
     */
    setStatus(){
        // 状態を設定後のイベント
        // Event.ExplorerItem.setStatus
    }
    /**
     * 表示にバインドする
     */
    bind(){
        // 表示にバインド後のイベント
        // Event.ExplorerItem.bind
    }
    /**
     * 見た目の開閉を開いたかの判断結果を取得
     */
    toggled(){
        // 見た目の開閉を開いたかの判断結果を取得後のイベント
        // Event.ExplorerItem.toggled
    }
    /**
     * 子階層を取得する
     */
    getItems(){
        // 子階層を取得後のイベント
        // Event.ExplorerItem.getItems
    }
    /**
     * 子階層を設定する
     */
    setItems(){
        // 子階層を設定後のイベント
        // Event.ExplorerItem.setItems
    }
    /**
     * 子階層をファイルシステムから読取
     */
    readItems(){
        // 子階層をファイルシステムから読取後のイベント
        // Event.ExplorerItem.readItems
    }
    /**
     * 子階層をリモートファイルシステムから読取
     */
    readItemsByRemote(){
        // 子階層をリモートファイルシステムから読取後のイベント
        // Event.ExplorerItem.readItemsByRemote
    }
    /**
     * 子階層をソート
     */
    sortItems(){
        // 子階層をソート後のイベント
        // Event.ExplorerItem.sortItems
    }
    /**
     * 子階層を表示にバインド
     */
    bindItems(){
        // 子階層を表示にバインド後のイベント
        // Event.ExplorerItem.bindItems
    }
}
