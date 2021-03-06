//******************************************************************************
// <copyright file="license.md" company="RawCMS project  (https://github.com/arduosoft/RawCMS)">
// Copyright (c) 2019 RawCMS project  (https://github.com/arduosoft/RawCMS)
// RawCMS project is released under GPL3 terms, see LICENSE file on repository root at  https://github.com/arduosoft/RawCMS .
// </copyright>
// <author>Daniele Fontani, Emanuele Bucarelli, Francesco Mina'</author>
// <autogenerated>true</autogenerated>
//******************************************************************************
using GraphQL;
using GraphQL.Http;
using GraphQL.Instrumentation;
using GraphQL.Types;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RawCMS.Library.Core.Attributes;
using RawCMS.Plugins.GraphQL.Classes;
using System;
using System.IO;
using System.Threading.Tasks;

namespace RawCMS.Plugins.GraphQL.Controllers
{
    [AllowAnonymous]
    [RawAuthentication]
    [Route("api/graphql")]
    public class GraphQLController : Controller
    {
        private readonly IDocumentExecuter _executer;
        private readonly IDocumentWriter _writer;
        private readonly GraphQLService _service;
        private readonly ISchema _schema;

        public GraphQLController(IDocumentExecuter executer,
            IDocumentWriter writer,
            GraphQLService graphQLService,
            ISchema schema)
        {
            _executer = executer;
            _writer = writer;
            _service = graphQLService;
            _schema = schema;
        }

        public static T Deserialize<T>(Stream s)
        {
            using (StreamReader reader = new StreamReader(s))
            using (JsonTextReader jsonReader = new JsonTextReader(reader))
            {
                JsonSerializer ser = new JsonSerializer();
                return ser.Deserialize<T>(jsonReader);
            }
        }

        [HttpPost]
        public async Task<ExecutionResult> Post([FromBody]GraphQLRequest request)
        {
            GraphQLRequest t = Deserialize<GraphQLRequest>(HttpContext.Request.Body);
            DateTime start = DateTime.UtcNow;

            ExecutionResult result = await _executer.ExecuteAsync(_ =>
            {
                _.Schema = _schema;
                _.Query = request.Query;
                _.OperationName = request.OperationName;
                _.Inputs = request.Variables.ToInputs();
                //_.UserContext = _service.Settings.BuildUserContext?.Invoke(HttpContext);
                _.EnableMetrics = _service.Settings.EnableMetrics;
                if (_service.Settings.EnableMetrics)
                {
                    _.FieldMiddleware.Use<InstrumentFieldsMiddleware>();
                }
            });

            if (_service.Settings.EnableMetrics)
            {
                result.EnrichWithApolloTracing(start);
            }

            return result;
        }
    }
}