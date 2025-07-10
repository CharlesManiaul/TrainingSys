using FluentFTP;
using Microsoft.AspNetCore.Mvc;
using System.Net;
namespace TrainingSys.Controllers
{
    public class ManualsController : Controller
    {
        public IActionResult DisplayAll()
        {
            string ftpServerUrl = "172.16.0.12";
            string ftpUsername = "ftpadmin";
            string ftpPassword = "welcome";
            string filePath = "/TrainSys/Manuals/TrainingSystem.pdf";

            try
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create("ftp://" + ftpServerUrl + filePath);
                request.Method = WebRequestMethods.Ftp.DownloadFile;
                request.Credentials = new NetworkCredential(ftpUsername, ftpPassword);

                using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                using (Stream responseStream = response.GetResponseStream())
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    responseStream.CopyTo(memoryStream);
                    byte[] fileBytes = memoryStream.ToArray();
                    return File(fileBytes, "application/pdf");
                }
            }
            catch (WebException ex)
            {
                // Handle FTP errors
                // Log or return appropriate error response
                return NotFound();
            }

        }



        public IActionResult DisplayTraining()
        {
            string ftpServerUrl = "172.16.0.12";
            string ftpUsername = "ftpadmin";
            string ftpPassword = "welcome";
            string filePath = "/TrainSys/Manuals/TrainingPage.pdf";

            try
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create("ftp://" + ftpServerUrl + filePath);
                request.Method = WebRequestMethods.Ftp.DownloadFile;
                request.Credentials = new NetworkCredential(ftpUsername, ftpPassword);

                using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                using (Stream responseStream = response.GetResponseStream())
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    responseStream.CopyTo(memoryStream);
                    byte[] fileBytes = memoryStream.ToArray();
                    return File(fileBytes, "application/pdf");
                }
            }
            catch (WebException ex)
            {
                // Handle FTP errors
                // Log or return appropriate error response
                return NotFound();
            }



        }


        public IActionResult DisplayProfile()
        {
            string ftpServerUrl = "172.16.0.12";
            string ftpUsername = "ftpadmin";
            string ftpPassword = "welcome";
            string filePath = "/TrainSys/Manuals/ProfilePage.pdf";

            try
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create("ftp://" + ftpServerUrl + filePath);
                request.Method = WebRequestMethods.Ftp.DownloadFile;
                request.Credentials = new NetworkCredential(ftpUsername, ftpPassword);

                using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                using (Stream responseStream = response.GetResponseStream())
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    responseStream.CopyTo(memoryStream);
                    byte[] fileBytes = memoryStream.ToArray();
                    return File(fileBytes, "application/pdf");
                }
            }
            catch (WebException ex)
            {
                // Handle FTP errors
                // Log or return appropriate error response
                return NotFound();
            }



        }

        public IActionResult DisplayPost()
        {
            string ftpServerUrl = "172.16.0.12";
            string ftpUsername = "ftpadmin";
            string ftpPassword = "welcome";
            string filePath = "/TrainSys/Manuals/PostPage.pdf";

            try
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create("ftp://" + ftpServerUrl + filePath);
                request.Method = WebRequestMethods.Ftp.DownloadFile;
                request.Credentials = new NetworkCredential(ftpUsername, ftpPassword);

                using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                using (Stream responseStream = response.GetResponseStream())
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    responseStream.CopyTo(memoryStream);
                    byte[] fileBytes = memoryStream.ToArray();
                    return File(fileBytes, "application/pdf");
                }
            }
            catch (WebException ex)
            {
                // Handle FTP errors
                // Log or return appropriate error response
                return NotFound();
            }



        }




        public IActionResult DisplayReports()
        {
            string ftpServerUrl = "172.16.0.12";
            string ftpUsername = "ftpadmin";
            string ftpPassword = "welcome";
            string filePath = "/TrainSys/Manuals/ReportsPage.pdf";

            try
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create("ftp://" + ftpServerUrl + filePath);
                request.Method = WebRequestMethods.Ftp.DownloadFile;
                request.Credentials = new NetworkCredential(ftpUsername, ftpPassword);

                using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                using (Stream responseStream = response.GetResponseStream())
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    responseStream.CopyTo(memoryStream);
                    byte[] fileBytes = memoryStream.ToArray();
                    return File(fileBytes, "application/pdf");
                }
            }
            catch (WebException ex)
            {
                // Handle FTP errors
                // Log or return appropriate error response
                return NotFound();
            }



        }

    }
}
