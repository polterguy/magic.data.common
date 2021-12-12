/*
 * Magic Cloud, copyright Aista, Ltd. See the attached LICENSE file for details.
 */

using Microsoft.Extensions.Configuration;
using magic.signals.contracts;
using magic.data.common.helpers;

namespace magic.data.common.slots
{
    /// <summary>
    /// [data.transaction.create] slot, for creating a database transaction,
    /// according to your configuration settings.
    /// </summary>
    [Slot(Name = "data.transaction.create")]
    public class CreateTransaction : DataSlotBase
    {
        /// <summary>
        /// Creates a new instance of your type.
        /// </summary>
        /// <param name="configuration">Configuration for your application.</param>
        public CreateTransaction(IConfiguration configuration)
            : base(".transaction.create", configuration)
        { }
    }
}
