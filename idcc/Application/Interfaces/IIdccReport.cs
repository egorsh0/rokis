using idcc.Models;
using idcc.Models.Dto;

namespace idcc.Application.Interfaces;

public interface IIdccReport
{
    Task<ReportDto?> GenerateAsync(Session session);
}