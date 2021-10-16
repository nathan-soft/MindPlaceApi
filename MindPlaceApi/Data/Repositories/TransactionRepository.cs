using MindPlaceApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MindPlaceApi.Data.Repositories
{
    public interface ITransactionRepository : IGenericRepository<Transaction>
    {

    }
    public class TransactionRepository : GenericRepository<Transaction>, ITransactionRepository
    {
        public TransactionRepository(IdentityAppContext context) : base(context)
        {}
    }
}
