/*
 * Magic, Copyright(c) Thomas Hansen 2019 - 2020, thomas@servergardens.com, all rights reserved.
 * See the enclosed LICENSE file for details.
 */

using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using magic.node;
using magic.node.extensions;

namespace magic.data.common.helpers
{
    /// <summary>
    /// Common base class for SQL generators requiring q where clause.
    /// </summary>
    public abstract class SqlWhereBuilder : SqlBuilder
    {
        #region [ -- Comparison operator resolver -- ]

        readonly static Dictionary<string, Func<StringBuilder, Node, Node, string, int, int>> _operators =
            new Dictionary<string, Func<StringBuilder, Node, Node, string, int, int>>
        {
            {"eq", (builder, result, colNode, escapeChar, level) => 
                DefaultOperator(
                    "=",
                    builder,
                    result,
                    colNode,
                    escapeChar,
                    level)
            },
            {"neq", (builder, result, colNode, escapeChar, level) =>
                DefaultOperator(
                    "!=",
                    builder,
                    result,
                    colNode,
                    escapeChar,
                    level)
            },
            {"mt", (builder, result, colNode, escapeChar, level) =>
                DefaultOperator(
                    ">",
                    builder,
                    result,
                    colNode,
                    escapeChar,
                    level)
            },
            {"mteq", (builder, result, colNode, escapeChar, level) =>
                DefaultOperator(
                    ">=",
                    builder,
                    result,
                    colNode,
                    escapeChar,
                    level)
            },
            {"lt", (builder, result, colNode, escapeChar, level) =>
                DefaultOperator(
                    "<",
                    builder,
                    result,
                    colNode,
                    escapeChar,
                    level)
            },
            {"lteq", (builder, result, colNode, escapeChar, level) =>
                DefaultOperator(
                    "<=",
                    builder,
                    result,
                    colNode,
                    escapeChar,
                    level)
            },
            {"like", (builder, result, colNode, escapeChar, level) =>
                DefaultOperator(
                    "like",
                    builder,
                    result,
                    colNode,
                    escapeChar,
                    level)
            },
            {"in", (builder, result, colNode, escapeChar, level) =>
                InOperator(builder, result, colNode, level)
            },
        };

        #endregion

        /// <summary>
        /// Creates a new SQL builder.
        /// </summary>
        /// <param name="node">Root node to generate your SQL from.</param>
        /// <param name="escapeChar">Escape character to use for escaping table names etc.</param>
        protected SqlWhereBuilder(Node node, string escapeChar)
            : base(node, escapeChar)
        { }

        #region [ -- Protected helper methods and properties -- ]

        /// <summary>
        /// Builds the 'where' parts of the SQL statement.
        /// </summary>
        /// <param name="result">Current input node from where to start looking for semantic where parts.</param>
        /// <param name="builder">String builder to put the results into.</param>
        protected virtual void BuildWhere(Node result, StringBuilder builder)
        {
            // finding where node, if any, and doing some basic sanity checking.
            var whereNodes = Root.Children.Where(x => x.Name == "where");
            if (whereNodes.Count() > 1)
                throw new ArgumentException($"Syntax error in '{GetType().FullName}', too many [where] nodes");

            // Checking that we actually have a [where] declaration at all.
            if (!whereNodes.Any())
                return; // No where statement supplied, or not children in [where] argument.

            // Extracting actual where node, and doing some more sanity checking.
            var where = whereNodes.First();
            if (!where.Children.Any())
                return; // Empty [where] collection.

            // Appending actual "where" parts into SQL.
            builder.Append(" where ");
            AppendWhereLevel(result, builder, whereNodes.First());
        }

        /// <summary>
        /// Appends a single [where] level.
        /// </summary>
        /// <param name="result">Where to append arguments, if requested by caller.</param>
        /// <param name="builder">Where to append SQL.</param>
        /// <param name="whereNode">Where node for current level.</param>
        protected void AppendWhereLevel(
            Node result,
            StringBuilder builder,
            Node whereNode)
        {
            /*
             * Recursively looping through each level, and appending its parts
             * as a "name/value" collection, making sure we add each value as an
             * SQL parameter.
             */
            foreach (var idx in whereNode.Children)
            {
                switch (idx.Name)
                {
                    case "or":
                    case "and":
                        BuildWhereLevel(
                            result,
                            builder,
                            idx,
                            idx.Name,
                            0,
                            false /* No outer most level paranthesis */);
                        break;

                    default:
                        throw new ArgumentException($"I don't understand '{idx.Name}' as a where clause while trying to build SQL");
                }
            }
        }

        #endregion

        #region [ -- Private helper methods -- ]

        /*
         * Building one "where level" (within one set of paranthesis),
         * and recursivelu adding a new level for each "and" and "or"
         * parts we can find in our level.
         */
        int BuildWhereLevel(
            Node result,
            StringBuilder builder,
            Node level,
            string logicalOperator,
            int levelNo,
            bool paranthesis = true)
        {
            if (paranthesis)
                builder.Append("(");

            var idxNo = 0;
            foreach (var idxCol in level.Children)
            {
                if (idxNo++ > 0)
                    builder.Append(" " + logicalOperator + " ");

                switch (idxCol.Name)
                {
                    case "and":
                    case "or":

                        // Recursively invoking self.
                        levelNo = BuildWhereLevel(
                            result,
                            builder,
                            idxCol,
                            idxCol.Name,
                            levelNo);
                        break;

                    default:

                        levelNo = CreateCondition(
                            result,
                            builder,
                            levelNo,
                            idxCol);
                        break;
                }
            }

            if (paranthesis)
                builder.Append(")");
            return levelNo;
        }

        /*
         * Default operator comparison implementation.
         */
        static int DefaultOperator(
            string oper,
            StringBuilder builder,
            Node result,
            Node colNode,
            string escapeChar,
            int level)
        {
            builder.Append($" {oper} ");
            if (result == null)
            {
                // Join invocation.
                var rhs = string.Join(
                    ".",
                    colNode.GetEx<string>()
                        .Split('.')
                        .Select(x => EscapeColumnName(x, escapeChar)));
                builder.Append(rhs);
                return level;
            }
            else
            {
                var argName = "@" + level;
                builder.Append(argName);
                result.Add(new Node(argName, colNode.GetEx<object>()));
                return ++level;
            }
        }

        /*
         * In operator implementation.
         */
        static int InOperator(
            StringBuilder builder,
            Node result,
            Node colNode,
            int level)
        {
            builder.Append(" in (");
            var idxNo = 0;
            foreach (var idx in colNode.Children.Select(x => x.GetEx<object>()).ToArray())
            {
                if (idxNo++ > 0)
                    builder.Append(",");
                builder.Append("@" + level);
                result.Add(new Node("@" + level, idx));
                ++level;
            }
            builder.Append(")");
            return level;
        }

        /*
         * Helper method to escape column names in static methods.
         */
        static string EscapeColumnName(string column, string escapeChar)
        {
            return escapeChar + 
                column.Replace(escapeChar, escapeChar + escapeChar) +
                escapeChar;
        }

        /*
         * Creates a single condition for a where clause.
         */
        int CreateCondition(
            Node result,
            StringBuilder builder,
            int levelNo,
            Node idxCol)
        {
            // Field comparison of some sort.
            var columnName = idxCol.Name;
            if (columnName.StartsWith("\\"))
            {
                // Allowing for escaped column names, to suppor columns containing "." as a part of their names.
                columnName = EscapeColumnName(columnName.Substring(1));
            }
            else if (columnName.Contains("."))
            {
                /*
                 * Notice, for simplicity reasons, and to allow passing in operators
                 * as a single level hierarchy, we allow for an additional method to supply the comparison
                 * operator, which is having the operator to the right of a ".", where the column name is
                 * the first parts.
                 * 
                 * Assuming first part is our operator.
                 */
                var entities = columnName.Split('.');
                var keyword = entities.Last();
                if (_operators.ContainsKey(keyword))
                {
                    columnName = string.Join(
                        ".",
                        entities.Take(entities.Count() - 1).Select(x => EscapeColumnName(x)));
                    builder.Append(columnName);
                    return _operators[keyword](builder, result, idxCol, EscapeChar, levelNo);
                }

                // Checking if last entity is escaped.
                var tmp = new List<string>();
                if (keyword.StartsWith("\\"))
                {
                    keyword = keyword.Substring(1);
                    tmp.AddRange(entities.Take(entities.Count() - 1));
                    tmp.Add(keyword);
                    entities = tmp.ToArray();
                }
                columnName = string.Join(
                    ".",
                    entities.Select(x => EscapeColumnName(x)));
            }
            else
            {
                columnName = EscapeColumnName(columnName);
            }
            builder.Append(columnName)
                .Append(" = ");
            if (result == null)
            {
                // Join invocation.
                var rhs = string.Join(
                    ".",
                    idxCol.GetEx<string>()
                        .Split('.')
                        .Select(x => EscapeColumnName(x)));
                builder.Append(rhs);
                return levelNo;
            }
            else
            {
                var argName = "@" + levelNo;
                builder.Append(argName);
                result.Add(new Node(argName, idxCol.GetEx<object>()));
                return ++levelNo;
            }
        }

        #endregion
    }
}
