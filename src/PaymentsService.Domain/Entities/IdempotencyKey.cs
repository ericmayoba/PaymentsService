using System;
using System.Collections.Generic;
using System.Text;

namespace PaymentsService.Domain.Entities;
public class IdempotencyKey
{
    public Guid Key { get; set; }
    public DateTime CreatedAt { get; set; }

    public IdempotencyKey() { }

    public IdempotencyKey(Guid key)
    {
        Key = key;
        CreatedAt = DateTime.UtcNow;
    }
}

