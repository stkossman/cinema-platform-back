namespace Cinema.Domain.Enums;

public enum OrderStatus: short
{
    Pending = 0,
    Paid = 1,
    Failed = 2,
    Cancelled = 3
}