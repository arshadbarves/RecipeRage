using System;
using System.Collections.Generic;

namespace RecipeRage.Store
{
    /// <summary>
    /// Represents the status of a purchase
    /// </summary>
    public enum PurchaseStatus
    {
        /// <summary>
        /// Purchase succeeded
        /// </summary>
        Success,

        /// <summary>
        /// Purchase failed
        /// </summary>
        Failed,

        /// <summary>
        /// Purchase is pending (e.g., waiting for payment processing)
        /// </summary>
        Pending,

        /// <summary>
        /// Purchase was cancelled by the user
        /// </summary>
        Cancelled,

        /// <summary>
        /// Purchase is being restored
        /// </summary>
        Restored
    }

    /// <summary>
    /// Represents the result of a purchase operation
    /// </summary>
    [Serializable]
    public class PurchaseResult
    {

        /// <summary>
        /// Default constructor
        /// </summary>
        public PurchaseResult()
        {
            PurchaseDate = DateTime.UtcNow;
            Status = PurchaseStatus.Failed;
        }

        /// <summary>
        /// Constructor for a successful purchase
        /// </summary>
        /// <param name="offerId"> ID of the offer </param>
        /// <param name="transactionId"> Transaction ID </param>
        /// <param name="providerName"> Name of the provider </param>
        public PurchaseResult(string offerId, string transactionId, string providerName)
        {
            OfferId = offerId;
            TransactionId = transactionId;
            ProviderName = providerName;
            Status = PurchaseStatus.Success;
            PurchaseDate = DateTime.UtcNow;
        }

        /// <summary>
        /// ID of the offer that was purchased
        /// </summary>
        public string OfferId { get; set; }

        /// <summary>
        /// Transaction ID for the purchase
        /// </summary>
        public string TransactionId { get; set; }

        /// <summary>
        /// Receipt data for verification
        /// </summary>
        public string Receipt { get; set; }

        /// <summary>
        /// Status of the purchase
        /// </summary>
        public PurchaseStatus Status { get; set; }

        /// <summary>
        /// Error message if the purchase failed
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Items granted in this purchase
        /// </summary>
        public List<InventoryItem> GrantedItems { get; set; } = new List<InventoryItem>();

        /// <summary>
        /// The offer that was purchased
        /// </summary>
        public StoreOffer Offer { get; set; }

        /// <summary>
        /// Date when the purchase was made
        /// </summary>
        public DateTime PurchaseDate { get; set; }

        /// <summary>
        /// Name of the provider that processed the purchase
        /// </summary>
        public string ProviderName { get; set; }

        /// <summary>
        /// Provider-specific data
        /// </summary>
        public object ProviderData { get; set; }

        /// <summary>
        /// Gets whether the purchase was successful
        /// </summary>
        public bool IsSuccess => Status == PurchaseStatus.Success || Status == PurchaseStatus.Restored;

        /// <summary>
        /// Gets whether the purchase failed
        /// </summary>
        public bool IsFailed => Status == PurchaseStatus.Failed;

        /// <summary>
        /// Gets whether the purchase is pending
        /// </summary>
        public bool IsPending => Status == PurchaseStatus.Pending;

        /// <summary>
        /// Gets whether the purchase was cancelled
        /// </summary>
        public bool IsCancelled => Status == PurchaseStatus.Cancelled;

        /// <summary>
        /// Creates a successful purchase result
        /// </summary>
        /// <param name="offerId"> ID of the offer </param>
        /// <param name="transactionId"> Transaction ID </param>
        /// <param name="providerName"> Name of the provider </param>
        /// <returns> A successful purchase result </returns>
        public static PurchaseResult CreateSuccessResult(string offerId, string transactionId, string providerName)
        {
            return new PurchaseResult
            {
                OfferId = offerId,
                TransactionId = transactionId,
                ProviderName = providerName,
                Status = PurchaseStatus.Success,
                PurchaseDate = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Creates a failed purchase result
        /// </summary>
        /// <param name="offerId"> ID of the offer </param>
        /// <param name="errorMessage"> Error message </param>
        /// <param name="providerName"> Name of the provider </param>
        /// <returns> A failed purchase result </returns>
        public static PurchaseResult CreateFailedResult(string offerId, string errorMessage, string providerName)
        {
            return new PurchaseResult
            {
                OfferId = offerId,
                ErrorMessage = errorMessage,
                ProviderName = providerName,
                Status = PurchaseStatus.Failed,
                PurchaseDate = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Creates a cancelled purchase result
        /// </summary>
        /// <param name="offerId"> ID of the offer </param>
        /// <param name="providerName"> Name of the provider </param>
        /// <returns> A cancelled purchase result </returns>
        public static PurchaseResult CreateCancelledResult(string offerId, string providerName)
        {
            return new PurchaseResult
            {
                OfferId = offerId,
                ProviderName = providerName,
                Status = PurchaseStatus.Cancelled,
                PurchaseDate = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Creates a pending purchase result
        /// </summary>
        /// <param name="offerId"> ID of the offer </param>
        /// <param name="transactionId"> Transaction ID </param>
        /// <param name="providerName"> Name of the provider </param>
        /// <returns> A pending purchase result </returns>
        public static PurchaseResult CreatePendingResult(string offerId, string transactionId, string providerName)
        {
            return new PurchaseResult
            {
                OfferId = offerId,
                TransactionId = transactionId,
                ProviderName = providerName,
                Status = PurchaseStatus.Pending,
                PurchaseDate = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Creates a restored purchase result
        /// </summary>
        /// <param name="offerId"> ID of the offer </param>
        /// <param name="transactionId"> Transaction ID </param>
        /// <param name="providerName"> Name of the provider </param>
        /// <param name="originalPurchaseDate"> Original purchase date </param>
        /// <returns> A restored purchase result </returns>
        public static PurchaseResult CreateRestoredResult(
            string offerId,
            string transactionId,
            string providerName,
            DateTime originalPurchaseDate)
        {
            return new PurchaseResult
            {
                OfferId = offerId,
                TransactionId = transactionId,
                ProviderName = providerName,
                Status = PurchaseStatus.Restored,
                PurchaseDate = originalPurchaseDate
            };
        }
    }
}