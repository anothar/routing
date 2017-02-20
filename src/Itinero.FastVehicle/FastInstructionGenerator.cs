using Itinero.Navigation.Instructions;
using System;
using System.Collections.Generic;
using System.Linq;
using Itinero.Navigation.Language;
using Itinero.Profiles;
using Itinero.Navigation.Directions;

namespace Itinero.FastVehicle
{
    internal class FastInstructionGenerator :
        IUnimodalInstructionGenerator
    {
        private Profile _profile;
        internal FastInstructionGenerator(Profile profile)
        {
            _profile = profile;
        }

        public IList<Instruction> Generate(Route route, ILanguageReference language)
        {
            if (route.IsMultimodal())
            {
                throw new ArgumentException("Cannot use a unimodal instruction generator on multimodal route.");
            }
            if (_profile.FullName.ToLowerInvariant() != route.Profile)
            {
                throw new ArgumentException(string.Format("Cannot generate instructions with a generator for profile {0} for a route with profile {1}.",
                    _profile.FullName, route.Profile));
            }
            var instructions = new List<Instruction>();
            foreach (var position in route)
            {
                if (position.IsFirst())
                    instructions.Add(GetStart(position, language));
                else if (position.IsLast())
                    instructions.Add(GetStop(position, language));
                else if (position.GetMetaAttribute("junction") ==
                "roundabout")
                {
                    var instruction = GetRoundabout(position, language);
                    if (instruction != null)
                        instructions.Add(instruction);
                }else
                {
                    var instruction = GetTurn(position, language);
                    if (instruction != null)
                        instructions.Add(instruction);
                }
            }
            return instructions;
        }

        private Instruction GetStart(RoutePosition position, ILanguageReference language)
        {
            var instruction = new Instruction
            {
                Shape = position.Shape,
                Type = "start"
            };
            var direction = position.Direction();
            instruction.Text = String.Format(language
                ["Start {0}."], language[direction.ToString()
                .ToLower()]);
            return instruction;
        }

        // gets the last instruction
        private Instruction GetStop(RoutePosition position, ILanguageReference language)
        {
            return new Instruction
            {
                Text = language["Arrived at destination."],
                Shape = position.Shape,
                Type = "stop"
            };
        }

        private IEnumerable<Route.Branch> GetTraversable(IEnumerable<Route.Branch> branches)
        {
            foreach (var branch in branches)
            {
                var factorAndSpeed = _profile.FactorAndSpeed(branch.Attributes);
                if (factorAndSpeed.SpeedFactor != 0)
                {
                    if (factorAndSpeed.Direction == 0 ||
                        factorAndSpeed.Direction == 3)
                    {
                        yield return branch;
                    }
                    else
                    {
                        if (branch.AttributesDirection)
                        {
                            if (factorAndSpeed.Direction == 1 ||
                                factorAndSpeed.Direction == 4)
                            {
                                yield return branch;
                            }
                        }
                        else
                        {
                            if (factorAndSpeed.Direction == 2 ||
                                factorAndSpeed.Direction == 5)
                            {
                                yield return branch;
                            }
                        }
                    }
                }
            }
        }


        //gets a roundabout instruction
        private Instruction GetRoundabout(RoutePosition position, ILanguageReference language)
        {
            position.Next();
            if (String.IsNullOrEmpty(position.GetMetaAttribute("junction")))
            {
                var instruction = new Instruction
                {
                    Shape = position.Shape,
                    Type = "roundabout"
                };

                var exit = 1;
                var count = 1;
                var previous = position.Previous();
                while (previous != null && previous.Value.GetMetaAttribute("junction") == "roundabout")
                {
                    var branches = previous.Value.Branches();
                    if (branches != null)
                    {
                        branches = GetTraversable(branches);
                        if (branches.Any())
                            exit++;
                    }
                    count++;
                    previous = previous.Value.Previous();
                }

                instruction.Text = string.Format(language["Take the {0}th exit at the next roundabout."], exit);
                if (exit == 1)
                    instruction.Text = string.Format(language["Take the first exit at the next roundabout."]);
                else if (exit == 2)
                    instruction.Text = string.Format(language["Take the second exit at the next roundabout."]);
                else if (exit == 3)
                    instruction.Text = string.Format(language["Take the third exit at the next roundabout."]);

                instruction.Shape = position.Shape;
                return instruction;
            }

            return null;
        }

        //gets a turn
        private Instruction GetTurn(RoutePosition position, ILanguageReference language)
        {
            var instruction = new Instruction
            {
                Shape = position.Shape,
                Type = "turn"
            };
            var relativeDirection = position.RelativeDirection().Direction;
            var turnRelevant = false;
            var branches = position.Branches();
            if (branches != null)
            {
                var traversedBranches =
                    GetTraversable(branches).ToList();

                if (relativeDirection == RelativeDirectionEnum.StraightOn &&
                    traversedBranches.Count >= 2)
                    turnRelevant = true;// straight on at cross road

                if (relativeDirection != RelativeDirectionEnum.StraightOn &&
            traversedBranches.Count > 0)
                    turnRelevant = true;// an actual normal turn
            }
            if (turnRelevant)
            {
                var next = position.Next();
                string name = null;
                if (next != null)
                    name = next.Value.GetMetaAttribute("name");
                if (!String.IsNullOrEmpty(name))
                {
                    instruction.Text = string.Format(language["Go {0} on {1}."],
                        language[relativeDirection.ToString()
                        .ToLower()], name);
                    instruction.Shape = position.Shape;
                }
                else
                {
                    instruction.Text = String.Format(language["Go {0}."],
                        language[relativeDirection.ToString().ToLower()]);
                    instruction.Shape = position.Shape;

                }
                return instruction;
            }
            return null;
        }
    }
}
