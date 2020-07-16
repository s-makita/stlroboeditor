<template>
    <ul class="nav nav-pills" role="tablist">
    <li class="nav-item p-2">
        <a class="nav-link active" id="texteditor-tab" data-toggle="pill" href="#texteditor" role="pill" aria-controls="texteditor" aria-selected="true">テキスト</a>
    </li>
    <li class="nav-item mr-auto p-2">
        <a class="nav-link" id="visualeditor-tab" data-toggle="pill" href="#visualeditor" role="pill" aria-controls="visualeditor" aria-selected="false">ビジュアル</a>
    </li>
    <li class="p-2 mb-2"><button type="button" v-on:click="onplay" class="btn btn-light"><i class="fas fa-play pr-2"></i>実行</button></li>
    <li class="p-2 mb-2"><button type="button" v-on:click="onstop" class="btn btn-light disable"><i class="fas fa-stop pr-2"></i>停止</button></li>
    <li class="p-2 mb-2"><button type="button" v-on:click="onrecording" class="btn btn-light"><i class="fas fa-video pr-2"></i>録画</button></li>
    </ul>
</template>

<script>
module.exports = {
    name: 'toolbar',
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
    }
};
</script>