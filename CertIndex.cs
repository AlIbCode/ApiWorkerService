namespace ApiService
{
    public class CertIndex
    {
        public CertIndexMeta meta { get; set; }
        public List<Cert> data { get; set; }
    }

    public class CertIndexMeta
    {
        public string? order { get; set; }
        public string? sort { get; set; }
        public int page { get; set; }
        public int per_page { get; set; }
        public int total_entries { get; set; }
        public int total_pages { get; set; }
    }

    public class Cert
    {
        public int id { get; set; }
        public string? obtained_at { get; set; }
        public string? referenceable_type { get; set; }
        public int? referenceable_id { get; set; }
        public string? name { get; set; }
        public string? referenceable_name { get; set; }
        public string? path { get; set; }
        public User User { get; set; }

    }
}