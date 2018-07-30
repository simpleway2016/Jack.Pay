function HttpClient() { 
    function createXMLHttp() {
        var request = false;
        if (window.XMLHttpRequest) {
            request = new XMLHttpRequest();
        }
        else if (window.ActiveXObject) {
            try {
                request = new ActiveXObject("Msxml2.XMLHTTP");
            }
            catch (e1) {
                try {
                    request = new ActiveXObject("Microsoft.XMLHTTP");
                }
                catch (e2) {
                    request = false;
                }
            }
        }
        return request;
    }

    this.async = true;
    this.xmlHttp = createXMLHttp();
    this.jsList = [];
}

HttpClient.prototype = {
    abort: function () {
        this.xmlHttp.abort();
    },

    setTimeout: function (millseconds) {
        this.xmlHttp.timeout = millseconds;
    },

    xmlHttpStatusChanged: function () {
        if (this.xmlHttp.readyState == 4) {
            if (this.xmlHttp.status == 200) {
                if (this.onCompleted) {
                    this.onCompleted(this.xmlHttp.responseText, null);
                }
            }
        }
    },

    postJson: function (url,obj) {
        var _this = this;

        this.xmlHttp.onreadystatechange = function () {
            return _this.xmlHttpStatusChanged();
        };

        this.xmlHttp.onerror = function (e) {
            if (_this.onCompleted) {
                _this.onCompleted(null, "无法连接服务器");
            }
        };

        this.xmlHttp.ontimeout = function () {
            if (_this.onCompleted) {
                _this.onCompleted(null, "连接服务器超时");
            }
        };
        this.xmlHttp.open("POST", url, this.async);
        this.xmlHttp.setRequestHeader("Content-Type", "application/json");
        this.xmlHttp.send(JSON.stringify(obj));
    },

    post: function (url, nameAndValues) {
        var _this = this;
        if (nameAndValues === void 0) {
            nameAndValues = null;
        }

        this.xmlHttp.onreadystatechange = function () {
            return _this.xmlHttpStatusChanged();
        };
        this.xmlHttp.onerror = function (e) {
            if (_this.onCompleted) {
                _this.onCompleted(null, e);
            }
        };
        this.xmlHttp.ontimeout = function () {
            if (_this.onCompleted) {
                _this.onCompleted(null, "连接服务器超时");
            }
        };
        var p = "";
        if (nameAndValues) {
            for (var i = 0; i < nameAndValues.length; i += 2) {
                if (i > 0)
                    p += "&";
                p += nameAndValues[i] + "=" + window.encodeURIComponent(nameAndValues[i + 1], "utf-8");
            }
        }

        this.xmlHttp.open("POST", url, this.async);
        this.xmlHttp.setRequestHeader("Content-Type","application/x-www-form-urlencoded");  
        this.xmlHttp.send(p);
    },

    linkJs: function (url) {
        if (this.jsList.indexOf(url) >= 0)
            return;
        this.jsList.push(url);
        this.async = false;
        var errcount = 0;
        var self = this;
        this.onCompleted = function (text, err) {
            if (!err) {
                document.writeln("<script lang='ja'>");
                document.writeln(text);
                document.write("</");
                document.writeln("script>");
            }
            else {
                //发生错误，重试2次
                errcount++;
                if (errcount < 2) {
                    self.download(url);
                }
            }
        }
        
        this.download(url);
        
    },

    download: function (url,nameAndValues) {
        var _this = this;
        if (nameAndValues === void 0) {
            nameAndValues = null;
        }

        this.xmlHttp.onreadystatechange = function () {
            return _this.xmlHttpStatusChanged();
        };
        this.xmlHttp.onerror = function (e) {
            if (_this.onCompleted) {
                _this.onCompleted(null, e);
            }
        };
        this.xmlHttp.ontimeout = function () {
            if (_this.onCompleted) {
                _this.onCompleted(null, "连接服务器超时");
            }
        };
        var p = "";
        if (nameAndValues) {
            for (var i = 0; i < nameAndValues.length; i += 2) {
                if (i > 0)
                    p += "&";
                p += nameAndValues[i] + "=" + window.encodeURIComponent(nameAndValues[i + 1], "utf-8");
            }
        }
        var myurl = url;
        if (nameAndValues && nameAndValues.length > 0) {
            if (myurl.indexOf("?") < 0)
                myurl += "?";
            else
                myurl += "&";
        }
        myurl += p;

        this.xmlHttp.open("GET", myurl, this.async);
        this.xmlHttp.send(null);
    },
};
