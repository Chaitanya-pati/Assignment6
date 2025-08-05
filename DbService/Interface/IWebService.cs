using DbService.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbService.Interface
{
    public interface IWebService
    {
        public HomeMasterViewModel GetWebSiteData();
    }
}
