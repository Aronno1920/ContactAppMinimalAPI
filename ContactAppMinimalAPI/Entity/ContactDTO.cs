namespace ContactAppMinimalAPI.Entity
{
    public class ContactDTO
    {
        public String FirstName { get; set; }
        public String? LastName { get; set; }
        public String? Office { get; set; }
        public String MobileNo { get; set; }
        public String? Email { get; set; }
        public DateTime? Birthday { get; set; }
    }
}
