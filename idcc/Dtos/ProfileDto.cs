namespace idcc.Dtos;

public record EmployeeProfileDto(
    int    Id,
    string FullName,
    string Email,
    CompanyProfileShortDto? Company);
public record CompanyProfileShortDto(
    int    Id,
    string Name,
    string Inn,
    string Email);

public record CompanyProfileDto(
    int    Id,
    string Name,
    string Inn,
    string Email,
    IEnumerable<EmployeeProfileShortDto> Employees);

public record EmployeeProfileShortDto(
    int Id,
    string FullName,
    string Email);

public record PersonProfileDto(
    int    Id,
    string FullName,
    string Email);