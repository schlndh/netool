namespace Netool.Controllers
{
    public interface IInstanceController
    {
        /// <summary>
        /// Start the associated instance.
        /// </summary>
        void Start();
        /// <summary>
        /// Stop the associated instance.
        /// </summary>
        void Stop();
        /// <summary>
        /// Called when instance tab is about to be closed.
        /// </summary>
        void Close();

        /// <summary>
        /// Show channel detail view
        /// </summary>
        /// <param name="id">channel ID</param>
        void ShowDetail(int id);
    }
}