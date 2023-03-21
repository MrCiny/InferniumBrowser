using CefSharp;
using CefSharp.Example.Handlers;
using CefSharp.WinForms;
using System;
using System.Linq;
using System.Windows.Forms;

namespace InferniumBrowser
{
    public partial class Infernium : Form
    {
        private ChromiumWebBrowser browser;
        DownloadHandler downloadHandler = new DownloadHandler();
        CustomMenuHandler mainMenuHandler = new CustomMenuHandler();

        string[] domains = { ".com", ".uk", ".de", ".ru", ".org", ".net", ".in", ".ir", ".br", ".au", ".eu", ".lv" }; //Masīvs, kas satur šīs pārlūkprogrammas atbalstītos augstākā līmeņa domēnus.

        public Infernium()
        {
            InitializeComponent();
            InitializeBrowser();
            InitializeForm();
        }

        private void InitializeForm()
        {
            BrowserTabs.Height = ClientRectangle.Height - 25;
        }

        private void InitializeHandlers()
        {
            browser.DownloadHandler = downloadHandler; //Atļauj Lejupielādes funkciju caur saitēm. (DownloadHandler.cs)
            browser.MenuHandler = mainMenuHandler; //Iespējo pielāgotu kontekstizvēlni. Ar peles labo pogu/taustiņu noklikšķiniet uz pārlūkprogrammas, lai to redzētu. (CustomMenuHandler.cs)
            browser.RequestHandler = new CustomRequestHandler(); //Brīdina lietotāju, kad apmeklē nedrošu vietni. (CustomRequestHandler.cs)
        }

        private void InitializeAdBlock() //Funkcija, kas bloķē YouTube reklāmas (CEFSharp ir limitēta pieeja chrome paplašinājumu API, tāpēc vienīgais veids, kā pievienot paplašinājumus jeb "extensions" ir modificējot CEFSharp paplašinājuma menedžeri, kas ir sarežģīts un laikietilpīgs process) 
        {
            if (toolStripAddressBar.Text.Contains("youtube.com"))
            {
                string script = "document.cookie=\"VISITOR_INFO1_LIVE = oKckVSqvaGw; path =/; domain =.youtube.com\";window.location.reload();}";
                browser.ExecuteScriptAsyncWhenPageLoaded($"console.log({script})");
            }
        }

        //Izveido pārlūkprogrammu, caur kuru var doties uz dažādām lapām.
        private void InitializeBrowser()
        {
            var settings = new CefSettings();
            Cef.Initialize(settings);

            AddBrowser();
            BrowserTabs.TabPages[0].Controls.Add(browser);
        }

        private void toolStripButtonGo_Click(object sender, EventArgs e)
        {
            Navigate(toolStripAddressBar.Text);
        }

        private void toolStripButtonBack_Click(object sender, EventArgs e)
        {
            var selectedBrowser = (ChromiumWebBrowser)BrowserTabs.SelectedTab.Controls[0];
            selectedBrowser.Back();
        }

        private void toolStripButtonForward_Click(object sender, EventArgs e)
        {
            var selectedBrowser = (ChromiumWebBrowser)BrowserTabs.SelectedTab.Controls[0];
            selectedBrowser.Forward();
        }

        private void toolStripButtonReload_Click(object sender, EventArgs e)
        {
            var selectedBrowser = (ChromiumWebBrowser)BrowserTabs.SelectedTab.Controls[0];
            selectedBrowser.Reload();
        }

        //Metode, kas aizver visas cilnes un atver 1 jaunu cilni. (C.A.O.)
        private void toolStripButtonCloseAndOpen_Click(object sender, EventArgs e)
        {
            int tabCount = BrowserTabs.TabPages.Count - 1;

            for (int i = 0; i < tabCount; i++)
            {
                BrowserTabs.TabPages.Remove(BrowserTabs.SelectedTab);
            }
            AddBrowserTab();
            BrowserTabs.SelectTab(0);
        }

        //Funkcija, kas izslēdz pogas skaņu, kad tiek nospiests "Enter".
        private void toolStripAddressBar_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                Navigate(toolStripAddressBar.Text);
                e.SuppressKeyPress = true;
            }
        }

        //Metode, kas aizver aktīvo cilni ar brīdinājumu.
        private void toolStripButtonCloseTab_Click(object sender, EventArgs e)
        {
            int tabCount = BrowserTabs.TabPages.Count - 1;
            if (tabCount > 1)
            {
                BrowserTabs.TabPages.Remove(BrowserTabs.SelectedTab);
            }
            else
            {
                string title = "Warning";
                MessageBoxButtons buttons = MessageBoxButtons.YesNo;
                DialogResult result = MessageBox.Show("Are you sure you want to close this browser?", title, buttons);
                if (result == DialogResult.Yes)
                {
                    this.Close();
                }
            }
        }

        //Funkcija, kas nodrošina, ka adreses joslas teksts tiek nomainīts uz lapas pilno adresi.
        private void Browser_AddressChanged(object sender, AddressChangedEventArgs e)
        {
            //Te ir "Try & catch" nosacījums, kas novērš kļūdas rašanos, kad iziet ārā no pārlūkprogrammas ar 2 vai vairāk atvērtām cilnēm.
            try
            {
                var selectedBrowser = (ChromiumWebBrowser)sender;
                this.Invoke(new MethodInvoker(() =>
                {
                    toolStripAddressBar.Text = e.Address;
                }));
            }
            catch
            {

            }
            string pageTitle = browser.Parent.Text;
        }

        //Funkcija, kas nodrošina, ka nomainās cilnes nosaukums.
        private void Browser_TitleChanged(object sender, TitleChangedEventArgs e)
        {
            var selectedBrowser = (ChromiumWebBrowser)sender;
            this.Invoke(new MethodInvoker(() =>
            {
                selectedBrowser.Parent.Text = e.Title;
            }));
        }

        private void Browser_LoadingStateChanged(object sender, LoadingStateChangedEventArgs e)
        {
            if (e.IsLoading == false)
            {
                InitializeAdBlock();
            }
        }

        //Funkcija, kas aizved pārlūkprogramu uz adresi, kas ir ierakstīta adreses joslā.
        private void Navigate(string address)
        {
            try
            {
                var selectedBrowser = (ChromiumWebBrowser)BrowserTabs.SelectedTab.Controls[0];

                //Nosacījuma bloks, kas nosaka, vai adreses joslā ir augstākā līmeņa domēns, kas atrodas "domains" sarakstā.
                if (domains.Any(toolStripAddressBar.Text.Contains) || toolStripAddressBar.Text == "chrome://extensions-support")
                {
                    selectedBrowser.Load(toolStripAddressBar.Text);
                }
                else
                {
                    selectedBrowser.Load("https://duckduckgo.com/?q=" + toolStripAddressBar.Text);
                }
            }
            catch
            {

            }
        }

        //Iedod pārlūkprogrammai parametrus un aizved uz DuckDuckGo lapu, kas skaitās kā "main" jeb galvenā lapa.
        private void AddBrowser()
        {
            var mainPagePath = "https://start.duckduckgo.com/";

            browser = new ChromiumWebBrowser(mainPagePath);
            browser.Dock = DockStyle.Fill;
            browser.AddressChanged += Browser_AddressChanged;
            browser.TitleChanged += Browser_TitleChanged;
            browser.LoadingStateChanged += Browser_LoadingStateChanged;

            InitializeHandlers();
        }

        //Pievieno jaunu cilni
        private void AddBrowserTab()
        {
            var newTabPage = new TabPage();
            newTabPage.Text = "New Tab";
            BrowserTabs.TabPages.Insert(BrowserTabs.TabPages.Count - 1, newTabPage);
            AddBrowser();
            newTabPage.Controls.Add(browser);
        }

        private void BrowserTabs_Click(object sender, EventArgs e)
        {
            if (BrowserTabs.SelectedTab == BrowserTabs.TabPages[BrowserTabs.TabPages.Count - 1])
            {
                AddBrowserTab();
                BrowserTabs.SelectedTab = BrowserTabs.TabPages[BrowserTabs.TabPages.Count - 2];
            }

            //Funkcija, kas adreses joslā parāda, kura lapa pašlaik ir aktīva.
            if (BrowserTabs.SelectedTab != BrowserTabs.TabPages[BrowserTabs.TabPages.Count - 1])
            {
                var selectedBrowser = (ChromiumWebBrowser)BrowserTabs.SelectedTab.Controls[0];

                this.Invoke(new MethodInvoker(() =>
                {
                    toolStripAddressBar.Text = selectedBrowser.Address;
                }));
            }
        }

        //Metode, kas parāda brīdinājumu, kad iziet ārā no pārlūkprogrammas, kurā ir 2 vai vairākas cilnes
        private void Infernium_FormClosing(object sender, FormClosingEventArgs e)
        {
            int tabCount = BrowserTabs.TabPages.Count - 1;
            string title = "Warning";
            MessageBoxButtons buttons = MessageBoxButtons.YesNo;

            if (tabCount > 1)
            {
                DialogResult result = MessageBox.Show("You are about to close " + tabCount + " tabs. Are you sure you want to continue?", title, buttons);

                if (result == DialogResult.Yes)
                {

                }
                else
                {
                    e.Cancel = true;
                }
            }
        }
    }
}
