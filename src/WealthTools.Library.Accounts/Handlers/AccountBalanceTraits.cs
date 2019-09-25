using System;
using System.Collections.Generic;
using System.Text;
using WealthTools.Library.Accounts.Models;

namespace WealthTools.Library.Accounts.Handlers
{
    public class AccountBalanceTraits
    {
        public static double CalculateBalance(AccountPosition accountPos)
        {
            double dBalance = 0.0;
            if (accountPos.IsCashBalancePreset)
                dBalance = accountPos.PresetCashBalance;
            else
            {
                dBalance = accountPos.CashComponent;
                // get the list of open positions, find the balance
                foreach (PositionBase post in accountPos.Positions)
                {
                    // check for either market price and quantity or amount
                    double dQuantity = post.Qty;
                    double dFactor = 1.0;
        
                    if (post.PositionTerm == PositionTerm.SHORT)
                        dFactor = post.MarketValue >= 0 ? -1.0 : 1.0;
        
                    // quantity is stored, use this instead of market value
                    if (post.Is_market_value_preset)
                    {
                        dBalance += dFactor * (post.MarketValue * post.SecurityDetail.CurrencyConvFactor);
                    }
                    else
                    {
                        post.MarketValue = post.Qty * post.CurrentPriceDB * post.SecurityDetail.PriceFactor;
                        if (post.CurrentFactor != 0.0)
                            post.MarketValue *= post.CurrentFactor;
                        dBalance += dFactor * post.MarketValue;
                        post.Is_market_value_preset = true;
                    }
                }
            }
            return dBalance;
        }
    
        public static double CalculateLongBalance(AccountPosition accountPos)
        {
            double dBalance = 0.0;
            if (accountPos.IsCashBalancePreset)
                dBalance = accountPos.LongBalance;
            else
            {
                //dBalance = accountPos.LongBalance;
                // get the list of open positions, find the balance
                foreach (PositionBase post in accountPos.Positions)
                {
                    if (post.PositionTerm == PositionTerm.SHORT)
                        continue;

                    // check for either market price and quantity or amount
                    double dQuantity = post.Qty;
                    // quantity is stored, use this instead of market value
                    if (post.Is_market_value_preset)
                        dBalance += (post.MarketValue);
                    else
                    {
                        post.MarketValue = post.Qty * post.CurrentPriceDB * post.SecurityDetail.PriceFactor;
                        dBalance += post.MarketValue;
                        post.Is_market_value_preset = true;
                    }
                }
            }
            return dBalance;
        }
    
        public static double CalculateShortBalance(AccountPosition accountPos)
        {
            double dBalance = 0.0;
            if (accountPos.IsCashBalancePreset)
                dBalance = accountPos.ShortBalance;
            else
            {
                // get the list of open positions, find the balance
                foreach (PositionBase post in accountPos.Positions)
                {
                    if (post.PositionTerm == PositionTerm.LONG)
                        continue;
        
                    // check for either market price and quantity or amount
                    double dQuantity = post.Qty;
                    // quantity is stored, use this instead of market value
                    if (post.Is_market_value_preset)
                        dBalance += (post.MarketValue);
                    else
                    {
                        post.MarketValue = post.Qty * post.CurrentPriceDB * post.SecurityDetail.PriceFactor;
                        dBalance += post.MarketValue;
                        post.Is_market_value_preset = true;
                    }
                }
            }
            return Math.Abs(dBalance);
        }
    
    public static double CalculateBalanceBasedOfPriceFactor(AccountPosition accountPos)
    {
        double dBalance = 0.0;
    
        if (accountPos.IsCashBalancePreset)
            dBalance = accountPos.PresetCashBalance;
        else
        {
            dBalance = accountPos.CashComponent;
    
            // get the list of open positions, find the balance
            foreach (PositionBase post in accountPos.Positions)
            {
                // check for either market price and quantity or amount
                double dQuantity = post.Qty;
                double dFactor = 1.0;
    
                if (post.PositionTerm == PositionTerm.SHORT)
                    dFactor = post.MarketValue >= 0 ? -1.0 : 1.0;
    
                // quantity is stored, use this instead of market value
                if (post.Is_market_value_preset)
                {
                    dBalance += dFactor * (post.MarketValue * post.SecurityDetail.CurrencyConvFactor);
                }
                else
                {
                    post.MarketValue = post.Qty * post.CurrentPriceDB * post.SecurityDetail.Price_ADJ_factor;
                    dBalance += dFactor * (post.MarketValue * post.SecurityDetail.CurrencyConvFactor);
                    post.Is_market_value_preset = true;
                }
            }
        }
    
        return dBalance;
    }
    
    public static double CalculateLongBalanceBasedOfPriceFactor(AccountPosition accountPos)
    {
        double dBalance = 0.0;
    
        if (accountPos.IsCashBalancePreset)
            dBalance = accountPos.LongBalance;
        else
        {
            //dBalance = accountPos.LongBalance;
    
            // get the list of open positions, find the balance
            foreach (PositionBase post in accountPos.Positions)
            {
                if (post.PositionTerm == PositionTerm.SHORT)
                    continue;
    
                // check for either market price and quantity or amount
                double dQuantity = post.Qty;
    
                // quantity is stored, use this instead of market value
                if (post.Is_market_value_preset)
                    dBalance += (post.MarketValue);
                else
                {
                    post.MarketValue = post.Qty * post.CurrentPriceDB * post.SecurityDetail.Price_ADJ_factor;
                    dBalance += post.MarketValue;
                    post.Is_market_value_preset = true;
                }
            }
        }
    
        return dBalance;
    }
        }

        public class AccountDateTraits
        {
            public static string CalculateLastUpdatedDate(AccountPosition accountPos)
            {
                string lastDate = string.Empty;

                // get the list of open positions, find the balance
                foreach (PositionBase post in accountPos.Positions)
                {
                    if (lastDate.CompareTo(post.PricingDate) < 0)
                        lastDate = post.PricingDate;
                }

                return lastDate;
            }
        }
    }

