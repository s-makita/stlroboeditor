<template>
<pre id="texteditor" class="h-100 m-3 border-left container-fluid tab-pane active" contenteditable="true" style="outline: none;">{{job}}</pre>
</template>

<script>
module.exports = {
    name: 'texteditor',
    props: {
        job: {
            type: String,
            default: "func main\r\n\tleft_down(27, 886)\r\n\twait(125)\r\n\tleft_up(26, 886)\r\n\twait(1140)\r\n\tleft_down(27, 884)\r\n\twait(125)\r\n\tleft_up(27, 884)\r\nendfunc"
        }
    }
};
</script>