﻿/**
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
       
    // custom styles
    var styles = {};
    styles["commonIcon"] = "";
    styles["addIcon"] = "fa fa-plus";
    styles["removeIcon"] = "fa fa-trash-o";
    styles["upIcon"] = "fa fa-chevron-up";
    styles["downIcon"] = "fa fa-chevron-down";
    styles["containerExpandedIcon"] = "glyphicon glyphicon-circle-arrow-down";
    styles["containerCollapsedIcon"] = "glyphicon glyphicon-circle-arrow-right";

    // custom callbacks
    var callbacks = {};
    callbacks["required"] = function () {
        var fieldEl = this.getFieldEl();
        var label = $(fieldEl).find("label.alpaca-control-label").addClass('dnnFormRequired');
        // required fields get a little star in their label
        //var label = $(fieldEl).find("label.alpaca-control-label");
        //$('<span class="alpaca-icon-required glyphicon glyphicon-star"></span>').prependTo(label);

    };
    callbacks["invalid"] = function () {
        // if this is a control field, add class "has-error"
        if (this.isControlField) {
            $(this.getFieldEl()).addClass('has-error');
        }

        /*
        // if this is a container field, add class "has-error"
        if (this.isContainerField)
        {
            $(this.getFieldEl()).addClass('has-error');
        }
        */

    };
    callbacks["valid"] = function () {
        // valid fields remove the class 'has-error'
        $(this.getFieldEl()).removeClass('has-error');
    };
    callbacks["control"] = function () {
        // controls get some special formatting

        // fieldEl
        var fieldEl = this.getFieldEl();

        // controlEl
        var controlEl = this.getControlEl();

        /*
        // all controls get the "form-control" class injected
        $(fieldEl).find("input").addClass("form-control");
        $(fieldEl).find("textarea").addClass("form-control");
        $(fieldEl).find("select").addClass("form-control");
        // except for the following
        $(fieldEl).find("input[type=checkbox]").removeClass("form-control");
        $(fieldEl).find("input[type=file]").removeClass("form-control");
        $(fieldEl).find("input[type=radio]").removeClass("form-control");

        // special case for type == color, remove form-control
        if (this.inputType === "color") {
            $(fieldEl).find("input").removeClass("form-control");
        }
        */
        $(fieldEl).find("input[type=file]").addClass("normalFileUpload");


        /*
        // any checkbox inputs get the "checkbox" class on their checkbox
        $(fieldEl).find("input[type=checkbox]").parent().parent().addClass("checkbox");
        // any radio inputs get the "radio" class on their radio
        $(fieldEl).find("input[type=radio]").parent().parent().addClass("radio");

        // if form has "form-inline" class, then radio and checkbox labels get inline classes
        if ($(fieldEl).parents("form").hasClass("form-inline")) {
            // checkboxes
            $(fieldEl).find("input[type=checkbox]").parent().addClass("checkbox-inline");

            // radios
            $(fieldEl).find("input[type=radio]").parent().addClass("radio-inline");
        }

        // all control labels get class "control-label"
        $(fieldEl).find("label.alpaca-control-label").addClass("control-label");

        // if in horizontal mode, add a wrapper div (col-sm-9) and label gets (col-sm-3)
        if (this.view.horizontal) {
            $(fieldEl).find("label.alpaca-control-label").addClass("col-sm-3");

            var wrapper = $("<div></div>");
            wrapper.addClass("col-sm-9");

            $(controlEl).after(wrapper);
            wrapper.append(controlEl);

            $(fieldEl).append("<div style='clear:both;'></div>");
        }
        */
    };
    callbacks["container"] = function () {
        var containerEl = this.getContainerEl();

        if (this.view.horizontal) {
            $(containerEl).addClass("form-horizontal");
        }
    };
    callbacks["form"] = function () {
        var formEl = this.getFormEl();

        // use pull-right for form buttons
        //$(formEl).find(".alpaca-form-buttons-container").addClass("pull-right");
    };
    callbacks["enableButton"] = function (button) {
        $(button).removeAttr("disabled");
    };
    callbacks["disableButton"] = function (button) {
        $(button).attr("disabled", "disabled");
    };
    callbacks["collapsible"] = function () {
        var fieldEl = this.getFieldEl();
        var legendEl = $(fieldEl).find("legend").first();
        var anchorEl = $("[data-toggle='collapse']", legendEl);
        if ($(anchorEl).length > 0) {
            var containerEl = this.getContainerEl();

            // container id
            var id = $(containerEl).attr("id");
            if (!id) {
                id = Alpaca.generateId();
                $(containerEl).attr("id", id);
            }

            // set up container to be collapsible
            $(containerEl).addClass("collapse in");

            // set up legend anchor
            $(anchorEl).attr("data-target", "#" + id);

            $(anchorEl).mouseover(function (e) {
                $(this).css("cursor", "pointer");
            })



        }
        $(fieldEl).dnnPanels();
        $('.dnnTooltip', fieldEl).dnnTooltip();
    };

    Alpaca.registerView({
        "id": "dnn-display",
        "parent": "web-display",
        "type": "display",
        "ui": "dnn",
        "title": "Display View for DNN",
        "displayReadonly": true,
        "callbacks": callbacks,
        "styles": styles,
        "templates": {}
    });

    Alpaca.registerView({
        "id": "dnn-display-horizontal",
        "parent": "dnn-display",
        "horizontal": true
    });

    Alpaca.registerView({
        "id": "dnn-edit",
        "parent": "web-edit",
        "type": "edit",
        "ui": "dnn",
        "title": "Edit View for DNN",
        "displayReadonly": false,
        "callbacks": callbacks,
        "styles": styles,
        "templates": {
            "control": "#dnn-edit-control",
            "container": "#dnn-edit-container",
            "control-image": "#dnn-edit-control-image",
            "control-imagecrop": "#dnn-edit-control-imagecrop",
            "control-imagecrop2": "#dnn-edit-control-imagecrop2",
            "control-imagex": "#dnn-edit-control-imagex",
            "control-imagecropper": "#dnn-edit-control-imagecropper",
            "control-file": "#dnn-edit-control-file",
            "control-wysihtml": "#dnn-edit-control-wysihtml",
            "control-ckeditor": "#dnn-edit-control-ckeditor",
            "control-file2": "#dnn-edit-control-file2"
        }
    });

    Alpaca.registerView({
        "id": "dnn-edit-horizontal",
        "parent": "dnn-edit",
        "horizontal": true
    });

    Alpaca.registerView({
        "id": "dnn-create",
        "parent": "dnn-edit",
        "title": "Create View for DNN",
        "type": "create",
        "displayReadonly": false
    });

    Alpaca.registerView({
        "id": "dnn-create-horizontal",
        "parent": "dnn-create",
        "horizontal": true
    });

})(jQuery);