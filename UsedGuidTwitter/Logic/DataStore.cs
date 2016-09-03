using System;
using System.Linq;

namespace UsedGuidTwitter.Logic
{
    public interface IManageYourData
    {
        bool SaveGuid(Guid guid);
        bool GuidExistsAlready(Guid guid);
    }

    public class DataStore : IManageYourData
    {
        public bool SaveGuid(Guid g)
        {
            using (var db = new CloudDbContext())
            {
                var guid = new UsedGuid { Guid = g };

                db.UsedGuids.Add(guid);
                return db.SaveChanges() > 0;
            }
        }

        public bool GuidExistsAlready(Guid g)
        {
            using (var db = new CloudDbContext())
            {
                return (from b in db.UsedGuids
                        where b.Guid == g
                        select b)
                        .Any();
            }
        }
    }
}