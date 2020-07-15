/**
 * メインクラス
 */
module.exports = class App {
    constructor() {
        // 設定への参照
        this.config;
        // エンジンのインスタンスへの参照
        this.engine;
        // オブザーバーのインスタンスへの参照
        this.observer;
        // 現在処理しているロボの参照
        this.current;
        // テキストエディタのインスタンスへの参照
        this.textEditor;
        // ブロックエディタのインスタンスへの参照
        this.blockEditor;
        // 現在選択中のエディタへの参照
        this.currentEditor;
    }

    /**
     * アプリ起動
     */
    boot() {
        // アプリ起動後のイベント
        // Event.App.boot
    }

    /**
     * 設定読込
     */
    init() {
        // 設定読込後のイベント
        // Event.App.init
    }

    /**
     * オブザーバー起動
     */
    bootObserver() {
        // オブザーバー起動後のイベント
        // Event.App.bootObserver
    }

    /**
     * エディタ変更
     */
    changeEditor() {
        // エディタ変更後のイベント
        // Event.App.changeEditor
    }
    /**
     * ロボの新規作成（未保存）
     */
    newRobo() {
        // ロボの新規作成（未保存）後のイベント
        // Event.App.newRobo
    }
    /**
     * ロボをデバッグ実行
     */
    startDebug(){
        // ロボをデバッグ実行後のイベント
        // Event.App.startDebug
    }
    /**
     * ロボをデバッグ停止
     */
    stopDebug(){
        // ロボをデバッグ停止後のイベント
        // Event.App.stopDebug
    }
    // スケジューラーのインスタンスへの参照	App.scheduler
    /**
     * スケジューラー起動
     */
    bootScheduler(){
        // スケジューラー起動後のイベント
        // Event.App.bootScheduler
    }
}