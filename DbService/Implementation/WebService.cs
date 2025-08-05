using DbService.Interface;
using DbService.Models;
using DbService.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DbService.Implementation
{
    public class WebService : IWebService
    {
        private readonly DbContextOptions<Assignment6Context> _dbconnection;
        public WebService(string conn)
        {
            _dbconnection = new DbContextOptionsBuilder<Assignment6Context>().UseSqlServer(conn).Options;
        }

        public HomeMasterViewModel GetWebSiteData()
        {
            using (var db = new Assignment6Context(_dbconnection))
            {
                HomeMasterViewModel homeMasterViewModel = new HomeMasterViewModel
                {

                    Bookings = db.Bookings.ToList(),
                    Homes = db.Homes.ToList(),
                    Rooms = db.Rooms.ToList(),
                    BookingRooms = db.BookingRooms.ToList()
                };

                return homeMasterViewModel;
            }
        }
    }
}
