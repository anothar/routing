using Itinero.Profiles;
using System;
using Itinero.Attributes;
using System.Collections.Generic;

namespace Itinero.FastVehicle
{
    public class CarVehicle : Vehicle
    {
        private HashSet<string> _profileWhiteList
           = new HashSet<string>() {
               "highway",
                "oneway",
                "motorcar",
                "motor_vehicle",
                "vehicle",
                "access",
                "maxspeed",
                "maxweight",
                "maxwidth",
                "junction",
                "route"
           };
        private HashSet<string> _metaWhiteList
            = new HashSet<string>();

        public CarVehicle()
        {
            _metaWhiteList.Add("name");
        }

        public override string Name
        {
            get
            {
                return "car";
            }
        }

        private static string[] Vehicles = new[] { "vehicle", "motor_vehicle", "motorcar" };

        public override string[] VehicleTypes
        {
            get
            {
                return Vehicles;
            }
        }

        public override HashSet<string> MetaWhiteList
        {
            get
            {
                return _metaWhiteList;
            }
        }

        public override HashSet<string> ProfileWhiteList
        {
            get
            {
                return _profileWhiteList;
            }
        }

        public override bool AddToWhiteList(IAttributeCollection attributes, Whitelist whitelist)
        {
            return CarProfile.FactorAndSpeed(attributes, whitelist).SpeedFactor > 0;
        }

        public override FactorAndSpeed FactorAndSpeed(IAttributeCollection attributes, Whitelist whitelist)
        {
            throw new NotImplementedException();
        }
    }
}
