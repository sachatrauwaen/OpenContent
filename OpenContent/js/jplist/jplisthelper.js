(function ($) {
    $(document).ready(function () {

        // improve typing within a jplist textbox search
        var isTyping = false;
        var typingHandler = null;
        var $textfilter = $(".textfilter", this);

        $textfilter.on('input', function (context) {
            if (isTyping) {
                window.clearTimeout(typingHandler);
            }
            else {
                isTyping = true;
            }

            typingHandler = window.setTimeout(function () {
                isTyping = false;
                $textfilter.trigger("keydelay");
            }, 1000);
        });


    });
}(jQuery));