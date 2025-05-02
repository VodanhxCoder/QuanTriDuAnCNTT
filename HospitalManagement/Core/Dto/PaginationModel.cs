using System.Collections.Generic;

namespace HospitalManagement.Core.Dto
{
    public class PaginationModel
    {
        public int Index { get; set; }
        public int PageSize { get; set; }
    }

    public class PaginationResponseModel<T>
    {
        public int Total { get; set; }
        public List<T> Items { get; set; }
    }
}