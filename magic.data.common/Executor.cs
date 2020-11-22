/*
 * Magic, Copyright(c) Thomas Hansen 2019 - 2020, thomas@servergardens.com, all rights reserved.
 * See the enclosed LICENSE file for details.
 */

using System;
using System.Linq;
using System.Data.Common;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using magic.node;
using magic.node.extensions;

namespace magic.data.common
{
    /// <summary>
    /// Helper class for creating and parametrizing an SQL command of some type.
    /// </summary>
    public static class Executor
    {
        /// <summary>
        /// Creates a new SQL command of some type, and parametrizes it with each
        /// child node specified in the invocation node as a key/value DB parameter -
        /// For then to invoke the specified functor lambda callback.
        /// </summary>
        /// <param name="input">Node containing SQL and parameters as children.</param>
        /// <param name="connection">Database connection.</param>
        /// <param name="transaction">Database transaction, or null if there are none.</param>
        /// <param name="functor">Lambda function responsible for executing the command somehow.</param>
        public static void Execute(
            Node input,
            DbConnection connection,
            Transaction transaction,
            Action<DbCommand, long> functor)
        {
            // Making sure we dispose our command after execution.
            using (var cmd = connection.CreateCommand())
            {
                // Checking if caller supplied a [max] argument, defaulting to -1
                var max = input.Children.FirstOrDefault(x => x.Name == "max")?.GetEx<long>() ?? -1;

                // Parametrizing and decorating command.
                PrepareCommand(cmd, transaction, input);

                // Invoking lambda callback supplied by caller.
                functor(cmd, max);
            }
        }

        /// <summary>
        /// Creates a new SQL command of some type, and parametrizes it with each
        /// child node specified in the invocation node as a key/value DB parameter -
        /// For then to invoke the specified functor lambda callback.
        /// </summary>
        /// <param name="input">Node containing SQL and parameters as children.</param>
        /// <param name="connection">Database connection.</param>
        /// <param name="transaction">Database transaction, or null if there are none.</param>
        /// <param name="functor">Lambda function responsible for executing the command somehow.</param>
        /// <returns>An awaitable task.</returns>
        public static async Task ExecuteAsync(
            Node input,
            DbConnection connection,
            Transaction transaction,
            Func<DbCommand, long, Task> functor)
        {
            using (var cmd = connection.CreateCommand())
            {
                // Checking if caller supplied a [max] argument, defaulting to -1
                var max = input.Children.FirstOrDefault(x => x.Name == "max")?.GetEx<long>() ?? -1;

                // Parametrizing and decorating command.
                PrepareCommand(cmd, transaction, input);

                // Invoking lambda callback supplied by caller.
                await functor(cmd, max);
            }
        }

        /// <summary>
        /// Creates a new SQL command of some type, and parametrizes it with each
        /// child node specified in the invocation node as a key/value DB parameter -
        /// For then to invoke the specified functor lambda callback.
        /// </summary>
        /// <param name="input">Node containing SQL and parameters as children.</param>
        /// <param name="connection">Database connection.</param>
        /// <param name="transaction">Database transaction, or null if there are none.</param>
        /// <param name="functor">Lambda function responsible for executing the command somehow.</param>
        public static void Execute(
            Node input,
            DbConnection connection,
            Transaction transaction,
            Action<DbCommand> functor)
        {
            // Making sure we dispose our command after execution.
            using (var cmd = connection.CreateCommand())
            {
                // Parametrizing and decorating command.
                PrepareCommand(cmd, transaction, input);

                // Invoking lambda callback supplied by caller.
                functor(cmd);
            }
        }

        /// <summary>
        /// Creates a new SQL command of some type, and parametrizes it with each
        /// child node specified in the invocation node as a key/value DB parameter -
        /// For then to invoke the specified functor lambda callback.
        /// </summary>
        /// <param name="input">Node containing SQL and parameters as children.</param>
        /// <param name="connection">Database connection.</param>
        /// <param name="transaction">Database transaction, or null if there are none.</param>
        /// <param name="functor">Lambda function responsible for executing the command somehow.</param>
        /// <returns>An awaitable task.</returns>
        public static async Task ExecuteAsync(
            Node input,
            DbConnection connection,
            Transaction transaction,
            Func<DbCommand, Task> functor)
        {
            using (var cmd = connection.CreateCommand())
            {
                // Parametrizing and decorating command.
                PrepareCommand(cmd, transaction, input);

                // Invoking lambda callback supplied by caller.
                await functor(cmd);
            }
        }

        /// <summary>
        /// Creates a connection string according to the arguments provided,
        /// and returns to caller.
        /// </summary>
        /// <param name="input">Node containing value trying to connect to a database</param>
        /// <param name="databaseType">Type of database adapter</param>
        /// <param name="defaultCatalogue">The default catalogue to use if no explicit database was specified</param>
        /// <param name="configuration">Configuration object from where to retrieve connection string templates</param>
        /// <returns>Connection string</returns>
        public static string GetConnectionString(
            Node input,
            string databaseType,
            string defaultCatalogue,
            IConfiguration configuration)
        {
            var connectionString = input.Value == null ? null : input.GetEx<string>();

            // Checking if this is a "generic connection string".
            if (string.IsNullOrEmpty(connectionString))
            {
                var generic = configuration[$"magic:databases:{databaseType}:generic"];
                connectionString = generic.Replace("{database}", defaultCatalogue);
            }
            else if (connectionString.StartsWith("[", StringComparison.InvariantCulture) &&
                connectionString.EndsWith("]", StringComparison.InvariantCulture))
            {
                connectionString = connectionString.Substring(1, connectionString.Length - 2);
                if (connectionString.Contains("|"))
                {
                    var segments = connectionString.Split('|');
                    if (segments.Length != 2)
                        throw new ArgumentException($"I don't understand '{connectionString}' as a connection string");
                    var generic = configuration[$"magic:databases:{databaseType}:{segments[0]}"];
                    connectionString = generic.Replace("{database}", segments[1]);
                }
                else
                {
                    var generic = configuration[$"magic:databases:{databaseType}:generic"];
                    connectionString = generic.Replace("{database}", connectionString);
                }
            }
            else if (!connectionString.Contains(";"))
            {
                var generic = configuration[$"magic:databases:{databaseType}:generic"];
                connectionString = generic.Replace("{database}", connectionString);
            }
            return connectionString;
        }

        #region [ -- Private helper methods -- ]

        /*
         * Helper method to parametrize command with SQL parameters, in addition to
         * decorating command with the specified transaction, if any.
         */
        static void PrepareCommand(
            DbCommand cmd, 
            Transaction transaction, 
            Node input)
        {
            // Associating transaction with command.
            if (transaction != null)
                cmd.Transaction = transaction.Value;

            // Retrieves the command text.
            cmd.CommandText = input.GetEx<string>();

            // Applies the parameters, if any.
            foreach (var idxPar in input.Children)
            {
                var par = cmd.CreateParameter();
                par.ParameterName = idxPar.Name;
                par.Value = idxPar.GetEx<object>();
                cmd.Parameters.Add(par);
            }

            // Making sure we clean nodes before invoking lambda callback.
            input.Value = null;
            input.Clear();
        }

        #endregion
    }
}
