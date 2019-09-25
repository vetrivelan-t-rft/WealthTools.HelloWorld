using System;
using System.Collections.Generic;

namespace WealthTools.Library.Accounts.Models
{
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
        public string Account_name { get; set; }
        public string Account_number { get; set; }
        public string Error_message { get; set; }
        public bool TestAccount { get; set; }
        public string Attribute2 { get; set; }
        public string Display_type_id { get; set; }

        #endregion Properties

    }

    public class AccountBasics : AccountKeyInfo
    {

        public enum tAccountDetail
        {

            INVALID_ACCOUNTDETAIL = -1,

            ACCT_DETAIL_SECURITY,

            ACCT_DETAIL_ASSET_CLASS,

            ACCT_DETAIL_MIXED
        };

        public enum tTaxTreatment
        {

            INVALID_TAXTREATMENT = -1,

            TAX_TREATMENT_DEFERRED,

            TAX_TREATMENT_EXEMPT,

            TAX_TREATMENT_TAXABLE,

            TAX_TREATMENT_TAX_ADVANTAGED_EDUCATION,

            TAX_TREATMENT_NON_QUALIFIED
        };

        public enum tOwnershipType
        {

            INVALID_OWNERSHIPTYPE = -1,

            OWNERSHIP_JOINT,

            OWNERSHIP_INDIVIDUAL
        };

        #region Properties
        private string includeAccountYn = "N";
        public string Include_account_yn
        {
            get { return includeAccountYn; }
            set { includeAccountYn = value; }
        }
        //[DBField("IRS_NAME")]
        public string IrsName { get; set; }
        //[DBField("OWNER_NAME")]
        public string Owner_name { get; set; }
        //[DBField("ATTR2")]
        public string ATTR2 { get; set; }
        //[DBField("PROGRAM_ID")]
        public string Program_id { get; set; }
        //[DBField("cash_component")]
        private double cash_component;
        public double Cash_component
        {
            get
            {
                if (this.rate.Equals(0.0)) return Math.Round(cash_component * this.rate, 4);
                else return cash_component;
            }
            set { cash_component = value; }
        }
        //[DBField("CURRENCY_CODE")]
        public string CurrencyCode { get; set; }
        //[DBField("rate")]
        private double rate;
        public double Rate {
            get { return rate; }
            set { rate = value; }
        }
        //[DBField("LONG_MARKET_VALUE")]
        public double Long_balance { get; set; }
        //[DBField("SHORT_MARKET_VALUE")]
        public double Short_balance { get; set; }
        //[DBField("MARKET_VALUE")]
        public double Market_value { get; set; }
        //CREATE_DATE
        private bool internal_yn;
        public bool InternalValue
        {
            get { return internal_yn; }
            set { internal_yn = value; }
        }
        //[DBField("internal_managed_yn")]
        public string InternalValueDBField
        { set { internal_yn = value != "N" ? true : false; } }
        //[DBField("manual_flag")]
        public bool ManualDBField { get; set; } //value != "N" ? true : false;

        private tTaxTreatment tax_status_desc;
        //[DBField("TAX_STATUS_ID")]
        public long Tax_treatmentDBField
        {
            set
            {
                switch (value)
                {
                    case 0:
                        tax_status_desc = tTaxTreatment.TAX_TREATMENT_DEFERRED;
                        tax_treatment = tTaxTreatment.TAX_TREATMENT_DEFERRED;
                        break;
                    case 1:
                        tax_status_desc = tTaxTreatment.TAX_TREATMENT_EXEMPT;
                        tax_treatment = tTaxTreatment.TAX_TREATMENT_EXEMPT;
                        break;
                    case 2:
                        tax_status_desc = tTaxTreatment.TAX_TREATMENT_TAXABLE;
                        tax_treatment = tTaxTreatment.TAX_TREATMENT_TAXABLE;
                        break;
                    case 3:
                        tax_status_desc = tTaxTreatment.TAX_TREATMENT_TAX_ADVANTAGED_EDUCATION;
                        tax_treatment = tTaxTreatment.TAX_TREATMENT_TAX_ADVANTAGED_EDUCATION;
                        break;
                    case 4:
                        tax_status_desc = tTaxTreatment.TAX_TREATMENT_NON_QUALIFIED;
                        tax_treatment = tTaxTreatment.TAX_TREATMENT_NON_QUALIFIED;
                        break;
                    default:
                        tax_status_desc = tTaxTreatment.INVALID_TAXTREATMENT;
                        tax_treatment = tTaxTreatment.INVALID_TAXTREATMENT;
                        break;
                }
            }
            //get { return Convert.ToInt64(tax_status_desc); }
        }
        public tTaxTreatment getTaxTreatment()
        {
            return tax_status_desc;
        }
        private tTaxTreatment tax_treatment;
        public string Tax_treatment
        {
            get { return TaxTreatmentToString(tax_treatment); }
            set { tax_treatment = StringToTaxTreatment(value); }
        }
        /* Convert a tax treatment enum to an account detail string for the set method.*/
        public string TaxTreatmentToString(tTaxTreatment eDetail)
        {
            switch (eDetail)
            {
                case tTaxTreatment.TAX_TREATMENT_TAXABLE:
                    return "TAXABLE";
                case tTaxTreatment.TAX_TREATMENT_DEFERRED:
                    return "TAX_DEFERRED";
                case tTaxTreatment.TAX_TREATMENT_EXEMPT:
                    return "EXEMPT";
                case tTaxTreatment.TAX_TREATMENT_TAX_ADVANTAGED_EDUCATION:
                    return "TAX_ADVANTAGED_EDUCATION";
                case tTaxTreatment.TAX_TREATMENT_NON_QUALIFIED:
                    return "NON_QUALIFIED";
                default:
                    return "";
            }
        }
        /* Convert a tax treatment string to an account detail enum for the get method.*/
        public tTaxTreatment StringToTaxTreatment(string sDetail)
        {
            if (sDetail == "TAXABLE")
                return tTaxTreatment.TAX_TREATMENT_TAXABLE;
            else if (sDetail == "TAX_DEFERRED")
                return tTaxTreatment.TAX_TREATMENT_DEFERRED;
            else if (sDetail == "EXEMPT")
                return tTaxTreatment.TAX_TREATMENT_EXEMPT;
            else if (sDetail == "TAX_ADVANTAGED_EDUCATION")
                return tTaxTreatment.TAX_TREATMENT_TAX_ADVANTAGED_EDUCATION;
            else if (sDetail == "NON_QUALIFIED")
                return tTaxTreatment.TAX_TREATMENT_NON_QUALIFIED;
            else
                return tTaxTreatment.INVALID_TAXTREATMENT;
        }

        private string nature_of_acct = string.Empty;
        //[DBField("NATURE_OF_ACCOUNT")]
        public string Nature_of_acct
        {
            get { return String.IsNullOrEmpty(nature_of_acct) ? Tax_treatment : nature_of_acct; }
            set { nature_of_acct = value; }
        }

        private tOwnershipType ownership_type;
        //[DBField("FILING_STATUS_ID")]
        public long Ownership_typeDBField
        {
            set
            {
                switch (value)
                {
                    case 0:
                        ownership_type = tOwnershipType.OWNERSHIP_JOINT;
                        break;
                    case 1:
                        ownership_type = tOwnershipType.OWNERSHIP_INDIVIDUAL;
                        break;
                    default:
                        ownership_type = tOwnershipType.INVALID_OWNERSHIPTYPE;
                        break;
                }
            }
            //get { return Convert.ToInt64(ownership_type); }

        }
        public tOwnershipType getOwnershipType()
        {
            return ownership_type;
        }
        public string Ownership_type
        {
            get { return OwnershipTypeToString(ownership_type); }
            set { ownership_type = StringToOwnershipType(value); }
        }
        /*Convert a tax treatment enum to an account detail string for the set method.*/
        public string OwnershipTypeToString(tOwnershipType eDetail)
        {
            switch (eDetail)
            {
                case tOwnershipType.OWNERSHIP_JOINT:
                    return "JOINT";
                case tOwnershipType.OWNERSHIP_INDIVIDUAL:
                    return "INDIVIDUAL";
                default:
                    return "";
            }
        }

        /* Convert a tax treatment string to an account detail enum for the get method.*/
        public tOwnershipType StringToOwnershipType(string sDetail)
        {
            if (sDetail == "JOINT")
                return tOwnershipType.OWNERSHIP_JOINT;
            else if (sDetail == "INDIVIDUAL")
                return tOwnershipType.OWNERSHIP_INDIVIDUAL;
            else
                return tOwnershipType.INVALID_OWNERSHIPTYPE;
        }

        //[DBField("ACCOUNT_TYPE_ID")]
        public long Account_type_id { get; set; }
        //[DBField("PRIMARY_OWNER_ID")]
        public string Primary_owner_id { get; set; }
        //[DBField("ACCT_OWNER_ID")]
        public string Acct_owner_id { get; set; }
        //[DBField("SECONDARY_OWNER_ID")]
        public string Secondary_owner_id { get; set; }

        private tAccountDetail account_detail;
        public tAccountDetail getAcctDetail()
        {
            return account_detail;
        }
        //[DBField("ACCOUNT_DETAIL")]
        public string AccountDetailDBField
        {
            set
            {
                if (value == "Y")
                {
                    account_detail = tAccountDetail.ACCT_DETAIL_SECURITY;
                }
                else
                    account_detail = tAccountDetail.ACCT_DETAIL_ASSET_CLASS;
            }
        }

        public string Account_detail
        {
            get
            {
                return AcctDetailToString(account_detail);

            }
            set
            {
                account_detail = StringToAcctDetail(value);
            }
        }
        public string AcctDetailToString(tAccountDetail eDetail)
        {
            switch (eDetail)
            {
                case tAccountDetail.ACCT_DETAIL_SECURITY:
                    return "SECURITY";
                case tAccountDetail.ACCT_DETAIL_ASSET_CLASS:
                    return "ASSET_CLASS";
                case tAccountDetail.ACCT_DETAIL_MIXED:
                    return "MIXED";
                default:
                    return "";
            }
        }
        public tAccountDetail StringToAcctDetail(string sDetail)
        {
            if (sDetail == "SECURITY")
                return tAccountDetail.ACCT_DETAIL_SECURITY;
            else if (sDetail == "ASSET_CLASS")
                return tAccountDetail.ACCT_DETAIL_ASSET_CLASS;
            else if (sDetail == "MIXED")
                return tAccountDetail.ACCT_DETAIL_MIXED;
            else
                return tAccountDetail.INVALID_ACCOUNTDETAIL;
        }

        //[DBField("PORTFOLIO_ACCT_TYPE")]
        public string Portfolio_Acct_Type { get; set; }
        //[DBField("DATASOURCE_ID")]
        public int DatasourceID { get; set; }
        private bool is_cash_balance_preset = false;
        public bool Is_cash_balance_preset
        {
            get { return is_cash_balance_preset; }
            set { is_cash_balance_preset = value; }
        }
        private double preset_cash_balance = 0;
        public double Preset_cash_balance
        {
            get { return preset_cash_balance; }
            set { preset_cash_balance = value; }
        }
        #endregion Properties
    }

    public class AccountPosition : AccountBasics
    {
        #region IAnyList

        #endregion IAnyList

        #region Properties
        public List<OpenPosition> Positions { get; set; } = new List<OpenPosition>();
        //[DBField("market_value")]
        public double AccountCashBalance { get; set; }
        public bool isSpecialProductAcct { get; set; }
        public int Sec_Count { get; set; }
        public bool IsExcludeOn { get; set; }
        public bool IsLockOn { get; set; }
        //[DBField("last_modified_date")]
        public string LastUpdatedDate { get; set; }
        public string Last_modified_date { get; set; }
        #endregion Properties
    }

    public interface ICurrencyInfo
    {
        // Property signatures:
        string CurrencyCode {get; set;}

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
        public string Sec_sedol { get; set; }
        public string Sec_name { get; set; }
        public string Sec_type { get; set; }
        public string Sub_type { get; set; }
        public string Sec_symbol { get; set; }
        public double Price_factor
        {
            get { return price_factor<=0.0 ? 1.0 : price_factor; }
            set { price_factor = value; }
        }
        public double Price_ADJ_factor
        {
            get { return price_adj_factor <= 0.0 ? Price_factor : price_adj_factor; }
            set { price_adj_factor = value; }
        }
        public string Currency_Code
        {
            get { return currency_code; }
            set { currency_code = value; }
        }
        public double Currency_Conv_Factor
        {
            get { return currency_conv_factor; }
            set { currency_conv_factor = value; }
        }
        public string Sec_status { get; set; }
        public bool FileExists { get; set; }
        public bool HasFactSheet { get; set; }
        #endregion
    }

    public class PositionBase : ICurrencyInfo
    {
        public enum tPositionTerm
        {
            INVALID_POSITIONTERM = -1,
            LONG = 1,
            SHORT
        };

        public enum tMarketValueEntry
        {
            INVALID_MARKETVALUEENTRY = -1,
            PRICE_AND_QUANTITY = 1,
            MARKET_VALUE
        };

        #region Properties
        public string Pos_type { get; set; }
        public string AccountID { get; set; }
        public long Pos_id { get; set; }
        public string CogId { get; set; }
        public string SecID { get; set; }
        public double Qty { get; set; }
        public double Current_priceDB
        {
            get { return current_price; }
            set { current_price = value; }
        }
        protected double current_price;
        public double Current_price
        {
            get
            {
                if (this.Security_Detail.Currency_Conv_Factor.Equals(0.0)) return Math.Round(current_price * this.Security_Detail.Currency_Conv_Factor, 4);
                else return current_price;
            }
            set { current_price = value; }
        }
        private string pricing_date = string.Empty;
       //[DBField("AS_OF_DATE")]
        public string Pricing_date
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
        public string Open_date
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
        public double Open_price { get; set; }
        //[DBField("MARKET_VALUE")]
        public double Market_value { get; set; }
        private double commissions;
        //[DBField("COMMISSION")]
        public double Commissions
        {
            get {
                if (this.Security_Detail.Currency_Conv_Factor.Equals(0.0)) return Math.Round(commissions * this.Security_Detail.Currency_Conv_Factor, 4);
                else return commissions;
            }
            set { commissions = value; }
        }
        private tPositionTerm position_term = tPositionTerm.INVALID_POSITIONTERM;
        public tPositionTerm Position_term
        {
            get { return position_term; }
            set { position_term = value; }
        }
        //[DBField("POSITION_TERM")]
        public string Position_termDB
        {
            set { position_term = (tPositionTerm)Enum.Parse(typeof(tPositionTerm), value); }
        }
        private tMarketValueEntry market_value_entry_mode = tMarketValueEntry.INVALID_MARKETVALUEENTRY;
        public tMarketValueEntry Market_value_entry_mode
        {
            get { return market_value_entry_mode; }
            set { market_value_entry_mode = value; }
        }
        //[DBField("MKT_VAL_ENTRY_MODE")]
        public string Market_value_entry_modeDB
        {
            set { market_value_entry_mode = (tMarketValueEntry)Enum.Parse(typeof(tMarketValueEntry), value); }
        }
        private bool is_market_value_preset;
        public bool Is_market_value_preset
        {
            get { return is_market_value_preset; }
            set { is_market_value_preset = value; }
        }
        //[DBField("MARKET_VALUE_PRESET")]
        public string Is_market_value_presetYN
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
        public SecurityBaseEx Security_Detail
        {
            get { return security_details; }
            set { security_details = value; }
        }
        public bool Locked_yn { get; set; }
        public bool Exclude_yn { get; set; }
        public string DisplayPrice { get; set; }
        public string DisplayMarketValue { get; set; }
        #endregion Properties

        #region ICurrencyInfo Members
        private string currencyCode;
        public string CurrencyCode
        {
            get
            {
                return this.security_details.Currency_Code;
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
}