using Itinero.Attributes;
using Itinero.Navigation.Instructions;
using Itinero.Profiles;
using System;
using System.Collections.Generic;

namespace Itinero.FastVehicle
{
    public class PedestrianProfile : Profile
    {
        private static readonly HashSet<String> AllowedHighways
           = new HashSet<string> { "pedestrian", "footway", "foot" };

        private static readonly Dictionary<String, int> SpeedProfiles =
            new Dictionary<string, int>
            {
                ["primary"] = 4,
                ["primary_link"] = 4,
                ["secondary"] = 4,
                ["secondary_link"] = 4,
                ["tertiary"] = 4,
                ["tertiary_link"] = 4,
                ["unclassified"] = 4,
                ["residential"] = 4,
                ["service"] = 4,
                ["services"] = 4,
                ["road"] = 4,
                ["track"] = 4,
                ["cycleway"] = 4,
                ["path"] = 4,
                ["footway"] = 4,
                ["pedestrian"] = 4,
                ["living_street"] = 4,
                ["ferry"] = 4,
                ["movable"] = 4,
                ["shuttle_train"] = 4,
                ["default"] = 4
            };

        public static readonly Dictionary<String, bool> AccessValues =
            new Dictionary<string, bool>
            {
                ["private"] = false,
                ["yes"] = true,
                ["no"] = false,
                ["permissive"] = true,
                ["destination"] = true,
                ["customers"] = false,
                ["designated"] = true,
                ["public"] = true,
                ["delivery"] = true,
                ["use_sidepath"] = false
            };

        private static readonly String[] Vehicles = new[] { "foot" };

        public PedestrianProfile() : base("fastpedestrian", ProfileMetric.TimeInSeconds,
           Vehicles,
           null, new PedestrianVehicle())
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

        internal static FactorAndSpeed FactorAndSpeed(IAttributeCollection attributes,
            Whitelist whitelist)
        {
            if (attributes == null || attributes.Count == 0)
            {
                return Itinero.Profiles.FactorAndSpeed.NoFactor;
            }

            string highway = null;
            if (attributes.TryGetValue("highway", out highway))
                whitelist.Add("highway");
            string foot = null;
            if (attributes.TryGetValue("foot", out foot))
            {
                if (foot == "no" || foot == "0")
                    return Itinero.Profiles.FactorAndSpeed.NoFactor;
                whitelist.Add("foot");
            }
            string footway;
            if (attributes.TryGetValue("footway", out footway))
                whitelist.Add("footway");
            var result = new FactorAndSpeed();
            var speed = 0.0f;
            short direction = 0;
            var canstop = true;
            if (String.IsNullOrEmpty(highway))
                if (!String.IsNullOrEmpty(foot))
                    highway = "footway";
                else
                    return Itinero.Profiles.FactorAndSpeed.NoFactor;
            //get default speed profiles
            var highway_speed = SpeedProfiles.ContainsKey(highway) ? (int?)SpeedProfiles[highway] : null;

            if (highway_speed != null)
            {
                speed = highway_speed.Value;
                direction = 0;
                canstop = true;
            }
            else
                return Itinero.Profiles.FactorAndSpeed.NoFactor;

            if (CanAccess(attributes) == false || speed == 0)
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
            var whitelist = new List<string>();
            return PedestrianProfile.FactorAndSpeed(attributes, new Whitelist());
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
