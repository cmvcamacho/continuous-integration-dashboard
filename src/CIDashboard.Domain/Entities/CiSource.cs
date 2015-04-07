using System.ComponentModel;

namespace CIDashboard.Domain.Entities
{
    public enum CiSource
    {
        [Description("")]
        Unknown = 0,

        [Description("TeamCity")]
        TeamCity = 1
    }
}
