namespace WhatsAppServices.API.DTO
{
    public class PagedResponse<T>
    {
        public IEnumerable<T> Data { get; init; } = Enumerable.Empty<T>();

        public int PageNumber { get; init; }

        public int PageSize { get; init; }

        public int TotalCount { get; init; }

        public int TotalPages { get; init; }

        public bool HasNextPage => PageNumber < TotalPages;

        public bool HasPreviousPage => PageNumber > 1;
    }
}
