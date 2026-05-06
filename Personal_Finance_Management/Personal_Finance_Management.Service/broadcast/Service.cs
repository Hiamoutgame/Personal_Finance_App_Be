using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Personal_Finance_Management.Repository;

namespace Personal_Finance_Management.Service.broadcast
{
    public class Service : IService
    {
        private readonly AppDbContext _dbContext;
        public Service(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public Task<Response.BroadcastsResponse> CreateBroadcast(Request.BroadcastsRequest request)
        {
            throw new NotImplementedException();
        }
    }
}