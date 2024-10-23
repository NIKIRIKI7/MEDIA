using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MEDIA.src
{
    public interface IDownloadVideo
    {
        public Task DownloadVideosAsync();
        public Task DownloadVideosAsync(List<string> videos, string downloadPathpath);
    }
}