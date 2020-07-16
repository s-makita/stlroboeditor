<template>
    <li class="nav-item has-treeview">
        <a href="#" class="nav-link">
            <i v-bind:class="['fas',item.isFile ? 'fa-robot' : 'fa-folder']"></i>
            <p>{{item.name}}</p>
            <i v-show="!item.isFile" class="right fas fa-angle-left"></i>
        </a>
        <div
            v-if="!item.isFile"
            class="nav nav-pills nav-treeview nav-sidebar flex-column"
            data-widget="treeview"
            role="menu"
            data-accordion="false"
        >
          <ul
            class="nav nav-pills nav-sidebar flex-column menu-open"
            data-widget="treeview"
            role="menu"
            data-accordion="false"
          >
            <explorer
              v-for="(child, index) in item.items"
              :key="index"
              :item="child"
            ></explorer>
          </ul>
        </div>
    </li>
</template>

<script>
module.exports = {
  name: 'explorer',
  props: {
    item: Object,
  },
  methods: {
    /**
     * イベント発火：ファイル選択 Event.Explorer.selected
     * UIから発火する。
     */
    onselected: function (event){
      // this._selected = event.target.getAttribute('data-path');
      // console.log('fugafugafuga');
    }
  }
};
</script>