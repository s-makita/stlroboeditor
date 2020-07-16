<template>
<pre style="font-size: 10px; white-space: pre-wrap ;"><code>{{trace}}</code></pre>
</template>

<script>
module.exports = {
    name: 'stacktrace',
    props: {
        trace: {
            type: String,
            default: ""
        }
    }
};
</script>