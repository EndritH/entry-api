using Microsoft.AspNetCore.Identity;

namespace WebApplication1.Models
{
    public class EntryTag
    {
        public int Id { get; set; }
        public DateTime? ActivationDate { get; set; }

        public bool Status { get; set; }

        public User User { get; set; }


        public bool wasNotActivatedBefore(DateTime before)
        {
            return ActivationDate!=null && ActivationDate > before;
        }

        public bool wasActivatedBeforeNow()
        {
            return ActivationDate != null && ActivationDate < DateTime.Now;
        }
    }
}
