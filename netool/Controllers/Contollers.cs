using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Netool.Controllers
{
    public interface IServerController
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
