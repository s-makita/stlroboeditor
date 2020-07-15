/**
 * ロボ実行エンジン
 */
module.exports = class Engine {
    constructor() {
        /**
         * 現在参照しているファイルの情報
         */
        this.current;
        /**
         * 処理の状態
         */
        this.status;
    }
    /**
     * ロボの処理を開始
     */
    startRun() {
        // ロボの処理を開始後のイベント
        // Event.Enjine.startRun
    }
    /**
     * ロボの処理を一時停止
     */
    pauseRun() {
        // ロボの処理を一時停止後のイベント
        // Event.Enjine.pauseRun
    }
    /**
     * ロボの処理を停止
     */
    stopRun() {
        // ロボの処理を停止後のイベント
        // Event.Enjine.stopRun
    }
    /**
     * ロボの処理を終了
     */
    finishRun() {
        // ロボの処理を終了後のイベント
        // Event.Enjine.finishRun
    }
    /**
     * レコーディング開始
     */
    startRecording() {
        // レコーディング開始後のイベント
        // Event.Enjine.startRecording
    }
    /**
     * レコーディング終了
     */
    stopRecording() {
        // レコーディング終了後のイベント
        // Event.Enjine.stopRecording
    }
    /**
     * ログの取得
     */
    getLog() {
        // ログの取得後のイベント
        // Event.Enjine.getLog
    }
}