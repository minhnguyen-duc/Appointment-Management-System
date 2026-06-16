namespace Domain.Exceptions;

/// <summary>AC5: Payment must be completed before confirmation.</summary>
public sealed class PaymentRequiredException()
    : Exception("Vui lòng hoàn tất thanh toán trước khi xác nhận lịch khám.") { }
