public class ApiResponse<T>
{
    public int Code { get; set; }
    public string Msg { get; set; } = string.Empty;
    public T? Data { get; set; }  // 允许为 null

    public static ApiResponse<T> Success(T data, string msg = "ok") =>
        new() { Code = 0, Msg = msg, Data = data };

    public static ApiResponse<T> Fail(int code, string msg) =>
        new() { Code = code, Msg = msg, Data = default };
}
