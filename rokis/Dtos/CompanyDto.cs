namespace rokis.Dtos;

/// <summary>
/// Краткая публичная информация о компании.<br/>
/// </summary>
/// <param name="Name">
/// Полное или сокращённое наименование компании  
/// </param>
/// <param name="Inn">
/// ИНН — идентификационный номер (10 / 12 цифр).
/// </param>
/// <param name="Email">
/// Основной корпоративный email для связи (уникален в системе).
/// </param>
public record CompanyInfoDto(
    string Name,
    string Inn,
    string Email);