using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackEnd.Data
{
    public class Attendee : ConferenceDTO.Attendee
    {
        public virtual ICollection<SessionAttendee> SessionAttendees { get; set; }

    }
}
