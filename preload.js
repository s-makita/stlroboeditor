process.once('loaded', () => {
  // console.log('---- preload.js loaded ----');
  global.path = require('path');
  global.process = process;
  global.ipc = require('electron');
  global.require = require('electron').remote.require;
  global.log = require('electron-log');
  global.config = new (require('electron-store'))({
    // cwd: __dirname,  // 保存ディレクトリを指定　※省略可。推奨されていない
    // name: 'config'  // 設定ファイル名を指定　※省略可。拡張子は.jsonになる
  });

  global.fs = require('fs');
  global.vue = require('./node_modules/vue/dist/vue.js');
  global.httpVueLoader = require('./node_modules/http-vue-loader');
  // global.componentdirpath = './Resource/Components/';
  // global.vue.registcomponent = function(name,encode='utf8',dirpath = global.componentdirpath){
  //   var component = require(`${dirpath}${name}/${name}.js`);
  //   component.template = fs.readFileSync(`${dirpath}${name}/${name}.html`, encode);
  //   global.vue.component(name, window.vue.extend( component ) );
  // }
  // global.explorer = global.vue.component("explorer" , require('./Components/explorer.vue'));
  global.scheduler = require("node-schedule");
  global.events = require('events');
  global.console = require('electron').remote.require('console');
  global.request = require("request");
  // global.jobrunner = new JobRunner();

  global.nodeRequire = require;
  delete global.require;
  delete global.exports;
  delete global.module;


});
