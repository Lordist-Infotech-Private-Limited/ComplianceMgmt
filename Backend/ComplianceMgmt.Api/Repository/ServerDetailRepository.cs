﻿using ComplianceMgmt.Api.Infrastructure;
using ComplianceMgmt.Api.IRepository;
using ComplianceMgmt.Api.Models;
using Dapper;

namespace ComplianceMgmt.Api.Repository
{
    public class ServerDetailRepository(ComplianceMgmtDbContext context) : IServerDetailRepository
    {
        public async Task<IEnumerable<ServerDetail>> GetServerDetailsAsync()
        {
            using (var connection = await context.CreateDefaultConnectionAsync())
            {
                var query = "SELECT * FROM serverdetails";
                return await connection.QueryAsync<ServerDetail>(query);
            }
        }
    }
}
