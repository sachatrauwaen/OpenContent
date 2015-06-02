/**
 * DNN Theme ("dnn")
 *
 * Defines the Alpaca theme for DNN.
 *
 * The views are:
 *
 *    dnn-view
 *    dnn-edit
 *    dnn-create
 *
 * This theme can also be selected by specifying the following view:
 *
 *    {
 *       "ui": "dnn",
 *       "type": "view" | "edit" | "create"
 *    }
 *
 */
(function ($) {

    var Alpaca = $.alpaca;
    
    Alpaca.registerView({
        "id": "dnnbootstrap-display",
        "parent": "bootstrap-display",
        "type": "display",
        "ui": "dnnbootstrap",
        "title": "Bootstrap Display View for DNN",
        "displayReadonly": true,
        "templates": {}
    });

    Alpaca.registerView({
        "id": "dnnbootstrap-display-horizontal",
        "parent": "dnnbootstrap-display",
        "horizontal": true
    });

    Alpaca.registerView({
        "id": "dnnbootstrap-edit",
        "parent": "bootstrap-edit",
        "type": "edit",
        "ui": "dnnbootstrap",
        "title": "Bootstrap Edit View for DNN",
        "displayReadonly": true,
        "templates": {
            "control-image": "/DesktopModules/OpenContent/alpaca/templates/dnn-edit/control-image.html",
            "control-wysihtml": "/DesktopModules/OpenContent/alpaca/templates/dnn-edit/control-wysihtml.html"
        }
    });

    Alpaca.registerView({
        "id": "dnnbootstrap-edit-horizontal",
        "parent": "dnnbootstrap-edit",
        "horizontal": true
    });

    Alpaca.registerView({
        "id": "dnnbootstrap-create",
        "parent": "dnnbootstrap-edit",
        "title": "Bootstrap Create View for DNN",
        "type": "create",
        "displayReadonly": false
    });

    Alpaca.registerView({
        "id": "dnnbootstrap-create-horizontal",
        "parent": "dnnbootstrap-create",
        "horizontal": true
    });

})(jQuery);