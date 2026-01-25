namespace Cinema.Domain.Shared;

public class Result
{
    public Error Error { get; }
    public bool IsSuccess => Error == Error.None;
    public bool IsFailure => !IsSuccess;

    protected Result(Error error)
    {
        Error = error;
    }

    public static Result Success() => new(Error.None);

    public static Result Failure(Error error) => new(error);
    
    public static Result<TValue> Success<TValue>(TValue value) => new(value, Error.None);
    
    public static Result<TValue> Failure<TValue>(Error error) => new(default, error);
}

public class Result<TValue> : Result
{
    private readonly TValue? _value;

    public Result(TValue? value, Error error) : base(error)
    {
        _value = value;
    }

    public TValue Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("The value of a failure result can not be accessed.");
    
    public static Result<TValue> Success(TValue value) => new(value, Error.None);
    
    public static new Result<TValue> Failure(Error error) => new(default, error);
    
    public static implicit operator Result<TValue>(TValue? value) => Create(value);

    public static Result<TValue> Create(TValue? value) =>
        value is not null ? Success(value) : Failure(Error.NullValue);
}