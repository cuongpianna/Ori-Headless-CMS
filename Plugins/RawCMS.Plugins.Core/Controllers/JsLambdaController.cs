﻿//******************************************************************************
// <copyright file="license.md" company="RawCMS project  (https://github.com/arduosoft/RawCMS)">
// Copyright (c) 2019 RawCMS project  (https://github.com/arduosoft/RawCMS)
// RawCMS project is released under GPL3 terms, see LICENSE file on repository root at  https://github.com/arduosoft/RawCMS .
// </copyright>
// <author>Daniele Fontani, Emanuele Bucarelli, Francesco Mina'</author>
// <autogenerated>true</autogenerated>
//******************************************************************************
using Jint;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using RawCMS.Library.Core;
using RawCMS.Library.Core.Attributes;
using RawCMS.Library.JavascriptClient;
using RawCMS.Library.Service;
using System;
using System.Collections.Generic;

namespace RawCMS.Plugins.Core.Controllers
{
    [AllowAnonymous]
    [RawAuthentication]
    [Route("api/js")]
    public class JsLambdaController
    {
        private readonly AppEngine lambdaManager;
        private readonly CRUDService crudService;
        private readonly ILogger logger;

        public JsLambdaController(AppEngine lambdaManager, CRUDService crudService, ILogger logger)
        {
            this.lambdaManager = lambdaManager;
            this.crudService = crudService;
            this.logger = logger;
        }

        [AllowAnonymous]
        [RawAuthentication]
        [HttpPost("{lambda}")]
        public JObject Post(string lambda, [FromBody] JObject input)
        {
            Library.DataModel.ItemList result = crudService.Query("_js", new Library.DataModel.DataQuery()
            {
                PageNumber = 1,
                PageSize = 1,
                RawQuery = $"{{\"Path\":\"{lambda}\"}}"
            });

            JToken js = result.Items[0];
            string code = js["Code"].ToString();

            Dictionary<string, object> tmpIn = input.ToObject<Dictionary<string, object>>();
            Dictionary<string, object> tmpOut = new Dictionary<string, object>();

            Engine engine = new Engine((x) => { x.AllowClr(typeof(JavascriptRestClient).Assembly); x.AllowClr(typeof(JavascriptRestClientRequest).Assembly); });

            engine.SetValue("input", tmpIn);
            engine.SetValue("RAWCMSRestClient", Jint.Runtime.Interop.TypeReference.CreateTypeReference(engine, typeof(JavascriptRestClient)));
            engine.SetValue("RAWCMSRestClientRequest", Jint.Runtime.Interop.TypeReference.CreateTypeReference(engine, typeof(JavascriptRestClientRequest)));
            engine.SetValue("RAWCMSCrudService", crudService);

            engine.SetValue("output", tmpOut);

            try
            {
                logger.LogDebug($"calling lambda: {lambda}");
                engine.Execute(code);
            }
            catch (Exception e)
            {
                logger.LogError($"Error on lambda javascript script: {e.Message} ");
                tmpOut.Add("Error", e.Message);
            }
            logger.LogDebug($"Lambda response: {tmpOut}");
            return JObject.FromObject(tmpOut);
        }
    }
}