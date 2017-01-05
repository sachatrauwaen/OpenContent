// need a sf in the root app

Vue.component('opencontent-form', {
    template: '<div></div>',
    props: ['form', 'action'],
    mounted: function () {
        var vm = this;
        $(this.$el)
          .val(this.value)
          // init 
          .openContentForm({
              servicesFramework: vm.$root.$options.sf,
              form: this.form ? this.form : "form",
              action: this.action ? this.action : "FormSubmit",
              onRendered: function (control) {
                  vm.$emit('onRendered', control);
              },
              onSubmit: function (data) {
                  vm.$emit('onSubmit', data);
              },
              onSubmited: function (data) {
                  vm.$emit('onSubmited', data);
              }
          });
    },
    //watch: {
    //    value: function (value) {
    //        // update value
    //        $(this.$el).openContentForm().setDate(value);
    //    }
    //},
    destroyed: function () {
        $(this.$el).openContentForm().destroy();
    },
    methods: {
        submit: function (id) {
            $(this.$el).openContentForm().submit(id);
        },
        getField: function (field) {
            return $(this.$el).openContentForm().getField(field);
        },
        setData: function (data) {
            return $(this.$el).openContentForm().setData(data);
        }
    }
})