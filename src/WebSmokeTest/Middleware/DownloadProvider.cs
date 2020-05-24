using System;
using System.Collections.Generic;

using Middleware;
using Middleware.Downloads;

namespace SmokeTest.Middleware
{
    public class DownloadProvider : IDownloadProvider
    {
        public List<DownloadCategory> DownloadCategoriesGet(in long userId)
        {
            throw new NotImplementedException();
        }

        public List<DownloadCategory> DownloadCategoriesGet()
        {
            throw new NotImplementedException();
        }

        public DownloadItem GetDownloadItem(in int fileId)
        {
            throw new NotImplementedException();
        }

        public DownloadItem GetDownloadItem(in long userId, in int fileId)
        {
            throw new NotImplementedException();
        }

        public void ItemDownloaded(in long userId, in int fileId)
        {
            throw new NotImplementedException();
        }

        public void ItemDownloaded(in int fileId)
        {
            throw new NotImplementedException();
        }
    }
}
