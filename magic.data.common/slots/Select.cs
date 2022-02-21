﻿/*
 * Magic Cloud, copyright Aista, Ltd. See the attached LICENSE file for details.
 */

using magic.signals.contracts;
using magic.data.common.helpers;
using magic.data.common.contracts;

namespace magic.data.common.slots
{
    /// <summary>
    /// [data.select] slot, for executing some SQL towards a database and returning a record result,
    /// according to your configuration settings.
    /// </summary>
    [Slot(Name = "data.select")]
    public class Select : DataSlotBase
    {
        /// <summary>
        /// Creates a new instance of your type.
        /// </summary>
        /// <param name="settings">Configuration object.</param>
        public Select(IDataSettings settings)
            : base(".select", settings)
        { }
    }
}
