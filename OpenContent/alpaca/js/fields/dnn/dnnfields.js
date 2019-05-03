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
            /// <summary>
            /// s this instance.
            /// </summary>
            /// <returns></returns>
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
                    "postalcode": {
                        "title": "Postal Code",
                        "type": "string"
                    },
                    "country": {
                        "title": "Country",
                        "type": "string"
                    },
                    "latitude": {
                        "title": "Latitude",
                        "type": "number"
                    },
                    "longitude": {
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
                    "postalcode": {
                        "fieldClass": "postalcode"
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
                    "latitude": {
                        "fieldClass": "lat"
                    },
                    "longitude": {
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
                if (value.number) {
                    address += value.number + " ";
                }
                if (value.city) {
                    address += value.city + " ";
                }
                if (value.state) {
                    address += value.state + " ";
                }
                if (value.postalcode) {
                    address += value.postalcode +  " ";
                }
                if (value.country) {
                    address += countryName(value.country);
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
                    if (input && (typeof google != "undefined") && google && google.maps) {
                        var searchBox = new google.maps.places.SearchBox(input);
                        google.maps.event.addListener(searchBox, 'places_changed', function () {
                            var places = searchBox.getPlaces();
                            if (places.length == 0) {
                                return;
                            }
                            var place = places[0];
                            $(".alpaca-field.postalcode input.alpaca-control", container).val(addressPart(place, "postal_code"));
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
            return "Address with Street, City, State, Postal code and Country. Also comes with support for Google map.";
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
        iso2 = iso2.toUpperCase();
        for (index = 0; index < countries.length; ++index) {
            if (countries[index].iso3 === iso3) {
                return countries[index].iso2;
            }
        }
        return "";
    }

    function countryISO3(iso2) {
        iso2 = iso2.toUpperCase();
        for (index = 0; index < countries.length; ++index) {
            if (countries[index].iso2 === iso2) {
                return countries[index].iso3.toLowerCase();
            }
        }
        return "";
    }

    function countryName(iso3) {
        iso3 = iso3.toUpperCase();
        for (index = 0; index < countries.length; ++index) {
            if (countries[index].iso3 === iso3) {
                return countries[index].countryName;
            }
        }
        return "";
    }

    Alpaca.registerFieldClass("address", Alpaca.Fields.AddressField);

})(jQuery);
(function ($) {

    var Alpaca = $.alpaca;

    Alpaca.Fields.CKEditorField = Alpaca.Fields.TextAreaField.extend(
    /**
     * @lends Alpaca.Fields.CKEditorField.prototype
     */
    {
        /**
         * @see Alpaca.Fields.TextAreaField#getFieldType
         */
        getFieldType: function () {
            return "ckeditor";
        },

        constructor: function (container, data, options, schema, view, connector) {
            var self = this;
            this.base(container, data, options, schema, view, connector);
            this.sf = connector.servicesFramework;
        },

        /**
         * @see Alpaca.Fields.TextAreaField#setup
         */
        setup: function () {
            if (!this.data) {
                this.data = "";
            }

            this.base();

            if (typeof (this.options.ckeditor) == "undefined") {
                this.options.ckeditor = {};
            }
            if (typeof (this.options.configset) == "undefined") {
                this.options.configset = "";
            }
        },

        afterRenderControl: function (model, callback) {
            var self = this;

            this.base(model, function () {

                // see if we can render CK Editor
                if (!self.isDisplayOnly() && self.control && typeof (CKEDITOR) !== "undefined") {

                    var defaultConfig = {
                        toolbar: [
                             { name: 'basicstyles', groups: ['basicstyles', 'cleanup'], items: ['Bold', 'Italic', 'Underline', 'Strike', 'Subscript', 'Superscript', '-', 'RemoveFormat'] },
                             { name: 'styles', items: ['Styles', 'Format'] },
                             { name: 'paragraph', groups: ['list', 'indent', 'blocks', 'align'], items: ['NumberedList', 'BulletedList', '-', 'Outdent', 'Indent', '-', 'JustifyLeft', 'JustifyCenter', 'JustifyRight', 'JustifyBlock', ] },
                             { name: 'links', items: ['Link', 'Unlink'] },

                             { name: 'document', groups: ['mode', 'document', 'doctools'], items: ['Source'] },
                        ],
                        // Set the most common block elements.
                        format_tags: 'p;h1;h2;h3;pre',

                        // Simplify the dialog windows.
                        removeDialogTabs: 'image:advanced;link:advanced',

                        // Remove one plugin.
                        removePlugins: 'elementspath',

                        extraPlugins: 'dnnpages',

                        //autoGrow_onStartup : true,
                        //autoGrow_minHeight : 100,
                        //autoGrow_maxHeight : 300,
                        height: 150,
                        //skin : 'flat',

                        customConfig: '',
                        stylesSet: []
                    };
                    if (self.options.configset == "basic") {
                        defaultConfig = {
                            toolbar: [
                                 { name: 'basicstyles', groups: ['basicstyles', 'cleanup'], items: ['Bold', 'Italic', 'Underline', 'Strike', 'Subscript', 'Superscript', '-', 'RemoveFormat'] },
                                 { name: 'styles', items: ['Styles', 'Format'] },
                                 { name: 'paragraph', groups: ['list', 'indent', 'blocks', 'align'], items: ['NumberedList', 'BulletedList', '-', 'Outdent', 'Indent', '-', 'JustifyLeft', 'JustifyCenter', 'JustifyRight', 'JustifyBlock', ] },
                                 { name: 'links', items: ['Link', 'Unlink'] },

                                 { name: 'document', groups: ['mode', 'document', 'doctools'], items: ['Maximize', 'Source'] },
                            ],
                            // Set the most common block elements.
                            format_tags: 'p;h1;h2;h3;pre',
                            // Simplify the dialog windows.
                            removeDialogTabs: 'image:advanced;link:advanced',
                            // Remove one plugin.
                            removePlugins: 'elementspath,link',
                            extraPlugins: 'dnnpages',
                            //autoGrow_onStartup : true,
                            //autoGrow_minHeight : 100,
                            //autoGrow_maxHeight : 300,
                            height: 150,
                            //skin : 'flat',
                            customConfig: '',
                            stylesSet: []
                        };
                    } else if (self.options.configset == "standard") {
                        defaultConfig = {
                            toolbar: [
                                 { name: 'basicstyles', groups: ['basicstyles', 'cleanup'], items: ['Bold', 'Italic', 'Underline', 'Strike', 'Subscript', 'Superscript', '-', 'RemoveFormat'] },
                                 { name: 'styles', items: ['Styles', 'Format'] },
                                 { name: 'paragraph', groups: ['list', 'indent', 'blocks', 'align'], items: ['NumberedList', 'BulletedList', '-', 'Outdent', 'Indent', '-', 'JustifyLeft', 'JustifyCenter', 'JustifyRight', 'JustifyBlock', ] },
                                 { name: 'links', items: ['Link', 'Unlink', 'Anchor'] },
                                 { name: 'insert', items: ['Table', 'Smiley', 'SpecialChar', 'Iframe'] },
                                 { name: 'document', groups: ['mode', 'document', 'doctools'], items: ['Maximize', 'ShowBlocks', 'Source'] }
                            ],
                            // Set the most common block elements.
                            format_tags: 'p;h1;h2;h3;pre;div',

                            //http://docs.ckeditor.com/#!/guide/dev_allowed_content_rules
                            extraAllowedContent:
                            'table tr th td caption[*](*);' +
                            'div span(*);'
                            //'a[!href](*);' 
                            //'img[!src,alt,width,height](*);' +
                            //'h1 h2 h3 p blockquote strong em(*);' +
                            ,

                            // Simplify the dialog windows.
                            removeDialogTabs: 'image:advanced;link:advanced',
                            // Remove one plugin.
                            removePlugins: 'elementspath,link',
                            extraPlugins: 'dnnpages',
                            //autoGrow_onStartup : true,
                            //autoGrow_minHeight : 100,
                            //autoGrow_maxHeight : 300,
                            height: 150,
                            //skin : 'flat',
                            customConfig: '',
                            stylesSet: []
                        };
                    } else if (self.options.configset == "full") {
                        defaultConfig = {
                            toolbar: [                                
                                { name: 'clipboard', items: ['Cut', 'Copy', 'Paste', 'PasteText', 'PasteFromWord', '-', 'Undo', 'Redo'] },
                                { name: 'editing', items: ['Find', 'Replace', '-', 'SelectAll', '-', 'SpellChecker', 'Scayt'] },
                                { name: 'insert', items: ['EasyImageUpload', 'Table', 'HorizontalRule', 'Smiley', 'SpecialChar', 'PageBreak', 'Iframe'] },
                                '/',
                                { name: 'basicstyles', items: ['Bold', 'Italic', 'Underline', 'Strike', 'Subscript', 'Superscript', '-', 'RemoveFormat'] },
                                {
                                    name: 'paragraph', items: ['NumberedList', 'BulletedList', '-', 'Outdent', 'Indent', '-', 'Blockquote', 'CreateDiv',
                                    '-', 'JustifyLeft', 'JustifyCenter', 'JustifyRight', 'JustifyBlock', '-', 'BidiLtr', 'BidiRtl']
                                },
                                { name: 'links', items: ['Link', 'Unlink', 'Anchor'] },
                                
                                '/',
                                { name: 'styles', items: ['Styles', 'Format', 'Font', 'FontSize'] },
                                { name: 'colors', items: ['TextColor', 'BGColor'] },
                                { name: 'tools', items: ['Maximize', 'ShowBlocks', '-', 'About', '-', 'Source'] }
                            ],
                            // Set the most common block elements.
                            format_tags: 'p;h1;h2;h3;pre;div',
                            //http://docs.ckeditor.com/#!/api/CKEDITOR.config-cfg-allowedContent
                            allowedContentRules: true, 
                            // Simplify the dialog windows.
                            removeDialogTabs: 'image:advanced;link:advanced',
                            // Remove one plugin.
                            removePlugins: 'elementspath,link,image',
                            extraPlugins: 'dnnpages',
                            //autoGrow_onStartup : true,
                            //autoGrow_minHeight : 100,
                            //autoGrow_maxHeight : 300,
                            height: 150,
                            //skin : 'flat',
                            customConfig: '',
                            stylesSet: [],
                            //easyimage_toolbar :['EasyImageAlignLeft', 'EasyImageAlignCenter', 'EasyImageAlignRight']
                        };
                    }
                    var config = $.extend({}, defaultConfig, self.options.ckeditor);

                    // wait for Alpaca to declare the DOM swapped and ready before we attempt to do anything with CKEditor
                    self.on("ready", function () {
                        if (!self.editor) {
                            if (self.sf) {
                                config.cloudServices_uploadUrl = self.sf.getServiceRoot('OpenContent') + "FileUpload/UploadEasyImage";
                                config.cloudServices_tokenUrl = self.sf.getServiceRoot('OpenContent') + "FileUpload/EasyImageToken";
                            }
                            self.editor = CKEDITOR.replace($(self.control)[0], config);
                            self.initCKEditorEvents();
                        }
                    });
                }

                // if the ckeditor's dom element gets destroyed, make sure we clean up the editor instance
                $(self.control).bind('destroyed', function () {

                    if (self.editor) {
                        self.editor.removeAllListeners();
                        // catch here because CKEditor has an issue if DOM element deletes before CKEditor cleans up
                        // see: https://github.com/lemonde/angular-ckeditor/issues/7
                        try { self.editor.destroy(false); } catch (e) { }
                        self.editor = null;
                    }

                });

                callback();
            });
        },

        initCKEditorEvents: function () {
            var self = this;

            if (self.editor) {
                // click event
                self.editor.on("click", function (e) {
                    self.onClick.call(self, e);
                    self.trigger("click", e);
                });

                // change event
                self.editor.on("change", function (e) {
                    self.onChange();
                    self.triggerWithPropagation("change", e);
                });

                // blur event
                self.editor.on('blur', function (e) {
                    self.onBlur();
                    self.trigger("blur", e);
                });

                // focus event
                self.editor.on("focus", function (e) {
                    self.onFocus.call(self, e);
                    self.trigger("focus", e);
                });

                // keypress event
                self.editor.on("key", function (e) {
                    self.onKeyPress.call(self, e);
                    self.trigger("keypress", e);
                });

                // NOTE: these do not seem to work with CKEditor?
                /*
                 // keyup event
                 self.editor.on("keyup", function(e) {
                 self.onKeyUp.call(self, e);
                 self.trigger("keyup", e);
                 });

                 // keydown event
                 self.editor.on("keydown", function(e) {
                 self.onKeyDown.call(self, e);
                 self.trigger("keydown", e);
                 });
                 */

                self.editor.on('fileUploadRequest', function (evt) {
                    self.sf.setModuleHeaders(evt.data.fileLoader.xhr);
                });
            }
        },

        setValue: function (value) {
            var self = this;

            // be sure to call into base method
            this.base(value);

            if (self.editor) {
                self.editor.setData(value);
            }
        },

        /**
         * @see Alpaca.Fields.ControlField#getControlValue
         */
        getControlValue: function () {
            var self = this;

            var value = null;

            if (self.editor) {
                value = self.editor.getData();
            }

            return value;
        },

        /**
         * @see Alpaca.Field#destroy
         */
        destroy: function () {
            var self = this;

            // destroy the plugin instance
            if (self.editor) {
                self.editor.destroy();
                self.editor = null;
            }

            // call up to base method
            this.base();
        }

        /* builder_helpers */

        /**
         * @see Alpaca.Fields.TextAreaField#getTitle
         */
        ,
        getTitle: function () {
            return "CK Editor";
        },

        /**
         * @see Alpaca.Fields.TextAreaField#getDescription
         */
        getDescription: function () {
            return "Provides an instance of a CK Editor control for use in editing HTML.";
        },

        /**
         * @private
         * @see Alpaca.ControlField#getSchemaOfOptions
         */
        getSchemaOfOptions: function () {
            return Alpaca.merge(this.base(), {
                "properties": {
                    "ckeditor": {
                        "title": "CK Editor options",
                        "description": "Use this entry to provide configuration options to the underlying CKEditor plugin.",
                        "type": "any"
                    }
                }
            });
        },

        /**
         * @private
         * @see Alpaca.ControlField#getOptionsForOptions
         */
        getOptionsForOptions: function () {
            return Alpaca.merge(this.base(), {
                "fields": {
                    "ckeditor": {
                        "type": "any"
                    }
                }
            });
        }

        /* end_builder_helpers */
    });

    Alpaca.registerFieldClass("ckeditor", Alpaca.Fields.CKEditorField);

})(jQuery);
(function($) {

    var Alpaca = $.alpaca;

    Alpaca.Fields.CheckBoxField = Alpaca.ControlField.extend(
        /**
         * @lends Alpaca.Fields.CheckBoxField.prototype
         */
        {
            /**
             * @see Alpaca.Field#getFieldType
             */
            getFieldType: function() {
                return "checkbox";
            },

            /**
             * @see Alpaca.Field#setup
             */
            setup: function() {

                var self = this;

                self.base();

                if (typeof(self.options.multiple) == "undefined")
                {
                    if (self.schema.type === "array")
                    {
                        self.options.multiple = true;
                    }
                    else if (typeof(self.schema["enum"]) !== "undefined")
                    {
                        self.options.multiple = true;
                    }
                }

                if (self.options.multiple)
                {
                    // multiple mode

                    self.checkboxOptions = [];

                    // if we have enum values, copy them into checkbox options
                    if (self.getEnum())
                    {
                        // sort the enumerated values
                        self.sortEnum();

                        var optionLabels = self.getOptionLabels();

                        $.each(self.getEnum(), function (index, value) {

                            var text = value;
                            if (optionLabels)
                            {
                                if (!Alpaca.isEmpty(optionLabels[index]))
                                {
                                    text = optionLabels[index];
                                }
                                else if (!Alpaca.isEmpty(optionLabels[value]))
                                {
                                    text = optionLabels[value];
                                }
                            }

                            self.checkboxOptions.push({
                                "value": value,
                                "text": text
                            });
                        });
                    }

                    // if they provided "datasource", we copy to "dataSource"
                    if (self.options.datasource && !self.options.dataSource) {
                        self.options.dataSource = self.options.datasource;
                        delete self.options.datasource;
                    }

                    // we optionally allow the data source return values to override the schema and options
                    if (typeof(self.options.useDataSourceAsEnum) === "undefined")
                    {
                        self.options.useDataSourceAsEnum = true;
                    }
                }
                else
                {
                    // single mode

                    if (!this.options.rightLabel) {
                        this.options.rightLabel = "";
                    }
                }
            },

            prepareControlModel: function(callback)
            {
                var self = this;

                this.base(function(model) {

                    if (self.checkboxOptions)
                    {
                        model.checkboxOptions = self.checkboxOptions;
                    }

                    callback(model);
                });
            },

            /**
             * @OVERRIDE
             */
            getEnum: function()
            {
                var values = this.base();
                if (!values)
                {
                    if (this.schema && this.schema.items && this.schema.items.enum)
                    {
                        values = this.schema.items.enum;
                    }
                }

                return values;
            },

            /**
             * @OVERRIDE
             */
            getOptionLabels: function()
            {
                var values = this.base();
                if (!values)
                {
                    if (this.options && this.options.items && this.options.items.optionLabels)
                    {
                        values = this.options.items.optionLabels;
                    }
                }

                return values;
            },

            /**
             * Handler for the event that the checkbox is clicked.
             *
             * @param e Event.
             */
            onClick: function(e)
            {
                this.refreshValidationState();
            },

            /**
             * @see Alpaca.ControlField#beforeRenderControl
             */
            beforeRenderControl: function(model, callback)
            {
                var self = this;

                this.base(model, function() {

                    if (self.options.dataSource)
                    {
                        // switch to multiple mode
                        self.options.multiple = true;

                        if (!self.checkboxOptions) {
                            model.checkboxOptions = self.checkboxOptions = [];
                        }

                        // clear the array
                        self.checkboxOptions.length = 0;

                        self.invokeDataSource(self.checkboxOptions, model, function(err) {

                            if (self.options.useDataSourceAsEnum)
                            {
                                // now build out the enum and optionLabels
                                var _enum = [];
                                var _optionLabels = [];
                                for (var i = 0; i < self.checkboxOptions.length; i++)
                                {
                                    _enum.push(self.checkboxOptions[i].value);
                                    _optionLabels.push(self.checkboxOptions[i].text);
                                }

                                self.setEnum(_enum);
                                self.setOptionLabels(_optionLabels);
                            }

                            callback();
                        });
                    }
                    else
                    {
                        callback();
                    }

                });
            },


            /**
             * @see Alpaca.ControlField#postRender
             */
            postRender: function(callback) {

                var self = this;

                this.base(function() {

                    // do this little trick so that if we have a default value, it gets set during first render
                    // this causes the checked state of the control to update
                    if (self.data && typeof(self.data) !== "undefined")
                    {
                        self.setValue(self.data);
                    }

                    // for multiple mode, mark values
                    if (self.options.multiple)
                    {
                        // none checked
                        $(self.getFieldEl()).find("input:checkbox").prop("checked", false);

                        if (self.data)
                        {
                            var dataArray = self.data;
                            if (typeof(self.data) === "string")
                            {
                                dataArray = self.data.split(",");
                                for (var a = 0; a < dataArray.length; a++)
                                {
                                    dataArray[a] = $.trim(dataArray[a]);
                                }
                            }

                            for (var k in dataArray)
                            {
                                $(self.getFieldEl()).find("input:checkbox[data-checkbox-value=\"" + dataArray[k] + "\"]").prop("checked", true);
                            }
                        }
                    }

                    // single mode

                    // whenever the state of one of our input:checkbox controls is changed (either via a click or programmatically),
                    // we signal to the top-level field to fire up a change
                    //
                    // this allows the dependency system to recalculate and such
                    //
                    $(self.getFieldEl()).find("input:checkbox").change(function(evt) {
                        self.triggerWithPropagation("change");
                    });

                    callback();
                });
            },

            /**
             * @see Alpaca.Field#getValue
             */
            getControlValue: function()
            {
                var self = this;

                var value = null;

                if (!self.options.multiple)
                {
                    // single scalar value
                    var input = $(self.getFieldEl()).find("input");
                    if (input.length > 0)
                    {
                        value = Alpaca.checked($(input[0]));
                    }
                    else
                    {
                        value = false;
                    }
                }
                else
                {
                    // multiple values
                    var values = [];
                    for (var i = 0; i < self.checkboxOptions.length; i++)
                    {
                        var inputField = $(self.getFieldEl()).find("input[data-checkbox-index='" + i + "']");
                        if (Alpaca.checked(inputField))
                        {
                            var v = $(inputField).attr("data-checkbox-value");
                            values.push(v);
                        }
                    }

                    // determine how we're going to hand this value back

                    // if type == "array", we just hand back the array
                    // if type == "string", we build a comma-delimited list
                    if (self.schema.type === "array")
                    {
                        value = values;
                    }
                    else if (self.schema.type === "string")
                    {
                        value = values.join(",");
                    }
                }

                return value;
            },
            isEmpty: function () {
                var self = this;
                var val = this.getControlValue();
                if (!self.options.multiple) {
                    return !val;
                }
                else {
                    if (self.schema.type === "array") {
                        return val.length == 0;
                    }
                    else if (self.schema.type === "string") {
                        return Alpaca.isEmpty(val);
                    }
                }
            },

            /**
             * @see Alpaca.Field#setValue
             */
            setValue: function(value)
            {
                var self = this;

                // value can be a boolean, string ("true"), string ("a,b,c") or an array of values

                var applyScalarValue = function(value)
                {
                    if (Alpaca.isString(value)) {
                        value = (value === "true");
                    }

                    var input = $(self.getFieldEl()).find("input");
                    if (input.length > 0)
                    {
                        Alpaca.checked($(input[0]), value);
                    }
                };

                var applyMultiValue = function(values)
                {
                    // allow for comma-delimited strings
                    if (typeof(values) === "string")
                    {
                        values = values.split(",");
                    }

                    // trim things to remove any excess white space
                    for (var i = 0; i < values.length; i++)
                    {
                        values[i] = Alpaca.trim(values[i]);
                    }

                    // walk through values and assign into appropriate inputs
                    Alpaca.checked($(self.getFieldEl()).find("input[data-checkbox-value]"), false);
                    for (var j = 0; j < values.length; j++)
                    {
                        var input = $(self.getFieldEl()).find("input[data-checkbox-value=\"" + values[j] + "\"]");
                        if (input.length > 0)
                        {
                            Alpaca.checked($(input[0]), value);
                        }
                    }
                };

                var applied = false;

                if (!self.options.multiple)
                {
                    // single value mode

                    // boolean
                    if (typeof(value) === "boolean")
                    {
                        applyScalarValue(value);
                        applied = true;
                    }
                    else if (typeof(value) === "string")
                    {
                        applyScalarValue(value);
                        applied = true;
                    }
                }
                else
                {
                    // multiple value mode

                    if (typeof(value) === "string")
                    {
                        applyMultiValue(value);
                        applied = true;
                    }
                    else if (Alpaca.isArray(value))
                    {
                        applyMultiValue(value);
                        applied = true;
                    }
                }

                if (!applied && value)
                {
                    Alpaca.logError("CheckboxField cannot set value for schema.type=" + self.schema.type + " and value=" + value);
                }

                // be sure to call into base method
                this.base(value);
            },

            /**
             * Validate against enum property in the case that the checkbox field is in multiple mode.
             *
             * @returns {Boolean} True if the element value is part of the enum list, false otherwise.
             */
            _validateEnum: function()
            {
                var self = this;

                if (!self.options.multiple)
                {
                    return true;
                }

                var val = self.getValue();
                if (!self.isRequired() && Alpaca.isValEmpty(val))
                {
                    return true;
                }

                // if val is a string, convert to array
                if (typeof(val) === "string")
                {
                    val = val.split(",");
                }

                return Alpaca.anyEquality(val, self.getEnum());
            },

            /**
             * @see Alpaca.Field#disable
             */
            disable: function()
            {
                $(this.control).find("input").each(function() {
                    $(this).disabled = true;
                    $(this).prop("disabled", true);
                });
            },

            /**
             * @see Alpaca.Field#enable
             */
            enable: function()
            {
                $(this.control).find("input").each(function() {
                    $(this).disabled = false;
                    $(this).prop("disabled", false);
                });
            },

            /**
             * @see Alpaca.Field#getType
             */
            getType: function() {
                return "boolean";
            },


            /* builder_helpers */

            /**
             * @see Alpaca.Field#getTitle
             */
            getTitle: function() {
                return "Checkbox Field";
            },

            /**
             * @see Alpaca.Field#getDescription
             */
            getDescription: function() {
                return "Checkbox Field for boolean (true/false), string ('true', 'false' or comma-delimited string of values) or data array.";
            },

            /**
             * @private
             * @see Alpaca.ControlField#getSchemaOfOptions
             */
            getSchemaOfOptions: function() {
                return Alpaca.merge(this.base(), {
                    "properties": {
                        "rightLabel": {
                            "title": "Option Label",
                            "description": "Optional right-hand side label for single checkbox field.",
                            "type": "string"
                        },
                        "multiple": {
                            "title": "Multiple",
                            "description": "Whether to render multiple checkboxes for multi-valued type (such as an array or a comma-delimited string)",
                            "type": "boolean"
                        },
                        "dataSource": {
                            "title": "Option DataSource",
                            "description": "Data source for generating list of options.  This can be a string or a function.  If a string, it is considered to be a URI to a service that produces a object containing key/value pairs or an array of elements of structure {'text': '', 'value': ''}.  This can also be a function that is called to produce the same list.",
                            "type": "string"
                        },
                        "useDataSourceAsEnum": {
                            "title": "Use Data Source as Enumerated Values",
                            "description": "Whether to constrain the field's schema enum property to the values that come back from the data source.",
                            "type": "boolean",
                            "default": true
                        }
                    }
                });
            },

            /**
             * @private
             * @see Alpaca.ControlField#getOptionsForOptions
             */
            getOptionsForOptions: function() {
                return Alpaca.merge(this.base(), {
                    "fields": {
                        "rightLabel": {
                            "type": "text"
                        },
                        "multiple": {
                            "type": "checkbox"
                        },
                        "dataSource": {
                            "type": "text"
                        }
                    }
                });
            }

            /* end_builder_helpers */

        });

    Alpaca.registerFieldClass("checkbox", Alpaca.Fields.CheckBoxField);
    Alpaca.registerDefaultSchemaFieldMapping("boolean", "checkbox");

})(jQuery);
(function ($) {

    // NOTE: this requires bootstrap-datetimepicker.js
    // NOTE: this requires moment.js

    var Alpaca = $.alpaca;

    Alpaca.Fields.DateField = Alpaca.Fields.TextField.extend(
    /**
     * @lends Alpaca.Fields.DateField.prototype
     */
    {
        /**
         * @see Alpaca.Fields.TextField#getFieldType
         */
        getFieldType: function () {
            return "date";
        },

        getDefaultFormat: function () {
            return "MM/DD/YYYY";
        },

        getDefaultExtraFormats: function () {
            return [];
        },

        /**
         * @see Alpaca.Fields.TextField#setup
         */
        setup: function () {
            var self = this;

            // default html5 input type = "date";
            //this.inputType = "date";

            this.base();

            if (!self.options.picker) {
                self.options.picker = {};
            }

            if (typeof (self.options.picker.useCurrent) === "undefined") {
                self.options.picker.useCurrent = false;
            }

            // date format

            /*
            if (self.options.picker.format) {
                self.options.dateFormat = self.options.picker.format;
            }
            */
            if (!self.options.dateFormat) {
                //self.options.dateFormat = self.getDefaultFormat();
            }
            if (!self.options.picker.format) {
                self.options.picker.format = self.options.dateFormat;
            }

            // extra formats
            if (!self.options.picker.extraFormats) {
                var extraFormats = self.getDefaultExtraFormats();
                if (extraFormats) {
                    self.options.picker.extraFormats = extraFormats;
                }
            }

            if (typeof (self.options.manualEntry) === "undefined") {
                self.options.manualEntry = false;
            }
            if (typeof (self.options.icon) === "undefined") {
                self.options.icon = false;
            }
        },

        onKeyPress: function (e) {
            if (this.options.manualEntry) {
                e.preventDefault();
                e.stopImmediatePropagation();
            }
            else {
                this.base(e);
                return;
            }
        },

        onKeyDown: function (e) {
            if (this.options.manualEntry) {
                e.preventDefault();
                e.stopImmediatePropagation();
            }
            else {
                this.base(e);
                return;
            }
        },

        beforeRenderControl: function (model, callback) {
            this.field.css("position", "relative");

            callback();
        },

        /**
         * @see Alpaca.Fields.TextField#afterRenderControl
         */
        afterRenderControl: function (model, callback) {

            var self = this;

            this.base(model, function () {

                if (self.view.type !== "display") {
                    $component = self.getControlEl();
                    if (self.options.icon) {
                        self.getControlEl().wrap('<div class="input-group date"></div>');
                        self.getControlEl().after('<span class="input-group-addon"><span class="glyphicon glyphicon-calendar"></span></span>');
                        var $component = self.getControlEl().parent();
                    }

                    if ($.fn.datetimepicker) {
                        
                        $component.datetimepicker(self.options.picker);
                        self.picker = $component.data("DateTimePicker");

                        $component.on("dp.change", function (e) {

                            // we use a timeout here because we want this to run AFTER control click handlers
                            setTimeout(function () {
                                self.onChange.call(self, e);
                                self.triggerWithPropagation("change", e);
                            }, 250);

                        });

                        // set value if provided
                        if (self.data) {
                            self.picker.date(self.data);
                        }
                    }
                }
                callback();
            });
        },

        /**
         * Returns field value as a JavaScript Date.
         *
         * @returns {Date} Field value.
         */
        getDate: function () {
            var self = this;

            var date = null;
            try {
                if (self.picker) {
                    date = (self.picker.date() ? self.picker.date()._d : null);
                }
                else {
                    date = new Date(this.getValue());
                }
            }
            catch (e) {
                console.error(e);
            }

            return date;
        },

        /**
         * Returns field value as a JavaScript Date.
         *
         * @returns {Date} Field value.
         */
        date: function () {
            return this.getDate();
        },

        /**
         * @see Alpaca.Field#onChange
         */
        onChange: function (e) {
            this.base();

            this.refreshValidationState();
        },

        isAutoFocusable: function () {
            return false;
        },

        /**
         * @see Alpaca.Fields.TextField#handleValidate
         */
        handleValidate: function () {
            var baseStatus = this.base();

            var valInfo = this.validation;

            var status = this._validateDateFormat();
            valInfo["invalidDate"] = {
                "message": status ? "" : Alpaca.substituteTokens(this.getMessage("invalidDate"), [this.options.dateFormat]),
                "status": status
            };

            return baseStatus && valInfo["invalidDate"]["status"];
        },

        /**
         * Validates date format.
         *
         * @returns {Boolean} True if it is a valid date, false otherwise.
         */
        _validateDateFormat: function () {
            var self = this;

            var isValid = true;

            if (self.options.dateFormat) {
                var value = self.getValue();
                if (value || self.isRequired()) {
                    // collect all formats
                    var dateFormats = [];
                    dateFormats.push(self.options.dateFormat);
                    if (self.options.picker && self.options.picker.extraFormats) {
                        for (var i = 0; i < self.options.picker.extraFormats.length; i++) {
                            dateFormats.push(self.options.picker.extraFormats[i]);
                        }
                    }

                    for (var i = 0; i < dateFormats.length; i++) {
                        isValid = isValid || moment(value, self.options.dateFormat, true).isValid();
                    }
                }
            }

            return isValid;
        },

        /**
         * @see Alpaca.Fields.TextField#setValue
         */
        setValue: function (value) {
            var self = this;
            if (value == "now") {
                value = moment().format();
            } else if (value == "today") {
                value = moment().startOf('day').format();
            }
            this.base(value);
            if (this.picker) {
                if (self.options.dateFormat) {
                    if (moment(value, self.options.dateFormat, true).isValid()) {
                        this.picker.date(value);
                    }
                }
                else {
                    if (moment(value).isValid()) {
                        this.picker.date(moment(value));
                    }
                }
            }
        },

        /**
         * @see Alpaca.Fields.TextField#getValue
         */
        getValue: function () {
            
            var self = this;

            var date = null;
            try {
                if (self.picker) {
                    date = (self.picker.date() ? self.picker.date().format("YYYY-MM-DDTHH:mm:ss") : null);
                }
                else {
                    date = this.base();
                }
            }
            catch (e) {
                console.error(e);
            }

            return date;

        },

        destroy: function () {
            this.base();

            this.picker = null;
        }


        /* builder_helpers */
        ,

        /**
         * @see Alpaca.Fields.TextField#getTitle
         */
        getTitle: function () {
            return "Date Field";
        },

        /**
         * @see Alpaca.Fields.TextField#getDescription
         */
        getDescription: function () {
            return "Date Field";
        },

        /**
         * @private
         * @see Alpaca.Fields.TextField#getSchemaOfSchema
         */
        getSchemaOfSchema: function () {
            return Alpaca.merge(this.base(), {
                "properties": {
                    "format": {
                        "title": "Format",
                        "description": "Property data format",
                        "type": "string",
                        "default": "date",
                        "enum": ["date"],
                        "readonly": true
                    }
                }
            });
        },

        /**
         * @private
         * @see Alpaca.Fields.TextField#getOptionsForSchema
         */
        getOptionsForSchema: function () {
            return Alpaca.merge(this.base(), {
                "fields": {
                    "format": {
                        "type": "text"
                    }
                }
            });
        },

        /**
         * @private
         * @see Alpaca.Fields.TextField#getSchemaOfOptions
         */
        getSchemaOfOptions: function () {
            return Alpaca.merge(this.base(), {
                "properties": {
                    "dateFormat": {
                        "title": "Date Format",
                        "description": "Date format (using moment.js format)",
                        "type": "string"
                    },
                    "picker": {
                        "title": "DatetimePicker options",
                        "description": "Options that are supported by the <a href='http://eonasdan.github.io/bootstrap-datetimepicker/'>Bootstrap DateTime Picker</a>.",
                        "type": "any"
                    }
                }
            });
        },

        /**
         * @private
         * @see Alpaca.Fields.TextField#getOptionsForOptions
         */
        getOptionsForOptions: function () {
            return Alpaca.merge(this.base(), {
                "fields": {
                    "dateFormat": {
                        "type": "text"
                    },
                    "picker": {
                        "type": "any"
                    }
                }
            });
        }

        /* end_builder_helpers */
    });

    Alpaca.registerMessages({
        "invalidDate": "Invalid date for format {0}"
    });
    Alpaca.registerFieldClass("date", Alpaca.Fields.DateField);
    Alpaca.registerDefaultFormatFieldMapping("date", "date");

})(jQuery);
(function($) {

    var Alpaca = $.alpaca;

    Alpaca.Fields.CheckBoxField = Alpaca.ControlField.extend(
        /**
         * @lends Alpaca.Fields.CheckBoxField.prototype
         */
        {
            /**
             * @see Alpaca.Field#getFieldType
             */
            getFieldType: function() {
                return "checkbox";
            },

            /**
             * @see Alpaca.Field#setup
             */
            setup: function() {

                var self = this;

                self.base();

                if (typeof(self.options.multiple) == "undefined")
                {
                    if (self.schema.type === "array")
                    {
                        self.options.multiple = true;
                    }
                    else if (typeof(self.schema["enum"]) !== "undefined")
                    {
                        self.options.multiple = true;
                    }
                }

                if (self.options.multiple)
                {
                    // multiple mode

                    self.checkboxOptions = [];

                    // if we have enum values, copy them into checkbox options
                    if (self.getEnum())
                    {
                        // sort the enumerated values
                        self.sortEnum();

                        var optionLabels = self.getOptionLabels();

                        $.each(self.getEnum(), function (index, value) {

                            var text = value;
                            if (optionLabels)
                            {
                                if (!Alpaca.isEmpty(optionLabels[index]))
                                {
                                    text = optionLabels[index];
                                }
                                else if (!Alpaca.isEmpty(optionLabels[value]))
                                {
                                    text = optionLabels[value];
                                }
                            }

                            self.checkboxOptions.push({
                                "value": value,
                                "text": text
                            });
                        });
                    }

                    // if they provided "datasource", we copy to "dataSource"
                    if (self.options.datasource && !self.options.dataSource) {
                        self.options.dataSource = self.options.datasource;
                        delete self.options.datasource;
                    }

                    // we optionally allow the data source return values to override the schema and options
                    if (typeof(self.options.useDataSourceAsEnum) === "undefined")
                    {
                        self.options.useDataSourceAsEnum = true;
                    }
                }
                else
                {
                    // single mode

                    if (!this.options.rightLabel) {
                        this.options.rightLabel = "";
                    }
                }
            },

            prepareControlModel: function(callback)
            {
                var self = this;

                this.base(function(model) {

                    if (self.checkboxOptions)
                    {
                        model.checkboxOptions = self.checkboxOptions;
                    }

                    callback(model);
                });
            },

            /**
             * @OVERRIDE
             */
            getEnum: function()
            {
                var values = this.base();
                if (!values)
                {
                    if (this.schema && this.schema.items && this.schema.items.enum)
                    {
                        values = this.schema.items.enum;
                    }
                }

                return values;
            },

            /**
             * @OVERRIDE
             */
            getOptionLabels: function()
            {
                var values = this.base();
                if (!values)
                {
                    if (this.options && this.options.items && this.options.items.optionLabels)
                    {
                        values = this.options.items.optionLabels;
                    }
                }

                return values;
            },

            /**
             * Handler for the event that the checkbox is clicked.
             *
             * @param e Event.
             */
            onClick: function(e)
            {
                this.refreshValidationState();
            },

            /**
             * @see Alpaca.ControlField#beforeRenderControl
             */
            beforeRenderControl: function(model, callback)
            {
                var self = this;

                this.base(model, function() {

                    if (self.options.dataSource)
                    {
                        // switch to multiple mode
                        self.options.multiple = true;

                        if (!self.checkboxOptions) {
                            model.checkboxOptions = self.checkboxOptions = [];
                        }

                        // clear the array
                        self.checkboxOptions.length = 0;

                        self.invokeDataSource(self.checkboxOptions, model, function(err) {

                            if (self.options.useDataSourceAsEnum)
                            {
                                // now build out the enum and optionLabels
                                var _enum = [];
                                var _optionLabels = [];
                                for (var i = 0; i < self.checkboxOptions.length; i++)
                                {
                                    _enum.push(self.checkboxOptions[i].value);
                                    _optionLabels.push(self.checkboxOptions[i].text);
                                }

                                self.setEnum(_enum);
                                self.setOptionLabels(_optionLabels);
                            }

                            callback();
                        });
                    }
                    else
                    {
                        callback();
                    }

                });
            },


            /**
             * @see Alpaca.ControlField#postRender
             */
            postRender: function(callback) {

                var self = this;

                this.base(function() {

                    // do this little trick so that if we have a default value, it gets set during first render
                    // this causes the checked state of the control to update
                    if (self.data && typeof(self.data) !== "undefined")
                    {
                        self.setValue(self.data);
                    }

                    // for multiple mode, mark values
                    if (self.options.multiple)
                    {
                        // none checked
                        $(self.getFieldEl()).find("input:checkbox").prop("checked", false);

                        if (self.data)
                        {
                            var dataArray = self.data;
                            if (typeof(self.data) === "string")
                            {
                                dataArray = self.data.split(",");
                                for (var a = 0; a < dataArray.length; a++)
                                {
                                    dataArray[a] = $.trim(dataArray[a]);
                                }
                            }

                            for (var k in dataArray)
                            {
                                $(self.getFieldEl()).find("input:checkbox[data-checkbox-value=\"" + dataArray[k] + "\"]").prop("checked", true);
                            }
                        }
                    }

                    // single mode

                    // whenever the state of one of our input:checkbox controls is changed (either via a click or programmatically),
                    // we signal to the top-level field to fire up a change
                    //
                    // this allows the dependency system to recalculate and such
                    //
                    $(self.getFieldEl()).find("input:checkbox").change(function(evt) {
                        self.triggerWithPropagation("change");
                    });

                    callback();
                });
            },

            /**
             * @see Alpaca.Field#getValue
             */
            getControlValue: function()
            {
                var self = this;

                var value = null;

                if (!self.options.multiple)
                {
                    // single scalar value
                    var input = $(self.getFieldEl()).find("input");
                    if (input.length > 0)
                    {
                        value = Alpaca.checked($(input[0]));
                    }
                    else
                    {
                        value = false;
                    }
                }
                else
                {
                    // multiple values
                    var values = [];
                    for (var i = 0; i < self.checkboxOptions.length; i++)
                    {
                        var inputField = $(self.getFieldEl()).find("input[data-checkbox-index='" + i + "']");
                        if (Alpaca.checked(inputField))
                        {
                            var v = $(inputField).attr("data-checkbox-value");
                            values.push(v);
                        }
                    }

                    // determine how we're going to hand this value back

                    // if type == "array", we just hand back the array
                    // if type == "string", we build a comma-delimited list
                    if (self.schema.type === "array")
                    {
                        value = values;
                    }
                    else if (self.schema.type === "string")
                    {
                        value = values.join(",");
                    }
                }

                return value;
            },
            isEmpty: function () {
                var self = this;
                var val = this.getControlValue();
                if (!self.options.multiple) {
                    return !val;
                }
                else {
                    if (self.schema.type === "array") {
                        return val.length == 0;
                    }
                    else if (self.schema.type === "string") {
                        return Alpaca.isEmpty(val);
                    }
                }
            },

            /**
             * @see Alpaca.Field#setValue
             */
            setValue: function(value)
            {
                var self = this;

                // value can be a boolean, string ("true"), string ("a,b,c") or an array of values

                var applyScalarValue = function(value)
                {
                    if (Alpaca.isString(value)) {
                        value = (value === "true");
                    }

                    var input = $(self.getFieldEl()).find("input");
                    if (input.length > 0)
                    {
                        Alpaca.checked($(input[0]), value);
                    }
                };

                var applyMultiValue = function(values)
                {
                    // allow for comma-delimited strings
                    if (typeof(values) === "string")
                    {
                        values = values.split(",");
                    }

                    // trim things to remove any excess white space
                    for (var i = 0; i < values.length; i++)
                    {
                        values[i] = Alpaca.trim(values[i]);
                    }

                    // walk through values and assign into appropriate inputs
                    Alpaca.checked($(self.getFieldEl()).find("input[data-checkbox-value]"), false);
                    for (var j = 0; j < values.length; j++)
                    {
                        var input = $(self.getFieldEl()).find("input[data-checkbox-value=\"" + values[j] + "\"]");
                        if (input.length > 0)
                        {
                            Alpaca.checked($(input[0]), value);
                        }
                    }
                };

                var applied = false;

                if (!self.options.multiple)
                {
                    // single value mode

                    // boolean
                    if (typeof(value) === "boolean")
                    {
                        applyScalarValue(value);
                        applied = true;
                    }
                    else if (typeof(value) === "string")
                    {
                        applyScalarValue(value);
                        applied = true;
                    }
                }
                else
                {
                    // multiple value mode

                    if (typeof(value) === "string")
                    {
                        applyMultiValue(value);
                        applied = true;
                    }
                    else if (Alpaca.isArray(value))
                    {
                        applyMultiValue(value);
                        applied = true;
                    }
                }

                if (!applied && value)
                {
                    Alpaca.logError("CheckboxField cannot set value for schema.type=" + self.schema.type + " and value=" + value);
                }

                // be sure to call into base method
                this.base(value);
            },

            /**
             * Validate against enum property in the case that the checkbox field is in multiple mode.
             *
             * @returns {Boolean} True if the element value is part of the enum list, false otherwise.
             */
            _validateEnum: function()
            {
                var self = this;

                if (!self.options.multiple)
                {
                    return true;
                }

                var val = self.getValue();
                if (!self.isRequired() && Alpaca.isValEmpty(val))
                {
                    return true;
                }

                // if val is a string, convert to array
                if (typeof(val) === "string")
                {
                    val = val.split(",");
                }

                return Alpaca.anyEquality(val, self.getEnum());
            },

            /**
             * @see Alpaca.Field#disable
             */
            disable: function()
            {
                $(this.control).find("input").each(function() {
                    $(this).disabled = true;
                    $(this).prop("disabled", true);
                });
            },

            /**
             * @see Alpaca.Field#enable
             */
            enable: function()
            {
                $(this.control).find("input").each(function() {
                    $(this).disabled = false;
                    $(this).prop("disabled", false);
                });
            },

            /**
             * @see Alpaca.Field#getType
             */
            getType: function() {
                return "boolean";
            },


            /* builder_helpers */

            /**
             * @see Alpaca.Field#getTitle
             */
            getTitle: function() {
                return "Checkbox Field";
            },

            /**
             * @see Alpaca.Field#getDescription
             */
            getDescription: function() {
                return "Checkbox Field for boolean (true/false), string ('true', 'false' or comma-delimited string of values) or data array.";
            },

            /**
             * @private
             * @see Alpaca.ControlField#getSchemaOfOptions
             */
            getSchemaOfOptions: function() {
                return Alpaca.merge(this.base(), {
                    "properties": {
                        "rightLabel": {
                            "title": "Option Label",
                            "description": "Optional right-hand side label for single checkbox field.",
                            "type": "string"
                        },
                        "multiple": {
                            "title": "Multiple",
                            "description": "Whether to render multiple checkboxes for multi-valued type (such as an array or a comma-delimited string)",
                            "type": "boolean"
                        },
                        "dataSource": {
                            "title": "Option DataSource",
                            "description": "Data source for generating list of options.  This can be a string or a function.  If a string, it is considered to be a URI to a service that produces a object containing key/value pairs or an array of elements of structure {'text': '', 'value': ''}.  This can also be a function that is called to produce the same list.",
                            "type": "string"
                        },
                        "useDataSourceAsEnum": {
                            "title": "Use Data Source as Enumerated Values",
                            "description": "Whether to constrain the field's schema enum property to the values that come back from the data source.",
                            "type": "boolean",
                            "default": true
                        }
                    }
                });
            },

            /**
             * @private
             * @see Alpaca.ControlField#getOptionsForOptions
             */
            getOptionsForOptions: function() {
                return Alpaca.merge(this.base(), {
                    "fields": {
                        "rightLabel": {
                            "type": "text"
                        },
                        "multiple": {
                            "type": "checkbox"
                        },
                        "dataSource": {
                            "type": "text"
                        }
                    }
                });
            }

            /* end_builder_helpers */

        });

    Alpaca.registerFieldClass("checkbox", Alpaca.Fields.CheckBoxField);
    Alpaca.registerDefaultSchemaFieldMapping("boolean", "checkbox");

})(jQuery);
(function ($) {

    var Alpaca = $.alpaca;

    Alpaca.Fields.MultiUploadField = Alpaca.Fields.ArrayField.extend(
    {
        constructor: function (container, data, options, schema, view, connector) {
            var self = this;
            this.base(container, data, options, schema, view, connector);
            this.sf = connector.servicesFramework;
            this.itemsCount = 0;
        },
        setup: function () {

            this.base();
            if (!this.options.uploadfolder) {
                this.options.uploadfolder = "";
            }
            this.urlfield = "";
            if (this.options && this.options.items && (this.options.items.fields || this.options.items.type) ) {
                if (this.options.items.type == "image") {

                } else if (this.options.items.fields ) {
                    for (var i in this.options.items.fields) {
                        var f = this.options.items.fields[i];
                        if (f.type == "image" || f.type == "mlimage" || f.type == "imagecrop") {
                            this.urlfield = i;
                            this.options.uploadfolder = f.uploadfolder;
                            break;
                        }
                        else if (f.type == "file" || f.type == "mlfile") {
                            this.urlfield = i;
                            this.options.uploadfolder = f.uploadfolder;
                            break;
                        } else if (f.type == "image2" || f.type == "mlimage2") {
                            this.urlfield = i;
                            this.options.uploadfolder = f.folder;
                            break;
                        }
                        else if (f.type == "file2" || f.type == "mlfile2") {
                            this.urlfield = i;
                            this.options.uploadfolder = f.folder;
                            break;
                        }
                    }
                }
            }
        },
        afterRenderContainer: function (model, callback) {
            var self = this;
            this.base(model, function () {
                var container = self.getContainerEl();
                //$(container).addClass("alpaca-MultiUpload");
                if (!self.isDisplayOnly() ) {
                   
                    $('<div style="clear:both;"></div>').prependTo(container);
                    var progressBar = $('<div class="progress" ><div class="bar" style="width: 0%;"></div></div>').prependTo(container);
                    var mapButton = $('<input type="file" multiple="multiple" />').prependTo(container);

                    this.wrapper = $("<span class='dnnInputFileWrapper dnnSecondaryAction' style='margin-bottom:10px;;'></span>");
                    this.wrapper.text("Upload muliple files");
                    mapButton.wrap(this.wrapper);
                    if (self.sf){
                    mapButton.fileupload({
                        dataType: 'json',
                        url: self.sf.getServiceRoot('OpenContent') + "FileUpload/UploadFile",
                        maxFileSize: 25000000,
                        formData: { uploadfolder: self.options.uploadfolder },
                        beforeSend: self.sf.setModuleHeaders,
                        change: function (e, data) {
                            self.itemsCount = self.children.length;
                        },
                        add: function (e, data) {
                            //data.context = $(opts.progressContextSelector);
                            //data.context.find($(opts.progressFileNameSelector)).html(data.files[0].name);
                            //data.context.show('fade');
                            data.submit();
                        },
                        progressall: function (e, data) {
                                var progress = parseInt(data.loaded / data.total * 100, 10);
                                $('.bar', progressBar).css('width', progress + '%').find('span').html(progress + '%');
                        },
                        done: function (e, data) {
                            if (data.result) {
                                $.each(data.result, function (index, file) {
                                    self.handleActionBarAddItemClick(self.itemsCount-1, function (item) {
                                        var val = item.getValue();
                                        if (self.urlfield == ""){
                                            val = file.url;
                                        }  else {
                                            val[self.urlfield] = file.url;
                                        }
                                        item.setValue(val);
                                        
                                    });
                                    self.itemsCount++;
                                });
                            }
                        }
                    }).data('loaded', true);
                    }
                }
                callback();
            });
        },
        getTitle: function () {
            return "Multi Upload";
        },
        getDescription: function () {
            return "Multi Upload for images and files";
        },

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

    });

    Alpaca.registerFieldClass("multiupload", Alpaca.Fields.MultiUploadField);

})(jQuery);
(function ($) {

    var Alpaca = $.alpaca;

    Alpaca.Fields.DocumentsField = Alpaca.Fields.MultiUploadField.extend(
    {
        setup: function () {
            this.base();
            this.schema.items = {
                "type": "object",
                "properties": {
                    "Title": {
                        "title": "Title",
                        "type": "string"
                    },
                    "File": {
                        "title": "File",
                        "type": "string"
                    },
                }
            };
            Alpaca.merge(this.options.items, {
                "fields": {
                    "File": {
                        "type": "file"
                    },
                }
            });
            this.urlfield = "File";
        },
        getTitle: function () {
            return "Gallery";
        },
        getDescription: function () {
            return "Image Gallery";
        },

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

    });
    Alpaca.registerFieldClass("documents", Alpaca.Fields.DocumentsField);

})(jQuery);
(function($) {

    var Alpaca = $.alpaca;
    
    Alpaca.Fields.File2Field = Alpaca.Fields.ListField.extend(
    /**
     * @lends Alpaca.Fields.File2Field.prototype
     */
    {
        constructor: function (container, data, options, schema, view, connector) {
            var self = this;
            this.base(container, data, options, schema, view, connector);
            this.sf = connector.servicesFramework;
            this.dataSource = {};
        },
        /**
         * @see Alpaca.Field#getFieldType
         */
        getFieldType: function()
        {
            return "file2";
        },

        /**
         * @see Alpaca.Fields.File2Field#setup
         */
        setup: function()
        {
            var self = this;
            if (self.schema["type"] && self.schema["type"] === "array") {
                self.options.multiple = true;
                self.options.removeDefaultNone = true;
                //self.options.hideNone = true;
            }
            if (!this.options.folder) {
                this.options.folder = "";
            }
            // filter = serverside c# regexp
            // exemple :  ^.*\.(jpg|JPG|gif|GIF|doc|DOC|pdf|PDF)$
            if (!this.options.filter) {
                this.options.filter = "";
            }
            if (!this.options.showUrlUpload) {
                this.options.showUrlUpload = false;
            }
            if (!this.options.showFileUpload) {
                this.options.showFileUpload = false;
            }
            if (this.options.showUrlUpload) {
                this.options.buttons = {
                    "downloadButton": {
                        "value": "Upload External File",
                        "click": function () {
                            this.DownLoadFile();
                        }
                    }
                };
            }
            var self = this;
            if (this.options.lazyLoading) {
                var pageSize = 10;
                this.options.select2 = {
                    ajax: {
                        url: this.sf.getServiceRoot("OpenContent") + "DnnEntitiesAPI" + "/" + "FilesLookup",
                        beforeSend: this.sf.setModuleHeaders,
                        type: "get",
                        dataType: 'json',
                        delay: 250,
                        data: function (params) {
                            return {
                                q: params.term ? params.term : "*", // search term
                                d: self.options.folder, 
                                filter: self.options.filter,
                                pageIndex: params.page ? params.page : 1,
                                pageSize: pageSize
                            };
                        },
                        processResults: function (data, params) {
                            params.page = params.page || 1;
                            if (params.page == 1) {
                                data.items.unshift({
                                    id: "",
                                    text: self.options.noneLabel
                                })
                            }
                            return {
                                results: data.items,
                                pagination: {
                                    more: (params.page * pageSize) < data.total
                                }
                            };
                        },
                        cache: true
                    },
                    escapeMarkup: function (markup) { return markup; },
                    minimumInputLength: 0
                }
            };
            this.base();
        },

        getValue: function () {
            if (this.control && this.control.length > 0) {
                var val = $(this.control).find('select').val();
                if (typeof (val) === "undefined") {
                    val = this.data;
                }
                else if (Alpaca.isArray(val)) {
                    for (var i = 0; i < val.length; i++) {
                        val[i] = this.ensureProperType(val[i]);
                    }
                }
                return val;
                //return this.base(val);
            }
            return null;
        },

        /**
         * @see Alpaca.Field#setValue
         */
        setValue: function(val)
        {
            if (Alpaca.isArray(val))
            {
                if (!Alpaca.compareArrayContent(val, this.getValue()))
                {
                    if (!Alpaca.isEmpty(val) && this.control)
                    {
                        $select = $(this.control).find('select');
                        $select.val(val);
                        $select.trigger('change.select2');
                    }
                    this.base(val);
                }
            }
            else
            {
                if (val !== this.getValue())
                {
                    /*
                    if (!Alpaca.isEmpty(val) && this.control)
                    {
                        this.control.val(val);
                    }
                    */
                    if (this.control && typeof(val) != "undefined" && val != null)
                    {
                        $select = $(this.control).find('select');
                        $select.val(val);
                        $select.trigger('change.select2');
                    }
                    this.base(val);
                }
            }
        },

        /**
         * @see Alpaca.File2Field#getEnum
         */
        getEnum: function()
        {
            if (this.schema)
            {
                if (this.schema["enum"])
                {
                    return this.schema["enum"];
                }
                else if (this.schema["type"] && this.schema["type"] === "array" && this.schema["items"] && this.schema["items"]["enum"])
                {
                    return this.schema["items"]["enum"];
                }
            }
        },
        /*
        initControlEvents: function()
        {
            var self = this;

            self.base();

            if (self.options.multiple)
            {
                var button = this.control.parent().find(".select2-search__field");

                button.focus(function(e) {
                    if (!self.suspendBlurFocus)
                    {
                        self.onFocus.call(self, e);
                        self.trigger("focus", e);
                    }
                });

                button.blur(function(e) {
                    if (!self.suspendBlurFocus)
                    {
                        self.onBlur.call(self, e);
                        self.trigger("blur", e);
                    }
                });
                this.control.on("change", function (e) {
                    self.onChange.call(self, e);
                    self.trigger("change", e);

                });
            }
        },
        */
        beforeRenderControl: function(model, callback)
        {
            var self = this;

            this.base(model, function() {
                self.selectOptions = [];
                if (self.sf) {
                    var completionFunction = function () {
                        self.schema.enum = [];
                        self.options.optionLabels = [];
                        for (var i = 0; i < self.selectOptions.length; i++) {
                            self.schema.enum.push(self.selectOptions[i].value);
                            self.options.optionLabels.push(self.selectOptions[i].text);
                        }
                        // push back to model
                        model.selectOptions = self.selectOptions;
                        callback();
                    };
                    if (self.options.lazyLoading) {
                        if (self.data) {
                            self.getFileUrl(self.data, function (data) {
                                self.selectOptions.push({
                                    "value": self.data,
                                    "text": data.text
                                });
                                self.dataSource[self.data] = data.text;
                                completionFunction();
                            });
                        } else {
                            completionFunction();
                        }
                    }
                    else {
                        var postData = { q: "*", d: self.options.folder, filter: self.options.filter };
                        $.ajax({
                            url: self.sf.getServiceRoot("OpenContent") + "DnnEntitiesAPI" + "/" + "FilesLookup",
                            beforeSend: self.sf.setModuleHeaders,
                            type: "get",
                            dataType: "json",
                            //contentType: "application/json; charset=utf-8",
                            data: postData,
                            success: function (jsonDocument) {
                                var ds = jsonDocument;
                                if (self.options.dsTransformer && Alpaca.isFunction(self.options.dsTransformer)) {
                                    ds = self.options.dsTransformer(ds);
                                }
                                if (ds) {
                                    if (Alpaca.isObject(ds)) {
                                        // for objects, we walk through one key at a time
                                        // the insertion order is the order of the keys from the map
                                        // to preserve order, consider using an array as below
                                        $.each(ds, function (key, value) {
                                            self.selectOptions.push({
                                                "value": key,
                                                "text": value
                                            });
                                        });
                                        completionFunction();
                                    }
                                    else if (Alpaca.isArray(ds)) {
                                        // for arrays, we walk through one index at a time
                                        // the insertion order is dictated by the order of the indices into the array
                                        // this preserves order
                                        $.each(ds, function (index, value) {
                                            self.selectOptions.push({
                                                "value": value.value,
                                                "text": value.text
                                            });
                                            self.dataSource[value.value] = value;
                                        });
                                        completionFunction();
                                    }
                                }
                            },
                            "error": function (jqXHR, textStatus, errorThrown) {

                                self.errorCallback({
                                    "message": "Unable to load data from uri : " + self.options.dataSource,
                                    "stage": "DATASOURCE_LOADING_ERROR",
                                    "details": {
                                        "jqXHR": jqXHR,
                                        "textStatus": textStatus,
                                        "errorThrown": errorThrown
                                    }
                                });
                            }
                        });
                    }
                }
                else {
                    callback();
                }
            });
        },

        prepareControlModel: function(callback)
        {
            var self = this;

            this.base(function(model) {

                model.selectOptions = self.selectOptions;

                callback(model);
            });
        },

        afterRenderControl: function(model, callback)
        {
            var self = this;

            this.base(model, function() {

                // if emptySelectFirst and nothing currently checked, then pick first item in the value list
                // set data and visually select it
                if (Alpaca.isUndefined(self.data) && self.options.emptySelectFirst && self.selectOptions && self.selectOptions.length > 0)
                {
                    self.data = self.selectOptions[0].value;
                }

                // do this little trick so that if we have a default value, it gets set during first render
                // this causes the state of the control
                if (self.data)
                {
                    self.setValue(self.data);
                }

                // if we are in multiple mode and the bootstrap multiselect plugin is available, bind it in
                //if (self.options.multiple && $.fn.multiselect)
                if ($.fn.select2)
                {
                    var settings = null;
                    if (self.options.select2) {
                        settings = self.options.select2;
                    }
                    else
                    {
                        settings = {};
                    }
                    /*
                    if (!settings.nonSelectedText)
                    {
                        settings.nonSelectedText = "None";
                        if (self.options.noneLabel)
                        {
                            settings.nonSelectedText = self.options.noneLabel;
                        }
                    }
                    if (self.options.hideNone)
                    {
                        delete settings.nonSelectedText;
                    }
                    */

                    
                    settings.templateResult = function (state) {

                        if (state.loading) return state.text;

                        //if (!state.id) { return state.text; }
                        
                        var $state = $(
                            '<span>' + state.text + '</span>'
                        );
                        return $state;
                    };

                    settings.templateSelection = function (state) {
                        if (!state.id) { return state.text; }
                        
                        var $state = $(
                            '<span>' + state.text + '</span>'
                        );
                        return $state;
                    };
                    
                    $('select', self.getControlEl()).select2(settings);
                }

                if (self.options.uploadhidden) {
                    $(self.getControlEl()).find('input[type=file]').hide();
                } else {
                    if (self.sf) {
                        $(self.getControlEl()).find('input[type=file]').fileupload({
                            dataType: 'json',
                            url: self.sf.getServiceRoot('OpenContent') + "FileUpload/UploadFile",
                            maxFileSize: 25000000,
                            formData: { uploadfolder: self.options.folder },
                            beforeSend: self.sf.setModuleHeaders,
                            add: function (e, data) {
                                //data.context = $(opts.progressContextSelector);
                                //data.context.find($(opts.progressFileNameSelector)).html(data.files[0].name);
                                //data.context.show('fade');

                                if (data && data.files && data.files.length > 0) {

                                    if (self.isFilter(data.files[0].name)) {
                                        data.submit();
                                    }
                                    else{
                                        alert("file not in filter");
                                        return;
                                    }
                                }
                                
                            },
                            progress: function (e, data) {
                                if (data.context) {
                                    var progress = parseInt(data.loaded / data.total * 100, 10);
                                    data.context.find(opts.progressBarSelector).css('width', progress + '%').find('span').html(progress + '%');
                                }
                            },
                            done: function (e, data) {
                                if (data.result) {
                                    $.each(data.result, function (index, file) {
                                        $select = $(self.control).find('select');
                                        if (self.options.lazyLoading) {
                                            self.getFileUrl(file.id, function (f) {
                                                $select.find("option").first().val(f.id).text(f.text).removeData();
                                                $select.val(file.id).change();
                                            });
                                        }
                                        else {
                                            self.refresh(function () {
                                                //self.setValue(file.id);
                                                $select = $(self.control).find('select');
                                                $select.val(file.id).change();
                                            });
                                        }
                                    });
                                }
                            }
                        }).data('loaded', true);
                    }
                }
                callback();
            });
        },

        getFileUrl : function(fileid, callback){
            var self = this;
            if (self.sf){
                var postData = { fileid: fileid, folder: self.options.folder };
                $.ajax({
                    url: self.sf.getServiceRoot("OpenContent") + "DnnEntitiesAPI" + "/" + "FileInfo",
                    beforeSend: self.sf.setModuleHeaders,
                    type: "get",
                    asych : false,
                    dataType: "json",
                    //contentType: "application/json; charset=utf-8",
                    data: postData,
                    success: function (data) {
                        if (callback) callback(data);
                    },
                    error: function (jqXHR, textStatus, errorThrown) {
                        alert("Error getFileUrl " + fileid);
                    }
                });
            }
        },

        /**
         * Validate against enum property.
         *
         * @returns {Boolean} True if the element value is part of the enum list, false otherwise.
         */
        _validateEnum: function()
        {
            var _this = this;

            if (this.schema["enum"])
            {
                var val = this.data;

                if (!this.isRequired() && Alpaca.isValEmpty(val))
                {
                    return true;
                }

                if (this.options.multiple)
                {
                    var isValid = true;

                    if (!val)
                    {
                        val = [];
                    }

                    if (!Alpaca.isArray(val) && !Alpaca.isObject(val))
                    {
                        val = [val];
                    }

                    $.each(val, function(i,v) {
                        /*
                        if ($.inArray(v, _this.schema["enum"]) <= -1)
                        {
                            isValid = false;
                            return false;
                        }
                        */
                    });

                    return isValid;
                }
                else
                {
                    //return ($.inArray(val, this.schema["enum"]) > -1);
                    return true;
                }
            }
            else
            {
                return true;
            }
        },

        /**
         * @see Alpaca.Field#onChange
         */
        onChange: function(e)
        {
            this.base(e);

            var _this = this;

            Alpaca.later(25, this, function() {
                var v = _this.getValue();
                _this.setValue(v);
                _this.refreshValidationState();
            });
        },

       
        /**
         * @see Alpaca.Field#focus
         */
        focus: function(onFocusCallback)
        {
            if (this.control && this.control.length > 0)
            {
                // set focus onto the select
                var el = $(this.control).get(0);

                el.focus();

                if (onFocusCallback)
                {
                    onFocusCallback(this);
                }
            }
        },
        getTextControlEl: function () {
            var self = this;
            return $(self.getControlEl()).find('input[type=text]');
        },

        DownLoadFile: function () {
            var self = this;
            var el = this.getTextControlEl();
            var data = el.val();
            if (!data || !self.isURL(data)) {
                alert("url not valid");
                return;
            }
            if (!self.isFilter(data)) {
                alert("url not in filter");
                return;
            }
            
            var postData = { url: data, uploadfolder: self.options.folder };
            $(self.getControlEl()).css('cursor', 'wait');
            $.ajax({
                type: "POST",
                url: self.sf.getServiceRoot('OpenContent') + "DnnEntitiesAPI/DownloadFile",
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                data: JSON.stringify(postData),
                beforeSend: self.sf.setModuleHeaders
            }).done(function (res) {
                if (res.error) {
                    alert(res.error);
                } else {                    
                    $select = $(self.control).find('select');
                    if (self.options.lazyLoading) {
                        self.getFileUrl(res.id, function (f) {
                            $select.find("option").first().val(f.id).text(f.text).removeData();
                            $select.val(res.id).change();
                        });
                    }
                    else {
                        self.refresh(function () {
                            //self.setValue(file.id);
                            $select = $(self.control).find('select');
                            $select.val(res.id).change();
                        });
                    }
                }
                setTimeout(function () {
                    $(self.getControlEl()).css('cursor', 'initial');
                }, 500);
            }).fail(function (xhr, result, status) {
                alert("Uh-oh, something broke: " + status);
                $(self.getControlEl()).css('cursor', 'initial');
            });
        },
        isURL: function (str) {
            var urlRegex = '^(?!mailto:)(?:(?:http|https|ftp)://)(?:\\S+(?::\\S*)?@)?(?:(?:(?:[1-9]\\d?|1\\d\\d|2[01]\\d|22[0-3])(?:\\.(?:1?\\d{1,2}|2[0-4]\\d|25[0-5])){2}(?:\\.(?:[0-9]\\d?|1\\d\\d|2[0-4]\\d|25[0-4]))|(?:(?:[a-z\\u00a1-\\uffff0-9]+-?)*[a-z\\u00a1-\\uffff0-9]+)(?:\\.(?:[a-z\\u00a1-\\uffff0-9]+-?)*[a-z\\u00a1-\\uffff0-9]+)*(?:\\.(?:[a-z\\u00a1-\\uffff]{2,})))|localhost)(?::\\d{2,5})?(?:(/|\\?|#)[^\\s]*)?$';
            var url = new RegExp(urlRegex, 'i');
            return str.length < 2083 && url.test(str);
        },
        isFilter: function (str) {
            if (this.options.filter) {                
                var url = new RegExp(this.options.filter, 'i');
                return str.length < 2083 && url.test(str);
            }
            return true;            
        },

        /**
         * @see Alpaca.Field#getTitle
         */
        getTitle: function() {
            return "Select File Field";
        },

        /**
         * @see Alpaca.Field#getDescription
         */
        getDescription: function() {
            return "Select File Field";
        },

        /**
         * @private
         * @see Alpaca.Fields.File2Field#getSchemaOfOptions
         */
        getSchemaOfOptions: function() {
            return Alpaca.merge(this.base(), {
                "properties": {
                    "multiple": {
                        "title": "Mulitple Selection",
                        "description": "Allow multiple selection if true.",
                        "type": "boolean",
                        "default": false
                    },
                    "size": {
                        "title": "Displayed Options",
                        "description": "Number of options to be shown.",
                        "type": "number"
                    },
                    "emptySelectFirst": {
                        "title": "Empty Select First",
                        "description": "If the data is empty, then automatically select the first item in the list.",
                        "type": "boolean",
                        "default": false
                    },
                    "multiselect": {
                        "title": "Multiselect Plugin Settings",
                        "description": "Multiselect plugin properties - http://davidstutz.github.io/bootstrap-multiselect",
                        "type": "any"
                    }
                }
            });
        },

        /**
         * @private
         * @see Alpaca.Fields.File2Field#getOptionsForOptions
         */
        getOptionsForOptions: function() {
            return Alpaca.merge(this.base(), {
                "fields": {
                    "multiple": {
                        "rightLabel": "Allow multiple selection ?",
                        "helper": "Allow multiple selection if checked",
                        "type": "checkbox"
                    },
                    "size": {
                        "type": "integer"
                    },
                    "emptySelectFirst": {
                        "type": "checkbox",
                        "rightLabel": "Empty Select First"
                    },
                    "multiselect": {
                        "type": "object",
                        "rightLabel": "Multiselect plugin properties - http://davidstutz.github.io/bootstrap-multiselect"
                    }
                }
            });
        }

        /* end_builder_helpers */

    });

    Alpaca.registerFieldClass("file2", Alpaca.Fields.File2Field);

})(jQuery);
(function ($) {

    var Alpaca = $.alpaca;

    Alpaca.Fields.FileField = Alpaca.Fields.TextField.extend(
    /**
     * @lends Alpaca.Fields.ImageField.prototype
     */
    {
        constructor: function (container, data, options, schema, view, connector) {
            var self = this;
            this.base(container, data, options, schema, view, connector);
            this.sf = connector.servicesFramework;
        },

        /**
         * @see Alpaca.Fields.TextField#getFieldType
         */
        getFieldType: function () {
            return "file";
        }
        ,
        setup: function () {
            if (!this.options.uploadfolder) {
                this.options.uploadfolder = "";
            }
            if (!this.options.downloadButton) {
                this.options.downloadButton = false;
            }
            if (this.options.downloadButton) {
                this.options.buttons = {
                    "downloadButton": {
                        "value": "Download",
                        "click": function () {
                            this.DownLoadFile();
                        }
                    }
                };
            }
            this.base();
        },

        /**
         * @see Alpaca.Fields.TextField#getTitle
         */
        getTitle: function () {
            return "File Field";
        },

        /**
         * @see Alpaca.Fields.TextField#getDescription
         */
        getDescription: function () {
            return "File Field.";
        },
        getTextControlEl: function () {
            return $(this.control.get(0)).find('input[type=text]#' + this.id);
        },
        setValue: function (value) {

            //var el = $( this.control).filter('#'+this.id);
            //var el = $(this.control.get(0)).find('input[type=text]');
            var el = this.getTextControlEl();

            if (el && el.length > 0) {
                if (Alpaca.isEmpty(value)) {
                    el.val("");
                }
                else {
                    //if (value) value = value.split("?")[0];
                    el.val(value);
                }
            }

            // be sure to call into base method
            //this.base(value);

            // if applicable, update the max length indicator
            this.updateMaxLengthIndicator();
        },

        getValue: function () {
            var value = null;

            //var el = $(this.control).filter('#' + this.id);
            //var el = $(this.control.get(0)).find('input[type=text]');
            var el = this.getTextControlEl();
            if (el && el.length > 0) {
                value = el.val();
            }
            return value;
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

            //var el = this.control;
            var el = this.getTextControlEl();
            if (self.sf) {

                $(this.control.get(0)).find('input[type=file]').fileupload({
                    dataType: 'json',
                    url: self.sf.getServiceRoot('OpenContent') + "FileUpload/UploadFile",
                    maxFileSize: 25000000,
                    formData: { uploadfolder: self.options.uploadfolder },
                    beforeSend: self.sf.setModuleHeaders,
                    add: function (e, data) {
                        //data.context = $(opts.progressContextSelector);
                        //data.context.find($(opts.progressFileNameSelector)).html(data.files[0].name);
                        //data.context.show('fade');
                        data.submit();
                    },
                    progress: function (e, data) {
                        if (data.context) {
                            var progress = parseInt(data.loaded / data.total * 100, 10);
                            data.context.find(opts.progressBarSelector).css('width', progress + '%').find('span').html(progress + '%');
                        }
                    },
                    done: function (e, data) {
                        if (data.result) {
                            $.each(data.result, function (index, file) {
                                self.setValue(file.url);
                                $(el).change();
                                //$(el).change();
                                //$(e.target).parent().find('input[type=text]').val(file.url);
                                //el.val(file.url);
                                //$(e.target).parent().find('.alpaca-image-display img').attr('src', file.url);
                            });
                        }
                    }
                }).data('loaded', true);
            }

            callback();
        },
        applyTypeAhead: function () {
            var self = this;

            if (self.control.typeahead && self.options.typeahead && !Alpaca.isEmpty(self.options.typeahead)  && self.sf) {

                var tConfig = self.options.typeahead.config;
                if (!tConfig) {
                    tConfig = {};
                }
                var tDatasets = tDatasets = {};
                if (!tDatasets.name) {
                    tDatasets.name = self.getId();
                }

                var tFolder = self.options.typeahead.Folder;
                if (!tFolder) {
                    tFolder = "";
                }

                var tEvents = tEvents = {};

                var bloodHoundConfig = {
                    datumTokenizer: function (d) {
                        return Bloodhound.tokenizers.whitespace(d.value);
                    },
                    queryTokenizer: Bloodhound.tokenizers.whitespace
                };

                /*
                if (tDatasets.type === "prefetch") {
                    bloodHoundConfig.prefetch = {
                        url: tDatasets.source,
                        ajax: {
                            //url: sf.getServiceRoot('OpenContent') + "FileUpload/UploadFile",
                            beforeSend: connector.servicesFramework.setModuleHeaders,
        
                        }
                    };
        
                    if (tDatasets.filter) {
                        bloodHoundConfig.prefetch.filter = tDatasets.filter;
                    }
                }
                */

                bloodHoundConfig.remote = {
                    url: self.sf.getServiceRoot('OpenContent') + "DnnEntitiesAPI/Files?q=%QUERY&d=" + tFolder,
                    ajax: {
                        beforeSend: self.sf.setModuleHeaders,
                    }
                };

                if (tDatasets.filter) {
                    bloodHoundConfig.remote.filter = tDatasets.filter;
                }

                if (tDatasets.replace) {
                    bloodHoundConfig.remote.replace = tDatasets.replace;
                }


                var engine = new Bloodhound(bloodHoundConfig);
                engine.initialize();
                tDatasets.source = engine.ttAdapter();

                tDatasets.templates = {
                    "empty": "Nothing found...",
                    "suggestion": "{{name}}"
                };

                // compile templates
                if (tDatasets.templates) {
                    for (var k in tDatasets.templates) {
                        var template = tDatasets.templates[k];
                        if (typeof (template) === "string") {
                            tDatasets.templates[k] = Handlebars.compile(template);
                        }
                    }
                }

                //var el = $(this.control.get(0)).find('input[type=text]');
                var el = this.getTextControlEl();
                // process typeahead
                $(el).typeahead(tConfig, tDatasets);

                // listen for "autocompleted" event and set the value of the field
                $(el).on("typeahead:autocompleted", function (event, datum) {
                    self.setValue(datum.value);
                    $(el).change();
                    //$(self.control).parent().find('input[type=text]').val(datum.value);
                    //$(self.control).parent().find('.alpaca-image-display img').attr('src', datum.value);
                });

                // listen for "selected" event and set the value of the field
                $(el).on("typeahead:selected", function (event, datum) {
                    self.setValue(datum.value);
                    $(el).change();
                    //$(self.control).parent().find('input[type=text]').val(datum.value);
                    //$(self.control).parent().find('.alpaca-image-display img').attr('src', datum.value);
                });

                // custom events
                if (tEvents) {
                    if (tEvents.autocompleted) {
                        $(el).on("typeahead:autocompleted", function (event, datum) {
                            tEvents.autocompleted(event, datum);
                        });
                    }
                    if (tEvents.selected) {
                        $(el).on("typeahead:selected", function (event, datum) {
                            tEvents.selected(event, datum);
                        });
                    }
                }

                // when the input value changes, change the query in typeahead
                // this is to keep the typeahead control sync'd with the actual dom value
                // only do this if the query doesn't already match
                //var fi = $(self.control);
                $(el).change(function () {

                    var value = $(this).val();

                    var newValue = $(el).typeahead('val');
                    if (newValue !== value) {
                        $(el).typeahead('val', value);
                    }

                });

                // some UI cleanup (we don't want typeahead to restyle)
                $(self.field).find("span.twitter-typeahead").first().css("display", "block"); // SPAN to behave more like DIV, next line
                $(self.field).find("span.twitter-typeahead input.tt-input").first().css("background-color", "");
            }
        },

        DownLoadFile: function () {
            var self = this;
            var el = this.getTextControlEl();
            var data = self.getValue();
            var urlPattern = new RegExp("^(http[s]?:\\/\\/(www\\.)?|ftp:\\/\\/(www\\.)?|(www\‌​\.)?){1}([0-9A-Za-z-‌​\\.@:%_\+~#=]+)+((\\‌​.[a-zA-Z]{2,3})+)(/(‌​.)*)?(\\?(.)*)?");
            if (!data || !self.isURL(data)) {
                alert("url not valid");
                return;
            }
            var postData = { url: data, uploadfolder: self.options.uploadfolder };
            $(self.getControlEl()).css('cursor', 'wait');
            $.ajax({
                type: "POST",
                url: self.sf.getServiceRoot('OpenContent') + "DnnEntitiesAPI/DownloadFile",
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                data: JSON.stringify(postData),
                beforeSend: self.sf.setModuleHeaders
            }).done(function (res) {
                if (res.error) {
                    alert(res.error);
                }else {
                    self.setValue(res.url);
                    $(el).change();
                }
                setTimeout(function () {
                    $(self.getControlEl()).css('cursor', 'initial');
                }, 500);
            }).fail(function (xhr, result, status) {
                alert("Uh-oh, something broke: " + status);
                $(self.getControlEl()).css('cursor', 'initial');
            });
        },
        isURL: function (str) {
            var urlRegex = '^(?!mailto:)(?:(?:http|https|ftp)://)(?:\\S+(?::\\S*)?@)?(?:(?:(?:[1-9]\\d?|1\\d\\d|2[01]\\d|22[0-3])(?:\\.(?:1?\\d{1,2}|2[0-4]\\d|25[0-5])){2}(?:\\.(?:[0-9]\\d?|1\\d\\d|2[0-4]\\d|25[0-4]))|(?:(?:[a-z\\u00a1-\\uffff0-9]+-?)*[a-z\\u00a1-\\uffff0-9]+)(?:\\.(?:[a-z\\u00a1-\\uffff0-9]+-?)*[a-z\\u00a1-\\uffff0-9]+)*(?:\\.(?:[a-z\\u00a1-\\uffff]{2,})))|localhost)(?::\\d{2,5})?(?:(/|\\?|#)[^\\s]*)?$';
            var url = new RegExp(urlRegex, 'i');
            return str.length < 2083 && url.test(str);
        }
        /* end_builder_helpers */
    });

    Alpaca.registerFieldClass("file", Alpaca.Fields.FileField);

})(jQuery);
(function($) {

    var Alpaca = $.alpaca;
    
    Alpaca.Fields.Folder2Field = Alpaca.Fields.ListField.extend(
    /**
     * @lends Alpaca.Fields.File2Field.prototype
     */
    {
        constructor: function (container, data, options, schema, view, connector) {
            var self = this;
            this.base(container, data, options, schema, view, connector);
            this.sf = connector.servicesFramework;
            this.dataSource = {};
        },
        /**
         * @see Alpaca.Field#getFieldType
         */
        getFieldType: function()
        {
            return "select";
        },

        /**
         * @see Alpaca.Fields.File2Field#setup
         */
        setup: function()
        {
            var self = this;
            if (self.schema["type"] && self.schema["type"] === "array") {
                self.options.multiple = true;
                self.options.removeDefaultNone = true;
                //self.options.hideNone = true;
            }
            if (!this.options.folder) {
                this.options.folder = "";
            }
            // filter = serverside c# regexp
            // exemple :  ^.*\.(jpg|JPG|gif|GIF|doc|DOC|pdf|PDF)$
            if (!this.options.filter) {
                this.options.filter = "";
            }
            this.base();
        },

        getValue: function () {
            if (this.control && this.control.length > 0) {
                var val = this._getControlVal(true);
                if (typeof (val) === "undefined") {
                    val = this.data;
                }
                else if (Alpaca.isArray(val)) {
                    for (var i = 0; i < val.length; i++) {
                        val[i] = this.ensureProperType(val[i]);
                    }
                }

                return this.base(val);
            }
        },

        /**
         * @see Alpaca.Field#setValue
         */
        setValue: function(val)
        {
            if (Alpaca.isArray(val))
            {
                if (!Alpaca.compareArrayContent(val, this.getValue()))
                {
                    if (!Alpaca.isEmpty(val) && this.control)
                    {
                        this.control.val(val);
                    }
                    this.base(val);
                }
            }
            else
            {
                if (val !== this.getValue())
                {
                    /*
                    if (!Alpaca.isEmpty(val) && this.control)
                    {
                        this.control.val(val);
                    }
                    */
                    if (this.control && typeof(val) != "undefined" && val != null)
                    {
                        this.control.val(val);
                    }
                    this.base(val);
                }
            }
        },

        /**
         * @see Alpaca.File2Field#getEnum
         */
        getEnum: function()
        {
            if (this.schema)
            {
                if (this.schema["enum"])
                {
                    return this.schema["enum"];
                }
                else if (this.schema["type"] && this.schema["type"] === "array" && this.schema["items"] && this.schema["items"]["enum"])
                {
                    return this.schema["items"]["enum"];
                }
            }
        },

        initControlEvents: function()
        {
            var self = this;

            self.base();

            if (self.options.multiple)
            {
                var button = this.control.parent().find(".select2-search__field");

                button.focus(function(e) {
                    if (!self.suspendBlurFocus)
                    {
                        self.onFocus.call(self, e);
                        self.trigger("focus", e);
                    }
                });

                button.blur(function(e) {
                    if (!self.suspendBlurFocus)
                    {
                        self.onBlur.call(self, e);
                        self.trigger("blur", e);
                    }
                });
                this.control.on("change", function (e) {
                    self.onChange.call(self, e);
                    self.trigger("change", e);

                });
            }
        },

        beforeRenderControl: function(model, callback)
        {
            var self = this;

            this.base(model, function() {
                self.selectOptions = [];
                if (self.sf) {

                    var completionFunction = function () {
                        self.schema.enum = [];
                        self.options.optionLabels = [];
                        for (var i = 0; i < self.selectOptions.length; i++) {
                            self.schema.enum.push(self.selectOptions[i].value);
                            self.options.optionLabels.push(self.selectOptions[i].text);
                        }
                        // push back to model
                        model.selectOptions = self.selectOptions;
                        callback();
                    };
                    var postData = { q: "*", d: self.options.folder, filter: self.options.filter };
                    $.ajax({
                        url: self.sf.getServiceRoot("OpenContent") + "DnnEntitiesAPI" + "/" + "FoldersLookup",
                        beforeSend: self.sf.setModuleHeaders,
                        type: "get",
                        dataType: "json",
                        //contentType: "application/json; charset=utf-8",
                        data: postData,
                        success: function (jsonDocument) {
                            var ds = jsonDocument;

                            if (self.options.dsTransformer && Alpaca.isFunction(self.options.dsTransformer)) {
                                ds = self.options.dsTransformer(ds);
                            }
                            if (ds) {
                                if (Alpaca.isObject(ds)) {
                                    // for objects, we walk through one key at a time
                                    // the insertion order is the order of the keys from the map
                                    // to preserve order, consider using an array as below
                                    $.each(ds, function (key, value) {
                                        self.selectOptions.push({
                                            "value": key,
                                            "text": value
                                        });
                                    });
                                    completionFunction();
                                }
                                else if (Alpaca.isArray(ds)) {
                                    // for arrays, we walk through one index at a time
                                    // the insertion order is dictated by the order of the indices into the array
                                    // this preserves order
                                    $.each(ds, function (index, value) {
                                        self.selectOptions.push({
                                            "value": value.value,
                                            "text": value.text
                                        });
                                        self.dataSource[value.value] = value;
                                    });
                                    completionFunction();
                                }
                            }
                        },
                        "error": function (jqXHR, textStatus, errorThrown) {

                            self.errorCallback({
                                "message": "Unable to load data from uri : " + self.options.dataSource,
                                "stage": "DATASOURCE_LOADING_ERROR",
                                "details": {
                                    "jqXHR": jqXHR,
                                    "textStatus": textStatus,
                                    "errorThrown": errorThrown
                                }
                            });
                        }
                    });
                } else {
                    callback();
                }

            });
        },

        prepareControlModel: function(callback)
        {
            var self = this;

            this.base(function(model) {

                model.selectOptions = self.selectOptions;

                callback(model);
            });
        },

        afterRenderControl: function(model, callback)
        {
            var self = this;

            this.base(model, function() {

                // if emptySelectFirst and nothing currently checked, then pick first item in the value list
                // set data and visually select it
                if (Alpaca.isUndefined(self.data) && self.options.emptySelectFirst && self.selectOptions && self.selectOptions.length > 0)
                {
                    self.data = self.selectOptions[0].value;
                }

                // do this little trick so that if we have a default value, it gets set during first render
                // this causes the state of the control
                if (self.data)
                {
                    self.setValue(self.data);
                }

                // if we are in multiple mode and the bootstrap multiselect plugin is available, bind it in
                //if (self.options.multiple && $.fn.multiselect)
                if ($.fn.select2)
                {
                    var settings = null;
                    if (self.options.select2) {
                        settings = self.options.select2;
                    }
                    else
                    {
                        settings = {};
                    }
                    /*
                    if (!settings.nonSelectedText)
                    {
                        settings.nonSelectedText = "None";
                        if (self.options.noneLabel)
                        {
                            settings.nonSelectedText = self.options.noneLabel;
                        }
                    }
                    if (self.options.hideNone)
                    {
                        delete settings.nonSelectedText;
                    }
                    */

                    settings.templateResult = function (state) {
                        if (!state.id) { return state.text; }
                        
                        var $state = $(
                          '<span>' + state.text + '</span>'
                        );
                        return $state;
                    };

                    settings.templateSelection = function (state) {
                        if (!state.id) { return state.text; }
                        
                        var $state = $(
                          '<span>' + state.text + '</span>'
                        );
                        return $state;
                    };

                    $(self.getControlEl()).select2(settings);
                }

                callback();

            });
        },

        getFileUrl : function(fileid){

            var postData = { fileid: fileid };
            $.ajax({
                url: self.sf.getServiceRoot("OpenContent") + "DnnEntitiesAPI" + "/" + "FileUrl",
                beforeSend: self.sf.setModuleHeaders,
                type: "get",
                asych : false,
                dataType: "json",
                //contentType: "application/json; charset=utf-8",
                data: postData,
                success: function (data) {
                    return data;
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    return "";
                }
            });

        },

        /**
         * Validate against enum property.
         *
         * @returns {Boolean} True if the element value is part of the enum list, false otherwise.
         */
        _validateEnum: function()
        {
            var _this = this;

            if (this.schema["enum"])
            {
                var val = this.data;

                if (!this.isRequired() && Alpaca.isValEmpty(val))
                {
                    return true;
                }

                if (this.options.multiple)
                {
                    var isValid = true;

                    if (!val)
                    {
                        val = [];
                    }

                    if (!Alpaca.isArray(val) && !Alpaca.isObject(val))
                    {
                        val = [val];
                    }

                    $.each(val, function(i,v) {

                        if ($.inArray(v, _this.schema["enum"]) <= -1)
                        {
                            isValid = false;
                            return false;
                        }

                    });

                    return isValid;
                }
                else
                {
                    return ($.inArray(val, this.schema["enum"]) > -1);
                }
            }
            else
            {
                return true;
            }
        },

        /**
         * @see Alpaca.Field#onChange
         */
        onChange: function(e)
        {
            this.base(e);

            var _this = this;

            Alpaca.later(25, this, function() {
                var v = _this.getValue();
                _this.setValue(v);
                _this.refreshValidationState();
            });
        },

        /**
         * Validates if number of items has been less than minItems.
         * @returns {Boolean} true if number of items has been less than minItems
         */
        _validateMinItems: function()
        {
            if (this.schema.items && this.schema.items.minItems)
            {
                if ($(":selected",this.control).length < this.schema.items.minItems)
                {
                    return false;
                }
            }

            return true;
        },

        /**
         * Validates if number of items has been over maxItems.
         * @returns {Boolean} true if number of items has been over maxItems
         */
        _validateMaxItems: function()
        {
            if (this.schema.items && this.schema.items.maxItems)
            {
                if ($(":selected",this.control).length > this.schema.items.maxItems)
                {
                    return false;
                }
            }

            return true;
        },

        /**
         * @see Alpaca.ContainerField#handleValidate
         */
        handleValidate: function()
        {
            var baseStatus = this.base();

            var valInfo = this.validation;

            var status = this._validateMaxItems();
            valInfo["tooManyItems"] = {
                "message": status ? "" : Alpaca.substituteTokens(this.getMessage("tooManyItems"), [this.schema.items.maxItems]),
                "status": status
            };

            status = this._validateMinItems();
            valInfo["notEnoughItems"] = {
                "message": status ? "" : Alpaca.substituteTokens(this.getMessage("notEnoughItems"), [this.schema.items.minItems]),
                "status": status
            };

            return baseStatus && valInfo["tooManyItems"]["status"] && valInfo["notEnoughItems"]["status"];
        },

        /**
         * @see Alpaca.Field#focus
         */
        focus: function(onFocusCallback)
        {
            if (this.control && this.control.length > 0)
            {
                // set focus onto the select
                var el = $(this.control).get(0);

                el.focus();

                if (onFocusCallback)
                {
                    onFocusCallback(this);
                }
            }
        }

        /* builder_helpers */
        ,

        /**
         * @see Alpaca.Field#getTitle
         */
        getTitle: function() {
            return "Select File Field";
        },

        /**
         * @see Alpaca.Field#getDescription
         */
        getDescription: function() {
            return "Select File Field";
        },

        /**
         * @private
         * @see Alpaca.Fields.File2Field#getSchemaOfOptions
         */
        getSchemaOfOptions: function() {
            return Alpaca.merge(this.base(), {
                "properties": {
                    "multiple": {
                        "title": "Mulitple Selection",
                        "description": "Allow multiple selection if true.",
                        "type": "boolean",
                        "default": false
                    },
                    "size": {
                        "title": "Displayed Options",
                        "description": "Number of options to be shown.",
                        "type": "number"
                    },
                    "emptySelectFirst": {
                        "title": "Empty Select First",
                        "description": "If the data is empty, then automatically select the first item in the list.",
                        "type": "boolean",
                        "default": false
                    },
                    "multiselect": {
                        "title": "Multiselect Plugin Settings",
                        "description": "Multiselect plugin properties - http://davidstutz.github.io/bootstrap-multiselect",
                        "type": "any"
                    }
                }
            });
        },

        /**
         * @private
         * @see Alpaca.Fields.File2Field#getOptionsForOptions
         */
        getOptionsForOptions: function() {
            return Alpaca.merge(this.base(), {
                "fields": {
                    "multiple": {
                        "rightLabel": "Allow multiple selection ?",
                        "helper": "Allow multiple selection if checked",
                        "type": "checkbox"
                    },
                    "size": {
                        "type": "integer"
                    },
                    "emptySelectFirst": {
                        "type": "checkbox",
                        "rightLabel": "Empty Select First"
                    },
                    "multiselect": {
                        "type": "object",
                        "rightLabel": "Multiselect plugin properties - http://davidstutz.github.io/bootstrap-multiselect"
                    }
                }
            });
        }

        /* end_builder_helpers */

    });

    Alpaca.registerFieldClass("folder2", Alpaca.Fields.Folder2Field);

})(jQuery);
(function ($) {

    var Alpaca = $.alpaca;

    Alpaca.Fields.GalleryField = Alpaca.Fields.MultiUploadField.extend(
    {
        setup: function () {
            this.base();
            this.schema.items = {
                "type": "object",
                "properties": {
                
                    "Image": {
                        "title": "Image",
                        "type": "string"
                    }
                }
            };
            Alpaca.merge(this.options.items, {
                "fields": {
                    "Image": {
                        "type": "image"
                    }
                }
            });
            this.urlfield = "Image";
        },
        getTitle: function () {
            return "Gallery";
        },
        getDescription: function () {
            return "Image Gallery";
        },

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

    });

    Alpaca.registerFieldClass("gallery", Alpaca.Fields.GalleryField);

})(jQuery);
(function ($) {

    var Alpaca = $.alpaca;

    Alpaca.Fields.GuidField = Alpaca.Fields.TextField.extend(
    /**
     * @lends Alpaca.Fields.TagField.prototype
     */
    {
        constructor: function (container, data, options, schema, view, connector) {
            var self = this;
            
            this.base(container, data, options, schema, view, connector);
           
        },
        setup: function () {
            var self = this;
            this.base();
            
        },
        setValue: function (value) {
     
            if (Alpaca.isEmpty(value)) {
                value = this.createGuid();
            }

            // be sure to call into base method
            this.base(value);

        },
        getValue: function () {
            var value = this.base();
            if (Alpaca.isEmpty(value) || value == "") {
                value = this.createGuid();
            }

            return value;
        },
        createGuid: function ()
        {
            return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function(c) {
                var r = Math.random()*16|0, v = c === 'x' ? r : (r&0x3|0x8);
                return v.toString(16);
            });
        },
        
        /**
         * @see Alpaca.Fields.TextField#getTitle
         */
        getTitle: function () {
            return "Guid Field";
        },

        /**
         * @see Alpaca.Fields.TextField#getDescription
         */
        getDescription: function () {
            return "Guid field .";
        },

        /**
         * @private
         * @see Alpaca.Fields.TextField#getSchemaOfOptions
         */
        getSchemaOfOptions: function () {
            return Alpaca.merge(this.base(), {
                "properties": {
                    "separator": {
                        "title": "Separator",
                        "description": "Separator used to split tags.",
                        "type": "string",
                        "default": ","
                    }
                }
            });
        },

        /**
         * @private
         * @see Alpaca.Fields.TextField#getOptionsForOptions
         */
        getOptionsForOptions: function () {
            return Alpaca.merge(this.base(), {
                "fields": {
                    "separator": {
                        "type": "text"
                    }
                }
            });
        }

        /* end_builder_helpers */
    });

    Alpaca.registerFieldClass("guid", Alpaca.Fields.GuidField);

})(jQuery);
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
(function ($) {
    var Alpaca = $.alpaca;
    Alpaca.Fields.ImageXField = Alpaca.Fields.ListField.extend(
        {
            constructor: function (container, data, options, schema, view, connector) {
                var self = this;
                this.base(container, data, options, schema, view, connector);
                this.sf = connector.servicesFramework;
                this.dataSource = {};
            },
            getFieldType: function () {
                return "imagex";
            },
            setup: function () {
                var self = this;
                //if (this.options.advanced === undefined) {
                    this.options.advanced = true;
                //}
                if (!this.options.fileExtensions) {
                    this.options.fileExtensions = 'gif|jpg|jpeg|tiff|png';
                }
                if (!this.options.fileMaxSize) {
                    this.options.fileMaxSize = 2000000;
                }
                if (!this.options.uploadfolder) {
                    this.options.uploadfolder = "";
                }
                if (!this.options.uploadhidden) {
                    this.options.uploadhidden = false;
                }
                if (!this.options.overwrite) {
                    this.options.overwrite = false;
                }
                if (!this.options.showOverwrite) {
                    this.options.showOverwrite = false;
                }
                if (this.options.uploadhidden) {
                    this.options.showOverwrite = false;
                }
                if (this.options.showCropper === undefined) {
                    this.options.showCropper = false;                    
                }
                if (this.options.showCropper) {
                    this.options.showImage = true;
                    this.options.advanced = true;
                }
                if (this.options.showImage === undefined) {
                    this.options.showImage = true;
                }
                if (this.options.showCropper) {
                    if (!this.options.cropfolder) {
                        this.options.cropfolder = this.options.uploadfolder;
                    }
                    if (!this.options.cropper) {
                        this.options.cropper = {};
                    }
                    if (this.options.width && this.options.height) {
                        this.options.cropper.aspectRatio = this.options.width / this.options.height;
                    }
                    if (this.options.ratio) {
                        this.options.cropper.aspectRatio = this.options.ratio;
                    }
                    this.options.cropper.responsive = false;
                    if (!this.options.cropper.autoCropArea) {
                        this.options.cropper.autoCropArea = 1;
                    }
                    if (!this.options.cropper.viewMode) {
                        this.options.cropper.viewMode = 1;
                    }
                    if (!this.options.cropper.zoomOnWheel) {
                        this.options.cropper.zoomOnWheel = false;
                    }
                    if (!this.options.saveCropFile) {
                        this.options.saveCropFile = false;
                    }
                    if (this.options.saveCropFile) {
                        this.options.buttons = {
                            "check": {
                                "value": "Crop Image",
                                "click": function () {
                                    this.cropImage();
                                }
                            }
                        };
                    }
                }
                this.base();
            },
            getValue: function () {
                return this.getBaseValue();
            },
            setValue: function (val) {
                var self = this;
                //if (val !== this.getValue()) {
                if (this.control && typeof (val) != "undefined" && val != null) {
                    $image = self.getImage();
                        //this.base(val); ???
                        if (Alpaca.isEmpty(val)) {
                            $image.attr('src', url);
                            if (this.options.showCropper) {
                                self.cropper("");
                                self.setCropUrl('');
                                
                            }
                            $(this.control).find('select').val("");
                        }
                        else if (Alpaca.isObject(val)) {
                            // Fix for OC data that still has the Cachebuster SQ parameter
                            if (val.url) val.url = val.url.split("?")[0];
                            $image.attr('src', val.url);
                            if (this.options.showCropper) {
                                if (val.cropUrl) val.cropUrl = val.cropUrl.split("?")[0];
                                if (val.cropdata && Object.keys(val.cropdata).length > 0) { // compatibility with imagecropper
                                    var firstcropdata = val.cropdata[Object.keys(val.cropdata)[0]];
                                    self.cropper(val.url, firstcropdata.cropper);
                                    self.setCropUrl(firstcropdata.url);
                                } else if (val.crop) {
                                    self.cropper(val.url, val.crop);
                                    self.setCropUrl( val.cropUrl);
                                } else {
                                    self.cropper(val.url, val.crop);
                                    self.setCropUrl( val.cropUrl);
                                }
                            }
                            $(this.control).find('select').val(val.url);

                        }
                        else {
                            $image.attr('src', val);
                            if (this.options.showCropper) {
                                self.cropper(val);
                                self.setCropUrl( '');
                            }
                            $(this.control).find('select').val(val);

                        }
                        $(this.control).find('select').trigger('change.select2');
                    }
                //}
            },
            getBaseValue: function () {
                var self = this;
                if (this.control && this.control.length > 0) {
                    var value = null;
                    $image = self.getImage();
                    value = {};
                    if (this.options.showCropper) {
                        if (self.cropperExist()) {
                            value.crop = $image.cropper('getData', { rounded: true });
                        }
                    }
                    var url = $(this.control).find('select').val();
                    if (self.options.advanced) {
                        value.url = url;
                    } else {
                        value = url; // compatibility mode
                    }
                    if (value.url) {
                        if (this.dataSource && this.dataSource[value.url]) {
                            value.id = this.dataSource[value.url].id;
                            value.filename = this.dataSource[value.url].filename;
                            value.width = this.dataSource[value.url].width;
                            value.height = this.dataSource[value.url].height;
                        }
                        if (this.options.showCropper) {
                            value.cropUrl = this.getCropUrl();
                        }
                    }
                    return value;
                }        
            },
            beforeRenderControl: function (model, callback) {
                var self = this;
                this.base(model, function () {
                    self.selectOptions = [];
                    if (self.sf) {
                        var completionFunction = function () {
                            self.schema.enum = [];
                            self.options.optionLabels = [];
                            for (var i = 0; i < self.selectOptions.length; i++) {
                                self.schema.enum.push(self.selectOptions[i].value);
                                self.options.optionLabels.push(self.selectOptions[i].text);
                            }
                            // push back to model
                            model.selectOptions = self.selectOptions;
                            callback();
                        };
                        var postData = { q: "*", folder: self.options.uploadfolder };
                        $.ajax({
                            url: self.sf.getServiceRoot("OpenContent") + "DnnEntitiesAPI" + "/" + "ImagesLookupExt",
                            beforeSend: self.sf.setModuleHeaders,
                            type: "get",
                            dataType: "json",
                            //contentType: "application/json; charset=utf-8",
                            data: postData,
                            success: function (jsonDocument) {
                                var ds = jsonDocument;
                                if (self.options.dsTransformer && Alpaca.isFunction(self.options.dsTransformer)) {
                                    ds = self.options.dsTransformer(ds);
                                }
                                if (ds) {
                                    if (Alpaca.isArray(ds)) {
                                        // for arrays, we walk through one index at a time
                                        // the insertion order is dictated by the order of the indices into the array
                                        // this preserves order
                                        $.each(ds, function (index, value) {
                                            self.selectOptions.push({
                                                "value": value.url,
                                                "thumbUrl": value.thumbUrl,
                                                "id": value.id,
                                                "text": value.text,
                                                "filename": value.filename,
                                                "width": value.width,
                                                "height": value.height,
                                            });
                                            self.dataSource[value.url] = value;
                                        });
                                        completionFunction();
                                    }
                                }
                            },
                            "error": function (jqXHR, textStatus, errorThrown) {
                                self.errorCallback({
                                    "message": "Unable to load data from uri : " + self.options.dataSource,
                                    "stage": "DATASOURCE_LOADING_ERROR",
                                    "details": {
                                        "jqXHR": jqXHR,
                                        "textStatus": textStatus,
                                        "errorThrown": errorThrown
                                    }
                                });
                            }
                        });
                    } else {
                        callback();
                    }
                });
            },

            prepareControlModel: function (callback) {
                var self = this;
                this.base(function (model) {
                    model.selectOptions = self.selectOptions;
                    callback(model);
                });
            },
            afterRenderControl: function (model, callback) {
                var self = this;
                this.base(model, function () {
                    // if emptySelectFirst and nothing currently checked, then pick first item in the value list
                    // set data and visually select it
                    if (Alpaca.isUndefined(self.data) && self.options.emptySelectFirst && self.selectOptions && self.selectOptions.length > 0) {
                        self.data = self.selectOptions[0].value;
                    }
                    // do this little trick so that if we have a default value, it gets set during first render
                    // this causes the state of the control
                    if (self.data) {
                        self.setValue(self.data);
                    }

                    if ($.fn.select2) {
                        var settings = null;
                        if (self.options.select2) {
                            settings = self.options.select2;
                        }
                        else {
                            settings = {};
                        }
                        settings.templateResult = function (state) {
                            if (!state.id) { return state.text; }

                            var $state = $(
                                '<span><img src="' + self.dataSource[state.id].thumbUrl + '" style="height: 45px;width: 54px;"  /> ' + state.text + '</span>'
                            );
                            return $state;
                        };

                        settings.templateSelection = function (state) {
                            if (!state.id) { return state.text; }

                            var $state = $(
                                '<span><img src="' + self.dataSource[state.id].thumbUrl + '" style="height: 15px;width: 18px;"  /> ' + state.text + '</span>'
                            );
                            return $state;
                        };

                        $(self.getControlEl().find('select')).select2(settings);
                    }
                    if (self.options.uploadhidden) {
                        $(self.getControlEl()).find('input[type=file]').hide();
                    } else {
                        if (self.sf) {

                            $(self.getControlEl()).find('input[type=file]').fileupload({
                                dataType: 'json',
                                url: self.sf.getServiceRoot('OpenContent') + "FileUpload/UploadFile",
                                maxFileSize: 25000000,
                                formData: function () {
                                    var formData = [{ name: 'uploadfolder', value: self.options.uploadfolder }];
                                    if (self.options.showOverwrite) {
                                        formData.push({ name: 'overwrite', value: self.isOverwrite() });
                                    } else if (self.options.overwrite) {
                                        formData.push({ name: 'overwrite', value: true });
                                    }
                                    return formData;
                                    //{ uploadfolder: self.options.uploadfolder, overwrite: self.isOverwrite() }
                                },
                                beforeSend: self.sf.setModuleHeaders,
                                add: function (e, data) {
                                    var goUpload = true;
                                    var uploadFile = data.files[0];        
                                    var regex = new RegExp('\\.(' + self.options.fileExtensions + ')$', 'i');
                                    if (!(regex).test(uploadFile.name)) {
                                        self.showAlert('You must select an image file only (' + self.options.fileExtensions+')');
                                        goUpload = false;
                                    }
                                    if (uploadFile.size > self.options.fileMaxSize) { 
                                        self.showAlert('Please upload a smaller image, max size is ' + self.options.fileMaxSize+ ' bytes');
                                        goUpload = false;
                                    }
                                    if (goUpload == true) {
                                        self.showAlert('File uploading...');
                                        data.submit();
                                    }
                                    //data.context = $(opts.progressContextSelector);
                                    //data.context.find($(opts.progressFileNameSelector)).html(data.files[0].name);
                                    //data.context.show('fade');
                                    //data.submit();
                                },
                                progress: function (e, data) {
                                    if (data.context) {
                                        var progress = parseInt(data.loaded / data.total * 100, 10);
                                        data.context.find(opts.progressBarSelector).css('width', progress + '%').find('span').html(progress + '%');
                                    }
                                },
                                done: function (e, data) {
                                    if (data.result) {
                                        $.each(data.result, function (index, file) {
                                            if (file.success) {
                                                self.refresh(function () {
                                                    self.setValue(file.url);
                                                    self.showAlert('File uploaded', true);
                                                });
                                            } else {
                                                self.showAlert(file.message, true);
                                            }
                                        });
                                    }

                                }
                            }).data('loaded', true);
                        }
                    }
                    if (!self.options.showOverwrite) {
                        $(self.control).parent().find('#' + self.id + '-overwriteLabel').hide();
                    }
                    callback();
                });
            },
            cropImage: function () {
                var self = this;
                var data = self.getBaseValue();
                if (!data.url) return;
                $image = self.getImage();
                var crop = $image.cropper('getData', { rounded: true }); 
                
                var postData = { url: data.url, cropfolder: self.options.cropfolder, crop: crop, id: "crop" };
                if (self.options.width && self.options.height) {
                    postData.resize = { width: self.options.width, height: self.options.height };
                }
                $(self.getControlEl()).css('cursor', 'wait');
                self.showAlert('Image cropping...');
                $.ajax({
                    type: "POST",
                    url: self.sf.getServiceRoot('OpenContent') + "DnnEntitiesAPI/CropImage",
                    contentType: "application/json; charset=utf-8",
                    dataType: "json",
                    data: JSON.stringify(postData),
                    beforeSend: self.sf.setModuleHeaders
                }).done(function (res) {
                    self.setCropUrl( res.url);
                    self.showAlert('Image cropped', true);
                    setTimeout(function () {
                        $(self.getControlEl()).css('cursor', 'initial');
                    }, 500);
                }).fail(function (xhr, result, status) {
                    alert("Uh-oh, something broke: " + status);
                    $(self.getControlEl()).css('cursor', 'initial');
                });
            },
            getFileUrl: function (fileid) {
                var self = this;
                if (self.sf) {
                    var postData = { fileid: fileid };
                    $.ajax({
                        url: self.sf.getServiceRoot("OpenContent") + "DnnEntitiesAPI" + "/" + "FileUrl",
                        beforeSend: self.sf.setModuleHeaders,
                        type: "get",
                        asych: false,
                        dataType: "json",
                        //contentType: "application/json; charset=utf-8",
                        data: postData,
                        success: function (data) {
                            return data;
                        },
                        error: function (jqXHR, textStatus, errorThrown) {
                            return "";
                        }
                    });
                }
            },
            cropper: function (url, data) {
                var self = this;
                $image = self.getImage();

                var cropperExist = $image.data('cropper');
                if (url) {
                    $image.show();
                    if (!cropperExist) {
                        var config = $.extend({}, {
                            aspectRatio: 16 / 9,
                            checkOrientation: false,
                            autoCropArea: 0.90,
                            minContainerHeight: 200,
                            minContainerWidth: 400,
                            toggleDragModeOnDblclick: false,
                            zoomOnWheel: false,
                            cropmove: function(event) {
                                self.setCropUrl('');
                            }
                        }, self.options.cropper);
                        if (data) {
                            config.data = data;
                        };

                        $image.cropper(config);
                    } else {
                        if (url != cropperExist.originalUrl || (cropperExist.url && url != cropperExist.url)) {
                            $image.cropper('replace', url);
                        }
                        //$image.cropper('reset');
                        if (data) {
                            $image.cropper('setData', data);
                        }
                    }
                } else {
                    $image.hide();
                    if (!cropperExist) {

                    } else {
                        $image.cropper('destroy');
                    }
                }
            },
            cropperExist: function () {
                var self = this;
                $image = self.getImage();
                var cropperData = $image.data('cropper');

                return cropperData;
            },
            getImage: function () {
                var self = this;
                return $(self.control).parent().find('#' + self.id + '-image'); //.find('.alpaca-image-display > img');

            },
            isOverwrite: function () {
                var self = this;
                if (this.options.showOverwrite) {
                    var checkbox = $(self.control).parent().find('#' + self.id + '-overwrite');
                    return Alpaca.checked(checkbox);
                } else {
                    return this.options.overwrite;
                }
            },
            getCropUrl: function () {
                var self = this;
                return $(self.getControlEl()).attr('data-cropurl');
            },
            setCropUrl: function (url) {
                var self = this;
                $(self.getControlEl()).attr('data-cropurl', url);
                self.refreshValidationState();
            },

            handleValidate: function () {
                var baseStatus = this.base();
                var valInfo = this.validation;

                var url = $(this.control).find('select').val();

                var status = !url || !this.options.showCropper || !this.options.saveCropFile || this.getCropUrl();
                
                valInfo["cropMissing"] = {
                    "message": status ? "" : this.getMessage("cropMissing"),
                    "status": status
                };

                return baseStatus && valInfo["cropMissing"]["status"];
            },

            /**
             * Validate against enum property.
             *
             * @returns {Boolean} True if the element value is part of the enum list, false otherwise.
             */
            _validateEnum: function () {
                var _this = this;
                
                if (this.schema["enum"]) {
                    var val = this.data ? this.data.url : "";

                    if (!this.isRequired() && Alpaca.isValEmpty(val)) {
                        return true;
                    }

                    if (this.options.multiple) {
                        var isValid = true;

                        if (!val) {
                            val = [];
                        }

                        if (!Alpaca.isArray(val) && !Alpaca.isObject(val)) {
                            val = [val];
                        }

                        $.each(val, function (i, v) {

                            if ($.inArray(v, _this.schema["enum"]) <= -1) {
                                isValid = false;
                                return false;
                            }

                        });

                        return isValid;
                    }
                    else {
                        return ($.inArray(val, this.schema["enum"]) > -1);
                    }
                }
                else {
                    return true;
                }
            },

            /**
             * @see Alpaca.Field#onChange
             */
            onChange: function (e) {
                this.base(e);
                var _this = this;
                _this.setCropUrl('');
                

                Alpaca.later(25, this, function () {
                    var v = _this.getValue();                    
                    _this.setValue(v);
                    _this.refreshValidationState();
                });
            },

            /**
             * @see Alpaca.Field#focus
             */
            focus: function (onFocusCallback) {
                if (this.control && this.control.length > 0) {
                    // set focus onto the select
                    var el = $(this.control).find('select');

                    el.focus();

                    if (onFocusCallback) {
                        onFocusCallback(this);
                    }
                }
            }

            /* builder_helpers */
            ,

            /**
             * @see Alpaca.Field#getTitle
             */
            getTitle: function () {
                return "Image Crop 2 Field";
            },

            /**
             * @see Alpaca.Field#getDescription
             */
            getDescription: function () {
                return "Image Crop 2 Field";
            },
            showAlert: function (text, time) {
                var self = this;
                $('#' + self.id + '-alert').text(text);
                $('#' + self.id + '-alert').show();
                if (time) {
                    setTimeout(function (text) {
                        $('#' + self.id + '-alert').hide();
                    }, 4000);
                }
            },
        });

    Alpaca.registerFieldClass("imagex", Alpaca.Fields.ImageXField);
    Alpaca.registerMessages({
        "cropMissing": "Cropped image missing (click the crop button)"
    });

})(jQuery);
(function ($) {

    var Alpaca = $.alpaca;
    
    Alpaca.Fields.ImageField = Alpaca.Fields.TextField.extend(
    /**
     * @lends Alpaca.Fields.ImageField.prototype
     */
    {
        constructor: function(container, data, options, schema, view, connector)
        {
            var self = this;
            this.base(container, data, options, schema, view, connector);
            this.sf = connector.servicesFramework;
        },

        /**
         * @see Alpaca.Fields.TextField#getFieldType
         */
        getFieldType: function () {
            return "image";
        }
        ,
        setup: function () {
            if (!this.options.uploadfolder) {
                this.options.uploadfolder = "";
            }
            if (!this.options.uploadhidden) {
                this.options.uploadhidden = false;
            }
            this.base();
        },

        /**
         * @see Alpaca.Fields.TextField#getTitle
         */
        getTitle: function () {
            return "Image Field";
        },

        /**
         * @see Alpaca.Fields.TextField#getDescription
         */
        getDescription: function () {
            return "Image Field.";
        },
        getTextControlEl: function () {
            return $(this.control.get(0)).find('input[type=text]#' + this.id);
        },
        setValue: function (value) {
            var self = this;
            //var el = $( this.control).filter('#'+this.id);
            //var el = $(this.control.get(0)).find('input[type=text]');
            var el = this.getTextControlEl();

            if (el && el.length > 0) {
                if (Alpaca.isEmpty(value)) {
                    el.val("");
                }
                else {
                    //if (value) value = value.split("?")[0];
                    el.val(value);
                    $(self.control).parent().find('.alpaca-image-display img').attr('src', value);
                }
            }
            
            // be sure to call into base method
            //this.base(value);

            // if applicable, update the max length indicator
            this.updateMaxLengthIndicator();
        },

        getValue: function () {
            var value = null;

            //var el = $(this.control).filter('#' + this.id);
            //var el = $(this.control.get(0)).find('input[type=text]');
            var el = this.getTextControlEl();
            if (el && el.length > 0) {
                    value = el.val();
            }
            return value;
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
            

            //var el = this.control;
            var el = this.getTextControlEl();

            if (self.options.uploadhidden) {
                $(this.control.get(0)).find('input[type=file]').hide();
            } else {
                if (self.sf){
                $(this.control.get(0)).find('input[type=file]').fileupload({
                    dataType: 'json',
                    url: self.sf.getServiceRoot('OpenContent') + "FileUpload/UploadFile",
                    maxFileSize: 25000000,
                    formData: { uploadfolder : self.options.uploadfolder },
                    beforeSend: self.sf.setModuleHeaders,
                    add: function (e, data) {
                        //data.context = $(opts.progressContextSelector);
                        //data.context.find($(opts.progressFileNameSelector)).html(data.files[0].name);
                        //data.context.show('fade');
                        data.submit();
                    },
                    progress: function (e, data) {
                        if (data.context) {
                            var progress = parseInt(data.loaded / data.total * 100, 10);
                            data.context.find(opts.progressBarSelector).css('width', progress + '%').find('span').html(progress + '%');
                        }
                    },
                    done: function (e, data) {
                        if (data.result) {
                            $.each(data.result, function (index, file) {
                                self.setValue(file.url);
                                $(el).change();
                                //$(el).change();
                                //$(e.target).parent().find('input[type=text]').val(file.url);
                                //el.val(file.url);
                                //$(e.target).parent().find('.alpaca-image-display img').attr('src', file.url);
                            });
                        }
                    }
                }).data('loaded', true);
                }
            }
            $(el).change(function () {

                var value = $(this).val();

                //var newValue = $(el).typeahead('val');
                //if (newValue !== value) {
                    $(self.control).parent().find('.alpaca-image-display img').attr('src', value);
                //}

            });

            if (self.options.manageurl) {
                var manageButton = $('<a href="' + self.options.manageurl + '" target="_blank" class="alpaca-form-button">Manage files</a>').appendTo($(el).parent());
            }
            
            callback();
        },
        applyTypeAhead: function () {
            var self = this;

            if (self.control.typeahead && self.options.typeahead && !Alpaca.isEmpty(self.options.typeahead) && self.sf) {

                var tConfig = self.options.typeahead.config;
                if (!tConfig) {
                    tConfig = {};
                }
                
                var tDatasets = self.options.typeahead.datasets;
                if (!tDatasets) {
                    tDatasets = {};
                }

                if (!tDatasets.name) {
                    tDatasets.name = self.getId();
                }

                var tFolder = self.options.typeahead.Folder;
                if (!tFolder) {
                    tFolder = "";
                }

                var tEvents = tEvents = {};

                var bloodHoundConfig = {
                    datumTokenizer: function (d) {
                        return Bloodhound.tokenizers.whitespace(d.value);
                    },
                    queryTokenizer: Bloodhound.tokenizers.whitespace
                };

                /*
                if (tDatasets.type === "prefetch") {
                    bloodHoundConfig.prefetch = {
                        url: tDatasets.source,
                        ajax: {
                            //url: sf.getServiceRoot('OpenContent') + "FileUpload/UploadFile",
                            beforeSend: connector.servicesFramework.setModuleHeaders,
        
                        }
                    };
        
                    if (tDatasets.filter) {
                        bloodHoundConfig.prefetch.filter = tDatasets.filter;
                    }
                }
                */

                bloodHoundConfig.remote = {
                    url: self.sf.getServiceRoot('OpenContent') + "DnnEntitiesAPI/Images?q=%QUERY&d=" + tFolder,
                    ajax: {
                        beforeSend: self.sf.setModuleHeaders,

                    }
                };

                if (tDatasets.filter) {
                    bloodHoundConfig.remote.filter = tDatasets.filter;
                }

                if (tDatasets.replace) {
                    bloodHoundConfig.remote.replace = tDatasets.replace;
                }


                var engine = new Bloodhound(bloodHoundConfig);
                engine.initialize();
                tDatasets.source = engine.ttAdapter();

                tDatasets.templates = {
                    "empty": "Nothing found...",
                    "suggestion": "<div style='width:20%;display:inline-block;background-color:#fff;padding:2px;'><img src='{{value}}' style='height:40px' /></div> {{name}}"
                };

                // compile templates
                if (tDatasets.templates) {
                    for (var k in tDatasets.templates) {
                        var template = tDatasets.templates[k];
                        if (typeof (template) === "string") {
                            tDatasets.templates[k] = Handlebars.compile(template);
                        }
                    }
                }

                //var el = $(this.control.get(0)).find('input[type=text]');
                var el = this.getTextControlEl();
                // process typeahead
                $(el).typeahead(tConfig, tDatasets);

                // listen for "autocompleted" event and set the value of the field
                $(el).on("typeahead:autocompleted", function (event, datum) {
                    self.setValue(datum.value);
                    $(el).change();
                    //$(self.control).parent().find('input[type=text]').val(datum.value);
                    //$(self.control).parent().find('.alpaca-image-display img').attr('src', datum.value);
                });

                // listen for "selected" event and set the value of the field
                $(el).on("typeahead:selected", function (event, datum) {
                    self.setValue(datum.value);
                    $(el).change();
                    //$(self.control).parent().find('input[type=text]').val(datum.value);
                    //$(self.control).parent().find('.alpaca-image-display img').attr('src', datum.value);
                });

                // custom events
                if (tEvents) {
                    if (tEvents.autocompleted) {
                        $(el).on("typeahead:autocompleted", function (event, datum) {
                            tEvents.autocompleted(event, datum);
                        });
                    }
                    if (tEvents.selected) {
                        $(el).on("typeahead:selected", function (event, datum) {
                            tEvents.selected(event, datum);
                        });
                    }
                }

                // when the input value changes, change the query in typeahead
                // this is to keep the typeahead control sync'd with the actual dom value
                // only do this if the query doesn't already match
                //var fi = $(self.control);
                $(el).change(function () {

                    var value = $(this).val();

                    var newValue = $(el).typeahead('val');
                    if (newValue !== value) {
                        $(el).typeahead('val', value);
                    }

                });

                // some UI cleanup (we don't want typeahead to restyle)
                $(self.field).find("span.twitter-typeahead").first().css("display", "block"); // SPAN to behave more like DIV, next line
                $(self.field).find("span.twitter-typeahead input.tt-input").first().css("background-color", "");
            }
        }

        /* end_builder_helpers */
    });

    Alpaca.registerFieldClass("image", Alpaca.Fields.ImageField);

})(jQuery);
(function($) {
    var Alpaca = $.alpaca;
    Alpaca.Fields.Image2Field = Alpaca.Fields.ListField.extend(
    /**
     * @lends Alpaca.Fields.Image2Field.prototype
     */
    {
        constructor: function (container, data, options, schema, view, connector) {
            var self = this;
            this.base(container, data, options, schema, view, connector);
            this.sf = connector.servicesFramework;
            this.dataSource = {};
        },
        /**
         * @see Alpaca.Field#getFieldType
         */
        getFieldType: function()
        {
            return "select";
        },

        /**
         * @see Alpaca.Fields.Image2Field#setup
         */
        setup: function()
        {
            var self = this;
            if (self.schema["type"] && self.schema["type"] === "array") {
                self.options.multiple = true;
                self.options.removeDefaultNone = true;
                //self.options.hideNone = true;
            }
            if (!this.options.folder) {
                this.options.folder = "";
            }
            this.base();
        },

        getValue: function () {
            if (this.control && this.control.length > 0) {
                var val = this._getControlVal(true);
                if (typeof (val) === "undefined") {
                    val = this.data;
                }
                else if (Alpaca.isArray(val)) {
                    for (var i = 0; i < val.length; i++) {
                        val[i] = this.ensureProperType(val[i]);
                    }
                }

                return this.base(val);
            }
        },

        /**
         * @see Alpaca.Field#setValue
         */
        setValue: function(val)
        {
            if (Alpaca.isArray(val))
            {
                if (!Alpaca.compareArrayContent(val, this.getValue()))
                {
                    if (!Alpaca.isEmpty(val) && this.control)
                    {
                        this.control.val(val);
                    }

                    this.base(val);
                }
            }
            else
            {
                if (val !== this.getValue())
                {
                    /*
                    if (!Alpaca.isEmpty(val) && this.control)
                    {
                        this.control.val(val);
                    }
                    */
                    if (this.control && typeof(val) != "undefined" && val != null)
                    {
                        this.control.val(val);
                    }

                    this.base(val);
                }
            }
        },

        /**
         * @see Alpaca.Image2Field#getEnum
         */
        getEnum: function()
        {
            if (this.schema)
            {
                if (this.schema["enum"])
                {
                    return this.schema["enum"];
                }
                else if (this.schema["type"] && this.schema["type"] === "array" && this.schema["items"] && this.schema["items"]["enum"])
                {
                    return this.schema["items"]["enum"];
                }
            }
        },

        initControlEvents: function()
        {
            var self = this;

            self.base();

            if (self.options.multiple)
            {
                var button = this.control.parent().find(".select2-search__field");

                button.focus(function(e) {
                    if (!self.suspendBlurFocus)
                    {
                        self.onFocus.call(self, e);
                        self.trigger("focus", e);
                    }
                });

                button.blur(function(e) {
                    if (!self.suspendBlurFocus)
                    {
                        self.onBlur.call(self, e);
                        self.trigger("blur", e);
                    }
                });

                this.control.on("change", function (e) {
                    self.onChange.call(self, e);
                    self.trigger("change", e);

                });
            }
        },

        beforeRenderControl: function(model, callback)
        {
            var self = this;
            this.base(model, function () {

                self.selectOptions = [];
                if (self.sf) {
                    var completionFunction = function () {
                        self.schema.enum = [];
                        self.options.optionLabels = [];
                        for (var i = 0; i < self.selectOptions.length; i++) {
                            self.schema.enum.push(self.selectOptions[i].value);
                            self.options.optionLabels.push(self.selectOptions[i].text);
                        }
                        // push back to model
                        model.selectOptions = self.selectOptions;
                        callback();
                    };

                    var postData = { q: "*", d: self.options.folder };

                    $.ajax({
                        url: self.sf.getServiceRoot("OpenContent") + "DnnEntitiesAPI" + "/" + "ImagesLookup",
                        beforeSend: self.sf.setModuleHeaders,
                        type: "get",
                        dataType: "json",
                        //contentType: "application/json; charset=utf-8",
                        data: postData,
                        success: function (jsonDocument) {

                            var ds = jsonDocument;

                            if (self.options.dsTransformer && Alpaca.isFunction(self.options.dsTransformer)) {
                                ds = self.options.dsTransformer(ds);
                            }

                            if (ds) {
                                if (Alpaca.isObject(ds)) {
                                    // for objects, we walk through one key at a time
                                    // the insertion order is the order of the keys from the map
                                    // to preserve order, consider using an array as below
                                    $.each(ds, function (key, value) {
                                        self.selectOptions.push({
                                            "value": key,
                                            "text": value
                                        });
                                    });
                                    completionFunction();
                                }
                                else if (Alpaca.isArray(ds)) {
                                    // for arrays, we walk through one index at a time
                                    // the insertion order is dictated by the order of the indices into the array
                                    // this preserves order
                                    $.each(ds, function (index, value) {
                                        self.selectOptions.push({
                                            "value": value.value,
                                            "text": value.text
                                        });
                                        self.dataSource[value.value] = value;
                                    });
                                    completionFunction();
                                }
                            }
                        },
                        "error": function (jqXHR, textStatus, errorThrown) {

                            self.errorCallback({
                                "message": "Unable to load data from uri : " + self.options.dataSource,
                                "stage": "DATASOURCE_LOADING_ERROR",
                                "details": {
                                    "jqXHR": jqXHR,
                                    "textStatus": textStatus,
                                    "errorThrown": errorThrown
                                }
                            });
                        }
                    });
                } else {
                    callback();
                }
            });
        },

        prepareControlModel: function(callback)
        {
            var self = this;
            this.base(function(model) {
                model.selectOptions = self.selectOptions;
                callback(model);
            });
        },

        afterRenderControl: function(model, callback)
        {
            var self = this;
            this.base(model, function() {
                // if emptySelectFirst and nothing currently checked, then pick first item in the value list
                // set data and visually select it
                if (Alpaca.isUndefined(self.data) && self.options.emptySelectFirst && self.selectOptions && self.selectOptions.length > 0)
                {
                    self.data = self.selectOptions[0].value;
                }
                // do this little trick so that if we have a default value, it gets set during first render
                // this causes the state of the control
                if (self.data)
                {
                    self.setValue(self.data);
                }

                // if we are in multiple mode and the bootstrap multiselect plugin is available, bind it in
                //if (self.options.multiple && $.fn.multiselect)
                if ($.fn.select2)
                {
                    var settings = null;
                    if (self.options.select2) {
                        settings = self.options.select2;
                    }
                    else
                    {
                        settings = {};
                    }
                    /*
                    if (!settings.nonSelectedText)
                    {
                        settings.nonSelectedText = "None";
                        if (self.options.noneLabel)
                        {
                            settings.nonSelectedText = self.options.noneLabel;
                        }
                    }
                    if (self.options.hideNone)
                    {
                        delete settings.nonSelectedText;
                    }
                    */

                    settings.templateResult = function (state) {
                        if (!state.id) { return state.text; }
                        
                        var $state = $(
                          '<span><img src="' + self.dataSource[state.id].url + '" style="height: 45px;width: 54px;"  /> ' + state.text + '</span>'
                        );
                        return $state;
                    };

                    settings.templateSelection = function (state) {
                        if (!state.id) { return state.text; }
                        
                        var $state = $(
                          '<span><img src="' + self.dataSource[state.id].url + '" style="height: 15px;width: 18px;"  /> ' + state.text + '</span>'
                        );
                        return $state;
                    };

                    $(self.getControlEl()).select2(settings);
                }

                callback();

            });
        },
        getFileUrl : function(fileid){
            if (self.sf) {
                var postData = { fileid: fileid };
                $.ajax({
                    url: self.sf.getServiceRoot("OpenContent") + "DnnEntitiesAPI" + "/" + "FileUrl",
                    beforeSend: self.sf.setModuleHeaders,
                    type: "get",
                    asych: false,
                    dataType: "json",
                    //contentType: "application/json; charset=utf-8",
                    data: postData,
                    success: function (data) {
                        return data;
                    },
                    error: function (jqXHR, textStatus, errorThrown) {
                        return "";
                    }
                });
            }
        },

        /**
         * Validate against enum property.
         *
         * @returns {Boolean} True if the element value is part of the enum list, false otherwise.
         */
        _validateEnum: function()
        {
            var _this = this;

            if (this.schema["enum"])
            {
                var val = this.data;

                if (!this.isRequired() && Alpaca.isValEmpty(val))
                {
                    return true;
                }

                if (this.options.multiple)
                {
                    var isValid = true;

                    if (!val)
                    {
                        val = [];
                    }

                    if (!Alpaca.isArray(val) && !Alpaca.isObject(val))
                    {
                        val = [val];
                    }

                    $.each(val, function(i,v) {

                        if ($.inArray(v, _this.schema["enum"]) <= -1)
                        {
                            isValid = false;
                            return false;
                        }

                    });

                    return isValid;
                }
                else
                {
                    return ($.inArray(val, this.schema["enum"]) > -1);
                }
            }
            else
            {
                return true;
            }
        },

        /**
         * @see Alpaca.Field#onChange
         */
        onChange: function(e)
        {
            this.base(e);

            var _this = this;

            Alpaca.later(25, this, function() {
                var v = _this.getValue();
                _this.setValue(v);
                _this.refreshValidationState();
            });
        },

        /**
         * Validates if number of items has been less than minItems.
         * @returns {Boolean} true if number of items has been less than minItems
         */
        _validateMinItems: function()
        {
            if (this.schema.items && this.schema.items.minItems)
            {
                if ($(":selected",this.control).length < this.schema.items.minItems)
                {
                    return false;
                }
            }

            return true;
        },

        /**
         * Validates if number of items has been over maxItems.
         * @returns {Boolean} true if number of items has been over maxItems
         */
        _validateMaxItems: function()
        {
            if (this.schema.items && this.schema.items.maxItems)
            {
                if ($(":selected",this.control).length > this.schema.items.maxItems)
                {
                    return false;
                }
            }

            return true;
        },

        /**
         * @see Alpaca.ContainerField#handleValidate
         */
        handleValidate: function()
        {
            var baseStatus = this.base();

            var valInfo = this.validation;

            var status = this._validateMaxItems();
            valInfo["tooManyItems"] = {
                "message": status ? "" : Alpaca.substituteTokens(this.getMessage("tooManyItems"), [this.schema.items.maxItems]),
                "status": status
            };

            status = this._validateMinItems();
            valInfo["notEnoughItems"] = {
                "message": status ? "" : Alpaca.substituteTokens(this.getMessage("notEnoughItems"), [this.schema.items.minItems]),
                "status": status
            };

            return baseStatus && valInfo["tooManyItems"]["status"] && valInfo["notEnoughItems"]["status"];
        },

        /**
         * @see Alpaca.Field#focus
         */
        focus: function(onFocusCallback)
        {
            if (this.control && this.control.length > 0)
            {
                // set focus onto the select
                var el = $(this.control).get(0);

                el.focus();

                if (onFocusCallback)
                {
                    onFocusCallback(this);
                }
            }
        }

        /* builder_helpers */
        ,

        /**
         * @see Alpaca.Field#getTitle
         */
        getTitle: function() {
            return "Select Field";
        },

        /**
         * @see Alpaca.Field#getDescription
         */
        getDescription: function() {
            return "Select Field";
        },

        /**
         * @private
         * @see Alpaca.Fields.Image2Field#getSchemaOfOptions
         */
        getSchemaOfOptions: function() {
            return Alpaca.merge(this.base(), {
                "properties": {
                    "multiple": {
                        "title": "Mulitple Selection",
                        "description": "Allow multiple selection if true.",
                        "type": "boolean",
                        "default": false
                    },
                    "size": {
                        "title": "Displayed Options",
                        "description": "Number of options to be shown.",
                        "type": "number"
                    },
                    "emptySelectFirst": {
                        "title": "Empty Select First",
                        "description": "If the data is empty, then automatically select the first item in the list.",
                        "type": "boolean",
                        "default": false
                    },
                    "multiselect": {
                        "title": "Multiselect Plugin Settings",
                        "description": "Multiselect plugin properties - http://davidstutz.github.io/bootstrap-multiselect",
                        "type": "any"
                    }
                }
            });
        },

        /**
         * @private
         * @see Alpaca.Fields.Image2Field#getOptionsForOptions
         */
        getOptionsForOptions: function() {
            return Alpaca.merge(this.base(), {
                "fields": {
                    "multiple": {
                        "rightLabel": "Allow multiple selection ?",
                        "helper": "Allow multiple selection if checked",
                        "type": "checkbox"
                    },
                    "size": {
                        "type": "integer"
                    },
                    "emptySelectFirst": {
                        "type": "checkbox",
                        "rightLabel": "Empty Select First"
                    },
                    "multiselect": {
                        "type": "object",
                        "rightLabel": "Multiselect plugin properties - http://davidstutz.github.io/bootstrap-multiselect"
                    }
                }
            });
        }

        /* end_builder_helpers */

    });

    Alpaca.registerFieldClass("image2", Alpaca.Fields.Image2Field);

})(jQuery);
(function ($) {
    var Alpaca = $.alpaca;
    Alpaca.Fields.ImageCrop2Field = Alpaca.Fields.ListField.extend(
    /**
     * @lends Alpaca.Fields.ImageCrop2Field.prototype
     */
    {
        constructor: function (container, data, options, schema, view, connector) {
            var self = this;
            this.base(container, data, options, schema, view, connector);
            this.sf = connector.servicesFramework;
            this.dataSource = {};
        },
        /**
         * @see Alpaca.Field#getFieldType
         */
        getFieldType: function () {
            return "imagecrop2";
        },
        /**
         * @see Alpaca.Fields.ImageCrop2Field#setup
         */
        setup: function () {
            var self = this;
            if (!this.options.uploadfolder) {
                this.options.uploadfolder = "";
            }
            if (!this.options.cropfolder) {
                this.options.cropfolder = this.options.uploadfolder;
            }
            if (!this.options.uploadhidden) {
                this.options.uploadhidden = false;
            }
            if (!this.options.cropper) {
                this.options.cropper = {};
            }
            if (this.options.width && this.options.height) {
                this.options.cropper.aspectRatio = this.options.width / this.options.height;
            }
            this.options.cropper.responsive = false;
            if (!this.options.cropper.autoCropArea) {
                this.options.cropper.autoCropArea = 1;
            }
            if (!this.options.cropButtonHidden) {
                this.options.cropButtonHidden = false;
            }
            if (!this.options.cropButtonHidden) {
                this.options.buttons = {
                    "check": {
                        "value": "Crop",
                        "click": function () {
                            this.cropImage();
                        }
                    }
                };
            }
            this.base();
        },
        getValue: function () {
            var self = this;
            if (this.control && this.control.length > 0) {
                /*
                var val = this._getControlVal(true);
                if (typeof (val) === "undefined") {
                    val = this.data;
                }
                var url = this.base(val);
                */
                var value = null;
                $image = self.getImage();
                if (self.cropperExist())
                    value = $image.cropper('getData', { rounded: true });
                else
                    value = {};

                value.url = $(this.control).find('select').val();
                if (value.url) {
                    if (this.dataSource && this.dataSource[value.url]) {
                        value.id = this.dataSource[value.url].id;
                    }
                    value.cropUrl = $(self.getControlEl()).attr('data-cropurl');
                }
                return value;
            }
        },
        /**
         * @see Alpaca.Field#setValue
         */
        setValue: function (val) {
            var self = this;
            if (val !== this.getValue()) {
                /*
                if (!Alpaca.isEmpty(val) && this.control)
                {
                    this.control.val(val);
                }
                */
                
                if (this.control && typeof (val) != "undefined" && val != null) {
                    //this.base(val); ???
                    if (Alpaca.isEmpty(val)) {
                        self.cropper("");
                        $(this.control).find('select').val("");
                        $(self.getControlEl()).attr('data-cropurl', '');
                    }
                    else if (Alpaca.isObject(val)) {
                        // Fix for OC data that still has the Cachebuster SQ parameter
                        if (val.url) val.url = val.url.split("?")[0];
                        if (val.cropUrl) val.cropUrl = val.cropUrl.split("?")[0];

                        if (val.cropdata && Object.keys(val.cropdata).length > 0) { // compatibility with imagecropper
                            var firstcropdata = val.cropdata[Object.keys(val.cropdata)[0]];
                            self.cropper(val.url, firstcropdata.cropper);
                            $(this.control).find('select').val(val.url);
                            $(self.getControlEl()).attr('data-cropurl', firstcropdata.url);
                        } else {
                            self.cropper(val.url, val);
                            $(this.control).find('select').val(val.url);
                            $(self.getControlEl()).attr('data-cropurl', val.cropUrl);
                        }
                    }
                    else {
                        self.cropper(val);
                        $(this.control).find('select').val(val);
                        $(self.getControlEl()).attr('data-cropurl', '');
                    }
                    $(this.control).find('select').trigger('change.select2');
                }
            }
        },

        /**
         * @see Alpaca.ImageCrop2Field#getEnum
         */
        getEnum: function () {
            if (this.schema) {
                if (this.schema["enum"]) {
                    return this.schema["enum"];
                }
                else if (this.schema["type"] && this.schema["type"] === "array" && this.schema["items"] && this.schema["items"]["enum"]) {
                    return this.schema["items"]["enum"];
                }
            }
        },

        initControlEvents: function () {
            var self = this;
            self.base();
            if (self.options.multiple) {
                var button = this.control.parent().find(".select2-search__field");
                button.focus(function (e) {
                    if (!self.suspendBlurFocus) {
                        self.onFocus.call(self, e);
                        self.trigger("focus", e);
                    }
                });
                button.blur(function (e) {
                    if (!self.suspendBlurFocus) {
                        self.onBlur.call(self, e);
                        self.trigger("blur", e);
                    }
                });
                this.control.on("change", function (e) {
                    self.onChange.call(self, e);
                    self.trigger("change", e);
                });
            }
        },

        beforeRenderControl: function (model, callback) {
            var self = this;
            this.base(model, function () {
                self.selectOptions = [];
                if (self.sf) {
                    var completionFunction = function () {
                        self.schema.enum = [];
                        self.options.optionLabels = [];
                        for (var i = 0; i < self.selectOptions.length; i++) {
                            self.schema.enum.push(self.selectOptions[i].value);
                            self.options.optionLabels.push(self.selectOptions[i].text);
                        }
                        // push back to model
                        model.selectOptions = self.selectOptions;
                        callback();
                    };
                    var postData = { q: "*", folder: self.options.uploadfolder };
                    $.ajax({
                        url: self.sf.getServiceRoot("OpenContent") + "DnnEntitiesAPI" + "/" + "ImagesLookupExt",
                        beforeSend: self.sf.setModuleHeaders,
                        type: "get",
                        dataType: "json",
                        //contentType: "application/json; charset=utf-8",
                        data: postData,
                        success: function (jsonDocument) {
                            var ds = jsonDocument;
                            if (self.options.dsTransformer && Alpaca.isFunction(self.options.dsTransformer)) {
                                ds = self.options.dsTransformer(ds);
                            }
                            if (ds) {
                                if (Alpaca.isArray(ds)) {
                                    // for arrays, we walk through one index at a time
                                    // the insertion order is dictated by the order of the indices into the array
                                    // this preserves order
                                    $.each(ds, function (index, value) {
                                        self.selectOptions.push({
                                            "value": value.url,
                                            "thumbUrl": value.thumbUrl,
                                            "id": value.id,
                                            "text": value.text
                                        });
                                        self.dataSource[value.url] = value;
                                    });
                                    completionFunction();
                                }
                            }
                        },
                        "error": function (jqXHR, textStatus, errorThrown) {
                            self.errorCallback({
                                "message": "Unable to load data from uri : " + self.options.dataSource,
                                "stage": "DATASOURCE_LOADING_ERROR",
                                "details": {
                                    "jqXHR": jqXHR,
                                    "textStatus": textStatus,
                                    "errorThrown": errorThrown
                                }
                            });
                        }
                    });
                } else {
                    callback();
                }
            });
        },

        prepareControlModel: function (callback) {
            var self = this;
            this.base(function (model) {
                model.selectOptions = self.selectOptions;
                callback(model);
            });
        },

        afterRenderControl: function (model, callback) {
            var self = this;
            this.base(model, function () {
                // if emptySelectFirst and nothing currently checked, then pick first item in the value list
                // set data and visually select it
                if (Alpaca.isUndefined(self.data) && self.options.emptySelectFirst && self.selectOptions && self.selectOptions.length > 0) {
                    self.data = self.selectOptions[0].value;
                }
                // do this little trick so that if we have a default value, it gets set during first render
                // this causes the state of the control
                if (self.data) {
                    self.setValue(self.data);
                }

                if ($.fn.select2) {
                    var settings = null;
                    if (self.options.select2) {
                        settings = self.options.select2;
                    }
                    else {
                        settings = {};
                    }
                    /*
                    if (!settings.nonSelectedText)
                    {
                        settings.nonSelectedText = "None";
                        if (self.options.noneLabel)
                        {
                            settings.nonSelectedText = self.options.noneLabel;
                        }
                    }
                    if (self.options.hideNone)
                    {
                        delete settings.nonSelectedText;
                    }
                    */

                    settings.templateResult = function (state) {
                        if (!state.id) { return state.text; }

                        var $state = $(
                          '<span><img src="' + self.dataSource[state.id].thumbUrl + '" style="height: 45px;width: 54px;"  /> ' + state.text + '</span>'
                        );
                        return $state;
                    };

                    settings.templateSelection = function (state) {
                        if (!state.id) { return state.text; }

                        var $state = $(
                          '<span><img src="' + self.dataSource[state.id].thumbUrl + '" style="height: 15px;width: 18px;"  /> ' + state.text + '</span>'
                        );
                        return $state;
                    };

                    $(self.getControlEl().find('select')).select2(settings);
                }
                if (self.options.uploadhidden) {
                    $(self.getControlEl()).find('input[type=file]').hide();
                } else {
                    if (self.sf) {
                        $(self.getControlEl()).find('input[type=file]').fileupload({
                            dataType: 'json',
                            url: self.sf.getServiceRoot('OpenContent') + "FileUpload/UploadFile",
                            maxFileSize: 25000000,
                            formData: { uploadfolder: self.options.uploadfolder },
                            beforeSend: self.sf.setModuleHeaders,
                            add: function (e, data) {
                                //data.context = $(opts.progressContextSelector);
                                //data.context.find($(opts.progressFileNameSelector)).html(data.files[0].name);
                                //data.context.show('fade');
                                data.submit();
                            },
                            progress: function (e, data) {
                                if (data.context) {
                                    var progress = parseInt(data.loaded / data.total * 100, 10);
                                    data.context.find(opts.progressBarSelector).css('width', progress + '%').find('span').html(progress + '%');
                                }
                            },
                            done: function (e, data) {
                                if (data.result) {
                                    $.each(data.result, function (index, file) {
                                        self.refresh(function () {
                                            self.setValue(file.url);
                                        });                                       
                                    });
                                }
                            }
                        }).data('loaded', true);
                    }
                }


                callback();
            });
        },
        cropImage: function () {
            var self = this;
            var data = self.getValue();
            var postData = { url: data.url, cropfolder: self.options.cropfolder, crop: data, id: "crop" };
            if (self.options.width && self.options.height) {
                postData.resize = { width: self.options.width, height: self.options.height };
            }
            $(self.getControlEl()).css('cursor', 'wait');
            $.ajax({
                type: "POST",
                url: self.sf.getServiceRoot('OpenContent') + "DnnEntitiesAPI/CropImage",
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                data: JSON.stringify(postData),
                beforeSend: self.sf.setModuleHeaders
            }).done(function (res) {

                $(self.getControlEl()).attr('data-cropurl', res.url);

                setTimeout(function () {
                    $(self.getControlEl()).css('cursor', 'initial');
                }, 500);
            }).fail(function (xhr, result, status) {
                alert("Uh-oh, something broke: " + status);
                $(self.getControlEl()).css('cursor', 'initial');
            });
        },
        getFileUrl: function (fileid) {
            var self = this;
            if (self.sf) {
                var postData = { fileid: fileid };
                $.ajax({
                    url: self.sf.getServiceRoot("OpenContent") + "DnnEntitiesAPI" + "/" + "FileUrl",
                    beforeSend: self.sf.setModuleHeaders,
                    type: "get",
                    asych: false,
                    dataType: "json",
                    //contentType: "application/json; charset=utf-8",
                    data: postData,
                    success: function (data) {
                        return data;
                    },
                    error: function (jqXHR, textStatus, errorThrown) {
                        return "";
                    }
                });
            }
        },
        cropper: function (url, data) {
            var self = this;
            $image = self.getImage();
            $image.attr('src', url);
            var cropperExist = $image.data('cropper');
            if (url) {
                $image.show();
                if (!cropperExist) {
                    var config = $.extend({}, {
                        aspectRatio: 16 / 9,
                        checkOrientation: false,
                        autoCropArea: 0.90,
                        minContainerHeight: 200,
                        minContainerWidth: 400,
                        toggleDragModeOnDblclick: false
                    }, self.options.cropper);
                    if (data) {
                        config.data = data;
                    }
                    $image.cropper(config);
                } else {
                    if (url != cropperExist.originalUrl || (cropperExist.url && url != cropperExist.url)) {
                        $image.cropper('replace', url);
                    }
                    //$image.cropper('reset');
                    if (data) {
                        $image.cropper('setData', data);
                    }
                }
            } else {
                $image.hide();
                if (!cropperExist) {

                } else {
                    $image.cropper('destroy');
                }
            }
        },
        cropperExist: function () {
            var self = this;
            $image = self.getImage();
            var cropperData = $image.data('cropper');

            return cropperData;
        },
        getImage: function () {
            var self = this;
            return $(self.control).parent().find('#' + self.id + '-image'); //.find('.alpaca-image-display > img');

        },
        /**
         * Validate against enum property.
         *
         * @returns {Boolean} True if the element value is part of the enum list, false otherwise.
         */
        _validateEnum: function () {
            var _this = this;

            if (this.schema["enum"]) {
                var val = this.data ? this.data.url : "";

                if (!this.isRequired() && Alpaca.isValEmpty(val)) {
                    return true;
                }

                if (this.options.multiple) {
                    var isValid = true;

                    if (!val) {
                        val = [];
                    }

                    if (!Alpaca.isArray(val) && !Alpaca.isObject(val)) {
                        val = [val];
                    }

                    $.each(val, function (i, v) {

                        if ($.inArray(v, _this.schema["enum"]) <= -1) {
                            isValid = false;
                            return false;
                        }

                    });

                    return isValid;
                }
                else {
                    return ($.inArray(val, this.schema["enum"]) > -1);
                }
            }
            else {
                return true;
            }
        },

        /**
         * @see Alpaca.Field#onChange
         */
        onChange: function (e) {
            this.base(e);
            var _this = this;
            Alpaca.later(25, this, function () {
                var v = _this.getValue();
                _this.setValue(v);
                _this.refreshValidationState();
            });
        },

        /**
         * @see Alpaca.Field#focus
         */
        focus: function (onFocusCallback) {
            if (this.control && this.control.length > 0) {
                // set focus onto the select
                var el = $(this.control).find('select');

                el.focus();

                if (onFocusCallback) {
                    onFocusCallback(this);
                }
            }
        }

        /* builder_helpers */
        ,

        /**
         * @see Alpaca.Field#getTitle
         */
        getTitle: function () {
            return "Image Crop 2 Field";
        },

        /**
         * @see Alpaca.Field#getDescription
         */
        getDescription: function () {
            return "Image Crop 2 Field";
        },

    });

    Alpaca.registerFieldClass("imagecrop2", Alpaca.Fields.ImageCrop2Field);

})(jQuery);
(function ($) {

    var Alpaca = $.alpaca;
    
    Alpaca.Fields.ImageCropField = Alpaca.Fields.TextField.extend(
    /**
     * @lends Alpaca.Fields.ImageField.prototype
     */
    {
        constructor: function(container, data, options, schema, view, connector)
        {
            var self = this;
            this.base(container, data, options, schema, view, connector);
            this.sf = connector.servicesFramework;
            //this.cropper = false;
        },

        /**
         * @see Alpaca.Fields.TextField#getFieldType
         */
        getFieldType: function () {
            return "imagecrop";
        }
        ,
        setup: function () {
            if (!this.options.uploadfolder) {
                this.options.uploadfolder = "";
            }
            if (!this.options.uploadhidden) {
                this.options.uploadhidden = false;
            }
            if (!this.options.cropper) {
                this.options.cropper = {};
            }
            this.options.cropper.responsive = false;
            if (!this.options.cropper.autoCropArea) {
                this.options.cropper.autoCropArea = 1;
            }
            this.base();
        },

        /**
         * @see Alpaca.Fields.TextField#getTitle
         */
        getTitle: function () {
            return "Image Crop Field";
        },

        /**
         * @see Alpaca.Fields.TextField#getDescription
         */
        getDescription: function () {
            return "Image Crop Field.";
        },
        getTextControlEl: function () {
            return $(this.control.get(0)).find('input[type=text]#' + this.id);
        },
        setValue: function (value) {
            var self = this;
            //var el = $( this.control).filter('#'+this.id);
            //var el = $(this.control.get(0)).find('input[type=text]');
            var el = this.getTextControlEl();
            //$image = $(self.control).parent().find('.alpaca-image-display > img');
            if (el && el.length > 0) {
                if (Alpaca.isEmpty(value)) {
                    el.val("");
                    self.cropper("");
                }
                else if (Alpaca.isObject(value)) {
                    el.val(value.url);
                    self.cropper(value.url, value);
                }
                else {
                    el.val(value);
                    self.cropper(value);
                }
            }
            
            // be sure to call into base method
            //this.base(value);

            // if applicable, update the max length indicator
            this.updateMaxLengthIndicator();
        },

        getValue: function () {
            var self = this;
            var value = null;

            //var el = $(this.control).filter('#' + this.id);
            //var el = $(this.control.get(0)).find('input[type=text]');
            var el = this.getTextControlEl();
            $image = self.getImage();
            if (el && el.length > 0) {
                if (self.cropperExist())
                    value = $image.cropper('getData', { rounded: true });
                else
                    value = {};

                value.url = el.val();
            }
            return value;
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
            var el = this.getTextControlEl();
            $image = $(self.control).parent().find('.alpaca-image-display img');
            if (self.sf){
                //var el = this.control;
                if (self.options.uploadhidden) {
                    $(this.control.get(0)).find('input[type=file]').hide();
                } else {
                    $(this.control.get(0)).find('input[type=file]').fileupload({
                        dataType: 'json',
                        url: self.sf.getServiceRoot('OpenContent') + "FileUpload/UploadFile",
                        maxFileSize: 25000000,
                        formData: { uploadfolder : self.options.uploadfolder },
                        beforeSend: self.sf.setModuleHeaders,
                        add: function (e, data) {
                            //data.context = $(opts.progressContextSelector);
                            //data.context.find($(opts.progressFileNameSelector)).html(data.files[0].name);
                            //data.context.show('fade');
                            data.submit();
                        },
                        progress: function (e, data) {
                            if (data.context) {
                                var progress = parseInt(data.loaded / data.total * 100, 10);
                                data.context.find(opts.progressBarSelector).css('width', progress + '%').find('span').html(progress + '%');
                            }
                        },
                        done: function (e, data) {
                            if (data.result) {
                                $.each(data.result, function (index, file) {
                                    //self.setValue(file.url);
                                    $(el).val(file.url);
                                    $(el).change();
                                    //$(el).change();
                                    //$(e.target).parent().find('input[type=text]').val(file.url);
                                    //el.val(file.url);
                                    //$(e.target).parent().find('.alpaca-image-display img').attr('src', file.url);
                                });
                            }
                        }
                    }).data('loaded', true);
                }
                $(el).change(function () {
                    //self.cropper = false;
                    var value = $(this).val();
                    self.cropper(value);
                    /*
                    $image.attr('src', value);
                    if (value) {
                        $image.parent().find('.cropper-container').show();
                    
                    }
                    else {
                        $image.parent().find('.cropper-container').hide();
                        if (self.cropper) {
                        
                            //self.cropper = false;
                        }
                    }
                    */

                });

                if (self.options.manageurl) {
                    var manageButton = $('<a href="' + self.options.manageurl + '" target="_blank" class="alpaca-form-button">Manage files</a>').appendTo($(el).parent());
                }
            }
            else {
                $image.hide();
            }
            callback();
            
        },
        cropper: function (url, data) {
            var self = this;
            $image = self.getImage();
            $image.attr('src', url);
            var cropperExist = $image.data('cropper');
            if (url) {
                $image.show();
                if (!cropperExist) {
                    var config = $.extend({}, {
                        aspectRatio: 16 / 9,
                        checkOrientation: false,
                        autoCropArea: 0.90,
                        minContainerHeight: 200,
                        minContainerWidth: 400,
                        toggleDragModeOnDblclick: false

                    }, self.options.cropper);
                    if (data) {
                        config.data = data;
                    }

                    $image.cropper(config);
                } else {
                    if (url != cropperExist.originalUrl){
                        $image.cropper('replace', url);
                    }
                    //$image.cropper('reset');
                    if (data) {
                        $image.cropper('setData', data);
                    }
                }
                
            } else {
                $image.hide();
                if (!cropperExist) {

                } else {
                    $image.cropper('destroy');
                }
            }
        },
        cropperExist: function () {
            var self = this;
            $image = self.getImage();
            var cropperData = $image.data('cropper');
            
            return cropperData;
        },
        getImage: function () {
            var self = this;
            return $(self.control).parent().find('#'+self.id+'-image'); //.find('.alpaca-image-display > img');

        },
        applyTypeAhead: function () {
            var self = this;

            if (self.control.typeahead && self.options.typeahead && !Alpaca.isEmpty(self.options.typeahead) && self.sf) {

                var tConfig = self.options.typeahead.config;
                if (!tConfig) {
                    tConfig = {};
                }
                var tDatasets = tDatasets = {};
                if (!tDatasets.name) {
                    tDatasets.name = self.getId();
                }

                var tFolder = self.options.typeahead.Folder;
                if (!tFolder) {
                    tFolder = "";
                }

                var tEvents = tEvents = {};

                var bloodHoundConfig = {
                    datumTokenizer: function (d) {
                        return Bloodhound.tokenizers.whitespace(d.value);
                    },
                    queryTokenizer: Bloodhound.tokenizers.whitespace
                };

                /*
                if (tDatasets.type === "prefetch") {
                    bloodHoundConfig.prefetch = {
                        url: tDatasets.source,
                        ajax: {
                            //url: sf.getServiceRoot('OpenContent') + "FileUpload/UploadFile",
                            beforeSend: connector.servicesFramework.setModuleHeaders,
        
                        }
                    };
        
                    if (tDatasets.filter) {
                        bloodHoundConfig.prefetch.filter = tDatasets.filter;
                    }
                }
                */

                bloodHoundConfig.remote = {
                    url: self.sf.getServiceRoot('OpenContent') + "DnnEntitiesAPI/Images?q=%QUERY&d=" + tFolder,
                    ajax: {
                        beforeSend: self.sf.setModuleHeaders,

                    }
                };

                if (tDatasets.filter) {
                    bloodHoundConfig.remote.filter = tDatasets.filter;
                }

                if (tDatasets.replace) {
                    bloodHoundConfig.remote.replace = tDatasets.replace;
                }


                var engine = new Bloodhound(bloodHoundConfig);
                engine.initialize();
                tDatasets.source = engine.ttAdapter();

                tDatasets.templates = {
                    "empty": "Nothing found...",
                    "suggestion": "<div style='width:20%;display:inline-block;background-color:#fff;padding:2px;'><img src='{{value}}' style='height:40px' /></div> {{name}}"
                };

                // compile templates
                if (tDatasets.templates) {
                    for (var k in tDatasets.templates) {
                        var template = tDatasets.templates[k];
                        if (typeof (template) === "string") {
                            tDatasets.templates[k] = Handlebars.compile(template);
                        }
                    }
                }

                //var el = $(this.control.get(0)).find('input[type=text]');
                var el = this.getTextControlEl();
                // process typeahead
                $(el).typeahead(tConfig, tDatasets);

                // listen for "autocompleted" event and set the value of the field
                $(el).on("typeahead:autocompleted", function (event, datum) {
                    //self.setValue(datum.value);
                    $(el).val(datum.value);
                    $(el).change();
                    //$(self.control).parent().find('input[type=text]').val(datum.value);
                    //$(self.control).parent().find('.alpaca-image-display img').attr('src', datum.value);
                });

                // listen for "selected" event and set the value of the field
                $(el).on("typeahead:selected", function (event, datum) {
                    //self.setValue(datum.value);
                    $(el).val(datum.value);
                    $(el).change();
                    //$(self.control).parent().find('input[type=text]').val(datum.value);
                    //$(self.control).parent().find('.alpaca-image-display img').attr('src', datum.value);
                });

                // custom events
                if (tEvents) {
                    if (tEvents.autocompleted) {
                        $(el).on("typeahead:autocompleted", function (event, datum) {
                            tEvents.autocompleted(event, datum);
                        });
                    }
                    if (tEvents.selected) {
                        $(el).on("typeahead:selected", function (event, datum) {
                            tEvents.selected(event, datum);
                        });
                    }
                }

                // when the input value changes, change the query in typeahead
                // this is to keep the typeahead control sync'd with the actual dom value
                // only do this if the query doesn't already match
                //var fi = $(self.control);
                $(el).change(function () {

                    var value = $(this).val();

                    var newValue = $(el).typeahead('val');
                    if (newValue !== value) {
                        $(el).typeahead('val', value);
                    }

                });

                // some UI cleanup (we don't want typeahead to restyle)
                $(self.field).find("span.twitter-typeahead").first().css("display", "block"); // SPAN to behave more like DIV, next line
                $(self.field).find("span.twitter-typeahead input.tt-input").first().css("background-color", "");
            }
        }

        /* end_builder_helpers */
    });

    Alpaca.registerFieldClass("imagecrop", Alpaca.Fields.ImageCropField);

})(jQuery);
(function ($) {

    var Alpaca = $.alpaca;

    Alpaca.Fields.ImageCropperField = Alpaca.Fields.TextField.extend(
    /**
     * @lends Alpaca.Fields.ImageField.prototype
     */
    {
        constructor: function (container, data, options, schema, view, connector) {
            var self = this;
            this.base(container, data, options, schema, view, connector);
            this.sf = connector.servicesFramework;
        },

        /**
         * @see Alpaca.Fields.TextField#getFieldType
         */
        getFieldType: function () {
            return "imagecropper";
        },
        setup: function () {
            if (!this.options.uploadfolder) {
                this.options.uploadfolder = "";
            }
            if (!this.options.uploadhidden) {
                this.options.uploadhidden = false;
            }
            if (!this.options.cropper) {
                this.options.cropper = {};
            }
            this.options.cropper.responsive = false;
            if (!this.options.cropper.autoCropArea) {
                this.options.cropper.autoCropArea = 1;
            }
            this.base();
        },

        /**
         * @see Alpaca.Fields.TextField#getTitle
         */
        getTitle: function () {
            return "Image Cropper Field";
        },

        /**
         * @see Alpaca.Fields.TextField#getDescription
         */
        getDescription: function () {
            return "Image Cropper Field.";
        },
        getTextControlEl: function () {
            return $(this.control.get(0)).find('input[type=text]#' + this.id);
        },
        setValue: function (value) {

            //var el = $( this.control).filter('#'+this.id);
            //var el = $(this.control.get(0)).find('input[type=text]');
            var el = this.getTextControlEl();

            if (el && el.length > 0) {
                if (Alpaca.isEmpty(value)) {
                    el.val("");
                }
                else if (Alpaca.isString(value)) {
                    el.val(value);
                }
                else {
                    el.val(value.url);
                    this.setCroppedData(value.cropdata);
                }
            }
            // be sure to call into base method
            //this.base(textvalue);

            // if applicable, update the max length indicator
            this.updateMaxLengthIndicator();
        },

        getValue: function () {
            var value = null;
            var el = this.getTextControlEl();
            if (el && el.length > 0) {
                //value = el.val();
                value = {
                    url: el.val()
                };
                value.cropdata = this.getCroppedData();
            }
            return value;
        },
        getCroppedData: function () {
            var el = this.getTextControlEl();
            var cropdata = {};
            for (var i in this.options.croppers) {
                var cropper = this.options.croppers[i];
                var id = this.id + '-' + i;
                var $cropbutton = $('#' + id);
                cropdata[i] = $cropbutton.data('cropdata');
            }
            return cropdata;
        },
        cropAllImages: function (url) {
            var self = this;
            for (var i in this.options.croppers) {

                var id = this.id + '-' + i;
                var $cropbutton = $('#' + id);

                //cropdata[i] = $cropbutton.data('cropdata');

                var cropopt = this.options.croppers[i];

                var crop = { "x": -1, "y": -1, "width": cropopt.width, "height": cropopt.height, "rotate": 0 };
                var postData = JSON.stringify({ url: url, id: i, crop: crop, resize: cropopt, cropfolder: this.options.cropfolder });

                var action = "CropImage";
                $.ajax({
                    type: "POST",
                    url: self.sf.getServiceRoot('OpenContent') + "DnnEntitiesAPI/" + action,
                    contentType: "application/json; charset=utf-8",
                    dataType: "json",
                    async: false,
                    data: postData,
                    beforeSend: self.sf.setModuleHeaders
                }).done(function (res) {
                    var cropdata = { url: res.url, cropper: {} };
                    self.setCroppedDataForId(id, cropdata);

                }).fail(function (xhr, result, status) {
                    alert("Uh-oh, something broke: " + status);
                });

            }
            //var data = $image.cropper('getData', { rounded: true });
            //var cropperId = cropButton.data('cropperId');

        },
        setCroppedData: function (value) {

            var el = this.getTextControlEl();
            var parentel = this.getFieldEl();
            if (el && el.length > 0) {
                if (Alpaca.isEmpty(value)) {

                }
                else {
                    var firstCropButton;
                    for (var i in this.options.croppers) {
                        var cropper = this.options.croppers[i];
                        var id = this.id + '-' + i;
                        var $cropbutton = $('#' + id);
                        cropdata = value[i];
                        if (cropdata) {
                            $cropbutton.data('cropdata', cropdata);
                        }

                        if (!firstCropButton) {
                            firstCropButton = $cropbutton;
                            $(firstCropButton).addClass('active');
                            if (cropdata) {
                                var $image = $(parentel).find('.alpaca-image-display img.image');
                                var cropper = $image.data('cropper');
                                if (cropper) {
                                    $image.cropper('setData', cropdata.cropper);
                                }
                            }
                        }

                    }
                }
            }

            /*
            var el = this.getTextControlEl();
            var $image = el.parent().find('.image');
            if (el && el.length > 0) {
                if (Alpaca.isEmpty(value)) {
                    $image.data('cropdata', {});
                }
                else {
                    $image.data('cropdata', value);
                }
            }
            */
        },

        setCroppedDataForId: function (id, value) {
            var el = this.getTextControlEl();
            if (value) {
                var $cropbutton = $('#' + id);
                $cropbutton.data('cropdata', value);
            }
        },
        getCurrentCropData: function () {
            /*
            var el = this.getTextControlEl();
            var curtab = $(el).parent().parent().find(".alpaca-form-tab.active");
            var cropdata = $(this).data('cropdata');
            */

            var el = this.getFieldEl(); //this.getTextControlEl();
            var curtab = $(el).parent().find(".alpaca-form-tab.active");
            var cropdata = $(curtab).data('cropdata');
            return cropdata;
        },
        setCurrentCropData: function (value) {
            var el = this.getFieldEl(); //this.getTextControlEl();
            var curtab = $(el).parent().find(".alpaca-form-tab.active");
            $(curtab).data('cropdata', value);

        },
        afterRenderControl: function (model, callback) {
            var self = this;
            this.base(model, function () {
                self.handlePostRender(function () {
                    callback();
                });
            });
        },
        cropChange: function (e) {
            var self = e.data;
            //var parentel = this.getFieldEl();

            var currentCropdata = self.getCurrentCropData();
            if (currentCropdata) {
                var cropper = currentCropdata.cropper;
                var $image = this; //$(parentel).find('.alpaca-image-display img.image');
                var data = $(this).cropper('getData', { rounded: true });
                if (data.x != cropper.x ||
                    data.y != cropper.y ||
                    data.width != cropper.width ||
                    data.height != cropper.height ||
                    data.rotate != cropper.rotate) {

                    var cropdata = {
                        url: "",
                        cropper: data
                    };
                    self.setCurrentCropData(cropdata);
                }
            }
            //self.setCroppedDataForId(cropperButtonIdcropButton.data('cropperButtonId'), cropdata);

        },
        getCropppersData: function () {
            for (var i in self.options.croppers) {
                var cropper = self.options.croppers[i];
                var id = self.id + '-' + i;

            }
        },
        handlePostRender: function (callback) {
            var self = this;
            var el = this.getTextControlEl();
            var parentel = this.getFieldEl();

            var cropButton = $('<a href="#" class="alpaca-form-button">Crop</a>');//.appendTo($(el).parent());
            cropButton.click(function () {
                /*
                var data = $image.cropper('getData', { rounded: true });
                var cropperId = cropButton.data('cropperId');
                var cropopt = self.options.croppers[cropperId];
                var postData = JSON.stringify({ url: el.val(), id: cropperId, crop: data, resize: cropopt });
                */
                var data = self.getCroppedData();
                var postData = JSON.stringify({ url: el.val(), cropfolder: self.options.cropfolder, cropdata: data, croppers: self.options.croppers });


                $(cropButton).css('cursor', 'wait');

                var action = "CropImages";
                $.ajax({
                    type: "POST",
                    url: self.sf.getServiceRoot('OpenContent') + "DnnEntitiesAPI/" + action,
                    contentType: "application/json; charset=utf-8",
                    dataType: "json",
                    data: postData,
                    beforeSend: self.sf.setModuleHeaders
                }).done(function (res) {
                    /*
                    var cropdata = { url: res.url, cropper: data };
                    self.setCroppedDataForId(cropButton.data('cropperButtonId'), cropdata);
                    */
                    for (var i in self.options.croppers) {
                        var cropper = self.options.croppers[i];
                        var id = self.id + '-' + i;
                        var $cropbutton = $('#' + id);
                        if (res.cropdata[i]) {
                            var cropdata = { url: res.cropdata[i].url, cropper: res.cropdata[i].crop };
                            if (cropdata) {
                                $cropbutton.data('cropdata', cropdata);
                            }
                        }
                    }
                    setTimeout(function () {
                        $(cropButton).css('cursor', 'initial');
                    }, 500);
                }).fail(function (xhr, result, status) {
                    alert("Uh-oh, something broke: " + status);
                    $(parentel).css('cursor', 'initial');
                });
                return false;
            });

            var firstCropButton;
            for (var i in self.options.croppers) {
                var cropper = self.options.croppers[i];
                var id = self.id + '-' + i;
                var cropperButton = $('<a id="' + id + '" data-id="' + i + '" href="#" class="alpaca-form-tab" >' + i + '</a>').appendTo($(el).parent());
                cropperButton.data('cropopt', cropper);
                cropperButton.click(function () {
                    $image.off('crop.cropper');

                    var cropdata = $(this).data('cropdata');
                    var cropopt = $(this).data('cropopt');
                    $image.cropper('setAspectRatio', cropopt.width / cropopt.height);
                    if (cropdata) {
                        $image.cropper('setData', cropdata.cropper);
                    } else {
                        $image.cropper('reset');
                    }
                    cropButton.data('cropperButtonId', this.id);
                    cropButton.data('cropperId', $(this).attr("data-id"));

                    $(this).parent().find('.alpaca-form-tab').removeClass('active');
                    $(this).addClass('active');

                    $image.on('crop.cropper', self, self.cropChange);

                    return false;
                });
                if (!firstCropButton) {
                    firstCropButton = cropperButton;
                    $(firstCropButton).addClass('active');
                    cropButton.data('cropperButtonId', $(firstCropButton).attr('id'));
                    cropButton.data('cropperId', $(firstCropButton).attr("data-id"));
                }
            }

            var $image = $(parentel).find('.alpaca-image-display img.image');
            //.cropper is a call to external cropper.js file. AlpacaEngine is responsible for loading that file.
            $image.cropper(self.options.cropper).on('built.cropper', function () {
                var cropopt = $(firstCropButton).data('cropopt');
                if (cropopt) {
                    $(this).cropper('setAspectRatio', cropopt.width / cropopt.height);
                }
                var cropdata = $(firstCropButton).data('cropdata');
                if (cropdata) {
                    $(this).cropper('setData', cropdata.cropper);
                }
                var $image = $(parentel).find('.alpaca-image-display img.image');
                $image.on('crop.cropper', self, self.cropChange);
            });

            if (self.options.uploadhidden) {
                $(this.control.get(0)).find('input[type=file]').hide();
            } else {
                $(this.control.get(0)).find('input[type=file]').fileupload({
                    dataType: 'json',
                    url: self.sf.getServiceRoot('OpenContent') + "FileUpload/UploadFile",
                    maxFileSize: 25000000,
                    formData: { uploadfolder: self.options.uploadfolder },
                    beforeSend: self.sf.setModuleHeaders,
                    add: function (e, data) {
                        //data.context = $(opts.progressContextSelector);
                        //data.context.find($(opts.progressFileNameSelector)).html(data.files[0].name);
                        //data.context.show('fade');
                        data.submit();
                    },
                    progress: function (e, data) {
                        if (data.context) {
                            var progress = parseInt(data.loaded / data.total * 100, 10);
                            data.context.find(opts.progressBarSelector).css('width', progress + '%').find('span').html(progress + '%');
                        }
                    },
                    done: function (e, data) {
                        if (data.result) {
                            $.each(data.result, function (index, file) {
                                //self.setValue(file.url);
                                el.val(file.url);

                                $(el).change();
                                //$(el).change();
                                //$(e.target).parent().find('input[type=text]').val(file.url);
                                //el.val(file.url);
                                //$(e.target).parent().find('.alpaca-image-display img').attr('src', file.url);
                            });
                        }
                    }
                }).data('loaded', true);
            }
            $(el).change(function () {

                var value = $(this).val();

                //var newValue = $(el).typeahead('val');
                //if (newValue !== value) {
                $(parentel).find('.alpaca-image-display img.image').attr('src', value);
                $image.cropper('replace', value);
                if (value) {
                    self.cropAllImages(value);
                }

                //}

            });
            cropButton.appendTo($(el).parent());
            if (self.options.manageurl) {
                var manageButton = $('<a href="' + self.options.manageurl + '" target="_blank" class="alpaca-form-button">Manage files</a>').appendTo($(el).parent());
            }


            callback();
        },
        applyTypeAhead: function () {
            var self = this;

            if (self.control.typeahead && self.options.typeahead && !Alpaca.isEmpty(self.options.typeahead)) {

                var tConfig = self.options.typeahead.config;
                if (!tConfig) {
                    tConfig = {};
                }
                var tDatasets = tDatasets = {};
                if (!tDatasets.name) {
                    tDatasets.name = self.getId();
                }

                var tFolder = self.options.typeahead.Folder;
                if (!tFolder) {
                    tFolder = "";
                }

                var tEvents = tEvents = {};

                var bloodHoundConfig = {
                    datumTokenizer: function (d) {
                        return Bloodhound.tokenizers.whitespace(d.value);
                    },
                    queryTokenizer: Bloodhound.tokenizers.whitespace
                };

                /*
                if (tDatasets.type === "prefetch") {
                    bloodHoundConfig.prefetch = {
                        url: tDatasets.source,
                        ajax: {
                            //url: sf.getServiceRoot('OpenContent') + "FileUpload/UploadFile",
                            beforeSend: connector.servicesFramework.setModuleHeaders,
        
                        }
                    };
        
                    if (tDatasets.filter) {
                        bloodHoundConfig.prefetch.filter = tDatasets.filter;
                    }
                }
                */

                bloodHoundConfig.remote = {
                    url: self.sf.getServiceRoot('OpenContent') + "DnnEntitiesAPI/Images?q=%QUERY&d=" + tFolder,
                    ajax: {
                        beforeSend: self.sf.setModuleHeaders,

                    }
                };

                if (tDatasets.filter) {
                    bloodHoundConfig.remote.filter = tDatasets.filter;
                }

                if (tDatasets.replace) {
                    bloodHoundConfig.remote.replace = tDatasets.replace;
                }


                var engine = new Bloodhound(bloodHoundConfig);
                engine.initialize();
                tDatasets.source = engine.ttAdapter();

                tDatasets.templates = {
                    "empty": "Nothing found...",
                    "suggestion": "<div style='width:20%;display:inline-block;background-color:#fff;padding:2px;'><img src='{{value}}' style='height:40px' /></div> {{name}}"
                };

                // compile templates
                if (tDatasets.templates) {
                    for (var k in tDatasets.templates) {
                        var template = tDatasets.templates[k];
                        if (typeof (template) === "string") {
                            tDatasets.templates[k] = Handlebars.compile(template);
                        }
                    }
                }

                //var el = $(this.control.get(0)).find('input[type=text]');
                var el = this.getTextControlEl();
                // process typeahead
                $(el).typeahead(tConfig, tDatasets);

                // listen for "autocompleted" event and set the value of the field
                $(el).on("typeahead:autocompleted", function (event, datum) {
                    //self.setValue(datum.value);
                    el.val(datum.value);
                    $(el).change();
                    //$(self.control).parent().find('input[type=text]').val(datum.value);
                    //$(self.control).parent().find('.alpaca-image-display img').attr('src', datum.value);
                });

                // listen for "selected" event and set the value of the field
                $(el).on("typeahead:selected", function (event, datum) {
                    //self.setValue(datum.value);
                    el.val(datum.value);
                    $(el).change();
                    //$(self.control).parent().find('input[type=text]').val(datum.value);
                    //$(self.control).parent().find('.alpaca-image-display img').attr('src', datum.value);
                });

                // custom events
                if (tEvents) {
                    if (tEvents.autocompleted) {
                        $(el).on("typeahead:autocompleted", function (event, datum) {
                            tEvents.autocompleted(event, datum);
                        });
                    }
                    if (tEvents.selected) {
                        $(el).on("typeahead:selected", function (event, datum) {
                            tEvents.selected(event, datum);
                        });
                    }
                }

                // when the input value changes, change the query in typeahead
                // this is to keep the typeahead control sync'd with the actual dom value
                // only do this if the query doesn't already match
                //var fi = $(self.control);
                $(el).change(function () {

                    var value = $(this).val();

                    var newValue = $(el).typeahead('val');
                    if (newValue !== value) {
                        $(el).typeahead('val', value);
                    }

                });

                // some UI cleanup (we don't want typeahead to restyle)
                $(self.field).find("span.twitter-typeahead").first().css("display", "block"); // SPAN to behave more like DIV, next line
                $(self.field).find("span.twitter-typeahead input.tt-input").first().css("background-color", "");
            }
        }

        /* end_builder_helpers */
    });

    Alpaca.registerFieldClass("imagecropper", Alpaca.Fields.ImageCropperField);

})(jQuery);
(function($) {

    var Alpaca = $.alpaca;

    Alpaca.Fields.NumberField = Alpaca.Fields.TextField.extend(
    /**
     * @lends Alpaca.Fields.NumberField.prototype
     */
    {
        constructor: function (container, data, options, schema, view, connector) {
            var self = this;
            this.base(container, data, options, schema, view, connector);
            this.numberDecimalSeparator = connector.numberDecimalSeparator || ".";
        },
        /**
         * @see Alpaca.Fields.TextField#setup
         */
        setup: function()
        {
            // default html5 input type = "number";
            //this.inputType = "number";
            // TODO: we can't do this because Chrome screws up it's handling of number type
            // and prevents us from validating properly
            // @see http://stackoverflow.com/questions/16420828/jquery-val-refuses-to-return-non-numeric-input-from-a-number-field-under-chrome

            this.base();
        },

        /**
         * @see Alpaca.Fields.TextField#getFieldType
         */
        getFieldType: function() {
            return "number";
        },

        /**
         * @see Alpaca.Fields.TextField#getValue
         */
        getValue: function()
        {
            var val = this._getControlVal(false);

            if (typeof(val) == "undefined" || "" == val)
            {
                return val;
            }

            if (this.numberDecimalSeparator != '.') {                
                val = ("" + val).replace(this.numberDecimalSeparator, '.');
            }

            return parseFloat(val);
        },
        setValue: function (value) {
            var val = value;
            if (this.numberDecimalSeparator != '.') {
                if (Alpaca.isEmpty(value)) {
                    val = "";
                }
                else {
                    val = ("" + value).replace('.', this.numberDecimalSeparator);
                }
            }
            // be sure to call into base method
            this.base(val);

        },

        /**
         * @see Alpaca.Fields.TextField#handleValidate
         */
        handleValidate: function() {
            var baseStatus = this.base();

            var valInfo = this.validation;

            var status = this._validateNumber();
            valInfo["stringNotANumber"] = {
                "message": status ? "" : this.view.getMessage("stringNotANumber"),
                "status": status
            };

            status = this._validateDivisibleBy();
            valInfo["stringDivisibleBy"] = {
                "message": status ? "" : Alpaca.substituteTokens(this.view.getMessage("stringDivisibleBy"), [this.schema.divisibleBy]),
                "status": status
            };

            status = this._validateMaximum();
            valInfo["stringValueTooLarge"] = {
                "message": "",
                "status": status
            };
            if (!status) {
                if (this.schema.exclusiveMaximum) {
                    valInfo["stringValueTooLarge"]["message"] = Alpaca.substituteTokens(this.view.getMessage("stringValueTooLargeExclusive"), [this.schema.maximum]);
                } else {
                    valInfo["stringValueTooLarge"]["message"] = Alpaca.substituteTokens(this.view.getMessage("stringValueTooLarge"), [this.schema.maximum]);
                }
            }

            status = this._validateMinimum();
            valInfo["stringValueTooSmall"] = {
                "message": "",
                "status": status
            };
            if (!status) {
                if (this.schema.exclusiveMinimum) {
                    valInfo["stringValueTooSmall"]["message"] = Alpaca.substituteTokens(this.view.getMessage("stringValueTooSmallExclusive"), [this.schema.minimum]);
                } else {
                    valInfo["stringValueTooSmall"]["message"] = Alpaca.substituteTokens(this.view.getMessage("stringValueTooSmall"), [this.schema.minimum]);
                }
            }

            status = this._validateMultipleOf();
            valInfo["stringValueNotMultipleOf"] = {
                "message": "",
                "status": status
            };
            if (!status)
            {
                valInfo["stringValueNotMultipleOf"]["message"] = Alpaca.substituteTokens(this.view.getMessage("stringValueNotMultipleOf"), [this.schema.multipleOf]);
            }

            // hand back a true/false
            return baseStatus && valInfo["stringNotANumber"]["status"] && valInfo["stringDivisibleBy"]["status"] && valInfo["stringValueTooLarge"]["status"] && valInfo["stringValueTooSmall"]["status"] && valInfo["stringValueNotMultipleOf"]["status"];
        },

        /**
         * Validates if it is a float number.
         * @returns {Boolean} true if it is a float number
         */
        _validateNumber: function() {

            // get value as text
            var textValue = this._getControlVal();
            if (this.numberDecimalSeparator != '.') {
                textValue = textValue.replace(this.numberDecimalSeparator, '.');
            }

            if (typeof(textValue) === "number")
            {
                textValue = "" + textValue;
            }

            // allow empty
            if (Alpaca.isValEmpty(textValue)) {
                return true;
            }

            // check if valid number format
            var validNumber = Alpaca.testRegex(Alpaca.regexps.number, textValue);
            if (!validNumber)
            {
                return false;
            }

            // quick check to see if what they entered was a number
            var floatValue = this.getValue();
            if (isNaN(floatValue)) {
                return false;
            }

            return true;
        },

        /**
         * Validates divisibleBy constraint.
         * @returns {Boolean} true if it passes the divisibleBy constraint.
         */
        _validateDivisibleBy: function() {
            var floatValue = this.getValue();
            if (!Alpaca.isEmpty(this.schema.divisibleBy)) {

                // mod
                if (floatValue % this.schema.divisibleBy !== 0)
                {
                    return false;
                }
            }
            return true;
        },

        /**
         * Validates maximum constraint.
         * @returns {Boolean} true if it passes the maximum constraint.
         */
        _validateMaximum: function() {
            var floatValue = this.getValue();

            if (!Alpaca.isEmpty(this.schema.maximum)) {
                if (floatValue > this.schema.maximum) {
                    return false;
                }

                if (!Alpaca.isEmpty(this.schema.exclusiveMaximum)) {
                    if (floatValue == this.schema.maximum && this.schema.exclusiveMaximum) { // jshint ignore:line
                        return false;
                    }
                }
            }

            return true;
        },

        /**
         * Validates maximum constraint.
         * @returns {Boolean} true if it passes the minimum constraint.
         */
        _validateMinimum: function() {
            var floatValue = this.getValue();

            if (!Alpaca.isEmpty(this.schema.minimum)) {
                if (floatValue < this.schema.minimum) {
                    return false;
                }

                if (!Alpaca.isEmpty(this.schema.exclusiveMinimum)) {
                    if (floatValue == this.schema.minimum && this.schema.exclusiveMinimum) { // jshint ignore:line
                        return false;
                    }
                }
            }

            return true;
        },

        /**
         * Validates multipleOf constraint.
         * @returns {Boolean} true if it passes the multipleOf constraint.
         */
        _validateMultipleOf: function() {
            var floatValue = this.getValue();

            if (!Alpaca.isEmpty(this.schema.multipleOf)) {
                if (floatValue && this.schema.multipleOf !== 0)
                {
                    return false;
                }
            }

            return true;
        },

        /**
         * @see Alpaca.Fields.TextField#getType
         */
        getType: function() {
            return "number";
        },

        /* builder_helpers */

        /**
         * @private
         * @see Alpaca.Fields.TextField#getSchemaOfSchema
         */
        getSchemaOfSchema: function() {
            return Alpaca.merge(this.base(), {
                "properties": {
                    "multipleOf": {
                        "title": "Multiple Of",
                        "description": "Property value must be a multiple of the multipleOf schema property such that division by this value yields an interger (mod zero).",
                        "type": "number"
                    },
                    "minimum": {
                        "title": "Minimum",
                        "description": "Minimum value of the property.",
                        "type": "number"
                    },
                    "maximum": {
                        "title": "Maximum",
                        "description": "Maximum value of the property.",
                        "type": "number"
                    },
                    "exclusiveMinimum": {
                        "title": "Exclusive Minimum",
                        "description": "Property value can not equal the number defined by the minimum schema property.",
                        "type": "boolean",
                        "default": false
                    },
                    "exclusiveMaximum": {
                        "title": "Exclusive Maximum",
                        "description": "Property value can not equal the number defined by the maximum schema property.",
                        "type": "boolean",
                        "default": false
                    }
                }
            });
        },

        /**
         * @private
         * @see Alpaca.Fields.TextField#getOptionsSchema
         */
        getOptionsForSchema: function() {
            return Alpaca.merge(this.base(), {
                "fields": {
                    "multipleOf": {
                        "title": "Multiple Of",
                        "description": "The value must be a integral multiple of the property",
                        "type": "number"
                    },
                    "minimum": {
                        "title": "Minimum",
                        "description": "Minimum value of the property",
                        "type": "number"
                    },
                    "maximum": {
                        "title": "Maximum",
                        "description": "Maximum value of the property",
                        "type": "number"
                    },
                    "exclusiveMinimum": {
                        "rightLabel": "Exclusive minimum ?",
                        "helper": "Field value must be greater than but not equal to this number if checked",
                        "type": "checkbox"
                    },
                    "exclusiveMaximum": {
                        "rightLabel": "Exclusive Maximum ?",
                        "helper": "Field value must be less than but not equal to this number if checked",
                        "type": "checkbox"
                    }
                }
            });
        },

        /**
         * @see Alpaca.Fields.TextField#getTitle
         */
        getTitle: function() {
            return "Number Field";
        },

        /**
         * @see Alpaca.Fields.TextField#getDescription
         */
        getDescription: function() {
            return "Field for float numbers.";
        }

        /* end_builder_helpers */
    });

    // Additional Registrations
    Alpaca.registerMessages({
        "stringValueTooSmall": "The minimum value for this field is {0}",
        "stringValueTooLarge": "The maximum value for this field is {0}",
        "stringValueTooSmallExclusive": "Value of this field must be greater than {0}",
        "stringValueTooLargeExclusive": "Value of this field must be less than {0}",
        "stringDivisibleBy": "The value must be divisible by {0}",
        "stringNotANumber": "This value is not a number.",
        "stringValueNotMultipleOf": "This value is not a multiple of {0}"
    });
    Alpaca.registerFieldClass("number", Alpaca.Fields.NumberField);
    Alpaca.registerDefaultSchemaFieldMapping("number", "number");

})(jQuery);

(function($) {

    var Alpaca = $.alpaca;
    
    Alpaca.Fields.Role2Field = Alpaca.Fields.ListField.extend(
    /**
     * @lends Alpaca.Fields.Role2Field.prototype
     */
    {
        constructor: function (container, data, options, schema, view, connector) {
            var self = this;
            this.base(container, data, options, schema, view, connector);
            this.sf = connector.servicesFramework;
            //this.culture = connector.culture;
            this.dataSource = {};
        },
        /**
         * @see Alpaca.Role2Field#getFieldType
         */
        getFieldType: function()
        {
            return "select";
        },

        /**
         * @see Alpaca.Fields.Role2Field#setup
         */
        setup: function()
        {
            var self = this;
            if (self.schema["type"] && self.schema["type"] === "array") {
                self.options.multiple = true;
                self.options.removeDefaultNone = true;
                //self.options.hideNone = true;
            }
            this.base();
        },

        getValue: function () {
            if (this.control && this.control.length > 0) {
                var val = this._getControlVal(true);
                if (typeof (val) === "undefined") {
                    val = this.data;
                }
                else if (Alpaca.isArray(val)) {
                    for (var i = 0; i < val.length; i++) {
                        val[i] = this.ensureProperType(val[i]);
                    }
                }
                return this.base(val);
            }
        },

        /**
         * @see Alpaca.Field#setValue
         */
        setValue: function(val)
        {
            if (Alpaca.isArray(val))
            {
                if (!Alpaca.compareArrayContent(val, this.getValue()))
                {
                    if (!Alpaca.isEmpty(val) && this.control)
                    {
                        this.control.val(val);
                    }
                    this.base(val);
                }
            }
            else
            {
                if (val !== this.getValue())
                {
                    /*
                    if (!Alpaca.isEmpty(val) && this.control)
                    {
                        this.control.val(val);
                    }
                    */
                    if (this.control && typeof(val) != "undefined" && val != null)
                    {
                        this.control.val(val);
                    }
                    this.base(val);
                }
            }
        },

        /**
         * @see Alpaca.Role2Field#getEnum
         */
        getEnum: function()
        {
            if (this.schema)
            {
                if (this.schema["enum"])
                {
                    return this.schema["enum"];
                }
                else if (this.schema["type"] && this.schema["type"] === "array" && this.schema["items"] && this.schema["items"]["enum"])
                {
                    return this.schema["items"]["enum"];
                }
            }
        },

        initControlEvents: function()
        {
            var self = this;

            self.base();

            if (self.options.multiple)
            {
                var button = this.control.parent().find(".select2-search__field");

                button.focus(function(e) {
                    if (!self.suspendBlurFocus)
                    {
                        self.onFocus.call(self, e);
                        self.trigger("focus", e);
                    }
                });

                button.blur(function(e) {
                    if (!self.suspendBlurFocus)
                    {
                        self.onBlur.call(self, e);
                        self.trigger("blur", e);
                    }
                });
                this.control.on("change", function (e) {
                    self.onChange.call(self, e);
                    self.trigger("change", e);

                });
            }
        },

        beforeRenderControl: function(model, callback)
        {
            var self = this;
            this.base(model, function() {
                self.selectOptions = [];

                if (self.sf) {

                    var completionFunction = function () {
                        self.schema.enum = [];
                        self.options.optionLabels = [];
                        for (var i = 0; i < self.selectOptions.length; i++) {
                            self.schema.enum.push(self.selectOptions[i].value);
                            self.options.optionLabels.push(self.selectOptions[i].text);
                        }
                        // push back to model
                        model.selectOptions = self.selectOptions;
                        callback();
                    };
                    var postData = { q: "*", l: self.culture };
                    $.ajax({
                        url: self.sf.getServiceRoot("OpenContent") + "DnnEntitiesAPI" + "/" + "RoleLookup",
                        beforeSend: self.sf.setModuleHeaders,
                        type: "get",
                        dataType: "json",
                        //contentType: "application/json; charset=utf-8",
                        data: postData,
                        success: function (jsonDocument) {
                            var ds = jsonDocument;

                            if (self.options.dsTransformer && Alpaca.isFunction(self.options.dsTransformer)) {
                                ds = self.options.dsTransformer(ds);
                            }
                            if (ds) {
                                if (Alpaca.isObject(ds)) {
                                    // for objects, we walk through one key at a time
                                    // the insertion order is the order of the keys from the map
                                    // to preserve order, consider using an array as below
                                    $.each(ds, function (key, value) {
                                        self.selectOptions.push({
                                            "value": key,
                                            "text": value
                                        });
                                    });
                                    completionFunction();
                                }
                                else if (Alpaca.isArray(ds)) {
                                    // for arrays, we walk through one index at a time
                                    // the insertion order is dictated by the order of the indices into the array
                                    // this preserves order
                                    $.each(ds, function (index, value) {
                                        self.selectOptions.push({
                                            "value": value.value,
                                            "text": value.text
                                        });
                                        self.dataSource[value.value] = value;
                                    });
                                    completionFunction();
                                }
                            }
                        },
                        "error": function (jqXHR, textStatus, errorThrown) {

                            self.errorCallback({
                                "message": "Unable to load data from uri : " + self.options.dataSource,
                                "stage": "DATASOURCE_LOADING_ERROR",
                                "details": {
                                    "jqXHR": jqXHR,
                                    "textStatus": textStatus,
                                    "errorThrown": errorThrown
                                }
                            });
                        }
                    });
                } else {
                    callback();
                }
            });
        },

        prepareControlModel: function(callback)
        {
            var self = this;
            this.base(function(model) {
                model.selectOptions = self.selectOptions;
                callback(model);
            });
        },

        afterRenderControl: function(model, callback)
        {
            var self = this;

            this.base(model, function() {

                // if emptySelectFirst and nothing currently checked, then pick first item in the value list
                // set data and visually select it
                if (Alpaca.isUndefined(self.data) && self.options.emptySelectFirst && self.selectOptions && self.selectOptions.length > 0)
                {
                    self.data = self.selectOptions[0].value;
                }

                // do this little trick so that if we have a default value, it gets set during first render
                // this causes the state of the control
                if (self.data)
                {
                    self.setValue(self.data);
                }

                // if we are in multiple mode and the bootstrap multiselect plugin is available, bind it in
                //if (self.options.multiple && $.fn.multiselect)
                if ($.fn.select2)
                {
                    var settings = null;
                    if (self.options.select2) {
                        settings = self.options.select2;
                    }
                    else
                    {
                        settings = {};
                    }
                    /*
                    if (!settings.nonSelectedText)
                    {
                        settings.nonSelectedText = "None";
                        if (self.options.noneLabel)
                        {
                            settings.nonSelectedText = self.options.noneLabel;
                        }
                    }
                    if (self.options.hideNone)
                    {
                        delete settings.nonSelectedText;
                    }
                    */

                    settings.templateResult = function (state) {
                        if (!state.id) { return state.text; }
                        
                        var $state = $(
                          '<span>' + state.text + '</span>'
                        );
                        return $state;
                    };

                    settings.templateSelection = function (state) {
                        if (!state.id) { return state.text; }
                        
                        var $state = $(
                          '<span>' + state.text + '</span>'
                        );
                        return $state;
                    };

                    $(self.getControlEl()).select2(settings);
                }

                callback();

            });
        },

        /**
         * Validate against enum property.
         *
         * @returns {Boolean} True if the element value is part of the enum list, false otherwise.
         */
        _validateEnum: function()
        {
            var _this = this;

            if (this.schema["enum"])
            {
                var val = this.data;

                if (!this.isRequired() && Alpaca.isValEmpty(val))
                {
                    return true;
                }

                if (this.options.multiple)
                {
                    var isValid = true;

                    if (!val)
                    {
                        val = [];
                    }

                    if (!Alpaca.isArray(val) && !Alpaca.isObject(val))
                    {
                        val = [val];
                    }

                    $.each(val, function(i,v) {

                        if ($.inArray(v, _this.schema["enum"]) <= -1)
                        {
                            isValid = false;
                            return false;
                        }

                    });

                    return isValid;
                }
                else
                {
                    return ($.inArray(val, this.schema["enum"]) > -1);
                }
            }
            else
            {
                return true;
            }
        },

        /**
         * @see Alpaca.Field#onChange
         */
        onChange: function(e)
        {
            this.base(e);

            var _this = this;

            Alpaca.later(25, this, function() {
                var v = _this.getValue();
                _this.setValue(v);
                _this.refreshValidationState();
            });
        },

        /**
         * Validates if number of items has been less than minItems.
         * @returns {Boolean} true if number of items has been less than minItems
         */
        _validateMinItems: function()
        {
            if (this.schema.items && this.schema.items.minItems)
            {
                if ($(":selected",this.control).length < this.schema.items.minItems)
                {
                    return false;
                }
            }

            return true;
        },

        /**
         * Validates if number of items has been over maxItems.
         * @returns {Boolean} true if number of items has been over maxItems
         */
        _validateMaxItems: function()
        {
            if (this.schema.items && this.schema.items.maxItems)
            {
                if ($(":selected",this.control).length > this.schema.items.maxItems)
                {
                    return false;
                }
            }

            return true;
        },

        /**
         * @see Alpaca.ContainerField#handleValidate
         */
        handleValidate: function()
        {
            var baseStatus = this.base();

            var valInfo = this.validation;

            var status = this._validateMaxItems();
            valInfo["tooManyItems"] = {
                "message": status ? "" : Alpaca.substituteTokens(this.getMessage("tooManyItems"), [this.schema.items.maxItems]),
                "status": status
            };

            status = this._validateMinItems();
            valInfo["notEnoughItems"] = {
                "message": status ? "" : Alpaca.substituteTokens(this.getMessage("notEnoughItems"), [this.schema.items.minItems]),
                "status": status
            };

            return baseStatus && valInfo["tooManyItems"]["status"] && valInfo["notEnoughItems"]["status"];
        },

        /**
         * @see Alpaca.Field#focus
         */
        focus: function(onFocusCallback)
        {
            if (this.control && this.control.length > 0)
            {
                // set focus onto the select
                var el = $(this.control).get(0);

                el.focus();

                if (onFocusCallback)
                {
                    onFocusCallback(this);
                }
            }
        }

        /* builder_helpers */
        ,

        /**
         * @see Alpaca.Field#getTitle
         */
        getTitle: function() {
            return "Select File Field";
        },

        /**
         * @see Alpaca.Field#getDescription
         */
        getDescription: function() {
            return "Select File Field";
        },

        /**
         * @private
         * @see Alpaca.Fields.Role2Field#getSchemaOfOptions
         */
        getSchemaOfOptions: function() {
            return Alpaca.merge(this.base(), {
                "properties": {
                    "multiple": {
                        "title": "Mulitple Selection",
                        "description": "Allow multiple selection if true.",
                        "type": "boolean",
                        "default": false
                    },
                    "size": {
                        "title": "Displayed Options",
                        "description": "Number of options to be shown.",
                        "type": "number"
                    },
                    "emptySelectFirst": {
                        "title": "Empty Select First",
                        "description": "If the data is empty, then automatically select the first item in the list.",
                        "type": "boolean",
                        "default": false
                    },
                    "multiselect": {
                        "title": "Multiselect Plugin Settings",
                        "description": "Multiselect plugin properties - http://davidstutz.github.io/bootstrap-multiselect",
                        "type": "any"
                    }
                }
            });
        },

        /**
         * @private
         * @see Alpaca.Fields.Role2Field#getOptionsForOptions
         */
        getOptionsForOptions: function() {
            return Alpaca.merge(this.base(), {
                "fields": {
                    "multiple": {
                        "rightLabel": "Allow multiple selection ?",
                        "helper": "Allow multiple selection if checked",
                        "type": "checkbox"
                    },
                    "size": {
                        "type": "integer"
                    },
                    "emptySelectFirst": {
                        "type": "checkbox",
                        "rightLabel": "Empty Select First"
                    },
                    "multiselect": {
                        "type": "object",
                        "rightLabel": "Multiselect plugin properties - http://davidstutz.github.io/bootstrap-multiselect"
                    }
                }
            });
        }

        /* end_builder_helpers */

    });

    Alpaca.registerFieldClass("role2", Alpaca.Fields.Role2Field);

})(jQuery);
(function($) {

    var Alpaca = $.alpaca;
    
    Alpaca.Fields.User2Field = Alpaca.Fields.ListField.extend(
    /**
     * @lends Alpaca.Fields.User2Field.prototype
     */
    {
        constructor: function (container, data, options, schema, view, connector) {
            var self = this;
            this.base(container, data, options, schema, view, connector);
            this.sf = connector.servicesFramework;
            this.dataSource = {};
        },
        /**
         * @see Alpaca.Field#getFieldType
         */
        getFieldType: function()
        {
            return "select";
        },

        /**
         * @see Alpaca.Fields.User2Field#setup
         */
        setup: function()
        {
            var self = this;
            if (self.schema["type"] && self.schema["type"] === "array") {
                self.options.multiple = true;
                self.options.removeDefaultNone = true;
                //self.options.hideNone = true;
            }
            if (!this.options.folder) {
                this.options.folder = "";
            }
       
            if (!this.options.role) {
                this.options.role = "";
            }
            var self = this;
            if (this.options.lazyLoading) {
                var pageSize = 10;
                this.options.select2 = {
                    ajax: {
                        url: this.sf.getServiceRoot("OpenContent") + "DnnEntitiesAPI" + "/" + "UsersLookup",
                        beforeSend: this.sf.setModuleHeaders,
                        type: "get",
                        dataType: 'json',
                        delay: 250,
                        data: function (params) {
                            return {
                                q: params.term ? params.term : "", // search term
                                d: self.options.folder, 
                                role: self.options.role,
                                pageIndex: params.page ? params.page : 1,
                                pageSize: pageSize
                            };
                        },
                        processResults: function (data, params) {
                            params.page = params.page || 1;
                            if (params.page == 1) {
                                data.items.unshift({
                                    id: "",
                                    text: self.options.noneLabel
                                })
                            }
                            return {
                                results: data.items,
                                pagination: {
                                    more: (params.page * pageSize) < data.total
                                }
                            };
                        },
                        cache: true
                    },
                    escapeMarkup: function (markup) { return markup; },
                    minimumInputLength: 0
                }
            };
            this.base();
        },

        getValue: function () {
            if (this.control && this.control.length > 0) {
                var val = this._getControlVal(true);
                if (typeof (val) === "undefined") {
                    val = this.data;
                }
                else if (Alpaca.isArray(val)) {
                    for (var i = 0; i < val.length; i++) {
                        val[i] = this.ensureProperType(val[i]);
                    }
                }
                return this.base(val);
            }
        },

        /**
         * @see Alpaca.Field#setValue
         */
        setValue: function (val) {
            if (Alpaca.isArray(val)) {
                if (!Alpaca.compareArrayContent(val, this.getValue())) {
                    if (!Alpaca.isEmpty(val) && this.control) {
                        this.control.val(val);
                    }
                    this.base(val);
                }
            }
            else {
                if (val !== this.getValue()) {
                    /*
                    if (!Alpaca.isEmpty(val) && this.control)
                    {
                        this.control.val(val);
                    }
                    */
                    if (this.control && typeof (val) != "undefined" && val != null) {
                        this.control.val(val);
                    }
                    this.base(val);
                }
            }
        },

        /**
         * @see Alpaca.User2Field#getEnum
         */
        getEnum: function()
        {
            if (this.schema)
            {
                if (this.schema["enum"])
                {
                    return this.schema["enum"];
                }
                else if (this.schema["type"] && this.schema["type"] === "array" && this.schema["items"] && this.schema["items"]["enum"])
                {
                    return this.schema["items"]["enum"];
                }
            }
        },
        /*
        initControlEvents: function()
        {
            var self = this;

            self.base();

            if (self.options.multiple)
            {
                var button = this.control.parent().find(".select2-search__field");

                button.focus(function(e) {
                    if (!self.suspendBlurFocus)
                    {
                        self.onFocus.call(self, e);
                        self.trigger("focus", e);
                    }
                });

                button.blur(function(e) {
                    if (!self.suspendBlurFocus)
                    {
                        self.onBlur.call(self, e);
                        self.trigger("blur", e);
                    }
                });
                this.control.on("change", function (e) {
                    self.onChange.call(self, e);
                    self.trigger("change", e);

                });
            }
        },
        */
        beforeRenderControl: function(model, callback)
        {
            var self = this;

            this.base(model, function() {
                self.selectOptions = [];
                if (self.sf) {
                    var completionFunction = function () {
                        self.schema.enum = [];
                        self.options.optionLabels = [];
                        for (var i = 0; i < self.selectOptions.length; i++) {
                            self.schema.enum.push(self.selectOptions[i].value);
                            self.options.optionLabels.push(self.selectOptions[i].text);
                        }
                        // push back to model
                        model.selectOptions = self.selectOptions;
                        callback();
                    };
                    if (self.options.lazyLoading) {
                        if (self.data) {
                            self.getUserName(self.data, function (data) {
                                self.selectOptions.push({
                                    "value": self.data,
                                    "text": data.text
                                });
                                self.dataSource[self.data] = data.text;
                                completionFunction();
                            });
                        } else {
                            completionFunction();
                        }
                    }
                    else {
                        var postData = { q: "", role: self.options.role };
                        $.ajax({
                            url: self.sf.getServiceRoot("OpenContent") + "DnnEntitiesAPI" + "/" + "UsersLookup",
                            beforeSend: self.sf.setModuleHeaders,
                            type: "get",
                            dataType: "json",
                            //contentType: "application/json; charset=utf-8",
                            data: postData,
                            success: function (jsonDocument) {
                                var ds = jsonDocument;
                                if (self.options.dsTransformer && Alpaca.isFunction(self.options.dsTransformer)) {
                                    ds = self.options.dsTransformer(ds);
                                }
                                if (ds) {
                                    if (Alpaca.isObject(ds)) {
                                        // for objects, we walk through one key at a time
                                        // the insertion order is the order of the keys from the map
                                        // to preserve order, consider using an array as below
                                        $.each(ds, function (key, value) {
                                            self.selectOptions.push({
                                                "value": key,
                                                "text": value
                                            });
                                        });
                                        completionFunction();
                                    }
                                    else if (Alpaca.isArray(ds)) {
                                        // for arrays, we walk through one index at a time
                                        // the insertion order is dictated by the order of the indices into the array
                                        // this preserves order
                                        $.each(ds, function (index, value) {
                                            self.selectOptions.push({
                                                "value": value.value,
                                                "text": value.text
                                            });
                                            self.dataSource[value.value] = value;
                                        });
                                        completionFunction();
                                    }
                                }
                            },
                            "error": function (jqXHR, textStatus, errorThrown) {

                                self.errorCallback({
                                    "message": "Unable to load data from uri : " + self.options.dataSource,
                                    "stage": "DATASOURCE_LOADING_ERROR",
                                    "details": {
                                        "jqXHR": jqXHR,
                                        "textStatus": textStatus,
                                        "errorThrown": errorThrown
                                    }
                                });
                            }
                        });
                    }
                }
                else {
                    callback();
                }
            });
        },

        prepareControlModel: function(callback)
        {
            var self = this;
            this.base(function(model) {
                model.selectOptions = self.selectOptions;
                callback(model);
            });
        },

        afterRenderControl: function(model, callback)
        {
            var self = this;
            this.base(model, function() {
                // if emptySelectFirst and nothing currently checked, then pick first item in the value list
                // set data and visually select it
                if (Alpaca.isUndefined(self.data) && self.options.emptySelectFirst && self.selectOptions && self.selectOptions.length > 0)
                {
                    self.data = self.selectOptions[0].value;
                }

                // do this little trick so that if we have a default value, it gets set during first render
                // this causes the state of the control
                if (self.data)
                {
                    self.setValue(self.data);
                }

                // if we are in multiple mode and the bootstrap multiselect plugin is available, bind it in
                //if (self.options.multiple && $.fn.multiselect)
                if ($.fn.select2) {
                    var settings = null;
                    if (self.options.select2) {
                        settings = self.options.select2;
                    }
                    else {
                        settings = {};
                    }
                    /*
                    if (!settings.nonSelectedText)
                    {
                        settings.nonSelectedText = "None";
                        if (self.options.noneLabel)
                        {
                            settings.nonSelectedText = self.options.noneLabel;
                        }
                    }
                    if (self.options.hideNone)
                    {
                        delete settings.nonSelectedText;
                    }
                    */
                    
                    settings.templateResult = function (state) {

                        if (state.loading) return state.text;

                        //if (!state.id) { return state.text; }

                        var $state = $(
                            '<span>' + state.text + '</span>'
                        );
                        return $state;
                    };

                    settings.templateSelection = function (state) {
                        if (!state.id) { return state.text; }

                        var $state = $(
                            '<span>' + state.text + '</span>'
                        );
                        return $state;
                    };
                    
                    $(self.getControlEl()).select2(settings);
                }

                
                callback();
            });
        },

        getUserName: function (userid, callback) {
            var self = this;
            if (self.sf){
                var postData = { userid: userid };
                $.ajax({
                    url: self.sf.getServiceRoot("OpenContent") + "DnnEntitiesAPI" + "/" + "GetUserInfo",
                    beforeSend: self.sf.setModuleHeaders,
                    type: "get",
                    asych : false,
                    dataType: "json",
                    //contentType: "application/json; charset=utf-8",
                    data: postData,
                    success: function (data) {
                        if (callback) callback(data);
                    },
                    error: function (jqXHR, textStatus, errorThrown) {
                        alert("Error GetUserInfo " + userid);
                    }
                });
            }
        },

        /**
         * Validate against enum property.
         *
         * @returns {Boolean} True if the element value is part of the enum list, false otherwise.
         */
        _validateEnum: function()
        {
            var _this = this;

            if (this.schema["enum"])
            {
                var val = this.data;

                if (!this.isRequired() && Alpaca.isValEmpty(val))
                {
                    return true;
                }

                if (this.options.multiple)
                {
                    var isValid = true;

                    if (!val)
                    {
                        val = [];
                    }

                    if (!Alpaca.isArray(val) && !Alpaca.isObject(val))
                    {
                        val = [val];
                    }

                    $.each(val, function(i,v) {
                        /*
                        if ($.inArray(v, _this.schema["enum"]) <= -1)
                        {
                            isValid = false;
                            return false;
                        }
                        */
                    });

                    return isValid;
                }
                else
                {
                    //return ($.inArray(val, this.schema["enum"]) > -1);
                    return true;
                }
            }
            else
            {
                return true;
            }
        },

        /**
         * @see Alpaca.Field#onChange
         */
        onChange: function(e)
        {
            this.base(e);

            var _this = this;

            Alpaca.later(25, this, function() {
                var v = _this.getValue();
                _this.setValue(v);
                _this.refreshValidationState();
            });
        },

       
        /**
         * @see Alpaca.Field#focus
         */
        focus: function(onFocusCallback)
        {
            if (this.control && this.control.length > 0)
            {
                // set focus onto the select
                var el = $(this.control).get(0);

                el.focus();

                if (onFocusCallback)
                {
                    onFocusCallback(this);
                }
            }
        },
        getTextControlEl: function () {
            var self = this;
            return $(self.getControlEl()).find('input[type=text]');
        },
        
        /**
         * @see Alpaca.Field#getTitle
         */
        getTitle: function() {
            return "Select User Field";
        },

        /**
         * @see Alpaca.Field#getDescription
         */
        getDescription: function() {
            return "Select User Field";
        },

        /**
         * @private
         * @see Alpaca.Fields.User2Field#getSchemaOfOptions
         */
        getSchemaOfOptions: function() {
            return Alpaca.merge(this.base(), {
                "properties": {
                    "multiple": {
                        "title": "Mulitple Selection",
                        "description": "Allow multiple selection if true.",
                        "type": "boolean",
                        "default": false
                    },
                    "size": {
                        "title": "Displayed Options",
                        "description": "Number of options to be shown.",
                        "type": "number"
                    },
                    "emptySelectFirst": {
                        "title": "Empty Select First",
                        "description": "If the data is empty, then automatically select the first item in the list.",
                        "type": "boolean",
                        "default": false
                    },
                    "multiselect": {
                        "title": "Multiselect Plugin Settings",
                        "description": "Multiselect plugin properties - http://davidstutz.github.io/bootstrap-multiselect",
                        "type": "any"
                    }
                }
            });
        },

        /**
         * @private
         * @see Alpaca.Fields.User2Field#getOptionsForOptions
         */
        getOptionsForOptions: function() {
            return Alpaca.merge(this.base(), {
                "fields": {
                    "multiple": {
                        "rightLabel": "Allow multiple selection ?",
                        "helper": "Allow multiple selection if checked",
                        "type": "checkbox"
                    },
                    "size": {
                        "type": "integer"
                    },
                    "emptySelectFirst": {
                        "type": "checkbox",
                        "rightLabel": "Empty Select First"
                    },
                    "multiselect": {
                        "type": "object",
                        "rightLabel": "Multiselect plugin properties - http://davidstutz.github.io/bootstrap-multiselect"
                    }
                }
            });
        }

        /* end_builder_helpers */

    });

    Alpaca.registerFieldClass("user2", Alpaca.Fields.User2Field);

})(jQuery);
(function($) {

    var Alpaca = $.alpaca;

    
    Alpaca.Fields.Select2Field = Alpaca.Fields.ListField.extend(
    /**
     * @lends Alpaca.Fields.SelectField.prototype
     */
    {
        constructor: function (container, data, options, schema, view, connector) {
            var self = this;
            this.base(container, data, options, schema, view, connector);
            this.sf = connector.servicesFramework;
        },
        /**
         * @see Alpaca.Field#getFieldType
         */
        getFieldType: function()
        {
            return "select";
        },

        /**
         * @see Alpaca.Fields.ListField#setup
         */
        setup: function()
        {
            var self = this;
            if (self.schema["type"] && self.schema["type"] === "array") {
                self.options.multiple = true;
                self.options.removeDefaultNone = true;
                //self.options.hideNone = true;
            }
            if (self.schema.required) {
                self.options.hideNone = false;
            }
            this.base();
        },

        getValue: function () {
            if (this.control && this.control.length > 0) {
                var val = this._getControlVal(true);
                if (typeof (val) === "undefined") {
                    val = this.data;
                }
                else if (Alpaca.isArray(val)) {
                    for (var i = 0; i < val.length; i++) {
                        val[i] = this.ensureProperType(val[i]);
                    }
                }

                return this.base(val);
            }
        },

        /**
         * @see Alpaca.Field#setValue
         */
        setValue: function(val)
        {
            if (Alpaca.isArray(val))
            {
                if (!Alpaca.compareArrayContent(val, this.getValue()))
                {
                    if (!Alpaca.isEmpty(val) && this.control)
                    {
                        this.control.val(val);
                    }

                    this.base(val);
                }
            }
            else
            {
                if (val !== this.getValue())
                {
                    /*
                    if (!Alpaca.isEmpty(val) && this.control)
                    {
                        this.control.val(val);
                    }
                    */
                    if (this.control && typeof(val) != "undefined" && val != null)
                    {
                        this.control.val(val);
                    }

                    this.base(val);
                }
            }
        },

        /**
         * @see Alpaca.ListField#getEnum
         */
        getEnum: function()
        {
            if (this.schema)
            {
                if (this.schema["enum"])
                {
                    return this.schema["enum"];
                }
                else if (this.schema["type"] && this.schema["type"] === "array" && this.schema["items"] && this.schema["items"]["enum"])
                {
                    return this.schema["items"]["enum"];
                }
            }
        },

        initControlEvents: function()
        {
            var self = this;

            self.base();

            if (self.options.multiple)
            {
                var button = this.control.parent().find(".select2-search__field");

                button.focus(function(e) {
                    if (!self.suspendBlurFocus)
                    {
                        self.onFocus.call(self, e);
                        self.trigger("focus", e);
                    }
                });

                button.blur(function(e) {
                    if (!self.suspendBlurFocus)
                    {
                        self.onBlur.call(self, e);
                        self.trigger("blur", e);
                    }
                });

                this.control.on("change", function (e) {
                    self.onChange.call(self, e);
                    self.trigger("change", e);

                });
            }
        },

        beforeRenderControl: function(model, callback)
        {
            var self = this;
            this.base(model, function() {
                if (self.options.dataService && self.options.dataService && self.sf) {
                    self.selectOptions = [];
                    var completionFunction = function () {
                        self.schema.enum = [];
                        self.options.optionLabels = [];
                        for (var i = 0; i < self.selectOptions.length; i++) {
                            self.schema.enum.push(self.selectOptions[i].value);
                            self.options.optionLabels.push(self.selectOptions[i].text);
                        }
                        // push back to model
                        model.selectOptions = self.selectOptions;
                        callback();
                    };
                    var postData = "{}";
                    if (self.options.dataService.data){
                        postData = self.options.dataService.data; //JSON.stringify(self.options.dataService.data);
                    }
                    if (!self.options.dataService.module) {
                        self.options.dataService.module = "OpenContent"
                    }
                    if (!self.options.dataService.controller) {
                        self.options.dataService.controller = "OpenContentAPI"
                    }
                    if (!self.options.dataService.action) {
                        self.options.dataService.action = "Lookup"
                    }
                    $.ajax({
                        url: self.sf.getServiceRoot(self.options.dataService.module) + self.options.dataService.controller + "/" + self.options.dataService.action,                        
                        beforeSend: self.sf.setModuleHeaders,
                        type: "post",
                        dataType: "json",
                        //contentType: "application/json; charset=utf-8",
                        data: postData,
                        success: function (jsonDocument) {
                            
                            var ds = jsonDocument;
                            if (self.options.dsTransformer && Alpaca.isFunction(self.options.dsTransformer)) {
                                ds = self.options.dsTransformer(ds);
                            }

                            if (ds) {
                                if (Alpaca.isObject(ds)) {
                                    // for objects, we walk through one key at a time
                                    // the insertion order is the order of the keys from the map
                                    // to preserve order, consider using an array as below
                                    $.each(ds, function (key, value) {
                                        self.selectOptions.push({
                                            "value": key,
                                            "text": value
                                        });
                                    });
                                    completionFunction();
                                }
                                else if (Alpaca.isArray(ds)) {
                                    // for arrays, we walk through one index at a time
                                    // the insertion order is dictated by the order of the indices into the array
                                    // this preserves order
                                    $.each(ds, function (index, value) {
                                        self.selectOptions.push({
                                            "value": value.value,
                                            "text": value.text
                                        });
                                    });
                                    completionFunction();
                                }
                            }
                        },
                        "error": function (jqXHR, textStatus, errorThrown) {
                            self.errorCallback({
                                "message": "Unable to load data from uri : " + self.options.dataSource,
                                "stage": "DATASOURCE_LOADING_ERROR",
                                "details": {
                                    "jqXHR": jqXHR,
                                    "textStatus": textStatus,
                                    "errorThrown": errorThrown
                                }
                            });
                        }
                    });
                }
                else {
                    callback();
                }
            });
        },

        prepareControlModel: function(callback)
        {
            var self = this;
            this.base(function(model) {
                model.selectOptions = self.selectOptions;
                callback(model);
            });
        },

        afterRenderControl: function(model, callback)
        {
            var self = this;

            this.base(model, function() {

                // if emptySelectFirst and nothing currently checked, then pick first item in the value list
                // set data and visually select it
                if (Alpaca.isUndefined(self.data) && self.options.emptySelectFirst && self.selectOptions && self.selectOptions.length > 0)
                {
                    self.data = self.selectOptions[0].value;
                }

                // do this little trick so that if we have a default value, it gets set during first render
                // this causes the state of the control
                if (self.data)
                {
                    self.setValue(self.data);
                }

                // if we are in multiple mode and the bootstrap multiselect plugin is available, bind it in
                //if (self.options.multiple && $.fn.multiselect)
                if ($.fn.select2)
                {
                    var settings = null;
                    if (self.options.select2) {
                        settings = self.options.select2;
                    }
                    else
                    {
                        settings = {};
                    }
                    /*
                    if (!settings.nonSelectedText)
                    {
                        settings.nonSelectedText = "None";
                        if (self.options.noneLabel)
                        {
                            settings.nonSelectedText = self.options.noneLabel;
                        }
                    }
                    if (self.options.hideNone)
                    {
                        delete settings.nonSelectedText;
                    }
                    */
                    $(self.getControlEl()).select2(settings);

                    /*
                    if (self.options.manageurl) {
                        var manageButton = $('<a href="' + self.options.manageurl + '" target="_blank" class="alpaca-form-button">Manage files</a>').appendTo($(el).parent());
                    }
                    */
                }
                callback();
            });
        },

        /**
         * Validate against enum property.
         *
         * @returns {Boolean} True if the element value is part of the enum list, false otherwise.
         */
        _validateEnum: function()
        {
            var _this = this;
            if (this.schema["enum"])
            {
                var val = this.data;
                if (!this.isRequired() && Alpaca.isValEmpty(val))
                {
                    return true;
                }
                if (this.options.multiple)
                {
                    var isValid = true;
                    if (!val)
                    {
                        val = [];
                    }

                    if (!Alpaca.isArray(val) && !Alpaca.isObject(val))
                    {
                        val = [val];
                    }
                    $.each(val, function(i,v) {
                        if ($.inArray(v, _this.schema["enum"]) <= -1)
                        {
                            isValid = false;
                            return false;
                        }
                    });
                    return isValid;
                }
                else
                {
                    return ($.inArray(val, this.schema["enum"]) > -1);
                }
            }
            else
            {
                return true;
            }
        },

        /**
         * @see Alpaca.Field#onChange
         */
        onChange: function(e)
        {
            this.base(e);

            var _this = this;

            Alpaca.later(25, this, function() {
                var v = _this.getValue();
                _this.setValue(v);
                _this.refreshValidationState();
            });
        },

        /**
         * Validates if number of items has been less than minItems.
         * @returns {Boolean} true if number of items has been less than minItems
         */
        _validateMinItems: function()
        {
            if (this.schema.items && this.schema.items.minItems)
            {
                if ($(":selected",this.control).length < this.schema.items.minItems)
                {
                    return false;
                }
            }

            return true;
        },

        /**
         * Validates if number of items has been over maxItems.
         * @returns {Boolean} true if number of items has been over maxItems
         */
        _validateMaxItems: function()
        {
            if (this.schema.items && this.schema.items.maxItems)
            {
                if ($(":selected",this.control).length > this.schema.items.maxItems)
                {
                    return false;
                }
            }

            return true;
        },

        /**
         * @see Alpaca.ContainerField#handleValidate
         */
        handleValidate: function()
        {
            var baseStatus = this.base();

            var valInfo = this.validation;

            var status = this._validateMaxItems();
            valInfo["tooManyItems"] = {
                "message": status ? "" : Alpaca.substituteTokens(this.getMessage("tooManyItems"), [this.schema.items.maxItems]),
                "status": status
            };

            status = this._validateMinItems();
            valInfo["notEnoughItems"] = {
                "message": status ? "" : Alpaca.substituteTokens(this.getMessage("notEnoughItems"), [this.schema.items.minItems]),
                "status": status
            };

            return baseStatus && valInfo["tooManyItems"]["status"] && valInfo["notEnoughItems"]["status"];
        },

        /**
         * @see Alpaca.Field#focus
         */
        focus: function(onFocusCallback)
        {
            if (this.control && this.control.length > 0)
            {
                // set focus onto the select
                var el = $(this.control).get(0);

                el.focus();

                if (onFocusCallback)
                {
                    onFocusCallback(this);
                }
            }
        }

        /* builder_helpers */
        ,

        /**
         * @see Alpaca.Field#getTitle
         */
        getTitle: function() {
            return "Select Field";
        },

        /**
         * @see Alpaca.Field#getDescription
         */
        getDescription: function() {
            return "Select Field";
        },

        /**
         * @private
         * @see Alpaca.Fields.ListField#getSchemaOfOptions
         */
        getSchemaOfOptions: function() {
            return Alpaca.merge(this.base(), {
                "properties": {
                    "multiple": {
                        "title": "Mulitple Selection",
                        "description": "Allow multiple selection if true.",
                        "type": "boolean",
                        "default": false
                    },
                    "size": {
                        "title": "Displayed Options",
                        "description": "Number of options to be shown.",
                        "type": "number"
                    },
                    "emptySelectFirst": {
                        "title": "Empty Select First",
                        "description": "If the data is empty, then automatically select the first item in the list.",
                        "type": "boolean",
                        "default": false
                    },
                    "multiselect": {
                        "title": "Multiselect Plugin Settings",
                        "description": "Multiselect plugin properties - http://davidstutz.github.io/bootstrap-multiselect",
                        "type": "any"
                    }
                }
            });
        },

        /**
         * @private
         * @see Alpaca.Fields.ListField#getOptionsForOptions
         */
        getOptionsForOptions: function() {
            return Alpaca.merge(this.base(), {
                "fields": {
                    "multiple": {
                        "rightLabel": "Allow multiple selection ?",
                        "helper": "Allow multiple selection if checked",
                        "type": "checkbox"
                    },
                    "size": {
                        "type": "integer"
                    },
                    "emptySelectFirst": {
                        "type": "checkbox",
                        "rightLabel": "Empty Select First"
                    },
                    "multiselect": {
                        "type": "object",
                        "rightLabel": "Multiselect plugin properties - http://davidstutz.github.io/bootstrap-multiselect"
                    }
                }
            });
        }

        /* end_builder_helpers */

    });

    Alpaca.registerFieldClass("select2", Alpaca.Fields.Select2Field);

})(jQuery);
(function ($) {

    var Alpaca = $.alpaca;

    $.alpaca.Fields.DnnUrlField = $.alpaca.Fields.TextField.extend({

        constructor: function (container, data, options, schema, view, connector) {
            var self = this;
            this.base(container, data, options, schema, view, connector);
            this.culture = connector.culture;
            this.sf = connector.servicesFramework;
        },


        setup: function () {
            this.base();
        },
        applyTypeAhead: function () {
            var self = this;
            if (self.sf){
            var tConfig = tConfig = {};
            var tDatasets = tDatasets = {};
            if (!tDatasets.name) {
                tDatasets.name = self.getId();
            }

            var tEvents = tEvents = {};

            var bloodHoundConfig = {
                datumTokenizer: function (d) {
                    return Bloodhound.tokenizers.whitespace(d.value);
                },
                queryTokenizer: Bloodhound.tokenizers.whitespace
            };

            /*
            if (tDatasets.type === "prefetch") {
                bloodHoundConfig.prefetch = {
                    url: tDatasets.source,
                    ajax: {
                        //url: sf.getServiceRoot('OpenContent') + "FileUpload/UploadFile",
                        beforeSend: connector.servicesFramework.setModuleHeaders,

                    }
                };

                if (tDatasets.filter) {
                    bloodHoundConfig.prefetch.filter = tDatasets.filter;
                }
            }
            */

            bloodHoundConfig.remote = {
                url: self.sf.getServiceRoot('OpenContent') + "DnnEntitiesAPI/Tabs?q=%QUERY&l=" + self.culture,
                ajax: {
                    beforeSend: self.sf.setModuleHeaders,
                }
            };

            if (tDatasets.filter) {
                bloodHoundConfig.remote.filter = tDatasets.filter;
            }

            if (tDatasets.replace) {
                bloodHoundConfig.remote.replace = tDatasets.replace;
            }


            var engine = new Bloodhound(bloodHoundConfig);
            engine.initialize();
            tDatasets.source = engine.ttAdapter();

            tDatasets.templates = {
                "empty": "Nothing found...",
                "suggestion": "{{name}}"
            };

            // compile templates
            if (tDatasets.templates) {
                for (var k in tDatasets.templates) {
                    var template = tDatasets.templates[k];
                    if (typeof (template) === "string") {
                        tDatasets.templates[k] = Handlebars.compile(template);
                    }
                }
            }

            // process typeahead
            $(self.control).typeahead(tConfig, tDatasets);

            // listen for "autocompleted" event and set the value of the field
            $(self.control).on("typeahead:autocompleted", function (event, datum) {
                self.setValue(datum.value);
                $(self.control).change();
            });

            // listen for "selected" event and set the value of the field
            $(self.control).on("typeahead:selected", function (event, datum) {
                self.setValue(datum.value);
                $(self.control).change();
            });

            // custom events
            if (tEvents) {
                if (tEvents.autocompleted) {
                    $(self.control).on("typeahead:autocompleted", function (event, datum) {
                        tEvents.autocompleted(event, datum);
                    });
                }
                if (tEvents.selected) {
                    $(self.control).on("typeahead:selected", function (event, datum) {
                        tEvents.selected(event, datum);
                    });
                }
            }

            // when the input value changes, change the query in typeahead
            // this is to keep the typeahead control sync'd with the actual dom value
            // only do this if the query doesn't already match
            var fi = $(self.control);
            $(self.control).change(function () {

                var value = $(this).val();

                var newValue = $(fi).typeahead('val');
                if (newValue !== value) {
                    $(fi).typeahead('val', newValue);
                }

            });

            // some UI cleanup (we don't want typeahead to restyle)
            $(self.field).find("span.twitter-typeahead").first().css("display", "block"); // SPAN to behave more like DIV, next line
            $(self.field).find("span.twitter-typeahead input.tt-input").first().css("background-color", "");
            }
        }
    });
    Alpaca.registerFieldClass("url", Alpaca.Fields.DnnUrlField);

})(jQuery);
(function($) {

    var Alpaca = $.alpaca;
    
    Alpaca.Fields.Url2Field = Alpaca.Fields.ListField.extend(
    /**
     * @lends Alpaca.Fields.Url2Field.prototype
     */
    {
        constructor: function (container, data, options, schema, view, connector) {
            var self = this;
            this.base(container, data, options, schema, view, connector);
            this.sf = connector.servicesFramework;
            this.culture = connector.culture;
            this.dataSource = {};
        },
        /**
         * @see Alpaca.Url2Field#getFieldType
         */
        getFieldType: function()
        {
            return "select";
        },

        /**
         * @see Alpaca.Fields.Url2Field#setup
         */
        setup: function()
        {
            var self = this;
            if (self.schema["type"] && self.schema["type"] === "array") {
                self.options.multiple = true;
                self.options.removeDefaultNone = true;
                //self.options.hideNone = true;
            }
            this.base();
        },

        getValue: function () {
            if (this.control && this.control.length > 0) {
                var val = this._getControlVal(true);
                if (typeof (val) === "undefined") {
                    val = this.data;
                }
                else if (Alpaca.isArray(val)) {
                    for (var i = 0; i < val.length; i++) {
                        val[i] = this.ensureProperType(val[i]);
                    }
                }

                return this.base(val);
            }
        },

        /**
         * @see Alpaca.Field#setValue
         */
        setValue: function(val)
        {
            if (Alpaca.isArray(val))
            {
                if (!Alpaca.compareArrayContent(val, this.getValue()))
                {
                    if (!Alpaca.isEmpty(val) && this.control)
                    {
                        this.control.val(val);
                    }
                    this.base(val);
                }
            }
            else
            {
                if (val !== this.getValue())
                {
                    /*
                    if (!Alpaca.isEmpty(val) && this.control)
                    {
                        this.control.val(val);
                    }
                    */
                    if (this.control && typeof(val) != "undefined" && val != null)
                    {
                        this.control.val(val);
                    }
                    this.base(val);
                }
            }
        },

        /**
         * @see Alpaca.Url2Field#getEnum
         */
        getEnum: function()
        {
            if (this.schema)
            {
                if (this.schema["enum"])
                {
                    return this.schema["enum"];
                }
                else if (this.schema["type"] && this.schema["type"] === "array" && this.schema["items"] && this.schema["items"]["enum"])
                {
                    return this.schema["items"]["enum"];
                }
            }
        },

        initControlEvents: function()
        {
            var self = this;

            self.base();

            if (self.options.multiple)
            {
                var button = this.control.parent().find(".select2-search__field");

                button.focus(function(e) {
                    if (!self.suspendBlurFocus)
                    {
                        self.onFocus.call(self, e);
                        self.trigger("focus", e);
                    }
                });

                button.blur(function(e) {
                    if (!self.suspendBlurFocus)
                    {
                        self.onBlur.call(self, e);
                        self.trigger("blur", e);
                    }
                });
                this.control.on("change", function (e) {
                    self.onChange.call(self, e);
                    self.trigger("change", e);

                });
            }
        },

        beforeRenderControl: function(model, callback)
        {
            var self = this;
            this.base(model, function() {
                self.selectOptions = [];

                if (self.sf) {

                    var completionFunction = function () {
                        self.schema.enum = [];
                        self.options.optionLabels = [];
                        for (var i = 0; i < self.selectOptions.length; i++) {
                            self.schema.enum.push(self.selectOptions[i].value);
                            self.options.optionLabels.push(self.selectOptions[i].text);
                        }
                        // push back to model
                        model.selectOptions = self.selectOptions;
                        callback();
                    };
                    var postData = { q: "*", l: self.culture };
                    $.ajax({
                        url: self.sf.getServiceRoot("OpenContent") + "DnnEntitiesAPI" + "/" + "TabsLookup",
                        beforeSend: self.sf.setModuleHeaders,
                        type: "get",
                        dataType: "json",
                        //contentType: "application/json; charset=utf-8",
                        data: postData,
                        success: function (jsonDocument) {
                            var ds = jsonDocument;

                            if (self.options.dsTransformer && Alpaca.isFunction(self.options.dsTransformer)) {
                                ds = self.options.dsTransformer(ds);
                            }
                            if (ds) {
                                if (Alpaca.isObject(ds)) {
                                    // for objects, we walk through one key at a time
                                    // the insertion order is the order of the keys from the map
                                    // to preserve order, consider using an array as below
                                    $.each(ds, function (key, value) {
                                        self.selectOptions.push({
                                            "value": key,
                                            "text": value
                                        });
                                    });
                                    completionFunction();
                                }
                                else if (Alpaca.isArray(ds)) {
                                    // for arrays, we walk through one index at a time
                                    // the insertion order is dictated by the order of the indices into the array
                                    // this preserves order
                                    $.each(ds, function (index, value) {
                                        self.selectOptions.push({
                                            "value": value.value,
                                            "text": value.text
                                        });
                                        self.dataSource[value.value] = value;
                                    });
                                    completionFunction();
                                }
                            }
                        },
                        "error": function (jqXHR, textStatus, errorThrown) {

                            self.errorCallback({
                                "message": "Unable to load data from uri : " + self.options.dataSource,
                                "stage": "DATASOURCE_LOADING_ERROR",
                                "details": {
                                    "jqXHR": jqXHR,
                                    "textStatus": textStatus,
                                    "errorThrown": errorThrown
                                }
                            });
                        }
                    });
                } else {
                    callback();
                }
            });
        },

        prepareControlModel: function(callback)
        {
            var self = this;
            this.base(function(model) {
                model.selectOptions = self.selectOptions;
                callback(model);
            });
        },

        afterRenderControl: function(model, callback)
        {
            var self = this;

            this.base(model, function() {

                // if emptySelectFirst and nothing currently checked, then pick first item in the value list
                // set data and visually select it
                if (Alpaca.isUndefined(self.data) && self.options.emptySelectFirst && self.selectOptions && self.selectOptions.length > 0)
                {
                    self.data = self.selectOptions[0].value;
                }

                // do this little trick so that if we have a default value, it gets set during first render
                // this causes the state of the control
                if (self.data)
                {
                    self.setValue(self.data);
                }

                // if we are in multiple mode and the bootstrap multiselect plugin is available, bind it in
                //if (self.options.multiple && $.fn.multiselect)
                if ($.fn.select2)
                {
                    var settings = null;
                    if (self.options.select2) {
                        settings = self.options.select2;
                    }
                    else
                    {
                        settings = {};
                    }
                    /*
                    if (!settings.nonSelectedText)
                    {
                        settings.nonSelectedText = "None";
                        if (self.options.noneLabel)
                        {
                            settings.nonSelectedText = self.options.noneLabel;
                        }
                    }
                    if (self.options.hideNone)
                    {
                        delete settings.nonSelectedText;
                    }
                    */

                    settings.templateResult = function (state) {
                        if (!state.id) { return state.text; }
                        
                        var $state = $(
                          '<span>' + state.text + '</span>'
                        );
                        return $state;
                    };

                    settings.templateSelection = function (state) {
                        if (!state.id) { return state.text; }
                        
                        var $state = $(
                          '<span>' + state.text + '</span>'
                        );
                        return $state;
                    };

                    $(self.getControlEl()).select2(settings);
                }

                callback();

            });
        },

        /**
         * Validate against enum property.
         *
         * @returns {Boolean} True if the element value is part of the enum list, false otherwise.
         */
        _validateEnum: function()
        {
            var _this = this;

            if (this.schema["enum"])
            {
                var val = this.data;

                if (!this.isRequired() && Alpaca.isValEmpty(val))
                {
                    return true;
                }

                if (this.options.multiple)
                {
                    var isValid = true;

                    if (!val)
                    {
                        val = [];
                    }

                    if (!Alpaca.isArray(val) && !Alpaca.isObject(val))
                    {
                        val = [val];
                    }

                    $.each(val, function(i,v) {

                        if ($.inArray(v, _this.schema["enum"]) <= -1)
                        {
                            isValid = false;
                            return false;
                        }

                    });

                    return isValid;
                }
                else
                {
                    return ($.inArray(val, this.schema["enum"]) > -1);
                }
            }
            else
            {
                return true;
            }
        },

        /**
         * @see Alpaca.Field#onChange
         */
        onChange: function(e)
        {
            this.base(e);

            var _this = this;

            Alpaca.later(25, this, function() {
                var v = _this.getValue();
                _this.setValue(v);
                _this.refreshValidationState();
            });
        },

        /**
         * Validates if number of items has been less than minItems.
         * @returns {Boolean} true if number of items has been less than minItems
         */
        _validateMinItems: function()
        {
            if (this.schema.items && this.schema.items.minItems)
            {
                if ($(":selected",this.control).length < this.schema.items.minItems)
                {
                    return false;
                }
            }

            return true;
        },

        /**
         * Validates if number of items has been over maxItems.
         * @returns {Boolean} true if number of items has been over maxItems
         */
        _validateMaxItems: function()
        {
            if (this.schema.items && this.schema.items.maxItems)
            {
                if ($(":selected",this.control).length > this.schema.items.maxItems)
                {
                    return false;
                }
            }

            return true;
        },

        /**
         * @see Alpaca.ContainerField#handleValidate
         */
        handleValidate: function()
        {
            var baseStatus = this.base();

            var valInfo = this.validation;

            var status = this._validateMaxItems();
            valInfo["tooManyItems"] = {
                "message": status ? "" : Alpaca.substituteTokens(this.getMessage("tooManyItems"), [this.schema.items.maxItems]),
                "status": status
            };

            status = this._validateMinItems();
            valInfo["notEnoughItems"] = {
                "message": status ? "" : Alpaca.substituteTokens(this.getMessage("notEnoughItems"), [this.schema.items.minItems]),
                "status": status
            };

            return baseStatus && valInfo["tooManyItems"]["status"] && valInfo["notEnoughItems"]["status"];
        },

        /**
         * @see Alpaca.Field#focus
         */
        focus: function(onFocusCallback)
        {
            if (this.control && this.control.length > 0)
            {
                // set focus onto the select
                var el = $(this.control).get(0);

                el.focus();

                if (onFocusCallback)
                {
                    onFocusCallback(this);
                }
            }
        }

        /* builder_helpers */
        ,

        /**
         * @see Alpaca.Field#getTitle
         */
        getTitle: function() {
            return "Select File Field";
        },

        /**
         * @see Alpaca.Field#getDescription
         */
        getDescription: function() {
            return "Select File Field";
        },

        /**
         * @private
         * @see Alpaca.Fields.Url2Field#getSchemaOfOptions
         */
        getSchemaOfOptions: function() {
            return Alpaca.merge(this.base(), {
                "properties": {
                    "multiple": {
                        "title": "Mulitple Selection",
                        "description": "Allow multiple selection if true.",
                        "type": "boolean",
                        "default": false
                    },
                    "size": {
                        "title": "Displayed Options",
                        "description": "Number of options to be shown.",
                        "type": "number"
                    },
                    "emptySelectFirst": {
                        "title": "Empty Select First",
                        "description": "If the data is empty, then automatically select the first item in the list.",
                        "type": "boolean",
                        "default": false
                    },
                    "multiselect": {
                        "title": "Multiselect Plugin Settings",
                        "description": "Multiselect plugin properties - http://davidstutz.github.io/bootstrap-multiselect",
                        "type": "any"
                    }
                }
            });
        },

        /**
         * @private
         * @see Alpaca.Fields.Url2Field#getOptionsForOptions
         */
        getOptionsForOptions: function() {
            return Alpaca.merge(this.base(), {
                "fields": {
                    "multiple": {
                        "rightLabel": "Allow multiple selection ?",
                        "helper": "Allow multiple selection if checked",
                        "type": "checkbox"
                    },
                    "size": {
                        "type": "integer"
                    },
                    "emptySelectFirst": {
                        "type": "checkbox",
                        "rightLabel": "Empty Select First"
                    },
                    "multiselect": {
                        "type": "object",
                        "rightLabel": "Multiselect plugin properties - http://davidstutz.github.io/bootstrap-multiselect"
                    }
                }
            });
        }

        /* end_builder_helpers */

    });

    Alpaca.registerFieldClass("url2", Alpaca.Fields.Url2Field);

})(jQuery);
(function ($) {

    var Alpaca = $.alpaca;

    Alpaca.Fields.wysihtmlField = Alpaca.Fields.TextAreaField.extend(
    /**
     * @lends Alpaca.Fields.CKEditorField.prototype
     */
    {
        /**
         * @see Alpaca.Fields.TextAreaField#getFieldType
         */
        getFieldType: function () {
            return "wysihtml";
        },

        /**
         * @see Alpaca.Fields.TextAreaField#setup
         */
        setup: function () {
            if (!this.data) {
                this.data = "";
            }

            this.base();

            if (typeof (this.options.wysihtml) == "undefined") {
                this.options.wysihtml = {};
            }
        },

        afterRenderControl: function (model, callback) {
            var self = this;

            this.base(model, function () {

                // see if we can render CK Editor
                if (!self.isDisplayOnly() && self.control ) {
                    //self.plugin = $(self.control).ckeditor(self.options.ckeditor); // Use CKEDITOR.replace() if element is <textarea>.
                    var el = self.control;
                    var ta = $(el).find('#'+self.id)[0];


                    self.editor = new wysihtml5.Editor(ta, {
                        toolbar: $(el).find('#' + self.id + '-toolbar')[0],                        
                        parserRules: wysihtml5ParserRules // defined in file parser rules javascript
                    });

                    wysihtml5.commands.custom_class = {
                        exec: function (composer, command, className) {
                            return wysihtml5.commands.formatBlock.exec(composer, command, "p", className, new RegExp(className, "g"));
                        },
                        state: function (composer, command, className) {
                            return wysihtml5.commands.formatBlock.state(composer, command, "p", className, new RegExp(className, "g"));
                        }
                    };

                }
                callback();
            });
        },
        getEditor: function () {
            return this.editor;
        },

        setValue: function(value)
        {
            var self = this;

            if (this.editor)
            {
                this.editor.setValue(value);
                //self.editor.clearSelection();
            }

            // be sure to call into base method
            this.base(value);
        },

        /**
         * @see Alpaca.Fields.TextField#getValue
         */
        getValue: function()
        {
            var value = null;
            if (this.editor)
            {
                if (this.editor.currentView == 'source')
                    value = this.editor.sourceView.textarea.value
                else 
                    value = this.editor.getValue();
            }
            return value;
        },


        /* builder_helpers */

        /**
         * @see Alpaca.Fields.TextAreaField#getTitle
         */
        getTitle: function () {
            return "wysihtml";
        },

        /**
         * @see Alpaca.Fields.TextAreaField#getDescription
         */
        getDescription: function () {
            return "Provides an instance of a wysihtml control for use in editing HTML.";
        },

        /**
         * @private
         * @see Alpaca.ControlField#getSchemaOfOptions
         */
        getSchemaOfOptions: function () {
            return Alpaca.merge(this.base(), {
                "properties": {
                    "wysihtml": {
                        "title": "CK Editor options",
                        "description": "Use this entry to provide configuration options to the underlying CKEditor plugin.",
                        "type": "any"
                    }
                }
            });
        },

        /**
         * @private
         * @see Alpaca.ControlField#getOptionsForOptions
         */
        getOptionsForOptions: function () {
            return Alpaca.merge(this.base(), {
                "fields": {
                    "wysiwyg": {
                        "type": "any"
                    }
                }
            });
        }

        /* end_builder_helpers */
    });

    Alpaca.registerFieldClass("wysihtml", Alpaca.Fields.wysihtmlField);

})(jQuery);
(function ($) {

    var Alpaca = $.alpaca;

    Alpaca.Fields.SummernoteField = Alpaca.Fields.TextAreaField.extend(
    /**
     * @lends Alpaca.Fields.SummernoteField.prototype
     */
    {
        /**
         * @see Alpaca.Fields.TextAreaField#getFieldType
         */
        getFieldType: function () {
            return "summernote";
        },

        /**
         * @see Alpaca.Fields.TextAreaField#setup
         */
        setup: function () {
            if (!this.data) {
                this.data = "";
            }

            this.base();

            if (typeof (this.options.summernote) == "undefined") {
                this.options.summernote = {
                    height: null,
                    minHeight: null,
                    maxHeight: null,
                    //focus: true
                };
            }
            if ( this.options.placeholder) {
                this.options.summernote = this.options.placeholder;
            }
        },

        afterRenderControl: function (model, callback) {
            var self = this;

            this.base(model, function () {

                // see if we can render Summernote Editor
                if (!self.isDisplayOnly() && self.control && $.fn.summernote) {
                    // wait for Alpaca to declare the DOM swapped and ready before we attempt to do anything with CKEditor
                    self.on("ready", function () {
                        $(self.control).summernote(self.options.summernote);
                    });
                }

                // if summernote's dom element gets destroyed, make sure we clean up the editor instance
                $(self.control).bind('destroyed', function () {
                    try { $(self.control).summernote('destroy'); } catch (e) { }
                });

                callback();
            });
        }

        /* builder_helpers */

        /**
         * @see Alpaca.Fields.TextAreaField#getTitle
         */
        ,
        getTitle: function () {
            return "Summernote Editor";
        },

        /**
         * @see Alpaca.Fields.TextAreaField#getDescription
         */
        getDescription: function () {
            return "Provides an instance of a Summernote Editor control for use in editing HTML.";
        },

        /**
         * @private
         * @see Alpaca.ControlField#getSchemaOfOptions
         */
        getSchemaOfOptions: function () {
            return Alpaca.merge(this.base(), {
                "properties": {
                    "summernote": {
                        "title": "Summernote Editor options",
                        "description": "Use this entry to provide configuration options to the underlying Summernote plugin.",
                        "type": "any"
                    }
                }
            });
        },

        /**
         * @private
         * @see Alpaca.ControlField#getOptionsForOptions
         */
        getOptionsForOptions: function () {
            return Alpaca.merge(this.base(), {
                "fields": {
                    "summernote": {
                        "type": "any"
                    }
                }
            });
        }

        /* end_builder_helpers */
    });

    Alpaca.registerFieldClass("summernote", Alpaca.Fields.SummernoteField);

})(jQuery);
(function ($) {

    var Alpaca = $.alpaca;

    Alpaca.Fields.MLSummernote = Alpaca.Fields.SummernoteField.extend(
    /**
     * @lends Alpaca.Fields.MLSummernote.prototype
     */
    {

        constructor: function (container, data, options, schema, view, connector) {
            var self = this;
            this.base(container, data, options, schema, view, connector);
            this.culture = connector.culture;
            this.defaultCulture = connector.defaultCulture;
            this.rootUrl = connector.rootUrl;
        },

        /**
         * @see Alpaca.Fields.MLSummernote#setup
         */
        setup: function () {
            if (this.data && Alpaca.isObject(this.data)) {
                this.olddata = this.data;
            } else if (this.data) {
                this.olddata = {};
                this.olddata[this.defaultCulture] = this.data;
            }
            
            if (this.culture != this.defaultCulture && this.olddata && this.olddata[this.defaultCulture]) {
                this.options.placeholder = this.olddata[this.defaultCulture];
            } else {
                this.options.placeholder = "";
            }

            this.base();
        },

        /**
         * @see Alpaca.Fields.MLSummernote#getValue
         */
        getValue: function () {
            var val = this.base();
            var self = this;
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
            return o;
        },

        /**
         * @see Alpaca.Fields.MLSummernote#setValue
         */
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
            else
            {
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
            callback();
        },
        
        /**
         * @see Alpaca.Fields.MLSummernote#getTitle
         */
        getTitle: function () {
            return "Multi Language CKEditor Field";
        },

        /**
         * @see Alpaca.Fields.MLSummernote#getDescription
         */
        getDescription: function () {
            return "Multi Language CKEditor field .";
        },

        /**
         * @private
         * @see Alpaca.Fields.MLSummernote#getSchemaOfOptions
         */
        getSchemaOfOptions: function () {
            return Alpaca.merge(this.base(), {
                "properties": {
                    "separator": {
                        "title": "Separator",
                        "description": "Separator used to split tags.",
                        "type": "string",
                        "default": ","
                    }
                }
            });
        },

        /**
         * @private
         * @see Alpaca.Fields.MLSummernote#getOptionsForOptions
         */
        getOptionsForOptions: function () {
            return Alpaca.merge(this.base(), {
                "fields": {
                    "separator": {
                        "type": "text"
                    }
                }
            });
        }

        /* end_builder_helpers */
    });

    Alpaca.registerFieldClass("mlsummernote", Alpaca.Fields.MLSummernote);

})(jQuery);
(function ($) {

    var Alpaca = $.alpaca;

    Alpaca.Fields.MLCKEditorField = Alpaca.Fields.CKEditorField.extend(
    /**
     * @lends Alpaca.Fields.CKEditorField.prototype
     */
    {

        constructor: function (container, data, options, schema, view, connector) {
            var self = this;
            this.base(container, data, options, schema, view, connector);
            this.culture = connector.culture;
            this.defaultCulture = connector.defaultCulture;
            this.rootUrl = connector.rootUrl;
        },

        /**
         * @see Alpaca.Fields.CKEditorField#setup
         */
        setup: function () {
            if (this.data && Alpaca.isObject(this.data)) {
                this.olddata = this.data;
            } else if (this.data) {
                this.olddata = {};
                this.olddata[this.defaultCulture] = this.data;
            }
            
            if (this.culture != this.defaultCulture && this.olddata && this.olddata[this.defaultCulture]) {
                this.options.placeholder = this.olddata[this.defaultCulture];
            } else {
                this.options.placeholder = "";
            }

            this.base();

            if (!this.options.ckeditor) {
                this.options.ckeditor = {};
            }
             if (CKEDITOR.config.enableConfigHelper && !this.options.ckeditor.extraPlugins) {
                this.options.ckeditor.extraPlugins = 'dnnpages,confighelper';
            } 
        },

        /**
         * @see Alpaca.Fields.CKEditorField#getValue
         */
        getValue: function () {
            var val = this.base();
            var self = this;
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
            return o;
        },

        /**
         * @see Alpaca.Fields.CKEditorField#setValue
         */
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
            else
            {
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
            callback();
        },
        
        /**
         * @see Alpaca.Fields.CKEditorField#getTitle
         */
        getTitle: function () {
            return "Multi Language CKEditor Field";
        },

        /**
         * @see Alpaca.Fields.CKEditorField#getDescription
         */
        getDescription: function () {
            return "Multi Language CKEditor field .";
        },

        /**
         * @private
         * @see Alpaca.Fields.CKEditorField#getSchemaOfOptions
         */
        getSchemaOfOptions: function () {
            return Alpaca.merge(this.base(), {
                "properties": {
                    "separator": {
                        "title": "Separator",
                        "description": "Separator used to split tags.",
                        "type": "string",
                        "default": ","
                    }
                }
            });
        },

        /**
         * @private
         * @see Alpaca.Fields.CKEditorField#getOptionsForOptions
         */
        getOptionsForOptions: function () {
            return Alpaca.merge(this.base(), {
                "fields": {
                    "separator": {
                        "type": "text"
                    }
                }
            });
        }

        /* end_builder_helpers */
    });

    Alpaca.registerFieldClass("mlckeditor", Alpaca.Fields.MLCKEditorField);

})(jQuery);
(function($) {

    var Alpaca = $.alpaca;
        
    Alpaca.Fields.MLFile2Field = Alpaca.Fields.File2Field.extend(
    /**
     * @lends Alpaca.Fields.File2Field.prototype
     */
    {
        constructor: function (container, data, options, schema, view, connector) {
            var self = this;
            this.base(container, data, options, schema, view, connector);
            this.culture = connector.culture;
            this.defaultCulture = connector.defaultCulture;
            this.rootUrl = connector.rootUrl;
        },
        /**
         * @see Alpaca.Fields.File2Field#setup
         */
        setup: function()
        {
            var self = this;
            if (this.data && Alpaca.isObject(this.data)) {
                this.olddata = this.data;
            } else if (this.data) {
                this.olddata = {};
                this.olddata[this.defaultCulture] = this.data;
            }
            this.base();
        },

        getValue: function () {
            
                var val = this.base(val);
                var self = this;
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
                return o;
        },

        /**
         * @see Alpaca.Field#setValue
         */
        setValue: function(val)
        {
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
                self.handlePostRender2(function () {
                    callback();
                });
            });
        },
        handlePostRender2: function (callback) {
            var self = this;
            var el = this.getControlEl();
            callback();
            $(this.control).after('<img src="' + self.rootUrl + 'images/Flags/' + this.culture + '.gif" class="flag" />');
        },
    });

    Alpaca.registerFieldClass("mlfile2", Alpaca.Fields.MLFile2Field);

})(jQuery);
(function ($) {

    var Alpaca = $.alpaca;

    Alpaca.Fields.MLFileField = Alpaca.Fields.FileField.extend(
    /**
     * @lends Alpaca.Fields.MLFileField.prototype
     */
    {
        constructor: function (container, data, options, schema, view, connector) {
            var self = this;
            this.base(container, data, options, schema, view, connector);
            this.culture = connector.culture;
            this.defaultCulture = connector.defaultCulture;
            this.rootUrl = connector.rootUrl;
        },

        /**
         * @see Alpaca.Fields.MLFileField#setup
         */
        setup: function () {
            if (this.data && Alpaca.isObject(this.data)) {
                this.olddata = this.data;
            } else if (this.data) {
                this.olddata = {};
                this.olddata[this.defaultCulture] = this.data;
            }
            if (this.culture != this.defaultCulture && this.olddata && this.olddata[this.defaultCulture]) {
                this.options.placeholder = this.olddata[this.defaultCulture];
            } else {
                this.options.placeholder = "";
            }

            this.base();
        },
        /**
         * @see Alpaca.Fields.MLFileField#getValue
         */
        getValue: function () {
            var val = this.base();
            var self = this;
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
            return o;
        },

        /**
         * @see Alpaca.Fields.MLFileField#setValue
         */
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
            else
            {
                this.base(val);
            }
        },
        afterRenderControl: function (model, callback) {
            var self = this;
            this.base(model, function () {
                self.handlePostRender2(function () {
                    callback();
                });
            });
        },
        handlePostRender2: function (callback) {
            var self = this;
            var el = this.getTextControlEl();
            $(this.control.get(0)).after('<img src="' + self.rootUrl + 'images/Flags/' + this.culture + '.gif" class="flag" />');
            callback();
        },
        
        /**
         * @see Alpaca.Fields.MLFileField#getTitle
         */
        getTitle: function () {
            return "Multi Language Url Field";
        },

        /**
         * @see Alpaca.Fields.MLFileField#getDescription
         */
        getDescription: function () {
            return "Multi Language Url field .";
        },

        /**
         * @private
         * @see Alpaca.Fields.MLFileField#getSchemaOfOptions
         */
        getSchemaOfOptions: function () {
            return Alpaca.merge(this.base(), {
                "properties": {
                    "separator": {
                        "title": "Separator",
                        "description": "Separator used to split tags.",
                        "type": "string",
                        "default": ","
                    }
                }
            });
        },

        /**
         * @private
         * @see Alpaca.Fields.MLFileField#getOptionsForOptions
         */
        getOptionsForOptions: function () {
            return Alpaca.merge(this.base(), {
                "fields": {
                    "separator": {
                        "type": "text"
                    }
                }
            });
        }

        /* end_builder_helpers */
    });

    Alpaca.registerFieldClass("mlfile", Alpaca.Fields.MLFileField);

})(jQuery);
(function($) {

    var Alpaca = $.alpaca;
        
    Alpaca.Fields.MLFolder2Field = Alpaca.Fields.Folder2Field.extend(
    /**
     * @lends Alpaca.Fields.Folder2Field.prototype
     */
    {
        constructor: function (container, data, options, schema, view, connector) {
            var self = this;
            this.base(container, data, options, schema, view, connector);
            this.culture = connector.culture;
            this.defaultCulture = connector.defaultCulture;
            this.rootUrl = connector.rootUrl;
        },
        /**
         * @see Alpaca.Fields.Folder2Field#setup
         */
        setup: function()
        {
            var self = this;
            if (this.data && Alpaca.isObject(this.data)) {
                this.olddata = this.data;
            } else if (this.data) {
                this.olddata = {};
                this.olddata[this.defaultCulture] = this.data;
            }
            this.base();
        },

        getValue: function () {
            
                var val = this.base(val);
                var self = this;
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
                return o;
        },

        /**
         * @see Alpaca.Field#setValue
         */
        setValue: function(val)
        {
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
                self.handlePostRender2(function () {
                    callback();
                });
            });
        },
        handlePostRender2: function (callback) {
            var self = this;
            var el = this.getControlEl();

            
            callback();

            $(this.control).parent().find('.select2').after('<img src="' + self.rootUrl + 'images/Flags/' + this.culture + '.gif" class="flag" />');
            
        },
    });

    Alpaca.registerFieldClass("mlfolder2", Alpaca.Fields.MLFolder2Field);

})(jQuery);
(function($) {

    var Alpaca = $.alpaca;
        
    Alpaca.Fields.MLImage2Field = Alpaca.Fields.Image2Field.extend(
    /**
     * @lends Alpaca.Fields.Image2Field.prototype
     */
    {
        constructor: function (container, data, options, schema, view, connector) {
            var self = this;
            this.base(container, data, options, schema, view, connector);
            //this.sf = connector.servicesFramework;
            //this.dataSource = {};
            this.culture = connector.culture;
            this.defaultCulture = connector.defaultCulture;
            this.rootUrl = connector.rootUrl;
        },
        /**
         * @see Alpaca.Fields.Image2Field#setup
         */
        setup: function()
        {
            var self = this;
            if (this.data && Alpaca.isObject(this.data)) {
                this.olddata = this.data;
            } else if (this.data) {
                this.olddata = {};
                this.olddata[this.defaultCulture] = this.data;
            }
            
            this.base();
        },

        getValue: function () {
                var val = this.base();
                var self = this;
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
                return o;
        },

        /**
         * @see Alpaca.Field#setValue
         */
        setValue: function(val)
        {
            
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
                self.handlePostRender2(function () {
                    callback();
                });
            });
        },
        handlePostRender2: function (callback) {
            var self = this;
            var el = this.getControlEl();

            
            callback();

            $(this.control).parent().find('.select2').after('<img src="' + self.rootUrl + 'images/Flags/' + this.culture + '.gif" class="flag" />');
            
        },
    });

    Alpaca.registerFieldClass("mlimage2", Alpaca.Fields.MLImage2Field);

})(jQuery);
(function($) {

    var Alpaca = $.alpaca;
        
    Alpaca.Fields.MLImageXField = Alpaca.Fields.ImageXField.extend(
    /**
     * @lends Alpaca.Fields.MLImageXField.prototype
     */
    {
        constructor: function (container, data, options, schema, view, connector) {
            var self = this;
            this.base(container, data, options, schema, view, connector);
            this.culture = connector.culture;
            this.defaultCulture = connector.defaultCulture;
            this.rootUrl = connector.rootUrl;
        },
        /**
         * @see Alpaca.Fields.MLImageXField#setup
         */
        setup: function()
        {
            var self = this;
            if (this.data && Alpaca.isObject(this.data)) {
                this.olddata = this.data;
            } else if (this.data) {
                this.olddata = {};
                this.olddata[this.defaultCulture] = this.data;
            }
            this.base();
        },

        getValue: function () {
                var val = this.base();
                var self = this;
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
                return o;
        },



        /**
         * @see Alpaca.MLImageXField#setValue
         */
        setValue: function(val)
        {
            
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
                self.handlePostRender2(function () {
                    callback();
                });
            });
        },
        handlePostRender2: function (callback) {
            var self = this;
            var el = this.getControlEl();
            callback();
            $(this.control).parent().find('.select2').after('<img src="' + self.rootUrl + 'images/Flags/' + this.culture + '.gif" class="flag" />');
            
        },
    });

    Alpaca.registerFieldClass("mlimagex", Alpaca.Fields.MLImageXField);

})(jQuery);
(function ($) {

    var Alpaca = $.alpaca;

    Alpaca.Fields.MLImageField = Alpaca.Fields.ImageField.extend(
    /**
     * @lends Alpaca.Fields.MLImageField.prototype
     */
    {
        constructor: function (container, data, options, schema, view, connector) {
            var self = this;
            this.base(container, data, options, schema, view, connector);
            this.culture = connector.culture;
            this.defaultCulture = connector.defaultCulture;
            this.rootUrl = connector.rootUrl;
        },

        /**
         * @see Alpaca.Fields.MLImageField#setup
         */
        setup: function () {
            if (this.data && Alpaca.isObject(this.data)) {
                this.olddata = this.data;
            } else if (this.data) {
                this.olddata = {};
                this.olddata[this.defaultCulture] = this.data;
            }
            if (this.culture != this.defaultCulture && this.olddata && this.olddata[this.defaultCulture]) {
                this.options.placeholder = this.olddata[this.defaultCulture];
            } else {
                this.options.placeholder = "";
            }
            this.base();
        },
        /**
         * @see Alpaca.Fields.MLImageField#getValue
         */
        getValue: function () {
            var val = this.base();
            var self = this;
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
            return o;
        },

        /**
         * @see Alpaca.Fields.MLImageField#setValue
         */
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
            else
            {
                this.base(val);
            }
        },
        
        afterRenderControl: function (model, callback) {
            var self = this;
            this.base(model, function () {
                self.handlePostRender2(function () {
                    callback();
                });
            });
        },
        handlePostRender2: function (callback) {
            var self = this;
            var el = this.getTextControlEl();
            $(this.control.get(0)).after('<img src="' + self.rootUrl + 'images/Flags/' + this.culture + '.gif" class="flag" />');
            callback();
        },
        
        /**
         * @see Alpaca.Fields.MLImageField#getTitle
         */
        getTitle: function () {
            return "Multi Language Url Field";
        },

        /**
         * @see Alpaca.Fields.MLImageField#getDescription
         */
        getDescription: function () {
            return "Multi Language Url field .";
        },

        /**
         * @private
         * @see Alpaca.Fields.MLImageField#getSchemaOfOptions
         */
        getSchemaOfOptions: function () {
            return Alpaca.merge(this.base(), {
                "properties": {
                    "separator": {
                        "title": "Separator",
                        "description": "Separator used to split tags.",
                        "type": "string",
                        "default": ","
                    }
                }
            });
        },

        /**
         * @private
         * @see Alpaca.Fields.MLImageField#getOptionsForOptions
         */
        getOptionsForOptions: function () {
            return Alpaca.merge(this.base(), {
                "fields": {
                    "separator": {
                        "type": "text"
                    }
                }
            });
        }

        /* end_builder_helpers */
    });

    Alpaca.registerFieldClass("mlimage", Alpaca.Fields.MLImageField);

})(jQuery);
(function ($) {

    var Alpaca = $.alpaca;

    Alpaca.Fields.MLTextAreaField = Alpaca.Fields.TextAreaField.extend(
    /**
     * @lends Alpaca.Fields.TagField.prototype
     */
    {
        constructor: function (container, data, options, schema, view, connector) {
            var self = this;
            this.base(container, data, options, schema, view, connector);
            this.culture = connector.culture;
            this.defaultCulture = connector.defaultCulture;
            this.rootUrl = connector.rootUrl;
        },
        /**
         * @see Alpaca.Fields.TextField#getFieldType
        */
        /*
        getFieldType: function () {
            return "text";
        },
        */

        /**
         * @see Alpaca.Fields.TextField#setup
         */
        setup: function () {

            if (this.data && Alpaca.isObject(this.data)) {             
                this.olddata = this.data;
            } else if (this.data) {
                this.olddata = {};
                this.olddata[this.defaultCulture] = this.data;
            }
            
            
            if (this.culture != this.defaultCulture && this.olddata && this.olddata[this.defaultCulture]) {
                this.options.placeholder = this.olddata[this.defaultCulture];
            } else {
                this.options.placeholder = "";
            }
            this.base();
            /*
            Alpaca.mergeObject(this.options, {
                "fieldClass": "flag-"+this.culture
            });
            */
        },
        /**
         * @see Alpaca.Fields.TextField#getValue
         */
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

        /**
         * @see Alpaca.Fields.TextField#setValue
         */
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
            else
            {
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
        
        /**
         * @see Alpaca.Fields.TextField#getTitle
         */
        getTitle: function () {
            return "Multi Language Text Field";
        },

        /**
         * @see Alpaca.Fields.TextField#getDescription
         */
        getDescription: function () {
            return "Multi Language Text field .";
        },

        /**
         * @private
         * @see Alpaca.Fields.TextField#getSchemaOfOptions
         */
        getSchemaOfOptions: function () {
            return Alpaca.merge(this.base(), {
                "properties": {
                    "separator": {
                        "title": "Separator",
                        "description": "Separator used to split tags.",
                        "type": "string",
                        "default": ","
                    }
                }
            });
        },

        /**
         * @private
         * @see Alpaca.Fields.TextField#getOptionsForOptions
         */
        getOptionsForOptions: function () {
            return Alpaca.merge(this.base(), {
                "fields": {
                    "separator": {
                        "type": "text"
                    }
                }
            });
        }

        /* end_builder_helpers */
    });

    Alpaca.registerFieldClass("mltextarea", Alpaca.Fields.MLTextAreaField);

})(jQuery);
(function ($) {

    var Alpaca = $.alpaca;

    Alpaca.Fields.MLTextField = Alpaca.Fields.TextField.extend(
    /**
     * @lends Alpaca.Fields.TagField.prototype
     */
    {
        constructor: function (container, data, options, schema, view, connector) {
            var self = this;
            this.base(container, data, options, schema, view, connector);
            this.culture = connector.culture;
            this.defaultCulture = connector.defaultCulture;
            this.rootUrl = connector.rootUrl;
        },
        /**
         * @see Alpaca.Fields.TextField#getFieldType
        */
        /*
        getFieldType: function () {
            return "text";
        },
        */

        /**
         * @see Alpaca.Fields.TextField#setup
         */
        setup: function () {

            if (this.data && Alpaca.isObject(this.data)) {             
                this.olddata = this.data;
            } else if (this.data) {
                this.olddata = {};
                this.olddata[this.defaultCulture] = this.data;
            }
            
            
            if (this.culture != this.defaultCulture && this.olddata && this.olddata[this.defaultCulture]) {
                this.options.placeholder = this.olddata[this.defaultCulture];
            } else {
                this.options.placeholder = "";
            }
            this.base();
            /*
            Alpaca.mergeObject(this.options, {
                "fieldClass": "flag-"+this.culture
            });
            */
        },
        /**
         * @see Alpaca.Fields.TextField#getValue
         */
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

        /**
         * @see Alpaca.Fields.TextField#setValue
         */
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
            else
            {
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
        
        /**
         * @see Alpaca.Fields.TextField#getTitle
         */
        getTitle: function () {
            return "Multi Language Text Field";
        },

        /**
         * @see Alpaca.Fields.TextField#getDescription
         */
        getDescription: function () {
            return "Multi Language Text field .";
        },

        /**
         * @private
         * @see Alpaca.Fields.TextField#getSchemaOfOptions
         */
        getSchemaOfOptions: function () {
            return Alpaca.merge(this.base(), {
                "properties": {
                    "separator": {
                        "title": "Separator",
                        "description": "Separator used to split tags.",
                        "type": "string",
                        "default": ","
                    }
                }
            });
        },

        /**
         * @private
         * @see Alpaca.Fields.TextField#getOptionsForOptions
         */
        getOptionsForOptions: function () {
            return Alpaca.merge(this.base(), {
                "fields": {
                    "separator": {
                        "type": "text"
                    }
                }
            });
        }

        /* end_builder_helpers */
    });

    Alpaca.registerFieldClass("mltext", Alpaca.Fields.MLTextField);

})(jQuery);
(function($) {

    var Alpaca = $.alpaca;
        
    Alpaca.Fields.MLUrl2Field = Alpaca.Fields.Url2Field.extend(
    /**
     * @lends Alpaca.Fields.Url2Field.prototype
     */
    {
        constructor: function (container, data, options, schema, view, connector) {
            var self = this;
            this.base(container, data, options, schema, view, connector);
            //this.sf = connector.servicesFramework;
            //this.dataSource = {};
            this.culture = connector.culture;
            this.defaultCulture = connector.defaultCulture;
            this.rootUrl = connector.rootUrl;
        },
        /**
         * @see Alpaca.Fields.Url2Field#setup
         */
        setup: function()
        {
            var self = this;
            if (this.data && Alpaca.isObject(this.data)) {
                this.olddata = this.data;
            } else if (this.data) {
                this.olddata = {};
                this.olddata[this.defaultCulture] = this.data;
            }
            
            this.base();
        },

        getValue: function () {
            
                var val = this.base(val);

                var self = this;
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
                return o;
            
        },

        /**
         * @see Alpaca.Field#setValue
         */
        setValue: function(val)
        {
            
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
                self.handlePostRender2(function () {
                    callback();
                });
            });
        },
        handlePostRender2: function (callback) {
            var self = this;
            var el = this.getControlEl();

            
            callback();

            $(this.control).parent().find('.select2').after('<img src="' + self.rootUrl + 'images/Flags/' + this.culture + '.gif" class="flag" />');
            
        },
    });

    Alpaca.registerFieldClass("mlurl2", Alpaca.Fields.MLUrl2Field);

})(jQuery);
(function ($) {

    var Alpaca = $.alpaca;

    Alpaca.Fields.MLUrlField = Alpaca.Fields.DnnUrlField.extend(
    /**
     * @lends Alpaca.Fields.MLUrlField.prototype
     */
    {
        constructor: function (container, data, options, schema, view, connector) {
            var self = this;
            this.base(container, data, options, schema, view, connector);
            this.culture = connector.culture;
            this.defaultCulture = connector.defaultCulture;
            this.rootUrl = connector.rootUrl;
        },

        /**
         * @see Alpaca.Fields.MLUrlField#setup
         */
        setup: function () {
            if (this.data && Alpaca.isObject(this.data)) {
                this.olddata = this.data;
            } else if (this.data) {
                this.olddata = {};
                this.olddata[this.defaultCulture] = this.data;
            }
            if (this.culture != this.defaultCulture && this.olddata && this.olddata[this.defaultCulture]) {
                this.options.placeholder = this.olddata[this.defaultCulture];
            } else {
                this.options.placeholder = "";
            }
            this.base();
        },
        /**
         * @see Alpaca.Fields.MLUrlField#getValue
         */
        getValue: function () {
            var val = this.base();
            var self = this;
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
            return o;
        },

        /**
         * @see Alpaca.Fields.MLUrlField#setValue
         */
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
            else
            {
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
            callback();
        },
        
        /**
         * @see Alpaca.Fields.MLUrlField#getTitle
         */
        getTitle: function () {
            return "Multi Language Url Field";
        },

        /**
         * @see Alpaca.Fields.MLUrlField#getDescription
         */
        getDescription: function () {
            return "Multi Language Url field .";
        },

        /**
         * @private
         * @see Alpaca.Fields.MLUrlField#getSchemaOfOptions
         */
        getSchemaOfOptions: function () {
            return Alpaca.merge(this.base(), {
                "properties": {
                    "separator": {
                        "title": "Separator",
                        "description": "Separator used to split tags.",
                        "type": "string",
                        "default": ","
                    }
                }
            });
        },

        /**
         * @private
         * @see Alpaca.Fields.MLUrlField#getOptionsForOptions
         */
        getOptionsForOptions: function () {
            return Alpaca.merge(this.base(), {
                "fields": {
                    "separator": {
                        "type": "text"
                    }
                }
            });
        }

        /* end_builder_helpers */
    });

    Alpaca.registerFieldClass("mlurl", Alpaca.Fields.MLUrlField);

})(jQuery);
(function ($) {

    var Alpaca = $.alpaca;

    Alpaca.Fields.MLwysihtmlField = Alpaca.Fields.wysihtmlField.extend(
    /**
     * @lends Alpaca.Fields.MLwysihtmlField.prototype
     */
    {
        constructor: function (container, data, options, schema, view, connector) {
            var self = this;
            this.base(container, data, options, schema, view, connector);
            this.culture = connector.culture;
            this.defaultCulture = connector.defaultCulture;
            this.rootUrl = connector.rootUrl;
        },
        /**
         * @see Alpaca.Fields.MLwysihtmlField#setup
         */
        setup: function () {
            if (this.data && Alpaca.isObject(this.data)) {
                this.olddata = this.data;
            } else if (this.data) {
                this.olddata = {};
                this.olddata[this.defaultCulture] = this.data;
            }
            this.base();
        },

        /**
         * @see Alpaca.Fields.MLwysihtmlField#getValue
         */
        getValue: function () {
            var val = this.base();
            var self = this;
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
            return o;
        },

        /**
         * @see Alpaca.Fields.MLwysihtmlField#setValue
         */
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
                self.handlePostRender2(function () {
                    callback();
                });
            });
        },
        handlePostRender2: function (callback) {
            var self = this;
            var el = this.getControlEl();
            $(this.control.get(0)).after('<img src="' + self.rootUrl + 'images/Flags/' + this.culture + '.gif" class="flag" />');
            callback();
        },


        /* builder_helpers */

        /**
         * @see Alpaca.Fields.TextAreaField#getTitle
         */
        getTitle: function () {
            return "ML wysihtml Field";
        },

        /**
         * @see Alpaca.Fields.TextAreaField#getDescription
         */
        getDescription: function () {
            return "Provides an instance of a wysihtml control for use in editing MLHTML.";
        },

        /**
         * @private
         * @see Alpaca.ControlField#getSchemaOfOptions
         */
        getSchemaOfOptions: function () {
            return Alpaca.merge(this.base(), {
                "properties": {
                    "wysihtml": {
                        "title": "CK Editor options",
                        "description": "Use this entry to provide configuration options to the underlying CKEditor plugin.",
                        "type": "any"
                    }
                }
            });
        },

        /**
         * @private
         * @see Alpaca.ControlField#getOptionsForOptions
         */
        getOptionsForOptions: function () {
            return Alpaca.merge(this.base(), {
                "fields": {
                    "wysiwyg": {
                        "type": "any"
                    }
                }
            });
        }

        /* end_builder_helpers */
    });

    Alpaca.registerFieldClass("mlwysihtml", Alpaca.Fields.MLwysihtmlField);


})(jQuery);
(function($) {

    var Alpaca = $.alpaca;

    Alpaca.Fields.Accordion = Alpaca.Fields.ArrayField.extend(
    /**
     * @lends Alpaca.Fields.TitleArray.prototype
     */
    {

        /**
        * @see Alpaca.ControlField#getFieldType
        */
        getFieldType: function () {
            return "accordion";
        },
        constructor: function (container, data, options, schema, view, connector) {
            var self = this;
            this.base(container, data, options, schema, view, connector);
            this.culture = connector.culture;
            this.defaultCulture = connector.defaultCulture;
            this.rootUrl = connector.rootUrl;
        },
        setup: function()
        {
            var self = this;
            this.base();

            if (!self.options.titleField) {                
                if (self.schema.items && self.schema.items.properties 
                    && Object.keys(self.schema.items.properties).length) {
                    self.options.titleField = Object.keys(self.schema.items.properties)[0];                    
                }
            }

            /*
            if (typeof (this.options.items.postRender) == "undefined")
            {
                var label = "[no title]";
                this.options.items.postRender = function (callback) {
                    var field = null;
                        field = this.childrenByPropertyId[this.options.titleField];
                    if (field) {
                        var val = field.getValue();
                        val = val ? val : label;
                        this.getContainerEl().closest('.panel').find('.panel-title a').text(val);
                        field.on("keyup", function () {
                            var val = this.getValue();
                            val = val ? val : label;
                            $(this.getControlEl()).closest('.panel').find('.panel-title a').text(val);
                        });
                    }                    
                    if (Alpaca.isFunction(callback)) {
                        callback();
                    }
                };
            }
            */
        },
            
        createItem: function (index, itemSchema, itemOptions, itemData, postRenderCallback) {
            var self = this;
            this.base(index, itemSchema, itemOptions, itemData, function (control) {
                var label = "[no title]";
                var field = control.childrenByPropertyId[self.options.titleField];                
                if (field) {
                    var val = field.getValue();

                    // multi language
                    if (Alpaca.isObject(val)) {             
                        val = val[self.culture];
                    } 

                    val = val ? val : label;
                    control.getContainerEl().closest('.panel').find('.panel-title a').first().text(val);
                    field.on("keyup", function () {
                        var val = this.getValue();
                        // multi language
                        if (Alpaca.isObject(val)) {             
                            val = val[self.culture];
                        } 
                        val = val ? val : label;
                        $(this.getControlEl()).closest('.panel').find('.panel-title a').first().text(val);
                    });
                }

                if (postRenderCallback) {
                    postRenderCallback(control);
                }

            });
        },

        /**
         * @see Alpaca.ControlField#getType
         */
        getType: function() {
            return "array";
        }

        /* builder_helpers */
        ,

        /**
         * @see Alpaca.ControlField#getTitle
         */
        getTitle: function() {
            return "accordion Field";
        },

        /**
         * @see Alpaca.ControlField#getDescription
         */
        getDescription: function() {
            return "Renders array with title";
        }

        /* end_builder_helpers */
    });

    Alpaca.registerFieldClass("accordion", Alpaca.Fields.Accordion);

})(jQuery);