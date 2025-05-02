namespace HospitalManagement.Core.Dto.Admin
{
    public class FilterUserModel : PaginationModel
    {
        public string Keyword { get; set; }
        public string OrderField { get; set; }
        public string OrderBy { get; set; }
    }
}