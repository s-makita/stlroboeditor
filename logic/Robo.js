/**
 * ロボ自体。コンストラクタでパスを指定した場合はファイルから読み取り。
 */
module.exports = class Robo {
    constructor() {
        // ロボのパス
        this.path;
        // ロボの処理内容
        this.program;
        // 変更したか
        this.edited;
    }

    /**
     * 保存
     */
    save() {
        // 保存後のイベント
        // Event.Robo.save
    }

    /**
     * 削除
     */
    remove() {
        // 削除後のイベント
        // Event.Robo.remove
    }

    /**
     * 名称変更
     */
    rename() {
        // 名称変更後のイベント
        // Event.Robo.rename
    }

    /**
     * 複製
     */
    copy() {
        // 複製後のイベント
        // Event.Robo.copy
    }
}
