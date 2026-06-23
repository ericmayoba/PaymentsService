using System;
using System.Collections.Generic;
using System.Text;

namespace PaymentsService.Application.Abstractions;
public interface IUnitOfWork
{
    Task BeginTransactionAsync();
    Task CommitAsync();
    Task RollbackAsync();
}
