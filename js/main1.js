define(["jquery", "alpaca", "bootstrap", "alpacaform","imagefield"], function ($) {

   
    //$('#content').html(JSON.stringify(window.frameElement.dnn));

    var dnn = window.frameElement.dnn;
    var a = require('alpacaform');
    a.createform(dnn);


});