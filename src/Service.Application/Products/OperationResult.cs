using FluentValidation.Results;

namespace Service.Application.Products;

public sealed class OperationResult<T>
{
    private OperationResult(T? value, ValidationResult? validation)
    {
        Value = value;
        Validation = validation;
    }

    public T? Value { get; }
    public ValidationResult? Validation { get; }

    public static OperationResult<T> Success(T value) =>
        new(value, null);

    public static OperationResult<T> Invalid(ValidationResult validation) =>
        new(default, validation);

    public TResult Match<TResult>(
        Func<T, TResult> onSuccess,
        Func<ValidationResult, TResult> onInvalid)
    {
        return Validation is null
            ? onSuccess(Value!)
            : onInvalid(Validation);
    }
}
