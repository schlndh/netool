namespace Netool.Controllers
{
    public interface IInstanceController
    {
        void Start();

        void Stop();

        /// <summary>
        /// Show channel detail view
        /// </summary>
        /// <param name="id">channel ID</param>
        void ShowDetail(int id);
    }
}