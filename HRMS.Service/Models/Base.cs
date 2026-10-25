namespace HRMS.Service.Base
{
    public class BaseResponse<T>
    {
        public T? Result { get; set; }

        public string? Message { get; set; }

        public int Total { get; set; }
    }

    public class BasePaging
    {
        public string? SortBy { get; set; }

        public string? Column { get; set; }

        public int Length { get; set; }

        public int Start { get; set; }
    }
}
