﻿using BussinessObject.Entity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repository
{
    public interface IReviewerAssignmentRepository : IRepositoryBase<ReviewerAssignment>
    {
        Task<IEnumerable<ReviewerAssignment>> GetByPaperId(int paperId);
        Task<IEnumerable<ReviewerAssignment>> GetByReviewerId(int reviewerId);

    }
}
