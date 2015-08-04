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
        alpacafields: 'alpaca/js/fields/dnn'
    },
    shim: {
    }
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
