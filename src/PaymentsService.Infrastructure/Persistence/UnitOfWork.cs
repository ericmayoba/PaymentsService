using Microsoft.EntityFrameworkCore.Storage;
using PaymentsService.Application.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace PaymentsService.Infrastructure.Persistence;
public class UnitOfWork : IUnitOfWork
{
    private readonly PaymentsDbContext _db;
    private IDbContextTransaction? _tx;

    public UnitOfWork(PaymentsDbContext db) => _db = db;

    public async Task BeginTransactionAsync() =>
        _tx = await _db.Database.BeginTransactionAsync();

    public async Task CommitAsync()
    {
        if (_tx is not null) await _tx.CommitAsync();
    }

    public async Task RollbackAsync()
    {
        if (_tx is not null) await _tx.RollbackAsync();
    }
}
