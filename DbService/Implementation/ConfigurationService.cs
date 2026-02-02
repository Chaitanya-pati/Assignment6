
using DbService.Interface;
using DbService.Models;
using DbService.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static Azure.Core.HttpHeader;

namespace DbService.Implementation
{
    public class ConfigurationService : IConfigurationService
    {
        private readonly DbContextOptions<Assignment6Context> _dbconnection;

        public ConfigurationService(string conn)
        {
            _dbconnection = new DbContextOptionsBuilder<Assignment6Context>().UseSqlServer(conn).Options;
        }

        public List<Home> GetHomeMaster()
        {
            List<Home> homes = new List<Home>();
            using (var db = new Assignment6Context(_dbconnection))
            {
                homes = db.Homes.Include(h => h.Rooms).ToList();

            }
            return homes;
        }

        public Home SaveHome(Home home)
        {
            Home savedHome = new Home();
            using (var db = new Assignment6Context(_dbconnection))
            {
                db.Homes.Add(home);
                db.SaveChanges();
                savedHome = db.Homes.Where(x => x.Id == home.Id).FirstOrDefault();
                return savedHome;
            }
        }

        public Home UpdateHome(Home home)
        {
            using (var db = new Assignment6Context(_dbconnection))
            {
                db.Homes.Update(home);
                db.SaveChanges();
                return db.Homes.Where(x => x.Id == home.Id).FirstOrDefault();
            }
        }

        public List<Room> SaveRooms(List<Room> rooms)
        {
            List<Room> savedRooms = new List<Room>();
            using (var db = new Assignment6Context(_dbconnection))
            {
                foreach (var room in rooms)
                {
                    room.CreatedAt = DateTime.Now;

                    if (room.Id == 0)
                    {
                        db.Rooms.Add(room);
                    }

                }
                db.SaveChanges();
            }

            return rooms;
        }



        public bool SaveLayoutElement(List<LayoutElement> layoutElements)
        {
            bool isSaved = false;
            using (var db = new Assignment6Context(_dbconnection))
            {
                try
                {
                    foreach (var element in layoutElements)
                    {
                        if (element.Id == 0)
                        {
                            db.LayoutElements.Add(element);
                        }
                    }
                    db.SaveChanges();
                    isSaved = true;
                }
                catch (Exception)
                {
                    isSaved = false;
                }
            }
            return isSaved;
        }

        public List<LayoutElement> GetLayout(int homeId)
        {
            var layoutElements = new List<LayoutElement>();
            using (var db = new Assignment6Context(_dbconnection))
            {
                layoutElements = db.LayoutElements.Where(x => x.HomeId == homeId).ToList();
            }
            return layoutElements;
        }
    }
}
