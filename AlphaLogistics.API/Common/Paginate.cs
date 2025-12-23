namespace WALMS.API.Common
{
    public class Paginate
    {
        public static (List<T> paginatedData, int totalCount) PaginateData<T>(IEnumerable<T> data, int pageNumber, int pageSize)
        {
            var totalCount = data.Count();
            var paginatedData = data.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
            return (paginatedData, totalCount);
        }

    }
}
