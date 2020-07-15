$('#play').on('click',(e)=>{
    path = '.\\StellarRobo\\StellarRoboEditor\\StellarRoboEditor\\bin\\Release\\StellarRoboEditor.exe ';
//    param = "func main\\r\\n\\tleft_down(27, 886)\\r\\n\\twait(125)\\r\\n\\tleft_up(26, 886)\\r\\n\\twait(1140)\\r\\n\\tleft_down(27, 884)\\r\\n\\twait(125)\\r\\n\\tleft_up(27, 884)\\r\\nendfunc";
    param = document.querySelector("#editor").innerHTML;
    param = param.replace(/\r?\n/g, '\\r\\n')
    run(path,param);
});
