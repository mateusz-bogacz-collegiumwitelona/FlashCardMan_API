using Data.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Repositories
{
    public class FlashCardRepo
    {
        private readonly ApplicationDbContext _dbContext;

        public FlashCardRepo(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        

    }
}
