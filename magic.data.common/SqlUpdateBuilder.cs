/*
 * Magic Cloud, copyright Aista, Ltd. See the attached LICENSE file for details.
 */

using System;
using System.Linq;
using System.Text;
using magic.node;
using magic.node.extensions;
using magic.data.common.helpers;

namespace magic.data.common
{
    /// <summary>
    /// Specialised update SQL builder, to create a select SQL statement by semantically traversing an input node.
    /// </summary>
    public class SqlUpdateBuilder : SqlWhereBuilder
    {
        /// <summary>
        /// Creates an update SQL statement
        /// </summary>
        /// <param name="node">Root node to generate your SQL from.</param>
        /// <param name="escapeChar">Escape character to use for escaping table names etc.</param>
        public SqlUpdateBuilder(Node node, string escapeChar)
            : base(node, escapeChar)
        { }

        /// <summary>
        /// Builds your update SQL statement, and returns a structured SQL statement, plus any parameters.
        /// </summary>
        /// <returns>Node containing update SQL as root node, and parameters as children.</returns>
        public override Node Build()
        {
            // Return value.
            var result = new Node("sql");
            var builder = new StringBuilder();

            // Starting build process.
            builder.Append("update ");
            AppendTableName(builder);
            builder.Append(" set ");
            AppendValues(builder, result);
            AppendWhere(builder, result);

            // Returning result to caller.
            result.Value = builder.ToString();
            return result;
        }

        #region [ -- Private helper methods -- ]

        /*
         * Appends values, and adds values into argument node.
         */
        void AppendValues(StringBuilder builder, Node args)
        {
            var valuesNodes = Root.Children.Where(x => x.Name == "values");
            if (!valuesNodes.Any())
                throw new HyperlambdaException($"Missing [values] node in '{GetType().FullName}'");

            var valuesNode = valuesNodes.First();
            if (!valuesNode.Children.Any())
                throw new HyperlambdaException($"No actual [values] provided to '{GetType().FullName}'");

            var idxNo = 0;
            var first = true;
            foreach (var idxCol in valuesNode.Children)
            {
                if (!first)
                    builder.Append(", ");
                first = false;

                builder.Append(EscapeColumnName(idxCol.Name));
                if (idxCol.Value == null)
                {
                    builder.Append(" = null");
                    continue;
                }
                builder.Append(" = @v" + idxNo);
                args.Add(new Node("@v" + idxNo, idxCol.GetEx<object>()));
                ++idxNo;
            }
        }

        #endregion
    }
}
