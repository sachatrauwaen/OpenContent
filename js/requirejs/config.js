var oc_moduleRoot = dnn.getVar('oc_websiteRoot');

function oc_loadmodules(options, callback) {
    var jsmodules = oc_modules(options);
    if (jsmodules.length > 0) {
        require(jsmodules, function () {
            callback();
        });
    }
    else {
        callback();
    }
}

function oc_modules(options) {
    var jsmodules = [];
    if (options) {
        var types = oc_fieldtypes(options);
        if ($.inArray("address", types) != -1) {
            jsmodules.push('addressfield');
        }
        if ($.inArray("imagecropper", types) != -1) {
            jsmodules.push('imagecropperfield');
        }
    }
    return jsmodules;
}

function oc_fieldtypes(options) {
    var types = [];
    if (options.fields) {
        fields = options.fields;
        for (var key in fields) {
            field = fields[key];
            if (field.type) {
                types.push(field.type);
            }
            var subtypes = oc_fieldtypes(field);
            types = types.concat(subtypes);
        }
    }
    else if (options.items && options.items.type) {
        types.push(options.items.type);
    }
    return types;
}

require.config({
    baseUrl : oc_moduleRoot + 'DesktopModules/OpenContent',
    paths: {
        'async': 'js/requirejs/async',
        'text': 'js/requirejs/text',
        'css': 'js/requirejs/css',
        'alpacafields': 'alpaca/js/fields/dnn',
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
        return window.google.maps;
    });

define('gmaps_places', ['async!https://maps.googleapis.com/maps/api/js?v=3.exp&signed_in=true&libraries=places'],
    function () {        
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



    