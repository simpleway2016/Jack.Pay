function KeyboardListener() {
    var self = this;
    var textbox = null;

    this.onReceive = null;    

    this.start = function () {
        if (textbox == null) {
            textbox = document.createElement("INPUT");
            textbox.type = "text";
            textbox.style.imeMode = "disabled";
            textbox.style.opacity = "0";
            document.body.appendChild(textbox);
            textbox.onblur = function () {
                textbox.focus();
            }
            textbox.onkeypress = function (e) {
                if (e.keyCode == 13) {
                    //条码接收完毕，触发onReceive事件
                    if (self.onReceive && textbox.value.length > 0) {
                        self.onReceive(textbox.value);
                    }
                    if (textbox) {
                        textbox.value = "";
                    }
                }
            }
            textbox.focus();
        }
    }

    this.stop = function () {
        if (textbox) {
            textbox.onblur = null;
            textbox.onkeypress = null;
            document.body.removeChild(textbox);
            textbox = null;
        }
    }

}