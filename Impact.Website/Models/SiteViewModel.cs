namespace Impact.Website.Models
{
    public class SiteViewModel
    {
        public SiteViewModel(string fullName)
        {
            FullName = fullName;
        }

        public string FullName { get; set; }
    }
}