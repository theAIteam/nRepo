﻿using System.Collections;
using System.Linq.Expressions;
using AI.nRepo.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AI.nRepo.Events;
using AI.nRepo.Sharding;


namespace AI.nRepo
{
    public abstract class RepositoryBase<T> : IRepository<T>
    {
        private IDataAccessor<T> _dataAccessor;
        private IDataAccessor<T> _defaultAccessor;
        protected RepositoryBase(string alias)
        {
            var repoConfiguration = Configure.MasterConfiguration.GetConfiguration(alias);
            _defaultAccessor = _dataAccessor = repoConfiguration.Create<T>();
        }

        public IDataAccessor<T> GetDataAccessor()
        {
            //TODO: shrug
            return _defaultAccessor;
        }

        public IDataAccessor<T> GetDataAccessor(T entity)
        {
            var shard = ShardLocator.GetShard(entity);
            var repoConfiguration = Configure.MasterConfiguration.GetConfiguration(shard);
            _dataAccessor = repoConfiguration.Create<T>();
            return _dataAccessor;
        }

        public IDataAccessor<T> GetDataAccessorByKey(object key)
        {
            //TODO: shrug
            return _dataAccessor;
        }

        public virtual IUnitOfWork UnitOfWork
        {
            set
            {
                value.AddWorkItem(this);
            }
        }
        public virtual void Add(T entity)
        {
            RepositoryEventRegistry.RaiseEvent<IBeforeAddListener>(entity);
            GetDataAccessor(entity).Add(entity);
            RepositoryEventRegistry.RaiseEvent<IAfterAddListener>(entity);
            
        }

        protected IList<T> ExecuteQuery(string query)
        {
            return GetDataAccessor().ExecuteQuery(query);
        } 

        public virtual void Remove(T entity)
        {
            RepositoryEventRegistry.RaiseEvent<IBeforeRemoveListener>(entity);
            GetDataAccessor(entity).Remove(entity);
            RepositoryEventRegistry.RaiseEvent<IAfterRemoveListener>(entity);
        }

        public virtual void Remove(IList<T> entities)
        {
            foreach (var entity in entities)
                this.Remove(entity);
        }

        public virtual T Get(object key)
        {
            return GetDataAccessorByKey(key).Get(key);
        }

        public virtual IList<T> GetAll()
        {
            return GetDataAccessor().GetAll();
        }

        public virtual IList<T> GetAll(int pageSize, int pageNumber)
        {
            return GetDataAccessor().GetAll(pageSize, pageNumber);
        }

        public void BeginTransaction()
        {
            GetDataAccessor().BeginTransaction();
        }

        public void CommitTransaction()
        {
            GetDataAccessor().CommitTransaction();
        }

        public void RollbackTransaction()
        {
            GetDataAccessor().RollbackTransaction();
        }

        public virtual void Add(IList<T> entities)
        {
            foreach (var entity in entities)
                this.Add(entity);
        }

        public IQueryable<T> CreateQuery()
        {
            return GetDataAccessor().CreateQuery();
        }
        
        public IEnumerator<T> GetEnumerator()
        {
            return CreateQuery().GetEnumerator();
        }
       
        public Type ElementType
        {
            get { return this.CreateQuery().ElementType; }
        }

        public Expression Expression
        {
            get { return CreateQuery().Expression; }
        }

        public IQueryProvider Provider
        {
            get { return CreateQuery().Provider; }
        }

        public void Dispose()
        {
            _dataAccessor.Dispose();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
