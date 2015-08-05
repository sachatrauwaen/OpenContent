var oc_moduleRoot = dnn.getVar('oc_moduleRoot');

var opencontent = {
    fields: {
        "address": {
            modules : [""]
        }
    }
};

require.config({
    baseUrl : oc_moduleRoot,
    paths: {
        'async': 'js/requirejs/async',
        'text': 'js/requirejs/text',
        'css': 'js/requirejs/css',
        alpacafields: 'alpaca/js/fields/dnn',
        'cropper':'js/cropper/cropper'
    },
    shim: {


    }
});

define('jquery', [], function() {
    return jQuery;
});

define('gmaps', ['async!http://maps.google.com/maps/api/js?v=3&sensor=false'],
    function () {
        // return the gmaps namespace for brevity
        return window.google.maps;
    });

define('gmaps_places', ['async!https://maps.googleapis.com/maps/api/js?v=3.exp&signed_in=true&libraries=places'],
    function () {
        // return the gmaps namespace for brevity
        return window.google.maps;
    });


define('addressfield', ['gmaps_places', 'alpacafields/AddressField'],
    function () {
        return Alpaca.Fields.AddressField;
    });

define('imagecropperfield', ['css!cropper', 'cropper', 'alpacafields/ImageCropperField'],
    function () {
        return Alpaca.Fields.ImageCropperField;
    });



    