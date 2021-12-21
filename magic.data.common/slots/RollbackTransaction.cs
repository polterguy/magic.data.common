/*
 * Magic Cloud, copyright Aista, Ltd. See the attached LICENSE file for details.
 */

using magic.node.contracts;
using magic.signals.contracts;
using magic.data.common.helpers;

namespace magic.data.common.slots
{
    /// <summary>
    /// [data.transaction.rollback] slot, for rolling back a database transaction,
    /// according to your configuration settings.
    /// </summary>
    [Slot(Name = "data.transaction.rollback")]
    public class RollbackTransaction : DataSlotBase
    {
        /// <summary>
        /// Creates a new instance of your type.
        /// </summary>
        /// <param name="configuration">Configuration for your application.</param>
        public RollbackTransaction(IMagicConfiguration configuration)
            : base(".transaction.rollback", configuration)
        { }
    }
}
