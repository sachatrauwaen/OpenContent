
/**
 *  jQuery fontIconPicker - 3.1.1
 *
 *  An icon picker built on top of font icons and jQuery
 *
 *  http://codeb.it/fontIconPicker
 *
 *  @author Alessandro Benoit & Swashata Ghosh
 *  @license MIT License
 *
 * {@link https://github.com/micc83/fontIconPicker}
 */
				
(function (global, factory) {
  typeof exports === 'object' && typeof module !== 'undefined' ? module.exports = factory(require('jquery')) :
  typeof define === 'function' && define.amd ? define(['jquery'], factory) :
  (global.initFontIconPickerNode = factory(global.jQuery));
}(this, (function (jQuery) { 'use strict';

  jQuery = jQuery && jQuery.hasOwnProperty('default') ? jQuery['default'] : jQuery;

  function _typeof(obj) {
    if (typeof Symbol === "function" && typeof Symbol.iterator === "symbol") {
      _typeof = function (obj) {
        return typeof obj;
      };
    } else {
      _typeof = function (obj) {
        return obj && typeof Symbol === "function" && obj.constructor === Symbol && obj !== Symbol.prototype ? "symbol" : typeof obj;
      };
    }

    return _typeof(obj);
  }

  function _toConsumableArray(arr) {
    return _arrayWithoutHoles(arr) || _iterableToArray(arr) || _nonIterableSpread();
  }

  function _arrayWithoutHoles(arr) {
    if (Array.isArray(arr)) {
      for (var i = 0, arr2 = new Array(arr.length); i < arr.length; i++) arr2[i] = arr[i];

      return arr2;
    }
  }

  function _iterableToArray(iter) {
    if (Symbol.iterator in Object(iter) || Object.prototype.toString.call(iter) === "[object Arguments]") return Array.from(iter);
  }

  function _nonIterableSpread() {
    throw new TypeError("Invalid attempt to spread non-iterable instance");
  }

  /**
   * Default configuration options of fontIconPicker
   */

  var options = {
    theme: 'fip-grey',
    // The CSS theme to use with this fontIconPicker. You can set different themes on multiple elements on the same page
    source: false,
    // Icons source (array|false|object)
    emptyIcon: true,
    // Empty icon should be shown?
    emptyIconValue: '',
    // The value of the empty icon, change if you select has something else, say "none"
    autoClose: true,
    // Whether or not to close the FIP automatically when clicked outside
    iconsPerPage: 20,
    // Number of icons per page
    hasSearch: true,
    // Is search enabled?
    searchSource: false,
    // Give a manual search values. If using attributes then for proper search feature we also need to pass icon names under the same order of source
    appendTo: 'self',
    // Where to append the selector popup. You can pass string selectors or jQuery objects
    useAttribute: false,
    // Whether to use attribute selector for printing icons
    attributeName: 'data-icon',
    // HTML Attribute name
    convertToHex: true,
    // Whether or not to convert to hexadecimal for attribute value. If true then please pass decimal integer value to the source (or as value="" attribute of the select field)
    allCategoryText: 'From all categories',
    // The text for the select all category option
    unCategorizedText: 'Uncategorized',
    // The text for the select uncategorized option
    iconGenerator: null,
    // Icon Generator function. Passes, item, flipBoxTitle and index
    windowDebounceDelay: 150,
    // Debounce delay while fixing position on windowResize
    searchPlaceholder: 'Search Icons' // Placeholder for the search input

  };

  /**
  * Implementation of debounce function
  *
  * {@link https://medium.com/a-developers-perspective/throttling-and-debouncing-in-javascript-b01cad5c8edf}
  * @param {Function} func callback function
  * @param {int} delay delay in milliseconds
  */
  var debounce = function debounce(func, delay) {
    var inDebounce;
    return function () {
      var context = this;
      var args = arguments;
      clearTimeout(inDebounce);
      inDebounce = setTimeout(function () {
        return func.apply(context, args);
      }, delay);
    };
  };

  var $ = jQuery; // A guid for implementing namespaced event

  var guid = 0;

  function FontIconPicker(element, options$$1) {
    this.element = $(element);
    this.settings = $.extend({}, options, options$$1);

    if (this.settings.emptyIcon) {
      this.settings.iconsPerPage--;
    }

    this.iconPicker = $('<div/>', {
      class: 'icons-selector',
      style: 'position: relative',
      html: this._getPickerTemplate(),
      attr: {
        'data-fip-origin': this.element.attr('id')
      }
    });
    this.iconContainer = this.iconPicker.find('.fip-icons-container');
    this.searchIcon = this.iconPicker.find('.selector-search i');
    this.selectorPopup = this.iconPicker.find('.selector-popup-wrap');
    this.selectorButton = this.iconPicker.find('.selector-button');
    this.iconsSearched = [];
    this.isSearch = false;
    this.totalPage = 1;
    this.currentPage = 1;
    this.currentIcon = false;
    this.iconsCount = 0;
    this.open = false;
    this.guid = guid++;
    this.eventNameSpace = ".fontIconPicker".concat(guid); // Set the default values for the search related variables

    this.searchValues = [];
    this.availableCategoriesSearch = []; // The trigger event for change

    this.triggerEvent = null; // Backups

    this.backupSource = [];
    this.backupSearch = []; // Set the default values of the category related variables

    this.isCategorized = false; // Automatically detects if the icon listing is categorized

    this.selectCategory = this.iconPicker.find('.icon-category-select'); // The category SELECT input field

    this.selectedCategory = false; // false means all categories are selected

    this.availableCategories = []; // Available categories, it is a two dimensional array which holds categorized icons

    this.unCategorizedKey = null; // Key of the uncategorized category
    // Initialize plugin

    this.init();
  }

  FontIconPicker.prototype = {
    /**
     * Init
     */
    init: function init() {
      // Add the theme CSS to the iconPicker
      this.iconPicker.addClass(this.settings.theme); // To properly calculate iconPicker height and width
      // We will first append it to body (with left: -9999px so that it is not visible)

      this.iconPicker.css({
        left: -9999
      }).appendTo('body');
      var iconPickerHeight = this.iconPicker.outerHeight(),
          iconPickerWidth = this.iconPicker.outerWidth(); // Now reset the iconPicker CSS

      this.iconPicker.css({
        left: ''
      }); // Add the icon picker after the select

      this.element.before(this.iconPicker); // Hide source element
      // Instead of doing a display:none, we would rather
      // make the element invisible
      // and adjust the margin

      this.element.css({
        visibility: 'hidden',
        top: 0,
        position: 'relative',
        zIndex: '-1',
        left: '-' + iconPickerWidth + 'px',
        display: 'inline-block',
        height: iconPickerHeight + 'px',
        width: iconPickerWidth + 'px',
        // Reset all margin, border and padding
        padding: '0',
        margin: '0 -' + iconPickerWidth + 'px 0 0',
        // Left margin adjustment to account for dangling space
        border: '0 none',
        verticalAlign: 'top',
        float: 'none' // Fixes positioning with floated elements

      }); // Set the trigger event

      if (!this.element.is('select')) {
        // Drop IE9 support and use the standard input event
        this.triggerEvent = 'input';
      } // If current element is SELECT populate settings.source


      if (!this.settings.source && this.element.is('select')) {
        // Populate data from select
        this._populateSourceFromSelect(); // Normalize the given source

      } else {
        this._initSourceIndex();
      } // load the categories


      this._loadCategories(); // Load icons


      this._loadIcons(); // Initialize dropdown button


      this._initDropDown(); // Category changer


      this._initCategoryChanger(); // Pagination


      this._initPagination(); // Icon Search


      this._initIconSearch(); // Icon Select


      this._initIconSelect();
      /**
       * On click out
       * Add the functionality #9
       * {@link https://github.com/micc83/fontIconPicker/issues/9}
       */


      this._initAutoClose(); // Window resize fix


      this._initFixOnResize();
    },

    /**
     * Set icons after the fip has been initialized
     */
    setIcons: function setIcons(newIcons, iconSearch) {
      this.settings.source = Array.isArray(newIcons) ? _toConsumableArray(newIcons) : $.extend({}, newIcons);
      this.settings.searchSource = Array.isArray(iconSearch) ? _toConsumableArray(iconSearch) : $.extend({}, iconSearch);

      this._initSourceIndex();

      this._loadCategories();

      this._resetSearch();

      this._loadIcons();
    },

    /**
     * Set currently selected icon programmatically
     *
     * @param {string} theIcon current icon value
     */
    setIcon: function setIcon() {
      var theIcon = arguments.length > 0 && arguments[0] !== undefined ? arguments[0] : '';

      this._setSelectedIcon(theIcon);
    },

    /**
     * Destroy picker and all events
     */
    destroy: function destroy() {
      this.iconPicker.off().remove();
      this.element.css({
        visibility: '',
        top: '',
        position: '',
        zIndex: '',
        left: '',
        display: '',
        height: '',
        width: '',
        padding: '',
        margin: '',
        border: '',
        verticalAlign: '',
        float: ''
      }); // Remove the delegated events

      $(window).off('resize' + this.eventNameSpace);
      $('html').off('click' + this.eventNameSpace);
    },

    /**
     * Manually reset position
     */
    resetPosition: function resetPosition() {
      this._fixOnResize();
    },

    /**
     * Manually set page
     * @param {int} pageNum
     */
    setPage: function setPage(pageNum) {
      if ('first' == pageNum) {
        pageNum = 1;
      }

      if ('last' == pageNum) {
        pageNum = this.totalPage;
      }

      pageNum = parseInt(pageNum, 10);

      if (isNaN(pageNum)) {
        pageNum = 1;
      }

      if (pageNum > this.totalPage) {
        pageNum = this.totalPage;
      }

      if (1 > pageNum) {
        pageNum = 1;
      }

      this.currentPage = pageNum;

      this._renderIconContainer();
    },

    /**
     * Initialize Fix on window resize with debouncing
     * This helps reduce function call unnecessary times.
     */
    _initFixOnResize: function _initFixOnResize() {
      var _this = this;

      $(window).on('resize' + this.eventNameSpace, debounce(function () {
        _this._fixOnResize();
      }, this.settings.windowDebounceDelay));
    },

    /**
     * Initiate autoClosing
     *
     * Checks for settings, and if set to yes, then autocloses the dropdown
     */
    _initAutoClose: function _initAutoClose() {
      var _this2 = this;

      if (this.settings.autoClose) {
        $('html').on('click' + this.eventNameSpace, function (event) {
          // Check if event is coming from selector popup or icon picker
          var target = event.target;

          if (_this2.selectorPopup.has(target).length || _this2.selectorPopup.is(target) || _this2.iconPicker.has(target).length || _this2.iconPicker.is(target)) {
            // Return
            return;
          } // Close it


          if (_this2.open) {
            _this2._toggleIconSelector();
          }
        });
      }
    },

    /**
     * Select Icon
     */
    _initIconSelect: function _initIconSelect() {
      var _this3 = this;

      this.selectorPopup.on('click', '.fip-box', function (e) {
        var fipBox = $(e.currentTarget);

        _this3._setSelectedIcon(fipBox.attr('data-fip-value'));

        _this3._toggleIconSelector();
      });
    },

    /**
     * Initiate realtime icon search
     */
    _initIconSearch: function _initIconSearch() {
      var _this4 = this;

      this.selectorPopup.on('input', '.icons-search-input', function (e) {
        // Get the search string
        var searchString = $(e.currentTarget).val(); // If the string is not empty

        if ('' === searchString) {
          _this4._resetSearch();

          return;
        } // Set icon search to X to reset search


        _this4.searchIcon.removeClass('fip-icon-search');

        _this4.searchIcon.addClass('fip-icon-cancel'); // Set this as a search


        _this4.isSearch = true; // Reset current page

        _this4.currentPage = 1; // Actual search
        // This has been modified to search the searchValues instead
        // Then return the value from the source if match is found

        _this4.iconsSearched = [];
        $.grep(_this4.searchValues, function (n, i) {
          if (0 <= n.toLowerCase().search(searchString.toLowerCase())) {
            _this4.iconsSearched[_this4.iconsSearched.length] = _this4.settings.source[i];
            return true;
          }
        }); // Render icon list

        _this4._renderIconContainer();
      });
      /**
      * Quit search
      */
      // Quit search happens only if clicked on the cancel button

      this.selectorPopup.on('click', '.selector-search .fip-icon-cancel', function () {
        _this4.selectorPopup.find('.icons-search-input').focus();

        _this4._resetSearch();
      });
    },

    /**
     * Initiate Pagination
     */
    _initPagination: function _initPagination() {
      var _this5 = this;

      /**
      * Next page
      */
      this.selectorPopup.on('click', '.selector-arrow-right', function (e) {
        if (_this5.currentPage < _this5.totalPage) {
          _this5.currentPage = _this5.currentPage + 1;

          _this5._renderIconContainer();
        }
      });
      /**
      * Prev page
      */

      this.selectorPopup.on('click', '.selector-arrow-left', function (e) {
        if (1 < _this5.currentPage) {
          _this5.currentPage = _this5.currentPage - 1;

          _this5._renderIconContainer();
        }
      });
    },

    /**
     * Initialize category changer dropdown
     */
    _initCategoryChanger: function _initCategoryChanger() {
      var _this6 = this;

      // Since the popup can be appended anywhere
      // We will add the event listener to the popup
      // And will stop the eventPropagation on click
      // @since v2.1.0
      this.selectorPopup.on('change keyup', '.icon-category-select', function (e) {
        // Don't do anything if not categorized
        if (false === _this6.isCategorized) {
          return false;
        }

        var targetSelect = $(e.currentTarget),
            currentCategory = targetSelect.val(); // Check if all categories are selected

        if ('all' === targetSelect.val()) {
          // Restore from the backups
          // @note These backups must be rebuild on source change, otherwise it will lead to error
          _this6.settings.source = _this6.backupSource;
          _this6.searchValues = _this6.backupSearch; // No? So there is a specified category
        } else {
          var key = parseInt(currentCategory, 10);

          if (_this6.availableCategories[key]) {
            _this6.settings.source = _this6.availableCategories[key];
            _this6.searchValues = _this6.availableCategoriesSearch[key];
          }
        }

        _this6._resetSearch();

        _this6._loadIcons();
      });
    },

    /**
     * Initialize Dropdown button
     */
    _initDropDown: function _initDropDown() {
      var _this7 = this;

      this.selectorButton.on('click', function (event) {
        // Open/Close the icon picker
        _this7._toggleIconSelector();
      });
    },

    /**
     * Get icon Picker Template String
     */
    _getPickerTemplate: function _getPickerTemplate() {
      var pickerTemplate = "\n<div class=\"selector\" data-fip-origin=\"".concat(this.element.attr('id'), "\">\n\t<span class=\"selected-icon\">\n\t\t<i class=\"fip-icon-block\"></i>\n\t</span>\n\t<span class=\"selector-button\">\n\t\t<i class=\"fip-icon-down-dir\"></i>\n\t</span>\n</div>\n<div class=\"selector-popup-wrap\" data-fip-origin=\"").concat(this.element.attr('id'), "\">\n\t<div class=\"selector-popup\" style=\"display: none;\"> ").concat(this.settings.hasSearch ? "<div class=\"selector-search\">\n\t\t\t<input type=\"text\" name=\"\" value=\"\" placeholder=\"".concat(this.settings.searchPlaceholder, "\" class=\"icons-search-input\"/>\n\t\t\t<i class=\"fip-icon-search\"></i>\n\t\t</div>") : '', "\n\t\t<div class=\"selector-category\">\n\t\t\t<select name=\"\" class=\"icon-category-select\" style=\"display: none\"></select>\n\t\t</div>\n\t\t<div class=\"fip-icons-container\"></div>\n\t\t<div class=\"selector-footer\" style=\"display:none;\">\n\t\t\t<span class=\"selector-pages\">1/2</span>\n\t\t\t<span class=\"selector-arrows\">\n\t\t\t\t<span class=\"selector-arrow-left\" style=\"display:none;\">\n\t\t\t\t\t<i class=\"fip-icon-left-dir\"></i>\n\t\t\t\t</span>\n\t\t\t\t<span class=\"selector-arrow-right\">\n\t\t\t\t\t<i class=\"fip-icon-right-dir\"></i>\n\t\t\t\t</span>\n\t\t\t</span>\n\t\t</div>\n\t</div>\n</div>");
      return pickerTemplate;
    },

    /**
     * Init the source & search index from the current settings
     * @return {void}
     */
    _initSourceIndex: function _initSourceIndex() {
      // First check for any sorts of errors
      if ('object' !== _typeof(this.settings.source)) {
        return;
      } // We are going to check if the passed source is an array or an object
      // If it is an array, then don't do anything
      // otherwise it has to be an object and therefore is it a categorized icon set


      if (Array.isArray(this.settings.source)) {
        // This is not categorized since it is 1D array
        this.isCategorized = false;
        this.selectCategory.html('').hide(); // We are going to convert the source items to string
        // This is necessary because passed source might not be "strings" for attribute related icons

        this.settings.source = $.map(this.settings.source, function (e, i) {
          if ('function' == typeof e.toString) {
            return e.toString();
          } else {
            return e;
          }
        }); // Now update the search
        // First check if the search is given by user

        if (Array.isArray(this.settings.searchSource)) {
          // Convert everything inside the searchSource to string
          this.searchValues = $.map(this.settings.searchSource, function (e, i) {
            if ('function' == typeof e.toString) {
              return e.toString();
            } else {
              return e;
            }
          }); // Clone the searchSource
          // Not given so use the source instead
        } else {
          this.searchValues = this.settings.source.slice(0); // Clone the source
        } // Categorized icon set

      } else {
        var originalSource = $.extend(true, {}, this.settings.source); // Reset the source

        this.settings.source = []; // Reset other variables

        this.searchValues = [];
        this.availableCategoriesSearch = [];
        this.selectedCategory = false;
        this.availableCategories = [];
        this.unCategorizedKey = null; // Set the categorized to true and reset the HTML

        this.isCategorized = true;
        this.selectCategory.html(''); // Now loop through the source and add to the list

        for (var categoryLabel in originalSource) {
          // Get the key of the new category array
          var thisCategoryKey = this.availableCategories.length,
              // Create the new option for the selectCategory SELECT field
          categoryOption = $('<option />'); // Set the value to this categorykey

          categoryOption.attr('value', thisCategoryKey); // Set the label

          categoryOption.html(categoryLabel); // Append to the DOM

          this.selectCategory.append(categoryOption); // Init the availableCategories array

          this.availableCategories[thisCategoryKey] = [];
          this.availableCategoriesSearch[thisCategoryKey] = []; // Now loop through it's icons and add to the list

          for (var newIconKey in originalSource[categoryLabel]) {
            // Get the new icon value
            var newIconValue = originalSource[categoryLabel][newIconKey]; // Get the label either from the searchSource if set, otherwise from the source itself

            var newIconLabel = this.settings.searchSource && this.settings.searchSource[categoryLabel] && this.settings.searchSource[categoryLabel][newIconKey] ? this.settings.searchSource[categoryLabel][newIconKey] : newIconValue; // Try to convert to the source value string
            // This is to avoid attribute related icon sets
            // Where hexadecimal or decimal numbers might be passed

            if ('function' == typeof newIconValue.toString) {
              newIconValue = newIconValue.toString();
            } // Check if the option element has value and this value does not equal to the empty value


            if (newIconValue && newIconValue !== this.settings.emptyIconValue) {
              // Push to the source, because at first all icons are selected
              this.settings.source.push(newIconValue); // Push to the availableCategories child array

              this.availableCategories[thisCategoryKey].push(newIconValue); // Push to the search values

              this.searchValues.push(newIconLabel);
              this.availableCategoriesSearch[thisCategoryKey].push(newIconLabel);
            }
          }
        }
      } // Clone and backup the original source and search


      this.backupSource = this.settings.source.slice(0);
      this.backupSearch = this.searchValues.slice(0);
    },

    /**
     * Populate source from select element
     * Check if select has optgroup, if so, then we are dealing with categorized
     * data. Otherwise, plain data.
     */
    _populateSourceFromSelect: function _populateSourceFromSelect() {
      var _this8 = this;

      // Reset the source and searchSource
      // These will be populated according to the available options
      this.settings.source = [];
      this.settings.searchSource = []; // Check if optgroup is present within the select
      // If it is present then the source has to be grouped

      if (this.element.find('optgroup').length) {
        // Set the categorized to true
        this.isCategorized = true;
        this.element.find('optgroup').each(function (i, el) {
          // Get the key of the new category array
          var thisCategoryKey = _this8.availableCategories.length,
              // Create the new option for the selectCategory SELECT field
          categoryOption = $('<option />'); // Set the value to this categorykey

          categoryOption.attr('value', thisCategoryKey); // Set the label

          categoryOption.html($(el).attr('label')); // Append to the DOM

          _this8.selectCategory.append(categoryOption); // Init the availableCategories array


          _this8.availableCategories[thisCategoryKey] = [];
          _this8.availableCategoriesSearch[thisCategoryKey] = []; // Now loop through it's option elements and add the icons

          $(el).find('option').each(function (i, cel) {
            var newIconValue = $(cel).val(),
                newIconLabel = $(cel).html(); // Check if the option element has value and this value does not equal to the empty value

            if (newIconValue && newIconValue !== _this8.settings.emptyIconValue) {
              // Push to the source, because at first all icons are selected
              _this8.settings.source.push(newIconValue); // Push to the availableCategories child array


              _this8.availableCategories[thisCategoryKey].push(newIconValue); // Push to the search values


              _this8.searchValues.push(newIconLabel);

              _this8.availableCategoriesSearch[thisCategoryKey].push(newIconLabel);
            }
          });
        }); // Additionally check for any first label option child

        if (this.element.find('> option').length) {
          this.element.find('> option').each(function (i, el) {
            var newIconValue = $(el).val(),
                newIconLabel = $(el).html(); // Don't do anything if the new icon value is empty

            if (!newIconValue || '' === newIconValue || newIconValue == _this8.settings.emptyIconValue) {
              return true;
            } // Set the uncategorized key if not set already


            if (null === _this8.unCategorizedKey) {
              _this8.unCategorizedKey = _this8.availableCategories.length;
              _this8.availableCategories[_this8.unCategorizedKey] = [];
              _this8.availableCategoriesSearch[_this8.unCategorizedKey] = []; // Create an option and append to the category selector

              $('<option />').attr('value', _this8.unCategorizedKey).html(_this8.settings.unCategorizedText).appendTo(_this8.selectCategory);
            } // Push the icon to the category


            _this8.settings.source.push(newIconValue);

            _this8.availableCategories[_this8.unCategorizedKey].push(newIconValue); // Push the icon to the search


            _this8.searchValues.push(newIconLabel);

            _this8.availableCategoriesSearch[_this8.unCategorizedKey].push(newIconLabel);
          });
        } // Not categorized

      } else {
        this.element.find('option').each(function (i, el) {
          var newIconValue = $(el).val(),
              newIconLabel = $(el).html();

          if (newIconValue) {
            _this8.settings.source.push(newIconValue);

            _this8.searchValues.push(newIconLabel);
          }
        });
      } // Clone and backup the original source and search


      this.backupSource = this.settings.source.slice(0);
      this.backupSearch = this.searchValues.slice(0);
    },

    /**
     * Load Categories
     * @return {void}
     */
    _loadCategories: function _loadCategories() {
      // Dont do anything if it is not categorized
      if (false === this.isCategorized) {
        return;
      } // Now append all to the category selector


      $('<option value="all">' + this.settings.allCategoryText + '</option>').prependTo(this.selectCategory); // Show it and set default value to all categories

      this.selectCategory.show().val('all').trigger('change');
    },

    /**
     * Load icons
     */
    _loadIcons: function _loadIcons() {
      // Set the content of the popup as loading
      this.iconContainer.html('<i class="fip-icon-spin3 animate-spin loading"></i>'); // If source is set

      if (Array.isArray(this.settings.source)) {
        // Render icons
        this._renderIconContainer();
      }
    },

    /**
     * Generate icons
     *
     * Supports hookable third-party renderer function.
     */
    _iconGenerator: function _iconGenerator(icon) {
      if ('function' === typeof this.settings.iconGenerator) {
        return this.settings.iconGenerator(icon);
      }

      return '<i ' + (this.settings.useAttribute ? this.settings.attributeName + '="' + (this.settings.convertToHex ? '&#x' + parseInt(icon, 10).toString(16) + ';' : icon) + '"' : 'class="' + icon + '"') + '></i>';
    },

    /**
     * Render icons inside the popup
     */
    _renderIconContainer: function _renderIconContainer() {
      var _this9 = this;

      var offset,
          iconsPaged = [];
   // Set a temporary array for icons

      if (this.isSearch) {
        iconsPaged = this.iconsSearched;
      } else {
        iconsPaged = this.settings.source;
      } // Count elements


      this.iconsCount = iconsPaged.length; // Calculate total page number

      this.totalPage = Math.ceil(this.iconsCount / this.settings.iconsPerPage); // Hide footer if no pagination is needed

      if (1 < this.totalPage) {
        this.selectorPopup.find('.selector-footer').show(); // Reset the pager buttons
        // Fix #8 {@link https://github.com/micc83/fontIconPicker/issues/8}
        // It is better to set/hide the pager button here
        // instead of all other functions that calls back _renderIconContainer

        if (this.currentPage < this.totalPage) {
          // current page is less than total, so show the arrow right
          this.selectorPopup.find('.selector-arrow-right').show();
        } else {
          // else hide it
          this.selectorPopup.find('.selector-arrow-right').hide();
        }

        if (1 < this.currentPage) {
          // current page is greater than one, so show the arrow left
          this.selectorPopup.find('.selector-arrow-left').show();
        } else {
          // else hide it
          this.selectorPopup.find('.selector-arrow-left').hide();
        }
      } else {
        this.selectorPopup.find('.selector-footer').hide();
      } // Set the text for page number index and total icons


      this.selectorPopup.find('.selector-pages').html(this.currentPage + '/' + this.totalPage + ' <em>(' + this.iconsCount + ')</em>'); // Set the offset for slice

      offset = (this.currentPage - 1) * this.settings.iconsPerPage; // Should empty icon be shown?

      if (this.settings.emptyIcon) {
        // Reset icon container HTML and prepend empty icon
        this.iconContainer.html('<span class="fip-box" data-fip-value="fip-icon-block"><i class="fip-icon-block"></i></span>'); // If not show an error when no icons are found
      } else if (1 > iconsPaged.length) {
        this.iconContainer.html('<span class="icons-picker-error" data-fip-value="fip-icon-block"><i class="fip-icon-block"></i></span>');
        return; // else empty the container
      } else {
        this.iconContainer.html('');
      } // Set an array of current page icons


      iconsPaged = iconsPaged.slice(offset, offset + this.settings.iconsPerPage); // List icons

      var _loop = function _loop(i, icon) {
        // eslint-disable-line
        // Set the icon title
        var fipBoxTitle = icon;
        $.grep(_this9.settings.source, $.proxy(function (e, i) {
          if (e === icon) {
            fipBoxTitle = this.searchValues[i];
            return true;
          }

          return false;
        }, _this9)); // Set the icon box

        $('<span/>', {
          html: _this9._iconGenerator(icon),
          attr: {
            'data-fip-value': icon
          },
          class: 'fip-box',
          title: fipBoxTitle
        }).appendTo(_this9.iconContainer);
      };

      for (var i = 0, icon; icon = iconsPaged[i++];) {
        _loop(i, icon);
      } // If no empty icon is allowed and no current value is set or current value is not inside the icon set


      if (!this.settings.emptyIcon && (!this.element.val() || -1 === $.inArray(this.element.val(), this.settings.source))) {
        // Get the first icon
        this._setSelectedIcon(iconsPaged[0]);
      } else if (-1 === $.inArray(this.element.val(), this.settings.source)) {
        // Issue #7
        // Need to pass empty string
        // Set empty
        // Otherwise DOM will be set to null value
        // which would break the initial select value
        this._setSelectedIcon('');
      } else {
        // Fix issue #7
        // The trick is to check the element value
        // Internally fip-icon-block must be used for empty values
        // So if element.val == emptyIconValue then pass fip-icon-block
        var passDefaultIcon = this.element.val();

        if (passDefaultIcon === this.settings.emptyIconValue) {
          passDefaultIcon = 'fip-icon-block';
        } // Set the default selected icon even if not set


        this._setSelectedIcon(passDefaultIcon);
      }
    },

    /**
     * Set Highlighted icon
     */
    _setHighlightedIcon: function _setHighlightedIcon() {
      this.iconContainer.find('.current-icon').removeClass('current-icon');

      if (this.currentIcon) {
        this.iconContainer.find('[data-fip-value="' + this.currentIcon + '"]').addClass('current-icon');
      }
    },

    /**
     * Set selected icon
     *
     * @param {string} theIcon
     */
    _setSelectedIcon: function _setSelectedIcon(theIcon) {
      if ('fip-icon-block' === theIcon) {
        theIcon = '';
      }

      var selectedIcon = this.iconPicker.find('.selected-icon'); // if the icon is empty, then reset to empty

      if ('' === theIcon) {
        selectedIcon.html('<i class="fip-icon-block"></i>');
      } else {
        // Pass it to the render function
        selectedIcon.html(this._iconGenerator(theIcon));
      } // Check if actually changing the DOM element


      var currentValue = this.element.val(); // Set the value of the element

      this.element.val('' === theIcon ? this.settings.emptyIconValue : theIcon); // trigger event if change has actually occured

      if (currentValue !== theIcon) {
        this.element.trigger('change');

        if (null !== this.triggerEvent) {
          this.element.trigger(this.triggerEvent);
        }
      }

      this.currentIcon = theIcon;

      this._setHighlightedIcon();
    },

    /**
     * Recalculate the position of the Popup
     */
    _repositionIconSelector: function _repositionIconSelector() {
      // Calculate the position + width
      var offset = this.iconPicker.offset(),
          offsetTop = offset.top + this.iconPicker.outerHeight(true),
          offsetLeft = offset.left;
      this.selectorPopup.css({
        left: offsetLeft,
        top: offsetTop
      });
    },

    /**
     * Fix window overflow of popup dropdown if needed
     *
     * This can happen if appending to self or someplace else
     */
    _fixWindowOverflow: function _fixWindowOverflow() {
      // Adjust the offsetLeft
      // Resolves issue #10
      // @link https://github.com/micc83/fontIconPicker/issues/10
      var visibilityStatus = this.selectorPopup.find('.selector-popup').is(':visible');

      if (!visibilityStatus) {
        this.selectorPopup.find('.selector-popup').show();
      }

      var popupWidth = this.selectorPopup.outerWidth(),
          windowWidth = $(window).width(),
          popupOffsetLeft = this.selectorPopup.offset().left,
          containerOffset = 'self' == this.settings.appendTo ? this.selectorPopup.parent().offset() : $(this.settings.appendTo).offset();

      if (!visibilityStatus) {
        this.selectorPopup.find('.selector-popup').hide();
      }

      if (popupOffsetLeft + popupWidth > windowWidth - 20
      /* 20px adjustment for better appearance */
      ) {
          // First we try to position with right aligned
          var pickerOffsetRight = this.selectorButton.offset().left + this.selectorButton.outerWidth();
          var preferredLeft = Math.floor(pickerOffsetRight - popupWidth - 1);
          /** 1px adjustment for sub-pixels */
          // If preferredLeft would put the popup out of window from left
          // then don't do it

          if (0 > preferredLeft) {
            this.selectorPopup.css({
              left: windowWidth - 20 - popupWidth - containerOffset.left
            });
          } else {
            // Put it in the preferred position
            this.selectorPopup.css({
              left: preferredLeft
            });
          }
        }
    },

    /**
     * Fix on Window Resize
     */
    _fixOnResize: function _fixOnResize() {
      // If the appendTo is not self, then we need to reposition the dropdown
      if ('self' !== this.settings.appendTo) {
        this._repositionIconSelector();
      } // In any-case, we need to fix for window overflow


      this._fixWindowOverflow();
    },

    /**
     * Open/close popup (toggle)
     */
    _toggleIconSelector: function _toggleIconSelector() {
      this.open = !this.open ? 1 : 0; // Append the popup if needed

      if (this.open) {
        // Check the origin
        if ('self' !== this.settings.appendTo) {
          // Append to the selector and set the CSS + theme
          this.selectorPopup.appendTo(this.settings.appendTo).css({
            zIndex: 1000 // Let's decrease the zIndex to something reasonable

          }).addClass('icons-selector ' + this.settings.theme); // call resize()

          this._repositionIconSelector();
        } // Fix positioning if needed


        this._fixWindowOverflow();
      }

      this.selectorPopup.find('.selector-popup').slideToggle(300, $.proxy(function () {
        this.iconPicker.find('.selector-button i').toggleClass('fip-icon-down-dir');
        this.iconPicker.find('.selector-button i').toggleClass('fip-icon-up-dir');

        if (this.open) {
          this.selectorPopup.find('.icons-search-input').trigger('focus').trigger('select');
        } else {
          // append and revert to the original position and reset theme
          this.selectorPopup.appendTo(this.iconPicker).css({
            left: '',
            top: '',
            zIndex: ''
          }).removeClass('icons-selector ' + this.settings.theme);
        }
      }, this));
    },

    /**
     * Reset search
     */
    _resetSearch: function _resetSearch() {
      // Empty input
      this.selectorPopup.find('.icons-search-input').val(''); // Reset search icon class

      this.searchIcon.removeClass('fip-icon-cancel');
      this.searchIcon.addClass('fip-icon-search'); // Go back to page 1

      this.currentPage = 1;
      this.isSearch = false; // Rerender icons

      this._renderIconContainer();
    }
  }; // ES6 Export it as module

  /**
   * Light weight wrapper to inject fontIconPicker
   * into jQuery.fn
   */

  function fontIconPickerShim($) {
    // Do not init if jQuery doesn't have needed stuff
    if (!$.fn) {
      return false;
    } // save from double init


    if ($.fn && $.fn.fontIconPicker) {
      return true;
    }

    $.fn.fontIconPicker = function (options) {
      var _this = this;

      // Instantiate the plugin
      this.each(function () {
        if (!$.data(this, 'fontIconPicker')) {
          $.data(this, 'fontIconPicker', new FontIconPicker(this, options));
        }
      }); // setIcons method

      this.setIcons = function () {
        var newIcons = arguments.length > 0 && arguments[0] !== undefined ? arguments[0] : false;
        var iconSearch = arguments.length > 1 && arguments[1] !== undefined ? arguments[1] : false;

        _this.each(function () {
          $.data(this, 'fontIconPicker').setIcons(newIcons, iconSearch);
        });
      }; // setIcon method


      this.setIcon = function () {
        var newIcon = arguments.length > 0 && arguments[0] !== undefined ? arguments[0] : '';

        _this.each(function () {
          $.data(this, 'fontIconPicker').setIcon(newIcon);
        });
      }; // destroy method


      this.destroyPicker = function () {
        _this.each(function () {
          if (!$.data(this, 'fontIconPicker')) {
            return;
          } // Remove the iconPicker


          $.data(this, 'fontIconPicker').destroy(); // destroy data

          $.removeData(this, 'fontIconPicker');
        });
      }; // reInit method


      this.refreshPicker = function (newOptions) {
        if (!newOptions) {
          newOptions = options;
        } // First destroy


        _this.destroyPicker(); // Now reset


        _this.each(function () {
          if (!$.data(this, 'fontIconPicker')) {
            $.data(this, 'fontIconPicker', new FontIconPicker(this, newOptions));
          }
        });
      }; // reposition method


      this.repositionPicker = function () {
        _this.each(function () {
          $.data(this, 'fontIconPicker').resetPosition();
        });
      }; // set page


      this.setPage = function (pageNum) {
        _this.each(function () {
          $.data(this, 'fontIconPicker').setPage(pageNum);
        });
      };

      return this;
    };

    return true;
  }

  function initFontIconPicker(jQuery$$1) {
    return fontIconPickerShim(jQuery$$1);
  }

  /**
   *  jQuery fontIconPicker
   *
   *  An icon picker built on top of font icons and jQuery
   *
   *  http://codeb.it/fontIconPicker
   *
   *  Made by Alessandro Benoit & Swashata
   *  Licensed under MIT License
   *
   * {@link https://github.com/micc83/fontIconPicker}
   */
  // In browser this will work
  // But in node environment it might not.
  // because if jQu

  if (jQuery && jQuery.fn) {
    initFontIconPicker(jQuery);
  } // Export the function anyway, so that it can be initiated
  // from node environment


  var jquery_fonticonpicker = (function (jQuery$$1) {
    return initFontIconPicker(jQuery$$1);
  });

  return jquery_fonticonpicker;

})));
//# sourceMappingURL=jquery.fonticonpicker.js.map
