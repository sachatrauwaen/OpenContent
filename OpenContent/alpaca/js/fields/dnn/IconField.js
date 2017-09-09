(function ($) {

    var Alpaca = $.alpaca;

    Alpaca.Fields.IconField = Alpaca.Fields.TextField.extend(
    /**
     * @lends Alpaca.Fields.IconField.prototype
     */
    {
        setup: function () {
            if (this.options.glyphicons === undefined) {
                this.options.glyphicons = false;
            }
            if (this.options.bootstrap === undefined) {
                this.options.bootstrap = false;
            }
            if (this.options.fontawesome === undefined) {
                this.options.fontawesome = true;
            }
            this.base();
        },
        setValue: function (value) {
            // be sure to call into base method
            this.base(value);
            this.loadIcons();

        },
        /**
         * @see Alpaca.Fields.TextField#getTitle
         */
        getTitle: function () {
            return "Icon Field";
        },

        /**
         * @see Alpaca.Fields.TextField#getDescription
         */
        getDescription: function () {
            return "Font Icon Field.";
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
            var el = this.control;
            this.control.fontIconPicker({
                //source: fontello_json_icons,
                emptyIcon: true,
                hasSearch: true
            });
            this.loadIcons();
            callback();
        },
        loadIcons: function () {
            var self = this;
            var icons = [];
            if (this.options.bootstrap) {
                $.each(bootstrap_icons, function (i, v) {
                    icons.push('glyphicon ' + v);
                });
            }
            if (this.options.fontawesome) {
                for (var i in fa_icons) {
                    icons.push('fa ' + i);
                }
            }
            if (this.options.glyphicons) {
                $.each(glyphicons_icons, function (i, v) {
                    icons.push('glyphicons ' + v);
                });
                $.each(dnngo_social, function (i, v) {
                    icons.push('social ' + v);
                });
            }
            this.control.fontIconPicker().setIcons(icons);
        }
    });

    Alpaca.registerFieldClass("icon", Alpaca.Fields.IconField);

    var bootstrap_icons = [
          "glyphicon-glass",
          "glyphicon-music",
          "glyphicon-search",
          "glyphicon-envelope",
          "glyphicon-heart",
          "glyphicon-star",
          "glyphicon-star-empty",
          "glyphicon-user",
          "glyphicon-film",
          "glyphicon-th-large",
          "glyphicon-th",
          "glyphicon-th-list",
          "glyphicon-ok",
          "glyphicon-remove",
          "glyphicon-zoom-in",
          "glyphicon-zoom-out",
          "glyphicon-off",
          "glyphicon-signal",
          "glyphicon-cog",
          "glyphicon-trash",
          "glyphicon-home",
          "glyphicon-file",
          "glyphicon-time",
          "glyphicon-road",
          "glyphicon-download-alt",
          "glyphicon-download",
          "glyphicon-upload",
          "glyphicon-inbox",
          "glyphicon-play-circle",
          "glyphicon-repeat",
          "glyphicon-refresh",
          "glyphicon-list-alt",
          "glyphicon-lock",
          "glyphicon-flag",
          "glyphicon-headphones",
          "glyphicon-volume-off",
          "glyphicon-volume-down",
          "glyphicon-volume-up",
          "glyphicon-qrcode",
          "glyphicon-barcode",
          "glyphicon-tag",
          "glyphicon-tags",
          "glyphicon-book",
          "glyphicon-bookmark",
          "glyphicon-print",
          "glyphicon-camera",
          "glyphicon-font",
          "glyphicon-bold",
          "glyphicon-italic",
          "glyphicon-text-height",
          "glyphicon-text-width",
          "glyphicon-align-left",
          "glyphicon-align-center",
          "glyphicon-align-right",
          "glyphicon-align-justify",
          "glyphicon-list",
          "glyphicon-indent-left",
          "glyphicon-indent-right",
          "glyphicon-facetime-video",
          "glyphicon-picture",
          "glyphicon-pencil",
          "glyphicon-map-marker",
          "glyphicon-adjust",
          "glyphicon-tint",
          "glyphicon-edit",
          "glyphicon-share",
          "glyphicon-check",
          "glyphicon-move",
          "glyphicon-step-backward",
          "glyphicon-fast-backward",
          "glyphicon-backward",
          "glyphicon-play",
          "glyphicon-pause",
          "glyphicon-stop",
          "glyphicon-forward",
          "glyphicon-fast-forward",
          "glyphicon-step-forward",
          "glyphicon-eject",
          "glyphicon-chevron-left",
          "glyphicon-chevron-right",
          "glyphicon-plus-sign",
          "glyphicon-minus-sign",
          "glyphicon-remove-sign",
          "glyphicon-ok-sign",
          "glyphicon-question-sign",
          "glyphicon-info-sign",
          "glyphicon-screenshot",
          "glyphicon-remove-circle",
          "glyphicon-ok-circle",
          "glyphicon-ban-circle",
          "glyphicon-arrow-left",
          "glyphicon-arrow-right",
          "glyphicon-arrow-up",
          "glyphicon-arrow-down",
          "glyphicon-share-alt",
          "glyphicon-resize-full",
          "glyphicon-resize-small",
          "glyphicon-plus",
          "glyphicon-minus",
          "glyphicon-asterisk",
          "glyphicon-exclamation-sign",
          "glyphicon-gift",
          "glyphicon-leaf",
          "glyphicon-fire",
          "glyphicon-eye-open",
          "glyphicon-eye-close",
          "glyphicon-warning-sign",
          "glyphicon-plane",
          "glyphicon-calendar",
          "glyphicon-random",
          "glyphicon-comment",
          "glyphicon-magnet",
          "glyphicon-chevron-up",
          "glyphicon-chevron-down",
          "glyphicon-retweet",
          "glyphicon-shopping-cart",
          "glyphicon-folder-close",
          "glyphicon-folder-open",
          "glyphicon-resize-vertical",
          "glyphicon-resize-horizontal",
          "glyphicon-hdd",
          "glyphicon-bullhorn",
          "glyphicon-bell",
          "glyphicon-certificate",
          "glyphicon-thumbs-up",
          "glyphicon-thumbs-down",
          "glyphicon-hand-right",
          "glyphicon-hand-left",
          "glyphicon-hand-up",
          "glyphicon-hand-down",
          "glyphicon-circle-arrow-right",
          "glyphicon-circle-arrow-left",
          "glyphicon-circle-arrow-up",
          "glyphicon-circle-arrow-down",
          "glyphicon-globe",
          "glyphicon-wrench",
          "glyphicon-tasks",
          "glyphicon-filter",
          "glyphicon-briefcase",
          "glyphicon-fullscreen",
          "glyphicon-dashboard",
          "glyphicon-paperclip",
          "glyphicon-heart-empty",
          "glyphicon-link",
          "glyphicon-phone",
          "glyphicon-pushpin",
          "glyphicon-euro",
          "glyphicon-usd",
          "glyphicon-gbp",
          "glyphicon-sort",
          "glyphicon-sort-by-alphabet",
          "glyphicon-sort-by-alphabet-alt",
          "glyphicon-sort-by-order",
          "glyphicon-sort-by-order-alt",
          "glyphicon-sort-by-attributes",
          "glyphicon-sort-by-attributes-alt",
          "glyphicon-unchecked",
          "glyphicon-expand",
          "glyphicon-collapse",
          "glyphicon-collapse-top"
    ];

    var fa_icons = {
        "fa-500px": {
            "unicode": "\\f26e",
            "name": "500px"
        },
        "fa-address-book": {
            "unicode": "\\f2b9",
            "name": "Address book"
        },
        "fa-address-book-o": {
            "unicode": "\\f2ba",
            "name": "Address book o"
        },
        "fa-address-card": {
            "unicode": "\\f2bb",
            "name": "Address card"
        },
        "fa-address-card-o": {
            "unicode": "\\f2bc",
            "name": "Address card o"
        },
        "fa-adjust": {
            "unicode": "\\f042",
            "name": "Adjust"
        },
        "fa-adn": {
            "unicode": "\\f170",
            "name": "Adn"
        },
        "fa-align-center": {
            "unicode": "\\f037",
            "name": "Align center"
        },
        "fa-align-justify": {
            "unicode": "\\f039",
            "name": "Align justify"
        },
        "fa-align-left": {
            "unicode": "\\f036",
            "name": "Align left"
        },
        "fa-align-right": {
            "unicode": "\\f038",
            "name": "Align right"
        },
        "fa-amazon": {
            "unicode": "\\f270",
            "name": "Amazon"
        },
        "fa-ambulance": {
            "unicode": "\\f0f9",
            "name": "Ambulance"
        },
        "fa-american-sign-language-interpreting": {
            "unicode": "\\f2a3",
            "name": "American sign language interpreting"
        },
        "fa-anchor": {
            "unicode": "\\f13d",
            "name": "Anchor"
        },
        "fa-android": {
            "unicode": "\\f17b",
            "name": "Android"
        },
        "fa-angellist": {
            "unicode": "\\f209",
            "name": "Angellist"
        },
        "fa-angle-double-down": {
            "unicode": "\\f103",
            "name": "Angle double down"
        },
        "fa-angle-double-left": {
            "unicode": "\\f100",
            "name": "Angle double left"
        },
        "fa-angle-double-right": {
            "unicode": "\\f101",
            "name": "Angle double right"
        },
        "fa-angle-double-up": {
            "unicode": "\\f102",
            "name": "Angle double up"
        },
        "fa-angle-down": {
            "unicode": "\\f107",
            "name": "Angle down"
        },
        "fa-angle-left": {
            "unicode": "\\f104",
            "name": "Angle left"
        },
        "fa-angle-right": {
            "unicode": "\\f105",
            "name": "Angle right"
        },
        "fa-angle-up": {
            "unicode": "\\f106",
            "name": "Angle up"
        },
        "fa-apple": {
            "unicode": "\\f179",
            "name": "Apple"
        },
        "fa-archive": {
            "unicode": "\\f187",
            "name": "Archive"
        },
        "fa-area-chart": {
            "unicode": "\\f1fe",
            "name": "Area chart"
        },
        "fa-arrow-circle-down": {
            "unicode": "\\f0ab",
            "name": "Arrow circle down"
        },
        "fa-arrow-circle-left": {
            "unicode": "\\f0a8",
            "name": "Arrow circle left"
        },
        "fa-arrow-circle-o-down": {
            "unicode": "\\f01a",
            "name": "Arrow circle o down"
        },
        "fa-arrow-circle-o-left": {
            "unicode": "\\f190",
            "name": "Arrow circle o left"
        },
        "fa-arrow-circle-o-right": {
            "unicode": "\\f18e",
            "name": "Arrow circle o right"
        },
        "fa-arrow-circle-o-up": {
            "unicode": "\\f01b",
            "name": "Arrow circle o up"
        },
        "fa-arrow-circle-right": {
            "unicode": "\\f0a9",
            "name": "Arrow circle right"
        },
        "fa-arrow-circle-up": {
            "unicode": "\\f0aa",
            "name": "Arrow circle up"
        },
        "fa-arrow-down": {
            "unicode": "\\f063",
            "name": "Arrow down"
        },
        "fa-arrow-left": {
            "unicode": "\\f060",
            "name": "Arrow left"
        },
        "fa-arrow-right": {
            "unicode": "\\f061",
            "name": "Arrow right"
        },
        "fa-arrow-up": {
            "unicode": "\\f062",
            "name": "Arrow up"
        },
        "fa-arrows": {
            "unicode": "\\f047",
            "name": "Arrows"
        },
        "fa-arrows-alt": {
            "unicode": "\\f0b2",
            "name": "Arrows alt"
        },
        "fa-arrows-h": {
            "unicode": "\\f07e",
            "name": "Arrows h"
        },
        "fa-arrows-v": {
            "unicode": "\\f07d",
            "name": "Arrows v"
        },
        "fa-assistive-listening-systems": {
            "unicode": "\\f2a2",
            "name": "Assistive listening systems"
        },
        "fa-asterisk": {
            "unicode": "\\f069",
            "name": "Asterisk"
        },
        "fa-at": {
            "unicode": "\\f1fa",
            "name": "At"
        },
        "fa-audio-description": {
            "unicode": "\\f29e",
            "name": "Audio description"
        },
        "fa-backward": {
            "unicode": "\\f04a",
            "name": "Backward"
        },
        "fa-balance-scale": {
            "unicode": "\\f24e",
            "name": "Balance scale"
        },
        "fa-ban": {
            "unicode": "\\f05e",
            "name": "Ban"
        },
        "fa-bandcamp": {
            "unicode": "\\f2d5",
            "name": "Bandcamp"
        },
        "fa-bar-chart": {
            "unicode": "\\f080",
            "name": "Bar chart"
        },
        "fa-barcode": {
            "unicode": "\\f02a",
            "name": "Barcode"
        },
        "fa-bars": {
            "unicode": "\\f0c9",
            "name": "Bars"
        },
        "fa-bath": {
            "unicode": "\\f2cd",
            "name": "Bath"
        },
        "fa-battery-empty": {
            "unicode": "\\f244",
            "name": "Battery empty"
        },
        "fa-battery-full": {
            "unicode": "\\f240",
            "name": "Battery full"
        },
        "fa-battery-half": {
            "unicode": "\\f242",
            "name": "Battery half"
        },
        "fa-battery-quarter": {
            "unicode": "\\f243",
            "name": "Battery quarter"
        },
        "fa-battery-three-quarters": {
            "unicode": "\\f241",
            "name": "Battery three quarters"
        },
        "fa-bed": {
            "unicode": "\\f236",
            "name": "Bed"
        },
        "fa-beer": {
            "unicode": "\\f0fc",
            "name": "Beer"
        },
        "fa-behance": {
            "unicode": "\\f1b4",
            "name": "Behance"
        },
        "fa-behance-square": {
            "unicode": "\\f1b5",
            "name": "Behance square"
        },
        "fa-bell": {
            "unicode": "\\f0f3",
            "name": "Bell"
        },
        "fa-bell-o": {
            "unicode": "\\f0a2",
            "name": "Bell o"
        },
        "fa-bell-slash": {
            "unicode": "\\f1f6",
            "name": "Bell slash"
        },
        "fa-bell-slash-o": {
            "unicode": "\\f1f7",
            "name": "Bell slash o"
        },
        "fa-bicycle": {
            "unicode": "\\f206",
            "name": "Bicycle"
        },
        "fa-binoculars": {
            "unicode": "\\f1e5",
            "name": "Binoculars"
        },
        "fa-birthday-cake": {
            "unicode": "\\f1fd",
            "name": "Birthday cake"
        },
        "fa-bitbucket": {
            "unicode": "\\f171",
            "name": "Bitbucket"
        },
        "fa-bitbucket-square": {
            "unicode": "\\f172",
            "name": "Bitbucket square"
        },
        "fa-black-tie": {
            "unicode": "\\f27e",
            "name": "Black tie"
        },
        "fa-blind": {
            "unicode": "\\f29d",
            "name": "Blind"
        },
        "fa-bluetooth": {
            "unicode": "\\f293",
            "name": "Bluetooth"
        },
        "fa-bluetooth-b": {
            "unicode": "\\f294",
            "name": "Bluetooth b"
        },
        "fa-bold": {
            "unicode": "\\f032",
            "name": "Bold"
        },
        "fa-bolt": {
            "unicode": "\\f0e7",
            "name": "Bolt"
        },
        "fa-bomb": {
            "unicode": "\\f1e2",
            "name": "Bomb"
        },
        "fa-book": {
            "unicode": "\\f02d",
            "name": "Book"
        },
        "fa-bookmark": {
            "unicode": "\\f02e",
            "name": "Bookmark"
        },
        "fa-bookmark-o": {
            "unicode": "\\f097",
            "name": "Bookmark o"
        },
        "fa-braille": {
            "unicode": "\\f2a1",
            "name": "Braille"
        },
        "fa-briefcase": {
            "unicode": "\\f0b1",
            "name": "Briefcase"
        },
        "fa-btc": {
            "unicode": "\\f15a",
            "name": "Btc"
        },
        "fa-bug": {
            "unicode": "\\f188",
            "name": "Bug"
        },
        "fa-building": {
            "unicode": "\\f1ad",
            "name": "Building"
        },
        "fa-building-o": {
            "unicode": "\\f0f7",
            "name": "Building o"
        },
        "fa-bullhorn": {
            "unicode": "\\f0a1",
            "name": "Bullhorn"
        },
        "fa-bullseye": {
            "unicode": "\\f140",
            "name": "Bullseye"
        },
        "fa-bus": {
            "unicode": "\\f207",
            "name": "Bus"
        },
        "fa-buysellads": {
            "unicode": "\\f20d",
            "name": "Buysellads"
        },
        "fa-calculator": {
            "unicode": "\\f1ec",
            "name": "Calculator"
        },
        "fa-calendar": {
            "unicode": "\\f073",
            "name": "Calendar"
        },
        "fa-calendar-check-o": {
            "unicode": "\\f274",
            "name": "Calendar check o"
        },
        "fa-calendar-minus-o": {
            "unicode": "\\f272",
            "name": "Calendar minus o"
        },
        "fa-calendar-o": {
            "unicode": "\\f133",
            "name": "Calendar o"
        },
        "fa-calendar-plus-o": {
            "unicode": "\\f271",
            "name": "Calendar plus o"
        },
        "fa-calendar-times-o": {
            "unicode": "\\f273",
            "name": "Calendar times o"
        },
        "fa-camera": {
            "unicode": "\\f030",
            "name": "Camera"
        },
        "fa-camera-retro": {
            "unicode": "\\f083",
            "name": "Camera retro"
        },
        "fa-car": {
            "unicode": "\\f1b9",
            "name": "Car"
        },
        "fa-caret-down": {
            "unicode": "\\f0d7",
            "name": "Caret down"
        },
        "fa-caret-left": {
            "unicode": "\\f0d9",
            "name": "Caret left"
        },
        "fa-caret-right": {
            "unicode": "\\f0da",
            "name": "Caret right"
        },
        "fa-caret-square-o-down": {
            "unicode": "\\f150",
            "name": "Caret square o down"
        },
        "fa-caret-square-o-left": {
            "unicode": "\\f191",
            "name": "Caret square o left"
        },
        "fa-caret-square-o-right": {
            "unicode": "\\f152",
            "name": "Caret square o right"
        },
        "fa-caret-square-o-up": {
            "unicode": "\\f151",
            "name": "Caret square o up"
        },
        "fa-caret-up": {
            "unicode": "\\f0d8",
            "name": "Caret up"
        },
        "fa-cart-arrow-down": {
            "unicode": "\\f218",
            "name": "Cart arrow down"
        },
        "fa-cart-plus": {
            "unicode": "\\f217",
            "name": "Cart plus"
        },
        "fa-cc": {
            "unicode": "\\f20a",
            "name": "Cc"
        },
        "fa-cc-amex": {
            "unicode": "\\f1f3",
            "name": "Cc amex"
        },
        "fa-cc-diners-club": {
            "unicode": "\\f24c",
            "name": "Cc diners club"
        },
        "fa-cc-discover": {
            "unicode": "\\f1f2",
            "name": "Cc discover"
        },
        "fa-cc-jcb": {
            "unicode": "\\f24b",
            "name": "Cc jcb"
        },
        "fa-cc-mastercard": {
            "unicode": "\\f1f1",
            "name": "Cc mastercard"
        },
        "fa-cc-paypal": {
            "unicode": "\\f1f4",
            "name": "Cc paypal"
        },
        "fa-cc-stripe": {
            "unicode": "\\f1f5",
            "name": "Cc stripe"
        },
        "fa-cc-visa": {
            "unicode": "\\f1f0",
            "name": "Cc visa"
        },
        "fa-certificate": {
            "unicode": "\\f0a3",
            "name": "Certificate"
        },
        "fa-chain-broken": {
            "unicode": "\\f127",
            "name": "Chain broken"
        },
        "fa-check": {
            "unicode": "\\f00c",
            "name": "Check"
        },
        "fa-check-circle": {
            "unicode": "\\f058",
            "name": "Check circle"
        },
        "fa-check-circle-o": {
            "unicode": "\\f05d",
            "name": "Check circle o"
        },
        "fa-check-square": {
            "unicode": "\\f14a",
            "name": "Check square"
        },
        "fa-check-square-o": {
            "unicode": "\\f046",
            "name": "Check square o"
        },
        "fa-chevron-circle-down": {
            "unicode": "\\f13a",
            "name": "Chevron circle down"
        },
        "fa-chevron-circle-left": {
            "unicode": "\\f137",
            "name": "Chevron circle left"
        },
        "fa-chevron-circle-right": {
            "unicode": "\\f138",
            "name": "Chevron circle right"
        },
        "fa-chevron-circle-up": {
            "unicode": "\\f139",
            "name": "Chevron circle up"
        },
        "fa-chevron-down": {
            "unicode": "\\f078",
            "name": "Chevron down"
        },
        "fa-chevron-left": {
            "unicode": "\\f053",
            "name": "Chevron left"
        },
        "fa-chevron-right": {
            "unicode": "\\f054",
            "name": "Chevron right"
        },
        "fa-chevron-up": {
            "unicode": "\\f077",
            "name": "Chevron up"
        },
        "fa-child": {
            "unicode": "\\f1ae",
            "name": "Child"
        },
        "fa-chrome": {
            "unicode": "\\f268",
            "name": "Chrome"
        },
        "fa-circle": {
            "unicode": "\\f111",
            "name": "Circle"
        },
        "fa-circle-o": {
            "unicode": "\\f10c",
            "name": "Circle o"
        },
        "fa-circle-o-notch": {
            "unicode": "\\f1ce",
            "name": "Circle o notch"
        },
        "fa-circle-thin": {
            "unicode": "\\f1db",
            "name": "Circle thin"
        },
        "fa-clipboard": {
            "unicode": "\\f0ea",
            "name": "Clipboard"
        },
        "fa-clock-o": {
            "unicode": "\\f017",
            "name": "Clock o"
        },
        "fa-clone": {
            "unicode": "\\f24d",
            "name": "Clone"
        },
        "fa-cloud": {
            "unicode": "\\f0c2",
            "name": "Cloud"
        },
        "fa-cloud-download": {
            "unicode": "\\f0ed",
            "name": "Cloud download"
        },
        "fa-cloud-upload": {
            "unicode": "\\f0ee",
            "name": "Cloud upload"
        },
        "fa-code": {
            "unicode": "\\f121",
            "name": "Code"
        },
        "fa-code-fork": {
            "unicode": "\\f126",
            "name": "Code fork"
        },
        "fa-codepen": {
            "unicode": "\\f1cb",
            "name": "Codepen"
        },
        "fa-codiepie": {
            "unicode": "\\f284",
            "name": "Codiepie"
        },
        "fa-coffee": {
            "unicode": "\\f0f4",
            "name": "Coffee"
        },
        "fa-cog": {
            "unicode": "\\f013",
            "name": "Cog"
        },
        "fa-cogs": {
            "unicode": "\\f085",
            "name": "Cogs"
        },
        "fa-columns": {
            "unicode": "\\f0db",
            "name": "Columns"
        },
        "fa-comment": {
            "unicode": "\\f075",
            "name": "Comment"
        },
        "fa-comment-o": {
            "unicode": "\\f0e5",
            "name": "Comment o"
        },
        "fa-commenting": {
            "unicode": "\\f27a",
            "name": "Commenting"
        },
        "fa-commenting-o": {
            "unicode": "\\f27b",
            "name": "Commenting o"
        },
        "fa-comments": {
            "unicode": "\\f086",
            "name": "Comments"
        },
        "fa-comments-o": {
            "unicode": "\\f0e6",
            "name": "Comments o"
        },
        "fa-compass": {
            "unicode": "\\f14e",
            "name": "Compass"
        },
        "fa-compress": {
            "unicode": "\\f066",
            "name": "Compress"
        },
        "fa-connectdevelop": {
            "unicode": "\\f20e",
            "name": "Connectdevelop"
        },
        "fa-contao": {
            "unicode": "\\f26d",
            "name": "Contao"
        },
        "fa-copyright": {
            "unicode": "\\f1f9",
            "name": "Copyright"
        },
        "fa-creative-commons": {
            "unicode": "\\f25e",
            "name": "Creative commons"
        },
        "fa-credit-card": {
            "unicode": "\\f09d",
            "name": "Credit card"
        },
        "fa-credit-card-alt": {
            "unicode": "\\f283",
            "name": "Credit card alt"
        },
        "fa-crop": {
            "unicode": "\\f125",
            "name": "Crop"
        },
        "fa-crosshairs": {
            "unicode": "\\f05b",
            "name": "Crosshairs"
        },
        "fa-css3": {
            "unicode": "\\f13c",
            "name": "Css3"
        },
        "fa-cube": {
            "unicode": "\\f1b2",
            "name": "Cube"
        },
        "fa-cubes": {
            "unicode": "\\f1b3",
            "name": "Cubes"
        },
        "fa-cutlery": {
            "unicode": "\\f0f5",
            "name": "Cutlery"
        },
        "fa-dashcube": {
            "unicode": "\\f210",
            "name": "Dashcube"
        },
        "fa-database": {
            "unicode": "\\f1c0",
            "name": "Database"
        },
        "fa-deaf": {
            "unicode": "\\f2a4",
            "name": "Deaf"
        },
        "fa-delicious": {
            "unicode": "\\f1a5",
            "name": "Delicious"
        },
        "fa-desktop": {
            "unicode": "\\f108",
            "name": "Desktop"
        },
        "fa-deviantart": {
            "unicode": "\\f1bd",
            "name": "Deviantart"
        },
        "fa-diamond": {
            "unicode": "\\f219",
            "name": "Diamond"
        },
        "fa-digg": {
            "unicode": "\\f1a6",
            "name": "Digg"
        },
        "fa-dot-circle-o": {
            "unicode": "\\f192",
            "name": "Dot circle o"
        },
        "fa-download": {
            "unicode": "\\f019",
            "name": "Download"
        },
        "fa-dribbble": {
            "unicode": "\\f17d",
            "name": "Dribbble"
        },
        "fa-dropbox": {
            "unicode": "\\f16b",
            "name": "Dropbox"
        },
        "fa-drupal": {
            "unicode": "\\f1a9",
            "name": "Drupal"
        },
        "fa-edge": {
            "unicode": "\\f282",
            "name": "Edge"
        },
        "fa-eercast": {
            "unicode": "\\f2da",
            "name": "Eercast"
        },
        "fa-eject": {
            "unicode": "\\f052",
            "name": "Eject"
        },
        "fa-ellipsis-h": {
            "unicode": "\\f141",
            "name": "Ellipsis h"
        },
        "fa-ellipsis-v": {
            "unicode": "\\f142",
            "name": "Ellipsis v"
        },
        "fa-empire": {
            "unicode": "\\f1d1",
            "name": "Empire"
        },
        "fa-envelope": {
            "unicode": "\\f0e0",
            "name": "Envelope"
        },
        "fa-envelope-o": {
            "unicode": "\\f003",
            "name": "Envelope o"
        },
        "fa-envelope-open": {
            "unicode": "\\f2b6",
            "name": "Envelope open"
        },
        "fa-envelope-open-o": {
            "unicode": "\\f2b7",
            "name": "Envelope open o"
        },
        "fa-envelope-square": {
            "unicode": "\\f199",
            "name": "Envelope square"
        },
        "fa-envira": {
            "unicode": "\\f299",
            "name": "Envira"
        },
        "fa-eraser": {
            "unicode": "\\f12d",
            "name": "Eraser"
        },
        "fa-etsy": {
            "unicode": "\\f2d7",
            "name": "Etsy"
        },
        "fa-eur": {
            "unicode": "\\f153",
            "name": "Eur"
        },
        "fa-exchange": {
            "unicode": "\\f0ec",
            "name": "Exchange"
        },
        "fa-exclamation": {
            "unicode": "\\f12a",
            "name": "Exclamation"
        },
        "fa-exclamation-circle": {
            "unicode": "\\f06a",
            "name": "Exclamation circle"
        },
        "fa-exclamation-triangle": {
            "unicode": "\\f071",
            "name": "Exclamation triangle"
        },
        "fa-expand": {
            "unicode": "\\f065",
            "name": "Expand"
        },
        "fa-expeditedssl": {
            "unicode": "\\f23e",
            "name": "Expeditedssl"
        },
        "fa-external-link": {
            "unicode": "\\f08e",
            "name": "External link"
        },
        "fa-external-link-square": {
            "unicode": "\\f14c",
            "name": "External link square"
        },
        "fa-eye": {
            "unicode": "\\f06e",
            "name": "Eye"
        },
        "fa-eye-slash": {
            "unicode": "\\f070",
            "name": "Eye slash"
        },
        "fa-eyedropper": {
            "unicode": "\\f1fb",
            "name": "Eyedropper"
        },
        "fa-facebook": {
            "unicode": "\\f09a",
            "name": "Facebook"
        },
        "fa-facebook-official": {
            "unicode": "\\f230",
            "name": "Facebook official"
        },
        "fa-facebook-square": {
            "unicode": "\\f082",
            "name": "Facebook square"
        },
        "fa-fast-backward": {
            "unicode": "\\f049",
            "name": "Fast backward"
        },
        "fa-fast-forward": {
            "unicode": "\\f050",
            "name": "Fast forward"
        },
        "fa-fax": {
            "unicode": "\\f1ac",
            "name": "Fax"
        },
        "fa-female": {
            "unicode": "\\f182",
            "name": "Female"
        },
        "fa-fighter-jet": {
            "unicode": "\\f0fb",
            "name": "Fighter jet"
        },
        "fa-file": {
            "unicode": "\\f15b",
            "name": "File"
        },
        "fa-file-archive-o": {
            "unicode": "\\f1c6",
            "name": "File archive o"
        },
        "fa-file-audio-o": {
            "unicode": "\\f1c7",
            "name": "File audio o"
        },
        "fa-file-code-o": {
            "unicode": "\\f1c9",
            "name": "File code o"
        },
        "fa-file-excel-o": {
            "unicode": "\\f1c3",
            "name": "File excel o"
        },
        "fa-file-image-o": {
            "unicode": "\\f1c5",
            "name": "File image o"
        },
        "fa-file-o": {
            "unicode": "\\f016",
            "name": "File o"
        },
        "fa-file-pdf-o": {
            "unicode": "\\f1c1",
            "name": "File pdf o"
        },
        "fa-file-powerpoint-o": {
            "unicode": "\\f1c4",
            "name": "File powerpoint o"
        },
        "fa-file-text": {
            "unicode": "\\f15c",
            "name": "File text"
        },
        "fa-file-text-o": {
            "unicode": "\\f0f6",
            "name": "File text o"
        },
        "fa-file-video-o": {
            "unicode": "\\f1c8",
            "name": "File video o"
        },
        "fa-file-word-o": {
            "unicode": "\\f1c2",
            "name": "File word o"
        },
        "fa-files-o": {
            "unicode": "\\f0c5",
            "name": "Files o"
        },
        "fa-film": {
            "unicode": "\\f008",
            "name": "Film"
        },
        "fa-filter": {
            "unicode": "\\f0b0",
            "name": "Filter"
        },
        "fa-fire": {
            "unicode": "\\f06d",
            "name": "Fire"
        },
        "fa-fire-extinguisher": {
            "unicode": "\\f134",
            "name": "Fire extinguisher"
        },
        "fa-firefox": {
            "unicode": "\\f269",
            "name": "Firefox"
        },
        "fa-first-order": {
            "unicode": "\\f2b0",
            "name": "First order"
        },
        "fa-flag": {
            "unicode": "\\f024",
            "name": "Flag"
        },
        "fa-flag-checkered": {
            "unicode": "\\f11e",
            "name": "Flag checkered"
        },
        "fa-flag-o": {
            "unicode": "\\f11d",
            "name": "Flag o"
        },
        "fa-flask": {
            "unicode": "\\f0c3",
            "name": "Flask"
        },
        "fa-flickr": {
            "unicode": "\\f16e",
            "name": "Flickr"
        },
        "fa-floppy-o": {
            "unicode": "\\f0c7",
            "name": "Floppy o"
        },
        "fa-folder": {
            "unicode": "\\f07b",
            "name": "Folder"
        },
        "fa-folder-o": {
            "unicode": "\\f114",
            "name": "Folder o"
        },
        "fa-folder-open": {
            "unicode": "\\f07c",
            "name": "Folder open"
        },
        "fa-folder-open-o": {
            "unicode": "\\f115",
            "name": "Folder open o"
        },
        "fa-font": {
            "unicode": "\\f031",
            "name": "Font"
        },
        "fa-font-awesome": {
            "unicode": "\\f2b4",
            "name": "Font awesome"
        },
        "fa-fonticons": {
            "unicode": "\\f280",
            "name": "Fonticons"
        },
        "fa-fort-awesome": {
            "unicode": "\\f286",
            "name": "Fort awesome"
        },
        "fa-forumbee": {
            "unicode": "\\f211",
            "name": "Forumbee"
        },
        "fa-forward": {
            "unicode": "\\f04e",
            "name": "Forward"
        },
        "fa-foursquare": {
            "unicode": "\\f180",
            "name": "Foursquare"
        },
        "fa-free-code-camp": {
            "unicode": "\\f2c5",
            "name": "Free code camp"
        },
        "fa-frown-o": {
            "unicode": "\\f119",
            "name": "Frown o"
        },
        "fa-futbol-o": {
            "unicode": "\\f1e3",
            "name": "Futbol o"
        },
        "fa-gamepad": {
            "unicode": "\\f11b",
            "name": "Gamepad"
        },
        "fa-gavel": {
            "unicode": "\\f0e3",
            "name": "Gavel"
        },
        "fa-gbp": {
            "unicode": "\\f154",
            "name": "Gbp"
        },
        "fa-genderless": {
            "unicode": "\\f22d",
            "name": "Genderless"
        },
        "fa-get-pocket": {
            "unicode": "\\f265",
            "name": "Get pocket"
        },
        "fa-gg": {
            "unicode": "\\f260",
            "name": "Gg"
        },
        "fa-gg-circle": {
            "unicode": "\\f261",
            "name": "Gg circle"
        },
        "fa-gift": {
            "unicode": "\\f06b",
            "name": "Gift"
        },
        "fa-git": {
            "unicode": "\\f1d3",
            "name": "Git"
        },
        "fa-git-square": {
            "unicode": "\\f1d2",
            "name": "Git square"
        },
        "fa-github": {
            "unicode": "\\f09b",
            "name": "Github"
        },
        "fa-github-alt": {
            "unicode": "\\f113",
            "name": "Github alt"
        },
        "fa-github-square": {
            "unicode": "\\f092",
            "name": "Github square"
        },
        "fa-gitlab": {
            "unicode": "\\f296",
            "name": "Gitlab"
        },
        "fa-glass": {
            "unicode": "\\f000",
            "name": "Glass"
        },
        "fa-glide": {
            "unicode": "\\f2a5",
            "name": "Glide"
        },
        "fa-glide-g": {
            "unicode": "\\f2a6",
            "name": "Glide g"
        },
        "fa-globe": {
            "unicode": "\\f0ac",
            "name": "Globe"
        },
        "fa-google": {
            "unicode": "\\f1a0",
            "name": "Google"
        },
        "fa-google-plus": {
            "unicode": "\\f0d5",
            "name": "Google plus"
        },
        "fa-google-plus-official": {
            "unicode": "\\f2b3",
            "name": "Google plus official"
        },
        "fa-google-plus-square": {
            "unicode": "\\f0d4",
            "name": "Google plus square"
        },
        "fa-google-wallet": {
            "unicode": "\\f1ee",
            "name": "Google wallet"
        },
        "fa-graduation-cap": {
            "unicode": "\\f19d",
            "name": "Graduation cap"
        },
        "fa-gratipay": {
            "unicode": "\\f184",
            "name": "Gratipay"
        },
        "fa-grav": {
            "unicode": "\\f2d6",
            "name": "Grav"
        },
        "fa-h-square": {
            "unicode": "\\f0fd",
            "name": "H square"
        },
        "fa-hacker-news": {
            "unicode": "\\f1d4",
            "name": "Hacker news"
        },
        "fa-hand-lizard-o": {
            "unicode": "\\f258",
            "name": "Hand lizard o"
        },
        "fa-hand-o-down": {
            "unicode": "\\f0a7",
            "name": "Hand o down"
        },
        "fa-hand-o-left": {
            "unicode": "\\f0a5",
            "name": "Hand o left"
        },
        "fa-hand-o-right": {
            "unicode": "\\f0a4",
            "name": "Hand o right"
        },
        "fa-hand-o-up": {
            "unicode": "\\f0a6",
            "name": "Hand o up"
        },
        "fa-hand-paper-o": {
            "unicode": "\\f256",
            "name": "Hand paper o"
        },
        "fa-hand-peace-o": {
            "unicode": "\\f25b",
            "name": "Hand peace o"
        },
        "fa-hand-pointer-o": {
            "unicode": "\\f25a",
            "name": "Hand pointer o"
        },
        "fa-hand-rock-o": {
            "unicode": "\\f255",
            "name": "Hand rock o"
        },
        "fa-hand-scissors-o": {
            "unicode": "\\f257",
            "name": "Hand scissors o"
        },
        "fa-hand-spock-o": {
            "unicode": "\\f259",
            "name": "Hand spock o"
        },
        "fa-handshake-o": {
            "unicode": "\\f2b5",
            "name": "Handshake o"
        },
        "fa-hashtag": {
            "unicode": "\\f292",
            "name": "Hashtag"
        },
        "fa-hdd-o": {
            "unicode": "\\f0a0",
            "name": "Hdd o"
        },
        "fa-header": {
            "unicode": "\\f1dc",
            "name": "Header"
        },
        "fa-headphones": {
            "unicode": "\\f025",
            "name": "Headphones"
        },
        "fa-heart": {
            "unicode": "\\f004",
            "name": "Heart"
        },
        "fa-heart-o": {
            "unicode": "\\f08a",
            "name": "Heart o"
        },
        "fa-heartbeat": {
            "unicode": "\\f21e",
            "name": "Heartbeat"
        },
        "fa-history": {
            "unicode": "\\f1da",
            "name": "History"
        },
        "fa-home": {
            "unicode": "\\f015",
            "name": "Home"
        },
        "fa-hospital-o": {
            "unicode": "\\f0f8",
            "name": "Hospital o"
        },
        "fa-hourglass": {
            "unicode": "\\f254",
            "name": "Hourglass"
        },
        "fa-hourglass-end": {
            "unicode": "\\f253",
            "name": "Hourglass end"
        },
        "fa-hourglass-half": {
            "unicode": "\\f252",
            "name": "Hourglass half"
        },
        "fa-hourglass-o": {
            "unicode": "\\f250",
            "name": "Hourglass o"
        },
        "fa-hourglass-start": {
            "unicode": "\\f251",
            "name": "Hourglass start"
        },
        "fa-houzz": {
            "unicode": "\\f27c",
            "name": "Houzz"
        },
        "fa-html5": {
            "unicode": "\\f13b",
            "name": "Html5"
        },
        "fa-i-cursor": {
            "unicode": "\\f246",
            "name": "I cursor"
        },
        "fa-id-badge": {
            "unicode": "\\f2c1",
            "name": "Id badge"
        },
        "fa-id-card": {
            "unicode": "\\f2c2",
            "name": "Id card"
        },
        "fa-id-card-o": {
            "unicode": "\\f2c3",
            "name": "Id card o"
        },
        "fa-ils": {
            "unicode": "\\f20b",
            "name": "Ils"
        },
        "fa-imdb": {
            "unicode": "\\f2d8",
            "name": "Imdb"
        },
        "fa-inbox": {
            "unicode": "\\f01c",
            "name": "Inbox"
        },
        "fa-indent": {
            "unicode": "\\f03c",
            "name": "Indent"
        },
        "fa-industry": {
            "unicode": "\\f275",
            "name": "Industry"
        },
        "fa-info": {
            "unicode": "\\f129",
            "name": "Info"
        },
        "fa-info-circle": {
            "unicode": "\\f05a",
            "name": "Info circle"
        },
        "fa-inr": {
            "unicode": "\\f156",
            "name": "Inr"
        },
        "fa-instagram": {
            "unicode": "\\f16d",
            "name": "Instagram"
        },
        "fa-internet-explorer": {
            "unicode": "\\f26b",
            "name": "Internet explorer"
        },
        "fa-ioxhost": {
            "unicode": "\\f208",
            "name": "Ioxhost"
        },
        "fa-italic": {
            "unicode": "\\f033",
            "name": "Italic"
        },
        "fa-joomla": {
            "unicode": "\\f1aa",
            "name": "Joomla"
        },
        "fa-jpy": {
            "unicode": "\\f157",
            "name": "Jpy"
        },
        "fa-jsfiddle": {
            "unicode": "\\f1cc",
            "name": "Jsfiddle"
        },
        "fa-key": {
            "unicode": "\\f084",
            "name": "Key"
        },
        "fa-keyboard-o": {
            "unicode": "\\f11c",
            "name": "Keyboard o"
        },
        "fa-krw": {
            "unicode": "\\f159",
            "name": "Krw"
        },
        "fa-language": {
            "unicode": "\\f1ab",
            "name": "Language"
        },
        "fa-laptop": {
            "unicode": "\\f109",
            "name": "Laptop"
        },
        "fa-lastfm": {
            "unicode": "\\f202",
            "name": "Lastfm"
        },
        "fa-lastfm-square": {
            "unicode": "\\f203",
            "name": "Lastfm square"
        },
        "fa-leaf": {
            "unicode": "\\f06c",
            "name": "Leaf"
        },
        "fa-leanpub": {
            "unicode": "\\f212",
            "name": "Leanpub"
        },
        "fa-lemon-o": {
            "unicode": "\\f094",
            "name": "Lemon o"
        },
        "fa-level-down": {
            "unicode": "\\f149",
            "name": "Level down"
        },
        "fa-level-up": {
            "unicode": "\\f148",
            "name": "Level up"
        },
        "fa-life-ring": {
            "unicode": "\\f1cd",
            "name": "Life ring"
        },
        "fa-lightbulb-o": {
            "unicode": "\\f0eb",
            "name": "Lightbulb o"
        },
        "fa-line-chart": {
            "unicode": "\\f201",
            "name": "Line chart"
        },
        "fa-link": {
            "unicode": "\\f0c1",
            "name": "Link"
        },
        "fa-linkedin": {
            "unicode": "\\f0e1",
            "name": "Linkedin"
        },
        "fa-linkedin-square": {
            "unicode": "\\f08c",
            "name": "Linkedin square"
        },
        "fa-linode": {
            "unicode": "\\f2b8",
            "name": "Linode"
        },
        "fa-linux": {
            "unicode": "\\f17c",
            "name": "Linux"
        },
        "fa-list": {
            "unicode": "\\f03a",
            "name": "List"
        },
        "fa-list-alt": {
            "unicode": "\\f022",
            "name": "List alt"
        },
        "fa-list-ol": {
            "unicode": "\\f0cb",
            "name": "List ol"
        },
        "fa-list-ul": {
            "unicode": "\\f0ca",
            "name": "List ul"
        },
        "fa-location-arrow": {
            "unicode": "\\f124",
            "name": "Location arrow"
        },
        "fa-lock": {
            "unicode": "\\f023",
            "name": "Lock"
        },
        "fa-long-arrow-down": {
            "unicode": "\\f175",
            "name": "Long arrow down"
        },
        "fa-long-arrow-left": {
            "unicode": "\\f177",
            "name": "Long arrow left"
        },
        "fa-long-arrow-right": {
            "unicode": "\\f178",
            "name": "Long arrow right"
        },
        "fa-long-arrow-up": {
            "unicode": "\\f176",
            "name": "Long arrow up"
        },
        "fa-low-vision": {
            "unicode": "\\f2a8",
            "name": "Low vision"
        },
        "fa-magic": {
            "unicode": "\\f0d0",
            "name": "Magic"
        },
        "fa-magnet": {
            "unicode": "\\f076",
            "name": "Magnet"
        },
        "fa-male": {
            "unicode": "\\f183",
            "name": "Male"
        },
        "fa-map": {
            "unicode": "\\f279",
            "name": "Map"
        },
        "fa-map-marker": {
            "unicode": "\\f041",
            "name": "Map marker"
        },
        "fa-map-o": {
            "unicode": "\\f278",
            "name": "Map o"
        },
        "fa-map-pin": {
            "unicode": "\\f276",
            "name": "Map pin"
        },
        "fa-map-signs": {
            "unicode": "\\f277",
            "name": "Map signs"
        },
        "fa-mars": {
            "unicode": "\\f222",
            "name": "Mars"
        },
        "fa-mars-double": {
            "unicode": "\\f227",
            "name": "Mars double"
        },
        "fa-mars-stroke": {
            "unicode": "\\f229",
            "name": "Mars stroke"
        },
        "fa-mars-stroke-h": {
            "unicode": "\\f22b",
            "name": "Mars stroke h"
        },
        "fa-mars-stroke-v": {
            "unicode": "\\f22a",
            "name": "Mars stroke v"
        },
        "fa-maxcdn": {
            "unicode": "\\f136",
            "name": "Maxcdn"
        },
        "fa-meanpath": {
            "unicode": "\\f20c",
            "name": "Meanpath"
        },
        "fa-medium": {
            "unicode": "\\f23a",
            "name": "Medium"
        },
        "fa-medkit": {
            "unicode": "\\f0fa",
            "name": "Medkit"
        },
        "fa-meetup": {
            "unicode": "\\f2e0",
            "name": "Meetup"
        },
        "fa-meh-o": {
            "unicode": "\\f11a",
            "name": "Meh o"
        },
        "fa-mercury": {
            "unicode": "\\f223",
            "name": "Mercury"
        },
        "fa-microchip": {
            "unicode": "\\f2db",
            "name": "Microchip"
        },
        "fa-microphone": {
            "unicode": "\\f130",
            "name": "Microphone"
        },
        "fa-microphone-slash": {
            "unicode": "\\f131",
            "name": "Microphone slash"
        },
        "fa-minus": {
            "unicode": "\\f068",
            "name": "Minus"
        },
        "fa-minus-circle": {
            "unicode": "\\f056",
            "name": "Minus circle"
        },
        "fa-minus-square": {
            "unicode": "\\f146",
            "name": "Minus square"
        },
        "fa-minus-square-o": {
            "unicode": "\\f147",
            "name": "Minus square o"
        },
        "fa-mixcloud": {
            "unicode": "\\f289",
            "name": "Mixcloud"
        },
        "fa-mobile": {
            "unicode": "\\f10b",
            "name": "Mobile"
        },
        "fa-modx": {
            "unicode": "\\f285",
            "name": "Modx"
        },
        "fa-money": {
            "unicode": "\\f0d6",
            "name": "Money"
        },
        "fa-moon-o": {
            "unicode": "\\f186",
            "name": "Moon o"
        },
        "fa-motorcycle": {
            "unicode": "\\f21c",
            "name": "Motorcycle"
        },
        "fa-mouse-pointer": {
            "unicode": "\\f245",
            "name": "Mouse pointer"
        },
        "fa-music": {
            "unicode": "\\f001",
            "name": "Music"
        },
        "fa-neuter": {
            "unicode": "\\f22c",
            "name": "Neuter"
        },
        "fa-newspaper-o": {
            "unicode": "\\f1ea",
            "name": "Newspaper o"
        },
        "fa-object-group": {
            "unicode": "\\f247",
            "name": "Object group"
        },
        "fa-object-ungroup": {
            "unicode": "\\f248",
            "name": "Object ungroup"
        },
        "fa-odnoklassniki": {
            "unicode": "\\f263",
            "name": "Odnoklassniki"
        },
        "fa-odnoklassniki-square": {
            "unicode": "\\f264",
            "name": "Odnoklassniki square"
        },
        "fa-opencart": {
            "unicode": "\\f23d",
            "name": "Opencart"
        },
        "fa-openid": {
            "unicode": "\\f19b",
            "name": "Openid"
        },
        "fa-opera": {
            "unicode": "\\f26a",
            "name": "Opera"
        },
        "fa-optin-monster": {
            "unicode": "\\f23c",
            "name": "Optin monster"
        },
        "fa-outdent": {
            "unicode": "\\f03b",
            "name": "Outdent"
        },
        "fa-pagelines": {
            "unicode": "\\f18c",
            "name": "Pagelines"
        },
        "fa-paint-brush": {
            "unicode": "\\f1fc",
            "name": "Paint brush"
        },
        "fa-paper-plane": {
            "unicode": "\\f1d8",
            "name": "Paper plane"
        },
        "fa-paper-plane-o": {
            "unicode": "\\f1d9",
            "name": "Paper plane o"
        },
        "fa-paperclip": {
            "unicode": "\\f0c6",
            "name": "Paperclip"
        },
        "fa-paragraph": {
            "unicode": "\\f1dd",
            "name": "Paragraph"
        },
        "fa-pause": {
            "unicode": "\\f04c",
            "name": "Pause"
        },
        "fa-pause-circle": {
            "unicode": "\\f28b",
            "name": "Pause circle"
        },
        "fa-pause-circle-o": {
            "unicode": "\\f28c",
            "name": "Pause circle o"
        },
        "fa-paw": {
            "unicode": "\\f1b0",
            "name": "Paw"
        },
        "fa-paypal": {
            "unicode": "\\f1ed",
            "name": "Paypal"
        },
        "fa-pencil": {
            "unicode": "\\f040",
            "name": "Pencil"
        },
        "fa-pencil-square": {
            "unicode": "\\f14b",
            "name": "Pencil square"
        },
        "fa-pencil-square-o": {
            "unicode": "\\f044",
            "name": "Pencil square o"
        },
        "fa-percent": {
            "unicode": "\\f295",
            "name": "Percent"
        },
        "fa-phone": {
            "unicode": "\\f095",
            "name": "Phone"
        },
        "fa-phone-square": {
            "unicode": "\\f098",
            "name": "Phone square"
        },
        "fa-picture-o": {
            "unicode": "\\f03e",
            "name": "Picture o"
        },
        "fa-pie-chart": {
            "unicode": "\\f200",
            "name": "Pie chart"
        },
        "fa-pied-piper": {
            "unicode": "\\f2ae",
            "name": "Pied piper"
        },
        "fa-pied-piper-alt": {
            "unicode": "\\f1a8",
            "name": "Pied piper alt"
        },
        "fa-pied-piper-pp": {
            "unicode": "\\f1a7",
            "name": "Pied piper pp"
        },
        "fa-pinterest": {
            "unicode": "\\f0d2",
            "name": "Pinterest"
        },
        "fa-pinterest-p": {
            "unicode": "\\f231",
            "name": "Pinterest p"
        },
        "fa-pinterest-square": {
            "unicode": "\\f0d3",
            "name": "Pinterest square"
        },
        "fa-plane": {
            "unicode": "\\f072",
            "name": "Plane"
        },
        "fa-play": {
            "unicode": "\\f04b",
            "name": "Play"
        },
        "fa-play-circle": {
            "unicode": "\\f144",
            "name": "Play circle"
        },
        "fa-play-circle-o": {
            "unicode": "\\f01d",
            "name": "Play circle o"
        },
        "fa-plug": {
            "unicode": "\\f1e6",
            "name": "Plug"
        },
        "fa-plus": {
            "unicode": "\\f067",
            "name": "Plus"
        },
        "fa-plus-circle": {
            "unicode": "\\f055",
            "name": "Plus circle"
        },
        "fa-plus-square": {
            "unicode": "\\f0fe",
            "name": "Plus square"
        },
        "fa-plus-square-o": {
            "unicode": "\\f196",
            "name": "Plus square o"
        },
        "fa-podcast": {
            "unicode": "\\f2ce",
            "name": "Podcast"
        },
        "fa-power-off": {
            "unicode": "\\f011",
            "name": "Power off"
        },
        "fa-print": {
            "unicode": "\\f02f",
            "name": "Print"
        },
        "fa-product-hunt": {
            "unicode": "\\f288",
            "name": "Product hunt"
        },
        "fa-puzzle-piece": {
            "unicode": "\\f12e",
            "name": "Puzzle piece"
        },
        "fa-qq": {
            "unicode": "\\f1d6",
            "name": "Qq"
        },
        "fa-qrcode": {
            "unicode": "\\f029",
            "name": "Qrcode"
        },
        "fa-question": {
            "unicode": "\\f128",
            "name": "Question"
        },
        "fa-question-circle": {
            "unicode": "\\f059",
            "name": "Question circle"
        },
        "fa-question-circle-o": {
            "unicode": "\\f29c",
            "name": "Question circle o"
        },
        "fa-quora": {
            "unicode": "\\f2c4",
            "name": "Quora"
        },
        "fa-quote-left": {
            "unicode": "\\f10d",
            "name": "Quote left"
        },
        "fa-quote-right": {
            "unicode": "\\f10e",
            "name": "Quote right"
        },
        "fa-random": {
            "unicode": "\\f074",
            "name": "Random"
        },
        "fa-ravelry": {
            "unicode": "\\f2d9",
            "name": "Ravelry"
        },
        "fa-rebel": {
            "unicode": "\\f1d0",
            "name": "Rebel"
        },
        "fa-recycle": {
            "unicode": "\\f1b8",
            "name": "Recycle"
        },
        "fa-reddit": {
            "unicode": "\\f1a1",
            "name": "Reddit"
        },
        "fa-reddit-alien": {
            "unicode": "\\f281",
            "name": "Reddit alien"
        },
        "fa-reddit-square": {
            "unicode": "\\f1a2",
            "name": "Reddit square"
        },
        "fa-refresh": {
            "unicode": "\\f021",
            "name": "Refresh"
        },
        "fa-registered": {
            "unicode": "\\f25d",
            "name": "Registered"
        },
        "fa-renren": {
            "unicode": "\\f18b",
            "name": "Renren"
        },
        "fa-repeat": {
            "unicode": "\\f01e",
            "name": "Repeat"
        },
        "fa-reply": {
            "unicode": "\\f112",
            "name": "Reply"
        },
        "fa-reply-all": {
            "unicode": "\\f122",
            "name": "Reply all"
        },
        "fa-retweet": {
            "unicode": "\\f079",
            "name": "Retweet"
        },
        "fa-road": {
            "unicode": "\\f018",
            "name": "Road"
        },
        "fa-rocket": {
            "unicode": "\\f135",
            "name": "Rocket"
        },
        "fa-rss": {
            "unicode": "\\f09e",
            "name": "Rss"
        },
        "fa-rss-square": {
            "unicode": "\\f143",
            "name": "Rss square"
        },
        "fa-rub": {
            "unicode": "\\f158",
            "name": "Rub"
        },
        "fa-safari": {
            "unicode": "\\f267",
            "name": "Safari"
        },
        "fa-scissors": {
            "unicode": "\\f0c4",
            "name": "Scissors"
        },
        "fa-scribd": {
            "unicode": "\\f28a",
            "name": "Scribd"
        },
        "fa-search": {
            "unicode": "\\f002",
            "name": "Search"
        },
        "fa-search-minus": {
            "unicode": "\\f010",
            "name": "Search minus"
        },
        "fa-search-plus": {
            "unicode": "\\f00e",
            "name": "Search plus"
        },
        "fa-sellsy": {
            "unicode": "\\f213",
            "name": "Sellsy"
        },
        "fa-server": {
            "unicode": "\\f233",
            "name": "Server"
        },
        "fa-share": {
            "unicode": "\\f064",
            "name": "Share"
        },
        "fa-share-alt": {
            "unicode": "\\f1e0",
            "name": "Share alt"
        },
        "fa-share-alt-square": {
            "unicode": "\\f1e1",
            "name": "Share alt square"
        },
        "fa-share-square": {
            "unicode": "\\f14d",
            "name": "Share square"
        },
        "fa-share-square-o": {
            "unicode": "\\f045",
            "name": "Share square o"
        },
        "fa-shield": {
            "unicode": "\\f132",
            "name": "Shield"
        },
        "fa-ship": {
            "unicode": "\\f21a",
            "name": "Ship"
        },
        "fa-shirtsinbulk": {
            "unicode": "\\f214",
            "name": "Shirtsinbulk"
        },
        "fa-shopping-bag": {
            "unicode": "\\f290",
            "name": "Shopping bag"
        },
        "fa-shopping-basket": {
            "unicode": "\\f291",
            "name": "Shopping basket"
        },
        "fa-shopping-cart": {
            "unicode": "\\f07a",
            "name": "Shopping cart"
        },
        "fa-shower": {
            "unicode": "\\f2cc",
            "name": "Shower"
        },
        "fa-sign-in": {
            "unicode": "\\f090",
            "name": "Sign in"
        },
        "fa-sign-language": {
            "unicode": "\\f2a7",
            "name": "Sign language"
        },
        "fa-sign-out": {
            "unicode": "\\f08b",
            "name": "Sign out"
        },
        "fa-signal": {
            "unicode": "\\f012",
            "name": "Signal"
        },
        "fa-simplybuilt": {
            "unicode": "\\f215",
            "name": "Simplybuilt"
        },
        "fa-sitemap": {
            "unicode": "\\f0e8",
            "name": "Sitemap"
        },
        "fa-skyatlas": {
            "unicode": "\\f216",
            "name": "Skyatlas"
        },
        "fa-skype": {
            "unicode": "\\f17e",
            "name": "Skype"
        },
        "fa-slack": {
            "unicode": "\\f198",
            "name": "Slack"
        },
        "fa-sliders": {
            "unicode": "\\f1de",
            "name": "Sliders"
        },
        "fa-slideshare": {
            "unicode": "\\f1e7",
            "name": "Slideshare"
        },
        "fa-smile-o": {
            "unicode": "\\f118",
            "name": "Smile o"
        },
        "fa-snapchat": {
            "unicode": "\\f2ab",
            "name": "Snapchat"
        },
        "fa-snapchat-ghost": {
            "unicode": "\\f2ac",
            "name": "Snapchat ghost"
        },
        "fa-snapchat-square": {
            "unicode": "\\f2ad",
            "name": "Snapchat square"
        },
        "fa-snowflake-o": {
            "unicode": "\\f2dc",
            "name": "Snowflake o"
        },
        "fa-sort": {
            "unicode": "\\f0dc",
            "name": "Sort"
        },
        "fa-sort-alpha-asc": {
            "unicode": "\\f15d",
            "name": "Sort alpha asc"
        },
        "fa-sort-alpha-desc": {
            "unicode": "\\f15e",
            "name": "Sort alpha desc"
        },
        "fa-sort-amount-asc": {
            "unicode": "\\f160",
            "name": "Sort amount asc"
        },
        "fa-sort-amount-desc": {
            "unicode": "\\f161",
            "name": "Sort amount desc"
        },
        "fa-sort-asc": {
            "unicode": "\\f0de",
            "name": "Sort asc"
        },
        "fa-sort-desc": {
            "unicode": "\\f0dd",
            "name": "Sort desc"
        },
        "fa-sort-numeric-asc": {
            "unicode": "\\f162",
            "name": "Sort numeric asc"
        },
        "fa-sort-numeric-desc": {
            "unicode": "\\f163",
            "name": "Sort numeric desc"
        },
        "fa-soundcloud": {
            "unicode": "\\f1be",
            "name": "Soundcloud"
        },
        "fa-space-shuttle": {
            "unicode": "\\f197",
            "name": "Space shuttle"
        },
        "fa-spinner": {
            "unicode": "\\f110",
            "name": "Spinner"
        },
        "fa-spoon": {
            "unicode": "\\f1b1",
            "name": "Spoon"
        },
        "fa-spotify": {
            "unicode": "\\f1bc",
            "name": "Spotify"
        },
        "fa-square": {
            "unicode": "\\f0c8",
            "name": "Square"
        },
        "fa-square-o": {
            "unicode": "\\f096",
            "name": "Square o"
        },
        "fa-stack-exchange": {
            "unicode": "\\f18d",
            "name": "Stack exchange"
        },
        "fa-stack-overflow": {
            "unicode": "\\f16c",
            "name": "Stack overflow"
        },
        "fa-star": {
            "unicode": "\\f005",
            "name": "Star"
        },
        "fa-star-half": {
            "unicode": "\\f089",
            "name": "Star half"
        },
        "fa-star-half-o": {
            "unicode": "\\f123",
            "name": "Star half o"
        },
        "fa-star-o": {
            "unicode": "\\f006",
            "name": "Star o"
        },
        "fa-steam": {
            "unicode": "\\f1b6",
            "name": "Steam"
        },
        "fa-steam-square": {
            "unicode": "\\f1b7",
            "name": "Steam square"
        },
        "fa-step-backward": {
            "unicode": "\\f048",
            "name": "Step backward"
        },
        "fa-step-forward": {
            "unicode": "\\f051",
            "name": "Step forward"
        },
        "fa-stethoscope": {
            "unicode": "\\f0f1",
            "name": "Stethoscope"
        },
        "fa-sticky-note": {
            "unicode": "\\f249",
            "name": "Sticky note"
        },
        "fa-sticky-note-o": {
            "unicode": "\\f24a",
            "name": "Sticky note o"
        },
        "fa-stop": {
            "unicode": "\\f04d",
            "name": "Stop"
        },
        "fa-stop-circle": {
            "unicode": "\\f28d",
            "name": "Stop circle"
        },
        "fa-stop-circle-o": {
            "unicode": "\\f28e",
            "name": "Stop circle o"
        },
        "fa-street-view": {
            "unicode": "\\f21d",
            "name": "Street view"
        },
        "fa-strikethrough": {
            "unicode": "\\f0cc",
            "name": "Strikethrough"
        },
        "fa-stumbleupon": {
            "unicode": "\\f1a4",
            "name": "Stumbleupon"
        },
        "fa-stumbleupon-circle": {
            "unicode": "\\f1a3",
            "name": "Stumbleupon circle"
        },
        "fa-subscript": {
            "unicode": "\\f12c",
            "name": "Subscript"
        },
        "fa-subway": {
            "unicode": "\\f239",
            "name": "Subway"
        },
        "fa-suitcase": {
            "unicode": "\\f0f2",
            "name": "Suitcase"
        },
        "fa-sun-o": {
            "unicode": "\\f185",
            "name": "Sun o"
        },
        "fa-superpowers": {
            "unicode": "\\f2dd",
            "name": "Superpowers"
        },
        "fa-superscript": {
            "unicode": "\\f12b",
            "name": "Superscript"
        },
        "fa-table": {
            "unicode": "\\f0ce",
            "name": "Table"
        },
        "fa-tablet": {
            "unicode": "\\f10a",
            "name": "Tablet"
        },
        "fa-tachometer": {
            "unicode": "\\f0e4",
            "name": "Tachometer"
        },
        "fa-tag": {
            "unicode": "\\f02b",
            "name": "Tag"
        },
        "fa-tags": {
            "unicode": "\\f02c",
            "name": "Tags"
        },
        "fa-tasks": {
            "unicode": "\\f0ae",
            "name": "Tasks"
        },
        "fa-taxi": {
            "unicode": "\\f1ba",
            "name": "Taxi"
        },
        "fa-telegram": {
            "unicode": "\\f2c6",
            "name": "Telegram"
        },
        "fa-television": {
            "unicode": "\\f26c",
            "name": "Television"
        },
        "fa-tencent-weibo": {
            "unicode": "\\f1d5",
            "name": "Tencent weibo"
        },
        "fa-terminal": {
            "unicode": "\\f120",
            "name": "Terminal"
        },
        "fa-text-height": {
            "unicode": "\\f034",
            "name": "Text height"
        },
        "fa-text-width": {
            "unicode": "\\f035",
            "name": "Text width"
        },
        "fa-th": {
            "unicode": "\\f00a",
            "name": "Th"
        },
        "fa-th-large": {
            "unicode": "\\f009",
            "name": "Th large"
        },
        "fa-th-list": {
            "unicode": "\\f00b",
            "name": "Th list"
        },
        "fa-themeisle": {
            "unicode": "\\f2b2",
            "name": "Themeisle"
        },
        "fa-thermometer-empty": {
            "unicode": "\\f2cb",
            "name": "Thermometer empty"
        },
        "fa-thermometer-full": {
            "unicode": "\\f2c7",
            "name": "Thermometer full"
        },
        "fa-thermometer-half": {
            "unicode": "\\f2c9",
            "name": "Thermometer half"
        },
        "fa-thermometer-quarter": {
            "unicode": "\\f2ca",
            "name": "Thermometer quarter"
        },
        "fa-thermometer-three-quarters": {
            "unicode": "\\f2c8",
            "name": "Thermometer three quarters"
        },
        "fa-thumb-tack": {
            "unicode": "\\f08d",
            "name": "Thumb tack"
        },
        "fa-thumbs-down": {
            "unicode": "\\f165",
            "name": "Thumbs down"
        },
        "fa-thumbs-o-down": {
            "unicode": "\\f088",
            "name": "Thumbs o down"
        },
        "fa-thumbs-o-up": {
            "unicode": "\\f087",
            "name": "Thumbs o up"
        },
        "fa-thumbs-up": {
            "unicode": "\\f164",
            "name": "Thumbs up"
        },
        "fa-ticket": {
            "unicode": "\\f145",
            "name": "Ticket"
        },
        "fa-times": {
            "unicode": "\\f00d",
            "name": "Times"
        },
        "fa-times-circle": {
            "unicode": "\\f057",
            "name": "Times circle"
        },
        "fa-times-circle-o": {
            "unicode": "\\f05c",
            "name": "Times circle o"
        },
        "fa-tint": {
            "unicode": "\\f043",
            "name": "Tint"
        },
        "fa-toggle-off": {
            "unicode": "\\f204",
            "name": "Toggle off"
        },
        "fa-toggle-on": {
            "unicode": "\\f205",
            "name": "Toggle on"
        },
        "fa-trademark": {
            "unicode": "\\f25c",
            "name": "Trademark"
        },
        "fa-train": {
            "unicode": "\\f238",
            "name": "Train"
        },
        "fa-transgender": {
            "unicode": "\\f224",
            "name": "Transgender"
        },
        "fa-transgender-alt": {
            "unicode": "\\f225",
            "name": "Transgender alt"
        },
        "fa-trash": {
            "unicode": "\\f1f8",
            "name": "Trash"
        },
        "fa-trash-o": {
            "unicode": "\\f014",
            "name": "Trash o"
        },
        "fa-tree": {
            "unicode": "\\f1bb",
            "name": "Tree"
        },
        "fa-trello": {
            "unicode": "\\f181",
            "name": "Trello"
        },
        "fa-tripadvisor": {
            "unicode": "\\f262",
            "name": "Tripadvisor"
        },
        "fa-trophy": {
            "unicode": "\\f091",
            "name": "Trophy"
        },
        "fa-truck": {
            "unicode": "\\f0d1",
            "name": "Truck"
        },
        "fa-try": {
            "unicode": "\\f195",
            "name": "Try"
        },
        "fa-tty": {
            "unicode": "\\f1e4",
            "name": "Tty"
        },
        "fa-tumblr": {
            "unicode": "\\f173",
            "name": "Tumblr"
        },
        "fa-tumblr-square": {
            "unicode": "\\f174",
            "name": "Tumblr square"
        },
        "fa-twitch": {
            "unicode": "\\f1e8",
            "name": "Twitch"
        },
        "fa-twitter": {
            "unicode": "\\f099",
            "name": "Twitter"
        },
        "fa-twitter-square": {
            "unicode": "\\f081",
            "name": "Twitter square"
        },
        "fa-umbrella": {
            "unicode": "\\f0e9",
            "name": "Umbrella"
        },
        "fa-underline": {
            "unicode": "\\f0cd",
            "name": "Underline"
        },
        "fa-undo": {
            "unicode": "\\f0e2",
            "name": "Undo"
        },
        "fa-universal-access": {
            "unicode": "\\f29a",
            "name": "Universal access"
        },
        "fa-university": {
            "unicode": "\\f19c",
            "name": "University"
        },
        "fa-unlock": {
            "unicode": "\\f09c",
            "name": "Unlock"
        },
        "fa-unlock-alt": {
            "unicode": "\\f13e",
            "name": "Unlock alt"
        },
        "fa-upload": {
            "unicode": "\\f093",
            "name": "Upload"
        },
        "fa-usb": {
            "unicode": "\\f287",
            "name": "Usb"
        },
        "fa-usd": {
            "unicode": "\\f155",
            "name": "Usd"
        },
        "fa-user": {
            "unicode": "\\f007",
            "name": "User"
        },
        "fa-user-circle": {
            "unicode": "\\f2bd",
            "name": "User circle"
        },
        "fa-user-circle-o": {
            "unicode": "\\f2be",
            "name": "User circle o"
        },
        "fa-user-md": {
            "unicode": "\\f0f0",
            "name": "User md"
        },
        "fa-user-o": {
            "unicode": "\\f2c0",
            "name": "User o"
        },
        "fa-user-plus": {
            "unicode": "\\f234",
            "name": "User plus"
        },
        "fa-user-secret": {
            "unicode": "\\f21b",
            "name": "User secret"
        },
        "fa-user-times": {
            "unicode": "\\f235",
            "name": "User times"
        },
        "fa-users": {
            "unicode": "\\f0c0",
            "name": "Users"
        },
        "fa-venus": {
            "unicode": "\\f221",
            "name": "Venus"
        },
        "fa-venus-double": {
            "unicode": "\\f226",
            "name": "Venus double"
        },
        "fa-venus-mars": {
            "unicode": "\\f228",
            "name": "Venus mars"
        },
        "fa-viacoin": {
            "unicode": "\\f237",
            "name": "Viacoin"
        },
        "fa-viadeo": {
            "unicode": "\\f2a9",
            "name": "Viadeo"
        },
        "fa-viadeo-square": {
            "unicode": "\\f2aa",
            "name": "Viadeo square"
        },
        "fa-video-camera": {
            "unicode": "\\f03d",
            "name": "Video camera"
        },
        "fa-vimeo": {
            "unicode": "\\f27d",
            "name": "Vimeo"
        },
        "fa-vimeo-square": {
            "unicode": "\\f194",
            "name": "Vimeo square"
        },
        "fa-vine": {
            "unicode": "\\f1ca",
            "name": "Vine"
        },
        "fa-vk": {
            "unicode": "\\f189",
            "name": "Vk"
        },
        "fa-volume-control-phone": {
            "unicode": "\\f2a0",
            "name": "Volume control phone"
        },
        "fa-volume-down": {
            "unicode": "\\f027",
            "name": "Volume down"
        },
        "fa-volume-off": {
            "unicode": "\\f026",
            "name": "Volume off"
        },
        "fa-volume-up": {
            "unicode": "\\f028",
            "name": "Volume up"
        },
        "fa-weibo": {
            "unicode": "\\f18a",
            "name": "Weibo"
        },
        "fa-weixin": {
            "unicode": "\\f1d7",
            "name": "Weixin"
        },
        "fa-whatsapp": {
            "unicode": "\\f232",
            "name": "Whatsapp"
        },
        "fa-wheelchair": {
            "unicode": "\\f193",
            "name": "Wheelchair"
        },
        "fa-wheelchair-alt": {
            "unicode": "\\f29b",
            "name": "Wheelchair alt"
        },
        "fa-wifi": {
            "unicode": "\\f1eb",
            "name": "Wifi"
        },
        "fa-wikipedia-w": {
            "unicode": "\\f266",
            "name": "Wikipedia w"
        },
        "fa-window-close": {
            "unicode": "\\f2d3",
            "name": "Window close"
        },
        "fa-window-close-o": {
            "unicode": "\\f2d4",
            "name": "Window close o"
        },
        "fa-window-maximize": {
            "unicode": "\\f2d0",
            "name": "Window maximize"
        },
        "fa-window-minimize": {
            "unicode": "\\f2d1",
            "name": "Window minimize"
        },
        "fa-window-restore": {
            "unicode": "\\f2d2",
            "name": "Window restore"
        },
        "fa-windows": {
            "unicode": "\\f17a",
            "name": "Windows"
        },
        "fa-wordpress": {
            "unicode": "\\f19a",
            "name": "Wordpress"
        },
        "fa-wpbeginner": {
            "unicode": "\\f297",
            "name": "Wpbeginner"
        },
        "fa-wpexplorer": {
            "unicode": "\\f2de",
            "name": "Wpexplorer"
        },
        "fa-wpforms": {
            "unicode": "\\f298",
            "name": "Wpforms"
        },
        "fa-wrench": {
            "unicode": "\\f0ad",
            "name": "Wrench"
        },
        "fa-xing": {
            "unicode": "\\f168",
            "name": "Xing"
        },
        "fa-xing-square": {
            "unicode": "\\f169",
            "name": "Xing square"
        },
        "fa-y-combinator": {
            "unicode": "\\f23b",
            "name": "Y combinator"
        },
        "fa-yahoo": {
            "unicode": "\\f19e",
            "name": "Yahoo"
        },
        "fa-yelp": {
            "unicode": "\\f1e9",
            "name": "Yelp"
        },
        "fa-yoast": {
            "unicode": "\\f2b1",
            "name": "Yoast"
        },
        "fa-youtube": {
            "unicode": "\\f167",
            "name": "Youtube"
        },
        "fa-youtube-play": {
            "unicode": "\\f16a",
            "name": "Youtube play"
        },
        "fa-youtube-square": {
            "unicode": "\\f166",
            "name": "Youtube square"
        }
    };

    var glyphicons_icons = [
        "glyph-glass",
        "glyph-leaf",
        "glyph-dog",
        "glyph-user",
        "glyph-girl",
        "glyph-car",
        "glyph-user-add",
        "glyph-user-remove",
        "glyph-film",
        "glyph-magic",
        "glyph-envelope",
        "glyph-camera",
        "glyph-heart",
        "glyph-beach-umbrella",
        "glyph-train",
        "glyph-print",
        "glyph-bin",
        "glyph-music",
        "glyph-note",
        "glyph-heart-empty",
        "glyph-home",
        "glyph-snowflake",
        "glyph-fire",
        "glyph-magnet",
        "glyph-parents",
        "glyph-binoculars",
        "glyph-road",
        "glyph-search",
        "glyph-cars",
        "glyph-notes-2",
        "glyph-pencil",
        "glyph-bus",
        "glyph-wifi-alt",
        "glyph-luggage",
        "glyph-old-man",
        "glyph-woman",
        "glyph-file",
        "glyph-coins",
        "glyph-airplane",
        "glyph-notes",
        "glyph-stats",
        "glyph-charts",
        "glyph-pie-chart",
        "glyph-group",
        "glyph-keys",
        "glyph-calendar",
        "glyph-router",
        "glyph-camera-small",
        "glyph-dislikes",
        "glyph-star",
        "glyph-link",
        "glyph-eye-open",
        "glyph-eye-close",
        "glyph-alarm",
        "glyph-clock",
        "glyph-stopwatch",
        "glyph-projector",
        "glyph-history",
        "glyph-truck",
        "glyph-cargo",
        "glyph-compass",
        "glyph-keynote",
        "glyph-paperclip",
        "glyph-power",
        "glyph-lightbulb",
        "glyph-tag",
        "glyph-tags",
        "glyph-cleaning",
        "glyph-ruller",
        "glyph-gift",
        "glyph-umbrella",
        "glyph-book",
        "glyph-bookmark",
        "glyph-wifi",
        "glyph-cup",
        "glyph-stroller",
        "glyph-headphones",
        "glyph-headset",
        "glyph-warning-sign",
        "glyph-signal",
        "glyph-retweet",
        "glyph-refresh",
        "glyph-roundabout",
        "glyph-random",
        "glyph-heat",
        "glyph-repeat",
        "glyph-display",
        "glyph-log-book",
        "glyph-address-book",
        "glyph-building",
        "glyph-eyedropper",
        "glyph-adjust",
        "glyph-tint",
        "glyph-crop",
        "glyph-vector-path-square",
        "glyph-vector-path-circle",
        "glyph-vector-path-polygon",
        "glyph-vector-path-line",
        "glyph-vector-path-curve",
        "glyph-vector-path-all",
        "glyph-font",
        "glyph-italic",
        "glyph-bold",
        "glyph-text-underline",
        "glyph-text-strike",
        "glyph-text-height",
        "glyph-text-width",
        "glyph-text-resize",
        "glyph-left-indent",
        "glyph-right-indent",
        "glyph-align-left",
        "glyph-align-center",
        "glyph-align-right",
        "glyph-justify",
        "glyph-list",
        "glyph-text-smaller",
        "glyph-text-bigger",
        "glyph-embed",
        "glyph-embed-close",
        "glyph-table",
        "glyph-message-full",
        "glyph-message-empty",
        "glyph-message-in",
        "glyph-message-out",
        "glyph-message-plus",
        "glyph-message-minus",
        "glyph-message-ban",
        "glyph-message-flag",
        "glyph-message-lock",
        "glyph-message-new",
        "glyph-inbox",
        "glyph-inbox-plus",
        "glyph-inbox-minus",
        "glyph-inbox-lock",
        "glyph-inbox-in",
        "glyph-inbox-out",
        "glyph-cogwheel",
        "glyph-cogwheels",
        "glyph-picture",
        "glyph-adjust-alt",
        "glyph-database-lock",
        "glyph-database-plus",
        "glyph-database-minus",
        "glyph-database-ban",
        "glyph-folder-open",
        "glyph-folder-plus",
        "glyph-folder-minus",
        "glyph-folder-lock",
        "glyph-folder-flag",
        "glyph-folder-new",
        "glyph-edit",
        "glyph-new-window",
        "glyph-check",
        "glyph-unchecked",
        "glyph-more-windows",
        "glyph-show-big-thumbnails",
        "glyph-show-thumbnails",
        "glyph-show-thumbnails-with-lines",
        "glyph-show-lines",
        "glyph-playlist",
        "glyph-imac",
        "glyph-macbook",
        "glyph-ipad",
        "glyph-iphone",
        "glyph-iphone-transfer",
        "glyph-iphone-exchange",
        "glyph-ipod",
        "glyph-ipod-shuffle",
        "glyph-ear-plugs",
        "glyph-record",
        "glyph-step-backward",
        "glyph-fast-backward",
        "glyph-rewind",
        "glyph-play",
        "glyph-pause",
        "glyph-stop",
        "glyph-forward",
        "glyph-fast-forward",
        "glyph-step-forward",
        "glyph-eject",
        "glyph-facetime-video",
        "glyph-download-alt",
        "glyph-mute",
        "glyph-volume-down",
        "glyph-volume-up",
        "glyph-screenshot",
        "glyph-move",
        "glyph-more",
        "glyph-brightness-reduce",
        "glyph-brightness-increase",
        "glyph-circle-plus",
        "glyph-circle-minus",
        "glyph-circle-remove",
        "glyph-circle-ok",
        "glyph-circle-question-mark",
        "glyph-circle-info",
        "glyph-circle-exclamation-mark",
        "glyph-remove",
        "glyph-ok",
        "glyph-ban",
        "glyph-download",
        "glyph-upload",
        "glyph-shopping-cart",
        "glyph-lock",
        "glyph-unlock",
        "glyph-electricity",
        "glyph-ok-2",
        "glyph-remove-2",
        "glyph-cart-out",
        "glyph-cart-in",
        "glyph-left-arrow",
        "glyph-right-arrow",
        "glyph-down-arrow",
        "glyph-up-arrow",
        "glyph-resize-small",
        "glyph-resize-full",
        "glyph-circle-arrow-left",
        "glyph-circle-arrow-right",
        "glyph-circle-arrow-top",
        "glyph-circle-arrow-down",
        "glyph-play-button",
        "glyph-unshare",
        "glyph-share",
        "glyph-chevron-right",
        "glyph-chevron-left",
        "glyph-bluetooth",
        "glyph-euro",
        "glyph-usd",
        "glyph-gbp",
        "glyph-retweet-2",
        "glyph-moon",
        "glyph-sun",
        "glyph-cloud",
        "glyph-direction",
        "glyph-brush",
        "glyph-pen",
        "glyph-zoom-in",
        "glyph-zoom-out",
        "glyph-pin",
        "glyph-albums",
        "glyph-rotation-lock",
        "glyph-flash",
        "glyph-google-maps",
        "glyph-anchor",
        "glyph-conversation",
        "glyph-chat",
        "glyph-male",
        "glyph-female",
        "glyph-asterisk",
        "glyph-divide",
        "glyph-snorkel-diving",
        "glyph-scuba-diving",
        "glyph-oxygen-bottle",
        "glyph-fins",
        "glyph-fishes",
        "glyph-boat",
        "glyph-delete",
        "glyph-sheriffs-star",
        "glyph-qrcode",
        "glyph-barcode",
        "glyph-pool",
        "glyph-buoy",
        "glyph-spade",
        "glyph-bank",
        "glyph-vcard",
        "glyph-electrical-plug",
        "glyph-flag",
        "glyph-credit-card",
        "glyph-keyboard-wireless",
        "glyph-keyboard-wired",
        "glyph-shield",
        "glyph-ring",
        "glyph-cake",
        "glyph-drink",
        "glyph-beer",
        "glyph-fast-food",
        "glyph-cutlery",
        "glyph-pizza",
        "glyph-birthday-cake",
        "glyph-tablet",
        "glyph-settings",
        "glyph-bullets",
        "glyph-cardio",
        "glyph-t-shirt",
        "glyph-pants",
        "glyph-sweater",
        "glyph-fabric",
        "glyph-leather",
        "glyph-scissors",
        "glyph-bomb",
        "glyph-skull",
        "glyph-celebration",
        "glyph-tea-kettle",
        "glyph-french-press",
        "glyph-coffe-cup",
        "glyph-pot",
        "glyph-grater",
        "glyph-kettle",
        "glyph-hospital",
        "glyph-hospital-h",
        "glyph-microphone",
        "glyph-webcam",
        "glyph-temple-christianity-church",
        "glyph-temple-islam",
        "glyph-temple-hindu",
        "glyph-temple-buddhist",
        "glyph-bicycle",
        "glyph-life-preserver",
        "glyph-share-alt",
        "glyph-comments",
        "glyph-flower",
        "glyph-baseball",
        "glyph-rugby",
        "glyph-ax",
        "glyph-table-tennis",
        "glyph-bowling",
        "glyph-tree-conifer",
        "glyph-tree-deciduous",
        "glyph-more-items",
        "glyph-sort",
        "glyph-filter",
        "glyph-gamepad",
        "glyph-playing-dices",
        "glyph-calculator",
        "glyph-tie",
        "glyph-wallet",
        "glyph-piano",
        "glyph-sampler",
        "glyph-podium",
        "glyph-soccer-ball",
        "glyph-blog",
        "glyph-dashboard",
        "glyph-certificate",
        "glyph-bell",
        "glyph-candle",
        "glyph-pushpin",
        "glyph-iphone-shake",
        "glyph-pin-flag",
        "glyph-turtle",
        "glyph-rabbit",
        "glyph-globe",
        "glyph-briefcase",
        "glyph-hdd",
        "glyph-thumbs-up",
        "glyph-thumbs-down",
        "glyph-hand-right",
        "glyph-hand-left",
        "glyph-hand-up",
        "glyph-hand-down",
        "glyph-fullscreen",
        "glyph-shopping-bag",
        "glyph-book-open",
        "glyph-nameplate",
        "glyph-nameplate-alt",
        "glyph-vases",
        "glyph-bullhorn",
        "glyph-dumbbell",
        "glyph-suitcase",
        "glyph-file-import",
        "glyph-file-export",
        "glyph-bug",
        "glyph-crown",
        "glyph-smoking",
        "glyph-cloud-upload",
        "glyph-cloud-download",
        "glyph-restart",
        "glyph-security-camera",
        "glyph-expand",
        "glyph-collapse",
        "glyph-collapse-top",
        "glyph-globe-af",
        "glyph-global",
        "glyph-spray",
        "glyph-nails",
        "glyph-claw-hammer",
        "glyph-classic-hammer",
        "glyph-hand-saw",
        "glyph-riflescope",
        "glyph-electrical-socket-eu",
        "glyph-electrical-socket-us",
        "glyph-message-forward",
        "glyph-coat-hanger",
        "glyph-dress",
        "glyph-bathrobe",
        "glyph-shirt",
        "glyph-underwear",
        "glyph-log-in",
        "glyph-log-out",
        "glyph-exit",
        "glyph-new-window-alt",
        "glyph-video-sd",
        "glyph-video-hd",
        "glyph-subtitles",
        "glyph-sound-stereo",
        "glyph-sound-dolby",
        "glyph-sound-5-1",
        "glyph-sound-6-1",
        "glyph-sound-7-1",
        "glyph-copyright-mark",
        "glyph-registration-mark",
        "glyph-radar",
        "glyph-skateboard",
        "glyph-golf-course",
        "glyph-sorting",
        "glyph-sort-by-alphabet",
        "glyph-sort-by-alphabet-alt",
        "glyph-sort-by-order",
        "glyph-sort-by-order-alt",
        "glyph-sort-by-attributes",
        "glyph-sort-by-attributes-alt",
        "glyph-compressed",
        "glyph-package",
        "glyph-cloud-plus",
        "glyph-cloud-minus",
        "glyph-disk-save",
        "glyph-disk-open",
        "glyph-disk-saved",
        "glyph-disk-remove",
        "glyph-disk-import",
        "glyph-disk-export",
        "glyph-tower",
        "glyph-send",
        "glyph-git-branch",
        "glyph-git-create",
        "glyph-git-private",
        "glyph-git-delete",
        "glyph-git-merge",
        "glyph-git-pull-request",
        "glyph-git-compare",
        "glyph-git-commit",
        "glyph-construction-cone",
        "glyph-shoe-steps",
        "glyph-plus",
        "glyph-minus",
        "glyph-redo",
        "glyph-undo",
        "glyph-golf",
        "glyph-hockey",
        "glyph-pipe",
        "glyph-wrench",
        "glyph-folder-closed",
        "glyph-phone-alt",
        "glyph-earphone",
        "glyph-floppy-disk",
        "glyph-floppy-saved",
        "glyph-floppy-remove",
        "glyph-floppy-save",
        "glyph-floppy-open",
        "glyph-translate",
        "glyph-fax",
        "glyph-factory",
        "glyph-shop-window",
        "glyph-shop",
        "glyph-kiosk",
        "glyph-kiosk-wheels",
        "glyph-kiosk-light",
        "glyph-kiosk-food",
        "glyph-transfer",
        "glyph-money",
        "glyph-header",
        "glyph-blacksmith",
        "glyph-saw-blade",
        "glyph-basketball",
        "glyph-server",
        "glyph-server-plus",
        "glyph-server-minus",
        "glyph-server-ban",
        "glyph-server-flag",
        "glyph-server-lock",
        "glyph-server-new"
    ];

    var dnngo_social = [

        "social-pinterest",
        "social-dropbox",
        "social-google-plus",
        "social-jolicloud",
        "social-yahoo",
        "social-blogger",
        "social-picasa",
        "social-amazon",
        "social-tumblr",
        "social-wordpress",
        "social-instapaper",
        "social-evernote",
        "social-xing",
        "social-zootool",
        "social-dribbble",
        "social-deviantart",
        "social-read-it-later",
        "social-linked-in",
        "social-forrst",
        "social-pinboard",
        "social-behance",
        "social-github",
        "social-youtube",
        "social-skitch",
        "social-foursquare",
        "social-quora",
        "social-badoo",
        "social-spotify",
        "social-stumbleupon",
        "social-readability",
        "social-facebook",
        "social-twitter",
        "social-instagram",
        "social-posterous-spaces",
        "social-vimeo",
        "social-flickr",
        "social-last-fm",
        "social-rss",
        "social-skype",
        "social-e-mail",
        "social-vine",
        "social-myspace",
        "social-goodreads",
        "social-apple",
        "social-windows",
        "social-yelp",
        "social-playstation",
        "social-xbox",
        "social-android",
        "social-ios"
    ];


})(jQuery);