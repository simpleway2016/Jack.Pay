(function (window, undefined) {
    "use strict";

    function MyClass(id) {
        this.name = id;
    }

    MyClass.prototype = {
        constructor: MyClass,
        check: function () {
            alert(23);
        },
    };
    MyClass.check = function () {
        alert(777);
    }
    MyClass.check();
})(window);