/*
 * Magic Cloud, copyright Aista, Ltd. See the attached LICENSE file for details.
 */

using System.Linq;
using System.Threading.Tasks;
using magic.node;
using magic.node.contracts;
using magic.node.extensions;
using magic.signals.contracts;

namespace magic.data.common.slots.crud
{
    /// <summary>
    /// [data.create] slot, for creating a record in your database,
    /// according to your configuration settings.
    /// </summary>
    [Slot(Name = "data.create")]
    [Slot(Name = "data.read")]
    [Slot(Name = "data.update")]
    [Slot(Name = "data.delete")]
    public class Crud : ISlot, ISlotAsync
    {
        readonly IMagicConfiguration _configuration;

        /// <summary>
        /// Creates a new instance of your type.
        /// </summary>
        /// <param name="configuration">Configuration for your application.</param>
        public Crud(IMagicConfiguration configuration)
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
            var databaseType = 
                input.Children.FirstOrDefault(x => x.Name == "database-type")?.GetEx<string>() ??
                _configuration["magic:databases:default"];
            input.Children.FirstOrDefault(x => x.Name == "database-type")?.UnTie();
            signaler.Signal($"{databaseType}.{GetCrudSlot(input.Name)}", input);
        }

        /// <summary>
        /// Implementation of your slot.
        /// </summary>
        /// <param name="signaler">Signaler used to raise the signal.</param>
        /// <param name="input">Arguments to your slot.</param>
        /// <returns>An awaitable task.</returns>
        public async Task SignalAsync(ISignaler signaler, Node input)
        {
            var databaseType = 
                input.Children.FirstOrDefault(x => x.Name == "database-type")?.GetEx<string>() ??
                _configuration["magic:databases:default"];
            input.Children.FirstOrDefault(x => x.Name == "database-type")?.UnTie();
            await signaler.SignalAsync($"{databaseType}.{GetCrudSlot(input.Name)}", input);
        }

        #region [ -- Private helper methods -- ]

        /*
         * Returns create, read, update or delete, according to what slot was actually invoked by caller.
         */
        static string GetCrudSlot(string name)
        {
            return name.Substring(name.IndexOf('.') + 1);
        }

        #endregion
    }
}
