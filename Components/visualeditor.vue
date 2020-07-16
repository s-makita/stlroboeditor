<template>
    <div id="visualeditor" class="container-fluid tab-pane h-100" style="z-index: 50; position: absolute; top: 0; right: 0;">
        <table style="height: 80%;width: 100%;">
            <tr>
            <td id="blocklyArea" style="position: absolute; top: 30px; left: 250px; width: 100%;"></td>
            </tr>
        </table>

        <!-- <div id="blocklyDiv" style="position: absolute"></div> -->
        <div id="blocklyDiv" style="position: absolute; top: 30px; left: 250px; width: 100%;"></div>
    </div>
</template>

<script>
module.exports = {
    name: 'visualeditor',
    props: {
        job: {
            type: String,
            default: "func main\r\n\tleft_down(27, 886)\r\n\twait(125)\r\n\tleft_up(26, 886)\r\n\twait(1140)\r\n\tleft_down(27, 884)\r\n\twait(125)\r\n\tleft_up(27, 884)\r\nendfunc"
        }
    }
};
</script>