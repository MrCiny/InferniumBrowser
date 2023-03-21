using CefSharp;
using Syroot.Windows.IO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows.Forms;

public class CustomMenuHandler : IContextMenuHandler
{
    string filter = null;
    bool result = false;

    SaveFileDialog saveFile;
    public void OnBeforeContextMenu(IWebBrowser browserControl, IBrowser browser, IFrame frame, IContextMenuParams parameters, IMenuModel model)
    {
        if (model.Count > 0)
        {
            model.AddSeparator();
        }

        model.AddItem((CefMenuCommand)26504, "Bookmark this WebPage");

        if(parameters.MediaType == ContextMenuMediaType.Image)
        {
            model.AddSeparator();

            model.AddItem((CefMenuCommand)26506, "Copy Image");
            model.AddItem((CefMenuCommand)26504, "Save image");
            model.AddItem((CefMenuCommand)26505, "Save image as");
        }
    }

    public bool OnContextMenuCommand(IWebBrowser browserControl, IBrowser browser, IFrame frame, IContextMenuParams parameters, CefMenuCommand commandId, CefEventFlags eventFlags)
    {
        if (commandId == (CefMenuCommand)26506) //Kopēt bildi funkcija
        {
            Clipboard.SetText(parameters.SourceUrl);

            string subPath = @"C:\temp";

            System.IO.Directory.CreateDirectory(subPath);

            CopyImage(parameters.SourceUrl);
        }

        if(commandId == (CefMenuCommand)26504) //Grāmatzīmes funkcija
        {
            
        }

        if (commandId == (CefMenuCommand)26504) //Saglabāt funkcija
        {
            string downloadFolder = new KnownFolder(KnownFolderType.Downloads).Path;
            var fileName = Path.GetFileName(parameters.LinkUrl);

            Download(parameters.LinkUrl, downloadFolder, fileName);
        }

        if (commandId == (CefMenuCommand)26505) //Saglabāt kā funkcija
        {
            SaveFileAs(parameters);

            if (result)
            {
                string filePath = Path.GetDirectoryName(saveFile.FileName);
                var fileName = Path.GetFileName(saveFile.FileName);

                Download(parameters.LinkUrl, filePath, fileName);
            }
        }

        return false;
    }

    public void OnContextMenuDismissed(IWebBrowser browserControl, IBrowser browser, IFrame frame)
    {

    }

    public bool RunContextMenu(IWebBrowser browserControl, IBrowser browser, IFrame frame, IContextMenuParams parameters, IMenuModel model, IRunContextMenuCallback callback)
    {
        return false;
    }


    public void Download(string url, string filePath, string fileName)
    {
        if(result != true)
        {
            return;
        }
        filePath += @"\";
        Console.WriteLine("Downloads folder path: " + filePath);
        string fileAndPath = filePath + fileName;
        using (WebClient client = new WebClient())
        {
            try
            {
                Uri uri = new Uri(url);
                client.DownloadFileCompleted += new AsyncCompletedEventHandler(Extract);
                client.DownloadFileAsync(uri, fileAndPath);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }

    public void Extract(object sender, AsyncCompletedEventArgs e)
    {
        Console.WriteLine("File has been downloaded.");
    }

    public void CopyImage(string imageUrl)
    {
        System.Net.WebClient client = new WebClient();
        System.IO.Stream stream = client.OpenRead(imageUrl);
        Bitmap bitmap = new Bitmap(stream);

        if (bitmap != null)
        {
            Clipboard.SetImage(bitmap);
        }

        string filePath = @"C:\temp";
        bitmap.Dispose();
        foreach (string file in Directory.GetFiles(filePath))
        {
            FileInfo fi = new FileInfo(file);
            if (fi.Name == "temp.bmp")
            {
                fi.Delete();
            }
        }
        stream.Flush();
        stream.Close();
        client.Dispose();
    }

    private void SaveFileAs(IContextMenuParams parameters)
    {
        CheckTheFilter(parameters);
        saveFile = new SaveFileDialog();
        saveFile.Title = "Save an image file";
        saveFile.Filter = filter;
        saveFile.FileName = Path.GetFileNameWithoutExtension(parameters.LinkUrl);

        DialogResult dialogResult = saveFile.ShowDialog();
        if (dialogResult == DialogResult.OK)
        {
            result = true;
        }
    }

    private void CheckTheFilter(IContextMenuParams parameters) //Metode, kas pārbauda faila formātu (Pagaidām strādā tikai uz bildēm)
    {
        var easyVar = Path.GetExtension(parameters.LinkUrl);

        var filterDictionary = new Dictionary<string, string>(){
            {".png", "PNG Image"},
            {".bmp", "Bitmap Image"},
            {".gif", "Gif Image"},
            {".jpeg", "JPEG Image"},
            {".jpg", "JPG Image"},
            {".tiff", "Tiff Image"},
            {".wmf", "Wmf Image"}
        };

        for (int i = 0; i < filterDictionary.Count; i++)
        {
            if (easyVar == filterDictionary.ElementAt(i).Key)
            {
                string key = filterDictionary.ElementAt(i).Key;
                string value = filterDictionary.ElementAt(i).Value;

                filter = $"{value} |{key}";
                return;
            }
            else
            {
                if(easyVar != "")
                {
                    filter = $"File {easyVar} |{easyVar}";
                }
                else
                {
                    filter = "All files |*.*";
                }
            }
        }
    }
}

