using GMWebApp.BAL;
using GMWebApp.BAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace GMWebApp.Controllers
{
    public class TenderApprovalController : Controller
    {
        private TenderBAL _tenderBAL;
        // GET: TenderApproval
        public ActionResult Index(string UserID, string UserType, string TenderID, string TenderNo)
        {

            try
            {
                string _UserID = string.Empty;
                string _UserType = string.Empty;
                string _TenderNo = string.Empty;
                string _TenderID = string.Empty;

                if (string.IsNullOrEmpty(UserID) || string.IsNullOrEmpty(UserType) || string.IsNullOrEmpty(TenderID) || string.IsNullOrEmpty(TenderNo))
                {
                    return View("Error");
                }

                _UserID = UserID;// Base64Decode(UserID);
                _UserType = UserType;// Base64Decode(UserType);
                _TenderID = TenderID;// Base64Decode(TenderID);
                _TenderNo = TenderNo;// Base64Decode(TenderNo);

                if (string.IsNullOrEmpty(_UserID) || string.IsNullOrEmpty(_UserType) || string.IsNullOrEmpty(_TenderID)
                     || string.IsNullOrEmpty(_TenderNo))
                {
                    return View("Error");
                }
                _tenderBAL = new TenderBAL();
                UsermasterModel _usermaster = new UsermasterModel();
                List<TenderItemsModel> tenderItemsList = new List<TenderItemsModel>();
                _usermaster = _tenderBAL.ValidateUserLinkAndGetUserDetails(Convert.ToInt32(_UserID), _UserType);

                if (_usermaster != null && _usermaster.Status == "1")
                {

                    _usermaster.TenderID = Convert.ToInt32(_TenderID);
                    _usermaster.TenderNo = _TenderNo;
                    //ViewBag.UserList = _usermaster;
                    tenderItemsList = _tenderBAL.GetPendingForApprovalItemsByUserType(Convert.ToInt32(_TenderID), _TenderNo, _UserType);
                    if (tenderItemsList != null && tenderItemsList.Count > 0)
                    {
                        //ViewBag.tenderItemsList = tenderItemsList;
                        _usermaster.tenderItemsList = tenderItemsList;
                    }
                }
                else
                {
                    return View("Error");
                }

                return View(_usermaster);
            }
            catch (Exception ex)
            {
                return View("Error");
            }
        }

        #region Base 64 Decode
        public static string Base64Decode(string base64EncodedData)
        {
            string resultString = string.Empty;
            try
            {
                var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
                resultString = System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
            }
            catch (Exception ex)
            {
                resultString = "";
            }
            return resultString;
        }
        #endregion


        [HttpPost]
        public ActionResult ApproveTenderItemID(int UserId, string Designation, int tenderItemID, string MaterialCode)
        {
            ResponseModel responseModel = new ResponseModel();
            string message = "Failed to approve the Tender Item.";
            try
            {
                responseModel.Status = "0";
                responseModel.StatusMessage = message;

                _tenderBAL = new TenderBAL();

                responseModel = _tenderBAL.ApprovePendingItemForLoginUser(UserId, Designation, tenderItemID);
                if (responseModel == null || string.IsNullOrWhiteSpace(responseModel.StatusMessage))
                {
                    responseModel = new ResponseModel();
                    responseModel.Status = "0";
                    responseModel.StatusMessage = MaterialCode + " : " + message;
                }
                else
                {
                    responseModel.StatusMessage = MaterialCode + " : " + responseModel.StatusMessage;
                }
            }
            catch (Exception ex)
            {
                responseModel = new ResponseModel();
                responseModel.Status = "0";
                responseModel.StatusMessage = MaterialCode + " : " + ex.Message.ToString();
            }
            return Json(new { responseModel, JsonRequestBehavior.AllowGet });
        }

        [HttpPost]
        public ActionResult ApproveAllTenderItems(int UserId, string Designation, int TenderID, string TenderNo)
        {
            ResponseModel responseModel = new ResponseModel();
            List<TenderItemsModel> tenderItemsList = new List<TenderItemsModel>();
            string message = "Failed to approve the Tender Item.";
            try
            {
                responseModel.Status = "0";
                responseModel.StatusMessage = message;

                _tenderBAL = new TenderBAL();


                tenderItemsList = _tenderBAL.GetPendingForApprovalItemsByUserType(Convert.ToInt32(TenderID), TenderNo, Designation);

                if (tenderItemsList != null && tenderItemsList.Count > 0)
                {
                    if (Designation.ToLower() == "BH".ToLower())
                    {
                        tenderItemsList = tenderItemsList.Where(x => x.Approved_BH.ToLower() == "Not Approved".ToLower()).ToList();
                    }
                    if (Designation.ToLower() == "DH".ToLower())
                    {
                        tenderItemsList = tenderItemsList.Where(x => x.Approved_DH.ToLower() == "Not Approved".ToLower()).ToList();
                    }
                    if (Designation.ToLower() == "IIFH".ToLower())
                    {
                        tenderItemsList = tenderItemsList.Where(x => x.Approved_IIFH.ToLower() == "Not Approved".ToLower()).ToList();
                    }
                    if (Designation.ToLower() == "IF_Head".ToLower())
                    {
                        tenderItemsList = tenderItemsList.Where(x => x.Approved_IF_Head.ToLower() == "Not Approved".ToLower()).ToList();
                    }

                    if (tenderItemsList != null && tenderItemsList.Count > 0)
                    {
                        responseModel.StatusMessage = "";
                        foreach (var itm in tenderItemsList)
                        {
                            responseModel.Status = "1";
                            try
                            {
                                var response = _tenderBAL.ApprovePendingItemForLoginUser(UserId, Designation, itm.ID);

                                if (response == null || string.IsNullOrWhiteSpace(response.StatusMessage))
                                {
                                    if (!string.IsNullOrWhiteSpace(responseModel.StatusMessage))
                                    {
                                        responseModel.StatusMessage = responseModel.StatusMessage + "\n" + itm.MaterialCode + " : " + message;
                                    }
                                    else
                                    {
                                        responseModel.StatusMessage = itm.MaterialCode + " : " + message;
                                    }
                                }
                                else
                                {
                                    if (!string.IsNullOrWhiteSpace(responseModel.StatusMessage))
                                    {
                                        responseModel.StatusMessage = responseModel.StatusMessage + "\n" + itm.MaterialCode + " : " + response.StatusMessage;
                                    }
                                    else
                                    {
                                        responseModel.StatusMessage = itm.MaterialCode + " : " + response.StatusMessage;
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                responseModel = new ResponseModel();
                                responseModel.Status = "0";

                                if (!string.IsNullOrWhiteSpace(responseModel.StatusMessage))
                                {
                                    responseModel.StatusMessage = responseModel.StatusMessage + "\n" + itm.MaterialCode + " : " + ex.Message.ToString();
                                }
                                else
                                {
                                    responseModel.StatusMessage = itm.MaterialCode + " : " + ex.Message.ToString();
                                }
                            }
                        }
                    }
                    else
                    {
                        responseModel = new ResponseModel();
                        responseModel.Status = "0";
                        responseModel.StatusMessage = "No Tender items are pending for approval.";
                    }
                }
                else
                {
                    responseModel = new ResponseModel();
                    responseModel.Status = "0";
                    responseModel.StatusMessage = "No Tender items are pending for approval.";
                }
            }
            catch (Exception ex)
            {
                responseModel = new ResponseModel();
                responseModel.Status = "0";
                responseModel.StatusMessage = ex.Message.ToString();
            }
            return Json(new { responseModel, JsonRequestBehavior.AllowGet });
        }
    }
}