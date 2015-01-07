using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml.Controls;
using HTMLParserLibrary;
using Windows.Storage;

namespace ParserApp
{
    public class Content
    {
        public async void LoadContent(StackPanel stack)
        {
            StorageFolder folder = Windows.Storage.ApplicationData.Current.LocalFolder;
            StorageFile file = await Windows.Storage.StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/file.html"));
            string text = await Windows.Storage.FileIO.ReadTextAsync(file);
            
            HTMLParser htmlParser = new HTMLParser(true);
            text = htmlParser.replaceCharacters(text);
            htmlParser.parseContent(stack, text);
        }
    }
}
