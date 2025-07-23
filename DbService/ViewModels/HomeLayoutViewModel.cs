using DbService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbService.ViewModels
{
    public class HomeLayoutViewModel
    {
        public Home Home { get; set; }
        public List<LayoutElement> Layouts { get; set; }
    }

}
