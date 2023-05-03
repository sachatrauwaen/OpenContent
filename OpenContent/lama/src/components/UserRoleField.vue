<script>
    import { VueSelectBaseField } from "lamavue";

    let UserRoleField = {
        name: "UserRoleField",
        extends: VueSelectBaseField,
        props: {},
        data() {
            return {};
        },
        computed: {
            query() {
                return {
                    type: "userrole"
                };
            },
        },
        methods: {
            map(item) {
                return item.text;
            },
            reduce(option) {
                return option;
            },
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
                        type: "userrole",
                        placeholder: field.placeholder,
                    },
                };
            },
            toBuilder(def) {
                let builder = {
                    fieldType: "userrole",
                    placeholder: def.options.placeholder,
                    many: def.schema.type == "array",
                };
                return builder;
            },
        },
    };

    export default UserRoleField;
</script>

<!-- Add "scoped" attribute to limit CSS to this component only -->
<style scoped>
</style>