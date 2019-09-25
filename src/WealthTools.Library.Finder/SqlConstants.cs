using System;
using System.Collections.Generic;
using System.Text;

namespace WealthTools.Library.Search
{
    public class SqlConstants
    {
        public const string SearchSecurities = @"
             SELECT
            SECURITY.SEC_ID,
            SECURITY.SYMBOL,
            SECURITY.CUSIP,
            SECURITY.TA_ID,
            SECURITY.SECURITY_NAME,
            SECURITY.SECURITY_TYPE,
            SECURITY.LAST_CLOSE_PRICE,
            SECURITY.COG_ID,
            SECURITY.FACTSHEET_FILE_NAME,
            SECURITY.external_id as Sec_No,
            SECURITY.PRICE_FACTOR,
            SECURITY.SUB_TYPE,
            'Y' AS IS_CONTEXT_ELIGIBLE,
            'Y' AS IS_MODEL_ELIGIBLE
            FROM SECURITY          
            WHERE SECURITY.SECURITY_TYPE not in ( 'VC', 'VA', 'UD' )
            AND
            (SECURITY.SYMBOL IN (:REPLACE_SEARCH_KEYS) OR SECURITY.CUSIP IN (:REPLACE_SEARCH_KEYS))            
            ";

    }
}
