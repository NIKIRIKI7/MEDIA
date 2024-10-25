using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MEDIA.src
{
    public interface IDownloadVideoService
    {
        public Task DownloadVideos();
    }
}