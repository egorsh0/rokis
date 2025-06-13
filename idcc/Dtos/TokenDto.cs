using System.ComponentModel.DataAnnotations;
using idcc.Infrastructures;

namespace idcc.Dtos;

/// <summary>
/// Представляет токен (GUID), купленный компанией или физическим лицом,
/// и его актуальное состояние.
/// </summary>
/// <remarks>
/// * **Pending** — заказ не оплачен, токен заблокирован.<br/>
/// * **Unused** — оплачен, но ещё не привязан к пользователю.<br/>
/// * **Bound**  — привязан к сотруднику / физлицу, но сессия не начиналась.<br/>
/// * **Used**   — токен использован, сессия завершена (есть сертификат).  
/// </remarks>
/// <param name="Id">Уникальный идентификатор токена (GUID).</param>
/// <param name="DirectionId">Идентификатор направления (справочник <c>Direction</c>).</param>
/// <param name="DirectionName">Человекочитаемое название направления (например «QA»).</param>
/// <param name="UnitPrice">Цена за токен <u>в момент покупки</u>, с учётом валюты системы.</param>
/// <param name="Status">Текущий статус токена: <c>Pending</c>, <c>Unused</c>, <c>Bound</c>, <c>Used</c>.</param>
/// <param name="PurchaseDate">Дата / время (UTC), когда токен был создан в заказе.</param>
/// <param name="Score">Баллы за тестирование по данному токену.</param>
/// <param name="BoundFullName">
/// ФИО пользователя, к которому привязан токен.<br/>
/// <c>null</c> — токен ещё не привязан.
/// </param>
/// <param name="BoundEmail">
/// Email того же пользователя; <c>null</c> — если токен не привязан.
/// </param>
/// <param name="UsedDate">
/// Время окончания сессии (UTC).<br/>
/// Заполняется только для токенов со статусом <c>Used</c>.
/// </param>
/// <param name="CertificateUrl">
/// Ссылка на PDF-сертификат (S3 / CDN).<br/>
/// <c>null</c> — если токен не использован или сертификат ещё не сгенерирован.
/// </param>
public record TokenDto(
    Guid Id,
    int DirectionId,
    string DirectionName,
    decimal UnitPrice,
    TokenStatus Status,
    DateTime PurchaseDate,
    double? Score,
    string? Grade,
    string? BoundFullName,
    string? BoundEmail,
    DateTime? UsedDate,
    string? CertificateUrl
    );

/// <summary>Команда «привязать токен к сотруднику».</summary>
/// <remarks>
/// Отправляется из UI компании - HR вводит email сотрудника.<br/>
/// Если <c>EmployeeEmail</c> не принадлежит этой компании  
/// или токен не в статусе <c>Pending/Unused</c> → 400.
/// </remarks>
/// <param name="TokenId">GUID токена, который нужно привязать.</param>
/// <param name="EmployeeEmail">
/// Email сотрудника (формат проверяется атрибутом <c>[EmailAddress]</c>).
/// </param>
public record BindTokenDto(
    Guid TokenId,
    [EmailAddress] string EmployeeEmail);


/// <summary>Привязка ранее использованного токена к своему профилю.</summary>
/// <param name="TokenId">GUID токена со статусом <c>Used</c>.</param>
/// <param name="UserEmail">
/// Email пользователя, указанный при прохождении теста.<br/>
/// Должен совпадать с Email залогинившегося пользователя.
/// </param>
public record BindUsedTokenDto(
    [Required] Guid TokenId,
    [EmailAddress] string UserEmail);


/// <summary>Короткое представление токена.</summary>
/// <param name="Id">GUID токена.</param>
/// <param name="DirectionId">Идентификатор направления.</param>
/// <param name="DirectionName">Название направления (например «QA»).</param>
/// <param name="Status">
/// Текущий статус: <c>Pending</c>, <c>Unused</c>, <c>Bound</c>, <c>Used</c>.
/// </param>
public record TokenShortDto(
    [Required] Guid Id,
    int DirectionId,
    string DirectionName,
    TokenStatus Status);