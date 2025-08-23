using System.ComponentModel.DataAnnotations;
using System.Reflection;
using MetalReleaseTracker.CoreDataService.Configuration;

namespace MetalReleaseTracker.CoreDataService.Extensions;

public static class DistributorCodeExtensions
{
    public static string? TryGetDisplayName(this DistributorCode distributorCode)
    {
        var memberInfo = distributorCode.GetType().GetMember(distributorCode.ToString());

        if (memberInfo.Length > 0)
        {
            var displayAttribute = memberInfo[0].GetCustomAttribute<DisplayAttribute>();
            return displayAttribute?.Name;
        }

        return null;
    }
}