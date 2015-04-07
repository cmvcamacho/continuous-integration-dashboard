using System.ComponentModel;

namespace CIDashboard.Web.Models
{
    public enum CiSource
    {
        [Description("")]
        Unknown = 0,

        [Description("TeamCity")]
        TeamCity = 1
    }
}