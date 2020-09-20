using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HGV.Nullifier.Collection.Services
{  
    public interface ILocationService
    {
        int GetRegion(int cluster);
        int GetArea(int region);
    }

    public class LocationService : ILocationService
    {
        private readonly Dictionary<int, int> regionMap;
        private readonly Dictionary<int, int> areaMap;

        public LocationService()
        {
            this.regionMap = new Dictionary<int, int>()
            {
                { 111, 1 },
                { 112, 1 },
                { 113, 1 },
                { 114, 1 },
                { 121, 2 },
                { 122, 2 },
                { 123, 2 },
                { 124, 2 },
                { 131, 3 },
                { 132, 3 },
                { 133, 3 },
                { 134, 3 },
                { 135, 3 },
                { 136, 3 },
                { 137, 3 },
                { 138, 3 },
                { 141, 19 },
                { 142, 19 },
                { 143, 19 },
                { 144, 19 },
                { 145, 19 },
                { 151, 5 },
                { 152, 5 },
                { 153, 5 },
                { 154, 5 },
                { 155, 5 },
                { 156, 5 },
                { 161, 6 },
                { 162, 6 },
                { 163, 6 },
                { 171, 7 },
                { 172, 7 },
                { 181, 8 },
                { 182, 8 },
                { 183, 8 },
                { 184, 8 },
                { 185, 8 },
                { 186, 8 },
                { 187, 8 },
                { 188, 8 },
                { 191, 9 },
                { 192, 9 },
                { 193, 9 },
                { 200, 10 },
                { 201, 10 },
                { 202, 10 },
                { 203, 10 },
                { 204, 10 },
                { 211, 11 },
                { 212, 11 },
                { 213, 11 },
                { 214, 11 },
                { 221, 12 },
                { 222, 12 },
                { 223, 18 },
                { 224, 12 },
                { 225, 17 },
                { 227, 20 },
                { 231, 13 },
                { 232, 25 },
                { 235, 13 },
                { 236, 13 },
                { 241, 14 },
                { 242, 14 },
                { 251, 15 },
                { 261, 16 }
            };

            this.areaMap = new Dictionary<int, int>()
            {
                { 1, 1 },  // Americas (north)
                { 2, 1 },  // Americas (north)
                { 3, 3 },  // Europe
                { 5, 4 },  // Asia
                { 6, 3 },  // Europe
                { 7, 4 },  // Asia
                { 8, 3 },  // Europe
                { 9, 3 },  // Europe
                { 10, 2 }, // Americas (south)
                { 11, 3 }, // Europe
                { 12, 5 }, // Asia (china)
                { 13, 5 }, // Asia (china)
                { 14, 2 }, // Americas (south)
                { 15, 2 }, // Americas (south)
                { 16, 4 }, // Asia
                { 17, 5 }, // Asia (china)
                { 18, 5 }, // Asia (china)
                { 19, 4 }, // Asia
                { 20, 5 }, // Asia (china)
                { 25, 5 }, // Asia (china)
            };
        }

        public int GetArea(int region)
        {
            if(this.areaMap.TryGetValue(region, out int value))
                return value;
            else
                throw new ArgumentOutOfRangeException(nameof(region));
        }

        public int GetRegion(int cluster)
        {
            if(this.regionMap.TryGetValue(cluster, out int value))
                return value;
            else
                throw new ArgumentOutOfRangeException(nameof(cluster));
        }
    }
}
