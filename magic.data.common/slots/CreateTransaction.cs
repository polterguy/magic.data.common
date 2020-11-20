/*
 * Magic, Copyright(c) Thomas Hansen 2019 - 2020, thomas@servergardens.com, all rights reserved.
 * See the enclosed LICENSE file for details.
 */

using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using magic.node;
using magic.signals.contracts;

namespace magic.data.common.slots
{
    /// <summary>
    /// [data.transaction.create] slot, for creating a database transaction,
    /// according to your configuration settings.
    /// </summary>
    [Slot(Name = "data.transaction.create")]
    public class CreateTransaction : ISlot, ISlotAsync
    {
        readonly IConfiguration _configuration;

        /// <summary>
        /// Creates a new instance of your type.
        /// </summary>
        /// <param name="configuration">Configuration for your application.</param>
        public CreateTransaction(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Implementation of your slot.
        /// </summary>
        /// <param name="signaler">Signaler used to raise the signal.</param>
        /// <param name="input">Arguments to your slot.</param>
        public void Signal(ISignaler signaler, Node input)
        {
            var databaseType = _configuration["magic:databases:default"];
            signaler.Signal($"{databaseType}.transaction.create", input);
        }

        /// <summary>
        /// Implementation of your slot.
        /// </summary>
        /// <param name="signaler">Signaler used to raise the signal.</param>
        /// <param name="input">Arguments to your slot.</param>
        /// <returns>An awaitable task.</returns>
        public async Task SignalAsync(ISignaler signaler, Node input)
        {
            var databaseType = _configuration["magic:databases:default"];
            await signaler.SignalAsync($"{databaseType}.transaction.create", input);
        }
    }
}
