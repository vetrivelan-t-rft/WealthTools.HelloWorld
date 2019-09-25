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
            if (accountPos.Is_cash_balance_preset)
                dBalance = accountPos.Preset_cash_balance;
            else
            {
                dBalance = accountPos.Cash_component;
                // get the list of open positions, find the balance
                foreach (PositionBase post in accountPos.Positions)
                {
                    // check for either market price and quantity or amount
                    double dQuantity = post.Qty;
                    double dFactor = 1.0;
        
                    if (post.Position_term == PositionBase.tPositionTerm.SHORT)
                        dFactor = post.Market_value >= 0 ? -1.0 : 1.0;
        
                    // quantity is stored, use this instead of market value
                    if (post.Is_market_value_preset)
                    {
                        dBalance += dFactor * (post.Market_value * post.Security_Detail.Currency_Conv_Factor);
                    }
                    else
                    {
                        post.Market_value = post.Qty * post.Current_priceDB * post.Security_Detail.Price_factor;
                        if (post.CurrentFactor != 0.0)
                            post.Market_value *= post.CurrentFactor;
                        dBalance += dFactor * post.Market_value;
                        post.Is_market_value_preset = true;
                    }
                }
            }
            return dBalance;
        }
    
        public static double CalculateLongBalance(AccountPosition accountPos)
        {
            double dBalance = 0.0;
            if (accountPos.Is_cash_balance_preset)
                dBalance = accountPos.Long_balance;
            else
            {
                //dBalance = accountPos.Long_balance;
                // get the list of open positions, find the balance
                foreach (PositionBase post in accountPos.Positions)
                {
                    if (post.Position_term == PositionBase.tPositionTerm.SHORT)
                        continue;

                    // check for either market price and quantity or amount
                    double dQuantity = post.Qty;
                    // quantity is stored, use this instead of market value
                    if (post.Is_market_value_preset)
                        dBalance += (post.Market_value);
                    else
                    {
                        post.Market_value = post.Qty * post.Current_priceDB * post.Security_Detail.Price_factor;
                        dBalance += post.Market_value;
                        post.Is_market_value_preset = true;
                    }
                }
            }
            return dBalance;
        }
    
        public static double CalculateShortBalance(AccountPosition accountPos)
        {
            double dBalance = 0.0;
            if (accountPos.Is_cash_balance_preset)
                dBalance = accountPos.Short_balance;
            else
            {
                // get the list of open positions, find the balance
                foreach (PositionBase post in accountPos.Positions)
                {
                    if (post.Position_term == PositionBase.tPositionTerm.LONG)
                        continue;
        
                    // check for either market price and quantity or amount
                    double dQuantity = post.Qty;
                    // quantity is stored, use this instead of market value
                    if (post.Is_market_value_preset)
                        dBalance += (post.Market_value);
                    else
                    {
                        post.Market_value = post.Qty * post.Current_priceDB * post.Security_Detail.Price_factor;
                        dBalance += post.Market_value;
                        post.Is_market_value_preset = true;
                    }
                }
            }
            return Math.Abs(dBalance);
        }
    
    public static double CalculateBalanceBasedOfPriceFactor(AccountPosition accountPos)
    {
        double dBalance = 0.0;
    
        if (accountPos.Is_cash_balance_preset)
            dBalance = accountPos.Preset_cash_balance;
        else
        {
            dBalance = accountPos.Cash_component;
    
            // get the list of open positions, find the balance
            foreach (PositionBase post in accountPos.Positions)
            {
                // check for either market price and quantity or amount
                double dQuantity = post.Qty;
                double dFactor = 1.0;
    
                if (post.Position_term == PositionBase.tPositionTerm.SHORT)
                    dFactor = post.Market_value >= 0 ? -1.0 : 1.0;
    
                // quantity is stored, use this instead of market value
                if (post.Is_market_value_preset)
                {
                    dBalance += dFactor * (post.Market_value * post.Security_Detail.Currency_Conv_Factor);
                }
                else
                {
                    post.Market_value = post.Qty * post.Current_priceDB * post.Security_Detail.Price_ADJ_factor;
                    dBalance += dFactor * (post.Market_value * post.Security_Detail.Currency_Conv_Factor);
                    post.Is_market_value_preset = true;
                }
            }
        }
    
        return dBalance;
    }
    
    public static double CalculateLongBalanceBasedOfPriceFactor(AccountPosition accountPos)
    {
        double dBalance = 0.0;
    
        if (accountPos.Is_cash_balance_preset)
            dBalance = accountPos.Long_balance;
        else
        {
            //dBalance = accountPos.Long_balance;
    
            // get the list of open positions, find the balance
            foreach (PositionBase post in accountPos.Positions)
            {
                if (post.Position_term == PositionBase.tPositionTerm.SHORT)
                    continue;
    
                // check for either market price and quantity or amount
                double dQuantity = post.Qty;
    
                // quantity is stored, use this instead of market value
                if (post.Is_market_value_preset)
                    dBalance += (post.Market_value);
                else
                {
                    post.Market_value = post.Qty * post.Current_priceDB * post.Security_Detail.Price_ADJ_factor;
                    dBalance += post.Market_value;
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
                    if (lastDate.CompareTo(post.Pricing_date) < 0)
                        lastDate = post.Pricing_date;
                }

                return lastDate;
            }
        }
    }

