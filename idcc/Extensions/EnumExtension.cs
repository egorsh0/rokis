using System.ComponentModel;
using System.Reflection;

namespace idcc.Extensions;

public static class EnumExtension
{
    /// <summary>
    /// Получить атрибут "Описание"
    /// </summary>
    /// <param name="value">Значение перечисления</param>
    /// <returns>Значение атрибута "Описание"</returns>
    public static string GetDescription(this Enum value)
    {
        FieldInfo? fi = value.GetType().GetField(value.ToString());

        DescriptionAttribute[] attributes =
            (DescriptionAttribute[])fi?.GetCustomAttributes(
                typeof(DescriptionAttribute),
                false)!;

        return attributes.Length > 0 ? attributes[0].Description : value.ToString();
    }
}