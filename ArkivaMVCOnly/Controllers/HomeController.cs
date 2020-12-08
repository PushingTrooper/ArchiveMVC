/*
 * Versioni: 1.0.0
 * Data: 03/08/2020
 * Programuesi: Junis Milori
 * Pershkrimi: Controller-i kryesor i Arkives, ketu kryhet hapja e cdo faqeje si dhe cdo komunikim me databazen
 * */

using ArkivaMVCOnly.Models;
using Microsoft.Ajax.Utilities;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Web.Mvc;

namespace ArkivaMVCOnly.Controllers
{
    /*
     * Data: 03/08/2020
     * Programuesi: Junis Milori
     * Klasa: HomeController
     * Arsyeja: Duhej nje controller kryesor per projektin
     * Pershkrimi: Hap views dhe permban llogjiken e biznesit per komunikimet me databazen
     * Trashegon nga: Controller
     * Metodat: Index, kthen view-n index.cshtml
     *          FileExplorer, kthen partial view fileexplorer.cshtml
     *          FileExplorer(folderName, folderId), ndryshon folderin ne fileexplorer.cshtml
     *          FileExplorer(root), ndryshon folderin e fileexplorer.cshtml ne folderin root
     *          search(), ndryshon folderin e fileexplorer.cshtml ne folderin search
     *          AddPopup(), kthen view-n addpopup.cshtml
     *          saveFiles(jsonDocs), kthen nje Json me tekst dhe sukses nese jane ruajtur ose jo files ne databaze
     *          FileViewer(id), kthen view te FileExplorer pasi vendos ID te duhur
     *          DownloadDocument(id), kthen nje json me te dhenat e dokumentit qe duhet shkarkuar
     *          FileViewer(), kthen view-n fileviewer.cshtml
     *          editLocation(), kthen nje Json me tekst dhe sukses nese eshte ruajtur ose jo vendodhja fizike ne databaze
     *          addIndex(), kthen nje Json me tekst dhe sukses nese eshte ruajtur ose jo indeksi i ri ne databaze
     *          deleteFolder(), kthen partial view FileExplorer.cshtml nese eshte fshire dokumenti
     *          Login(), kthen view te login.cshtml
     *          Login(user), kthen view index.cshtml
     *          Logout(), kthen view te login.cshtml
     *          Register(), kthen view te register.cshtml
     *          Register(user), kthen view te register.cstml
     * */

    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            if (Session["UserName"] != null)
            {
                Session["folderType"] = Folder.Root;
                return View();
            }
            else
            {
                return RedirectToAction("Login");
            }
        }

        public ActionResult FileExplorer()
        {
            return View();
        }

        [HttpPost]
        [ActionName("changeFolder")]
        public ActionResult FileExplorer(String folderName, String folderId)
        {
            List<String> path;
            List<String> pathIds;
            if (Session["folderPath"] != null && Session["pathIds"] != null)
            {
                path = Session["folderPath"] as List<String>;
                pathIds = Session["pathIds"] as List<string>;
            }
            else
            {
                path = new List<string>();
                pathIds = new List<string>();
            }

            //Kerkon id te path(jo emer pasi mund te jete i njejte) ne listen e pathIds dhe nese e gjen atehere e heq pasi
            //po kthehemi mbrapa, nese nuk e gjen atehere e shton
            int pathIndex = pathIds.FindIndex(x => x == folderId);
            if (pathIndex != -1)
            {
                path.RemoveRange(pathIndex + 1, path.Count() - (pathIndex + 1));
                pathIds.RemoveRange(pathIndex + 1, path.Count() - (pathIndex + 1));
            }
            else
            {
                path.Add(folderName);
                pathIds.Add(folderId);
            }
            //Ndryshon tipin e folderit qe me pas do te perdeoret ne razor code te FileExplorer.cshtml
            switch (path.Count)
            {
                case 0:
                    Session["folderType"] = Folder.Root;
                    break;

                case 1:
                    Session["folderType"] = Folder.Subjekt;
                    break;

                case 2:
                    Session["folderType"] = Folder.Inspektim;
                    break;

                case 3:
                    Session["folderType"] = Folder.Lloji;
                    break;
            }
            Session["folderPath"] = path;
            Session["pathIds"] = pathIds;
            Session["folderName"] = folderName;
            return PartialView("FileExplorer");
        }

        [HttpPost]
        [ActionName("goToRoot")]
        public ActionResult FileExplorer(Boolean root)
        {
            Session["folderType"] = Folder.Root;
            Session["folderPath"] = null;
            Session["pathIds"] = null;
            Session["folderName"] = null;
            return PartialView("FileExplorer");
        }

        [HttpPost]
        [ActionName("search")]
        public ActionResult search(string emSub, string nrIns,
            string lloji, string emDok, string fshInd,
            string zyra, string nrKut, string rafti,
            string dateStart, string dateEnd)
        {
            Session["folderType"] = Folder.Search;
            Session["folderPath"] = null;
            Session["pathIds"] = null;
            Session["folderName"] = null;

            Session["emSub"] = emSub;
            Session["nrIns"] = nrIns;
            Session["lloji"] = lloji;
            Session["emDok"] = emDok;
            Session["fshInd"] = fshInd;
            Session["zyra"] = zyra;
            Session["nrKut"] = nrKut;
            Session["rafti"] = rafti;
            Session["dateStart"] = dateStart;
            Session["dateEnd"] = dateEnd;

            return PartialView("FileExplorer");
        }

        public ActionResult AddPopup()
        {
            return View();
        }

        [HttpPost]
        [ActionName("saveFiles")]
        public ActionResult saveFiles([Microsoft.AspNetCore.Mvc.FromBody] List<string> jsonDocs)
        {
            List<TJ_Dokument> docs = new List<TJ_Dokument>();
            List<string> path = Session["folderPath"] as List<string>;
            //konverton dokumentat nga json ne Tj_Dokumenta
            for (int i = 0; i < jsonDocs.Count; i++)
            {
                var format = "d/M/yyyy"; // your datetime format
                dynamic data = JObject.Parse(jsonDocs[i]);
                string emriDok = data.emri_dokumentit;
                string emriSub = path[0];
                string nrInspekt = path[1];
                string lloji = data.lloji;
                string fushaIndeks = data.fusha_indeksimit;
                string zyra = data.vend_zyra;
                string nrKutise = data.vend_nr_kutise;
                string rafti = data.vend_rafti;
                String dateValue = data.date_regjistrimi;
                DateTime date = DateTime.ParseExact(dateValue, format, new CultureInfo("sq-AL"));
                DateTime albanianFormatedDate;
                DateTime.TryParseExact(date.ToString("dd/M/yyyy"), "dd/M/yyyy", new CultureInfo("sq-AL"), DateTimeStyles.None, out albanianFormatedDate);
                string dok = data.dokumenti;
                dok = dok.Remove(0, 0);
                dok = dok.Remove(dok.Length - 1, 0);
                byte[] dokumenti = Convert.FromBase64String(dok);
                string CreatedBy = Session["UserName"] as string;

                docs.Add(new TJ_Dokument(emriDok, emriSub, nrInspekt, lloji, fushaIndeks, zyra, nrKutise, rafti, albanianFormatedDate, dokumenti, CreatedBy));
            }
            using (User db = new User())
            {
                try
                {
                    db.TJ_Dokument.AddRange(docs);
                    db.SaveChanges();
                    Session["folderType"] = Folder.Root;
                    Session["folderPath"] = null;
                    Session["pathIds"] = null;
                    Session["folderName"] = null;
                    Session["docId"] = null;
                    return Json(new { success = true, responseText = "U ruajt me sukses" }, JsonRequestBehavior.AllowGet);
                }
                catch (DbUpdateException)
                {
                    //Kthen nje json me data success = false nese te dhenat nuk jane ruajtur ne databaze prej arsyeve te pergjithshme si 
                    //moskomunikimi me serverin ose mos pasja e aksesit te duhur
                    return Json(new { success = false, responseText = "Dokumentet nuk u ruajten ne databaze" }, JsonRequestBehavior.AllowGet);
                }
                catch (DbEntityValidationException e)
                {
                    foreach (var eve in e.EntityValidationErrors)
                    {
                        Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                            eve.Entry.Entity.GetType().Name, eve.Entry.State);
                        foreach (var ve in eve.ValidationErrors)
                        {
                            Console.WriteLine("- Property: \"{0}\", Value: \"{1}\", Error: \"{2}\"",
                                ve.PropertyName,
                                eve.Entry.CurrentValues.GetValue<object>(ve.PropertyName),
                                ve.ErrorMessage);
                        }
                    }
                    //Kthen nje json me data success = false nese te tipet e te dhenave nuk perkojne me tipet e kolonave te databaze
                    return Json(new { success = false, responseText = "Dokumentet nuk u ruajten ne databaze" }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        [HttpPost]
        [ActionName("openDocument")]
        public ActionResult FileViewer([Microsoft.AspNetCore.Mvc.FromBody] string id)
        {
            Session["docId"] = id;
            return PartialView("FileViewer", id);
        }

        [HttpPost]
        [ActionName("downloadDocument")]
        public ActionResult DownloadDocument([Microsoft.AspNetCore.Mvc.FromBody] string id)
        {
            using (User db = new User())
            {
                int docId = int.Parse(id);

                var data = (from doc in db.TJ_Dokument
                            where doc.IDTJ_Dokument == docId
                            group doc by new { doc.dokumenti, doc.emri_dokumentit } into tempDocs
                            select new { tempDocs.Key }).ToList();

                string emriDoc = data[0].Key.emri_dokumentit;
                int indexOfPoint = emriDoc.LastIndexOf('.') + 1;
                string type = emriDoc.Substring(indexOfPoint, emriDoc.Length - indexOfPoint);
                string typeOfDoc = "";
                switch (type)
                {
                    case "pdf":
                        typeOfDoc = "application/pdf";
                        break;

                    case "jpg":
                        typeOfDoc = "image/jpeg";
                        break;

                    case "png":
                        typeOfDoc = "image/png";
                        break;

                    case "docx":
                        typeOfDoc = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                        break;

                    case "xlsx":
                        typeOfDoc = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                        break;

                    case "pptx":
                        typeOfDoc = "application/vnd.openxmlformats-officedocument.presentationml.presentation";
                        break;

                    case "doc":
                        typeOfDoc = "application/msword";
                        break;

                    case "xls":
                        typeOfDoc = "application/vnd.ms-excel";
                        break;

                    case "ppt":
                        typeOfDoc = "application/vnd.ms-powerpoint";
                        break;

                    case "odt":
                        typeOfDoc = "application/vnd.oasis.opendocument.text";
                        break;
                }
                return Json(new { success = true, fileData = data[0].Key.dokumenti, fileName = data[0].Key.emri_dokumentit, fileType = typeOfDoc }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        [ActionName("downloadDocuments")]
        public ActionResult DownloadDocuments([Microsoft.AspNetCore.Mvc.FromBody] List<string> ids)
        {
            using (User db = new User())
            {
                List<int> docIds = new List<int>();
                foreach (string tempid in ids)
                {
                    docIds.Add(int.Parse(tempid));
                }

                var docList = (from doc in db.TJ_Dokument
                               where docIds.Contains(doc.IDTJ_Dokument)
                               group doc by new { doc.dokumenti, doc.emri_dokumentit } into tempDocs
                               select new { tempDocs.Key }).ToList();
                using (FileStream zipFile = System.IO.File.Create("./docs.zip"))
                {
                    using (ZipArchive archive = new ZipArchive(zipFile, ZipArchiveMode.Update))
                    {
                        foreach (var doc in docList)
                        {
                            string emriDoc = doc.Key.emri_dokumentit;
                            int indexOfPoint = emriDoc.LastIndexOf('.') + 1;
                            string type = emriDoc.Substring(indexOfPoint, emriDoc.Length - indexOfPoint);
                            string typeOfDoc = "";
                            switch (type)
                            {
                                case "pdf":
                                    typeOfDoc = "application/pdf";
                                    break;

                                case "jpg":
                                    typeOfDoc = "image/jpeg";
                                    break;

                                case "png":
                                    typeOfDoc = "image/png";
                                    break;

                                case "docx":
                                    typeOfDoc = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                                    break;

                                case "xlsx":
                                    typeOfDoc = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                                    break;

                                case "pptx":
                                    typeOfDoc = "application/vnd.openxmlformats-officedocument.presentationml.presentation";
                                    break;

                                case "doc":
                                    typeOfDoc = "application/msword";
                                    break;

                                case "xls":
                                    typeOfDoc = "application/vnd.ms-excel";
                                    break;

                                case "ppt":
                                    typeOfDoc = "application/vnd.ms-powerpoint";
                                    break;

                                case "odt":
                                    typeOfDoc = "application/vnd.oasis.opendocument.text";
                                    break;
                            }


                            //System.IO.File.WriteAllBytes("C:/Users/Admin/source/repos/arkivadesignmvc/" + doc.Key.emri_dokumentit, doc.Key.dokumenti);

                            //archive.CreateEntryFromFile("C:/Users/Admin/source/repos/arkivadesignmvc/" + doc.Key.emri_dokumentit, doc.Key.emri_dokumentit);
                            var zipArchiveEntry = archive.CreateEntry(doc.Key.emri_dokumentit, CompressionLevel.Fastest);
                            using (var zipStream = zipArchiveEntry.Open())
                                zipStream.Write(doc.Key.dokumenti, 0, doc.Key.dokumenti.Length);
                        }

                    }
                    byte[] test = System.IO.File.ReadAllBytes("./docs.zip");
                    return Json(new { success = true, fileData = test, fileName = "docs.zip", fileType = "application/zip" }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        [HttpPost]
        [ActionName("downloadDocumentsFromFolder")]
        public ActionResult DownloadDocumentsFromFolder([Microsoft.AspNetCore.Mvc.FromBody] List<string> folders)
        {
            using (User db = new User())
            {
                List<String> path;
                if (Session["folderPath"] != null)
                {
                    path = Session["folderPath"] as List<String>;
                }
                else
                {
                    path = new List<string>();
                }

                string emSub = null;
                string nrIns = null;
                string lloji = null;

                if (path.Count() == 1)
                {
                    emSub = path[0];
                }
                else if (path.Count() == 2)
                {
                    emSub = path[0];
                    nrIns = path[1];
                }

                var docList = new[] { new { Key = new { dokumenti = (byte[])null, emri_dokumentit = (string)null } } }.ToList();

                if (emSub.IsNullOrWhiteSpace())
                {
                    docList = (from doc in db.TJ_Dokument
                               where (folders.Contains(doc.emri_subjektit))
                               group doc by new { doc.dokumenti, doc.emri_dokumentit } into tempDocs
                               select new { tempDocs.Key }).ToList();
                }
                else if (nrIns.IsNullOrWhiteSpace())
                {
                    docList = (from doc in db.TJ_Dokument
                               where (doc.emri_subjektit == emSub
                                        && folders.Contains(doc.nr_inspektimi))
                               group doc by new { doc.dokumenti, doc.emri_dokumentit } into tempDocs
                               select new { tempDocs.Key }).ToList();
                }
                else
                {
                    docList = (from doc in db.TJ_Dokument
                               where (doc.emri_subjektit == emSub
                                        && doc.nr_inspektimi == nrIns
                                        && (folders.Contains(doc.lloji)))
                               group doc by new { doc.dokumenti, doc.emri_dokumentit } into tempDocs
                               select new { tempDocs.Key }).ToList();
                }
                /*
                 * docList = (from doc in db.TJ_Dokument
                               where ((emSub == "" || doc.emri_subjektit == emSub)
                                        && (nrIns == "" || doc.nr_inspektimi == nrIns)
                                        && (lloji == "" || doc.lloji == lloji))
                               group doc by new { doc.dokumenti, doc.emri_dokumentit } into tempDocs
                               select new { tempDocs.Key }).ToList();
                 * */

                using (FileStream zipFile = System.IO.File.Create("./docs.zip"))
                {
                    using (ZipArchive archive = new ZipArchive(zipFile, ZipArchiveMode.Update))
                    {
                        foreach (var doc in docList)
                        {
                            string emriDoc = doc.Key.emri_dokumentit;
                            int indexOfPoint = emriDoc.LastIndexOf('.') + 1;
                            string type = emriDoc.Substring(indexOfPoint, emriDoc.Length - indexOfPoint);
                            string typeOfDoc = "";
                            switch (type)
                            {
                                case "pdf":
                                    typeOfDoc = "application/pdf";
                                    break;

                                case "jpg":
                                    typeOfDoc = "image/jpeg";
                                    break;

                                case "png":
                                    typeOfDoc = "image/png";
                                    break;

                                case "docx":
                                    typeOfDoc = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                                    break;

                                case "xlsx":
                                    typeOfDoc = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                                    break;

                                case "pptx":
                                    typeOfDoc = "application/vnd.openxmlformats-officedocument.presentationml.presentation";
                                    break;

                                case "doc":
                                    typeOfDoc = "application/msword";
                                    break;

                                case "xls":
                                    typeOfDoc = "application/vnd.ms-excel";
                                    break;

                                case "ppt":
                                    typeOfDoc = "application/vnd.ms-powerpoint";
                                    break;

                                case "odt":
                                    typeOfDoc = "application/vnd.oasis.opendocument.text";
                                    break;
                            }


                            //System.IO.File.WriteAllBytes("C:/Users/Admin/source/repos/arkivadesignmvc/" + doc.Key.emri_dokumentit, doc.Key.dokumenti);

                            //archive.CreateEntryFromFile("C:/Users/Admin/source/repos/arkivadesignmvc/" + doc.Key.emri_dokumentit, doc.Key.emri_dokumentit);
                            var zipArchiveEntry = archive.CreateEntry(doc.Key.emri_dokumentit, CompressionLevel.Fastest);
                            using (var zipStream = zipArchiveEntry.Open())
                                zipStream.Write(doc.Key.dokumenti, 0, doc.Key.dokumenti.Length);
                        }

                    }
                    byte[] test = System.IO.File.ReadAllBytes("./docs.zip");
                    return Json(new { success = true, fileData = test, fileName = "docs.zip", fileType = "application/zip" }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        protected override JsonResult Json(object data, string contentType, System.Text.Encoding contentEncoding, JsonRequestBehavior behavior)
        {
            return new JsonResult()
            {
                Data = data,
                ContentType = contentType,
                ContentEncoding = contentEncoding,
                JsonRequestBehavior = behavior,
                MaxJsonLength = Int32.MaxValue
            };
        }

        public ActionResult FileViewer()
        {
            return View();
        }

        [HttpPost]
        [ActionName("editLocation")]
        public ActionResult editLocation(string zyra, string nrKut, string rafti)
        {
            using (User db = new User())
            {
                int docId = int.Parse(Session["docId"] as string);
                (from doc in db.TJ_Dokument
                 where doc.IDTJ_Dokument == docId
                 select doc).ToList()
                 .ForEach(x => { x.vend_zyra = zyra; x.vend_nr_kutise = nrKut; x.vend_rafti = rafti; });

                try
                {
                    db.SaveChanges();

                    Console.WriteLine(zyra);
                    return Json(new { success = true, responseText = "Vendodhja u perditesua me sukses" }, JsonRequestBehavior.AllowGet);
                }
                catch (DbUpdateException)
                {
                    return Json(new { success = false, responseText = "Dokumentet nuk u ruajten ne databaze" }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        [HttpPost]
        [ActionName("addIndex")]
        public ActionResult addIndex(string fushaIdx)
        {
            using (User db = new User())
            {
                int docId = int.Parse(Session["docId"] as string);
                (from doc in db.TJ_Dokument
                 where doc.IDTJ_Dokument == docId
                 select doc).ToList()
                 .ForEach(x => x.fusha_indeksimit += (fushaIdx + ";"));
                try
                {
                    db.SaveChanges();
                    return Json(new { success = true, responseText = "Fusha e indeksimit u shtua me sukses", data = fushaIdx }, JsonRequestBehavior.AllowGet);
                }
                catch (DbUpdateException)
                {
                    return Json(new { success = false, responseText = "Dokumentet nuk u ruajten ne databaze" }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        [HttpPost]
        [ActionName("deleteFolder")]
        public ActionResult deleteFolder(List<string> folderNames)
        {
            if (folderNames != null)
            {
                using (User db = new User())
                {
                    int numOfDeleted = 0;
                    foreach (string folder in folderNames)
                    {
                        string createdBy = Session["UserName"] as string;
                        var deletedList = db.TJ_Dokument.RemoveRange(db.TJ_Dokument.Where(x => x.emri_subjektit == folder && x.CreatedBy.Equals(createdBy)));
                        numOfDeleted += deletedList.Count();
                    }
                    if (numOfDeleted == 0)
                        return new HttpStatusCodeResult(401, "Nuk keni akses fshirje ne keto dokumenta"); // Unauthorized
                    else
                        db.SaveChanges();
                }
            }
            return PartialView("FileExplorer");
        }

        [HttpPost]
        [ActionName("deleteFile")]
        public ActionResult deleteFile(List<string> fileIds)
        {
            if (fileIds != null)
            {
                using (User db = new User())
                {
                    int numOfDeleted = 0;
                    foreach (string id in fileIds)
                    {
                        int parsedId = int.Parse(id);
                        string createdBy = Session["UserName"] as string;
                        var deletedList = db.TJ_Dokument.RemoveRange(db.TJ_Dokument.Where(x => x.IDTJ_Dokument == parsedId && x.CreatedBy.Equals(createdBy)));
                        numOfDeleted += deletedList.Count();
                    }
                    if (numOfDeleted == 0)
                        return new HttpStatusCodeResult(401, "Nuk keni akses fshirje ne keto dokumenta"); // Unauthorized
                    else
                        db.SaveChanges();
                }
            }
            return PartialView("FileExplorer");
        }

        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(TJ_Users objUser)
        {
            if (ModelState.IsValid)
            {
                using (User db = new User())
                {
                    var obj = db.TJ_Users.Where(a => a.username.Equals(objUser.username) && a.password.Equals(objUser.password)).FirstOrDefault();
                    if (obj != null)
                    {
                        Session["UserName"] = obj.username.ToString();
                        Session["DbError"] = null;
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        Session["DbError"] = "Username ose password-i janë gabim, provoni përsëri";
                    }
                }
            }
            return View(objUser);
        }

        public ActionResult Logout()
        {
            Session["UserName"] = null;
            return RedirectToAction("Login");
        }

        public ActionResult Register()
        {
            Session["DbError"] = null;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(TJ_Users objUser)
        {
            if (ModelState.IsValid)
            {
                using (User db = new User())
                {
                    db.TJ_Users.Add(objUser);
                    try
                    {
                        db.SaveChanges();
                        Session["DbError"] = null;
                        return RedirectToAction("Login");
                    }
                    catch (DbUpdateException)
                    {
                        Session["DbError"] = "Ky përdorues ekziston";
                        return View(objUser);
                    }
                }
            }
            return View(objUser);
        }
    }
}