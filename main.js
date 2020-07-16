const express = require("express");
const exapp = express();
const fs = require("fs");
const path = require('path');
const walkdir = require('walkdir');
const child_process = require('child_process');
const log = require('electron-log');
const ExplorerItem = require("./logic/ExplorerItem");

exapp.use(express.static("./"));
exapp.use(express.urlencoded({extended: true}));
/**
 * ツリー情報を取得する。
 */
exapp.post('/api/dir/scan',(req,res) => {
    // ルートディレクトリのパスを作成。
    var rootPath = "." + req.body.path;
    // ファイル（またはフォルダ）の情報を取得
    var stats = fs.statSync(rootPath);
    // ファイルの場合処理を中断する。
    if((stats.isFile())){
        return;
    }
    // ルートディレクトリ配下のアイテムを取得
    var subItems = scanDir(rootPath);
    // フォルダなので拡張子は無い。
    var ext = null;
    var item = new ExplorerItem(rootPath,rootPath,null,null,stats.isFile(),ext,subItems);
    res.json(item);
});
exapp.listen(3000, "127.0.0.1");

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


const { app, BrowserWindow, ipcMain } = require('electron');
var windows = [];
function createWindow(url){
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
    window.loadURL(url);
//    run();
    return window;
}

function scanDir(targetPath){
    var result = [];
    // 指定フォルダ内のファイル、サブフォルダを取得
    var subitems = fs.readdirSync(targetPath);
    subitems.forEach(function(itemName){
        // フルパスを取得
        var fullPath = path.join(targetPath, itemName);
        // ファイル（またはフォルダ）の情報を取得
        var stats = fs.statSync(fullPath);
        // フォルダの場合、サブアイテム情報を格納する入れ物
        var items = null;
        // フォルダの場合
        if(stats.isDirectory()){
            // 再帰呼び出し
            items = scanDir(fullPath);
        }
        var ext = null;
        // ファイルの場合
        if(stats.isFile()){
            ext = path.extname(fullPath);
        }
        // サブアイテムの情報
        var item = new ExplorerItem(itemName,fullPath,null,null,stats.isFile(),ext,items);
        // サブアイテムの情報を追加
        result.push(item);
    });
    return result;
}

// アプリ起動時
app.on('ready', function() {
    windows.push(createWindow('http://localhost:3000/index.html'));
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
