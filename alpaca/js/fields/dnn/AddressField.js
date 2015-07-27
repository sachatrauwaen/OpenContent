(function ($) {

    var Alpaca = $.alpaca;

    Alpaca.Fields.AddressField = Alpaca.Fields.ObjectField.extend(
    /**
     * @lends Alpaca.Fields.AddressField.prototype
     */
    {
        /**
         * @see Alpaca.Fields.ObjectField#getFieldType
         */
        getFieldType: function () {
            return "address";
        },

        /**
         * @private
         * @see Alpaca.Fields.ObjectField#setup
         */
        setup: function () {
            this.base();

            if (this.data === undefined) {
                this.data = {
                    
                };
            }

            this.schema = {
                "title": "Address",
                "type": "object",
                "properties": {
                    "search": {
                        "title": "Search",
                        "type": "string"
                    },
                    "street": {
                        "title": "Street",
                        "type": "string"
                    },
                    "number": {
                        "title": "House Number",
                        "type": "string"
                    },
                    "city": {
                        "title": "City",
                        "type": "string"
                    },
                    "state": {
                        "title": "State",
                        "type": "string",
                        "enum": ["AL", "AK", "AS", "AZ", "AR", "CA", "CO", "CT", "DE", "DC", "FM", "FL", "GA", "GU", "HI", "ID", "IL", "IN", "IA", "KS", "KY", "LA", "ME", "MH", "MD", "MA", "MI", "MN", "MS", "MO", "MT", "NE", "NV", "NH", "NJ", "NM", "NY", "NC", "ND", "MP", "OH", "OK", "OR", "PW", "PA", "PR", "RI", "SC", "SD", "TN", "TX", "UT", "VT", "VI", "VA", "WA", "WV", "WI", "WY"]
                    },
                    "postcode": {
                        "title": "Postcode",
                        "type": "string"
                    },
                    "country": {
                        "title": "Country",
                        "type": "string"
                    },
                    "lat": {
                        "title": "Latitude",
                        "type": "number"
                    },
                    "lng": {
                        "title": "Longitude",
                        "type": "number"
                    }
                }
            };
            Alpaca.merge(this.options, {
                "fields": {
                    "search": {
                        "fieldClass": "googlesearch"
                    },
                    "street": {
                        "fieldClass": "street"
                    },
                    "number": {
                        "fieldClass": "number"
                    },
                    "city": {
                        "fieldClass": "city"
                    },
                    "postcode": {
                        "fieldClass": "postcode"
                    },
                    "state": {
                        "optionLabels": ["ALABAMA", "ALASKA", "AMERICANSAMOA", "ARIZONA", "ARKANSAS", "CALIFORNIA", "COLORADO", "CONNECTICUT", "DELAWARE", "DISTRICTOFCOLUMBIA", "FEDERATEDSTATESOFMICRONESIA", "FLORIDA", "GEORGIA", "GUAM", "HAWAII", "IDAHO", "ILLINOIS", "INDIANA", "IOWA", "KANSAS", "KENTUCKY", "LOUISIANA", "MAINE", "MARSHALLISLANDS", "MARYLAND", "MASSACHUSETTS", "MICHIGAN", "MINNESOTA", "MISSISSIPPI", "MISSOURI", "MONTANA", "NEBRASKA", "NEVADA", "NEWHAMPSHIRE", "NEWJERSEY", "NEWMEXICO", "NEWYORK", "NORTHCAROLINA", "NORTHDAKOTA", "NORTHERNMARIANAISLANDS", "OHIO", "OKLAHOMA", "OREGON", "PALAU", "PENNSYLVANIA", "PUERTORICO", "RHODEISLAND", "SOUTHCAROLINA", "SOUTHDAKOTA", "TENNESSEE", "TEXAS", "UTAH", "VERMONT", "VIRGINISLANDS", "VIRGINIA", "WASHINGTON", "WESTVIRGINIA", "WISCONSIN", "WYOMING"],
                        "fieldClass": "state",
                        "hidden": true
                    },
                    "country": {
                        "type": "country",
                        "fieldClass": "country"
                    },
                    "lat": {
                        "fieldClass": "lat"
                    },
                    "lng": {
                        "fieldClass": "lng"
                    }
                }
            });

            if (Alpaca.isEmpty(this.options.addressValidation)) {
                this.options.addressValidation = true;
            }
        },

        /**
         * @see Alpaca.Field#isContainer
         */
        isContainer: function () {
            return false;
        },

        /**
         * Returns address in a single line string.
         *
         * @returns {String} Address as a single line string.
         */
        getAddress: function () {
            var value = this.getValue();
            if (this.view.type === "view") {
                value = this.data;
            }
            var address = "";
            if (value) {
                if (value.street) {
                    address += value.street + " ";
                }
                if (value.city) {
                    address += value.city + " ";
                }
                if (value.state) {
                    address += value.state + " ";
                }
                if (value.zip) {
                    address += value.zip;
                }
                if (value.country) {
                    address += value.country;
                }
            }

            return address;
        },

        /**
         * @see Alpaca.Field#afterRenderContainer
         */
        afterRenderContainer: function (model, callback) {

            var self = this;

            this.base(model, function () {
                var container = self.getContainerEl();

                // apply additional css
                $(container).addClass("alpaca-addressfield");

                if (self.options.addressValidation && !self.isDisplayOnly()) {
                    $('<div style="clear:both;"></div>').appendTo(container);
                    var mapButton = $('<a href="#" class="alpaca-form-button">Geocode Address</a>').appendTo(container);
                    if (mapButton.button) {
                        mapButton.button({
                            text: true
                        });
                    }
                    mapButton.click(function () {

                        if (google && google.maps) {
                            var geocoder = new google.maps.Geocoder();
                            var address = self.getAddress();
                            if (geocoder) {
                                geocoder.geocode({
                                    'address': address
                                }, function (results, status) {
                                    if (status === google.maps.GeocoderStatus.OK) {
                                        /*
                                        var mapCanvasId = self.getId() + "-map-canvas";
                                        if ($('#' + mapCanvasId).length === 0) {
                                            $("<div id='" + mapCanvasId + "' class='alpaca-field-address-mapcanvas'></div>").appendTo(self.getFieldEl());
                                        }

                                        var map = new google.maps.Map(document.getElementById(self.getId() + "-map-canvas"), {
                                            "zoom": 10,
                                            "center": results[0].geometry.location,
                                            "mapTypeId": google.maps.MapTypeId.ROADMAP
                                        });

                                        var marker = new google.maps.Marker({
                                            map: map,
                                            position: results[0].geometry.location
                                        });
                                        */
                                        $(".alpaca-field.lng input.alpaca-control", container).val(results[0].geometry.location.lng());
                                        $(".alpaca-field.lat input.alpaca-control", container).val(results[0].geometry.location.lat());
                                    }
                                    else {
                                        self.displayMessage("Geocoding failed: " + status);
                                    }
                                });
                            }

                        }
                        else {
                            self.displayMessage("Google Map API is not installed.");
                        }
                        return false;
                    }).wrap('<small/>');

                    //var mapSearchId = self.getId() + "-map-search";
                    //var input = $("<input type='textbox' id='" + mapSearchId + "' class='alpaca-field-address-mapsearch'></div>").prependTo(container)[0];
                    var input = $(".alpaca-field.googlesearch input.alpaca-control", container)[0];
                    //var input = document.getElementById(mapSearchId);
                    if (input) {
                        var searchBox = new google.maps.places.SearchBox(input);
                        google.maps.event.addListener(searchBox, 'places_changed', function () {
                            var places = searchBox.getPlaces();
                            if (places.length == 0) {
                                return;
                            }
                            var place = places[0];
                            $(".alpaca-field.postcode input.alpaca-control", container).val(addressPart(place, "postal_code"));
                            $(".alpaca-field.city input.alpaca-control", container).val(addressPart(place, "locality"));
                            $(".alpaca-field.street input.alpaca-control", container).val(addressPart(place, "route"));
                            $(".alpaca-field.number input.alpaca-control", container).val(addressPart(place, "street_number"));
                            $(".alpaca-field.country select.alpaca-control", container).val(countryISO3(addressCountry(place, "country")));

                            $(".alpaca-field.lng input.alpaca-control", container).val(place.geometry.location.lng());
                            $(".alpaca-field.lat input.alpaca-control", container).val(place.geometry.location.lat());
                            input.value = '';

                        });
                        google.maps.event.addDomListener(input, 'keydown', function (e) {
                            if (e.keyCode == 13) {
                                e.preventDefault();
                            }
                        });
                    }

                    if (self.options.showMapOnLoad) {
                        mapButton.click();
                    }
                }

                callback();

            });
        },

        /**
         * @see Alpaca.Fields.ObjectField#getType
         */
        getType: function () {
            return "any";
        }


        /* builder_helpers */
        ,

        /**
         * @see Alpaca.Fields.ObjectField#getTitle
         */
        getTitle: function () {
            return "Address";
        },

        /**
         * @see Alpaca.Fields.ObjectField#getDescription
         */
        getDescription: function () {
            return "Address with Street, City, State, Zip and Country. Also comes with support for Google map.";
        },

        /**
         * @private
         * @see Alpaca.Fields.ObjectField#getSchemaOfOptions
         */
        getSchemaOfOptions: function () {
            return Alpaca.merge(this.base(), {
                "properties": {
                    "validateAddress": {
                        "title": "Address Validation",
                        "description": "Enable address validation if true",
                        "type": "boolean",
                        "default": true
                    },
                    "showMapOnLoad": {
                        "title": "Whether to show the map when first loaded",
                        "type": "boolean"
                    }
                }
            });
        },

        /**
         * @private
         * @see Alpaca.Fields.ObjectField#getOptionsForOptions
         */
        getOptionsForOptions: function () {
            return Alpaca.merge(this.base(), {
                "fields": {
                    "validateAddress": {
                        "helper": "Address validation if checked",
                        "rightLabel": "Enable Google Map for address validation?",
                        "type": "checkbox"
                    }
                }
            });
        }

        /* end_builder_helpers */
    });

    function addressPart(place, adrtype) {
        var res = "";
        if (place && place.address_components) {
            $.each(place.address_components, function (i, comp) {
                $.each(comp.types, function (i, comptype) {
                    if (comptype == adrtype) {
                        //alert(comp.long_name);
                        res = comp.long_name;
                        return;
                    }
                });
                if (res != "") return;
            });
        }
        return res;
    }
    function addressCountry(place) {
        var res = "";
        if (place && place.address_components) {
            $.each(place.address_components, function (i, comp) {
                $.each(comp.types, function (i, comptype) {
                    if (comptype == 'country') {
                        //alert(comp.long_name);
                        res = comp.short_name;
                        return;
                    }
                });
                if (res != "") return;
            });
        }
        return res;
    }

    var countries = [{ "countryName": "Afghanistan", "iso2": "AF", "iso3": "AFG", "phoneCode": "93" }, { "countryName": "Albania", "iso2": "AL", "iso3": "ALB", "phoneCode": "355" }, { "countryName": "Algeria", "iso2": "DZ", "iso3": "DZA", "phoneCode": "213" }, { "countryName": "American Samoa", "iso2": "AS", "iso3": "ASM", "phoneCode": "1 684" }, { "countryName": "Andorra", "iso2": "AD", "iso3": "AND", "phoneCode": "376" }, { "countryName": "Angola", "iso2": "AO", "iso3": "AGO", "phoneCode": "244" }, { "countryName": "Anguilla", "iso2": "AI", "iso3": "AIA", "phoneCode": "1 264" }, { "countryName": "Antarctica", "iso2": "AQ", "iso3": "ATA", "phoneCode": "672" }, { "countryName": "Antigua and Barbuda", "iso2": "AG", "iso3": "ATG", "phoneCode": "1 268" }, { "countryName": "Argentina", "iso2": "AR", "iso3": "ARG", "phoneCode": "54" }, { "countryName": "Armenia", "iso2": "AM", "iso3": "ARM", "phoneCode": "374" }, { "countryName": "Aruba", "iso2": "AW", "iso3": "ABW", "phoneCode": "297" }, { "countryName": "Australia", "iso2": "AU", "iso3": "AUS", "phoneCode": "61" }, { "countryName": "Austria", "iso2": "AT", "iso3": "AUT", "phoneCode": "43" }, { "countryName": "Azerbaijan", "iso2": "AZ", "iso3": "AZE", "phoneCode": "994" }, { "countryName": "Bahamas", "iso2": "BS", "iso3": "BHS", "phoneCode": "1 242" }, { "countryName": "Bahrain", "iso2": "BH", "iso3": "BHR", "phoneCode": "973" }, { "countryName": "Bangladesh", "iso2": "BD", "iso3": "BGD", "phoneCode": "880" }, { "countryName": "Barbados", "iso2": "BB", "iso3": "BRB", "phoneCode": "1 246" }, { "countryName": "Belarus", "iso2": "BY", "iso3": "BLR", "phoneCode": "375" }, { "countryName": "Belgium", "iso2": "BE", "iso3": "BEL", "phoneCode": "32" }, { "countryName": "Belize", "iso2": "BZ", "iso3": "BLZ", "phoneCode": "501" }, { "countryName": "Benin", "iso2": "BJ", "iso3": "BEN", "phoneCode": "229" }, { "countryName": "Bermuda", "iso2": "BM", "iso3": "BMU", "phoneCode": "1 441" }, { "countryName": "Bhutan", "iso2": "BT", "iso3": "BTN", "phoneCode": "975" }, { "countryName": "Bolivia", "iso2": "BO", "iso3": "BOL", "phoneCode": "591" }, { "countryName": "Bosnia and Herzegovina", "iso2": "BA", "iso3": "BIH", "phoneCode": "387" }, { "countryName": "Botswana", "iso2": "BW", "iso3": "BWA", "phoneCode": "267" }, { "countryName": "Brazil", "iso2": "BR", "iso3": "BRA", "phoneCode": "55" }, { "countryName": "British Indian Ocean Territory", "iso2": "IO", "iso3": "IOT", "phoneCode": "" }, { "countryName": "British Virgin Islands", "iso2": "VG", "iso3": "VGB", "phoneCode": "1 284" }, { "countryName": "Brunei", "iso2": "BN", "iso3": "BRN", "phoneCode": "673" }, { "countryName": "Bulgaria", "iso2": "BG", "iso3": "BGR", "phoneCode": "359" }, { "countryName": "Burkina Faso", "iso2": "BF", "iso3": "BFA", "phoneCode": "226" }, { "countryName": "Burma (Myanmar)", "iso2": "MM", "iso3": "MMR", "phoneCode": "95" }, { "countryName": "Burundi", "iso2": "BI", "iso3": "BDI", "phoneCode": "257" }, { "countryName": "Cambodia", "iso2": "KH", "iso3": "KHM", "phoneCode": "855" }, { "countryName": "Cameroon", "iso2": "CM", "iso3": "CMR", "phoneCode": "237" }, { "countryName": "Canada", "iso2": "CA", "iso3": "CAN", "phoneCode": "1" }, { "countryName": "Cape Verde", "iso2": "CV", "iso3": "CPV", "phoneCode": "238" }, { "countryName": "Cayman Islands", "iso2": "KY", "iso3": "CYM", "phoneCode": "1 345" }, { "countryName": "Central African Republic", "iso2": "CF", "iso3": "CAF", "phoneCode": "236" }, { "countryName": "Chad", "iso2": "TD", "iso3": "TCD", "phoneCode": "235" }, { "countryName": "Chile", "iso2": "CL", "iso3": "CHL", "phoneCode": "56" }, { "countryName": "China", "iso2": "CN", "iso3": "CHN", "phoneCode": "86" }, { "countryName": "Christmas Island", "iso2": "CX", "iso3": "CXR", "phoneCode": "61" }, { "countryName": "Cocos (Keeling) Islands", "iso2": "CC", "iso3": "CCK", "phoneCode": "61" }, { "countryName": "Colombia", "iso2": "CO", "iso3": "COL", "phoneCode": "57" }, { "countryName": "Comoros", "iso2": "KM", "iso3": "COM", "phoneCode": "269" }, { "countryName": "Cook Islands", "iso2": "CK", "iso3": "COK", "phoneCode": "682" }, { "countryName": "Costa Rica", "iso2": "CR", "iso3": "CRC", "phoneCode": "506" }, { "countryName": "Croatia", "iso2": "HR", "iso3": "HRV", "phoneCode": "385" }, { "countryName": "Cuba", "iso2": "CU", "iso3": "CUB", "phoneCode": "53" }, { "countryName": "Cyprus", "iso2": "CY", "iso3": "CYP", "phoneCode": "357" }, { "countryName": "Czech Republic", "iso2": "CZ", "iso3": "CZE", "phoneCode": "420" }, { "countryName": "Democratic Republic of the Congo", "iso2": "CD", "iso3": "COD", "phoneCode": "243" }, { "countryName": "Denmark", "iso2": "DK", "iso3": "DNK", "phoneCode": "45" }, { "countryName": "Djibouti", "iso2": "DJ", "iso3": "DJI", "phoneCode": "253" }, { "countryName": "Dominica", "iso2": "DM", "iso3": "DMA", "phoneCode": "1 767" }, { "countryName": "Dominican Republic", "iso2": "DO", "iso3": "DOM", "phoneCode": "1 809" }, { "countryName": "Ecuador", "iso2": "EC", "iso3": "ECU", "phoneCode": "593" }, { "countryName": "Egypt", "iso2": "EG", "iso3": "EGY", "phoneCode": "20" }, { "countryName": "El Salvador", "iso2": "SV", "iso3": "SLV", "phoneCode": "503" }, { "countryName": "Equatorial Guinea", "iso2": "GQ", "iso3": "GNQ", "phoneCode": "240" }, { "countryName": "Eritrea", "iso2": "ER", "iso3": "ERI", "phoneCode": "291" }, { "countryName": "Estonia", "iso2": "EE", "iso3": "EST", "phoneCode": "372" }, { "countryName": "Ethiopia", "iso2": "ET", "iso3": "ETH", "phoneCode": "251" }, { "countryName": "Falkland Islands", "iso2": "FK", "iso3": "FLK", "phoneCode": "500" }, { "countryName": "Faroe Islands", "iso2": "FO", "iso3": "FRO", "phoneCode": "298" }, { "countryName": "Fiji", "iso2": "FJ", "iso3": "FJI", "phoneCode": "679" }, { "countryName": "Finland", "iso2": "FI", "iso3": "FIN", "phoneCode": "358" }, { "countryName": "France", "iso2": "FR", "iso3": "FRA", "phoneCode": "33" }, { "countryName": "French Polynesia", "iso2": "PF", "iso3": "PYF", "phoneCode": "689" }, { "countryName": "Gabon", "iso2": "GA", "iso3": "GAB", "phoneCode": "241" }, { "countryName": "Gambia", "iso2": "GM", "iso3": "GMB", "phoneCode": "220" }, { "countryName": "Gaza Strip", "iso2": "", "iso3": "", "phoneCode": "970" }, { "countryName": "Georgia", "iso2": "GE", "iso3": "GEO", "phoneCode": "995" }, { "countryName": "Germany", "iso2": "DE", "iso3": "DEU", "phoneCode": "49" }, { "countryName": "Ghana", "iso2": "GH", "iso3": "GHA", "phoneCode": "233" }, { "countryName": "Gibraltar", "iso2": "GI", "iso3": "GIB", "phoneCode": "350" }, { "countryName": "Greece", "iso2": "GR", "iso3": "GRC", "phoneCode": "30" }, { "countryName": "Greenland", "iso2": "GL", "iso3": "GRL", "phoneCode": "299" }, { "countryName": "Grenada", "iso2": "GD", "iso3": "GRD", "phoneCode": "1 473" }, { "countryName": "Guam", "iso2": "GU", "iso3": "GUM", "phoneCode": "1 671" }, { "countryName": "Guatemala", "iso2": "GT", "iso3": "GTM", "phoneCode": "502" }, { "countryName": "Guinea", "iso2": "GN", "iso3": "GIN", "phoneCode": "224" }, { "countryName": "Guinea-Bissau", "iso2": "GW", "iso3": "GNB", "phoneCode": "245" }, { "countryName": "Guyana", "iso2": "GY", "iso3": "GUY", "phoneCode": "592" }, { "countryName": "Haiti", "iso2": "HT", "iso3": "HTI", "phoneCode": "509" }, { "countryName": "Holy See (Vatican City)", "iso2": "VA", "iso3": "VAT", "phoneCode": "39" }, { "countryName": "Honduras", "iso2": "HN", "iso3": "HND", "phoneCode": "504" }, { "countryName": "Hong Kong", "iso2": "HK", "iso3": "HKG", "phoneCode": "852" }, { "countryName": "Hungary", "iso2": "HU", "iso3": "HUN", "phoneCode": "36" }, { "countryName": "Iceland", "iso2": "IS", "iso3": "IS", "phoneCode": "354" }, { "countryName": "India", "iso2": "IN", "iso3": "IND", "phoneCode": "91" }, { "countryName": "Indonesia", "iso2": "ID", "iso3": "IDN", "phoneCode": "62" }, { "countryName": "Iran", "iso2": "IR", "iso3": "IRN", "phoneCode": "98" }, { "countryName": "Iraq", "iso2": "IQ", "iso3": "IRQ", "phoneCode": "964" }, { "countryName": "Ireland", "iso2": "IE", "iso3": "IRL", "phoneCode": "353" }, { "countryName": "Isle of Man", "iso2": "IM", "iso3": "IMN", "phoneCode": "44" }, { "countryName": "Israel", "iso2": "IL", "iso3": "ISR", "phoneCode": "972" }, { "countryName": "Italy", "iso2": "IT", "iso3": "ITA", "phoneCode": "39" }, { "countryName": "Ivory Coast", "iso2": "CI", "iso3": "CIV", "phoneCode": "225" }, { "countryName": "Jamaica", "iso2": "JM", "iso3": "JAM", "phoneCode": "1 876" }, { "countryName": "Japan", "iso2": "JP", "iso3": "JPN", "phoneCode": "81" }, { "countryName": "Jersey", "iso2": "JE", "iso3": "JEY", "phoneCode": "" }, { "countryName": "Jordan", "iso2": "JO", "iso3": "JOR", "phoneCode": "962" }, { "countryName": "Kazakhstan", "iso2": "KZ", "iso3": "KAZ", "phoneCode": "7" }, { "countryName": "Kenya", "iso2": "KE", "iso3": "KEN", "phoneCode": "254" }, { "countryName": "Kiribati", "iso2": "KI", "iso3": "KIR", "phoneCode": "686" }, { "countryName": "Kosovo", "iso2": "", "iso3": "", "phoneCode": "381" }, { "countryName": "Kuwait", "iso2": "KW", "iso3": "KWT", "phoneCode": "965" }, { "countryName": "Kyrgyzstan", "iso2": "KG", "iso3": "KGZ", "phoneCode": "996" }, { "countryName": "Laos", "iso2": "LA", "iso3": "LAO", "phoneCode": "856" }, { "countryName": "Latvia", "iso2": "LV", "iso3": "LVA", "phoneCode": "371" }, { "countryName": "Lebanon", "iso2": "LB", "iso3": "LBN", "phoneCode": "961" }, { "countryName": "Lesotho", "iso2": "LS", "iso3": "LSO", "phoneCode": "266" }, { "countryName": "Liberia", "iso2": "LR", "iso3": "LBR", "phoneCode": "231" }, { "countryName": "Libya", "iso2": "LY", "iso3": "LBY", "phoneCode": "218" }, { "countryName": "Liechtenstein", "iso2": "LI", "iso3": "LIE", "phoneCode": "423" }, { "countryName": "Lithuania", "iso2": "LT", "iso3": "LTU", "phoneCode": "370" }, { "countryName": "Luxembourg", "iso2": "LU", "iso3": "LUX", "phoneCode": "352" }, { "countryName": "Macau", "iso2": "MO", "iso3": "MAC", "phoneCode": "853" }, { "countryName": "Macedonia", "iso2": "MK", "iso3": "MKD", "phoneCode": "389" }, { "countryName": "Madagascar", "iso2": "MG", "iso3": "MDG", "phoneCode": "261" }, { "countryName": "Malawi", "iso2": "MW", "iso3": "MWI", "phoneCode": "265" }, { "countryName": "Malaysia", "iso2": "MY", "iso3": "MYS", "phoneCode": "60" }, { "countryName": "Maldives", "iso2": "MV", "iso3": "MDV", "phoneCode": "960" }, { "countryName": "Mali", "iso2": "ML", "iso3": "MLI", "phoneCode": "223" }, { "countryName": "Malta", "iso2": "MT", "iso3": "MLT", "phoneCode": "356" }, { "countryName": "Marshall Islands", "iso2": "MH", "iso3": "MHL", "phoneCode": "692" }, { "countryName": "Mauritania", "iso2": "MR", "iso3": "MRT", "phoneCode": "222" }, { "countryName": "Mauritius", "iso2": "MU", "iso3": "MUS", "phoneCode": "230" }, { "countryName": "Mayotte", "iso2": "YT", "iso3": "MYT", "phoneCode": "262" }, { "countryName": "Mexico", "iso2": "MX", "iso3": "MEX", "phoneCode": "52" }, { "countryName": "Micronesia", "iso2": "FM", "iso3": "FSM", "phoneCode": "691" }, { "countryName": "Moldova", "iso2": "MD", "iso3": "MDA", "phoneCode": "373" }, { "countryName": "Monaco", "iso2": "MC", "iso3": "MCO", "phoneCode": "377" }, { "countryName": "Mongolia", "iso2": "MN", "iso3": "MNG", "phoneCode": "976" }, { "countryName": "Montenegro", "iso2": "ME", "iso3": "MNE", "phoneCode": "382" }, { "countryName": "Montserrat", "iso2": "MS", "iso3": "MSR", "phoneCode": "1 664" }, { "countryName": "Morocco", "iso2": "MA", "iso3": "MAR", "phoneCode": "212" }, { "countryName": "Mozambique", "iso2": "MZ", "iso3": "MOZ", "phoneCode": "258" }, { "countryName": "Namibia", "iso2": "NA", "iso3": "NAM", "phoneCode": "264" }, { "countryName": "Nauru", "iso2": "NR", "iso3": "NRU", "phoneCode": "674" }, { "countryName": "Nepal", "iso2": "NP", "iso3": "NPL", "phoneCode": "977" }, { "countryName": "Netherlands", "iso2": "NL", "iso3": "NLD", "phoneCode": "31" }, { "countryName": "Netherlands Antilles", "iso2": "AN", "iso3": "ANT", "phoneCode": "599" }, { "countryName": "New Caledonia", "iso2": "NC", "iso3": "NCL", "phoneCode": "687" }, { "countryName": "New Zealand", "iso2": "NZ", "iso3": "NZL", "phoneCode": "64" }, { "countryName": "Nicaragua", "iso2": "NI", "iso3": "NIC", "phoneCode": "505" }, { "countryName": "Niger", "iso2": "NE", "iso3": "NER", "phoneCode": "227" }, { "countryName": "Nigeria", "iso2": "NG", "iso3": "NGA", "phoneCode": "234" }, { "countryName": "Niue", "iso2": "NU", "iso3": "NIU", "phoneCode": "683" }, { "countryName": "Norfolk Island", "iso2": "", "iso3": "NFK", "phoneCode": "672" }, { "countryName": "North Korea", "iso2": "KP", "iso3": "PRK", "phoneCode": "850" }, { "countryName": "Northern Mariana Islands", "iso2": "MP", "iso3": "MNP", "phoneCode": "1 670" }, { "countryName": "Norway", "iso2": "NO", "iso3": "NOR", "phoneCode": "47" }, { "countryName": "Oman", "iso2": "OM", "iso3": "OMN", "phoneCode": "968" }, { "countryName": "Pakistan", "iso2": "PK", "iso3": "PAK", "phoneCode": "92" }, { "countryName": "Palau", "iso2": "PW", "iso3": "PLW", "phoneCode": "680" }, { "countryName": "Panama", "iso2": "PA", "iso3": "PAN", "phoneCode": "507" }, { "countryName": "Papua New Guinea", "iso2": "PG", "iso3": "PNG", "phoneCode": "675" }, { "countryName": "Paraguay", "iso2": "PY", "iso3": "PRY", "phoneCode": "595" }, { "countryName": "Peru", "iso2": "PE", "iso3": "PER", "phoneCode": "51" }, { "countryName": "Philippines", "iso2": "PH", "iso3": "PHL", "phoneCode": "63" }, { "countryName": "Pitcairn Islands", "iso2": "PN", "iso3": "PCN", "phoneCode": "870" }, { "countryName": "Poland", "iso2": "PL", "iso3": "POL", "phoneCode": "48" }, { "countryName": "Portugal", "iso2": "PT", "iso3": "PRT", "phoneCode": "351" }, { "countryName": "Puerto Rico", "iso2": "PR", "iso3": "PRI", "phoneCode": "1" }, { "countryName": "Qatar", "iso2": "QA", "iso3": "QAT", "phoneCode": "974" }, { "countryName": "Republic of the Congo", "iso2": "CG", "iso3": "COG", "phoneCode": "242" }, { "countryName": "Romania", "iso2": "RO", "iso3": "ROU", "phoneCode": "40" }, { "countryName": "Russia", "iso2": "RU", "iso3": "RUS", "phoneCode": "7" }, { "countryName": "Rwanda", "iso2": "RW", "iso3": "RWA", "phoneCode": "250" }, { "countryName": "Saint Barthelemy", "iso2": "BL", "iso3": "BLM", "phoneCode": "590" }, { "countryName": "Saint Helena", "iso2": "SH", "iso3": "SHN", "phoneCode": "290" }, { "countryName": "Saint Kitts and Nevis", "iso2": "KN", "iso3": "KNA", "phoneCode": "1 869" }, { "countryName": "Saint Lucia", "iso2": "LC", "iso3": "LCA", "phoneCode": "1 758" }, { "countryName": "Saint Martin", "iso2": "MF", "iso3": "MAF", "phoneCode": "1 599" }, { "countryName": "Saint Pierre and Miquelon", "iso2": "PM", "iso3": "SPM", "phoneCode": "508" }, { "countryName": "Saint Vincent and the Grenadines", "iso2": "VC", "iso3": "VCT", "phoneCode": "1 784" }, { "countryName": "Samoa", "iso2": "WS", "iso3": "WSM", "phoneCode": "685" }, { "countryName": "San Marino", "iso2": "SM", "iso3": "SMR", "phoneCode": "378" }, { "countryName": "Sao Tome and Principe", "iso2": "ST", "iso3": "STP", "phoneCode": "239" }, { "countryName": "Saudi Arabia", "iso2": "SA", "iso3": "SAU", "phoneCode": "966" }, { "countryName": "Senegal", "iso2": "SN", "iso3": "SEN", "phoneCode": "221" }, { "countryName": "Serbia", "iso2": "RS", "iso3": "SRB", "phoneCode": "381" }, { "countryName": "Seychelles", "iso2": "SC", "iso3": "SYC", "phoneCode": "248" }, { "countryName": "Sierra Leone", "iso2": "SL", "iso3": "SLE", "phoneCode": "232" }, { "countryName": "Singapore", "iso2": "SG", "iso3": "SGP", "phoneCode": "65" }, { "countryName": "Slovakia", "iso2": "SK", "iso3": "SVK", "phoneCode": "421" }, { "countryName": "Slovenia", "iso2": "SI", "iso3": "SVN", "phoneCode": "386" }, { "countryName": "Solomon Islands", "iso2": "SB", "iso3": "SLB", "phoneCode": "677" }, { "countryName": "Somalia", "iso2": "SO", "iso3": "SOM", "phoneCode": "252" }, { "countryName": "South Africa", "iso2": "ZA", "iso3": "ZAF", "phoneCode": "27" }, { "countryName": "South Korea", "iso2": "KR", "iso3": "KOR", "phoneCode": "82" }, { "countryName": "Spain", "iso2": "ES", "iso3": "ESP", "phoneCode": "34" }, { "countryName": "Sri Lanka", "iso2": "LK", "iso3": "LKA", "phoneCode": "94" }, { "countryName": "Sudan", "iso2": "SD", "iso3": "SDN", "phoneCode": "249" }, { "countryName": "Suriname", "iso2": "SR", "iso3": "SUR", "phoneCode": "597" }, { "countryName": "Svalbard", "iso2": "SJ", "iso3": "SJM", "phoneCode": "" }, { "countryName": "Swaziland", "iso2": "SZ", "iso3": "SWZ", "phoneCode": "268" }, { "countryName": "Sweden", "iso2": "SE", "iso3": "SWE", "phoneCode": "46" }, { "countryName": "Switzerland", "iso2": "CH", "iso3": "CHE", "phoneCode": "41" }, { "countryName": "Syria", "iso2": "SY", "iso3": "SYR", "phoneCode": "963" }, { "countryName": "Taiwan", "iso2": "TW", "iso3": "TWN", "phoneCode": "886" }, { "countryName": "Tajikistan", "iso2": "TJ", "iso3": "TJK", "phoneCode": "992" }, { "countryName": "Tanzania", "iso2": "TZ", "iso3": "TZA", "phoneCode": "255" }, { "countryName": "Thailand", "iso2": "TH", "iso3": "THA", "phoneCode": "66" }, { "countryName": "Timor-Leste", "iso2": "TL", "iso3": "TLS", "phoneCode": "670" }, { "countryName": "Togo", "iso2": "TG", "iso3": "TGO", "phoneCode": "228" }, { "countryName": "Tokelau", "iso2": "TK", "iso3": "TKL", "phoneCode": "690" }, { "countryName": "Tonga", "iso2": "TO", "iso3": "TON", "phoneCode": "676" }, { "countryName": "Trinidad and Tobago", "iso2": "TT", "iso3": "TTO", "phoneCode": "1 868" }, { "countryName": "Tunisia", "iso2": "TN", "iso3": "TUN", "phoneCode": "216" }, { "countryName": "Turkey", "iso2": "TR", "iso3": "TUR", "phoneCode": "90" }, { "countryName": "Turkmenistan", "iso2": "TM", "iso3": "TKM", "phoneCode": "993" }, { "countryName": "Turks and Caicos Islands", "iso2": "TC", "iso3": "TCA", "phoneCode": "1 649" }, { "countryName": "Tuvalu", "iso2": "TV", "iso3": "TUV", "phoneCode": "688" }, { "countryName": "Uganda", "iso2": "UG", "iso3": "UGA", "phoneCode": "256" }, { "countryName": "Ukraine", "iso2": "UA", "iso3": "UKR", "phoneCode": "380" }, { "countryName": "United Arab Emirates", "iso2": "AE", "iso3": "ARE", "phoneCode": "971" }, { "countryName": "United Kingdom", "iso2": "GB", "iso3": "GBR", "phoneCode": "44" }, { "countryName": "United States", "iso2": "US", "iso3": "USA", "phoneCode": "1" }, { "countryName": "Uruguay", "iso2": "UY", "iso3": "URY", "phoneCode": "598" }, { "countryName": "US Virgin Islands", "iso2": "VI", "iso3": "VIR", "phoneCode": "1 340" }, { "countryName": "Uzbekistan", "iso2": "UZ", "iso3": "UZB", "phoneCode": "998" }, { "countryName": "Vanuatu", "iso2": "VU", "iso3": "VUT", "phoneCode": "678" }, { "countryName": "Venezuela", "iso2": "VE", "iso3": "VEN", "phoneCode": "58" }, { "countryName": "Vietnam", "iso2": "VN", "iso3": "VNM", "phoneCode": "84" }, { "countryName": "Wallis and Futuna", "iso2": "WF", "iso3": "WLF", "phoneCode": "681" }, { "countryName": "West Bank", "iso2": "", "iso3": "", "phoneCode": "970" }, { "countryName": "Western Sahara", "iso2": "EH", "iso3": "ESH", "phoneCode": "" }, { "countryName": "Yemen", "iso2": "YE", "iso3": "YEM", "phoneCode": "967" }, { "countryName": "Zambia", "iso2": "ZM", "iso3": "ZMB", "phoneCode": "260" }, { "countryName": "Zimbabwe", "iso2": "ZW", "iso3": "ZWE", "phoneCode": "263" }];

    function countryISO2(iso3) {
        for (index = 0; index < countries.length; ++index) {
            if (countries[index].iso3 === iso3) {
                return countries[index].iso2;
            }
        }
        return "";
    }

    function countryISO3(iso2) {
        for (index = 0; index < countries.length; ++index) {
            if (countries[index].iso2 === iso2) {
                return countries[index].iso3.toLowerCase();
            }
        }
        return "";
    }

    Alpaca.registerFieldClass("address", Alpaca.Fields.AddressField);

})(jQuery);