namespace Domain.Exceptions;

/// <summary>AC4: Time slot has reached MaxPatientsPerHour capacity.</summary>
public sealed class SlotFullException(DateTime slot, int max)
    : Exception($"Khung giờ {slot:HH:mm dd/MM/yyyy} đã đủ {max} bệnh nhân. Vui lòng chọn khung giờ khác.") { }
