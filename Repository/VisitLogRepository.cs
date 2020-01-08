﻿using System;
using NOSQLTask.Context;
using NOSQLTask.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nest;
using Microsoft.Extensions.Options;

namespace NOSQLTask.Repository
{
    public interface IVisitLogRepository
    {
        Task<IEnumerable<VisitLog>> GetLogsFrom(int ClientId, string ProductId);
        Task<IEnumerable<VisitLog>> GetLogsSameProduct(int ClientId, string ProductId);
        Task<VisitLog> GetLog(string key);
        Task AddLog(VisitLog item);
        Task RemoveLog(string id);
        Task UpdateLog(string key, VisitLog item);
    }

    public class VisitLogRepository : IVisitLogRepository
    {
        private readonly ESContext _context = null;

        public VisitLogRepository(IOptions<Settings> settings)
        {
            _context = new ESContext(settings);
        }

        public async Task AddLog(VisitLog item)
        {
            var response = await _context.Connection.IndexAsync(item, idx => idx.Index("contextidx"));
        }

        public async Task<IEnumerable<VisitLog>> GetLogsFrom(int ClientId, string ProductId)
        {
            var response = await _context.Connection.SearchAsync<VisitLog>(s => s
                        .From(0)
                        .Size(10)
                        .Index("contextidx")
                        .Query(q => q
                            .Bool(b => b
                                    .MustNot(bs => bs.Term(t => t.ClientId, ClientId))
                                 ) && q
                            .Match(mq => mq.Field(f => f.FromProduct).Query(ProductId))
                            )
                        );

            return response.Documents;
        }

        public async Task<VisitLog> GetLog(string key)
        {
            var response = await _context.Connection.GetAsync<VisitLog>(1, idx => idx.Index("contextidx"));
            var log = response.Source; // the original document

            return log;
        }

        public Task RemoveLog(string id)
        {
            throw new NotImplementedException();
        }

        public Task UpdateLog(string key, VisitLog item)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<VisitLog>> GetLogsSameProduct(int ClientId, string ProductId)
        {
            var response = await _context.Connection.SearchAsync<VisitLog>(s => s
            .From(0)
            .Size(10)
            .Index("contextidx")
            .Query(q => q
                .Bool(b => b
                        .MustNot(bs => bs.Term(t => t.ClientId, ClientId))
                     ) && q
                .Match(mq => mq.Field(f => f.ToProduct).Query(ProductId))
                )
            );

            return response.Documents;
        }
    }
}
