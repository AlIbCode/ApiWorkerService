namespace ApiService
{
    public class UserIndex
    {

        public MetaData Meta { get; set; }
        public List<User> Data { get; set; }

    }
    public class MetaData
    {
        public string? Order { get; set; }
        public string? Sort { get; set; }
        public int Page { get; set; }
        public int Per_Page { get; set; }
        public int Total_Entries { get; set; }
        public int Total_Pages { get; set; }
    }

    public class User
    {
        public Guid oid { get; set; }
        public int id { get; set; }
        public string? email { get; set; }
        public string? username { get; set; }
        public string? full_name { get; set; }
        public string? first_name { get; set; }
        public string? last_name { get; set; }
        public string? personal_number { get; set; }
        public DateTime? date_of_entry { get; set; }
        public DateTime? date_of_leaving { get; set; }
        public string? locale { get; set; }
        public string? time_zone { get; set; }
        public DateTime? last_sign_in_at { get; set; }
        public DateTime? created_at { get; set; }
        public DateTime? updated_at { get; set; }
        public UserInformation? user_information { get; set; }
        public List<Role>? roles { get; set; }
        public List<object>? companies { get; set; }
        public List<object>? departments { get; set; }
        public List<object>? locations { get; set; }
        public List<object>? jobs { get; set; }
        public List<object>? cost_units { get; set; }
        public List<object>? authentifications { get; set; }
        public ICollection<Cert> Certs { get; set; }
    }


    public class UserInformation
    {
        public DateTime? Birth_Day { get; set; }
        public string? Description { get; set; }
        public string? Gender { get; set; }
        public string? Secondary_Email { get; set; }
        public string? User_Function { get; set; }
        public string? Phone { get; set; }
    }

    public class Role
    {
        public int id { get; set; }
        public string? name_human { get; set; }
    }
}