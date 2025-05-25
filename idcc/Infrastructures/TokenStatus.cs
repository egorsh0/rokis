namespace idcc.Infrastructures;

/// <summary>Этап жизненного цикла токена.</summary>
/// <remarks>
/// • <c>Pending</c>  – заказ создан, но ещё **не оплачен**.<br/>
/// • <c>Unused</c>   – платёж прошёл, токен свободен.<br/>
/// • <c>Bound</c>    – привязан к сотруднику или физлицу.<br/>
/// • <c>Used</c>     – токен отработал; сессия завершена.
/// </remarks>
public enum TokenStatus { Pending, Unused, Bound, Used }