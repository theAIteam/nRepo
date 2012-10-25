﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace AI.nRepo
{
    public interface IRepository<TAggregate>
    {
        void Add(TAggregate entity);

        void Remove(TAggregate entity);

        void Remove(IList<TAggregate> entities);

        TAggregate Get(object key);

        IList<TAggregate> GetAll();

        IList<TAggregate> GetAll(int pageSize, int pageNumber);

        void BeginTransaction();

        void CommitTransaction();

        void RollbackTransaction();

        void Add(IList<TAggregate> entities);

        IQueryable<TAggregate> CreateQuery();
    }
}
