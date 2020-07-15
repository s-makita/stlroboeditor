module.exports = {
  // template: window.fs.readFileSync('./Resource/Components/explorer.html', 'utf8'),
  props: {
    path: {
      type: String,
      default: ''
    }
  },
  computed: {
    items: function(){
      return this.read();
    }
  },
  methods: {
    /**
     * イベント発火：ファイル選択 Event.Explorer.selected
     * UIから発火する。
     */
    onselected(event){
      this._selected = event.target.getAttribute('data-path');
      console.log('fugafugafuga');
    },
    ontoggle(event) {
      $(e.target).tab('show');
      console.log('hogehogehoge');
      if (!this.isFolder) {
          return;
      }
      if (this.isOpen) {
      } else {
      }
      this.isOpen = !this.isOpen;
      this.tree();
    },
    /**
     * ツリー情報を取得する。
     */
    read(path = null){
      if(path) {
          this.path = path;
      }
      if(this.path == null) {
          return;
      }
      var root = this.path;
      console.log('hoge is: ' + this.path);
      const filenames = window.fs.readdirSync(this.path);
      var tree = [];
      filenames.forEach((filename) => {
          const fullPath = window.path.join(root, filename);
          const stats = window.fs.statSync(fullPath);
          console.log(fullPath);
          tree.push(new class{
            constructor(){
                this.name = filename;
                this.path = fullPath;
                this.stats = stats;
                this.isFile = stats.isFile();
                this.isOpen = false;
            }
          });
      });
      tree.sort(function(a,b){
          if(a.isFile){
              return 1;
          }
          return -1;
      })
      return tree;
    }
  }
};
