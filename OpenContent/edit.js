$.alpaca.Fields.RawField = $.alpaca.Fields.TextAreaField.extend({
    getFieldType: function () {
        return "raw";
    },
    onChange: function(e) {
        // alert("The value of: " + this.name + " was changed to: " + this.getValue());
        // $(this.domEl).css("background-color", "yellow");
    },
    setup: function (callback) {
        var self = this;
        this.base();
        this.options = this.options || {};
        this.options.buttons = {
            "check": {
                "value": "Preview",
                "click": function () {
                // Create popup container
                var popup = $('<div>').css({
                    'position': 'fixed',
                    'top': '50%',
                    'left': '50%',
                    'transform': 'translate(-50%, -50%)',
                    'width': '800px',
                    'height': '600px',
                    'background': 'white',
                    'border': '1px solid #ccc',
                    'box-shadow': '0 0 10px rgba(0,0,0,0.5)',
                    'z-index': 1000
                });

                // Create iframe
                var iframe = $('<iframe>').css({
                    'width': '100%',
                    'height': '100%',
                    'border': 'none'
                });

                // Add close button
                var closeBtn = $('<button>').text('×').css({
                    'position': 'absolute',
                    'right': '10px',
                    'top': '10px',
                    'background': 'transparent',
                    'border': 'none',
                    'font-size': '20px',
                    'cursor': 'pointer'
                }).click(function() {
                    popup.remove();
                });

                // Append elements
                popup.append(closeBtn);
                popup.append(iframe);
                $('body').append(popup);

                // Write content to iframe
                var iframeDoc = iframe[0].contentWindow.document;
                iframeDoc.open();
                iframeDoc.write('<html><head><title>Preview</title></head><body>' + self.getValue() + '</body></html>');
                iframeDoc.close();
                }
            }
        }
    }
});
Alpaca.registerFieldClass("raw", Alpaca.Fields.RawField);

Alpaca.Fields.MLRawField = Alpaca.Fields.RawField.extend(
    {
        constructor: function (container, data, options, schema, view, connector) {
            var self = this;
            this.base(container, data, options, schema, view, connector);
            this.culture = connector.culture;
            this.defaultCulture = connector.defaultCulture;
            this.rootUrl = connector.rootUrl;
        },
        /*
        getFieldType: function () {
            return "text";
        },
        */

        setup: function () {
            var self = this;
            if (this.data && Alpaca.isObject(this.data)) {
                this.olddata = this.data;
            } else if (this.data) {
                this.olddata = {};
                this.olddata[this.defaultCulture] = this.data;
            }


            if (this.culture != this.defaultCulture && this.olddata && this.olddata[this.defaultCulture]) {
                this.options.placeholder = this.olddata[this.defaultCulture];
            } else if (this.olddata && Object.keys(this.olddata).length && this.olddata[Object.keys(this.olddata)[0]]) {
                this.options.placeholder = this.olddata[Object.keys(this.olddata)[0]];
            } else {
                this.options.placeholder = "";
            }
            this.base();
            /*
            Alpaca.mergeObject(this.options, {
                "fieldClass": "flag-"+this.culture
            });
            */

            this.options = this.options || {};
            this.options.buttons = {
                "check": {
                    "value": "Preview",
                    "click": function () {
                        // Create popup container
                        var popup = $('<div>').css({
                            'position': 'fixed',
                            'top': '50%',
                            'left': '50%',
                            'transform': 'translate(-50%, -50%)',
                            'width': '800px',
                            'height': '600px',
                            'background': 'white',
                            'border': '1px solid #ccc',
                            'box-shadow': '0 0 10px rgba(0,0,0,0.5)',
                            'z-index': 1000
                        });

                        // Create iframe
                        var iframe = $('<iframe>').css({
                            'width': '100%',
                            'height': '100%',
                            'border': 'none'
                        });

                        // Add close button
                        var closeBtn = $('<button>').text('×').css({
                            'position': 'absolute',
                            'right': '10px',
                            'top': '10px',
                            'background': 'transparent',
                            'border': 'none',
                            'font-size': '20px',
                            'cursor': 'pointer'
                        }).click(function () {
                            popup.remove();
                        });

                        // Append elements
                        popup.append(closeBtn);
                        popup.append(iframe);
                        $('body').append(popup);

                        var val = self.getValue();
                        if (Alpaca.isObject(val)) {
                            val =val[self.culture];
                        }

                        // Write content to iframe
                        var iframeDoc = iframe[0].contentWindow.document;
                        iframeDoc.open();
                        iframeDoc.write('<html><head><title>Preview</title></head><body>' + val + '</body></html>');
                        iframeDoc.close();
                    }
                }
            }


        },
        getValue: function () {
            var val = this.base();
            var self = this;
            /*
            if (val === "") {
                return [];
            }
            */

            var o = {};
            if (this.olddata && Alpaca.isObject(this.olddata)) {
                $.each(this.olddata, function (key, value) {
                    var v = Alpaca.copyOf(value);
                    if (key != self.culture) {
                        o[key] = v;
                    }
                });
            }
            if (val != "") {
                o[self.culture] = val;
            }
            if ($.isEmptyObject(o)) {
                return "";
            }
            //o["_type"] = "languages";
            return o;
        },

        setValue: function (val) {
            if (val === "") {
                return;
            }
            if (!val) {
                this.base("");
                return;
            }
            if (Alpaca.isObject(val)) {
                var v = val[this.culture];
                if (!v) {
                    this.base("");
                    return;
                }
                this.base(v);
            }
            else {
                this.base(val);
            }
        },
        afterRenderControl: function (model, callback) {
            var self = this;
            this.base(model, function () {
                self.handlePostRender(function () {
                    callback();
                });
            });
        },
        handlePostRender: function (callback) {
            var self = this;
            var el = this.getControlEl();
            $(this.control.get(0)).after('<img src="' + self.rootUrl + 'images/Flags/' + this.culture + '.gif" class="flag" />');
            //$(this.control.get(0)).after('<div style="background:#eee;margin-bottom: 18px;display:inline-block;padding-bottom:8px;"><span>' + this.culture + '</span></div>');
            callback();
        },

        getTitle: function () {
            return "Multi Language Raw Field";
        },

        getDescription: function () {
            return "Multi Language Raw field .";
        },

    });

Alpaca.registerFieldClass("mlraw", Alpaca.Fields.MLRawField);


$(document).on("postSubmit.openform", function (event, data, moduleid, sf) {
    
});
$(document).on("postRender.opencontent", function (event, control, moduleid, sf) {
    /*
    var emailField = control.childrenByPropertyId["Email"];
    emailField.setValue("xxx@xxx.com");
    emailField.on("change", function () {
        alert(emailField.getValue());
    });
    */
});