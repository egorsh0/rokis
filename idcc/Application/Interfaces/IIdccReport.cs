using idcc.Dtos;
using idcc.Models;

namespace idcc.Application.Interfaces;

public interface IIdccReport
{
    Task<ReportDto?> GenerateAsync(Session session);
}