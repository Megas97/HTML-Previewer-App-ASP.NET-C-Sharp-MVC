using System;
using System.Linq;
using System.Web.Mvc;
using System.IO;
using HTMLPreviewerApp.Models;

namespace HTMLPreviewerApp.Controllers
{
    public class HomeController : Controller
    {
        #region // Index HTTPGET
        [HttpGet]
        public ActionResult Index(int id = 0)
        {
            if (id == 0)
            {
                ViewBag.HTMLCodeInput = TempData["HTMLCodeInput"];
                ViewBag.HTMLCodePreview = TempData["HTMLCodePreview"];
                ViewBag.Message = TempData["Message"];
            }
            else
            {
                using (DatabaseEntities db = new DatabaseEntities())
                {
                    HTMLCode code = db.HTMLCode.Where(a => a.id == id).FirstOrDefault();
                    if (code != null)
                    {
                        ViewBag.HTMLCodeID = code.id;
                        ViewBag.HTMLCodeInput = TempData["HTMLCodeInput"] == null ? Base64Decode(code.html) : TempData["HTMLCodeInput"];
                        ViewBag.HTMLCodePreview = TempData["HTMLCodePreview"] == null ? Base64Decode(code.html) : TempData["HTMLCodePreview"];
                        ViewBag.CreatedOn = code.created;
                        ViewBag.LastModified = code.edited;
                        ViewBag.EditMessage = "Showing result for id " + id;
                    }
                    else
                    {
                        ViewBag.EditMessage = "No result found for id " + id;
                    }
                    ViewBag.Message = TempData["Message"];
                }
            }
            return View();
        }
        #endregion

        #region // Index HTTPPOST
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult ProcessFormSubmit(string previewButton, string checkButton, string saveButton, FormCollection formCollection)
        {
            int HTMLCodeID = 0;
            if (formCollection["HTMLCodeInput"] == "")
            {
                TempData["Message"] = "Please write some HTML code first";
                HTMLCodeID = formCollection["HTMLCodeID"] == "" ? 0 : Convert.ToInt32(formCollection["HTMLCodeID"]);
                TempData["HTMLCodeInput"] = formCollection["HTMLCodeInput"];
                TempData["HTMLCodePreview"] = formCollection["HTMLCodeInput"];
            }
            else
            {
                if (!string.IsNullOrEmpty(previewButton))
                {
                    HTMLCodeID = formCollection["HTMLCodeID"] == "" ? 0 : Convert.ToInt32(formCollection["HTMLCodeID"]);
                    TempData["HTMLCodeInput"] = formCollection["HTMLCodeInput"];
                    TempData["HTMLCodePreview"] = formCollection["HTMLCodeInput"];
                }
                else if (!string.IsNullOrEmpty(checkButton))
                {
                    using (DatabaseEntities db = new DatabaseEntities())
                    {
                        string HTMLInput = Base64Encode(formCollection["HTMLCodeInput"]);
                        HTMLCode code = db.HTMLCode.Where(a => a.html.Equals(HTMLInput)).FirstOrDefault();
                        TempData["Message"] = code != null ? "An identical HTML code already exists in the database" : "Your HTML code is unique";
                    }
                    HTMLCodeID = formCollection["HTMLCodeID"] == "" ? 0 : Convert.ToInt32(formCollection["HTMLCodeID"]);
                    TempData["HTMLCodeInput"] = formCollection["HTMLCodeInput"];
                    TempData["HTMLCodePreview"] = formCollection["HTMLCodeInput"];
                }
                else if (!string.IsNullOrEmpty(saveButton))
                {
                    var tempFilePath = Path.GetTempFileName();
                    using (StreamWriter outputFile = new StreamWriter(tempFilePath))
                    {
                        outputFile.WriteLine(formCollection["HTMLCodeInput"]);
                    }
                    int tempFileSizeLimit = 5242880; // 5 MB
                    long tempFileSize = 0;
                    using (StreamReader inputFile = new StreamReader(tempFilePath))
                    {
                        tempFileSize = inputFile.BaseStream.Length;
                    }
                    var file = new FileInfo(tempFilePath);
                    file.Delete();
                    if (tempFileSize < tempFileSizeLimit)
                    {
                        using (DatabaseEntities db = new DatabaseEntities())
                        {
                            int CodeID = formCollection["HTMLCodeID"] == "" ? 0 : Convert.ToInt32(formCollection["HTMLCodeID"]);
                            HTMLCode existingHTMLCode = db.HTMLCode.Where(a => a.id == CodeID).FirstOrDefault();
                            if (existingHTMLCode != null)
                            {
                                if (Base64Encode(formCollection["HTMLCodeInput"]).Equals(existingHTMLCode.html))
                                {
                                    TempData["Message"] = "You did not make any changes to your HTML code";
                                }
                                else
                                {
                                    existingHTMLCode.html = Base64Encode(formCollection["HTMLCodeInput"]);
                                    existingHTMLCode.edited = DateTime.Now;
                                    db.Configuration.ValidateOnSaveEnabled = false;
                                    db.SaveChanges();
                                    TempData["Message"] = "Your HTML code was successfully updated";
                                }
                                HTMLCodeID = existingHTMLCode.id;
                            }
                            else
                            {
                                HTMLCode code = new HTMLCode();
                                code.html = Base64Encode(formCollection["HTMLCodeInput"]);
                                code.created = DateTime.Now;
                                code.edited = null;
                                db.HTMLCode.Add(code);
                                db.SaveChanges();
                                TempData["Message"] = "Your HTML code was successfully saved";
                                HTMLCodeID = code.id;
                            }
                        }
                    }
                    else
                    {
                        TempData["Message"] = "The maximum allowed size of the HTML code is 5 MB";
                        HTMLCodeID = formCollection["HTMLCodeID"] == "" ? 0 : Convert.ToInt32(formCollection["HTMLCodeID"]);
                    }
                }
            }
            return HTMLCodeID != 0 ? RedirectToAction("Index", new { id = HTMLCodeID }) : RedirectToAction("Index");
        }
        #endregion

        #region // Helper Functions
        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        public static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }
        #endregion
    }
}