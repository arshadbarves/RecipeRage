using System.Collections.Generic;
using System.Linq;
using Gameplay.Characters;
using Gameplay.Cooking;
using Gameplay.Shared;
using Gameplay.Stations;
using UnityEngine;

namespace Gameplay.Networking.Bot
{
    public sealed class BotKitchenSnapshot
    {
        private readonly IMatchContext _matchContext;
        private readonly Dictionary<string, Component> _stationsById = new Dictionary<string, Component>();
        private readonly StandardDishValidator _dishValidator = new StandardDishValidator();
        private OrderManager _orderManager;

        public BotKitchenSnapshot(IMatchContext matchContext)
        {
            _matchContext = matchContext;
        }

        public BotPlanningSnapshot Capture(PlayerController player, string botId, BotClaimRegistry claimRegistry)
        {
            _orderManager = _matchContext?.OrderManager;
            RefreshStations();

            int? claimedOrderId = claimRegistry.GetClaimedOrderId(botId);
            string claimedCounterId = claimedOrderId.HasValue ? claimRegistry.GetClaimedCounterId(claimedOrderId.Value) : null;

            var snapshot = new BotPlanningSnapshot
            {
                TeamId = player != null ? player.TeamId : 0,
                ClaimedOrderId = claimedOrderId,
                ClaimedCounterId = claimedCounterId,
                HeldItem = BuildHeldItemState(player),
                Stations = BuildStationDescriptors(player != null ? player.transform.position : Vector3.zero),
                Orders = BuildOrderDescriptors(botId, claimRegistry),
                OwnSinkDirty = FindOwnDirtySink(player) > 0
            };

            return snapshot;
        }

        public Component ResolveStation(string stationId)
        {
            return !string.IsNullOrWhiteSpace(stationId) && _stationsById.TryGetValue(stationId, out Component station)
                ? station
                : null;
        }

        public CounterStation FindNearestAvailableCounter(Vector3 origin, string botId, BotClaimRegistry claimRegistry)
        {
            return _stationsById.Values
                .OfType<CounterStation>()
                .Where(counter => counter.IsEmpty && !claimRegistry.IsCounterClaimedByAnotherBot(GetStationId(counter), botId))
                .OrderBy(counter => (counter.transform.position - origin).sqrMagnitude)
                .FirstOrDefault();
        }

        public RecipeOrderState? FindOrderState(int orderId)
        {
            if (_orderManager == null)
            {
                _orderManager = _matchContext?.OrderManager;
            }

            if (_orderManager == null)
            {
                return null;
            }

            foreach (RecipeOrderState order in _orderManager.GetActiveOrders())
            {
                if (order.OrderId == orderId)
                {
                    return order;
                }
            }

            return null;
        }

        public Recipe GetRecipe(int recipeId)
        {
            if (_orderManager == null)
            {
                _orderManager = _matchContext?.OrderManager;
            }

            return _orderManager != null ? _orderManager.GetRecipeById(recipeId) : null;
        }

        private List<BotStationDescriptor> BuildStationDescriptors(Vector3 origin)
        {
            var descriptors = new List<BotStationDescriptor>();

            foreach (IngredientCrate crate in _stationsById.Values.OfType<IngredientCrate>())
            {
                descriptors.Add(new BotStationDescriptor
                {
                    StationId = GetStationId(crate),
                    Type = BotStationType.IngredientCrate,
                    Position = crate.transform.position - origin,
                    IngredientId = crate.ProvidedIngredient != null ? crate.ProvidedIngredient.Id : 0
                });
            }

            foreach (CuttingStation cuttingStation in _stationsById.Values.OfType<CuttingStation>())
            {
                descriptors.Add(BuildProcessingDescriptor(cuttingStation, BotStationType.CuttingStation, origin));
            }

            foreach (CookingStation cookingStation in _stationsById.Values.OfType<CookingStation>())
            {
                descriptors.Add(BuildProcessingDescriptor(cookingStation, BotStationType.CookingStation, origin));
            }

            foreach (CounterStation counter in _stationsById.Values.OfType<CounterStation>())
            {
                descriptors.Add(new BotStationDescriptor
                {
                    StationId = GetStationId(counter),
                    Type = BotStationType.CounterStation,
                    Position = counter.transform.position - origin,
                    HasItem = !counter.IsEmpty,
                    HasPlate = counter.HeldPlate != null,
                    StockCount = counter.HeldPlate != null ? counter.HeldPlate.IngredientCount : 0
                });
            }

            foreach (ServingStation servingStation in _stationsById.Values.OfType<ServingStation>())
            {
                descriptors.Add(new BotStationDescriptor
                {
                    StationId = GetStationId(servingStation),
                    Type = BotStationType.ServingStation,
                    Position = servingStation.transform.position - origin,
                    TeamId = servingStation.TeamId,
                    IsShared = servingStation.TeamId < 0
                });
            }

            foreach (SinkStation sink in _stationsById.Values.OfType<SinkStation>())
            {
                descriptors.Add(new BotStationDescriptor
                {
                    StationId = GetStationId(sink),
                    Type = BotStationType.SinkStation,
                    Position = sink.transform.position - origin,
                    TeamId = sink.TeamId,
                    IsShared = sink.TeamId < 0,
                    StockCount = sink.DirtyPlateCount
                });
            }

            foreach (PlateDispenser plateDispenser in _stationsById.Values.OfType<PlateDispenser>())
            {
                descriptors.Add(new BotStationDescriptor
                {
                    StationId = GetStationId(plateDispenser),
                    Type = BotStationType.PlateDispenser,
                    Position = plateDispenser.transform.position - origin,
                    TeamId = plateDispenser.TeamId,
                    IsShared = plateDispenser.IsShared,
                    StockCount = plateDispenser.AvailableStock
                });
            }

            return descriptors;
        }

        private List<BotOrderDescriptor> BuildOrderDescriptors(string botId, BotClaimRegistry claimRegistry)
        {
            var descriptors = new List<BotOrderDescriptor>();

            if (_orderManager == null)
            {
                return descriptors;
            }

            foreach (RecipeOrderState order in _orderManager.GetActiveOrders())
            {
                Recipe recipe = _orderManager.GetRecipeById(order.RecipeId);
                if (recipe == null)
                {
                    continue;
                }

                string counterId = claimRegistry.GetClaimedCounterId(order.OrderId);
                CounterStation counter = ResolveStation(counterId) as CounterStation;
                PlateItem plate = counter != null ? counter.HeldPlate : null;
                AnalyzePlateProgress(recipe, plate, out bool readyToServe, out bool invalidAssembly, out List<BotIngredientRequirement> missing);

                descriptors.Add(new BotOrderDescriptor
                {
                    OrderId = order.OrderId,
                    RecipeId = order.RecipeId,
                    RemainingTime = order.RemainingTime,
                    IsCompleted = order.IsCompleted,
                    IsExpired = order.IsExpired,
                    IsClaimedByAnotherBot = claimRegistry.IsOrderClaimedByAnotherBot(order.OrderId, botId),
                    ClaimedCounterId = counterId,
                    CounterHasPlate = plate != null,
                    CounterReadyToServe = readyToServe,
                    HasInvalidAssembly = invalidAssembly,
                    MissingIngredients = missing
                });
            }

            return descriptors;
        }

        private static BotHeldItemState BuildHeldItemState(PlayerController player)
        {
            if (player == null || !player.IsHoldingObject())
            {
                return BotHeldItemState.Empty();
            }

            GameObject heldObject = player.GetHeldObject();
            if (heldObject == null)
            {
                return BotHeldItemState.Empty();
            }

            PlateItem plate = heldObject.GetComponent<PlateItem>();
            if (plate != null)
            {
                return new BotHeldItemState
                {
                    Type = BotHeldItemType.Plate,
                    PlateIngredientCount = plate.IngredientCount
                };
            }

            IngredientItem ingredient = heldObject.GetComponent<IngredientItem>();
            if (ingredient != null)
            {
                return new BotHeldItemState
                {
                    Type = BotHeldItemType.Ingredient,
                    IngredientId = ingredient.Ingredient != null ? ingredient.Ingredient.Id : 0,
                    IsCut = ingredient.IsCut,
                    IsCooked = ingredient.IsCooked,
                    IsBurned = ingredient.IsBurned
                };
            }

            return BotHeldItemState.Empty();
        }

        private int FindOwnDirtySink(PlayerController player)
        {
            if (player == null)
            {
                return 0;
            }

            return _stationsById.Values
                .OfType<SinkStation>()
                .Where(sink => sink.TeamId == player.TeamId)
                .Sum(sink => sink.DirtyPlateCount);
        }

        private BotStationDescriptor BuildProcessingDescriptor(ProcessingStation station, BotStationType type, Vector3 origin)
        {
            IngredientItem ingredient = station.CurrentIngredient;

            return new BotStationDescriptor
            {
                StationId = GetStationId(station),
                Type = type,
                Position = station.transform.position - origin,
                IsBusy = station.IsProcessing,
                HasItem = ingredient != null,
                StoredIngredientId = ingredient != null && ingredient.Ingredient != null ? ingredient.Ingredient.Id : 0,
                StoredIngredientIsCut = ingredient != null && ingredient.IsCut,
                StoredIngredientIsCooked = ingredient != null && ingredient.IsCooked,
                StoredIngredientIsBurned = ingredient != null && ingredient.IsBurned,
                StoredIngredientReady = ingredient != null && !station.IsProcessing
            };
        }

        private void RefreshStations()
        {
            _stationsById.Clear();

            IReadOnlyList<Component> stations = _matchContext?.BotKitchenRuntime?.Stations;
            if (stations == null)
            {
                return;
            }

            foreach (Component station in stations)
            {
                if (station != null)
                {
                    _stationsById[GetStationId(station)] = station;
                }
            }
        }

        private void AnalyzePlateProgress(
            Recipe recipe,
            PlateItem plate,
            out bool readyToServe,
            out bool invalidAssembly,
            out List<BotIngredientRequirement> missingIngredients)
        {
            missingIngredients = new List<BotIngredientRequirement>();
            readyToServe = false;
            invalidAssembly = false;

            if (recipe == null)
            {
                invalidAssembly = true;
                return;
            }

            List<IngredientItem> currentIngredients = plate != null ? plate.GetIngredients() : new List<IngredientItem>();
            var usedIndices = new HashSet<int>();

            foreach (RecipeIngredient requiredIngredient in recipe.Ingredients)
            {
                int matchIndex = -1;
                for (int i = 0; i < currentIngredients.Count; i++)
                {
                    if (usedIndices.Contains(i))
                    {
                        continue;
                    }

                    IngredientItem current = currentIngredients[i];
                    if (current?.Ingredient == null || current.Ingredient.Id != requiredIngredient.Ingredient.Id)
                    {
                        continue;
                    }

                    if (requiredIngredient.RequireCut && !current.IsCut)
                    {
                        continue;
                    }

                    if (requiredIngredient.RequireCooked && !current.IsCooked)
                    {
                        continue;
                    }

                    if (current.IsBurned)
                    {
                        continue;
                    }

                    matchIndex = i;
                    break;
                }

                if (matchIndex >= 0)
                {
                    usedIndices.Add(matchIndex);
                    continue;
                }

                missingIngredients.Add(new BotIngredientRequirement
                {
                    IngredientId = requiredIngredient.Ingredient != null ? requiredIngredient.Ingredient.Id : 0,
                    RequiresCut = requiredIngredient.RequireCut,
                    RequiresCooked = requiredIngredient.RequireCooked
                });
            }

            if (currentIngredients.Count > usedIndices.Count)
            {
                invalidAssembly = true;
                return;
            }

            readyToServe = plate != null &&
                missingIngredients.Count == 0 &&
                currentIngredients.Count == recipe.Ingredients.Count &&
                _dishValidator.ValidateDish(currentIngredients, recipe);
        }

        private static string GetStationId(Component station)
        {
            if (station == null)
            {
                return null;
            }

            if (station.TryGetComponent(out Unity.Netcode.NetworkObject networkObject))
            {
                return networkObject.IsSpawned
                    ? networkObject.NetworkObjectId.ToString()
                    : station.GetInstanceID().ToString();
            }

            return station.GetInstanceID().ToString();
        }
    }
}
