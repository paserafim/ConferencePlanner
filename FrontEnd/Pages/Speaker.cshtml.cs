using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ConferenceDTO;
using FrontEnd.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FrontEnd.Pages
{
    public class SpeakerModel : PageModel
    {
        private readonly IApiClient _apiclient;
        public SpeakerModel(IApiClient apiClient)
        {
            _apiclient = apiClient;
        }

        public SpeakerResponse Speaker { get; set; }

        public async Task<IActionResult> OnGet(int id)
        {
            Speaker = await _apiclient.GetSpeakerAsync(id);

            if (Speaker == null)
            {
                return NotFound();
            }

            return Page();

        }
    }
}
