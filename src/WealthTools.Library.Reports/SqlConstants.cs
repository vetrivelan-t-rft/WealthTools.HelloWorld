using System;
using System.Collections.Generic;
using System.Text;

namespace WealthTools.Library.Reports
{
   internal class SqlConstants
    {
        public const string READ_REPORT_LIST = @"SELECT   c.report_seq AS inst_report_seq, c.print_seq AS inst_print_seq,
                     c.usage_code AS inst_usage_code,c.report_type_id AS inst_report_type_id,c.inst_type_name as inst_report_type_name,a.report_id, a.NAME AS report_name,
                     a.product_id, a.report_type_id, a.report_seq, a.print_seq,
                     a.usage_code, b.NAME AS report_type_name,b.workflow_type_id
                FROM web_report a,
                     web_report_type b,
                     ( SELECT i.report_id, i.report_seq, i.print_seq, i.usage_code,i.report_type_id,t.name as inst_type_name  FROM
                      web_inst_report i,web_report_type t where i.report_type_id=t.report_type_id(+) and institution_id = :INSTITUTION_ID) c
               WHERE a.product_id =:PRODUCT_ID
                 AND a.report_type_id = b.report_type_id
                 AND a.report_id = c.report_id(+)
            ORDER BY report_id";

        public const string READ_REPORT_TEMPLATE = @"SELECT WRT.REPORT_TEMPLATE_ID,WRTR.REPORT_ID ,WRTR.REPORT_TYPE_ID, WRTR.INCLUDE_YN,WRTR.DISPLAY_OPTION
                FROM WEB_REPORT_TEMPLATE WRT, WEB_REPORT_TEMPLATE_X_REPORT WRTR, WEB_INST_REPORT_TEMPLATE WIRT
                WHERE WRT.REPORT_TEMPLATE_ID = WIRT.REPORT_TEMPLATE_ID
                AND WIRT.INSTITUTION_ID = :INSTITUTION_ID  
                AND WRT.PRODUCT_MODULE_ID = :PRODUCTMODULE_ID                            
                AND WIRT.REPORT_TEMPLATE_ID = WRTR.REPORT_TEMPLATE_ID ORDER BY REPORT_TEMPLATE_ID ";

        public const string READ_REPORT_TEMPLATE_LIST = @"SELECT WRT.REPORT_TEMPLATE_ID,WRTR.REPORT_ID ,WRTR.REPORT_TYPE_ID, WRTR.INCLUDE_YN,WRTR.DISPLAY_OPTION
                FROM WEB_REPORT_TEMPLATE WRT, WEB_REPORT_TEMPLATE_X_REPORT WRTR, WEB_INST_REPORT_TEMPLATE WIRT
                WHERE WRT.REPORT_TEMPLATE_ID = WIRT.REPORT_TEMPLATE_ID
                AND WIRT.INSTITUTION_ID = :INSTITUTION_ID  
                AND WRT.PRODUCT_MODULE_ID = :PRODUCTMODULE_ID                            
                AND WIRT.REPORT_TEMPLATE_ID = WRTR.REPORT_TEMPLATE_ID ORDER BY REPORT_TEMPLATE_ID";


        public const string READ_REPORT_USER_TEMPLATE_LIST = @"SELECT WURT.REPORT_TEMPLATE_ID, WURT.NAME, 'Y' ACTIVE_YN, 'Y' DEFAULT_YN, ROWNUM AS TEMPLATE_ORDER
                FROM   WEB_USER_RPT_TEMPLATE WURT
                WHERE      WURT.INSTITUTION_ID = :INSTITUTION_ID
                           AND WURT.PRODUCT_MODULE_ID = :PRODUCTMODULE_ID
                           AND WURT.BROKER_ID = :BROKER_ID
                ORDER BY   WURT.NAME ";


    }
}
