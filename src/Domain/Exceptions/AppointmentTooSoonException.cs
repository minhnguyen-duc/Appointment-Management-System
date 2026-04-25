namespace Domain.Exceptions;

public class AppointmentTooSoonException(int minHours)
    : Exception($"Appointments must be booked at least {minHours} hours in advance.");

public class CapacityExceededException(int maxPerHour)
    : Exception($"This time slot is full. Maximum {maxPerHour} patients per hour.");
