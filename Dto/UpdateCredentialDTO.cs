namespace Adventure19.Dto
{
    public class UpdateCredentialDTO
    {
        //Nuovo DB
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }


        //Vecchio DB

        public string? CompanyName { get; set; }

        public string? Phone { get; set; }
    }
}
