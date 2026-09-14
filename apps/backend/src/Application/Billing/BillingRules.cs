using Kcow.Domain.Entities;

namespace Kcow.Application.Billing;

public static class BillingRules
{
    public static void ValidateInvoiceLink(int studentId, Invoice invoice)
    {
        if (invoice.StudentId != studentId)
            throw new InvalidOperationException("Invoice does not belong to this student");
        if (invoice.Status == 3)
            throw new InvalidOperationException("A cancelled invoice cannot receive payments");
    }

    public static string InvoiceStatus(int status) => status switch
    {
        0 => "Pending", 1 => "Paid", 2 => "Overdue", 3 => "Cancelled", _ => $"Unknown({status})"
    };
}
