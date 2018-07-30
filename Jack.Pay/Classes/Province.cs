using System;
using System.Collections.Generic;
using System.Text;

namespace Jack.Pay
{
    public class Province
    {
        public string Name { get; set; }
        public string Id { get; set; }

        List<City> _Cities;
        public List<City> Cities {
            get
            {
                return _Cities ?? (_Cities = new List<City>());
            }
        }
    }

    public class City
    {
        public string Name { get; set; }
        public string Id { get; set; }

        List<CityArea> _CityAreas;
        public List<CityArea> CityAreas
        {
            get
            {
                return _CityAreas ?? (_CityAreas = new List<CityArea>());
            }
        }
    }

    public class CityArea
    {
        public string Name { get; set; }
        public string Id { get; set; }
    }
}
