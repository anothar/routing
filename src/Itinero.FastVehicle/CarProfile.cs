using Itinero.Profiles;
using System;
using System.Collections.Generic;
using Itinero.Attributes;
using Itinero.Navigation.Instructions;

namespace Itinero.FastVehicle
{
    public class CarProfile : Profile
    {
        private static Dictionary<String, int> _speedProfiles =
            new Dictionary<string, int>
            {
                {"motorway",120 },
                {"motorway_link",120 },
                {"trunk",90 },
                {"trunk_link",90 },
                {"primary",90 },
                {"primary_link",90 },
                {"secondary",70 },
                {"secondary_link",70 },
                {"tertiary",70 },
                {"tertiary_link",70 },
                {"unclassified",50 },
                {"residential",50 },
                {"service",30 },
                {"services",30 },
                {"road",30 },
                {"track",30 },
                {"living_street",5 },
                {"ferry",5 },
                {"movable",5 },
                {"shuttle_train",10 },
                {"default",10 }
            };
        private static HashSet<String> _profileWhitelist =
            new HashSet<string>
            {
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
        private static Dictionary<String, bool> AccessValues =
            new Dictionary<string, bool>
            {
                {"private",false},
                {"yes",true },
                {"no",false },
                {"permissive",true },
                {"destination",true },
                {"customers",false },
                {"designated",true },
                {"public",true },
                {"delivery",true},
                {"use_sidepath",false }
            };

        private static string[] Vehicles = new[] { "vehicle", "motor_vehicle", "motorcar" };

        private const int _minSpeed = 30;
        private const int _maxSpeed = 200;

        public CarProfile() : base("fastcar", ProfileMetric.TimeInSeconds,
           Vehicles, null, new CarVehicle())
        {
            _instructionGenerator = new FastInstructionGenerator(this);
        }

        //interprets access tags
        private static bool? CanAccess(IAttributeCollection attributes)
        {

            bool? last_access = null;
            string accessValue = null;
            if (attributes.TryGetValue("access", out accessValue) &&
                    AccessValues.ContainsKey(accessValue))
            {
                var access = AccessValues[accessValue];
                last_access = access;
            }
            foreach (var vtype in Vehicles)
            {
                string accessKey = null;
                if (attributes.TryGetValue(vtype, out accessKey) && AccessValues.ContainsKey(accessKey))
                {
                    var access = AccessValues[accessKey];
                    last_access = access;
                }
            }
            return last_access;
        }

        private static short? IsOneway(IAttributeCollection attributes, Whitelist whitelist, String name)
        {
            String oneway = null;
            if (attributes.TryGetValue(name, out oneway))
            {
                whitelist.Add(name);
                if (!String.IsNullOrEmpty(oneway))
                {
                    if (oneway == "yes" ||
                        oneway == "true" ||
                       oneway == "1")
                        return 1;
                    if (oneway == "-1")
                        return 2;
                }
            }
            return null;
        }


        internal static FactorAndSpeed FactorAndSpeed(IAttributeCollection attributes,
           Whitelist whitelist)
        {
            if (attributes == null || attributes.Count == 0)
            {
                return Profiles.FactorAndSpeed.NoFactor;
            }
            string highway = null;
            if (attributes.TryGetValue("highway", out highway))
                whitelist.Add("highway");
            var result = new FactorAndSpeed();
            var speed = 0.0f;
            short direction = 0;
            var canstop = true;
            //set highway to ferry when ferry.
            string route = null;
            if (attributes.TryGetValue("route", out route))
                whitelist.Add("route");
            if (route == "ferry")
            {
                highway = "ferry";
            }

            if(String.IsNullOrEmpty(highway))
                return Itinero.Profiles.FactorAndSpeed.NoFactor;
            //get default speed profiles
            var highway_speed = _speedProfiles.ContainsKey(highway) ? (int?)_speedProfiles[highway] : null;

            if (highway_speed != null)
            {
                speed = highway_speed.Value;
                direction = 0;
                canstop = true;
                if (highway == "motorway" ||
                   highway == "motorway_link")
                    canstop = false;
            }
            else
                return Itinero.Profiles.FactorAndSpeed.NoFactor;

            if (CanAccess(attributes) == false)
                return Itinero.Profiles.FactorAndSpeed.NoFactor;

            //get maxspeed if any.
            string maxSpeed = null;
            if (attributes.TryGetValue("maxspeed", out maxSpeed))
            {
                whitelist.Add("maxspeed");
                float lspeed;
                if (float.TryParse(maxSpeed, out lspeed))
                    speed = lspeed * 0.75f;
            }

            //get maxweight and maxwidth constraints if any
            var maxweight = 0.0f;
            var maxwidth = 0.0f;
            string maxWeightString = null;
            if (attributes.TryGetValue("maxweight", out maxWeightString))
            {
                whitelist.Add("maxweight");
                float.TryParse(maxWeightString, out maxweight);
            }

            string maxWidthString = null;
            if (attributes.TryGetValue("maxwidth", out maxWidthString))
            {
                whitelist.Add("maxwidth");
                float.TryParse(maxWidthString, out maxwidth);
            }

            if (maxwidth != 0 || maxweight != 0)
            {
                result.Constraints = new[]
                    { maxweight, maxwidth};
            }

            //get directional information
            String junction = null;
            if (attributes.TryGetValue("junction", out junction))
            {
                whitelist.Add("junction");
                if (junction == "roundabout")
                    direction = 1;
            }
            var ldirection = IsOneway(attributes,whitelist, "oneway");

            if (ldirection != null)
                direction = ldirection.Value;
            if (speed == 0)
                return Itinero.Profiles.FactorAndSpeed.NoFactor;
            result.SpeedFactor = 1.0f / (speed / 3.6f); // 1/m/s
            result.Value = result.SpeedFactor;
            result.Direction = direction;
            if (!canstop)
            {
                result.Direction += 3;
            }
            return result;
        }

        public override FactorAndSpeed FactorAndSpeed(IAttributeCollection attributes)
        {
            return CarProfile.FactorAndSpeed(attributes, new Whitelist());
        }

        /// </summary>
        /// <remarks>
        /// Default implementation compares attributes one-by-one.
        /// </remarks>
        public override sealed bool Equals(IAttributeCollection attributes1, IAttributeCollection attributes2)
        {
            return attributes1.ContainsSame(attributes2);
        }

        private IUnimodalInstructionGenerator _instructionGenerator;
        public override IUnimodalInstructionGenerator InstructionGenerator
        {
            get { return _instructionGenerator; }
        }
    }
}
