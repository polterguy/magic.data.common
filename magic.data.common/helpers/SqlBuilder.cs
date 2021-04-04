/*
 * Magic, Copyright(c) Thomas Hansen 2019 - 2021, thomas@servergardens.com, all rights reserved.
 * See the enclosed LICENSE file for details.
 */

using System;
using System.Linq;
using System.Text;
using magic.node;
using magic.node.extensions;
using magic.signals.contracts;

namespace magic.data.common.helpers
{
    /// <summary>
    /// Common base class for SQL generators, allowing you to generate SQL from a lambda object.
    /// </summary>
    public abstract class SqlBuilder
    {
        /// <summary>
        /// Creates a new SQL builder.
        /// </summary>
        /// <param name="node">Root node to generate your SQL from.</param>
        /// <param name="escapeChar">Escape character to use for escaping table names etc.</param>
        protected SqlBuilder(Node node, string escapeChar)
        {
            Root = node ?? throw new ArgumentNullException(nameof(node));
            EscapeChar = escapeChar ?? throw new ArgumentNullException(nameof(escapeChar));
        }

        /// <summary>
        /// Builds your SQL statement, and returns a structured SQL statement, plus any parameters.
        /// </summary>
        /// <returns>Node containing SQL as root node, and parameters as children.</returns>
        public abstract Node Build();

        /// <summary>
        /// Signals to inherited class if this is a pure generate job, or if it should also evaluate the SQL command.
        /// </summary>
        public bool IsGenerateOnly => Root.Children.FirstOrDefault(x => x.Name == "generate")?.Get<bool>() ?? false;

        /// <summary>
        /// Returns the escape character, which is normally for instance " or `
        /// </summary>
        protected string EscapeChar { get; private set; }

        /// <summary>
        /// Generic helper method to create an SqlBuilder of type T, and use it to semantically
        /// traverse a node hierarchy, to create the relevant SQL and its parameter collection.
        /// </summary>
        /// <typeparam name="T">Type of SQL builder to create.</typeparam>
        /// <param name="signaler">Signaler for instance.</param>
        /// <param name="input">Node to parser.</param>
        /// <returns>If execution of node should be done, the method will return the node to execute, otherwise null will be returned.</returns>
        public static Node Parse<T>(ISignaler signaler, Node input) where T : SqlBuilder
        {
            /*
             * Unfortunately this is our only means to create an instance of type,
             * since it requires arguments in its CTOR, and we can't create constraints
             * for constructor arguments using generic constraints.
             */
            var builder = Activator.CreateInstance(typeof(T), new object[] { input, signaler }) as T;
            var sqlNode = builder.Build();

            // Checking if this is a "build only" invocation.
            if (builder.IsGenerateOnly)
            {
                input.Value = sqlNode.Value;
                input.Clear();
                input.AddRange(sqlNode.Children.ToList());
                return null ;
            }
            return sqlNode;
        }

        #region [ -- Protected helper methods and properties -- ]

        /// <summary>
        /// Root node from which the SQL generator is being evaluated towards.
        /// </summary>
        protected Node Root { get; private set; }

        /// <summary>
        /// Securely adds the table name into the specified builder.
        /// </summary>
        /// <param name="builder">StringBuilder to append the table name into.</param>
        protected virtual void AppendTableName(StringBuilder builder)
        {
            // Retrieving actual table name from [table] node.
            var tableName = Root.Children.FirstOrDefault(x => x.Name == "table")?.GetEx<string>();
            if (tableName == null)
                throw new ArgumentException($"No [table[ supplied to '{GetType().FullName}'");
            builder.Append(EscapeTypeName(tableName));
        }

        /// <summary>
        /// Escapes a column name, and returns to caller.
        /// </summary>
        /// <param name="column">Name of column as supplied by lambda object.</param>
        /// <returns>The escaped column's name.</returns>
        protected virtual string EscapeColumnName(string column)
        {
            return EscapeChar + 
                column.Replace(EscapeChar, EscapeChar + EscapeChar) +
                EscapeChar;
        }

        /// <summary>
        /// Escapes a single table or column name, and appends to builder.
        /// </summary>
        /// <param name="typeName">Actual typename.</param>
        /// <returns>The escaped typename.</returns>
        protected virtual string EscapeTypeName(string typeName)
        {
            return string.Join(".", typeName.Split('.').Select(x => EscapeColumnName(x)));
        }

        #endregion
    }
}
