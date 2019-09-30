/*
 * Magic, Copyright(c) Thomas Hansen 2019 - thomas@gaiasoul.com
 * Licensed as Affero GPL unless an explicitly proprietary license has been obtained.
 */

using System;
using System.Linq;
using System.Text;
using magic.node;
using magic.node.extensions;
using magic.signals.contracts;

namespace magic.data.common
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
        /// <param name="signaler">Signaler to invoke slots.</param>
        /// <param name="escapeChar">Escape character to use for escaping table names etc.</param>
        public SqlBuilder(Node node, ISignaler signaler, string escapeChar)
        {
            Root = node ?? throw new ArgumentNullException(nameof(node));
            Signaler = signaler ?? throw new ArgumentNullException(nameof(signaler));
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

        #region [ -- Protected helper methods and properties -- ]

        /// <summary>
        /// Root node from which the SQL generator is being evaluated towards.
        /// </summary>
        protected Node Root { get; private set; }

        /// <summary>
        /// Signaler provided to CTOR during construction.
        /// </summary>
        protected ISignaler Signaler { get; private set; }

        /// <summary>
        /// Securely adds the table name into the specified builder.
        /// </summary>
        /// <param name="builder">StringBuilder to append the table name into.</param>
        protected void GetTableName(StringBuilder builder)
        {
            builder.Append(EscapeChar);

            // Retrieving actual table name from [table] node.
            var tableName = Root.Children.FirstOrDefault(x => x.Name == "table")?.GetEx<string>();
            if (tableName == null)
                throw new ApplicationException($"No table name supplied to '{GetType().FullName}'");
            builder.Append(tableName.Replace(EscapeChar, EscapeChar + EscapeChar));

            builder.Append(EscapeChar);
        }

        /// <summary>
        /// Builds the 'where' parts of the SQL statement.
        /// </summary>
        /// <param name="whereNode">Current input node from where to start looking for semantic where parts.</param>
        /// <param name="builder">String builder to put the results into.</param>
        protected void BuildWhere(Node whereNode, StringBuilder builder)
        {
            var where = Root.Children.Where(x => x.Name == "where");
            if (where.Count() > 1)
                throw new ApplicationException($"Syntax error in '{GetType().FullName}', too many [where] nodes");

            // Checking we actuall have a [where] declaration
            if (!where.Any() || !where.First().Children.Any())
                return;

            builder.Append(" where ");

            int levelNo = 0;
            foreach (var idx in where.First().Children)
            {
                switch (idx.Name)
                {
                    case "and":
                        if (levelNo != 0)
                            builder.Append(" and ");
                        BuildWhereLevel(whereNode, builder, idx, "and", ref levelNo);
                        break;

                    case "or":
                        if (levelNo != 0)
                            builder.Append(" or ");
                        BuildWhereLevel(whereNode, builder, idx, "or", ref levelNo);
                        break;

                    default:
                        throw new ArgumentException($"I don't understand '{idx.Name}' as a where clause while trying to build SQL");
                }
            }
        }

        #endregion

        #region [ -- Private helper methods -- ]

        void BuildWhereLevel(
            Node result,
            StringBuilder builder,
            Node level,
            string logicalOperator,
            ref int levelNo,
            string comparisonOperator = "=",
            bool paranthesis = true)
        {
            if (paranthesis)
                builder.Append("(");

            bool first = true;
            foreach (var idxCol in level.Children)
            {
                if (first)
                    first = false;
                else
                    builder.Append(" " + logicalOperator + " ");

                switch (idxCol.Name)
                {
                    case "and":
                        BuildWhereLevel(result, builder, idxCol, "and", ref levelNo);
                        break;

                    case "or":
                        BuildWhereLevel(result, builder, idxCol, "or", ref levelNo);
                        break;

                    case ">":
                    case "<":
                    case ">=":
                    case "<=":
                    case "!=":
                    case "=":
                    case "like":
                        BuildWhereLevel(result, builder, idxCol, logicalOperator, ref levelNo, idxCol.Name, false);
                        break;

                    default:
                        var unwrapped = idxCol.GetEx<object>();
                        var argName = "@" + levelNo;
                        var arg = EscapeChar +
                            idxCol.Name.Replace(EscapeChar, EscapeChar + EscapeChar) +
                            EscapeChar + " " + comparisonOperator + " " +
                            argName;
                        builder.Append(arg);
                        result.Add(new Node(argName, unwrapped));
                        ++levelNo;
                        break;
                }
            }

            if (paranthesis)
                builder.Append(")");
        }

        #endregion
    }
}
