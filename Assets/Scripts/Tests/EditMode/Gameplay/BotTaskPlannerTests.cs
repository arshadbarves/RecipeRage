using System.Collections.Generic;
using Gameplay.Networking.Bot;
using NUnit.Framework;
using UnityEngine;

namespace RecipeRage.Tests.EditMode.Gameplay
{
    public class BotTaskPlannerTests
    {
        [Test]
        public void Plan_PrioritizesServingWhenHoldingCompletedPlate()
        {
            BotTaskPlanner planner = new();
            BotPlanningSnapshot snapshot = new()
            {
                HeldItem = new BotHeldItemState
                {
                    Type = BotHeldItemType.Plate,
                    PlateIngredientCount = 2
                },
                Stations = new List<BotStationDescriptor>
                {
                    new()
                    {
                        StationId = "serve",
                        Type = BotStationType.ServingStation,
                        Position = Vector3.zero
                    }
                }
            };

            BotTaskPlan plan = planner.Plan(snapshot);

            Assert.AreEqual(BotTaskType.ServeDish, plan.Type);
            Assert.AreEqual("serve", plan.TargetStationId);
        }

        [Test]
        public void Plan_SendsRawIngredientToRequiredProcessingStation()
        {
            BotTaskPlanner planner = new();
            BotPlanningSnapshot snapshot = new()
            {
                ClaimedOrderId = 10,
                HeldItem = new BotHeldItemState
                {
                    Type = BotHeldItemType.Ingredient,
                    IngredientId = 2001,
                    IsCut = false
                },
                Orders = new List<BotOrderDescriptor>
                {
                    new()
                    {
                        OrderId = 10,
                        RecipeId = 3001,
                        MissingIngredients = new List<BotIngredientRequirement>
                        {
                            new()
                            {
                                IngredientId = 2001,
                                RequiresCut = true
                            }
                        }
                    }
                },
                Stations = new List<BotStationDescriptor>
                {
                    new()
                    {
                        StationId = "cut",
                        Type = BotStationType.CuttingStation,
                        Position = Vector3.zero,
                        IsBusy = false
                    }
                }
            };

            BotTaskPlan plan = planner.Plan(snapshot);

            Assert.AreEqual(BotTaskType.ProcessIngredient, plan.Type);
            Assert.AreEqual("cut", plan.TargetStationId);
            Assert.AreEqual(BotStationType.CuttingStation, plan.TargetStationType);
        }

        [Test]
        public void Plan_ReclaimsUnusableOrderAndTargetsFreshOrder()
        {
            BotTaskPlanner planner = new();
            BotPlanningSnapshot snapshot = new()
            {
                ClaimedOrderId = 10,
                Orders = new List<BotOrderDescriptor>
                {
                    new()
                    {
                        OrderId = 10,
                        RecipeId = 3001,
                        IsExpired = true
                    },
                    new()
                    {
                        OrderId = 11,
                        RecipeId = 3002,
                        RemainingTime = 15f
                    }
                }
            };

            BotTaskPlan plan = planner.Plan(snapshot);

            Assert.AreEqual(BotTaskType.ClaimOrder, plan.Type);
            Assert.AreEqual(11, plan.OrderId);
        }

        [Test]
        public void Plan_ChoosesIngredientCrateWhenNeededIngredientIsMissing()
        {
            BotTaskPlanner planner = new();
            BotPlanningSnapshot snapshot = new()
            {
                ClaimedOrderId = 25,
                ClaimedCounterId = "counter-1",
                Orders = new List<BotOrderDescriptor>
                {
                    new()
                    {
                        OrderId = 25,
                        RecipeId = 3002,
                        CounterHasPlate = true,
                        ClaimedCounterId = "counter-1",
                        MissingIngredients = new List<BotIngredientRequirement>
                        {
                            new()
                            {
                                IngredientId = 2002,
                                RequiresCooked = true
                            }
                        }
                    }
                },
                Stations = new List<BotStationDescriptor>
                {
                    new()
                    {
                        StationId = "crate-steak",
                        Type = BotStationType.IngredientCrate,
                        IngredientId = 2002,
                        Position = Vector3.zero
                    }
                }
            };

            BotTaskPlan plan = planner.Plan(snapshot);

            Assert.AreEqual(BotTaskType.FetchIngredient, plan.Type);
            Assert.AreEqual("crate-steak", plan.TargetStationId);
            Assert.AreEqual(2002, plan.IngredientId);
        }
    }
}
