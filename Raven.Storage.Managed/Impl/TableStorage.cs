﻿using System;
using System.Threading;
using Newtonsoft.Json.Linq;
using Raven.Database.Data;

namespace Raven.Storage.Managed.Impl
{
    /// <summary>
    /// Give us nice names for the persistent dictionaries
    /// 0 - Details
    /// 1 - Identity
    /// 2 - Attachments
    /// </summary>
    public class TableStorage : AggregateDictionary
    {
        private readonly ThreadLocal<Guid> txId = new ThreadLocal<Guid>(() => Guid.Empty);

        public TableStorage(IPersistentSource persistentSource)
            : base(persistentSource)
        {
            Details = new PersistentDictionaryAdapter(txId,
                                                      Add(new PersistentDictionary(persistentSource,
                                                                                   JTokenComparer.Instance)));
            Identity = new PersistentDictionaryAdapter(txId,
                                                       Add(new PersistentDictionary(persistentSource, new ModifiedJTokenComparer(x=>x.Value<string>("name")))));

            Attachments = new PersistentDictionaryAdapter(txId, Add(new PersistentDictionary(persistentSource, new ModifiedJTokenComparer(x => x.Value<string>("key")))))
            {
                {"ByEtag", x => x.Value<byte[]>("etag")}
            };

            Documents = new PersistentDictionaryAdapter(txId, Add(new PersistentDictionary(persistentSource, new ModifiedJTokenComparer(x => x.Value<string>("key")))))
            {
                {"ByKey", x => x.Value<string>("key")},
                {"ById", x => x.Value<string>("id")},
                {"ByEtag", x => x.Value<byte[]>("etag")}
            };

            DocumentsModifiedByTransactions = new PersistentDictionaryAdapter(txId, 
                Add(new PersistentDictionary(persistentSource, new ModifiedJTokenComparer(x => x.Value<string>("key")))))
            {
                {"ByTxId", x => x.Value<byte[]>("txId")}
            };
            Transactions = new PersistentDictionaryAdapter(txId,
                Add(new PersistentDictionary(persistentSource,new ModifiedJTokenComparer(x => x.Value<byte[]>("txId")))));

            IndexingStats = new PersistentDictionaryAdapter(txId,
                Add(new PersistentDictionary(persistentSource,new ModifiedJTokenComparer(x =>x.Value<string>("index")))));

            MappedResults = new PersistentDictionaryAdapter(txId, Add(new PersistentDictionary(persistentSource, JTokenComparer.Instance)))
            {
                {"ByViewAndReduceKey", x => new JObject
                    {
                        {"view", x.Value<string>("view")},
                        {"reduceKey", x.Value<string>("reduceKey")}
                    }},
               {"ByViewAndDocumentId", x => new JObject
                    {
                        {"view", x.Value<string>("view")},
                        {"docId", x.Value<string>("docId")}
                    }}
            };

            Queues = new PersistentDictionaryAdapter(txId, Add(new PersistentDictionary(persistentSource, 
                                                                                        new ModifiedJTokenComparer(x=> new JObject
                                                                                        {
                                                                                            {"name", x.Value<string>("name")},
                                                                                            {"id", x.Value<byte[]>("id")},
                                                                                        }))))
            {
                {"ByName", x=>x.Value<string>("name")}
            };

            Tasks = new PersistentDictionaryAdapter(txId, Add(new PersistentDictionary(persistentSource,
                                                                                       new ModifiedJTokenComparer(x => new JObject
                                                                                        {
                                                                                            {"index", x.Value<string>("index")},
                                                                                            {"id", x.Value<byte[]>("id")},
                                                                                        }))))
            {
                {"ByIndex", x=>x.Value<string>("index")},
                {"ByIndexAndType", x=>new JObject
                {
                    {"index", x.Value<string>("index")},
                    {"type", x.Value<string>("type")},
                }}
            };
        }

        public PersistentDictionaryAdapter Tasks { get; private set; }

        public PersistentDictionaryAdapter  Queues { get; private set; }

        public PersistentDictionaryAdapter MappedResults { get; private set; }

        public PersistentDictionaryAdapter IndexingStats { get; private set; }

        public PersistentDictionaryAdapter Transactions { get; private set; }

        public PersistentDictionaryAdapter DocumentsModifiedByTransactions { get; private set; }

        public PersistentDictionaryAdapter Documents { get; private set; }

        public PersistentDictionaryAdapter Attachments { get; private set; }

        public PersistentDictionaryAdapter Identity { get; private set; }

        public PersistentDictionaryAdapter Details { get; private set; }

        public IDisposable BeginTransaction()
        {
            if (txId.Value != Guid.Empty)
                return new DisposableAction(() => { }); // no op, already in tx

            txId.Value = Guid.NewGuid();

            return new DisposableAction(() =>
            {
                if (txId.Value != Guid.Empty) // tx not committed
                    Rollback();
            });
        }

        public void Commit()
        {
            if (txId.Value == Guid.Empty)
                return;

            Commit(txId.Value);
        }

        public void Rollback()
        {
            Rollback(txId.Value);

            txId.Value = Guid.Empty;
        }
    }
}