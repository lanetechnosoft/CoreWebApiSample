using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using CoreWebApi.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ServiceReference1;
using ServiceReference2;

namespace CoreWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        [HttpGet]
        public String Hello() {
            return "hello";
        }

        [HttpGet("getAccountDetail/{Account}")]
        public async Task<IActionResult> GetAccountAsync([FromRoute] String Account)
        {

            if (Account.Length==0|| Account.Equals("")|| Account==null) {
                OkObjectResult result = Ok(new { result = false, status = HttpStatusCode.NotFound, message = "something input wrong", data = "" });
                return result;
            }

            try {
                
                var proxy = new FCUBSAccServiceSEIClient();
                await proxy.OpenAsync();

                ServiceReference1.FCUBS_HEADERType header = new ServiceReference1.FCUBS_HEADERType();
                QUERYCUSTACC_IOFS_REQ reqMsg = new QUERYCUSTACC_IOFS_REQ();
                QUERYCUSTACC_IOFS_REQFCUBS_BODY reqbody = new QUERYCUSTACC_IOFS_REQFCUBS_BODY();

                CustAccQueryIOType body = new CustAccQueryIOType();

                header.SOURCE = "BEGW";
                header.UBSCOMP = ServiceReference1.UBSCOMPType.FCUBS;
                header.USERID = "444444";
                header.BRANCH = "001";
                //header.MODULEID = "WB";
                header.SERVICE = "FCUBSAccService";
                header.OPERATION = "QueryCustAcc";
                //header.FUNCTIONID = "1006";
                reqMsg.FCUBS_HEADER = header;

                body.BRN = Account.Substring(0,3);//"001";
                body.ACC = Account; //"00120010002550177";
                reqbody.CustAccountIO = body;
                reqMsg.FCUBS_BODY = reqbody;

                QueryCustAccIOResponse resMsg = await proxy.QueryCustAccIOAsync(reqMsg);
                QUERYCUSTACC_IOFS_RES res = resMsg.QUERYCUSTACC_IOFS_RES;

                ServiceReference1.MsgStatType status = res.FCUBS_HEADER.MSGSTAT;

                if (!status.ToString().Equals("SUCCESS")) {
                    //await Task.Delay(2000);
                    OkObjectResult result = Ok(new { result = false, status = HttpStatusCode.NotFound,message=res.FCUBS_BODY.FCUBS_ERROR_RESP[0].EDESC, data = res });
                    return result;
                } else {
                    //await Task.Delay(2000);
                    
                    OkObjectResult result = Ok(new { result = true, status = HttpStatusCode.OK, message = res.FCUBS_BODY.FCUBS_WARNING_RESP[0].WDESC, data = res });
                    return result;
                }
                
            } catch (Exception ex) {
                //throw ex;
                return BadRequest(new { result = false, status = HttpStatusCode.BadRequest, data = ex.Message});
                
            }
        }

        [HttpPost("createTrx")]
        public async Task<IActionResult> GetTransaction([FromBody]TransferModelReq req)
        {
            /*
            if (Account.Length == 0 || Account.Equals("") || Account == null)
            {
                OkObjectResult result = Ok(new { result = false, status = HttpStatusCode.NotFound, message = "something input wrong", data = "" });
                return result;
            }
            */

            try
            {

                var proxy = new FCUBSRTServiceSEIClient();
                await proxy.OpenAsync();

                ServiceReference2.FCUBS_HEADERType header = new ServiceReference2.FCUBS_HEADERType();
                CREATETRANSACTION_FSFS_REQ reqMsg = new CREATETRANSACTION_FSFS_REQ();
                CREATETRANSACTION_FSFS_REQFCUBS_BODY reqbody = new CREATETRANSACTION_FSFS_REQFCUBS_BODY();
                //RetailTellerTypeIO body = new RetailTellerTypeIO();
                RetailTellerTypeFull body = new RetailTellerTypeFull();
                ServiceReference2.ChgdetsType charge = new ServiceReference2.ChgdetsType();
                ServiceReference2.ChgdetsType[] chargeArr = new ServiceReference2.ChgdetsType[1];
                
                header.SOURCE = "PNGW";
                header.UBSCOMP = ServiceReference2.UBSCOMPType.FCUBS;
                header.USERID = "444444";
                header.BRANCH = "001";
                //header.MODULEID = "WB";
                header.SERVICE = "FCUBSRTService";
                header.OPERATION = "CreateTransaction";
                //header.FUNCTIONID = "1006";
                reqMsg.FCUBS_HEADER = header;

                body.BRN =req.txnbrn;
                body.PRD = "MBAP";
                body.TXNBRN = req.txnacc.Substring(0,3);
                body.TXNACC = req.txnacc;
                body.TXNAMT = req.txnamt;
                body.TXNAMTSpecified = true;
                body.OFFSETACC = "00120010002982681"; //"00120010002550177";
                body.OFFSETBRN = "001";
                body.OFFSETCCY = "LAK";
                //body.OFFSETAMT = 1000;
                body.NARRATIVE = req.narrative;

                /*  Add Charge Array Component */
                charge.CHGCOMP = "FEE_TRANSFER_FROM_MOBILE";
                charge.CHGAMT = req.chgamt;
                charge.CHGCCY = "LAK";
                chargeArr[0] = charge;

                body.ChargeDetails = chargeArr;

                reqbody.TransactionDetails = body;
                //reqbody.TransactionDetailsIO = body;
                reqMsg.FCUBS_BODY = reqbody;

                CREATETRANSACTION_FSFS_RES res =  proxy.CreateTransactionFSAsync(reqMsg).Result.CREATETRANSACTION_FSFS_RES;
                //CREATETRANSACTION_IOPK_RES res = resMsg.CREATETRANSACTION_IOPK_RES;
                /*
                OkObjectResult result = Ok(new { result = false, status = HttpStatusCode.OK, message = "Response", data = res });
                return result;
                */
                
                ServiceReference2.MsgStatType status = res.FCUBS_HEADER.MSGSTAT;
                
                if (!status.ToString().Equals("SUCCESS"))
                {
                    //await Task.Delay(2000);
                    OkObjectResult result = Ok(new { result = false, status = HttpStatusCode.NotFound, message = res.FCUBS_BODY.FCUBS_ERROR_RESP[0].EDESC, data = res });
                    return result;
                }
                else
                {
                    //await Task.Delay(2000);

                    OkObjectResult result = Ok(new { result = true, status = HttpStatusCode.OK, message = res.FCUBS_BODY.FCUBS_WARNING_RESP[0].WDESC, data = res.FCUBS_BODY.TransactionDetails,response = res.FCUBS_BODY.TransactionDetails .TXNDATE});
                    return result;
                }
                

            }
            catch (Exception ex)
            {
                //throw ex;
                return BadRequest(new { result = false, status = HttpStatusCode.BadRequest, data = ex.Message });

            }
        }
    }
}
