using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MEDIA.src
{
    public interface IDownloadContentService
    {
        public Task DownloadContents();
    }
}