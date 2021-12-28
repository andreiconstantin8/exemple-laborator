using Exemple.Data.Models;
using Exemple.Dto.Events;
using Exemple.Events;
using Exemple.Events.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Exemple.Accomodation.EventProcessor
{
    internal class CosPublishedEventHandler : AbstractEventHandler<IEnumerable<CosDto>>
    {
        public override string[] EventTypes => new string[] { typeof(IEnumerable<CosDto>).Name };

        protected override Task<EventProcessingResult> OnHandleAsync(IEnumerable<CosDto> eventData)
        {
            Console.WriteLine(eventData.ToString());
            return Task.FromResult(EventProcessingResult.Completed);
        }
    }
}
