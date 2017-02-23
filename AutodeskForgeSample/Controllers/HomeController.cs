using Autodesk.Forge;
using Autodesk.Forge.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Configuration;
using System.Web.Http;
using System.Web.Mvc;

namespace AutodeskForgeSample.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        [System.Web.Mvc.HttpPost]
        public ActionResult Upload(HttpPostedFileBase file)
        {
            if (file == null || file.ContentLength == 0) return RedirectToAction("Index");

            string fileName = Path.GetFileName(file.FileName);
            string fileSavePath = Path.Combine(Server.MapPath("~/App_Data/"), fileName);
            file.SaveAs(fileSavePath);

            // get a write enabled token
            TwoLeggedApi oauthApi = new TwoLeggedApi();
            dynamic bearer = oauthApi.Authenticate(
                WebConfigurationManager.AppSettings["FORGE_CLIENT_ID"],
                WebConfigurationManager.AppSettings["FORGE_CLIENT_SECRET"],
                "client_credentials",
                new Scope[] { Scope.BucketCreate, Scope.DataCreate, Scope.DataWrite, Scope.DataRead });

            // create a randomg bucket name (fixed prefix + randomg guid)
            string bucketKey = "forgeapp" + Guid.NewGuid().ToString("N").ToLower();

            // create the Forge bucket
            PostBucketsPayload postBucket = new PostBucketsPayload(bucketKey, null, PostBucketsPayload.PolicyKeyEnum.Transient /* erase after 24h*/ );
            BucketsApi bucketsApi = new BucketsApi();
            bucketsApi.Configuration.AccessToken = bearer.access_token;
            dynamic newBucket = bucketsApi.CreateBucket(postBucket);

            // upload file (a.k.a. Objects)
            ObjectsApi objectsApi = new ObjectsApi();
            oauthApi.Configuration.AccessToken = bearer.access_token;
            dynamic newObject;
            using (StreamReader fileStream = new StreamReader(fileSavePath))
            {
                newObject = objectsApi.UploadObject(bucketKey, fileName,
                    (int)fileStream.BaseStream.Length, fileStream.BaseStream,
                    "application/octet-stream");
            }

            // translate file
            string objectIdBase64 = ToBase64(newObject.objectId);
            List<JobPayloadItem> postTranslationOutput = new List<JobPayloadItem>()
            {
                new JobPayloadItem(
                    JobPayloadItem.TypeEnum.Svf /* Viewer*/,
                    new List<JobPayloadItem.ViewsEnum>()
                    {
                        JobPayloadItem.ViewsEnum._3d,
                        JobPayloadItem.ViewsEnum._2d
                    })
            };

            JobPayload postTranslation = new JobPayload(
                new JobPayloadInput(objectIdBase64),
                new JobPayloadOutput(postTranslationOutput));
            DerivativesApi derivativeApi = new DerivativesApi();
            derivativeApi.Configuration.AccessToken = bearer.access_token;
            dynamic translation = derivativeApi.Translate(postTranslation);

            // check if is ready
            int progress = 0;
            do
            {
                System.Threading.Thread.Sleep(1000); // wait 1 second
                try
                {
                    dynamic manifest = derivativeApi.GetManifest(objectIdBase64);
                    progress = (string.IsNullOrWhiteSpace(Regex.Match(manifest.progress, @"\d+").Value) ? 100 : Int32.Parse(Regex.Match(manifest.progress, @"\d+").Value));
                }
                catch (Exception ex)
                {

                }
            } while (progress < 100);

            // clean up
            System.IO.File.Delete(fileSavePath);
            //Directory.Delete(fileSavePath, true);
            
            return RedirectToAction("DisplayModel", "Home", new { characterName = objectIdBase64 });
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        //ForgeViewer
        public ActionResult ForgeViewer()
        {
            ViewBag.Message = "This is the Autodesk Forge Viewer.";
            string test = "dXJuOmFkc2sub2JqZWN0czpvcy5vYmplY3Q6Zm9yZ2VhcHAyMTM0MmI5Y2Y0YTc0Y2IwYjE3OTUzYjYyZmNmMzVkNi9TYW1wbGUxLmlmYw==";
            return View((object)test);
        }

        public ActionResult DisplayModel(string characterName)
        {
            
            return View("ForgeViewer", (object)characterName);
        }



        /// <summary>
        /// Convert a string into Base64 (source http://stackoverflow.com/a/11743162)
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public string ToBase64(string input)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(input);
            return System.Convert.ToBase64String(plainTextBytes);
        }
    }
}