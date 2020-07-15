const { app, BrowserWindow, ipcMain } = require('electron');

/**
 * スケジューラー
 */
const Scheduler = class {
    /**
     * スケジュールを登録する。
     * @param {*} date 指定の日時
     * @param {*} path 任意のファイル
     */
    scheduleJob(date,path){
      window.Scheduler.scheduleJob(date,function(path){
        // イベント発火：スケジュール実行
        Event.emit('Event.Scheduler.onschedule',path);
      });
    }
  };

const JobRunner = new class {
    /**
     * 処理をロボに依頼する。
     * @param {*} job 
     * @param {*} closure 
     */
    run(job,closure){
        // 処理内容を指定してロボを起動する。
        var exe_path = path.join(__dirname, '/lib/StellarRobo/StellarRoboEditor/StellarRoboEditor/bin/Release/StellarRoboEditor.exe ');
        var result = child_process.execSync(exe_path + ' "' + job + '"');
        if(closure){
            // 処理終了時、指定の処理を実行する。
            closure(result);
        }
    };
}

const path = require('path');
const child_process = require('child_process');
const log = require('electron-log');
var windows = [];
function createWindow(html_path){
    let window = new BrowserWindow({
        width: 1440,
        height: 900,
        webPreferences: {
            nodeIntegration: false,
            contextIsolation: false,
            preload: path.join(__dirname, '/preload.js'),
        }
    });
    window.on('closed', () => {
        window = null
    })
    window.openDevTools();
    window.loadFile(html_path);
//    run();
    return window;
}
// アプリ起動時
app.on('ready', function() {
    windows.push(createWindow('Resource/index.html'));
});
// アプリケーションがウィンドウをクローズし始める前
app.on('before-quit', function() {
    if (false) {
        // アプリケーション終了を止める。
        event.preventDefault();
    }
});
// 全てのウィンドウを閉じたとき
app.on('window-all-closed', function() {
    if (process.platform !== 'darwin') {
        app.quit();
    }
});
// アプリケーションが終了したとき
app.on('quit', function() {
});
