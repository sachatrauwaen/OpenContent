requirejs.config({
    //By default load any module IDs from js/lib
    baseUrl: 'js',
    //except, if the module ID starts with "app",
    //load it from the js/app directory. paths
    //config is relative to the baseUrl, and
    //never includes a ".js" extension since
    //the paths config could be for a directory.
    paths: {
        app: 'app',
        jquery: '../../../../Resources/libraries/jQuery/01_09_01/jquery',
        handlebars: '../../OpenContent/js/alpaca-1.5.8/lib/handlebars/handlebars',
        bootstrap: 'https://maxcdn.bootstrapcdn.com/bootstrap/3.3.4/js/bootstrap.min',
        alpaca: '../../OpenContent/js/alpaca-1.5.8/alpaca/bootstrap/alpaca',
        dnnalpaca: '../alpaca/js/views/dnnbootstrap',
        typeahead: 'alpaca-1.5.8/lib/typeahead.js/dist/typeahead.bundle.min',
        wysihtmltoolbar: 'wysihtml/wysihtml-toolbar',
        advancedopencontent: 'wysihtml/parser_rules/advanced_opencontent',
        imagefield: '../alpaca/js/fields/dnn/ImageField',
        urlfield: '../alpaca/js/fields/dnn/UrlField',
        ckeditorfield: '../alpaca/js/fields/dnn/CKEditorField',
        wysihtmlfield: '../alpaca/js/fields/dnn/wysihtmlField',
        jqueryfileupload: '../../../../Resources/Shared/Scripts/jquery/jquery.fileupload',
        jqueryui: '../../../../Resources/libraries/jQuery-UI/01_10_03/jquery-ui',
        jqueryiframetransport: '/Resources/Shared/Scripts/jquery/jquery.iframe-transport'
    },
    shim: {
        xjquery: {
            exports: 'jQuery',
        },
        handlebars: {
            exports: 'Handlebars',
        },
        bootstrap: { "deps": ['jquery'] },
        typeahead: { "deps": ['jquery'] },
        wysihtmlField: { "deps": ['jquery','wysihtmltoolbar', 'advancedopencontent'] },
        imagefield: { "deps": ['alpaca', 'typeahead', 'jqueryfileupload'] },
        jqueryfileupload: { "deps": ['jqueryiframetransport'] },
        urlfield: { "deps": ['jquery','typeahead'] },
        alpaca: {
            "deps": ['jquery', 'bootstrap']
        },
        dnnalpaca: {
            "deps": ['alpaca']
        },
        alpacaform : {
            "deps": ['dnnalpaca' /*, 'wysihtmlfield', 'imagefield', 'urlfield', 'ckeditorfield' */]
        }
    }
});

// Start the main app logic.

define(["jquery", "alpaca", "bootstrap", "alpacaform", "imagefield"], function ($) {


    //$('#content').html(JSON.stringify(window.frameElement.dnn));

    var dnn = window.frameElement.dnn;
    var a = require('alpacaform');
    a.createform(dnn);


});

