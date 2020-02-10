using MongoDB.Driver;
using MongoDB.Driver.Linq;
using MongoDB.Entities.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoDB.Entities.Relationships
{
    //todo: xml docs
    public class Multiple<T> where T : IEntity
    {
        private IMongoCollection<T> childCollection;
        private IEntity parent;
        private string parentName;
        private string property;

        internal Multiple() => throw new InvalidOperationException("Parameterless constructor is disabled!");

        internal Multiple(object parent, string property)
        {
            Init((dynamic)parent, property);
        }

        private void Init<TParent>(TParent parent, string property) where TParent : IEntity
        {
            this.parent = parent;
            parentName = parent.GetType().Name;
            this.property = property;
            childCollection = DB.Collection<T>(parent.Database());
        }

        //todo: add multiple children with bulk update

        public void Add(T child, IClientSessionHandle session = null)
        {
            Run.Sync(() => AddAsync(child, session));
        }

        public Task AddAsync(T child, IClientSessionHandle session = null)
        {
            parent.ThrowIfUnsaved();
            child.ThrowIfUnsaved();

            var filter = Builders<T>.Filter.Eq(c => c.ID, child.ID);
            var update = Builders<T>.Update.Set($"_ref_{parentName}-{property}", parent.ID);

            return session == null
                    ? childCollection.UpdateOneAsync(filter, update)
                    : childCollection.UpdateOneAsync(session, filter, update);
        }
    }
}
