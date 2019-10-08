/*
 * Magic, Copyright(c) Thomas Hansen 2019 - thomas@gaiasoul.com
 * Licensed as Affero GPL unless an explicitly proprietary license has been obtained.
 */

using System;
using System.Data.Common;
using magic.signals.contracts;

namespace magic.data.common
{
     /// <summary>
     /// Wraps a database transaction, such that it automatically is rolled back when
     /// instance is disposed, unless it has been previously rolled back, or committed.
     /// </summary>
    public class Transaction : IDisposable
    {
        readonly DbTransaction _transaction;
        bool _signaled;

        /// <summary>
        /// Creates a new instance of your type.
        /// </summary>
        /// <param name="signaler">Signaler used to retrieve connection as stack object.</param>
        /// <param name="connection">Database connection.</param>
        public Transaction(ISignaler signaler, DbConnection connection)
        {
            _transaction = connection.BeginTransaction();
        }

        /// <summary>
        /// Explicitly rolls back the transaction.
        /// </summary>
        public void Rollback()
        {
            _signaled = true;
            _transaction.Rollback();
        }

         /// <summary>
         /// Explicitly committing your transaction.
         /// </summary>
        public void Commit()
        {
            _signaled = true;
            _transaction.Commit();
        }

        #region [ -- Interface implementation -- ]

        /// <summary>
        /// Disposes the instance.
        /// </summary>
        public void Dispose()
        {
            if (!_signaled)
                _transaction.Rollback();
        }

        #endregion
    }
}
