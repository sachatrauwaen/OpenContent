﻿/*globals jQuery, window, Sys */
(function ($, Sys) {
    var OpenContent = function () {
        return {
            version: { major: 2, minor: 1, patch: 1 }
        };
    };
    $.fn.openContent = function (options) {
        return OpenContent();
    };
    $.fn.openContent.printLogs = function (title, logs) {
        if (window.console) {
            console.group(title);
            for (var i in logs) {
                console.group(i);
                for (var j = 0; j < logs[i].length; j++) {
                    console.log(logs[i][j].label, logs[i][j].message);
                }
                console.groupEnd();
            }
            console.groupEnd();
        }
    };
    /*
    var myString = $(this).closest("div[class*='DnnModule-']").attr('class');
    var myRegexp = /DnnModule-(\d+)/g;
    var match = myRegexp.exec(myString);
    alert(match[1]);  // abc
    */
    $(document).ready(function () {
        $(document).trigger("opencontent.ready", document);
        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function (sender, args) {
            var sName = args.get_response().get_webRequest().get_userContext();
            var div = $("#" + sName);
            $(document).trigger("opencontent.change", div);
        });
        Sys.WebForms.PageRequestManager.getInstance().add_beginRequest(function (sender, args) {
            var sName = args.get_postBackElement().id;
            args.get_request().set_userContext(args.get_postBackElement().id);
        });
    });
}(jQuery, window.Sys));
