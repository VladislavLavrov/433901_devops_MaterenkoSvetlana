using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Calculator.Models
{
    public static class EnumExtensions
    {
        public static string GetDisplayName(this Enum val)
        {
            return val.GetType()
                .GetMember(val.ToString())
                .FirstOrDefault()
                ?.GetCustomAttribute<DisplayAttribute>(false)
                ?.Name
                ?? val.ToString();
        }
    }
}