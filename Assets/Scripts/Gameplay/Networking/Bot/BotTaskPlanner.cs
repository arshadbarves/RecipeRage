using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Gameplay.Networking.Bot
{
    public sealed class BotTaskPlanner
    {
        public BotTaskPlan Plan(BotPlanningSnapshot snapshot)
        {
            if (snapshot == null)
            {
                return BotTaskPlan.Idle();
            }

            BotOrderDescriptor claimedOrder = GetClaimedOrder(snapshot);
            BotOrderDescriptor supportOrder = GetSupportOrder(snapshot);
            BotOrderDescriptor workOrder = claimedOrder ?? supportOrder;
            string targetCounterId = GetTargetCounterId(snapshot, workOrder);

            if (snapshot.HeldItem.Type == BotHeldItemType.Plate)
            {
                if (snapshot.HeldItem.PlateIngredientCount > 0)
                {
                    BotStationDescriptor serving = GetNearestStation(snapshot, BotStationType.ServingStation);
                    if (serving != null)
                    {
                        return new BotTaskPlan
                        {
                            Type = BotTaskType.ServeDish,
                            OrderId = workOrder?.OrderId,
                            RecipeId = workOrder?.RecipeId,
                            TargetStationId = serving.StationId,
                            TargetStationType = BotStationType.ServingStation
                        };
                    }
                }

                if (!string.IsNullOrWhiteSpace(targetCounterId))
                {
                    return new BotTaskPlan
                    {
                        Type = BotTaskType.AssembleDish,
                        OrderId = workOrder?.OrderId,
                        RecipeId = workOrder?.RecipeId,
                        TargetStationId = targetCounterId,
                        TargetStationType = BotStationType.CounterStation
                    };
                }

                return new BotTaskPlan
                {
                    Type = BotTaskType.Recover
                };
            }

            if (snapshot.HeldItem.Type == BotHeldItemType.Ingredient)
            {
                if (snapshot.HeldItem.IsBurned)
                {
                    return new BotTaskPlan
                    {
                        Type = BotTaskType.Recover
                    };
                }

                BotIngredientRequirement heldRequirement = workOrder?.MissingIngredients.FirstOrDefault(
                    requirement => requirement.IngredientId == snapshot.HeldItem.IngredientId);

                if (heldRequirement != null)
                {
                    if (heldRequirement.RequiresCut && !snapshot.HeldItem.IsCut)
                    {
                        BotStationDescriptor cuttingStation = GetNearestAvailableProcessingStation(
                            snapshot,
                            BotStationType.CuttingStation);

                        if (cuttingStation != null)
                        {
                            return CreateStationPlan(
                                BotTaskType.ProcessIngredient,
                                workOrder,
                                snapshot.HeldItem.IngredientId,
                                cuttingStation);
                        }
                    }

                    if (heldRequirement.RequiresCooked && !snapshot.HeldItem.IsCooked)
                    {
                        BotStationDescriptor cookingStation = GetNearestAvailableProcessingStation(
                            snapshot,
                            BotStationType.CookingStation);

                        if (cookingStation != null)
                        {
                            return CreateStationPlan(
                                BotTaskType.ProcessIngredient,
                                workOrder,
                                snapshot.HeldItem.IngredientId,
                                cookingStation);
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(targetCounterId))
                    {
                        BotStationDescriptor counter = snapshot.Stations.FirstOrDefault(
                            station => station.StationId == targetCounterId);

                        if (counter != null && counter.HasPlate)
                        {
                            return CreateStationPlan(
                                BotTaskType.AssembleDish,
                                workOrder,
                                snapshot.HeldItem.IngredientId,
                                counter);
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(targetCounterId))
                    {
                        return new BotTaskPlan
                        {
                            Type = BotTaskType.Recover,
                            OrderId = workOrder?.OrderId,
                            RecipeId = workOrder?.RecipeId,
                            IngredientId = snapshot.HeldItem.IngredientId
                        };
                    }
                }

                return new BotTaskPlan
                {
                    Type = BotTaskType.Recover
                };
            }

            if (snapshot.OwnSinkDirty)
            {
                BotStationDescriptor sink = GetNearestTeamStation(snapshot, BotStationType.SinkStation);
                if (sink != null)
                {
                    return CreateStationPlan(BotTaskType.WashDishes, claimedOrder, null, sink);
                }
            }

            if (claimedOrder == null || claimedOrder.IsExpired || claimedOrder.IsCompleted)
            {
                BotOrderDescriptor nextOrder = snapshot.Orders
                    .Where(order => !order.IsExpired && !order.IsCompleted && !order.HasInvalidAssembly && !order.IsClaimedByAnotherBot)
                    .OrderBy(order => order.RemainingTime)
                    .FirstOrDefault();

                if (nextOrder == null)
                {
                    claimedOrder = GetSupportOrder(snapshot);
                    if (claimedOrder == null)
                    {
                        return BotTaskPlan.Idle();
                    }
                }
                else
                {
                    return new BotTaskPlan
                    {
                        Type = BotTaskType.ClaimOrder,
                        OrderId = nextOrder.OrderId,
                        RecipeId = nextOrder.RecipeId
                    };
                }
            }

            workOrder = claimedOrder ?? supportOrder;
            if (workOrder == null)
            {
                return BotTaskPlan.Idle();
            }

            targetCounterId = GetTargetCounterId(snapshot, workOrder);

            if (claimedOrder != null && claimedOrder.HasInvalidAssembly)
            {
                return new BotTaskPlan
                {
                    Type = BotTaskType.Recover,
                    OrderId = claimedOrder.OrderId,
                    RecipeId = claimedOrder.RecipeId
                };
            }

            if (workOrder.CounterReadyToServe && !string.IsNullOrWhiteSpace(workOrder.ClaimedCounterId))
            {
                return new BotTaskPlan
                {
                    Type = BotTaskType.AssembleDish,
                    OrderId = workOrder.OrderId,
                    RecipeId = workOrder.RecipeId,
                    TargetStationId = workOrder.ClaimedCounterId,
                    TargetStationType = BotStationType.CounterStation
                };
            }

            BotIngredientRequirement nextMissing = workOrder.MissingIngredients.FirstOrDefault();
            if (nextMissing == null)
            {
                return BotTaskPlan.Idle();
            }

            if (!workOrder.CounterHasPlate)
            {
                BotStationDescriptor plateDispenser = GetNearestTeamStation(snapshot, BotStationType.PlateDispenser);
                if (plateDispenser != null)
                {
                    return CreateStationPlan(BotTaskType.AcquirePlate, workOrder, null, plateDispenser);
                }
            }

            BotStationDescriptor readyStation = FindReadyIngredientStation(snapshot, nextMissing);
            if (readyStation != null)
            {
                return CreateStationPlan(
                    BotTaskType.ProcessIngredient,
                    workOrder,
                    nextMissing.IngredientId,
                    readyStation);
            }

            BotStationDescriptor ingredientCrate = GetNearestIngredientCrate(snapshot, nextMissing.IngredientId);
            if (ingredientCrate != null)
            {
                return CreateStationPlan(
                    BotTaskType.FetchIngredient,
                    workOrder,
                    nextMissing.IngredientId,
                    ingredientCrate);
            }

            return new BotTaskPlan
            {
                Type = BotTaskType.Recover,
                OrderId = workOrder.OrderId,
                RecipeId = workOrder.RecipeId,
                IngredientId = nextMissing.IngredientId
            };
        }

        private static BotTaskPlan CreateStationPlan(
            BotTaskType taskType,
            BotOrderDescriptor order,
            int? ingredientId,
            BotStationDescriptor station)
        {
            return new BotTaskPlan
            {
                Type = taskType,
                OrderId = order?.OrderId,
                RecipeId = order?.RecipeId,
                IngredientId = ingredientId,
                TargetStationId = station?.StationId,
                TargetStationType = station?.Type
            };
        }

        private static BotOrderDescriptor GetClaimedOrder(BotPlanningSnapshot snapshot)
        {
            if (!snapshot.ClaimedOrderId.HasValue)
            {
                return null;
            }

            return snapshot.Orders.FirstOrDefault(order => order.OrderId == snapshot.ClaimedOrderId.Value);
        }

        private static BotOrderDescriptor GetSupportOrder(BotPlanningSnapshot snapshot)
        {
            return snapshot.Orders
                .Where(order =>
                    order.IsClaimedByAnotherBot &&
                    !order.IsExpired &&
                    !order.IsCompleted &&
                    !order.HasInvalidAssembly &&
                    !string.IsNullOrWhiteSpace(order.ClaimedCounterId))
                .OrderBy(order => order.RemainingTime)
                .FirstOrDefault();
        }

        private static string GetTargetCounterId(BotPlanningSnapshot snapshot, BotOrderDescriptor claimedOrder)
        {
            if (!string.IsNullOrWhiteSpace(snapshot.ClaimedCounterId))
            {
                return snapshot.ClaimedCounterId;
            }

            return claimedOrder?.ClaimedCounterId;
        }

        private static BotStationDescriptor GetNearestIngredientCrate(BotPlanningSnapshot snapshot, int ingredientId)
        {
            return snapshot.Stations
                .Where(station => station.Type == BotStationType.IngredientCrate && station.IngredientId == ingredientId)
                .OrderBy(station => station.Position.sqrMagnitude)
                .FirstOrDefault();
        }

        private static BotStationDescriptor GetNearestAvailableProcessingStation(BotPlanningSnapshot snapshot, BotStationType type)
        {
            return snapshot.Stations
                .Where(station => station.Type == type && !station.IsBusy)
                .OrderBy(station => station.Position.sqrMagnitude)
                .FirstOrDefault();
        }

        private static BotStationDescriptor GetNearestStation(BotPlanningSnapshot snapshot, BotStationType type)
        {
            return snapshot.Stations
                .Where(station => station.Type == type)
                .OrderBy(station => station.Position.sqrMagnitude)
                .FirstOrDefault();
        }

        private static BotStationDescriptor GetNearestTeamStation(BotPlanningSnapshot snapshot, BotStationType type)
        {
            return snapshot.Stations
                .Where(station => station.Type == type && (station.IsShared || station.TeamId == snapshot.TeamId))
                .OrderBy(station => station.Position.sqrMagnitude)
                .FirstOrDefault();
        }

        private static BotStationDescriptor FindReadyIngredientStation(
            BotPlanningSnapshot snapshot,
            BotIngredientRequirement requirement)
        {
            return snapshot.Stations
                .Where(station =>
                    (station.Type == BotStationType.CuttingStation || station.Type == BotStationType.CookingStation) &&
                    station.StoredIngredientReady &&
                    station.StoredIngredientId == requirement.IngredientId &&
                    (!requirement.RequiresCut || station.StoredIngredientIsCut) &&
                    (!requirement.RequiresCooked || station.StoredIngredientIsCooked) &&
                    !station.StoredIngredientIsBurned)
                .OrderBy(station => station.Position.sqrMagnitude)
                .FirstOrDefault();
        }
    }
}
