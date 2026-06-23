using System;
using System.Collections.Generic;
using System.Text;

namespace PaymentsService.Application.Abstractions;
public interface IIdempotencyRepository
{
    Task<bool> ExistsAsync(Guid key);
    Task SaveAsync(Guid key);
}
