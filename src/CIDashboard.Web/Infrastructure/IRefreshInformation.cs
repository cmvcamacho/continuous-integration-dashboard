namespace CIDashboard.Web.Infrastructure
{
    public interface IRefreshInformation 
    {
        void AddBuilds(string username, string connectionId);
        
        void RemoveBuilds(string connectionId);

        void RefreshBuilds();
    }
}