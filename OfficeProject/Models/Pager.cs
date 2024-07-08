namespace OfficeProject.Models
{
    public class Pager
    {
        public int TotalItems { get; private set; }
        public int TotalPages { get; private set; }
        public int CurrentPage { get; private set; }
        public int PageSize { get; private set; }
        public int StartPage { get; private set; }
        public int EndPage { get; private set; }

        public Pager()
        {
            // Default constructor
        }

        public Pager(int totalItems, int page, int pageSize = 10)
        {
            TotalItems = totalItems;
            PageSize = pageSize;

            // Calculate total pages after assigning PageSize
            TotalPages = (int)Math.Ceiling((decimal)TotalItems / PageSize);
            CurrentPage = page;

            StartPage = CurrentPage - 5;
            EndPage = CurrentPage + 4;

            if (StartPage <= 0)
            {
                EndPage -= (StartPage - 1);
                StartPage = 1;
            }

            if (EndPage > TotalPages)
            {
                EndPage = TotalPages;
                if (EndPage > 10)
                {
                    StartPage = EndPage - 9;
                }
            }
        }
    }
}
