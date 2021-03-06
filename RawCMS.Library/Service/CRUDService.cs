//******************************************************************************
// <copyright file="license.md" company="RawCMS project  (https://github.com/arduosoft/RawCMS)">
// Copyright (c) 2019 RawCMS project  (https://github.com/arduosoft/RawCMS)
// RawCMS project is released under GPL3 terms, see LICENSE file on repository root at  https://github.com/arduosoft/RawCMS .
// </copyright>
// <author>Daniele Fontani, Emanuele Bucarelli, Francesco Mina'</author>
// <autogenerated>true</autogenerated>
//******************************************************************************
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Newtonsoft.Json.Linq;
using RawCMS.Library.Core;
using RawCMS.Library.Core.Enum;
using RawCMS.Library.Core.Exceptions;
using RawCMS.Library.DataModel;
using RawCMS.Library.Lambdas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace RawCMS.Library.Service
{
    public class CRUDService
    {
        private readonly MongoService _mongoService;
        private readonly MongoSettings _settings;
        private readonly List<string> collectionNames = new List<string>();
        private readonly AppEngine lambdaManager;

        private readonly JsonWriterSettings js = new JsonWriterSettings()
        {
            OutputMode = JsonOutputMode.Strict,
            GuidRepresentation = GuidRepresentation.CSharpLegacy
        };

        public CRUDService(MongoService mongoService, MongoSettings settings, AppEngine lambdaManager)
        {
            _mongoService = mongoService;
            _settings = settings;
            this.lambdaManager = lambdaManager;
            LoadCollectionNames();
        }

        private void LoadCollectionNames()
        {
            foreach (BsonDocument collection in _mongoService.GetDatabase().ListCollections().ToEnumerable())
            {
                collectionNames.Add(collection["name"].AsString);
            }
        }

        public JObject Insert(string collection, JObject newitem)
        {
            var dataContext = new Dictionary<string, object>();
            InvokeValidation(newitem, collection);

            EnsureCollection(collection);

            InvokeProcess(collection, ref newitem, PipelineStage.PreOperation, DataOperation.Write, dataContext);

            string json = newitem.ToString();
            BsonDocument itemToAdd = BsonSerializer.Deserialize<BsonDocument>(json);
            if (itemToAdd.Contains("_id"))
            {
                itemToAdd.Remove("_id");
            }

            itemToAdd.Remove("_metadata");
            _mongoService.GetCollection<BsonDocument>(collection).InsertOne(itemToAdd);

            //sanitize
            itemToAdd["_id"] = itemToAdd["_id"].ToString();

            var addedItem = JObject.Parse(itemToAdd.ToJson(js));

            InvokeProcess(collection, ref addedItem, PipelineStage.PostOperation, DataOperation.Write, dataContext);

            return addedItem;
        }

        private void InvokeValidation(JObject newitem, string collection)
        {
            List<Error> errors = Validate(newitem, collection);
            if (errors.Count > 0)
            {
                throw new ValidationException(errors, null);
            }
        }

        private void InvokeProcess(string collection, ref JObject item, PipelineStage save, DataOperation dataOperation, Dictionary<string, object> dataContext)
        {
            List<Lambda> processhandlers = lambdaManager.Lambdas
                .Where(x => x is DataProcessLambda)
                .Where(x => ((DataProcessLambda)x).Stage == save
                && ((DataProcessLambda)x).Operation == dataOperation)
                .ToList();

            foreach (DataProcessLambda h in processhandlers)
            {
                h.Execute(collection, ref item, ref dataContext);
            }
        }

        private void InvokeAlterQuery(string collection, FilterDefinition<BsonDocument> query)
        {
            List<Lambda> genericAlter = lambdaManager.Lambdas
                .Where(x => x is AlterQueryLambda)
                .ToList();

            foreach (AlterQueryLambda h in genericAlter)
            {
                h.Alter(collection, query);
            }

            List<CollectionAlterQueryLambda> collectionAlter = lambdaManager.Lambdas
                .Where(x => x is CollectionAlterQueryLambda)
                .Cast<CollectionAlterQueryLambda>()
                .Where(x => Regex.IsMatch(collection, x.Collection))
                .ToList();

            foreach (CollectionAlterQueryLambda h in collectionAlter)
            {
                h.Alter(query);
            }
        }

        public JObject Update(string collection, JObject item, bool replace)
        {
            var dataContext = new Dictionary<string, object>();

            var id = item["_id"].Value<string>();
            var itemToCheck = item;

            if (replace == false)
            {
                itemToCheck = Get(collection, id);
                itemToCheck.Merge(item, new JsonMergeSettings()
                {
                    MergeNullValueHandling = MergeNullValueHandling.Merge,
                    MergeArrayHandling = MergeArrayHandling.Replace
                });
            }
            InvokeValidation(itemToCheck, collection);

            //TODO: create collection if not exists
            EnsureCollection(collection);

            FilterDefinition<BsonDocument> filter = Builders<BsonDocument>.Filter.Eq("_id", BsonObjectId.Create(item["_id"].Value<string>()));

            InvokeProcess(collection, ref item, PipelineStage.PreOperation, DataOperation.Write, dataContext);

            //insert id (mandatory)
            BsonDocument doc = BsonDocument.Parse(item.ToString());
            doc["_id"] = BsonObjectId.Create(id);
            doc.Remove("_metadata");

            UpdateOptions o = new UpdateOptions()
            {
                IsUpsert = true,
                BypassDocumentValidation = true
            };

            if (replace)
            {
                _mongoService.GetCollection<BsonDocument>(collection).ReplaceOne(filter, doc, o);
            }
            else
            {
                BsonDocument dbset = new BsonDocument("$set", doc);
                _mongoService.GetCollection<BsonDocument>(collection).UpdateOne(filter, dbset, o);
            }
            var fullSaved = Get(collection, id);
            InvokeProcess(collection, ref fullSaved, PipelineStage.PostOperation, DataOperation.Write, dataContext);
            return fullSaved;
        }

        public void EnsureCollection(string collection)
        {
            if (!collectionNames.Any(x => x.Equals(collection)))
            {
                _mongoService.GetDatabase().CreateCollection(collection);
                collectionNames.Add(collection);
            }
        }

        public bool Delete(string collection, string id)
        {
            EnsureCollection(collection);

            var old = Get(collection, id);
            var dataContext = new Dictionary<string, object>();

            InvokeProcess(collection, ref old, PipelineStage.PreOperation, DataOperation.Delete, dataContext);

            FilterDefinition<BsonDocument> filter = Builders<BsonDocument>.Filter.Eq("_id", BsonObjectId.Create(id));

            DeleteResult result = _mongoService.GetCollection<BsonDocument>(collection).DeleteOne(filter);

            InvokeProcess(collection, ref old, PipelineStage.PostOperation, DataOperation.Delete, dataContext);

            return result.DeletedCount == 1;
        }

        public JObject Get(string collection, string id, List<string> expando = null)
        {
            FilterDefinition<BsonDocument> filter = Builders<BsonDocument>.Filter.Eq("_id", BsonObjectId.Create(id));
            JObject nullValue = new JObject();
            var dataContext = new Dictionary<string, object>();
            dataContext["expando"] = expando ?? new List<string>();

            InvokeProcess(collection, ref nullValue, PipelineStage.PreOperation, DataOperation.Read, dataContext);

            IFindFluent<BsonDocument, BsonDocument> results = _mongoService
                .GetCollection<BsonDocument>(collection)
                .Find<BsonDocument>(filter);

            List<BsonDocument> list = results.ToList();

            var item = list.FirstOrDefault();
            string json = "{}";
            //sanitize id format
            if (item != null)
            {
                item["_id"] = item["_id"].ToString();
                json = item.ToJson(js);
            }

            var output = JObject.Parse(json);
            InvokeProcess(collection, ref output, PipelineStage.PostOperation, DataOperation.Read, dataContext);
            return output;
        }

        public long Count(string collection, string query)
        {
            FilterDefinition<BsonDocument> filter = FilterDefinition<BsonDocument>.Empty;
            if (!string.IsNullOrWhiteSpace(query))
            {
                filter = new JsonFilterDefinition<BsonDocument>(query);
            }
            return Count(collection, filter);
        }

        public long Count(string collection, FilterDefinition<BsonDocument> filter)
        {
            InvokeAlterQuery(collection, filter);
            long count = _mongoService
               .GetCollection<BsonDocument>(collection).Find<BsonDocument>(filter).Count();
            return count;
        }

        public ItemList Query(string collection, DataQuery query)
        {
            FilterDefinition<BsonDocument> filter = FilterDefinition<BsonDocument>.Empty;
            var dataContext = new Dictionary<string, object>();
            dataContext["expando"] = query.Expando;

            if (query.RawQuery != null)
            {
                filter = new JsonFilterDefinition<BsonDocument>(query.RawQuery);
            }

            InvokeAlterQuery(collection, filter);

            IFindFluent<BsonDocument, BsonDocument> results = _mongoService
                .GetCollection<BsonDocument>(collection).Find<BsonDocument>(filter)
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Limit(query.PageSize);

            if (query.Sort != null)
            {
                var sort = new SortDefinitionBuilder<BsonDocument>();
                SortDefinition<BsonDocument> sortDef = null;
                foreach (var sortable in query.Sort)
                {
                    FieldDefinition<BsonDocument> field = sortable.Field;

                    if (sortable.Ascending)
                    {
                        sortDef = (sortDef == null) ? sort.Ascending(field) : sortDef.Ascending(field);
                    }
                    else
                    {
                        sortDef = (sortDef == null) ? sort.Descending(field) : sortDef.Descending(field);
                    }
                }
                results = results.Sort(sortDef);
            }

            long count = Count(collection, filter);

            List<BsonDocument> list = results.ToList();

            //sanitize id format
            foreach (BsonDocument item in list)
            {
                item["_id"] = item["_id"].ToString();
            }

            string json = list.ToJson(js);
            var result = JArray.Parse(json);
            for (int i = 0; i < result.Count; i++)
            {
                var node = (JObject)result[i];
                InvokeProcess(collection, ref node, PipelineStage.PostOperation, DataOperation.Read, dataContext);
            }
            return new ItemList(result, (int)count, query.PageNumber, query.PageSize);
        }

        public List<Error> Validate(JObject item, string collection)
        {
            List<Error> result = new List<Error>();
            result.AddRange(ValidateGeneric(item, collection));
            result.AddRange(ValidateSpecific(item, collection));

            return result;
        }

        private List<Error> ValidateSpecific(JObject item, string collection)
        {
            List<Error> result = new List<Error>();

            List<Lambda> labdas = lambdaManager.Lambdas
                .Where(x => x is CollectionValidationLambda)
                .Where(x => ((CollectionValidationLambda)x).TargetCollections.Contains(collection))
                .ToList();

            foreach (CollectionValidationLambda lambda in labdas)
            {
                List<Error> errors = lambda.Validate(item);
                result.AddRange(errors);
            }

            return result;
        }

        private List<Error> ValidateGeneric(JObject item, string collection)
        {
            List<Error> result = new List<Error>();

            List<Lambda> labdas = lambdaManager.Lambdas
                .Where(x => x is SchemaValidationLambda).ToList();

            foreach (SchemaValidationLambda lambda in labdas)
            {
                List<Error> errors = lambda.Validate(item, collection);
                result.AddRange(errors);
            }

            return result;
        }
    }
}