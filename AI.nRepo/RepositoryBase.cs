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
        private IShardSelection _forcedShard;
        private string _defaultAlias;
        protected RepositoryBase(string alias)
        {
            _defaultAlias = alias;
            SetAccessor(alias);
        }

        protected IDataAccessor<T> SetAccessor(string alias)
        {
            var repoConfiguration = Configure.MasterConfiguration.GetConfiguration(alias);
            _dataAccessor = repoConfiguration.Create<T>();
            return _dataAccessor;
        }

        public IDataAccessor<T> GetDataAccessor()
        {
            return _dataAccessor ;
        }

        public IDataAccessor<T> GetDataAccessor(T entity)
        {
            if (_forcedShard == null)
            {
                var shard = ShardLocator.GetShard(entity);
                SetAccessor(shard);
            }
            return _dataAccessor;
        }

        public IDataAccessor<T> GetDataAccessorByKey(object key)
        {
            if (_forcedShard == null)
            {
                var shard = ShardLocator.GetShardByKey<T>(key);
                SetAccessor(shard);
            }
            return _dataAccessor;
        }

        public void UseShard(string alias)
        {
            if (String.IsNullOrEmpty(alias))
            {
                alias = _defaultAlias;
                _forcedShard = null;
                SetAccessor(_defaultAlias);
            }
            else
            {
                _forcedShard = new ShardSelection(alias);
                var repoConfiguration = Configure.MasterConfiguration.GetConfiguration(alias);
                _dataAccessor = repoConfiguration.Create<T>();
            }
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
            //GetDataAccessor().BeginTransaction();
        }

        public void CommitTransaction()
        {
            //GetDataAccessor().CommitTransaction();
        }

        public void RollbackTransaction()
        {
            //GetDataAccessor().RollbackTransaction();
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
