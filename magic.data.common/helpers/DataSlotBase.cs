/*
 * Magic Cloud, copyright Aista, Ltd. See the attached LICENSE file for details.
 */

using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using magic.node;
using magic.node.extensions;
using magic.signals.contracts;

namespace magic.data.common.helpers
{
    /// <summary>
    /// Abstract base class for generic slots simply invoking specialized slot for database type.
    /// </summary>
    public abstract class DataSlotBase : ISlot, ISlotAsync
    {
        readonly string _slot;
        readonly IConfiguration _configuration;

        /// <summary>
        /// Creates an instance of your type.
        /// </summary>
        /// <param name="slot">Last parts of the name of slot to signal.</param>
        /// <param name="configuration">Configuration object used to retrieve default database type if no explicit
        /// database type is supplied in arguments.</param>
        protected DataSlotBase(string slot, IConfiguration configuration)
        {
            _configuration = configuration;
            _slot = slot;
        }

        /// <summary>
        /// Implementation of your slot.
        /// </summary>
        /// <param name="signaler">Signaler used to raise the signal.</param>
        /// <param name="input">Arguments to your slot.</param>
        public void Signal(ISignaler signaler, Node input)
        {
            var databaseType = GetDefaultDatabaseType(_configuration, input);
            signaler.Signal($"{databaseType}{_slot}", input);
        }

        /// <summary>
        /// Implementation of your slot.
        /// </summary>
        /// <param name="signaler">Signaler used to raise the signal.</param>
        /// <param name="input">Arguments to your slot.</param>
        /// <returns>An awaitable task.</returns>
        public async Task SignalAsync(ISignaler signaler, Node input)
        {
            var databaseType = GetDefaultDatabaseType(_configuration, input);
            await signaler.SignalAsync($"{databaseType}{_slot}", input);
        }

        #region [ -- Private helper methods -- ]

        static string GetDefaultDatabaseType(IConfiguration configuration, Node input)
        {
            var databaseType = 
                input.Children.FirstOrDefault(x => x.Name == "database-type")?.GetEx<string>() ??
                configuration["magic:databases:default"];
            input.Children.FirstOrDefault(x => x.Name == "database-type")?.UnTie();
            return databaseType;
        }

        #endregion
    }
}
