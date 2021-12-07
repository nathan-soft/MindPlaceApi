using MindPlaceApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MindPlaceApi.Data.Repositories
{
    public interface IWalletRepository : IGenericRepository<Wallet>
    {
    }
    public class WalletRepository : GenericRepository<Wallet>, IWalletRepository
    {
        public WalletRepository(IdentityAppContext context) : base(context)
        {}
    }
}
