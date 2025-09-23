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