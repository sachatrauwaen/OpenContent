<template>
  <control v-bind="props" v-slot="flags">
    <input
      type="text"
      class="form-control"
      :aria-describedby="options.label"
      v-model="model"
      :class="{'is-invalid':flags.invalid && flags.touched}"
      :placeholder="options.placeholder"
    />
  </control>
</template>

<script>
import {Control, ControlField} from "lamavue";

let TextField = {
  name: "HelloWorldField",
  extends: ControlField,
  props: {
    value: {
      type: String
    }
  },
  computed: {},
  methods: {},
  components: { Control },
  builder: {
    props() {
      return {
        schema: {
          type: "object",
          properties: {
            required: {              
              type: "boolean"
            },
            placeholder: {
              title: "Placeholder",
              type: "string"
            },
            multilanguage: {
              title: "Multi language",
              type: "boolean"
            }
          }
        },
        options: {
          fields: {
            required: {
              rightLabel: "Required"
            }
          }
        }
      };
    },
    fromBuilder(field) {
      return {
        schema: {
          title: field.label || 'helloword' ,
          type: "string",
          required: field.required
        },
        options: {
           type: "helloworld",
          placeholder: field.placeholder,
          multilanguage: field.multilanguage
        }
      };
    },
    toBuilder(def) {
      return {
        label: def.schema.title,
        fieldType: "helloworld",
        required: def.schema.required,
        placeholder: def.options.placeholder,
        multilanguage: def.options.multilanguage
      };
    }
  }
};

export default TextField;
</script>

<!-- Add "scoped" attribute to limit CSS to this component only -->
<style scoped>
</style>
