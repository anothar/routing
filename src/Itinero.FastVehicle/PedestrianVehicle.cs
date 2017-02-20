using Itinero.Profiles;
using System;
using System.Collections.Generic;
using Itinero.Attributes;

namespace Itinero.FastVehicle
{
    public class PedestrianVehicle:Vehicle
    {
        private HashSet<string> _profileWhiteList
            = new HashSet<string>();
        private HashSet<string> _metaWhiteList
            = new HashSet<string>();

        public PedestrianVehicle():base()
        {
            _profileWhiteList.Add("highway");
            _profileWhiteList.Add("foot");
            _profileWhiteList.Add("access");
            _profileWhiteList.Add("footway");
            _metaWhiteList.Add("name");
        }

        public override string Name
        {
            get
            {
                return "pedestrian";
            }
        }

        private static string[] Vehicles = new[] { "foot",
        "pedestrian"};

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
            return PedestrianProfile.FactorAndSpeed(attributes, whitelist).SpeedFactor>0;
        }

        public override FactorAndSpeed FactorAndSpeed(IAttributeCollection attributes, Whitelist whitelist)
        {
            throw new NotImplementedException();
        }
    }
}
