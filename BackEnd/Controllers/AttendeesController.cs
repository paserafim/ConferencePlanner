using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BackEnd.Data;
using BackEnd.Infrastructure;
using ConferenceDTO;

namespace BackEnd.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AttendeesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AttendeesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpDelete("{username}/session/{sessionId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> RemoveSession(string username, int sessionId)
        {
            var attendee = await _context.Attendees
                                 .Include(a => a.SessionAttendees)
                                 .SingleOrDefaultAsync(a => a.UserName == username);

            if (attendee == null)
            {
                return NotFound();
            }

            var session = await _context.Sessions.FindAsync(sessionId);

            if (session == null)
            {
                return BadRequest();
            }

            var sessionAttendee = attendee.SessionAttendees.FirstOrDefault(sa => sa.SessionId == sessionId);
            attendee.SessionAttendees.Remove(sessionAttendee);

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPost("{username}/session/{sessionId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public async Task<ActionResult<AttendeeResponse>> AddSession(string username, int sessionId)
        {
            var attendee = await _context.Attendees
                                 .Include(a => a.SessionAttendees)
                                 .ThenInclude(sa => sa.Session)
                                 .SingleOrDefaultAsync(a => a.UserName == username);

            if (attendee == null)
            {
                return NotFound();
            }

            var session = await _context.Sessions.FindAsync(sessionId);

            if (session == null)
            {
                return BadRequest();
            }

            attendee.SessionAttendees.Add(new SessionAttendee
            {
                AttendeeId = attendee.Id,
                SessionId = sessionId
            });

            await _context.SaveChangesAsync();

            var result = attendee.MapAttendeeResponse();

            return result;
        }

        // GET: api/Attendees
        [HttpGet("{username}")]
        public async Task<ActionResult<AttendeeResponse>> Get(string username)
        {
            var attendee = await _context.Attendees.Include(a => a.SessionAttendees)
                                             .ThenInclude(sa => sa.Session)
                                             .SingleOrDefaultAsync(a => a.UserName == username);

            if (attendee == null)
            {
                return NotFound();
            }

            return attendee.MapAttendeeResponse();
        }

        // POST: api/Attendees
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<AttendeeResponse>> PostAttendee(ConferenceDTO.Attendee input)
        {
            // Varificar se existe o Atendee
            var existingAttendee = await _context.Attendees
                                        .Where(a => a.UserName == input.UserName)
                                        .FirstOrDefaultAsync();

            if (existingAttendee != null)
            {
                return Conflict(input);
            }

            var attendee = new Data.Attendee
            {
                FirstName = input.FirstName,
                LastName = input.LastName,
                UserName = input.UserName,
                EmailAddress = input.EmailAddress
            };

            _context.Attendees.Add(attendee);

            await _context.SaveChangesAsync();

            var result = attendee.MapAttendeeResponse();

            return CreatedAtAction(nameof(Get), new { username = result.UserName }, result);
        }

        [HttpGet("{username}/sessions")]
        public async Task<ActionResult<List<SessionResponse>>> GetSessions(string username)
        {
            var sessions = await _context.Sessions.AsNoTracking()
                                                .Include(s => s.Track)
                                                .Include(s => s.SessionSpeakers)
                                                    .ThenInclude(ss => ss.Speaker)
                                                .Where(s => s.SessionAttendees.Any(sa => sa.Attendee.UserName == username))
                                                .Select(m => m.MapSessionResponse())
                                                .ToListAsync();
            return sessions;
        }
    }
}
