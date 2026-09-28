namespace HmCrudPLusAuth.Common
{
    public class Result<T>
    {
        public int Status { get; set; }
        public T? Value { get; set; }
        public string? Message { get; set; }
        public Result(int status, T? value, string? msg)
        {

            Status = status;
            Value = value;
            Message = msg;

        }

        public static Result<T> Ok(T value) => new(200, value, default);
        public static Result<T> BadRequest(string msg) => new(400, default, msg);
        public static Result<T> NotFound(string msg) => new(404, default, msg);

    }
}
