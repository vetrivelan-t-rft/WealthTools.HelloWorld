using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;

namespace WealthTools.Library.Accounts.Models
{
    public enum AccountDetail
    {
        SECURITY,
        ASSET_CLASS,
        //MIXED

    }
    public enum OwnershipType
    {
        //INVALID_OWNERSHIPTYPE = -1,
        JOINT,
        INDIVIDUAL
    };

    public enum TaxTreatment
    {
        //INVALID_TAXTREATMENT = -1,
        DEFERRED,
        EXEMPT,
        TAXABLE,
        TAX_ADVANTAGED_EDUCATION,
        NON_QUALIFIED
    };

    public enum PositionTerm
    {
       LONG = 1,
        SHORT
    };

    public enum MarketValueEntry
    {
        PRICE_AND_QUANTITY = 1,
        MARKET_VALUE
    };
    public class PriceHelper
    {
        public static double ZeroPrice = 0.00001;
        public static double DefaultPrice = 1.0;
    }

    public class AccountKeyInfo
    {

        #region Properties

        public string InternalYN { get; set; }
        public string AccountID { get; set; }
        public string MarginableYN { get; set; }
        public string ProgramID { get; set; }
        public string AccountName { get; set; }
        public string AccountNumber { get; set; }
        public string ErrorMessage { get; set; }
        public bool TestAccount { get; set; }
        public string Attribute2 { get; set; }
        public string DisplayTypeId { get; set; }

        #endregion Properties

    }

    public class AccountBasics : AccountKeyInfo
    {
        #region Properties
        public string HouseholdId { get; set; }
        private string includeAccountYn = "N";
        public string IncludeAccountYN
        {
            get { return includeAccountYn; }
            set { includeAccountYn = value; }
        }
        public string IrsName { get; set; }
        public string OwnerName { get; set; }
        public string ATTR2 { get; set; }
        public string ProgramId { get; set; }
        public double CashComponent { get; set; }
        public string CurrencyCode { get; set; }
        public double Rate { get; set; }
        public double LongBalance { get; set; }
        public double ShortBalance { get; set; }
        public double MarketValue { get; set; }
        //CREATE_DATE
        private bool internal_yn;
        public bool InternalValue { get; set; } = false;
        public string InternalValueDBField
        { set { internal_yn = value != "N" ? true : false; } }
        public bool ManualDBField { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public TaxTreatment TaxTreatment { get; set; } = TaxTreatment.DEFERRED;
        private string nature_of_acct = string.Empty;
        public string NatureOfAcct
        {
            get { return String.IsNullOrEmpty(nature_of_acct) ? TaxTreatment.ToString() : nature_of_acct; }
            set { nature_of_acct = value; }
        }
        [JsonConverter(typeof(StringEnumConverter))]
        public OwnershipType OwnershipType { get; set; } = OwnershipType.INDIVIDUAL;

        public long AccountTypeId { get; set; } = 30;
        public string PrimaryOwnerId { get; set; }
        public string AcctOwnerId { get; set; }
        public string SecondaryOwnerId { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public AccountDetail AccountDetail { get; set; } = Models.AccountDetail.SECURITY;

        public string PortfolioAcctType { get; set; }
        public int DatasourceID { get; set; }
        public bool IsCashBalancePreset { get; set; }
        public double PresetCashBalance { get; set; }
        #endregion Properties
    }

    public class AccountPosition : AccountBasics
    {
        #region IAnyList

        #endregion IAnyList

        #region Properties
        public List<OpenPosition> Positions { get; set; } = new List<OpenPosition>();
        public double AccountCashBalance { get; set; }
        public int SecCount { get; set; }
        public string LastUpdatedDate { get; set; }
        public string LastModifiedDate { get; set; }
        #endregion Properties
    }

    public interface ICurrencyInfo
    {
        // Property signatures:
        string CurrencyCode { get; set; }

    }

    public class SecurityBaseEx
    {
        #region private
        private double price_factor;
        private double price_adj_factor;
        private string currency_code = "USD";
        private double currency_conv_factor = 1.0;
        #endregion

        #region public
        public string SecID { get; set; }
        public string Cusip { get; set; }
        public string SecSedol { get; set; }
        public string SecName { get; set; }
        public string SecType { get; set; }
        public string SubType { get; set; }
        public string SecSymbol { get; set; }
        public double PriceFactor
        {
            get { return price_factor <= 0.0 ? 1.0 : price_factor; }
            set { price_factor = value; }
        }
        public double Price_ADJ_factor
        {
            get { return price_adj_factor <= 0.0 ? PriceFactor : price_adj_factor; }
            set { price_adj_factor = value; }
        }
        public string CurrencyCode
        {
            get { return currency_code; }
            set { currency_code = value; }
        }
        public double CurrencyConvFactor
        {
            get { return currency_conv_factor; }
            set { currency_conv_factor = value; }
        }
        public string SecStatus { get; set; }
        public bool FileExists { get; set; }
        public bool HasFactSheet { get; set; }
        #endregion
    }

    public class PositionBase : ICurrencyInfo
    {
       

        #region Properties
        public string PosType { get; set; }
        public string AccountID { get; set; }
        public long PosId { get; set; }
        public string CogId { get; set; }
        public string SecID { get; set; }
        public double Qty { get; set; }
        public double CurrentPriceDB
        {
            get { return current_price; }
            set { current_price = value; }
        }
        protected double current_price;
        public double CurrentPrice
        {
            get
            {
                if (this.SecurityDetail.CurrencyConvFactor.Equals(0.0)) return Math.Round(current_price * this.SecurityDetail.CurrencyConvFactor, 4);
                else return current_price;
            }
            set { current_price = value; }
        }
        private string pricing_date = string.Empty;
        //[DBField("AS_OF_DATE")]
        public string PricingDate
        {
            get
            {
                if (pricing_date != null && pricing_date != "")
                    return DateTime.Parse(pricing_date).ToShortDateString();
                return pricing_date;
            }
            set { pricing_date = value; }
        }
        private string open_date = string.Empty;
        //[DBField("PURCHASE_DATE")]
        public string OpenDate
        {
            get
            {
                if (open_date.Length != 0)
                    return DateTime.Parse(open_date).ToShortDateString();
                return open_date;
            }
            set { open_date = value; }
        }
        //[DBField("PURCHASE_PRICE")]
        public double OpenPrice { get; set; }
        //[DBField("MARKET_VALUE")]
        public double MarketValue { get; set; }
        private double commissions;
        //[DBField("COMMISSION")]
        public double Commissions
        {
            get {
                if (this.SecurityDetail.CurrencyConvFactor.Equals(0.0)) return Math.Round(commissions * this.SecurityDetail.CurrencyConvFactor, 4);
                else return commissions;
            }
            set { commissions = value; }
        }
        private PositionTerm position_term = PositionTerm.LONG;
        public PositionTerm PositionTerm
        {
            get { return position_term; }
            set { position_term = value; }
        }
        //[DBField("POSITION_TERM")]
        public string PositionTermDB
        {
            set { position_term = (PositionTerm)Enum.Parse(typeof(PositionTerm), value); }
        }
        private MarketValueEntry market_value_entry_mode = MarketValueEntry.PRICE_AND_QUANTITY;
        public MarketValueEntry Marketalue_entry_mode
        {
            get { return market_value_entry_mode; }
            set { market_value_entry_mode = value; }
        }
        //[DBField("MKT_VAL_ENTRY_MODE")]
        public string MarketValue_entry_modeDB
        {
            set { market_value_entry_mode = (MarketValueEntry)Enum.Parse(typeof(MarketValueEntry), value); }
        }
        private bool is_market_value_preset;
        public bool Is_market_value_preset
        {
            get { return is_market_value_preset; }
            set { is_market_value_preset = value; }
        }
        //[DBField("MARKET_VALUE_PRESET")]
        public string IsMarketValuePresetYN
        {
            set
            {
                if (value == "Y")
                    is_market_value_preset = true;
                else
                    is_market_value_preset = false;
            }
        }
        //[DBField("current_factor")]
        public double CurrentFactor { get; set; }
        private SecurityBaseEx security_details = new SecurityBaseEx();
        //[DBField("", IsContainer = true)]
        public SecurityBaseEx SecurityDetail
        {
            get { return security_details; }
            set { security_details = value; }
        }
        public bool LockedYN { get; set; }
        public bool ExcludeYN { get; set; }
        public string DisplayPrice { get; set; }
        public string DisplayMarketValue { get; set; }
        #endregion Properties

        #region ICurrencyInfo Members
        private string currencyCode;
        public string CurrencyCode
        {
            get
            {
                return this.security_details.CurrencyCode;
            }
            set { currencyCode = value; }
        }

        #endregion
    }

    public class OpenPosition : PositionBase
    {

        private double _estimatedAnnualIncome;
        private bool _estimatedAnnualIncomeIsNull = true;

        //[DBField("ESTIMATED_ANNUAL_INCOME")]
        public double EstimatedAnnualIncomeDB
        {
            set { _estimatedAnnualIncome = value;
                _estimatedAnnualIncomeIsNull = false; }
        }
        public double EstimatedAnnualIncome
        {
            get { return _estimatedAnnualIncome; }
            set { _estimatedAnnualIncome = value; }
        }
        public bool EstimatedAnnualIncomeIsNull
        {
            get { return _estimatedAnnualIncomeIsNull; }
            set { _estimatedAnnualIncomeIsNull = value; }
        }

        public List<AssetClassification> AssetClassifications { get; set; } = new List<AssetClassification>();

    }

    public abstract class CollectionBaseEx<T> : ICloneable

    {
        public List<T> List = new List<T>();

        // Provide the explicit interface member for ICollection.
        public void CopyTo(T[] array, int index)
        {
            List.CopyTo(array, index);
        }

        public int Count
        {
            get { return List.Count; }
        }

        /// <summary>
        /// Index method to set or get the object to and from the collection
        /// </summary>
        public T this[int index]
        {
            get { return (T)List[index]; }
            set { List[index] = value; }
        }

        /// <summary>
        /// This method is used to add a object to the list.
        /// </summary>
        /// <param name="T">Object of type T</param>
        /// <returns></returns>
        public void Add(T personObject)
        {
            List.Add(personObject);

        }

        public void Add(T[] personObjects)
        {
            if (personObjects != null)
                foreach (T person in personObjects)
                    List.Add(person);
        }


        /// <summary>
        /// This method will return whether the List contain this object or not.
        /// </summary>
        /// <param name="T">Object of type T</param>
        /// <returns></returns>
        public virtual bool Contains(T personObject)
        {
            return List.Contains(personObject);
        }

        /// <summary>
        /// This method will contain the ContactPerson index in the list
        /// </summary>
        /// <param name="T">Object of type T</param>
        /// <returns></returns>
        public int IndexOf(T personObject)
        {
            return List.IndexOf(personObject);
        }

        /// <summary>
        /// This method will insert the object in to the list if it is valid.
        /// </summary>
        /// <param name="index">Location in the list</param>
        /// <param name="T">Object of type T</param>
        public void Insert(int index, T personObject)
        {
            List.Insert(index, personObject);
        }

        /// <summary>
        /// This method will remove the ContactPerson from the list
        /// </summary>
        /// <param name="T">Object of type T</param>
        public bool Remove(T personObject)
        {
            return List.Remove(personObject);
        }

        //Creating a template type using new opertor is supported in 3.5 and we should use it 
        //instead of doing it in each class
        public T[] ToArray()
        {
            T[] retList = new T[this.Count];

            ICollection<T> collection = (ICollection<T>)this.List;
            collection.CopyTo(retList, 0);
            return retList;
        }


        #region ICloneable Members

        public object Clone()
        {
            CollectionBaseEx<T> result = (CollectionBaseEx<T>)(MemberwiseClone());

            result.List = new List<T>();

            foreach (T item in List)
            {
                ICloneable cloneable = item as ICloneable;
                if (cloneable == null)
                    throw new NotImplementedException();
                result.List.Add((T)cloneable.Clone());
            }

            return result;
        }

        #endregion
    }

    public class PositionFilter
    {
        #region private
        string _portfolioId = "-1";
        string _secId = string.Empty;
        bool _lockedYN = true;
        bool _excludeYN = false;
        int _pct = -1;
        #endregion

        #region public

        public string AccountId { get; set; }
        public string SecId { get; set; }
        public string PortfolioId
        {
            get { return _portfolioId; }
            set { _portfolioId = value; }
        }
        public bool lockedYN
        {
            get { return _lockedYN; }
            set { _lockedYN = value; }
        }
        public bool ExcludeYN
        {
            get { return _excludeYN; }
            set { _excludeYN = value; }
        }
        public int Pct
        {
            get { return _pct; }
            set { _pct = value; }
        }

        #endregion
    }

    public class PositionFilterCollection : CollectionBaseEx<PositionFilter>
    {
        public PositionFilterCollection()
        {
        }

        #region public

        /// <summary>
        ///		Converts the Collectionbase to array
        /// </summary>
        /// <returns>Returns the Array of Account Class</returns>
        public PositionFilter[] ToArray()
        {
            PositionFilter[] PositionFilterList = new PositionFilter[this.Count];

            ICollection<PositionFilter> collection = (ICollection<PositionFilter>)this;
            collection.CopyTo(PositionFilterList, 0);
            return PositionFilterList;
        }

        #endregion public
    }

    public class AssetClassification
    {
        public string Idxid { get; set; }
        public string AssetClassid { get; set; }
        public string Pct { get; set; }
        public string AssetClassName { get; set; }
    }

    public class HoldingRequest
    {
        public AccountPosition accountPosition { get; set; }
        public string planId { get; set; }
    }

    public class PositionAmountPair
    {
        public long m_lPositionID;
        public double m_dAmount;
    }

    public enum TransactionType
    {
        INVALID_TRANSACTIONTYPE = -1,
        BUY = 1,
        SELL,
        SHORT,
        COVER
    }


    /////model for LockExclude
    public class LockExcludeRequest
    {
        public LockExclAcct accountPosition { get; set; }
        public string planId { get; set; }
    }

    public class LockExclAcct
    {
        public string AccountID { get; set; }
        public List<LockExclPosition> Positions { get; set; } = new List<LockExclPosition>();
    }

    public class LockExclPosition
    {
        public string SecID { get; set; }
        public bool LockedYN { get; set; }
        public bool ExcludeYN { get; set; }
    }

    /////Create/Update Account 
    public class AccountBasicInfoRequest
    {

        #region Properties
        public string HouseholdId { get; set; }
        public string AccountID { get; set; }
         public string AccountName { get; set; }
        public string AccountNumber { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public TaxTreatment TaxTreatment { get; set; } = TaxTreatment.DEFERRED;
        public long AccountTypeId { get; set; } = 30;
        [JsonConverter(typeof(StringEnumConverter))]
        public OwnershipType OwnershipType { get; set; } = OwnershipType.INDIVIDUAL;
        public string PrimaryOwnerId { get; set; }    
        public string SecondaryOwnerId { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public AccountDetail AccountDetail { get; set; } = Models.AccountDetail.SECURITY;


        #endregion Properties

    }


    ////Create/Update Save Security Positions
    public class SaveSecurityPositionsRequest
    {
        public string AccountID { get; set; } = string.Empty;
        public double CashComponent { get; set; }
        public List<SaveSecurityPosition> Positions { get; set; } = new List<SaveSecurityPosition>();


    }
    public class SaveSecurityPosition
    {
        public PositionTerm PositionTerm { get; set; } = PositionTerm.LONG;
        public string Qty { get; set; }
        public double CurrentPrice { get; set; }
        public string OpenDate { get; set; } = string.Empty;
        public MarketValueEntry Marketalue_entry_mode { get; set; } = MarketValueEntry.PRICE_AND_QUANTITY;
        public string SecID { get; set; } = string.Empty;
        public double Commissions { get; set; }

    }





}