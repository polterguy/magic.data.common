﻿/*
 * Magic Cloud, copyright Aista, Ltd. See the attached LICENSE file for details.
 */

using magic.signals.contracts;
using magic.data.common.helpers;
using magic.data.common.contracts;

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
        /// <param name="settings">Configuration object.</param>
        public RollbackTransaction(IDataSettings settings)
            : base(".transaction.rollback", settings)
        { }
    }
}
