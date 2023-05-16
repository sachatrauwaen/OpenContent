<script>
import { VueSelectBaseField } from "lamavue";

let CollectionField = {
        name: "CollectionField",
        extends: VueSelectBaseField,
        props: {},
        data() {
            return {};
        },
        computed: {
            query() {
                return {
                    type: "relation",
                    action: this.options.dataService.action,
                    collection: this.options.dataService.data.collection,
                    textField: this.options.dataService.data.textField,
                };
            },
        },
        methods: {
            // map(item) {
            //   return {
            //     value: item[this.options.dataService.data.valueField],
            //     text: item[this.options.dataService.data.textField],
            //   };
            // },
        },
        components: {},
        builder: {
            props() {
                return {
                    schema: {
                        type: "object",
                        properties: {
                            many: {
                                type: "boolean",
                            },
                            collection: {
                                title: "Collection",
                                type: "string",
                            },
                            textField: {
                                title: "TextField",
                                type: "string",
                            },
                            placeholder: {
                                title: "Placeholder",
                                type: "string",
                            },
                        },
                    },
                    options: {
                        fields: {
                            many: {
                                rightLabel: "Many",
                            },
                        },
                    },
                };
            },
            fromBuilder(field) {
                return {
                    schema: {
                        type: field.many ? "array" : "string",
                    },
                    options: {
                        type: "collection",
                        placeholder: field.placeholder,
                        dataService: {
                            action: "LookupCollection",
                            data: {
                                collection: field.collection,
                                textField: field.textField,
                            },
                        },
                    },
                };
            },
            toBuilder(def) {
                let builder = {
                    fieldType: "collection",
                    placeholder: def.options.placeholder,
                    many: def.schema.type == "array",
                };
                if (
                    def.options &&
                    def.options.dataService &&
                    def.options.dataService.data
                ) {
                    builder.collection = def.options.dataService.data.collection;
                    builder.textField = def.options.dataService.data.textField;
                }
                return builder;
            },
        },
};

export default CollectionField;
</script>

<!-- Add "scoped" attribute to limit CSS to this component only -->
<style scoped>
</style>