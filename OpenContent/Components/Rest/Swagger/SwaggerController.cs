using DotNetNuke.Web.Api;
using Newtonsoft.Json;
using Satrabel.OpenContent.Components.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Satrabel.OpenContent.Components.Rest.Swagger
{
    [AllowAnonymous]
    public class SwaggerController : DnnApiController
    {
        public SwaggerController()
        {
            var json = GlobalConfiguration.Configuration.Formatters.JsonFormatter;
            //json.SerializerSettings.DateTimeZoneHandling = Newtonsoft.Json.DateTimeZoneHandling.Local;
            json.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
            json.SerializerSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter() { CamelCaseText = true });
            json.SerializerSettings.ContractResolver = new CamelCaseExceptDictionaryResolver();
            json.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Serialize;
            json.SerializerSettings.Formatting = Formatting.Indented;

            //GlobalConfiguration.Configuration.Formatters.XmlFormatter.SupportedMediaTypes.Clear();
        }

        [HttpGet]
        public HttpResponseMessage Json(int moduleId, int tabId)
        {
            try
            {
                var module = new OpenContentModuleInfo(moduleId, tabId);
                var manifest = module.Settings.Manifest;
                string templateFolder = module.Settings.TemplateDir.UrlFolder;

                var swagger = new SwaggerRoot();
                swagger.Info.Title = "OpenContent Rest API";
                swagger.Info.License = new License()
                {
                    Name = "MIT",
                    Url = "https://opencontent.codeplex.com/license"
                };
                swagger.Info.Version = "1.0";
                swagger.Host = PortalSettings.PortalAlias.HTTPAlias;
                swagger.BasePath = "/Desktopmodules/OpenContent/api/Rest/v1";
                swagger.Schemes = new List<Protocol>();
                swagger.Schemes.Add(Protocol.Http);
                swagger.Consumes.Add("application/json");
                swagger.Produces.Add("application/json");
                swagger.Tags.Add(new Tag()
                {
                    Name = "Items",
                    Description = manifest.Title
                });
                if (manifest.AdditionalDataDefinition != null)
                {
                    foreach (var entity in manifest.AdditionalDataDefinition)
                    {
                        swagger.Tags.Add(new Tag()
                        {
                            Name = entity.Key,
                            Description = entity.Value.Title
                        });
                    }
                }
                //swagger.Consumes.Add("text/html");
                //swagger.Produces.Add("text/html");

                var headers = new List<Parameter>();
                /*
                headers.Add(new Parameter()
                {
                    Name = "TabId",
                    In = Location.Header,
                    Description = "Tab Id",
                    Required = true,
                    Type = SchemaType.String,

                });
                headers.Add(new Parameter()
                {
                    Name = "ModuleId",
                    In = Location.Header,
                    Description = "Module Id",
                    Required = true,
                    Type = SchemaType.String,

                });
                */
                {
                    // main item
                    var schemaJson = JsonUtils.LoadJsonFromFile(templateFolder + "schema.json");

                    //var resItems = new List<SchemaObject>();
                    //resItems.Add(new SchemaObject()
                    //{
                    //    Ref = "#/definitions/items"
                    //});
                    
                    // Get()
                    // Get(pageIndex, pageSize, filter, sort)
                    var pi = new PathItem();

                    pi.Parameters = headers;
                    var getParams = new List<Parameter>();
                    getParams.Add(new Parameter()
                    {
                        Name = "pageIndex",
                        Description = "Page Index (start at 0)",
                        In = Location.Query,
                        Required = true,
                        Type = SchemaType.Number,
                        Default = 0
                    });
                    getParams.Add(new Parameter()
                    {
                        Name = "pageSize",
                        Description = "Page Size",
                        In = Location.Query,
                        Required = true,
                        Type = SchemaType.Number,
                        Default = 10
                    });
                    getParams.Add(new Parameter()
                    {
                        Name = "filter",
                        Description = "Filter definition (JSON.stringify)",
                        In = Location.Query,
                        Required = false,
                        Type = SchemaType.String
                    });
                    getParams.Add(new Parameter()
                    {
                        Name = "sort",
                        Description = "Sort definition (JSON.stringify)",
                        In = Location.Query,
                        Required = false,
                        Type = SchemaType.String
                    });
                    pi.Get = new Operation()
                    {
                        Summary = "Get main items",
                        Description = "No parameters for all items otherwise all parameters are required",
                        Parameters = getParams
                    };
                    pi.Get.Responses.Add("200", new Response()
                    {
                        Description = "succes",
                        Schema = new SchemaObject()
                        {
                            Type = SchemaType.Array,
                            Items = new SchemaObject()
                            {
                                Ref = "#/definitions/items"
                            }
                        }
                    });
                    pi.Get.Tags.Add("Items");
                    // Post()
                    var postProps = new Dictionary<string, SchemaObject>();
                    postProps.Add("item", new SchemaObject()
                    {
                        Ref = "#/definitions/items"
                    });
                    var postParams = new List<Parameter>();
                    postParams.Add(new Parameter()
                    {
                        Name = "body",
                        In = Location.Body,
                        Required = true,
                        Schema = new SchemaObject()
                        {
                            Type = SchemaType.Object,
                            Properties = postProps
                            //Ref = "#/definitions/items"
                        }
                    });

                    pi.Post = new Operation()
                    {
                        Summary = "Add new item",
                        Parameters = postParams
                    };
                    pi.Post.Responses.Add("200", new Response()
                    {
                        Description = "succes"
                    });
                    pi.Post.Tags.Add("Items");
                    swagger.Paths.Add("/items", pi);
                    // Get(id)
                    pi = new PathItem();
                    pi.Parameters = headers;
                    getParams = new List<Parameter>();
                    getParams.Add(new Parameter()
                    {
                        Name = "id",
                        In = Location.Path,
                        Required = true,
                        Type = SchemaType.String
                    });
                    pi.Get = new Operation()
                    {
                        Summary = "Get a item",
                        Parameters = getParams
                    };
                    pi.Get.Responses.Add("200", new Response()
                    {
                        Description = "succes",
                        Schema = new SchemaObject()
                        {
                            Type = SchemaType.Array,
                            Items = new SchemaObject()
                            {
                                Ref = "#/definitions/items"
                            }
                        }
                    });
                    pi.Get.Tags.Add("Items");
                    // Put(id) - update
                    var putProps = new Dictionary<string, SchemaObject>();
                    putProps.Add("item", new SchemaObject()
                    {
                        Ref = "#/definitions/items"
                    });
                    var putParams = new List<Parameter>();
                    putParams.Add(new Parameter()
                    {
                        Name = "id",
                        In = Location.Path,
                        Required = true,
                        Type = SchemaType.String
                    });
                    putParams.Add(new Parameter()
                    {
                        Name = "body",
                        In = Location.Body,
                        Required = true,
                        Schema = new SchemaObject()
                        {
                            Type = SchemaType.Object,
                            Properties = putProps
                            //Ref = "#/definitions/items"
                        }
                    });
                    pi.Put = new Operation()
                    {
                        Summary = "Update a item",
                        Parameters = putParams
                    };
                    pi.Put.Responses.Add("200", new Response()
                    {
                        Description = "succes"
                    });
                    pi.Put.Tags.Add("Items");
                    // Delete(id)
                    var deleteParams = new List<Parameter>();
                    deleteParams.Add(new Parameter()
                    {
                        Name = "id",
                        In = Location.Path,
                        Required = true,
                        Type = SchemaType.String
                    });
                    pi.Delete = new Operation()
                    {
                        Summary = "Delete a item",
                        Parameters = deleteParams
                    };
                    pi.Delete.Responses.Add("200", new Response()
                    {
                        Description = "succes",
                        Schema = new SchemaObject()
                        {
                            Type = SchemaType.Array,
                            Items = new SchemaObject()
                            {
                                Ref = "#/definitions/items"
                            }
                        }
                    });
                    pi.Delete.Tags.Add("Items");
                    swagger.Paths.Add("/items/{id}", pi);

                    // main item definition
                    var props = new Dictionary<string, SchemaObject>();
                    props.Add("Title", new SchemaObject()
                    {
                        Type = SchemaType.String
                    });
                    swagger.Definitions.Add("items", schemaJson);
                }

                if (manifest.AdditionalDataDefinition != null)
                {
                    foreach (var entity in manifest.AdditionalDataDefinition.Keys)
                    {
                        var schemaJson = JsonUtils.LoadJsonFromFile(templateFolder + entity + "-schema.json");
                        if (schemaJson["items"] != null)
                        {
                            var entityName = entity.ToLower();
                            var resItems = new List<SchemaObject>();
                            resItems.Add(new SchemaObject()
                            {
                                Ref = "#/definitions/" + entityName
                            });
                            var pi = new PathItem();
                            pi.Get = new Operation()
                            {
                                Summary = "Get all " + entity,
                                //Parameters = headers
                                OperationId = "get" + entity
                            };
                            pi.Get.Responses.Add("200", new Response()
                            {
                                Description = "succes",
                                Schema = new SchemaObject()
                                {
                                    Type = SchemaType.Array,
                                    Items = new SchemaObject()
                                    {
                                        Ref = "#/definitions/" + entityName
                                    }
                                }
                            });
                            pi.Parameters = headers;
                            pi.Get.Tags.Add(entity);
                            swagger.Paths.Add("/" + entity, pi);
                            swagger.Definitions.Add(entityName, schemaJson["items"]);
                        }
                    }
                }
                return Request.CreateResponse(HttpStatusCode.OK, swagger, Configuration.Formatters.JsonFormatter);

            }
            catch (Exception exc)
            {
                Log.Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }
    }
}