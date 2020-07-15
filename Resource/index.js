// import { Stats } from "fs";
window.vue.registcomponent('explorer');

//設定ファイルにデータを保存する。（programsというキーでJSONデータを保存する）
// window.config.set("programs", { index: 1, name: 'TEST', path: 'c/' });

var App = new window.vue({
  el: '#app',
  props: {
    _job: {
      type: String,
      default: "func main\r\n\tleft_down(27, 886)\r\n\twait(125)\r\n\tleft_up(26, 886)\r\n\twait(1140)\r\n\tleft_down(27, 884)\r\n\twait(125)\r\n\tleft_up(27, 884)\r\nendfunc"
    },
    _workspace: {
      type: String,
      default: ''
    },
    _current: {
      type: String,
      default: ''
    },
  },
  methods: {
    onplay(event){
      Event.emit('App.onplay');
    },
    onstop(event){
      Event.emit('App.onstop');
    },
    onrecording(event){
      Event.emit('App.onrecording');
    }
  },
  computed: {
    job: {
      get(){
        return this._job;
      },
      set(value){
        this._job = value;
        // イベント発火：処理内容を変更
        Event.emit('App.job.set');
      }
    },
    workspace: {
      get(){
        return this._workspace;
      },
      set(value){
        this._workspace = value;
        // イベント発火：ワークスペースのパスを変更
        Event.emit('App.workspace.set');
      }
    },
    current: {
      get(){
        return this._current;
      },
      set(value){
        this._current = value;
        // イベント発火：現在のファイルのパスを変更
        Event.emit('App.current.set');
      }
    }
  }
});


/**
 * イベント
 * 当アプリ全体のイベントに関する実装。イベント発火のみを実装範囲とする。
 * イベント名の固定値を持つ。
 * 外部イベントを内部イベントへ変換する。
 * オブザーバーからの通知をイベントへ変換する。
 */
const Event = new window.events();
Event.on('App.onplay',function(){
  window.console.log(App._job);
  window.console.log('play');
  window.jobrunner.run(App.job);
});
Event.on('App.onstop',function(){
  window.console.log('stop');
});
Event.on('App.onrecording',function(){
  window.console.log('recording');
});
// イベント：処理内容を変更
Event.on('App.job.set',function(value){
  // Blockly 描画
  var blocklyArea = document.getElementById("blocklyArea");
  var blocklyDiv = document.getElementById("blocklyDiv");
  var demoWorkspace = Blockly.inject(blocklyDiv, {
    media: "../node_modules/blockly/media/",
    toolbox: document.getElementById("toolbox")
  });
  var onresize = function(e) {
    // Compute the absolute coordinates and dimensions of blocklyArea.
    var element = blocklyArea;
    var x = 0;
    var y = 0;
    do {
      x += element.offsetLeft;
      y += element.offsetTop;
      element = element.offsetParent;
    } while (element);
    // Position blocklyDiv over blocklyArea.
    blocklyDiv.style.left = x + "px";
    blocklyDiv.style.top = y + "px";
    blocklyDiv.style.width = blocklyArea.offsetWidth + "px";
    blocklyDiv.style.height = blocklyArea.offsetHeight + "px";
    Blockly.svgResize(demoWorkspace);
  };
  window.addEventListener("resize", onresize, false);
  onresize();
  Blockly.svgResize(demoWorkspace);
});

// イベント：ワークスペースのパスを変更
Event.on('App.workspace.set',function(){
  App.current = null;
  if(App.workspace == null) {
    return;
  }
  // 内容をエクスプローラーに表示する。
  // Explorer.bind(App.workspace);
});

// イベント：現在のファイルのパスを変更
Event.on('App.current.set',function(){
  if(App.current == null) {
    return;
  }
  // 変更されたパスのファイルを読み込み。
  window.fs.readFile(App.current, function(error, result) {
    // エラーだったらスローする。
    if (error != null) {
      throw error;
    }
    // 処理内容をファイルの内容に変更。
    App.job = result.toString();
  });
});

// イベント：ファイル選択 Event.Explorer.selected
Event.on('Explorer.file.onselected',function(){
  // 現在のファイルのパスを、選択されたファイルのパスに変更する。
  // App.current = Explorer.selected;
});
