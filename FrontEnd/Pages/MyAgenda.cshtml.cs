using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ConferenceDTO;
using FrontEnd.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace FrontEnd.Pages
{
    [Authorize]
    public class MyAgendaModel : IndexModel
    {
        public MyAgendaModel(IApiClient client, IMemoryCache cache)
            : base(client, cache)
        {
        }
        protected override Task<ConferenceData> GetConferenceDataAsync()
        {
            var sessions = UserSessions;
            return Task.FromResult(GenerateConferenceData(sessions));
        }
    }
}
