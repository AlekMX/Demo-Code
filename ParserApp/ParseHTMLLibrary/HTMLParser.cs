using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;


namespace HTMLParserLibrary
{
    public class HTMLParser
    {
        public bool parseMedia;

        /// <summary>
        /// COnstructor por defecto que no procesa las imágenes y videos que estén dentro del contenido HTML
        /// </summary>
        public HTMLParser()
        {
            parseMedia = false;
        }

        /// <summary>
        /// Constructor que permite indicar si las imágenes y videos dentro del HTML serán procesados
        /// </summary>
        /// <param name="media"></param>
        public HTMLParser(bool media)
        {
            parseMedia = media;
        }

        /// <summary>
        /// Procesa la fecha dada en RFC3339 obteniendo solo la fecha
        /// </summary>
        /// <param name="text">La cadena que contiene la fecha en formato RFC3339 </param>
        /// <returns>Cadena de texto con la fecha</returns>
        public string parseDate(string text)
        {
            char[] separators = { '-' };
            string[] tokens;
            string newDate = text.Substring(0, text.IndexOf("T"));
            tokens = newDate.Split(separators);
            newDate = tokens[2] + "." + tokens[1] + "." + tokens[0];
            return newDate;
        }

        /// <summary>
        /// Procesa la fecha en formato RFC3339 y obtiene la hora de publicación
        /// </summary>
        /// <param name="text">La cadena que contiene la hora en formato RFC3339</param>
        /// <returns>Cadena de texto con la hora de la publicación</returns>
        public string parseTime(string text)
        {
            char[] separators = { '-' };
            int iInit = text.IndexOf("T");
            int iFin = text.IndexOf("-", iInit);

            string newTime = text.Substring(iInit + 1, (iFin - iInit) - 1);
            return newTime;
        }

        /// <summary>
        /// Procesa el texto HTML reemplazando caracteres especiales
        /// </summary>
        /// <param name="text">el texto con contenido HTML</param>
        /// <returns>Cadena de texto con los caracteres reemplazados</returns>
        public string replaceCharacters(string text)
        {
            text = text.Replace("&nbsp;", " ");
            text = text.Replace("&quot;", "\"");
            text = text.Replace("&amp;", "&");
            return text;
        }

        /// <summary>
        /// Busca y regresa la primera imagen encontrada dentrl del código HTML para ser utilizada como imagen del post,
        /// en caso de no encontrar ninguna imagen, regresa la URI de una imagen por default contenida en la app
        /// </summary>
        /// <param name="htmlContent">Texto con contenido HTML</param>
        /// <returns>URL de la imagen</returns>
        public string parseMainImage(string htmlContent)
        {
            bool found = false;
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(htmlContent);
            HtmlNodeCollection nodeCollection = doc.DocumentNode.ChildNodes[0].ChildNodes;
            string path = "/Assets/ApplicationIcon.png";
            foreach (HtmlNode node in nodeCollection)
            {
                if (node.Name.Equals("div"))
                {
                    if (node.HasChildNodes)
                    {
                        foreach (HtmlNode imgNode in node.Descendants("img").ToList())
                        {
                            path = imgNode.GetAttributeValue("src", "/Assets/ApplicationIcon.png");
                            found = true;
                            break;
                        }
                        foreach (HtmlNode imgNode in node.Descendants("object").ToList())
                        {
                            path = imgNode.GetAttributeValue("data-thumbnail-src", "/Assets/ApplicationIcon.png");
                            found = true;
                            break;
                        }
                        if (found) break;
                    }
                }
            }
            return path;
        }

        /// <summary>
        /// Recolecta todas las URLs que de las etiquetas <img>, por cada una crea un BitmapImage
        /// y este último lo agrega a la lista que se le pasa como parámetro
        /// </summary>
        /// <param name="htmlContent">el texto con contenido HTML</param>
        /// <param name="URLImages">La lista donde se guardarán los objetos de las imágenes</param>
        public void parseImages(string htmlContent, List<BitmapImage> URLImages)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(htmlContent);
            HtmlNodeCollection nodeCollection = doc.DocumentNode.ChildNodes[0].ChildNodes;
            string def = "/Assets/ApplicationIcon.png";
            string path = def;
            foreach (HtmlNode node in nodeCollection)
            {
                if (node.HasChildNodes)
                {
                    foreach (HtmlNode imgNode in node.Descendants("img").ToList())
                    {
                        path = imgNode.GetAttributeValue("src", def);
                        URLImages.Add(new BitmapImage(new Uri(path))
                        {
                            DecodePixelHeight = 200
                        });
                    }
                }

            }
        }

        /// <summary>
        /// Agrega todos los videos encontrados al contentStack 
        /// </summary>
        /// <param name="htmlContent">Cadena con contenido HTML</param>
        /// <param name="contentStack">StackPanel donde se agregarán los videos</param>
        public void parseVideos(string htmlContent, StackPanel contentStack)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(htmlContent);
            HtmlNodeCollection nodeCollection = doc.DocumentNode.ChildNodes[0].ChildNodes;
            string def = "/AssetsApplicationIcon.png";
            string path = def;
            foreach (HtmlNode node in nodeCollection)
            {
                if (node.HasChildNodes)
                {
                    foreach (HtmlNode videoNode in node.Descendants("object").ToList())
                    {
                        parseVideoObjectNode(videoNode, contentStack);
                    }
                }
            }
        }

        /// <summary>
        /// Busca dentro del HTMLNode las porpiedades necesarias para general un control Button que permita lanzar el navegador para visualizar el video
        /// NOTA: Solo soporta videos de youtube.
        /// </summary>
        /// <param name="videoNode">El nodo a revisar</param>
        /// <param name="stack">El StackPanel donde se agregará el botón</param>
        private void parseVideoObjectNode(HtmlNode videoNode, StackPanel stack)
        {
            Grid grid = new Grid();
            string path = videoNode.GetAttributeValue("data-thumbnail-src", "ms-appx:///Assets/SmallLogo.scale.png");
            BitmapImage biTmp = new BitmapImage(new Uri(path, UriKind.RelativeOrAbsolute));
            Image img = new Image();
            img.Stretch = Stretch.Uniform;
            img.Source = biTmp;
            grid.Children.Add(img);
            biTmp = new BitmapImage(new Uri("ms-appx:///Assets/appbar.youtube.play.png", UriKind.RelativeOrAbsolute));
            img = new Image();
            img.Stretch = Stretch.None;
            img.Source = biTmp;
            grid.Children.Add(img);
            foreach (HtmlNode embedNode in videoNode.Descendants("embed").ToList())
            {
                Button button = parseVideoNode(embedNode, grid);
                if (button != null)
                {
                    stack.Children.Add(button);
                }
            }
        }

        /// <summary>
        /// Procesa el link del video añadiendolo a un control Button 
        /// </summary>
        /// <param name="node">El nodo donde se encuentra el link</param>
        /// <param name="grid">El contenido del nuevo botón</param>
        /// <returns></returns>
        private Button parseVideoNode(HtmlNode node, Grid grid)
        {
            string path = node.GetAttributeValue("src", "");
            if (path.Length > 0)
            {
                int index = path.IndexOf("v/");
                int index2 = path.IndexOf("&source");
                if (index > 0 || index2 > 0)
                {
                    Button button = new Button();
                    if (index > 0 && index2 > 0)
                    {
                        button.Tag = path.Substring(index + 2, index2 - (index + 2));
                    }
                    else if (index > 0)
                    {
                        button.Tag = path.Substring(index + 2);
                    }
                    button.Content = grid;
                    button.Click +=button_Click;
                    return button;
                }
            }
            return null;
        }

        /// <summary>
        /// lanza el navegador web creando una URi con la información del control Button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        async void button_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri("http://www.youtube.com/embed/" + ((Button)sender).Tag, UriKind.RelativeOrAbsolute));
            
        }

        /// <summary>
        /// Inicia la recursión sobre el contenido HTML
        /// </summary>
        /// <param name="contentStack">StackPnale donde se agregan los elementos</param>
        /// <param name="content">Texto con código HTML</param>
        public void parseContent(StackPanel contentStack, string content)
        {
            HtmlDocument doc = new HtmlDocument();
            HtmlNodeCollection nodeCollection;
            doc.LoadHtml(content);
            nodeCollection = doc.DocumentNode.ChildNodes[0].ChildNodes;
            List<char> styles = new List<char>();
            parseTags(nodeCollection, contentStack, new Paragraph(), styles);
            
        }

        /// <summary>
        /// Método que procesa las etiquetas HTML recursivamente
        /// </summary>
        /// <param name="nodeCollection">Colección de nodos</param>
        /// <param name="stack">StackPanel de la interfaz</param>
        /// <param name="paragraph">Objeto Paragraph en donde se acumulan los diferentes textos</param>
        /// <param name="styles">Lista de estilos que se aplicarán en este nivel de recursividad</param>
        /// <param name="pos">Alineación del texto</param>
        /// <param name="link">URL de hipervínculo encontrado en recursión</param>
        public void parseTags(HtmlNodeCollection nodeCollection,
            StackPanel stack, Paragraph paragraph, List<char> styles, string pos = "left", string link = "")
        {
            bool addBr = false;
            //int brCount = 0;
            foreach (HtmlNode node in nodeCollection)
            {
                switch (node.Name)
                {
                    case "br":
                        if (addBr)
                        {
                            Run runText;
                            runText = new Run();
                            runText.Text = "";
                            paragraph.Inlines.Add(runText);
                        }
                        if (paragraph.Inlines.Count > 0)
                        {
                            ParagraphToStack(stack, paragraph);
                            paragraph = new Paragraph();
                        }
                        addBr = !addBr;
                        break;
                    case "html":
                        break;
                    case "o:p":
                    case "#text":
                        bool textToAdd = false;
                        if (styles.Any())
                        {
                            Span spanContent = new Span();
                            if (textToAdd = addText(node.InnerText, spanContent, styles, link))
                            {
                                paragraph.Inlines.Add(spanContent);
                            }
                        }
                        else
                        {
                            textToAdd = addText(node.InnerText, paragraph);
                        }
                        switch (pos)
                        {
                            case "left":
                                paragraph.TextAlignment = TextAlignment.Left;
                                break;
                            case "center":
                                paragraph.TextAlignment = TextAlignment.Center;
                                break;
                            case "justify":
                                paragraph.TextAlignment = TextAlignment.Justify;
                                break;
                            case "right":
                                paragraph.TextAlignment = TextAlignment.Right;
                                break;
                            default:
                                paragraph.TextAlignment = TextAlignment.Left;
                                break;
                        }
                        break;
                    case "strong":
                    case "b":
                        if (node.HasChildNodes)
                        {
                            styles.Add('b');
                            parseTags(node.ChildNodes, stack, paragraph, styles, pos, link);
                            styles.RemoveAt(styles.Count - 1);
                        }
                        break;
                    case "i":
                        if (node.HasChildNodes)
                        {
                            styles.Add('i');
                            parseTags(node.ChildNodes, stack, paragraph, styles, pos, link);
                            styles.RemoveAt(styles.Count - 1);
                        }
                        break;
                    case "u":
                        if (node.HasChildNodes)
                        {
                            styles.Add('u');
                            parseTags(node.ChildNodes, stack, paragraph, styles, pos, link);
                            styles.RemoveAt(styles.Count - 1);
                        }
                        break;
                    case "a":
                        if (node.HasChildNodes)
                        {
                            string href = node.GetAttributeValue("href", "");
                            int index = href.IndexOf("mailto");
                            if (index != -1)
                            {
                                href = href.Substring(index + 7);
                                styles.Add('e');
                            }
                            else
                            {
                                styles.Add('h');
                            }
                            parseTags(node.ChildNodes, stack, paragraph, styles, pos, href);
                            styles.RemoveAt(styles.Count - 1);
                        }
                        break;
                    case "span":
                        if (node.HasChildNodes)
                        {
                            parseTags(node.ChildNodes, stack, paragraph, styles, pos, link);
                        }
                        break;
                    case "img":
                        if (parseMedia)
                        {
                            string path = node.GetAttributeValue("src", "ms-appx:///Assets/SmallLogo.scale.png");
                            BitmapImage biTmp = new BitmapImage(new Uri(path, UriKind.RelativeOrAbsolute));
                            Image img = new Image();
                            img.Stretch = Stretch.Uniform;
                            img.Margin = new Thickness(0, 15, 0, 15);
                            img.Source = biTmp;
                            InlineUIContainer MyUI = new InlineUIContainer();
                            MyUI.Child = img;
                            paragraph.Inlines.Add(MyUI);
                        }
                        break;
                    case "object":
                        if (parseMedia)
                        {
                            parseVideoObjectNode(node, stack);
                        }
                        break;
                    case "div":
                        if (node.HasChildNodes)
                        {
                            string path = node.GetAttributeValue("style", "text-align:left;");
                            int a = path.IndexOf("text-align");
                            int b = -1;
                            int c = -1;
                            if (a != -1)
                            {
                                b = path.IndexOf(":", a);
                                if (b != -1)
                                {
                                    b++;
                                    c = path.IndexOf(";", b);
                                    if (c != -1)
                                    {
                                        string align = path.Substring(b, c - b).Trim();
                                        if (align.Any())
                                        {
                                            parseTags(node.ChildNodes, stack, paragraph, styles, align);
                                        }
                                        else
                                        {
                                            parseTags(node.ChildNodes, stack, paragraph, styles, pos);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                parseTags(node.ChildNodes, stack, paragraph, styles, pos);
                            }

                            if (paragraph.Inlines.Count > 0)
                            {
                                Run runText;
                                runText = new Run();
                                runText.Text = "";
                                paragraph.Inlines.Add(runText);
                                ParagraphToStack(stack, paragraph);
                                paragraph = new Paragraph();
                            }
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// Agrega el contenido de un parrafo a un RichTextBlock permitiendo mantener diferentes estilos 
        /// y ser agregable al StackPanel
        /// </summary>
        /// <param name="stack">El StackPanel donde se agrega la información</param>
        /// <param name="p">El párrafo a ser agregado</param>
        private void ParagraphToStack(StackPanel stack, Paragraph p)
        {
            RichTextBlock textBlock = new RichTextBlock();
            textBlock.TextWrapping = TextWrapping.Wrap;
            textBlock.Margin = new Thickness(0, 0, 0, 0);

            textBlock.FontSize = (double)Application.Current.Resources["ControlContentThemeFontSize"];
            textBlock.Blocks.Add(p);
            stack.Children.Add(textBlock);
        }

        /// <summary>
        /// Genera un objeto Hyperlink ara contenido ligado a la web
        /// </summary>
        /// <param name="link">el texto que representa el link</param>
        /// <returns></returns>
        private Hyperlink createHyperlink(string link)
        {
            Hyperlink hlink = new Hyperlink()
            {
                FontFamily = new FontFamily("Arial"),
                Foreground = new SolidColorBrush(Windows.UI.Colors.Blue),
                NavigateUri = new Uri(link, UriKind.RelativeOrAbsolute)
            };
            return hlink;
        }

        /// <summary>
        /// Ejecuta la creación de un email 
        /// NOTA: sin Soporte actualmente ya que winRT no tiene una única forma de realizar la misma
        /// tarea en dos plataformas distintas
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void hlink_Click(object sender, RoutedEventArgs e)
        {
            //TODO agregar soporte para envio de correos 
            /*
            Hyperlink hlink = sender as Hyperlink;
            EmailComposeTask emailComposeTask = new EmailComposeTask();
            emailComposeTask.Subject = AppResources.MailContact;
            emailComposeTask.Body = AppResources.MailContactContent;
            emailComposeTask.To = hlink.NavigateUri.OriginalString;
            emailComposeTask.Show();
             */
        }

        /// <summary>
        /// Agrega texto con diferentes estilos a un objeto Span
        /// </summary>
        /// <param name="text">el texto a agregar</param>
        /// <param name="span">Objeto Span donde se regresa el texto agregado</param>
        /// <param name="styles">lista de estilos que se aplicarán</param>
        /// <param name="link">URL para redireccionar a una página web en caso de que se trate de un Hyperlink</param>
        /// <returns>Verdadero si se agregó algún texto, falso en cualquier otro caso</returns>
        public bool addText(string text, Span span, List<char> styles, string link)
        {
            Run runText;
            Span tmpSpan = span;
            if (text.Length > 0 && !text.Equals("\n"))
            {
                span.Foreground = new SolidColorBrush(Windows.UI.Colors.Black);
                span.FontFamily = new FontFamily("Arial");
                //se verifica el estilo del texto
                foreach (char style in styles)
                {
                    switch (style)
                    {
                        case 'b': //bold
                            tmpSpan.Inlines.Add(new Bold());
                            tmpSpan = (Span)tmpSpan.Inlines.Last();
                            break;
                        case 'i'://italic
                            tmpSpan.Inlines.Add(new Italic());
                            tmpSpan = (Span)tmpSpan.Inlines.Last();
                            break;
                        case 'u': // underline
                            span.Inlines.Add(new Underline());
                            tmpSpan = (Span)tmpSpan.Inlines.Last();
                            break;
                        case 'h': //hyperlink
                            tmpSpan.Inlines.Add(createHyperlink(link));
                            tmpSpan = (Span)tmpSpan.Inlines.Last();
                            break;
                        case 'e'://hyperlink email
                            Hyperlink hlink = createHyperlink(link);
                            hlink.Click += hlink_Click;
                            tmpSpan.Inlines.Add(hlink);
                            tmpSpan = (Span)tmpSpan.Inlines.Last();
                            break;
                    }
                }
                runText = new Run();
                runText.Text = text;
                tmpSpan.Inlines.Add(runText);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Agrega texto con un estilo básico predeterminado
        /// </summary>
        /// <param name="node"></param>
        /// <param name="paragraph"></param>
        /// <returns>Verdadero si se agregó algún texto, falso en cualquier otro caso</returns>
        public bool addText(string text, Paragraph paragraph)
        {
            Run runText;
            if (text.Length > 0 && !text.Equals("\n"))
            {
                runText = new Run();
                runText.Text = text;
                runText.FontFamily = new FontFamily("Arial");
                runText.Foreground = new SolidColorBrush(Windows.UI.Colors.Black);
                paragraph.Inlines.Add(runText);
                return true;
            }
            return false;
        }
    }
}
