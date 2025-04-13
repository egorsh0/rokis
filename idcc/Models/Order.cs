namespace idcc.Models;

public class Order
{
    public int Id { get; set; }

    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }

    /// <summary>
    /// Ссылка на пользователя, оформившего заказ (Company или Person)
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    // Навигационное свойство
    public virtual ApplicationUser? User { get; set; }
}